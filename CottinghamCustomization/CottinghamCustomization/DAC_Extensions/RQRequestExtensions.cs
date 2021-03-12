// Decompiled with JetBrains decompiler
// Type: PX.Objects.RQ.RQ_RQRequest_ExistingColumn
// Assembly: CottinghamCustomization, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F8F49B4-1D52-44D4-A89E-7C7E4BA7A8DF
// Assembly location: C:\Program Files\Acumatica ERP\Cottingham\Bin\CottinghamCustomization.dll

using PX.Data;
using PX.Objects.AP;

namespace PX.Objects.RQ
{
  [PXNonInstantiatedExtension]
  public class RQ_RQRequest_ExistingColumn : PXCacheExtension<RQRequest>
  {
    [PXDefault(typeof (Search<RQRequestClassExt.usrVendor, Where<RQRequestClass.reqClassID, Equal<Current<RQRequest.reqClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
    [PXFormula(typeof (Default<RQRequest.reqClassID>))]
    [VendorNonEmployeeActive(CacheGlobal = true, DescriptionField = typeof (Vendor.acctName), Filterable = true, Visibility = PXUIVisibility.SelectorVisible)]
    public int? VendorID { get; set; }
  }
}
