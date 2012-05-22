using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.Diagnostics;
using CursesSharp;

namespace DreamscapeCore
{
    public static class InternalProgs
    {
        internal static SortedList<string, Executable> GenerateList()
        {
            SortedList<string, Executable> output = new SortedList<string, Executable>();
            Executable temp;

            //Program name: cd
            temp = new Executable("cd", "/bin", Chdir, true);
            output.Add(temp.Name, temp);

            //Program name: mkdir
            temp = new Executable("mkdir", "/bin", Mkdir, true);
            output.Add(temp.Name, temp);

            //Program name: ls
            temp = new Executable("ls", "/bin", Ls, true);
            output.Add(temp.Name, temp);

            //Program name: cat
            temp = new Executable("cat", "/bin", Cat, true);
            output.Add(temp.Name, temp);

            //Program name: pwd
            temp = new Executable("pwd", "/bin", Pwd, true);
            output.Add(temp.Name, temp);

            //Program name: clear
            temp = new Executable("clear", "/bin", Clear, true);
            output.Add(temp.Name, temp);

            //Program name: exit
            temp = new Executable("exit", "/bin", Exit, true);
            output.Add(temp.Name, temp);

            //Program name: brainfuck
            temp = new Executable("bf", "/bin", BFRun, true);
            output.Add(temp.Name, temp);

            //Program name: edit
            temp = new Executable("edit", "/bin", Edit, true);
            output.Add(temp.Name, temp);

            //Program name: rain
            temp = new Executable("rain", "/bin", Rain, true);
            output.Add(temp.Name, temp);

            return output;

        }
        
        internal static void Chdir(string[] args)
        {
            if (args.Length == 0)
            {
                Interpreter.workDir = Interpreter.root.GetChild("home").GetChild("user");
                return;
            }

            Directory newDir = Directory.ParsePath(args[0], Interpreter.root, Interpreter.workDir);
            if (newDir == null) { Console.WriteLine("Invalid path.\r\n"); return; }
            
            Interpreter.workDir = newDir;

            return;
        }

        internal static void Mkdir(string[] args)
        {
            if (args.Length == 0)
                return;

            Directory.CreateTree(args[0], Interpreter.root, Interpreter.workDir);
        }

        internal static void Ls(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine(Interpreter.workDir.ToString());
                return;
            }

            Directory temp = Directory.ParsePath(args[0], Interpreter.root, Interpreter.workDir);
            if (temp == null) { Console.WriteLine("Invalid path."); return; }
            Console.WriteLine(temp.ToString());
        }

        internal static void Cat(string[] args)
        {
            if (args.Length == 0)
                return;

            Console.Write(Directory.ParseFilePath(args[0], Interpreter.root, Interpreter.workDir).ToString());
        }

        internal static void Pwd(string[] args)
        {
            Console.WriteLine(Interpreter.workDir.GetPath());
        }

        internal static void Clear(string[] args)
        {
            Console.Clear();
        }

        internal static void Exit(string[] args)
        {
            Environment.Exit(0);
        }

        internal static void BFRun(string[] args)
        {
            if (args.Length == 0)
            {
                Brainfuck.Brainfuck.Run(Properties.Resources.helloWorld);
            }
            else
            {
                File codeFile = Directory.ParseFilePath(args[0], Interpreter.root, Interpreter.workDir);
                string codeString = "";
                foreach (string line in codeFile.Data)
                    codeString += line; ;
                Brainfuck.Brainfuck.Run(codeString);
            }
        }

        internal static void Edit(string[] args)
        {
            string[] newData;
            
            if (args.Length != 0)
            {
                //string name = args[0];
                //if (args[0].Contains('/'))
                //{
                //    string path = args[0].Substring(0, args[0].LastIndexOf('/') + 1);
                //    Directory dir = Directory.ParsePath(path, Interpreter.root, Interpreter.workDir);
                //    if (dir == null) { Console.WriteLine("Path not found."); return; }
                //    name = args[0].Substring(args[0].LastIndexOf('/') + 1);
                //}
                File file = Directory.ParseFilePath(args[0], Interpreter.root, Interpreter.workDir);
                if (file == null)
                    newData = AMI.Main.Run(args[0], null);
                else
                    newData = AMI.Main.Run(args[0], file.Data);
            }
            else
                newData = AMI.Main.Run("", null);

            if (newData != null)
            {
                string name = newData[newData.Length - 1];
                File insert = Directory.ParseFilePath(name, Interpreter.root, Interpreter.workDir);
                if (insert == null)
                {
                    if (name.Contains('/'))
                    {
                        string path = name.Substring(0, name.LastIndexOf('/') + 1);
                        name = name.Substring(name.LastIndexOf('/') + 1);
                        Directory dir = Directory.ParsePath(path, Interpreter.root, Interpreter.workDir);
                        insert = new File(name, newData.Take(newData.Length - 1).ToArray(), false);
                        dir.AddFile(insert);
                    }
                    else
                    {
                        insert = new File(name, newData.Take(newData.Length - 1).ToArray(), false);
                        Interpreter.workDir.AddFile(insert);
                    }
                }
                else
                {
                    insert.Data = newData.Take(newData.Length - 1).ToArray();
                }
            }

            Console.Clear();
        }
        
