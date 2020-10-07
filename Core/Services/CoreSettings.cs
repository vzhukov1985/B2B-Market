using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Services
{
    public static class CoreSettings
    {
        //Local Asus cable -  server=192.168.1.3;UserId=Admin;Password=MySQLPassword;database=b2bmarket;
        public static readonly string EncryptedDbConnectionString = "9ucmH/N6jVy9M9+laUnDs3x62+eernAe6yqDs/Fa/NlwXJuKNovM/GgAkVzrRii6rvCVKFIJd0dnwn3OVNAUYsI7U3MCSJCqVJf8MJOEAU78GEHA3LSSYwTgiFYXHa2NQZhQ/28SMCryMXMlI38PcA==";
        //Local Asus wifi - server=192.168.1.100;UserId=Admin;Password=MySQLPassword;database=b2bmarket;
        // public static readonly string EncryptedDbConnectionString = "IDATzl/gMMa2UQTYY+HkEDkA2h9hjuaspgJI/rCw4QZiqnYZwToM9irkNEZqbt/YZ6aVrb1SH0Qqv1g4E4qcqIGlAvJhfB9a5hYWWmpgUQnCBf7DJN9uyutq/+FATwF4SnEqXmcZoygDo54wBYT10g==";
        //Local PhoneHotSpot
        //public static readonly string EncryptedDbConnectionString = "y/9eacJ85HVpnFIWVOu2JVn6npM9wL7hN20h/3k/wCjYJf8MohZUnHfjSo4WXswIK/yG/KbjlTt3m6CNYnNL7A4xI2cpDWX830/kFPXshrPjuw2hRyKu0jccpzHC3/Z3B+AzJ1pwq62PZPxbzobI+A==";
        //Local MacOs
        //public static readonly string EncryptedDbConnectionString = "UKRgGGYSFpJVVACCNWIEHtdYUmebshbyqiWzyg6g2JuFYuF0FYd0BG9CFyzY48qpFiwwFJg6CKcZT0nTeKk7X998rH/txEPBscI4G7slwgc5CCOL4GUJNHGnDY+uXLkV9lnrL7865ui4pWNGCnTN1Q==";


        public static readonly string DbConnectionString;

        public static readonly string DbConnectionSalt = "gy!34q2.gteas_";

        public static readonly string ClientUserPwdSalt = "q98`Рsd~вя$";
        public static readonly string ClientUserPINSalt = "!kj8__dsf%Z";

        //Local Asus cable
        //public readonly static string ServerIP = "192.168.1.3";
        //Local Asus wifi
        //public readonly static string ServerIP = "192.168.1.100";
        //Local Phone hotspot
        //public readonly static string ServerIP = "192.168.43.100";
        //Local MacOs
        //public readonly static string ServerIP = "localhost";
        //Remote server
        public readonly static string ServerIP = "45.150.64.30";
        public readonly static string b2bDataLocalDir = @"D:/B2B FTP Server Mirror";

        public readonly static string HTTPServerUrl = "http://"+ ServerIP;

        public readonly static string FTPAdminUser = "b2b-ftpadmin";
        public readonly static string FTPAdminPassword = "yn7˜g8=i";
        public readonly static string FTPAdminAccessString = FTPManager.GetFTPAccessString(FTPAdminUser, FTPAdminPassword);

        public readonly static string LogFileName = "AgentLog.txt";

        public readonly static string PictureExtension = ".jpg";
        public readonly static int ProductPictureWidth = 300;
        public readonly static int ProductPictureHeight = 300;

        public readonly static int TopCategoryPictureWidth = 600;
        public readonly static int TopCategoryPictureHeight = 300;


        public readonly static string PicturesPath = "/Pictures";
        public readonly static string ProductsPicturesPath = PicturesPath + "/Products";
        public readonly static string MatchedProductsPicturesDir = "/Matched";
        public readonly static string MatchedProductsPicturesPath = ProductsPicturesPath + MatchedProductsPicturesDir;
        public readonly static string UnmatchedProductsPicturesPath = ProductsPicturesPath + "/Unmatched";
        public readonly static string ConflictedProductsPictureDir = "/Conflicted";
        public readonly static string ConflictedProductsPicturesPath = ProductsPicturesPath + ConflictedProductsPictureDir;
        public readonly static string TopCategoriesPicturePath = PicturesPath + "/TopCategories";
        public readonly static string SuppliersPicturePath = PicturesPath + "/Suppliers";

        public readonly static string SuppliersPath = "/Suppliers";
        public readonly static string SupplierOffersPath = "/Offers";
        public readonly static string SupplierOrdersPath = "/Orders";
        public readonly static string SupplierDescriptionsPath = "/Descriptions";

        public readonly static string ProductPicturesRequestFileName = "reqprodpics.xml";
        public readonly static string ProductPicturesExtractionFileName = "prodpics.xml";
        public readonly static string ProductDescriptionsRequestFileName = "reqproddesc.xml";
        public readonly static string ProductDescriptionsExtractionFileName = "proddesc.xml";

        public readonly static string SettingsPath = "/Settings";
        public readonly static string AgentSettingsFileName = "Agent.xml";
        
        public readonly static string AdminTGBotSettingsFileName = "AdminTGBot.xml";
        public readonly static string SuperAdminTGBotPasswordHash = "$2b$10$KAhKW4/LGrdzVzUvFTyorOW058ZwE4DxnSOmJNf3q7gRLFnWridmC"; //ap~G3te!d_2G
        public readonly static string OperatorTGBotSettingsFileName = "OperatorTGBot.xml";

        static CoreSettings()
        {
            //DbConnectionString = StringCipher.Decrypt(EncryptedDbConnectionString, DbConnectionSalt);
            //DbConnectionString = "server=localhost;UserId=root;Password=MySQLPassword;database=b2bmarket;";
            DbConnectionString = "server=45.150.64.30;port=3306;UserId=b2buser;Password=oD88x-2!Hq;database=b2bmarket_test";
//            DbConnectionString = "server=45.150.64.30;port=3306;UserId=b2bdbadmin;Password=ght3!2_sq2;database=b2bmarket_test";
        }
    }
}
