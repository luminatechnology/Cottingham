using PX.Data;

namespace PX.Objects.RQ
{
    public class RQRequestClassMaint_Extension : PXGraphExtension<RQRequestClassMaint>
    {
        #region Event Handler
        protected void RQRequestClass_RowSelected(PXCache cache, PXRowSelectedEventArgs e, PXRowSelected InvokeBaseHandler)
        {
            if (InvokeBaseHandler != null)
                InvokeBaseHandler(cache, e);

            RQRequestClass row = (RQRequestClass)e.Row;
            
            if (row == null) { return; }
                
            PXCache cache1 = cache;
            RQRequestClass rqRequestClass = row;
            bool? nullable = row.VendorNotRequest;
            bool flag1 = false;
            int num;
            if (nullable.GetValueOrDefault() == flag1 & nullable.HasValue)
            {
                nullable = row.VendorMultiply;
                bool flag2 = false;
                num = nullable.GetValueOrDefault() == flag2 & nullable.HasValue ? 1 : 0;
            }
            else
                num = 0;
            PXUIFieldAttribute.SetEnabled<RQRequestClassExt.usrVendor>(cache1, (object)rqRequestClass, num != 0);
        }
        #endregion
    }
}
