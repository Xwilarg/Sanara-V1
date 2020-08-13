using Discord;
using Discord.Commands;
using Google;
using Google.Apis.Services;
using Google.Apis.Urlshortener.v1;
using Google.Apis.YouTube.v3;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Bot
{
    public class DebugModule : ModuleBase
    {
        Program p = Program.p;

        [Command("Display TODO list"), Summary("Display TODO list")]
        public async Task todoList()
        {
            p.doAction(Context.User, Context.Guild.Id, Program.Module.Debug);
            string finalStr =
                        "Here are the next features that my master promised me that we will learn for each modules:" + Environment.NewLine +
                        "**Booru**: The rating system must be review." + Environment.NewLine +
                        "**Code**: This module isn't finish yet..." + Environment.NewLine +
                        "**Communication**: I'm going to learn how to answer to more sentences." + Environment.NewLine +
                        "**Console**: I will need to learn the cp command." + Environment.NewLine +
                        "**Debug**: No further improvement is planned for now. You can still give suggestions if you have an idea." + Environment.NewLine +
                        "**Game**: No further improvement is planned for now. You can still give suggestions if you have an idea." + Environment.NewLine +
                        "**Jisho**:  No further improvement is planned for now. You can still give suggestions if you have an idea." + Environment.NewLine +
                        "**Kancolle**: No further improvement is planned for now. You can still give suggestions if you have an idea." + Environment.NewLine +
                        "**Settings**: The functions to enable or disable module was totally remove since the last library update so I need to do it again." + Environment.NewLine +
                        "**Settings**: No further improvement is planned for now. You can still give suggestions if you have an idea." + Environment.NewLine +
                        "**Task**: No further improvement is planned for now. You can still give suggestions if you have an idea." + Environment.NewLine +
                        "**Youtube**: Giving an ID to find a channel can be annoying, being able to give the channel name could be nice." + Environment.NewLine;
            await ReplyAsync(finalStr);
        }

        [Command("Print debug informations", RunMode = RunMode.Async), Summary("Get informations about the server and about each modules")]
        public async Task debugInfos()
        {
            p.doAction(Context.User, Context.Guild.Id, Program.Module.Debug);
            await ReplyAsync("Here are the debug informations for each modules:" + Environment.NewLine);
            string infosDebug;

            // GENERAL
            infosDebug = "**General**:" + Environment.NewLine;
            string dateCrea = File.ReadAllLines("Saves/sanaraDatas.dat")[0];
            string serverJoined = File.ReadAllLines("Saves/Servers/" + Context.Guild.Id + "/serverDatas.dat")[0];
            string dateCreafull = dateCrea.Substring(0, 2) + "/" + dateCrea.Substring(2, 2) + "/" + dateCrea.Substring(4, 2) +
                                 " at " + dateCrea.Substring(6, 2) + ":" + dateCrea.Substring(8, 2) + ":" + dateCrea.Substring(10, 2);
            string serverJoinedFull = serverJoined.Substring(0, 2) + "/" + serverJoined.Substring(2, 2) + "/" + serverJoined.Substring(4, 2) +
                                 " at " + serverJoined.Substring(6, 2) + ":" + serverJoined.Substring(8, 2) + ":" + serverJoined.Substring(10, 2);
            int userMet = 0;
            int userKnow = 0;
            string mostSpeakUser = "0";
            int nbMessages = 0;
            int totalMessages = 0;
            foreach (string s in Directory.GetFiles("Saves/Users/"))
            {
                userMet++;
                string[] details = File.ReadAllLines(s);
                if (details[2] != "No")
                {
                    userKnow++;
                    totalMessages += Convert.ToInt32(details[3]);
                    if (mostSpeakUser == "0" || Convert.ToInt32(details[3]) > nbMessages)
                    {
                        mostSpeakUser = details[0];
                        nbMessages = Convert.ToInt32(details[3]);
                    }
                }
            }
            int[] percentageModules = new int[(int)Program.Module.Youtube + 1];
            int total = 0;
            for (int i = 0; i < (int)Program.Module.Youtube + 1; i++)
            {
                percentageModules[i] = Convert.ToInt32(File.ReadAllText("Saves/Servers/" + Context.Guild.Id + "/ModuleCount/" + DateTime.UtcNow.ToString("yyyyMM") + "/"
                        + ((Program.Module)i).ToString()[0] + ((Program.Module)i).ToString().ToLower().Substring(1, ((Program.Module)i).ToString().Length - 1) + ".dat"));
                total += percentageModules[i];
            }
            string lastVersion = Directory.GetFiles("Logs/")[Directory.GetFiles("Logs/").Length - 1];
            infosDebug += "Creator: Zirk#1001." + Environment.NewLine +
            "Creation date: 28/06/17 UTC+0 (My first memories are from the " + dateCreafull + " UTC+0)." + Environment.NewLine +
            "Server joined: " + ((dateCrea == serverJoined) ? ("I don't remember, sorry...") : (serverJoinedFull + " UTC+0")) + "." + Environment.NewLine +
            "Messages received: " + totalMessages + "." + Environment.NewLine +
            "(The user who sent me more messages is " + mostSpeakUser + " with " + nbMessages + " messages)." + Environment.NewLine +
            "Users know: " + userMet + " (I already spoked with " + userKnow + " of them)." + Environment.NewLine +
            "Servers know: " + Directory.GetDirectories("Saves/Servers/").Length + " (" + p.client.Guilds.Count + " of them are available)." + Environment.NewLine +
            "Here's the percentage of utilisation for each module for this month on this server:" + Environment.NewLine;
            for (int i = 0; i < (int)Program.Module.Youtube + 1; i++)
            {
                infosDebug += ((Program.Module)i).ToString()[0] + ((Program.Module)i).ToString().ToLower().Substring(1, ((Program.Module)i).ToString().Length - 1) + ": "
                    + (percentageModules[i] * 100 / total) + "%" + Environment.NewLine;
            }
            infosDebug += "Version: " + lastVersion.Split('/')[lastVersion.Split('/').Length - 1].Substring(0, lastVersion.Split('/')[lastVersion.Split('/').Length - 1].Length - 4) + "." + Environment.NewLine +
            "Last changes: " + Environment.NewLine + File.ReadAllText(lastVersion) + Environment.NewLine;

            // ARENA
            /*string nbSeason = File.ReadAllLines("Saves/Arena/currSeason.dat")[0];
            string[] stats = File.ReadAllLines("Saves/Arena/Season" + nbSeason + "/scores.dat");
            string first;
            if (Context.Guild.Id != 146701031227654144)
                first = "Arena isn't available on this server.";
            else
            {
                string bestPlayer = "";
                int ratio = -1;
                foreach (string str in stats)
                {
                    string[] s = str.Split(' ');
                    int currRatio = Convert.ToInt32(s[1]) * 100 / Convert.ToInt32(s[2]);
                    if (currRatio > ratio && Convert.ToInt32(s[2]) >= 10)
                    {
                        bestPlayer = (await Context.Guild.GetUserAsync(Convert.ToUInt64(s[0]))).Username;
                        ratio = currRatio;
                    }
                }
                if (ratio == -1)
                    first = "Not one meet the requirement to be the next arena's champion.";
                else
                    first = "The current best player is " + bestPlayer + " with a winning ratio of " + ratio + "%.";
            }
            infosDebug += "**Arena**:" + Environment.NewLine +
            //"There are currently " + nbWeapons + " weapons and " + nbItems + " items available." + Environment.NewLine +
            "There is a total of " + stats.Length + " bots who fought for the season " + nbSeason + "." + Environment.NewLine +
            first + Environment.NewLine;*/
            //infosDebug += "**Arena**:" + Environment.NewLine +
        //"The arena isn't available yet but will be back soon (50%)." + Environment.NewLine;

            // BOORU
            double latenceKona, latenceGel, latenceLoli, latenceSafe;
            DateTime now = DateTime.UtcNow;
            string konaImages = BooruModule.getWebRequest("https://www.konachan.com/post.xml?limit=1"); // KONACHAN
            latenceKona = DateTime.UtcNow.Subtract(now).TotalMilliseconds;
            now = DateTime.UtcNow;
            string gelImages = BooruModule.getWebRequest("https://gelbooru.com/index.php?page=dapi&s=post&q=index&limit=1"); // GELBOORU
            latenceGel = DateTime.UtcNow.Subtract(now).TotalMilliseconds;
            now = DateTime.UtcNow;
            string loliImages = BooruModule.getWebRequest("https://lolibooru.moe/post.xml?limit=1"); // LOLIBOORU
            latenceLoli = DateTime.UtcNow.Subtract(now).TotalMilliseconds;
            now = DateTime.UtcNow;
            string safeImages = BooruModule.getWebRequest("http://safebooru.org/index.php?page=dapi&s=post&q=index&limit=1"); // SAFEBOORU
            latenceSafe = DateTime.UtcNow.Subtract(now).TotalMilliseconds;
            int konachanImages = Convert.ToInt32(BooruModule.getElementXml("posts count=\"", "", konaImages, '"'));
            int gelbooruImages;
            try
            {
                gelbooruImages = Convert.ToInt32(BooruModule.getElementXml("posts count=\"", "", gelImages, '"'));
            } catch (FormatException) { gelbooruImages = -1; }
            int lolibooruImages = Convert.ToInt32(BooruModule.getElementXml("posts count=\"", "", loliImages, '"'));
            int safebooruImages = Convert.ToInt32(BooruModule.getElementXml("posts count=\"", "", safeImages, '"'));
            string[] booruTimeSpent = File.ReadAllLines("Saves/booruAnalysis/stats.dat");
            double seconds = Convert.ToDouble(booruTimeSpent[0]);
            int hour = (int)(seconds / 3600);
            int minutes = (int)(seconds % 3600) / 60;
            int second = (int)(seconds % 3600) % 60;
            string finalTime = "";
            if (hour > 0)
                finalTime += hour + " hours, ";
            if (minutes > 0 || hour > 0)
                finalTime += minutes + " minutes and ";
            finalTime += second + " seconds.";
            string[] progBooru = File.ReadAllLines("Saves/booruAnalysis/stats.dat");
            infosDebug += "**Booru**:" + Environment.NewLine +
            "Konachan's latence: " + Math.Round(latenceKona) + "ms (" + konachanImages.ToString("N0", System.Globalization.CultureInfo.GetCultureInfo("fr")) + " images available)." + Environment.NewLine;
            if (gelbooruImages == -1)
                infosDebug += "Gelbooru's latence: The API was disabled." + Environment.NewLine;
            else
                infosDebug += "Gelbooru's latence: " + Math.Round(latenceGel) + "ms (" + gelbooruImages.ToString("N0", System.Globalization.CultureInfo.GetCultureInfo("fr")) + " images available)." + Environment.NewLine;
            infosDebug += "Lolibooru's latence: " + Math.Round(latenceLoli) + "ms (" + lolibooruImages.ToString("N0", System.Globalization.CultureInfo.GetCultureInfo("fr")) + " images available)." + Environment.NewLine +
            "Safebooru's latence: " + Math.Round(latenceSafe) + "ms (" + safebooruImages.ToString("N0", System.Globalization.CultureInfo.GetCultureInfo("fr")) + " images available)." + Environment.NewLine +
            "Time spent on learning tags: " + finalTime + Environment.NewLine +
            "Konachan completion rate: " + (Convert.ToSingle(progBooru[1]) * 100 / konachanImages).ToString("0.0", System.Globalization.CultureInfo.InvariantCulture) + "%." + Environment.NewLine;
            if (gelbooruImages == -1)
                infosDebug += "Gelbooru completion rate: Unknowed." + Environment.NewLine;
            else
                infosDebug += "Gelbooru completion rate: " + (Convert.ToSingle(progBooru[2]) * 100 / gelbooruImages).ToString("0.0", System.Globalization.CultureInfo.InvariantCulture) + "%." + Environment.NewLine;
            infosDebug += "Lolibooru completion rate: " + (Convert.ToSingle(progBooru[3]) * 100 / lolibooruImages).ToString("0.0", System.Globalization.CultureInfo.InvariantCulture) + "%." + Environment.NewLine +
            "Safebooru completion rate: " + (Convert.ToSingle(progBooru[4]) * 100 / safebooruImages).ToString("0.0", System.Globalization.CultureInfo.InvariantCulture) + "%." + Environment.NewLine;

            // CONSOLE
            infosDebug += "**Code**:" + Environment.NewLine;
            infosDebug += "This module isn't finish yet..." + Environment.NewLine;

            // COMMUNICATION
            infosDebug += "**Communication**:" + Environment.NewLine;
            infosDebug += "No information available." + Environment.NewLine;

            // CONSOLE
            infosDebug += "**Console**:" + Environment.NewLine;
            infosDebug += "No information available." + Environment.NewLine;

            // DEBUG
            PerformanceCounter cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            PerformanceCounter ramCounter = new PerformanceCounter("Memory", "Available MBytes");
            infosDebug += "**Debug**:" + Environment.NewLine +
                          "CPU usage: " + cpuCounter.NextValue() + "%." + Environment.NewLine +
                          "RAM usage: " + ramCounter.NextValue() + " MB." + Environment.NewLine;

            EmbedBuilder embed = new EmbedBuilder()
            {
                Description = infosDebug,
                Color = Color.Purple,
            };
            await ReplyAsync("", false, embed);
            infosDebug = "";

            // GAME
            // KANCOLLE
            int totalGames = 0;
            int hightestScore = 0;
            string[] myDatas = File.ReadAllLines("Saves/Servers/" + Context.Guild.Id + "/kancolle.dat");
            int scoreHere = Convert.ToInt32(myDatas[3]);
            int totalGamesHere = Convert.ToInt32(myDatas[0]);
            string bestIdKancolle = myDatas[4];
            foreach (string s in Directory.GetDirectories("Saves/Servers/"))
            {
                string[] infos = File.ReadAllLines(s + "/kancolle.dat");
                totalGames += Convert.ToInt32(infos[0]);
                if (Convert.ToInt32(infos[3]) > hightestScore)
                {
                    hightestScore = Convert.ToInt32(infos[3]);
                    bestIdKancolle = infos[4];
                }
            }
            List<string> finalBestIdKancolle = new List<string>();

            // BOORU
            int totalGamesBooruAll = 0;
            int hightestScoreBooruAll = 0;
            myDatas = File.ReadAllLines("Saves/Servers/" + Context.Guild.Id + "/quizzTagAll.dat");
            int scoreHereBooruAll = Convert.ToInt32(myDatas[3]);
            int totalGamesHereBooruAll = Convert.ToInt32(myDatas[0]);
            string bestIdBooruAll = myDatas[4];
            foreach (string s in Directory.GetDirectories("Saves/Servers/"))
            {
                try
                {
                    string[] infos = File.ReadAllLines(s + "/quizzTagAll.dat");
                    totalGamesBooruAll += Convert.ToInt32(infos[0]);
                    if (Convert.ToInt32(infos[3]) > hightestScoreBooruAll)
                    {
                        hightestScoreBooruAll = Convert.ToInt32(infos[3]);
                        bestIdBooruAll = infos[4];
                    }
                }
                catch (FileNotFoundException) { }
            }
            string[] bestIdBooruAllStr = bestIdBooruAll.Split('|');
            List<string> finalBestIdBooruAll = new List<string>();

            int totalGamesBooruSafe = 0;
            int hightestScoreBooruSafe = 0;
            myDatas = File.ReadAllLines("Saves/Servers/" + Context.Guild.Id + "/quizzTagSafe.dat");
            int scoreHereBooruSafe = Convert.ToInt32(myDatas[3]);
            int totalGamesHereBooruSafe = Convert.ToInt32(myDatas[0]);
            string bestIdBooruSafe = myDatas[4];
            foreach (string s in Directory.GetDirectories("Saves/Servers/"))
            {
                try
                {
                    string[] infos = File.ReadAllLines(s + "/quizzTagSafe.dat");
                    totalGamesBooruSafe += Convert.ToInt32(infos[0]);
                    if (Convert.ToInt32(infos[3]) > hightestScoreBooruSafe)
                    {
                        hightestScoreBooruSafe = Convert.ToInt32(infos[3]);
                        bestIdBooruSafe = infos[4];
                    }
                }
                catch (FileNotFoundException) { }
            }
            string[] bestIdBooruSafeStr = bestIdBooruSafe.Split('|');
            List<string> finalBestIdBooruSafe = new List<string>();

            int nbGamesShiritori = 0;
            int bestScore = 0;
            string whoHaveBestScore = "";
            foreach (string s in Directory.GetFiles("Saves/Users/"))
            {
                try
                {
                    string[] datas = File.ReadAllLines(s);
                    nbGamesShiritori += Convert.ToInt32(datas[11]);
                    int score = Convert.ToInt32(datas[14]);

                    if (score > bestScore)
                    {
                        bestScore = score;
                        whoHaveBestScore = datas[0];
                    }
                    string id = datas[1];
                    id = id.Substring(0, id.Length - 4);
                    if (bestIdKancolle.Contains(id) && id != "0")
                    {
                        finalBestIdKancolle.Add(datas[0]);
                    }
                    if (bestIdBooruAll.Contains(id) && id != "0")
                    {
                        finalBestIdBooruAll.Add(datas[0]);
                    }
                    if (bestIdBooruSafe.Contains(id) && id != "0")
                    {
                        finalBestIdBooruSafe.Add(datas[0]);
                    }
                }
                catch (Exception) // Account with id '0'
                { }
            }
            bestIdKancolle = "";
            if (finalBestIdKancolle.Count == 0)
                bestIdKancolle = "nobody";
            else if (finalBestIdKancolle.Count < 2)
                bestIdKancolle = finalBestIdKancolle[0];
            else
            {
                for (int i = 0; i < finalBestIdKancolle.Count - 1; i++)
                    bestIdKancolle += finalBestIdKancolle[i] + ", ";
                bestIdKancolle = bestIdKancolle.Substring(0, bestIdKancolle.Length - 2);
                bestIdKancolle += " and " + finalBestIdKancolle[finalBestIdKancolle.Count - 1];
            }
            bestIdBooruAll = "";
            if (finalBestIdBooruAll.Count == 0)
                bestIdBooruAll = "nobody";
            else if (finalBestIdBooruAll.Count < 2)
                bestIdBooruAll = finalBestIdBooruAll[0];
            else
            {
                for (int i = 0; i < finalBestIdBooruAll.Count - 1; i++)
                    bestIdBooruAll += finalBestIdBooruAll[i] + ", ";
                bestIdBooruAll = bestIdBooruAll.Substring(0, bestIdBooruAll.Length - 2);
                bestIdBooruAll += " and " + finalBestIdBooruAll[finalBestIdBooruAll.Count - 1];
            }
            bestIdBooruSafe = "";
            if (finalBestIdBooruSafe.Count == 0)
                bestIdBooruSafe = "nobody";
            else if (finalBestIdBooruSafe.Count < 2)
                bestIdBooruSafe = finalBestIdBooruSafe[0];
            else
            {
                for (int i = 0; i < finalBestIdBooruSafe.Count - 1; i++)
                    bestIdBooruSafe += finalBestIdBooruSafe[i] + ", ";
                bestIdBooruSafe = bestIdBooruSafe.Substring(0, bestIdBooruSafe.Length - 2);
                bestIdBooruSafe += " and " + finalBestIdBooruSafe[finalBestIdBooruSafe.Count - 1];
            }
            string[] shiritori = File.ReadAllLines("Saves/shiritoriStats.dat");
            seconds = Convert.ToDouble(shiritori[6]);
            hour = (int)(seconds / 3600);
            minutes = (int)(seconds % 3600) / 60;
            second = (int)(seconds % 3600) % 60;
            finalTime = "";
            if (hour > 0)
                finalTime += hour + " hours, ";
            if (minutes > 0 || hour > 0)
                finalTime += minutes + " minutes and ";
            finalTime += second + " seconds";
            infosDebug += "**Game**:" + Environment.NewLine +
                          "A total of " + (totalGames + nbGamesShiritori + totalGamesBooruAll + totalGamesBooruSafe).ToString() + " games were made, " + totalGames + " games of Kancolle Quizz (" + totalGamesHere + " from this server), "
                          + nbGamesShiritori + " games of shiritori, " + (totalGamesBooruSafe + totalGamesBooruAll) + " games of Booru quizz (" + totalGamesBooruSafe + " safe, " + totalGamesBooruAll + " with all rating)." + Environment.NewLine +
                          "Kancolle Quizz: There are " + p.names.Count + " ships available." + Environment.NewLine +
                          "                " + ((scoreHere == hightestScore) ? ("This server currently have the best score with a score of " + scoreHere + " point" + ((Convert.ToInt32(scoreHere) > 1) ? ("s") : ("")) + ".")
                                               : ("The best score is " + hightestScore + " point" + ((Convert.ToInt32(hightestScore) > 1) ? ("s") : ("")) + " and is from another server. The best score here is " + scoreHere + "point" + ((Convert.ToInt32(scoreHere) > 1) ? ("s") : ("")) + ".")) + Environment.NewLine +
                          "                This score was obtain by " + bestIdKancolle + "." + Environment.NewLine +
                          "Shiritori: The current best score is " + bestScore + " and was obtain by " + whoHaveBestScore + "." + Environment.NewLine +
                          "                I actually know " + shiritori[5] + " words." + Environment.NewLine +
                          "                I spent " + finalTime + " and " + (Convert.ToInt32(shiritori[0]) + Convert.ToInt32(shiritori[1]) + Convert.ToInt32(shiritori[2]) + Convert.ToInt32(shiritori[3]) + Convert.ToInt32(shiritori[4]) + Convert.ToInt32(shiritori[5])).ToString() + " attempts to learn these words." + Environment.NewLine +
                          "                Between all my failed attemps: " + Environment.NewLine +
                          "                " + shiritori[0] + " of them were words I already know." + Environment.NewLine +
                          "                " + shiritori[1] + " of them weren't real words." + Environment.NewLine +
                          "                " + shiritori[2] + " of them weren't nouns." + Environment.NewLine +
                          "                " + shiritori[3] + " of them were words finishing by a ん." + Environment.NewLine +
                          "                " + shiritori[4] + " of them were words with only 1 character." + Environment.NewLine +
                          "Booru Quizz (safe version):" + Environment.NewLine +
                          "                " + ((scoreHereBooruSafe == hightestScoreBooruSafe) ? ("This server currently have the best score with a score of " + scoreHereBooruSafe + " point" + ((Convert.ToInt32(scoreHereBooruSafe) > 1) ? ("s") : ("")) + ".")
                                               : ("The best score is " + hightestScoreBooruSafe + " point" + ((Convert.ToInt32(hightestScoreBooruSafe) > 1) ? ("s") : ("")) + " and is from another server. The best score here is " + scoreHereBooruSafe + " point" + ((Convert.ToInt32(scoreHereBooruSafe) > 1) ? ("s") : ("")) + ".")) + Environment.NewLine +
                          "                This score was obtain by " + bestIdBooruSafe + "." + Environment.NewLine +
                          "Booru Quizz (all rating version):" + Environment.NewLine +
                          "                " + ((scoreHereBooruAll == hightestScoreBooruAll) ? ("This server currently have the best score with a score of " + scoreHereBooruAll + " point" + ((Convert.ToInt32(scoreHereBooruAll) > 1) ? ("s") : ("")) + ".")
                                               : ("The best score is " + hightestScoreBooruAll + " point" + ((Convert.ToInt32(hightestScoreBooruAll) > 1) ? ("s") : ("")) + " and is from another server. The best score here is " + scoreHereBooruAll + " point" + ((Convert.ToInt32(scoreHereBooruAll) > 1) ? ("s") : ("")) + ".")) + Environment.NewLine +
                          "                This score was obtain by " + bestIdBooruAll + "." + Environment.NewLine;

            // GOOGLE SHORTENER
            double latence;
            bool canSendRequest = true;
            now = DateTime.UtcNow;
            UrlshortenerService service = new UrlshortenerService(new BaseClientService.Initializer
            {
                ApiKey = File.ReadAllText("Keys/URLShortenerAPIKey.dat"),
            });
            try
            {
                using (WebClient wc = new WebClient())
                {
                    wc.Encoding = Encoding.UTF8;
                    await service.Url.Get("http://goo.gl/fbsS").ExecuteAsync();
                }
            }
            catch (GoogleApiException) { canSendRequest = false; }
            latence = DateTime.UtcNow.Subtract(now).TotalMilliseconds;
            infosDebug += "**Google Shortener**:" + Environment.NewLine +
                          "Google link shortener's latence: " + Math.Round(latence) + "ms" + ((!canSendRequest) ? (" (exceeded request amount).") : (".")) + Environment.NewLine;

            // JISHO
            double latencePaste;
            now = DateTime.UtcNow;
            using (WebClient wc = new WebClient())
            {
                wc.Encoding = Encoding.UTF8;
                wc.DownloadString("http://www.jisho.org/api/v1/search/words");
            }
            latence = DateTime.UtcNow.Subtract(now).TotalMilliseconds;
            now = DateTime.UtcNow;
            using (WebClient wc = new WebClient())
            {
                wc.Encoding = Encoding.UTF8;
                wc.DownloadString("https://pastebin.com/");
            }
            latencePaste = DateTime.UtcNow.Subtract(now).TotalMilliseconds;
            infosDebug += "**Jisho**:" + Environment.NewLine +
                          "Jisho's latence: " + Math.Round(latence) + "ms." + Environment.NewLine +
                          "Pastebin's latence: " + Math.Round(latencePaste) + "ms." + Environment.NewLine;


            // KANCOLLE
            now = DateTime.UtcNow;
            using (WebClient wc = new WebClient())
            {
                wc.Encoding = Encoding.UTF8;
                wc.DownloadString("https://kancolle.wikia.com/api/v1/Search/List?query=ryuujou&limit=1");
            }
            latence = DateTime.UtcNow.Subtract(now).TotalMilliseconds;
            infosDebug += "**Kancolle**:" + Environment.NewLine +
                          "Kancolle wikia's latence: " + Math.Round(latence) + "ms." + Environment.NewLine;

            // NHENTAI
            now = DateTime.UtcNow;
            using (WebClient wc = new WebClient())
            {
                wc.Encoding = Encoding.UTF8;
                wc.DownloadString("https://nhentai.net/api/galleries/search?query=loli");
            }
            latence = DateTime.UtcNow.Subtract(now).TotalMilliseconds;
            infosDebug += "**Nhentai**:" + Environment.NewLine +
                          "Nhentai's latence: " + Math.Round(latence) + "ms." + Environment.NewLine;

            // SETTINGS
            //DateTime.UtcNow.ToString("yy-MM-dd-HH-mm-ss");
            string date = Directory.GetDirectories("Archives/")[Directory.GetDirectories("Archives/").Length - 1];
            string fullDate = date.Substring(15, 2) + "/" + date.Substring(12, 2) + "/" + date.Substring(9, 2) + " at "
                              + date.Substring(18, 2) + ":" + date.Substring(21, 2) + ":" + date.Substring(24, 2);
            infosDebug += "**Settings**:" + Environment.NewLine +
                          "The last archive created is from the " + fullDate + "." + Environment.NewLine;

            // TASK
            int nbTasks = 0;
            int nbCathegories = 0;
            int nbServers = 0;
            foreach (string s in Directory.GetDirectories("Saves/Servers/"))
            {
                string[] dirs = Directory.GetDirectories(s + "/Tasks/");
                if (dirs.Length > 0)
                {
                    nbServers++;
                    nbCathegories += dirs.Length;
                }
                else
                    continue;
                foreach (string s2 in dirs)
                {
                    nbTasks += Directory.GetFiles(s2).Length;
                }
            }
            infosDebug += "**Task**:" + Environment.NewLine +
                          "There are currently " + nbTasks + " tasks distributed in " + nbCathegories + " categories, and there are " + nbServers + " servers using them." + Environment.NewLine;

            // YOUTUBE
            now = DateTime.UtcNow;
            YouTubeService yt = new YouTubeService(new BaseClientService.Initializer() { ApiKey = File.ReadAllText("Keys/YoutubeAPIKey.dat") });
            var searchListRequest = yt.Search.List("snippet");
            searchListRequest.ChannelId = "UCYfqOkiuHA1ElyZx7ZvKNMg";
            searchListRequest.MaxResults = 50;
            try
            {
                var searchListResult = searchListRequest.Execute();
            }
            catch (Exception) { }
            latence = DateTime.UtcNow.Subtract(now).TotalMilliseconds;
            infosDebug += "**Youtube**:" + Environment.NewLine +
                          "Youtube's latence: " + Math.Round(latence) + "ms." + Environment.NewLine;

            if (Context.User.Id == 144851584478740481)
                infosDebug += Environment.NewLine + "I will also send you some additional informations in private message.";
            embed = new EmbedBuilder()
            {
                Description = infosDebug,
                Color = Color.Purple,
            };
            await ReplyAsync("", false, embed);

            if (Context.User.Id == 144851584478740481)
            {
                 IDMChannel c = await Context.User.GetOrCreateDMChannelAsync();
                 await c.SendMessageAsync("Here are the debug informations:");
                 string mpDebug = "Here are all the servers I know: " + Environment.NewLine;
                 foreach (string f in Directory.GetDirectories("Saves/Servers/"))
                 {
                     mpDebug += File.ReadAllLines(f + "/serverDatas.dat")[2] + Environment.NewLine;
                 }
                 mpDebug += Environment.NewLine + "Users (Only if stat > 0):" + Environment.NewLine;
                 int counter = 10;
                 foreach (Character r in p.relations)
                 {
                     if (r.returnInfosValuable() != null)
                     {
                         mpDebug += r.returnInfosValuable() + Environment.NewLine;
                         counter--;
                         if (counter == 0)
                         {
                             counter = 10;
                             await c.SendMessageAsync(mpDebug);
                             mpDebug = "";
                         }
                     }
                 }
                 await c.SendMessageAsync(mpDebug);
            }
        }
    }
}