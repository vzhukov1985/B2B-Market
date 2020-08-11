using Core.DBModels;
using Core.Services;
using Microsoft.EntityFrameworkCore;
using OperatorApp.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace OperatorApp.ViewModels
{
    public class ProductForPictureView:Product
    {
        private bool _hasPicture;
        public bool HasPicture
        {
            get { return _hasPicture; }
            set
            {
                _hasPicture = value;
                OnPropertyChanged("HasPicture");
            }
        }

        private bool _hasPictureConflict;
        public bool HasPictureConflict
        {
            get { return _hasPictureConflict; }
            set
            {
                _hasPictureConflict = value;
                OnPropertyChanged("HasPictureConflict");
            }
        }

        private List<ConflictedPic> _conflictedPics;
        public List<ConflictedPic> ConflictedPics
        {
            get { return _conflictedPics; }
            set
            {
                _conflictedPics = value;
                OnPropertyChanged("ConflictedPics");
            }
        }

        public ProductForPictureView(Product p)
        {
            this.BestDiscountPriceOffer = p.BestDiscountPriceOffer;
            this.BestRetailPriceOffer = p.BestRetailPriceOffer;
            this.Category = p.Category;
            this.CategoryId = p.CategoryId;
            this.Code = p.Code;
            this.Description = p.Description;
            this.ExtraProperties = p.ExtraProperties;
            this.Favorites = p.Favorites;
            this.Id = p.Id;
            this.IsFavoriteForUser = p.IsFavoriteForUser;
            this.IsOfContractedSupplier = p.IsOfContractedSupplier;
            this.Name = p.Name;
            this.Offers = p.Offers;
            this.Volume = p.Volume;
            this.VolumeType = p.VolumeType;
            this.VolumeTypeId = p.VolumeTypeId;
            this.VolumeUnit = p.VolumeUnit;
            this.VolumeUnitId = p.VolumeUnitId;
        }
    }



    public class PicturesPageVM<CommandType> : INotifyPropertyChanged where CommandType : IRelayCommand, new()
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        private ObservableCollection<ProductForPictureView> _products;
        public ObservableCollection<ProductForPictureView> Products
        {
            get { return _products; }
            set
            {
                _products = value;
                OnPropertyChanged("Products");
            }
        }

        private ProductForPictureView _selectedProduct;
        public ProductForPictureView SelectedProduct
        {
            get { return _selectedProduct; }
            set
            {
                _selectedProduct = value;
                SelectedProductConflictedPictureIndex = 0;
                CustomPicture = null;
                OnPropertyChanged("SelectedProduct");
            }
        }

        private int _selectedProductConflictedPictureIndex;
        public int SelectedProductConflictedPictureIndex
        {
            get { return _selectedProductConflictedPictureIndex; }
            set
            {
                _selectedProductConflictedPictureIndex = value;

                if (SelectedProduct != null && SelectedProduct.ConflictedPics != null && SelectedProduct.ConflictedPics.Count > 0)
                {
                    SelectedProductConflictedPicture = FTPManager.GetConflictedPicture(SelectedProduct.ConflictedPics[value].Id);
                }
                else
                {
                    SelectedProductConflictedPicture = null;
                }

                OnPropertyChanged("SelectedProductConflictedPictureIndex");
            }
        }

        private byte[] _selectedProductConflictedPicture;
        public byte[] SelectedProductConflictedPicture
        {
            get { return _selectedProductConflictedPicture; }
            set
            {
                _selectedProductConflictedPicture = value;
                OnPropertyChanged("SelectedProductConflictedPicture");
            }
        }

        private byte[] _customPicture;
        public byte[] CustomPicture
        {
            get { return _customPicture; }
            set
            {
                _customPicture = value;
                OnPropertyChanged("CustomPicture");
            }
        }

        private int _productsWithoutPicturesCount;
        public int ProductsWithoutPicturesCount
        {
            get { return _productsWithoutPicturesCount; }
            set
            {
                _productsWithoutPicturesCount = value;
                OnPropertyChanged("ProductsWithoutPicturesCount");
            }
        }

        private int _productsWithConflictedPicsCount;
        public int ProductsWithConflictedPicsCount
        {
            get { return _productsWithConflictedPicsCount; }
            set
            {
                _productsWithConflictedPicsCount = value;
                OnPropertyChanged("ProductsWithConflictedPicsCount");
            }
        }

        private void UpdateConflictedPicture()
        {
            if (DialogService.ShowOkCancelDialog("Сопоставить новую картинку с выбранным товаром?", "Подтвердите сопоставление"))
            {
                using (MarketDbContext db = new MarketDbContext())
                {
                    foreach (ConflictedPic conflictedPic in SelectedProduct.ConflictedPics)
                    {

                        if (SelectedProductConflictedPictureIndex == SelectedProduct.ConflictedPics.IndexOf(conflictedPic))
                        {
                            FTPManager.UpdateMatchedPicWithConflicted(conflictedPic.Id, SelectedProduct.Id);
                        }
                        else
                        {
                            FTPManager.RemoveConflictedPic(conflictedPic.Id);
                        }
                        db.ConflictedPics.Remove(conflictedPic);
                        allConflictedPics.Remove(conflictedPic);
                    }
                    db.SaveChanges();
                }
                SelectedProduct.ConflictedPics = null;
                ProductsWithConflictedPicsCount--;
                SelectedProduct.HasPictureConflict = false;
                var prod = SelectedProduct;
                int ind = Products.IndexOf(SelectedProduct);
                Products.RemoveAt(ind);
                Products.Insert(ind, prod);
                SelectedProduct.OnPropertyChanged("PictureUri");
                SelectedProductConflictedPicture = null;
            }
        }

        public void RemoveConflictedPictures()
        {
            if (DialogService.ShowOkCancelDialog("Оставить выбранную картинку, удалив все конфликтующие?", "Подтвердите сопоставление"))
            {
                using (MarketDbContext db = new MarketDbContext())
                {
                    foreach (ConflictedPic conflictedPic in SelectedProduct.ConflictedPics)
                    {

                        FTPManager.RemoveConflictedPic(conflictedPic.Id);
                        db.ConflictedPics.Remove(conflictedPic);
                        allConflictedPics.Remove(conflictedPic);
                    }
                    db.SaveChanges();
                }
                SelectedProduct.ConflictedPics = null;
                ProductsWithConflictedPicsCount--;
                SelectedProduct.HasPictureConflict = false;
                SelectedProductConflictedPicture = null;
                int ind = Products.IndexOf(SelectedProduct);
                SelectedProduct = Products[ind + 1];
            }
        }

        private void LoadCustomPicture()
        {
            if (DialogService.ShowOpenPictureDialog())
            {
                if (Path.GetExtension(DialogService.FilePath).ToUpper() == ".PNG")
                {
                    byte[] data = File.ReadAllBytes(DialogService.FilePath);
                    if (data[0] == 137 &&
                        data[1] == 80 &&
                        data[2] == 78 &&
                        data[3] == 71 &&
                        data[4] == 13 &&
                        data[5] == 10 &&
                        data[6] == 26 &&
                        data[7] == 10)
                    {
                        CustomPicture = data;
                    }
                }
            }
        }

        private void MatchWithCustomPicture()
        {
            if (DialogService.ShowOkCancelDialog("Сопоставить новую картинку с выбранным товаром?", "Подтвердите сопоставление"))
            {
                FTPManager.RemoveMatchedPic(SelectedProduct.Id);
                FTPManager.UploadPicDataToMatchedPics(SelectedProduct.Id, CustomPicture);
                if (SelectedProduct.HasPicture == false)
                {
                    SelectedProduct.HasPicture = true;
                    ProductsWithoutPicturesCount--;
                }
                var prod = SelectedProduct;
                int ind = Products.IndexOf(SelectedProduct);
                Products.RemoveAt(ind);
                Products.Insert(ind, prod);
                SelectedProduct.OnPropertyChanged("PictureUri");
                CustomPicture = null;
            }
        }

        private void RequestPicturesForProduct(Product product)
        {
            foreach (Offer offer in product.Offers)
            {
                Stream xmlReadStream = FTPManager.GetReqProdPicsStreamIfAvailable(offer.Supplier.FTPUser, offer.Supplier.FTPPassword);
                if (xmlReadStream != null)
                {
                    MemoryStream streamToWrite = XMLProcessor.UpdateReqProdPics(xmlReadStream, offer);
                    xmlReadStream.Close();
                    if (streamToWrite != null)
                    {
                        if (FTPManager.UpdateReqProdPicsFile(streamToWrite, offer.Supplier.FTPUser, offer.Supplier.FTPPassword) == false)
                            DialogService.ShowMessageDialog("Ошибка связи с сервером. Попробуйте позже", "Ошибка");
                        streamToWrite.Close();
                    }
                }
                else
                {
                    MemoryStream streamToWrite = XMLProcessor.CreateReqProdPics(offer);
                    if (FTPManager.UpdateReqProdPicsFile(streamToWrite, offer.Supplier.FTPUser, offer.Supplier.FTPPassword) == false)
                        DialogService.ShowMessageDialog("Ошибка связи с сервером. Попробуйте позже", "Ошибка");
                    streamToWrite.Close();
                }
            }
        }

        private void RequestPicturesForAllProducts()
        {
            //TODO: Make optimization for creating multiple pictures requests
            foreach(Product product in Products.Where(p => p.HasPicture == false))
            {
                RequestPicturesForProduct(product);
            }
        }

        private List<ConflictedPic> allConflictedPics;
        private List<Guid> matchedPicsGuids;

        private void QueryConflictedNoPictureGuids()
        {
            using (MarketDbContext db = new MarketDbContext())
            {
                allConflictedPics = db.ConflictedPics.AsNoTracking().ToList();
            }
            matchedPicsGuids = FTPManager.GetProductsMatchedPicturesGuids();
        }


        private async void QueryDb()
        {
            using (MarketDbContext db = new MarketDbContext())
            {
                Products = new ObservableCollection<ProductForPictureView>(await db.Products
                    .AsNoTracking()
                    .Include(p => p.Offers)
                    .ThenInclude(of => of.Supplier)
                    .Include(p => p.Category)
                    .Include(p => p.ExtraProperties)
                    .ThenInclude(ep => ep.PropertyType)
                    .Include(p => p.VolumeType)
                    .Include(p => p.VolumeUnit)
                    .Select(p => new ProductForPictureView(p))
                    .ToListAsync());
            }

            foreach(ProductForPictureView product in Products)
            {
                product.HasPicture = matchedPicsGuids == null ? false : matchedPicsGuids.Contains(product.Id);
                product.HasPictureConflict = allConflictedPics == null ? false : allConflictedPics.Select(cp => cp.ProductId).Contains(product.Id);
                product.ConflictedPics = allConflictedPics == null ? null : allConflictedPics.Where(cp => cp.ProductId == product.Id).ToList(); 
            }

            ProductsWithoutPicturesCount = Products.Where(p => p.HasPicture == false).Count();
            ProductsWithConflictedPicsCount = Products.Where(p => p.HasPictureConflict == true).Count();
        }

        private readonly IPageService PageService;
        private readonly IDialogService DialogService;

        public CommandType ShowPreviousConflictPicCommand { get; }
        public CommandType ShowNextConflictPicCommand { get; }
        public CommandType UpdateConflictedPictureCommand { get; }
        public CommandType RemoveConflictedPicturesCommand { get; }
        public CommandType LoadCustomPictureCommand { get; }
        public CommandType MatchWithCustomPictureCommand { get; }

        public CommandType RequestPicturesForProductCommand { get; }
        public CommandType RequestPicturesForAllProductsCommand { get; }

        public CommandType ShowPreviousPageCommand { get; }
        public CommandType ShowNextPageCommand { get; }

        public PicturesPageVM(IPageService pageService, IDialogService dialogService)
        {
            PageService = pageService;
            DialogService = dialogService;

            ShowPreviousConflictPicCommand = new CommandType();
            ShowPreviousConflictPicCommand.Create(_ => SelectedProductConflictedPictureIndex--, _ => SelectedProduct!= null && SelectedProductConflictedPictureIndex > 0);
            ShowNextConflictPicCommand = new CommandType();
            ShowNextConflictPicCommand.Create(_ => SelectedProductConflictedPictureIndex++, _ => SelectedProduct!=null && SelectedProduct.ConflictedPics != null && SelectedProductConflictedPictureIndex < SelectedProduct.ConflictedPics.Count - 1);
            UpdateConflictedPictureCommand = new CommandType();
            UpdateConflictedPictureCommand.Create(_ => UpdateConflictedPicture(), _ => SelectedProductConflictedPicture != null);
            RemoveConflictedPicturesCommand = new CommandType();
            RemoveConflictedPicturesCommand.Create(_ => RemoveConflictedPictures(), _ => SelectedProduct != null && SelectedProduct.ConflictedPics != null && SelectedProduct.ConflictedPics.Count > 0);
            LoadCustomPictureCommand = new CommandType();
            LoadCustomPictureCommand.Create(_ => LoadCustomPicture());
            MatchWithCustomPictureCommand = new CommandType();
            MatchWithCustomPictureCommand.Create(_ => MatchWithCustomPicture(), _ => CustomPicture != null);

            RequestPicturesForProductCommand = new CommandType();
            RequestPicturesForProductCommand.Create(_ => RequestPicturesForProduct(SelectedProduct), _ => SelectedProduct != null && SelectedProduct.Offers != null && SelectedProduct.Offers.Count > 0);
            RequestPicturesForAllProductsCommand = new CommandType();
            RequestPicturesForAllProductsCommand.Create(_ => RequestPicturesForAllProducts(), _ => ProductsWithoutPicturesCount > 0);

            ShowPreviousPageCommand = new CommandType();
            ShowPreviousPageCommand.Create(_ => PageService.ShowOffersPage());
            ShowNextPageCommand = new CommandType();
            ShowNextPageCommand.Create(_ => PageService.ShowDescriptionsPage());

            QueryConflictedNoPictureGuids();
            QueryDb();
        }
    }
}
