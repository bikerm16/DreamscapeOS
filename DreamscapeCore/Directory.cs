using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DreamscapeCore
{
    public class Directory
    {
        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                if (value.All(Char.IsLetterOrDigit))
                    _name = value;
                else
                    throw new ArgumentException("Name can only contain letters and digits");
            }
        }

        private SortedList<string, Directory> dirList;
        private SortedList<string, File> fileList;
        private Directory parent;

        public SortedList<string, Directory> Directories { get { return dirList; } }
        public SortedList<string, File> Files { get { return fileList; } }

        /// <summary>
        /// Creates a new directory.
        /// </summary>
        /// <param name="name">The name. DO NOT SET TO NULL UNLESS YOU KNOW WHAT YOU'RE DOING.</param>
        /// <param name="parent">This directory's parent. DO NOT SET TO NULL UNLESS YOU KNOW WHAT YOU'RE DOING.</param>
        public Directory(string name, Directory parent)
        {
            this.Name = name;
            this.parent = parent;
            this.dirList = new SortedList<string, Directory>();
            this.fileList = new SortedList<string, File>();
        }

        /// <summary>
        /// Adds a subdirectory.
        /// </summary>
        /// <param name="name">The new subdirectory's name.</param>
        public void AddDir(string name)
        {
            if (name == "")
                throw new ArgumentException("A directory name must be specified");

            try
            {
                this.dirList.Add(name, new Directory(name, this));
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Check your input", ex);
            }
        }

        /// <summary>
        /// Removes a subdirectory.
        /// </summary>
        /// <param name="name">The name of the directory to remove.</param>
        public void RemoveDir(string name)
        {
            if (name == "")
                throw new System.ArgumentException("A directory name must be specified");
            if (name.Contains(' '))
                throw new System.ArgumentException("Name must not contain spaces");
            if (!this.dirList.ContainsKey(name))
                throw new System.ArgumentException("Directory does not exist");
            this.dirList.Remove(name);
        }

        public void AddFile(File file)
        {
            try { this.fileList.Add(file.Name, file); }
            catch (ArgumentException ex) { throw new ArgumentException("File already exists.", ex); }
        }

        public void SetParent(Directory parent) { this.parent = parent; }
        public Directory GetParent() { return this.parent; }
        public void SetName(string name) { this.Name = name; }
        public string GetName() { return this.Name; }

        /// <summary>
        /// Use this to get a child directory from this one.
        /// </summary>
        /// <param name="name">The name of the child directory.</param>
        /// <returns>A directory-class object of the child directory or throws a System.ArgumentException when it is not found.</returns>
        public Directory GetChild(string name)
        {
            if (name == "")
                throw new ArgumentException("A directory name must be specified");
            
            try
            {
                return this.dirList[name];
            }
            catch (System.Collections.Generic.KeyNotFoundException ex)
            {
                throw new ArgumentException("Directory does not exist", ex);
            }
        }

        public File GetFile(string name)
        {
            if (name == "")
                throw new ArgumentException("A filename must be specified");

            try
            {
                return this.fileList[name];
            }
            catch (System.Collections.Generic.KeyNotFoundException ex)
            {
                throw new ArgumentException("File does not exist", ex);
            }
        }

        /// <summary>
        /// Returns a string with the contents of this directory.
        /// </summary>
        /// <returns>Returns a string with the contents of this directory.</returns>
        public override string ToString()
        {
            string str = "";
            for (int i = 0; i < this.dirList.Count; i++)
                str += "*" + this.dirList.ElementAt(i).Key + "\t";
            for (int i = 0; i < this.fileList.Count; i++)
                str += "#" + this.fileList.ElementAt(i).Key + "\t";
            return str;
        }

        /// <summary>
        /// Use this to parse a given filesystem path.
        /// </summary>
        /// <param name="path">String containing the path to parse.</param>
        /// <param name="root">The root directory of the filesystem for which to parse.</param>
        /// <param name="workDir">The current work directory for which to parse the path.</param>
        /// <returns>The final directory or null.</returns>
        internal static Directory ParsePath(string path, Directory root, Directory workDir)
        {
            Directory tempDir = workDir;
            string tempPath = path;

            tempPath = tempPath.Trim();

            while (tempPath.IndexOf("//", 0, tempPath.Length) != -1)
                tempPath = tempPath.Remove(tempPath.IndexOf("//", 0, tempPath.Length), 1);

            if (tempPath[0] == '/')
                tempDir = root;

            tempPath = tempPath.Trim('/');

            if (tempPath == "")
                return tempDir;

            string[] output = tempPath.Split('/');

            for (int i = 0; i < output.Length; i++)
            {
                try
                {
                    if (output[i] == ".") { continue; }
                    else if (output[i] == "..") { tempDir = tempDir.GetParent(); }
                    else { tempDir = tempDir.GetChild(output[i]); }
                }
                catch (System.ArgumentException) { return null; }
                if (tempDir == null) { tempDir = root; }
            }

            return tempDir;
        }

        internal static File ParseFilePath(string path, Directory root, Directory workDir)
        {
            string tempPath = path.Trim().TrimEnd('/');

            if (tempPath.Contains('/'))
            {
                Directory fileParent = ParsePath(tempPath.Substring(0, tempPath.LastIndexOf('/') + 1), root, workDir);
                if (fileParent == null) { return null; }

                string filename = tempPath.Substring(tempPath.LastIndexOf('/') + 1, tempPath.Length - tempPath.LastIndexOf('/') - 1);
                filename = filename.Trim();

                try { return fileParent.GetFile(filename); }
                //catch (Exception ex) { throw new ArgumentException("No file with that name exists over here. Maybe it's even a directory.", ex); }
                catch (Exception) { return null; }
            }
            else
            {
                try { return workDir.GetFile(tempPath); }
                //catch (Exception ex) { throw new ArgumentException("No file with that name exists over here. Maybe it's even a directory.", ex); }
                catch (Exception) { return null; }
            }
        }

        internal static void CreateTree(string path, Directory root, Directory workDir)
        {
            if (path == "")
                throw new System.ArgumentException("No path was specified");
            
            Directory tempDir = workDir;
            string tempPath = path;

            tempPath = tempPath.Trim();

            while (tempPath.IndexOf("//", 0, tempPath.Length) != -1)
                tempPath = tempPath.Remove(tempPath.IndexOf("//", 0, tempPath.Length), 1);
            
            if (tempPath[0] == '/')
                tempDir = root;

            tempPath = tempPath.Trim('/');

            string[] tempArr = tempPath.Split('/');

            for (int i = 0; i < tempArr.Length; i++)
            {
                try { tempDir.AddDir(tempArr[i]); }
                catch (System.ArgumentException) { }
                tempDir = tempDir.GetChild(tempArr[i]);
            }
        }

        internal string GetPath()
        {
            string output = this.GetName() + "/";
            Directory temp = this;

            while (temp.GetParent() != null)
            {
                temp = temp.GetParent();
                output = temp.GetName() + "/" + output;
            }

            if (output.Length > 1)
                output = output.TrimEnd('/');

            return output;
        }
    }
}
