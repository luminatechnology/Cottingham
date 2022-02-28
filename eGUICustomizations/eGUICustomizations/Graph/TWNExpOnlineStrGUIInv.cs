using System;
using System.Collections.Generic;
using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AR;
using eGUICustomizations.DAC;
using eGUICustomizations.Descriptor;

namespace eGUICustomizations.Graph
{
    public class TWNExpOnlineStrGUIInv : PXGraph<TWNExpOnlineStrGUIInv>
    {
        #region Features
        public PXCancel<TWNGUITrans> Cancel;
        public PXProcessing<TWNGUITrans,
                            Where<TWNGUITrans.eGUIExcluded, Equal<False>,
                                  And<TWNGUITrans.gUIFormatcode, Equal<TWNExpGUIInv2BankPro.VATOutCode35>,
                                       And<Where<TWNGUITrans.eGUIExported, Equal<False>,
                                                 Or<TWNGUITrans.eGUIExported, IsNull>>>>>> GUITranProc;
        //public PXProcessing<TWNGUITrans,
        //                    Where<TWNGUITrans.eGUIExcluded, Equal<False>,
        //                          And<TWNGUITrans.gUIFormatcode, Equal<VATOutCode35>,
        //                               And2<Where<TWNGUITrans.eGUIExported, Equal<False>,
        //                                          Or<TWNGUITrans.eGUIExported, IsNull>>,
        //                                    And<Where<TWNGUITrans.taxNbr, IsNull,
        //                                              Or<TWNGUITrans.taxNbr, Equal<StringEmpty>>>>>>>> GUITranProc;
        #endregion

        #region Ctor
        public TWNExpOnlineStrGUIInv()
        {
            GUITranProc.SetProcessCaption(ActionsMessages.Upload);
            GUITranProc.SetProcessAllCaption(TWMessages.UploadAll);
            GUITranProc.SetProcessDelegate(Upload);
        }
        #endregion

