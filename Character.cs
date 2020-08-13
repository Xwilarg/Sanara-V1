using System;
using System.Collections.Generic;
using System.IO;

namespace Bot
{
    class Character
    {
        public Character()
        {
            resetHard();
            _timeThanks = DateTime.UtcNow;
            _currDoujinshi = 0;
            _maxDoujinshi = 0; // page max
            _currPage = 1;
            _doujinshiSendMethodDownload = true;
        }
        public Character(ulong name, string nameStr)
        {
            resetHard();
            _name = name;
            _nameStr = nameStr;
            _timeThanks = DateTime.UtcNow;
            _currDoujinshi = 0;
            _maxDoujinshi = 0; // page max
            _currPage = 1;
            _doujinshiSendMethodDownload = true;
        }

        public string returnInfosValuable()
        {
            if (getNbMessage() > 0)
                return (returnInformations());
            else
                return (null);
        }
        public string returnInformations()
        {
            return ("Name: " + _nameStr + " (Id: " + _name + ") ; variables stats: " + Convert.ToInt32(getNbMessage())
                + " ; " + Convert.ToInt32(getNbDiscution())
                + " ; " + Convert.ToInt32(getNbImage())
                + " ; " + Convert.ToInt32(getNbLewdImage())
                + " ; " + Convert.ToInt32(getNbRequest())
                + " ; " + Convert.ToInt32(getNbRequestThanks())
                + " ; " + Convert.ToInt32(getNbGuessKancolle())
                + " ; " + Convert.ToInt32(getNbGoodGuessKancolle())
                + " ; " + Convert.ToInt32(getShiritoriGames())
                + " ; " + Convert.ToInt32(getShiritoriGuess())
                + " ; " + Convert.ToInt32(getShiritoriGoodGuess())
                + " ; " + Convert.ToInt32(getShiritoriBestScore())
                + " ; " + Convert.ToInt32(getNbGuessBooru())
                + " ; " + Convert.ToInt32(getNbGoodGuessBooru())
                + " ; " + Convert.ToInt32(getNbGuessBooruAll())
                + " ; " + Convert.ToInt32(getNbGoodGuessBooruAll())
                + " || " + _firstMeet);
        }
        public string returnInformationsRaw(bool isNew)
        {
            if (isNew)
                return (_nameStr + Environment.NewLine + _name + Environment.NewLine + _firstMeet + Environment.NewLine + 0 + Environment.NewLine + 0
                    + Environment.NewLine + 0 + Environment.NewLine + 0 + Environment.NewLine + 0 + Environment.NewLine + 0
                     + Environment.NewLine + 0 + Environment.NewLine + 0 // Kancolle
                      + Environment.NewLine + 0 + Environment.NewLine + 0 + Environment.NewLine + 0 + Environment.NewLine + 0 // Shiritori
                       + Environment.NewLine + 0 + Environment.NewLine + 0 + Environment.NewLine + 0 + Environment.NewLine + 0); // Booru game
            else
            {
                return (_nameStr + Environment.NewLine + _name + Environment.NewLine + _firstMeet + Environment.NewLine + (Convert.ToInt32(getNbMessage()) + Convert.ToInt32(_nbMessage))
                        + Environment.NewLine + (Convert.ToInt32(getNbDiscution()) + _nbDiscution)
                        + Environment.NewLine + (Convert.ToInt32(getNbImage()) + _nbImage)
                        + Environment.NewLine + (Convert.ToInt32(getNbLewdImage()) + _nbLewdImage)
                        + Environment.NewLine + 0 + Environment.NewLine + 0
                        + Environment.NewLine + (Convert.ToInt32(getNbGuessKancolle()) + _nbGuessKancolle)
                        + Environment.NewLine + (Convert.ToInt32(getNbGoodGuessKancolle()) + _nbGoodGuessKancolle)
                        + Environment.NewLine + (Convert.ToInt32(getShiritoriGames()) + _nbGamesShiritori)
                        + Environment.NewLine + (Convert.ToInt32(getShiritoriGuess()) + _attemptShiritori)
                        + Environment.NewLine + (Convert.ToInt32(getShiritoriGoodGuess()) + _goodAttemptShiritori)
                        + Environment.NewLine + getShiritoriBestScore()
                        + Environment.NewLine + (Convert.ToInt32(getNbGuessBooru()) + _nbGuessBooru)
                        + Environment.NewLine + (Convert.ToInt32(getNbGoodGuessBooru()) + _nbGoodGuessBooru)
                        + Environment.NewLine + (Convert.ToInt32(getNbGuessBooruAll()) + _nbGuessBooruAll)
                        + Environment.NewLine + (Convert.ToInt32(getNbGoodGuessBooruAll()) + _nbGoodGuessBooruAll));
            }
        }
        public void saveAndParseInfos(string[] infos)
        {
            try
            {
                _nameStr = infos[0];
                _name = Convert.ToUInt64(infos[1]);
                _firstMeet = infos[2];
                /*_nbMessage = Convert.ToInt32(infos[3]);
                _nbDiscution = Convert.ToInt32(infos[4]);
                _nbImage = Convert.ToInt32(infos[5]);
                _nbLewdImage = Convert.ToInt32(infos[6]);*/
            }
            catch (IndexOutOfRangeException)
            { }
        }
        public void meet()
        {
            if (_firstMeet == "No")
                _firstMeet = DateTime.UtcNow.ToString("ddMMyyHHmmss");
        }
        public void resetHard()
        {
            reset();
            _firstMeet = "No";
            _isRequestingImage = false;
        }
        public void reset()
        {
            _nbMessage = 0;
            _nbDiscution = 0;
            _nbImage = 0;
            _nbLewdImage = 0;
            _request = 0;
            _requestThanks = 0;
            _needThanks = false;
            _nbGuessKancolle = 0;
            _nbGoodGuessKancolle = 0;
            _bestShiritoriScore = 0;
            _attemptShiritori = 0;
            _goodAttemptShiritori = 0;
            _nbGamesShiritori = 0;
            _nbGuessBooru = 0;
            _nbGoodGuessBooru = 0;
            _nbGuessBooruAll = 0;
            _nbGoodGuessBooruAll = 0;
        }
        public ulong getName()
        {
            return (_name);
        }
        public void increaseNbDiscution()
        {
            _nbDiscution++;
        }
        public void increaseNbImage()
        {
            _nbImage++;
        }
        public void increaseNbLewdImage()
        {
            _nbLewdImage++;
        }
        public void increaseNbMessage()
        {
            _nbMessage++;
        }
        public void increaseNbRequest()
        {
            _request++;
        }
        public void increaseNbRequestThanks()
        {
            _requestThanks++;
        }
        public void increaseGuessKancolle(bool isGoodGuess)
        {
            _nbGuessKancolle++;
            if (isGoodGuess)
                _nbGoodGuessKancolle++;
        }
        public void increaseGuessBooru(bool isGoodGuess)
        {
            _nbGuessBooru++;
            if (isGoodGuess)
                _nbGoodGuessBooru++;
        }
        public void increaseGuessBooruAll(bool isGoodGuess)
        {
            _nbGuessBooruAll++;
            if (isGoodGuess)
                _nbGoodGuessBooruAll++;
        }
        public bool setShiritoriScore(int score)
        {
            if (score > _bestShiritoriScore)
            {
                _bestShiritoriScore = score;
                return (true);
            }
            else
                return (false);
        }
        public void increaseShiritoriAttempt(bool isGoodGuess)
        {
            _attemptShiritori++;
            if (isGoodGuess)
                _goodAttemptShiritori++;
        }
        public void increaseShiritoriGames()
        {
            _nbGamesShiritori++;
        }
        public string getFirstMeet()
        {
            return (File.ReadAllLines("Saves/Users/" + _name + ".dat")[2]);
        }
        public int getNbMessage()
        {
            return (Convert.ToInt32(File.ReadAllLines("Saves/Users/" + _name + ".dat")[3]));
        }
        public float getRatioLewdImages()
        {
            if (_nbImage == 0)
                return (0);
            else
                return (getNbLewdImage() / getNbImage());
        }
        public int getNbImage()
        {
            return (Convert.ToInt32(File.ReadAllLines("Saves/Users/" + _name + ".dat")[5]));
        }
        private int getNbLewdImage()
        {
            return (Convert.ToInt32(File.ReadAllLines("Saves/Users/" + _name + ".dat")[6]));
        }
        public int getNbDiscution()
        {
            return (Convert.ToInt32(File.ReadAllLines("Saves/Users/" + _name + ".dat")[4]));
        }
        public int getNbRequest()
        {
            return (Convert.ToInt32(File.ReadAllLines("Saves/Users/" + _name + ".dat")[7]));
        }
        public int getNbRequestThanks()
        {
            return (Convert.ToInt32(File.ReadAllLines("Saves/Users/" + _name + ".dat")[8]));
        }
        public int getNbGuessKancolle()
        {
            try
            {
                return (Convert.ToInt32(File.ReadAllLines("Saves/Users/" + _name + ".dat")[9]));
            }
            catch (IndexOutOfRangeException)
            {
                return (0);
            }
        }
        public int getNbGoodGuessKancolle()
        {
            try
            {
                return (Convert.ToInt32(File.ReadAllLines("Saves/Users/" + _name + ".dat")[10]));
            }
            catch (IndexOutOfRangeException)
            {
                return (0);
            }
        }
        public int getShiritoriGames() { try { return (Convert.ToInt32(File.ReadAllLines("Saves/Users/" + _name + ".dat")[11])); } catch (IndexOutOfRangeException) { return (0); } }
        public int getShiritoriGuess() { try { return (Convert.ToInt32(File.ReadAllLines("Saves/Users/" + _name + ".dat")[12])); } catch (IndexOutOfRangeException) { return (0); } }
        public int getShiritoriGoodGuess() { try { return (Convert.ToInt32(File.ReadAllLines("Saves/Users/" + _name + ".dat")[13])); } catch (IndexOutOfRangeException) { return (0); } }
        public int getShiritoriBestScore()
        {
            try
            {
                int lastScore = Convert.ToInt32(File.ReadAllLines("Saves/Users/" + _name + ".dat")[14]);
                if (lastScore > _bestShiritoriScore)
                    return (lastScore);
                else
                    return (_bestShiritoriScore);
            }
            catch (IndexOutOfRangeException) { return (0); }
        }
        public int getNbGuessBooru()
        {
            try
            {
                return (Convert.ToInt32(File.ReadAllLines("Saves/Users/" + _name + ".dat")[15]));
            }
            catch (IndexOutOfRangeException)
            {
                return (0);
            }
        }
        public int getNbGoodGuessBooru()
        {
            try
            {
                return (Convert.ToInt32(File.ReadAllLines("Saves/Users/" + _name + ".dat")[16]));
            }
            catch (IndexOutOfRangeException)
            {
                return (0);
            }
        }
        public int getNbGuessBooruAll()
        {
            try
            {
                return (Convert.ToInt32(File.ReadAllLines("Saves/Users/" + _name + ".dat")[17]));
            }
            catch (IndexOutOfRangeException)
            {
                return (0);
            }
        }
        public int getNbGoodGuessBooruAll()
        {
            try
            {
                return (Convert.ToInt32(File.ReadAllLines("Saves/Users/" + _name + ".dat")[18]));
            }
            catch (IndexOutOfRangeException)
            {
                return (0);
            }
        }
        string _nameStr;
        ulong _name;
        int _nbDiscution;
        int _nbImage;
        int _nbLewdImage;
        int _nbMessage;
        int _nbGuessKancolle;
        int _nbGoodGuessKancolle;
        int _bestShiritoriScore;
        int _attemptShiritori;
        int _goodAttemptShiritori;
        int _nbGamesShiritori;
        int _nbGuessBooru;
        int _nbGoodGuessBooru;
        int _nbGuessBooruAll;
        int _nbGoodGuessBooruAll;
        private int _request;
        private int _requestThanks;
        string _firstMeet;
        public DateTime _timeThanks { set; get; }
        public bool _needThanks { set; get; }
        public bool _isRequestingImage { set; get; }
        public List<string> artists, general;
        public List<string> animeFrom;
        public List<string> characs;
        public char rating;
        public int _currDoujinshi { set; get; }
        public int _maxDoujinshi { set; get; }
        public int _currPage { set; get; }
        public bool _doujinshiSendMethodDownload { set; get; }
    }
}