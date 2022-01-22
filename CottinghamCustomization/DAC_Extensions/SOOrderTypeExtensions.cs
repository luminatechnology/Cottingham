using PX.Data;

namespace PX.Objects.SO
{
    public class SOOrderTypeExt : PXCacheExtension<PX.Objects.SO.SOOrderType>
    {
        #region UsrCopyNoteToPO
        [PXDBBool]
        [PXUIField(DisplayName = "Copy Notes To PO")]
        public virtual bool? UsrCopyNotesToPO { get; set; }
        public abstract class usrCopyNotesToPO : PX.Data.BQL.BqlBool.Field<usrCopyNotesToPO> { }
        #endregion
    
        #region UsrCopyFilesToPO
        [PXDBBool]
        [PXUIField(DisplayName = "Copy Attachments To PO")]
        public virtual bool? UsrCopyFilesToPO { get; set; }
        public abstract class usrCopyFilesToPO : PX.Data.BQL.BqlBool.Field<usrCopyFilesToPO> { }
        #endregion
    }
}