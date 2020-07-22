using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace OperatorApp.DialogsViewModels
{
    public class WarningElementsRemoveDlgVM:INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        private ObservableCollection<Tuple<string,string>> _elements;
        public ObservableCollection<Tuple<string, string>> Elements
        {
            get { return _elements; }
            set
            {
                _elements = value;
                OnPropertyChanged("Elements");
            }
        }


        public WarningElementsRemoveDlgVM(List<Tuple<string,string>> elements)
        {
            Elements = new ObservableCollection<Tuple<string, string>>(elements);
        }
    }
}
