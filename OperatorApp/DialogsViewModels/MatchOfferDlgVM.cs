using Core.DBModels;
using Core.Services;
using OperatorApp.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace OperatorApp.DialogsViewModels
{
    public class MatchOfferDlgVM<CommandType> : INotifyPropertyChanged where CommandType : IRelayCommand, new()
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        private readonly IDialogService DialogService;

        private MatchOffer _matchOffer;
        public MatchOffer MatchOffer
        {
            get { return _matchOffer; }
            set
            {
                _matchOffer = value;
                OnPropertyChanged("MatchOffer");
            }
        }

        private Offer _offer;
        public Offer Offer
        {
            get { return _offer; }
            set
            {
                _offer = value;
                OnPropertyChanged("Offer");
            }
        }


        private ObservableCollection<ProductCategory> _availableCategories;
        public ObservableCollection<ProductCategory> AvailableCategories
        {
            get { return _availableCategories; }
            set
            {
                _availableCategories = value;
                OnPropertyChanged("AvailableCategories");
            }
        }
        private ObservableCollection<VolumeType> _availableVolumeTypes;
        public ObservableCollection<VolumeType> AvailableVolumeTypes
        {
            get { return _availableVolumeTypes; }
            set
            {
                _availableVolumeTypes = value;
                OnPropertyChanged("AvailableVolumeTypes");
            }
        }
        private ObservableCollection<VolumeUnit> _availableVolumeUnits;
        public ObservableCollection<VolumeUnit> AvailableVolumeUnits
        {
            get { return _availableVolumeUnits; }
            set
            {
                _availableVolumeUnits = value;
                OnPropertyChanged("AvailableVolumeUnits");
            }
        }

        private ObservableCollection<ProductExtraPropertyType> _availableProductExtraPropertyTypes;
        public ObservableCollection<ProductExtraPropertyType> AvailableProductExtraPropertyTypes
        {
            get { return _availableProductExtraPropertyTypes; }
            set
            {
                _availableProductExtraPropertyTypes = value;
                OnPropertyChanged("AvailableProductExtraPropertyTypes");
            }
        }

        private ObservableCollection<QuantityUnit> _availableQuantityUnits;
        public ObservableCollection<QuantityUnit> AvailableQuantityUnits
        {
            get { return _availableQuantityUnits; }
            set
            {
                _availableQuantityUnits = value;
                OnPropertyChanged("AvailableQuantityUnits");
            }
        }

        private void AddNewProperty(MatchProductExtraProperty matchProperty)
        {
            ProductExtraProperty newProperty = new ProductExtraProperty
            {
                Id = Guid.NewGuid(),
                Product = Offer.Product,
                PropertyType = matchProperty.MatchProductExtraPropertyTypeId == null ? null : AvailableProductExtraPropertyTypes.Where(apept => apept.Id == matchProperty.MatchProductExtraPropertyType.ProductExtraPropertyTypeId).FirstOrDefault(),
                Value = matchProperty.Value,
            };

            bool proceedAdding = true;
            foreach (ProductExtraProperty existingProperty in Offer.Product.ExtraProperties)
            {
                if (existingProperty.PropertyType == newProperty.PropertyType)
                    proceedAdding = false;
            }
            if (proceedAdding)
            {
                Offer.Product.ExtraProperties.Add(newProperty);
            }
            else
            {
                DialogService.ShowMessageDialog("Невозможно добавить. Свойство с таким типом уже существует.", "Ошибка");
            }
        }

        private void RemoveProductProperty(ProductExtraProperty property)
        {
            if (DialogService.ShowOkCancelDialog("ВНИМАНИЕ!!! Дополнительное свойство \""+property.PropertyType.Name+": "+ property.Value+"\" будет удалено!", "ВНИМАНИЕ!!!"))
                Offer.Product.ExtraProperties.Remove(property);
        }


        public CommandType AddNewPropertyCommand { get; }
        public CommandType RemoveProductPropertyCommand { get; }

        public CommandType OkCommand { get; }

        public MatchOfferDlgVM(
            MatchOffer matchOffer,
            Offer offer,
            IDialogService dialogService,
            List<ProductCategory> availableCategories,
            List<VolumeType> availableVolumeTypes,
            List<VolumeUnit> availableVolumeUnits,
            List<ProductExtraPropertyType> availableProductExtraPropertyTypes,
            List<QuantityUnit> availableQuantityUnits
            )
        {
            MatchOffer = matchOffer;
            Offer = offer;
            DialogService = dialogService;

            AddNewPropertyCommand = new CommandType();
            AddNewPropertyCommand.Create(o => AddNewProperty((MatchProductExtraProperty)o));

            RemoveProductPropertyCommand = new CommandType();
            RemoveProductPropertyCommand.Create(o => RemoveProductProperty((ProductExtraProperty)o));

            OkCommand = new CommandType();
            OkCommand.Create(_ => { }, _ => {
                bool canProceed = true;
                foreach (ProductExtraProperty pep in Offer.Product.ExtraProperties)
                {
                    if (pep.PropertyType == null) canProceed = false;
                }
                if (Offer.Product.Name == "" && Offer.Product.Category == null && Offer.Product.Volume == 0 && Offer.Product.VolumeType == null && Offer.Product.VolumeUnit == null && Offer.QuantityUnit == null)
                    canProceed = false;
                return canProceed;
                });

            AvailableCategories = new ObservableCollection<ProductCategory>(availableCategories);
            AvailableVolumeTypes = new ObservableCollection<VolumeType>(availableVolumeTypes);
            AvailableVolumeUnits = new ObservableCollection<VolumeUnit>(availableVolumeUnits);
            AvailableProductExtraPropertyTypes = new ObservableCollection<ProductExtraPropertyType>(availableProductExtraPropertyTypes);
            AvailableQuantityUnits = new ObservableCollection<QuantityUnit>(availableQuantityUnits);
        }

    }
}
