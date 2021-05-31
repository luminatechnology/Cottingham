using System;
using PX.Data;
using PX.Objects.PO;

namespace PX.Objects.IN
{
    public class InventoryItemMaint_Extension : PXGraphExtension<InventoryItemMaint>
    {
        public const string VendInvtNonExist = "No Vendor Details / Default Value Not Set.";

        #region Event Handlers
        protected void _(Events.RowPersisting<InventoryItem> e, PXRowPersisting baseHandler)
        {
            baseHandler?.Invoke(e.Cache, e.Args);

            bool? invaild = Base.VendorItems.Current != null;

            foreach (POVendorInventory row in Base.VendorItems.Select())
            {
                invaild = (row.IsDefault ?? false) && (row.Active ?? false);

                if (invaild == true) { break; }
            }
        
            if (invaild == false)
            {
                throw new PXSetPropertyException(VendInvtNonExist);
            }
        }
        #endregion
    }
}