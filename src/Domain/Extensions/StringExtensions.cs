using System;
using System.IO;
using System.Linq;

namespace PlexRipper.Domain
{
    public static class StringExtensions
    {
        private static Random random = new Random();

        public static string GetActualCasing(this string path)
        {
            if (OsInfo.IsNotWindows || path.StartsWith("\\"))
            {
                return path;
            }

            if (Directory.Exists(path) && (File.GetAttributes(path) & FileAttributes.Directory) == FileAttributes.Directory)
            {
                return GetProperCapitalization(new DirectoryInfo(path));
            }

            var fileInfo = new FileInfo(path);
            var dirInfo = fileInfo.Directory;

            var fileName = fileInfo.Name;

            if (dirInfo != null && fileInfo.Exists)
            {
                fileName = dirInfo.GetFiles(fileInfo.Name)[0].Name;
            }

            return Path.Combine(GetProperCapitalization(dirInfo), fileName);
        }

        private static string GetProperCapitalization(DirectoryInfo dirInfo)
        {
            var parentDirInfo = dirInfo.Parent;
            if (parentDirInfo == null)
            {
                //Drive letter
                return dirInfo.Name.ToUpper();
            }

            var folderName = dirInfo.Name;

            if (dirInfo.Exists)
            {
                folderName = parentDirInfo.GetDirectories(dirInfo.Name)[0].Name;
            }

            return Path.Combine(GetProperCapitalization(parentDirInfo), folderName);
        }

        public static string RandomString(int length, bool allowNumbers = false, bool allowCapitalLetters = false)
        {
            string chars = "abcdefghijklmnopqrstuvwxyz";

            if (allowCapitalLetters)
            {
                chars += "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            }

            if (allowNumbers)
            {
                chars += "0123456789";
            }

            return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static string SanitizeFolderName(this string folderName)
        {
            folderName = folderName.Replace(@"·", "-").Replace(":", " ");

            return new string(folderName.Where(ch => !Path.GetInvalidFileNameChars().Contains(ch)).ToArray());
        }
    }
}