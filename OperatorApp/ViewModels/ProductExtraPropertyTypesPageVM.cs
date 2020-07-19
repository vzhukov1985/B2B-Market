using Core.Models;
using Core.Services;
using Microsoft.EntityFrameworkCore;
using OperatorApp.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace OperatorApp.ViewModels
{
    public class ProductExtraPropertyTypesPageVM<CommandType> : INotifyPropertyChanged where CommandType : IRelayCommand, new()
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        private readonly IPageService PageService;
        private readonly IDialogService DialogService;

        private ObservableCollection<MatchProductExtraPropertyType> _productExtraPropertyTypesToMatch;
        public ObservableCollection<MatchProductExtraPropertyType> ProductExtraPropertyTypesToMatch
        {
            get { return _productExtraPropertyTypesToMatch; }
            set
            {
                _productExtraPropertyTypesToMatch = value;
                OnPropertyChanged("ProductExtraPropertyTypesToMatch");
            }
        }

        private ObservableCollection<ProductExtraPropertyType> _productExtraPropertyTypes;
        public ObservableCollection<ProductExtraPropertyType> ProductExtraPropertyTypes
        {
            get { return _productExtraPropertyTypes; }
            set
            {
                _productExtraPropertyTypes = value;
                OnPropertyChanged("ProductExtraPropertyTypes");
            }
        }

        private bool _showUncheckedOnly;
        public bool ShowUncheckedOnly
        {
            get { return _showUncheckedOnly; }
            set
            {
                _showUncheckedOnly = value;
                QueryDb();
                OnPropertyChanged("ShowUncheckedOnly");
            }
        }

        private int _uncheckedCount;
        public int UncheckedCount
        {
            get { return _uncheckedCount; }
            set
            {
                _uncheckedCount = value;
                OnPropertyChanged("UncheckedCount");
            }
        }

        private MatchProductExtraPropertyType _selectedMatchProductExtraPropertyType;
        public MatchProductExtraPropertyType SelectedMatchProductExtraPropertyType
        {
            get { return _selectedMatchProductExtraPropertyType; }
            set
            {
                _selectedMatchProductExtraPropertyType = value;

                if (ProductExtraPropertyTypes != null && _selectedMatchProductExtraPropertyType != null)
                    SelectedProductExtraPropertyType = ProductExtraPropertyTypes.Where(vt => vt.Id == _selectedMatchProductExtraPropertyType.ProductExtraPropertyTypeId).FirstOrDefault();

                OnPropertyChanged("SelectedMatchProductExtraPropertyType");
            }
        }

        private ProductExtraPropertyType _selectedProductExtraPropertyType;
        public ProductExtraPropertyType SelectedProductExtraPropertyType
        {
            get { return _selectedProductExtraPropertyType; }
            set
            {
                _selectedProductExtraPropertyType = value;
                OnPropertyChanged("SelectedProductExtraPropertyType");
            }
        }

        private string _searchMatchProductExtraPropertyTypesText;
        public string SearchMatchProductExtraPropertyTypesText
        {
            get { return _searchMatchProductExtraPropertyTypesText; }
            set
            {
                _searchMatchProductExtraPropertyTypesText = value;
                OnPropertyChanged("SearchMatchProductExtraPropertyTypesText");
            }
        }

        private string _searchProductExtraPropertyTypesText;
        public string SearchProductExtraPropertyTypesText
        {
            get { return _searchProductExtraPropertyTypesText; }
            set
            {
                _searchProductExtraPropertyTypesText = value;
                OnPropertyChanged("SearchProductExtraPropertyTypesText");
            }
        }


        private void UpdateTypesMatching()
        {
            using (MarketDbContext db = new MarketDbContext())
            {
                var productExtraPropertiesToUpdate = db.ProductExtraProperties
                    .Where(pep => pep.PropertyTypeId == SelectedMatchProductExtraPropertyType.ProductExtraPropertyTypeId);
                foreach (ProductExtraProperty productExtraProperty in productExtraPropertiesToUpdate)
                    productExtraProperty.PropertyTypeId = SelectedProductExtraPropertyType.Id;

                db.ProductExtraProperties.UpdateRange(productExtraPropertiesToUpdate);
                db.SaveChanges();
            }

            using (MarketDbContext db = new MarketDbContext())
            {

                SelectedMatchProductExtraPropertyType.ProductExtraPropertyTypeId = SelectedProductExtraPropertyType.Id;
                if (SelectedMatchProductExtraPropertyType.IsChecked == false)
                {
                    SelectedMatchProductExtraPropertyType.IsChecked = true;
                    UncheckedCount--;
                }
                db.MatchProductExtraPropertyTypes.Update(SelectedMatchProductExtraPropertyType);

                if (SelectedProductExtraPropertyType.IsChecked == false)
                {
                    SelectedProductExtraPropertyType.IsChecked = true;
                    db.ProductExtraPropertyTypes.Update(SelectedProductExtraPropertyType);
                }
                db.SaveChanges();
            }
        }


        private async void MatchProductExtraPropertyTypes()
        {
            int newListItemIndex = ProductExtraPropertyTypesToMatch.IndexOf(SelectedMatchProductExtraPropertyType);
            using (MarketDbContext db = new MarketDbContext())
            {
                ProductExtraPropertyType productExtraPropertyTypeToRemove = await db.ProductExtraPropertyTypes.FindAsync(SelectedMatchProductExtraPropertyType.ProductExtraPropertyTypeId);
                if (await db.MatchProductExtraPropertyTypes.Where(mvt => mvt.ProductExtraPropertyTypeId == productExtraPropertyTypeToRemove.Id).CountAsync() == 1 && productExtraPropertyTypeToRemove.Id != SelectedProductExtraPropertyType.Id)
                {
                    if (DialogService.ShowWarningMatchAndDeleteDialog(SelectedMatchProductExtraPropertyType.Supplier.ShortName,
                        $"\"{SelectedMatchProductExtraPropertyType.SupplierProductExtraPropertyTypeName}\"",
                        $"\"{SelectedProductExtraPropertyType.Name}\"",
                         $"\"{productExtraPropertyTypeToRemove.Name}\""))
                    {
                        UpdateTypesMatching();
                        db.ProductExtraPropertyTypes.Remove(productExtraPropertyTypeToRemove);
                        ProductExtraPropertyTypes.Remove(ProductExtraPropertyTypes.Where(vt => vt.Id == productExtraPropertyTypeToRemove.Id).FirstOrDefault());
                        await db.SaveChangesAsync();
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    UpdateTypesMatching();
                }
            }

            if (ShowUncheckedOnly)
            {
                ProductExtraPropertyTypesToMatch.Remove(SelectedMatchProductExtraPropertyType);
            }
            else
            {
                newListItemIndex++;
            }

            if (ProductExtraPropertyTypesToMatch != null && newListItemIndex < ProductExtraPropertyTypesToMatch.Count)
            {
                SelectedMatchProductExtraPropertyType = ProductExtraPropertyTypesToMatch[newListItemIndex];
            }
        }

        private async void EditProductExtraPropertyType()
        {
            ProductExtraPropertyType updatedProductExtraPropertyType = DialogService.ShowEditProductExtraPropertyTypeDialog(SelectedProductExtraPropertyType);
            if (updatedProductExtraPropertyType != null)
            {
                using (MarketDbContext db = new MarketDbContext())
                {
                    SelectedProductExtraPropertyType.Name = updatedProductExtraPropertyType.Name;
                    db.ProductExtraPropertyTypes.Update(SelectedProductExtraPropertyType);
                    await db.SaveChangesAsync();
                }
            }
        }



        public async void QueryDb()
        {
            using (MarketDbContext db = new MarketDbContext())
            {
                ProductExtraPropertyTypesToMatch = new ObservableCollection<MatchProductExtraPropertyType>(await db.MatchProductExtraPropertyTypes
                    .Include(mvt => mvt.Supplier)
                    .Where(mvt => ShowUncheckedOnly ? mvt.IsChecked == false : true)
                    .Where(mvt => mvt.SupplierProductExtraPropertyTypeName.Contains(SearchMatchProductExtraPropertyTypesText) || mvt.Supplier.ShortName.Contains(SearchMatchProductExtraPropertyTypesText))
                    .AsNoTracking()
                    .ToListAsync()
                    );

                ProductExtraPropertyTypes = new ObservableCollection<ProductExtraPropertyType>(await db.ProductExtraPropertyTypes
                    .Where(vt => vt.Name.Contains(SearchProductExtraPropertyTypesText))
                    .AsNoTracking()
                    .ToListAsync()
                    );

                UncheckedCount = await db.MatchProductExtraPropertyTypes.Where(mvt => mvt.IsChecked == false).CountAsync();
            }
        }

        public CommandType SearchMatchProductExtraPropertyTypesCommand { get; }
        public CommandType CancelSearchMatchProductExtraPropertyTypesCommand { get; }

        public CommandType EditProductExtraPropertyTypeCommand { get; }
        public CommandType SearchProductExtraPropertyTypesCommand { get; }
        public CommandType CancelSearchProductExtraPropertyTypesCommand { get; }

        public CommandType MatchProductExtraPropertyTypesCommand { get; }

        public CommandType ShowNextPageCommand { get; }

        public ProductExtraPropertyTypesPageVM(IPageService pageService, IDialogService dialogService)
        {
            PageService = pageService;
            DialogService = dialogService;

            ShowUncheckedOnly = true;
            SearchProductExtraPropertyTypesText = "";
            SearchMatchProductExtraPropertyTypesText = "";

            SearchMatchProductExtraPropertyTypesCommand = new CommandType();
            SearchMatchProductExtraPropertyTypesCommand.Create(_ => QueryDb());
            CancelSearchMatchProductExtraPropertyTypesCommand = new CommandType();
            CancelSearchMatchProductExtraPropertyTypesCommand.Create(_ => { SearchMatchProductExtraPropertyTypesText = ""; QueryDb(); });

            EditProductExtraPropertyTypeCommand = new CommandType();
            EditProductExtraPropertyTypeCommand.Create(_ => EditProductExtraPropertyType(), _ => SelectedProductExtraPropertyType != null);
            SearchProductExtraPropertyTypesCommand = new CommandType();
            SearchProductExtraPropertyTypesCommand.Create(_ => QueryDb());
            CancelSearchProductExtraPropertyTypesCommand = new CommandType();
            CancelSearchProductExtraPropertyTypesCommand.Create(_ => { SearchProductExtraPropertyTypesText = ""; QueryDb(); });

            MatchProductExtraPropertyTypesCommand = new CommandType();
            MatchProductExtraPropertyTypesCommand.Create(_ => MatchProductExtraPropertyTypes(), _ => SelectedProductExtraPropertyType != null && SelectedMatchProductExtraPropertyType != null);

            ShowNextPageCommand = new CommandType();
            ShowNextPageCommand.Create(_ => PageService.ShowVolumeUnitsPage(), _ => UncheckedCount == 0);

            QueryDb();
        }
    }
}
