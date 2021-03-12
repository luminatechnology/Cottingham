using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Common;
using PX.Data;
using PX.Objects.GL;
using PX.Objects.GL.FinPeriods;
using PX.Objects.PM;

namespace CottinghamCustomization
{
    public class COProdContribInq : PXGraph<COProdContribInq>
    {
        #region Constant string Variables
        public const string ContribReportID = "GL651000";
        public const string AcctGrp_Sales   = "SALES";
        public const string AcctGrp_COGS    = "COGS";
        public const string AcctGrp_ATL     = "ATL";
        public const string AcctGrp_BTL     = "BTL";
        public const string AcctGrp_Prin    = "PRINCIPAL";
        public const string AcctTyp_Sales   = "Net Revenue sales";
        public const string AcctTyp_COGS    = AcctGrp_COGS;
        public const string AcctTyp_GrProf  = "Gross Profit";
        public const string AcctTyp_Matg    = "Marketing";
        public const string AcctTyp_ATL     = AcctGrp_ATL;
        public const string AcctTyp_BTL     = AcctGrp_BTL;
        public const string AcctTyp_ToMatg  = "Total Marketing Exp.";
        public const string AcctTyp_SupPrin = "Support from Principal";
        public const string AcctTyp_NetProf = "Net Profile";
        #endregion

        #region Select & Features
        public PXCancel<BudgetFilter> Cancel;
        public PXFilter<BudgetFilter> Filter;
        [PXFilterable]
        public PXSelect<ProductContributionData> Results;
        #endregion

        #region Delegate Data View
        protected virtual IEnumerable results()
        {
            return LoadAllData();
        }
        #endregion

        #region Ctor
        public COProdContribInq()
        {
            this.Results.Cache.AllowInsert = false;
            this.Results.Cache.AllowDelete = false;

            PXUIFieldAttribute.SetEnabled(Results.Cache, null, false);
        }
        #endregion

