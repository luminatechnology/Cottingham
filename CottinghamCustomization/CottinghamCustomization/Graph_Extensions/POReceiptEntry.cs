// Decompiled with JetBrains decompiler
// Type: PX.Objects.PO.POReceiptEntry_Extension
// Assembly: CottinghamCustomization, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F8F49B4-1D52-44D4-A89E-7C7E4BA7A8DF
// Assembly location: C:\Program Files\Acumatica ERP\Cottingham\Bin\CottinghamCustomization.dll

using PX.Data;

namespace PX.Objects.PO
{
  public class POReceiptEntry_Extension : PXGraphExtension<POReceiptEntry>
  {
    [PXOverride]
    public void AddPurchaseOrder(
      POOrder order,
      POReceiptEntry_Extension.AddPurchaseOrderDelegate baseMethod)
    {
      if (order.VendorRefNbr != null)
        this.Base.Document.SetValueExt<POReceipt.invoiceNbr>(this.Base.Document.Current, (object) order.VendorRefNbr);
      baseMethod(order);
    }

    public delegate void AddPurchaseOrderDelegate(POOrder order);
  }
}
