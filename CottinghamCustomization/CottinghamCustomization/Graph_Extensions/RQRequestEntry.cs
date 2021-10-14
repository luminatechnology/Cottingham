using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AP;
using PX.Objects.CM;
using PX.Objects.PO;

namespace PX.Objects.RQ
{
    public class RQRequestEntry_Extension : PXGraphExtension<RQRequestEntry>
    {
        #region Event Handlers        
        protected void _(Events.FieldUpdated<RQRequestLine.inventoryID> e, PXFieldUpdated baseHandler)
        {
            var vi = SelectFrom<POVendorInventory>.Where<POVendorInventory.active.IsEqual<True>.And<POVendorInventory.inventoryID.IsEqual<@P.AsInt>>>.View.Select(Base, e.NewValue).TopFirst;

            Base.Document.Cache.SetValueExt<RQRequest.vendorID>(Base.Document.Current, vi?.IsDefault == true ? vi?.VendorID : Base.reqclass.Current.GetExtension<RQRequestClassExt>().UsrVendor);

            if (Base.vendor.Current != null)
            {
                Base.Document.Cache.SetValueExt<RQRequest.curyID>(Base.Document.Current, Base.vendor.Current.CuryID);
            }

            baseHandler?.Invoke(e.Cache, e.Args);
        }

        protected void _(Events.FieldDefaulting<RQRequestLine.curyEstUnitCost> e, PXFieldDefaulting baseHandler)
        {
            baseHandler?.Invoke(e.Cache, e.Args);

            RQRequestLine row = e.Row as RQRequestLine;

            if (row?.ManualPrice == true && (row?.InventoryID == null || row?.VendorID == null))
            {
                e.NewValue = row.CuryEstUnitCost ?? 0m;
                
                return;
            }

            if (row.UOM != null)
            {
                CurrencyInfo curyInfo = Base.currencyinfo.Search<CurrencyInfo.curyInfoID>(row.CuryInfoID);

                decimal? vendorUnitCost = APVendorPriceMaint.CalculateUnitCost(e.Cache, row.VendorID, row.VendorLocationID, row.InventoryID, Base.Document.Current.SiteID,
                                                                               curyInfo, row.UOM, row.OrderQty, Base.Document.Current.OrderDate.Value, row.CuryEstUnitCost);

                e.NewValue = vendorUnitCost;
            }
        }
        #endregion
    }
}
