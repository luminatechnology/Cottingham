using PX.Data;

namespace PX.Objects.RQ
{
    public class RQRequisitionLineExt : PXCacheExtension<PX.Objects.RQ.RQRequisitionLine>
    {
        #region UsrPackaging
        [PXString]
        [PXUIField(DisplayName = "Packaging", IsReadOnly = true)]
        public virtual string UsrPackaging { get; set; }
        public abstract class usrPackaging : PX.Data.BQL.BqlString.Field<usrPackaging> { }
        #endregion
    }
}
