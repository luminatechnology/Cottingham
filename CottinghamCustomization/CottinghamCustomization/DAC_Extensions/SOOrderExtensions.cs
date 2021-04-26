using PX.Data;
using PX.Objects.CT;
using PX.Objects.PM;

namespace PX.Objects.SO
{
    [PXNonInstantiatedExtension]
    public class SO_SOOrder_ExistingColumn : PXCacheExtension<SOOrder>
    {
        [ProjectDefault(GL.BatchModule.SO, typeof (Search<PX.Objects.CR.Location.cDefProjectID, Where<PX.Objects.CR.Location.bAccountID, Equal<Current<SOOrder.customerID>>, 
                                                                                                      And<PX.Objects.CR.Location.locationID, Equal<Current<SOOrder.customerLocationID>>>>>))]
        [PXRestrictor(typeof(Where<PMProject.isCancelled, Equal<False>>), 
                      PM.Messages.CancelledContract, 
                      typeof(PMProject.contractCD))]
        [PXRestrictor(typeof(Where<PMProject.visibleInSO, Equal<True>, Or<PMProject.nonProject, Equal<True>>>), 
                      PM.Messages.ProjectInvisibleInModule, 
                      typeof(PMProject.contractCD))]
        [ProjectBase]
        //[ProjectBaseAttribute(typeof(SOOrder.customerID))]
        public int? ProjectID { get; set; }
    }
}