        #region Static Method
        public static void Upload(List<TWNGUITrans> tWNGUITrans)
        {
            try
            {
                // Avoid to create empty content file in automation schedule.
                if (tWNGUITrans.Count == 0) { return; }

                TWNExpOnlineStrGUIInv graph   = CreateInstance<TWNExpOnlineStrGUIInv>();
                TWNExpGUIInv2BankPro invGraph = CreateInstance<TWNExpGUIInv2BankPro>();

                string lines = "", fileName = "";

                string verticalBar = TWNExpGUIInv2BankPro.verticalBar;

                TWNGUIPreferences preferences = PXSelect<TWNGUIPreferences>.Select(graph);

                fileName = preferences.OurTaxNbr + "-O-" + DateTime.Today.ToString("yyyyMMdd") + "-" + DateTime.Now.ToString("hhmmss") + ".txt";

                foreach (TWNGUITrans gUITrans in tWNGUITrans)
                {
                    // 主檔代號
                    lines += "M" + verticalBar;
                    // 訂單編號
                    lines += gUITrans.OrderNbr + verticalBar;
                    // 訂單狀態
                    lines += ((gUITrans.VATType != TWGUIFormatCode.vATOutCode33) ? "0" : "3") + verticalBar;
                    // 訂單日期
                    lines += gUITrans.TransDate.Value.ToString("yyyy/MM/dd") + verticalBar;
                    // 預計出貨日
                    lines += gUITrans.GUIDate.Value.ToString("yyyy/MM/dd") + verticalBar;
                    // 稅率別 -> 1:應稅 2:零稅率 3:免稅 4:特殊稅率(需帶36 & 37欄位) 
                    lines += (gUITrans.GUIFormatcode.IsIn(TWGUIFormatCode.vATOutCode36, TWGUIFormatCode.vATOutCode37)) ? "4" : TWNExpGUIInv2BankPro.GetTaxType(gUITrans.VATType) + verticalBar;
                    // 訂單金額(未稅)
                    lines += gUITrans.NetAmount + verticalBar;
                    // 訂單稅額
                    lines += gUITrans.TaxAmount + verticalBar;
                    // 訂單金額(含稅)
                    lines += (gUITrans.NetAmount + gUITrans.TaxAmount) + verticalBar;
                    // 賣方統一編號
                    lines += gUITrans.OurTaxNbr + verticalBar;
                    // 賣方廠編
                    lines += verticalBar;
                    // 買方統一編號
                    lines += gUITrans.TaxNbr + verticalBar;
                    // 買受人公司名稱
                    lines += verticalBar;
                    // 會員編號
                    lines += gUITrans.CustVend + verticalBar;
                    // 會員姓名
                    lines += gUITrans.GUITitle + verticalBar;
                    // 會員郵遞區號
                    lines += verticalBar;
                    // 會員地址
                    ARAddress address = SelectFrom<ARAddress>.InnerJoin<ARInvoice>.On<ARInvoice.billAddressID.IsEqual<ARAddress.addressID>>.Where<ARInvoice.refNbr.IsEqual<@P.AsString>>.View.SelectSingleBound(graph, null, gUITrans.OrderNbr);
                    lines += address?.AddressLine1 + verticalBar;
                    // 會員電話
                    lines += verticalBar;
                    // 會員行動電話
                    //PX.Objects.AR.Customer  customer = PXSelectReadonly<PX.Objects.AR.Customer, Where<PX.Objects.AR.Customer.acctCD, Equal<Required<PX.Objects.AR.Customer.acctCD>>>>.Select(graph, gUITrans.CustVend);
                    //PX.Objects.CR.CRContact contact = PXSelectReadonly<PX.Objects.CR.CRContact, Where<PX.Objects.CR.CRContact.contactID, Equal<Required<PX.Objects.AR.Customer.defContactID>>>>.Select(graph, customer.DefContactID);
                    ARContact contact = SelectFrom<ARContact>.InnerJoin<ARInvoice>.On<ARInvoice.billContactID.IsEqual<ARContact.contactID>>.Where<ARInvoice.refNbr.IsEqual<@P.AsString>>.View.SelectSingleBound(graph, null, gUITrans.OrderNbr);
                    lines += contact?.Phone1 + verticalBar;
                    // 會員電子郵件
                    lines += contact?.Email + verticalBar;
                    // 紅利點數折扣金額
                    lines += verticalBar;
                    // 索取紙本發票                       
                    lines += "N" + verticalBar;
                    // 發票捐贈註記
                    lines += gUITrans.NPONbr + verticalBar;
                    // 訂單註記
                    // 付款方式
                    // 相關號碼1(出貨單號)
                    lines += new string(char.Parse(verticalBar), 3);
                    // 相關號碼2
                    lines += gUITrans.BatchNbr + verticalBar;
                    // 相關號碼3
                    // 主檔備註
                    // 商品名稱
                    lines += new string(char.Parse(verticalBar), 3);
                    // 載具類別號碼
                    lines += gUITrans.CarrierType + verticalBar;
                    // 載具顯碼id1(明碼)
                    lines += gUITrans.CarrierID + verticalBar;
                    // 載具隱碼id2(內碼)
                    lines += verticalBar;
                    // 發票號碼
                    lines += gUITrans.GUINbr + verticalBar;
                    // 隨機碼
                    lines += gUITrans.OrderNbr.Substring(gUITrans.OrderNbr.Length - 4, 4) + verticalBar;
                    // 稅率代碼
                    // 稅率
                    lines += "0" + verticalBar + "\r\n";

                    int num = 1;
                    foreach (PXResult<PX.Objects.AR.ARTran> result in invGraph.RetrieveARTran(gUITrans.OrderNbr))
                    {
                        PX.Objects.AR.ARTran aRTran = result;

                        // 明細代號
                        lines += "D" + verticalBar;
                        // 序號
                        lines += num++ + verticalBar;
                        // 訂單編號
                        lines += aRTran.RefNbr + verticalBar;
                        // 商品編號
                        // 商品條碼
                        lines += new string(char.Parse(verticalBar), 2);
                        // 商品名稱
                        lines += System.Text.RegularExpressions.Regex.Replace((aRTran.TranDesc ?? "").Replace("'", @"\'").Trim(), @"[\r\n]+", "") + verticalBar;
                        // 商品規格
                        // 單位
                        // 單價
                        lines += new string(char.Parse(verticalBar), 3);
                        // 數量
                        lines += (aRTran.Qty == 0m ? 1 : aRTran.Qty) + verticalBar;
                        // 未稅金額
                        lines += verticalBar;
                        // 含稅金額
                        PX.Objects.AR.ARInvoice invoice = PX.Objects.AR.ARInvoice.PK.Find(graph, aRTran.TranType, aRTran.RefNbr);
                        lines += (invoice.TaxCalcMode != PX.Objects.TX.TaxCalculationMode.Gross ? aRTran.CuryTranAmt * (decimal)1.05 : aRTran.CuryTranAmt) + verticalBar;
                        // 健康捐
                        lines += "0" + verticalBar;
                        // 稅率別
                        lines += TWNExpGUIInv2BankPro.GetTaxType(gUITrans.VATType) + verticalBar;
                        // 紅利點數折扣金額
                        // 明細備註
                        lines += new string(char.Parse(verticalBar), 1) + "\r\n";
                    }

                    // The following method is only for voided invoice.
                    if (gUITrans.GUIStatus == TWNStringList.TWNGUIStatus.Voided)
                    {
                        TWNExpGUIInv2BankPro.CreateVoidedDetailLine(verticalBar, gUITrans.OrderNbr, ref lines);
                    }
                }

                // Total Records
                lines += tWNGUITrans.Count;

                invGraph.UploadFile2FTP(fileName, lines);
                invGraph.UpdateGUITran(tWNGUITrans);
            }
            catch (Exception ex)
            {
                PXProcessing<TWNGUITrans>.SetError(ex);
                throw;
            }
        }
        #endregion
    }
}