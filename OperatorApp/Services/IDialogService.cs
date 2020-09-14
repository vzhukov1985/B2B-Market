using Core.DBModels;
using Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace OperatorApp.Services
{
    public interface IDialogService
    {
        string FilePath { get; set; }

        bool ShowOkCancelDialog(string text, string caption);
        void ShowMessageDialog(string text, string caption);

        List<ElementField> ShowAddEditElementDlg(List<ElementField> fields, bool isEditing);
        bool ShowMatchOfferDlg(MatchOffer matchOffer,
            Offer offer,
            List<ProductCategory> availableCategories,
            List<VolumeType> availableVolumeTypes,
            List<VolumeUnit> availableVolumeUnits,
            List<ProductExtraPropertyType> availableProductExtraPropertyTypes,
            List<QuantityUnit> availableQuantityUnits);

        string ShowPositionOffers(MatchQuantityUnit qu);
        string ShowPositionOffers(QuantityUnit qu);
        string ShowPositionOffers(MatchVolumeType vt);
        string ShowPositionOffers(VolumeType vt);
        string ShowPositionOffers(MatchVolumeUnit vu);
        string ShowPositionOffers(VolumeUnit vu);
        string ShowPositionOffers(MatchProductExtraPropertyType pept);
        string ShowPositionOffers(ProductExtraPropertyType pept);
        string ShowPositionOffers(MatchProductCategory pc);
        string ShowPositionOffers(ProductCategory pc);

        bool ShowWarningElementsRemoveDialog(List<Tuple<string,string>> elements);

        bool ShowOpenPictureDialog();
    }
}
