// Decompiled with JetBrains decompiler
// Type: PX.Objects.IN.INTranExt
// Assembly: CottinghamCustomization, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F8F49B4-1D52-44D4-A89E-7C7E4BA7A8DF
// Assembly location: C:\Program Files\Acumatica ERP\Cottingham\Bin\CottinghamCustomization.dll

using PX.Data;
using PX.Data.BQL;

namespace PX.Objects.IN
{
  public class INTranExt : PXCacheExtension<INTran>
  {
    [PXUIField(DisplayName = "SO Customer", IsReadOnly = true)]
    [PXDimensionSelector("BIZACCT", typeof (PX.Objects.AR.Customer.bAccountID), typeof (PX.Objects.AR.Customer.bAccountID), new System.Type[] {typeof (PX.Objects.AR.Customer.acctName)})]
    [PXDBScalar(typeof (Search2<PX.Objects.AR.Customer.bAccountID, InnerJoin<PX.Objects.SO.SOOrder, On<PX.Objects.SO.SOOrder.customerID, Equal<PX.Objects.AR.Customer.bAccountID>>>, Where<PX.Objects.SO.SOOrder.orderType, Equal<INTran.sOOrderType>, And<PX.Objects.SO.SOOrder.orderNbr, Equal<INTran.sOOrderNbr>>>>))]
    public virtual int? UsrSOCustomerID { get; set; }

    public abstract class usrSOCustomerID : BqlType<IBqlInt, int>.Field<INTranExt.usrSOCustomerID>
    {
    }
  }
}
