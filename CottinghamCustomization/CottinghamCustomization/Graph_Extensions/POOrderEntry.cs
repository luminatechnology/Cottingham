// Decompiled with JetBrains decompiler
// Type: PX.Objects.PO.POOrderEntry_Extension
// Assembly: CottinghamCustomization, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F8F49B4-1D52-44D4-A89E-7C7E4BA7A8DF
// Assembly location: C:\Program Files\Acumatica ERP\Cottingham\Bin\CottinghamCustomization.dll

using PX.Data;
using PX.Objects.IN;
using System;

namespace PX.Objects.PO
{
  public class POOrderEntry_Extension : PXGraphExtension<POOrderEntry>
  {
    public const string errorMeg = "Last Case Not Fully Packed";

    protected void _(Events.FieldSelecting<POLineExt.usrPackaging> e)
    {
      if (!(e.Row is POLine row) || !row.InventoryID.HasValue)
        return;
      e.ReturnValue = (object) this.UOMConversion(row.InventoryID.Value, row.OrderQty.Value, row.UOM);
    }

    protected void _(Events.FieldVerifying<POLine.orderQty> e)
    {
      POLine row = e.Row as POLine;
      string str = this.UOMConversion(row.InventoryID.Value, (Decimal) e.NewValue, row.UOM);
      int startIndex = str.IndexOf("C") + 2;
      int num = str.IndexOf("B") - 1;
      if (!(str.Substring(startIndex, num - startIndex) != "0"))
        return;
      e.Cache.RaiseExceptionHandling<POLine.orderQty>(e.Row, e.NewValue, (Exception) new PXSetPropertyException("Last Case Not Fully Packed", PXErrorLevel.Warning));
    }

    protected void _(Events.FieldVerifying<POLine.uOM> e)
    {
      POLine row = e.Row as POLine;
      if (!row.OrderQty.HasValue)
        return;
      string str = this.UOMConversion(row.InventoryID.Value, row.OrderQty.Value, e.NewValue.ToString());
      int startIndex = str.IndexOf("C") + 2;
      int num = str.IndexOf("B") - 1;
      if (!(str.Substring(startIndex, num - startIndex) != "0"))
        return;
      e.Cache.RaiseExceptionHandling<POLine.orderQty>((object) row, (object) row.OrderQty, (Exception) new PXSetPropertyException("Last Case Not Fully Packed", PXErrorLevel.Warning));
    }

    public string UOMConversion(int inventoryID, Decimal orderQty, string uOM)
    {
      int num1 = 0;
      int num2 = 0;
      INUnit inUnit = (INUnit) PXSelectBase<INUnit, PXSelectReadonly2<INUnit, InnerJoinSingleTable<PX.Objects.IN.InventoryItem, On<PX.Objects.IN.InventoryItem.inventoryID, Equal<INUnit.inventoryID>, And<PX.Objects.IN.InventoryItem.baseUnit, NotEqual<INUnit.fromUnit>>>>, Where<INUnit.inventoryID, Equal<Required<POLine.inventoryID>>>>.Config>.Select(new PXGraph(), (object) inventoryID);
      if (inUnit != null && orderQty != 0M)
      {
        if (uOM.Equals(inUnit.FromUnit))
        {
          num2 = (int) orderQty;
          num1 = 0;
        }
        else
        {
          Decimal num3 = orderQty;
          Decimal? unitRate = inUnit.UnitRate;
          num2 = (int) (unitRate.HasValue ? new Decimal?(num3 / unitRate.GetValueOrDefault()) : new Decimal?()).Value;
          Decimal num4 = orderQty;
          unitRate = inUnit.UnitRate;
          num1 = (int) (unitRate.HasValue ? new Decimal?(num4 % unitRate.GetValueOrDefault()) : new Decimal?()).Value;
        }
      }
      return string.Format("{0} C {1} B", (object) num2, (object) num1);
    }
  }
}
