using Discord;
using Discord.Commands;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Bot
{
    public class MyAnimeListModule : ModuleBase
    {
        Program p = Program.p;

        [Command("MyAnimeList")]
        public async Task mal(string animeName)
        {
            p.doAction(Context.User, Context.Guild.Id, Program.Module.MyAnimeList);
            using (WebClient wc = new WebClient())
            {
                wc.Credentials = new NetworkCredential("Zirk", "yrck9562");
                string result = wc.DownloadString("https://myanimelist.net/api/anime/search.xml?q=" + animeName.Replace(' ', '+'));
                string title = BooruModule.getElementXml("<title>", "", result, '<');
                string english = BooruModule.getElementXml("<english>", "", result, '<');
                string synonyms = BooruModule.getElementXml("<synonyms>", "", result, '<');
                string episodes = BooruModule.getElementXml("<episodes>", "", result, '<');
                string type = BooruModule.getElementXml("<type>", "", result, '<');
                string status = BooruModule.getElementXml("<status>", "", result, '<');
                string score = BooruModule.getElementXml("<score>", "", result, '<'); 
                string synopsis = BooruModule.getElementXml("<synopsis>", "", result, '<');
                synopsis = synopsis.Replace("&amp;quot;", "\"");
                synopsis = synopsis.Replace("[i]", "*");
                synopsis = synopsis.Replace("[/i]", "*");
                synopsis = synopsis.Replace("&amp;#039;", "'");
                synopsis = synopsis.Replace("&lt;br /&gt;", Environment.NewLine);
                EmbedBuilder embed = new EmbedBuilder()
                {
                    Description = "**" + title + "** (" + english + ")" + Environment.NewLine
                    + "or " + synonyms + Environment.NewLine + Environment.NewLine
                    + "The anime format is " + type + " and is currently " + status + " with " + episodes + " episodes." + Environment.NewLine
                    + "It got a score of " + score + "/10." + Environment.NewLine + Environment.NewLine
                    + "**Synopsis:**" + Environment.NewLine + synopsis,
                    Color = Color.Green,
                };
                await ReplyAsync("", false, embed);
            }
        }
    }
}