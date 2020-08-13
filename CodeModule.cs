using Discord.Commands;
using System;
using System.Collections.Generic;
using System.IO;

namespace Bot
{
    public class CodeModule : ModuleBase
    {
        Program p = Program.p;

        /*[Command("Compile")]
        public async Task compile(string code)
        {
            Console.WriteLine(code);
        }*/

        public static string getString(string str, out bool isError, out bool isString)
        {
            string final = "";
            isError = false;
            isString = (str[0] == '"');
            if (!isString) final += str[0];
            for (int i = 1; i < str.Length; i++)
            {
                if (str[i] == '"')
                {
                    if (!isString)
                    {
                        isError = true;
                        return ("unexpected '\"' after variable name.");
                    }
                    else
                    {
                        if (i == str.Length - 1)
                            return (final);
                        else
                        {
                            isError = true;
                            return ("unexpected characters after '\"'.");
                        }
                    }
                }
                else
                    final += str[i];
            }
            if (isString)
            {
                isError = true;
                return ("Unmatching '\"'.");
            }
            else
                return (final);
        }

        private static string[] cutArgs(string str)
        {
            bool waitingDoubleQuote = false;
            List<string> args = new List<string>();
            List<string> finalArgs = new List<string>();
            string currArg = "";
            foreach (char c in str)
            {
                if (char.IsWhiteSpace(c) && !waitingDoubleQuote)
                {
                    if (currArg != "")
                    {
                        args.Add(currArg);
                        currArg = "";
                    }
                }
                else
                    currArg += c;
                if (c == '"')
                {
                    if (waitingDoubleQuote)
                    {
                        args.Add(currArg);
                        currArg = "";
                    }
                    else
                    {
                        if (currArg != "\"")
                        {
                            args.Add(currArg);
                            currArg = "";
                        }
                    }
                    waitingDoubleQuote = !waitingDoubleQuote;
                }
            }
            if (currArg != "") args.Add(currArg);
            foreach (string s in args)
            {
                if (s == "//")
                    break;
                else
                    finalArgs.Add(s);
            }
            return (finalArgs.ToArray());
        }

        public class Variable
        {
            public enum type
            {
                INT,
                FLOAT,
                CHAR,
                STRING
            }

            public Variable(string name, type varType, string value) // value is null is not initialized
            {
                _name = name;
                _varType = varType;
                _value = value;
                _neverAssigned = (value == null);
                _neverUsed = true;
            }

            public string _name { private set; get; }
            public type _varType { private set; get; }
            public string _value { private set; get; }
            public bool _neverUsed { private set; get; }
            public bool _neverAssigned { private set; get; }
        }

        private static bool isAlphaDigitUnderscore(string str)
        {
            foreach (char c in str)
            {
                if (!char.IsLetterOrDigit(c) && c != '_')
                    return (false);
            }
            return (true);
        }

        private static bool isFloat(string var)
        {
            bool firstDot = false;
            foreach (char c in var)
            {
                if (!char.IsNumber(c))
                {
                    if (c == '.')
                    {
                        if (firstDot) return (false);
                        else firstDot = true;
                    }
                    else
                        return (false);
                }
            }
            return (true);
        }

        private static Variable.type? getType(string var)
        {
            if (var[0] == '"' && var[var.Length - 1] == '"') return (Variable.type.STRING);
            if (var[0] == '\'' && var[var.Length - 1] == '\'' && var.Length == 3) return (Variable.type.CHAR);
            try {
                Convert.ToInt32(var); return (Variable.type.INT);
            }catch (FormatException) { }
            if (isFloat(var))
                return (Variable.type.FLOAT);
            return (null);
        }

        private static string addVar(string[] line, Variable.type? _varType, out bool isError, ref List<Variable> vars)
        {
            isError = true;
            string[] keyWords = new string[] { "write", "writeln", "auto", "int", "char", "string", "float" };
            if (line.Length < 2)
                return ("variable declaration must be of the following format: type name.");
            if (line.Length == 3)
                return ("variable declaration if assigned must be of the following format: type name = value.");
            if (line.Length > 4)
                return ("invalid characters after variable value.");
            if (!isAlphaDigitUnderscore(line[1]))
                return ("variable's name must only contain letters, digits or underscores.");
            if (vars.Find(x => x._name == line[1]) != null)
                 return ("a variable with the same name already exist.");
            Variable.type? type = null;
            if (line.Length == 4)
            {
                type = getType(line[3]);
                if (line[2] != "=")
                    return ("variable's name and value must be separate by a '='.");
                if (type == null)
                    return ("variable's type is invalid.");
                if (_varType == null)
                {
                    isError = false;
                    vars.Add(new Variable(line[1], (Variable.type)type, line[3]));
                    return (type.ToString().ToLower() + " " + line[1] + " " + line[3]);
                }
                if (_varType != type)
                    return ("can't convert " + type.ToString().ToLower() + " to " + _varType.ToString().ToLower() + ".");
            }
            if (_varType == null)
                return ("variable's value must be set at creation when using auto");
            isError = false;
            vars.Add(new Variable(line[1], (Variable.type)_varType, ((line.Length == 4) ? (" " + line[3]) : (null))));
            return (_varType.ToString().ToLower() + " " + line[1] + ((line.Length == 4) ? (" " + line[3]) : ("")));

        }

