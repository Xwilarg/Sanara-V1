using Discord;
using Discord.Commands;
using Discord.WebSocket;

using Microsoft.Extensions.DependencyInjection;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Bot
{
    partial class Program
    {
        public static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();
        public readonly DiscordSocketClient client;
        private readonly IServiceCollection map = new ServiceCollection();
        private readonly CommandService commands = new CommandService();
        //private IServiceProvider services = null;

        public bool mustSorry;
        public List<Character> relations;
        public Random rand;
        public Thread shiritoriThread;
        public List<string> names;
        public List<currGame> serverPlayingKancolle;
        public Thread quizzThread;
        public static Program p;
        public IGuild shiritoriServer;
        public bool isLearning;
        public List<string> knowedWords;
        public int newWords, notWord, notNoun, alreadyKnow, invalidWord, tooShort;
        public DateTime timeLearn;
        public bool isLearningBooru;
        public int nbRequestKona, nbRequestGel, nbRequestLoli, nbRequestSafe;
        public int nbRequestKonaBase, nbRequestGelBase, nbRequestLoliBase, nbRequestSafeBase;
        public ITextChannel stopChan;
        public bool prepareStopLearn;
        public Mutex lockBooru;
        public List<Server> allServers;
        IGuild defaultGuild;
        bool sayHiEveryone;

        private Program()
        {
            allServers = new List<Server>();
            lockBooru = new Mutex();
            p = this;
            mustSorry = false;
            relations = new List<Character>();
            rand = new Random(DateTime.UtcNow.Millisecond);
            shiritoriThread = new Thread(new ThreadStart(learn));
            quizzThread = new Thread(new ThreadStart(playKancolleQuizz));
            serverPlayingKancolle = new List<currGame>();
            isLearning = false;
            prepareStopLearn = false;
            serverPlayingKancolle = new List<currGame>();

            prepareQuizz();

            client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Verbose,

                // If you or another service needs to do anything with messages
                // (eg. checking Reactions, checking the content of edited/deleted messages),
                // you must set the MessageCacheSize. You may adjust the number as needed.
                //MessageCacheSize = 50,
            });
            client.Log += Log;
            commands.Log += Log;
        }

        private async Task MainAsync()
        {
            await commands.AddModuleAsync<CommunicationModule>();
            await commands.AddModuleAsync<SettingsModule>();
            await commands.AddModuleAsync<JishoModule>();
            await commands.AddModuleAsync<KancolleModule>();
            await commands.AddModuleAsync<DebugModule>();
            await commands.AddModuleAsync<BooruModule>();
            await commands.AddModuleAsync<YoutubeModule>();
            await commands.AddModuleAsync<TaskModule>();
            await commands.AddModuleAsync<GameModule>();
            await commands.AddModuleAsync<GoogleShortenerModule>();
            await commands.AddModuleAsync<ConsoleModule>();
            await commands.AddModuleAsync<CodeModule>();
            await commands.AddModuleAsync<NhentaiModule>();
            await commands.AddModuleAsync<MyAnimeListModule>();

            client.MessageReceived += HandleCommandAsync;
            client.GuildAvailable += Connect;
            client.UserJoined += UserJoin;
            client.JoinedGuild += GuildJoin;

            await client.LoginAsync(TokenType.Bot, File.ReadAllText("Keys/token.dat"));
            await client.StartAsync();

            await Task.Delay(-1);
        }

        public void learn()
        {
            while (Thread.CurrentThread.IsAlive)
            {
                if (isLearning)
                {
                    if (!isLearningBooru)
                        learnShiritori();
                    else
                        learnBooru();
                }
            }
        }
        
        private async Task GuildJoin(SocketGuild arg)
        {
            if (!allServers.Exists(x => x._id == arg.Id))
                allServers.Add(new Server(arg.Id));
            if (arg.Id.ToString() == File.ReadAllLines("Saves/sanaraDatas.dat")[2])
                defaultGuild = arg;
            string currTime = DateTime.UtcNow.ToString("ddMMyyHHmmss");
            ITextChannel chan = returnChannel(arg.Channels.ToList(), arg.Id);
            if (!Directory.Exists("Saves"))
                Directory.CreateDirectory("Saves");
            if (!File.Exists("Saves/sanaraDatas.dat"))
                File.WriteAllText("Saves/sanaraDatas.dat", currTime + Environment.NewLine + "0" + Environment.NewLine + "0"); // Creation date | is closed correcly | id default server
            if (!File.Exists("Saves/arena.dat"))
                File.Create("Saves/arena.dat");
            if (!File.Exists("Saves/shiritoriStats.dat"))
                File.WriteAllText("Saves/shiritoriStats.dat", "0" + Environment.NewLine + "0" + Environment.NewLine + "0" + Environment.NewLine + "0" + Environment.NewLine + "0" + Environment.NewLine + "0" + Environment.NewLine + "0");
            // Already know, Not a word, not a noun, invalid word, too short, new word learn, total seconds learn
            if (!File.Exists("Saves/shiritoriWords.dat"))
                File.Create("Saves/shiritoriWords.dat");
            if (!Directory.Exists("Saves/booruAnalysis"))
                Directory.CreateDirectory("Saves/booruAnalysis");
            if (!File.Exists("Saves/booruAnalysis/stats.dat"))
                File.WriteAllText("Saves/booruAnalysis/stats.dat", "0" + Environment.NewLine + "0" + Environment.NewLine + "0" + Environment.NewLine + "0" + Environment.NewLine + "0");
            // time spent learning, prog kona gel loli safe
            if (!Directory.Exists("Saves/booruAnalysis/Konachan"))
                Directory.CreateDirectory("Saves/booruAnalysis/Konachan");
            if (!Directory.Exists("Saves/booruAnalysis/Gelbooru"))
                Directory.CreateDirectory("Saves/booruAnalysis/Gelbooru");
            if (!Directory.Exists("Saves/booruAnalysis/Lolibooru"))
                Directory.CreateDirectory("Saves/booruAnalysis/Lolibooru");
            if (!Directory.Exists("Saves/booruAnalysis/Safebooru"))
                Directory.CreateDirectory("Saves/booruAnalysis/Safebooru");
            if (!Directory.Exists("Saves/Servers/" + arg.Id))
            {
                Directory.CreateDirectory("Saves/Servers/" + arg.Id);
                Directory.CreateDirectory("Saves/Servers/" + arg.Id + "/Tasks");
                using (File.Create("Saves/Servers/" + arg.Id + "/serverDatas.dat"))
                { }
                File.WriteAllText("Saves/Servers/" + arg.Id + "/serverDatas.dat", currTime + Environment.NewLine + chan.Id + Environment.NewLine + arg.Name + Environment.NewLine + "0"
                                  + Environment.NewLine + "1" + Environment.NewLine + "1" + Environment.NewLine + "1"); // Join date | default chan id | server name | modules....
                await chan.SendMessageAsync("Hi everyone, my name is Sanara." + Environment.NewLine + "Nice to meet you all.");
            }
            else
            {
                if (File.ReadAllLines("Saves/Servers/" + arg.Id + "/serverDatas.dat")[3] == "1")
                {
                    if (arg.Id.ToString() == File.ReadAllLines("Saves/sanaraDatas.dat")[2] && File.ReadAllLines("Saves/sanaraDatas.dat")[1] == "1")
                    {
                        string[] content = File.ReadAllLines("Saves/sanaraDatas.dat");
                        File.WriteAllText("Saves/sanaraDatas.dat", content[0] + Environment.NewLine + "0" + Environment.NewLine + content[2]);
                        await chan.SendMessageAsync($"<@{144851584478740481}> Master! Please don't shut me down so suddenly! You know it can corrupt my database...");
                        mustSorry = true;
                    }
                    else if (sayHiEveryone)
                        await chan.SendMessageAsync("Hi everyone!");
                }
            }
            if (!Directory.Exists("Saves/Servers/" + arg.Id + "/ModuleCount"))
                Directory.CreateDirectory("Saves/Servers/" + arg.Id + "/ModuleCount");
            if (!File.Exists("Saves/Servers/" + arg.Id + "/kancolle.dat"))
                File.WriteAllText("Saves/Servers/" + arg.Id + "/kancolle.dat", "0" + Environment.NewLine + "0" + Environment.NewLine + "0" + Environment.NewLine + "0" + Environment.NewLine + "0");
            // Attempt game, attempt ship, ship found, bestScore, ids of people who help to have the best score
            if (!File.Exists("Saves/Servers/" + arg.Id + "/quizzTagSafe.dat"))
                File.WriteAllText("Saves/Servers/" + arg.Id + "/quizzTagSafe.dat", "0" + Environment.NewLine + "0" + Environment.NewLine + "0" + Environment.NewLine + "0" + Environment.NewLine + "0");
            // Attempt game, attempt tag, tags found, bestScore, ids of people who help to have the best score
            if (!File.Exists("Saves/Servers/" + arg.Id + "/quizzTagAll.dat"))
                File.WriteAllText("Saves/Servers/" + arg.Id + "/quizzTagAll.dat", "0" + Environment.NewLine + "0" + Environment.NewLine + "0" + Environment.NewLine + "0" + Environment.NewLine + "0");
            // Attempt game, attempt tag, tags found, bestScore, ids of people who help to have the best score
            if (!Directory.Exists("Saves/Users"))
            {
                Directory.CreateDirectory("Saves/Users");
            }
            foreach (IUser u in arg.Users)
            {
                if (!File.Exists("Saves/Users/" + u.Id + ".dat"))
                {
                    relations.Add(new Character(u.Id, u.Username));
                }
                else
                {
                    try
                    {
                        if (!relations.Any(x => x.getName() == Convert.ToUInt64(File.ReadAllLines("Saves/Users/" + u.Id + ".dat")[1])))
                        {
                            relations.Add(new Character());
                            relations[relations.Count - 1].saveAndParseInfos(File.ReadAllLines("Saves/Users/" + u.Id + ".dat"));
                        }
                    }
                    catch (IndexOutOfRangeException)
                    {
                        if (arg.Id.ToString() == File.ReadAllLines("Saves/sanaraDatas.dat")[2])
                        {
                            await chan.SendMessageAsync($"<@{144851584478740481}> Master, the user " + u.Id.ToString() + " named " + u.Username + " is corrupted in my database." + Environment.NewLine +
                                "Please check that manually.");
                        }
                    }
                }
            }
            saveDatas(arg.Id);
        }

        private async Task Connect(SocketGuild arg)
        {
            if (!allServers.Exists(x => x._id == arg.Id))
                allServers.Add(new Server(arg.Id));
            if (arg.Id.ToString() == File.ReadAllLines("Saves/sanaraDatas.dat")[2])
                defaultGuild = arg;
            string currTime = DateTime.UtcNow.ToString("ddMMyyHHmmss");
            ITextChannel chan = returnChannel(arg.Channels.ToList(), arg.Id);
            if (!Directory.Exists("Saves"))
                Directory.CreateDirectory("Saves");
            if (!File.Exists("Saves/sanaraDatas.dat"))
                File.WriteAllText("Saves/sanaraDatas.dat", currTime + Environment.NewLine + "0" + Environment.NewLine + "0"); // Creation date | is closed correcly | id default server
            if (!File.Exists("Saves/arena.dat"))
                File.Create("Saves/arena.dat");
            if (!File.Exists("Saves/shiritoriStats.dat"))
                File.WriteAllText("Saves/shiritoriStats.dat", "0" + Environment.NewLine + "0" + Environment.NewLine + "0" + Environment.NewLine + "0" + Environment.NewLine + "0" + Environment.NewLine + "0" + Environment.NewLine + "0");
            // Already know, Not a word, not a noun, invalid word, too short, new word learn, total seconds learn
            if (!File.Exists("Saves/shiritoriWords.dat"))
                File.Create("Saves/shiritoriWords.dat");
            if (!Directory.Exists("Saves/booruAnalysis"))
                Directory.CreateDirectory("Saves/booruAnalysis");
            if (!File.Exists("Saves/booruAnalysis/stats.dat"))
                File.WriteAllText("Saves/booruAnalysis/stats.dat", "0" + Environment.NewLine + "0" + Environment.NewLine + "0" + Environment.NewLine + "0" + Environment.NewLine + "0");
            // time spent learning, prog kona gel loli safe
            if (!Directory.Exists("Saves/booruAnalysis/Konachan"))
                Directory.CreateDirectory("Saves/booruAnalysis/Konachan");
            if (!Directory.Exists("Saves/booruAnalysis/Gelbooru"))
                Directory.CreateDirectory("Saves/booruAnalysis/Gelbooru");
            if (!Directory.Exists("Saves/booruAnalysis/Lolibooru"))
                Directory.CreateDirectory("Saves/booruAnalysis/Lolibooru");
            if (!Directory.Exists("Saves/booruAnalysis/Safebooru"))
                Directory.CreateDirectory("Saves/booruAnalysis/Safebooru");
            if (!Directory.Exists("Saves/Servers/" + arg.Id))
            {
                Directory.CreateDirectory("Saves/Servers/" + arg.Id);
                Directory.CreateDirectory("Saves/Servers/" + arg.Id + "/Tasks");
                using (File.Create("Saves/Servers/" + arg.Id + "/serverDatas.dat"))
                { }
                File.WriteAllText("Saves/Servers/" + arg.Id + "/serverDatas.dat", currTime + Environment.NewLine + chan.Id + Environment.NewLine + arg.Name + Environment.NewLine + "0"
                                  + Environment.NewLine + "1" + Environment.NewLine + "1" + Environment.NewLine + "1"); // Join date | default chan id | server name | modules....
                await chan.SendMessageAsync("Hi everyone, my name is Sanara." + Environment.NewLine + "Nice to meet you all.");
            }
            else
            {
                if (File.ReadAllLines("Saves/Servers/" + arg.Id + "/serverDatas.dat")[3] == "1")
                {
                    if (arg.Id.ToString() == File.ReadAllLines("Saves/sanaraDatas.dat")[2] && File.ReadAllLines("Saves/sanaraDatas.dat")[1] == "1")
                    {
                        string[] content = File.ReadAllLines("Saves/sanaraDatas.dat");
                        File.WriteAllText("Saves/sanaraDatas.dat", content[0] + Environment.NewLine + "0" + Environment.NewLine + content[2]);
                        await chan.SendMessageAsync($"<@{144851584478740481}> Master! Please don't shut me down so suddenly! You know it can corrupt my database...");
                        mustSorry = true;
                    }
                    else if (sayHiEveryone)
                        await chan.SendMessageAsync("Hi everyone!");
                }
            }
            if (!Directory.Exists("Saves/Servers/" + arg.Id + "/ModuleCount"))
                Directory.CreateDirectory("Saves/Servers/" + arg.Id + "/ModuleCount");
            if (!File.Exists("Saves/Servers/" + arg.Id + "/kancolle.dat"))
                File.WriteAllText("Saves/Servers/" + arg.Id + "/kancolle.dat", "0" + Environment.NewLine + "0" + Environment.NewLine + "0" + Environment.NewLine + "0" + Environment.NewLine + "0");
            // Attempt game, attempt ship, ship found, bestScore, ids of people who help to have the best score
            if (!File.Exists("Saves/Servers/" + arg.Id + "/quizzTagSafe.dat"))
                File.WriteAllText("Saves/Servers/" + arg.Id + "/quizzTagSafe.dat", "0" + Environment.NewLine + "0" + Environment.NewLine + "0" + Environment.NewLine + "0" + Environment.NewLine + "0");
            // Attempt game, attempt tag, tags found, bestScore, ids of people who help to have the best score
            if (!File.Exists("Saves/Servers/" + arg.Id + "/quizzTagAll.dat"))
                File.WriteAllText("Saves/Servers/" + arg.Id + "/quizzTagAll.dat", "0" + Environment.NewLine + "0" + Environment.NewLine + "0" + Environment.NewLine + "0" + Environment.NewLine + "0");
            // Attempt game, attempt tag, tags found, bestScore, ids of people who help to have the best score
            if (!Directory.Exists("Saves/Users"))
            {
                Directory.CreateDirectory("Saves/Users");
            }
            foreach (IUser u in arg.Users)
            {
                if (!File.Exists("Saves/Users/" + u.Id + ".dat"))
                {
                    relations.Add(new Character(u.Id, u.Username));
                }
                else
                {
                    try
                    {
                        if (!relations.Any(x => x.getName() == Convert.ToUInt64(File.ReadAllLines("Saves/Users/" + u.Id + ".dat")[1])))
                        {
                            relations.Add(new Character());
                            relations[relations.Count - 1].saveAndParseInfos(File.ReadAllLines("Saves/Users/" + u.Id + ".dat"));
                        }
                    }
                    catch (IndexOutOfRangeException)
                    {
                        if (arg.Id.ToString() == File.ReadAllLines("Saves/sanaraDatas.dat")[2])
                        {
                            await chan.SendMessageAsync($"<@{144851584478740481}> Master, the user " + u.Id.ToString() + " named " + u.Username + " is corrupted in my database." + Environment.NewLine +
                                "Please check that manually.");
                        }
                    }
                }
            }
            saveDatas(arg.Id);
        }

        private async Task UserJoin(SocketGuildUser arg)
        {
            string currTime = DateTime.UtcNow.ToString("ddMMyyHHmmss");
            if (!File.Exists("Saves/Users/" + arg.Id + ".dat"))
            {
                relations.Add(new Character(arg.Id, arg.Nickname));
                saveDatas(arg.Guild.Id);
            }
            if (returnChannel(arg.Guild.Channels.ToList(), arg.Guild.Id) != null)
                await returnChannel(arg.Guild.Channels.ToList(), arg.Guild.Id).SendMessageAsync($"<@{arg.Id}> Welcome on this server.");
        }

        private long getLenghtFolder(string folder)
        {
            long currSize = 0;
            foreach (string s in Directory.GetFiles(folder))
            {
                FileInfo fi = new FileInfo(s);
                currSize += fi.Length;
            }
            foreach (string s in Directory.GetDirectories(folder))
            {
                currSize += getLenghtFolder(s);
            }
            return (currSize);
        }

        private async Task HandleCommandAsync(SocketMessage arg)
        {
            var msg = arg as SocketUserMessage;
            if (msg == null) return;
            if (arg.Author.Id == 352216646267437059 && arg.Content == "<@&330483872544587776> Please send me your informations for comparison.")
            {
                List<ulong> users = new List<ulong>();
                foreach (var g in p.client.Guilds)
                {
                    foreach (var g2 in g.Users)
                    {
                        if (!users.Contains(g2.Id))
                        {
                            users.Add(g2.Id);
                        }
                    }
                }
                await (((ITextChannel)(arg.Channel))).SendMessageAsync($"<@{352216646267437059}> Here are the compare informations: " + p.client.Guilds.Count + "|" + users.Count + "|" + getLenghtFolder("Saves") + "o|Zirk");
            }
            else if (arg.Author.Id == 352216646267437059 && msg.Content.Length > 75
                && (msg.Content.Substring(0, 75) == "<@329664361016721408> Do you accept that Hista represent you in the arena ?"
                || msg.Content.Substring(0, 74) == "<@329664361016721408> Do you accept that Ryaa represent you in the arena ?"))
            {
                await arg.Channel.SendMessageAsync("Yes");
            }
            // Create a number to track where the prefix ends and the command begins
            int pos = 0;
            if (msg.HasMentionPrefix(client.CurrentUser, ref pos))
            {
                // Create a Command Context.
                var context = new SocketCommandContext(client, msg);
                if (allServers.Find(x => x._id == context.Guild.Id)._inConsoleMode)
                {
                    executeCommand(context);
                }
                else
                {
                    // Execute the command. (result does not indicate a return value, 
                    // rather an object stating if the command executed succesfully).
                    var result = await commands.ExecuteAsync(context, pos);
                }
            }
        }

        public void saveDatas(ulong servId, string additionalPath = "Saves")
        {
            foreach (Character r in relations)
            {
                if (!File.Exists(additionalPath + "/Users/" + r.getName() + ".dat"))
                {
                    using (File.Create(additionalPath + "/Users/" + r.getName() + ".dat"))
                    { }
                    File.WriteAllText(additionalPath + "/Users/" + r.getName() + ".dat", r.returnInformationsRaw(true));
                    File.WriteAllText(additionalPath + "/Users/" + r.getName() + ".dat", r.returnInformationsRaw(false));
                }
                else
                    File.WriteAllText(additionalPath + "/Users/" + r.getName() + ".dat", r.returnInformationsRaw(false));
                r.reset(); // A character only stock it's current informations
            }
        }

        public enum Module
        {
            Arena,
            Booru,
            Code,
            Communication,
            Console,
            Debug,
            Game,
            GoogleShortener,
            Jisho,
            Kancolle,
            Nhentai,
            Settings,
            Task,
            Youtube,
            MyAnimeList
        }

        public void doAction(IUser u, ulong serverId, Module m, bool needThanks = false)
        {
            mustSorry = false;
            if (!Directory.Exists("Saves/Servers/" + serverId + "/ModuleCount/" + DateTime.UtcNow.ToString("yyyyMM")))
            {
                Directory.CreateDirectory("Saves/Servers/" + serverId + "/ModuleCount/" + DateTime.UtcNow.ToString("yyyyMM"));
            }
            for (int i = 0; i < (int)Module.MyAnimeList + 1; i++)
            {
                if (!File.Exists("Saves/Servers/" + serverId + "/ModuleCount/" + DateTime.UtcNow.ToString("yyyyMM") + "/"
                    + ((Module)i).ToString()[0] + ((Module)i).ToString().ToLower().Substring(1, ((Module)i).ToString().Length - 1) + ".dat"))
                    File.WriteAllText("Saves/Servers/" + serverId + "/ModuleCount/" + DateTime.UtcNow.ToString("yyyyMM") + "/"
                        + ((Module)i).ToString()[0] + ((Module)i).ToString().ToLower().Substring(1, ((Module)i).ToString().Length - 1) + ".dat", "0");
            }
            File.WriteAllText("Saves/Servers/" + serverId + "/ModuleCount/" + DateTime.UtcNow.ToString("yyyyMM") + "/"
                        + m.ToString()[0] + m.ToString().ToLower().Substring(1, m.ToString().Length - 1) + ".dat",
                        (Convert.ToInt32(File.ReadAllText("Saves/Servers/" + serverId + "/ModuleCount/" + DateTime.UtcNow.ToString("yyyyMM") + "/"
                        + m.ToString()[0] + m.ToString().ToLower().Substring(1, m.ToString().Length - 1) + ".dat")) + 1).ToString());
            string[] content = File.ReadAllLines("Saves/sanaraDatas.dat");
            File.WriteAllText("Saves/sanaraDatas.dat", content[0] + Environment.NewLine + "1" + Environment.NewLine + content[2]);
            foreach (Character c in relations)
            {
                if (u.Id == c.getName())
                {
                    c.increaseNbMessage();
                    c.meet();
                    if (needThanks)
                        c.increaseNbRequest();
                    saveDatas(serverId);
                    if (needThanks)
                    {
                        c._needThanks = true;
                        c._timeThanks = DateTime.UtcNow;
                    }
                    else
                        c._needThanks = false;
                    break;
                }
            }
        }

        private ITextChannel returnChannel(List<SocketGuildChannel> s, ulong serverId)
        {
            foreach (IChannel c in s)
            {
                if (c.GetType().Name == "SocketTextChannel")
                {
                    if ((!Directory.Exists("Saves/Servers/" + serverId))// && c.per .cSendMessages)
                    || (Directory.Exists("Saves/Servers/" + serverId) && c.Id == Convert.ToUInt64(File.ReadAllLines("Saves/Servers/" + serverId + "/serverDatas.dat")[1])))
                    {
                        return (ITextChannel)(c);
                    }
                }
            }
            foreach (IChannel c in s)
            {
                if (c.GetType().Name == "SocketTextChannel")
                {
                    return (ITextChannel)(c);
                }
            }
            return (null);
        }

        private Task Log(LogMessage msg)
        {
            var cc = Console.ForegroundColor;
            switch (msg.Severity)
            {
                case LogSeverity.Critical:
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    break;
                case LogSeverity.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case LogSeverity.Warning:
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    break;
                case LogSeverity.Info:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case LogSeverity.Verbose:
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;
                case LogSeverity.Debug:
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    break;
            }
            //Console.WriteLine($"{DateTime.Now,-19} [{msg.Severity,8}] {msg.Source}: {msg.Message}");
            Console.WriteLine(msg);
            Console.ForegroundColor = cc;

            return Task.CompletedTask;
        }
    }
}
