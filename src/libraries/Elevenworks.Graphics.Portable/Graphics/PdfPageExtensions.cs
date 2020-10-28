using System;
using System.IO;
using System.Threading.Tasks;

namespace Elevenworks.Graphics
{
    public static class PdfPageExtensions
    {
        public static byte[] AsBytes(this EWPdfPage target)
        {
            if (target == null)
                return null;

            using (var stream = new MemoryStream())
            {
                target.Save(stream);
                return stream.ToArray();
            }
        }

        public static Stream AsStream(this EWPdfPage target)
        {
            if (target == null)
                return null;

            var stream = new MemoryStream();
            target.Save(stream);
            stream.Position = 0;

            return stream;
        }

        public static async Task<byte[]> AsBytesAsync(this EWPdfPage target)
        {
            if (target == null)
                return null;

            using (var stream = new MemoryStream())
            {
                await target.SaveAsync(stream);
                return stream.ToArray();
            }
        }
        
        public static string AsBase64(this EWPdfPage target)
        {
            if (target == null)
                return null;

            var bytes = target.AsBytes();
            return Convert.ToBase64String(bytes);
        }
    }
}