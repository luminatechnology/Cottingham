using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.CS;
using PX.Objects.TX;
using eGUICustomizations.DAC;
using eGUICustomizations.Descriptor;

namespace PX.Objects.AP
{
    public class APInvoiceEntry_Extension : PXGraphExtension<APInvoiceEntry>
    {
        #region Selects        
        [PXCopyPasteHiddenFields(typeof(TWNManualGUIAPBill.docType),
                                 typeof(TWNManualGUIAPBill.refNbr),
                                 typeof(TWNManualGUIAPBill.gUINbr))]
        public SelectFrom<TWNManualGUIAPBill>.Where<TWNManualGUIAPBill.docType.IsEqual<APInvoice.docType.FromCurrent>
                                                    .And<TWNManualGUIAPBill.refNbr.IsEqual<APInvoice.refNbr.FromCurrent>>>.View ManualAPBill;
        #endregion

        #region Event Handlers
        public bool activateGUI = TWNGUIValidation.ActivateTWGUI(new PXGraph());

        protected void _(Events.RowPersisting<APInvoice> e, PXRowPersisting baseHandler)
        {
            baseHandler?.Invoke(e.Cache, e.Args);

            //if (ManualAPBill.Select().Count == 0 && Base.Taxes.Select().Count > 0)
            //{
            //    throw new PXException(TWMessages.NoGUIWithTax);
            //}

            //if (activateGUI.Equals(false) || e.Row.Status.Equals(APDocStatus.Open)) { return; }

            //APRegisterExt regisExt = PXCache<APRegister>.GetExtension<APRegisterExt>(e.Row);

            //TWNGUIValidation tWNGUIValidation = new TWNGUIValidation();

            //if (regisExt.UsrVATInCode == TWGUIFormatCode.vATInCode21 ||
            //    regisExt.UsrVATInCode == TWGUIFormatCode.vATInCode22 ||
            //    regisExt.UsrVATInCode == TWGUIFormatCode.vATInCode25)
            //{
            //    tWNGUIValidation.CheckGUINbrExisted(Base, regisExt.UsrGUINbr, regisExt.UsrVATInCode);
            //}
            //else
            //{
            //    tWNGUIValidation.CheckCorrespondingInv(Base, regisExt.UsrGUINbr, regisExt.UsrVATInCode);

            //    if (tWNGUIValidation.errorOccurred.Equals(true))
            //    {
            //        e.Cache.RaiseExceptionHandling<APRegisterExt.usrGUINbr>(e.Row, regisExt.UsrGUINbr, new PXSetPropertyException(tWNGUIValidation.errorMessage, PXErrorLevel.Error));
            //    }
            //}
        }

        protected void _(Events.RowSelected<APInvoice> e, PXRowSelected baseHandler)
        {
            baseHandler?.Invoke(e.Cache, e.Args);

            var row = e.Row as APInvoice;

            if (row != null)
            {
                ManualAPBill.Cache.AllowSelect = TWNGUIValidation.ActivateTWGUI(Base);
                ManualAPBill.Cache.AllowDelete = ManualAPBill.Cache.AllowInsert = ManualAPBill.Cache.AllowUpdate = row.Status == APDocStatus.Hold;
            }

            //PXUIFieldAttribute.SetVisible<APRegisterExt.usrDeduction>(e.Cache, null, activateGUI);
            //PXUIFieldAttribute.SetVisible<APRegisterExt.usrGUIDate>(e.Cache, null, activateGUI);
            //PXUIFieldAttribute.SetVisible<APRegisterExt.usrGUINbr>(e.Cache, null, activateGUI);
            //PXUIFieldAttribute.SetVisible<APRegisterExt.usrOurTaxNbr>(e.Cache, null, activateGUI);
            //PXUIFieldAttribute.SetVisible<APRegisterExt.usrTaxNbr>(e.Cache, null, activateGUI);
            //PXUIFieldAttribute.SetVisible<APRegisterExt.usrVATInCode>(e.Cache, null, activateGUI);
        }

        protected void _(Events.RowInserting<APInvoice> e)
        {
            if (e.Row == null || Base.vendor.Current == null || activateGUI.Equals(false)) { return; }

            APRegisterExt regisExt = PXCache<APRegister>.GetExtension<APRegisterExt>(e.Row);

            string vATIncode = regisExt.UsrVATInCode ?? string.Empty;

            if (string.IsNullOrEmpty(vATIncode))
            {
                CSAnswers cSAnswers = SelectCSAnswers(Base, Base.vendor.Current.NoteID);

                vATIncode = (e.Row.IsRetainageDocument == true || cSAnswers == null) ? null : cSAnswers.Value;
            }

            regisExt.UsrVATInCode = e.Row.DocType.Equals(APDocType.DebitAdj, System.StringComparison.CurrentCulture) && 
                                    !string.IsNullOrEmpty(vATIncode) ? TWGUIFormatCode.vATInCode23 /*(int.Parse(vATIncode) + 2).ToString()*/ : vATIncode;
        }

