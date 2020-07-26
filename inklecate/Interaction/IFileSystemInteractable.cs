using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ink.Inklecate.Interaction
{
    /// <summary>The IFileSystemInteractable interface defines the interaction with the file system.</summary>
    public interface IFileSystemInteractable
    {
        /// <summary>Gets the current working directory of the application.</summary>
        /// <returns></returns>
        string GetCurrentDirectory();

        /// <summary>Sets the application's current working directory to the specified directory.</summary>
        /// <param name="directoryPath">The directory path.</param>
        void SetCurrentDirectory(string directoryPath);

        /// <summary>Reads all text from file. then closes the file.</summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        string ReadAllTextFromFile(string path);

        /// <summary>Creates a new file, writes the specified string to the file using the specified encoding, 
        /// and then closes the file. If the target file already exists, it is overwritten.</summary>
        /// <param name="path">The path.</param>
        /// <param name="contents">The contents.</param>
        /// <param name="encoding">The encoding.</param>
        void WriteAllTextToFile(string path, string contents, Encoding encoding);
    }
}
