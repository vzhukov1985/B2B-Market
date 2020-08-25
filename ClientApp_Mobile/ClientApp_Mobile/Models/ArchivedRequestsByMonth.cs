using Core.DBModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace ClientApp_Mobile.Models
{
    public class ArchivedRequestsByMonth:List<ArchivedRequest>, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        private string _month;
        public string Month
        {
            get { return _month; }
            set
            {
                _month = value;
                OnPropertyChanged("Month");
            }
        }

        public ArchivedRequestsByMonth(string month, IEnumerable<ArchivedRequest> archivedRequests):base(archivedRequests)
        {
            Month = month;
        }


    }
}
