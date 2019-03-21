using System.IO;
using Java.IO;

namespace Zebble.Device
{
    internal static class ContentUriConverter
    {
        internal static byte[] GetFileData(string path)
        {
            if (string.IsNullOrEmpty(path)) return null;

            var uri = Android.Net.Uri.Parse(path);
            var resolver = UIRuntime.CurrentActivity.ContentResolver;
            var inputStream = resolver.OpenInputStream(uri);
            return GetBytes(inputStream);
        }

        static byte[] GetBytes(Stream input)
        {
            var buffer = new byte[16 * 1024];
            using (var ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }

                return ms.ToArray();
            }
        }
    }
}