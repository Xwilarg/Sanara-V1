using Discord.Commands;
using Google;
using Google.Apis.Services;
using Google.Apis.Urlshortener.v1;
using Google.Apis.Urlshortener.v1.Data;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Bot
{
    public class GoogleShortenerModule : ModuleBase
    {
        Program p = Program.p;

        [Command("Random goo.gl URL", RunMode = RunMode.Async), Summary("Give a random pastebin")]
        public async Task randomPastebin()
        {
            p.doAction(Context.User, Context.Guild.Id, Program.Module.GoogleShortener);
            if (!Context.Channel.IsNsfw)
            {
                await ReplyAsync("Since we don't know what kind of content I can find, I can only let you do that in a NSFW channel.");
            }
            else
            {
                string result = "";
                string shortResult = "";
                int iteration = 1;
                string finalStr;
                UrlshortenerService service = new UrlshortenerService(new BaseClientService.Initializer
                {
                    ApiKey = File.ReadAllText("Keys/URLShortenerAPIKey.dat"),
                });
                using (WebClient wc = new WebClient())
                {
                    wc.Encoding = Encoding.UTF8;
                    while (true)
                    {
                        finalStr = "";
                        for (int i = 0; i < 6; i++)
                        {
                            int nb = p.rand.Next(0, 62);
                            if (nb < 26)
                                finalStr += (char)(nb + 'a');
                            else if (nb < 52)
                                finalStr += (char)(nb + 'A' - 26);
                            else
                                finalStr += (char)(nb + '0' - 52);
                        }
                        try
                        {
                            Url response = await service.Url.Get("http://goo.gl/" + finalStr).ExecuteAsync();
                            result = response.LongUrl;
                            shortResult = response.Id;
                            break;
                        }
                        catch (GoogleApiException ex)
                        {
                            if (ex.HttpStatusCode == HttpStatusCode.NotFound) iteration++;
                            else if (ex.HttpStatusCode == HttpStatusCode.Forbidden)
                            {
                                await ReplyAsync("Seam like I exceed the number of requests on the goo.gl API. You should wait a bit before retrying.");
                                return;
                            }
                        }
                        if (iteration == 500) break;
                    }
                }
                if (iteration == 500)
                    await ReplyAsync("I didn't find anything after 500 iterations.");
                else
                {
                    await ReplyAsync("I found something, here is the short URL: " + shortResult + Environment.NewLine
                        + ((result != null) ? ("It'll lead you here: " + result) : ("It will lead you nowhere since the URL was disabled...")));
                }
            }
        }
    }
}