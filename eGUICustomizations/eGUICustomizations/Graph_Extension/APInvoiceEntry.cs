using PX.Common;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.CS;
using eGUICustomizations.DAC;
using eGUICustomizations.Descriptor;

namespace PX.Objects.AP
{
    public class APInvoiceEntry_Extension : PXGraphExtension<APInvoiceEntry>
    {
        public bool activateGUI = TWNGUIValidation.ActivateTWGUI(new PXGraph());

        #region Selects        
        [PXCopyPasteHiddenFields(typeof(TWNManualGUIAPBill.docType),
                                 typeof(TWNManualGUIAPBill.refNbr),
                                 typeof(TWNManualGUIAPBill.gUINbr))]
        public SelectFrom<TWNManualGUIAPBill>.Where<TWNManualGUIAPBill.docType.IsEqual<APInvoice.docType.FromCurrent>
                                                    .And<TWNManualGUIAPBill.refNbr.IsEqual<APInvoice.refNbr.FromCurrent>>>.View ManualAPBill;

        [PXCopyPasteHiddenFields(typeof(TWNWHT.docType), typeof(TWNWHT.refNbr))]
        public SelectFrom<TWNWHT>.Where<TWNWHT.docType.IsEqual<APInvoice.docType.FromCurrent>
                                        .And<TWNWHT.refNbr.IsEqual<APInvoice.refNbr.FromCurrent>>>.View WHTView;

        public SelectFrom<TWNGUIPreferences>.View GUISetup;
        #endregion

        #region Delegate Methods
        public delegate void PersistDelegate();
        [PXOverride]
        public void Persist(PersistDelegate baseMethod)
        {
            var invoice = Base.CurrentDocument.Current;

            if (invoice != null && string.IsNullOrEmpty(invoice.InvoiceNbr))
            {
                Base.CurrentDocument.Cache.SetValue<APInvoice.invoiceNbr>(invoice, ManualAPBill.Select().TopFirst?.GUINbr);
                Base.CurrentDocument.UpdateCurrent();
            }

            baseMethod();
        }
        #endregion

        #region Event Handlers
        protected void _(Events.RowPersisting<APInvoice> e, PXRowPersisting baseHandler)
        {
            baseHandler?.Invoke(e.Cache, e.Args);

            var row = e.Row as APInvoice;

            if (row != null && row.DocType.IsIn(APDocType.Invoice, APDocType.DebitAdj) && string.IsNullOrEmpty(row.OrigRefNbr))
            {
                if (ManualAPBill.Select().Count == 0 && Base.Taxes.Select().Count > 0)
                {
                    throw new PXException(TWMessages.NoGUIWithTax);
                }
                else
                {
                    decimal? taxSum = 0;

                    foreach (TWNManualGUIAPBill line in ManualAPBill.Select())
                    {
                        tWNGUIValidation.CheckCorrespondingInv(Base, line.GUINbr, line.VATInCode);

                        if (tWNGUIValidation.errorOccurred == true)
                        {
                            e.Cache.RaiseExceptionHandling<TWNManualGUIAPBill.gUINbr>(e.Row, line.GUINbr, new PXSetPropertyException(tWNGUIValidation.errorMessage, PXErrorLevel.RowError));
                        }

                        taxSum += line.TaxAmt.Value;
                    }

                    if (taxSum != row.TaxTotal && e.Operation != PXDBOperation.Delete)
                    {
                        throw new PXException(TWMessages.ChkTotalGUIAmt);
                    }
                }
            }
        }

        protected void _(Events.RowSelected<APInvoice> e, PXRowSelected baseHandler)
        {
            baseHandler?.Invoke(e.Cache, e.Args);

            var row = e.Row as APInvoice;

            if (row != null)
            {
                ManualAPBill.Cache.AllowSelect = activateGUI;
                ManualAPBill.Cache.AllowDelete = ManualAPBill.Cache.AllowInsert = ManualAPBill.Cache.AllowUpdate = row.Status.IsIn(APDocStatus.Hold, APDocStatus.Balanced);
            }

            WHTView.AllowSelect = GUISetup.Select().TopFirst?.EnableWHT == true;
            WHTView.AllowDelete = WHTView.AllowInsert = WHTView.AllowUpdate = Base.Transactions.AllowUpdate;
        }

        //protected void _(Events.RowInserting<APInvoice> e)
        //{
        //    if (e.Row == null || Base.vendor.Current == null || activateGUI == false) { return; }

        //    APRegisterExt regisExt = e.Row.GetExtension<APRegisterExt>();

        //    string vATIncode = regisExt.UsrVATInCode;

        //    if (string.IsNullOrEmpty(vATIncode))
        //    {
        //        CSAnswers cSAnswers = CSAnswers.PK.Find(Base, Base.vendor.Current.NoteID, TWNManualGUIAPBill.VATINFRMTName);

        //        vATIncode = (e.Row.IsRetainageDocument == true || cSAnswers == null) ? null : cSAnswers.Value;
        //    }

        //    regisExt.UsrVATInCode = e.Row.DocType == APDocType.DebitAdj &&
        //                            !string.IsNullOrEmpty(vATIncode) ? TWGUIFormatCode.vATInCode23 /*(int.Parse(vATIncode) + 2).ToString()*/ : vATIncode;
        //}

        protected void _(Events.FieldUpdated<APInvoice.vendorID> e)
        {
            var vendor = Base.vendor.Current;

            if (vendor == null || activateGUI == false) { return; }

            //switch (row.DocType)
            //{
            //    case APDocType.DebitAdj:
            //        row.GetExtension<APRegisterExt>().UsrVATInCode = TWGUIFormatCode.vATInCode23;
            //        break;

            //    case APDocType.Invoice:
            //        CSAnswers cSAnswers = CSAnswers.PK.Find(Base, vendor.NoteID, TWNManualGUIAPBill.VATINFRMTName);

            //        row.GetExtension<APRegisterExt>().UsrVATInCode = cSAnswers?.Value;
            //        break;
            //}

            if (GUISetup.Select().TopFirst?.EnableWHT == true &&
                Base.Document.Current != null && Base.Document.Current.DocType == APDocType.Invoice)
            {
                TWNWHT wNWHT = new TWNWHT()
                {
                    DocType = Base.Document.Current.DocType
                };

                WHTView.Cache.SetDefaultExt<TWNWHT.personalID>(wNWHT);
                WHTView.Cache.SetDefaultExt<TWNWHT.propertyID>(wNWHT);
                WHTView.Cache.SetDefaultExt<TWNWHT.typeOfIn>(wNWHT);
                WHTView.Cache.SetDefaultExt<TWNWHT.wHTFmtCode>(wNWHT);
                WHTView.Cache.SetDefaultExt<TWNWHT.wHTFmtSub>(wNWHT);
                WHTView.Cache.SetDefaultExt<TWNWHT.wHTTaxPct>(wNWHT);
                WHTView.Cache.SetDefaultExt<TWNWHT.secNHICode>(wNWHT);

                WHTView.Cache.Insert(wNWHT);
            }
        }

        #region TWNManualGUIAPBill
        TWNGUIValidation tWNGUIValidation = new TWNGUIValidation();

        protected void _(Events.FieldDefaulting<TWNManualGUIAPBill.deduction> e)
        {
            var row = e.Row as TWNManualGUIAPBill;

            /// If user doesn't choose a vendor then bring the fixed default value from Attribure "DEDUCTCODE" first record.
            e.NewValue = row.VendorID == null ? new string1() : e.NewValue;
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
        #endregion

        #endregion
    }
}