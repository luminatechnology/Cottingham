using PX.Data;

namespace PX.Objects.RQ
{
    public class RQRequestClassMaint_Extension : PXGraphExtension<RQRequestClassMaint>
    {
        #region Event Handler
        protected void _(Events.RowSelected<RQRequestClass> e, PXRowSelected baseHandler)
        {
            baseHandler?.Invoke(e.Cache, e.Args);

            RQRequestClass row = (RQRequestClass)e.Row;
            
            if (row == null) { return; }

            PXUIFieldAttribute.SetEnabled<RQRequestClassExt.usrVendor>(e.Cache, row, (row.VendorNotRequest ?? false) == false && (row.VendorMultiply ?? false) == false);
        }
        #endregion
    }
}
