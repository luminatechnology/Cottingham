using System.Linq;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;

namespace PX.Objects.IN
{
    public class InventoryItemMaint_Extension : PXGraphExtension<InventoryItemMaint>
    {
        //public const string VendInvtNonExist = "No Vendor Details / Default Value Not Set
        public const string SourceTenant = "Cottingham TPE";
        public const string DestinationTen = "Prestige House";

        #region Override Methods
        public delegate void PersitsDelegate();
        [PXOverride]
        public void Persist(PersitsDelegate baseMethod)
        {
            try
            {
                bool insertData = Base.Item.Cache.Inserted.RowCast<InventoryItem>().Count() > 0;

                baseMethod();

                if (insertData == true && Base.Accessinfo.CompanyName.Equals(SourceTenant) == true)
                {
                    InventoryItem fromItem = Base.Item.Current;

                    string fromItemClass = INItemClass.PK.Find(Base, fromItem.ItemClassID).ItemClassCD;

                    using (new PXLoginScope($"{Base.Accessinfo.UserName}@{PX.Data.Update.PXCompanyHelper.SelectCompanies().Where(w => w.CompanyCD.Equals(DestinationTen)).FirstOrDefault().LoginName}"))
                    {
                        InventoryItemMaint toGraph = PXGraph.CreateInstance<InventoryItemMaint>();

                        InventoryItem toItem = toGraph.Item.Cache.CreateInstance() as InventoryItem;

                        toItem.InventoryCD = fromItem.InventoryCD;
                        toItem.Descr       = fromItem.Descr;

                        toItem = toGraph.Item.Insert(toItem);

                        ///<remarks> The record must be inserted before the key field [Item class] is updated to bypass any special errors. </remarks>
                        toItem.ItemClassID = GetCrossCompanySimilarityItemClass(fromItemClass);

                        toGraph.Item.Update(toItem);

                        toGraph.Save.Press();
                    }
                }
            }  
            catch (PXException e)
            {
                throw e;
            }
        }
        #endregion

        #region Event Handlers
        //protected void _(Events.RowPersisting<InventoryItem> e, PXRowPersisting baseHandler)
        //{
        //    baseHandler?.Invoke(e.Cache, e.Args);

        //    bool? invaild = Base.VendorItems.Current != null;

        //    foreach (POVendorInventory row in Base.VendorItems.Select())
        //    {
        //        invaild = (row.IsDefault ?? false) && (row.Active ?? false);

        //        if (invaild == true) { break; }
        //    }

        //    if (invaild == false && (e.Operation == PXDBOperation.Insert || e.Operation == PXDBOperation.Update) && Base.IsImport == false)
        //    {
        //        throw new PXSetPropertyException(VendInvtNonExist);
        //    }
        //}
        #endregion

        #region Methods
        private int? GetCrossCompanySimilarityItemClass(string source) => SelectFrom<INItemClass>.Where<INItemClass.itemClassCD.EndsWith<@P.AsString>>.View.SelectSingleBound(Base, null, source).TopFirst?.ItemClassID;
        #endregion
    }
}