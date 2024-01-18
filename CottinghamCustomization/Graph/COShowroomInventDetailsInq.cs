using System;
using System.Collections;
using System.Collections.Generic;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Data.Maintenance.GI;
using PX.Data.Update;
using PX.Objects.AR;
using PX.Objects.CS;
using PX.Objects.IN;
using CottinghamCustomization.Descriptor;

namespace CottinghamCustomization
{
    public class COShowroomInventDetailsInq : PXGraph<COShowroomInventDetailsInq>
    {
		#region Constant string Classes & Variables
		private const string GIHasModified = "The Source Generic Inquiry Has Been Modified To Be Different From Current One.";

		public const string PriceClass_WS = "WS";
		public class PriceClassName_WS : PX.Data.BQL.BqlString.Constant<PriceClassName_WS>
        {
			public PriceClassName_WS() : base(PriceClass_WS) { }
        }
		public const string Warehouse_SRW = "SRW";
		public class WarehouseName_SRW : PX.Data.BQL.BqlString.Constant<WarehouseName_SRW>
		{
			public WarehouseName_SRW() : base(Warehouse_SRW) { }
		}
        #endregion

        #region Selects & Features
        //public PXCancel<COShowroomDetailsFilter> Cancel;
		public PXFilter<COShowroomDetailsFilter> Filter;
		[PXFilterable()]
        public SelectFrom<COInventoryDetails>.OrderBy<COInventoryDetails.inventoryID.Asc, COInventoryDetails.postClassID.Asc>.View.ReadOnly InventDetails;
		#endregion

		#region Delegate Date Views
		public virtual IEnumerable inventDetails()
        {
			List<object> lists = new List<object>();

            try
            {
				string companyName = PXCompanyHelper.FindCompany(Convert.ToInt32(Filter.Current?.TenantID))?.LoginName;

				using (new PXLoginScope($"{Accessinfo.UserName}@{companyName}"))
				{
                    //PXGenericInqGrph giGraph = PXGenericInqGrph.CreateInstance("Inventory for Prestige");
                    PXGenericInqGrph giGraph = PXGenericInqGrph.CreateInstance((Guid)SelectFrom<GIDesign>.Where<GIDesign.name.IsEqual<P.AsString>>
																										 .View.SelectSingleBound(this, null, "Inventory for Prestige")
																										 .TopFirst?.DesignID);

                    foreach (GenericResult item in giGraph.Results.Select())
					{
						ARSalesPrice	 salesPrice = item.Values[nameof(ARSalesPrice)] as ARSalesPrice;
						InventoryItem	 inventory  = item.Values["Item"] as InventoryItem;
						INPostClass		 postClass  = item.Values["PostClass"] as INPostClass;
						INLocationStatus locStatus  = item.Values["InventorySiteStatus"] as INLocationStatus;

						COInventoryDetails details = new COInventoryDetails()
						{
							InventoryID    = GetSegmentDimension(InventoryRawAttribute.DimensionName, InventoryItem.PK.Find(giGraph, locStatus.InventoryID)?.InventoryCD),
							InventDescr    = inventory.Descr,
							SiteCD         = INSite.PK.Find(giGraph, locStatus.SiteID)?.SiteCD,
							LocationCD     = INLocation.PK.Find(giGraph, locStatus.LocationID).LocationCD,
							//QtyAvail = locStatus.QtyAvail,
							SalesPrice     = salesPrice.SalesPrice,
							PostClassID	   = postClass.PostClassID,
							PostClassDescr = postClass.Descr,
							ItemClassCD    = GetSegmentDimension(INItemClass.Dimension, INItemClass.PK.Find(giGraph, inventory.ItemClassID)?.ItemClassCD)
						};

						Dictionary<string, object> dicFormula = (Dictionary<string, object>)item.Values[nameof(GenericResult)];

						foreach (string key in dicFormula.Keys)
						{
							dicFormula.TryGetValue(key, out object value);

							details.QtyAvail = Convert.ToDecimal(value ?? 0m);
						}

						lists.Add(details);
					}
				}
			}
            catch (Exception)
            {
                throw new PXException(GIHasModified);
            }
			
			return lists;
		}
        #endregion

