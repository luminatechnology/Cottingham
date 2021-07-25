using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AP;
using PX.Objects.GL;
using PX.Objects.FS;
using PX.Objects.TX;
using eGUICustomizations.DAC;
using eGUICustomizations.Graph_Release;
using eGUICustomizations.Descriptor;
using static eGUICustomizations.Descriptor.TWNStringList;

namespace PX.Objects.AR
{
    public class ARReleaseProcess_Extension : PXGraphExtension<ARReleaseProcess>
    {
        #region Select
        public SelectFrom<TWNGUITrans>
                          .Where<TWNGUITrans.orderNbr.IsEqual<APInvoice.refNbr.FromCurrent>>.View ViewGUITrans;
        #endregion

        public static bool IsActive() => TWNGUIValidation.ActivateTWGUI(new PXGraph());

        /// <summary>
        ///  The variable works for skip first overriding Persist when the second BLC extension has override the same method.
        /// </summary>
        public bool skipPersist = false;

        #region Delegate Method
        public delegate void PersistDelegate();
        [PXOverride]
        public void Persist(PersistDelegate baseMethod)
        {
            baseMethod();

            if (skipPersist == false)
            {
                ARRegister    doc    = Base.ARDocument.Current;
                ARRegisterExt docExt = PXCache<ARRegister>.GetExtension<ARRegisterExt>(doc);

                // Check for document and released flag
                if (doc != null && 
                    doc.Released == true &&
                    doc.DocType.IsIn(ARDocType.Invoice, ARDocType.CreditMemo, ARDocType.CashSale) &&
                    ((string.IsNullOrEmpty(docExt.UsrGUINbr) && docExt.UsrVATOutCode == TWGUIFormatCode.vATOutCode36) || !string.IsNullOrEmpty(docExt.UsrVATOutCode)) 
                   )
                {
                    if (docExt.UsrVATOutCode.IsIn(TWGUIFormatCode.vATOutCode33, TWGUIFormatCode.vATOutCode34) &&
                        docExt.UsrCreditAction == TWNCreditAction.NO )
                    {
                        throw new PXException(TWMessages.CRACIsNone);
                    }
                    else if (docExt.UsrVATOutCode == TWGUIFormatCode.vATOutCode37)
                    {
                        throw new PXException (TWMessages.VATOutCodeIs37);
                    }

                    TaxTran xTran = APReleaseProcess_Extension.SelectTaxTran(Base, doc.DocType, doc.RefNbr, BatchModule.AR);

                    if (xTran == null) { throw new PXException(TWMessages.NoInvTaxDtls); }

                    if (PXCache<Tax>.GetExtension<TaxExt>(Tax.PK.Find(Base, xTran.TaxID)).UsrTWNGUI != true) { return; }

                    decimal? netAmt = xTran.TaxableAmt + xTran.RetainedTaxableAmt;
                    decimal? taxAmt = xTran.TaxAmt + xTran.RetainedTaxAmt;
                    decimal? remainNet = netAmt;
                    decimal? remainTax = taxAmt;
                    decimal? settledNet = 0;
                    decimal? settledTax = 0;

                    using (PXTransactionScope ts = new PXTransactionScope())
                    {
                        TWNReleaseProcess rp = PXGraph.CreateInstance<TWNReleaseProcess>();

                        Customer customer = SelectFrom<Customer>.Where<Customer.bAccountID.IsEqual<@P.AsInt>>.View.ReadOnly.Select(Base, doc.CustomerID);

                        string[] gUINbrs = !string.IsNullOrEmpty(docExt.UsrGUINbr) ? docExt.UsrGUINbr.Split(';') : new string[1] { string.Empty };

                        for (int i = 0; i < gUINbrs.Length; i++)
                        {
                            string gUINbr = gUINbrs[i].TrimStart();

                            // Avoid standard logic calling this method twice and inserting duplicate records into TWNGUITrans.
                            if (APReleaseProcess_Extension.CountExistedRec(Base, gUINbr, docExt.UsrVATOutCode, doc.RefNbr) > 0) { return; }

                            TWNGUITrans tWNGUITrans = rp.InitAndCheckOnAR(gUINbr, docExt.UsrVATOutCode);

                            string taxCateID = string.Empty;

                            foreach (ARTran row in Base.ARTran_TranType_RefNbr.Cache.Cached)
                            {
                                taxCateID = row.TaxCategoryID;

                                goto CreatGUI;
                            }

                        CreatGUI:
                            if (docExt.UsrCreditAction.IsIn(TWNCreditAction.CN, TWNCreditAction.NO) )
                            {
                                //TWNGUIPreferences gUIPreferences = SelectFrom<TWNGUIPreferences>.View.Select(Base);

                                //string numberingSeq = (docExt.UsrVATOutCode == TWGUIFormatCode.vATOutCode32) ? gUIPreferences.GUI2CopiesNumbering : gUIPreferences.GUI3CopiesNumbering;

                                //docExt.UsrGUINbr = ARGUINbrAutoNumAttribute.GetNextNumber(Base.ARDocument.Cache, doc, numberingSeq, doc.DocDate);

                                FSAppointment appointment = SelectFrom<FSAppointment>.LeftJoin<FSPostDoc>.On<FSPostDoc.appointmentID.IsEqual<FSAppointment.appointmentID>>
                                                                                     .Where<FSPostDoc.postDocType.IsEqual<@P.AsString>
                                                                                            .And<FSPostDoc.postRefNbr.IsEqual<@P.AsString>>>
                                                                                     .View.ReadOnly.Select(Base, doc.DocType, doc.RefNbr);

                                if (tWNGUITrans != null)
                                {
                                    settledNet = (tWNGUITrans.NetAmtRemain < remainNet) ? tWNGUITrans.NetAmtRemain : remainNet;
                                    settledTax = (tWNGUITrans.TaxAmtRemain < remainTax) ? tWNGUITrans.TaxAmtRemain : remainTax;

                                    remainNet -= settledNet;
                                    remainTax -= settledTax;
                                }
                                else
                                {
                                    settledNet = remainNet;
                                    settledTax = remainTax;
                                }

                                rp.CreateGUITrans(new STWNGUITran()
                                {
                                    VATCode       = docExt.UsrVATOutCode,
                                    GUINbr        = gUINbr ?? string.Empty,
                                    GUIStatus     = doc.CuryOrigDocAmt == 0m ? TWNGUIStatus.Voided : TWNGUIStatus.Used,
                                    BranchID      = doc.BranchID,
                                    GUIDirection  = TWNGUIDirection.Issue,
                                    GUIDate       = docExt.UsrGUIDate.Value.Date.Add(doc.CreatedDateTime.Value.TimeOfDay),
                                    GUITitle      = customer.AcctName,
                                    TaxZoneID     = Base.ARInvoice_DocType_RefNbr.Current.TaxZoneID,
                                    TaxCategoryID = taxCateID,
                                    TaxID         = xTran.TaxID,
                                    TaxNbr        = docExt.UsrTaxNbr,
                                    OurTaxNbr     = docExt.UsrOurTaxNbr,
                                    NetAmount     = settledNet < 0 ? 0 : settledNet,
                                    TaxAmount     = settledTax < 0 ? 0 : settledTax,
                                    AcctCD        = customer.AcctCD,
                                    AcctName      = customer.AcctName,
                                    Remark        = (appointment is null) ? string.Empty : appointment.RefNbr,
                                    BatchNbr      = doc.BatchNbr,
                                    OrderNbr      = doc.RefNbr,
                                    CarrierType   = GetCarrierType(docExt.UsrCarrierID),
                                    CarrierID     = docExt.UsrB2CType == TWNB2CType.MC ? GetCarrierID(docExt.UsrTaxNbr, docExt.UsrCarrierID) : null,
                                    NPONbr        = docExt.UsrB2CType == TWNB2CType.NPO ? GetNPOBAN(docExt.UsrTaxNbr, docExt.UsrNPONbr) : null,
                                    B2CPrinted    = (docExt.UsrB2CType == TWNB2CType.DEF && string.IsNullOrEmpty(docExt.UsrTaxNbr)) ? true : false,
                                });
                            }                           

                            if (tWNGUITrans != null)
                            {
                                if (docExt.UsrCreditAction.Equals(TWNCreditAction.VG))
                                {
                                    ViewGUITrans.SetValueExt<TWNGUITrans.gUIStatus>   (tWNGUITrans, TWNGUIStatus.Voided);
                                    ViewGUITrans.SetValueExt<TWNGUITrans.eGUIExported>(tWNGUITrans, false);
                                }
                                else
                                {
                                    if (remainNet < 0) { throw new PXException(TWMessages.RemainAmt); }

                                    ViewGUITrans.SetValueExt<TWNGUITrans.netAmtRemain>(tWNGUITrans, (tWNGUITrans.NetAmtRemain -= settledNet));
                                    ViewGUITrans.SetValueExt<TWNGUITrans.taxAmtRemain>(tWNGUITrans, (tWNGUITrans.TaxAmtRemain -= settledTax));
                                }

                                ViewGUITrans.Update(tWNGUITrans);
                            }
                        }

                        // Manually Saving as base code will not call base graph persist.
                        ViewGUITrans.Cache.Persist(PXDBOperation.Insert);
                        ViewGUITrans.Cache.Persist(PXDBOperation.Update);

                        ts.Complete(Base);

                        //if (doc.DocType == ARDocType.Invoice && gUINbr != null && rp.ViewGUITrans.Current.GUIStatus.Equals(TWNGUIStatus.Used) && docExt.UsrB2CType.Equals(TWNB2CType.DEF))
                        //{
                        //    Base.ARTran_TranType_RefNbr.WhereAnd<Where<ARTran.curyExtPrice, Greater<decimal0>>>();
                        //    PXGraph.CreateInstance<eGUIInquiry>().PrintReport(Base.ARTran_TranType_RefNbr.Select(doc.DocType, doc.RefNbr), rp.ViewGUITrans.Current, false);
                        //}
                    }
                }
                // Triggering after save events.
                ViewGUITrans.Cache.Persisted(false);

                //Base.ARDocument.Cache.SetValue<ARRegisterExt.usrGUINbr>(doc, gUINbr);
                //Base.ARDocument.Cache.MarkUpdated(doc);
            }

            baseMethod();
        }
        #endregion

        #region Static Methods
        public static string GetCarrierType(string carrierID)
        {
            // 手機條碼：3J0002, 自然人憑證：CQ0001
            string num1 = "3J0002", num2 = "CQ0001";

            if (string.IsNullOrEmpty(carrierID)) { return carrierID; }
                
            return carrierID.Substring(0, 1).Equals("/") ? num1 : num2;
        }

        public static string GetCarrierID(string taxNbr, string carrierID)
        {
            return string.IsNullOrEmpty(taxNbr) ? carrierID : null;
        }

        public static string GetNPOBAN(string taxNbr, string nPONbr)
        {
            return string.IsNullOrEmpty(taxNbr) ? nPONbr : null;
        }
        #endregion
    }
}