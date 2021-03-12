// Decompiled with JetBrains decompiler
// Type: PX.Objects.PO.POLineExt
// Assembly: CottinghamCustomization, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F8F49B4-1D52-44D4-A89E-7C7E4BA7A8DF
// Assembly location: C:\Program Files\Acumatica ERP\Cottingham\Bin\CottinghamCustomization.dll

using PX.Data;
using PX.Data.BQL;
using PX.Objects.CS;

namespace PX.Objects.PO
{
  public class POLineExt : PXCacheExtension<POLine>
  {
    public const string Alcohol = "ALCOHOL";

    [PXString]
    [PXUIField(DisplayName = "Alc. %", IsReadOnly = true)]
    [PXUnboundDefault(typeof (Search2<CSAnswers.value, InnerJoinSingleTable<PX.Objects.IN.InventoryItem, On<PX.Objects.IN.InventoryItem.inventoryID, Equal<Current<POLine.inventoryID>>>>, Where<CSAnswers.refNoteID, Equal<PX.Objects.IN.InventoryItem.noteID>, And<CSAnswers.attributeID, Equal<POLineExt.AlcAttr>>>>))]
    [PXFormula(typeof (Default<POLine.inventoryID>))]
    public virtual string UsrAlcoholAttr { get; set; }

    [PXString]
    [PXUIField(DisplayName = "Packaging", IsReadOnly = true)]
    public virtual string UsrPackaging { get; set; }

    public class AlcAttr : BqlType<IBqlString, string>.Constant<POLineExt.AlcAttr>
    {
      public AlcAttr()
        : base("ALCOHOL")
      {
      }
    }

    public abstract class usrAlcoholAttr : BqlType<IBqlString, string>.Field<POLineExt.usrAlcoholAttr>
    {
    }

    public abstract class usrPackaging : BqlType<IBqlString, string>.Field<POLineExt.usrPackaging>
    {
    }
  }
}
