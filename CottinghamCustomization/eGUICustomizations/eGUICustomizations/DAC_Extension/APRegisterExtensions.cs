using System;
using PX.Data;
using PX.Objects.CS;
using eGUICustomizations.Graph;

namespace PX.Objects.AP
{
    public class APRegisterExt : PXCacheExtension<PX.Objects.AP.APRegister>
    {
        #region UsrVATInCode
        public const string VATINFRMTName= "VATINFRMT";
        public class VATINFRMTNameAtt : PX.Data.BQL.BqlString.Constant<VATINFRMTNameAtt>
        {
           public VATINFRMTNameAtt() : base(VATINFRMTName) { }
        }

        [PXDBString(2, IsUnicode = true)]
        [PXUIField(DisplayName = "VAT Format Code", Visibility = PXUIVisibility.SelectorVisible)]
        //[PXSelector(typeof(Search<CSAttributeDetail.valueID,
        //                          Where<CSAttributeDetail.attributeID, Equal<VATINFRMTNameAtt>>>),
        //            typeof(CSAttributeDetail.description),
        //            DescriptionField = typeof(CSAttributeDetail.description))]
        public virtual string UsrVATInCode { get; set; }
        public abstract class usrVATInCode : IBqlField { }
        #endregion

        #region UsrTaxNbr
        public const string TaxNbrName= "TAXNbr";
        public class TaxNbrNameAtt : PX.Data.BQL.BqlString.Constant<TaxNbrNameAtt>
        {
           public TaxNbrNameAtt() : base(TaxNbrName) { }
        }

        [TaxNbrVerify(8, IsUnicode = true)]
        [PXUIField(DisplayName = "Tax Nbr")]
        //[PXDefault(typeof(Search<CSAnswers.value, 
        //                         Where<CSAnswers.refNoteID, Equal<Current<Vendor.noteID>>,
        //                               And<CSAnswers.attributeID, Equal<TaxNbrNameAtt>>>>),
        //           PersistingCheck = PXPersistingCheck.Nothing)]
        //[PXFormula(typeof(Default<APInvoice.vendorID>))]        
        public virtual string UsrTaxNbr { get; set; }
        public abstract class usrTaxNbr : IBqlField { }
        #endregion

        #region UsrOurTaxNbr
        public const string OurTaxNbrName= "OURTAXNBR";
        public class OurTaxNbrNameAtt : PX.Data.BQL.BqlString.Constant<OurTaxNbrNameAtt>
        {
           public OurTaxNbrNameAtt() : base(OurTaxNbrName) { }
        }

        [TaxNbrVerify(8, IsUnicode = true)]
        [PXUIField(DisplayName = "Our Tax Nbr")]
        //[PXDefault(typeof(Search<CSAnswers.value, 
        //                         Where<CSAnswers.refNoteID, Equal<Current<Vendor.noteID>>,
        //                               And<CSAnswers.attributeID, Equal<OurTaxNbrNameAtt>>>>),
        //           PersistingCheck = PXPersistingCheck.Nothing)]
        //[PXFormula(typeof(Default<APInvoice.vendorID>))]        
        public virtual string UsrOurTaxNbr { get; set; }
        public abstract class usrOurTaxNbr : IBqlField { }
        #endregion

        #region UsrDeduction
        public const string DeductionName = "DEDUCTCODE";
        public class DeductionNameAtt : PX.Data.BQL.BqlString.Constant<DeductionNameAtt>
        {
           public DeductionNameAtt () : base(DeductionName) { }
        }

        [PXDBString(1, IsUnicode = true)]
        [PXUIField(DisplayName = "Deduction Code", Visibility = PXUIVisibility.SelectorVisible)]
        //[PXDefault(typeof(Search<CSAnswers.value, 
        //                         Where<CSAnswers.refNoteID, Equal<Current<Vendor.noteID>>,
        //                               And<CSAnswers.attributeID, Equal<DeductionNameAtt>>>>),
        //           PersistingCheck = PXPersistingCheck.Nothing)]
        //[PXFormula(typeof(Default<APInvoice.vendorID>))]
        //[PXSelector(typeof(Search<CSAttributeDetail.valueID,
        //                          Where<CSAttributeDetail.attributeID, Equal<DeductionNameAtt>>>),
        //            typeof(CSAttributeDetail.description),
        //            DescriptionField = typeof(CSAttributeDetail.description))]         
        public virtual string UsrDeduction { get; set; }
        public abstract class usrDeduction : IBqlField { }
        #endregion

        #region UsrGUINbr
        [PXUIField(DisplayName = "GUI Nbr.")]
        //[PXFormula(typeof(APInvoice.invoiceNbr))]
        [GUINumber(15, IsUnicode = true, InputMask = ">aaaaaaaaaaaaaa")]       
        public virtual string UsrGUINbr { get; set; }
        public abstract class usrGUINbr : IBqlField { }
        #endregion

        #region UsrGUIDate
        [PXDBDate()]        
        [PXUIField(DisplayName = "GUI Date")]
        //[PXDefault(typeof(AccessInfo.businessDate), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual DateTime? UsrGUIDate { get; set; }
        public abstract class usrGUIDate : IBqlField { }
        #endregion
    }
}