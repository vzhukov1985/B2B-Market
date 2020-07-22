using Core.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace OperatorApp.DialogsViewModels
{
    public class AddEditElementDlgVM: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        private ObservableCollection<ElementField> _fields;
        public ObservableCollection<ElementField> Fields
        {
            get { return _fields; }
            set
            {
                _fields = value;
                OnPropertyChanged("Fields");
            }
        }

        private string _caption;
        public string Caption
        {
            get { return _caption; }
            set
            {
                _caption = value;
                OnPropertyChanged("Caption");
            }
        }

        private string _header;
        public string Header
        {
            get { return _header; }
            set
            {
                _header = value;
                OnPropertyChanged("Header");
            }
        }



        public AddEditElementDlgVM(List<ElementField> fields, bool isEditing)
        {
            Fields = new ObservableCollection<ElementField>(fields);
            if (isEditing)
            {
                Caption = "Редактирование";
                Header = "Редактирование элемента";
            }
            else
            {
                Caption = "Добавление";
                Header = "Добавление нового элемента";
            }

        }

    }
}
