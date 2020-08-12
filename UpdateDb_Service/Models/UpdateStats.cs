using System;
using System.Collections.Generic;
using System.Text;

namespace UpdateDb_Service.Models
{
    public class ExtractionInfo
    {
        public string SupplierDirectory { get; set; }
        public string FileName { get; set; }
        public bool IsSuccessful { get; set; }

        public ExtractionInfo()
        {
            IsSuccessful = false;
        }

        public ExtractionInfo(string supplierDirectory, string fileName):this()
        {
            SupplierDirectory = supplierDirectory;
            FileName = fileName;
        }
    }

    public class UpdateException : Exception
    {
        public string FilePath { get; set; }
        public string CodeBlock { get; set; }

        public UpdateException(string message) : base(message) { }
        public UpdateException(string message, Exception inner) : base(message, inner) { }
    }

    public class UpdateStats
    {
        public int NewProductCategoriesAdded { get; set; }
        public int NewVolumeTypesAdded { get; set; }
        public int NewVolumeUnitsAdded { get; set; }
        public int NewQuantityUnitsAdded { get; set; }
        public int NewExtraPropertiesAdded { get; set; }
        public int NewOffersAdded { get; set; }
        public int MatchOffersDeleted { get; set; }
        public int ExistingOffersDeleted { get; set; }

        public int NewUnmatchedPicsAdded { get; set; }
        public int NewMatchedPicsAdded { get; set; }
        public int NewConflictedPicsAdded { get; set; }
        public int ProblemPics { get; set; }

        public int NewUnmatchedDescsAdded { get; set; }
        public int NewMatchedDescsAdded { get; set; }
        public int NewConflictedDescsAdded { get; set; }

        public List<ExtractionInfo> OffersExtractions { get; set; }
        public List<ExtractionInfo> PicturesExtractions { get; set; }
        public List<ExtractionInfo> DescriptionsExtractions { get; set; }

        public List<UpdateException> Exceptions { get; set; }

        public UpdateStats()
        {
            NewOffersAdded = 0;
            NewExtraPropertiesAdded = 0;
            NewProductCategoriesAdded = 0;
            NewVolumeTypesAdded = 0;
            NewVolumeUnitsAdded = 0;
            NewQuantityUnitsAdded = 0;
            MatchOffersDeleted = 0;
            ExistingOffersDeleted = 0;

            NewUnmatchedPicsAdded = 0;
            NewMatchedPicsAdded = 0;
            NewConflictedPicsAdded = 0;
            ProblemPics = 0;

            NewUnmatchedDescsAdded = 0;
            NewMatchedDescsAdded = 0;
            NewConflictedDescsAdded = 0;

            OffersExtractions = new List<ExtractionInfo>();
            PicturesExtractions = new List<ExtractionInfo>();
            DescriptionsExtractions = new List<ExtractionInfo>();

            Exceptions = new List<UpdateException>();
        }
    }
}
