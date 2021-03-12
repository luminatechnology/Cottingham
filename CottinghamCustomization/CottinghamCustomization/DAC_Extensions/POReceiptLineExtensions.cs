// Decompiled with JetBrains decompiler
// Type: PX.Objects.PO.POReceiptLineExt
// Assembly: CottinghamCustomization, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F8F49B4-1D52-44D4-A89E-7C7E4BA7A8DF
// Assembly location: C:\Program Files\Acumatica ERP\Cottingham\Bin\CottinghamCustomization.dll

using PX.Data;
using PX.Data.BQL;
using System;

namespace PX.Objects.PO
{
  public class POReceiptLineExt : PXCacheExtension<POReceiptLine>
  {
    [PXDBDecimal(0)]
    [PXDefault(TypeCode.Decimal, "0.0")]
    [PXUIField(DisplayName = "Base Receipt Qty.", Enabled = false, Visible = false)]
    public Decimal? BaseReceiptQty { get; set; }

    public abstract class baseReceiptQty : BqlType<IBqlDecimal, Decimal>.Field<POReceiptLineExt.baseReceiptQty>
    {
    }
  }
}
