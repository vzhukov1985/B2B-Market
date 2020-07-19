using Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace OperatorApp.Services
{
    public interface IDialogService
    {
        bool ShowOkCancelDialog(string text, string caption);

        QuantityUnit ShowEditQuantityUnitDialog(QuantityUnit quantityUnit);
        VolumeType ShowEditVolumeTypeDialog(VolumeType volumeType);
        VolumeUnit ShowEditVolumeUnitDialog(VolumeUnit volumeUnit);
        ProductExtraPropertyType ShowEditProductExtraPropertyTypeDialog(ProductExtraPropertyType productExtraPropertyType);

        bool ShowWarningMatchAndDeleteDialog(string SupplierName, string MatchUnit, string QuantityUnit, string DeleteUnit);
    }
}
