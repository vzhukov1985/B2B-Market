using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Text;

namespace ClientApp.Services
{
    public static class ClientAppResourceManager
    {
        public static string GetString(string resourceName)
        {
            ResourceManager rm = new ResourceManager("ClientApp.Resources.UILang", Assembly.GetExecutingAssembly());
            return rm.GetString(resourceName);
        }

        public static byte[] GetImageAsByteArray(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var fullResourceName = string.Concat("ClientApp.Resources.UILang.", resourceName);
            using (var stream = assembly.GetManifestResourceStream(fullResourceName))
            {
                var buffer = new byte[stream.Length];
                stream.Read(buffer, 0, (int)stream.Length);
                return buffer;
            }
        }
    }
}
