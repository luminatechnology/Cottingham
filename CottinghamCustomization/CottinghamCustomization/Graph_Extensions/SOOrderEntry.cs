using PX.Data;
using PX.Objects.EP;

namespace PX.Objects.SO
{
    public class SOOrderEntry_Extension : PXGraphExtension<SOOrderEntry>
    {
        #region Event Handlers
        protected void _(Events.RowPersisting<SOLine> e, PXRowPersisting baseHandler)
        {
            baseHandler?.Invoke(e.Cache, e.Args);

            if (e.Row == null || e.Operation.Command() == PXDBOperation.Delete) { return; }

            PXDefaultAttribute.SetPersistingCheck<SOLine.reasonCode>(e.Cache, e.Row, e.Row.IsFree.Value ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
        }

        protected void _(Events.FieldUpdated<SOLine.isFree> e)
        {
            SOLine row = e.Row as SOLine;

            if (!(bool)e.NewValue || !string.IsNullOrEmpty(row.ReasonCode)) { return; }

            this.Base.Transactions.Cache.RaiseExceptionHandling<SOLine.reasonCode>(row, e.NewValue, new PXSetPropertyException(PX.SM.MyMessages.MandatoryField, PXErrorLevel.Warning));
        }

        protected void _(Events.FieldUpdated<SOOrder.ownerID> e)
        {
            var row = e.Row as SOOrder;

            if (row != null && e.NewValue != null)
            {
                row.SalesPersonID = PXSelectReadonly<EPEmployee, Where<EPEmployee.userID, Equal<Required<SOOrder.ownerID>>,
                                                                       And<EPEmployee.status, Equal<PX.Objects.AP.Vendor.status.active>>>>.Select(Base, e.NewValue).TopFirst?.SalesPersonID;
            }
        }
        #endregion
    }
}
