using System.Security.Cryptography;
using System.Text;

namespace PlexRipper.Domain
{
    public static class HashGenerator
    {
        public static string Generate(DownloadTask downloadTask)
        {
            var copy = new
            {
                downloadTask.Key,
                downloadTask.PlexLibrary,
                downloadTask.PlexServerId,
                downloadTask.DownloadTaskType,
            };

            return CreateMD5(copy.ToString());
        }

        /// <summary>
        /// Source: https://stackoverflow.com/a/24031467/8205497.
        /// </summary>
        /// <param name="input">The string to convert to MD5.</param>
        /// <returns>The MD5 string.</returns>
        public static string CreateMD5(string input)
        {
            // Use input string to calculate MD5 hash
            using MD5 md5 = MD5.Create();
            var inputBytes = Encoding.ASCII.GetBytes(input);
            var hashBytes = md5.ComputeHash(inputBytes);

            // Convert the byte array to hexadecimal string
            StringBuilder sb = new StringBuilder();
            foreach (var bytes in hashBytes)
            {
                sb.Append(bytes.ToString("X2"));
            }

            return sb.ToString();
        }
    }
}