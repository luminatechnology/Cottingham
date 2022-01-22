using PX.Data;

namespace PX.Objects.RQ
{
    public class RQSetupExt : PXCacheExtension<PX.Objects.RQ.RQSetup>
    {
        #region UsrCloseCuryCnvDialog
        [PXDBBool]
        [PXUIField(DisplayName = "Close Currency Conversion Dialogue")]
        public virtual bool? UsrCloseCuryCnvDialog { get; set; }
        public abstract class usrCloseCuryCnvDialog : PX.Data.BQL.BqlBool.Field<usrCloseCuryCnvDialog> { }
        #endregion
    }
}