using System;
using System.Collections;
using System.Collections.Generic;
using PX.Common;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Data.Update;
using PX.Objects.AR;
using PX.Objects.IN;

namespace CottinghamCustomization
{
    public class COShowroomInventDetailsInq : PXGraph<COShowroomInventDetailsInq>
    {
        #region Constant string Classes & Variables
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

		public PXCancel<COShowroomDetailsFilter> Cancel;
		public PXFilter<COShowroomDetailsFilter> Filter;
		[PXFilterable()]
        public SelectFrom<COInventoryDetails>.View.ReadOnly InventDetails;

        #region Delegate Date Views
        public virtual IEnumerable inventDetails()
        {
			List<object> lists = new List<object>();

			string companyName = PXCompanyHelper.FindCompany(Convert.ToInt32(Filter.Current?.TenantID))?.LoginName;

			using (new PXLoginScope($"{Accessinfo.UserName}@{companyName}"))
			{
				foreach (PXResult<INLocationStatus, InventoryItem, INSite, ARSalesPrice> result in SelectFrom<INLocationStatus>.InnerJoin<InventoryItem>.On<InventoryItem.inventoryID.IsEqual<INLocationStatus.inventoryID>>
																															   .InnerJoin<INSite>.On<INSite.siteID.IsEqual<INLocationStatus.siteID>>
																															   .LeftJoin<ARSalesPrice>.On<ARSalesPrice.inventoryID.IsEqual<InventoryItem.inventoryID>
																																						  .And<ARSalesPrice.breakQty.IsEqual<PX.Objects.CS.decimal0>>
																																							   .And<ARSalesPrice.custPriceClassID.IsEqual<PriceClassName_WS>>>
																															   .Where<INLocationStatus.qtyOnHand.IsNotEqual<PX.Objects.CS.decimal0>
																																	  .And<INSite.siteCD.IsEqual<WarehouseName_SRW>>>
																															   .OrderBy<InventoryItem.inventoryCD.Asc>.View.Select(new PXGraph()))
				{
					ARSalesPrice salesPrice = result;

					if (salesPrice.CustPriceClassID == PriceClass_WS &&
						(salesPrice.ExpirationDate >= this.Accessinfo.BusinessDate ||
					 	 (salesPrice.ExpirationDate == null && salesPrice.EffectiveDate <= this.Accessinfo.BusinessDate)
						 ) ||
						 string.IsNullOrEmpty(salesPrice.CustPriceClassID)
					   )
					{
						INLocationStatus locStatus = result;
						InventoryItem    item = result;

						COInventoryDetails details = new COInventoryDetails()
						{
							InventoryID = locStatus.InventoryID,
							SiteID = locStatus.SiteID,
							LocationID = locStatus.LocationID,
							QtyAvail = locStatus.QtyAvail,
							SalesPrice = salesPrice.SalesPrice,
							PostClassID = item.PostClassID,
							ItemClassID = item.ItemClassID
						};

						lists.Add(details);
					}
				}
			}

			return lists;
		}
        #endregion

        #region Event Handlers
        protected virtual void _(Events.RowSelected<COShowroomDetailsFilter> e) => PXUIFieldAttribute.SetEnabled<COShowroomDetailsFilter.tenantID>(e.Cache, null, PXContext.PXIdentity.User.IsInRole(PXAccess.GetAdministratorRole()));
        #endregion
    }

    #region Unbound DAC
    [Serializable]
	[PXCacheName("Showroom Details Filter")]
	public class COShowroomDetailsFilter : IBqlTable
    {
        #region TenantID
        [PXInt()]
		[PXUIField(DisplayName = "Tenant ID", Visibility = PXUIVisibility.SelectorVisible)]
		[Descriptor.CompanySelector(SubstituteKey = typeof(PX.SM.UPCompany.companyCD))]
		[PXDefault(2)]
		public virtual int? TenantID { get; set; }
		public abstract class tenantID : PX.Data.BQL.BqlInt.Field<tenantID> { }
		#endregion
	}

	[Serializable]
	[PXCacheName("Showroom Inventory Details")]
    public class COInventoryDetails : IBqlTable
    {
		#region InventoryID
		[StockItem(IsKey = true)]
        public virtual int? InventoryID { get; set; }
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
        #endregion

		#region SiteID
		[Site(IsKey = true)]
		public virtual int? SiteID { get; set; }
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		#endregion

		#region LocationID
		[Location(IsKey = true)]
		public virtual int? LocationID { get; set; }
        public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }
		#endregion

		#region QtyAvail
		[PXDBDecimal(0)]
		[PXUIField(DisplayName = "Qty. Available")]
		public virtual decimal? QtyAvail { get; set; }
		public abstract class qtyAvail : PX.Data.BQL.BqlDecimal.Field<qtyAvail> { }
		#endregion

		#region SalesPrice
		[PXDBDecimal(0)]
		[PXUIField(DisplayName = "WS Price", Visibility = PXUIVisibility.Visible)]
		public virtual decimal? SalesPrice { get; set; }
		public abstract class salesPrice : PX.Data.BQL.BqlDecimal.Field<salesPrice> { }
		#endregion

		#region PostClassID
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Brand")]
		[PXSelector(typeof(INPostClass.postClassID), DescriptionField = typeof(INPostClass.descr))]
		public virtual string PostClassID { get; set; }
		public abstract class postClassID : PX.Data.BQL.BqlString.Field<postClassID> { }
		#endregion

		#region ItemClassID
		[PXDBInt()]
		[PXUIField(DisplayName = "Principal", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDimensionSelector(INItemClass.Dimension, typeof(Search<INItemClass.itemClassID>), typeof(INItemClass.itemClassCD), DescriptionField = typeof(INItemClass.descr), CacheGlobal = true)]
		public virtual int? ItemClassID { get; set; }
		public abstract class itemClassID : PX.Data.BQL.BqlInt.Field<itemClassID> { }
		#endregion
	}
	#endregion
}