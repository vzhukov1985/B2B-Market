using Core.DBModels;
using Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace UpdateDb_Service.Models
{
    public static class Broadcaster
    {
        public static string GetDbDataStatus()
        {
            DbDataStatus status = MarketDbContext.GetDbDataStatusLocal();
            string res  = "";
            res += "<b>Состояние данных БД:</b>\n";
            if (status.IsConnected)
            {
                res += "<i>Есть соединение с БД</i>\n";
            }
            else
            {
                res = "<b>Нет соединения с БД</b>\n";
                return res;
            }
            if (status.UnMatchedProductCategories > 0)
            {
                res += $"Несопост. кат. товаров: <b>{status.UnMatchedProductCategories}</b>\n";
            }
            if (status.UnMatchedVolumeTypes > 0)
            {
                res += $"Несопост. типов об./вес: <b>{status.UnMatchedVolumeTypes}</b>\n";
            }
            if (status.UnMatchedVolumeUnits > 0)
            {
                res += $"Несопост. ед. изм. об./вес: <b>{status.UnMatchedVolumeUnits}</b>\n";
            }
            if (status.UnMatchedQuantityUnits > 0)
            {
                res += $"Несопост. ед. изм. кол-ва: <b>{status.UnMatchedQuantityUnits}</b>\n";
            }
            if (status.UnMatchedExtraProperties > 0)
            {
                res += $"Несопост. доп. св-в: <b>{status.UnMatchedExtraProperties}</b>\n";
            }
            if (status.UnMatchedOffers > 0)
            {
                res += $"Несопост. товаров: <b>{status.UnMatchedOffers}</b>\n";
            }
            
            if (status.ConflictedPics > 0)
            {
                res += $"Товаров с кофликт. изобр.: <b>{status.ConflictedPics}</b>\n";
            }
            if (status.ProductsWithoutPics > 0)
            {
                res += $"Товаров без изобр.: <b>{status.ProductsWithoutPics}</b>\n";
            }

            if (status.ConflictedDescs > 0)
            {
                res += $"Товаров с кофликт. опис.: <b>{status.ConflictedDescs}</b>\n";
            }
            if (status.ProductsWithoutDescs > 0)
            {
                res += $"Товаров без опис.: <b>{status.ProductsWithoutDescs}</b>\n";
            }

            return res;
        }
    }
}
