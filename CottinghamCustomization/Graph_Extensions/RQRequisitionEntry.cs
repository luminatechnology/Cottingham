using PX.Data;

namespace PX.Objects.RQ
{
    public class RQRequisitionEntry_Extension : PXGraphExtension<PX.Objects.RQ.RQRequisitionEntry>
	{ 
        #region Event Handlers
        [System.Obsolete("The standard event that are forced to be overwritten that needs to pay attention after upgrading.")]
		protected virtual void RQRequisition_RowUpdating(PXCache sender, PXRowUpdatingEventArgs e, PXRowUpdating baseHandler)
		{
			if (Base.Setup.Current?.GetExtension<RQSetupExt>().UsrCloseCuryCnvDialog != true)
			{
				baseHandler?.Invoke(sender, e);
			}
			else
			{
				RQRequisition row = (RQRequisition)e.Row;
				RQRequisition newRow = (RQRequisition)e.NewRow;

				if (row != null && (row.CustomerID != newRow.CustomerID || row.CustomerLocationID != newRow.CustomerLocationID))
				{
					RQRequisitionContent cont = PXSelectJoin<RQRequisitionContent, InnerJoin<RQRequest, On<RQRequest.orderNbr, Equal<RQRequisitionContent.orderNbr>>>,
																				   Where<RQRequisitionContent.reqNbr, Equal<Required<RQRequisitionContent.reqNbr>>,
																						  And<Where<RQRequest.employeeID, NotEqual<Required<RQRequest.employeeID>>,
																								   Or<RQRequest.locationID, NotEqual<Required<RQRequest.locationID>>>>>>>
																				   .SelectWindowed(Base, 0, 1, row.ReqNbr, newRow.CustomerID, newRow.CustomerLocationID);
					if (cont != null &&
						Base.Contents.Ask(Messages.AskConfirmation,
						Messages.CustomerUpdateConfirmation,
						MessageButtons.YesNo) == WebDialogResult.No)
					{
						newRow.CustomerID = row.CustomerID;
						newRow.CustomerLocationID = row.CustomerLocationID;
					}
				}

				if (newRow.VendorID != null &&
					!sender.ObjectsEqual<RQRequisition.vendorID>(e.Row, e.NewRow) &&
					PXAccess.FeatureInstalled<CS.FeaturesSet.multicurrency>())
				{
					AP.Vendor vendor = (AP.Vendor)Base.vendor.Cache.CreateCopy(Base.vendor.View.SelectSingleBound(new object[] { newRow }));

					RQBiddingVendor bidder = PXSelect<RQBiddingVendor, Where<RQBiddingVendor.reqNbr, Equal<Required<RQBiddingVendor.reqNbr>>,
																			  And<RQBiddingVendor.vendorID, Equal<Required<RQBiddingVendor.vendorID>>,
																				 And<RQBiddingVendor.vendorLocationID, Equal<Required<RQBiddingVendor.vendorLocationID>>>>>>
																	   .SelectWindowed(Base, 0, 1, newRow.ReqNbr,
																									  newRow.VendorID,
																								   newRow.VendorLocationID);

					CM.CurrencyInfo vendorInfo = bidder != null ? Base.currencyinfo.Select(bidder.CuryInfoID) : null;

					if (vendorInfo != null)
					{
						vendor.CuryID = vendorInfo.CuryID;
						vendor.CuryRateTypeID = vendorInfo.CuryRateTypeID;
					}

					CM.CurrencyInfo current = Base.currencyinfo.Select(newRow.CuryInfoID);

					if (vendor.CuryID == null)
						vendor.CuryID = Base.company.Current.BaseCuryID;

					if (vendor.CuryRateTypeID == null)
						vendor.CuryRateTypeID = Base.cmsetup.Current.APRateTypeDflt;

					/* As requested by Adam, the verification below is forced to be bypassed because the operation stops even if the "Yes" button is clicked.
					string message = null;

					if (vendor.CuryID != current.CuryID)
						message = PXMessages.LocalizeFormatNoPrefixNLA(Messages.RequisitionVendorCuryIDValidation, vendor.CuryID);
					else if (current.CuryRateTypeID != null && vendor.CuryRateTypeID != current.CuryRateTypeID)
						message = PXMessages.LocalizeFormatNoPrefixNLA(Messages.RequisitionVendorCuryRateIDValidation, vendor.CuryRateTypeID);

					if (message != null && Base.Document.Ask(row, Messages.Warning, message, MessageButtons.YesNo) != WebDialogResult.Yes)
					{
						e.Cancel = true;
					}*/
				}
			}
		}

		protected void _(Events.FieldSelecting<RQRequisitionLineExt.usrPackaging> e)
        {
            if (!(e.Row is RQRequisitionLine row) || !row.InventoryID.HasValue) { return; }

            e.ReturnValue = PO.POOrderEntry_Extension.UOMConversion(Base, row.InventoryID.Value, row.OrderQty.Value, row.UOM);
        }
		#endregion
	}
}