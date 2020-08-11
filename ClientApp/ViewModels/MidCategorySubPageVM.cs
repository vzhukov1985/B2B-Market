using ClientApp.Services;
using Core.DBModels;
using Core.Services;
using Microsoft.EntityFrameworkCore;
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
    public class MidCategorySubPageVM<CommandType> : INotifyPropertyChanged where CommandType : IRelayCommand, new()
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

        private TopCategory _upperCategory;
        public TopCategory UpperCategory
        {
            get { return _upperCategory; }
            set
            {
                _upperCategory = value;
                OnPropertyChanged("UpperCategory");
            }
        }

        private ObservableCollection<MidCategory> _subCategories;
        public ObservableCollection<MidCategory> SubCategories
        {
            get { return _subCategories; }
            set
            {
                _subCategories = value;
                OnPropertyChanged("SubCategories");
            }
        }

        private async void CategorySelected(MidCategory selectedCategory)
        {
            if (selectedCategory.Name == "Все товары")
            {
                var AllMidCategoriesGuids = SubCategories.Select(sc => sc.Id);
                List<Guid> filterGuids;
                using (MarketDbContext db = new MarketDbContext())
                {
                    filterGuids = await db.ProductCategories.AsNoTracking().Where(c => AllMidCategoriesGuids.Contains((Guid)c.MidCategoryId)).Select(c => c.Id).ToListAsync();
                }
                PageService.ShowOffersSubPage(User, UpperCategory.Name, filterGuids, null);
            }
            else
            {
                PageService.ShowProductCategoriesSubPage(User, selectedCategory);
            }
        }

        private void QueryDb()
        {
            using (MarketDbContext db = new MarketDbContext())
                SubCategories = new ObservableCollection<MidCategory>(db.MidCategories.AsNoTracking().Where(c => c.TopCategoryId == UpperCategory.Id).OrderBy(c => c.Name));

            SubCategories.Insert(0, new MidCategory { Name = "Все товары" });

            if (SubCategories.Count < 3)
            {
                IsRedirectOnLoad = true;
                CategorySelectedCommand.Execute(SubCategories[SubCategories.Count - 1]);
            }
        }

        public bool IsRedirectOnLoad { get; set; }

        public CommandType ShowSearchSubPageCommand { get; }
        public CommandType NavigationBackCommand { get; }
        public CommandType CategorySelectedCommand { get; }

        public MidCategorySubPageVM(ClientUser user, TopCategory topCategory, IPageService pageService)
        {
            User = user;
            UpperCategory = topCategory;
            PageService = pageService;
            IsRedirectOnLoad = false;

            ShowSearchSubPageCommand = new CommandType();
            ShowSearchSubPageCommand.Create(_ => PageService.ShowSearchSubPage(User));
            NavigationBackCommand = new CommandType();
            NavigationBackCommand.Create(_ => PageService.SubPageNavigationBack());
            CategorySelectedCommand = new CommandType();
            CategorySelectedCommand.Create(c => CategorySelected((MidCategory)c));

            QueryDb();
        }
    }
}
