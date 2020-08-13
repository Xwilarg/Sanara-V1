using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace Bot
{
    public class BooruModule : ModuleBase
    {
        Program p = Program.p;

        public enum booru
        {
            konachan,
            gelbooru,
            lolibooru,
            safebooru,
        }

        private enum contentKonachan
        {
            image,
            xml,
            icon
        }

        [Command("Help booru"), Summary("Get help about booru module")]
        public async Task help()
        {
            p.doAction(Context.User, Context.Guild.Id, Program.Module.Booru);
            string finalStr = "Here's a short description of all the available 'booru' website I can search images on." + Environment.NewLine +
                   "I won't explain all the rules in details, mostly all of them have some common points: they only allow manga-style drawing, and they often don't allow the following things: bestiality, furry, toddlercon, copyright content, image taken from manga, torture, grotesque, etc..." + Environment.NewLine +
                   "Some exceptions can be made for some websites, but I won't list them, that's not my goal." + Environment.NewLine +
                   "Konachan: This website only contain images that can be used at wallpaper (who are big enough and have a decent AR - Aspect Ratio -)." + Environment.NewLine +
                   "Gelbooru: This website doesn't have any particular rules (except the ones said above)." + Environment.NewLine +
                   "Lolibooru: This website mosly only contain images containing characters with a young appearance. I won't display toddlercon content though." + Environment.NewLine +
                   "Safebooru: This website only contain SFW (Safe For Work) images, no total nudity or porn.";
            await ReplyAsync(finalStr);
        }

        [Command("Konachan", RunMode = RunMode.Async), Summary("Get an image from Konachan")]
        public async Task konachanSearch(string type, string rating = "", string sendMethod = "", string displayTags = "", params string[] tags)
        {
            p.doAction(Context.User, Context.Guild.Id, Program.Module.Booru);
            konachan(Context.Channel, Context.Guild, Context.User, booru.konachan, type, rating, sendMethod, displayTags, tags);
        }

        [Command("Gelbooru", RunMode = RunMode.Async), Summary("Get an image from Gelbooru")]
        public async Task gelbooruSearch(string type, string rating = "", string sendMethod = "", string displayTags = "", params string[] tags)
        {
            p.doAction(Context.User, Context.Guild.Id, Program.Module.Booru);
            konachan(Context.Channel, Context.Guild, Context.User, booru.gelbooru, type, rating, sendMethod, displayTags, tags);
        }

        [Command("Lolibooru", RunMode = RunMode.Async), Summary("Get an image from Lolibooru")]
        public async Task lolibooruSearch(string type, string rating = "", string sendMethod = "", string displayTags = "", params string[] tags)
        {
            p.doAction(Context.User, Context.Guild.Id, Program.Module.Booru);
            konachan(Context.Channel, Context.Guild, Context.User, booru.lolibooru, type, rating, sendMethod, displayTags, tags);
        }

        [Command("Safebooru", RunMode = RunMode.Async), Summary("Get an image from Safebooru")]
        public async Task safebooruSearch(string type, string rating = "", string sendMethod = "", string displayTags = "", params string[] tags)
        {
            p.doAction(Context.User, Context.Guild.Id, Program.Module.Booru);
            konachan(Context.Channel, Context.Guild, Context.User, booru.safebooru, type, rating, sendMethod, displayTags, tags);
        }

#pragma warning disable CS1998
        [Command("Booru", RunMode = RunMode.Async), Summary("Get an image from from a random booru")]
        public async Task booruSearch(string type, string rating = "", string sendMethod = "", string displayTags = "", params string[] tags)
        {
            booru randomBooru;
            do
            {
                randomBooru = (booru)(p.rand.Next(0, 4));
            } while (randomBooru == booru.safebooru && (rating == "questionable" || rating == "explicit"));
            if (randomBooru == booru.konachan)
            {
                for (int i = 0; i < tags.Length; i++)
                {
                    if (tags[i] == "school_uniform")
                        tags[i] = "seifuku";
                    if (tags[i] == "pantsu")
                        tags[i] = "panties";
                    if (tags[i] == "shimapan")
                        tags[i] = "striped_panties";
                    tags[i] = tags[i].Replace("(kantai_collection)", "(kancolle)");
                }
            }
            else if (randomBooru == booru.lolibooru)
            {
                for (int i = 0; i < tags.Length; i++)
                {
                    if (tags[i] == "touhou")
                        tags[i] = "touhou_project";
                }
            }
            p.doAction(Context.User, Context.Guild.Id, Program.Module.Booru);
            konachan(Context.Channel, Context.Guild, Context.User, randomBooru, type, rating, sendMethod, displayTags, tags);
        }
#pragma warning restore CS1998

        private string getKonachanImage(contentKonachan getContent, char rating, string tag, booru whichBooru, bool isDownload, Character ch)
        {
            XmlDocument requestXml = new XmlDocument();
            string url;
            int iteration = 0;

            char imageRating;
            do
            {
                int randomNbMax;
                try
                {
                    if (whichBooru == booru.konachan)
                        randomNbMax = Convert.ToInt32(getElementXml("posts count=\"", "", getWebRequest("https://www.konachan.com/post.xml?limit=1" + tag), '"'));
                    else if (whichBooru == booru.gelbooru)
                        randomNbMax = Convert.ToInt32(getElementXml("posts count=\"", "", getWebRequest("https://gelbooru.com/index.php?page=dapi&s=post&q=index&limit=1" + tag), '"')) - 1;
                    else if (whichBooru == booru.lolibooru)
                        randomNbMax = Convert.ToInt32(getElementXml("posts count=\"", "", getWebRequest("https://lolibooru.moe/post.xml?limit=1" + tag), '"'));
                    else
                        randomNbMax = Convert.ToInt32(getElementXml("posts count=\"", "", getWebRequest("http://safebooru.org/index.php?page=dapi&s=post&q=index&limit=1" + tag), '"')) - 1;
                } catch (FormatException)
                {
                    return ("$The website refused to give me an image, please retry again later.");
                }
                if (randomNbMax <= 0)
                {
                    tag = tag.Substring(6, tag.Length - 6);
                    string[] tags = tag.Split(new string[] { "+" }, StringSplitOptions.RemoveEmptyEntries);
                    if (tags.Length == 1)
                        return ("$I didn't find any image with the tag '" + tags[0] + "'.");
                    string finalTags = "$I didn't find any image with the tags ";
                    for (int i = 0; i < tags.Length - 1; i++)
                        finalTags += "'" + tags[i] + "', ";
                    return (finalTags.Substring(0, finalTags.Length - 2) + " and '" + tags[tags.Length - 1] + "'.");
                }
                string xml;
                string link;
                //bool isOkay;
                //do
                //{
                    int randomPage = p.rand.Next(randomNbMax) + 1;
                    if (whichBooru == booru.konachan)
                        link = "https://www.konachan.com/post.xml?page=" + randomPage + tag + "&limit=1";
                    else if (whichBooru == booru.gelbooru)
                        link = "https://gelbooru.com/index.php?page=dapi&s=post&q=index&pid=" + randomPage + tag + "&limit=1";
                    else if (whichBooru == booru.lolibooru)
                        link = "https://lolibooru.moe/post.xml?page=" + randomPage + tag + "&limit=1";
                    else
                        link = "https://safebooru.org/index.php?page=dapi&s=post&q=index&pid=" + randomPage + tag + "&limit=1";
                //    isOkay = (link != "https://www.");
                 //   if (!isOkay) Console.WriteLine("Nyan");
                //} while (!isOkay);
                Console.WriteLine(link);
                xml = getWebRequest(link);
                string[] allTags = getElementXml("tags=\"", "", xml, '"').Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                /*using (WebClient wc = new WebClient())
                {
                    //HttpWebRequest req = (HttpWebRequest)WebRequest.Create("https://myanimelist.net");
                    wc.Credentials = new NetworkCredential("Zirk", "mdp");*/
                ch.animeFrom = new List<string>();
                ch.characs = new List<string>();
                ch.general = new List<string>(); ch.artists = new List<string>();
                using (WebClient w = new WebClient())
                {
                    w.Encoding = Encoding.UTF8;
                    foreach (string t in allTags)
                    {
                        if (t == "toddlercon") return ("$The image found contain toddlercon and I don't want to display that kind of content.");
                        else if (whichBooru == booru.lolibooru && t == "Photorealistic") return ("$The image found contain lolicon and photorealism, I don't want to mix these two kind of content.");
                        string newTag;
                        newTag = t;
                        newTag = newTag.Replace('+', '_');
                        string xml2;
                        if (whichBooru == booru.gelbooru)
                            xml2 = w.DownloadString("https://gelbooru.com/index.php?page=dapi&s=tag&q=index&name=" + newTag);
                        else if (whichBooru == booru.konachan)
                            xml2 = w.DownloadString("https://konachan.com/tag.xml?limit=10000&name=" + newTag);
                        else if (whichBooru == booru.lolibooru)
                            xml2 = w.DownloadString("https://lolibooru.moe/tag.xml?limit=10000&name=" + newTag);
                        else
                            xml2 = w.DownloadString("https://safebooru.org/index.php?page=dapi&s=tag&q=index&name=" + newTag);
                        foreach (string s in xml2.Split('<'))
                        {
                            if (getElementXml("name=\"", "", s, '"') == newTag)
                            {
                                if (getElementXml("type=\"", "", s, '"') == "0")
                                {
                                    ch.general.Add(t);
                                    break;
                                }
                                if (getElementXml("type=\"", "", s, '"') == "1")
                                {
                                    ch.artists.Add(t);
                                    break;
                                }
                                else if (getElementXml("type=\"", "", s, '"') == "3")
                                {
                                    ch.animeFrom.Add(t);
                                    break;
                                }
                                else if (getElementXml("type=\"", "", s, '"') == "4")
                                {
                                    ch.characs.Add(t);
                                    break;
                                }
                                else
                                    break;
                            }
                        }
                    }
                }
                imageRating = getElementXml("rating=\"", "", xml, '"')[0];
                ch.rating = imageRating;
                if (getContent == contentKonachan.xml)
                {
                    if (getContent == contentKonachan.xml && !isDownload)
                        url = link;
                    else
                        url = xml;
                }
                else if (getContent == contentKonachan.image)
                {
                    if (whichBooru == booru.konachan)
                    {
                        url = getElementXml("file_url=\"", "", xml, '"');
                        Console.WriteLine("Url = " + url);
                    }
                    else if (whichBooru == booru.safebooru)
                        url = getElementXml("file_url=\"//", "https://", xml, '"');
                    else
                        url = getElementXml("file_url=\"", "", xml, '"');
                }
                else if (getContent == contentKonachan.icon)
                {
                    if (whichBooru == booru.konachan)
                        url = getElementXml("preview_url=\"", "https://www.", xml, '"');
                    else if (whichBooru == booru.safebooru)
                        url = getElementXml("preview_url=\"//", "https://", xml, '"');
                    else
                        url = getElementXml("preview_url=\"", "", xml, '"');
                }
                else
                    url = "$Error, contentKonachan doesn't have a valid value.";
                iteration++;
                if (iteration == 50)
                {
                    tag = tag.Substring(6, tag.Length - 6);
                    string[] tags = tag.Split(new string[] { "+" }, StringSplitOptions.RemoveEmptyEntries);
                    if (tags.Length == 1)
                        return ("$I didn't find any image with the tag '" + tags[0] + "' with this rating.");
                    string finalTags = "$I didn't find any image with the tags ";
                    for (int i = 0; i < tags.Length - 1; i++)
                        finalTags += "'" + tags[i] + "', ";
                    return (finalTags.Substring(0, finalTags.Length - 2) + " and '" + tags[tags.Length - 1] + "' and with this rating.");
                }
            } while (rating != 'a' && rating != imageRating);
            return (url);
        }

        private async void konachan(IMessageChannel chan, IGuild serv, IUser user, booru whichBooru, string stype, string srating, string ssendMethod, string displayTags, string[] stags)
        {
            Character currUser = p.relations.Find(x => x.getName() == Context.User.Id);
            if (File.ReadAllLines("Saves/Servers/" + serv.Id + "/serverDatas.dat")[5] == "0")
            {
                await ReplyAsync("Sorry but this module was disabled.");
            }
            else if (currUser._isRequestingImage)
            {
                await ReplyAsync("I'm already searching an image for you, please wait until I'm done.");
            }
            else
            {
                if (displayTags != "" && displayTags != "normal" && displayTags != "debug")
                    await ReplyAsync("I'm sorry but I don't understand if you want the debug or the normal mode.");
                currUser._isRequestingImage = true;
                int currentTime = Convert.ToInt32(DateTime.Now.ToString("HHmmss"));
                string rating = "";
                char ratingAbbr = 'a';
                contentKonachan currContent;
                string contentSent = "";
                bool useDownload = true;
                if (srating == "safe")
                {
                    rating = " rated safe";
                    ratingAbbr = 's';
                }
                else if (srating == "questionable")
                {
                    rating = " rated questionable";
                    ratingAbbr = 'q';
                    if (whichBooru == booru.safebooru)
                    {
                        await ReplyAsync("I'm sorry but Safebooru only contain safe images, please use the 'safe' or 'all' rating instead.");
                        currUser._isRequestingImage = false;
                        return;
                    }
                }
                else if (srating == "explicit")
                {
                    rating = " rated explicit";
                    ratingAbbr = 'e';
                    if (whichBooru == booru.safebooru)
                    {
                        await ReplyAsync("I'm sorry but Safebooru only contain safe images, please use the 'safe' or 'all' rating instead.");
                        currUser._isRequestingImage = false;
                        return;
                    }
                }
                else if (srating != "all" && srating != "")
                {
                    await ReplyAsync("I'm sorry but I don't understand what this rating is. Please check the format of your request.");
                    currUser._isRequestingImage = false;
                    return;
                }
                if (stype == "xml")
                {
                    currContent = contentKonachan.xml;
                    contentSent = "XML's file";
                }
                else if (stype == "image")
                {
                    currContent = contentKonachan.image;
                    contentSent = "image";
                }
                else if (stype == "icon")
                {
                    currContent = contentKonachan.icon;
                    contentSent = "icon";
                }
                else
                {
                    await ReplyAsync("I'm sorry but I don't understand what you want. Please check the format of your request.");
                    currUser._isRequestingImage = false;
                    return;
                }
                IGuildUser me = await Context.Guild.GetUserAsync(329664361016721408); // Sanara
                if (ssendMethod == "url")
                    useDownload = false;
                else if (ssendMethod == "download")
                {
                    if (!me.GuildPermissions.AttachFiles)
                        await ReplyAsync("I'm sorry but I don't have the permissions to upload files." + Environment.NewLine + "I'll send a link instead.");
                    else
                        useDownload = true;
                }
                else if (ssendMethod != "")
                {
                    await ReplyAsync("I'm sorry but I don't understand how you want me to get the image. Please check the format of your request.");
                    currUser._isRequestingImage = false;
                    return;
                }
                if (!me.GuildPermissions.AttachFiles)
                    useDownload = false;
                bool allowedChan = false;
                if (chan.IsNsfw)
                    allowedChan = true;
                if (stype != "xml")
                {
                    foreach (Character c in p.relations)
                    {
                        if (c.getName() == user.Id)
                        {
                            c.increaseNbImage();
                            if (rating != "safe") c.increaseNbLewdImage();
                        }
                    }
                }
                string image;
                string tags = "&tags=";
                if (stags.Length > 0)
                {
                    tags += stags[0];
                    for (int i = 1; i < stags.Length; i++)
                        tags += "+" + stags[i];
                }
                bool tooBig = false;
                string source;
                if (whichBooru == booru.gelbooru)
                    source = "Gelbooru";
                else if (whichBooru == booru.konachan)
                    source = "Konachan";
                else if (whichBooru == booru.lolibooru)
                    source = "Lolibooru";
                else
                    source = "Safebooru";
                try
                {
                    if (allowedChan)
                    {
                        await ReplyAsync("Here's your " + contentSent + rating + " from " + source + ":" + Environment.NewLine);
                        image = getKonachanImage(currContent, ratingAbbr, tags, whichBooru, useDownload, currUser);
                        if (image[0] == '$')
                        {
                            image = image.Substring(1, image.Length - 1);
                            await ReplyAsync(image);
                            currUser._isRequestingImage = false;
                            return;
                        }
                        else
                        {
                            if (!useDownload || currContent == contentKonachan.xml)
                            {
                                await ReplyAsync(image);
                            }
                            else
                            {
                                using (var client = new WebClient())
                                {
                                    client.DownloadFile(image, "imageKonachan" + currentTime + "." + image.Split('.')[image.Split('.').Length - 1]);
                                }
                                FileInfo file = new FileInfo("imageKonachan" + currentTime + "." + image.Split('.')[image.Split('.').Length - 1]);
                                if (file.Length >= 8000000)
                                {
                                    await ReplyAsync("I wasn't able to send the image since its size was superior to 8MB.");
                                    tooBig = true;
                                }
                                else
                                    await chan.SendFileAsync("imageKonachan" + currentTime + "." + image.Split('.')[image.Split('.').Length - 1]);
                                File.Delete("imageKonachan" + currentTime + "." + image.Split('.')[image.Split('.').Length - 1]);
                            }
                        }
                    }
                    else
                    {
                        if (srating == "safe" || whichBooru == booru.safebooru)
                        {
                            await ReplyAsync("Here's your " + contentSent + rating + " from " + source + ":" + Environment.NewLine);
                            image = getKonachanImage(currContent, 's', tags, whichBooru, useDownload, currUser);
                            if (image[0] == '$')
                            {
                                image = image.Substring(1, image.Length - 1);
                                await ReplyAsync(image);
                                currUser._isRequestingImage = false;
                                return;
                            }
                            else
                            {
                                if (!useDownload || currContent == contentKonachan.xml)
                                {
                                    await ReplyAsync(image);
                                }
                                else
                                {
                                    using (var client = new WebClient())
                                    {
                                        client.DownloadFile(image, "imageKonachan" + currentTime + "." + image.Split('.')[image.Split('.').Length - 1]); // https:// ?
                                    }
                                    FileInfo file = new FileInfo("imageKonachan" + currentTime + "." + image.Split('.')[image.Split('.').Length - 1]);
                                    if (file.Length >= 8000000)
                                    {
                                        await ReplyAsync("I wasn't able to send the image since its size was superior to 8MB.");
                                        tooBig = true;
                                    }
                                    else
                                        await chan.SendFileAsync("imageKonachan" + currentTime + "." + image.Split('.')[image.Split('.').Length - 1]);
                                    File.Delete("imageKonachan" + currentTime + "." + image.Split('.')[image.Split('.').Length - 1]);
                                }
                            }
                        }
                        else
                        {
                            if (whichBooru != booru.gelbooru)
                                await ReplyAsync("I'm not allowed to post that kind of content here, sorry." + Environment.NewLine
                                    + "You can request a specify 'safe' to launch a safe research.");
                            else
                                await ReplyAsync("I'm not allowed to post that kind of content here, sorry." + Environment.NewLine
                                    + "You can request a specify using Konachan and specifying 'safe' to launch a safe research.");
                            currUser._isRequestingImage = false;
                            return;
                        }
                    }
                }
                catch (TimeoutException)
                { }
                if (!tooBig)
                {
                    for (int i = 0; i < currUser.general.Count; i++)
                        currUser.general[i] = fixName(currUser.general[i]);
                    for (int i = 0; i < currUser.artists.Count; i++)
                        currUser.artists[i] = fixName(currUser.artists[i]);
                    for (int i = 0; i < currUser.characs.Count; i++)
                        currUser.characs[i] = fixName(currUser.characs[i]);
                    for (int i = 0; i < currUser.animeFrom.Count; i++)
                        currUser.animeFrom[i] = fixName(currUser.animeFrom[i]);
                    List<string> finalStrCharacs = new List<string>();
                    finalStrCharacs.Add("");
                    int indexCharacFrom = 0;
                    if (currUser.characs.Count == 1)
                        finalStrCharacs[indexCharacFrom] = currUser.characs[0];
                    else if (currUser.characs.Count > 1)
                    {
                        bool doesContainTagMe = false;
                        foreach (string s in currUser.characs)
                        {
                            if (s == "Tagme" || s == "Character Request")
                                doesContainTagMe = true;
                        }
                        for (int i = 0; i < currUser.characs.Count - 1; i++)
                        {
                            if (finalStrCharacs[indexCharacFrom].Length > 1500)
                            {
                                indexCharacFrom++;
                                finalStrCharacs.Add("");
                            }
                            if (currUser.characs[i] != "Tagme" && currUser.characs[i] != "Character Request")
                                finalStrCharacs[indexCharacFrom] += currUser.characs[i] + ", ";
                        }
                        if (!doesContainTagMe)
                        {
                            finalStrCharacs[indexCharacFrom] = finalStrCharacs[indexCharacFrom].Substring(0, finalStrCharacs[indexCharacFrom].Length - 2);
                            finalStrCharacs[indexCharacFrom] += " and " + currUser.characs[currUser.characs.Count - 1];
                        }
                        else
                        {
                            if (currUser.characs[currUser.characs.Count - 1] == "Tagme" || currUser.characs[currUser.characs.Count - 1] == "Source Request"
                                || currUser.characs[currUser.characs.Count - 1] == "Copyright Request")
                            {
                                finalStrCharacs[indexCharacFrom] = finalStrCharacs[indexCharacFrom].Substring(0, finalStrCharacs[indexCharacFrom].Length - 2);
                                finalStrCharacs[indexCharacFrom] += " and some other character who weren't tag";
                            }
                            else
                                finalStrCharacs[indexCharacFrom] += ", " + currUser.characs[currUser.characs.Count - 1] + " and some other character who weren't tag";
                        }
                    }
                    List<string> finalStrFrom = new List<string>();
                    int indexStrFrom = 0;
                    finalStrFrom.Add("");
                    if (currUser.animeFrom.Count == 1)
                        finalStrFrom[indexStrFrom] = currUser.animeFrom[0];
                    else if (currUser.animeFrom.Count > 1)
                    {
                        for (int i = 0; i < currUser.animeFrom.Count - 1; i++)
                            finalStrFrom[indexStrFrom] += currUser.animeFrom[i] + ", ";
                        finalStrFrom[indexStrFrom] = finalStrFrom[indexStrFrom].Substring(0, finalStrFrom[indexStrFrom].Length - 2);
                        finalStrFrom[indexStrFrom] += " and " + currUser.animeFrom[currUser.animeFrom.Count - 1];
                        if (finalStrFrom[indexStrFrom].Length > 1500)
                        {
                            indexStrFrom++;
                            finalStrFrom.Add("");
                        }
                    }
                    string finalStr;
                    if (currUser.animeFrom.Count == 1 && currUser.animeFrom[0] == "Original")
                        finalStr = "It look like this image is an original content." + Environment.NewLine;
                    else if (currUser.animeFrom.Count == 1 && (currUser.animeFrom[0] == "Tagme" || currUser.animeFrom[0] == "Source Request" || currUser.animeFrom[0] == "Copyright Request"))
                        finalStr = "It look like the source of this image wasn't tag." + Environment.NewLine;
                    else if (finalStrFrom[0] != "")
                    {
                        finalStr = "I think this image is from ";
                        foreach (string s in finalStrFrom)
                        {
                            if (finalStr.Length + s.Length > 1500)
                            {
                                await ReplyAsync(finalStr);
                                finalStr = "";
                            }
                            finalStr += s;
                        }
                        finalStr += "." + Environment.NewLine;
                    }
                    else
                        finalStr = "I don't know where this image is from." + Environment.NewLine;
                    if (finalStrCharacs[0] == "")
                        finalStr += "I don't know who are the characters." + Environment.NewLine;
                    else if (currUser.characs.Count == 1 && (currUser.characs[0] == "Tagme" || currUser.characs[0] == "Character Request"))
                        finalStr += "It look like the characters of this image weren't tag." + Environment.NewLine;
                    else if (currUser.characs.Count == 1)
                        finalStr += "I think the character is " + finalStrCharacs[0] + "." + Environment.NewLine;
                    else
                    {
                        if (finalStr.Length > 1500)
                        {
                            await ReplyAsync(finalStr);
                            finalStr = "";
                        }
                        finalStr += "I think the characters are ";
                        foreach (string s in finalStrCharacs)
                        {
                            if ((finalStr.Length + s.Length) > 1500)
                            {
                                await ReplyAsync(finalStr);
                                finalStr = "";
                            }
                            finalStr += s;
                        }
                        finalStr += "." + Environment.NewLine;
                    }
                    if (displayTags == "debug")
                    {
                        finalStr += "All the others tags are:" + Environment.NewLine;
                        for (int i = 0; i < currUser.general.Count; i++)
                        {
                            if (i < currUser.general.Count - 2)
                                finalStr += currUser.general[i] + ", ";
                            else if (i == currUser.general.Count - 2)
                                finalStr += currUser.general[i] + " and ";
                            else
                                finalStr += currUser.general[i] + ".";
                            if (finalStr.Length >= 1500)
                            {
                                await ReplyAsync(finalStr);
                                finalStr = "";
                            }
                        }
                    }
                    //await ReplyAsync("Here are all the tags who were here: " + p.debugTags.Substring(0, p.debugTags.Length - 3));
                    await ReplyAsync(finalStr);
                    if (displayTags == "debug")
                    {
                        finalStr = "";
                        List<float> allScores = new List<float>();
                        List<float> multiplicators = new List<float>();

                        string fileTag;
                        string[] desc = new string[] { "general", "artist", "", "source", "character" };
                        List<string>[] lists = new List<string>[] { currUser.general, currUser.artists, null, currUser.animeFrom, currUser.characs };
                        for (int i = 0; i < 1; i++)
                        {
                            p.lockBooru.WaitOne();
                            if (lists[i].Count > 0)
                            {
                                foreach (string t in lists[i])
                                {
                                    fileTag = "";
                                    foreach (char c in t)
                                    {
                                        if (char.IsLetterOrDigit(c))
                                            fileTag += c;
                                    }
                                    if (File.Exists("Saves/booruAnalysis/" + source + "/" + fileTag + ".dat"))
                                    {
                                        string[] content = File.ReadAllLines("Saves/booruAnalysis/" + source + "/" + fileTag + ".dat");
                                        int e = Convert.ToInt32(content[1]);
                                        int q = Convert.ToInt32(content[2]);
                                        int s = Convert.ToInt32(content[3]);
                                        float score = ((e * 100 / (e + q + s)) * 1f) + ((q * 100 / (e + q + s)) * 0.5f);
                                        allScores.Add(score);
                                        float x = score * 0.01f / 100;
                                        float xMax = 100 * 0.01f / 100;
                                        float multiplicator = ((10 * x) / (x + 0.01f)) * 120 / ((10 * xMax) / (xMax + 0.01f)) - 20;
                                        //float multiplicator = x * 100 / xMax;
                                        if (multiplicator < 0)
                                            multiplicator = 0;
                                        multiplicators.Add(multiplicator);
                                        // finalStr += " **(" + t + ", " + multiplicator + "-" + score + ")** " + ((10 * x) / (x + 0.01)) + " / " + ((10 * xMax) / (xMax + 0.01)) + Environment.NewLine;
                                        /*if (finalStr.Length > 1500)
                                        {
                                            await ReplyAsync(finalStr);
                                            finalStr = "";
                                        }*/
                                    }
                                }
                            }
                            p.lockBooru.ReleaseMutex();
                            finalStr += Environment.NewLine;
                        }
                        float finalKaiNi = 0;
                        for (int i = 0; i < allScores.Count; i++)
                        {
                            finalKaiNi += allScores[i] * multiplicators[i];
                            //while (finalKaiNi > 100)
                            //  finalKaiNi = finalKaiNi / 100;
                        }
                        finalKaiNi /= multiplicators.Sum();
                        finalStr += "**(BETA) Lewdness rate is of " + finalKaiNi.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture) + "%.**" + Environment.NewLine;
                        if ((currUser.rating == 's' && finalKaiNi > 33.3f) || (currUser.rating == 'q' && (finalKaiNi < 33.3f || finalKaiNi > 66.6f)) || (currUser.rating == 'e' && finalKaiNi < 66.6f))
                            finalStr += "**WARNING: This rate doesn't seam coherant with the rating " + currUser.rating + " of the image.**";
                        await ReplyAsync(finalStr);
                    }
                }
                currUser._isRequestingImage = false;
            }
        }

        public static string fixName(string original)
        {
            original = Regex.Replace(original, @"\(([^\)]+)\)", "");
            string newName = "";
            bool isPreviousSpace = true;
            foreach (char c in original)
            {
                if (isPreviousSpace)
                    newName += char.ToUpper(c);
                else if (c == '_')
                    newName += ' ';
                else
                    newName += c;
                isPreviousSpace = (c == '_');
            }
            newName = newName.Trim();
            return newName;
        }

        public static string getElementXml(string tag, string saveString, string file, char stopCharac)
        {
            int prog = 0;
            char lastChar = ' ';
            foreach (char c in file)
            {
                if (prog == tag.Length)
                {
                    if (c == stopCharac
                        && ((stopCharac == '"' && lastChar != '\\') || stopCharac != '"'))
                        break;
                    saveString += c;
                }
                else
                {
                    if (c == tag[prog])
                    {
                        prog++;
                    }
                    else
                        prog = 0;
                }
                lastChar = c;
            }
            return (saveString);
        }

        public static string getWebRequest(string request)
        {
            string xml = "";
            XmlDocument responseXml = new XmlDocument(); using (WebClient w = new WebClient())
            {
                w.Encoding = Encoding.UTF8;
                xml = w.DownloadString(request);
            }
            return (xml);
        }

        [Command("Start learning booru"), Summary("Begin to learn for booru")]
        public async Task learnShiritori()
        {
            p.doAction(Context.User, Context.Guild.Id, Program.Module.Booru);
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
                string[] progBooru = File.ReadAllLines("Saves/booruAnalysis/stats.dat");
                p.nbRequestKonaBase = Convert.ToInt32(progBooru[1]);
                p.nbRequestGelBase = Convert.ToInt32(progBooru[2]);
                p.nbRequestLoliBase = Convert.ToInt32(progBooru[3]);
                p.nbRequestSafeBase = Convert.ToInt32(progBooru[4]);
                p.nbRequestKona = p.nbRequestKonaBase;
                p.nbRequestGel = p.nbRequestGelBase;
                p.nbRequestLoli = p.nbRequestLoliBase;
                p.nbRequestSafe = p.nbRequestSafeBase;
                p.timeLearn = DateTime.UtcNow;
                await ReplyAsync("Alright, I will begin to learn!");
                p.prepareStopLearn = false;
                p.isLearning = true;
                p.isLearningBooru = true;
            }
        }

        [Command("Stop learning booru"), Summary("Stop to learn for booru")]
        public async Task stopLearnShiritori()
        {
            p.doAction(Context.User, Context.Guild.Id, Program.Module.Booru);
            if (!p.isLearning)
            {
                await ReplyAsync("I'm not currently learning.");
            }
            else if (!p.isLearningBooru)
            {
                await ReplyAsync("I'm not currently learning booru.");
            }
            else if (Context.User.Id != 144851584478740481)
            {
                await ReplyAsync(Sentences.onlyMasterStr);
            }
            else
            {
                p.stopChan = (ITextChannel)Context.Channel;
                p.prepareStopLearn = true;
            }
        }

        [Command("Stats tag"), Summary("Get some stats about a tag")]
        public async Task getStatsTag(string tag)
        {
            p.doAction(Context.User, Context.Guild.Id, Program.Module.Booru);
            string fileTag = "";
            foreach (char c in tag)
            {
                if (char.IsLetterOrDigit(c))
                    fileTag += c;
            }
            string[] boorus = new string[] { "Konachan", "Gelbooru", "Lolibooru", "Safebooru" };
            string finalStr = "";
            foreach (string b in boorus)
            {
                if (File.Exists("Saves/booruAnalysis/" + b + "/" + fileTag + ".dat"))
                {
                    string[] content = File.ReadAllLines("Saves/booruAnalysis/" + b + "/" + fileTag + ".dat");
                    int e = Convert.ToInt32(content[1]);
                    int q = Convert.ToInt32(content[2]);
                    int s = Convert.ToInt32(content[3]);
                    finalStr += "For this tag in " + b + " in my database, there are " + (e * 100 / (e + q + s)) + "% of explicit, " + (q * 100 / (e + q + s)) + "% of questionable and "
                         + (s * 100 / (e + q + s)) + "% of safe." + Environment.NewLine;
                }
                else
                    finalStr += "I didn't register this tag for " + b + " in my database." + Environment.NewLine;
            }
            await ReplyAsync(finalStr);
        }

        [Command("Definition tag"), Summary("Get the definition of a tag from Konachan")]
        public async Task getDefinitionTag(string tag)
        {
            p.doAction(Context.User, Context.Guild.Id, Program.Module.Booru);
            string fileTag = "";
            foreach (char c in tag)
            {
                if (char.IsLetterOrDigit(c))
                    fileTag += c;
            }
            if (File.Exists("Saves/booruAnalysis/Konachan/" + fileTag + ".dat"))
            {
                try
                {
                    tag = File.ReadAllLines("Saves/booruAnalysis/Konachan/" + fileTag + ".dat")[4];
                }
                catch (IndexOutOfRangeException) { }
            }
            string xml = getWebRequest("https://konachan.com/wiki.xml?query=" + tag);
            string[] allDefs = xml.Split(new string[] { "wiki-page" }, StringSplitOptions.None);
            foreach (string def in allDefs)
            {
                if (getElementXml("title=\"", "", def, '"') == tag)
                {
                    await ReplyAsync("Here is the definition of this tag:");
                    string finalDef = getElementXml("body=\"", "", def, '"');
                    finalDef = finalDef.Replace("*", Environment.NewLine);
                    finalDef = finalDef.Replace("[[", "");
                    finalDef = finalDef.Replace("]]", "");
                    finalDef = finalDef.Replace("{{", "");
                    finalDef = finalDef.Replace("}}", "");
                    finalDef = finalDef.Replace("&quot;", "\"");
                    finalDef = finalDef.Replace("&lt;", " ");
                    finalDef = finalDef.Replace("&gt;", " ");
                    finalDef = finalDef.Replace("[b]", "**");
                    finalDef = finalDef.Replace("[/b]", "**");
                    finalDef = finalDef.Replace("[i]", "*");
                    finalDef = finalDef.Replace("[/i]", "*");
                    finalDef = Regex.Replace(finalDef, @"&#[0-9][0-9];", "");
                    finalDef = Regex.Replace(finalDef, @"h[1-6]", "");
                    await ReplyAsync(finalDef);
                    return;
                }
            }
            await ReplyAsync("I'm sorry I didn't find any definition for this tag.");
        }
    }

    partial class Program
    {
        private async void endLearnBooru()
        {
            isLearning = false;
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
            File.WriteAllText("Saves/booruAnalysis/stats.dat", seconds + Convert.ToDouble(File.ReadAllLines("Saves/booruAnalysis/stats.dat")[0]) + Environment.NewLine +
                p.nbRequestKona + Environment.NewLine + p.nbRequestGel + Environment.NewLine + p.nbRequestLoli + Environment.NewLine + p.nbRequestSafe);
            if (p.nbRequestKona == p.nbRequestKonaBase && p.nbRequestGel == p.nbRequestGelBase && p.nbRequestLoli == p.nbRequestLoliBase && p.nbRequestSafe == p.nbRequestSafeBase)
                await stopChan.SendMessageAsync("That's mean, you didn't even let me the time to begin to learn...");
            else
                await stopChan.SendMessageAsync("After " + finalTime + ", I learned new tags from "
                        + (p.nbRequestKona - p.nbRequestKonaBase + p.nbRequestGel - p.nbRequestGelBase + p.nbRequestLoli - p.nbRequestLoliBase + p.nbRequestSafe - p.nbRequestSafeBase) + " images.");
        }

        private void learnBooru()
        {
            BooruModule.booru whichBooru = (BooruModule.booru)(rand.Next(0, 4));
            string xml;
            string link;
            string destination = "Saves/booruAnalysis/";
            if (whichBooru == BooruModule.booru.konachan)
            {
                nbRequestKona++;
                link = "https://www.konachan.com/post.xml?page=" + nbRequestKona + "&limit=1";
                destination += "Konachan";
            }
            else if (whichBooru == BooruModule.booru.gelbooru)
            {
                nbRequestGel++;
                link = "https://gelbooru.com/index.php?page=dapi&s=post&q=index&pid=" + (nbRequestGel - 1) + "&limit=1";
                destination += "Gelbooru";
            }
            else if (whichBooru == BooruModule.booru.lolibooru)
            {
                nbRequestLoli++;
                link = "https://lolibooru.moe/post.xml?page=" + nbRequestLoli + "&limit=1";
                destination += "Lolibooru";
            }
            else
            {
                nbRequestSafe++;
                link = "http://safebooru.org/index.php?page=dapi&s=post&q=index&pid=" + (nbRequestSafe - 1) + "&limit=1";
                destination += "Safebooru";
            }
            char imageRating;
            string[] allTags;
            while (true)
            {
                xml = null;
                try
                {
                    xml = BooruModule.getWebRequest(link);
                    imageRating = BooruModule.getElementXml("rating=\"", "", xml, '"')[0]; // System index out of range 
                    allTags = BooruModule.getElementXml("tags=\"", "", xml, '"').Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                    break;
                }
                catch (WebException we)
                {
                    Console.WriteLine("Line 892: " + we.Message);
                }
                catch (IndexOutOfRangeException)
                {
                    Console.WriteLine(link);
                    Console.WriteLine(xml);
                    throw new Exception();
                    int b = 0;
                    int a = 3 / b;
                }
            }
            using (WebClient w = new WebClient())
            {
                w.Encoding = Encoding.UTF8;
                foreach (string t in allTags)
                {
                    string fileTag = "";
                    foreach (char c in t)
                    {
                        if (char.IsLetterOrDigit(c))
                            fileTag += c;
                    }
                    if (File.Exists(destination + "/" + fileTag + ".dat"))
                    {
                        string[] content = File.ReadAllLines(destination + "/" + fileTag + ".dat");
                        if (imageRating == 'e')
                            File.WriteAllText(destination + "/" + fileTag + ".dat",
                                content[0] + Environment.NewLine + (Convert.ToInt32(content[1]) + 1).ToString() + Environment.NewLine + content[2] + Environment.NewLine + content[3] + Environment.NewLine + t);
                        else if (imageRating == 'q')
                            File.WriteAllText(destination + "/" + fileTag + ".dat",
                                content[0] + Environment.NewLine + content[1] + Environment.NewLine + (Convert.ToInt32(content[2]) + 1).ToString() + Environment.NewLine + content[3] + Environment.NewLine + t);
                        else if (imageRating == 's')
                            File.WriteAllText(destination + "/" + fileTag + ".dat",
                                content[0] + Environment.NewLine + content[1] + Environment.NewLine + content[2] + Environment.NewLine + (Convert.ToInt32(content[3]) + 1).ToString() + Environment.NewLine + t);
                        else
                            throw new FormatException("Wrong character '" + imageRating + "'.");
                    }
                    else
                    {
                        string newTag;
                        newTag = t;
                        newTag = newTag.Replace('+', '_');
                        string xml2;
                        while (true)
                        {
                            try
                            {
                                if (whichBooru == BooruModule.booru.gelbooru)
                                    xml2 = w.DownloadString("https://gelbooru.com/index.php?page=dapi&s=tag&q=index&name=" + newTag);
                                else if (whichBooru == BooruModule.booru.konachan)
                                    xml2 = w.DownloadString("https://konachan.com/tag.xml?limit=10000&name=" + newTag);
                                else if (whichBooru == BooruModule.booru.lolibooru)
                                    xml2 = w.DownloadString("https://lolibooru.moe/tag.xml?limit=10000&name=" + newTag);
                                else
                                    xml2 = w.DownloadString("https://safebooru.org/index.php?page=dapi&s=tag&q=index&name=" + newTag);
                                break;
                            }
                            catch (WebException we)
                            {
                                Console.WriteLine("Line 950: " + we.Message);
                            }
                        }
                        foreach (string s in xml2.Split('<'))
                        {
                            if (BooruModule.getElementXml("name=\"", "", s, '"') == newTag)
                            {
                                if (imageRating == 'e')
                                    File.WriteAllText(destination + "/" + fileTag + ".dat",
                                        BooruModule.getElementXml("type=\"", "", s, '"') + Environment.NewLine + 1 + Environment.NewLine + 0 + Environment.NewLine + 0 + Environment.NewLine + t);
                                else if (imageRating == 'q')
                                    File.WriteAllText(destination + "/" + fileTag + ".dat",
                                        BooruModule.getElementXml("type=\"", "", s, '"') + Environment.NewLine + 0 + Environment.NewLine + 1 + Environment.NewLine + 0 + Environment.NewLine + t);
                                else if (imageRating == 's')
                                    File.WriteAllText(destination + "/" + fileTag + ".dat",
                                        BooruModule.getElementXml("type=\"", "", s, '"') + Environment.NewLine + 0 + Environment.NewLine + 0 + Environment.NewLine + 1 + Environment.NewLine + t);
                                else
                                    throw new FormatException("Wrong character '" + imageRating + "'.");
                                break;
                            }
                        }
                    }
                }
                if (prepareStopLearn)
                    endLearnBooru();
            }
        }
    }
}