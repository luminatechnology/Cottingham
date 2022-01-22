// Decompiled with JetBrains decompiler
// Type: PX.Objects.SO.SOLineExt
// Assembly: CottinghamCustomization, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F8F49B4-1D52-44D4-A89E-7C7E4BA7A8DF
// Assembly location: C:\Program Files\Acumatica ERP\Cottingham\Bin\CottinghamCustomization.dll

using PX.Data;
using PX.Data.BQL;
using PX.Objects.CS;
using PX.Objects.PO;

namespace PX.Objects.SO
{
  public class SOLineExt : PXCacheExtension<SOLine>
  {
    [PXString]
    [PXUIField(DisplayName = "Alc. %", IsReadOnly = true)]
    [PXUnboundDefault(typeof (Search2<CSAnswers.value, InnerJoinSingleTable<PX.Objects.IN.InventoryItem, On<PX.Objects.IN.InventoryItem.inventoryID, Equal<Current<SOLine.inventoryID>>>>, Where<CSAnswers.refNoteID, Equal<PX.Objects.IN.InventoryItem.noteID>, And<CSAnswers.attributeID, Equal<POLineExt.AlcAttr>>>>))]
    [PXFormula(typeof (Default<SOLine.inventoryID>))]
    public virtual string UsrAlcoholAttr { get; set; }

    public abstract class usrAlcoholAttr : BqlType<IBqlString, string>.Field<SOLineExt.usrAlcoholAttr>
    {
    }
  }
}
