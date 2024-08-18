using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Encoder = System.Drawing.Imaging.Encoder;

namespace ShadowBot.Common.Utilities
{
    public class ImageHelper
    {
        public static byte[] ToBytes(Image image, ImageFormat imageFormat)
        {
            using (var memoryStream = new MemoryStream())
            {
                image.Save(memoryStream, imageFormat);
                return memoryStream.ToArray();
            }
        }

        public static string ToBase64String(Image image, ImageFormat imageFormat)
        {
            return Convert.ToBase64String(ToBytes(image, imageFormat));
        }

        public static Image FromBase64String(string base64String)
        {
            var bytes = Convert.FromBase64String(base64String);
            using (var memoryStream = new MemoryStream(bytes))
                return Image.FromStream(memoryStream);
        }

        public static void CompressImage(Image bitmap, string outputPath, long quality)
        {
            EncoderParameters eps = new EncoderParameters(1);
            eps.Param[0] = new EncoderParameter(Encoder.Quality, quality);

            ImageCodecInfo jpgEncoder = GetEncoder(ImageFormat.Jpeg);
            bitmap.Save(outputPath, jpgEncoder, eps);
        }

        private static ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();

            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }

            return null;
        }
    }
}
