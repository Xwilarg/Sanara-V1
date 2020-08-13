using System;

namespace Bot
{
    public static class Sentences
    {
        // All
        public const string onlyMasterStr = "Only my master can order me that, sorry.";
        public const string moduleDisabledStr = "Sorry but this module was disabled.";

        // Games (not finished)

        // Debug (not finished)

        // Communication (not finished)
        public const string hiStr = "Hi~";
        public const string goodnightStr = "Thanks, have nice dreams!";
        public const string howAreYou1Str = "I'm fine, it's nice to be here speaking with people.";
        public const string howAreYou3Str = "I'm fine but I need to admit that the requests of some people are a bit disturbing.";
        public const string howAreYou5Str = "I'm fine, but I wish I had more time to study.";
        public static string whoIAmStr(string currDate)
        {
            return ("My name is Sanaya Miyuki (差成夜 深雪). You write my first name with the kanjis for shine, become and evening and my last name with the kanjis for deep snow, but you can just call me Sanara." + Environment.NewLine +
                    "My first memories are from the " + currDate + " UTC+0, even though I was born the 28/08/17" + Environment.NewLine +
                    "Please forgive me if I don't understand everything you say, I'm quite young, but my master, Zirk is doing his best to help me learning things.");
        }

        // Character (not finished)

        // Bot (not finished)

        // Arena (not finished)

        // ActionArena (not finished)

        // Gladiator (not finished)

        // Konachan (not finished)

        // Settings (not finished)
        public const string alreadyKnowStr =
            "I already know that.";
        public const string alreadyDefaultChannelStr =
            "This channel is already the default channel.";
        public const string setDefaultChannelStr =
            "Understood, I'll now use this channel as the default channel.";
        public const string alreadyDefaultServerStr =
            "This server is already the default server.";
        public const string setDefaultServerStr =
            "Understood, I'll now use this server as the default server.";
        public const string sayHiJoinStr =
            "Understood, I'll say hi when I'll join this server.";
        public const string dontSayHiJoinStr =
            "Understood, I won't say hi when I'll join this server.";
        public const string leaverServer =
            "Alright. It was nice to know you @everyone. I hope we will meet each other again.";
        public static string createArchiveStr(string currTime)
        { return ($"I created the new archive {currTime} to save my datas, thanks !"); }
    }
}