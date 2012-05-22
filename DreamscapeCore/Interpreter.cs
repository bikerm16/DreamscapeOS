using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace DreamscapeCore
{
    public static class Interpreter
    {
        internal static Directory root;
        private static WeakReference _workDir, _binDir;
        internal static Directory workDir
        {
            get { return _workDir.Target as Directory; }
            set { _workDir.Target = value; }
        }
        internal static Directory binDir
        {
            get { return _binDir.Target as Directory; }
            set { _binDir.Target = value; }
        }
        public static Directory RootDirectory
        {
            get
            {
                if (superMode)
                    return root;
                else
                    return null;
            }
        }
        public static Directory WorkDirectory
        {
            get
            {
                if (superMode)
                    return workDir;
                else
                    return null;
            }
            set
            {
                if (superMode)
                    workDir = value;
            }
        }
        public static Directory BinDirectory
        {
            get
            {
                if (superMode)
                    return binDir;
                else
                    return null;
            }
        }
        
        private static SortedList<string, Executable> externalProg;
        private static SortedList<string, Executable> internalProg;
        private static List<Executable> mergedList;
        private static bool superMode = false;
        
        private struct FilesReader
        {
            public string path;
            public string name;
            public bool binary;
            public List<string> lines;
        }

        public static string Init(bool enhanced)
        {
            superMode = enhanced;
            _workDir = new WeakReference(null);
            _binDir = new WeakReference(null);
            
            root = new Directory("", null);

            InitFS();

            workDir = root.GetChild("home").GetChild("user");
            binDir = Directory.ParsePath("/bin", root, workDir);

            externalProg = new SortedList<string, Executable>();
            internalProg = InternalProgs.GenerateList();
            UpdateMergedList();

            foreach (KeyValuePair<string, Executable> item in internalProg)
            {
                File newProg = new File(item.Key, new string[] { "This is a binary file.", "Go away." }, true);
                if (item.Value.System)
                {
                    Directory.ParsePath("/bin", root, workDir).AddFile(newProg);
                }
                else
                {
                    try { Directory.CreateTree(item.Value.Path, root, workDir); }
                    catch { }
                    Directory.ParsePath(item.Value.Path, root, workDir).AddFile(newProg);
                }
            }

            return "user@box:" + workDir.GetPath() + "$ ";
        }

        private static void InitFS()
        {
            string error = "It seems to me that the base file system disappeared into oblivion. " +
                "I don't know how you did that, but I would really appreciate it if you told me. Oh, and don't do that next time.";

            // Creating directories
            string[] tree = Properties.Resources.tree.Split("\r\n".ToArray(), System.StringSplitOptions.RemoveEmptyEntries);
            //string[] tree = { };
            if (tree.Length == 0) { throw new ApplicationException(error); }
            for (int i = 0; i < tree.Length; i++)
            {
                try { Directory.CreateTree(tree[i], root, root); }
                catch { }
            }

            // Creating files (DO NOT TOUCH THIS. CHANGING EVEN A SINGLE BIT OF THIS CODE CAN LEAD TO
            // TOTAL SPACETIME MELTDOWN. NEVER EVER TOUCH THIS.
            string[] files = Properties.Resources.files.Split("\r\n".ToArray(), System.StringSplitOptions.RemoveEmptyEntries);
            FilesReader temp = new FilesReader(); temp.lines = new List<string>();
            int t = 0;
            if (files.Length == 0) { throw new ApplicationException(error); }
            while (t < files.Length)
            {
                if (files[t] == "--begin--")
                {
                    temp.path = files[t + 1].Split(' ')[0];
                    temp.name = files[t + 1].Split(' ')[1];
                    if (files[t + 1].Split(' ')[2].ToLower() == "b")
                        temp.binary = true;
                    else
                        temp.binary = false;
                    t += 2;
                }
                else if (files[t] == "--end--")
                {
                    File insert = new File(temp.name, temp.lines.ToArray(), temp.binary);
                    try { Directory.CreateTree(temp.path, root, root); }
                    catch { }
                    Directory.ParsePath(temp.path, root, root).AddFile(insert);
                    temp = new FilesReader(); temp.lines = new List<string>();
                    insert = null;
                    t++;
                }
                else
                {
                    temp.lines.Add(files[t]);
                    t++;
                }
            }
        }

        public static string Parse(string input)
        {
            //Setting the prompt
            string prompt = "user@box:" + workDir.GetPath() + "$ ";

            //Checking whether there is a command to parse
            if (input == "")
                return prompt;
            
            //Splitting the command line
            input = input.Trim();
            while (input.IndexOf("  ", 0, input.Length) != -1)
                input = input.Remove(input.IndexOf("  ", 0, input.Length), 1);
            string[] cmd = input.Split(' ');

            string exec = cmd[0];
            string[] args = new string[cmd.Length - 1];
            for (int i = 1; i < cmd.Length; i++)
            {
                args[i - 1] = cmd[i];
            }
            cmd = null;
            input = null;

            File program = null;    //Variable to hold the file for the executable
            bool system = false;    //Variable to indicate where the executable was found (default=current directory)
            
            //Looking for the executable in the current directory
            foreach (KeyValuePair<string,File> item in workDir.Files)
            {
                if ((item.Key == exec) && (item.Value.Binary == true))
                {
                    program = item.Value;
                    break;
                }
            }
            
            //If not found, looking in the system directory
            if (program == null)
            {
                foreach (KeyValuePair<string,File> item in binDir.Files)
                {
                    if ((item.Key == exec) && (item.Value.Binary == true))
                    {
                        program = item.Value;
                        system = true;          //Setting to true to indicate we found it in system directory
                        break;
                    }
                }
            }

            if (program == null)    //If not found, returning to prompt
            {
                return "Unrecognized command.\n" + prompt;
            }
            else
            {
                if (system)     //If found in system directory
                {
                    foreach (Executable item in mergedList)
                    {
                        //Looking for executables with the right name
                        //that are marked as system executables
                        if ((item.Name == program.Name) && (item.System == true))
                        {
                            item.Invoke(args);     //Invoking the method
                            prompt = "user@box:" + workDir.GetPath() + "$ ";    //Resetting the prompt (may have changed)
                            return prompt;
                        }
                    }
                }
                else  //If found in current directory
                {
                    //Looking for executables with the right name
                    //and the right path
                    foreach (Executable item in mergedList)
                    {
                        if ((item.Name == program.Name) && (item.Path == workDir.GetPath()))
                        {
                            item.Invoke(args);     //Invoking the method
                            prompt = "user@box:" + workDir.GetPath() + "$ ";    //Resetting the prompt (may have changed)
                            return prompt;
                        }
                    }
                }
            }

            //In case something went wrong (probably File object exists but not Executable)
            return "An error occured while launching the program. Sorry.\n" + prompt;
        }

        /// <summary>
        /// Adds a program into the list of allowed executables and adds the appropriate file to the FS
        /// </summary>
        /// <param name="exec">The program to add</param>
        public static void AddProgram(Executable exec)
        {
            externalProg.Add(exec.Name, exec);

            UpdateMergedList();

            File newProg = new File(exec.Name, new string[] { "This is a binary file.", "Go away." }, true);
            if (exec.System)
            {
                Directory.ParsePath("/bin", root, workDir).AddFile(newProg);
            }
            else
            {
                try { Directory.CreateTree(exec.Path, root, workDir); }
                catch { }
                Directory.ParsePath(exec.Path, root, workDir).AddFile(newProg);
            }
        }

        private static void UpdateMergedList()
        {
            mergedList = new List<Executable>();
            foreach (KeyValuePair<string, Executable> item in externalProg)
                mergedList.Add(item.Value);
            foreach (KeyValuePair<string, Executable> item in internalProg)
                mergedList.Add(item.Value);
        }
    }
}
