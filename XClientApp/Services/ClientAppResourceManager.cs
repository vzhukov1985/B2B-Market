using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Text;

namespace XClientApp.Services
{
    public static class ClientAppResourceManager
    {
        public static string GetString(string resourceName)
        {
            ResourceManager rm = new ResourceManager("ClientApp.Resources.UILang", Assembly.GetExecutingAssembly());
            return rm.GetString(resourceName);
        }
    }
}
