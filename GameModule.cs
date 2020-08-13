using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bot
{
    public class GameModule : ModuleBase
    {
        Program p = Program.p;

        [Command("Start learning shiritori"), Summary("Begin to learn new words for Shiritori")]
        public async Task learnShiritori()
        {
            p.doAction(Context.User, Context.Guild.Id, Program.Module.Game);
            if (p.isLearning)
            {
                await ReplyAsync("I'm already learning.");
            }
            else if (Context.User.Id != 144851584478740481)
            {
                await ReplyAsync(Sentences.onlyMasterStr);
            }
            else
            {
                if (!p.shiritoriThread.IsAlive)
                    p.shiritoriThread.Start();
                p.knowedWords = File.ReadAllLines("Saves/shiritoriWords.dat").ToList();
                p.newWords = 0;
                p.notWord = 0;
                p.notNoun = 0;
                p.alreadyKnow = 0;
                p.invalidWord = 0;
                p.tooShort = 0;
                p.timeLearn = DateTime.UtcNow;
                p.shiritoriServer = Context.Guild;
                p.isLearning = true;
                p.isLearningBooru = false;
                await ReplyAsync("Alright, I will begin to learn!");
            }
        }

        [Command("Stop learning shiritori"), Summary("Stop to learn new words for Shiritori")]
        public async Task stopLearnShiritori()
        {
            p.doAction(Context.User, Context.Guild.Id, Program.Module.Game);
            if (!p.isLearning)
            {
                await ReplyAsync("I'm not currently learning.");
            }
            else if (p.isLearningBooru)
            {
                await ReplyAsync("I'm not currently learning shiritori.");
            }
            else if (Context.User.Id != 144851584478740481)
            {
                await ReplyAsync(Sentences.onlyMasterStr);
            }
            else
            {
                p.isLearning = false;
                double seconds = DateTime.UtcNow.Subtract(p.timeLearn).TotalSeconds;
                int hour = (int)(seconds / 3600);
                int minutes = (int)(seconds % 3600) / 60;
                int second = (int)(seconds % 3600) % 60;
                string finalTime = "";
                if (hour > 0)
                    finalTime += hour + " hours, ";
                if (minutes > 0 || hour > 0)
                    finalTime += minutes + " minutes and ";
                finalTime += second + " seconds";
                if (p.newWords == 0)
                {
                    if (p.notWord == 0 && p.notNoun == 0 && p.alreadyKnow == 0 && p.invalidWord == 0)
                        await ReplyAsync("That's mean, you didn't even let me the time to begin to learn...");
                    else
                        await ReplyAsync("I'm sorry, but after " + finalTime + " and " + (p.notWord + p.notNoun + p.alreadyKnow + p.invalidWord).ToString() + " attempts I wasn't able to learn any new word...");
                }
                else
                    await ReplyAsync("After " + finalTime + " and " + (p.notWord + p.notNoun + p.newWords + p.alreadyKnow + p.invalidWord).ToString() + " attempts, I learned " + p.newWords.ToString() + " new words!");
                endLearn(seconds);
            }
        }

        private void endLearn(double seconds)
        {
            p.isLearning = false;
            File.WriteAllLines("Saves/shiritoriWords.dat", p.knowedWords);
            string[] stats = File.ReadAllLines("Saves/shiritoriStats.dat");
            File.WriteAllText("Saves/shiritoriStats.dat", (Convert.ToInt32(stats[0]) + p.alreadyKnow).ToString() + Environment.NewLine
                                                        + (Convert.ToInt32(stats[1]) + p.notWord).ToString() + Environment.NewLine
                                                        + (Convert.ToInt32(stats[2]) + p.notNoun).ToString() + Environment.NewLine
                                                        + (Convert.ToInt32(stats[3]) + p.invalidWord).ToString() + Environment.NewLine
                                                        + (Convert.ToInt32(stats[4]) + p.tooShort).ToString() + Environment.NewLine
                                                        + (Convert.ToInt32(stats[5]) + p.newWords).ToString() + Environment.NewLine
                                                        + (Convert.ToDouble(stats[6]) + seconds).ToString());
        }

        [Command("quizz Booru"), Summary("Try to guess the tag in Booru game")]
        public async Task guessBooru(string answer)
        {
            p.doAction(Context.User, Context.Guild.Id, Program.Module.Game);
            if (File.ReadAllLines("Saves/Servers/" + Context.Guild.Id + "/serverDatas.dat")[6] == "0")
            {
                await ReplyAsync("Sorry but this module was disabled.");
            }
            else if (!p.serverPlayingKancolle.Any(x => x.idServer == Context.Guild.Id))
            {
                await ReplyAsync("Sorry but this game didn't start yet.");
            }
            else
            {
                foreach (TagBooruGame g in p.serverPlayingKancolle)
                {
                    if (Context.Guild.Id == g.idServer)
                    {
                        g.isCorrect(answer, Context.User.Id);
                        return;
                    }
                }
                await ReplyAsync("Sorry but this game didn't start yet.");
            }
        }

        [Command("Play booru quizz"), Summary("Launch booru game")]
        public async Task playBooru(string safeOrAll)
        {
            p.doAction(Context.User, Context.Guild.Id, Program.Module.Game);
            IGuildUser me = await Context.Guild.GetUserAsync(329664361016721408); // Sanara
            if (File.ReadAllLines("Saves/Servers/" + Context.Guild.Id + "/serverDatas.dat")[6] == "0")
            {
                await ReplyAsync("Sorry but this module was disabled.");
            }
            else if (!me.GuildPermissions.AttachFiles)
            {
                await ReplyAsync("Sorry but I need to permission to attach files to play at this here.");
            }
            else if (p.serverPlayingKancolle.Any(x => x.idServer == Context.Guild.Id))
            {
                await ReplyAsync("Sorry but a game is already running on this server.");
            }
            else if (p.isLearning)
            {
                await ReplyAsync("Sorry but I'm currently busy with learning new things, playing right now would really mess up everything in my notes.");
            }
            else
            {
                if (safeOrAll == "")
                {
                    if (Context.Channel.IsNsfw)
                        safeOrAll = "all";
                    else
                        safeOrAll = "safe";
                }
                else if (safeOrAll == "all" && !Context.Channel.IsNsfw)
                {
                    await ReplyAsync("I'm not allowed to post lewd content here, sorry." + Environment.NewLine
                                    + "You can request a safe game by specifying safe instead of all.");
                    return;
                }
                else if (safeOrAll != "all" && safeOrAll != "safe")
                {
                    await ReplyAsync("I don't understand what kind of rating you want for this game.");
                    return;
                }
                string score;
                if (safeOrAll == "all")
                    score = File.ReadAllLines("Saves/Servers/" + Context.Guild.Id + "/quizzTagAll.dat")[3];
                else
                    score = File.ReadAllLines("Saves/Servers/" + Context.Guild.Id + "/quizzTagSafe.dat")[3];
                await ReplyAsync("I will give you 3 images, try to find what is the name common in these 3." + Environment.NewLine
                    + "To give the answer, just tag me in this channel and say \"quizz Booru [your answer]\"." + Environment.NewLine
                    + "If nobody find the answer after 45 seconds, the game will end." + Environment.NewLine
                    + "The current best score on this server is " + score + " point" + ((Convert.ToInt32(score) > 1) ? ("s") : ("")) + ".");
                if (safeOrAll == "all")
                    p.serverPlayingKancolle.Add(new TagBooruGame(Context.Guild.Id, Context.Channel.Id, p, (ITextChannel)Context.Channel, true));
                else
                    p.serverPlayingKancolle.Add(new TagBooruGame(Context.Guild.Id, Context.Channel.Id, p, (ITextChannel)Context.Channel, false));
                if (!p.quizzThread.IsAlive)
                    p.quizzThread.Start();
            }
        }

        [Command("quizz Kancolle"), Summary("Try to guess the character in Kancolle game")]
        public async Task guessKancolle(string answer)
        {
            p.doAction(Context.User, Context.Guild.Id, Program.Module.Game);
            if (File.ReadAllLines("Saves/Servers/" + Context.Guild.Id + "/serverDatas.dat")[6] == "0")
            {
                await ReplyAsync("Sorry but this module was disabled.");
            }
            else if (!p.serverPlayingKancolle.Any(x => x.idServer == Context.Guild.Id))
            {
                await ReplyAsync("Sorry but this game didn't start yet.");
            }
            else
            {
                foreach (KancolleGame g in p.serverPlayingKancolle)
                {
                    if (Context.Guild.Id == g.idServer)
                    {
                        g.isCorrect(answer, Context.User.Id);
                        return;
                    }
                }
                await ReplyAsync("Sorry but this game didn't start yet.");
            }
        }

        [Command("Play kancolle quizz"), Summary("Launch kancolle game")]
        public async Task playKancolle()
        {
            p.doAction(Context.User, Context.Guild.Id, Program.Module.Game);
            IGuildUser me = await Context.Guild.GetUserAsync(329664361016721408); // Sanara
            if (File.ReadAllLines("Saves/Servers/" + Context.Guild.Id + "/serverDatas.dat")[6] == "0")
            {
                await ReplyAsync("Sorry but this module was disabled.");
            }
            else if (!me.GuildPermissions.AttachFiles)
            {
                await ReplyAsync("Sorry but I need to permission to attach files to play at this here.");
            }
            else if (p.serverPlayingKancolle.Any(x => x.idServer == Context.Guild.Id))
            {
                await ReplyAsync("Sorry but a game is already running on this server.");
            }
            else if (p.isLearning)
            {
                await ReplyAsync("Sorry but I'm currently busy with learning new things, playing right now would really mess up everything in my notes.");
            }
            else
            {
                string score = File.ReadAllLines("Saves/Servers/" + Context.Guild.Id + "/kancolle.dat")[3];
                await ReplyAsync("I will give you an image of a shipgirl, try to find her name." + Environment.NewLine
                    + "To give the answer, just tag me in this channel and say \"quizz Kancolle [your answer]\"." + Environment.NewLine
                    + "If nobody find the answer after 15 seconds, the game will end." + Environment.NewLine
                    + "The current best score on this server is " + score + " point" + ((Convert.ToInt32(score) > 1) ? ("s") : ("")) + ".");
                p.serverPlayingKancolle.Add(new KancolleGame(Context.Guild.Id, Context.Channel.Id, p, (ITextChannel)Context.Channel));
                if (!p.quizzThread.IsAlive)
                    p.quizzThread.Start();
            }
        }

        [Command("Shiritori"), Summary("Give a word for shiritori game"), Alias("しりとり")]
        public async Task guessShiritori(string answer)
        {
            p.doAction(Context.User, Context.Guild.Id, Program.Module.Game);
            if (File.ReadAllLines("Saves/Servers/" + Context.Guild.Id + "/serverDatas.dat")[6] == "0")
            {
                await ReplyAsync("Sorry but this module was disabled.");
            }
            else if (!p.serverPlayingKancolle.Any(x => x.idServer == Context.Guild.Id))
            {
                await ReplyAsync("Sorry but this game didn't start yet.");
            }
            else
            {
                foreach (ShiritoriGame g in p.serverPlayingKancolle)
                {
                    if (Context.Guild.Id == g.idServer)
                    {
                        if (g.player.Id == Context.User.Id)
                            g.isCorrect(answer, Context.User.Id);
                        else
                            await ReplyAsync("I'm sorry but I'm currently playing with someone else.");
                        return;
                    }
                }
                await ReplyAsync("Sorry but this game didn't start yet.");
            }
        }

        [Command("Play shiritori"), Summary("Launch shiritori game")]
        public async Task playShiritori(string romajiOnly = "")
        {
            p.doAction(Context.User, Context.Guild.Id, Program.Module.Game);
            if (File.ReadAllLines("Saves/Servers/" + Context.Guild.Id + "/serverDatas.dat")[6] == "0")
            {
                await ReplyAsync("Sorry but this module was disabled.");
            }
            else if (p.serverPlayingKancolle.Any(x => x.idServer == Context.Guild.Id))
            {
                await ReplyAsync("Sorry but a game is already running on this server.");
            }
            else if (p.isLearning)
            {
                await ReplyAsync("Sorry but I'm currently busy with learning new things, playing right now would really mess up everything in my notes.");
            }
            else
            {
                string score = File.ReadAllLines("Saves/Users/" + Context.User.Id + ".dat")[14];
                await ReplyAsync("I give you a word, try to find another one who begin by the last letter of my word. Your words must be noun and musn't finish by a ん." + Environment.NewLine
                    + "To give the answer, just tag me in this channel and say \"Shiritori [your answer]\" (in hiragana only)." + Environment.NewLine
                    + "If nobody find the answer after 30 seconds, the game will end." + Environment.NewLine
                    + "The current best score on this server is " + score + " point" + ((Convert.ToInt32(score) > 1) ? ("s") : ("")) + ".");
                p.serverPlayingKancolle.Add(new ShiritoriGame(Context.Guild.Id, Context.Channel.Id, p, (ITextChannel)Context.Channel, Context.User, (romajiOnly == "romajiOnly")));
                if (!p.quizzThread.IsAlive)
                    p.quizzThread.Start();
            }
        }
    }

    abstract class currGame
    {
        public enum phase
        {
            didntStart,
            playing
        }
        public bool isTimeOk()
        {   
            if (timeQuestion != DateTime.MinValue && timeQuestion.AddSeconds(refTime).CompareTo(DateTime.Now) == -1)
                return (false);
            else
                return (true);
        }
        public abstract void post();
        public abstract void isCorrect(string name, ulong userId);
        public abstract void loose();

        protected DateTime timeQuestion { set; get; }
        protected int refTime { set; private get; }
        public ulong idServer { protected set; get; }
        protected ulong idChan;
        public phase currPhase;
        protected Program b;
        protected ITextChannel chan;
        public bool lost { protected set; get; }
    }

    class TagBooruGame : currGame
    {
        public TagBooruGame(ulong midServer, ulong midChan, Program bot, ITextChannel mchan, bool misRatingAll)
        {
            idServer = midServer;
            idChan = midChan;
            currPhase = phase.didntStart;
            b = bot;
            timeQuestion = DateTime.MinValue;
            tagFound = 0;
            tagAttempt = 0;
            refTime = 45;
            chan = mchan;
            lost = false;
            userIds = new List<ulong>();
            isRatingAll = misRatingAll;
        }

        public override async void post()
        {
            try
            {
                int randomNbMax;
                string randomTag = "";
                string xml = "";
                int randomP;
                string li;
                string directory = "Saves/booruAnalysis/";
                if (isRatingAll)
                    directory += "Gelbooru/";
                else
                    directory += "Safebooru/";
                string[] allFiles = Directory.GetFiles(directory);
                while (true)
                {
                    try
                    {
                        string[] datas = File.ReadAllLines(allFiles[b.rand.Next(0, allFiles.Length)]);
                        if (datas[0] == "0")
                        {
                            randomTag = datas[4];
                            break;
                        }
                    }
                    catch (IndexOutOfRangeException)
                    { }
                }
                List<string> urls = new List<string>();
                try
                {
                    if (isRatingAll)
                        xml = BooruModule.getWebRequest("https://gelbooru.com/index.php?page=dapi&s=post&q=index&limit=1&tags=" + randomTag);
                    else
                        xml = BooruModule.getWebRequest("http://safebooru.org/index.php?page=dapi&s=post&q=index&limit=1&tags=" + randomTag);
                    randomNbMax = Convert.ToInt32(BooruModule.getElementXml("posts count=\"", "", xml, '"')) - 1;
                } catch (FormatException)
                {
                    await chan.SendMessageAsync("The website refused to give me an image, please retry again later.");
                    loose();
                    return;
                }
                for (int y = 0; y < 3; y++)
                {
                    string url;
                    randomP = b.rand.Next(randomNbMax) + 1;
                    if (isRatingAll)
                    {
                        li = "https://gelbooru.com/index.php?page=dapi&s=post&q=index&pid=" + randomP + "&limit=1&tags=" + randomTag;
                        xml = BooruModule.getWebRequest(li);
                        url = BooruModule.getElementXml("file_url=\"", "", xml, '"');
                    }
                    else
                    {
                        li = "http://safebooru.org/index.php?page=dapi&s=post&q=index&pid=" + randomP + "&limit=1&tags=" + randomTag;
                        xml = BooruModule.getWebRequest(li);
                        url = BooruModule.getElementXml("file_url=\"//", "https://", xml, '"');
                    }
                    urls.Add(url);
                }
                List<string> toDelete = new List<string>();
                int i = 0;
                foreach (string s in urls)
                {
                    int currentTime = Convert.ToInt32(DateTime.Now.ToString("HHmmss"));
                    using (var client = new WebClient())
                    {
                        client.DownloadFile(s, "imageKonachan" + currentTime + i + "." + s.Split('.')[s.Split('.').Length - 1]);
                    }
                    FileInfo file = new FileInfo("imageKonachan" + currentTime + i + "." + s.Split('.')[s.Split('.').Length - 1]);
                    if (file.Length >= 8000000)
                    {
                        await chan.SendMessageAsync("I wasn't able to send one of the images since its size was superior to 8MB.");
                        File.Delete("imageKonachan" + currentTime + i + "." + s.Split('.')[s.Split('.').Length - 1]);
                    }
                    else
                        toDelete.Add("imageKonachan" + currentTime + i + "." + s.Split('.')[s.Split('.').Length - 1]);
                    i++;
                }
                foreach (string s in toDelete)
                {
                    await chan.SendFileAsync(s);
                    File.Delete(s);
                }
                toGuess = BooruModule.fixName(randomTag);
                timeQuestion = DateTime.Now;
            }
            catch (WebException)
            {
                lost = true;
                await chan.SendMessageAsync("An error occured while trying to request an image, I can't continue this game, sorry...");
            }
        }

        public override async void isCorrect(string name, ulong userId)
        {
            if (timeQuestion == DateTime.MinValue)
            {
                await chan.SendMessageAsync("Please wait until I posted all the images.");
            }
            else
            {
                tagAttempt++;
                Character me = null;
                foreach (Character c in b.relations)
                {
                    if (c.getName() == userId)
                    {
                        me = c;
                        break;
                    }
                }
                if (toGuess == BooruModule.fixName(name))
                {
                    timeQuestion = DateTime.MinValue;
                    if (isRatingAll)
                        me.increaseGuessBooruAll(true);
                    else
                        me.increaseGuessBooruAll(true);
                    if (!userIds.Contains(userId))
                        userIds.Add(userId);
                    await chan.SendMessageAsync("Congratulation, you found the right answer!" + Environment.NewLine
                        + "I'll download the 3 next images.");
                    post();
                    tagFound++;
                }
                else
                {
                    if (isRatingAll)
                        me.increaseGuessBooruAll(false);
                    else
                        me.increaseGuessBooruAll(false);
                    await chan.SendMessageAsync("No, this is not " + name + ".");
                }
            }
        }

        public override async void loose()
        {
            string[] datas;
            if (isRatingAll)
                datas = File.ReadAllLines("Saves/Servers/" + idServer + "/quizzTagAll.dat");
            else
                datas = File.ReadAllLines("Saves/Servers/" + idServer + "/quizzTagSafe.dat");
            string allUsers = "";
            if (tagFound > Convert.ToInt32(datas[3]))
            {
                foreach (ulong u in userIds)
                {
                    allUsers += u.ToString() + "|";
                }
                if (allUsers != "")
                    allUsers = allUsers.Substring(0, allUsers.Length - 1);
            }
            else
            {
                allUsers = datas[4];
            }
            if (isRatingAll)
                File.WriteAllText("Saves/Servers/" + idServer + "/quizzTagAll.dat",
                    (Convert.ToInt32(datas[0]) + 1).ToString() + Environment.NewLine +
                    (Convert.ToInt32(datas[1]) + tagAttempt).ToString() + Environment.NewLine +
                    (Convert.ToInt32(datas[2]) + tagFound).ToString() + Environment.NewLine +
                    ((tagFound > Convert.ToInt32(datas[3])) ? (tagFound.ToString()) : (Convert.ToInt32(datas[3]).ToString())) + Environment.NewLine +
                    allUsers);
            else
                File.WriteAllText("Saves/Servers/" + idServer + "/quizzTagSafe.dat",
                    (Convert.ToInt32(datas[0]) + 1).ToString() + Environment.NewLine +
                    (Convert.ToInt32(datas[1]) + tagAttempt).ToString() + Environment.NewLine +
                    (Convert.ToInt32(datas[2]) + tagFound).ToString() + Environment.NewLine +
                    ((tagFound > Convert.ToInt32(datas[3])) ? (tagFound.ToString()) : (Convert.ToInt32(datas[3]).ToString())) + Environment.NewLine +
                    allUsers);
            string finalStr = "Time out, the answer was " + toGuess + "." + Environment.NewLine;
            if (tagFound > Convert.ToInt32(datas[3]))
                finalStr += "Congratulation, you beat the previous best score of " + Convert.ToInt32(datas[3]) + " with a new score of " + tagFound + ".";
            else if (tagFound == Convert.ToInt32(datas[3]))
                finalStr += "You equilized the previous best score of " + tagFound + ".";
            else
                finalStr += "You didn't beat the current best score of " + Convert.ToInt32(datas[3]) + " with the score of " + tagFound + ".";
            await chan.SendMessageAsync(finalStr);
        }

        public bool isRatingAll { set; get; }
        public string toGuess { set; get; }
        public int tagFound { private set; get; }
        public int tagAttempt { private set; get; }
        List<ulong> userIds;
    }

    class ShiritoriGame : currGame
    {
        public ShiritoriGame(ulong midServer, ulong midChan, Program bot, ITextChannel mchan, IUser mplayer, bool mromaji)
        {
            idServer = midServer;
            idChan = midChan;
            currPhase = phase.didntStart;
            b = bot;
            timeQuestion = DateTime.Now;
            refTime = 30;
            chan = mchan;
            words = File.ReadAllLines("Saves/shiritoriWords.dat").ToList();
            currentWord = null;
            lost = false;
            alreadySaid = new List<string>();
            player = mplayer;
            score = 0;
            relationPlayer = b.relations.Find(x => x.getName() == player.Id);
            romajiOnly = mromaji;
        }

        public override async void post()
        {
            if (currentWord == null)
            {
                currentWord = "しりとり";
                if (romajiOnly)
                    await chan.SendMessageAsync("shiritori");
                else
                    await chan.SendMessageAsync("しりとり");
            }
            else
            {
                string[] corrWords = words.Where(x => x[0] == currentWord[currentWord.Length - 1]).ToArray();
                if (corrWords.Length == 0)
                {
                    await chan.SendMessageAsync("I don't know any other word...");
                }
                else
                {
                    string word = corrWords[b.rand.Next(0, corrWords.Length)];
                    string[] insideWord = word.Split('$');
                    if (romajiOnly)
                        await chan.SendMessageAsync(JishoModule.toRomaji(insideWord[0]) + " - Meaning: " + insideWord[1]);
                    else
                        await chan.SendMessageAsync(insideWord[0] + " - Meaning: " + insideWord[1]);
                    words.Remove(insideWord[0]);
                    currentWord = insideWord[0];
                    alreadySaid.Add(insideWord[0]);
                }
            }
            timeQuestion = DateTime.Now;
        }

        public override async void isCorrect(string name, ulong userId)
        {
            if (timeQuestion == DateTime.MinValue)
            {
                await chan.SendMessageAsync("Please wait until I posted all the images.");
            }
            else
            {
                name = JishoModule.toHiragana(name);
                foreach (char c in name)
                {
                    if (c < 0x3041 && c > 0x3096)
                    {
                        await chan.SendMessageAsync("I'm sorry but I'm only handling hiragana and romaji for now.");
                        return;
                    }
                }
                string json;
                using (WebClient wc = new WebClient())
                {
                    wc.Encoding = Encoding.UTF8;
                    json = wc.DownloadString("http://www.jisho.org/api/v1/search/words?keyword=" + name);
                }
                bool isCorrect = false;
                foreach (string s in BooruModule.getElementXml("\"japanese\":[", "", json, '$').Split(new string[] { "\"japanese\":[" }, StringSplitOptions.None))
                {
                    string hiragana = BooruModule.getElementXml("\"reading\":\"", "", s, '"');
                    if (name == hiragana)
                    {
                        isCorrect = true;
                        if (BooruModule.getElementXml("parts_of_speech\":[\"", "", json, '"') != "Noun")
                        {
                            await chan.SendMessageAsync("This word isn't a noun.");
                            relationPlayer.increaseShiritoriAttempt(false);
                            return;
                        }
                        break;
                    }
                }
                if (!isCorrect)
                {
                    await chan.SendMessageAsync("This word doesn't exist.");
                    relationPlayer.increaseShiritoriAttempt(false);
                    return;
                }
                if (name[0] != currentWord[currentWord.Length - 1])
                {
                    await chan.SendMessageAsync("Your word must begin by a " + currentWord[currentWord.Length - 1] + ".");
                    relationPlayer.increaseShiritoriAttempt(false);
                    return;
                }
                if (alreadySaid.Contains(name))
                {
                    await chan.SendMessageAsync("This word was already said.");
                    lost = true;
                    relationPlayer.increaseShiritoriAttempt(false);
                    return;
                }
                if (name[name.Length - 1] == 'ん')
                {
                    await chan.SendMessageAsync("Your word is finishing with a ん.");
                    lost = true;
                    relationPlayer.increaseShiritoriAttempt(false);
                    return;
                }
                timeQuestion = DateTime.MinValue;
                words.Remove(name);
                currentWord = name;
                post();
                relationPlayer.increaseShiritoriAttempt(true);
                score++;
            }
        }

        public override async void loose()
        {
            string finalStr = "You lost." + Environment.NewLine;
            string[] corrWords = words.Where(x => x[0] == currentWord[currentWord.Length - 1]).ToArray();
            if (corrWords.Length == 0)
            {
                finalStr += "To be honest, I didn't know a word to answer too." + Environment.NewLine;
            }
            else
            {
                string word = corrWords[b.rand.Next(0, corrWords.Length)];
                string[] insideWord = word.Split('$');
                finalStr += "Here's a word you could have say: " + JishoModule.toRomaji(insideWord[0]) + " - Meaning: " + insideWord[1] + Environment.NewLine;
            }
            relationPlayer.increaseShiritoriGames();
            if (score > relationPlayer.getShiritoriBestScore())
                finalStr += "Congratulation, you beat the previous best score of " + relationPlayer.getShiritoriBestScore() + " with a new score of " + score + "." + Environment.NewLine;
            else if (score == relationPlayer.getShiritoriBestScore())
                finalStr += "You equilized the previous best score of " + score + ".";
            else
                finalStr += "You didn't beat the current best score of " + relationPlayer.getShiritoriBestScore() + " with the score of " + score + "." + Environment.NewLine;
            relationPlayer.setShiritoriScore(score);
            await chan.SendMessageAsync(finalStr);
        }
        
        List<string> words;
        string currentWord;
        List<string> alreadySaid;
        public IUser player { private set; get; }
        Character relationPlayer;
        int score;
        bool romajiOnly;
    }

    class KancolleGame : currGame
    {
        public KancolleGame(ulong midServer, ulong midChan, Program bot, ITextChannel mchan)
        {
            idServer = midServer;
            idChan = midChan;
            currPhase = phase.didntStart;
            b = bot;
            idImage = "-1";
            timeQuestion = DateTime.Now;
            shipFound = 0;
            shipAttempt = 0;
            refTime = 15;
            chan = mchan;
            lost = false;
            userIds = new List<ulong>();
        }

        public override async void post()
        {
            toGuess = b.names[b.rand.Next(b.names.Count)];
            using (WebClient w = new WebClient())
            {
                w.Encoding = Encoding.UTF8;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                string url = "https://kancolle.wikia.com/api/v1/Search/List?query=" + toGuess + "&limit=1";
                string json = w.DownloadString(url);
                string code = BooruModule.getElementXml("\"id\":", "", json, ',');
                idImage = code;
                url = "https://kancolle.wikia.com/api/v1/Search/List?query=" + toGuess + "/Gallery&limit=1";
                json = w.DownloadString(url);
                code = BooruModule.getElementXml("\"id\":", "", json, ',');
                url = "http://kancolle.wikia.com/api/v1/Articles/Details?ids=" + code;
                json = w.DownloadString(url);
                string image = BooruModule.getElementXml("\"thumbnail\":\"", "", json, '"');
                image = image.Split(new string[] { ".jpg" }, StringSplitOptions.None)[0] + ".jpg";
                image = image.Replace("\\", "");
                int currentTime = Convert.ToInt32(DateTime.Now.ToString("HHmmss"));
                w.DownloadFile(image, "shipgirlquizz" + currentTime + ".jpg");
                await chan.SendFileAsync("shipgirlquizz" + currentTime + ".jpg");
                File.Delete("shipgirlquizz" + currentTime + ".jpg");
                timeQuestion = DateTime.Now;
            }
        }

        public override async void isCorrect(string name, ulong userId)
        {
            if (timeQuestion == DateTime.MinValue)
            {
                await chan.SendMessageAsync("Please wait until I posted the image.");
            }
            else
            {
                shipAttempt++;
                Character me = null;
                foreach (Character c in b.relations)
                {
                    if (c.getName() == userId)
                    {
                        me = c;
                        break;
                    }
                }
                try
                {
                    bool isSpace = true;
                    string newName = "";
                    foreach (char c in name)
                    {
                        if (c == ' ')
                        {
                            isSpace = true;
                            newName += ' ';
                        }
                        else
                        {
                            if (isSpace)
                                newName += char.ToUpper(c);
                            else
                                newName += c;
                            isSpace = false;
                        }
                    }
                    using (WebClient w = new WebClient())
                    {
                        string url = "https://kancolle.wikia.com/api/v1/Search/List?query=" + newName + "&limit=1";
                        string json = w.DownloadString(url);
                        string code = BooruModule.getElementXml("\"title\":\"", "", json, '"');
                        url = "http://kancolle.wikia.com/wiki/" + code + "?action=raw";
                        url = url.Replace(' ', '_');
                        json = w.DownloadString(url);
                        if (BooruModule.getElementXml("{{", "", json, '}') != "ShipPageHeader")
                        {
                            me.increaseGuessKancolle(false);
                            await chan.SendMessageAsync("There is no shipgirl with this name...");
                        }
                        else
                        {
                            me.increaseGuessKancolle(true);
                            w.Encoding = Encoding.UTF8;
                            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                            url = "https://kancolle.wikia.com/api/v1/Search/List?query=" + newName + "&limit=1";
                            json = w.DownloadString(url);
                            code = BooruModule.getElementXml("\"id\":", "", json, ',');
                            if (idImage == code)
                            {
                                timeQuestion = DateTime.MinValue;
                                if (!userIds.Contains(userId))
                                    userIds.Add(userId);
                                await chan.SendMessageAsync("Congratulation, you found the right answer!");
                                post();
                                shipFound++;
                            }
                            else
                            {
                                me.increaseGuessKancolle(false);
                                await chan.SendMessageAsync("No, this is not " + newName + ".");
                            }
                        }
                    }
                }
                catch (WebException ex)
                {
                    me.increaseGuessKancolle(false);
                    HttpWebResponse code = ex.Response as HttpWebResponse;
                    if (code.StatusCode == HttpStatusCode.NotFound)
                        await chan.SendMessageAsync("There is no shipgirl with this name...");
                }
            }
        }

        public override async void loose()
        {
            string[] datas = File.ReadAllLines("Saves/Servers/" + idServer + "/kancolle.dat");
            string allUsers = "";
            if (shipFound > Convert.ToInt32(datas[3]))
            {
                foreach (ulong u in userIds)
                {
                    allUsers += u.ToString() + "|";
                }
                if (allUsers != "")
                    allUsers = allUsers.Substring(0, allUsers.Length - 1);
            }
            else
            {
                allUsers = datas[4];
            }
            File.WriteAllText("Saves/Servers/" + idServer + "/kancolle.dat",
                (Convert.ToInt32(datas[0]) + 1).ToString() + Environment.NewLine +
                (Convert.ToInt32(datas[1]) + shipAttempt).ToString() + Environment.NewLine +
                (Convert.ToInt32(datas[2]) + shipFound).ToString() + Environment.NewLine +
                ((shipFound > Convert.ToInt32(datas[3])) ? (shipFound.ToString()) : (Convert.ToInt32(datas[3]).ToString())) + Environment.NewLine +
                allUsers);
            string finalStr = "Time out, the answer was " + toGuess + "." + Environment.NewLine;
            if (shipFound > Convert.ToInt32(datas[3]))
                finalStr += "Congratulation, you beat the previous best score of " + Convert.ToInt32(datas[3]) + " with a new score of " + shipFound + ".";
            else if (shipFound == Convert.ToInt32(datas[3]))
                finalStr += "You equilized the previous best score of " + shipFound + ".";
            else
                finalStr += "You didn't beat the current best score of " + Convert.ToInt32(datas[3]) + " with the score of " + shipFound + ".";
            await chan.SendMessageAsync(finalStr);
        }

        public string toGuess { set; get; }
        public int shipFound { private set; get; }
        public int shipAttempt { private set; get; }
        string idImage;
        List<ulong> userIds;
    }

    partial class Program
    {
        public void prepareQuizz()
        {
            using (WebClient w = new WebClient())
            {
                w.Encoding = Encoding.UTF8;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                string json = w.DownloadString("http://kancolle.wikia.com/wiki/Ship?action=raw");
                string[] cathegories = json.Split(new string[] { "==" }, StringSplitOptions.None);
                bool didBegan = false;
                string shipName = "";
                bool beginRead = false;
                bool readBeginLine = true;
                p.names = new List<string>();
                foreach (char c in cathegories[2]) // Get all ship's name
                {
                    if (!didBegan && c == '<')
                    {
                        didBegan = true;
                    }
                    else if (didBegan)
                    {
                        if (c == '[' && readBeginLine)
                        {
                            beginRead = true;
                            shipName = "";
                        }
                        else if ((c == '|' || c == ']') && shipName != "" && beginRead)
                        {
                            p.names.Add(shipName);
                            shipName = "";
                            beginRead = false;
                        }
                        else if (c == '\n')
                            readBeginLine = true;
                        else
                        {
                            shipName += c;
                            readBeginLine = false;
                        }
                    }
                }
            }
        }

        public void playKancolleQuizz()
        {
            while (Thread.CurrentThread.IsAlive)
            {
                for (int i = p.serverPlayingKancolle.Count - 1; i >= 0; i--)
                {
                    try
                    {
                        if (p.serverPlayingKancolle[i].currPhase == currGame.phase.didntStart)
                        {
                            p.serverPlayingKancolle[i].post();
                            p.serverPlayingKancolle[i].currPhase = currGame.phase.playing;
                        }
                        else
                        {
                            if (!p.serverPlayingKancolle[i].isTimeOk() || p.serverPlayingKancolle[i].lost)
                            {
                                p.serverPlayingKancolle[i].loose();
                                p.serverPlayingKancolle.RemoveAt(i);
                                if (p.serverPlayingKancolle.Count == 0)
                                {
                                    /*quizzThread.Abort();
                                    return;*/
                                }
                            }
                        }
                    }
                    catch (NullReferenceException) { }
                }
            }
        }

        private void learnShiritori()
        {
            int nbCharacs = rand.Next(2, 11);
            string finalStr = "";
            for (int i = 0; i < nbCharacs; i++)
                finalStr += (char)(rand.Next(0x3041, 0x3095));
            string json;
            using (WebClient wc = new WebClient())
            {
                wc.Encoding = Encoding.UTF8;
                json = wc.DownloadString("http://www.jisho.org/api/v1/search/words?keyword=" + finalStr);
            }
            string code = BooruModule.getElementXml("status\":", "", json, '}');
            string word = BooruModule.getElementXml("english_definitions\":[", "", json, ']');
            string type = BooruModule.getElementXml("parts_of_speech\":[\"", "", json, '"');
            if (word == "")
                notWord++;
            else if (type != "Noun")
                notNoun++;
            else
            {
                string hiragana = BooruModule.getElementXml("\"reading\":\"", "", json, '"');
                word = hiragana + "$" + word;
                if (hiragana.Length == 1)
                    tooShort++;
                else if (hiragana[hiragana.Length - 1] == 'ん')
                    invalidWord++;
                else
                {
                    bool alreadyKnowed = false;
                    foreach (string s in knowedWords)
                    {
                        if (hiragana == s.Split('$')[0])
                        {
                            alreadyKnowed = true;
                            break;
                        }
                    }
                    if (alreadyKnowed)
                        alreadyKnow++;
                    else
                    {
                        knowedWords.Add(word);
                        newWords++;
                    }
                }
            }
        }
    }
}