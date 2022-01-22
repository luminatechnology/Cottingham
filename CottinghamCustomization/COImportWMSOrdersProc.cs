using PX.Data;
using PX.Data.BQL.Fluent;
using CottinghamCustomization.DAC;

namespace CottinghamCustomization
{
    public class COImportWMSOrdersProc : PXGraph<COImportWMSOrdersProc>
    {
        public PXSave<COImportWMSOrders> Save;
        public PXCancel<COImportWMSOrders> Cancel;
        public PXFilter<PX.Objects.PO.POCreate.POCreateFilter> Filter;
        
        public SelectFrom<COImportWMSOrders>.Where<COImportWMSOrders.orderType.IsEqual<ImportOrderType.poReceipt>>.View Receipt;
        public SelectFrom<COImportWMSOrders>.Where<COImportWMSOrders.orderType.IsEqual<ImportOrderType.soShipment>>.View Shipment;
    }
}