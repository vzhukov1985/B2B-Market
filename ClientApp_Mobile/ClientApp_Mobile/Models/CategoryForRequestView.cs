using Core.DBModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace ClientApp_Mobile.Models
{
    public class CategoryForRequestView : List<ProductForRequestView>, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged("Name");
            }
        }


        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                OnPropertyChanged("IsSelected");
            }
        }


        public decimal Subtotal
        {
            get
            {
                return this.Sum(p => p.Orders.Sum(o => o.PriceForClient * o.OrderQuantity));
            }
        }

        public bool? IsContractedCategory
        {
            get
            {
                if (isGroupingTypeBySupplier)
                {
                    return this.All(p => p.IsOfContractedSupplier == true);
                }
                else
                {
                    return null;
                }
            }
        }

        private readonly bool isGroupingTypeBySupplier;

        public CategoryForRequestView(TopCategory category, IEnumerable<ProductForRequestView> collection) : base(collection)
        {
            this.Name = category.Name;
            isGroupingTypeBySupplier = false;
        }

        public CategoryForRequestView(Supplier supplier, IEnumerable<ProductForRequestView> collection) : base(collection)
        {
            this.Name = supplier.ShortName;
            isGroupingTypeBySupplier = true;
        }
    }
}
