using System;
using PX.Data;
using PX.Objects.CM;
using PX.Objects.GL;

namespace CottinghamCustomization
{
    [Serializable]
    [PXCacheName("GL Compare IN_SQLView")]
    public class V_GLCompareIN : IBqlTable
    {
        #region BranchID
        [Branch(typeof(Batch.branchID))]
        public virtual Int32? BranchID { get; set; }
        public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
        #endregion

        #region Module
        [PXDBString(2, IsKey = true, IsFixed = true)]
        [PXUIField(DisplayName = "Module")]
        [BatchModule.List()]
        public virtual string Module { get; set; }
        public abstract class module : PX.Data.BQL.BqlString.Field<module> { }
        #endregion

        #region BatchNbr
        [PXDBString(15, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Batch Nbr")]
        public virtual string BatchNbr { get; set; }
        public abstract class batchNbr : PX.Data.BQL.BqlString.Field<batchNbr> { }
        #endregion

        #region LedgerID
        [PXDBInt()]
        [PXUIField(DisplayName = "Ledger ID")]
        [PXSelector(typeof(Ledger.ledgerID), SubstituteKey = typeof(Ledger.ledgerCD))]
        public virtual int? LedgerID { get; set; }
        public abstract class ledgerID : PX.Data.BQL.BqlInt.Field<ledgerID> { }
        #endregion

        #region AccountID
        [PXUIField(DisplayName = "Account")]
        [Account(typeof(branchID), LedgerID = typeof(ledgerID), DescriptionField = typeof(Account.description))]
        public virtual int? AccountID { get; set; }
        public abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }
        #endregion

        #region SubID
        [PXUIField(DisplayName = "Sub")]
        [SubAccount(typeof(accountID), typeof(branchID))]
        public virtual int? SubID { get; set; }
        public abstract class subID : PX.Data.BQL.BqlInt.Field<subID> { }
        #endregion

        #region FinPeriodID
        [PXDBString(6, IsFixed = true, InputMask = "")]
        [PXUIField(DisplayName = "Fin Period")]
        [FinPeriodIDFormatting]
        public virtual string FinPeriodID { get; set; }
        public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
        #endregion

        //#region CuryInfoID
        //[PXDBLong()]
        //[CurrencyInfo(typeof(CurrencyInfo.curyInfoID))]
        //public virtual long? CuryInfoID { get; set; }
        //public abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }
        //#endregion

        #region CuryCreditAmt
        [PXDBDecimal()]
        [PXUIField(DisplayName = "Credit Amount")]
        public virtual Decimal? CuryCreditAmt { get; set; }
        public abstract class curyCreditAmt : PX.Data.BQL.BqlDecimal.Field<curyCreditAmt> { }
        #endregion

        #region CuryDebitAmt
        [PXDBDecimal()]
        [PXUIField(DisplayName = "Debit Amountt")]
        public virtual Decimal? CuryDebitAmt { get; set; }
        public abstract class curyDebitAmt : PX.Data.BQL.BqlDecimal.Field<curyDebitAmt> { }
        #endregion
    }
}