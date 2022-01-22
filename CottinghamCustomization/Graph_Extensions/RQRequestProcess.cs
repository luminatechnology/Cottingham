using PX.Data;
using PX.Objects.CS;
using PX.Objects.PO;
using System;
using System.Collections.Generic;
using static PX.Objects.RQ.RQRequestProcess;

namespace PX.Objects.RQ
{
    public class RQRequestProcess_Extensions : PXGraphExtension<PX.Objects.RQ.RQRequestProcess>
    {
        #region Event Handlers
        [Obsolete("The standard event that are forced to be overwritten that needs to pay attention after upgrading.")]
        protected void _(Events.RowSelected<RQRequestSelection> e, PXRowSelected baseHandler)
        {
            if (Base.Setup.Current?.GetExtension<RQSetupExt>().UsrCloseCuryCnvDialog != true)
            {
                baseHandler?.Invoke(e.Cache, e.Args);
            }
            else
            {
                RQRequestSelection filter = Base.Filter.Current;
                Base.Records.SetProcessDelegate(list => GenerateRequisition2(filter, list));
            }
        }
        #endregion

        #region Private Static Methods
        private static void GenerateRequisition2(RQRequestSelection filter, List<RQRequestLineOwned> lines)
		{
			RQRequisitionEntry graph = PXGraph.CreateInstance<RQRequisitionEntry>();
			graph.Clear();

            try
            {
                GenerateRequisition(filter, lines, graph);
            }
            catch (PXBaseRedirectException)
            {
                for (int i = 0; i < lines.Count; i++)
                {
                    PXProcessing<RQRequestLine>.SetInfo(i, PXMessages.LocalizeFormatNoPrefixNLA(Messages.RequisitionCreated, graph.Document.Current.ReqNbr));
                }

                throw;
            }
            catch (Exception exception)
            {
                for (int i = 0; i < lines.Count; i++)
                {
                    PXProcessing<RQRequestLine>.SetError(i, exception);
                }
                return;
            }
		}

