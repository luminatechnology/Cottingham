using System;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.GL.Descriptor;
using PX.Objects.GL.FinPeriods.TableDefinition;

namespace CottinghamCustomization.DAC
{
    [Serializable]
    [PXCacheName("Sales Person Target")]
    public class COSalesPersonTarget : IBqlTable
    {
        #region SalespersonID
        [PXDBInt(IsKey = true)]
        [PXUIField(DisplayName = "Salesperson")]
        [PXDBDefault(typeof(SalesPerson.salesPersonID))]
        [PXParent(typeof(Select<SalesPerson, Where<SalesPerson.salesPersonID, Equal<Current<salespersonID>>>>))]
        public virtual int? SalespersonID { get; set; }
        public abstract class salespersonID : PX.Data.BQL.BqlInt.Field<salespersonID> { }
        #endregion

        #region Year
        [PXDBString(4, IsKey = true, IsFixed = true)]
        [PXUIField(DisplayName = "Year")]
        [PXDefault()]
        [GenericFinYearSelector(typeof(Search3<FinYear.year, OrderBy<Desc<FinYear.year>>>), null)]
        public virtual string Year { get; set; }
        public abstract class year : PX.Data.BQL.BqlString.Field<year> { }
        #endregion

        #region Period01
        [PXDBDecimal(0)]
        [PXUIField(DisplayName = "Period 01")]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual decimal? Period01 { get; set; }
        public abstract class period01 : PX.Data.BQL.BqlDecimal.Field<period01> { }
        #endregion

        #region Period02
        [PXDBDecimal(0)]
        [PXUIField(DisplayName = "Period 02")]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual decimal? Period02 { get; set; }
        public abstract class period02 : PX.Data.BQL.BqlDecimal.Field<period02> { }
        #endregion

        #region Period03
        [PXDBDecimal(0)]
        [PXUIField(DisplayName = "Period 03")]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual decimal? Period03 { get; set; }
        public abstract class period03 : PX.Data.BQL.BqlDecimal.Field<period03> { }
        #endregion

        #region Period04
        [PXDBDecimal(0)]
        [PXUIField(DisplayName = "Period 04")]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual decimal? Period04 { get; set; }
        public abstract class period04 : PX.Data.BQL.BqlDecimal.Field<period04> { }
        #endregion

        #region Period05
        [PXDBDecimal(0)]
        [PXUIField(DisplayName = "Period 05")]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual decimal? Period05 { get; set; }
        public abstract class period05 : PX.Data.BQL.BqlDecimal.Field<period05> { }
        #endregion

        #region Period06
        [PXDBDecimal(0)]
        [PXUIField(DisplayName = "Period 06")]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual decimal? Period06 { get; set; }
        public abstract class period06 : PX.Data.BQL.BqlDecimal.Field<period06> { }
        #endregion

        #region Period07
        [PXDBDecimal(0)]
        [PXUIField(DisplayName = "Period 07")]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual decimal? Period07 { get; set; }
        public abstract class period07 : PX.Data.BQL.BqlDecimal.Field<period07> { }
        #endregion

        #region Period08
        [PXDBDecimal(0)]
        [PXUIField(DisplayName = "Period 08")]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual decimal? Period08 { get; set; }
        public abstract class period08 : PX.Data.BQL.BqlDecimal.Field<period08> { }
        #endregion

        #region Period09
        [PXDBDecimal(0)]
        [PXUIField(DisplayName = "Period 09")]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual decimal? Period09 { get; set; }
        public abstract class period09 : PX.Data.BQL.BqlDecimal.Field<period09> { }
        #endregion

        #region Period10
        [PXDBDecimal(0)]
        [PXUIField(DisplayName = "Period 10")]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual decimal? Period10 { get; set; }
        public abstract class period10 : PX.Data.BQL.BqlDecimal.Field<period10> { }
        #endregion

        #region Period11
        [PXDBDecimal(0)]
        [PXUIField(DisplayName = "Period 11")]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual decimal? Period11 { get; set; }
        public abstract class period11 : PX.Data.BQL.BqlDecimal.Field<period11> { }
        #endregion

        #region Period12
        [PXDBDecimal(0)]
        [PXUIField(DisplayName = "Period 12")]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual decimal? Period12 { get; set; }
        public abstract class period12 : PX.Data.BQL.BqlDecimal.Field<period12> { }
        #endregion

        #region TotalAmt
        [PXDecimal(0)]
        [PXUIField(DisplayName = "Total", Enabled = false)]
        [PXFormula(typeof(Add<period01, Add<period02, Add<period03, Add<period04, Add<period05, Add<period06, Add<period07, Add<period08, Add<period09, Add<period10, Add<period11, period12>>>>>>>>>>>))]
        public virtual decimal? TotalAmt { get; set; }
        public abstract class totalAmt : PX.Data.BQL.BqlDecimal.Field<totalAmt> { }
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

        #region CreatedDateTime
        [PXDBCreatedDateTime()]
        public virtual DateTime? CreatedDateTime { get; set; }
        public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
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

        #region LastModifiedDateTime
        [PXDBLastModifiedDateTime()]
        public virtual DateTime? LastModifiedDateTime { get; set; }
        public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
        #endregion

        #region NoteID
        [PXNote()]
        public virtual Guid? Noteid { get; set; }
        public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
        #endregion

        #region Tstamp
        [PXDBTimestamp()]
        public virtual byte[] Tstamp { get; set; }
        public abstract class tstamp : PX.Data.BQL.BqlByteArray.Field<tstamp> { }
        #endregion
    }
}