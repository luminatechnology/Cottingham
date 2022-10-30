using System;
using System.Linq;
using System.Collections.Generic;
using PX.Data;
using PX.Objects.IN;

namespace PX.Objects.AR
{
    public class ARSalesPriceMaint_Extension : PXGraphExtension<ARSalesPriceMaint>
    {
        #region Event Handlers
        public delegate void PersistDelegate();
        [PXOverride]
        public void Persist(PersistDelegate baseMethod)
        {
            List<ValueTuple<string, decimal?>> sources = new List<(string, decimal?)>();

            foreach (ARSalesPrice price in Base.Records.Cache.Inserted.RowCast<ARSalesPrice>().Where(w => w.PriceCode == "WS" && (w.ExpirationDate == null || w.ExpirationDate >= Base.Accessinfo.BusinessDate)))
            {
                sources.Add(ValueTuple.Create(InventoryItem.PK.Find(Base, price.InventoryID).InventoryCD, price.SalesPrice));
            }

            baseMethod();

            for (int i = 0; i < sources.Count; i++)
            {
                using (new PXLoginScope($"{Base.Accessinfo.UserName}@{InventoryItemMaint_Extension.DestinationTen}"))
                {
                    PXUpdate<Set<InventoryItem.basePrice, Required<InventoryItem.basePrice>>,
                             InventoryItem,
                             Where<InventoryItem.inventoryCD, Equal<Required<InventoryItem.inventoryCD>>,
                                   And<InventoryItem.basePrice, Equal<CS.decimal0>>>>
                             .Update(new PXGraph(), sources[i].Item2, sources[i].Item1);
                }
            }
        }
        #endregion
    }
}