        [Obsolete("The standard Private method of copying needs to pay attention after upgrading.")]
        private static void GenerateRequisition(RQRequestSelection filter, List<RQRequestLineOwned> lines, RQRequisitionEntry graph)
        {
            RQRequisition requisition = (RQRequisition)graph.Document.Cache.CreateInstance();
            graph.Document.Insert(requisition);
            requisition.ShipDestType = null;

            bool isCustomerSet = true;
            bool isVendorSet = true;
            bool isShipToSet = true;
            int? shipContactID = null;
            int? shipAddressID = null;
            var vendors = new HashSet<VendorRef>();

            foreach (RQRequestLine line in lines)
            {
                PXResult<RQRequest, RQRequestClass> e = (PXResult<RQRequest, RQRequestClass>)PXSelectJoin<RQRequest, InnerJoin<RQRequestClass, On<RQRequestClass.reqClassID, Equal<RQRequest.reqClassID>>>,
                                                                                                                     Where<RQRequest.orderNbr, Equal<Required<RQRequest.orderNbr>>>>.Select(graph, line.OrderNbr);

                RQRequest req = e;
                RQRequestClass reqclass = e;

                requisition = PXCache<RQRequisition>.CreateCopy(graph.Document.Current);

                if (reqclass.CustomerRequest == true && isCustomerSet)
                {
                    if (requisition.CustomerID == null)
                    {
                        requisition.CustomerID = req.EmployeeID;
                        requisition.CustomerLocationID = req.LocationID;
                    }
                    else if (requisition.CustomerID != req.EmployeeID || requisition.CustomerLocationID != req.LocationID)
                    {
                        isCustomerSet = false;
                    }
                }
                else
                    isCustomerSet = false;

                if (isShipToSet)
                {
                    if (shipContactID == null && shipAddressID == null)
                    {
                        requisition.ShipDestType = req.ShipDestType;
                        requisition.SiteID = req.SiteID;
                        requisition.ShipToBAccountID = req.ShipToBAccountID;
                        requisition.ShipToLocationID = req.ShipToLocationID;
                        shipContactID = req.ShipContactID;
                        shipAddressID = req.ShipAddressID;
                    }
                    else if (requisition.ShipDestType != req.ShipDestType ||
                            requisition.SiteID != req.SiteID ||
                            requisition.ShipToBAccountID != req.ShipToBAccountID ||
                            requisition.ShipToLocationID != req.ShipToLocationID)
                    {
                        isShipToSet = false;
                    }
                }

                if (line.VendorID != null && line.VendorLocationID != null)
                {
                    VendorRef vendor = new VendorRef()
                    {
                        VendorID = line.VendorID.Value,
                        LocationID = line.VendorLocationID.Value
                    };

                    vendors.Add(vendor);

                    if (isVendorSet)
                    {
                        if (requisition.VendorID == null)
                        {
                            requisition.VendorID = line.VendorID;
                            requisition.VendorLocationID = line.VendorLocationID;
                        }
                        else if (requisition.VendorID != line.VendorID ||
                                 requisition.VendorLocationID != line.VendorLocationID)
                            isVendorSet = false;
                    }
                }
                else
                    isVendorSet = false;

                if (!isCustomerSet)
                {
                    requisition.CustomerID = null;
                    requisition.CustomerLocationID = null;
                }

                if (!isVendorSet)
                {
                    requisition.VendorID = null;
                    requisition.VendorLocationID = null;
                    requisition.RemitAddressID = null;
                    requisition.RemitContactID = null;
                }
                else if (requisition.VendorID == req.VendorID && requisition.VendorLocationID == req.VendorLocationID)
                {
                    requisition.RemitAddressID = req.RemitAddressID;
                    requisition.RemitContactID = req.RemitContactID;
                }

                if (!isShipToSet)
                {
                    requisition.ShipDestType = PX.Objects.PO.POShippingDestination.CompanyLocation;
                    graph.Document.Cache.SetDefaultExt<RQRequisition.shipToBAccountID>(requisition);
                }

                graph.Document.Update(requisition);

                if (line.OpenQty > 0)
                {
                    if (!graph.Lines.Cache.IsDirty && req.CuryID != requisition.CuryID)
                    {
                        requisition = PXCache<RQRequisition>.CreateCopy(graph.Document.Current);
                        requisition.CuryID = req.CuryID;
                        graph.Document.Update(requisition);
                    }

                    graph.InsertRequestLine(line, line.OpenQty.GetValueOrDefault(), filter.AddExists == true);
                }
            }

            if (isShipToSet)
            {
                foreach (PXResult<POAddress, POContact> res in PXSelectJoin<POAddress, CrossJoin<POContact>,
                                                                                       Where<POAddress.addressID, Equal<Required<RQRequisition.shipAddressID>>,
                                                                                             And<POContact.contactID, Equal<Required<RQRequisition.shipContactID>>>>>.Select(graph, shipAddressID, shipContactID))
                {
                    AddressAttribute.CopyRecord<RQRequisition.shipAddressID>(graph.Document.Cache, graph.Document.Current, (POAddress)res, true);
                    AddressAttribute.CopyRecord<RQRequisition.shipContactID>(graph.Document.Cache, graph.Document.Current, (POContact)res, true);
                }
            }

            if (requisition.VendorID == null && vendors.Count > 0)
            {
                foreach (var vendor in vendors)
                {
                    RQBiddingVendor bid = PXCache<RQBiddingVendor>.CreateCopy(graph.Vendors.Insert());
                    bid.VendorID = vendor.VendorID;
                    bid.VendorLocationID = vendor.LocationID;
                    graph.Vendors.Update(bid);
                }
            }

            if (graph.Lines.Cache.IsDirty)
            {
                graph.Save.Press();
                throw new PXRedirectRequiredException(graph, string.Format(Messages.RequisitionCreated, graph.Document.Current.ReqNbr));
            }
        }
        #endregion
    }
}
