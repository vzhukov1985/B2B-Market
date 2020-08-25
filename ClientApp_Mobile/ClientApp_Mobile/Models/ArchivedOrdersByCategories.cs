using Core.DBModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace ClientApp_Mobile.Models
{
    public class ArchivedOrdersByCategories: List<ArchivedOrder>, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        private string _categoryName;
        public string CategoryName
        {
            get { return _categoryName; }
            set
            {
                _categoryName = value;
                OnPropertyChanged("CategoryName");
            }
        }

        public ArchivedOrdersByCategories(string categoryName, IEnumerable<ArchivedOrder> archivedOrders) : base(archivedOrders)
        {
            CategoryName = categoryName;
        }

    }
}
