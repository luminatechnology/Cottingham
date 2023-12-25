using System;
using System.Collections.Generic;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.SO;

namespace PX.Objects.PO
{
    public class POCreate_Extension : PXGraphExtension<POCreate>
    {
        //#region Event Handlers
        ///// <summary>
        ///// Override this event handler to define another new logical static method.
        ///// </summary>
        //protected void _(Events.RowSelected<POCreate.POCreateFilter> e, PXRowSelected baseHandler)
        //{
        //    //baseHandler?.Invoke(e.Cache, e.Args);

        //    POCreate.POCreateFilter filter = Base.Filter.Current;

        //    if (filter == null) return;

        //    Base.FixedDemand.SetProcessDelegate(delegate (List<POFixedDemand> list)
        //    {
        //        CreateProc2(list, filter.PurchDate, filter.OrderNbr != null, filter.BranchID);
        //    });

        //    PXLongRunStatus status = PXLongOperation.GetStatus(Base.UID, out TimeSpan span, out Exception message);

        //    PXUIFieldAttribute.SetVisible<POLine.orderNbr>(Base.Caches[typeof(POLine)], null, (status == PXLongRunStatus.Completed || status == PXLongRunStatus.Aborted));
        //    PXUIFieldAttribute.SetVisible<POCreate.POCreateFilter.orderTotal>(e.Cache, null, filter.VendorID != null);
        //}
        //#endregion      
        ///// <summary>
        ///// Copy standard method and add one new customizaed method.
        ///// </summary>
        //public static void CreateProc2(List<POFixedDemand> list, DateTime? orderDate, bool extSort, int? branchID = null)
        //{
        //    PXRedirectRequiredException poredirect = CreatePOOrders(list, orderDate, extSort, branchID);

        //    CopyNoteOrFile2PO(list);

        //    if (poredirect != null) { throw poredirect; }
        //}

        #region Delegate Methods
        [PXOverride]
        public PXRedirectRequiredException CreatePOOrders(List<POFixedDemand> list, DateTime? PurchDate, bool extSort, int? branchID = null,
                                                          Func<List<POFixedDemand>, DateTime?, bool, int?, PXRedirectRequiredException> baseMethod = null)
        {
            PXRedirectRequiredException poredirect = baseMethod(list, PurchDate, extSort, branchID);
            
            CopyNoteOrFile2PO(list);
            
            return poredirect;
        }
        #endregion

        #region Static Methods
        /// <summary>
        /// Copy notes or attchments from SO to PO by configuration of sales order type.
        /// </summary>
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