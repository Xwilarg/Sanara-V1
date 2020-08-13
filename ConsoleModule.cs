using Discord;
using Discord.Commands;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Bot
{
    partial class Program
    {
        public enum errorCode
        {
            Success = 01,
            NotAuthorized = 10,
            NotFound = 11,
            OutputTooBig = 12,
            FormatError = 13,
            WrongNbOfArgs = 14,
            SudoRequired = 15,
            WrongFileExtension = 16,
            DirectoryNotFound = 20,
            FileNotFound = 21,
            UnknowError = 30
        }

        private string getCmdDescription(errorCode code)
        {
            switch (code) // 0x: ok, 1x: can't execute, 2x: error while executing
            {
                case errorCode.Success: return ("Command completed with success.");
                case errorCode.NotAuthorized: return ("You're not authorized to access to this ressource.");
                case errorCode.NotFound: return ("Command not found.");
                case errorCode.OutputTooBig: return ("The output too big to be display.");
                case errorCode.FormatError: return ("The command isn't correctly formated.");
                case errorCode.WrongNbOfArgs: return ("Wrong number of arguments for this command.");
                case errorCode.SudoRequired: return ("You must launch the console in sudo mode to do this.");
                case errorCode.WrongFileExtension: return ("This file don't have the correct file extension for this command.");
                case errorCode.DirectoryNotFound: return ("The directory specified was not found");
                case errorCode.FileNotFound: return ("The file was not found");
                case errorCode.UnknowError: return ("Unidentified error.");
                default: return ("Invalid error code.");
            }
        }

        public async void executeCommand(SocketCommandContext Context)
        {
            string msg = Context.Message.Content;
            string[] firstCommand = msg.Split(' ');
            msg = "";
            for (int i = 1; i < firstCommand.Length; i++)
                msg += firstCommand[i] + " ";
            msg = msg.Substring(0, msg.Length - 1);
            string[] allCommand = msg.Split(new string[] { "&&" }, StringSplitOptions.None);
            Server s = allServers.Find(x => x._id == Context.Guild.Id);
            var embed = new EmbedBuilder()
            {
                Color = Color.Green,
                Description = "",
                Footer = new EmbedFooterBuilder
                {
                    Text = "Code 01"
                }
            };
            try
            {
                foreach (string cmd in allCommand)
                {
                    if (cmd == "")
                    {
                        embed.Color = Color.Red;
                        embed.Description += getCmdDescription(errorCode.FormatError);
                        embed.Footer.Text = "Code 13";
                        break;
                    }
                    string[] command = cmd.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                    if (command[0] == "exit" || command[0] == "pwd" || command[0] == "ls" || command[0] == "cd" || command[0] == "cat" || command[0] == "compile" || command[0] == "launch")
                    {
                        if (s._idConsoleLaunch != Context.User.Id)
                        {
                            embed.Color = Color.Red;
                            embed.Description += getCmdDescription(errorCode.NotAuthorized);
                            embed.Footer.Text = "Code 10";
                            break;
                        }
                        else
                        {
                            if ((command[0] == "cd" && command.Length != 2)
                                || (command[0] == "exit" && command.Length != 1)
                                || (command[0] == "pwd" && command.Length != 1)
                                || (command[0] == "cat" && command.Length != 2)
                                || (command[0] == "compile" && command.Length != 2)
                                || (command[0] == "launch" && command.Length != 2)
                                || (command[0] == "ls" && command.Length > 2))
                            {
                                embed.Color = Color.Red;
                                embed.Description += getCmdDescription(errorCode.WrongNbOfArgs);
                                embed.Footer.Text = "Code " + (int)errorCode.WrongNbOfArgs;
                                break;
                            }
                            if (command[0] == "exit")
                            {
                                s._inConsoleMode = false;
                                embed.Description += "Console exited" + Environment.NewLine + Environment.NewLine;
                            }
                            else if (command[0] == "pwd")
                            {
                                embed.Description += "/" + s.pwd + Environment.NewLine + Environment.NewLine;
                            }
                            else if (command[0] == "ls" || command[0] == "cd" || command[0] == "cat" || command[0] == "compile" || command[0] == "launch")
                            {
                                bool isOk = true;
                                string[] requested = new string[0];
                                string toSearch = s.pwd;
                                if (command.Length > 1 && (command[1][0] == '/' || command[1][0] == '\\'))
                                {
                                    s.pwd = "";
                                    command[1] = command[1].Substring(1, command[1].Length - 1);
                                    if (command[1].Length > 0 && (command[1][0] == '/' || command[1][0] == '\\'))
                                    {
                                        embed.Color = Color.Red;
                                        embed.Description += getCmdDescription(errorCode.DirectoryNotFound);
                                        embed.Footer.Text = "Code 20";
                                        isOk = false;
                                    }
                                }
                                if (command.Length > 1 && command[1].Length > 1 && command[1][0] == '.' && command[1][1] == '.')
                                {
                                    command[1] = command[1].Substring(2, command[1].Length - 2);
                                    if (s.pwd != "")
                                    {
                                        string[] currPwd = s.pwd.Split('/');
                                        if (currPwd.Length > 1)
                                        {
                                            s.pwd = currPwd[0];
                                            for (int i = 1; i < currPwd.Length - 1; i++)
                                            {
                                                s.pwd += "/" + currPwd[i];
                                            }
                                        }
                                        else
                                            s.pwd = "";
                                    }
                                    else
                                    {
                                        embed.Color = Color.Red;
                                        embed.Description += getCmdDescription(errorCode.DirectoryNotFound);
                                        embed.Footer.Text = "Code 20";
                                        isOk = false;
                                    }
                                }
                                if (command.Length > 1)
                                {
                                    if (command[1] == ".") command[1] = "";
                                    command[1] = command[1].Replace("/./", "/");
                                    command[1] = command[1].Replace("\\.\\", "/");
                                    command[1] = command[1].Replace("./", "");
                                    command[1] = command[1].Replace(".\\", "");
                                    requested = (s.pwd + command[1]).Split(new string[] { "/", "\\" }, StringSplitOptions.RemoveEmptyEntries);
                                }
                                string tmp = toSearch;
                                toSearch = s.pwd;
                                if (command[0] != "cd")
                                    s.pwd = tmp;
                                if (requested.Length > 0 && isOk)
                                {
                                    if (toSearch != "") toSearch += "/";
                                    toSearch += command[1];
                                    if (requested.Contains("..") || (command[0] != "cat" && command[0] != "compile" && command[0] != "launch" && !Directory.Exists(toSearch)))
                                    {
                                        embed.Color = Color.Red;
                                        embed.Description += getCmdDescription(errorCode.DirectoryNotFound);
                                        embed.Footer.Text = "Code 20";
                                        isOk = false;
                                    }
                                    if ((command[0] == "cat" || command[0] == "compile" || command[0] == "launch") && !File.Exists(toSearch))
                                    {
                                        embed.Color = Color.Red;
                                        embed.Description += getCmdDescription(errorCode.FileNotFound);
                                        embed.Footer.Text = "Code 21";
                                        isOk = false;
                                    }
                                    else if (requested.Contains("Keys") || requested.Contains("Tasks"))
                                    {
                                        embed.Color = Color.Red;
                                        embed.Description += getCmdDescription(errorCode.NotAuthorized);
                                        embed.Footer.Text = "Code 10";
                                        isOk = false;
                                    }
                                }
                                if (isOk)
                                {
                                    if (command[0] == "ls")
                                    {
                                        string allFiles = "";
                                        int total = 0;
                                        if (toSearch == "")
                                        {
                                            total = 4;
                                            allFiles += "Archives/" + Environment.NewLine +
                                                        "ConsoleDebug/" + Environment.NewLine +
                                                        "Keys/" + Environment.NewLine +
                                                        "Logs/" + Environment.NewLine +
                                                        "Saves/" + Environment.NewLine;
                                        }
                                        else
                                        {
                                            foreach (string str in Directory.GetDirectories(toSearch))
                                            {
                                                string newStr = str.Replace('\\', '/');
                                                allFiles += newStr.Split('/')[newStr.Split('/').Length - 1] + "/" + Environment.NewLine;
                                                total++;
                                            }
                                            foreach (string str in Directory.GetFiles(toSearch))
                                            {
                                                string newStr = str.Replace('\\', '/');
                                                allFiles += newStr.Split('/')[newStr.Split('/').Length - 1] + Environment.NewLine;
                                                total++;
                                            }
                                        }
                                        embed.Description += "Total " + total + Environment.NewLine + allFiles + Environment.NewLine + Environment.NewLine;
                                    }
                                    else if (command[0] == "cd")
                                    {
                                        if (s.pwd == "")
                                            s.pwd += command[1];
                                        else
                                            s.pwd += "/" + command[1];
                                        embed.Description += "Done" + Environment.NewLine + Environment.NewLine;
                                    }
                                    else if (command[0] == "cat")
                                    {
                                        if (toSearch.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries).Length < 2)
                                        {
                                            embed.Color = Color.Red;
                                            embed.Description += getCmdDescription(errorCode.FileNotFound);
                                            embed.Footer.Text = "Code " + (int)errorCode.FileNotFound;
                                            break;
                                        }
                                        else if (s.allRight)
                                            embed.Description += File.ReadAllText(toSearch);
                                        else
                                        {
                                            embed.Color = Color.Red;
                                            embed.Description += getCmdDescription(errorCode.SudoRequired);
                                            embed.Footer.Text = "Code " + (int)errorCode.SudoRequired;
                                            break;
                                        }
                                    }
                                    else if (command[0] == "compile")
                                    {
                                        if (toSearch.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries).Length < 2)
                                        {
                                            embed.Color = Color.Red;
                                            embed.Description += getCmdDescription(errorCode.FileNotFound);
                                            embed.Footer.Text = "Code " + (int)errorCode.FileNotFound;
                                            break;
                                        }
                                        else if (toSearch.Split('.')[toSearch.Split('.').Length - 1] != "sy")
                                        {
                                            embed.Color = Color.Red;
                                            embed.Description += getCmdDescription(errorCode.WrongFileExtension);
                                            embed.Footer.Text = "Code " + (int)errorCode.WrongFileExtension;
                                            break;
                                        }
                                        else
                                        {
                                            int returnCode;
                                            embed.Description += CodeModule.compile(File.ReadAllLines(toSearch), out returnCode, toSearch) + Environment.NewLine;
                                            embed.Footer.Text = "Compilation of program returned code " + returnCode;
                                            if (returnCode != 0)
                                            {
                                                embed.Color = Color.Red;
                                                break;
                                            }
                                        }
                                    }
                                    else if (command[0] == "launch")
                                    {
                                        if (toSearch.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries).Length < 2)
                                        {
                                            embed.Color = Color.Red;
                                            embed.Description += getCmdDescription(errorCode.FileNotFound);
                                            embed.Footer.Text = "Code " + (int)errorCode.FileNotFound;
                                            break;
                                        }
                                        else if (toSearch.Split('.')[toSearch.Split('.').Length - 1] != "sye")
                                        {
                                            embed.Color = Color.Red;
                                            embed.Description += getCmdDescription(errorCode.WrongFileExtension);
                                            embed.Footer.Text = "Code " + (int)errorCode.WrongFileExtension;
                                            break;
                                        }
                                        else
                                        {
                                            int returnCode;
                                            embed.Description += CodeModule.launch(File.ReadAllLines(toSearch), out returnCode) + Environment.NewLine;
                                            embed.Footer.Text = "Compilation of program returned code " + returnCode;
                                            if (returnCode != 0)
                                            {
                                                embed.Color = Color.Red;
                                                break;
                                            }
                                        }
                                    }
                                }
                                else
                                    break;
                            }
                        }
                    }
                    else
                    {
                        embed.Color = Color.Red;
                        embed.Description += getCmdDescription(errorCode.NotFound);
                        embed.Footer.Text = "Code 11";
                        break;
                    }
                }
            }
            catch (ArgumentException)
            {
                embed.Color = Color.Red;
                embed.Description += getCmdDescription(errorCode.OutputTooBig);
                embed.Footer.Text = "Code 12";
            }
            catch (Exception ex)
            {
                embed.Color = Color.Red;
                embed.Description += ex.Message;
                embed.Footer.Text = "Code 30";
            }
            await Context.Channel.SendMessageAsync("", false, embed);
        }
    }


    public class ConsoleModule : ModuleBase
    {
        Program p = Program.p;

        [Command("Enter console")]
        public async Task startConsole()
        {
            p.doAction(Context.User, Context.Guild.Id, Program.Module.Console);
            Server s = p.allServers.Find(x => x._id == Context.Guild.Id);
            s._inConsoleMode = true;
            s._idConsoleLaunch = Context.User.Id;
            s.pwd = "";
            s.allRight = false;
            await ReplyAsync("Console launched.");
        }

        [Command("Enter console sudo")]
        public async Task startConsoleSudo()
        {
            p.doAction(Context.User, Context.Guild.Id, Program.Module.Console);
            if (Context.User.Id != 144851584478740481)
            {
                await ReplyAsync(Sentences.onlyMasterStr);
            }
            else
            {
                Server s = p.allServers.Find(x => x._id == Context.Guild.Id);
                s._inConsoleMode = true;
                s._idConsoleLaunch = Context.User.Id;
                s.pwd = "";
                s.allRight = true;
                await ReplyAsync("Console launched in sudo mode.");
            }
        }
    }
}