using Core.DBModels;
using Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace OperatorApp.Services
{
    public interface IDialogService
    {
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

        bool ShowWarningElementsRemoveDialog(List<Tuple<string,string>> elements);
    }
}
