using PX.Data;
using PX.Data.BQL;
using PX.Objects.CS;

namespace PX.Objects.PO
{
    public class POLineExt : PXCacheExtension<POLine>
    {
        public const string Alcohol = "ALCOHOL";

        #region UsrAlcoholAttr
        public class AlcAttr : BqlType<IBqlString, string>.Constant<POLineExt.AlcAttr>
        {
            public AlcAttr() : base(Alcohol) { }
        }

        [PXString]
        [PXUIField(DisplayName = "Alc. %", IsReadOnly = true)]
        [PXUnboundDefault(typeof (Search2<CSAnswers.value, InnerJoinSingleTable<PX.Objects.IN.InventoryItem, On<PX.Objects.IN.InventoryItem.inventoryID, Equal<Current<POLine.inventoryID>>>>, 
                                                           Where<CSAnswers.refNoteID, Equal<PX.Objects.IN.InventoryItem.noteID>, 
                                                                 And<CSAnswers.attributeID, Equal<POLineExt.AlcAttr>>>>))]
        [PXFormula(typeof (Default<POLine.inventoryID>))]
        public virtual string UsrAlcoholAttr { get; set; }
        public abstract class usrAlcoholAttr : BqlType<IBqlString, string>.Field<POLineExt.usrAlcoholAttr> { }
        #endregion

        #region UsrPackaging
        [PXString]
        [PXUIField(DisplayName = "Packaging", IsReadOnly = true)]
        public virtual string UsrPackaging { get; set; }
        public abstract class usrPackaging : BqlType<IBqlString, string>.Field<POLineExt.usrPackaging> { }
        #endregion
    }
}
