using PX.Data;

namespace PX.Objects.IN
{
    public class InventoryTranHistEnq_Extension : PXGraphExtension<InventoryTranHistEnq>
    {
        #region Cacche Attached
        [PXDBString(15, IsUnicode = true)]
        [PXUIField(DisplayName = "SO Shipment Nbr.", Visible = false, Visibility = PXUIVisibility.Visible)]
        [PXSelector(typeof(Search<SO.SOShipment.shipmentNbr>))]
        protected virtual void _(Events.CacheAttached<INTran.sOShipmentNbr> e) { }
        #endregion
    }
}
