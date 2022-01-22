// Decompiled with JetBrains decompiler
// Type: PX.Objects.AP.APInvoiceEntry_Extension
// Assembly: CottinghamCustomization, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F8F49B4-1D52-44D4-A89E-7C7E4BA7A8DF
// Assembly location: C:\Program Files\Acumatica ERP\Cottingham\Bin\CottinghamCustomization.dll

using PX.Data;
using PX.Objects.PO;

namespace PX.Objects.AP
{
  public class APInvoiceEntry_Extension : PXGraphExtension<APInvoiceEntry>
  {
    [PXOverride]
    public void InvoicePOOrder(
      POOrder order,
      bool createNew,
      bool keepOrderTaxes,
      APInvoiceEntry_Extension.InvoicePOOrderDelegate baseMethod)
    {
      if (createNew && order != null)
      {
        this.Base.Document.SetValueExt<APInvoice.invoiceNbr>(this.Base.Document.Current, (object) order.VendorRefNbr);
        this.Base.Document.SetValueExt<APInvoice.docDesc>(this.Base.Document.Current, (object) order.OrderDesc);
      }
      baseMethod(order, createNew, keepOrderTaxes);
    }

    public delegate void InvoicePOOrderDelegate(POOrder order, bool createNew, bool keepOrderTaxes);
  }
}