        public static string compile(string[] file, out int returnCode, string fileName)
        {
            string result = "";
            string resultError = "";
            returnCode = 0;
            int nbLine = 1;
            List<Variable> vars = new List<Variable>();
            foreach (string line in file)
            {
                string[] currLine = cutArgs(line);
                if (currLine[0] == "write" || currLine[0] == "writeln")
                {
                    if (currLine.Length != 2)
                    { returnCode = 1; resultError += "line " + nbLine + ": write must take only 1 argument." + Environment.NewLine; }
                    else
                    {
                        bool isError, isString;
                        string str = getString(currLine[1], out isError, out isString);
                        if (isError)
                        { returnCode = 1; resultError += "line " + nbLine + ": " + str + Environment.NewLine; }
                        else if (isString || (!isString && vars.Find(x => x._name == str) != null))
                        {
                            if (isString)
                            {
                                if (currLine[0] == "write")
                                    result += "wri \"" + str + "\"" + Environment.NewLine;
                                else
                                    result += "wrl \"" + str + "\"" + Environment.NewLine;
                            }
                            else
                            {
                                if (vars.Find(x => x._name == str)._neverAssigned)
                                {
                                    returnCode = 1;
                                    resultError += "line " + nbLine + ": the variable is used but never assigned." + Environment.NewLine;
                                }
                                else if (currLine[0] == "write")
                                    result += "wri " + str + Environment.NewLine;
                                else
                                    result += "wrl " + str + Environment.NewLine;
                            }
                        }
                        else
                        { returnCode = 1; resultError += "line " + nbLine + ": the variable " + str + " doesn't exist." + Environment.NewLine; }
                    }
                }
                else if (currLine[0] == "auto" || currLine[0] == "int" || currLine[0] == "float" || currLine[0] == "char" || currLine[0] == "string")
                {
                    bool isError;
                    string tmp;
                    if (currLine[0] == "int") tmp = addVar(currLine, Variable.type.INT, out isError, ref vars);
                    else if (currLine[0] == "float") tmp = addVar(currLine, Variable.type.FLOAT, out isError, ref vars);
                    else if (currLine[0] == "char") tmp = addVar(currLine, Variable.type.CHAR, out isError, ref vars);
                    else if (currLine[0] == "string") tmp = addVar(currLine, Variable.type.STRING, out isError, ref vars);
                    else tmp = addVar(currLine, null, out isError, ref vars);
                    if (isError) { returnCode = 1; resultError += "line " + nbLine + ": " + tmp + Environment.NewLine; }
                    else result += tmp + Environment.NewLine;
                }
                nbLine++;
            }
            if (returnCode != 0) return (resultError);
            else
            {
                File.WriteAllText(fileName.Substring(0, fileName.Length - fileName.Split('.')[fileName.Split('.').Length - 1].Length) + "sye", result);
                return ("Compilation suceed.");
            }
        }

        public static string launch(string[] file, out int returnCode)
        {
            try
            {
                returnCode = 0;
                string output = "";
                List<Variable> vars = new List<Variable>();
                foreach (string line in file)
                {
                    string[] currLine = cutArgs(line);
                    if (currLine[0] == "wri")
                    {
                        if (currLine[1][0] == '"')
                            output += currLine[1].Substring(1, currLine[1].Length - 2);
                        else
                            output += vars.Find(x => x._name == currLine[1])._value;
                    }
                    else if (currLine[0] == "wrl")
                    {
                        if (currLine[1][0] == '"')
                            output += currLine[1].Substring(1, currLine[1].Length - 2) + Environment.NewLine;
                        else
                            output += vars.Find(x => x._name == currLine[1])._value + Environment.NewLine;
                    }
                    else if (currLine[0] == "int") vars.Add(new Variable(currLine[1], Variable.type.INT, ((currLine.Length == 3) ? (currLine[2]) : (null))));
                    else if (currLine[0] == "float") vars.Add(new Variable(currLine[1], Variable.type.FLOAT, ((currLine.Length == 3) ? (currLine[2]) : (null))));
                    else if (currLine[0] == "char") vars.Add(new Variable(currLine[1], Variable.type.CHAR, ((currLine.Length == 3) ? (currLine[2]) : (null))));
                    else if (currLine[0] == "string") vars.Add(new Variable(currLine[1], Variable.type.STRING, ((currLine.Length == 3) ? (currLine[2]) : (null))));
                }
                return (output);
            } catch (Exception)
            {
                returnCode = 1;
                return ("An error occured while executing the file, it may be corrupted.");
            }
        }
    }
}