using Core.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Core.DBModels
{
    public class TopCategory: INotifyPropertyChanged
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

        private async void LoadPictureAsync()
        {
            Picture = await HTTPManager.GetTopCategoryPictureAsync(Id);
        }

        private byte[] _picture;
        [NotMapped]
        public byte[] Picture
        {
            get
            {
                if (_picture == null)
                {
                    LoadPictureAsync();
                    return null;
                }

                if (_picture.SequenceEqual(Encoding.ASCII.GetBytes("NoPicture")))
                    return null;

                return _picture;
            }
            set
            {
                _picture = value;
                OnPropertyChanged("Picture");
            }
        }
    }
}
