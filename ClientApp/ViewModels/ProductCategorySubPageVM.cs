using ClientApp.Services;
using Core.DBModels;
using Core.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace ClientApp.ViewModels
{
    public class ProductCategorySubPageVM<CommandType> : INotifyPropertyChanged where CommandType : IRelayCommand, new()
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        private readonly IPageService PageService;

        private ClientUser _user;
        public ClientUser User
        {
            get { return _user; }
            set
            {
                _user = value;
                OnPropertyChanged("User");
            }
        }

        private MidCategory _upperCategory;
        public MidCategory UpperCategory
        {
            get { return _upperCategory; }
            set
            {
                _upperCategory = value;
                OnPropertyChanged("UpperCategory");
            }
        }

        private ObservableCollection<ProductCategory> _subCategories;
        public ObservableCollection<ProductCategory> SubCategories
        {
            get { return _subCategories; }
            set
            {
                _subCategories = value;
                OnPropertyChanged("SubCategories");
            }
        }

        private void CategorySelected(ProductCategory selectedCategory)
        {
            List<Guid> filterGuids;
            
            if (selectedCategory.Name == ClientAppResourceManager.GetString("UI_CategoriesSubPage_AllProducts"))
            {
                filterGuids = SubCategories.Select(sc => sc.Id).ToList();
                PageService.ShowOffersSubPage(User, UpperCategory.Name, filterGuids, null);
            }
            else
            {
                filterGuids = new List<Guid> { selectedCategory.Id };
                PageService.ShowOffersSubPage(User, selectedCategory.Name, filterGuids, null);
            }
        }

        public bool IsRedirectOnLoad { get; set; }

        public CommandType ShowSearchSubPageCommand { get; }
        public CommandType NavigationBackCommand { get; }
        public CommandType CategorySelectedCommand { get; }

        public ProductCategorySubPageVM(ClientUser user, MidCategory midCategory, IPageService pageService)
        {         
            User = user;
            UpperCategory = midCategory;
            PageService = pageService;
            IsRedirectOnLoad = false;

            ShowSearchSubPageCommand = new CommandType();
            ShowSearchSubPageCommand.Create(_ => PageService.ShowSearchSubPage(User));
            NavigationBackCommand = new CommandType();
            NavigationBackCommand.Create(_ => PageService.SubPageNavigationBack());
            CategorySelectedCommand = new CommandType();
            CategorySelectedCommand.Create(c => CategorySelected((ProductCategory)c));

            using (MarketDbContext db = new MarketDbContext())
                SubCategories = new ObservableCollection<ProductCategory>(db.ProductCategories.Where(c => c.MidCategoryId == UpperCategory.Id).OrderBy(c => c.Name));

            SubCategories.Insert(0, new ProductCategory { Name = ClientAppResourceManager.GetString("UI_CategoriesSubPage_AllProducts") });



            if (SubCategories.Count < 3)
            {
                IsRedirectOnLoad = true;
                CategorySelectedCommand.Execute(SubCategories[SubCategories.Count-1]);
            }

        }
    }
}
