using PX.Data;
using PX.Objects.AP;

namespace PX.Objects.RQ
{
    public class RQRequestClassExt : PXCacheExtension<RQRequestClass>
    {
        #region UsrVendor
        [PXUIField(DisplayName = "Default Vendor")]
        [VendorNonEmployeeActive(CacheGlobal = true, DescriptionField = typeof(Vendor.acctName), Filterable = true, Visibility = PXUIVisibility.SelectorVisible)]
        public virtual int? UsrVendor { get; set; }
        public abstract class usrVendor : PX.Data.BQL.BqlInt.Field<usrVendor> { }
        #endregion
    }
}
