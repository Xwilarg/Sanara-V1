using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Bot
{
    public class NhentaiModule : ModuleBase
    {
        Program p = Program.p;
        public async void postImage(string image, bool download)
        {
            if (download)
            {
                int currentTime = Convert.ToInt32(DateTime.Now.ToString("HHmmss"));
                using (var client = new WebClient())
                {
                    client.DownloadFile(image, "imageNhentai" + currentTime + "." + image.Split('.')[image.Split('.').Length - 1]);
                }
                await Context.Channel.SendFileAsync("imageNhentai" + currentTime + "." + image.Split('.')[image.Split('.').Length - 1]);
            }
            else
                await ReplyAsync(image);
        }

        [Command("nhentai next", RunMode = RunMode.Async)]
        public async Task nextNhentai()
        {
            p.doAction(Context.User, Context.Guild.Id, Program.Module.Nhentai);
            try
            {
                Character c = p.relations.Find(x => x.getName() == Context.User.Id);
                if (c._currDoujinshi == 0)
                    await ReplyAsync("You didn't load any doujinshi, please use the load command before.");
                else if (!Context.Channel.IsNsfw)
                    await ReplyAsync("I can only send doujinshi in NSFW channels.");
                else if (c._currPage < c._maxDoujinshi)
                {
                    c._currPage++;
                    await ReplyAsync("Page " + c._currPage + "/" + c._maxDoujinshi);
                    postImage("https://i.nhentai.net/galleries/" + c._currDoujinshi + "/" + c._currPage + ".jpg", c._doujinshiSendMethodDownload);
                }
                else
                    await ReplyAsync("You are on the last page, you can't go any further.");
            }
            catch (WebException we)
            {
                if (we.Response as HttpWebResponse == null)
                    await ReplyAsync("An unexpected error happened. Please retry later.");
            }
        }

        [Command("nhentai previous", RunMode = RunMode.Async)]
        public async Task prevNhentai()
        {
            p.doAction(Context.User, Context.Guild.Id, Program.Module.Nhentai);
            try
            {
                Character c = p.relations.Find(x => x.getName() == Context.User.Id);
                if (c._currDoujinshi == 0)
                    await ReplyAsync("You didn't load any doujinshi, please use the load command before.");
                else if (!Context.Channel.IsNsfw)
                    await ReplyAsync("I can only send doujinshi in NSFW channels.");
                else if (c._currPage > 1)
                {
                    c._currPage--;
                    await ReplyAsync("Page " + c._currPage + "/" + c._maxDoujinshi);
                    postImage("https://i.nhentai.net/galleries/" + c._currDoujinshi + "/" + c._currPage + ".jpg", c._doujinshiSendMethodDownload);
                }
                else
                    await ReplyAsync("You are on the first page, you can't go back.");
            }
            catch (WebException we)
            {
                if (we.Response as HttpWebResponse == null)
                    await ReplyAsync("An unexpected error happened. Please retry later.");
            }
        }

        [Command("nhentai jump", RunMode = RunMode.Async)]
        public async Task jumpNhentai(string pageNumber)
        {
            p.doAction(Context.User, Context.Guild.Id, Program.Module.Nhentai);
            Character c = p.relations.Find(x => x.getName() == Context.User.Id);
            try
            {
                int nb = Convert.ToInt32(pageNumber);
                if (c._currDoujinshi == 0)
                    await ReplyAsync("You didn't load any doujinshi, please use the load command before.");
                else if (!Context.Channel.IsNsfw)
                    await ReplyAsync("I can only send doujinshi in NSFW channels.");
                else if (nb < 1 || nb > c._maxDoujinshi)
                    await ReplyAsync("The page number must be between 1 and " + c._maxDoujinshi + ".");
                else
                {
                    c._currPage = nb;
                    await ReplyAsync("Page " + c._currPage + "/" + c._maxDoujinshi);
                    postImage("https://i.nhentai.net/galleries/" + c._currDoujinshi + "/" + c._currPage + ".jpg", c._doujinshiSendMethodDownload);
                }
            }
            catch (WebException we)
            {
                if (we.Response as HttpWebResponse == null)
                    await ReplyAsync("An unexpected error happened. Please retry later.");
            }
            catch (FormatException)
            {
                await ReplyAsync("You must give a page number between 1 and " + c._maxDoujinshi + " in parameter.");
            }
        }

        [Command("nhentai stop")]
        public async Task stopNhentai()
        {
            p.doAction(Context.User, Context.Guild.Id, Program.Module.Nhentai);
            Character c = p.relations.Find(x => x.getName() == Context.User.Id);
            if (c._currDoujinshi == 0)
                await ReplyAsync("You didn't load any doujinshi, please use the load command before.");
            else
            {
                c._currDoujinshi = 0;
                c._maxDoujinshi = 0;
                c._currPage = 1;
                c._doujinshiSendMethodDownload = true;
                await ReplyAsync("Done.");
            }
        }

        [Command("nhentai load", RunMode = RunMode.Async)]
        public async Task loadNhentai(int id, string method = "download")
        {
            p.doAction(Context.User, Context.Guild.Id, Program.Module.Nhentai);
            try
            {
                if (!Context.Channel.IsNsfw)
                {
                    await ReplyAsync("I can only send doujinshi in NSFW channels.");
                    return;
                }
                if (method != "url" && method != "download")
                {
                    await ReplyAsync("I'm sorry but I don't understand how you want me to get the image. Please check the format of your request.");
                    return;
                }
                string xml;
                using (WebClient w = new WebClient())
                {
                    w.Encoding = Encoding.UTF8;
                    xml = w.DownloadString("https://nhentai.net/api/gallery/" + id);
                }
                Character c = p.relations.Find(x => x.getName() == Context.User.Id);
                c._currDoujinshi = Convert.ToInt32(BooruModule.getElementXml("\"media_id\":\"", "", xml, '"'));
                c._maxDoujinshi = Convert.ToInt32(BooruModule.getElementXml("\"num_pages\":", "", xml, '}'));
                c._currPage = 1;
                c._doujinshiSendMethodDownload = (method == "download");
                await ReplyAsync("Page " + c._currPage + "/" + c._maxDoujinshi);
                postImage("https://i.nhentai.net/galleries/" + c._currDoujinshi + "/" + c._currPage + ".jpg", c._doujinshiSendMethodDownload);
            }
            catch (WebException we)
            {
                HttpWebResponse r = we.Response as HttpWebResponse;
                if (r == null)
                    await ReplyAsync("An unexpected error happened. Please retry later.");
                else if (r.StatusCode == HttpStatusCode.Forbidden)
                    await ReplyAsync("I didn't find any doujinshi with the id " + id + ".");
            }
        }

        [Command("nhentai search", RunMode = RunMode.Async)]
        public async Task getNhentai(params string[] keywords)
        {
            p.doAction(Context.User, Context.Guild.Id, Program.Module.Nhentai);
            try
            {
                if (!Context.Channel.IsNsfw)
                {
                    await ReplyAsync("I can only send doujinshi in NSFW channels.");
                    return;
                }
                string tags = "";
                if (keywords.Length != 0)
                {
                    foreach (string s in keywords)
                    {
                        tags += s + "+";
                    }
                    tags = tags.Substring(0, tags.Length - 1);
                }
                string xml;
                using (WebClient w = new WebClient())
                {
                    w.Encoding = Encoding.UTF8;
                    if (keywords.Length == 0)
                        xml = w.DownloadString("https://nhentai.net/api/galleries/all?page=0");
                    else
                        xml = w.DownloadString("https://nhentai.net/api/galleries/search?query=" + tags + "&page=8000");
                }
                int page = p.rand.Next(Convert.ToInt32(BooruModule.getElementXml("\"num_pages\":", "", xml, ','))) + 1;
                using (WebClient w = new WebClient())
                {
                    w.Encoding = Encoding.UTF8;
                    if (keywords.Length == 0)
                        xml = w.DownloadString("https://nhentai.net/api/galleries/all?page=" + page);
                    else
                        xml = w.DownloadString("https://nhentai.net/api/galleries/search?query=" + tags + "&page=" + page);
                }
                List <string> allDoujinshi = xml.Split(new string[] { "title" }, StringSplitOptions.None).ToList();
                allDoujinshi.RemoveAt(0);
                if (allDoujinshi.Count == 0)
                {
                    string[] allTags = tags.Split(new string[] { "+" }, StringSplitOptions.RemoveEmptyEntries);
                    if (allTags.Length == 1)
                        await ReplyAsync("I didn't find any doujinshi with the tag '" + allTags[0] + "'.");
                    string finalTags = "I didn't find any doujinshi with the tags ";
                    for (int i = 0; i < allTags.Length - 1; i++)
                        finalTags += "'" + allTags[i] + "', ";
                    await ReplyAsync(finalTags.Substring(0, finalTags.Length - 2) + " and '" + allTags[allTags.Length - 1] + "'.");
                }
                else
                {
                    string curr = allDoujinshi[p.rand.Next(allDoujinshi.Count)];
                    string[] ids = curr.Split(new string[] { "}]" }, StringSplitOptions.None);
                    string currBlock = "";
                    for (int i = ids.Length - 1; i >= 0; i--)
                    {
                        currBlock = BooruModule.getElementXml("id\":", "", ids[i], ',');
                        if (currBlock != "")
                        {
                            if (keywords.Length == 0)
                                await ReplyAsync("https://nhentai.net/g/" + currBlock);
                            else
                            {
                                string finalOk = "";
                                foreach (string t in keywords)
                                {
                                    bool isOk = false;
                                    foreach (string s in ids[i - 1].Split(new string[] { "},{" }, StringSplitOptions.None))
                                    {
                                        if (BooruModule.getElementXml("\"name\":\"", "", s, '"').Contains(t))
                                        {
                                            isOk = true;
                                            break;
                                        }
                                    }
                                    if (!isOk)
                                    {
                                        finalOk = t;
                                        break;
                                    }
                                }
                                if (finalOk == "")
                                    await ReplyAsync("https://nhentai.net/g/" + currBlock);
                                else
                                    await ReplyAsync("I didn't find any doujinshi with the tag '" + finalOk + "'.");
                            }
                            break;
                        }
                    }
                }
            }
            catch (WebException we)
            {
                if (we.Response as HttpWebResponse == null)
                    await ReplyAsync("An unexpected error happened. Please retry later.");
            }
        }
    }
}