using Core.Services;
using System;

namespace PicConverter
{
    class Program
    {
        static void Main()
        {
            Uri srcPath = new Uri(CoreSettings.FTPAdminAccessString+"/Test.png");
            Uri destPath = new Uri(CoreSettings.FTPAdminAccessString + "/Res.jpg");
            if (ImageProcessor.ResizeAndConvertImageToJpeg(srcPath, destPath))
            {
                Console.WriteLine("Works!");
            }
            {
                Console.WriteLine("UnsupportedFmt");
            }
        }
    }
}