        //#region Event Handlers
        //protected virtual void _(Events.RowSelected<COShowroomDetailsFilter> e) => PXUIFieldAttribute.SetEnabled<COShowroomDetailsFilter.tenantID>(e.Cache, null, PXContext.PXIdentity.User.IsInRole(PXAccess.GetAdministratorRole()));
        //#endregion

        #region Methods
		private string GetSegmentDimension(string dimensionID, string source)
        {
			short count = 0;
			short length = 0;

			foreach (Segment row in SelectFrom<Segment>.Where<Segment.dimensionID.IsEqual<@P.AsString>>.View.Select(new PXGraph(), dimensionID))
            {
				length += row.Length.Value;
				length += count;

				if (length < source.Length)
				{
					source = source.Insert(length, row.Separator);

					count++;
				}
			}

			return source;
        }
        #endregion
    }

    #region Unbound DAC
    [Serializable]
	[PXCacheName("Showroom Details Filter")]
	public class COShowroomDetailsFilter : IBqlTable
    {
        #region TenantID
        [PXInt()]
		[PXUIField(DisplayName = "Source Tenant")]
		[CompanySelector(SubstituteKey = typeof(PX.SM.UPCompany.companyCD))]
		[PXUnboundDefault(4)] // 4 -> The current operating production tenant ID.
		public virtual int? TenantID { get; set; }
		public abstract class tenantID : PX.Data.BQL.BqlInt.Field<tenantID> { }
		#endregion
	}

	[Serializable]
	[PXCacheName("Showroom Inventory Details")]
    public class COInventoryDetails : IBqlTable
    {
		#region InventoryID
		[PXDBString(50, IsKey = true, IsUnicode = true)]
		[PXUIField(DisplayName = "Invnetory ID")]
        public virtual string InventoryID { get; set; }
		public abstract class inventoryID : PX.Data.BQL.BqlString.Field<inventoryID> { }
		#endregion

		#region InventDescr
		[PXDBString(PX.Objects.Common.Constants.TranDescLength, IsUnicode = true)]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual string InventDescr { get; set; }
		public abstract class inventDescr : BqlString.Field<inventDescr> { }
		#endregion

		#region SiteCD
		[PXDBString(30, IsKey = true, IsUnicode = true)]
		[PXUIField(DisplayName = "Warehouse")]
		public virtual string SiteCD { get; set; }
		public abstract class siteCD : PX.Data.BQL.BqlString.Field<siteCD> { }
		#endregion

		#region LocationCD
		[PXDBString(30, IsKey = true, IsUnicode = true)]
		[PXUIField(DisplayName = "Location")]
		public virtual string LocationCD { get; set; }
        public abstract class locationCD : PX.Data.BQL.BqlString.Field<locationCD> { }
		#endregion

		#region QtyAvail
		[PXDBDecimal(0)]
		[PXUIField(DisplayName = "Qty. Available")]
		public virtual decimal? QtyAvail { get; set; }
		public abstract class qtyAvail : PX.Data.BQL.BqlDecimal.Field<qtyAvail> { }
		#endregion

		#region SalesPrice
		[PXDBDecimal(0)]
		[PXUIField(DisplayName = "RSP Price", Visibility = PXUIVisibility.Visible)]
		public virtual decimal? SalesPrice { get; set; }
		public abstract class salesPrice : PX.Data.BQL.BqlDecimal.Field<salesPrice> { }
		#endregion

		#region PostClassID
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Brand")]
		public virtual string PostClassID { get; set; }
		public abstract class postClassID : PX.Data.BQL.BqlString.Field<postClassID> { }
		#endregion

		#region PostClassDescr
		[PXDBString(PX.Objects.Common.Constants.TranDescLength, IsUnicode = true)]
		[PXUIField(DisplayName = "Brand Descr", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual string PostClassDescr { get; set; }
		public abstract class postClassDescr : BqlString.Field<postClassDescr> { }
		#endregion

		#region ItemClassCD
		[PXDBString(30, IsUnicode = true)]
		[PXUIField(DisplayName = "Principal", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual string ItemClassCD { get; set; }
		public abstract class itemClassCD : BqlString.Field<itemClassCD> { }
		#endregion
	}
	#endregion
}