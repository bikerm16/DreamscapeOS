using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CursesSharp;

namespace DreamscapeCore.AMI
{
    public static class Main
    {
        private static Window title, status, tools;
        private static string[] lineOne = { "^C", "Cur Pos" };
        private static string[] lineTwo = { "^X", "Exit" };
        
        public static string[] Run(string file, string[] read)
        {
            Window std = Curses.InitScr();
            Curses.StartColor();
            Curses.InitPair(1, Colors.BLACK, Colors.WHITE);

            title = std.SubWin(1, 80, 0, 0);
            title.Background = Curses.COLOR_PAIR(1);
            title.Add(0, 2, "AMI v2.2.6");
            if (file != "")
                title.Add(0, (80 - file.Length - "File: ".Length) / 2, "File: " + file);
            else
                title.Add(0, (80 - "New Buffer".Length) / 2, "New Buffer");

            status = std.SubWin(1, 80, 22, 0);
            status.AttrOn(CursesSharp.Attrs.REVERSE);
            string statusLine = "[ Status bar ]";
            status.Add(0, (80 - statusLine.Length) / 2, statusLine);

            tools = std.SubWin(2, 80, 23, 0);
            UpdateTools(lineOne, lineTwo);

            std.Refresh();

            AMI.TextBox test = new AMI.TextBox(20, 80, 2, 0, title, status);
            test.Fill(read);
            while (true)
            {
                string[] data = test.Edit();
                if (data != null)
                {
                    status.Move(0, 0); status.ClearToEol();
                    uint bgStatus = status.Background;
                    status.AttrOff(CursesSharp.Attrs.REVERSE);
                    status.Background = Curses.COLOR_PAIR(1);
                    status.Add(0, 0, "Save modified buffer (ANSWERING \"No\" WILL DESTROY CHANGES) ? ");
                    UpdateTools(new string[] { " Y", "Yes" }, new string[] { " N", "No\t\t", "^C", "Cancel" });
                    int response = status.GetChar();
                    if ((char)response == 'n' || (char)response == 'N')
                    {
                        Curses.EndWin();
                        return null;
                    }
                    else if ((char)response == 'y' || (char)response == 'Y')
                    {
                        string name;
                        while (true)
                        {
                            status.Move(0, 0); status.ClearToEol();
                            if (file == "")
                                status.Add(0, 0, "File Name to Write: ");
                            else
                                status.Add(0, 0, "File Name to Write [" + file + "]: ");
                            Curses.Echo = true;
                            name = status.GetString();
                            if (name == "")
                            {
                                if (file != "")
                                {
                                    name = file;
                                    break;
                                }
                            }
                            else
                                break;
                        }
                        if (name.Contains('/'))
                        {
                            Directory dir = Directory.ParsePath(name.Substring(0, name.LastIndexOf('/') + 1), Interpreter.root, Interpreter.workDir);
                            if (dir == null)
                            {
                                string error = "[ Error writing " + name + ": No such file or directory ]";
                                status.Background = bgStatus;
                                status.AttrOn(CursesSharp.Attrs.REVERSE);
                                status.Move(0, 0); status.ClearToEol();
                                status.Add(0, (80 - error.Length) / 2, error);
                                status.Refresh();
                                continue;
                            }
                        }
                        Curses.EndWin();
                        string[] output = new string[data.Length + 1];
                        data.CopyTo(output, 0);
                        output[output.Length - 1] = name;
                        return output;
                    }
                    else if (response == 3)
                    {
                        status.Background = bgStatus;
                        status.AttrOn(CursesSharp.Attrs.REVERSE);
                        status.Move(0, 0); status.ClearToEol();
                        status.Add(0, (80 - statusLine.Length) / 2, statusLine);
                        status.Refresh();
                        UpdateTools(lineOne, lineTwo);
                    }
                }
                else
                {
                    Curses.EndWin();
                    return null;
                }
            }
        }

        private static void UpdateTools(string[] lineOne, string[] lineTwo)
        {
            int i = 0;
            tools.Clear();
            tools.Move(0, 0);
            while (i < lineOne.Length)
            {
                tools.AttrOn(CursesSharp.Attrs.REVERSE);
                tools.Add(lineOne[i]);
                i++;
                tools.AttrOff(CursesSharp.Attrs.REVERSE);
                tools.Add(" " + lineOne[i] + "  ");
                i++;
            }
            
            i = 0;
            tools.Move(1, 0);
            while (i < lineTwo.Length)
            {
                tools.AttrOn(CursesSharp.Attrs.REVERSE);
                tools.Add(lineTwo[i]);
                i++;
                tools.AttrOff(CursesSharp.Attrs.REVERSE);
                tools.Add(" " + lineTwo[i] + "  ");
                i++;
            }

            tools.Refresh();
        }
    }
}