        #region Actions
        public PXAction<BudgetFilter> contribReport;
        [PXUIField(DisplayName = "Product Contribution", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton(CommitChanges = true)]
        protected virtual IEnumerable ContribReport(PXAdapter adapter)
        {
            PXLongOperation.StartOperation(this, () => { throw RunReport(); });

            return adapter.Get();
        }
        #endregion

        #region Event Handler
        protected virtual void _(Events.FieldVerifying<BudgetFilter.subIDFilter> e)
        {
            e.Cancel = true;
        }
        #endregion

        #region Methods
        public virtual List<ProductContributionData> LoadAllData()
        {
            var filter    = Filter.Current;
            var filterExt = filter.GetExtension<BudgetFilterExt>();
            var records   = new List<ProductContributionData>();

            PXSelectBase<GLHistory> cmdHistory = new PXSelectReadonly2<GLHistory, LeftJoin<Account, On<GLHistory.accountID, Equal<Account.accountID>>,
                                                                                  LeftJoin<Sub, On<Sub.subID, Equal<GLHistory.subID>>,
                                                                                  InnerJoin<MasterFinPeriod, On<True, Equal<True>,
                                                                                                                And<GLHistory.finPeriodID, Equal<MasterFinPeriod.finPeriodID>>>>>>,
                                                                                  Where<MasterFinPeriod.finPeriodID, Equal<Required<MasterFinPeriod.finPeriodID>>,
                                                                                        And<GLHistory.balanceType, Equal<LedgerBalanceType.actual>,
                                                                                            And<GLHistory.branchID, Equal<Required<GLHistory.branchID>>>>>,
                                                                                  OrderBy<Asc<GLHistory.accountID,
                                                                                              Asc<GLHistory.subID,
                                                                                                  Asc<MasterFinPeriod.periodNbr>>>>>(this);

            if (filter.SubIDFilter != null)
            {
                cmdHistory.WhereAnd<Where<Sub.subCD, Contains<Current<BudgetFilter.subCDWildcard>>>>();
            }

            foreach (PXResult<GLHistory, Account, Sub, MasterFinPeriod> aggregatingResult in cmdHistory.Select(filterExt.UsrFinPeriodID, filter.BranchID))
            {
                GLHistory histAggr = aggregatingResult;
                Account histAcct = aggregatingResult;
                Branch histBran = Branch.PK.Find(this, histAggr.BranchID);
                MasterFinPeriod histperd = aggregatingResult;
                PMAccountGroup acctGrop = PXSelectReadonly<PMAccountGroup, Where<PMAccountGroup.groupID, Equal<Required<Account.accountGroupID>>>>.Select(this, histAcct.AccountGroupID);

                if (acctGrop != null && acctGrop.GroupCD.Trim().IsIn(AcctGrp_Sales, AcctGrp_COGS, AcctGrp_ATL, AcctGrp_BTL, AcctGrp_Prin))
                {
                    ProductContributionData contribData = new ProductContributionData()
                    {
                        PeriodNbr = histperd.PeriodNbr,
                        BranchID = histAggr.BranchID,
                        AcctName = histBran.AcctName,
                        LogoNameRpt = histBran.BranchOrOrganizationLogoNameReport,
                        AccountID = histAggr.AccountID,
                        AccountCD = histAcct.AccountCD,
                        SubID = histAggr.SubID,
                        AcctGroup = acctGrop?.GroupCD,
                        BudgetAmt = 0m,
                        ActualAmt = histAggr.CuryFinPtdCredit + histAggr.CuryFinPtdDebit
                    };

                    records.Add(contribData);
                }
            }

            PXSelectBase<GLBudgetLine> cmdBudgetLine = new PXSelectReadonly2<GLBudgetLine, InnerJoin<MasterFinPeriod, On<True, Equal<True>>,
                                                                                           InnerJoin<Account, On<Account.accountID, Equal<GLBudgetLine.accountID>>,
                                                                                           LeftJoin<Sub, On<Sub.subID, Equal<GLBudgetLine.subID>>,
                                                                                           InnerJoin<GLBudgetLineDetail, On<GLBudgetLineDetail.groupID, Equal<GLBudgetLine.groupID>,
                                                                                                                            And<GLBudgetLineDetail.finPeriodID, Equal<MasterFinPeriod.finPeriodID>>>>>>>,
                                                                                           Where<GLBudgetLine.ledgerID, Equal<Required<BudgetFilter.ledgerID>>,
                                                                                                 And<GLBudgetLine.branchID, Equal<Required<BudgetFilter.branchID>>,
                                                                                                     And<MasterFinPeriod.finPeriodID, Equal<Required<MasterFinPeriod.finPeriodID>>,
                                                                                                         And<Match<Current<AccessInfo.userName>>>>>>>(this);
            if (filter.SubIDFilter != null)
            {
                cmdBudgetLine.WhereAnd<Where<Sub.subCD, Contains<Current<BudgetFilter.subCDWildcard>>>>();
            }

            foreach (PXResult<GLBudgetLine, MasterFinPeriod, Account, Sub, GLBudgetLineDetail> result in cmdBudgetLine.Select(filter.LedgerID, filter.BranchID, filterExt.UsrFinPeriodID))
            {
                Account            acct     = result;
                GLBudgetLine       budLine  = result;
                MasterFinPeriod    period   = result;
                GLBudgetLineDetail lineDtl  = result;
                Branch             branch   = Branch.PK.Find(this, budLine.BranchID);
                PMAccountGroup     acctGrop = PXSelectReadonly<PMAccountGroup, Where<PMAccountGroup.groupID, Equal<Required<Account.accountGroupID>>>>.Select(this, acct.AccountGroupID);

                if (acctGrop != null && acctGrop.GroupCD.Trim().IsIn(AcctGrp_Sales, AcctGrp_COGS, AcctGrp_ATL, AcctGrp_BTL, AcctGrp_Prin))
                {
                    ProductContributionData contribData = new ProductContributionData()
                    {
                        PeriodNbr = period.PeriodNbr,
                        BranchID = branch.BranchID,
                        AcctName = branch.AcctName,
                        LogoNameRpt = branch.BranchOrOrganizationLogoNameReport,
                        AccountID = acct.AccountID,
                        AccountCD = acct.AccountCD,
                        SubID = lineDtl.SubID,
                        AcctGroup = acctGrop?.GroupCD,
                        BudgetAmt = lineDtl.Amount,
                        ActualAmt = 0m
                    };

                    records.Add(contribData);
                }
            }

            var recordAggr = records.OrderBy(data => data.PeriodNbr).ThenBy(data => data.AccountCD)
                                    .GroupBy(data => new { data.BranchID, data.AcctName, data.LogoNameRpt, data.AcctGroup, data.PeriodNbr })
                                    .Select(x => new
                                    {
                                        PeriodNbr = x.Key.PeriodNbr,
                                        BranchID = x.Key.BranchID,
                                        Logo = x.Key.LogoNameRpt,
                                        AcctName = x.Key.AcctName,
                                        AcctGroup = x.Key.AcctGroup,
                                        BudgetAmt = x.Sum(y => y.BudgetAmt),
                                        ActualAmt = x.Sum(y => y.ActualAmt)
                                    }).ToList();

            // Reset list collection records.
            records.Clear();

            int recCount = 1;
            decimal? totalBudAmt = 0m;
            decimal? totalActAmt = 0m;
            decimal? totalNetBud = 0m;
            decimal? totalNetAct = 0m;

            // Insert "Fixed Number" record into temp table.
            do
            {
                ProductContributionData contribData = null;

                switch (recCount)
                {
                    case (int)GLAccountType.Sales:
                        var row = recordAggr.Find(f => f.AcctGroup.Trim() == AcctGrp_Sales);

                        //totalBudAmt = recordAggr.Find(f => f.AcctGroup.Trim() == AcctGrp_Sales && f.ActualAmt == 0m)?.BudgetAmt ?? 0m;

                        contribData = CreateDetailRecord(AcctTyp_Sales, row?.BranchID, row?.AcctName, row?.Logo, row?.PeriodNbr, row?.AcctGroup, row?.BudgetAmt, row?.ActualAmt);
                        
                        totalBudAmt = row?.BudgetAmt;
                        totalActAmt = row?.ActualAmt;
                        break;

                    case (int)GLAccountType.COGS:
                        row = recordAggr.Find(f => f.AcctGroup.Trim() == AcctGrp_COGS);

                        //totalBudAmt = recordAggr.Find(f => f.AcctGroup.Trim() == AcctGrp_COGS && f.ActualAmt == 0m)?.BudgetAmt ?? 0m;

                        contribData = CreateDetailRecord(AcctTyp_COGS, row?.BranchID, row?.AcctName, row?.Logo, row?.PeriodNbr, row?.AcctGroup, row?.BudgetAmt, row?.ActualAmt, true);
                        
                        totalBudAmt += row?.BudgetAmt;
                        totalActAmt += row?.ActualAmt;
                        break;

                    case (int)GLAccountType.GrossProfile:
                        contribData = CreateDetailRecord(AcctTyp_GrProf, null, null, null, null, null, totalBudAmt, totalActAmt);

                        totalNetBud = totalBudAmt;
                        totalNetAct = totalActAmt;
                        break;

                    case (int)GLAccountType.Martketing:
                        contribData = CreateDetailRecord(AcctTyp_Matg, null, null, null, null, null, null, null);
                        break;

                    case (int)GLAccountType.ATL:
                        row = recordAggr.Find(f => f.AcctGroup.Trim() == AcctGrp_ATL);

                        //totalBudAmt = recordAggr.Find(f => f.AcctGroup.Trim() == AcctGrp_ATL && f.ActualAmt == 0m)?.BudgetAmt ?? 0m;

                        contribData = CreateDetailRecord(AcctGrp_ATL, row?.BranchID, row?.AcctName, row?.Logo, row?.PeriodNbr, row?.AcctGroup, row?.BudgetAmt, row?.ActualAmt);
                        
                        totalBudAmt = row?.BudgetAmt;
                        totalActAmt = row?.ActualAmt;
                        break;

                    case (int)GLAccountType.BTL:
                        row = recordAggr.Find(f => f.AcctGroup.Trim() == AcctGrp_BTL);

                        //totalBudAmt = recordAggr.Find(f => f.AcctGroup.Trim() == AcctGrp_BTL && f.ActualAmt == 0m)?.BudgetAmt ?? 0m;

                        contribData = CreateDetailRecord(AcctGrp_BTL, row?.BranchID, row?.AcctName, row?.Logo, row?.PeriodNbr, row?.AcctGroup, row?.BudgetAmt, row?.ActualAmt, true);
                        
                        totalBudAmt += row?.BudgetAmt;
                        totalActAmt += row?.ActualAmt;
                        break;

                    case (int)GLAccountType.TotalMatg:
                        contribData = CreateDetailRecord(AcctTyp_ToMatg, null, null, null, null, null, totalBudAmt, totalActAmt);

                        totalNetBud += totalBudAmt;
                        totalNetAct += totalActAmt;
                        break;

                    case (int)GLAccountType.SupFmPrin:
                        row = recordAggr.Find(f => f.AcctGroup.Trim() == AcctGrp_Prin);

                        //totalBudAmt = recordAggr.Find(f => f.AcctGroup.Trim() == AcctGrp_Prin && f.ActualAmt == 0m)?.BudgetAmt ?? 0m;

                        contribData = CreateDetailRecord(AcctTyp_SupPrin, row?.BranchID, row?.AcctName, row?.Logo, row?.PeriodNbr, row?.AcctGroup, row?.BudgetAmt, row?.ActualAmt, true);

                        totalNetBud -= totalBudAmt;
                        totalNetAct -= totalActAmt;
                        break;

                    case (int)GLAccountType.NetProfile:
                        contribData = CreateDetailRecord(AcctTyp_NetProf, null, null, null, null, null, totalNetBud, totalNetAct, true);
                        break;
                }
                records.Add(contribData);

                recCount++;
            }
            while (recCount <= (int)GLAccountType.NetProfile);

            return records;
        }

        public virtual PXReportRequiredException RunReport()
        {
            return RunReport(PXBaseRedirectException.WindowMode.Same);
        }

        public virtual PXReportRequiredException RunReport(PXBaseRedirectException.WindowMode windowMode)
        {
            var reportData = new PXReportResultset(typeof(ProductContributionData));

            foreach (ProductContributionData row in Results.Select())
            {
                reportData.Add(row);
            }

            return new PXReportRequiredException(reportData, ContribReportID, windowMode);
        }
        #endregion

        #region Static Method
        /// <summary>
        /// Generate the report data according to parameters.
        /// </summary>
        /// <param name="acctType"></param>
        /// <param name="branchID"></param>
        /// <param name="acctName"></param>
        /// <param name="logo"></param>
        /// <param name="periodNbr"></param>
        /// <param name="acctGrp"></param>
        /// <param name="budgetAmt"></param>
        /// <param name="actualAmt"></param>
        /// <param name="bottomSold"></param>
        /// <returns></returns>
        protected static ProductContributionData CreateDetailRecord(string acctType, int? branchID, string acctName, string logo, string periodNbr, 
                                                                    string acctGrp, decimal? budgetAmt, decimal? actualAmt, bool? bottomSold = false)
        {
            decimal? comptRate = null;
            
            if (budgetAmt.HasValue)
            {
                comptRate = budgetAmt.Value <= 0m ? 0m : Math.Round(actualAmt.GetValueOrDefault() / budgetAmt.GetValueOrDefault(), 2);
            }

            ProductContributionData contribData = new ProductContributionData()
            {
                AcctType    = acctType,
                BranchID    = branchID,
                AcctName    = acctName,
                LogoNameRpt = logo,
                PeriodNbr   = periodNbr,
                AcctGroup   = acctGrp,
                BudgetAmt   = budgetAmt,
                ActualAmt   = actualAmt,
                CompltRate  = comptRate,
                BottomSold  = bottomSold
            };

            return contribData;
        }
        #endregion
    }

