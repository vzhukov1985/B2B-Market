using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Core.DBModels
{
    public class ArchivedClient : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        public Guid Id { get; set; }

        private string _shortName;
        public string ShortName
        {
            get { return _shortName; }
            set
            {
                _shortName = value;
                OnPropertyChanged("ShortName");
            }
        }

        public string FullName { get; set; }

        public string Bin { get; set; }

        public string Country { get; set; }

        public string City { get; set; }

        public string Address { get; set; }

        public string Phone { get; set; }

        public string Email { get; set; }

        public string ContactPersonName { get; set; }

        public string ContactPersonPhone { get; set; }

        public static ArchivedClient CloneForDb(ArchivedClient archivedClient)
        {
            return new ArchivedClient
            {
                Address = archivedClient.Address,
                Bin = archivedClient.Bin,
                City = archivedClient.City,
                Country = archivedClient.Country,
                Email = archivedClient.Email,
                FullName = archivedClient.FullName,
                Id = archivedClient.Id,
                Phone = archivedClient.Phone,
                ShortName = archivedClient.ShortName
            };
        }

        public ArchivedClient()
        {

        }
    }
}
