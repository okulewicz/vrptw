using System;
using System.IO;

namespace VRPTWOptimizer.Utils.Reader
{
    public class FileWrapper
    {
        public static string SafeReadAllText(string path)
        {
            if (!File.Exists(path))
            {
                throw new Exception($"File: {path} not exists in: {Path.GetFullPath(path)}");
            }
            return File.ReadAllText(path);
        }

        public static void SafeSaveAllText(string path, string contents)
        {
            try
            {
                File.WriteAllText(path, contents);
            }
            catch (IOException)
            {
                Console.Error.WriteLine($"Could not write to {path}");
            }
        }
    }
}