using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using System.Text;

namespace Core.DBModels
{
    public class MatchProductExtraProperty: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        private Guid _id;
        [Key]
        public Guid Id
        {
            get { return _id; }
            set
            {
                _id = value;
                OnPropertyChanged("Id");
            }
        }

        private Guid _matchOfferId;
        public Guid MatchOfferId
        {
            get { return _matchOfferId; }
            set
            {
                _matchOfferId = value;
                OnPropertyChanged("MatchOfferId");
            }
        }

        private Guid _matchProductExtraPropertyTypeId;
        public Guid MatchProductExtraPropertyTypeId
        {
            get { return _matchProductExtraPropertyTypeId; }
            set
            {
                _matchProductExtraPropertyTypeId = value;
                OnPropertyChanged("MatchProductExtraPropertyTypeId");
            }
        }

        private MatchProductExtraPropertyType _matchProductExtraPropertyType;
        public MatchProductExtraPropertyType MatchProductExtraPropertyType
        {
            get { return _matchProductExtraPropertyType; }
            set
            {
                _matchProductExtraPropertyType = value;
                OnPropertyChanged("MatchProductExtraPropertyType");
            }
        }


        private string _value;
        public string Value
        {
            get { return _value; }
            set
            {
                _value = value;
                OnPropertyChanged("Value");
            }
        }




    }
}
