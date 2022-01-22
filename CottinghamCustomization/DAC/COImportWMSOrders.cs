using System;
using PX.Data;

namespace CottinghamCustomization.DAC
{
    [Serializable]
    [PXCacheName("Import WMS Orders")]
    public class COImportWMSOrders : IBqlTable
    {
        #region OrderNbr
        [PXDBString(15, IsKey = true, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Order Nbr")]
        public virtual string OrderNbr { get; set; }
        public abstract class orderNbr : PX.Data.BQL.BqlString.Field<orderNbr> { }
        #endregion
    
        #region LineNbr
        [PXDBInt(IsKey = true)]
        [PXUIField(DisplayName = "Line Nbr")]
        public virtual int? LineNbr { get; set; }
        public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
        #endregion
    
        #region OrderType
        [PXDBString(2, IsFixed = true, InputMask = "")]
        [PXUIField(DisplayName = "Order Type")]
        [ImportOrderType.List]
        public virtual string OrderType { get; set; }
        public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }
        #endregion
    
        #region InventoryID
        [PX.Objects.IN.Inventory()]
        public virtual int? InventoryID { get; set; }
        public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
        #endregion

        #region OrderedQty
        [PX.Objects.IN.PXDBQuantity()]
        [PXUIField(DisplayName = "Ordered Qty")]
        public virtual decimal? OrderedQty { get; set; }
        public abstract class orderedQty : PX.Data.BQL.BqlDecimal.Field<orderedQty> { }
        #endregion
    
        #region ActualQty
        [PX.Objects.IN.PXDBQuantity()]
        [PXUIField(DisplayName = "Actual Qty")]
        public virtual decimal? ActualQty { get; set; }
        public abstract class actualQty : PX.Data.BQL.BqlDecimal.Field<actualQty> { }
        #endregion

        #region DifferenceQty
        [PX.Objects.IN.PXDBQuantity()]
        [PXUIField(DisplayName = "Difference Qty")]
        public virtual decimal? DifferenceQty { get; set; }
        public abstract class differenceQty : PX.Data.BQL.BqlDecimal.Field<differenceQty> { }
        #endregion
    
        #region UOM
        [PXDBString(6, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "UOM")]
        public virtual string UOM { get; set; }
        public abstract class uOM : PX.Data.BQL.BqlString.Field<uOM> { }
        #endregion
    
        #region OrderDate
        [PXDBString(10, IsFixed = true, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Order Date")]
        public virtual string OrderDate { get; set; }
        public abstract class orderDate : PX.Data.BQL.BqlString.Field<orderDate> { }
        #endregion
    
        #region CreatedDateTime
        [PXDBCreatedDateTime()]
        public virtual DateTime? CreatedDateTime { get; set; }
        public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
        #endregion
    
        #region CreatedByID
        [PXDBCreatedByID()]
        public virtual Guid? CreatedByID { get; set; }
        public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
        #endregion
    
        #region CreatedByScreenID
        [PXDBCreatedByScreenID()]
        public virtual string CreatedByScreenID { get; set; }
        public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
        #endregion
    
        #region LastModifiedDateTime
        [PXDBLastModifiedDateTime()]
        public virtual DateTime? LastModifiedDateTime { get; set; }
        public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
        #endregion
    
        #region LastModifiedByID
        [PXDBLastModifiedByID()]
        public virtual Guid? LastModifiedByID { get; set; }
        public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
        #endregion
    
        #region LastModifiedByScreenID
        [PXDBLastModifiedByScreenID()]
        public virtual string LastModifiedByScreenID { get; set; }
        public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
        #endregion
    }

    public static class ImportOrderType
    {
        public const string POReceipt = "RT";
        public const string SOShipment = "ST";

        public class ListAttribute : PXStringListAttribute
        {
            public ListAttribute() : base(
                new[]
                {
                    Pair(POReceipt, PX.Objects.PO.Messages.PurchaseReceipt),
                    Pair(SOShipment, PX.Objects.SO.Messages.SOShipment),
                })
            { }

            internal bool TryGetValue(string label, out string value)
            {
                var index = Array.IndexOf(_AllowedLabels, label);
                if (index >= 0)
                {
                    value = _AllowedValues[index];
                    return true;
                }
                value = null;
                return false;
            }
        }

        public class poReceipt : PX.Data.BQL.BqlString.Constant<poReceipt>
        {
            public poReceipt() : base(POReceipt) { }
        }

        public class soShipment : PX.Data.BQL.BqlString.Constant<soShipment>
        {
            public soShipment() : base(SOShipment) { }
        }
    }
}