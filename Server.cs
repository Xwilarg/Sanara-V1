namespace Bot
{
    class Server
    {
        public Server(ulong id)
        {
            _id = id;
            _inConsoleMode = false;
            _idConsoleLaunch = 0;
            pwd = "";
            allRight = false;
        }
        public bool _inConsoleMode { set; get; }
        public ulong _id { private set; get; }
        public ulong _idConsoleLaunch { set; get; }
        public string pwd { set; get; }
        public bool allRight { set; get; }
    }
}