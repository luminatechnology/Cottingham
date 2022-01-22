using PX.Data;
using PX.Data.BQL.Fluent;
using CottinghamCustomization.DAC;

namespace PX.Objects.AR
{
    public class SalesPersonMaint_Extension : PXGraphExtension<SalesPersonMaint>
    {
        public SelectFrom<COSalesPersonTarget>.Where<COSalesPersonTarget.salespersonID.IsEqual<SalesPerson.salesPersonID.FromCurrent>>.View SalesTarget;
    }
}