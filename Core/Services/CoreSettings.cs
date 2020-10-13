using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Services
{
    public static class CoreSettings
    {
#if HOMEDEBUG
        public static readonly string EncryptedDbConnectionString = "9ucmH/N6jVy9M9+laUnDs3x62+eernAe6yqDs/Fa/NlwXJuKNovM/GgAkVzrRii6rvCVKFIJd0dnwn3OVNAUYsI7U3MCSJCqVJf8MJOEAU78GEHA3LSSYwTgiFYXHa2NQZhQ/28SMCryMXMlI38PcA==";

        public static readonly string LocalDbConnectionString = "server=localhost;protocol=SOCKET;UserId=b2buser;Password=oD88x-2!Hq;database=b2bmarket_test";
        public static readonly string RemoteDbConnectionString = "server=192.168.1.3;port=3306;UserId=b2buser;Password=oD88x-2!Hq;database=b2bmarket_test";

        public static string DbConnectionString;

        public static readonly string DbConnectionSalt = "gy!34q2.gteas_";

        public static readonly string ClientUserPwdSalt = "q98`Рsd~вя$";
        public static readonly string ClientUserPINSalt = "!kj8__dsf%Z";

        public readonly static string ServerIP = "192.168.1.3";
        public readonly static string b2bDataLocalDir = @"//ROUTER_MAIN/Media Server/B2B Market/FTP/Active";
#if HOMEDEBUGMAC
        public readonly static string PrivateAPIUrl = "http://localhost:47342";
#else
        public readonly static string PrivateAPIUrl = "http://192.168.1.3:47342";
#endif
        public readonly static string FTPAdminUser = "b2b-ftpadmindata";
        public readonly static string FTPAdminPassword = "yn7+g8=i";
        public readonly static string FTPAdminAccessString = FTPManager.GetFTPAccessString(FTPAdminUser, FTPAdminPassword);

        public readonly static string LogFileName = "agent_log.txt";

        public readonly static string PictureExtension = ".jpg";
        public readonly static int ProductPictureWidth = 300;
        public readonly static int ProductPictureHeight = 300;

        public readonly static int TopCategoryPictureWidth = 600;
        public readonly static int TopCategoryPictureHeight = 300;


        public readonly static string PicturesPath = "/pictures";
        public readonly static string ProductsPicturesPath = PicturesPath + "/products";
        public readonly static string MatchedProductsPicturesDir = "/matched";
        public readonly static string MatchedProductsPicturesPath = ProductsPicturesPath + MatchedProductsPicturesDir;
        public readonly static string UnmatchedProductsPicturesPath = ProductsPicturesPath + "/unmatched";
        public readonly static string ConflictedProductsPictureDir = "/conflicted";
        public readonly static string ConflictedProductsPicturesPath = ProductsPicturesPath + ConflictedProductsPictureDir;
        public readonly static string TopCategoriesPicturePath = PicturesPath + "/topcategories";
        public readonly static string SuppliersPicturePath = PicturesPath + "/suppliers";

        public readonly static string SuppliersPath = "/suppliers";
        public readonly static string SupplierOffersPath = "/offers";
        public readonly static string SupplierOrdersPath = "/orders";
        public readonly static string SupplierDescriptionsPath = "/descriptions";

        public readonly static string ProductPicturesRequestFileName = "reqprodpics.xml";
        public readonly static string ProductPicturesExtractionFileName = "prodpics.xml";
        public readonly static string ProductDescriptionsRequestFileName = "reqproddesc.xml";
        public readonly static string ProductDescriptionsExtractionFileName = "proddesc.xml";

        public readonly static string SettingsPath = "/settings";
        public readonly static string AgentSettingsFileName = "agent.xml";

        public readonly static string AdminTGBotSettingsFileName = "admintgbot.xml";
        public readonly static string SuperAdminTGBotPasswordHash = "$2b$10$KAhKW4/LGrdzVzUvFTyorOW058ZwE4DxnSOmJNf3q7gRLFnWridmC"; //ap~G3te!d_2G
        public readonly static string OperatorTGBotSettingsFileName = "operatortgbot.xml";

#else
        public static readonly string EncryptedDbConnectionString = "9ucmH/N6jVy9M9+laUnDs3x62+eernAe6yqDs/Fa/NlwXJuKNovM/GgAkVzrRii6rvCVKFIJd0dnwn3OVNAUYsI7U3MCSJCqVJf8MJOEAU78GEHA3LSSYwTgiFYXHa2NQZhQ/28SMCryMXMlI38PcA==";

        public static readonly string LocalDbConnectionString = "server=localhost;protocol=SOCKET;UserId=b2buser;Password=oD88x-2!Hq;database=b2bmarket_test";
        public static readonly string RemoteDbConnectionString = "server=185.121.81.212;port=3306;UserId=b2buser;Password=oD88x-2!Hq;database=b2bmarket_test";

        public static string DbConnectionString;

        public static readonly string DbConnectionSalt = "gy!34q2.gteas_";

        public static readonly string ClientUserPwdSalt = "q98`Рsd~вя$";
        public static readonly string ClientUserPINSalt = "!kj8__dsf%Z";

        public readonly static string ServerIP = "185.121.81.212";
        public readonly static string b2bDataLocalDir = @"/usr/local/lib/b2bmarket";

        public readonly static string PrivateAPIUrl = $"http://{ServerIP}:47342";

        public readonly static string FTPAdminUser = "b2b-ftpadmindata";
        public readonly static string FTPAdminPassword = "yn7+g8=i";
        public readonly static string FTPAdminAccessString = FTPManager.GetFTPAccessString(FTPAdminUser, FTPAdminPassword);

        public readonly static string LogFileName = "agent_log.txt";

        public readonly static string PictureExtension = ".jpg";
        public readonly static int ProductPictureWidth = 300;
        public readonly static int ProductPictureHeight = 300;

        public readonly static int TopCategoryPictureWidth = 600;
        public readonly static int TopCategoryPictureHeight = 300;


        public readonly static string PicturesPath = "/pictures";
        public readonly static string ProductsPicturesPath = PicturesPath + "/products";
        public readonly static string MatchedProductsPicturesDir = "/matched";
        public readonly static string MatchedProductsPicturesPath = ProductsPicturesPath + MatchedProductsPicturesDir;
        public readonly static string UnmatchedProductsPicturesPath = ProductsPicturesPath + "/unmatched";
        public readonly static string ConflictedProductsPictureDir = "/conflicted";
        public readonly static string ConflictedProductsPicturesPath = ProductsPicturesPath + ConflictedProductsPictureDir;
        public readonly static string TopCategoriesPicturePath = PicturesPath + "/topcategories";
        public readonly static string SuppliersPicturePath = PicturesPath + "/suppliers";

        public readonly static string SuppliersPath = "/suppliers";
        public readonly static string SupplierOffersPath = "/offers";
        public readonly static string SupplierOrdersPath = "/orders";
        public readonly static string SupplierDescriptionsPath = "/descriptions";

        public readonly static string ProductPicturesRequestFileName = "reqprodpics.xml";
        public readonly static string ProductPicturesExtractionFileName = "prodpics.xml";
        public readonly static string ProductDescriptionsRequestFileName = "reqproddesc.xml";
        public readonly static string ProductDescriptionsExtractionFileName = "proddesc.xml";

        public readonly static string SettingsPath = "/settings";
        public readonly static string AgentSettingsFileName = "agent.xml";

        public readonly static string AdminTGBotSettingsFileName = "admintgbot.xml";
        public readonly static string SuperAdminTGBotPasswordHash = "$2b$10$KAhKW4/LGrdzVzUvFTyorOW058ZwE4DxnSOmJNf3q7gRLFnWridmC"; //ap~G3te!d_2G
        public readonly static string OperatorTGBotSettingsFileName = "operatortgbot.xml";
#endif

        static CoreSettings()
        {
            DbConnectionString = RemoteDbConnectionString;
        }
    }
}
