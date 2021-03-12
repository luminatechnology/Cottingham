// Decompiled with JetBrains decompiler
// Type: PX.Objects.SO.SOOrderEntry_Extension
// Assembly: CottinghamCustomization, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F8F49B4-1D52-44D4-A89E-7C7E4BA7A8DF
// Assembly location: C:\Program Files\Acumatica ERP\Cottingham\Bin\CottinghamCustomization.dll

using PX.Data;
using System;

namespace PX.Objects.SO
{
  public class SOOrderEntry_Extension : PXGraphExtension<SOOrderEntry>
  {
    protected void _(Events.RowPersisting<SOLine> e, PXRowPersisting invokeBaseHandler)
    {
      if (invokeBaseHandler != null)
        invokeBaseHandler(e.Cache, e.Args);
      if (e.Row == null || e.Operation.Command() == PXDBOperation.Delete)
        return;
      PXDefaultAttribute.SetPersistingCheck<SOLine.reasonCode>(e.Cache, (object) e.Row, e.Row.IsFree.Value ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
    }

    protected virtual void _(Events.FieldUpdated<SOLine.isFree> e)
    {
      SOLine row = e.Row as SOLine;
      if (!(bool) e.NewValue || !string.IsNullOrEmpty(row.ReasonCode))
        return;
      this.Base.Transactions.Cache.RaiseExceptionHandling<SOLine.reasonCode>((object) row, e.NewValue, (Exception) new PXSetPropertyException("This field is mandatory", PXErrorLevel.Warning));
    }
  }
}
