// Decompiled with JetBrains decompiler
// Type: PX.Objects.RQ.RQRequestEntry_Extension
// Assembly: CottinghamCustomization, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F8F49B4-1D52-44D4-A89E-7C7E4BA7A8DF
// Assembly location: C:\Program Files\Acumatica ERP\Cottingham\Bin\CottinghamCustomization.dll

using PX.Data;

namespace PX.Objects.RQ
{
  public class RQRequestEntry_Extension : PXGraphExtension<RQRequestEntry>
  {
    protected void RQRequest_ReqClassID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
    {
      RQRequest row = (RQRequest) e.Row;
      if (this.Base.vendor.Current == null)
        return;
      this.Base.Document.Cache.SetValueExt<RQRequest.curyID>((object) row, (object) this.Base.vendor.Current.CuryID);
    }
  }
}
