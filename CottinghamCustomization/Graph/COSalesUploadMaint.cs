using System;
using System.IO;
using System.Collections;
using PX.Common;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.SO;
using NPOI.XSSF.UserModel;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;

namespace CottinghamCustomization
{
    public class COSalesUploadMaint : PXGraph<COSalesUploadMaint>
    {
        public PXSave<COSalesOrderDef> Save;
        public PXCancel<COSalesOrderDef> Cancel;
        public PXFilter<COSalesOrderDef> Default;

        #region Actions
        public PXAction<COSalesOrderDef> uploadNCreateSO;
        [PXProcessButton(ImageSet = "main", ImageKey = PX.Web.UI.Sprite.Main.Upload), PXUIField(MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        public virtual IEnumerable UploadNCreateSO(PXAdapter adapter)
        {
            if (this.Default.AskExt() == WebDialogResult.OK)
            {
                const string PanelSessionKey = "ImportDataFromFile";

                PX.SM.FileInfo info = PXContext.SessionTyped<PXSessionStatePXData>().FileInfo[PanelSessionKey] as PX.SM.FileInfo;

                PXLongOperation.StartOperation(this, () =>
                {
                    ImportDataFromExcel(info, Default.Current?.CustomerID);
                });
            }

            return adapter.Get();
        }
        #endregion

        #region Methods
        public static void ImportDataFromExcel(PX.SM.FileInfo file, int? customerID)
        {
            try
            {
                const string EmptyData = "There Is No Data To Upload And Import.";

                SOOrderEntry graph = PXGraph.CreateInstance<SOOrderEntry>();

                using (Stream stream = new MemoryStream(file.BinData))
                {
                    HSSFWorkbook hssfwb = new HSSFWorkbook(stream);

                    // Get sheet
                    ISheet sheet = hssfwb.GetSheet(hssfwb.GetSheetName(0));

                    string lastOrder = sheet.GetRow(10).GetCell(1).ToString();

                    if (string.IsNullOrEmpty(lastOrder)) { throw new PXException(EmptyData); }

                    graph.Document.Insert(new SOOrder()
                    {
                        OrderType = "IN",
                        CustomerID = customerID,
                        OrderDesc = "Invoice From POS",
                        CustomerOrderNbr = lastOrder
                    });

                    string item, qty, amt;
                    for (int i = 14; i < sheet.LastRowNum; i++)
                    {
                        //§ì¨ú¬YRow ¬Ycolumn
                        item = sheet.GetRow(i).GetCell(4)?.ToString();

                        if (string.IsNullOrEmpty(item)) { break; }

                        qty = sheet.GetRow(i).GetCell(7).ToString();
                        amt = sheet.GetRow(i).GetCell(8).ToString();

                        SOLine line = new SOLine()
                        {
                            InventoryID = PX.Objects.IN.InventoryItem.UK.Find(graph, item.Replace("-", ""))?.InventoryID
                        };

                        line = graph.Transactions.Insert(line);

                        graph.Transactions.SetValueExt<SOLine.orderQty>(line, Convert.ToDecimal(qty));
                        graph.Transactions.SetValueExt<SOLine.curyExtPrice>(line, Convert.ToDecimal(amt));

                        graph.Transactions.Update(line);
                    }
                }

                graph.Actions.PressSave();
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion
    }

    #region Unbound DAC
    [Serializable]
    [PXCacheName("Create SO Default Values")]
    public class COSalesOrderDef : IBqlTable
    {
        #region CustomerID		
        [CustomerActive(typeof(Search<BAccountR.bAccountID, Where<True, Equal<True>>>), // TODO: remove fake Where after AC-101187
                        Visibility = PXUIVisibility.SelectorVisible,
                        DescriptionField = typeof(Customer.acctName),
                        Filterable = true)]
        [PXDefault("C70074")]
        public virtual int? CustomerID { get; set; }
        public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
        #endregion
    }
    #endregion
}