    #region Temp DAC
    /// <summary>
    /// The DAC is only for product contribution report.
    /// </summary>
    [Serializable]
    [PXCacheName("Temp Product Contribution")]
    public class ProductContributionData : PX.Data.IBqlTable
    {
        #region BranchID
        [Branch()]
        public virtual int? BranchID { get; set; }
        public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
        #endregion

        #region AcctName
        [PXString(60, IsUnicode = true)]
        [PXUIField(DisplayName = "Branch Name", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual String AcctName { get; set; }
        public abstract class acctName : PX.Data.BQL.BqlString.Field<acctName> { }
        #endregion

        #region LogoNameRpt
        [PXString(IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Branch or Organization Report Logo File")]
        public string LogoNameRpt { get; set; }
        public abstract class logoNameRpt : PX.Data.BQL.BqlString.Field<logoNameRpt> { }
        #endregion

        #region AccountID
        [Account()]
        public virtual int? AccountID { get; set; }
        public abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }
        #endregion

        #region AccountCD
        [PXString()]
        public virtual String AccountCD { get; set; }
        public abstract class accountCD : PX.Data.BQL.BqlString.Field<accountCD> { }
        #endregion

        #region SubID
        [SubAccount()]
        public virtual int? SubID { get; set; }
        public abstract class subID : PX.Data.BQL.BqlInt.Field<subID> { }
        #endregion

        #region AcctGroup
        [PXDimensionSelector(AccountGroupAttribute.DimensionName, typeof(Search<PMAccountGroup.groupCD>),
                             typeof(PMAccountGroup.groupCD),
                             typeof(PMAccountGroup.groupCD),
                             typeof(PMAccountGroup.description),
                             typeof(PMAccountGroup.type),
                             typeof(PMAccountGroup.isActive),
                             DescriptionField = typeof(PMTask.description))]
        [PXString(IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Account Group ID", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual string AcctGroup { get; set; }
        public abstract class acctGroup : PX.Data.BQL.BqlString.Field<acctGroup> { }
        #endregion

        #region AcctType
        [PXString(IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Account Type")]
        public virtual string AcctType { get; set; }
        public abstract class acctType : PX.Data.BQL.BqlString.Field<acctType> { }
        #endregion

        #region BudgetAmt
        [PXDecimal()]
        public virtual decimal? BudgetAmt { get; set; }
        public abstract class budgetAmt : PX.Data.BQL.BqlDecimal.Field<budgetAmt> { }
        #endregion

        #region ActualAmt
        [PXDecimal()]
        public virtual decimal? ActualAmt { get; set; }
        public abstract class actualAmt : PX.Data.BQL.BqlDecimal.Field<actualAmt> { }
        #endregion

        #region CompltRate
        [PXDecimal()]
        public virtual decimal? CompltRate { get; set; }
        public abstract class compltRate : PX.Data.BQL.BqlDecimal.Field<compltRate> { }
        #endregion

        #region BottomSold
        [PXBool]
        public virtual bool? BottomSold { get; set; }
        public abstract class bottomSold : PX.Data.BQL.BqlBool.Field<bottomSold> { }
        #endregion

        #region PeriodNbr
        [PXString(2, IsFixed = true)]
        public virtual String PeriodNbr { get; set; }
        public abstract class periodNbr : PX.Data.BQL.BqlString.Field<periodNbr> { }
        #endregion
    }
    #endregion

    #region enum
    public enum GLAccountType
    {
        Sales = 1,
        COGS = 2,
        GrossProfile = 3,
        Martketing = 4,
        ATL = 5,
        BTL = 6,
        TotalMatg = 7,
        SupFmPrin = 8,
        NetProfile = 9
    }
    #endregion
}