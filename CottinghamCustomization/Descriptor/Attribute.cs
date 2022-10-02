using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Data.Update;
using PX.Objects.IN;
using PX.SM;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace CottinghamCustomization.Descriptor
{
	#region CompanySelectorAttribute
	/// <summary>
	/// Copy the attribute class from PX.SM namespace in PX.Data.dll reference.
	/// </summary>
	internal class CompanySelectorAttribute : PXCustomSelectorAttribute
	{
		public CompanySelectorAttribute() : base(typeof(UPCompany.companyID)) { }

		protected virtual IEnumerable GetRecords()
		{
			return PXCompanyHelper.SelectCompanies(PXCompanySelectOptions.Visible);
		}

		public override void DescriptionFieldSelecting(PXCache sender, PXFieldSelectingEventArgs e, string alias)
		{
			if (e.Row == null || sender.GetValue(e.Row, this._FieldOrdinal) == null)
			{
				base.DescriptionFieldSelecting(sender, e, alias);
				return;
			}
			UPCompany item = null;
			int key = (int)sender.GetValue(e.Row, this._FieldOrdinal);
			foreach (UPCompany info in PXCompanyHelper.SelectCompanies(PXCompanySelectOptions.Visible))
			{
				int? companyID = info.CompanyID;
				int num = key;
				if (companyID.GetValueOrDefault() == num & companyID != null)
				{
					item = info;
					break;
				}
			}
			if (item != null)
			{
				e.ReturnValue = sender.Graph.Caches[this._Type].GetValue(item, this._DescriptionField.Name);
			}
		}
	}
	#endregion

	#region GlobalStockItemAttribute
	public class GlobalStockItemAttribute : PXCustomSelectorAttribute
	{
		public GlobalStockItemAttribute() : base(typeof(InventoryItem.inventoryID))
		{
            SubstituteKey = typeof(InventoryItem.inventoryCD);
            DescriptionField = typeof(InventoryItem.descr);
            //PXDimensionSelectorAttribute attr = new PXDimensionSelectorAttribute("INVENTORY", typeof(Search<InventoryItem.inventoryID>), typeof(InventoryItem.inventoryCD));
            //attr.CacheGlobal = true;
            //attr.DescriptionField = DescriptionField;
            //_Attributes.Add(attr);
            //_SelAttrIndex = _Attributes.Count - 1;
        }

		protected virtual IEnumerable GetRecords()
		{
			using (new PXLoginScope($"admin@{PXCompanyHelper.FindCompany(2)?.LoginName}"))
			{
				List<InventoryItem> items = SelectFrom<InventoryItem>.Where<InventoryItem.itemStatus.IsEqual<INItemStatus.active>>.View.Select(new PXGraph()).RowCast<InventoryItem>().ToList();

				return items;
			}
		}
	}
	#endregion
}
