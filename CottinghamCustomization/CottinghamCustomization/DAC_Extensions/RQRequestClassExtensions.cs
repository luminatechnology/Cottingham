// Decompiled with JetBrains decompiler
// Type: PX.Objects.RQ.RQRequestClassExt
// Assembly: CottinghamCustomization, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F8F49B4-1D52-44D4-A89E-7C7E4BA7A8DF
// Assembly location: C:\Program Files\Acumatica ERP\Cottingham\Bin\CottinghamCustomization.dll

using PX.Data;
using PX.Data.BQL;
using PX.Objects.AP;

namespace PX.Objects.RQ
{
  public class RQRequestClassExt : PXCacheExtension<RQRequestClass>
  {
    [VendorNonEmployeeActive(CacheGlobal = true, DescriptionField = typeof (Vendor.acctName), Filterable = true, Visibility = PXUIVisibility.SelectorVisible)]
    [PXUIField(DisplayName = "Default Vendor")]
    public virtual int? UsrVendor { get; set; }

    public abstract class usrVendor : BqlType<IBqlInt, int>.Field<RQRequestClassExt.usrVendor>
    {
    }
  }
}
