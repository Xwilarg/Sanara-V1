using Discord.Commands;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Bot
{
    public class SettingsModule : ModuleBase
    {
        Program p = Program.p;
        private void copyContent(string source, string destination)
        {
            source = source.Replace('\\', '/');
            destination = destination.Replace('\\', '/');
            foreach (string f in Directory.GetFiles(source))
            {
                string f2 = f.Replace('\\', '/');
                File.Copy(f2, destination + "/" + f2.Split('/')[f2.Split('/').Length - 1]);
            }
            foreach (string d in Directory.GetDirectories(source))
            {
                string d2 = d.Replace('\\', '/');
                Directory.CreateDirectory(destination + "/" + d2.Split('/')[d2.Split('/').Length - 1]);
                copyContent(d2, destination + "/" + d2.Split('/')[d2.Split('/').Length - 1]);
            }
        }

        [Command("Archive", RunMode = RunMode.Async), Summary("Create an archive for all datas")]
        public async Task archive()
        {
            p.doAction(Context.User, Context.Guild.Id, Program.Module.Settings);
            if (Context.User.Id != 144851584478740481)
            {
                await ReplyAsync(Sentences.onlyMasterStr);
            }
            else
            {
                await ReplyAsync("Please let me some time so I can copy all my files.");
                p.saveDatas(Context.Guild.Id);
                if (!Directory.Exists("Archives"))
                    Directory.CreateDirectory("Archives");
                string currTime = DateTime.UtcNow.ToString("yy-MM-dd-HH-mm-ss");
                Directory.CreateDirectory("Archives/" + currTime);
                copyContent("Saves", "Archives/" + currTime);
                await ReplyAsync(Sentences.createArchiveStr(currTime));
            }
        }

        [Command("Set default channel"), Summary("Set the default channel")]
        public async Task setDefaultChannel()
        {
            p.doAction(Context.User, Context.Guild.Id, Program.Module.Settings);
            if (Context.User.Id != 144851584478740481)
            {
                await ReplyAsync(Sentences.onlyMasterStr);
            }
            else
            {
                if (Convert.ToUInt64(File.ReadAllLines("Saves/Servers/" + Context.Guild.Id + "/serverDatas.dat")[1]) == Context.Channel.Id)
                    await ReplyAsync(Sentences.alreadyDefaultChannelStr);
                else
                {
                    string[] file;
                    file = File.ReadAllLines("Saves/Servers/" + Context.Guild.Id + "/serverDatas.dat");
                    File.WriteAllText("Saves/Servers/" + Context.Guild.Id + "/serverDatas.dat",
                        file[0] + Environment.NewLine + Context.Channel.Id + Environment.NewLine + file[2] + Environment.NewLine + file[3]
                         + Environment.NewLine + file[4] + Environment.NewLine + file[5] + Environment.NewLine + file[6]);
                    await ReplyAsync(Sentences.setDefaultChannelStr);
                }
            }
        }

        [Command("Set default server"), Summary("Set the default server gor error messages")]
        public async Task setDefaultServer()
        {
            p.doAction(Context.User, Context.Guild.Id, Program.Module.Settings);
            if (Context.User.Id != 144851584478740481)
            {
                await ReplyAsync(Sentences.onlyMasterStr);
            }
            else
            {
                if (Convert.ToUInt64(File.ReadAllLines("Saves/sanaraDatas.dat")[2]) == Context.Guild.Id)
                    await ReplyAsync(Sentences.alreadyDefaultServerStr);
                else
                {

                    string[] content = File.ReadAllLines("Saves/sanaraDatas.dat");
                    File.WriteAllText("Saves/sanaraDatas.dat", content[0] + Environment.NewLine + content[1] + Environment.NewLine + Context.Guild.Id);
                    await ReplyAsync(Sentences.setDefaultServerStr);
                }
            }
        }

        [Command("Enable welcome"), Summary("Let the bot say hi when he the server become available")]
        public async Task enableWelcome()
        {
            p.doAction(Context.User, Context.Guild.Id, Program.Module.Settings);
            if (Context.User.Id != 144851584478740481)
            {
                await ReplyAsync(Sentences.onlyMasterStr);
            }
            else if (File.ReadAllLines("Saves/Servers/" + Context.Guild.Id + "/serverDatas.dat")[3] == "1")
            {
                await ReplyAsync(Sentences.alreadyKnowStr);
            }
            else
            {
                string[] file;
                file = File.ReadAllLines("Saves/Servers/" + Context.Guild.Id + "/serverDatas.dat");
                File.WriteAllText("Saves/Servers/" + Context.Guild.Id + "/serverDatas.dat", file[0] + Environment.NewLine + file[1] + Environment.NewLine + file[2] + Environment.NewLine + "1"
                    + Environment.NewLine + file[4] + Environment.NewLine + file[5] + Environment.NewLine + file[6]);
                await ReplyAsync(Sentences.sayHiJoinStr);
            }
        }

        [Command("Disable welcome"), Summary("Don't let the bot say hi when he the server become available")]
        public async Task disableWelcome()
        {
            p.doAction(Context.User, Context.Guild.Id, Program.Module.Settings);
            if (Context.User.Id != 144851584478740481)
            {
                await ReplyAsync(Sentences.onlyMasterStr);
            }
            else if (File.ReadAllLines("Saves/Servers/" + Context.Guild.Id + "/serverDatas.dat")[3] == "0")
            {
                await ReplyAsync(Sentences.alreadyKnowStr);
            }
            else
            {
                string[] file;
                file = File.ReadAllLines("Saves/Servers/" + Context.Guild.Id + "/serverDatas.dat");
                File.WriteAllText("Saves/Servers/" + Context.Guild.Id + "/serverDatas.dat", file[0] + Environment.NewLine + file[1] + Environment.NewLine + file[2] + Environment.NewLine + "0"
                    + Environment.NewLine + file[4] + Environment.NewLine + file[5] + Environment.NewLine + file[6]);
                await ReplyAsync(Sentences.dontSayHiJoinStr);
            }
        }


        [Command("Leave server"), Summary("Leave the server")]
        public async Task leave()
        {
            p.doAction(Context.User, Context.Guild.Id, Program.Module.Settings);
            if (Context.User.Id != 144851584478740481)
            {
                await ReplyAsync(Sentences.onlyMasterStr);
            }
            else
            {
                await ReplyAsync(Sentences.leaverServer);
                await Context.Guild.LeaveAsync();
            }
        }

        [Command("Quit"), Summary("Quit")]
        public async Task quit()
        {
            p.doAction(Context.User, Context.Guild.Id, Program.Module.Settings);
            if (Context.User.Id != 144851584478740481)
            {
                await ReplyAsync(Sentences.onlyMasterStr);
            }
            else if (p.isLearning)
            {
                await ReplyAsync("Please don't shut me down, I'm currently learning.");
            }
            else if (p.serverPlayingKancolle.Count > 0)
            {
                await ReplyAsync("Please don't shut me down, I'm currently playing with someone.");
            }
            else
            {
                string[] content = File.ReadAllLines("Saves/sanaraDatas.dat");
                File.WriteAllText("Saves/sanaraDatas.dat", content[0] + Environment.NewLine + "0" + Environment.NewLine + content[2]);
                //arenaThread.Abort();
                await ReplyAsync("I'm ready to be shut down, master.");
            }
        }
    }
}