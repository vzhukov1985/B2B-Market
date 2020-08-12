using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Services
{
    public static class CoreSettings
    {
        //server=localhost;UserId=root;Password=MySQLPassword;database=b2bmarket;
        public static readonly string EncryptedDbConnectionString = "VE/QWGXEbJjApGZjklT5CPf0mWrBHRTTqCc3qFm0kDh1vEhEAH9xVcF+TD/CI+QIxTiKRT5CuZouniH1L+1+GCPsepH7CVAn7A/gYYJR32BvUEj2TVWMwKa9ZtwJdnLSCU4Srm7i5Y8p4rCo/SPAKQ==";
        public static readonly string DbConnectionSalt = "gy!34q2.gteas_";

        public static readonly string ClientUserPwdSalt = "q98`Рsd~вя$";

        public readonly static string ServerIP = "192.168.1.3";
        public readonly static string b2bDataLocalDir = @"D:/B2B FTP Server Mirror";

        public readonly static string HTTPServerUrl = "http://"+ ServerIP;

        public readonly static string FTPAdminUser = "B2BAdmin";
        public readonly static string FTPAdminPassword = "B2BAdminPassword";
        public readonly static string FTPAdminAccessString = FTPManager.GetFTPAccessString(FTPAdminUser, FTPAdminPassword);

        public readonly static string LogFileName = "AgentLog.txt";

        public readonly static string PictureExtension = ".jpg";
        public readonly static int ProductPictureHeight = 300;
        public readonly static int ProductPictureWidth = 300;

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

        public readonly static string ProductPicturesRequestFileName = "ReqProdPics.xml";
        public readonly static string ProductPicturesExtractionFileName = "ProdPics.xml";
        public readonly static string ProductDescriptionsRequestFileName = "ReqProdDesc.xml";
        public readonly static string ProductDescriptionsExtractionFileName = "ProdDesc.xml";

        public readonly static string SettingsPath = "/Settings";
        public readonly static string AgentSettingsFileName = "Agent.xml";
        
        public readonly static string AdminTGBotSettingsFileName = "AdminTGBot.xml";
        public readonly static string SuperAdminTGBotPasswordHash = "$2b$10$KAhKW4/LGrdzVzUvFTyorOW058ZwE4DxnSOmJNf3q7gRLFnWridmC"; //ap~G3te!d_2G
        public readonly static string OperatorTGBotSettingsFileName = "OperatorTGBot.xml";
    }
}
