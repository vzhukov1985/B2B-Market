using Core.DBModels;
using Core.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using TextCopy;

namespace OperatorApp.DialogsViewModels
{
    public class PositionDependenciesDlgVM<CommandType> : INotifyPropertyChanged where CommandType : IRelayCommand, new()
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }


        private List<string> _positions;
        public List<string> Positions
        {
            get { return _positions; }
            set
            {
                _positions = value;
                OnPropertyChanged("Positions");
            }
        }

        private string _selectedPositionName;
        public string SelectedPositionName
        {
            get { return _selectedPositionName; }
            set
            {
                _selectedPositionName = value;
                OnPropertyChanged("SelectedPositionName");
            }
        }

        public CommandType CopyToClipboardCommand { get; }
        public CommandType FindPositionCommand { get; }

        public PositionDependenciesDlgVM()
        {
            CopyToClipboardCommand = new CommandType();
            CopyToClipboardCommand.Create(_ => ClipboardService.SetText(SelectedPositionName), _ => SelectedPositionName != null);
        }

        public PositionDependenciesDlgVM(MatchQuantityUnit qu):this()
        {
            using (MarketDbContext db = new MarketDbContext())
            {
                Positions = db.MatchOffers.Where(of => of.MatchQuantityUnitId == qu.Id).Select(of => $"{of.ProductName}").ToList();
            }
        }
        public PositionDependenciesDlgVM(QuantityUnit qu) : this()
        {
            using (MarketDbContext db = new MarketDbContext())
            {
                Positions = db.MatchQuantityUnits.Where(d => d.QuantityUnitId == qu.Id).Select(d => $"\"{d.SupplierQUShortName}\"-\"{d.SupplierQUFullName}\" ({d.Supplier.ShortName})").ToList();
            }
        }
        public PositionDependenciesDlgVM(MatchVolumeType vt) : this()
        {
            using (MarketDbContext db = new MarketDbContext())
            {
                Positions = db.MatchOffers.Where(of => of.MatchVolumeTypeId == vt.Id).Select(of => $"{of.ProductName}").ToList();
            }
        }
        public PositionDependenciesDlgVM(VolumeType vt) : this()
        {
            using (MarketDbContext db = new MarketDbContext())
            {
                Positions = db.MatchVolumeTypes.Where(d => d.VolumeTypeId == vt.Id).Select(d => $"\"{d.SupplierVolumeTypeName}\" ({d.Supplier.ShortName})").ToList();
            }
        }
        public PositionDependenciesDlgVM(MatchVolumeUnit vu) : this()
        {
            using (MarketDbContext db = new MarketDbContext())
            {
                Positions = db.MatchOffers.Where(of => of.MatchVolumeUnitId == vu.Id).Select(of => $"{of.ProductName}").ToList();
            }
        }
        public PositionDependenciesDlgVM(VolumeUnit vu) : this()
        {
            using (MarketDbContext db = new MarketDbContext())
            {
                Positions = db.MatchVolumeUnits.Where(of => of.VolumeUnitId == vu.Id).Select(d => $"\"{d.SupplierVUShortName}\"-\"{d.SupplierVUFullName}\" ({d.Supplier.ShortName})").ToList();
            }
        }
        public PositionDependenciesDlgVM(MatchProductExtraPropertyType smpept) : this()
        {
            using (MarketDbContext db = new MarketDbContext())
            {
                Positions = db.MatchOffers.Where(of => of.MatchProductExtraProperties.Select(mpep => mpep.MatchProductExtraPropertyTypeId).Any(mpept => mpept == smpept.Id)).Select(of => $"{of.ProductName}").ToList();
            }
        }
        public PositionDependenciesDlgVM(ProductExtraPropertyType smpept) : this()
        {
            using (MarketDbContext db = new MarketDbContext())
            {
                Positions = db.MatchProductExtraPropertyTypes.Where(mpept => mpept.ProductExtraPropertyTypeId == smpept.Id).Select(of => $"{of.SupplierProductExtraPropertyTypeName} ({of.Supplier.ShortName})").ToList();
            }
        }
        public PositionDependenciesDlgVM(MatchProductCategory pc) : this()
        {
            using (MarketDbContext db = new MarketDbContext())
            {
                Positions = db.MatchOffers.Where(of => of.MatchProductCategoryId == pc.Id).Select(of => $"{of.ProductName}").ToList();
            }
        }
        public PositionDependenciesDlgVM(ProductCategory pc) : this()
        {
            using (MarketDbContext db = new MarketDbContext())
            {
                Positions = db.MatchProductCategories.Where(mpc => mpc.ProductCategoryId == pc.Id).Select(mpc => $"{mpc.SupplierProductCategoryName} ({mpc.Supplier.ShortName})").ToList();
            }
        }
    }
}
