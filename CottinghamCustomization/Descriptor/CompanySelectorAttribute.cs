using PX.Data;
using PX.Data.Update;
using PX.SM;
using System.Collections;

namespace CottinghamCustomization.Descriptor
{
	/// <summary>
	/// Copy the attribute class from PX.SM namespace in PX.Data.dll reference.
	/// </summary>
	internal class CompanySelectorAttribute : PXCustomSelectorAttribute
	{
		public CompanySelectorAttribute() : base(typeof(UPCompany.companyID))
		{
		}

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
}