        internal static void Rain(string[] args)
        {
            Curses.InitScr();
            try
            {
                RainDemo.Main.Run();
            }
            finally
            {
                Curses.EndWin();
                Console.Clear();
            }
        }

        //internal static void Nano(string[] args)
        //{
        //    string tempFileName = "temp.garbage";   //Variable to hold name for unnamed files
        //    Directory dir;  //Where to put new file
        //    File edit = null;   //Variable to hold file object in virtual FS
        //    if (args.Length == 0)   //Making sure there is a filename to work with
        //    {
        //        args = new string[1];
        //        args[0] = tempFileName;
        //    }
        //    edit = Directory.ParseFilePath(args[0], Interpreter.root, Interpreter.workDir); //Trying to get a file object (if exists)
        //    if (edit != null)
        //    {
        //        if (edit.Binary == true)    //Check that this is not a binary file
        //        {
        //            Console.WriteLine("Do you expect me to open a binary file in a text editor?");
        //            return;
        //        }
        //    }
        //    if (args[0].Contains('/'))  //If path contains '/', get the file's final destination
        //    {
        //        dir = Directory.ParsePath(args[0].Substring(0, args[0].LastIndexOf('/') + 1), Interpreter.root, Interpreter.workDir);
        //        args[0] = args[0].Substring(args[0].LastIndexOf('/') + 1); //Make sure we're using just the filename
        //    }
        //    else  //If not, file is to be written to current dir
        //    {
        //        dir = Interpreter.workDir;
        //    }
                
            
        //    //Writing the archive to disk
        //    FileStream nano = new FileStream("nano.zip", FileMode.Create);
        //    nano.Write(Properties.Resources.nano_2_2_6, 0, Properties.Resources.nano_2_2_6.Length);
        //    nano.Close();

        //    //Extracting contents
        //    using (ZipFile zip = ZipFile.Read("nano.zip"))
        //    {
        //        zip.ExtractAll("nano");
        //    }

        //    //Preparing to launch
        //    Process process = new Process();
        //    process.StartInfo.FileName = "nano\\nano.exe";
        //    if (edit != null)   //Dumping local file's contents to disk (if exists)
        //    {
        //        StringBuilder sb = new StringBuilder();
        //        foreach (string line in edit.Data)
        //            sb.AppendLine(line);
        //        StreamWriter outfile = new StreamWriter(args[0]);
        //        outfile.Write(sb.ToString());
        //        outfile.Close();
        //    }
        //    process.StartInfo.Arguments = "-t " + args[0];
        //    process.StartInfo.UseShellExecute = false;
        //    process.StartInfo.RedirectStandardOutput = false;

        //    //Launching
        //    process.Start();
        //    process.WaitForExit();

        //    //Reading data back
        //    if (System.IO.File.Exists(args[0]))     //Checking whether the user wrote something
        //    {
        //        StreamReader sr = new StreamReader(args[0]);
        //        List<string> lines = new List<string>();
        //        while (!sr.EndOfStream)
        //            lines.Add(sr.ReadLine());
        //        sr.Close();
        //        File temp = new File(args[0], lines.ToArray(), false);  //Holding an object with the new data
        //        Console.Write("Do you want to save changes? (Y/N): ");
        //        ConsoleKeyInfo response = Console.ReadKey();
        //        Console.WriteLine();
        //        if (response.Key == ConsoleKey.Y)
        //        {
        //            if (edit != null)
        //                edit.Data = temp.Data;  //Updating existing file
        //            else
        //                dir.AddFile(temp);  //Or creating a new one
        //        }
        //        System.IO.File.Delete(args[0]); //Cleaning up
        //    }

        //    //Removing executables
        //    System.IO.Directory.Delete("nano", true);
        //    System.IO.File.Delete("nano.zip");
        //}
    }
}
