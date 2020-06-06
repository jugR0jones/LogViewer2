using System.IO;
using System.Diagnostics;

namespace LogViewer2.Utilities
{
    internal class FileUtilities
    {
        private static string GetApplicationFolder()
        {
            Process currentProcess = Process.GetCurrentProcess();
            string applicationDirectory = Path.GetDirectoryName(currentProcess.MainModule.FileName);

            return applicationDirectory;
        }

        /// <summary>
        /// Return the path in the application folder for the given filename
        /// </summary>
        /// <returns></returns>
        public static string GetPathForFilename(string filename)
        {
            string applicationFolder = GetApplicationFolder();
            string path = Path.Combine(applicationFolder, filename);
            
            return path;
        }
    }
}
