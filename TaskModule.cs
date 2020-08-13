using Discord.Commands;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Bot
{
    public class TaskModule : ModuleBase
    {
        Program p = Program.p;
        [Command("Create category"), Summary("Create a new category")]
        public async Task createCategory(string name)
        {
            p.doAction(Context.User, Context.Guild.Id, Program.Module.Task);
            if (name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                await ReplyAsync("I'm sorry but the name you entered is invalid.");
            }
            else if (Directory.Exists("Saves/Servers/" + Context.Guild.Id + "/Tasks/" + name))
            {
                await ReplyAsync("I'm sorry but there is already a category with the same name.");
            }
            else
            {
                Directory.CreateDirectory("Saves/Servers/" + Context.Guild.Id + "/Tasks/" + name);
                await ReplyAsync("I created this new category.");
            }
        }

        [Command("Delete category"), Summary("Delete a category")]
        public async Task deleteeCategory(string name)
        {
            p.doAction(Context.User, Context.Guild.Id, Program.Module.Task);
            if (name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                await ReplyAsync("I'm sorry but the name you entered is invalid.");
            }
            else if (!Directory.Exists("Saves/Servers/" + Context.Guild.Id + "/Tasks/" + name))
            {
                await ReplyAsync("I'm sorry but I didn't find any category with this name.");
            }
            else
            {
                foreach (string s in Directory.GetFiles("Saves/Servers/" + Context.Guild.Id + "/Tasks/" + name))
                {
                    File.Delete(s);
                }
                Directory.Delete("Saves/Servers/" + Context.Guild.Id + "/Tasks/" + name);
                await ReplyAsync("I deleted the category.");
            }
        }

        [Command("Create task"), Summary("Create a new task")]
        public async Task createTask(string category, string name, string description = "")
        {
            p.doAction(Context.User, Context.Guild.Id, Program.Module.Task);
            if (category.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                await ReplyAsync("I'm sorry but the name you entered is invalid.");
            }
            else if (!Directory.Exists("Saves/Servers/" + Context.Guild.Id + "/Tasks/" + category))
            {
                await ReplyAsync("I'm sorry but this category doesn't exist.");
            }
            else if (File.Exists("Saves/Servers/" + Context.Guild.Id + "/Tasks/" + category + "/" + name + ".txt"))
            {
                await ReplyAsync("I'm sorry but there is already a task with the same name in this category.");
            }
            else
            {
                File.WriteAllText("Saves/Servers/" + Context.Guild.Id + "/Tasks/" + category + "/" + name + ".txt", description);
                await ReplyAsync("I created this new task.");
            }
        }

        [Command("Delete task"), Summary("Delete a task")]
        public async Task deleteTask(string category, string name)
        {
            p.doAction(Context.User, Context.Guild.Id, Program.Module.Task);
            if (category.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                await ReplyAsync("The name you entered is invalid.");
            }
            else if (!Directory.Exists("Saves/Servers/" + Context.Guild.Id + "/Tasks/" + category))
            {
                await ReplyAsync("This category doesn't exist.");
            }
            else if (!File.Exists("Saves/Servers/" + Context.Guild.Id + "/Tasks/" + category + "/" + name + ".txt"))
            {
                await ReplyAsync("I'm sorry but there is no task with this name.");
            }
            else
            {
                File.Delete("Saves/Servers/" + Context.Guild.Id + "/Tasks/" + category + "/" + name + ".txt");
                await ReplyAsync("I deleted the task.");
            }
        }

        [Command("Display task"), Summary("Display all tasks")]
        public async Task displayTask(string category = "")
        {
            p.doAction(Context.User, Context.Guild.Id, Program.Module.Task);
            if (category != "" && category.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                await ReplyAsync("I'm sorry but the name you entered is invalid.");
            }
            else if (category != "" && !Directory.Exists("Saves/Servers/" + Context.Guild.Id + "/Tasks/" + category))
            {
                await ReplyAsync("I'm sorry but this category doesn't exist.");
            }
            else if (Directory.GetDirectories("Saves/Servers/" + Context.Guild.Id + "/Tasks/").Length == 0)
            {
                await ReplyAsync("I'm sorry but you didn't create any task yet.");
            }
            else
            {
                string msg;
                if (category == "")
                {
                    msg = "Here are all the tasks you made:" + Environment.NewLine + Environment.NewLine;
                    foreach (string s in Directory.GetDirectories("Saves/Servers/" + Context.Guild.Id + "/Tasks/"))
                    {
                        msg += displayAllTasks(s);
                        msg += Environment.NewLine;
                    }
                }
                else
                {
                    msg = "Here are all the tasks for the category you asked:" + Environment.NewLine + Environment.NewLine;
                    msg += displayAllTasks("Saves/Servers/" + Context.Guild.Id + "/Tasks/" + category);
                }
                await ReplyAsync(msg);
            }
        }

        private string displayAllTasks(string source)
        {
            source = source.Replace('\\', '/');
            string msg = source.Split('/')[source.Split('/').Length - 1] + ":" + Environment.NewLine;
            foreach (string f in Directory.GetFiles(source))
            {
                string f2 = f.Replace('\\', '/');
                msg += "    - " + f2.Split('/')[f2.Split('/').Length - 1].Split('.')[0] + ": " + File.ReadAllText(f) + Environment.NewLine;
            }
            return (msg);
        }
    }
}