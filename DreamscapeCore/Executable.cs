using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DreamscapeCore
{
    /// <summary>
    /// Describes an executable program in Dreamscape
    /// </summary>
    public class Executable
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public Action<string[]> Code { get; set; }
        public bool System { get; set; }

        /// <summary>
        /// Creates a new executable for Dreamscape
        /// </summary>
        /// <param name="name">The file name of the new executable</param>
        /// <param name="path">The executable's path (ignored if system=true)</param>
        /// <param name="code">The method to run when executable is launched</param>
        /// <param name="system">Specifies whether the executable is a system program (put in /bin)</param>
        public Executable(string name, string path, Action<string[]> code, bool system)
        {
            this.Name = name;
            this.System = system;

            if (system)
                this.Path = Interpreter.binDir.GetPath();
            else
                if (Directory.ParsePath(path, Interpreter.root, Interpreter.workDir).GetPath() ==
                    Interpreter.binDir.GetPath())
                    this.Path = "/home/user";
                else
                    this.Path = path;
            
            this.Code = code;
        }

        /// <summary>
        /// Invokes the method associated with the executable
        /// </summary>
        /// <param name="root">The current system root</param>
        /// <param name="workDir">The current directory</param>
        /// <param name="args">Arguments to pass to the method</param>
        public void Invoke(string[] args)
        {
            this.Code.Invoke(args);
        }
    }
}
