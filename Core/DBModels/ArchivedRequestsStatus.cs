using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;
using System.Text;

namespace Core.DBModels
{
    public class ArchivedRequestsStatus: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        private Guid _archivedRequestId;
        public Guid ArchivedRequestId
        {
            get { return _archivedRequestId; }
            set
            {
                _archivedRequestId = value;
                OnPropertyChanged("ArchivedRequestId");
            }
        }

        private ArchivedRequest _archivedRequest;
        public ArchivedRequest ArchivedRequest
        {
            get { return _archivedRequest; }
            set
            {
                _archivedRequest = value;
                if (_archivedRequest != null)
                    ArchivedRequestId = _archivedRequest.Id;
                OnPropertyChanged("ArchivedRequest");
            }
        }

        private Guid _archivedRequestStatusTypeId;
        public Guid ArchivedRequestStatusTypeId
        {
            get { return _archivedRequestStatusTypeId; }
            set
            {
                _archivedRequestStatusTypeId = value;
                OnPropertyChanged("ArchivedRequestStatusTypeId");
            }
        }

        private ArchivedRequestStatusType _archivedRequestStatusType;
        public ArchivedRequestStatusType ArchivedRequestStatusType
        {
            get { return _archivedRequestStatusType; }
            set
            {
                _archivedRequestStatusType = value;
                if (_archivedRequestStatusType != null)
                    ArchivedRequestStatusTypeId = _archivedRequestStatusType.Id;
                OnPropertyChanged("ArchivedRequestStatusType");
            }
        }

        private DateTime _dateTime;
        public DateTime DateTime
        {
            get { return _dateTime; }
            set
            {
                _dateTime = value;
                OnPropertyChanged("DateTime");
            }
        }

        public static ArchivedRequestsStatus CloneForDb(ArchivedRequestsStatus status)
        {
            return new ArchivedRequestsStatus
            {
                ArchivedRequestId = status.ArchivedRequestId,
                ArchivedRequestStatusTypeId = status.ArchivedRequestStatusTypeId,
                DateTime = status.DateTime
            };
        }
    }
}
