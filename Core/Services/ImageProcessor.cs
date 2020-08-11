using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Text;

namespace Core.Services
{
    public static class ImageProcessor
    {
        private static void DeleteSourceImage(Uri uri)
        {
            switch (uri.Scheme)
            {
                case "file":
                    File.Delete(uri.LocalPath);
                    break;
                case "ftp":
                    FTPManager.DeleteFile(uri);
                    break;
                default:
                    break;
            }
        }

        private static Bitmap ResizeBitmap(Bitmap bmpSource)
        {
            int sourceWidth = bmpSource.Width;
            int sourceHeight = bmpSource.Height;
            int sourceX = 0;
            int sourceY = 0;
            int destX = 0;
            int destY = 0;
            int Width = CoreSettings.ProductPictureWidth;
            int Height = CoreSettings.ProductPictureHeight;

            float nPercent;
            float nPercentW;
            float nPercentH;

            nPercentW = ((float)Width / (float)sourceWidth);
            nPercentH = ((float)Height / (float)sourceHeight);
            if (nPercentH < nPercentW)
            {
                nPercent = nPercentH;
                destX = System.Convert.ToInt16((Width -
                              (sourceWidth * nPercent)) / 2);
            }
            else
            {
                nPercent = nPercentW;
                destY = System.Convert.ToInt16((Height -
                              (sourceHeight * nPercent)) / 2);
            }

            int destWidth = (int)(sourceWidth * nPercent);
            int destHeight = (int)(sourceHeight * nPercent);

            Bitmap bmpDest = new Bitmap(Width, Height,
                              PixelFormat.Format24bppRgb);
            bmpDest.SetResolution(bmpSource.HorizontalResolution,
                             bmpSource.VerticalResolution);

            Graphics grDest = Graphics.FromImage(bmpDest);
            grDest.Clear(Color.White);
            grDest.InterpolationMode =
                    InterpolationMode.HighQualityBicubic;

            grDest.DrawImage(bmpSource,
                new Rectangle(destX, destY, destWidth, destHeight),
                new Rectangle(sourceX, sourceY, sourceWidth, sourceHeight),
                GraphicsUnit.Pixel);

            return bmpDest;
        }

        public static bool ResizeAndConvertImageToJpeg(Uri source, Uri dest)
        {
            Bitmap bmpSource;
            Stream srcStream;
            Stream destStream;

            WebClient wc = new WebClient();
            switch (source.Scheme)
            {
                case "file":
                    srcStream = new FileStream(source.LocalPath, FileMode.Open);
                    
                    break;
                case "ftp":
                    try
                    {
                        srcStream = wc.OpenRead(source);
                    }
                    catch
                    {
                        return false;
                    }
                    break;
                default:
                    return false;
            }
            
            try
            {
                bmpSource = new Bitmap(srcStream);
            }
            catch
            {
                wc.Dispose();
                srcStream.Dispose();
                DeleteSourceImage(source);
                return false;
            }

            Bitmap bmpDest = ResizeBitmap(bmpSource);

            switch (dest.Scheme)
            {
                case "file":
                    destStream = new FileStream(dest.LocalPath, FileMode.Create);
                    bmpDest.Save(destStream, ImageFormat.Jpeg);
                    break;
                case "ftp":
                    try
                    {
                        destStream = new MemoryStream();
                        bmpDest.Save(destStream, ImageFormat.Jpeg);
                        wc.UploadData(dest, ((MemoryStream)destStream).ToArray());
                    }
                    catch
                    {
                        return false;
                    }
                    break;
                default:
                    return false;
            }


            wc.Dispose();
            bmpSource.Dispose();
            bmpDest.Dispose();
            srcStream.Dispose();
            destStream.Dispose();
            DeleteSourceImage(source);
            return true;
        }

        public static byte[] GetResizedConvertedImageData(Uri source)
        {
            Bitmap bmpSource;
            Stream srcStream;

            WebClient wc = new WebClient();
            switch (source.Scheme)
            {
                case "file":
                    srcStream = new FileStream(source.LocalPath, FileMode.Open);

                    break;
                case "ftp":
                    try
                    {
                        srcStream = wc.OpenRead(source);
                    }
                    catch
                    {
                        return null;
                    }
                    break;
                default:
                    return null;
            }

            try
            {
                bmpSource = new Bitmap(srcStream);
            }
            catch
            {
                return null;
            }

            Bitmap bmpDest = ResizeBitmap(bmpSource);
            using (var stream = new MemoryStream())
            {
                wc.Dispose();
                bmpSource.Dispose();
                bmpDest.Save(stream, ImageFormat.Jpeg);
                bmpDest.Dispose();
                srcStream.Dispose();
                return stream.ToArray();
            }

        }

    }
}
