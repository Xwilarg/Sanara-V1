using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Bot
{
    public class KancolleModule : ModuleBase
    {
        Program p = Program.p;
        [Command("Kancolle"), Summary("Get informations about a Kancolle character")]
        public async Task charac(string shipName)
        {
            p.doAction(Context.User, Context.Guild.Id, Program.Module.Kancolle);
            IGuildUser me = await Context.Guild.GetUserAsync(329664361016721408); // Sanara
            if (!me.GuildPermissions.AttachFiles)
            {
                await ReplyAsync("Sorry but I need to permission to attach files to do this here.");
            }
            else
            {
                string url = "https://kancolle.wikia.com/api/v1/Search/List?query=" + shipName + "&limit=1";
                try
                {
                    using (WebClient w = new WebClient())
                    {
                        w.Encoding = Encoding.UTF8;
                        List<string> finalStr = new List<string>();
                        finalStr.Add("");
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                        string json = w.DownloadString(url);
                        string code = BooruModule.getElementXml("\"id\":", "", json, ',');
                        url = "http://kancolle.wikia.com/api/v1/Articles/Details?ids=" + code;
                        json = w.DownloadString(url);
                        string image = BooruModule.getElementXml("\"thumbnail\":\"", "", json, '"');
                        url = "http://kancolle.wikia.com/wiki/" + BooruModule.getElementXml("\"title\":\"", "", json, '"') + "?action=raw";
                        json = w.DownloadString(url);
                        if (BooruModule.getElementXml("{{", "", json, '}') != "ShipPageHeader")
                        {
                            await ReplyAsync("I didn't find any shipgirl with this name.");
                            return;
                        }
                        image = image.Split(new string[] { ".jpg" }, StringSplitOptions.None)[0] + ".jpg";
                        image = image.Replace("\\", "");
                        int currentTime = Convert.ToInt32(DateTime.Now.ToString("HHmmss"));
                        w.DownloadFile(image, "shipgirl" + currentTime + ".jpg");
                        url = "http://kancolle.wikia.com/api/v1/Articles/AsSimpleJson?id=" + code;
                        json = w.DownloadString(url);
                        string[] jsonInside = json.Split(new string[] { "\"title\"" }, StringSplitOptions.None);
                        int currI = 0;
                        foreach (string s in jsonInside)
                        {
                            if (s.Contains("Personality"))
                            {
                                finalStr[0] += "**Personality**" + Environment.NewLine;
                                string[] allExplanations = s.Split(new string[] { "\"te" }, StringSplitOptions.None);
                                foreach (string str in allExplanations)
                                {
                                    string per = BooruModule.getElementXml("xt\":\"", "", str, '"');
                                    if (per != "")
                                        finalStr[0] += per + Environment.NewLine;
                                }
                                break;
                            }
                        }
                        foreach (string s in jsonInside)
                        {
                            if (s.Contains("Appearance"))
                            {
                                finalStr[0] += Environment.NewLine + "**Appearance**" + Environment.NewLine;
                                string[] allExplanations = s.Split(new string[] { "\"te" }, StringSplitOptions.None);
                                foreach (string str in allExplanations)
                                {
                                    string per = BooruModule.getElementXml("xt\":\"", "", str, '"');
                                    if (per != "")
                                    {
                                        if (finalStr[currI].Length > 1500)
                                        {
                                            currI++;
                                            finalStr.Add("");
                                        }
                                        finalStr[currI] += per + Environment.NewLine;
                                    }
                                }
                                break;
                            }
                        }
                        foreach (string s in jsonInside)
                        {
                            if (s.Contains("Second Remodel"))
                            {
                                finalStr[0] += "**Second Remodel**" + Environment.NewLine;
                                string[] allExplanations = s.Split(new string[] { "\"te" }, StringSplitOptions.None);
                                foreach (string str in allExplanations)
                                {
                                    string per = BooruModule.getElementXml("xt\":\"", "", str, '"');
                                    if (per != "")
                                    {
                                        if (finalStr[currI].Length > 1500)
                                        {
                                            currI++;
                                            finalStr.Add("");
                                        }
                                        finalStr[currI] += per + Environment.NewLine;
                                    }
                                }
                                break;
                            }
                        }
                        foreach (string s in jsonInside)
                        {
                            if (s.Contains("Trivia"))
                            {
                                finalStr[currI] += Environment.NewLine + "**Trivia**" + Environment.NewLine;
                                string[] allExplanations = s.Split(new string[] { "\"te" }, StringSplitOptions.None);
                                foreach (string str in allExplanations)
                                {
                                    string per = BooruModule.getElementXml("xt\":\"", "", str, '"');
                                    if (per != "")
                                    {
                                        if (finalStr[currI].Length > 1500)
                                        {
                                            currI++;
                                            finalStr.Add("");
                                        }
                                        finalStr[currI] += per + Environment.NewLine;
                                    }
                                }
                                break;
                            }
                        }
                        await Context.Channel.SendFileAsync("shipgirl" + currentTime + ".jpg");
                        foreach (string s in finalStr)
                        {
                            await ReplyAsync(Regex.Replace(
                            s,
                            @"\\[Uu]([0-9A-Fa-f]{4})",
                            m => char.ToString(
                                (char)ushort.Parse(m.Groups[1].Value, NumberStyles.AllowHexSpecifier)))); // Replace \\u1313 by \u1313
                        }
                        File.Delete("shipgirl" + currentTime + ".jpg");
                    }
                }
                catch (WebException ex)
                {
                    HttpWebResponse code = ex.Response as HttpWebResponse;
                    if (code.StatusCode == HttpStatusCode.NotFound)
                        await ReplyAsync("I didn't find any shipgirl with this name.");
                }
            }
        }
    }
}