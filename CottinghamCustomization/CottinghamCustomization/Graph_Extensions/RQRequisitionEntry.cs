using PX.Data;
using PX.Objects.PO;

namespace PX.Objects.RQ
{
    public class RQRequisitionEntry_Extension : PXGraphExtension<PX.Objects.RQ.RQRequisitionEntry>
    {
        protected void _(Events.FieldSelecting<RQRequisitionLineExt.usrPackaging> e)
        {
            if (!(e.Row is RQRequisitionLine row) || !row.InventoryID.HasValue) { return; }

            e.ReturnValue = POOrderEntry_Extension.UOMConversion(Base, row.InventoryID.Value, row.OrderQty.Value, row.UOM);
        }
    }
}