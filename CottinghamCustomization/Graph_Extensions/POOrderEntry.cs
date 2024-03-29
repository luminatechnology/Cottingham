﻿using System;
using PX.Data;
using PX.Objects.IN;

namespace PX.Objects.PO
{
    public class POOrderEntry_Extension : PXGraphExtension<POOrderEntry>
    {
        public const string errorMeg = "Last Case Not Fully Packed";

        #region Event Handlers
        protected void _(Events.FieldSelecting<POLineExt.usrPackaging> e)
        {
            if (!(e.Row is POLine row) || !row.InventoryID.HasValue)
                return;
            e.ReturnValue = UOMConversion(Base, row.InventoryID.Value, row.OrderQty.Value, row.UOM);
        }

        protected void _(Events.FieldVerifying<POLine.orderQty> e)
        {
            POLine row = e.Row as POLine;
            string str = UOMConversion(Base, row.InventoryID.Value, (Decimal) e.NewValue, row.UOM);
            int startIndex = str.IndexOf("C") + 2;
            int num = str.IndexOf("B") - 1;
            if (!(str.Substring(startIndex, num - startIndex) != "0"))
                return;
            e.Cache.RaiseExceptionHandling<POLine.orderQty>(e.Row, e.NewValue, (Exception) new PXSetPropertyException(errorMeg, PXErrorLevel.Warning));
        }

        protected void _(Events.FieldVerifying<POLine.uOM> e)
        {
            POLine row = e.Row as POLine;
            if (!row.OrderQty.HasValue)
                return;
            string str = UOMConversion(Base, row.InventoryID.Value, row.OrderQty.Value, e.NewValue.ToString());
            int startIndex = str.IndexOf("C") + 2;
            int num = str.IndexOf("B") - 1;
            if (!(str.Substring(startIndex, num - startIndex) != "0"))
                return;
            e.Cache.RaiseExceptionHandling<POLine.orderQty>((object) row, (object) row.OrderQty, (Exception) new PXSetPropertyException(errorMeg, PXErrorLevel.Warning));
        }
        #endregion

        #region Static Methods        
        public static string UOMConversion(PXGraph graph, int inventoryID, decimal orderQty, string uOM)
        {
            int cases   = 0;
            int bottles = 0;

            INUnit inUnit = PXSelectBase<INUnit, PXSelectReadonly2<INUnit, InnerJoinSingleTable<InventoryItem, On<InventoryItem.inventoryID, Equal<INUnit.inventoryID>, 
                                                                                                                  And<InventoryItem.baseUnit, NotEqual<INUnit.fromUnit>>>>,
                                                                           Where<INUnit.inventoryID, Equal<Required<POLine.inventoryID>>>>.Config>.Select(graph, inventoryID);
            if (inUnit != null && orderQty != 0M)
            {
                if (uOM == inUnit.FromUnit)
                {
                    cases   = (int)orderQty;
                    bottles = 0;
                }
                else
                {
                    cases   = (int)(inUnit.UnitRate.HasValue ? new Decimal?(orderQty / inUnit.UnitRate.Value) : new Decimal?()).Value;
                    bottles = (int)(inUnit.UnitRate.HasValue ? new Decimal?(orderQty % inUnit.UnitRate.Value) : new Decimal?()).Value;
                }
            }

            return string.Format("{0} C {1} B", cases, bottles);
        }
        #endregion
    }
}
