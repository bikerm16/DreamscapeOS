using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DreamscapeCore
{
    public class File
    {
        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                if (value == "")
                    throw new ArgumentException("Name must not be empty");

                if (value.All(c => Char.IsLetterOrDigit(c) || c == '.'))
                    _name = value;
                else
                    throw new ArgumentException("Name can only contain letters and digits");
            }
        }

        public string[] Data { get; set; }
        public bool Binary { get; set; }

        /// <summary>
        /// Creates a new text file.
        /// </summary>
        /// <param name="name">Filename.</param>
        /// <param name="data">The data to put into the file.</param>
        /// <param name="system">Indicates whether this is a binary file.</param>
        public File(string name, string[] data, bool binary)
        {
            try { this.Name = name; }
            catch (ArgumentNullException ex) { throw new ArgumentException("Looks like your filename is null. I forbid this.", ex); }
            catch (ArgumentException ex) { throw new ArgumentException("Rules: filename must not be empty. Filename can contain only letters, digits and dots. Do not disappoint me again.", ex); }
            
            this.Data = data;
            this.Binary = binary;
        }

        /// <summary>
        /// Returns the file's contents.
        /// </summary>
        /// <returns>Returns the file's contents.</returns>
        public override string ToString()
        {
            string output = "\r\n";

            for (int i = 0; i < this.Data.Length; i++)
                output += this.Data[i] + "\r\n";

            output += "\r\n";

            return output;
        }
    }
}
