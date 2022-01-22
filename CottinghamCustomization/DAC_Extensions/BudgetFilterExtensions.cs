using System;
using PX.Data;

namespace PX.Objects.GL
{
    public class BudgetFilterExt : PXCacheExtension<PX.Objects.GL.BudgetFilter>
    {
		#region UsrFinPeriodID
		[FinPeriodSelector(null,
						   typeof(AccessInfo.businessDate),
						   branchSourceType: typeof(GLHistoryEnqFilter.branchID),
						   organizationSourceType: typeof(GLHistoryEnqFilter.organizationID),
						   useMasterCalendarSourceType: typeof(GLHistoryEnqFilter.useMasterCalendar),
						   redefaultOrRevalidateOnOrganizationSourceUpdated: false)]
		[PXUIField(DisplayName = "Fin. Period", Visibility = PXUIVisibility.Visible)]
		public virtual string UsrFinPeriodID { get; set; }
		public abstract class usrFinPeriodID : PX.Data.BQL.BqlString.Field<usrFinPeriodID> { }
		#endregion
	}
}