        protected void _(Events.FieldUpdated<APInvoice.vendorID> e)
        {
            var row    = e.Row as APInvoice;
            var vendor = Base.vendor.Current;

            if (vendor == null || activateGUI == false) { return; }

            switch (row.DocType)
            {
                case APDocType.DebitAdj:
                    PXCache<APRegister>.GetExtension<APRegisterExt>(row).UsrVATInCode = TWGUIFormatCode.vATInCode23;
                    break;

                case APDocType.Invoice:
                    CSAnswers cSAnswers = SelectCSAnswers(Base, vendor.NoteID);

                    PXCache<APRegister>.GetExtension<APRegisterExt>(row).UsrVATInCode = cSAnswers?.Value;
                    break;
            }
        }

        #region TWNManualGUIAPBill
        TWNGUIValidation tWNGUIValidation = new TWNGUIValidation();

        protected void _(Events.RowPersisting<TWNManualGUIAPBill> e)
        {
            if (Base.Document.Current == null) { return; }

            tWNGUIValidation.CheckCorrespondingInv(Base, e.Row.GUINbr, e.Row.VATInCode);

            if (tWNGUIValidation.errorOccurred == true)
            {
                e.Cache.RaiseExceptionHandling<TWNManualGUIAPBill.gUINbr>(e.Row, e.Row.GUINbr, new PXSetPropertyException(tWNGUIValidation.errorMessage, PXErrorLevel.RowError));
            }

            if (Base.Document.Current.DocType == APDocType.Invoice)
            {
                decimal? taxSum = 0;

                foreach (TWNManualGUIAPBill row in ManualAPBill.Cache.Cached)
                {
                    taxSum += row.TaxAmt.Value;
                }

                if (taxSum != Base.Document.Current.CuryTaxTotal)
                {
                    throw new PXException(TWMessages.ChkTotalGUIAmt);
                }
            }
        }

        protected void _(Events.FieldDefaulting<TWNManualGUIAPBill.deduction> e)
        {
            var row = e.Row as TWNManualGUIAPBill;

            /// If user doesn't choose a vendor then bring the fixed default value from Attribure "DEDUCTCODE" first record.
            e.NewValue = row.VendorID == null ? "1" : e.NewValue;
        }

        protected void _(Events.FieldDefaulting<TWNManualGUIAPBill.ourTaxNbr> e)
        {
            var row = e.Row as TWNManualGUIAPBill;

            TWNGUIPreferences preferences = SelectFrom<TWNGUIPreferences>.View.Select(Base);

            e.NewValue = row.VendorID == null ? preferences.OurTaxNbr : e.NewValue;
        }

        protected void _(Events.FieldVerifying<TWNManualGUIAPBill.gUINbr> e)
        {
            var row = e.Row as TWNManualGUIAPBill;

            tWNGUIValidation.CheckGUINbrExisted(Base, (string)e.NewValue, row.VATInCode);
        }

        protected void _(Events.FieldVerifying<TWNManualGUIAPBill.taxAmt> e)
        {
            var row = e.Row as TWNManualGUIAPBill;

            tWNGUIValidation.CheckTaxAmount((decimal)row.NetAmt, (decimal)e.NewValue);
        }

        protected void _(Events.FieldUpdated<TWNManualGUIAPBill.netAmt> e)
        {
            var row = e.Row as TWNManualGUIAPBill;

            foreach (TaxRev taxRev in SelectFrom<TaxRev>.Where<TaxRev.taxID.IsEqual<@P.AsString>
                                                               .And<TaxRev.taxType.IsEqual<TaxRev.taxType>>>.View.Select(Base, row.TaxID, "P")) // P = Group type (Input)
            {
                row.TaxAmt = row.NetAmt * (taxRev.TaxRate / taxRev.NonDeductibleTaxRate);
            }
        }
        #endregion

        #endregion

        #region Static Method
        public static CSAnswers SelectCSAnswers(PXGraph graph, System.Guid? noteID)
        {
            return SelectFrom<CSAnswers>.Where<CSAnswers.refNoteID.IsEqual<@P.AsGuid>
                                               .And<CSAnswers.attributeID.IsEqual<APRegisterExt.VATINFRMTNameAtt>>>
                                        .View.Select(graph, noteID);
        }
        #endregion
    }
}