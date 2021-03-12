// Decompiled with JetBrains decompiler
// Type: PX.Objects.SO.SO_SOOrder_ExistingColumn
// Assembly: CottinghamCustomization, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F8F49B4-1D52-44D4-A89E-7C7E4BA7A8DF
// Assembly location: C:\Program Files\Acumatica ERP\Cottingham\Bin\CottinghamCustomization.dll

using PX.Data;
using PX.Objects.CT;
using PX.Objects.PM;

namespace PX.Objects.SO
{
  [PXNonInstantiatedExtension]
  public class SO_SOOrder_ExistingColumn : PXCacheExtension<SOOrder>
  {
    [ProjectDefault("SO", typeof (Search<PX.Objects.CR.Location.cDefProjectID, Where<PX.Objects.CR.Location.bAccountID, Equal<Current<SOOrder.customerID>>, And<PX.Objects.CR.Location.locationID, Equal<Current<SOOrder.customerLocationID>>>>>))]
    [PXRestrictor(typeof (Where<Contract.isCancelled, Equal<False>>), "The {0} project or contract is canceled.", new System.Type[] {typeof (PMProject.contractCD)})]
    [PXRestrictor(typeof (Where<PMProject.visibleInSO, Equal<True>, Or<PMProject.nonProject, Equal<True>>>), "The '{0}' project is invisible in the module.", new System.Type[] {typeof (PMProject.contractCD)})]
    [ProjectBase]
    public int? ProjectID { get; set; }
  }
}
