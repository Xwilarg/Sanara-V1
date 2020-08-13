using Discord;
using Discord.Commands;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Bot
{
    public class CommunicationModule : ModuleBase
    {
        Program p = Program.p;

        [Command("Commands"), Summary("Give the help"), Alias("Help", "Print help", "Print commands")]
        public async Task help()
        {
            p.doAction(Context.User, Context.Guild.Id, Program.Module.Communication);
            string help = "Here is what I'm able to do for now:" + Environment.NewLine +
                                 "**Booru**" + Environment.NewLine +
                                 "Booru/Konachan/Gelbooru/Lolibooru/Safebooru image/xml/icon [rating=safe/questionable/explicit/all (default=all)] [postBy=url/download (default=download)] [mode=debug/normal (default=normal)] [tags (optional)]: Take a random image/xml/icon from Booru/Konachan/Gelbooru/Lolibooru/Safebooru given a rating" + Environment.NewLine +
                                 "Stats tag [tag]: Display how a tag is rate for each booru." + Environment.NewLine +
                                 "Definition tag [tag]: Get the definition of a tag using Konachan wiki." + Environment.NewLine +
                                 "Help booru: Display the help about the booru websites I can search images on." + Environment.NewLine +
                                 "**Communication**" + Environment.NewLine +
                                 "Commands: Display this help." + Environment.NewLine +
                                 //"Commands full: Display a more detailled help." + Environment.NewLine +
                                 "**Debug**" + Environment.NewLine +
                                 "Print debug informations: Display some debug informations." + Environment.NewLine +
                                 "Display TODO list: Display the next things that I'm going to learn." + Environment.NewLine + 
                                 "**Console**" + Environment.NewLine +
                                 "No command available." + Environment.NewLine;
            string help2 = "**Game**" + Environment.NewLine +
                                 "Play kancolle quizz: Try to guess what are the names of the Kantai Collection character I'm displaying." + Environment.NewLine +
                                 "Play shiritori [(optional) romajiOnly]: I will give you a word in hiragana, you must answer by a word beginning by the last hiragana of my word." + Environment.NewLine +
                                 "Play booru quizz [safe/all]: I will give you 3 images, you must guess what is the common tag between these 3." + Environment.NewLine +
                                 "**Google Shortener**" + Environment.NewLine +
                                 "Random goo.gl URL: Give a random functional link from goo.gl." + Environment.NewLine +
                                 "**Jisho**" + Environment.NewLine +
                                 "Jisho meaning [word (can be in hiragana, katakana, kanji, romanji or english)] [all]: Give the Japanese translation using Jisho." + Environment.NewLine +
                                 "Jisho combinaison [all kanjis without spaces or a pastebin link] [kanji/hiragana (default=kanji)] [definition/noDefinition (default=definition)] [all/kanjiOnly (default=kanjiOnly)] [displayRanking/dontDisplayRanking (default=dontDisplayRanking)]:" +
                                                    "Give all possible combinaisons with the kanjis gave in parameter and their meaning. If you don't specify 'all', I'll ignore words containing hiragana." + Environment.NewLine +
                                 "To hiragana [word in romaji]: Give a word in hiragana given a word in romaji." + Environment.NewLine +
                                 "To romaji [word in hiragana]: Give a word in romaji given a word in hiragana." + Environment.NewLine +
                                 "**Kancolle**" + Environment.NewLine +
                                 "Kancolle [ship name]: Return the informations about the shipgirl given in parameter." + Environment.NewLine +
                                  "**Nhentai**" + Environment.NewLine +
                                 "Nhentai search [tags (optional)]: Give a doujinshi from nhentai using the tags given in parameter." + Environment.NewLine +
                                 "Nhentai load [id] [send method (download/url, default=download): Load a doujinshi given an id and display its first page." + Environment.NewLine +
                                 "Nhentai next: Display the next page of the current doujinshi." + Environment.NewLine +
                                 "Nhentai previous: Display the previous page of the current doujinshi." + Environment.NewLine +
                                 "Nhentai jump [page number]: Display the next page given in parameter of the current doujinshi." + Environment.NewLine +
                                 "Nhentai stop: Unload the current doujinshi." + Environment.NewLine +
                                 "**Settings**" + Environment.NewLine +
                                 "No command available" + Environment.NewLine;
            string help3 = "**Task**" + Environment.NewLine +
                                "Create category [name]: Create a new task's category." + Environment.NewLine +
                                "Delete category [name]: Delete a task's category." + Environment.NewLine +
                                "Create task [category] [name] [(optional)description]: Create a new task given its category and its name." + Environment.NewLine +
                                "Delete task [category] [name]: Delete a task given its category and its name." + Environment.NewLine +
                                "Display task [(optional)category]: Display all the currently existing task." + Environment.NewLine +
                                "**Youtube**" + Environment.NewLine +
                                "Get youtube video playlist [id] [nbVideoMax < 50 (default: 50)]: Get a random video of the channel id given in parameter. Get the nbVideoMax firsts video of the channel." + Environment.NewLine +
                                "Get youtube video keyword [keyword] [nbVideoMax < 50 (default: 50)]: Get a random video of the keyword given in parameter. Get the nbVideoMax firsts video of the search." + Environment.NewLine;
            EmbedBuilder embed = new EmbedBuilder()
            {
                Description = help,
                Color = Color.Purple,
            };
            await ReplyAsync("", false, embed);
            embed.Description = help2;
            await ReplyAsync("", false, embed);
            embed.Description = help3;
            await ReplyAsync("", false, embed);
        }

        /*
        [Command("Commands full"), Summary("Give the full help"), Alias("Help full", "Print help full", "Print commands full")]
        public async Task helpFull()
        {
            p.doAction(Context.User, Context.Guild.Id);
            string help = "Here is the full list of available commands with complete explanations for all of them:" + Environment.NewLine +
                                 "Please make not that I'm saving some informations about each users, but I'm not saving any private information or any message sent." + Environment.NewLine +
                                 "~~**Arena**~~" + Environment.NewLine +
                                 "~~*This module allow you to use the arena. It's a place where bots can fight again each other. This module can only be used in the development server.*~~" + Environment.NewLine +
                                 "~~Help arena: display all the help you will need to create and understand how the arena is working.~~" + Environment.NewLine +
                                 "~~Display arena victories: display the arena's victory rate in percentage for each other who fought enough.~~" + Environment.NewLine +
                                 "**Booru**" + Environment.NewLine +
                                 "*This module allow you go get images from some 'booru' websites.*" + Environment.NewLine +
                                 "Booru/Konachan/Gelbooru/Lolibooru/Safebooru image/xml/icon [rating=safe/questionable/explicit/all (default=all)] [postBy=url/download (default=download)] [tags]: Take a random image, icon or XML from a booru website." +
                                 " The following argument are optional ; Rating: define the 'lewdness' of the image, must be 'safe', 'questionable', 'explicit' or 'all'. PostBy: define how the file will be display, either 'download' if the file will be download then post, or 'url' to have an url to it." +
                                 " Tags: rules on tags are more complicate, you can search for an anime/manga (by replacing spaces by underscore), like kantai_collection, you can search for a character with the last name and the first name, like tsutsukakushi_tsukiko. You can also use more generic tags like blue_hair or undressing." + Environment.NewLine +
                                 "Help booru: Display more detailled explanations about each booru website available, and more details about what you can find on each of them." + Environment.NewLine +
                                 "**Communication**" + Environment.NewLine +
                                 "*This module is about everything that you can say to me to have discutions. Most of them are hidden and not show in the command list.*" + Environment.NewLine +
                                 "Commands: Display a lighter version of this help." + Environment.NewLine +
                                 "Commands full: Display this help." + Environment.NewLine;
            string help2 = "**Debug**" + Environment.NewLine +
                                  "*This module is about every detailled things I can give, like statistics or that kind of things.*" + Environment.NewLine +
                                  "Print debug informations: Display general informations about the server, and about each modules." + Environment.NewLine +
                                  "Display TODO list: Display the next things that I'm going to learn for each modules." + Environment.NewLine +
                                  "Display CPU and RAM usage: Display how much CPU and RAM I'm using." + Environment.NewLine +
                                  "**Game**" + Environment.NewLine +
                                  "*This module allow you to play at games.*" + Environment.NewLine +
                                  "Play kancolle quizz: I will display images of Kantai Collection's character and you will have to give me the name of the character." + Environment.NewLine +
                                  "**Jisho**" + Environment.NewLine +
                                  "*This module allow you to interact with Jisho, who is a Japanese-English dictionary.*" + Environment.NewLine +
                                  "Jisho meaning [word (can be in hiragana, katakana, kanji, romanji or english)] [kanji/hiragana (default=kanji)] [definition/noDefinition (default=definition)] [all]: Give the Japanese translation of a word using Jisho. " + Environment.NewLine + 
                                  "You can add 'hiragana' to display the words in hiragana instead of kanji, and then you can add 'noDefinition' if you don't want to display the definitions." + Environment.NewLine +
                                  "Specifying 'all' will allow you to get all the definitions, if you don't, you will only get the more pertinant one." + Environment.NewLine +
                                  "Jisho combinaison [all kanjis without spaces] [all]: Give all possible combinaisons with the kanjis gave in parameter and their meaning using Jisho. If you don't specify 'all', I'll ignore words containing hiragana." + Environment.NewLine +
                                  "**Kancolle**" + Environment.NewLine +
                                  "*This module allow you to do things related to the game Kantai Collection.*" + Environment.NewLine;
           string help3 = "Kancolle [ship name]: Display some informations of a Kantai Collection character along with an image." + Environment.NewLine +
                                 "**Settings**" + Environment.NewLine +
                                 "*This module contain things related to administration.*" + Environment.NewLine +
                                 "**Task**" + Environment.NewLine +
                                 "*This module allow you to create tasks. Each tasks must be put inside a category. They are shared with all the server.*" + Environment.NewLine +
                                 "Create category [name]: Create a new task's category." + Environment.NewLine +
                                 "Delete category [name]: Delete a task's category." + Environment.NewLine +
                                 "Create task [category] [name] [(optional)description]: Create a new task given its category and its name. A description can also be given." + Environment.NewLine +
                                 "Delete task [category] [name]: Delete a task given its category and its name." + Environment.NewLine +
                                 "Display task [(optional)category]: Display all the currently existing task. The category can be given to only display tasks who are in it." + Environment.NewLine +
                                 "**Youtube**" + Environment.NewLine +
                                 "*This module allow you to interact with YouTube.*" + Environment.NewLine +
                                 "Get youtube video [id]: Get a random video of the channel id given in parameter. You can find it by going on a channel, and it'll be the far right of the URL. Here's an example: In the URL https://www.youtube.com/channel/UCMMBGMjrrWcRZmG_lW4jC-Q, the id is \"UCMMBGMjrrWcRZmG_lW4jC-Q\"." + Environment.NewLine;
            await ReplyAsync(help);
            await ReplyAsync(help2);
            await ReplyAsync(help3);
        }*/

        [Command("Hi"), Summary("Answer with hi"), Alias("Hey", "Hello", "Hi!", "Hey!", "Hello!")]
        public async Task SayHi()
        {
            increaseMessage(Context);
            p.doAction(Context.User, Context.Guild.Id, Program.Module.Communication);
            if (File.ReadAllLines("Saves/Servers/" + Context.Guild.Id + "/serverDatas.dat")[4] == "0")
            {
                await ReplyAsync(Sentences.moduleDisabledStr);
            }
            else
            {
                await ReplyAsync(Sentences.hiStr);
            }
        }

        [Command("How are you ?"), Summary("Answer with how she is"), Alias("How are you")]
        public async Task SayHowAreYou()
        {
            increaseMessage(Context);
            p.doAction(Context.User, Context.Guild.Id, Program.Module.Communication);
            int random = p.rand.Next(3);
            if (random == 0)
                await ReplyAsync(Sentences.howAreYou1Str);
            else if (random == 1)
                await ReplyAsync(Sentences.howAreYou3Str);
            else if (random == 2)
                await ReplyAsync(Sentences.howAreYou5Str);
        }

        [Command("Good night"), Summary("Answer with good night")]
        public async Task GoodNight()
        {
            increaseMessage(Context);
            p.doAction(Context.User, Context.Guild.Id, Program.Module.Communication);
            if (File.ReadAllLines("Saves/Servers/" + Context.Guild.Id + "/serverDatas.dat")[4] == "0")
            {
                await ReplyAsync(Sentences.moduleDisabledStr);
            }
            else
            {
                await ReplyAsync(Sentences.goodnightStr);
            }
        }

        [Command("I like you"), Summary("Answer with how much she like the user")]
        public async Task LikeYou()
        {
            increaseMessage(Context);
            p.doAction(Context.User, Context.Guild.Id, Program.Module.Communication);
            if (File.ReadAllLines("Saves/Servers/" + Context.Guild.Id + "/serverDatas.dat")[4] == "0")
            {
                await ReplyAsync(Sentences.moduleDisabledStr);
            }
            else
            {
                string currTime = DateTime.UtcNow.ToString("ddMMyyHHmmss");
                string meetTime = "";
                Character currCharac = null;
                foreach (Character c in p.relations)
                {
                    if (c.getName() == Context.User.Id)
                    {
                        meetTime = c.getFirstMeet();
                        currCharac = c;
                        break;
                    }
                }
                int diffYear = Convert.ToInt32(currTime.Substring(4, 2)) - Convert.ToInt32(meetTime.Substring(4, 2));
                int diffMonth = Convert.ToInt32(currTime.Substring(2, 2)) - Convert.ToInt32(meetTime.Substring(2, 2));
                int diffDay = Convert.ToInt32(currTime.Substring(0, 2)) - Convert.ToInt32(meetTime.Substring(0, 2));
                if (Context.User.Id == 144851584478740481)
                {
                    if (currCharac.getNbMessage() < 200)
                        await ReplyAsync("Thanks master, I like you too !");
                    else if (currCharac.getNbMessage() < 2000)
                        await ReplyAsync("Thanks a lot master, I like you a much too !");
                    else
                        await ReplyAsync("Thanks a lot, I love you master !");
                }
                else if (diffYear == 0 && diffMonth == 0 && diffDay < 14 && currCharac.getRatioLewdImages() > 0.6f && currCharac.getNbImage() > 5)
                    await ReplyAsync("Um... Thanks.... But I need to admit I find you a bit creepy...");
                else if (diffYear == 0 && diffMonth == 0 && diffDay < 1)
                    await ReplyAsync("Um... Thanks, but we should speak more, I know you since less than one day you know.");
                else if (diffYear == 0 && diffMonth == 0 && diffDay < 2)
                    await ReplyAsync("Um... Thanks, but we should speak more, I don't know since a long time...");
                else if (diffYear == 0 && diffMonth == 0 && diffDay < 14)
                {
                    if (currCharac.getNbMessage() < 20)
                        await ReplyAsync("Thanks, but... I don't really know you, you know... We should definitly speak more.");
                    else if (currCharac.getNbDiscution() < 20)
                        await ReplyAsync("Thanks, even if I don't know since a long time. We should definitly speak more together.");
                    else
                        await ReplyAsync("Thanks. You know, even if I don't know you since a very lone time, I like to speak with you.");
                }
                else
                {
                    if (currCharac.getNbMessage() < 30 || currCharac.getNbDiscution() < 20)
                        await ReplyAsync("Thanks, but I need to admit that I don't feel like I know you very well.");
                    else if (currCharac.getNbMessage() < 100 || currCharac.getNbDiscution() < 40)
                        await ReplyAsync("Thanks, I like these conversations we have together, we should definitly speak more.");
                    else if (currCharac.getNbMessage() < 500 || currCharac.getNbDiscution() < 70)
                        await ReplyAsync("Thanks, I like to speak with you.");
                    else if (currCharac.getNbMessage() < 1300 || currCharac.getNbDiscution() < 100 || (diffYear == 0 && diffMonth < 2))
                    {
                        if (currCharac.getRatioLewdImages() > 0.7f && currCharac.getNbImage() > 30)
                            await ReplyAsync("Thanks, I really enjoy to speak with you... Even if you're a bit a pervert");
                        else
                            await ReplyAsync("Thanks, I really enjoy to speak with you.");
                    }
                    else
                    {
                        if (currCharac.getRatioLewdImages() > 0.7f && currCharac.getNbImage() > 100)
                            await ReplyAsync("Thanks, despite the fact that you're a pervert, you're really someone important for me, you know.");
                        else
                            await ReplyAsync("Thanks, you're really someone important for me, you know.");
                    }
                }
            }
        }

        [Command("Who are you ?"), Summary("Answer with who she is"), Alias("Who are you", "Who are you?")]
        public async Task WhoAreYou()
        {
            increaseMessage(Context);
            p.doAction(Context.User, Context.Guild.Id, Program.Module.Communication);
            if (File.ReadAllLines("Saves/Servers/" + Context.Guild.Id + "/serverDatas.dat")[4] == "0")
            {
                await ReplyAsync(Sentences.moduleDisabledStr);
            }
            else
            {
                string memoryDate = File.ReadAllLines("Saves/sanaraDatas.dat")[0];
                string getDate = memoryDate.Substring(0, 2) + "/" + memoryDate.Substring(2, 2) + "/" + memoryDate.Substring(4, 2) +
                                 " at " + memoryDate.Substring(6, 2) + ":" + memoryDate.Substring(8, 2) + ":" + memoryDate.Substring(10, 2);
                await ReplyAsync(Sentences.whoIAmStr(getDate));
            }
        }

        private void increaseMessage(ICommandContext e)
        {
            foreach (Character c in p.relations)
            {
                if (c.getName() == e.User.Id)
                {
                    c.increaseNbDiscution();
                    break;
                }
            }
        }

        [Command("Sorry"), Summary("Answer to the player saying sorry")]
        public async Task sorry()
        {
            if (Context.User.Id != 144851584478740481 || !p.mustSorry)
            {
                await ReplyAsync("Eh ? You did nothing wrong.");
            }
            else
            {
                int nb = p.rand.Next(0, 3);
                if (nb == 0) await ReplyAsync("It's okay, please just pay more attention about it...");
                else if (nb == 0) await ReplyAsync("It's okay, I only hope that I didn't lost anything...");
                else await ReplyAsync("It's okay but please don't forget next time...");
                p.mustSorry = false;
            }
            p.doAction(Context.User, Context.Guild.Id, Program.Module.Communication);
        }
    }
}