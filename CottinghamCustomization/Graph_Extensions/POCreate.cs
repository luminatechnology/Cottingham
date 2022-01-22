using System;
using System.Collections.Generic;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.SO;
using static PX.Objects.PO.POCreate;

namespace PX.Objects.PO
{
    public class POCreate_Extension : PXGraphExtension<POCreate>
    {
        #region Event Handlers
        /// <summary>
        /// Override this event handler to define another new logical static method.
        /// </summary>
        /// <param name="e"></param>
        /// <param name="baseHandler"></param>
        protected void _(Events.RowSelected<POCreateFilter> e, PXRowSelected baseHandler)
        {
            POCreateFilter filter = Base.Filter.Current;

            if (filter == null) return;

            Base.FixedDemand.SetProcessDelegate(delegate (List<POFixedDemand> list)
            {
                CreateProc2(list, filter.PurchDate, filter.OrderNbr != null);
            });

            TimeSpan span;
            Exception message;
            PXLongRunStatus status = PXLongOperation.GetStatus(Base.UID, out span, out message);

            PXUIFieldAttribute.SetVisible<POLine.orderNbr>(Base.Caches[typeof(POLine)], null, (status == PXLongRunStatus.Completed || status == PXLongRunStatus.Aborted));
            PXUIFieldAttribute.SetVisible<POCreateFilter.orderTotal>(e.Cache, null, filter.VendorID != null);
        }
        #endregion

        #region Static Methods
        /// <summary>
        /// Copy standard method and add one new customizaed method.
        /// </summary>
        /// <param name="list"></param>
        /// <param name="orderDate"></param>
        /// <param name="extSort"></param>
        public static void CreateProc2(List<POFixedDemand> list, DateTime? orderDate, bool extSort)
        {
            PXRedirectRequiredException poredirect = CreatePOOrders(list, orderDate, extSort);

            CopyNoteOrFile2PO(list);

            if (poredirect != null) { throw poredirect; }
        }

        /// <summary>
        /// Copy notes or attchments from SO to PO by configuration of sales order type.
        /// </summary>
        /// <param name="list"></param>
        public static void CopyNoteOrFile2PO(List<POFixedDemand> list)
        {
            POOrderEntry orderEntry = PXGraph.CreateInstance<POOrderEntry>();

            foreach (POFixedDemand demand in list)
            {
                PXResult<SOOrder, SOOrderType> result = (PXResult<SOOrder, SOOrderType>)SelectFrom<SOOrder>.InnerJoin<SOOrderType>.On<SOOrderType.orderType.IsEqual<SOOrder.orderType>>
                                                                                                       .Where<SOOrder.noteID.IsEqual<@P.AsGuid>>.View.SelectSingleBound(orderEntry, null, demand.RefNoteID);
                SOOrder     sOOrder   = result;
                SOOrderType sOOrdType = result;

                SOLineSplit lineSplit = SelectFrom<SOLineSplit>.Where<SOLineSplit.orderType.IsEqual<@P.AsString>
                                                                      .And<SOLineSplit.orderNbr.IsEqual<@P.AsString>
                                                                           .And<SOLineSplit.planID.IsEqual<@P.AsInt>>>>.View.Select(orderEntry, sOOrder.OrderType, sOOrder.OrderNbr, demand.PlanID).TopFirst;

                if (lineSplit != null)
                {
                    orderEntry.CurrentDocument.Current = POOrder.PK.Find(orderEntry, lineSplit.POType, lineSplit.PONbr);

                    PXNoteAttribute.CopyNoteAndFiles(orderEntry.Caches[typeof(SOOrder)], sOOrder,
                                                     orderEntry.CurrentDocument.Cache, orderEntry.CurrentDocument.Current,
                                                     sOOrdType.GetExtension<SOOrderTypeExt>().UsrCopyNotesToPO ?? false, sOOrdType.GetExtension<SOOrderTypeExt>().UsrCopyFilesToPO ?? false);  
                }
            }

            orderEntry.Save.Press();
        }
        #endregion
    }
}