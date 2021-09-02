using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Common;
using PX.Data;
using PX.Objects.CS;
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
        public const string AcctTyp_NetProf = "Net Profit";
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

            string yearStart = filterExt.UsrFinPeriodID.Substring(0, 4) + "01";

            /// <summary> Generate MTD GLHistrory records. </summary>
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
                GLHistory       histAggr = aggregatingResult;
                Account         histAcct = aggregatingResult;
                Branch          histBran = Branch.PK.Find(this, histAggr.BranchID);
                MasterFinPeriod histperd = aggregatingResult;
                PMAccountGroup  acctGrop = PXSelectReadonly<PMAccountGroup, Where<PMAccountGroup.groupID, Equal<Required<Account.accountGroupID>>>>.Select(this, histAcct.AccountGroupID);

                if (acctGrop != null && acctGrop.GroupCD.Trim().IsIn(AcctGrp_Sales, AcctGrp_COGS, AcctGrp_ATL, AcctGrp_BTL, AcctGrp_Prin))
                {
                    ProductContributionData contribData = new ProductContributionData()
                    {
                        PeriodNbr = histperd.PeriodNbr,
                        FinYear = histperd.FinYear,
                        SubCDWildcard = GetSubCDSegmentDescr(this, filter.SubIDFilter),
                        BranchID = histAggr.BranchID,
                        AcctName = histBran.AcctName,
                        LogoNameRpt = histBran.BranchOrOrganizationLogoNameReport,
                        AccountID = histAggr.AccountID,
                        AccountCD = histAcct.AccountCD,
                        SubID = histAggr.SubID,
                        AcctGroup = acctGrop?.GroupCD,
                        BudgetAmt = 0m,
                        ActualPtdAmt = Math.Abs(histAggr.CuryFinPtdCredit.Value - histAggr.CuryFinPtdDebit.Value),
                        ActualYtdAmt = 0m
                    };

                    records.Add(contribData);
                }
            }

            /// <summary> Generate YTD GLHistrory records. </summary>
            PXSelectBase<GLHistory> cmdHistory2 = new PXSelectReadonly2<GLHistory, LeftJoin<Account, On<GLHistory.accountID, Equal<Account.accountID>>,
                                                                                   LeftJoin<Sub, On<Sub.subID, Equal<GLHistory.subID>>,
                                                                                   InnerJoin<MasterFinPeriod, On<True, Equal<True>,
                                                                                                                 And<GLHistory.finPeriodID, Equal<MasterFinPeriod.finPeriodID>>>>>>,
                                                                                   Where<MasterFinPeriod.finPeriodID, GreaterEqual<Required<MasterFinPeriod.finPeriodID>>,
                                                                                         And<MasterFinPeriod.finPeriodID, LessEqual<Required<MasterFinPeriod.finPeriodID>>,
                                                                                             And<GLHistory.balanceType, Equal<LedgerBalanceType.actual>,
                                                                                                 And<GLHistory.branchID, Equal<Required<GLHistory.branchID>>>>>>,
                                                                                   OrderBy<Asc<GLHistory.accountID,
                                                                                               Asc<GLHistory.subID,
                                                                                                   Asc<MasterFinPeriod.periodNbr>>>>>(this);

            if (filter.SubIDFilter != null)
            {
                cmdHistory2.WhereAnd<Where<Sub.subCD, Contains<Current<BudgetFilter.subCDWildcard>>>>();
            }

            foreach (PXResult<GLHistory, Account, Sub, MasterFinPeriod> aggregatingResult in cmdHistory2.Select(yearStart, filterExt.UsrFinPeriodID, filter.BranchID))
            {
                GLHistory       histAggr = aggregatingResult;
                Account         histAcct = aggregatingResult;
                Branch          histBran = Branch.PK.Find(this, histAggr.BranchID);
                MasterFinPeriod histperd = aggregatingResult;
                PMAccountGroup  acctGrop = PXSelectReadonly<PMAccountGroup, Where<PMAccountGroup.groupID, Equal<Required<Account.accountGroupID>>>>.Select(this, histAcct.AccountGroupID);

                if (acctGrop != null && acctGrop.GroupCD.Trim().IsIn(AcctGrp_Sales, AcctGrp_COGS, AcctGrp_ATL, AcctGrp_BTL, AcctGrp_Prin))
                {
                    ProductContributionData contribData = new ProductContributionData()
                    {
                        PeriodNbr = filterExt.UsrFinPeriodID.Substring(4, 2),
                        FinYear = filterExt.UsrFinPeriodID.Substring(0, 4),
                        SubCDWildcard = GetSubCDSegmentDescr(this, filter.SubIDFilter),
                        BranchID = histAggr.BranchID,
                        AcctName = histBran.AcctName,
                        LogoNameRpt = histBran.BranchOrOrganizationLogoNameReport,
                        AccountID = histAggr.AccountID,
                        AccountCD = histAcct.AccountCD,
                        SubID = histAggr.SubID,
                        AcctGroup = acctGrop?.GroupCD,
                        BudgetAmt = 0m,
                        ActualPtdAmt = 0m,
                        ActualYtdAmt = Math.Abs(histAggr.CuryFinPtdCredit.Value - histAggr.CuryFinPtdDebit.Value)
                    };

                    records.Add(contribData);
                }
            }

            /// <summary> Generate budget line detail records. </summary>
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
                        FinYear = period.FinYear,
                        SubCDWildcard = GetSubCDSegmentDescr(this, filter.SubIDFilter),
                        BranchID = branch.BranchID,
                        AcctName = branch.AcctName,
                        LogoNameRpt = branch.BranchOrOrganizationLogoNameReport,
                        AccountID = acct.AccountID,
                        AccountCD = acct.AccountCD,
                        SubID = lineDtl.SubID,
                        AcctGroup = acctGrop?.GroupCD,
                        BudgetAmt = lineDtl.Amount,
                        ActualPtdAmt = 0m,
                        ActualYtdAmt = 0m
                    };

                    records.Add(contribData);
                }
            }

            var recordAggr = records.OrderBy(data => data.PeriodNbr).ThenBy(data => data.AccountCD)
                                    .GroupBy(data => new { data.BranchID, data.AcctName, data.LogoNameRpt, data.AcctGroup, data.PeriodNbr, data.FinYear, data.SubCDWildcard })
                                    .Select(x => new
                                    {
                                        PeriodNbr = x.Key.PeriodNbr,
                                        FinYear = x.Key.FinYear,
                                        SubCD = x.Key.SubCDWildcard,
                                        BranchID = x.Key.BranchID,
                                        Logo = x.Key.LogoNameRpt,
                                        AcctName = x.Key.AcctName,
                                        AcctGroup = x.Key.AcctGroup,
                                        BudgetAmt = x.Sum(y => y.BudgetAmt),
                                        ActualPtdAmt = x.Sum(y => y.ActualPtdAmt),
                                        ActualYtdAmt = x.Sum(y => y.ActualYtdAmt)
                                    }).ToList();

            // Reset list collection records.
            records.Clear();

            int recCount = 1;
            decimal? totalBudAmt  = 0m;
            decimal? totActPtdAmt = 0m;
            decimal? totActYtdAmt = 0m;
            decimal? totalNetBud  = 0m;
            decimal? totNetActPtd = 0m;
            decimal? totNetActYtd = 0m;

            // Insert "Fixed Number" record into temp table.
            do
            {
                ProductContributionData contribData = null;

                switch (recCount)
                {
                    case (int)GLAccountType.Sales:
                        var row = recordAggr.Find(f => f.AcctGroup.Trim() == AcctGrp_Sales);

                        contribData = CreateDetailRecord(AcctTyp_Sales, row?.BranchID, row?.AcctName, row?.Logo, row?.PeriodNbr, row?.FinYear, row?.SubCD, row?.AcctGroup, row?.BudgetAmt, row?.ActualPtdAmt, row?.ActualYtdAmt);
                        
                        totalBudAmt  = row?.BudgetAmt;
                        totActPtdAmt = row?.ActualPtdAmt;
                        totActYtdAmt = row?.ActualYtdAmt;
                        break;

                    case (int)GLAccountType.COGS:
                        row = recordAggr.Find(f => f.AcctGroup.Trim() == AcctGrp_COGS);

                        contribData = CreateDetailRecord(AcctTyp_COGS, row?.BranchID, row?.AcctName, row?.Logo, row?.PeriodNbr, row?.FinYear, row?.SubCD, row?.AcctGroup, row?.BudgetAmt, row?.ActualPtdAmt, row?.ActualYtdAmt, true);
                        
                        totalBudAmt  -= row?.BudgetAmt;
                        totActPtdAmt -= row?.ActualPtdAmt;
                        totActYtdAmt -= row?.ActualYtdAmt;
                        break;

                    case (int)GLAccountType.GrossProfile:
                        contribData = CreateDetailRecord(AcctTyp_GrProf, null, null, null, null, null, null, null, totalBudAmt, totActPtdAmt, totActYtdAmt);

                        totalNetBud  = totalBudAmt ?? 0m;
                        totNetActPtd = totActPtdAmt ?? 0m;
                        totNetActYtd = totActYtdAmt ?? 0m;
                        break;

                    case (int)GLAccountType.Martketing:
                        contribData = CreateDetailRecord(AcctTyp_Matg, null, null, null, null, null, null, null, null, null, null);
                        break;

                    case (int)GLAccountType.ATL:
                        row = recordAggr.Find(f => f.AcctGroup.Trim() == AcctGrp_ATL);

                        contribData = CreateDetailRecord(AcctGrp_ATL, row?.BranchID, row?.AcctName, row?.Logo, row?.PeriodNbr, row?.FinYear, row?.SubCD, row?.AcctGroup, row?.BudgetAmt, row?.ActualPtdAmt, row?.ActualYtdAmt);
                        
                        totalBudAmt  = row?.BudgetAmt;
                        totActPtdAmt = row?.ActualPtdAmt;
                        totActYtdAmt = row?.ActualYtdAmt;
                        break;

                    case (int)GLAccountType.BTL:
                        row = recordAggr.Find(f => f.AcctGroup.Trim() == AcctGrp_BTL);

                        contribData = CreateDetailRecord(AcctGrp_BTL, row?.BranchID, row?.AcctName, row?.Logo, row?.PeriodNbr, row?.FinYear, row?.SubCD, row?.AcctGroup, row?.BudgetAmt, row?.ActualPtdAmt, row?.ActualYtdAmt, true);
                        
                        totalBudAmt  += row?.BudgetAmt;
                        totActPtdAmt += row?.ActualPtdAmt;
                        totActYtdAmt += row?.ActualYtdAmt;
                        break;

                    case (int)GLAccountType.TotalMatg:
                        contribData = CreateDetailRecord(AcctTyp_ToMatg, null, null, null, null, null, null, null, totalBudAmt, totActPtdAmt, totActYtdAmt);

                        totalNetBud  -= totalBudAmt ?? 0m;
                        totNetActPtd -= totActPtdAmt ?? 0m;
                        totNetActYtd -= totActYtdAmt ?? 0m;
                        break;

                    case (int)GLAccountType.SupFmPrin:
                        row = recordAggr.Find(f => f.AcctGroup.Trim() == AcctGrp_Prin);

                        var rowATL = recordAggr.Find(f => f.AcctGroup.Trim() == AcctGrp_ATL);
                        var rowBTL = recordAggr.Find(f => f.AcctGroup.Trim() == AcctGrp_BTL);

                        decimal? actualPtdAmt = row == null ? 0 : (row.ActualYtdAmt - (rowATL.ActualYtdAmt + rowBTL.ActualYtdAmt) + (rowATL.ActualPtdAmt + rowBTL.ActualPtdAmt) ) / 12;

                        contribData = CreateDetailRecord(AcctTyp_SupPrin, row?.BranchID, row?.AcctName, row?.Logo, row?.PeriodNbr, row?.FinYear, row?.SubCD, row?.AcctGroup, row?.BudgetAmt, actualPtdAmt, row?.ActualYtdAmt, true, true);

                        totalBudAmt  = row?.BudgetAmt;
                        totActPtdAmt = actualPtdAmt;
                        totActYtdAmt = row?.ActualYtdAmt;

                        totalNetBud  += totalBudAmt ?? 0m;
                        totNetActPtd += totActPtdAmt ?? 0m;
                        totNetActYtd += totActYtdAmt ?? 0m;
                        break;

                    case (int)GLAccountType.NetProfile:
                        contribData = CreateDetailRecord(AcctTyp_NetProf, null, null, null, null, null, null, null, totalNetBud, totNetActPtd, totNetActYtd, true, true);
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
        protected static ProductContributionData CreateDetailRecord(string acctType, int? branchID, string acctName, string logo, string periodNbr, string finYear,
                                                                    string subCD, string acctGrp, decimal? budgetAmt, decimal? actualPtdAmt, decimal? actualYtdAmt, 
                                                                    bool? bottomSold = false, bool? hasEST = false)
        {
            decimal? comptRate = null;
            
            if (budgetAmt.HasValue)
            {
                comptRate = budgetAmt.Value <= 0m ? 0m : Math.Abs(Math.Round(actualPtdAmt.GetValueOrDefault() / budgetAmt.GetValueOrDefault() * 100, 2) );
            }

            ProductContributionData contribData = new ProductContributionData()
            {
                AcctType      = acctType,
                BranchID      = branchID,
                AcctName      = acctName,
                LogoNameRpt   = logo,
                PeriodNbr     = periodNbr,
                FinYear       = finYear,
                SubCDWildcard = subCD,
                AcctGroup     = acctGrp,
                BudgetAmt     = budgetAmt,
                ActualPtdAmt  = actualPtdAmt,
                ActualYtdAmt  = actualYtdAmt,
                CompltRate    = comptRate,
                BottomSold    = bottomSold,
                Estimated     = hasEST
            };

            return contribData;
        }

        /// <summary>
        /// Get sub account segment value description according to parameter.
        /// </summary>
        protected static string GetSubCDSegmentDescr(PXGraph graph, string subCD)
        {
            string segSubStrValue = string.Empty;

            if (!string.IsNullOrEmpty(subCD) )
            {
                short segStartIndex = (short)subCD.Length;

                foreach (Segment segment in PXSelect<Segment, Where<Segment.dimensionID, Equal<Required<Segment.dimensionID>>>,
                                                              OrderBy<Desc<Segment.segmentID>>>.Select(graph, PX.Objects.GL.SubAccountAttribute.DimensionName))
                {
                    if (segment.SegmentID.HasValue && segment.Length.HasValue)
                    {
                        segStartIndex -= segment.Length.Value;

                        segSubStrValue = subCD.Substring(segStartIndex, (int)segment.Length);
                    }

                    if (segSubStrValue.Trim() != string.Empty)
                    {
                        segSubStrValue = PXSelect<SegmentValue, Where<SegmentValue.dimensionID, Equal<Required<Segment.dimensionID>>,
                                                                      And<SegmentValue.segmentID, Equal<Required<Segment.segmentID>>,
                                                                          And<SegmentValue.value, Equal<Required<SegmentValue.value>>>>>>
                                                                .SelectSingleBound(graph, null, segment.DimensionID, segment.SegmentID, segSubStrValue).TopFirst?.Descr;

                        break;
                    }
                }
            }

            return segSubStrValue;
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

        #region SubCDWildcard
        [PXString(IsUnicode = true)]
        public virtual string SubCDWildcard { get; set; }
        public abstract class subCDWildcard : PX.Data.BQL.BqlString.Field<subCDWildcard> { };
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

        #region ActualPtdAmt
        [PXDecimal()]
        public virtual decimal? ActualPtdAmt { get; set; }
        public abstract class actualPtdAmt : PX.Data.BQL.BqlDecimal.Field<actualPtdAmt> { }
        #endregion

        #region ActualYtdAmt
        [PXDecimal()]
        public virtual decimal? ActualYtdAmt { get; set; }
        public abstract class actualYtdAmt : PX.Data.BQL.BqlDecimal.Field<actualYtdAmt> { }
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

        #region FinYear
        [PXString(4, IsFixed = true)]
        public virtual string FinYear { get; set; }
        public abstract class finYear : PX.Data.BQL.BqlString.Field<finYear> { }
        #endregion

        #region Estimated
        [PXBool]
        public virtual bool? Estimated { get; set; }
        public abstract class estimated : PX.Data.BQL.BqlBool.Field<estimated> { }
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