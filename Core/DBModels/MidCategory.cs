using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace Core.DBModels
{
    public class MidCategory: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        private Guid _id;
        public Guid Id
        {
            get { return _id; }
            set
            {
                _id = value;
                OnPropertyChanged("Id");
            }
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

        private Guid? _topCategoryId;
        public Guid? TopCategoryId
        {
            get { return _topCategoryId; }
            set
            {
                _topCategoryId = value;
                OnPropertyChanged("TopCategoryId");
            }
        }

        private TopCategory _topCategory;
        public TopCategory TopCategory
        {
            get { return _topCategory; }
            set
            {
                _topCategory = value;
                OnPropertyChanged("TopCategory");
            }
        }
    }
}
