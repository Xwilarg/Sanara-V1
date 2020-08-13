using Discord.Commands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Bot
{
    public class JishoModule : ModuleBase
    {
        Program p = Program.p;

        [Command("To hiragana"), Summary("Romanji to hiragana")]
        public async Task toHiraganaCmd(string word)
        {
            p.doAction(Context.User, Context.Guild.Id, Program.Module.Jisho);
            if (File.ReadAllLines("Saves/Servers/" + Context.Guild.Id + "/serverDatas.dat")[5] == "0")
            {
                await ReplyAsync("Sorry but this module was disabled.");
            }
            else
            {
                await ReplyAsync(toHiragana(word));
            }
        }

        [Command("To romaji"), Summary("Hiragana to romaji")]
        public async Task toRomajiCmd(string word)
        {
            p.doAction(Context.User, Context.Guild.Id, Program.Module.Jisho);
            if (File.ReadAllLines("Saves/Servers/" + Context.Guild.Id + "/serverDatas.dat")[5] == "0")
            {
                await ReplyAsync("Sorry but this module was disabled.");
            }
            else
            {
                await ReplyAsync(toRomaji(word));
            }
        }

        [Command("Jisho meaning"), Summary("Give the meaning of a word")]
        public async Task meaning(string word, string all = "")
        {
            p.doAction(Context.User, Context.Guild.Id, Program.Module.Jisho);
            if (File.ReadAllLines("Saves/Servers/" + Context.Guild.Id + "/serverDatas.dat")[5] == "0")
            {
                await ReplyAsync("Sorry but this module was disabled.");
            }
            else
            {
                foreach (string s in getAllKanjis(word, ((all == "all") ? (true) : (false))))
                {
                    await ReplyAsync(s);
                }
                await ReplyAsync("Done.");
            }
        }

        [Command("Jisho combinaison", RunMode = RunMode.Async), Summary("Give the meaning of a word")]
        public async Task combinaison(string allKanjis, string isKanjis = "kanji", string definition = "definition", string acceptHiragana = "kanjiOnly", string displayRanking = "dontDisplayRanking")
        {
            p.doAction(Context.User, Context.Guild.Id, Program.Module.Jisho);
            if (File.ReadAllLines("Saves/Servers/" + Context.Guild.Id + "/serverDatas.dat")[5] == "0")
            {
                await ReplyAsync("Sorry but this module was disabled.");
            }
            else if (isKanjis != "kanji" && isKanjis != "hiragana")
            {
                await ReplyAsync("I'm sorry but I didn't understand how you want me to display the results.");
            }
            else if (definition != "definition" && definition != "noDefinition")
            {
                await ReplyAsync("I'm sorry but I didn't understand if you want the definitions or not.");
            }
            else
            {
                if (allKanjis.Length > 21 && allKanjis.Substring(0, 21) == "https://pastebin.com/")
                {
                    string json;
                    using (WebClient wc = new WebClient())
                    {
                        wc.Encoding = Encoding.UTF8;
                        try
                        {
                            json = wc.DownloadString("https://pastebin.com/" + allKanjis.Split('/')[allKanjis.Split('/').Length - 1] + "/raw");
                            allKanjis = BooruModule.getElementXml("<ol class=\"text\"><li class=\"li1\"><div class=\"de1\">", "", json, '<');
                        }
                        catch (WebException ex)
                        {
                            HttpWebResponse code = ex.Response as HttpWebResponse;
                            if (code.StatusCode == HttpStatusCode.NotFound)
                            {
                                await ReplyAsync("This pastebin doesn't exist.");
                                return;
                            }
                        }
                    }
                }
                Dictionary<char, int> ranking = new Dictionary<char, int>();
                List<char> alreadyCounted = new List<char>();
                string tmpWord = "";
                foreach (char c in allKanjis)
                {
                    if (c < 0x4E00 || c > 0x9FBF)
                    {
                        await ReplyAsync("I'm sorry but I can only search kanjis combinaison if all the characters you gave are kanjis and '" + c + "' isn't one.");
                        return;
                    }
                    if (!alreadyCounted.Contains(c))
                    {
                        ranking.Add(c, 0);
                        alreadyCounted.Add(c);
                        tmpWord += c;
                    }
                }
                allKanjis = tmpWord;
                if (allKanjis.Length == 1)
                {
                    await ReplyAsync("I'm sorry but I need at least 2 kanjis to look for combinaisons.");
                    return;
                }
                await ReplyAsync("Here are all the different combinaisons with these kanji:");
                bool hiragana = false;
                if (acceptHiragana == "all") hiragana = true;
                string finalStr = "";
                bool noAnswer = true;
                int nbWords = 0;
                foreach (char c in allKanjis)
                {
                    string json;
                    using (WebClient wc = new WebClient())
                    {
                        wc.Encoding = Encoding.UTF8;
                        json = wc.DownloadString("http://www.jisho.org/api/v1/search/words?keyword=" + c);
                    }
                    string[] url = BooruModule.getElementXml("\"japanese\":[", "", json, '$').Split(new string[] { "\"japanese\":[" }, StringSplitOptions.None);
                    foreach (string str in url)
                    {
                        string[] urlResult = str.Split(new string[] { "},{" }, StringSplitOptions.None);
                        string word;
                        word = BooruModule.getElementXml("\"word\":\"", "", urlResult[0], '"');
                        if (word.Length == 1) continue;
                        bool isCorrect = true;
                        string saveWord = word;
                        foreach (char ch in word)
                        {
                            if (!allKanjis.Contains(ch.ToString()) && (!hiragana || ch < 0x3041 || ch > 0x3096))
                            {
                                isCorrect = false;
                                break;
                            }
                            if (isKanjis == "hiragana")
                            {
                                word = BooruModule.getElementXml("\"reading\":\"", "", urlResult[0], '"');
                            }
                        }
                        if (!isCorrect) continue;
                        List<char> alreadyCount = new List<char>();
                        foreach (char ch in saveWord)
                        {
                            if (ch >= 0x4E00 && c <= 0x9FBF && !alreadyCount.Contains(ch))
                            {
                                ranking[ch]++;
                                alreadyCount.Add(ch);
                            }
                        }
                        noAnswer = false;
                        string[] meanings = BooruModule.getElementXml("english_definitions\":[", "", str, ']').Split(new string[] { "\",\"" }, StringSplitOptions.None);
                        string allMeanings = "";
                        if (definition == "definition")
                        {
                            foreach (string sm in meanings)
                            {
                                allMeanings += sm + " / ";
                            }
                        }
                        if (allMeanings != " / " && word != "")
                        {
                            nbWords++;
                            if (definition == "definition")
                                finalStr += word + ": " + allMeanings.Substring(1, allMeanings.Length - 5) + Environment.NewLine;
                            else
                                finalStr += word + Environment.NewLine;
                            if (finalStr.Length > 1750)
                            {
                                await ReplyAsync(finalStr);
                                finalStr = "";
                            }
                        }
                    }
                }
                if (noAnswer)
                    await ReplyAsync("I didn't find any combinaison with these kanjis.");
                else
                {
                    if (displayRanking == "displayRanking")
                    {
                        finalStr += "There was " + nbWords + " combinaison" + ((nbWords > 1) ? ("s") : ("")) + ", here's how many time each kanji occured:";
                        await ReplyAsync(finalStr);
                        finalStr = "";
                        char bestChar;
                        int valueBestChar;
                        while (ranking.Count > 0)
                        {
                            bestChar = '\0';
                            valueBestChar = 0;
                            foreach (var k in ranking)
                            {
                                if (valueBestChar == 0 || k.Value > valueBestChar)
                                {
                                    valueBestChar = k.Value;
                                    bestChar = k.Key;
                                }
                            }
                            finalStr += bestChar + ": " + valueBestChar + Environment.NewLine;
                            if (finalStr.Length > 1750)
                            {
                                await ReplyAsync(finalStr);
                                finalStr = "";
                            }
                            ranking.Remove(bestChar);
                        }
                        await ReplyAsync(finalStr);
                    }
                    else
                    {
                        finalStr += "Done! There was " + nbWords + " combinaison" + ((nbWords > 1) ? ("s") : ("")) + ".";
                        await ReplyAsync(finalStr);
                    }
                }
            }
        }

        private List<string> getAllKanjis(string word, bool all)
        {
            string newWord = word.Replace(" ", "%20");
            string json;
            using (WebClient wc = new WebClient())
            {
                wc.Encoding = Encoding.UTF8;
                json = wc.DownloadString("http://www.jisho.org/api/v1/search/words?keyword=" + newWord);
            }
            string[] url = BooruModule.getElementXml("\"japanese\":[", "", json, '$').Split(new string[] { "\"japanese\":[" }, StringSplitOptions.None);
            string finalStr = "Here are the japanese translations for " + word + ":" + Environment.NewLine + Environment.NewLine;
            if (url[0] == "")
                return new List<string>() { "I didn't find any japanase translation for " + word + "." };
            else
            {
                List<string> finalList = new List<string>();
                foreach (string str in url)
                {
                    string[] urlResult = str.Split(new string[] { "},{" }, StringSplitOptions.None);
                    string[] meanings = BooruModule.getElementXml("english_definitions\":[", "", str, ']').Split(new string[] { "\",\"" }, StringSplitOptions.None);
                    foreach (string s in urlResult)
                    {
                        if (BooruModule.getElementXml("\"reading\":\"", "", s, '"') == "" && BooruModule.getElementXml("\"word\":\"", "", s, '"') == "")
                            continue;
                        finalStr += ((BooruModule.getElementXml("\"word\":\"", "", s, '"') == "") ? (BooruModule.getElementXml("\"reading\":\"", "", s, '"') + ".")
                        : (BooruModule.getElementXml("\"word\":\"", "", s, '"')
                        + ((BooruModule.getElementXml("\"reading\":\"", "", s, '"') == "") ? ("") : (" (" + BooruModule.getElementXml("\"reading\":\"", "", s, '"') + " - " + toRomaji(BooruModule.getElementXml("\"reading\":\"", "", s, '"') + ")"))))) + Environment.NewLine;
                    }
                    finalStr += "Meaning: ";
                    string allMeanings = "";
                    foreach (string sm in meanings)
                    {
                        allMeanings += sm + " / ";
                    }
                    finalStr += allMeanings.Substring(1, allMeanings.Length - 5);
                    if (!all)
                        break;
                    else
                    {
                        if (finalStr.Length > 1250)
                        {
                            finalList.Add(finalStr);
                        }
                        else
                            finalStr += Environment.NewLine + Environment.NewLine;
                    }
                }
                finalList.Add(finalStr);
                return (finalList);
            }
        }

        public static string toRomaji(string name)
        {
            string finalName = "";
            string finalStr = "";
            int doubleVoy = 0;
            for (int i = 0; i < name.Length; i++)
            {
                finalName = "";
                doubleVoy--;
                char curr = name[i];
                char next = ((i < name.Length - 1) ? (name[i + 1]) : (' '));
                char nnext = ((i < name.Length - 2) ? (name[i + 2]) : (' '));
                if (curr == 'っ')
                {
                    doubleVoy = 2;
                }
                else if (curr == 'あ') finalName += 'a';
                else if (curr == 'い') finalName += 'i';
                else if (curr == 'う') finalName += 'u';
                else if (curr == 'え') finalName += 'e';
                else if (curr == 'お') finalName += 'o';
                else if (curr == 'か') finalName += "ka";
                else if (curr == 'き' && next == 'ゃ') { finalName += "kya"; i++; }
                else if (curr == 'き' && next == 'ぃ') { finalName += "kyi"; i++; }
                else if (curr == 'き' && next == 'ゅ') { finalName += "kyu"; i++; }
                else if (curr == 'き' && next == 'ぇ') { finalName += "kye"; i++; }
                else if (curr == 'き' && next == 'ょ') { finalName += "kyo"; i++; }
                else if (curr == 'き') finalName += "ki";
                else if (curr == 'く') finalName += "ku";
                else if (curr == 'け') finalName += "ke";
                else if (curr == 'こ') finalName += "ko";
                else if (curr == 'が') finalName += "ga";
                else if (curr == 'ぎ' && next == 'ゃ') { finalName += "gya"; i++; }
                else if (curr == 'ぎ' && next == 'ぃ') { finalName += "gyi"; i++; }
                else if (curr == 'ぎ' && next == 'ゅ') { finalName += "gyu"; i++; }
                else if (curr == 'ぎ' && next == 'ぇ') { finalName += "gye"; i++; }
                else if (curr == 'ぎ' && next == 'ょ') { finalName += "gyo"; i++; }
                else if (curr == 'ぎ') finalName += "gi";
                else if (curr == 'ぐ') finalName += "gu";
                else if (curr == 'げ') finalName += "ge";
                else if (curr == 'ご') finalName += "go";
                else if (curr == 'さ') finalName += "sa";
                else if (curr == 'し' && next == 'ゃ') { finalName += "sha"; i++; }
                else if (curr == 'し' && next == 'ゅ') { finalName += "shu"; i++; }
                else if (curr == 'し' && next == 'ぇ') { finalName += "she"; i++; }
                else if (curr == 'し' && next == 'ょ') { finalName += "sho"; i++; }
                else if (curr == 'し') finalName += "shi";
                else if (curr == 'す') finalName += "su";
                else if (curr == 'せ') finalName += "se";
                else if (curr == 'そ') finalName += "so";
                else if (curr == 'ざ') finalName += "za";
                else if (curr == 'じ' && next == 'ゃ') { finalName += "dja"; i++; }
                else if (curr == 'じ' && next == 'ぃ') { finalName += "dji"; i++; }
                else if (curr == 'じ' && next == 'ゅ') { finalName += "dju"; i++; }
                else if (curr == 'じ' && next == 'ぇ') { finalName += "dje"; i++; }
                else if (curr == 'じ' && next == 'ょ') { finalName += "djo"; i++; }
                else if (curr == 'じ') finalName += "dji";
                else if (curr == 'ず') finalName += "zu";
                else if (curr == 'ぜ') finalName += "ze";
                else if (curr == 'ぞ') finalName += "zo";
                else if (curr == 'た') finalName += "ta";
                else if (curr == 'ち' && next == 'ゃ') { finalName += "cha"; i++; }
                else if (curr == 'ち' && next == 'ぃ') { finalName += "chi"; i++; }
                else if (curr == 'ち' && next == 'ゅ') { finalName += "chu"; i++; }
                else if (curr == 'ち' && next == 'ぇ') { finalName += "che"; i++; }
                else if (curr == 'ち' && next == 'ょ') { finalName += "cho"; i++; }
                else if (curr == 'ち') finalName += "chi";
                else if (curr == 'つ') finalName += "tsu";
                else if (curr == 'て') finalName += "te";
                else if (curr == 'と') finalName += "to";
                else if (curr == 'だ') finalName += "da";
                else if (curr == 'ぢ' && next == 'ゃ') { finalName += "dja"; i++; }
                else if (curr == 'ぢ' && next == 'ぃ') { finalName += "dji"; i++; }
                else if (curr == 'ぢ' && next == 'ゅ') { finalName += "dju"; i++; }
                else if (curr == 'ぢ' && next == 'ぇ') { finalName += "dje"; i++; }
                else if (curr == 'ぢ' && next == 'ょ') { finalName += "djo"; i++; }
                else if (curr == 'ぢ') finalName += "dji";
                else if (next == 'づ') finalName += "dzu";
                else if (curr == 'で') finalName += "de";
                else if (curr == 'ど') finalName += "do";
                else if (curr == 'な') finalName += "na";
                else if (curr == 'に' && next == 'ゃ') { finalName += "nya"; i++; }
                else if (curr == 'に' && next == 'ぃ') { finalName += "nyi"; i++; }
                else if (curr == 'に' && next == 'ゅ') { finalName += "nyu"; i++; }
                else if (curr == 'に' && next == 'ぇ') { finalName += "nye"; i++; }
                else if (curr == 'に' && next == 'ょ') { finalName += "nyo"; i++; }
                else if (curr == 'に') finalName += "ni";
                else if (curr == 'ぬ') finalName += "nu";
                else if (curr == 'ね') finalName += "ne";
                else if (curr == 'の') finalName += "no";
                else if (curr == 'は') finalName += "ha";
                else if (curr == 'ひ' && next == 'ゃ') { finalName += "hya"; i++; }
                else if (curr == 'ひ' && next == 'ぃ') { finalName += "hyi"; i++; }
                else if (curr == 'ひ' && next == 'ゅ') { finalName += "hyu"; i++; }
                else if (curr == 'ひ' && next == 'ぇ') { finalName += "hye"; i++; }
                else if (curr == 'ひ' && next == 'ょ') { finalName += "hyo"; i++; }
                else if (curr == 'ひ') finalName += "hi";
                else if (curr == 'ふ') finalName += "fu";
                else if (curr == 'へ') finalName += "he";
                else if (curr == 'ほ') finalName += "ho";
                else if (curr == 'ば') finalName += "ba";
                else if (curr == 'び' && next == 'ゃ') { finalName += "bya"; i++; }
                else if (curr == 'び' && next == 'ぃ') { finalName += "byi"; i++; }
                else if (curr == 'び' && next == 'ゅ') { finalName += "byu"; i++; }
                else if (curr == 'び' && next == 'ぇ') { finalName += "bye"; i++; }
                else if (curr == 'び' && next == 'ょ') { finalName += "byo"; i++; }
                else if (curr == 'び') finalName += "bi";
                else if (curr == 'ぶ') finalName += "bu";
                else if (curr == 'べ') finalName += "be";
                else if (curr == 'ぼ') finalName += "bo";
                else if (curr == 'ぱ') finalName += "pa";
                else if (curr == 'ぴ' && next == 'ゃ') { finalName += "pya"; i++; }
                else if (curr == 'ぴ' && next == 'ぃ') { finalName += "pyi"; i++; }
                else if (curr == 'ぴ' && next == 'ゅ') { finalName += "pyu"; i++; }
                else if (curr == 'ぴ' && next == 'ぇ') { finalName += "pye"; i++; }
                else if (curr == 'ぴ' && next == 'ょ') { finalName += "pyo"; i++; }
                else if (curr == 'ぴ') finalName += "pi";
                else if (curr == 'ぷ') finalName += "pu";
                else if (curr == 'ぺ') finalName += "pe";
                else if (curr == 'ぽ') finalName += "po";
                else if (curr == 'ま') finalName += "ma";
                else if (curr == 'み' && next == 'ゃ') { finalName += "mya"; i++; }
                else if (curr == 'み' && next == 'ぃ') { finalName += "myi"; i++; }
                else if (curr == 'み' && next == 'ゅ') { finalName += "myu"; i++; }
                else if (curr == 'み' && next == 'ぇ') { finalName += "mye"; i++; }
                else if (curr == 'み' && next == 'ょ') { finalName += "myo"; i++; }
                else if (curr == 'み') finalName += "mi";
                else if (curr == 'む') finalName += "mu";
                else if (curr == 'め') finalName += "me";
                else if (curr == 'も') finalName += "mo";
                else if (curr == 'や') finalName += "ya";
                else if (curr == 'ゆ') finalName += "yu";
                else if (curr == 'よ') finalName += "yo";
                else if (curr == 'ら') finalName += "ra";
                else if (curr == 'り' && next == 'ゃ') { finalName += "rya"; i++; }
                else if (curr == 'り' && next == 'ぃ') { finalName += "ryi"; i++; }
                else if (curr == 'り' && next == 'ゅ') { finalName += "ryu"; i++; }
                else if (curr == 'り' && next == 'ぇ') { finalName += "rye"; i++; }
                else if (curr == 'り' && next == 'ょ') { finalName += "ryo"; i++; }
                else if (curr == 'り') finalName += "ri";
                else if (curr == 'る') finalName += "ru";
                else if (curr == 'れ') finalName += "re";
                else if (curr == 'ろ') finalName += "ro";
                else if (curr == 'わ') finalName += "wa";
                else if (curr == 'ゐ') finalName += "wi";
                else if (curr == 'ゑ') finalName += "we";
                else if (curr == 'を') finalName += "wo";
                else if (curr == 'ゔ') finalName += "vu";
                else if (curr == 'ん') finalName += "n";
                else finalName += curr;
                if (doubleVoy == 1 && curr != 'ん' && curr != 'ゔ' && curr != 'ゃ' && curr != 'ぃ' && curr != 'ゅ' && curr != 'ぇ' && curr != 'ょ'
                     && curr != 'っ' && curr != 'あ' && curr != 'い' && curr != 'う' && curr != 'え' && curr != 'お')
                {
                    finalName = finalName[0] + finalName;
                }
                finalStr += finalName;
                finalName = "";
            }
            finalStr += finalName;
            return (finalStr);
        }

        public static string toHiragana(string name)
        {
            string finalName = "";
            name = name.ToLower();
            for (int i = 0; i < name.Length; i += 2)
            {
                char curr = name[i];
                char next = ((i < name.Length - 1) ? (name[i + 1]) : (' '));
                char nnext = ((i < name.Length - 2) ? (name[i + 2]) : (' '));
                if (curr != 'a' && curr != 'i' && curr != 'u' && curr != 'e' && curr != 'o' && curr != 'n'
                    && curr == next)
                { finalName += "っ"; i--; continue; }
                if (curr == 'a') { finalName += 'あ'; i--; }
                else if (curr == 'i') { finalName += 'い'; i--; }
                else if (curr == 'u') { finalName += 'う'; i--; }
                else if (curr == 'e') { finalName += 'え'; i--; }
                else if (curr == 'o') { finalName += 'お'; i--; }
                else if (curr == 'k' && next == 'a') finalName += "か";
                else if (curr == 'k' && next == 'y' && nnext == 'a') { finalName += "きゃ"; i++; }
                else if (curr == 'k' && next == 'y' && nnext == 'i') { finalName += "きぃ"; i++; }
                else if (curr == 'k' && next == 'y' && nnext == 'u') { finalName += "きゅ"; i++; }
                else if (curr == 'k' && next == 'y' && nnext == 'e') { finalName += "きぇ"; i++; }
                else if (curr == 'k' && next == 'y' && nnext == 'o') { finalName += "きょ"; i++; }
                else if (curr == 'k' && next == 'i') finalName += "き";
                else if (curr == 'k' && next == 'u') finalName += "く";
                else if (curr == 'k' && next == 'e') finalName += "け";
                else if (curr == 'k' && next == 'o') finalName += "こ";
                else if (curr == 'g' && next == 'a') finalName += "が";
                else if (curr == 'g' && next == 'y' && nnext == 'a') { finalName += "ぎゃ"; i++; }
                else if (curr == 'g' && next == 'y' && nnext == 'i') { finalName += "ぎぃ"; i++; }
                else if (curr == 'g' && next == 'y' && nnext == 'u') { finalName += "ぎゅ"; i++; }
                else if (curr == 'g' && next == 'y' && nnext == 'e') { finalName += "ぎぇ"; i++; }
                else if (curr == 'g' && next == 'y' && nnext == 'o') { finalName += "ぎょ"; i++; }
                else if (curr == 'g' && next == 'i') finalName += "ぎ";
                else if (curr == 'g' && next == 'u') finalName += "ぐ";
                else if (curr == 'g' && next == 'e') finalName += "げ";
                else if (curr == 'g' && next == 'o') finalName += "ご";
                else if (curr == 's' && next == 'a') finalName += "さ";
                else if (curr == 's' && next == 'h' && nnext == 'i') { finalName += "し"; i++; }
                else if (curr == 's' && next == 'h' && nnext == 'a') { finalName += "しゃ"; i++; }
                else if (curr == 's' && next == 'h' && nnext == 'u') { finalName += "しゅ"; i++; }
                else if (curr == 's' && next == 'h' && nnext == 'e') { finalName += "しぇ"; i++; }
                else if (curr == 's' && next == 'h' && nnext == 'o') { finalName += "しょ"; i++; }
                else if (curr == 's' && next == 'u') finalName += "す";
                else if (curr == 's' && next == 'e') finalName += "せ";
                else if (curr == 's' && next == 'o') finalName += "そ";
                else if (curr == 'z' && next == 'a') finalName += "ざ";
                else if (curr == 'j' && next == 'i') finalName += "じ";
                else if (curr == 'j' && next == 'y' && nnext == 'a') { finalName += "じゃ"; i++; }
                else if (curr == 'j' && next == 'y' && nnext == 'i') { finalName += "じぃ"; i++; }
                else if (curr == 'j' && next == 'y' && nnext == 'u') { finalName += "じゅ"; i++; }
                else if (curr == 'j' && next == 'y' && nnext == 'e') { finalName += "じぇ"; i++; }
                else if (curr == 'j' && next == 'y' && nnext == 'o') { finalName += "じょ"; i++; }
                else if (curr == 'j' && next == 'a') finalName += "じゃ";
                else if (curr == 'j' && next == 'u') finalName += "じゅ";
                else if (curr == 'j' && next == 'e') finalName += "じぇ";
                else if (curr == 'j' && next == 'o') finalName += "じょ";
                else if (curr == 'z' && next == 'u') finalName += "ず";
                else if (curr == 'z' && next == 'e') finalName += "ぜ";
                else if (curr == 'z' && next == 'o') finalName += "ぞ";
                else if (curr == 't' && next == 'a') finalName += "た";
                else if (curr == 't' && next == 'y' && nnext == 'a') { finalName += "ちゃ"; i++; }
                else if (curr == 't' && next == 'y' && nnext == 'i') { finalName += "ちぃ"; i++; }
                else if (curr == 't' && next == 'y' && nnext == 'u') { finalName += "ちゅ"; i++; }
                else if (curr == 't' && next == 'y' && nnext == 'e') { finalName += "ちぇ"; i++; }
                else if (curr == 't' && next == 'y' && nnext == 'o') { finalName += "ちょ"; i++; }
                else if (curr == 'c' && next == 'h' && nnext == 'i') { finalName += "ち"; i++; }
                else if (curr == 'c' && next == 'h' && nnext == 'a') { finalName += "ちゃ"; i++; }
                else if (curr == 'c' && next == 'h' && nnext == 'u') { finalName += "ちゅ"; i++; }
                else if (curr == 'c' && next == 'h' && nnext == 'e') { finalName += "ちぇ"; i++; }
                else if (curr == 'c' && next == 'h' && nnext == 'o') { finalName += "ちょ"; i++; }
                else if (curr == 't' && next == 's' && nnext == 'u') { finalName += "つ"; i++; }
                else if (curr == 't' && next == 's' && nnext == 'u') { finalName += "つ"; i++; }
                else if (curr == 't' && next == 'e') finalName += "て";
                else if (curr == 't' && next == 'o') finalName += "と";
                else if (curr == 'd' && next == 'a') finalName += "だ";
                else if (curr == 'd' && next == 'y' && nnext == 'a') { finalName += "ぢゃ"; i++; }
                else if (curr == 'd' && next == 'y' && nnext == 'i') { finalName += "ぢぃ"; i++; }
                else if (curr == 'd' && next == 'y' && nnext == 'u') { finalName += "ぢゅ"; i++; }
                else if (curr == 'd' && next == 'y' && nnext == 'e') { finalName += "ぢぇ"; i++; }
                else if (curr == 'd' && next == 'y' && nnext == 'o') { finalName += "ぢょ"; i++; }
                else if (curr == 'd' && next == 'z' && next == 'u') { finalName += "づ"; i++; }
                else if (next == 'z' && next == 'u') finalName += "づ";
                else if (curr == 'd' && next == 'e') finalName += "で";
                else if (curr == 'd' && next == 'o') finalName += "ど";
                else if (curr == 'n' && next == 'a') finalName += "な";
                else if (curr == 'n' && next == 'y' && nnext == 'a') { finalName += "にゃ"; i++; }
                else if (curr == 'n' && next == 'y' && nnext == 'i') { finalName += "にぃ"; i++; }
                else if (curr == 'n' && next == 'y' && nnext == 'u') { finalName += "にゅ"; i++; }
                else if (curr == 'n' && next == 'y' && nnext == 'e') { finalName += "にぇ"; i++; }
                else if (curr == 'n' && next == 'y' && nnext == 'o') { finalName += "にょ"; i++; }
                else if (curr == 'n' && next == 'i') finalName += "に";
                else if (curr == 'n' && next == 'u') finalName += "ぬ";
                else if (curr == 'n' && next == 'e') finalName += "ね";
                else if (curr == 'n' && next == 'o') finalName += "の";
                else if (curr == 'h' && next == 'a') finalName += "は";
                else if (curr == 'h' && next == 'y' && nnext == 'a') { finalName += "ひゃ"; i++; }
                else if (curr == 'h' && next == 'y' && nnext == 'i') { finalName += "ひぃ"; i++; }
                else if (curr == 'h' && next == 'y' && nnext == 'u') { finalName += "ひゅ"; i++; }
                else if (curr == 'h' && next == 'y' && nnext == 'e') { finalName += "ひぇ"; i++; }
                else if (curr == 'h' && next == 'y' && nnext == 'o') { finalName += "ひょ"; i++; }
                else if (curr == 'h' && next == 'i') finalName += "ひ";
                else if (curr == 'f' && next == 'u') finalName += "ふ";
                else if (curr == 'h' && next == 'e') finalName += "へ";
                else if (curr == 'h' && next == 'o') finalName += "ほ";
                else if (curr == 'b' && next == 'a') finalName += "ば";
                else if (curr == 'b' && next == 'y' && nnext == 'a') { finalName += "びゃ"; i++; }
                else if (curr == 'b' && next == 'y' && nnext == 'i') { finalName += "びぃ"; i++; }
                else if (curr == 'b' && next == 'y' && nnext == 'u') { finalName += "びゅ"; i++; }
                else if (curr == 'b' && next == 'y' && nnext == 'e') { finalName += "びぇ"; i++; }
                else if (curr == 'b' && next == 'y' && nnext == 'o') { finalName += "びょ"; i++; }
                else if (curr == 'b' && next == 'i') finalName += "び";
                else if (curr == 'b' && next == 'u') finalName += "ぶ";
                else if (curr == 'b' && next == 'e') finalName += "べ";
                else if (curr == 'b' && next == 'o') finalName += "ぼ";
                else if (curr == 'p' && next == 'a') finalName += "ぱ";
                else if (curr == 'p' && next == 'y' && nnext == 'a') { finalName += "ぴゃ"; i++; }
                else if (curr == 'p' && next == 'y' && nnext == 'i') { finalName += "ぴぃ"; i++; }
                else if (curr == 'p' && next == 'y' && nnext == 'u') { finalName += "ぴゅ"; i++; }
                else if (curr == 'p' && next == 'y' && nnext == 'e') { finalName += "ぴぇ"; i++; }
                else if (curr == 'p' && next == 'y' && nnext == 'o') { finalName += "ぴょ"; i++; }
                else if (curr == 'p' && next == 'i') finalName += "ぴ";
                else if (curr == 'p' && next == 'u') finalName += "ぷ";
                else if (curr == 'p' && next == 'e') finalName += "ぺ";
                else if (curr == 'p' && next == 'o') finalName += "ぽ";
                else if (curr == 'm' && next == 'a') finalName += "ま";
                else if (curr == 'm' && next == 'y' && nnext == 'a') { finalName += "みゃ"; i++; }
                else if (curr == 'm' && next == 'y' && nnext == 'i') { finalName += "みぃ"; i++; }
                else if (curr == 'm' && next == 'y' && nnext == 'u') { finalName += "みゅ"; i++; }
                else if (curr == 'm' && next == 'y' && nnext == 'e') { finalName += "みぇ"; i++; }
                else if (curr == 'm' && next == 'y' && nnext == 'o') { finalName += "みょ"; i++; }
                else if (curr == 'm' && next == 'i') finalName += "み";
                else if (curr == 'm' && next == 'u') finalName += "む";
                else if (curr == 'm' && next == 'e') finalName += "め";
                else if (curr == 'm' && next == 'o') finalName += "も";
                else if (curr == 'y' && next == 'a') finalName += "や";
                else if (curr == 'y' && next == 'u') finalName += "ゆ";
                else if (curr == 'y' && next == 'o') finalName += "よ";
                else if (curr == 'r' && next == 'a') finalName += "ら";
                else if (curr == 'r' && next == 'y' && nnext == 'a') { finalName += "りゃ"; i++; }
                else if (curr == 'r' && next == 'y' && nnext == 'i') { finalName += "りぃ"; i++; }
                else if (curr == 'r' && next == 'y' && nnext == 'u') { finalName += "りゅ"; i++; }
                else if (curr == 'r' && next == 'y' && nnext == 'e') { finalName += "りぇ"; i++; }
                else if (curr == 'r' && next == 'y' && nnext == 'o') { finalName += "りょ"; i++; }
                else if (curr == 'r' && next == 'i') finalName += "り";
                else if (curr == 'r' && next == 'u') finalName += "る";
                else if (curr == 'r' && next == 'e') finalName += "れ";
                else if (curr == 'r' && next == 'o') finalName += "ろ";
                else if (curr == 'w' && next == 'a') finalName += "わ";
                else if (curr == 'w' && next == 'i') finalName += "ゐ";
                else if (curr == 'w' && next == 'e') finalName += "ゑ";
                else if (curr == 'w' && next == 'o') finalName += "を";
                else if (curr == 'v' && next == 'u') finalName += "ゔ";
                else if (curr == 'n') { finalName += "ん"; i--; }
                else { finalName += curr; i--; }
            }
            return (finalName);
        }
    }
}