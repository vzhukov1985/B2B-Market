
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
    public class ProductForDescView : Product
    {
        private bool _hasDescription;
        public bool HasDescription
        {
            get { return _hasDescription; }
            set
            {
                _hasDescription = value;
                OnPropertyChanged("HasDescription");
            }
        }

        private bool _hasDescriptionConflict;
        public bool HasDescriptionConflict
        {
            get { return _hasDescriptionConflict; }
            set
            {
                _hasDescriptionConflict = value;
                OnPropertyChanged("HasDescriptionConflict");
            }
        }

        private List<ConflictedDescription> _conflictedDescs;
        public List<ConflictedDescription> ConflictedDescs
        {
            get { return _conflictedDescs; }
            set
            {
                _conflictedDescs = value;
                OnPropertyChanged("ConflictedDescs");
            }
        }

        public ProductForDescView(Product p)
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



    public class DescriptionsPageVM<CommandType> : INotifyPropertyChanged where CommandType : IRelayCommand, new()
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        private ObservableCollection<ProductForDescView> _products;
        public ObservableCollection<ProductForDescView> Products
        {
            get { return _products; }
            set
            {
                _products = value;
                OnPropertyChanged("Products");
            }
        }

        private ProductForDescView _selectedProduct;
        public ProductForDescView SelectedProduct
        {
            get { return _selectedProduct; }
            set
            {
                if (CurrentDescriptionWasChanged)
                {
                    if (DialogService.ShowOkCancelDialog("Описание было изменено, но не сохранено. Отменить изменения?", "ВНИМАНИЕ!!!") == false)
                    {
                        CurrentDescriptionWasChanged = false;
                        int ind = Products.IndexOf(_selectedProduct);
                        var p = _selectedProduct;
                        Products.Remove(_selectedProduct);
                        Products.Insert(ind, p);
                        SelectedProduct = Products[ind];
                        CurrentDescriptionWasChanged = true;
                        return;
                    }
                    else
                    {
                        using (MarketDbContext db = new MarketDbContext())
                        {
                            SelectedProduct.Description = db.ProductDescriptions.Find(SelectedProduct.Id);
                        }
                    }
                }
                _selectedProduct = value;
                SelectedProductConflictedDescIndex = 0;
                OnPropertyChanged("SelectedProduct");
                if (SelectedProduct.Description == null)
                    SelectedProduct.Description = new ProductDescription { ProductId = SelectedProduct.Id, Text = "" };
                CurrentDescriptionWasChanged = false;
            }
        }

        private int _selectedProductConflictedDescIndex;
        public int SelectedProductConflictedDescIndex
        {
            get { return _selectedProductConflictedDescIndex; }
            set
            {
                _selectedProductConflictedDescIndex = value;

                if (SelectedProduct.ConflictedDescs != null && SelectedProduct.ConflictedDescs.Count > 0)
                {
                    SelectedProductConflictedDesc = SelectedProduct.ConflictedDescs[value].Description;
                }
                else
                {
                    SelectedProductConflictedDesc = "";
                }

                OnPropertyChanged("SelectedProductConflictedDescIndex");
            }
        }

        private string _selectedProductConflictedDesc;
        public string SelectedProductConflictedDesc
        {
            get { return _selectedProductConflictedDesc; }
            set
            {
                _selectedProductConflictedDesc = value;
                OnPropertyChanged("SelectedProductConflictedDesc");
            }
        }

        private int _productsWithoutDescCount;
        public int ProductsWithoutDescCount
        {
            get { return _productsWithoutDescCount; }
            set
            {
                _productsWithoutDescCount = value;
                OnPropertyChanged("ProductsWithoutDescCount");
            }
        }

        private int _productsWithConflictedDescsCount;
        public int ProductsWithConflictedDescsCount
        {
            get { return _productsWithConflictedDescsCount; }
            set
            {
                _productsWithConflictedDescsCount = value;
                OnPropertyChanged("ProductsWithConflictedDescsCount");
            }
        }

        private bool _currentDescriptionWasChanged;
        public bool CurrentDescriptionWasChanged
        {
            get { return _currentDescriptionWasChanged; }
            set
            {
                _currentDescriptionWasChanged = value;
                OnPropertyChanged("CurrentDescriptionWasChanged");
            }
        }


        private void RequestDescsForProduct(Product product)
        {
            foreach (Offer offer in product.Offers)
            {
                Stream xmlReadStream = FTPManager.GetReqProdDescStreamIfAvailable(offer.Supplier.FTPUser, offer.Supplier.FTPPassword);
                if (xmlReadStream != null)
                {
                    MemoryStream streamToWrite = XMLProcessor.UpdateReqProdDesc(xmlReadStream, offer);
                    xmlReadStream.Close();
                    if (streamToWrite != null)
                    {
                        if (FTPManager.UpdateReqProdDescFile(streamToWrite, offer.Supplier.FTPUser, offer.Supplier.FTPPassword) == false)
                            DialogService.ShowMessageDialog("Ошибка связи с сервером. Попробуйте позже", "Ошибка");
                        streamToWrite.Close();
                    }
                }
                else
                {
                    MemoryStream streamToWrite = XMLProcessor.CreateReqProdDesc(offer);
                    if (FTPManager.UpdateReqProdDescFile(streamToWrite, offer.Supplier.FTPUser, offer.Supplier.FTPPassword) == false)
                        DialogService.ShowMessageDialog("Ошибка связи с сервером. Попробуйте позже", "Ошибка");
                    streamToWrite.Close();
                }
            }
        }

        private void RequestDescsForAllProducts()
        {
            //TODO: Make optimization for creating multiple descriptions requests
            foreach (Product product in Products.Where(p => p.HasDescription == false))
            {
                RequestDescsForProduct(product);
            }
        }

        private void UpdateCurrentDescription()
        {
            using (MarketDbContext db = new MarketDbContext())
            {
                ProductDescription desc = db.ProductDescriptions.Find(SelectedProduct.Id);
                if (desc == null)
                {
                    db.Add(new ProductDescription { ProductId = SelectedProduct.Id, Text = SelectedProduct.Description.Text });
                }
                else
                {
                    desc.Text = SelectedProduct.Description.Text;
                    db.ProductDescriptions.Update(desc);
                }
                if (SelectedProduct.HasDescriptionConflict == true)
                {
                    db.ConflictedDescriptions.RemoveRange(SelectedProduct.ConflictedDescs);
                }
                db.SaveChanges();
            }
            if (SelectedProduct.HasDescription == false)
            {
                SelectedProduct.HasDescription = true;
                ProductsWithoutDescCount--;
            }
            if (SelectedProduct.HasDescriptionConflict == true)
            {
                SelectedProduct.HasDescriptionConflict = false;
                SelectedProduct.ConflictedDescs = null;
                SelectedProductConflictedDescIndex = 0;
                ProductsWithConflictedDescsCount--;
            }


            CurrentDescriptionWasChanged = false;

        }

        private List<ConflictedDescription> allConflictedDescs;
        private List<Guid> matchedDescsGuids;

        private void QueryConflictedNoDescGuids()
        {
            using (MarketDbContext db = new MarketDbContext())
            {
                allConflictedDescs = db.ConflictedDescriptions.AsNoTracking().ToList();
                matchedDescsGuids = db.ProductDescriptions.Select(pd => pd.ProductId).ToList();
            }


        }


        private async void QueryDb()
        {
            using (MarketDbContext db = new MarketDbContext())
            {
                Products = new ObservableCollection<ProductForDescView>(await db.Products
                    .AsNoTracking()
                    .Include(p => p.Offers)
                    .ThenInclude(of => of.Supplier)
                    .Include(p => p.Description)
                    .Include(p => p.Category)
                    .Include(p => p.ExtraProperties)
                    .ThenInclude(ep => ep.PropertyType)
                    .Include(p => p.VolumeType)
                    .Include(p => p.VolumeUnit)
                    .Select(p => new ProductForDescView(p))
                    .ToListAsync());
            }

            foreach (ProductForDescView product in Products)
            {
                product.HasDescription = matchedDescsGuids == null ? false : matchedDescsGuids.Contains(product.Id);
                product.HasDescriptionConflict = allConflictedDescs == null ? false : allConflictedDescs.Select(cp => cp.ProductId).Contains(product.Id);
                product.ConflictedDescs = allConflictedDescs == null ? null : allConflictedDescs.Where(cp => cp.ProductId == product.Id).ToList();
            }

            ProductsWithoutDescCount = Products.Where(p => p.HasDescription == false).Count();
            ProductsWithConflictedDescsCount = Products.Where(p => p.HasDescriptionConflict == true).Count();
        }

        private readonly IPageService PageService;
        private readonly IDialogService DialogService;

        public CommandType ShowPreviousConflictDescCommand { get; }
        public CommandType ShowNextConflictDescCommand { get; }


        public CommandType RequestDescsForProductCommand { get; }
        public CommandType RequestDescsForAllProductsCommand { get; }

        public CommandType CurrentDescriptionChangedCommand { get; }
        public CommandType UpdateCurrentDescriptionCommand { get; }

        public CommandType ShowPreviousPageCommand { get; }

        public DescriptionsPageVM(IPageService pageService, IDialogService dialogService)
        {
            PageService = pageService;
            DialogService = dialogService;

            ShowPreviousConflictDescCommand = new CommandType();
            ShowPreviousConflictDescCommand.Create(_ => SelectedProductConflictedDescIndex--, _ => SelectedProduct != null && SelectedProductConflictedDescIndex > 0);
            ShowNextConflictDescCommand = new CommandType();
            ShowNextConflictDescCommand.Create(_ => SelectedProductConflictedDescIndex++, _ => SelectedProduct != null && SelectedProduct.ConflictedDescs != null && SelectedProductConflictedDescIndex < SelectedProduct.ConflictedDescs.Count - 1);


            RequestDescsForProductCommand = new CommandType();
            RequestDescsForProductCommand.Create(_ => RequestDescsForProduct(SelectedProduct), _ => SelectedProduct != null && SelectedProduct.Offers != null && SelectedProduct.Offers.Count > 0);
            RequestDescsForAllProductsCommand = new CommandType();
            RequestDescsForAllProductsCommand.Create(_ => RequestDescsForAllProducts(), _ => ProductsWithoutDescCount > 0);
            CurrentDescriptionChangedCommand = new CommandType();
            CurrentDescriptionChangedCommand.Create(_ => CurrentDescriptionWasChanged = true);
            UpdateCurrentDescriptionCommand = new CommandType();
            UpdateCurrentDescriptionCommand.Create(_ => UpdateCurrentDescription(), _ => CurrentDescriptionWasChanged || (SelectedProduct != null && SelectedProduct.HasDescriptionConflict == true));

            ShowPreviousPageCommand = new CommandType();
            ShowPreviousPageCommand.Create(_ => PageService.ShowOffersPage());
            
            QueryConflictedNoDescGuids();
            QueryDb();
        }
    }
}

