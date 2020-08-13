using Discord.Commands;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Bot
{
    public class YoutubeModule : ModuleBase
    {
        Program p = Program.p;
        [Command("Get youtube video channel"), Summary("Get a random video given a playlist")]
        public async Task getRandomVideo(string id, string maxNb = "50")
        {
            p.doAction(Context.User, Context.Guild.Id, Program.Module.Youtube);
            YouTubeService yt = new YouTubeService(new BaseClientService.Initializer() { ApiKey = File.ReadAllText("Keys/YoutubeAPIKey.dat") });
            var searchListRequest = yt.Search.List("snippet");
            searchListRequest.ChannelId = id;
            try
            {
                searchListRequest.MaxResults = Convert.ToInt32(maxNb);
                if (searchListRequest.MaxResults < 1 || searchListRequest.MaxResults > 50)
                {
                    await ReplyAsync("The number max of video must be between 1 and 50.");
                    return;
                }
            }
            catch (FormatException)
            {
                await ReplyAsync("The number max of video must be a number.");
                return;
            }
            catch (OverflowException)
            {
                await ReplyAsync("The number max of video must be between 1 and 50.");
                return;
            }
            Google.Apis.YouTube.v3.Data.SearchListResponse searchListResult = null;
            try
            {
                searchListResult = searchListRequest.Execute();
            }
            catch (Google.GoogleApiException gae)
            {
                if (gae.Error.Code == 404)
                {
                    await ReplyAsync("I didn't find any channel with this id.");
                    return;
                }
            }
            if (searchListResult.Items.Count == 0)
            {
                await ReplyAsync("This channel doesn't contain any video.");
                return;
            }
            await ReplyAsync("Here is a random video of " + searchListResult.Items[0].Snippet.ChannelTitle + ":");
            await ReplyAsync("https://www.youtube.com/watch?v=" + searchListResult.Items[p.rand.Next(searchListResult.Items.Count)].Id.VideoId);
        }

        [Command("Get youtube video keyword"), Summary("Get a random video given a playlist")]
        public async Task randomVideo(string keyword, string maxNb = "50")
        {
            p.doAction(Context.User, Context.Guild.Id, Program.Module.Youtube);
            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = File.ReadAllText("Keys/YoutubeAPIKey.dat")
            });

            var searchListRequest = youtubeService.Search.List("snippet");
            searchListRequest.Q = keyword;
            try
            {
                searchListRequest.MaxResults = Convert.ToInt32(maxNb);
                if (searchListRequest.MaxResults < 1 || searchListRequest.MaxResults > 50)
                {
                    await ReplyAsync("The number max of video must be between 1 and 50.");
                    return;
                }
            }
            catch (FormatException)
            {
                await ReplyAsync("The number max of video must be a number.");
                return;
            }
            catch (OverflowException)
            {
                await ReplyAsync("The number max of video must be between 1 and 50.");
                return;
            }
            var searchListResponse = await searchListRequest.ExecuteAsync();
            await ReplyAsync("Here is a random video with the keyword \"" + keyword + "\":");
            if (searchListResponse.Items.Count == 0)
            {
                await ReplyAsync("I didn't find any video with this keyword.");
                return;
            }
            Google.Apis.YouTube.v3.Data.SearchResult sr;
            do
            {
                sr = searchListResponse.Items[p.rand.Next(0, searchListResponse.Items.Count)];
            } while (sr.Id.Kind != "youtube#video");
            await ReplyAsync("https://www.youtube.com/watch?v=" + sr.Id.VideoId);
        }
    }
}