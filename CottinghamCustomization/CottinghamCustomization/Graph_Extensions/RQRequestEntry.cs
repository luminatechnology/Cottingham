using System.Linq;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.PO;

namespace PX.Objects.RQ
{
    public class RQRequestEntry_Extension : PXGraphExtension<RQRequestEntry>
    {
        #region Event Handlers        
        protected void _(Events.FieldUpdated<RQRequestLine.inventoryID> e, PXFieldUpdated baseHandler)
        {
            baseHandler?.Invoke(e.Cache, e.Args);

            var vi = SelectFrom<POVendorInventory>.Where<POVendorInventory.active.IsEqual<True>.And<POVendorInventory.inventoryID.IsEqual<@P.AsInt>>>.View.Select(Base, e.NewValue).TopFirst;

            Base.Document.Cache.SetValueExt<RQRequest.vendorID>(Base.Document.Current, vi?.IsDefault == true ? vi?.VendorID : Base.reqclass.Current.GetExtension<RQRequestClassExt>().UsrVendor);

            if (Base.vendor.Current != null)
            {
                Base.Document.Cache.SetValueExt<RQRequest.curyID>(Base.Document.Current, Base.vendor.Current.CuryID);
            }
        }
        #endregion
    }
}
