using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Data.WorkflowAPI;
using PX.Objects.CR;
using PX.Objects.EP;
using System.Collections;
using System.Collections.Generic;

namespace PX.Objects.SO
{
    public class SOOrderEntry_Extension : PXGraphExtension<SOOrderEntry_Workflow, SOOrderEntry>
    {
        public const string LSPriceOrder = "SO641011";

        #region Override Methods
        public override void Configure(PXScreenConfiguration config)
        {
            Configure(config.GetScreenConfigurationContext<SOOrderEntry, SOOrder>());
        }

        protected virtual void Configure(WorkflowContext<SOOrderEntry, SOOrder> context)
        {
            context.UpdateScreenConfigurationFor(screen =>
            {
                return screen.WithActions(actions =>
                {
                    actions.Add<SOOrderEntry_Extension>(e => e.printLSPriceOrder,
                                                        a => a.WithCategory(PX.Objects.Common.CommonActionCategories.Get(context).PrintingAndEmailing/*"Printing and Emailing"*/).PlaceAfter(s => s.printSalesOrder));
                });
            });
        }
        #endregion

        #region Actions
        public PXAction<SOOrder> printLSPriceOrder;
        [PXButton()]
        [PXUIField(DisplayName = "Print Sales Order(LS Price)", MapEnableRights = PXCacheRights.Select)]
        protected virtual IEnumerable PrintLSPriceOrder(PXAdapter adapter)
        {
            if (Base.Document.Current != null)
            {
                Dictionary<string, string> parameters = new Dictionary<string, string>
                {
                    [nameof(SOOrder.OrderType)] = Base.Document.Current.OrderType,
                    [nameof(SOOrder.RefNbr)] = Base.Document.Current.RefNbr
                };

                throw new PXReportRequiredException(parameters, LSPriceOrder, LSPriceOrder);
            }

            return adapter.Get();
        }
        #endregion 

        #region Event Handlers
        protected void _(Events.RowPersisting<SOLine> e, PXRowPersisting baseHandler)
        {
            baseHandler?.Invoke(e.Cache, e.Args);

            if (e.Row == null || e.Operation.Command() == PXDBOperation.Delete) { return; }

            PXDefaultAttribute.SetPersistingCheck<SOLine.reasonCode>(e.Cache, e.Row, e.Row.IsFree.Value ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
        }

        protected void _(Events.FieldUpdated<SOOrder.ownerID> e)
        {
            var row = e.Row as SOOrder;

            if (row != null && e.NewValue != null)
            {
                row.SalesPersonID = SelectFrom<EPEmployee>.InnerJoin<BAccount>.On<BAccount.bAccountID.IsEqual<EPEmployee.bAccountID>>
                                                          .Where<BAccount.defContactID.IsEqual<@P.AsInt>>.View.ReadOnly.Select(Base, e.NewValue).TopFirst?.SalesPersonID;
            }
        }

        protected void _(Events.FieldDefaulting<SOLine.pOCreate> e, PXFieldDefaulting baseHandler)
        {
            baseHandler?.Invoke(e.Cache, e.Args);

            e.NewValue = Base.Document.Current != null && Base.Document.Current.OrderType == "SA";
        }

        protected void _(Events.FieldUpdated<SOLine.isFree> e)
        {
            SOLine row = e.Row as SOLine;

            if (!(bool)e.NewValue || !string.IsNullOrEmpty(row.ReasonCode)) { return; }

            this.Base.Transactions.Cache.RaiseExceptionHandling<SOLine.reasonCode>(row, e.NewValue, new PXSetPropertyException(PX.SM.MyMessages.MandatoryField, PXErrorLevel.Warning));
        }
        #endregion
    }
}
