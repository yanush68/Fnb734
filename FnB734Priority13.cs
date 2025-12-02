// Program: FN_B734_PRIORITY_1_3, ID: 945117537, model: 746.
// Short name: SWE03080
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_B734_PRIORITY_1_3.
/// </para>
/// <para>
/// Priority 1-3: Percent of Cases Paying on Arrears
/// </para>
/// </summary>
[Serializable]
[Program("SWE03080")]
public partial class FnB734Priority13: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B734_PRIORITY_1_3 program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB734Priority13(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB734Priority13.
  /// </summary>
  public FnB734Priority13(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ---------------------------------------------------------------------------------------------------
    //                                     
    // C H A N G E    L O G
    // ---------------------------------------------------------------------------------------------------
    // Date      Developer     Request #	Description
    // --------  ----------    ----------	
    // -----------------------------------------------------------
    // 02/20/13  GVandy	CQ36547		Initial Development.  Priority 1-1, 1-3, and 1-
    // 4.
    // 			Segment A
    // 02/04/20  GVandy	CQ66220		Correlate with OCSE157 changes beginning in FY 
    // 2022.
    // 					These changes include only amounts in OCSE157
    // 					Lines 25, 27, and 29 that are both distributed
    // 					and disbursed.  Export a cutoff FY which defaults to
    // 					2022 but can be overridden with a code table value for testing.
    // ---------------------------------------------------------------------------------------------------
    // -------------------------------------------------------------------------------------
    // Priority 1-3: Percent of Cases Paying on Arrears
    // -------------------------------------------------------------------------------------
    // Number of Cases paying towards arrears divided by number of Cases with 
    // arrears owed (OCSE157 Line29/ OCSE157 Line28)
    // Report Level: State, Judicial District
    // Report Period: Month (Fiscal year-to-date calculation)
    // Numerator-
    // The Number of Cases Paying Toward Arrears during the Report Period.  This
    // is a subset of the denominator.  Only include cases that are also counted
    // in the denominator.  Of cases included in the denominator, count those
    // where:
    // 	1) Money applied to arrears
    // 	2) Bypass FcrtRec and FDIR (REIP) cash receipt types.  Bypass fees,
    // 	   recoveries, bypass 718B and bypass MJ (AF, AFI, FC, FCI).
    // 	3) Cannot count cases where money was due to the family and none of the
    // 	   collections received were paid to the family.
    // 	4) Do not count cases with arrears collections where money was due to 
    // the
    // 	   family (NA or NAI, and money due to the state (AF or any other 
    // program),
    // 	   but no money applied to NA.
    // 	   Hierarchy-
    // 		a) Collections NA or NAI- they are auto counted.
    // 		b) Any other collections, see if arrears are due to the family as of
    // 		   collection date, it cannot be counted.-
    // 	5) If the only activity on the case is an adjustment in the reporting 
    // period
    // 	   on a collection created in a previous reporting period, exclude the 
    // case.
    // 	6) The case is determined based on AP/Supported Person on the debt.
    // 	   AP/Supported Person roles overlap on case at any time.
    // Denominator-
    // Cases open at any point During the Report Period with Arrears Due.
    // 	1) Count cases open during the report period. (If the case is currently 
    // open,
    // 	   it goes to the current office, if it has been closed, it goes to the
    // 	   office assigned at time of closure.)
    // 	2) Cases- no arrears on last day of report period, but had arrears due
    // 	   sometime during the report period.
    // 	3) No arrears last day of report period, no collection towards arrears 
    // during
    // 	   report period.  Look for negative adjustment that occurred during 
    // report
    // 	   period and determine if the adjustment was towards arrears.
    // 	4) In joint/several situations, both cases will be counted in this line.
    // 	5) Include CSENet collection
    // 	6) If the only activity on the case is an adjustment in the current 
    // report
    // 	   period on a collection created in a prior report period, exclude the 
    // case.
    // 	7) Any obligation with a balance due on the first day of any month in 
    // the
    // 	   FFY, on an obligation that accrued in a prior month, is considered 
    // arrears
    // 	   for accruing debts.
    // 	8) Non-accruing debts bypass voluntaries, 718B, fee, recoveries and MJ (
    // AF,
    // 	   AFI, FC, FCI).
    // 	9) Count each case only once.
    // 	10) The case is determined based on AP/Supported Person on the debt.
    // 	    AP/Supported Person roles overlap on case at any time.
    // -------------------------------------------------------------------------------------
    MoveDashboardAuditData2(import.DashboardAuditData, local.Initialized);
    MoveProgramCheckpointRestart(import.ProgramCheckpointRestart,
      local.ProgramCheckpointRestart);

    // -- Determine the federal fiscal year.
    local.FiscalYear.Year = Year(AddMonths(import.ReportStartDate.Date, 3));

    // -- Determine Previous Year report month.
    local.PreviousYear.ReportMonth = import.DashboardAuditData.ReportMonth - 100
      ;

    // --  Initialize Statewide view
    local.Statewide.AsOfDate = import.ProgramProcessingInfo.ProcessDate;
    local.Statewide.ReportLevel = "ST";
    local.Statewide.ReportLevelId = "KS";
    local.Statewide.ReportMonth = import.DashboardAuditData.ReportMonth;
    local.Contractor.Index = -1;
    local.Contractor.Count = 0;

    // --  Initialize Judicial District group view
    foreach(var _ in ReadCseOrganization())
    {
      if (Verify(entities.CseOrganization.Code, "0123456789") != 0)
      {
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "Judical District code not numeric.  JD Code = " + String
          (entities.CseOrganization.Code, CseOrganization.Code_MaxLength);
        UseCabErrorReport();
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      local.Local1.Index =
        (int)StringToNumber(entities.CseOrganization.Code) - 1;
      local.Local1.CheckSize();

      local.Local1.Update.G.AsOfDate = import.ProgramProcessingInfo.ProcessDate;
      local.Local1.Update.G.ReportLevel = "JD";
      local.Local1.Update.G.ReportLevelId = entities.CseOrganization.Code;
      local.Local1.Update.G.ReportMonth = import.DashboardAuditData.ReportMonth;

      // -- Determine contractor from the judicial district to which they are 
      // assigned on the report period end date.
      local.DashboardAuditData.JudicialDistrict = entities.CseOrganization.Code;
      UseFnB734DeterContractorFromJd();
      local.Local1.Update.G.ContractorNumber = local.Contractor1.Code;

      if (local.Contractor.Count < 1)
      {
        ++local.Contractor.Index;
        local.Contractor.CheckSize();

        local.Contractor.Update.Gcontractor.ContractorNumber =
          local.Contractor1.Code;
        local.Contractor.Update.Gcontractor.ReportLevel = "XJ";
        local.Contractor.Update.Gcontractor.ReportMonth =
          import.DashboardAuditData.ReportMonth;
        local.Contractor.Update.Gcontractor.ReportLevelId =
          local.Local1.Item.G.ContractorNumber;
        local.Contractor.Update.Gcontractor.AsOfDate =
          import.ProgramProcessingInfo.ProcessDate;
      }
      else
      {
        for(local.Contractor.Index = 0; local.Contractor.Index < local
          .Contractor.Count; ++local.Contractor.Index)
        {
          if (!local.Contractor.CheckSize())
          {
            break;
          }

          if (Equal(local.Contractor1.Code,
            local.Contractor.Item.Gcontractor.ContractorNumber))
          {
            goto ReadEach1;
          }
        }

        local.Contractor.CheckIndex();

        local.Contractor.Index = local.Contractor.Count;
        local.Contractor.CheckSize();

        local.Contractor.Update.Gcontractor.ContractorNumber =
          local.Contractor1.Code;
        local.Contractor.Update.Gcontractor.ReportLevel = "XJ";
        local.Contractor.Update.Gcontractor.ReportLevelId =
          local.Local1.Item.G.ContractorNumber;
        local.Contractor.Update.Gcontractor.ReportMonth =
          import.DashboardAuditData.ReportMonth;
        local.Contractor.Update.Gcontractor.AsOfDate =
          import.ProgramProcessingInfo.ProcessDate;
      }

ReadEach1:
      ;
    }

    // @@@ Restart logic for CQ66220
    // ------------------------------------------------------------------------------
    // -- Determine if we're restarting and set appropriate restart information.
    // ------------------------------------------------------------------------------
    if (AsChar(import.ProgramCheckpointRestart.RestartInd) == 'Y' && Equal
      (import.ProgramCheckpointRestart.RestartInfo, 81, 8, "1-03    "))
    {
      // -- Checkpoint Info
      // Positions   Value
      // ---------   
      // ------------------------------------
      //  001-080    General Checkpoint Info for PRAD
      //  081-088    Dashboard Priority
      //  089-089    Blank
      //  090-099    CSE Case Number
      local.Restart.Number =
        Substring(import.ProgramCheckpointRestart.RestartInfo, 90, 10);

      if (!IsEmpty(local.Restart.Number))
      {
        // -- Load statewide counts.
        foreach(var _ in ReadDashboardStagingPriority1())
        {
          local.Statewide.Assign(entities.DashboardStagingPriority12);
        }

        // -- Load Judicial District counts.
        foreach(var _ in ReadDashboardStagingPriority2())
        {
          local.Local1.Index =
            (int)StringToNumber(entities.DashboardStagingPriority12.
              ReportLevelId) - 1;
          local.Local1.CheckSize();

          local.Local1.Update.G.Assign(entities.DashboardStagingPriority12);
        }
      }
      else
      {
        // -- Load statewide counts.
        foreach(var _ in ReadDashboardStagingPriority3())
        {
          local.Statewide.Assign(entities.DashboardStagingPriority12);
          local.Statewide.CasesPayingArrearsDenominator = 0;
          local.Statewide.CasesPayingArrearsNumerator = 0;
          local.Statewide.CasesPayingArrearsPercent = 0;
          local.Statewide.CasesPayingArrearsRank = 0;
        }

        // -- Load Judicial District counts.
        foreach(var _ in ReadDashboardStagingPriority4())
        {
          local.Local1.Index =
            (int)StringToNumber(entities.DashboardStagingPriority12.
              ReportLevelId) - 1;
          local.Local1.CheckSize();

          local.Local1.Update.G.Assign(entities.DashboardStagingPriority12);
          local.Local1.Update.G.CasesPayingArrearsDenominator = 0;
          local.Local1.Update.G.CasesPayingArrearsNumerator = 0;
          local.Local1.Update.G.CasesPayingArrearsPercent = 0;
          local.Local1.Update.G.CasesPayingArrearsRank = 0;
        }

        foreach(var _ in ReadDashboardStagingPriority5())
        {
          try
          {
            UpdateDashboardStagingPriority1();
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "DASHBOARD_STAGING_PRI_3_5_NU";

                break;
              case ErrorCode.PermittedValueViolation:
                ExitState = "DASHBOARD_STAGING_PRI_3_5_PV";

                break;
              default:
                throw;
            }
          }
        }
      }
    }
    else
    {
      local.Restart.Number = "";

      foreach(var _ in ReadDashboardStagingPriority5())
      {
        try
        {
          UpdateDashboardStagingPriority1();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "DASHBOARD_STAGING_PRI_3_5_NU";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "DASHBOARD_STAGING_PRI_3_5_PV";

              break;
            default:
              throw;
          }
        }
      }
    }

    if (local.FiscalYear.Year < (import.Cq66220EffectiveFy.FiscalYear ?? 0))
    {
      // ----------------------------------------------------------------------
      // Read each case open at some point during the FY.
      // -----------------------------------------------------------------------
      foreach(var _ in ReadCaseCaseAssignment())
      {
        if (Equal(entities.Case1.Number, local.Prev.Number))
        {
          continue;
        }
        else
        {
          // ------------------------------------------------------------------------------
          // -- Checkpoint saving all the info needed for restarting.
          // ------------------------------------------------------------------------------
          if (local.RecordsReadSinceCommit.Count >= (
            import.ProgramCheckpointRestart.ReadFrequencyCount ?? 0))
          {
            // -- Save the Statewide counts.
            if (ReadDashboardStagingPriority6())
            {
              try
              {
                UpdateDashboardStagingPriority2();
              }
              catch(Exception e)
              {
                switch(GetErrorCode(e))
                {
                  case ErrorCode.AlreadyExists:
                    ExitState = "DASHBOARD_STAGING_PRI_1_2_NU";

                    break;
                  case ErrorCode.PermittedValueViolation:
                    ExitState = "DASHBOARD_STAGING_PRI_1_2_PV";

                    break;
                  default:
                    throw;
                }
              }
            }
            else
            {
              try
              {
                CreateDashboardStagingPriority1();
              }
              catch(Exception e)
              {
                switch(GetErrorCode(e))
                {
                  case ErrorCode.AlreadyExists:
                    ExitState = "DASHBOARD_STAGING_PRI_1_2_AE";

                    break;
                  case ErrorCode.PermittedValueViolation:
                    ExitState = "DASHBOARD_STAGING_PRI_1_2_PV";

                    break;
                  default:
                    throw;
                }
              }
            }

            // -- Save Judicial District counts.
            for(local.Local1.Index = 0; local.Local1.Index < local
              .Local1.Count; ++local.Local1.Index)
            {
              if (!local.Local1.CheckSize())
              {
                break;
              }

              if (ReadDashboardStagingPriority7())
              {
                try
                {
                  UpdateDashboardStagingPriority3();
                }
                catch(Exception e)
                {
                  switch(GetErrorCode(e))
                  {
                    case ErrorCode.AlreadyExists:
                      ExitState = "DASHBOARD_STAGING_PRI_1_2_NU";

                      break;
                    case ErrorCode.PermittedValueViolation:
                      ExitState = "DASHBOARD_STAGING_PRI_1_2_PV";

                      break;
                    default:
                      throw;
                  }
                }
              }
              else
              {
                try
                {
                  CreateDashboardStagingPriority2();
                }
                catch(Exception e)
                {
                  switch(GetErrorCode(e))
                  {
                    case ErrorCode.AlreadyExists:
                      ExitState = "DASHBOARD_STAGING_PRI_1_2_AE";

                      break;
                    case ErrorCode.PermittedValueViolation:
                      ExitState = "DASHBOARD_STAGING_PRI_1_2_PV";

                      break;
                    default:
                      throw;
                  }
                }
              }
            }

            local.Local1.CheckIndex();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              local.EabFileHandling.Action = "WRITE";
              local.EabReportSend.RptDetail =
                "Error creating/updating Dashboard_Staging_Priority_1_2.";
              UseCabErrorReport();
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              return;
            }

            // -- Checkpoint Info
            // Positions   Value
            // ---------   
            // ------------------------------------
            //  001-080    General Checkpoint Info for PRAD
            //  081-088    Dashboard Priority
            //  089-089    Blank
            //  090-099    CSE Case Number
            local.ProgramCheckpointRestart.RestartInd = "Y";
            local.ProgramCheckpointRestart.RestartInfo =
              Substring(import.ProgramCheckpointRestart.RestartInfo, 250, 1, 80) +
              "1-03    " + " " + String
              (local.Prev.Number, Case1.Number_MaxLength);
            UseUpdateCheckpointRstAndCommit();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              local.EabFileHandling.Action = "WRITE";
              local.EabReportSend.RptDetail = "Error taking checkpoint.";
              UseCabErrorReport();
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              return;
            }

            local.RecordsReadSinceCommit.Count = 0;
          }
        }

        local.Prev.Number = entities.Case1.Number;
        ++local.RecordsReadSinceCommit.Count;

        // -- Re-initialize Judicial District and Office
        // -- (these attributes are used to save data between the denominator 
        // and numerator processing)
        local.Initialized.JudicialDistrict = "";
        local.Initialized.Office = 0;
        MoveDashboardAuditData1(local.Initialized, local.DashboardAuditData);
        local.CountCase.Flag = "N";

        // -------------------------------------------------------------------------------------
        // --  D E N O M I N A T O R  (Number of Cases with Arrears Due) (
        // OCSE157 Line 28)
        // -------------------------------------------------------------------------------------
        // ----------------------------------------------------------------------
        // Read all valid AP/CH and AP/AR combos - active or not.
        // READ EACH property is set to fetch 'distinct' rows to avoid
        // spinning through same AP/CH or AP/AR combo multiple times.
        // Date checks are to ensure we retrieve overlapping roles only.
        // -----------------------------------------------------------------------
        foreach(var _1 in ReadCaseRoleCsePersonCaseRoleCsePerson())
        {
          // -------------------------------------------------------------------
          // Step #1. - Read debts where bal is due on 'run date'. There
          // is a good chance that balance was also due on 9/30. Count
          // case if bal is due on 9/30.
          // -------------------------------------------------------------------
          foreach(var _2 in ReadDebtObligationObligationTypeDebtDetail1())
          {
            // -------------------------------------------------------------------
            // -Skip 718B
            // -------------------------------------------------------------------
            if (Equal(entities.ObligationType.Code, "718B"))
            {
              continue;
            }

            // -------------------------------------------------------------------
            // -Skip MJ AF, MJ FC, MJ AFI, MJ FCI.
            // -------------------------------------------------------------------
            if (Equal(entities.ObligationType.Code, "MJ"))
            {
              UseFnDeterminePgmForDebtDetail1();

              if (Equal(local.Program.Code, "AF") || Equal
                (local.Program.Code, "AFI") || Equal
                (local.Program.Code, "FC") || Equal(local.Program.Code, "FCI"))
              {
                // -----------------------------------------------
                // Skip this debt detail.
                // -----------------------------------------------
                continue;
              }
            }

            // ---------------------------------------------------------------
            // You are now looking at a debt with balance due on run date
            // and it meets ob type criteria.
            // We now need to 'undo' debt adjustments, collections and
            // collection adjustments to obtain balance due as of FY end.
            // ---------------------------------------------------------------
            local.BalDueOnFyEnd.Currency152 = entities.DebtDetail.BalanceDueAmt;

            // -----------------------------------------------
            // Process Debt Adjustments first.
            // -----------------------------------------------
            foreach(var _3 in ReadDebtAdjustment1())
            {
              // ------------------------------------------------------------------
              // I-type adj increases balance_due. So we need to 'subtract'
              // this amt from balance at run time to get balance due on 9/30.
              // Similarily 'add' D-type adjustments back.
              // ------------------------------------------------------------------
              if (AsChar(entities.DebtAdjustment.DebtAdjustmentType) == 'I')
              {
                local.BalDueOnFyEnd.Currency152 -= entities.DebtAdjustment.
                  Amount;
              }
              else
              {
                local.BalDueOnFyEnd.Currency152 += entities.DebtAdjustment.
                  Amount;
              }
            }

            // -------------------------------------------------------------
            // Next - process collections and collection adjustments.
            // Read un-adj collections created after FY end.
            // Read adj collections created during or before FY,
            // but adjusted 'after' FY end.
            // Ok to read concurrent collection.
            // --------------------------------------------------------------
            foreach(var _3 in ReadCollection1())
            {
              // --------------------------------------------------------------
              // Subtract un-adj collections. Add adjusted collections.
              // --------------------------------------------------------------
              if (AsChar(entities.Collection.AdjustedInd) == 'N')
              {
                local.BalDueOnFyEnd.Currency152 += entities.Collection.Amount;
              }
              else
              {
                local.BalDueOnFyEnd.Currency152 -= entities.Collection.Amount;
              }
            }

            // --------------------------------------------------
            // Count case if balance > 0  on FY end.
            // --------------------------------------------------
            if (local.BalDueOnFyEnd.Currency152 > 0)
            {
              local.CountCase.Flag = "Y";

              // -- Set Dashboard audit data values.
              local.DashboardAuditData.DashboardPriority = "1-3(D)#1";
              local.DashboardAuditData.CaseNumber = entities.Case1.Number;
              local.DashboardAuditData.DebtType = entities.ObligationType.Code;
              local.DashboardAuditData.DebtDueDate = entities.DebtDetail.DueDt;
              local.DashboardAuditData.DebtBalanceDue =
                entities.DebtDetail.BalanceDueAmt;
              local.DashboardAuditData.SuppCspNumber =
                entities.ChOrArCsePerson.Number;
              local.DashboardAuditData.PayorCspNumber =
                entities.ApCsePerson.Number;

              goto ReadEach2;
            }
          }

          // -------------------------------------------------------------------
          // We got here because there is either no debt with balance
          // due as of run date or balance is due on run date but
          // nothing was due at the end of FY.
          // -------------------------------------------------------------------
          // -------------------------------------------------------------------
          // Step #2. - Check if Arrears Collection was created 'during' FY.
          // -Skip direct payments through REIP (CRT = 2 or 7)
          // -Include concurrent collections.
          // -Skip collections created and adjusted during FY.
          // -------------------------------------------------------------------
          foreach(var _2 in ReadCollectionObligationTypeCollection())
          {
            if (!entities.ObligationType.Populated)
            {
              continue;
            }

            // --------------------------
            // Skip Fees, Recoveries, 718B.
            // --------------------------
            if (AsChar(entities.ObligationType.Classification) == 'F' || AsChar
              (entities.ObligationType.Classification) == 'R' || Equal
              (entities.ObligationType.Code, "718B"))
            {
              continue;
            }

            // -----------------------------------------
            // Skip MJ AF, MJ AFI, MJ FC, MJ FCI.
            // -----------------------------------------
            if (Equal(entities.ObligationType.Code, "MJ") && (
              Equal(entities.Collection.ProgramAppliedTo, "AF") || Equal
              (entities.Collection.ProgramAppliedTo, "AFI") || Equal
              (entities.Collection.ProgramAppliedTo, "FC") || Equal
              (entities.Collection.ProgramAppliedTo, "FCI")))
            {
              continue;
            }

            // -------------------------------------------------------------------------
            // 09/14/2010  CQ21451
            // If the only activity on a case is an adjustment in the current FY
            // on a collection created in a previous FY, exclude the case.
            // --------------------------------------------------------------------------
            if (entities.Adjusted.Populated)
            {
              continue;
            }

            // -----------------------------------------------------------
            // Yipeee! We found an Arrears collection created during FY.
            // -----------------------------------------------------------
            local.CountCase.Flag = "Y";

            // -- Set Dashboard audit data values.
            local.DashboardAuditData.DashboardPriority = "1-3(D)#2";
            local.DashboardAuditData.CaseNumber = entities.Case1.Number;
            local.DashboardAuditData.DebtType = entities.ObligationType.Code;
            local.DashboardAuditData.SuppCspNumber =
              entities.ChOrArCsePerson.Number;
            local.DashboardAuditData.PayorCspNumber =
              entities.ApCsePerson.Number;
            local.DashboardAuditData.CollAppliedToCd =
              entities.Collection.AppliedToCode;
            local.DashboardAuditData.CollectionAmount =
              entities.Collection.Amount;
            local.DashboardAuditData.CollectionCreatedDate =
              Date(entities.Collection.CreatedTmst);

            goto ReadEach2;
          }

          // -------------------------------------------------------------------
          // We got here because there is no debt with balance due as of
          // run date and no Arrears Collection is created 'during' FY.
          // -------------------------------------------------------------------
          // -------------------------------------------------------------------
          // Step # 3. - Check for D-type adjustments created during FY.
          // -------------------------------------------------------------------
          foreach(var _2 in ReadDebtObligationObligationTypeDebtDetailDebtAdjustment())
          {
            // -----------------------------------------------------------------
            // For Accruing debts, include if adj occurs atleast 1 month after
            // due date. (Remember - accruing debts are not considered
            // 'arrears' until 1 month after due date)
            // For Non-accruing debts, include all D-type adjustments.
            // ----------------------------------------------------------------
            if (AsChar(entities.ObligationType.Classification) == 'A' && !
              Lt(AddMonths(entities.DebtDetail.DueDt, 1),
              entities.DebtAdjustment.DebtAdjustmentDt))
            {
              continue;
            }

            // -------------------------------------------------------------------
            // -Skip 718B
            // -------------------------------------------------------------------
            if (Equal(entities.ObligationType.Code, "718B"))
            {
              continue;
            }

            // -------------------------------------------------------------------
            // -Skip MJ AF, MJ FC, MJ AFI, MJ FCI.
            // -------------------------------------------------------------------
            if (Equal(entities.ObligationType.Code, "MJ"))
            {
              UseFnDeterminePgmForDebtDetail1();

              if (Equal(local.Program.Code, "AF") || Equal
                (local.Program.Code, "AFI") || Equal
                (local.Program.Code, "FC") || Equal(local.Program.Code, "FCI"))
              {
                // -----------------------------------------------
                // Skip this debt detail.
                // -----------------------------------------------
                continue;
              }
            }

            // -------------------------------------------------------------------
            // Yipee! D-type adj found, count case.
            // -------------------------------------------------------------------
            local.CountCase.Flag = "Y";

            // -- Set Dashboard audit data values.
            local.DashboardAuditData.DashboardPriority = "1-3(D)#3";
            local.DashboardAuditData.CaseNumber = entities.Case1.Number;
            local.DashboardAuditData.DebtType = entities.ObligationType.Code;
            local.DashboardAuditData.DebtDueDate = entities.DebtDetail.DueDt;
            local.DashboardAuditData.DebtBalanceDue =
              entities.DebtDetail.BalanceDueAmt;
            local.DashboardAuditData.SuppCspNumber =
              entities.ChOrArCsePerson.Number;
            local.DashboardAuditData.PayorCspNumber =
              entities.ApCsePerson.Number;

            goto ReadEach2;
          }

          // -------------------------------------------------------------------
          // We got here because
          // No balance is due as of run date  and
          // No arrears collection was created during FY  and
          // No D-type adj is done during FY.
          // Debts could have an outstanding balance as of FY end but
          // zero balance is due on run date. This could happen if
          // 1. Collection is applied after FY end  or
          // 2. D-type debt adj is done after FY end.
          // -------------------------------------------------------------------
          // -------------------------------------------------------------------
          // Step # 4. - Look for debts with 'Zero' bal due but where a
          // Coll is applied to debt after FY end.
          // READ EACH properties are set to fetch distinct rows - so we
          // process each debt only once
          // -------------------------------------------------------------------
          foreach(var _2 in ReadDebtObligationObligationTypeDebtDetail2())
          {
            // -------------------------------------------------------------------
            // -Skip 718B
            // -------------------------------------------------------------------
            if (Equal(entities.ObligationType.Code, "718B"))
            {
              continue;
            }

            // -------------------------------------------------------------------
            // -Skip MJ AF, MJ FC, MJ AFI, MJ FCI.
            // -------------------------------------------------------------------
            if (Equal(entities.ObligationType.Code, "MJ"))
            {
              UseFnDeterminePgmForDebtDetail1();

              if (Equal(local.Program.Code, "AF") || Equal
                (local.Program.Code, "AFI") || Equal
                (local.Program.Code, "FC") || Equal(local.Program.Code, "FCI"))
              {
                // -----------------------------------------------
                // Skip this debt detail.
                // -----------------------------------------------
                continue;
              }
            }

            // --------------------------------------------------------------
            // DD balance due is 0. In effect, this SET statement resets
            // local view for each parse.
            // --------------------------------------------------------------
            local.BalDueOnFyEnd.Currency152 = entities.DebtDetail.BalanceDueAmt;

            // ---------------------------------------------------------------
            // You are now looking at a debt with zero balance on run date
            // and it meets ob type criteria. We also know that there is
            // atleast one coll applied to current debt after FY end.
            // We now need to 'undo' debt adjustments, collections and
            // collection adjustments to obtain balance due as of FY end.
            // ---------------------------------------------------------------
            // -----------------------------------------------
            // Process Debt Adjustments first.
            // -----------------------------------------------
            foreach(var _3 in ReadDebtAdjustment1())
            {
              // ------------------------------------------------------------------
              // I-type adj increases balance_due. So we need to 'subtract'
              // this amt from balance at run time to get balance due on 9/30.
              // Similarily 'add' D-type adjustments back.
              // ------------------------------------------------------------------
              if (AsChar(entities.DebtAdjustment.DebtAdjustmentType) == 'I')
              {
                local.BalDueOnFyEnd.Currency152 -= entities.DebtAdjustment.
                  Amount;
              }
              else
              {
                local.BalDueOnFyEnd.Currency152 += entities.DebtAdjustment.
                  Amount;
              }
            }

            // -------------------------------------------------------------
            // Next, process collections and collection adjustments.
            // Read un-adjusted collections created after FY end.
            // Read adj collections created during or before FY,
            // but adjusted 'after' FY end.
            // Include concurrent collections.
            // -------------------------------------------------------------
            foreach(var _3 in ReadCollection1())
            {
              // -----------------------------------------------------------
              // Subtract collections. Add collection adjustments.
              // -----------------------------------------------------------
              if (AsChar(entities.Collection.AdjustedInd) == 'N')
              {
                local.BalDueOnFyEnd.Currency152 += entities.Collection.Amount;
              }
              else
              {
                local.BalDueOnFyEnd.Currency152 -= entities.Collection.Amount;
              }
            }

            // -----------------------------------------------
            // Count case if balance was due on 9/30
            // -----------------------------------------------
            if (local.BalDueOnFyEnd.Currency152 > 0)
            {
              local.CountCase.Flag = "Y";

              // -- Set Dashboard audit data values.
              local.DashboardAuditData.DashboardPriority = "1-3(D)#4";
              local.DashboardAuditData.CaseNumber = entities.Case1.Number;
              local.DashboardAuditData.DebtType = entities.ObligationType.Code;
              local.DashboardAuditData.DebtDueDate = entities.DebtDetail.DueDt;
              local.DashboardAuditData.DebtBalanceDue =
                entities.DebtDetail.BalanceDueAmt;
              local.DashboardAuditData.SuppCspNumber =
                entities.ChOrArCsePerson.Number;
              local.DashboardAuditData.PayorCspNumber =
                entities.ApCsePerson.Number;

              goto ReadEach2;
            }
          }

          // -------------------------------------------------------------------
          // Step # 5. - Look for debts with 'Zero' bal due but where a
          // D-type adjustment is made to debt after FY end.
          // READ EACH properties are set to fetch distinct rows - so we
          // process each debt only once
          // -------------------------------------------------------------------
          foreach(var _2 in ReadDebtObligationObligationTypeDebtDetail3())
          {
            // -------------------------------------------------------------------
            // -Skip 718B
            // -------------------------------------------------------------------
            if (Equal(entities.ObligationType.Code, "718B"))
            {
              continue;
            }

            // -------------------------------------------------------------------
            // -Skip MJ AF, MJ FC, MJ AFI, MJ FCI.
            // -------------------------------------------------------------------
            if (Equal(entities.ObligationType.Code, "MJ"))
            {
              UseFnDeterminePgmForDebtDetail1();

              if (Equal(local.Program.Code, "AF") || Equal
                (local.Program.Code, "AFI") || Equal
                (local.Program.Code, "FC") || Equal(local.Program.Code, "FCI"))
              {
                // -----------------------------------------------
                // Skip this debt detail.
                // -----------------------------------------------
                continue;
              }
            }

            // --------------------------------------------------------------
            // DD balance due is 0. In effect, this SET statement resets
            // local view for each parse.
            // --------------------------------------------------------------
            local.BalDueOnFyEnd.Currency152 = entities.DebtDetail.BalanceDueAmt;

            // ---------------------------------------------------------------
            // You are now looking at a debt with zero balance on run date
            // and it meets ob type criteria. We also know that there is
            // atleast one D-type adjustment to debt 'after' FY end.
            // We now need to 'undo' debt adjustments, collections and
            // collection adjustments to obtain balance due as of FY end.
            // ---------------------------------------------------------------
            // -----------------------------------------------
            // Process Debt Adjustments first.
            // -----------------------------------------------
            foreach(var _3 in ReadDebtAdjustment1())
            {
              // ------------------------------------------------------------------
              // I-type adj increases balance_due. So we need to 'subtract'
              // this amt from balance at run time to get balance due on 9/30.
              // Similarily 'add' D-type adjustments back.
              // ------------------------------------------------------------------
              if (AsChar(entities.DebtAdjustment.DebtAdjustmentType) == 'I')
              {
                local.BalDueOnFyEnd.Currency152 -= entities.DebtAdjustment.
                  Amount;
              }
              else
              {
                local.BalDueOnFyEnd.Currency152 += entities.DebtAdjustment.
                  Amount;
              }
            }

            // -------------------------------------------------------------
            // Next, process collections and collection adjustments.
            // Read un-adjusted collections created after FY end.
            // Read adj collections created during or before FY,
            // but adjusted 'after' FY end.
            // Include concurrent collections.
            // -------------------------------------------------------------
            foreach(var _3 in ReadCollection1())
            {
              // -----------------------------------------------------------
              // Subtract collections. Add collection adjustments.
              // -----------------------------------------------------------
              if (AsChar(entities.Collection.AdjustedInd) == 'N')
              {
                local.BalDueOnFyEnd.Currency152 += entities.Collection.Amount;
              }
              else
              {
                local.BalDueOnFyEnd.Currency152 -= entities.Collection.Amount;
              }
            }

            // -----------------------------------------------
            // Count case if balance was due on 9/30
            // -----------------------------------------------
            if (local.BalDueOnFyEnd.Currency152 > 0)
            {
              local.CountCase.Flag = "Y";

              // -- Set Dashboard audit data values.
              local.DashboardAuditData.DashboardPriority = "1-3(D)#5";
              local.DashboardAuditData.CaseNumber = entities.Case1.Number;
              local.DashboardAuditData.DebtType = entities.ObligationType.Code;
              local.DashboardAuditData.DebtDueDate = entities.DebtDetail.DueDt;
              local.DashboardAuditData.DebtBalanceDue =
                entities.DebtDetail.BalanceDueAmt;
              local.DashboardAuditData.SuppCspNumber =
                entities.ChOrArCsePerson.Number;
              local.DashboardAuditData.PayorCspNumber =
                entities.ApCsePerson.Number;

              goto ReadEach2;
            }
          }

          // -----------------------------------------------
          // End of AP/CH READ EACH.
          // -----------------------------------------------
        }

ReadEach2:

        if (AsChar(local.CountCase.Flag) == 'N')
        {
          // -- Case does not owe arrears.  Skip this case.
          continue;
        }

        // -------------------------------------------------------------------------------------
        // -- Case owes arrears.
        // -- Include case in the Priority 1-3 denominator (Number of Cases with
        // Arrears Due).
        // -- This is the same as OCSE157 Line 28.
        // -------------------------------------------------------------------------------------
        // -- Increment Statewide Level
        local.Statewide.CasesPayingArrearsDenominator =
          (local.Statewide.CasesPayingArrearsDenominator ?? 0) + 1;

        // -- Determine Judicial District...
        if (!Lt(import.ReportEndDate.Date,
          entities.CaseAssignment.DiscontinueDate))
        {
          // -- Pass the case assignment end date to the Determine JD cab so it
          // -- will find the JD the case belonged to on the closure date.
          local.TempEndDate.Date = entities.CaseAssignment.DiscontinueDate;
        }
        else
        {
          local.TempEndDate.Date = import.ReportEndDate.Date;
        }

        UseFnB734DetermineJdFromCase();

        // -- Save office and judicial district for the numerator processing.
        local.Initialized.JudicialDistrict =
          local.DashboardAuditData.JudicialDistrict;
        local.Initialized.Office = local.DashboardAuditData.Office ?? 0;

        // -- Increment Judicial District Level
        if (!IsEmpty(local.DashboardAuditData.JudicialDistrict))
        {
          local.Local1.Index =
            (int)StringToNumber(local.DashboardAuditData.JudicialDistrict) - 1;
          local.Local1.CheckSize();

          local.Local1.Update.G.CasesPayingArrearsDenominator =
            (local.Local1.Item.G.CasesPayingArrearsDenominator ?? 0) + 1;
        }

        if (AsChar(import.AuditFlag.Flag) == 'Y')
        {
          // -- Log to the dashboard audit table.
          UseFnB734CreateDashboardAudit2();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            return;
          }
        }

        local.CountCaseWk.Flag = "";

        if (ReadCaseAssignmentServiceProvider())
        {
          local.CountCaseWk.Flag = "Y";
        }

        local.DashboardStagingPriority35.Assign(
          local.NullDashboardStagingPriority35);

        if (AsChar(local.CountCaseWk.Flag) == 'Y')
        {
          local.Worker.Assign(local.DashboardAuditData);
          local.DashboardStagingPriority35.AsOfDate =
            import.ProgramProcessingInfo.ProcessDate;
          local.DashboardStagingPriority35.ReportLevel = "CW";
          local.DashboardStagingPriority35.ReportLevelId =
            entities.WorkerServiceProvider.UserId;
          local.DashboardStagingPriority35.ReportMonth =
            import.DashboardAuditData.ReportMonth;
          local.DashboardStagingPriority35.CasesPayingArrearsDenominator =
            (local.DashboardStagingPriority35.CasesPayingArrearsDenominator ?? 0)
            + 1;

          switch(TrimEnd(local.DashboardAuditData.DashboardPriority))
          {
            case "1-3(D)#1":
              local.Worker.DashboardPriority = "1-3.1D1";

              break;
            case "1-3(D)#2":
              local.Worker.DashboardPriority = "1-3.1D2";

              break;
            case "1-3(D)#3":
              local.Worker.DashboardPriority = "1-3.1D3";

              break;
            case "1-3(D)#4":
              local.Worker.DashboardPriority = "1-3.1D4";

              break;
            case "1-3(D)#5":
              local.Worker.DashboardPriority = "1-3.1D5";

              break;
            default:
              local.Worker.DashboardPriority = "1-3.1";

              break;
          }

          local.Worker.DebtBalanceDue =
            local.DashboardAuditData.DebtBalanceDue ?? 0M;
          local.Worker.CollectionAmount =
            local.DashboardAuditData.CollectionAmount ?? 0M;
          local.Worker.CaseNumber = entities.Case1.Number;
          local.Worker.CaseDate = entities.Case1.StatusDate;
          local.Worker.WorkerId =
            local.DashboardStagingPriority35.ReportLevelId;

          if (AsChar(import.AuditFlag.Flag) == 'Y')
          {
            // -- Log to the dashboard audit table.
            UseFnB734CreateDashboardAudit1();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }
          }

          if (ReadDashboardStagingPriority8())
          {
            try
            {
              UpdateDashboardStagingPriority4();
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  ExitState = "DASHBOARD_STAGING_PRI_3_5_NU";

                  break;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "DASHBOARD_STAGING_PRI_3_5_PV";

                  break;
                default:
                  throw;
              }
            }
          }
          else
          {
            try
            {
              CreateDashboardStagingPriority3();
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  ExitState = "DASHBOARD_STAGING_PRI_3_5_AE";

                  break;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "DASHBOARD_STAGING_PRI_3_5_PV";

                  break;
                default:
                  throw;
              }
            }
          }

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail =
              "Error creating/updating Dashboard_Staging_Priority_1_2.";
            UseCabErrorReport();
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }
        }

        local.DashboardStagingPriority35.Assign(
          local.NullDashboardStagingPriority35);
        local.CountCaseAtty.Flag = "";

        foreach(var _1 in ReadLegalReferralServiceProvider())
        {
          local.Worker.Assign(local.DashboardAuditData);
          local.Worker.LegalReferralNumber = entities.LegalReferral.Identifier;
          local.Worker.LegalReferralDate = entities.LegalReferral.ReferralDate;
          local.CountCaseAtty.Flag = "Y";

          if (AsChar(local.CountCaseAtty.Flag) == 'Y')
          {
            // -- Case does not owe arrears.  Skip this case.
            local.DashboardStagingPriority35.AsOfDate =
              import.ProgramProcessingInfo.ProcessDate;

            if (Equal(entities.ServiceProvider.RoleCode, "AT") || Equal
              (entities.ServiceProvider.RoleCode, "CT"))
            {
              local.DashboardStagingPriority35.ReportLevel = "AT";
            }
            else
            {
              local.DashboardStagingPriority35.ReportLevel = "CA";
            }

            local.DashboardStagingPriority35.ReportLevelId =
              entities.ServiceProvider.UserId;
            local.DashboardStagingPriority35.ReportMonth =
              import.DashboardAuditData.ReportMonth;
            local.DashboardStagingPriority35.CasesPayingArrearsDenominator = 1;
            local.Worker.CaseNumber = entities.Case1.Number;
            local.Worker.CaseDate = entities.Case1.StatusDate;
            local.Worker.WorkerId =
              local.DashboardStagingPriority35.ReportLevelId;

            switch(TrimEnd(local.DashboardAuditData.DashboardPriority))
            {
              case "1-3(D)#1":
                local.Worker.DashboardPriority = "1-3.2D1";

                break;
              case "1-3(D)#2":
                local.Worker.DashboardPriority = "1-3.2D2";

                break;
              case "1-3(D)#3":
                local.Worker.DashboardPriority = "1-3.2D3";

                break;
              case "1-3(D)#4":
                local.Worker.DashboardPriority = "1-3.2D4";

                break;
              case "1-3(D)#5":
                local.Worker.DashboardPriority = "1-3.2D5";

                break;
              default:
                local.Worker.DashboardPriority = "1-3.2";

                break;
            }

            if (AsChar(import.AuditFlag.Flag) == 'Y')
            {
              // -- Log to the dashboard audit table.
              UseFnB734CreateDashboardAudit1();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                return;
              }
            }

            if (ReadDashboardStagingPriority8())
            {
              try
              {
                UpdateDashboardStagingPriority4();
              }
              catch(Exception e)
              {
                switch(GetErrorCode(e))
                {
                  case ErrorCode.AlreadyExists:
                    ExitState = "DASHBOARD_STAGING_PRI_3_5_NU";

                    break;
                  case ErrorCode.PermittedValueViolation:
                    ExitState = "DASHBOARD_STAGING_PRI_3_5_PV";

                    break;
                  default:
                    throw;
                }
              }
            }
            else
            {
              try
              {
                CreateDashboardStagingPriority3();
              }
              catch(Exception e)
              {
                switch(GetErrorCode(e))
                {
                  case ErrorCode.AlreadyExists:
                    ExitState = "DASHBOARD_STAGING_PRI_3_5_AE";

                    break;
                  case ErrorCode.PermittedValueViolation:
                    ExitState = "DASHBOARD_STAGING_PRI_3_5_PV";

                    break;
                  default:
                    throw;
                }
              }
            }

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              local.EabFileHandling.Action = "WRITE";
              local.EabReportSend.RptDetail =
                "Error creating/updating Dashboard_Staging_Priority_1_2.";
              UseCabErrorReport();
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              return;
            }

            break;
          }
        }

        // -------------------------------------------------------------------------------------
        // -- N U M E R A T O R (Number of Cases Paying on Arrears) (OCSE157 
        // Line 29)
        // -------------------------------------------------------------------------------------
        // ----------------------------------------------------------------------
        // Read all APs on Case - active or not.
        // READ EACH property is set to fetch 'distinct' rows to avoid
        // spinning through same person multiple times.
        // -----------------------------------------------------------------------
        // -- The local_initialized view will have the Judicial District and 
        // Office derived during denominator processing.
        MoveDashboardAuditData1(local.Initialized, local.DashboardAuditData);
        local.CountCase.Flag = "N";

        foreach(var _1 in ReadCsePersonCollectionCsePersonObligationType1())
        {
          // -------------------------------------------------------------------
          // Step # 1 - Look for NA or NAI Collection for current AP
          // -Created 'during' FY.
          // -Applied to Arrears
          // -Skip direct payments through REIP (CRT = 2 or 7)
          // -Skip concurrent collections.(9/4/2001)
          // -Skip collections created and adjusted during FY.
          // -------------------------------------------------------------------
          if (entities.Collection.Populated)
          {
            if (!entities.Supp.Populated)
            {
              continue;
            }

            if (!entities.ObligationType.Populated)
            {
              continue;
            }

            // ---------------------------------
            // Skip Fees, Recoveries, 718B.
            // ---------------------------------
            if (AsChar(entities.ObligationType.Classification) == 'F' || AsChar
              (entities.ObligationType.Classification) == 'R' || Equal
              (entities.ObligationType.Code, "718B"))
            {
              continue;
            }

            // -----------------------------------------
            // Skip MJ AF, MJ AFI, MJ FC, MJ FCI.
            // -----------------------------------------
            if (Equal(entities.ObligationType.Code, "MJ") && (
              Equal(entities.Collection.ProgramAppliedTo, "AF") || Equal
              (entities.Collection.ProgramAppliedTo, "AFI") || Equal
              (entities.Collection.ProgramAppliedTo, "FC") || Equal
              (entities.Collection.ProgramAppliedTo, "FCI")))
            {
              continue;
            }

            // -----------------------------------------------------------
            // Ok, we found a desired Arrears collection. But was this
            // collection applied to a CH/AR on this case?
            // -----------------------------------------------------------
            if (!entities.ChOrArCaseRole.Populated)
            {
              continue;
            }

            // ------------------------------------------------------------------
            // 07/11/2008  CQ2461
            // If the only activity on the Case is an adjustment in the current 
            // FY
            // on a Collection created in a previous FY, exclude the case.
            // ------------------------------------------------------------------
            if (entities.Adjusted.Populated)
            {
              continue;
            }
            else
            {
              // -- Continue...
            }

            // -----------------------------------------------------------
            // Yipeee! We found an NA or NAI collection - Count case.
            // -----------------------------------------------------------
            local.CountCase.Flag = "Y";
            local.DashboardAuditData.DashboardPriority = "1-3(N)#1";
            local.DashboardAuditData.CaseNumber = entities.Case1.Number;
            local.DashboardAuditData.CollectionAmount =
              entities.Collection.Amount;
            local.DashboardAuditData.CollAppliedToCd =
              entities.Collection.AppliedToCode;
            local.DashboardAuditData.CollectionCreatedDate =
              Date(entities.Collection.CreatedTmst);
            local.DashboardAuditData.DebtType = entities.ObligationType.Code;
            local.DashboardAuditData.SuppCspNumber = entities.Supp.Number;
            local.DashboardAuditData.PayorCspNumber =
              entities.ApCsePerson.Number;

            goto ReadEach3;
          }
        }

ReadEach3:

        if (AsChar(local.CountCase.Flag) == 'N')
        {
          // -------------------------------------------------------------------
          // We got here because no NA or NAI arrears coll was received.
          // Now check if we received any non-NA arrears coll during FY.
          // -------------------------------------------------------------------
          foreach(var _1 in ReadCsePerson())
          {
            // -------------------------------------------------------------------
            // Step #2 - Look for non-NA, non-NAI Collection for current AP
            // -Created 'during' FY.
            // -Applied to Arrears
            // -Skip direct payments through REIP (CRT = 2 or 7)
            // -Skip concurrent collections. (9/4/2001)
            // -Skip collections created and adjusted during FY.
            // -------------------------------------------------------------------
            local.CollFound.Flag = "N";

            foreach(var _2 in ReadCollectionCsePersonObligationTypeCaseRoleCollection1())
            {
              if (!entities.Supp.Populated)
              {
                continue;
              }

              if (!entities.ObligationType.Populated)
              {
                continue;
              }

              // ---------------------------------
              // Skip Fees, Recoveries, 718B.
              // ---------------------------------
              if (AsChar(entities.ObligationType.Classification) == 'F' || AsChar
                (entities.ObligationType.Classification) == 'R' || Equal
                (entities.ObligationType.Code, "718B"))
              {
                continue;
              }

              // -----------------------------------------
              // Skip MJ AF, MJ AFI, MJ FC, MJ FCI.
              // -----------------------------------------
              if (Equal(entities.ObligationType.Code, "MJ") && (
                Equal(entities.Collection.ProgramAppliedTo, "AF") || Equal
                (entities.Collection.ProgramAppliedTo, "AFI") || Equal
                (entities.Collection.ProgramAppliedTo, "FC") || Equal
                (entities.Collection.ProgramAppliedTo, "FCI")))
              {
                continue;
              }

              // -----------------------------------------------------------
              // Ok, we found a desired Arrears collection for AP. But does
              // the Supp Person participate on this case?
              // -----------------------------------------------------------
              if (!entities.ChOrArCaseRole.Populated)
              {
                continue;
              }

              // ------------------------------------------------------------------
              // 07/11/2008  CQ2461
              // If the only activity on the Case is an adjustment in the 
              // current FY
              // on a Collection created in a previous FY, exclude the case.
              // ------------------------------------------------------------------
              if (entities.Adjusted.Populated)
              {
                continue;
              }
              else
              {
                // -- Continue...
              }

              local.CollFound.Flag = "Y";

              // -----------------------------------------------------------
              // Save these views to create verification record later.
              // -----------------------------------------------------------
              local.Ap.Number = entities.ApCsePerson.Number;
              local.Supp.Number = entities.Supp.Number;
              MoveCollection(entities.Collection, local.NonNa);

              goto ReadEach4;
            }
          }

ReadEach4:

          if (AsChar(local.CollFound.Flag) == 'N')
          {
            // -------------------------------------------------------------
            // No Arrears coll was received during FY. Skip Case.
            // -------------------------------------------------------------
            continue;
          }

          // -------------------------------------------------------------------
          // We got here because no NA or NAI arrears coll was received
          // but a collection was definitely received.
          // -------------------------------------------------------------------
          // -------------------------------------------------------------------
          // Step # 3. - Look for FDSO collection (coll type = 3 or 27)
          // -------------------------------------------------------------------
          foreach(var _1 in ReadCsePerson())
          {
            local.CollectionDate.Date = local.NullDateWorkArea.Date;

            foreach(var _2 in ReadCollectionCollection1())
            {
              if (Equal(entities.Collection.CollectionDt,
                local.CollectionDate.Date))
              {
                continue;
              }

              local.CollectionDate.Date = entities.Collection.CollectionDt;

              // -------------------------------------------------------------
              // Check to see if all supp persons have AF, AFI, FC, FCI active
              // as of collection date?
              // -------------------------------------------------------------
              UseFnCheckForActiveAfFcPgm();

              // ----------------------------------------------------------
              // If all supp persons on Case are on assistance as of coll date,
              // we would never derive NA on any debt. Skip Collection.
              // ----------------------------------------------------------
              if (AsChar(local.Assistance.Flag) == 'Y')
              {
                continue;
              }

              // ------------------------------------------------------------------
              // 07/11/2008  CQ2461
              // If the only activity on the Case is an adjustment in the 
              // current FY
              // on a Collection created in a previous FY, exclude the case.
              // ------------------------------------------------------------------
              if (entities.Adjusted.Populated)
              {
                continue;
              }
              else
              {
                // -- Continue...
              }

              // ----------------------------------------------------------
              // Atleast one person on case is not on assistance as of coll
              // date. Spin through each debt with due date <= coll date to
              // check if NA or NAI program is determined.
              // ----------------------------------------------------------
              // ------------------------------------------------------------
              // We will only look for debts with bal due as of FY end.
              // First, read debts with bal due as of today.
              // Later, we'll read debts with zero bal today but bal was due
              // on FY end.
              // -------------------------------------------------------------
              foreach(var _3 in ReadDebtDetailObligationObligationTypeCsePerson1())
              {
                UseFnDeterminePgmForDebtDetail2();

                if (Equal(local.Program.Code, "NA") || Equal
                  (local.Program.Code, "NAI"))
                {
                  // -----------------------------------------------------------
                  // Ok, we found a debt for current AP that derives NA. Does 
                  // the
                  // supp person participate on current case?
                  // -----------------------------------------------------------
                  if (!ReadCaseRole())
                  {
                    // -----------------------------------------------------------
                    // AP must be on multiple cases and we probably hit a debt 
                    // for
                    // a kid on another case
                    // -----------------------------------------------------------
                    continue;
                  }

                  // ----------------------------------------------------------------
                  // NA or NAI was owed and we know that no arrears collection
                  // was applied to NA. Skip Case.
                  // ----------------------------------------------------------------
                  // @@@
                  goto ReadEach5;
                }
              }

              // -----------------------------------------------------------------------
              // We either didn't find any debt with bal due or we found a debt
              // but no NA or NAI was owed on Collection date.
              // Now read for debts with zero bal today but where bal was
              // due on FY end.
              // -----------------------------------------------------------------------
              // -----------------------------------------------------------------------
              // Only do this if there is any collection/debt activity
              // for AP after FY end. There is no point in spinning through all
              // Zero debts if there is no activity at all for this AP.
              // -----------------------------------------------------------------------
              if (!ReadCollection2())
              {
                if (!ReadDebtAdjustment2())
                {
                  // -------------------------------------------------------------------------
                  // No collection/debt activity for AP after FY end.
                  // Process next FDSO Collection.
                  // --------------------------------------------------------------------------
                  continue;
                }
              }

              // -----------------------------------------------------------------------
              // We got here because there was some activity for AP since
              // FY end.
              // Now read for debts with zero bal today. Then determine if bal
              // was due on FY end.
              // -----------------------------------------------------------------------
              foreach(var _3 in ReadDebtDetailObligationObligationTypeCsePerson2())
              {
                // -----------------------------------------------------------------------
                // Check if debt has had any activity after FY end.
                // -----------------------------------------------------------------------
                if (!entities.AfterFy.Populated)
                {
                  if (!ReadDebtAdjustment3())
                  {
                    // -------------------------------------------------------------------------
                    // No activity on debt since FY end. Process next debt.
                    // --------------------------------------------------------------------------
                    continue;
                  }
                }

                // -------------------------------------------------------------------------
                // You are looking at a debt with zero bal today but balance
                // was due on FY end.
                // --------------------------------------------------------------------------
                UseFnDeterminePgmForDebtDetail2();

                if (Equal(local.Program.Code, "NA") || Equal
                  (local.Program.Code, "NAI"))
                {
                  // -----------------------------------------------------------
                  // Ok, we found a debt for current AP that derives NA. Does 
                  // the
                  // supp person participate on current case?
                  // -----------------------------------------------------------
                  if (!ReadCaseRole())
                  {
                    // -----------------------------------------------------------
                    // AP must be on multiple cases and we probably hit a debt 
                    // for
                    // a kid on another case
                    // -----------------------------------------------------------
                    continue;
                  }

                  // ----------------------------------------------------------------
                  // NA or NAI was owed and we know that no arrears collection
                  // was applied to NA. Skip Case.
                  // ----------------------------------------------------------------
                  // @@@
                  goto ReadEach5;
                }
              }

              // ----------------------------------------------------------
              // We got here because we didn't derive NA or NAI on any debt
              // for current FDSO collection date. Process next FDSO coll.
              // ----------------------------------------------------------
            }
          }

          // ----------------------------------------------------------
          // We got here because we either didn't find any FDSO
          // collection or we found FDSO collections but none of the
          // debts derived NA or NAI.
          // We also know that non-NA arrears collection was definitely
          // received. No further processing is necessary. Count case!
          // ----------------------------------------------------------
          local.CountCase.Flag = "Y";
          local.DashboardAuditData.DashboardPriority = "1-3(N)#2";
          local.DashboardAuditData.CaseNumber = entities.Case1.Number;
          local.DashboardAuditData.CollectionAmount = local.NonNa.Amount;
          local.DashboardAuditData.CollAppliedToCd = local.NonNa.AppliedToCode;
          local.DashboardAuditData.CollectionCreatedDate =
            Date(local.NonNa.CreatedTmst);
          local.DashboardAuditData.SuppCspNumber = local.Supp.Number;
          local.DashboardAuditData.PayorCspNumber = local.Ap.Number;
        }

        if (AsChar(local.CountCase.Flag) == 'N')
        {
          // -- Case does not meet criteria.
          continue;
        }

        // -------------------------------------------------------------------------------------
        // -- Case paid on arrears.
        // -- Include case in the Priority 1-3 numerator (Number of Cases Paying
        // on Arrears).
        // -- This is the same as OCSE157 Line 29.
        // -------------------------------------------------------------------------------------
        // -- Increment Statewide Level
        local.Statewide.CasesPayingArrearsNumerator =
          (local.Statewide.CasesPayingArrearsNumerator ?? 0) + 1;

        // -- Judicial District and office were retrieved in the denominator 
        // processing.
        // -- Increment Judicial District Level
        if (!IsEmpty(local.DashboardAuditData.JudicialDistrict))
        {
          local.Local1.Index =
            (int)StringToNumber(local.DashboardAuditData.JudicialDistrict) - 1;
          local.Local1.CheckSize();

          local.Local1.Update.G.CasesPayingArrearsNumerator =
            (local.Local1.Item.G.CasesPayingArrearsNumerator ?? 0) + 1;
        }

        if (AsChar(import.AuditFlag.Flag) == 'Y')
        {
          // -- Log to the dashboard audit table.
          UseFnB734CreateDashboardAudit2();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            return;
          }
        }

        local.DashboardStagingPriority35.Assign(
          local.NullDashboardStagingPriority35);

        if (AsChar(local.CountCaseWk.Flag) == 'Y')
        {
          local.DashboardStagingPriority35.AsOfDate =
            import.ProgramProcessingInfo.ProcessDate;
          local.DashboardStagingPriority35.ReportLevel = "CW";
          local.DashboardStagingPriority35.ReportLevelId =
            entities.WorkerServiceProvider.UserId;
          local.DashboardStagingPriority35.ReportMonth =
            import.DashboardAuditData.ReportMonth;
          local.DashboardStagingPriority35.CasesPayingArrearsNumerator =
            (local.DashboardStagingPriority35.CasesPayingArrearsNumerator ?? 0) +
            1;
          local.Worker.Assign(local.DashboardAuditData);

          switch(TrimEnd(local.DashboardAuditData.DashboardPriority))
          {
            case "1-3(N)#1":
              local.Worker.DashboardPriority = "1-3.1N1";
              local.Worker.CollectionAmount =
                local.DashboardAuditData.CollectionAmount ?? 0M;

              break;
            case "1-3(N)#2":
              local.Worker.DashboardPriority = "1-3.1N2";
              local.Worker.CollectionAmount = local.NonNa.Amount;

              break;
            case "1-3(N)#3":
              local.Worker.DashboardPriority = "1-3.1N3";

              break;
            default:
              local.Worker.DashboardPriority = "1-3.1";

              break;
          }

          local.Worker.CaseNumber = entities.Case1.Number;
          local.Worker.CaseDate = entities.Case1.StatusDate;
          local.Worker.WorkerId =
            local.DashboardStagingPriority35.ReportLevelId;

          // -- Determine office and judicial district to which case is assigned
          // on the report period end date.
          if (AsChar(import.AuditFlag.Flag) == 'Y')
          {
            // -- Log to the dashboard audit table.
            UseFnB734CreateDashboardAudit1();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }
          }

          if (ReadDashboardStagingPriority8())
          {
            try
            {
              UpdateDashboardStagingPriority5();
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  ExitState = "DASHBOARD_STAGING_PRI_3_5_NU";

                  break;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "DASHBOARD_STAGING_PRI_3_5_PV";

                  break;
                default:
                  throw;
              }
            }
          }
          else
          {
            try
            {
              CreateDashboardStagingPriority4();
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  ExitState = "DASHBOARD_STAGING_PRI_3_5_AE";

                  break;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "DASHBOARD_STAGING_PRI_3_5_PV";

                  break;
                default:
                  throw;
              }
            }
          }

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail =
              "Error creating/updating Dashboard_Staging_Priority_1_2.";
            UseCabErrorReport();
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }
        }

        local.DashboardStagingPriority35.Assign(
          local.NullDashboardStagingPriority35);

        if (AsChar(local.CountCaseAtty.Flag) == 'Y')
        {
          // -- Case does not owe arrears.  Skip this case.
          local.DashboardStagingPriority35.AsOfDate =
            import.ProgramProcessingInfo.ProcessDate;

          if (Equal(entities.ServiceProvider.RoleCode, "AT") || Equal
            (entities.ServiceProvider.RoleCode, "CT"))
          {
            local.DashboardStagingPriority35.ReportLevel = "AT";
          }
          else
          {
            local.DashboardStagingPriority35.ReportLevel = "CA";
          }

          local.DashboardStagingPriority35.ReportLevelId =
            entities.ServiceProvider.UserId;
          local.DashboardStagingPriority35.ReportMonth =
            import.DashboardAuditData.ReportMonth;
          local.DashboardStagingPriority35.CasesPayingArrearsNumerator = 1;
          local.Worker.Assign(local.DashboardAuditData);

          switch(TrimEnd(local.DashboardAuditData.DashboardPriority))
          {
            case "1-3(N)#1":
              local.Worker.DashboardPriority = "1-3.2N1";
              local.Worker.CollectionAmount =
                local.DashboardAuditData.CollectionAmount ?? 0M;

              break;
            case "1-3(N)#2":
              local.Worker.DashboardPriority = "1-3.2N2";
              local.Worker.CollectionAmount = local.NonNa.Amount;

              break;
            case "1-3(N)#3":
              local.Worker.DashboardPriority = "1-3.2N3";

              break;
            default:
              local.Worker.DashboardPriority = "1-3.2";

              break;
          }

          local.Worker.CaseNumber = entities.Case1.Number;
          local.Worker.CaseDate = entities.Case1.StatusDate;
          local.Worker.WorkerId =
            local.DashboardStagingPriority35.ReportLevelId;

          if (AsChar(import.AuditFlag.Flag) == 'Y')
          {
            // -- Log to the dashboard audit table.
            UseFnB734CreateDashboardAudit1();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }
          }

          if (ReadDashboardStagingPriority8())
          {
            try
            {
              UpdateDashboardStagingPriority5();
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  ExitState = "DASHBOARD_STAGING_PRI_3_5_NU";

                  break;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "DASHBOARD_STAGING_PRI_3_5_PV";

                  break;
                default:
                  throw;
              }
            }
          }
          else
          {
            try
            {
              CreateDashboardStagingPriority4();
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  ExitState = "DASHBOARD_STAGING_PRI_3_5_AE";

                  break;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "DASHBOARD_STAGING_PRI_3_5_PV";

                  break;
                default:
                  throw;
              }
            }
          }

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail =
              "Error creating/updating Dashboard_Staging_Priority_1_2.";
            UseCabErrorReport();
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }
        }

ReadEach5:
        ;
      }
    }
    else
    {
      // 2/04/20 GVandy  CQ66220  Beginning in FY 2022, include only amounts 
      // that are both
      // distributed and disbursed.
      // ----------------------------------------------------------------------
      // Read each case open at some point during the FY.
      // -----------------------------------------------------------------------
      foreach(var _ in ReadCaseCaseAssignment())
      {
        if (Equal(entities.Case1.Number, local.Prev.Number))
        {
          continue;
        }
        else
        {
          // ------------------------------------------------------------------------------
          // -- Checkpoint saving all the info needed for restarting.
          // ------------------------------------------------------------------------------
          if (local.RecordsReadSinceCommit.Count >= (
            import.ProgramCheckpointRestart.ReadFrequencyCount ?? 0))
          {
            // -- Save the Statewide counts.
            if (ReadDashboardStagingPriority6())
            {
              try
              {
                UpdateDashboardStagingPriority2();
              }
              catch(Exception e)
              {
                switch(GetErrorCode(e))
                {
                  case ErrorCode.AlreadyExists:
                    ExitState = "DASHBOARD_STAGING_PRI_1_2_NU";

                    break;
                  case ErrorCode.PermittedValueViolation:
                    ExitState = "DASHBOARD_STAGING_PRI_1_2_PV";

                    break;
                  default:
                    throw;
                }
              }
            }
            else
            {
              try
              {
                CreateDashboardStagingPriority1();
              }
              catch(Exception e)
              {
                switch(GetErrorCode(e))
                {
                  case ErrorCode.AlreadyExists:
                    ExitState = "DASHBOARD_STAGING_PRI_1_2_AE";

                    break;
                  case ErrorCode.PermittedValueViolation:
                    ExitState = "DASHBOARD_STAGING_PRI_1_2_PV";

                    break;
                  default:
                    throw;
                }
              }
            }

            // -- Save Judicial District counts.
            for(local.Local1.Index = 0; local.Local1.Index < local
              .Local1.Count; ++local.Local1.Index)
            {
              if (!local.Local1.CheckSize())
              {
                break;
              }

              if (ReadDashboardStagingPriority7())
              {
                try
                {
                  UpdateDashboardStagingPriority3();
                }
                catch(Exception e)
                {
                  switch(GetErrorCode(e))
                  {
                    case ErrorCode.AlreadyExists:
                      ExitState = "DASHBOARD_STAGING_PRI_1_2_NU";

                      break;
                    case ErrorCode.PermittedValueViolation:
                      ExitState = "DASHBOARD_STAGING_PRI_1_2_PV";

                      break;
                    default:
                      throw;
                  }
                }
              }
              else
              {
                try
                {
                  CreateDashboardStagingPriority2();
                }
                catch(Exception e)
                {
                  switch(GetErrorCode(e))
                  {
                    case ErrorCode.AlreadyExists:
                      ExitState = "DASHBOARD_STAGING_PRI_1_2_AE";

                      break;
                    case ErrorCode.PermittedValueViolation:
                      ExitState = "DASHBOARD_STAGING_PRI_1_2_PV";

                      break;
                    default:
                      throw;
                  }
                }
              }
            }

            local.Local1.CheckIndex();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              local.EabFileHandling.Action = "WRITE";
              local.EabReportSend.RptDetail =
                "Error creating/updating Dashboard_Staging_Priority_1_2.";
              UseCabErrorReport();
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              return;
            }

            // -- Checkpoint Info
            // Positions   Value
            // ---------   
            // ------------------------------------
            //  001-080    General Checkpoint Info for PRAD
            //  081-088    Dashboard Priority
            //  089-089    Blank
            //  090-099    CSE Case Number
            local.ProgramCheckpointRestart.RestartInd = "Y";
            local.ProgramCheckpointRestart.RestartInfo =
              Substring(import.ProgramCheckpointRestart.RestartInfo, 250, 1, 80) +
              "1-03    " + " " + String
              (local.Prev.Number, Case1.Number_MaxLength);
            UseUpdateCheckpointRstAndCommit();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              local.EabFileHandling.Action = "WRITE";
              local.EabReportSend.RptDetail = "Error taking checkpoint.";
              UseCabErrorReport();
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              return;
            }

            local.RecordsReadSinceCommit.Count = 0;
          }
        }

        local.Prev.Number = entities.Case1.Number;
        ++local.RecordsReadSinceCommit.Count;

        // -- Re-initialize Judicial District and Office
        // -- (these attributes are used to save data between the denominator 
        // and numerator processing)
        local.Initialized.JudicialDistrict = "";
        local.Initialized.Office = 0;
        MoveDashboardAuditData1(local.Initialized, local.DashboardAuditData);
        local.CountCase.Flag = "N";

        // -------------------------------------------------------------------------------------
        // --  D E N O M I N A T O R  (Number of Cases with Arrears Due) (
        // OCSE157 Line 28)
        // -------------------------------------------------------------------------------------
        // ----------------------------------------------------------------------
        // Read all valid AP/CH and AP/AR combos - active or not.
        // READ EACH property is set to fetch 'distinct' rows to avoid
        // spinning through same AP/CH or AP/AR combo multiple times.
        // Date checks are to ensure we retrieve overlapping roles only.
        // -----------------------------------------------------------------------
        foreach(var _1 in ReadCaseRoleCsePersonCaseRoleCsePerson())
        {
          // -------------------------------------------------------------------
          // Step #1. - Read debts where bal is due on 'run date'. There
          // is a good chance that balance was also due on 9/30. Count
          // case if bal is due on 9/30.
          // -------------------------------------------------------------------
          foreach(var _2 in ReadDebtObligationObligationTypeDebtDetail1())
          {
            // -------------------------------------------------------------------
            // -Skip 718B
            // -------------------------------------------------------------------
            if (Equal(entities.ObligationType.Code, "718B"))
            {
              continue;
            }

            // -------------------------------------------------------------------
            // -Skip MJ AF, MJ FC, MJ AFI, MJ FCI.
            // -------------------------------------------------------------------
            if (Equal(entities.ObligationType.Code, "MJ"))
            {
              UseFnDeterminePgmForDebtDetail1();

              if (Equal(local.Program.Code, "AF") || Equal
                (local.Program.Code, "AFI") || Equal
                (local.Program.Code, "FC") || Equal(local.Program.Code, "FCI"))
              {
                // -----------------------------------------------
                // Skip this debt detail.
                // -----------------------------------------------
                continue;
              }
            }

            // ---------------------------------------------------------------
            // You are now looking at a debt with balance due on run date
            // and it meets ob type criteria.
            // We now need to 'undo' debt adjustments, collections and
            // collection adjustments to obtain balance due as of FY end.
            // ---------------------------------------------------------------
            local.BalDueOnFyEnd.Currency152 = entities.DebtDetail.BalanceDueAmt;

            // -----------------------------------------------
            // Process Debt Adjustments first.
            // -----------------------------------------------
            foreach(var _3 in ReadDebtAdjustment1())
            {
              // ------------------------------------------------------------------
              // I-type adj increases balance_due. So we need to 'subtract'
              // this amt from balance at run time to get balance due on 9/30.
              // Similarily 'add' D-type adjustments back.
              // ------------------------------------------------------------------
              if (AsChar(entities.DebtAdjustment.DebtAdjustmentType) == 'I')
              {
                local.BalDueOnFyEnd.Currency152 -= entities.DebtAdjustment.
                  Amount;
              }
              else
              {
                local.BalDueOnFyEnd.Currency152 += entities.DebtAdjustment.
                  Amount;
              }
            }

            // -------------------------------------------------------------
            // Next - process collections and collection adjustments.
            // Read un-adj collections created after FY end.
            // Read adj collections created during or before FY,
            // but adjusted 'after' FY end.
            // Ok to read concurrent collection.
            // --------------------------------------------------------------
            foreach(var _3 in ReadCollection1())
            {
              // --------------------------------------------------------------
              // Subtract un-adj collections. Add adjusted collections.
              // --------------------------------------------------------------
              if (AsChar(entities.Collection.AdjustedInd) == 'N')
              {
                local.BalDueOnFyEnd.Currency152 += entities.Collection.Amount;
              }
              else
              {
                local.BalDueOnFyEnd.Currency152 -= entities.Collection.Amount;
              }
            }

            // --------------------------------------------------
            // Count case if balance > 0  on FY end.
            // --------------------------------------------------
            if (local.BalDueOnFyEnd.Currency152 > 0)
            {
              local.CountCase.Flag = "Y";

              // -- Set Dashboard audit data values.
              local.DashboardAuditData.DashboardPriority = "1-3(D)#1";
              local.DashboardAuditData.CaseNumber = entities.Case1.Number;
              local.DashboardAuditData.DebtType = entities.ObligationType.Code;
              local.DashboardAuditData.DebtDueDate = entities.DebtDetail.DueDt;
              local.DashboardAuditData.DebtBalanceDue =
                entities.DebtDetail.BalanceDueAmt;
              local.DashboardAuditData.SuppCspNumber =
                entities.ChOrArCsePerson.Number;
              local.DashboardAuditData.PayorCspNumber =
                entities.ApCsePerson.Number;

              goto ReadEach6;
            }
          }

          // -------------------------------------------------------------------
          // We got here because there is either no debt with balance
          // due as of run date or balance is due on run date but
          // nothing was due at the end of FY.
          // -------------------------------------------------------------------
          // -------------------------------------------------------------------
          // Step #2. - Check if Arrears Collection was created 'during' FY.
          // -Skip direct payments through REIP (CRT = 2 or 7)
          // -Include concurrent collections.
          // -Skip collections created and adjusted during FY.
          // -------------------------------------------------------------------
          foreach(var _2 in ReadCollectionObligationTypeCollection())
          {
            if (!entities.ObligationType.Populated)
            {
              continue;
            }

            // --------------------------
            // Skip Fees, Recoveries, 718B.
            // --------------------------
            if (AsChar(entities.ObligationType.Classification) == 'F' || AsChar
              (entities.ObligationType.Classification) == 'R' || Equal
              (entities.ObligationType.Code, "718B"))
            {
              continue;
            }

            // -----------------------------------------
            // Skip MJ AF, MJ AFI, MJ FC, MJ FCI.
            // -----------------------------------------
            if (Equal(entities.ObligationType.Code, "MJ") && (
              Equal(entities.Collection.ProgramAppliedTo, "AF") || Equal
              (entities.Collection.ProgramAppliedTo, "AFI") || Equal
              (entities.Collection.ProgramAppliedTo, "FC") || Equal
              (entities.Collection.ProgramAppliedTo, "FCI")))
            {
              continue;
            }

            // -------------------------------------------------------------------------
            // 09/14/2010  CQ21451
            // If the only activity on a case is an adjustment in the current FY
            // on a collection created in a previous FY, exclude the case.
            // --------------------------------------------------------------------------
            if (entities.Adjusted.Populated)
            {
              continue;
            }

            // -----------------------------------------------------------
            // Yipeee! We found an Arrears collection created during FY.
            // -----------------------------------------------------------
            local.CountCase.Flag = "Y";

            // -- Set Dashboard audit data values.
            local.DashboardAuditData.DashboardPriority = "1-3(D)#2";
            local.DashboardAuditData.CaseNumber = entities.Case1.Number;
            local.DashboardAuditData.DebtType = entities.ObligationType.Code;
            local.DashboardAuditData.SuppCspNumber =
              entities.ChOrArCsePerson.Number;
            local.DashboardAuditData.PayorCspNumber =
              entities.ApCsePerson.Number;
            local.DashboardAuditData.CollAppliedToCd =
              entities.Collection.AppliedToCode;
            local.DashboardAuditData.CollectionAmount =
              entities.Collection.Amount;
            local.DashboardAuditData.CollectionCreatedDate =
              Date(entities.Collection.CreatedTmst);

            goto ReadEach6;
          }

          // -------------------------------------------------------------------
          // We got here because there is no debt with balance due as of
          // run date and no Arrears Collection is created 'during' FY.
          // -------------------------------------------------------------------
          // -------------------------------------------------------------------
          // Step # 3. - Check for D-type adjustments created during FY.
          // -------------------------------------------------------------------
          foreach(var _2 in ReadDebtObligationObligationTypeDebtDetailDebtAdjustment())
          {
            // -----------------------------------------------------------------
            // For Accruing debts, include if adj occurs atleast 1 month after
            // due date. (Remember - accruing debts are not considered
            // 'arrears' until 1 month after due date)
            // For Non-accruing debts, include all D-type adjustments.
            // ----------------------------------------------------------------
            if (AsChar(entities.ObligationType.Classification) == 'A' && !
              Lt(AddMonths(entities.DebtDetail.DueDt, 1),
              entities.DebtAdjustment.DebtAdjustmentDt))
            {
              continue;
            }

            // -------------------------------------------------------------------
            // -Skip 718B
            // -------------------------------------------------------------------
            if (Equal(entities.ObligationType.Code, "718B"))
            {
              continue;
            }

            // -------------------------------------------------------------------
            // -Skip MJ AF, MJ FC, MJ AFI, MJ FCI.
            // -------------------------------------------------------------------
            if (Equal(entities.ObligationType.Code, "MJ"))
            {
              UseFnDeterminePgmForDebtDetail1();

              if (Equal(local.Program.Code, "AF") || Equal
                (local.Program.Code, "AFI") || Equal
                (local.Program.Code, "FC") || Equal(local.Program.Code, "FCI"))
              {
                // -----------------------------------------------
                // Skip this debt detail.
                // -----------------------------------------------
                continue;
              }
            }

            // -------------------------------------------------------------------
            // Yipee! D-type adj found, count case.
            // -------------------------------------------------------------------
            local.CountCase.Flag = "Y";

            // -- Set Dashboard audit data values.
            local.DashboardAuditData.DashboardPriority = "1-3(D)#3";
            local.DashboardAuditData.CaseNumber = entities.Case1.Number;
            local.DashboardAuditData.DebtType = entities.ObligationType.Code;
            local.DashboardAuditData.DebtDueDate = entities.DebtDetail.DueDt;
            local.DashboardAuditData.DebtBalanceDue =
              entities.DebtDetail.BalanceDueAmt;
            local.DashboardAuditData.SuppCspNumber =
              entities.ChOrArCsePerson.Number;
            local.DashboardAuditData.PayorCspNumber =
              entities.ApCsePerson.Number;

            goto ReadEach6;
          }

          // -------------------------------------------------------------------
          // We got here because
          // No balance is due as of run date  and
          // No arrears collection was created during FY  and
          // No D-type adj is done during FY.
          // Debts could have an outstanding balance as of FY end but
          // zero balance is due on run date. This could happen if
          // 1. Collection is applied after FY end  or
          // 2. D-type debt adj is done after FY end.
          // -------------------------------------------------------------------
          // -------------------------------------------------------------------
          // Step # 4. - Look for debts with 'Zero' bal due but where a
          // Coll is applied to debt after FY end.
          // READ EACH properties are set to fetch distinct rows - so we
          // process each debt only once
          // -------------------------------------------------------------------
          foreach(var _2 in ReadDebtObligationObligationTypeDebtDetail2())
          {
            // -------------------------------------------------------------------
            // -Skip 718B
            // -------------------------------------------------------------------
            if (Equal(entities.ObligationType.Code, "718B"))
            {
              continue;
            }

            // -------------------------------------------------------------------
            // -Skip MJ AF, MJ FC, MJ AFI, MJ FCI.
            // -------------------------------------------------------------------
            if (Equal(entities.ObligationType.Code, "MJ"))
            {
              UseFnDeterminePgmForDebtDetail1();

              if (Equal(local.Program.Code, "AF") || Equal
                (local.Program.Code, "AFI") || Equal
                (local.Program.Code, "FC") || Equal(local.Program.Code, "FCI"))
              {
                // -----------------------------------------------
                // Skip this debt detail.
                // -----------------------------------------------
                continue;
              }
            }

            // --------------------------------------------------------------
            // DD balance due is 0. In effect, this SET statement resets
            // local view for each parse.
            // --------------------------------------------------------------
            local.BalDueOnFyEnd.Currency152 = entities.DebtDetail.BalanceDueAmt;

            // ---------------------------------------------------------------
            // You are now looking at a debt with zero balance on run date
            // and it meets ob type criteria. We also know that there is
            // atleast one coll applied to current debt after FY end.
            // We now need to 'undo' debt adjustments, collections and
            // collection adjustments to obtain balance due as of FY end.
            // ---------------------------------------------------------------
            // -----------------------------------------------
            // Process Debt Adjustments first.
            // -----------------------------------------------
            foreach(var _3 in ReadDebtAdjustment1())
            {
              // ------------------------------------------------------------------
              // I-type adj increases balance_due. So we need to 'subtract'
              // this amt from balance at run time to get balance due on 9/30.
              // Similarily 'add' D-type adjustments back.
              // ------------------------------------------------------------------
              if (AsChar(entities.DebtAdjustment.DebtAdjustmentType) == 'I')
              {
                local.BalDueOnFyEnd.Currency152 -= entities.DebtAdjustment.
                  Amount;
              }
              else
              {
                local.BalDueOnFyEnd.Currency152 += entities.DebtAdjustment.
                  Amount;
              }
            }

            // -------------------------------------------------------------
            // Next, process collections and collection adjustments.
            // Read un-adjusted collections created after FY end.
            // Read adj collections created during or before FY,
            // but adjusted 'after' FY end.
            // Include concurrent collections.
            // -------------------------------------------------------------
            foreach(var _3 in ReadCollection1())
            {
              // -----------------------------------------------------------
              // Subtract collections. Add collection adjustments.
              // -----------------------------------------------------------
              if (AsChar(entities.Collection.AdjustedInd) == 'N')
              {
                local.BalDueOnFyEnd.Currency152 += entities.Collection.Amount;
              }
              else
              {
                local.BalDueOnFyEnd.Currency152 -= entities.Collection.Amount;
              }
            }

            // -----------------------------------------------
            // Count case if balance was due on 9/30
            // -----------------------------------------------
            if (local.BalDueOnFyEnd.Currency152 > 0)
            {
              local.CountCase.Flag = "Y";

              // -- Set Dashboard audit data values.
              local.DashboardAuditData.DashboardPriority = "1-3(D)#4";
              local.DashboardAuditData.CaseNumber = entities.Case1.Number;
              local.DashboardAuditData.DebtType = entities.ObligationType.Code;
              local.DashboardAuditData.DebtDueDate = entities.DebtDetail.DueDt;
              local.DashboardAuditData.DebtBalanceDue =
                entities.DebtDetail.BalanceDueAmt;
              local.DashboardAuditData.SuppCspNumber =
                entities.ChOrArCsePerson.Number;
              local.DashboardAuditData.PayorCspNumber =
                entities.ApCsePerson.Number;

              goto ReadEach6;
            }
          }

          // -------------------------------------------------------------------
          // Step # 5. - Look for debts with 'Zero' bal due but where a
          // D-type adjustment is made to debt after FY end.
          // READ EACH properties are set to fetch distinct rows - so we
          // process each debt only once
          // -------------------------------------------------------------------
          foreach(var _2 in ReadDebtObligationObligationTypeDebtDetail3())
          {
            // -------------------------------------------------------------------
            // -Skip 718B
            // -------------------------------------------------------------------
            if (Equal(entities.ObligationType.Code, "718B"))
            {
              continue;
            }

            // -------------------------------------------------------------------
            // -Skip MJ AF, MJ FC, MJ AFI, MJ FCI.
            // -------------------------------------------------------------------
            if (Equal(entities.ObligationType.Code, "MJ"))
            {
              UseFnDeterminePgmForDebtDetail1();

              if (Equal(local.Program.Code, "AF") || Equal
                (local.Program.Code, "AFI") || Equal
                (local.Program.Code, "FC") || Equal(local.Program.Code, "FCI"))
              {
                // -----------------------------------------------
                // Skip this debt detail.
                // -----------------------------------------------
                continue;
              }
            }

            // --------------------------------------------------------------
            // DD balance due is 0. In effect, this SET statement resets
            // local view for each parse.
            // --------------------------------------------------------------
            local.BalDueOnFyEnd.Currency152 = entities.DebtDetail.BalanceDueAmt;

            // ---------------------------------------------------------------
            // You are now looking at a debt with zero balance on run date
            // and it meets ob type criteria. We also know that there is
            // atleast one D-type adjustment to debt 'after' FY end.
            // We now need to 'undo' debt adjustments, collections and
            // collection adjustments to obtain balance due as of FY end.
            // ---------------------------------------------------------------
            // -----------------------------------------------
            // Process Debt Adjustments first.
            // -----------------------------------------------
            foreach(var _3 in ReadDebtAdjustment1())
            {
              // ------------------------------------------------------------------
              // I-type adj increases balance_due. So we need to 'subtract'
              // this amt from balance at run time to get balance due on 9/30.
              // Similarily 'add' D-type adjustments back.
              // ------------------------------------------------------------------
              if (AsChar(entities.DebtAdjustment.DebtAdjustmentType) == 'I')
              {
                local.BalDueOnFyEnd.Currency152 -= entities.DebtAdjustment.
                  Amount;
              }
              else
              {
                local.BalDueOnFyEnd.Currency152 += entities.DebtAdjustment.
                  Amount;
              }
            }

            // -------------------------------------------------------------
            // Next, process collections and collection adjustments.
            // Read un-adjusted collections created after FY end.
            // Read adj collections created during or before FY,
            // but adjusted 'after' FY end.
            // Include concurrent collections.
            // -------------------------------------------------------------
            foreach(var _3 in ReadCollection1())
            {
              // -----------------------------------------------------------
              // Subtract collections. Add collection adjustments.
              // -----------------------------------------------------------
              if (AsChar(entities.Collection.AdjustedInd) == 'N')
              {
                local.BalDueOnFyEnd.Currency152 += entities.Collection.Amount;
              }
              else
              {
                local.BalDueOnFyEnd.Currency152 -= entities.Collection.Amount;
              }
            }

            // -----------------------------------------------
            // Count case if balance was due on 9/30
            // -----------------------------------------------
            if (local.BalDueOnFyEnd.Currency152 > 0)
            {
              local.CountCase.Flag = "Y";

              // -- Set Dashboard audit data values.
              local.DashboardAuditData.DashboardPriority = "1-3(D)#5";
              local.DashboardAuditData.CaseNumber = entities.Case1.Number;
              local.DashboardAuditData.DebtType = entities.ObligationType.Code;
              local.DashboardAuditData.DebtDueDate = entities.DebtDetail.DueDt;
              local.DashboardAuditData.DebtBalanceDue =
                entities.DebtDetail.BalanceDueAmt;
              local.DashboardAuditData.SuppCspNumber =
                entities.ChOrArCsePerson.Number;
              local.DashboardAuditData.PayorCspNumber =
                entities.ApCsePerson.Number;

              goto ReadEach6;
            }
          }

          // -----------------------------------------------
          // End of AP/CH READ EACH.
          // -----------------------------------------------
        }

ReadEach6:

        if (AsChar(local.CountCase.Flag) == 'N')
        {
          // -- Case does not owe arrears.  Skip this case.
          continue;
        }

        // -------------------------------------------------------------------------------------
        // -- Case owes arrears.
        // -- Include case in the Priority 1-3 denominator (Number of Cases with
        // Arrears Due).
        // -- This is the same as OCSE157 Line 28.
        // -------------------------------------------------------------------------------------
        // -- Increment Statewide Level
        local.Statewide.CasesPayingArrearsDenominator =
          (local.Statewide.CasesPayingArrearsDenominator ?? 0) + 1;

        // -- Determine Judicial District...
        if (!Lt(import.ReportEndDate.Date,
          entities.CaseAssignment.DiscontinueDate))
        {
          // -- Pass the case assignment end date to the Determine JD cab so it
          // -- will find the JD the case belonged to on the closure date.
          local.TempEndDate.Date = entities.CaseAssignment.DiscontinueDate;
        }
        else
        {
          local.TempEndDate.Date = import.ReportEndDate.Date;
        }

        UseFnB734DetermineJdFromCase();

        // -- Save office and judicial district for the numerator processing.
        local.Initialized.JudicialDistrict =
          local.DashboardAuditData.JudicialDistrict;
        local.Initialized.Office = local.DashboardAuditData.Office ?? 0;

        // -- Increment Judicial District Level
        if (!IsEmpty(local.DashboardAuditData.JudicialDistrict))
        {
          local.Local1.Index =
            (int)StringToNumber(local.DashboardAuditData.JudicialDistrict) - 1;
          local.Local1.CheckSize();

          local.Local1.Update.G.CasesPayingArrearsDenominator =
            (local.Local1.Item.G.CasesPayingArrearsDenominator ?? 0) + 1;
        }

        if (AsChar(import.AuditFlag.Flag) == 'Y')
        {
          // -- Log to the dashboard audit table.
          UseFnB734CreateDashboardAudit2();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            return;
          }
        }

        local.CountCaseWk.Flag = "";

        if (ReadCaseAssignmentServiceProvider())
        {
          local.CountCaseWk.Flag = "Y";
        }

        local.DashboardStagingPriority35.Assign(
          local.NullDashboardStagingPriority35);

        if (AsChar(local.CountCaseWk.Flag) == 'Y')
        {
          local.Worker.Assign(local.DashboardAuditData);
          local.DashboardStagingPriority35.AsOfDate =
            import.ProgramProcessingInfo.ProcessDate;
          local.DashboardStagingPriority35.ReportLevel = "CW";
          local.DashboardStagingPriority35.ReportLevelId =
            entities.WorkerServiceProvider.UserId;
          local.DashboardStagingPriority35.ReportMonth =
            import.DashboardAuditData.ReportMonth;
          local.DashboardStagingPriority35.CasesPayingArrearsDenominator =
            (local.DashboardStagingPriority35.CasesPayingArrearsDenominator ?? 0)
            + 1;

          switch(TrimEnd(local.DashboardAuditData.DashboardPriority))
          {
            case "1-3(D)#1":
              local.Worker.DashboardPriority = "1-3.1D1";

              break;
            case "1-3(D)#2":
              local.Worker.DashboardPriority = "1-3.1D2";

              break;
            case "1-3(D)#3":
              local.Worker.DashboardPriority = "1-3.1D3";

              break;
            case "1-3(D)#4":
              local.Worker.DashboardPriority = "1-3.1D4";

              break;
            case "1-3(D)#5":
              local.Worker.DashboardPriority = "1-3.1D5";

              break;
            default:
              local.Worker.DashboardPriority = "1-3.1";

              break;
          }

          local.Worker.DebtBalanceDue =
            local.DashboardAuditData.DebtBalanceDue ?? 0M;
          local.Worker.CollectionAmount =
            local.DashboardAuditData.CollectionAmount ?? 0M;
          local.Worker.CaseNumber = entities.Case1.Number;
          local.Worker.CaseDate = entities.Case1.StatusDate;
          local.Worker.WorkerId =
            local.DashboardStagingPriority35.ReportLevelId;

          if (AsChar(import.AuditFlag.Flag) == 'Y')
          {
            // -- Log to the dashboard audit table.
            UseFnB734CreateDashboardAudit1();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }
          }

          if (ReadDashboardStagingPriority8())
          {
            try
            {
              UpdateDashboardStagingPriority4();
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  ExitState = "DASHBOARD_STAGING_PRI_3_5_NU";

                  break;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "DASHBOARD_STAGING_PRI_3_5_PV";

                  break;
                default:
                  throw;
              }
            }
          }
          else
          {
            try
            {
              CreateDashboardStagingPriority3();
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  ExitState = "DASHBOARD_STAGING_PRI_3_5_AE";

                  break;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "DASHBOARD_STAGING_PRI_3_5_PV";

                  break;
                default:
                  throw;
              }
            }
          }

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail =
              "Error creating/updating Dashboard_Staging_Priority_1_2.";
            UseCabErrorReport();
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }
        }

        local.DashboardStagingPriority35.Assign(
          local.NullDashboardStagingPriority35);
        local.CountCaseAtty.Flag = "";

        foreach(var _1 in ReadLegalReferralServiceProvider())
        {
          local.Worker.Assign(local.DashboardAuditData);
          local.Worker.LegalReferralNumber = entities.LegalReferral.Identifier;
          local.Worker.LegalReferralDate = entities.LegalReferral.ReferralDate;
          local.CountCaseAtty.Flag = "Y";

          if (AsChar(local.CountCaseAtty.Flag) == 'Y')
          {
            // -- Case does not owe arrears.  Skip this case.
            local.DashboardStagingPriority35.AsOfDate =
              import.ProgramProcessingInfo.ProcessDate;

            if (Equal(entities.ServiceProvider.RoleCode, "AT") || Equal
              (entities.ServiceProvider.RoleCode, "CT"))
            {
              local.DashboardStagingPriority35.ReportLevel = "AT";
            }
            else
            {
              local.DashboardStagingPriority35.ReportLevel = "CA";
            }

            local.DashboardStagingPriority35.ReportLevelId =
              entities.ServiceProvider.UserId;
            local.DashboardStagingPriority35.ReportMonth =
              import.DashboardAuditData.ReportMonth;
            local.DashboardStagingPriority35.CasesPayingArrearsDenominator = 1;
            local.Worker.CaseNumber = entities.Case1.Number;
            local.Worker.CaseDate = entities.Case1.StatusDate;
            local.Worker.WorkerId =
              local.DashboardStagingPriority35.ReportLevelId;

            switch(TrimEnd(local.DashboardAuditData.DashboardPriority))
            {
              case "1-3(D)#1":
                local.Worker.DashboardPriority = "1-3.2D1";

                break;
              case "1-3(D)#2":
                local.Worker.DashboardPriority = "1-3.2D2";

                break;
              case "1-3(D)#3":
                local.Worker.DashboardPriority = "1-3.2D3";

                break;
              case "1-3(D)#4":
                local.Worker.DashboardPriority = "1-3.2D4";

                break;
              case "1-3(D)#5":
                local.Worker.DashboardPriority = "1-3.2D5";

                break;
              default:
                local.Worker.DashboardPriority = "1-3.2";

                break;
            }

            if (AsChar(import.AuditFlag.Flag) == 'Y')
            {
              // -- Log to the dashboard audit table.
              UseFnB734CreateDashboardAudit1();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                return;
              }
            }

            if (ReadDashboardStagingPriority8())
            {
              try
              {
                UpdateDashboardStagingPriority4();
              }
              catch(Exception e)
              {
                switch(GetErrorCode(e))
                {
                  case ErrorCode.AlreadyExists:
                    ExitState = "DASHBOARD_STAGING_PRI_3_5_NU";

                    break;
                  case ErrorCode.PermittedValueViolation:
                    ExitState = "DASHBOARD_STAGING_PRI_3_5_PV";

                    break;
                  default:
                    throw;
                }
              }
            }
            else
            {
              try
              {
                CreateDashboardStagingPriority3();
              }
              catch(Exception e)
              {
                switch(GetErrorCode(e))
                {
                  case ErrorCode.AlreadyExists:
                    ExitState = "DASHBOARD_STAGING_PRI_3_5_AE";

                    break;
                  case ErrorCode.PermittedValueViolation:
                    ExitState = "DASHBOARD_STAGING_PRI_3_5_PV";

                    break;
                  default:
                    throw;
                }
              }
            }

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              local.EabFileHandling.Action = "WRITE";
              local.EabReportSend.RptDetail =
                "Error creating/updating Dashboard_Staging_Priority_1_2.";
              UseCabErrorReport();
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              return;
            }

            break;
          }
        }

        // -------------------------------------------------------------------------------------
        // -- N U M E R A T O R (Number of Cases Paying on Arrears) (OCSE157 
        // Line 29)
        // -------------------------------------------------------------------------------------
        // ----------------------------------------------------------------------
        // Read all APs on Case - active or not.
        // READ EACH property is set to fetch 'distinct' rows to avoid
        // spinning through same person multiple times.
        // -----------------------------------------------------------------------
        // -- The local_initialized view will have the Judicial District and 
        // Office derived during denominator processing.
        MoveDashboardAuditData1(local.Initialized, local.DashboardAuditData);
        local.CountCase.Flag = "N";

        foreach(var _1 in ReadCsePersonCollectionCsePersonObligationType2())
        {
          // -------------------------------------------------------------------
          // Step # 1 - Look for NA or NAI Collection for current AP
          // -Created 'during' FY.
          // -Applied to Arrears
          // -Skip direct payments through REIP (CRT = 2 or 7)
          // -Skip concurrent collections.(9/4/2001)
          // -Skip collections created and adjusted during FY.
          // -------------------------------------------------------------------
          if (entities.Collection.Populated)
          {
            if (!entities.Supp.Populated)
            {
              continue;
            }

            if (!entities.ObligationType.Populated)
            {
              continue;
            }

            // ---------------------------------
            // Skip Fees, Recoveries, 718B.
            // ---------------------------------
            if (AsChar(entities.ObligationType.Classification) == 'F' || AsChar
              (entities.ObligationType.Classification) == 'R' || Equal
              (entities.ObligationType.Code, "718B"))
            {
              continue;
            }

            // -----------------------------------------
            // Skip MJ AF, MJ AFI, MJ FC, MJ FCI.
            // -----------------------------------------
            if (Equal(entities.ObligationType.Code, "MJ") && (
              Equal(entities.Collection.ProgramAppliedTo, "AF") || Equal
              (entities.Collection.ProgramAppliedTo, "AFI") || Equal
              (entities.Collection.ProgramAppliedTo, "FC") || Equal
              (entities.Collection.ProgramAppliedTo, "FCI")))
            {
              continue;
            }

            // -----------------------------------------------------------
            // Ok, we found a desired Arrears collection. But was this
            // collection applied to a CH/AR on this case?
            // -----------------------------------------------------------
            if (!entities.ChOrArCaseRole.Populated)
            {
              continue;
            }

            // ------------------------------------------------------------------
            // 07/11/2008  CQ2461
            // If the only activity on the Case is an adjustment in the current 
            // FY
            // on a Collection created in a previous FY, exclude the case.
            // ------------------------------------------------------------------
            if (entities.Adjusted.Populated)
            {
              continue;
            }
            else
            {
              // -- Continue...
            }

            // -----------------------------------------------------------
            // Yipeee! We found an NA or NAI collection - Count case.
            // -----------------------------------------------------------
            local.CountCase.Flag = "Y";
            local.DashboardAuditData.DashboardPriority = "1-3(N)#1";
            local.DashboardAuditData.CaseNumber = entities.Case1.Number;
            local.DashboardAuditData.CollectionAmount =
              entities.Collection.Amount;
            local.DashboardAuditData.CollAppliedToCd =
              entities.Collection.AppliedToCode;
            local.DashboardAuditData.CollectionCreatedDate =
              Date(entities.Collection.CreatedTmst);
            local.DashboardAuditData.DebtType = entities.ObligationType.Code;
            local.DashboardAuditData.SuppCspNumber = entities.Supp.Number;
            local.DashboardAuditData.PayorCspNumber =
              entities.ApCsePerson.Number;

            goto ReadEach7;
          }
        }

ReadEach7:

        if (AsChar(local.CountCase.Flag) == 'N')
        {
          // -------------------------------------------------------------------
          // We got here because no NA or NAI arrears coll was received.
          // Now check if we received any non-NA arrears coll during FY.
          // -------------------------------------------------------------------
          foreach(var _1 in ReadCsePerson())
          {
            // -------------------------------------------------------------------
            // Step #2 - Look for non-NA, non-NAI Collection for current AP
            // -Created 'during' FY.
            // -Applied to Arrears
            // -Skip direct payments through REIP (CRT = 2 or 7)
            // -Skip concurrent collections. (9/4/2001)
            // -Skip collections created and adjusted during FY.
            // -------------------------------------------------------------------
            local.CollFound.Flag = "N";

            foreach(var _2 in ReadCollectionCsePersonObligationTypeCaseRoleCollection2())
            {
              if (!entities.Supp.Populated)
              {
                continue;
              }

              if (!entities.ObligationType.Populated)
              {
                continue;
              }

              // ---------------------------------
              // Skip Fees, Recoveries, 718B.
              // ---------------------------------
              if (AsChar(entities.ObligationType.Classification) == 'F' || AsChar
                (entities.ObligationType.Classification) == 'R' || Equal
                (entities.ObligationType.Code, "718B"))
              {
                continue;
              }

              // -----------------------------------------
              // Skip MJ AF, MJ AFI, MJ FC, MJ FCI.
              // -----------------------------------------
              if (Equal(entities.ObligationType.Code, "MJ") && (
                Equal(entities.Collection.ProgramAppliedTo, "AF") || Equal
                (entities.Collection.ProgramAppliedTo, "AFI") || Equal
                (entities.Collection.ProgramAppliedTo, "FC") || Equal
                (entities.Collection.ProgramAppliedTo, "FCI")))
              {
                continue;
              }

              // -----------------------------------------------------------
              // Ok, we found a desired Arrears collection for AP. But does
              // the Supp Person participate on this case?
              // -----------------------------------------------------------
              if (!entities.ChOrArCaseRole.Populated)
              {
                continue;
              }

              // ------------------------------------------------------------------
              // 07/11/2008  CQ2461
              // If the only activity on the Case is an adjustment in the 
              // current FY
              // on a Collection created in a previous FY, exclude the case.
              // ------------------------------------------------------------------
              if (entities.Adjusted.Populated)
              {
                continue;
              }
              else
              {
                // -- Continue...
              }

              local.CollFound.Flag = "Y";

              // -----------------------------------------------------------
              // Save these views to create verification record later.
              // -----------------------------------------------------------
              local.Ap.Number = entities.ApCsePerson.Number;
              local.Supp.Number = entities.Supp.Number;
              MoveCollection(entities.Collection, local.NonNa);

              goto ReadEach8;
            }

            foreach(var _2 in ReadCollectionCsePersonObligationTypeCaseRoleCollection3())
            {
              if (!entities.Supp.Populated)
              {
                continue;
              }

              if (!entities.ObligationType.Populated)
              {
                continue;
              }

              // ---------------------------------
              // Skip Fees, Recoveries, 718B.
              // ---------------------------------
              if (AsChar(entities.ObligationType.Classification) == 'F' || AsChar
                (entities.ObligationType.Classification) == 'R' || Equal
                (entities.ObligationType.Code, "718B"))
              {
                continue;
              }

              // -----------------------------------------
              // Skip MJ AF, MJ AFI, MJ FC, MJ FCI.
              // -----------------------------------------
              if (Equal(entities.ObligationType.Code, "MJ") && (
                Equal(entities.Collection.ProgramAppliedTo, "AF") || Equal
                (entities.Collection.ProgramAppliedTo, "AFI") || Equal
                (entities.Collection.ProgramAppliedTo, "FC") || Equal
                (entities.Collection.ProgramAppliedTo, "FCI")))
              {
                continue;
              }

              // -----------------------------------------------------------
              // Ok, we found a desired Arrears collection for AP. But does
              // the Supp Person participate on this case?
              // -----------------------------------------------------------
              if (!entities.ChOrArCaseRole.Populated)
              {
                continue;
              }

              // ------------------------------------------------------------------
              // 07/11/2008  CQ2461
              // If the only activity on the Case is an adjustment in the 
              // current FY
              // on a Collection created in a previous FY, exclude the case.
              // ------------------------------------------------------------------
              if (entities.Adjusted.Populated)
              {
                continue;
              }
              else
              {
                // -- Continue...
              }

              local.CollFound.Flag = "Y";

              // -----------------------------------------------------------
              // Save these views to create verification record later.
              // -----------------------------------------------------------
              local.Ap.Number = entities.ApCsePerson.Number;
              local.Supp.Number = entities.Supp.Number;
              MoveCollection(entities.Collection, local.NonNa);

              goto ReadEach8;
            }
          }

ReadEach8:

          if (AsChar(local.CollFound.Flag) == 'N')
          {
            // -------------------------------------------------------------
            // No Arrears coll was received during FY. Skip Case.
            // -------------------------------------------------------------
            continue;
          }

          // -------------------------------------------------------------------
          // We got here because no NA or NAI arrears coll was received
          // but a collection was definitely received.
          // -------------------------------------------------------------------
          // -------------------------------------------------------------------
          // Step # 3. - Look for FDSO collection (coll type = 3 or 27)
          // -------------------------------------------------------------------
          foreach(var _1 in ReadCsePerson())
          {
            local.CollectionDate.Date = local.NullDateWorkArea.Date;

            foreach(var _2 in ReadCollectionCollection2())
            {
              if (Equal(entities.Collection.CollectionDt,
                local.CollectionDate.Date))
              {
                continue;
              }

              local.CollectionDate.Date = entities.Collection.CollectionDt;

              // -------------------------------------------------------------
              // Check to see if all supp persons have AF, AFI, FC, FCI active
              // as of collection date?
              // -------------------------------------------------------------
              UseFnCheckForActiveAfFcPgm();

              // ----------------------------------------------------------
              // If all supp persons on Case are on assistance as of coll date,
              // we would never derive NA on any debt. Skip Collection.
              // ----------------------------------------------------------
              if (AsChar(local.Assistance.Flag) == 'Y')
              {
                continue;
              }

              // ------------------------------------------------------------------
              // 07/11/2008  CQ2461
              // If the only activity on the Case is an adjustment in the 
              // current FY
              // on a Collection created in a previous FY, exclude the case.
              // ------------------------------------------------------------------
              if (entities.Adjusted.Populated)
              {
                continue;
              }
              else
              {
                // -- Continue...
              }

              // ----------------------------------------------------------
              // Atleast one person on case is not on assistance as of coll
              // date. Spin through each debt with due date <= coll date to
              // check if NA or NAI program is determined.
              // ----------------------------------------------------------
              // ------------------------------------------------------------
              // We will only look for debts with bal due as of FY end.
              // First, read debts with bal due as of today.
              // Later, we'll read debts with zero bal today but bal was due
              // on FY end.
              // -------------------------------------------------------------
              foreach(var _3 in ReadDebtDetailObligationObligationTypeCsePerson1())
              {
                UseFnDeterminePgmForDebtDetail2();

                if (Equal(local.Program.Code, "NA") || Equal
                  (local.Program.Code, "NAI"))
                {
                  // -----------------------------------------------------------
                  // Ok, we found a debt for current AP that derives NA. Does 
                  // the
                  // supp person participate on current case?
                  // -----------------------------------------------------------
                  if (!ReadCaseRole())
                  {
                    // -----------------------------------------------------------
                    // AP must be on multiple cases and we probably hit a debt 
                    // for
                    // a kid on another case
                    // -----------------------------------------------------------
                    continue;
                  }

                  // ----------------------------------------------------------------
                  // NA or NAI was owed and we know that no arrears collection
                  // was applied to NA. Skip Case.
                  // ----------------------------------------------------------------
                  // @@@
                  goto ReadEach9;
                }
              }

              // -----------------------------------------------------------------------
              // We either didn't find any debt with bal due or we found a debt
              // but no NA or NAI was owed on Collection date.
              // Now read for debts with zero bal today but where bal was
              // due on FY end.
              // -----------------------------------------------------------------------
              // -----------------------------------------------------------------------
              // Only do this if there is any collection/debt activity
              // for AP after FY end. There is no point in spinning through all
              // Zero debts if there is no activity at all for this AP.
              // -----------------------------------------------------------------------
              if (!ReadCollection2())
              {
                if (!ReadDebtAdjustment2())
                {
                  // -------------------------------------------------------------------------
                  // No collection/debt activity for AP after FY end.
                  // Process next FDSO Collection.
                  // --------------------------------------------------------------------------
                  continue;
                }
              }

              // -----------------------------------------------------------------------
              // We got here because there was some activity for AP since
              // FY end.
              // Now read for debts with zero bal today. Then determine if bal
              // was due on FY end.
              // -----------------------------------------------------------------------
              foreach(var _3 in ReadDebtDetailObligationObligationTypeCsePerson2())
              {
                // -----------------------------------------------------------------------
                // Check if debt has had any activity after FY end.
                // -----------------------------------------------------------------------
                if (!entities.AfterFy.Populated)
                {
                  if (!ReadDebtAdjustment3())
                  {
                    // -------------------------------------------------------------------------
                    // No activity on debt since FY end. Process next debt.
                    // --------------------------------------------------------------------------
                    continue;
                  }
                }

                // -------------------------------------------------------------------------
                // You are looking at a debt with zero bal today but balance
                // was due on FY end.
                // --------------------------------------------------------------------------
                UseFnDeterminePgmForDebtDetail2();

                if (Equal(local.Program.Code, "NA") || Equal
                  (local.Program.Code, "NAI"))
                {
                  // -----------------------------------------------------------
                  // Ok, we found a debt for current AP that derives NA. Does 
                  // the
                  // supp person participate on current case?
                  // -----------------------------------------------------------
                  if (!ReadCaseRole())
                  {
                    // -----------------------------------------------------------
                    // AP must be on multiple cases and we probably hit a debt 
                    // for
                    // a kid on another case
                    // -----------------------------------------------------------
                    continue;
                  }

                  // ----------------------------------------------------------------
                  // NA or NAI was owed and we know that no arrears collection
                  // was applied to NA. Skip Case.
                  // ----------------------------------------------------------------
                  // @@@
                  goto ReadEach9;
                }
              }

              // ----------------------------------------------------------
              // We got here because we didn't derive NA or NAI on any debt
              // for current FDSO collection date. Process next FDSO coll.
              // ----------------------------------------------------------
            }

            local.CollectionDate.Date = local.NullDateWorkArea.Date;

            foreach(var _2 in ReadCollectionCollection3())
            {
              if (Equal(entities.Collection.CollectionDt,
                local.CollectionDate.Date))
              {
                continue;
              }

              local.CollectionDate.Date = entities.Collection.CollectionDt;

              // -------------------------------------------------------------
              // Check to see if all supp persons have AF, AFI, FC, FCI active
              // as of collection date?
              // -------------------------------------------------------------
              UseFnCheckForActiveAfFcPgm();

              // ----------------------------------------------------------
              // If all supp persons on Case are on assistance as of coll date,
              // we would never derive NA on any debt. Skip Collection.
              // ----------------------------------------------------------
              if (AsChar(local.Assistance.Flag) == 'Y')
              {
                continue;
              }

              // ------------------------------------------------------------------
              // 07/11/2008  CQ2461
              // If the only activity on the Case is an adjustment in the 
              // current FY
              // on a Collection created in a previous FY, exclude the case.
              // ------------------------------------------------------------------
              if (entities.Adjusted.Populated)
              {
                continue;
              }
              else
              {
                // -- Continue...
              }

              // ----------------------------------------------------------
              // Atleast one person on case is not on assistance as of coll
              // date. Spin through each debt with due date <= coll date to
              // check if NA or NAI program is determined.
              // ----------------------------------------------------------
              // ------------------------------------------------------------
              // We will only look for debts with bal due as of FY end.
              // First, read debts with bal due as of today.
              // Later, we'll read debts with zero bal today but bal was due
              // on FY end.
              // -------------------------------------------------------------
              foreach(var _3 in ReadDebtDetailObligationObligationTypeCsePerson1())
              {
                UseFnDeterminePgmForDebtDetail2();

                if (Equal(local.Program.Code, "NA") || Equal
                  (local.Program.Code, "NAI"))
                {
                  // -----------------------------------------------------------
                  // Ok, we found a debt for current AP that derives NA. Does 
                  // the
                  // supp person participate on current case?
                  // -----------------------------------------------------------
                  if (!ReadCaseRole())
                  {
                    // -----------------------------------------------------------
                    // AP must be on multiple cases and we probably hit a debt 
                    // for
                    // a kid on another case
                    // -----------------------------------------------------------
                    continue;
                  }

                  // ----------------------------------------------------------------
                  // NA or NAI was owed and we know that no arrears collection
                  // was applied to NA. Skip Case.
                  // ----------------------------------------------------------------
                  // @@@
                  goto ReadEach9;
                }
              }

              // -----------------------------------------------------------------------
              // We either didn't find any debt with bal due or we found a debt
              // but no NA or NAI was owed on Collection date.
              // Now read for debts with zero bal today but where bal was
              // due on FY end.
              // -----------------------------------------------------------------------
              // -----------------------------------------------------------------------
              // Only do this if there is any collection/debt activity
              // for AP after FY end. There is no point in spinning through all
              // Zero debts if there is no activity at all for this AP.
              // -----------------------------------------------------------------------
              if (!ReadCollection2())
              {
                if (!ReadDebtAdjustment2())
                {
                  // -------------------------------------------------------------------------
                  // No collection/debt activity for AP after FY end.
                  // Process next FDSO Collection.
                  // --------------------------------------------------------------------------
                  continue;
                }
              }

              // -----------------------------------------------------------------------
              // We got here because there was some activity for AP since
              // FY end.
              // Now read for debts with zero bal today. Then determine if bal
              // was due on FY end.
              // -----------------------------------------------------------------------
              foreach(var _3 in ReadDebtDetailObligationObligationTypeCsePerson2())
              {
                // -----------------------------------------------------------------------
                // Check if debt has had any activity after FY end.
                // -----------------------------------------------------------------------
                if (!entities.AfterFy.Populated)
                {
                  if (!ReadDebtAdjustment3())
                  {
                    // -------------------------------------------------------------------------
                    // No activity on debt since FY end. Process next debt.
                    // --------------------------------------------------------------------------
                    continue;
                  }
                }

                // -------------------------------------------------------------------------
                // You are looking at a debt with zero bal today but balance
                // was due on FY end.
                // --------------------------------------------------------------------------
                UseFnDeterminePgmForDebtDetail2();

                if (Equal(local.Program.Code, "NA") || Equal
                  (local.Program.Code, "NAI"))
                {
                  // -----------------------------------------------------------
                  // Ok, we found a debt for current AP that derives NA. Does 
                  // the
                  // supp person participate on current case?
                  // -----------------------------------------------------------
                  if (!ReadCaseRole())
                  {
                    // -----------------------------------------------------------
                    // AP must be on multiple cases and we probably hit a debt 
                    // for
                    // a kid on another case
                    // -----------------------------------------------------------
                    continue;
                  }

                  // ----------------------------------------------------------------
                  // NA or NAI was owed and we know that no arrears collection
                  // was applied to NA. Skip Case.
                  // ----------------------------------------------------------------
                  // @@@
                  goto ReadEach9;
                }
              }

              // ----------------------------------------------------------
              // We got here because we didn't derive NA or NAI on any debt
              // for current FDSO collection date. Process next FDSO coll.
              // ----------------------------------------------------------
            }
          }

          // ----------------------------------------------------------
          // We got here because we either didn't find any FDSO
          // collection or we found FDSO collections but none of the
          // debts derived NA or NAI.
          // We also know that non-NA arrears collection was definitely
          // received. No further processing is necessary. Count case!
          // ----------------------------------------------------------
          local.CountCase.Flag = "Y";
          local.DashboardAuditData.DashboardPriority = "1-3(N)#2";
          local.DashboardAuditData.CaseNumber = entities.Case1.Number;
          local.DashboardAuditData.CollectionAmount = local.NonNa.Amount;
          local.DashboardAuditData.CollAppliedToCd = local.NonNa.AppliedToCode;
          local.DashboardAuditData.CollectionCreatedDate =
            Date(local.NonNa.CreatedTmst);
          local.DashboardAuditData.SuppCspNumber = local.Supp.Number;
          local.DashboardAuditData.PayorCspNumber = local.Ap.Number;
        }

        if (AsChar(local.CountCase.Flag) == 'N')
        {
          // -- Case does not meet criteria.
          continue;
        }

        // -------------------------------------------------------------------------------------
        // -- Case paid on arrears.
        // -- Include case in the Priority 1-3 numerator (Number of Cases Paying
        // on Arrears).
        // -- This is the same as OCSE157 Line 29.
        // -------------------------------------------------------------------------------------
        // -- Increment Statewide Level
        local.Statewide.CasesPayingArrearsNumerator =
          (local.Statewide.CasesPayingArrearsNumerator ?? 0) + 1;

        // -- Judicial District and office were retrieved in the denominator 
        // processing.
        // -- Increment Judicial District Level
        if (!IsEmpty(local.DashboardAuditData.JudicialDistrict))
        {
          local.Local1.Index =
            (int)StringToNumber(local.DashboardAuditData.JudicialDistrict) - 1;
          local.Local1.CheckSize();

          local.Local1.Update.G.CasesPayingArrearsNumerator =
            (local.Local1.Item.G.CasesPayingArrearsNumerator ?? 0) + 1;
        }

        if (AsChar(import.AuditFlag.Flag) == 'Y')
        {
          // -- Log to the dashboard audit table.
          UseFnB734CreateDashboardAudit2();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            return;
          }
        }

        local.DashboardStagingPriority35.Assign(
          local.NullDashboardStagingPriority35);

        if (AsChar(local.CountCaseWk.Flag) == 'Y')
        {
          local.DashboardStagingPriority35.AsOfDate =
            import.ProgramProcessingInfo.ProcessDate;
          local.DashboardStagingPriority35.ReportLevel = "CW";
          local.DashboardStagingPriority35.ReportLevelId =
            entities.WorkerServiceProvider.UserId;
          local.DashboardStagingPriority35.ReportMonth =
            import.DashboardAuditData.ReportMonth;
          local.DashboardStagingPriority35.CasesPayingArrearsNumerator =
            (local.DashboardStagingPriority35.CasesPayingArrearsNumerator ?? 0) +
            1;
          local.Worker.Assign(local.DashboardAuditData);

          switch(TrimEnd(local.DashboardAuditData.DashboardPriority))
          {
            case "1-3(N)#1":
              local.Worker.DashboardPriority = "1-3.1N1";
              local.Worker.CollectionAmount =
                local.DashboardAuditData.CollectionAmount ?? 0M;

              break;
            case "1-3(N)#2":
              local.Worker.DashboardPriority = "1-3.1N2";
              local.Worker.CollectionAmount = local.NonNa.Amount;

              break;
            case "1-3(N)#3":
              local.Worker.DashboardPriority = "1-3.1N3";

              break;
            default:
              local.Worker.DashboardPriority = "1-3.1";

              break;
          }

          local.Worker.CaseNumber = entities.Case1.Number;
          local.Worker.CaseDate = entities.Case1.StatusDate;
          local.Worker.WorkerId =
            local.DashboardStagingPriority35.ReportLevelId;

          // -- Determine office and judicial district to which case is assigned
          // on the report period end date.
          if (AsChar(import.AuditFlag.Flag) == 'Y')
          {
            // -- Log to the dashboard audit table.
            UseFnB734CreateDashboardAudit1();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }
          }

          if (ReadDashboardStagingPriority8())
          {
            try
            {
              UpdateDashboardStagingPriority5();
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  ExitState = "DASHBOARD_STAGING_PRI_3_5_NU";

                  break;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "DASHBOARD_STAGING_PRI_3_5_PV";

                  break;
                default:
                  throw;
              }
            }
          }
          else
          {
            try
            {
              CreateDashboardStagingPriority4();
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  ExitState = "DASHBOARD_STAGING_PRI_3_5_AE";

                  break;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "DASHBOARD_STAGING_PRI_3_5_PV";

                  break;
                default:
                  throw;
              }
            }
          }

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail =
              "Error creating/updating Dashboard_Staging_Priority_1_2.";
            UseCabErrorReport();
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }
        }

        local.DashboardStagingPriority35.Assign(
          local.NullDashboardStagingPriority35);

        if (AsChar(local.CountCaseAtty.Flag) == 'Y')
        {
          // -- Case does not owe arrears.  Skip this case.
          local.DashboardStagingPriority35.AsOfDate =
            import.ProgramProcessingInfo.ProcessDate;
          local.DashboardStagingPriority35.ReportLevel = "AT";
          local.DashboardStagingPriority35.ReportLevelId =
            entities.ServiceProvider.UserId;
          local.DashboardStagingPriority35.ReportMonth =
            import.DashboardAuditData.ReportMonth;
          local.DashboardStagingPriority35.CasesPayingArrearsNumerator = 1;
          local.Worker.Assign(local.DashboardAuditData);

          switch(TrimEnd(local.DashboardAuditData.DashboardPriority))
          {
            case "1-3(N)#1":
              local.Worker.DashboardPriority = "1-3.2N1";
              local.Worker.CollectionAmount =
                local.DashboardAuditData.CollectionAmount ?? 0M;

              break;
            case "1-3(N)#2":
              local.Worker.DashboardPriority = "1-3.2N2";
              local.Worker.CollectionAmount = local.NonNa.Amount;

              break;
            case "1-3(N)#3":
              local.Worker.DashboardPriority = "1-3.2N3";

              break;
            default:
              local.Worker.DashboardPriority = "1-3.2";

              break;
          }

          local.Worker.CaseNumber = entities.Case1.Number;
          local.Worker.CaseDate = entities.Case1.StatusDate;
          local.Worker.WorkerId =
            local.DashboardStagingPriority35.ReportLevelId;

          if (AsChar(import.AuditFlag.Flag) == 'Y')
          {
            // -- Log to the dashboard audit table.
            UseFnB734CreateDashboardAudit1();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }
          }

          if (ReadDashboardStagingPriority8())
          {
            try
            {
              UpdateDashboardStagingPriority5();
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  ExitState = "DASHBOARD_STAGING_PRI_3_5_NU";

                  break;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "DASHBOARD_STAGING_PRI_3_5_PV";

                  break;
                default:
                  throw;
              }
            }
          }
          else
          {
            try
            {
              CreateDashboardStagingPriority4();
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  ExitState = "DASHBOARD_STAGING_PRI_3_5_AE";

                  break;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "DASHBOARD_STAGING_PRI_3_5_PV";

                  break;
                default:
                  throw;
              }
            }
          }

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail =
              "Error creating/updating Dashboard_Staging_Priority_1_2.";
            UseCabErrorReport();
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }
        }

ReadEach9:
        ;
      }
    }

    // ------------------------------------------------------------------------------
    // -- Store final Statewide counts.
    // ------------------------------------------------------------------------------
    if (ReadDashboardStagingPriority6())
    {
      try
      {
        UpdateDashboardStagingPriority2();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "DASHBOARD_STAGING_PRI_1_2_NU";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "DASHBOARD_STAGING_PRI_1_2_PV";

            break;
          default:
            throw;
        }
      }
    }
    else
    {
      try
      {
        CreateDashboardStagingPriority1();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "DASHBOARD_STAGING_PRI_1_2_AE";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "DASHBOARD_STAGING_PRI_1_2_PV";

            break;
          default:
            throw;
        }
      }
    }

    // ------------------------------------------------------------------------------
    // -- Store final Judicial District counts.
    // ------------------------------------------------------------------------------
    // -- Save Judicial District counts.
    for(local.Local1.Index = 0; local.Local1.Index < local.Local1.Count; ++
      local.Local1.Index)
    {
      if (!local.Local1.CheckSize())
      {
        break;
      }

      if (ReadDashboardStagingPriority7())
      {
        try
        {
          UpdateDashboardStagingPriority3();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "DASHBOARD_STAGING_PRI_1_2_NU";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "DASHBOARD_STAGING_PRI_1_2_PV";

              break;
            default:
              throw;
          }
        }
      }
      else
      {
        try
        {
          CreateDashboardStagingPriority2();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "DASHBOARD_STAGING_PRI_1_2_AE";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "DASHBOARD_STAGING_PRI_1_2_PV";

              break;
            default:
              throw;
          }
        }
      }
    }

    local.Local1.CheckIndex();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error creating/updating Dashboard_Staging_Priority_1_2.";
      UseCabErrorReport();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // ------------------------------------------------------------------------------
    // -- Rollup current year Judicial District counts to the Contractor Level.
    // ------------------------------------------------------------------------------
    for(local.Local1.Index = 0; local.Local1.Index < local.Local1.Count; ++
      local.Local1.Index)
    {
      if (!local.Local1.CheckSize())
      {
        break;
      }

      // -- Add all the current jd amounts into the correct contractor
      for(local.Contractor.Index = 0; local.Contractor.Index < local
        .Contractor.Count; ++local.Contractor.Index)
      {
        if (!local.Contractor.CheckSize())
        {
          break;
        }

        if (Equal(local.Local1.Item.G.ContractorNumber,
          local.Contractor.Item.Gcontractor.ContractorNumber))
        {
          local.Contractor.Update.Gcontractor.CasesPayingArrearsNumerator =
            (local.Contractor.Item.Gcontractor.CasesPayingArrearsNumerator ?? 0) +
            (local.Local1.Item.G.CasesPayingArrearsNumerator ?? 0);
          local.Contractor.Update.Gcontractor.CasesPayingArrearsDenominator =
            (local.Contractor.Item.Gcontractor.
              CasesPayingArrearsDenominator ?? 0) + (
              local.Local1.Item.G.CasesPayingArrearsDenominator ?? 0);

          goto Next;
        }
      }

      local.Contractor.CheckIndex();

Next:
      ;
    }

    local.Local1.CheckIndex();

    // ------------------------------------------------------------------------------
    // --                    I M P O R T A N T     N O T E
    // --
    // -- The logic below is rolling up previous year JD data to the Contractor 
    // level
    // -- based on the Contractor hierarchy as it exists TODAY, not the 
    // hierarchy as
    // -- it existed last year.  Therefore, if the contractor hierarchy has 
    // changed
    // -- then the values derived in this logic will not match the reported 
    // Dashboard
    // -- values for the previous year for the contractor.
    // ------------------------------------------------------------------------------
    // ------------------------------------------------------------------------------
    // -- Rollup previous year Judicial District counts to the Contractor level.
    // ------------------------------------------------------------------------------
    foreach(var _ in ReadDashboardStagingPriority9())
    {
      local.DashboardAuditData.JudicialDistrict =
        entities.PreviousYear.ReportLevelId;
      UseFnB734DeterContractorFromJd();

      // -- Add previous years Cases Paying Arrears values to appropriate 
      // contractor
      for(local.Contractor.Index = 0; local.Contractor.Index < local
        .Contractor.Count; ++local.Contractor.Index)
      {
        if (!local.Contractor.CheckSize())
        {
          break;
        }

        if (Equal(local.Contractor.Item.Gcontractor.ContractorNumber,
          local.Contractor1.Code))
        {
          local.Contractor.Update.Gcontractor.PrvYrCasesPaidArrearsNumtr =
            (local.Contractor.Item.Gcontractor.PrvYrCasesPaidArrearsNumtr ?? 0) +
            (entities.PreviousYear.CasesPayingArrearsNumerator ?? 0);
          local.Contractor.Update.Gcontractor.PrvYrCasesPaidArrearsDenom =
            (local.Contractor.Item.Gcontractor.PrvYrCasesPaidArrearsDenom ?? 0) +
            (entities.PreviousYear.CasesPayingArrearsDenominator ?? 0);

          goto ReadEach10;
        }
      }

      local.Contractor.CheckIndex();

ReadEach10:
      ;
    }

    // ------------------------------------------------------------------------------
    // -- Store final Contractor counts.
    // ------------------------------------------------------------------------------
    for(local.Contractor.Index = 0; local.Contractor.Index < local
      .Contractor.Count; ++local.Contractor.Index)
    {
      if (!local.Contractor.CheckSize())
      {
        break;
      }

      if (ReadDashboardStagingPriority10())
      {
        try
        {
          UpdateDashboardStagingPriority6();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "DASHBOARD_STAGING_PRI_1_2_NU";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "DASHBOARD_STAGING_PRI_1_2_PV";

              break;
            default:
              throw;
          }
        }
      }
      else
      {
        try
        {
          CreateDashboardStagingPriority5();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "DASHBOARD_STAGING_PRI_1_2_AE";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "DASHBOARD_STAGING_PRI_1_2_PV";

              break;
            default:
              throw;
          }
        }
      }
    }

    local.Contractor.CheckIndex();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error creating/updating Dashboard_Staging_Priority_1_2.";
      UseCabErrorReport();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // ------------------------------------------------------------------------------
    // -- Calculate the Statewide, Judicial District and Contractor's Cases 
    // Paying
    // -- Arrears Percent,  Previous Year Cases Paying Arrears Percent, and 
    // Percent
    // -- Change from the Previous Year.
    // ------------------------------------------------------------------------------
    foreach(var _ in ReadDashboardStagingPriority11())
    {
      local.Temp.Assign(entities.DashboardStagingPriority12);

      // -- Calculate Current Year Cases Paying Arrears percent.
      if ((local.Temp.CasesPayingArrearsDenominator ?? 0) == 0)
      {
        local.Temp.CasesPayingArrearsPercent = 0;
      }
      else
      {
        local.Temp.CasesPayingArrearsPercent =
          Math.Round((decimal)(local.Temp.CasesPayingArrearsNumerator ?? 0) /
          (local.Temp.CasesPayingArrearsDenominator ?? 0), 3,
          MidpointRounding.AwayFromZero);
      }

      // -- Read for the previous year Cases Paying Arrears values for all but 
      // the contractor level.
      // -- The contractor level previous year values were calculated and stored
      // earlier.
      if (!Equal(entities.DashboardStagingPriority12.ReportLevel, "XJ"))
      {
        if (ReadDashboardStagingPriority13())
        {
          local.Temp.PrvYrCasesPaidArrearsNumtr =
            entities.PreviousYear.CasesPayingArrearsNumerator;
          local.Temp.PrvYrCasesPaidArrearsDenom =
            entities.PreviousYear.CasesPayingArrearsDenominator;
        }
        else
        {
          local.Temp.PrvYrCasesPaidArrearsNumtr = 0;
          local.Temp.PrvYrCasesPaidArrearsDenom = 0;
        }
      }

      // -- Calculate Previous Year Cases Paying Arrears percent.
      if ((local.Temp.PrvYrCasesPaidArrearsDenom ?? 0) == 0)
      {
        local.Temp.CasesPayArrearsPrvYrPct = 0;
      }
      else
      {
        local.Temp.CasesPayArrearsPrvYrPct =
          Math.Round((decimal)(local.Temp.PrvYrCasesPaidArrearsNumtr ?? 0) /
          (local.Temp.PrvYrCasesPaidArrearsDenom ?? 0), 3,
          MidpointRounding.AwayFromZero);
      }

      // -- Calculate percent change between Current Year Cases Paying Arrears 
      // percent and Previous Year Cases Paying Arrears percent.
      if ((local.Temp.CasesPayArrearsPrvYrPct ?? 0M) == 0)
      {
        local.Temp.PctChgBtwenYrsCasesPayArrs = 0;
      }
      else
      {
        local.Temp.PctChgBtwenYrsCasesPayArrs =
          Math.Round(((local.Temp.CasesPayingArrearsPercent ?? 0M) - (
            local.Temp.CasesPayArrearsPrvYrPct ?? 0M
          )) /
          (local.Temp.CasesPayArrearsPrvYrPct ?? 0M), 3,
          MidpointRounding.AwayFromZero);
      }

      try
      {
        UpdateDashboardStagingPriority7();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "DASHBOARD_STAGING_PRI_1_2_NU";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "DASHBOARD_STAGING_PRI_1_2_PV";

            break;
          default:
            throw;
        }
      }
    }

    foreach(var _ in ReadDashboardStagingPriority14())
    {
      local.DashboardStagingPriority35.Assign(
        entities.DashboardStagingPriority35);

      // -- Calculate Current Year Cases Paying Arrears percent.
      if ((local.DashboardStagingPriority35.CasesPayingArrearsDenominator ?? 0) ==
        0)
      {
        local.DashboardStagingPriority35.CasesPayingArrearsPercent = 0;
      }
      else
      {
        local.DashboardStagingPriority35.CasesPayingArrearsPercent =
          Math.Round((decimal)(
            local.DashboardStagingPriority35.CasesPayingArrearsNumerator ?? 0
          ) /
          (local.DashboardStagingPriority35.CasesPayingArrearsDenominator ?? 0),
          3, MidpointRounding.AwayFromZero);
      }

      try
      {
        UpdateDashboardStagingPriority8();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "DASHBOARD_STAGING_PRI_3_5_NU";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "DASHBOARD_STAGING_PRI_3_5_PV";

            break;
          default:
            throw;
        }
      }
    }

    local.Common.Count = 0;
    local.PreviousRank.CasesPayingArrearsPercent = 0;
    local.DashboardStagingPriority35.CasesPayingArrearsRank = 1;

    // ------------------------------------------------------------------------------
    // -- Calculate Case Worker Ranking.
    // ------------------------------------------------------------------------------
    foreach(var _ in ReadDashboardStagingPriority15())
    {
      ++local.Common.Count;

      if ((entities.DashboardStagingPriority35.CasesPayingArrearsPercent ?? 0M) ==
        (local.PreviousRank.CasesPayingArrearsPercent ?? 0M))
      {
        // -- The ranking for this case worker is tied with the previous case 
        // worker
        // -- This worker gets the same ranking already in the local_case_paying
        // arrears rank.
      }
      else
      {
        local.DashboardStagingPriority35.CasesPayingArrearsRank =
          local.Common.Count;
      }

      try
      {
        UpdateDashboardStagingPriority9();
        local.PreviousRank.CasesPayingArrearsPercent =
          entities.DashboardStagingPriority35.CasesPayingArrearsPercent;
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "DASHBOARD_STAGING_PRI_3_5_NU";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "DASHBOARD_STAGING_PRI_3_5_PV";

            break;
          default:
            throw;
        }
      }
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error creating/updating Dashboard_Staging_Priority_1_2.";
      UseCabErrorReport();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.Common.Count = 0;
    local.PreviousRank.CasesPayingArrearsPercent = 0;
    local.DashboardStagingPriority35.CasesPayingArrearsRank = 1;

    // ------------------------------------------------------------------------------
    // -- Calculate Attorney Ranking.
    // ------------------------------------------------------------------------------
    foreach(var _ in ReadDashboardStagingPriority15())
    {
      ++local.Common.Count;

      if ((entities.DashboardStagingPriority35.CasesPayingArrearsPercent ?? 0M) ==
        (local.PreviousRank.CasesPayingArrearsPercent ?? 0M))
      {
        // -- The ranking for this attorney is tied with the previous attorney.
        // -- This attorney gets the same ranking already in the 
        // local_case_paying arrears_rank.
      }
      else
      {
        local.DashboardStagingPriority35.CasesPayingArrearsRank =
          local.Common.Count;
      }

      try
      {
        UpdateDashboardStagingPriority9();
        local.PreviousRank.CasesPayingArrearsPercent =
          entities.DashboardStagingPriority35.CasesPayingArrearsPercent;
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "DASHBOARD_STAGING_PRI_3_5_NU";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "DASHBOARD_STAGING_PRI_3_5_PV";

            break;
          default:
            throw;
        }
      }
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error creating/updating Dashboard_Staging_Priority_1_2.";
      UseCabErrorReport();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.Common.Count = 0;
    local.PrevRank.CasesPayingArrearsPercent = 0;
    local.Temp.CasesPayingArrearsRank = 1;

    // ------------------------------------------------------------------------------
    // -- Calculate Judicial District Ranking.
    // ------------------------------------------------------------------------------
    foreach(var _ in ReadDashboardStagingPriority16())
    {
      ++local.Common.Count;

      if ((entities.DashboardStagingPriority12.CasesPayingArrearsPercent ?? 0M) ==
        (local.PrevRank.CasesPayingArrearsPercent ?? 0M))
      {
        // -- The ranking for this judicial district is tied with the previous 
        // judicial district.
        // -- This JD gets the same ranking already in the local_temp 
        // case_under_order_rank.
      }
      else
      {
        local.Temp.CasesPayingArrearsRank = local.Common.Count;
      }

      try
      {
        UpdateDashboardStagingPriority10();
        local.PrevRank.CasesPayingArrearsPercent =
          entities.DashboardStagingPriority12.CasesPayingArrearsPercent;
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "DASHBOARD_STAGING_PRI_1_2_NU";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "DASHBOARD_STAGING_PRI_1_2_PV";

            break;
          default:
            throw;
        }
      }
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error creating/updating Dashboard_Staging_Priority_1_2.";
      UseCabErrorReport();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.Common.Count = 0;
    local.PrevRank.CasesPayingArrearsPercent = 0;
    local.Temp.CasesPayingArrearsRank = 1;

    // ------------------------------------------------------------------------------
    // -- Calculate Contractor Ranking.
    // ------------------------------------------------------------------------------
    foreach(var _ in ReadDashboardStagingPriority17())
    {
      ++local.Common.Count;

      if ((entities.DashboardStagingPriority12.CasesPayingArrearsPercent ?? 0M) ==
        (local.PrevRank.CasesPayingArrearsPercent ?? 0M))
      {
        // -- The ranking for this judicial district is tied with the previous 
        // judicial district.
        // -- This JD gets the same ranking already in the local_temp 
        // case_under_order_rank.
      }
      else
      {
        local.Temp.CasesPayingArrearsRank = local.Common.Count;
      }

      try
      {
        UpdateDashboardStagingPriority10();
        local.PrevRank.CasesPayingArrearsPercent =
          entities.DashboardStagingPriority12.CasesPayingArrearsPercent;
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "DASHBOARD_STAGING_PRI_1_2_NU";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "DASHBOARD_STAGING_PRI_1_2_PV";

            break;
          default:
            throw;
        }
      }
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error creating/updating Dashboard_Staging_Priority_1_2.";
      UseCabErrorReport();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // ------------------------------------------------------------------------------
    // -- Take a final checkpoint for restarting at the next priority.
    // ------------------------------------------------------------------------------
    // -- Checkpoint Info
    // Positions   Value
    // ---------   
    // ------------------------------------
    //  001-080    General Checkpoint Info for PRAD
    //  081-088    Dashboard Priority
    local.ProgramCheckpointRestart.RestartInd = "Y";
    local.ProgramCheckpointRestart.RestartInfo = "";
    local.ProgramCheckpointRestart.RestartInfo =
      Substring(import.ProgramCheckpointRestart.RestartInfo, 250, 1, 80) + "1-04    ";
    UseUpdateCheckpointRstAndCommit();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "Error taking checkpoint.";
      UseCabErrorReport();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }
  }

  private static void MoveCollection(Collection source, Collection target)
  {
    target.ProgramAppliedTo = source.ProgramAppliedTo;
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Amount = source.Amount;
    target.AppliedToCode = source.AppliedToCode;
    target.CollectionDt = source.CollectionDt;
    target.AdjustedInd = source.AdjustedInd;
    target.ConcurrentInd = source.ConcurrentInd;
    target.CollectionAdjustmentDt = source.CollectionAdjustmentDt;
    target.CreatedTmst = source.CreatedTmst;
  }

  private static void MoveCseOrganization(CseOrganization source,
    CseOrganization target)
  {
    target.Code = source.Code;
    target.Name = source.Name;
  }

  private static void MoveDashboardAuditData1(DashboardAuditData source,
    DashboardAuditData target)
  {
    target.ReportMonth = source.ReportMonth;
    target.DashboardPriority = source.DashboardPriority;
    target.RunNumber = source.RunNumber;
    target.Office = source.Office;
    target.JudicialDistrict = source.JudicialDistrict;
    target.WorkerId = source.WorkerId;
    target.CaseNumber = source.CaseNumber;
    target.StandardNumber = source.StandardNumber;
    target.PayorCspNumber = source.PayorCspNumber;
    target.SuppCspNumber = source.SuppCspNumber;
    target.Fte = source.Fte;
    target.CollectionAmount = source.CollectionAmount;
    target.CollAppliedToCd = source.CollAppliedToCd;
    target.CollectionCreatedDate = source.CollectionCreatedDate;
    target.CollectionType = source.CollectionType;
    target.DebtBalanceDue = source.DebtBalanceDue;
    target.DebtDueDate = source.DebtDueDate;
    target.DebtType = source.DebtType;
    target.LegalActionDate = source.LegalActionDate;
    target.LegalReferralDate = source.LegalReferralDate;
    target.LegalReferralNumber = source.LegalReferralNumber;
    target.DaysReported = source.DaysReported;
    target.VerifiedDate = source.VerifiedDate;
    target.CaseDate = source.CaseDate;
    target.ReviewDate = source.ReviewDate;
  }

  private static void MoveDashboardAuditData2(DashboardAuditData source,
    DashboardAuditData target)
  {
    target.ReportMonth = source.ReportMonth;
    target.RunNumber = source.RunNumber;
  }

  private static void MoveDashboardAuditData3(DashboardAuditData source,
    DashboardAuditData target)
  {
    target.Office = source.Office;
    target.JudicialDistrict = source.JudicialDistrict;
    target.CaseNumber = source.CaseNumber;
  }

  private static void MoveDebtDetail(DebtDetail source, DebtDetail target)
  {
    target.DueDt = source.DueDt;
    target.CoveredPrdStartDt = source.CoveredPrdStartDt;
    target.PreconversionProgramCode = source.PreconversionProgramCode;
  }

  private static void MoveObligationType(ObligationType source,
    ObligationType target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Classification = source.Classification;
  }

  private static void MoveProgramCheckpointRestart(
    ProgramCheckpointRestart source, ProgramCheckpointRestart target)
  {
    target.ProgramName = source.ProgramName;
    target.UpdateFrequencyCount = source.UpdateFrequencyCount;
    target.ReadFrequencyCount = source.ReadFrequencyCount;
    target.RestartInd = source.RestartInd;
    target.RestartInfo = source.RestartInfo;
  }

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    context.Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseFnB734CreateDashboardAudit1()
  {
    var useImport = new FnB734CreateDashboardAudit.Import();
    var useExport = new FnB734CreateDashboardAudit.Export();

    useImport.DashboardAuditData.Assign(local.Worker);

    context.Call(FnB734CreateDashboardAudit.Execute, useImport, useExport);
  }

  private void UseFnB734CreateDashboardAudit2()
  {
    var useImport = new FnB734CreateDashboardAudit.Import();
    var useExport = new FnB734CreateDashboardAudit.Export();

    useImport.DashboardAuditData.Assign(local.DashboardAuditData);

    context.Call(FnB734CreateDashboardAudit.Execute, useImport, useExport);
  }

  private void UseFnB734DeterContractorFromJd()
  {
    var useImport = new FnB734DeterContractorFromJd.Import();
    var useExport = new FnB734DeterContractorFromJd.Export();

    useImport.ReportEndDate.Date = import.ReportEndDate.Date;
    useImport.DashboardAuditData.JudicialDistrict =
      local.DashboardAuditData.JudicialDistrict;

    context.Call(FnB734DeterContractorFromJd.Execute, useImport, useExport);

    MoveCseOrganization(useExport.Contractor, local.Contractor1);
  }

  private void UseFnB734DetermineJdFromCase()
  {
    var useImport = new FnB734DetermineJdFromCase.Import();
    var useExport = new FnB734DetermineJdFromCase.Export();

    useImport.Case1.Number = entities.Case1.Number;
    useImport.ReportEndDate.Date = local.TempEndDate.Date;

    context.Call(FnB734DetermineJdFromCase.Execute, useImport, useExport);

    MoveDashboardAuditData3(useExport.DashboardAuditData,
      local.DashboardAuditData);
  }

  private void UseFnCheckForActiveAfFcPgm()
  {
    var useImport = new FnCheckForActiveAfFcPgm.Import();
    var useExport = new FnCheckForActiveAfFcPgm.Export();

    useImport.Case1.Assign(entities.Case1);
    useImport.AsOfDate.Date = local.CollectionDate.Date;

    context.Call(FnCheckForActiveAfFcPgm.Execute, useImport, useExport);

    local.CollectionDate.Date = useImport.AsOfDate.Date;
    local.Assistance.Flag = useExport.AssistanceProgram.Flag;
  }

  private void UseFnDeterminePgmForDebtDetail1()
  {
    var useImport = new FnDeterminePgmForDebtDetail.Import();
    var useExport = new FnDeterminePgmForDebtDetail.Export();

    MoveObligationType(entities.ObligationType, useImport.ObligationType);
    useImport.SupportedPerson.Number = entities.ChOrArCsePerson.Number;
    MoveDebtDetail(entities.DebtDetail, useImport.DebtDetail);

    context.Call(FnDeterminePgmForDebtDetail.Execute, useImport, useExport);

    local.Program.Code = useExport.Program.Code;
  }

  private void UseFnDeterminePgmForDebtDetail2()
  {
    var useImport = new FnDeterminePgmForDebtDetail.Import();
    var useExport = new FnDeterminePgmForDebtDetail.Export();

    MoveObligationType(entities.ObligationType, useImport.ObligationType);
    MoveDebtDetail(entities.DebtDetail, useImport.DebtDetail);

    useImport.SupportedPerson.Number = entities.Supp.Number;
    useImport.Collection.Date = local.CollectionDate.Date;

    context.Call(FnDeterminePgmForDebtDetail.Execute, useImport, useExport);

    local.Program.Code = useExport.Program.Code;
  }

  private void UseUpdateCheckpointRstAndCommit()
  {
    var useImport = new UpdateCheckpointRstAndCommit.Import();
    var useExport = new UpdateCheckpointRstAndCommit.Export();

    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);

    context.Call(UpdateCheckpointRstAndCommit.Execute, useImport, useExport);
  }

  private void CreateDashboardStagingPriority1()
  {
    var reportMonth = local.Statewide.ReportMonth;
    var reportLevel = local.Statewide.ReportLevel;
    var reportLevelId = local.Statewide.ReportLevelId;
    var asOfDate = local.Statewide.AsOfDate;
    var param = 0M;
    var casesPayingArrearsNumerator =
      local.Statewide.CasesPayingArrearsNumerator ?? 0;
    var casesPayingArrearsDenominator =
      local.Statewide.CasesPayingArrearsDenominator ?? 0;
    var casesPayingArrearsPercent =
      local.Statewide.CasesPayingArrearsPercent ?? 0M;
    var casesPayingArrearsRank = local.Statewide.CasesPayingArrearsRank ?? 0;

    entities.DashboardStagingPriority12.Populated = false;
    Update("CreateDashboardStagingPriority1",
      (db, command) =>
      {
        db.SetInt32(command, "reportMonth", reportMonth);
        db.SetString(command, "reportLevel", reportLevel);
        db.SetString(command, "reportLevelId", reportLevelId);
        db.SetNullableDate(command, "asOfDt", asOfDate);
        db.SetNullableInt32(command, "casUnderOrdNum", 0);
        db.SetNullableDecimal(command, "casUnderOrdPer", param);
        db.SetNullableInt32(
          command, "casPayingArrNum", casesPayingArrearsNumerator);
        db.SetNullableInt32(
          command, "casPayingArrDen", casesPayingArrearsDenominator);
        db.SetNullableDecimal(
          command, "casPayingArrPer", casesPayingArrearsPercent);
        db.SetNullableInt32(command, "casPayingArrRnk", casesPayingArrearsRank);
        db.SetNullableDecimal(command, "curSupPdMthNum", param);
        db.SetNullableDecimal(command, "casPerFteDen", param);
        db.SetNullableString(command, "contractorNum", "");
        db.SetNullableInt32(command, "prvYrPdArNumtr", 0);
        db.SetNullableInt32(command, "prvYrPdArDenom", 0);
        db.SetNullableDecimal(command, "payArPrvYrPct", param);
        db.SetNullableDecimal(command, "pctChgByrArsPd", param);
      });

    entities.DashboardStagingPriority12.ReportMonth = reportMonth;
    entities.DashboardStagingPriority12.ReportLevel = reportLevel;
    entities.DashboardStagingPriority12.ReportLevelId = reportLevelId;
    entities.DashboardStagingPriority12.AsOfDate = asOfDate;
    entities.DashboardStagingPriority12.CasesPayingArrearsNumerator =
      casesPayingArrearsNumerator;
    entities.DashboardStagingPriority12.CasesPayingArrearsDenominator =
      casesPayingArrearsDenominator;
    entities.DashboardStagingPriority12.CasesPayingArrearsPercent =
      casesPayingArrearsPercent;
    entities.DashboardStagingPriority12.CasesPayingArrearsRank =
      casesPayingArrearsRank;
    entities.DashboardStagingPriority12.ContractorNumber = "";
    entities.DashboardStagingPriority12.PrvYrCasesPaidArrearsNumtr = 0;
    entities.DashboardStagingPriority12.PrvYrCasesPaidArrearsDenom = 0;
    entities.DashboardStagingPriority12.CasesPayArrearsPrvYrPct = param;
    entities.DashboardStagingPriority12.PctChgBtwenYrsCasesPayArrs = param;
    entities.DashboardStagingPriority12.Populated = true;
  }

  private void CreateDashboardStagingPriority2()
  {
    var reportMonth = local.Local1.Item.G.ReportMonth;
    var reportLevel = local.Local1.Item.G.ReportLevel;
    var reportLevelId = local.Local1.Item.G.ReportLevelId;
    var asOfDate = local.Local1.Item.G.AsOfDate;
    var param = 0M;
    var casesPayingArrearsNumerator =
      local.Local1.Item.G.CasesPayingArrearsNumerator ?? 0;
    var casesPayingArrearsDenominator =
      local.Local1.Item.G.CasesPayingArrearsDenominator ?? 0;
    var casesPayingArrearsPercent =
      local.Local1.Item.G.CasesPayingArrearsPercent ?? 0M;
    var casesPayingArrearsRank = local.Local1.Item.G.CasesPayingArrearsRank ?? 0
      ;

    entities.DashboardStagingPriority12.Populated = false;
    Update("CreateDashboardStagingPriority2",
      (db, command) =>
      {
        db.SetInt32(command, "reportMonth", reportMonth);
        db.SetString(command, "reportLevel", reportLevel);
        db.SetString(command, "reportLevelId", reportLevelId);
        db.SetNullableDate(command, "asOfDt", asOfDate);
        db.SetNullableInt32(command, "casUnderOrdNum", 0);
        db.SetNullableDecimal(command, "casUnderOrdPer", param);
        db.SetNullableInt32(
          command, "casPayingArrNum", casesPayingArrearsNumerator);
        db.SetNullableInt32(
          command, "casPayingArrDen", casesPayingArrearsDenominator);
        db.SetNullableDecimal(
          command, "casPayingArrPer", casesPayingArrearsPercent);
        db.SetNullableInt32(command, "casPayingArrRnk", casesPayingArrearsRank);
        db.SetNullableDecimal(command, "curSupPdMthNum", param);
        db.SetNullableDecimal(command, "casPerFteDen", param);
        db.SetNullableString(command, "contractorNum", "");
        db.SetNullableInt32(command, "prvYrPdArNumtr", 0);
        db.SetNullableInt32(command, "prvYrPdArDenom", 0);
        db.SetNullableDecimal(command, "payArPrvYrPct", param);
        db.SetNullableDecimal(command, "pctChgByrArsPd", param);
      });

    entities.DashboardStagingPriority12.ReportMonth = reportMonth;
    entities.DashboardStagingPriority12.ReportLevel = reportLevel;
    entities.DashboardStagingPriority12.ReportLevelId = reportLevelId;
    entities.DashboardStagingPriority12.AsOfDate = asOfDate;
    entities.DashboardStagingPriority12.CasesPayingArrearsNumerator =
      casesPayingArrearsNumerator;
    entities.DashboardStagingPriority12.CasesPayingArrearsDenominator =
      casesPayingArrearsDenominator;
    entities.DashboardStagingPriority12.CasesPayingArrearsPercent =
      casesPayingArrearsPercent;
    entities.DashboardStagingPriority12.CasesPayingArrearsRank =
      casesPayingArrearsRank;
    entities.DashboardStagingPriority12.ContractorNumber = "";
    entities.DashboardStagingPriority12.PrvYrCasesPaidArrearsNumtr = 0;
    entities.DashboardStagingPriority12.PrvYrCasesPaidArrearsDenom = 0;
    entities.DashboardStagingPriority12.CasesPayArrearsPrvYrPct = param;
    entities.DashboardStagingPriority12.PctChgBtwenYrsCasesPayArrs = param;
    entities.DashboardStagingPriority12.Populated = true;
  }

  private void CreateDashboardStagingPriority3()
  {
    var reportMonth = local.DashboardStagingPriority35.ReportMonth;
    var reportLevel = local.DashboardStagingPriority35.ReportLevel;
    var reportLevelId = local.DashboardStagingPriority35.ReportLevelId;
    var asOfDate = local.DashboardStagingPriority35.AsOfDate;
    var param = 0M;
    var casesPayingArrearsDenominator =
      local.DashboardStagingPriority35.CasesPayingArrearsDenominator ?? 0;

    entities.DashboardStagingPriority35.Populated = false;
    Update("CreateDashboardStagingPriority3",
      (db, command) =>
      {
        db.SetInt32(command, "reportMonth", reportMonth);
        db.SetString(command, "reportLevel", reportLevel);
        db.SetString(command, "reportLevelId", reportLevelId);
        db.SetNullableDate(command, "asOfDate", asOfDate);
        db.SetNullableInt32(command, "casWEstRef", 0);
        db.SetNullableDecimal(command, "fullTimeEqvlnt", param);
        db.SetNullableDecimal(command, "STypeCollAmt", param);
        db.SetNullableDecimal(command, "STypeCollPer", param);
        db.SetNullableInt32(
          command, "casPayingArrDen", casesPayingArrearsDenominator);
        db.SetNullableInt32(command, "casPayingArrNum", 0);
        db.SetNullableDecimal(command, "casPayingArrPer", param);
        db.SetNullableInt32(command, "casPayingArrRnk", 0);
      });

    entities.DashboardStagingPriority35.ReportMonth = reportMonth;
    entities.DashboardStagingPriority35.ReportLevel = reportLevel;
    entities.DashboardStagingPriority35.ReportLevelId = reportLevelId;
    entities.DashboardStagingPriority35.AsOfDate = asOfDate;
    entities.DashboardStagingPriority35.CasesPayingArrearsDenominator =
      casesPayingArrearsDenominator;
    entities.DashboardStagingPriority35.CasesPayingArrearsNumerator = 0;
    entities.DashboardStagingPriority35.CasesPayingArrearsPercent = param;
    entities.DashboardStagingPriority35.CasesPayingArrearsRank = 0;
    entities.DashboardStagingPriority35.Populated = true;
  }

  private void CreateDashboardStagingPriority4()
  {
    var reportMonth = local.DashboardStagingPriority35.ReportMonth;
    var reportLevel = local.DashboardStagingPriority35.ReportLevel;
    var reportLevelId = local.DashboardStagingPriority35.ReportLevelId;
    var asOfDate = local.DashboardStagingPriority35.AsOfDate;
    var param = 0M;
    var casesPayingArrearsNumerator =
      local.DashboardStagingPriority35.CasesPayingArrearsNumerator ?? 0;

    entities.DashboardStagingPriority35.Populated = false;
    Update("CreateDashboardStagingPriority4",
      (db, command) =>
      {
        db.SetInt32(command, "reportMonth", reportMonth);
        db.SetString(command, "reportLevel", reportLevel);
        db.SetString(command, "reportLevelId", reportLevelId);
        db.SetNullableDate(command, "asOfDate", asOfDate);
        db.SetNullableInt32(command, "casWEstRef", 0);
        db.SetNullableDecimal(command, "fullTimeEqvlnt", param);
        db.SetNullableDecimal(command, "STypeCollAmt", param);
        db.SetNullableDecimal(command, "STypeCollPer", param);
        db.SetNullableInt32(command, "casPayingArrDen", 0);
        db.SetNullableInt32(
          command, "casPayingArrNum", casesPayingArrearsNumerator);
        db.SetNullableDecimal(command, "casPayingArrPer", param);
        db.SetNullableInt32(command, "casPayingArrRnk", 0);
      });

    entities.DashboardStagingPriority35.ReportMonth = reportMonth;
    entities.DashboardStagingPriority35.ReportLevel = reportLevel;
    entities.DashboardStagingPriority35.ReportLevelId = reportLevelId;
    entities.DashboardStagingPriority35.AsOfDate = asOfDate;
    entities.DashboardStagingPriority35.CasesPayingArrearsDenominator = 0;
    entities.DashboardStagingPriority35.CasesPayingArrearsNumerator =
      casesPayingArrearsNumerator;
    entities.DashboardStagingPriority35.CasesPayingArrearsPercent = param;
    entities.DashboardStagingPriority35.CasesPayingArrearsRank = 0;
    entities.DashboardStagingPriority35.Populated = true;
  }

  private void CreateDashboardStagingPriority5()
  {
    var reportMonth = local.Contractor.Item.Gcontractor.ReportMonth;
    var reportLevel = local.Contractor.Item.Gcontractor.ReportLevel;
    var reportLevelId = local.Contractor.Item.Gcontractor.ReportLevelId;
    var asOfDate = local.Contractor.Item.Gcontractor.AsOfDate;
    var param = 0M;
    var casesPayingArrearsNumerator =
      local.Contractor.Item.Gcontractor.CasesPayingArrearsNumerator ?? 0;
    var casesPayingArrearsDenominator =
      local.Contractor.Item.Gcontractor.CasesPayingArrearsDenominator ?? 0;
    var casesPayingArrearsPercent =
      local.Contractor.Item.Gcontractor.CasesPayingArrearsPercent ?? 0M;
    var casesPayingArrearsRank =
      local.Contractor.Item.Gcontractor.CasesPayingArrearsRank ?? 0;
    var contractorNumber =
      local.Contractor.Item.Gcontractor.ContractorNumber ?? "";
    var prvYrCasesPaidArrearsNumtr =
      local.Contractor.Item.Gcontractor.PrvYrCasesPaidArrearsNumtr ?? 0;
    var prvYrCasesPaidArrearsDenom =
      local.Contractor.Item.Gcontractor.PrvYrCasesPaidArrearsDenom ?? 0;
    var casesPayArrearsPrvYrPct =
      local.Contractor.Item.Gcontractor.CasesPayArrearsPrvYrPct ?? 0M;
    var pctChgBtwenYrsCasesPayArrs =
      local.Contractor.Item.Gcontractor.PctChgBtwenYrsCasesPayArrs ?? 0M;

    entities.DashboardStagingPriority12.Populated = false;
    Update("CreateDashboardStagingPriority5",
      (db, command) =>
      {
        db.SetInt32(command, "reportMonth", reportMonth);
        db.SetString(command, "reportLevel", reportLevel);
        db.SetString(command, "reportLevelId", reportLevelId);
        db.SetNullableDate(command, "asOfDt", asOfDate);
        db.SetNullableInt32(command, "casUnderOrdNum", 0);
        db.SetNullableDecimal(command, "casUnderOrdPer", param);
        db.SetNullableInt32(
          command, "casPayingArrNum", casesPayingArrearsNumerator);
        db.SetNullableInt32(
          command, "casPayingArrDen", casesPayingArrearsDenominator);
        db.SetNullableDecimal(
          command, "casPayingArrPer", casesPayingArrearsPercent);
        db.SetNullableInt32(command, "casPayingArrRnk", casesPayingArrearsRank);
        db.SetNullableDecimal(command, "curSupPdMthNum", param);
        db.SetNullableDecimal(command, "casPerFteDen", param);
        db.SetNullableString(command, "contractorNum", contractorNumber);
        db.SetNullableInt32(
          command, "prvYrPdArNumtr", prvYrCasesPaidArrearsNumtr);
        db.SetNullableInt32(
          command, "prvYrPdArDenom", prvYrCasesPaidArrearsDenom);
        db.
          SetNullableDecimal(command, "payArPrvYrPct", casesPayArrearsPrvYrPct);
        db.SetNullableDecimal(
          command, "pctChgByrArsPd", pctChgBtwenYrsCasesPayArrs);
      });

    entities.DashboardStagingPriority12.ReportMonth = reportMonth;
    entities.DashboardStagingPriority12.ReportLevel = reportLevel;
    entities.DashboardStagingPriority12.ReportLevelId = reportLevelId;
    entities.DashboardStagingPriority12.AsOfDate = asOfDate;
    entities.DashboardStagingPriority12.CasesPayingArrearsNumerator =
      casesPayingArrearsNumerator;
    entities.DashboardStagingPriority12.CasesPayingArrearsDenominator =
      casesPayingArrearsDenominator;
    entities.DashboardStagingPriority12.CasesPayingArrearsPercent =
      casesPayingArrearsPercent;
    entities.DashboardStagingPriority12.CasesPayingArrearsRank =
      casesPayingArrearsRank;
    entities.DashboardStagingPriority12.ContractorNumber = contractorNumber;
    entities.DashboardStagingPriority12.PrvYrCasesPaidArrearsNumtr =
      prvYrCasesPaidArrearsNumtr;
    entities.DashboardStagingPriority12.PrvYrCasesPaidArrearsDenom =
      prvYrCasesPaidArrearsDenom;
    entities.DashboardStagingPriority12.CasesPayArrearsPrvYrPct =
      casesPayArrearsPrvYrPct;
    entities.DashboardStagingPriority12.PctChgBtwenYrsCasesPayArrs =
      pctChgBtwenYrsCasesPayArrs;
    entities.DashboardStagingPriority12.Populated = true;
  }

  private bool ReadCaseAssignmentServiceProvider()
  {
    entities.WorkerCaseAssignment.Populated = false;
    entities.WorkerServiceProvider.Populated = false;

    return Read("ReadCaseAssignmentServiceProvider",
      (db, command) =>
      {
        db.SetString(command, "casNo", entities.Case1.Number);
        db.SetDate(command, "effectiveDate", import.ReportEndDate.Date);
        db.SetNullableDate(
          command, "discontinueDate", import.ReportStartDate.Date);
      },
      (db, reader) =>
      {
        entities.WorkerCaseAssignment.ReasonCode = db.GetString(reader, 0);
        entities.WorkerCaseAssignment.EffectiveDate = db.GetDate(reader, 1);
        entities.WorkerCaseAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 2);
        entities.WorkerCaseAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 3);
        entities.WorkerCaseAssignment.SpdId = db.GetInt32(reader, 4);
        entities.WorkerCaseAssignment.OffId = db.GetInt32(reader, 5);
        entities.WorkerCaseAssignment.OspCode = db.GetString(reader, 6);
        entities.WorkerCaseAssignment.OspDate = db.GetDate(reader, 7);
        entities.WorkerCaseAssignment.CasNo = db.GetString(reader, 8);
        entities.WorkerServiceProvider.SystemGeneratedId =
          db.GetInt32(reader, 9);
        entities.WorkerServiceProvider.UserId = db.GetString(reader, 10);
        entities.WorkerServiceProvider.RoleCode =
          db.GetNullableString(reader, 11);
        entities.WorkerCaseAssignment.Populated = true;
        entities.WorkerServiceProvider.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCaseCaseAssignment()
  {
    return ReadEachInSeparateTransaction("ReadCaseCaseAssignment",
      (db, command) =>
      {
        db.SetDate(command, "effectiveDate", import.ReportEndDate.Date);
        db.SetNullableDate(
          command, "discontinueDate", import.ReportStartDate.Date);
        db.SetString(command, "numb", local.Restart.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.CaseAssignment.CasNo = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Case1.StatusDate = db.GetNullableDate(reader, 2);
        entities.Case1.CseOpenDate = db.GetNullableDate(reader, 3);
        entities.Case1.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.Case1.InterstateCaseId = db.GetNullableString(reader, 5);
        entities.Case1.NoJurisdictionCd = db.GetNullableString(reader, 6);
        entities.CaseAssignment.ReasonCode = db.GetString(reader, 7);
        entities.CaseAssignment.EffectiveDate = db.GetDate(reader, 8);
        entities.CaseAssignment.DiscontinueDate = db.GetNullableDate(reader, 9);
        entities.CaseAssignment.CreatedTimestamp = db.GetDateTime(reader, 10);
        entities.CaseAssignment.SpdId = db.GetInt32(reader, 11);
        entities.CaseAssignment.OffId = db.GetInt32(reader, 12);
        entities.CaseAssignment.OspCode = db.GetString(reader, 13);
        entities.CaseAssignment.OspDate = db.GetDate(reader, 14);
        entities.Case1.Populated = true;
        entities.CaseAssignment.Populated = true;

        return true;
      },
      () =>
      {
        entities.Case1.Populated = false;
        entities.CaseAssignment.Populated = false;
      });
  }

  private bool ReadCaseRole()
  {
    entities.ChOrArCaseRole.Populated = false;

    return Read("ReadCaseRole",
      (db, command) =>
      {
        db.SetString(command, "cspNumber1", entities.Supp.Number);
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetString(command, "cspNumber2", entities.ApCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ChOrArCaseRole.CasNumber = db.GetString(reader, 0);
        entities.ChOrArCaseRole.CspNumber = db.GetString(reader, 1);
        entities.ChOrArCaseRole.Type1 = db.GetString(reader, 2);
        entities.ChOrArCaseRole.Identifier = db.GetInt32(reader, 3);
        entities.ChOrArCaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.ChOrArCaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.ChOrArCaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ChOrArCaseRole.Type1);
      });
  }

  private IEnumerable<bool> ReadCaseRoleCsePersonCaseRoleCsePerson()
  {
    return ReadEach("ReadCaseRoleCsePersonCaseRoleCsePerson",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.ApCaseRole.CasNumber = db.GetString(reader, 0);
        entities.ApCaseRole.CspNumber = db.GetString(reader, 1);
        entities.ApCsePerson.Number = db.GetString(reader, 1);
        entities.ApCaseRole.Type1 = db.GetString(reader, 2);
        entities.ApCaseRole.Identifier = db.GetInt32(reader, 3);
        entities.ApCaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.ApCaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.ChOrArCaseRole.CasNumber = db.GetString(reader, 7);
        entities.ChOrArCaseRole.CspNumber = db.GetString(reader, 8);
        entities.ChOrArCsePerson.Number = db.GetString(reader, 8);
        entities.ChOrArCaseRole.Type1 = db.GetString(reader, 9);
        entities.ChOrArCaseRole.Identifier = db.GetInt32(reader, 10);
        entities.ChOrArCaseRole.StartDate = db.GetNullableDate(reader, 11);
        entities.ChOrArCaseRole.EndDate = db.GetNullableDate(reader, 12);

        if (Equal(entities.ApCaseRole.Type1, "CH"))
        {
          entities.ApCaseRole.DateOfEmancipation =
            db.GetNullableDate(reader, 6);
        }
        else
        {
          entities.ApCaseRole.DateOfEmancipation = null;
        }

        entities.ApCaseRole.Populated = true;
        entities.ApCsePerson.Populated = true;
        entities.ChOrArCaseRole.Populated = true;
        entities.ChOrArCsePerson.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ApCaseRole.Type1);
        CheckValid<CaseRole>("Type1", entities.ChOrArCaseRole.Type1);

        return true;
      },
      () =>
      {
        entities.ChOrArCsePerson.Populated = false;
        entities.ApCsePerson.Populated = false;
        entities.ApCaseRole.Populated = false;
        entities.ChOrArCaseRole.Populated = false;
      });
  }

  private IEnumerable<bool> ReadCollection1()
  {
    System.Diagnostics.Debug.Assert(entities.Debt.Populated);

    return ReadEach("ReadCollection1",
      (db, command) =>
      {
        db.SetDate(command, "collAdjDt", import.ReportEndDate.Date);
        db.SetDateTime(command, "createdTmst", import.ReportEndDate.Timestamp);
        db.SetInt32(command, "otyId", entities.Debt.OtyType);
        db.SetString(command, "otrType", entities.Debt.Type1);
        db.SetInt32(command, "otrId", entities.Debt.SystemGeneratedIdentifier);
        db.SetString(command, "cpaType", entities.Debt.CpaType);
        db.SetString(command, "cspNumber", entities.Debt.CspNumber);
        db.SetInt32(command, "obgId", entities.Debt.ObgGeneratedId);
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.AppliedToCode = db.GetString(reader, 1);
        entities.Collection.CollectionDt = db.GetDate(reader, 2);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 3);
        entities.Collection.ConcurrentInd = db.GetString(reader, 4);
        entities.Collection.CrtType = db.GetInt32(reader, 5);
        entities.Collection.CstId = db.GetInt32(reader, 6);
        entities.Collection.CrvId = db.GetInt32(reader, 7);
        entities.Collection.CrdId = db.GetInt32(reader, 8);
        entities.Collection.ObgId = db.GetInt32(reader, 9);
        entities.Collection.CspNumber = db.GetString(reader, 10);
        entities.Collection.CpaType = db.GetString(reader, 11);
        entities.Collection.OtrId = db.GetInt32(reader, 12);
        entities.Collection.OtrType = db.GetString(reader, 13);
        entities.Collection.OtyId = db.GetInt32(reader, 14);
        entities.Collection.CollectionAdjustmentDt = db.GetDate(reader, 15);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 16);
        entities.Collection.Amount = db.GetDecimal(reader, 17);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 18);
        entities.Collection.Populated = true;
        CheckValid<Collection>("AppliedToCode",
          entities.Collection.AppliedToCode);
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("ConcurrentInd",
          entities.Collection.ConcurrentInd);
        CheckValid<Collection>("ProgramAppliedTo",
          entities.Collection.ProgramAppliedTo);

        return true;
      },
      () =>
      {
        entities.Collection.Populated = false;
      });
  }

  private bool ReadCollection2()
  {
    entities.AfterFy.Populated = false;

    return Read("ReadCollection2",
      (db, command) =>
      {
        db.SetDateTime(command, "createdTmst", import.ReportEndDate.Timestamp);
        db.SetString(command, "cspNumber", entities.ApCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.AfterFy.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.AfterFy.AppliedToCode = db.GetString(reader, 1);
        entities.AfterFy.CollectionDt = db.GetDate(reader, 2);
        entities.AfterFy.AdjustedInd = db.GetNullableString(reader, 3);
        entities.AfterFy.ConcurrentInd = db.GetString(reader, 4);
        entities.AfterFy.CrtType = db.GetInt32(reader, 5);
        entities.AfterFy.CstId = db.GetInt32(reader, 6);
        entities.AfterFy.CrvId = db.GetInt32(reader, 7);
        entities.AfterFy.CrdId = db.GetInt32(reader, 8);
        entities.AfterFy.ObgId = db.GetInt32(reader, 9);
        entities.AfterFy.CspNumber = db.GetString(reader, 10);
        entities.AfterFy.CpaType = db.GetString(reader, 11);
        entities.AfterFy.OtrId = db.GetInt32(reader, 12);
        entities.AfterFy.OtrType = db.GetString(reader, 13);
        entities.AfterFy.OtyId = db.GetInt32(reader, 14);
        entities.AfterFy.CollectionAdjustmentDt = db.GetDate(reader, 15);
        entities.AfterFy.CreatedTmst = db.GetDateTime(reader, 16);
        entities.AfterFy.Amount = db.GetDecimal(reader, 17);
        entities.AfterFy.ProgramAppliedTo = db.GetString(reader, 18);
        entities.AfterFy.Populated = true;
        CheckValid<Collection>("AppliedToCode", entities.AfterFy.AppliedToCode);
        CheckValid<Collection>("AdjustedInd", entities.AfterFy.AdjustedInd);
        CheckValid<Collection>("ConcurrentInd", entities.AfterFy.ConcurrentInd);
        CheckValid<Collection>("ProgramAppliedTo",
          entities.AfterFy.ProgramAppliedTo);
      });
  }

  private IEnumerable<bool> ReadCollectionCollection1()
  {
    return ReadEach("ReadCollectionCollection1",
      (db, command) =>
      {
        db.SetDate(command, "collAdjDt", import.ReportEndDate.Date);
        db.
          SetDateTime(command, "createdTmst1", import.ReportStartDate.Timestamp);
        db.SetDateTime(command, "createdTmst2", import.ReportEndDate.Timestamp);
        db.SetString(command, "cspNumber", entities.ApCsePerson.Number);
        db.SetDate(command, "date", import.ReportStartDate.Date);
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.AppliedToCode = db.GetString(reader, 1);
        entities.Collection.CollectionDt = db.GetDate(reader, 2);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 3);
        entities.Collection.ConcurrentInd = db.GetString(reader, 4);
        entities.Collection.CrtType = db.GetInt32(reader, 5);
        entities.Collection.CstId = db.GetInt32(reader, 6);
        entities.Collection.CrvId = db.GetInt32(reader, 7);
        entities.Collection.CrdId = db.GetInt32(reader, 8);
        entities.Collection.ObgId = db.GetInt32(reader, 9);
        entities.Collection.CspNumber = db.GetString(reader, 10);
        entities.Collection.CpaType = db.GetString(reader, 11);
        entities.Collection.OtrId = db.GetInt32(reader, 12);
        entities.Collection.OtrType = db.GetString(reader, 13);
        entities.Collection.OtyId = db.GetInt32(reader, 14);
        entities.Collection.CollectionAdjustmentDt = db.GetDate(reader, 15);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 16);
        entities.Collection.Amount = db.GetDecimal(reader, 17);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 18);
        entities.Adjusted.SystemGeneratedIdentifier = db.GetInt32(reader, 19);
        entities.Adjusted.AdjustedInd = db.GetNullableString(reader, 20);
        entities.Adjusted.CrtType = db.GetInt32(reader, 21);
        entities.Adjusted.CstId = db.GetInt32(reader, 22);
        entities.Adjusted.CrvId = db.GetInt32(reader, 23);
        entities.Adjusted.CrdId = db.GetInt32(reader, 24);
        entities.Adjusted.ObgId = db.GetInt32(reader, 25);
        entities.Adjusted.CspNumber = db.GetString(reader, 26);
        entities.Adjusted.CpaType = db.GetString(reader, 27);
        entities.Adjusted.OtrId = db.GetInt32(reader, 28);
        entities.Adjusted.OtrType = db.GetString(reader, 29);
        entities.Adjusted.OtyId = db.GetInt32(reader, 30);
        entities.Adjusted.CollectionAdjustmentDt = db.GetDate(reader, 31);
        entities.Adjusted.CreatedTmst = db.GetDateTime(reader, 32);
        entities.Collection.Populated = true;
        entities.Adjusted.Populated = db.GetNullableInt32(reader, 19) != null;
        CheckValid<Collection>("AppliedToCode",
          entities.Collection.AppliedToCode);
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("ConcurrentInd",
          entities.Collection.ConcurrentInd);
        CheckValid<Collection>("ProgramAppliedTo",
          entities.Collection.ProgramAppliedTo);

        if (entities.Adjusted.Populated)
        {
          CheckValid<Collection>("AdjustedInd", entities.Adjusted.AdjustedInd);
        }

        return true;
      },
      () =>
      {
        entities.Collection.Populated = false;
        entities.Adjusted.Populated = false;
      });
  }

  private IEnumerable<bool> ReadCollectionCollection2()
  {
    return ReadEach("ReadCollectionCollection2",
      (db, command) =>
      {
        db.SetDate(command, "collAdjDt", import.ReportEndDate.Date);
        db.
          SetDateTime(command, "createdTmst1", import.ReportStartDate.Timestamp);
        db.SetDateTime(command, "createdTmst2", import.ReportEndDate.Timestamp);
        db.SetString(command, "cspNumber", entities.ApCsePerson.Number);
        db.SetDate(command, "date", import.ReportStartDate.Date);
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.AppliedToCode = db.GetString(reader, 1);
        entities.Collection.CollectionDt = db.GetDate(reader, 2);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 3);
        entities.Collection.ConcurrentInd = db.GetString(reader, 4);
        entities.Collection.CrtType = db.GetInt32(reader, 5);
        entities.Collection.CstId = db.GetInt32(reader, 6);
        entities.Collection.CrvId = db.GetInt32(reader, 7);
        entities.Collection.CrdId = db.GetInt32(reader, 8);
        entities.Collection.ObgId = db.GetInt32(reader, 9);
        entities.Collection.CspNumber = db.GetString(reader, 10);
        entities.Collection.CpaType = db.GetString(reader, 11);
        entities.Collection.OtrId = db.GetInt32(reader, 12);
        entities.Collection.OtrType = db.GetString(reader, 13);
        entities.Collection.OtyId = db.GetInt32(reader, 14);
        entities.Collection.CollectionAdjustmentDt = db.GetDate(reader, 15);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 16);
        entities.Collection.Amount = db.GetDecimal(reader, 17);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 18);
        entities.Adjusted.SystemGeneratedIdentifier = db.GetInt32(reader, 19);
        entities.Adjusted.AdjustedInd = db.GetNullableString(reader, 20);
        entities.Adjusted.CrtType = db.GetInt32(reader, 21);
        entities.Adjusted.CstId = db.GetInt32(reader, 22);
        entities.Adjusted.CrvId = db.GetInt32(reader, 23);
        entities.Adjusted.CrdId = db.GetInt32(reader, 24);
        entities.Adjusted.ObgId = db.GetInt32(reader, 25);
        entities.Adjusted.CspNumber = db.GetString(reader, 26);
        entities.Adjusted.CpaType = db.GetString(reader, 27);
        entities.Adjusted.OtrId = db.GetInt32(reader, 28);
        entities.Adjusted.OtrType = db.GetString(reader, 29);
        entities.Adjusted.OtyId = db.GetInt32(reader, 30);
        entities.Adjusted.CollectionAdjustmentDt = db.GetDate(reader, 31);
        entities.Adjusted.CreatedTmst = db.GetDateTime(reader, 32);
        entities.Collection.Populated = true;
        entities.Adjusted.Populated = db.GetNullableInt32(reader, 19) != null;
        CheckValid<Collection>("AppliedToCode",
          entities.Collection.AppliedToCode);
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("ConcurrentInd",
          entities.Collection.ConcurrentInd);
        CheckValid<Collection>("ProgramAppliedTo",
          entities.Collection.ProgramAppliedTo);

        if (entities.Adjusted.Populated)
        {
          CheckValid<Collection>("AdjustedInd", entities.Adjusted.AdjustedInd);
        }

        return true;
      },
      () =>
      {
        entities.Collection.Populated = false;
        entities.Adjusted.Populated = false;
      });
  }

  private IEnumerable<bool> ReadCollectionCollection3()
  {
    return ReadEach("ReadCollectionCollection3",
      (db, command) =>
      {
        db.SetDate(command, "collAdjDt", import.ReportEndDate.Date);
        db.SetString(command, "cspNumber", entities.ApCsePerson.Number);
        db.SetDateTime(command, "timestamp1", import.ReportStartDate.Timestamp);
        db.SetDateTime(command, "timestamp2", import.ReportEndDate.Timestamp);
        db.SetDate(command, "date", import.ReportStartDate.Date);
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.AppliedToCode = db.GetString(reader, 1);
        entities.Collection.CollectionDt = db.GetDate(reader, 2);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 3);
        entities.Collection.ConcurrentInd = db.GetString(reader, 4);
        entities.Collection.CrtType = db.GetInt32(reader, 5);
        entities.Collection.CstId = db.GetInt32(reader, 6);
        entities.Collection.CrvId = db.GetInt32(reader, 7);
        entities.Collection.CrdId = db.GetInt32(reader, 8);
        entities.Collection.ObgId = db.GetInt32(reader, 9);
        entities.Collection.CspNumber = db.GetString(reader, 10);
        entities.Collection.CpaType = db.GetString(reader, 11);
        entities.Collection.OtrId = db.GetInt32(reader, 12);
        entities.Collection.OtrType = db.GetString(reader, 13);
        entities.Collection.OtyId = db.GetInt32(reader, 14);
        entities.Collection.CollectionAdjustmentDt = db.GetDate(reader, 15);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 16);
        entities.Collection.Amount = db.GetDecimal(reader, 17);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 18);
        entities.Adjusted.SystemGeneratedIdentifier = db.GetInt32(reader, 19);
        entities.Adjusted.AdjustedInd = db.GetNullableString(reader, 20);
        entities.Adjusted.CrtType = db.GetInt32(reader, 21);
        entities.Adjusted.CstId = db.GetInt32(reader, 22);
        entities.Adjusted.CrvId = db.GetInt32(reader, 23);
        entities.Adjusted.CrdId = db.GetInt32(reader, 24);
        entities.Adjusted.ObgId = db.GetInt32(reader, 25);
        entities.Adjusted.CspNumber = db.GetString(reader, 26);
        entities.Adjusted.CpaType = db.GetString(reader, 27);
        entities.Adjusted.OtrId = db.GetInt32(reader, 28);
        entities.Adjusted.OtrType = db.GetString(reader, 29);
        entities.Adjusted.OtyId = db.GetInt32(reader, 30);
        entities.Adjusted.CollectionAdjustmentDt = db.GetDate(reader, 31);
        entities.Adjusted.CreatedTmst = db.GetDateTime(reader, 32);
        entities.Collection.Populated = true;
        entities.Adjusted.Populated = db.GetNullableInt32(reader, 19) != null;
        CheckValid<Collection>("AppliedToCode",
          entities.Collection.AppliedToCode);
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("ConcurrentInd",
          entities.Collection.ConcurrentInd);
        CheckValid<Collection>("ProgramAppliedTo",
          entities.Collection.ProgramAppliedTo);

        if (entities.Adjusted.Populated)
        {
          CheckValid<Collection>("AdjustedInd", entities.Adjusted.AdjustedInd);
        }

        return true;
      },
      () =>
      {
        entities.Collection.Populated = false;
        entities.Adjusted.Populated = false;
      });
  }

  private IEnumerable<bool>
    ReadCollectionCsePersonObligationTypeCaseRoleCollection1()
  {
    return ReadEach("ReadCollectionCsePersonObligationTypeCaseRoleCollection1",
      (db, command) =>
      {
        db.SetDate(command, "collAdjDt", import.ReportEndDate.Date);
        db.
          SetDateTime(command, "createdTmst1", import.ReportStartDate.Timestamp);
        db.SetDateTime(command, "createdTmst2", import.ReportEndDate.Timestamp);
        db.SetString(command, "cspNumber", entities.ApCsePerson.Number);
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetDate(command, "date", import.ReportStartDate.Date);
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.AppliedToCode = db.GetString(reader, 1);
        entities.Collection.CollectionDt = db.GetDate(reader, 2);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 3);
        entities.Collection.ConcurrentInd = db.GetString(reader, 4);
        entities.Collection.CrtType = db.GetInt32(reader, 5);
        entities.Collection.CstId = db.GetInt32(reader, 6);
        entities.Collection.CrvId = db.GetInt32(reader, 7);
        entities.Collection.CrdId = db.GetInt32(reader, 8);
        entities.Collection.ObgId = db.GetInt32(reader, 9);
        entities.Collection.CspNumber = db.GetString(reader, 10);
        entities.Collection.CpaType = db.GetString(reader, 11);
        entities.Collection.OtrId = db.GetInt32(reader, 12);
        entities.Collection.OtrType = db.GetString(reader, 13);
        entities.Collection.OtyId = db.GetInt32(reader, 14);
        entities.Collection.CollectionAdjustmentDt = db.GetDate(reader, 15);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 16);
        entities.Collection.Amount = db.GetDecimal(reader, 17);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 18);
        entities.Supp.Number = db.GetString(reader, 19);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 20);
        entities.ObligationType.Code = db.GetString(reader, 21);
        entities.ObligationType.Classification = db.GetString(reader, 22);
        entities.ChOrArCaseRole.CasNumber = db.GetString(reader, 23);
        entities.ChOrArCaseRole.CspNumber = db.GetString(reader, 24);
        entities.ChOrArCaseRole.Type1 = db.GetString(reader, 25);
        entities.ChOrArCaseRole.Identifier = db.GetInt32(reader, 26);
        entities.ChOrArCaseRole.StartDate = db.GetNullableDate(reader, 27);
        entities.ChOrArCaseRole.EndDate = db.GetNullableDate(reader, 28);
        entities.Adjusted.SystemGeneratedIdentifier = db.GetInt32(reader, 29);
        entities.Adjusted.AdjustedInd = db.GetNullableString(reader, 30);
        entities.Adjusted.CrtType = db.GetInt32(reader, 31);
        entities.Adjusted.CstId = db.GetInt32(reader, 32);
        entities.Adjusted.CrvId = db.GetInt32(reader, 33);
        entities.Adjusted.CrdId = db.GetInt32(reader, 34);
        entities.Adjusted.ObgId = db.GetInt32(reader, 35);
        entities.Adjusted.CspNumber = db.GetString(reader, 36);
        entities.Adjusted.CpaType = db.GetString(reader, 37);
        entities.Adjusted.OtrId = db.GetInt32(reader, 38);
        entities.Adjusted.OtrType = db.GetString(reader, 39);
        entities.Adjusted.OtyId = db.GetInt32(reader, 40);
        entities.Adjusted.CollectionAdjustmentDt = db.GetDate(reader, 41);
        entities.Adjusted.CreatedTmst = db.GetDateTime(reader, 42);
        entities.Collection.Populated = true;
        entities.Supp.Populated = db.GetNullableString(reader, 19) != null;
        entities.ObligationType.Populated = db.GetNullableInt32(reader, 20) != null
          ;
        entities.ChOrArCaseRole.Populated =
          db.GetNullableString(reader, 23) != null;
        entities.Adjusted.Populated = db.GetNullableInt32(reader, 29) != null;
        CheckValid<Collection>("AppliedToCode",
          entities.Collection.AppliedToCode);
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("ConcurrentInd",
          entities.Collection.ConcurrentInd);
        CheckValid<Collection>("ProgramAppliedTo",
          entities.Collection.ProgramAppliedTo);

        if (entities.ObligationType.Populated)
        {
          CheckValid<ObligationType>("Classification",
            entities.ObligationType.Classification);
        }

        if (entities.ChOrArCaseRole.Populated)
        {
          CheckValid<CaseRole>("Type1", entities.ChOrArCaseRole.Type1);
        }

        if (entities.Adjusted.Populated)
        {
          CheckValid<Collection>("AdjustedInd", entities.Adjusted.AdjustedInd);
        }

        return true;
      },
      () =>
      {
        entities.ObligationType.Populated = false;
        entities.Collection.Populated = false;
        entities.Adjusted.Populated = false;
        entities.ChOrArCaseRole.Populated = false;
        entities.Supp.Populated = false;
      });
  }

  private IEnumerable<bool>
    ReadCollectionCsePersonObligationTypeCaseRoleCollection2()
  {
    return ReadEach("ReadCollectionCsePersonObligationTypeCaseRoleCollection2",
      (db, command) =>
      {
        db.SetDate(command, "collAdjDt", import.ReportEndDate.Date);
        db.
          SetDateTime(command, "createdTmst1", import.ReportStartDate.Timestamp);
        db.SetDateTime(command, "createdTmst2", import.ReportEndDate.Timestamp);
        db.SetString(command, "cspNumber", entities.ApCsePerson.Number);
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetDate(command, "date", import.ReportStartDate.Date);
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.AppliedToCode = db.GetString(reader, 1);
        entities.Collection.CollectionDt = db.GetDate(reader, 2);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 3);
        entities.Collection.ConcurrentInd = db.GetString(reader, 4);
        entities.Collection.CrtType = db.GetInt32(reader, 5);
        entities.Collection.CstId = db.GetInt32(reader, 6);
        entities.Collection.CrvId = db.GetInt32(reader, 7);
        entities.Collection.CrdId = db.GetInt32(reader, 8);
        entities.Collection.ObgId = db.GetInt32(reader, 9);
        entities.Collection.CspNumber = db.GetString(reader, 10);
        entities.Collection.CpaType = db.GetString(reader, 11);
        entities.Collection.OtrId = db.GetInt32(reader, 12);
        entities.Collection.OtrType = db.GetString(reader, 13);
        entities.Collection.OtyId = db.GetInt32(reader, 14);
        entities.Collection.CollectionAdjustmentDt = db.GetDate(reader, 15);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 16);
        entities.Collection.Amount = db.GetDecimal(reader, 17);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 18);
        entities.Supp.Number = db.GetString(reader, 19);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 20);
        entities.ObligationType.Code = db.GetString(reader, 21);
        entities.ObligationType.Classification = db.GetString(reader, 22);
        entities.ChOrArCaseRole.CasNumber = db.GetString(reader, 23);
        entities.ChOrArCaseRole.CspNumber = db.GetString(reader, 24);
        entities.ChOrArCaseRole.Type1 = db.GetString(reader, 25);
        entities.ChOrArCaseRole.Identifier = db.GetInt32(reader, 26);
        entities.ChOrArCaseRole.StartDate = db.GetNullableDate(reader, 27);
        entities.ChOrArCaseRole.EndDate = db.GetNullableDate(reader, 28);
        entities.Adjusted.SystemGeneratedIdentifier = db.GetInt32(reader, 29);
        entities.Adjusted.AdjustedInd = db.GetNullableString(reader, 30);
        entities.Adjusted.CrtType = db.GetInt32(reader, 31);
        entities.Adjusted.CstId = db.GetInt32(reader, 32);
        entities.Adjusted.CrvId = db.GetInt32(reader, 33);
        entities.Adjusted.CrdId = db.GetInt32(reader, 34);
        entities.Adjusted.ObgId = db.GetInt32(reader, 35);
        entities.Adjusted.CspNumber = db.GetString(reader, 36);
        entities.Adjusted.CpaType = db.GetString(reader, 37);
        entities.Adjusted.OtrId = db.GetInt32(reader, 38);
        entities.Adjusted.OtrType = db.GetString(reader, 39);
        entities.Adjusted.OtyId = db.GetInt32(reader, 40);
        entities.Adjusted.CollectionAdjustmentDt = db.GetDate(reader, 41);
        entities.Adjusted.CreatedTmst = db.GetDateTime(reader, 42);
        entities.Collection.Populated = true;
        entities.Supp.Populated = db.GetNullableString(reader, 19) != null;
        entities.ObligationType.Populated = db.GetNullableInt32(reader, 20) != null
          ;
        entities.ChOrArCaseRole.Populated =
          db.GetNullableString(reader, 23) != null;
        entities.Adjusted.Populated = db.GetNullableInt32(reader, 29) != null;
        CheckValid<Collection>("AppliedToCode",
          entities.Collection.AppliedToCode);
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("ConcurrentInd",
          entities.Collection.ConcurrentInd);
        CheckValid<Collection>("ProgramAppliedTo",
          entities.Collection.ProgramAppliedTo);

        if (entities.ObligationType.Populated)
        {
          CheckValid<ObligationType>("Classification",
            entities.ObligationType.Classification);
        }

        if (entities.ChOrArCaseRole.Populated)
        {
          CheckValid<CaseRole>("Type1", entities.ChOrArCaseRole.Type1);
        }

        if (entities.Adjusted.Populated)
        {
          CheckValid<Collection>("AdjustedInd", entities.Adjusted.AdjustedInd);
        }

        return true;
      },
      () =>
      {
        entities.ObligationType.Populated = false;
        entities.Collection.Populated = false;
        entities.Adjusted.Populated = false;
        entities.ChOrArCaseRole.Populated = false;
        entities.Supp.Populated = false;
      });
  }

  private IEnumerable<bool>
    ReadCollectionCsePersonObligationTypeCaseRoleCollection3()
  {
    return ReadEach("ReadCollectionCsePersonObligationTypeCaseRoleCollection3",
      (db, command) =>
      {
        db.SetDate(command, "collAdjDt", import.ReportEndDate.Date);
        db.SetString(command, "cspNumber", entities.ApCsePerson.Number);
        db.SetDateTime(command, "timestamp1", import.ReportStartDate.Timestamp);
        db.SetDateTime(command, "timestamp2", import.ReportEndDate.Timestamp);
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetDate(command, "date", import.ReportStartDate.Date);
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.AppliedToCode = db.GetString(reader, 1);
        entities.Collection.CollectionDt = db.GetDate(reader, 2);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 3);
        entities.Collection.ConcurrentInd = db.GetString(reader, 4);
        entities.Collection.CrtType = db.GetInt32(reader, 5);
        entities.Collection.CstId = db.GetInt32(reader, 6);
        entities.Collection.CrvId = db.GetInt32(reader, 7);
        entities.Collection.CrdId = db.GetInt32(reader, 8);
        entities.Collection.ObgId = db.GetInt32(reader, 9);
        entities.Collection.CspNumber = db.GetString(reader, 10);
        entities.Collection.CpaType = db.GetString(reader, 11);
        entities.Collection.OtrId = db.GetInt32(reader, 12);
        entities.Collection.OtrType = db.GetString(reader, 13);
        entities.Collection.OtyId = db.GetInt32(reader, 14);
        entities.Collection.CollectionAdjustmentDt = db.GetDate(reader, 15);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 16);
        entities.Collection.Amount = db.GetDecimal(reader, 17);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 18);
        entities.Supp.Number = db.GetString(reader, 19);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 20);
        entities.ObligationType.Code = db.GetString(reader, 21);
        entities.ObligationType.Classification = db.GetString(reader, 22);
        entities.ChOrArCaseRole.CasNumber = db.GetString(reader, 23);
        entities.ChOrArCaseRole.CspNumber = db.GetString(reader, 24);
        entities.ChOrArCaseRole.Type1 = db.GetString(reader, 25);
        entities.ChOrArCaseRole.Identifier = db.GetInt32(reader, 26);
        entities.ChOrArCaseRole.StartDate = db.GetNullableDate(reader, 27);
        entities.ChOrArCaseRole.EndDate = db.GetNullableDate(reader, 28);
        entities.Adjusted.SystemGeneratedIdentifier = db.GetInt32(reader, 29);
        entities.Adjusted.AdjustedInd = db.GetNullableString(reader, 30);
        entities.Adjusted.CrtType = db.GetInt32(reader, 31);
        entities.Adjusted.CstId = db.GetInt32(reader, 32);
        entities.Adjusted.CrvId = db.GetInt32(reader, 33);
        entities.Adjusted.CrdId = db.GetInt32(reader, 34);
        entities.Adjusted.ObgId = db.GetInt32(reader, 35);
        entities.Adjusted.CspNumber = db.GetString(reader, 36);
        entities.Adjusted.CpaType = db.GetString(reader, 37);
        entities.Adjusted.OtrId = db.GetInt32(reader, 38);
        entities.Adjusted.OtrType = db.GetString(reader, 39);
        entities.Adjusted.OtyId = db.GetInt32(reader, 40);
        entities.Adjusted.CollectionAdjustmentDt = db.GetDate(reader, 41);
        entities.Adjusted.CreatedTmst = db.GetDateTime(reader, 42);
        entities.Collection.Populated = true;
        entities.Supp.Populated = db.GetNullableString(reader, 19) != null;
        entities.ObligationType.Populated = db.GetNullableInt32(reader, 20) != null
          ;
        entities.ChOrArCaseRole.Populated =
          db.GetNullableString(reader, 23) != null;
        entities.Adjusted.Populated = db.GetNullableInt32(reader, 29) != null;
        CheckValid<Collection>("AppliedToCode",
          entities.Collection.AppliedToCode);
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("ConcurrentInd",
          entities.Collection.ConcurrentInd);
        CheckValid<Collection>("ProgramAppliedTo",
          entities.Collection.ProgramAppliedTo);

        if (entities.ObligationType.Populated)
        {
          CheckValid<ObligationType>("Classification",
            entities.ObligationType.Classification);
        }

        if (entities.ChOrArCaseRole.Populated)
        {
          CheckValid<CaseRole>("Type1", entities.ChOrArCaseRole.Type1);
        }

        if (entities.Adjusted.Populated)
        {
          CheckValid<Collection>("AdjustedInd", entities.Adjusted.AdjustedInd);
        }

        return true;
      },
      () =>
      {
        entities.ObligationType.Populated = false;
        entities.Collection.Populated = false;
        entities.Adjusted.Populated = false;
        entities.ChOrArCaseRole.Populated = false;
        entities.Supp.Populated = false;
      });
  }

  private IEnumerable<bool> ReadCollectionObligationTypeCollection()
  {
    return ReadEach("ReadCollectionObligationTypeCollection",
      (db, command) =>
      {
        db.SetDate(command, "collAdjDt", import.ReportEndDate.Date);
        db.
          SetDateTime(command, "createdTmst1", import.ReportStartDate.Timestamp);
        db.SetDateTime(command, "createdTmst2", import.ReportEndDate.Timestamp);
        db.SetString(command, "cspNumber", entities.ApCsePerson.Number);
        db.SetNullableString(
          command, "cspSupNumber", entities.ChOrArCsePerson.Number);
        db.SetDate(command, "date", import.ReportStartDate.Date);
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.AppliedToCode = db.GetString(reader, 1);
        entities.Collection.CollectionDt = db.GetDate(reader, 2);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 3);
        entities.Collection.ConcurrentInd = db.GetString(reader, 4);
        entities.Collection.CrtType = db.GetInt32(reader, 5);
        entities.Collection.CstId = db.GetInt32(reader, 6);
        entities.Collection.CrvId = db.GetInt32(reader, 7);
        entities.Collection.CrdId = db.GetInt32(reader, 8);
        entities.Collection.ObgId = db.GetInt32(reader, 9);
        entities.Collection.CspNumber = db.GetString(reader, 10);
        entities.Collection.CpaType = db.GetString(reader, 11);
        entities.Collection.OtrId = db.GetInt32(reader, 12);
        entities.Collection.OtrType = db.GetString(reader, 13);
        entities.Collection.OtyId = db.GetInt32(reader, 14);
        entities.Collection.CollectionAdjustmentDt = db.GetDate(reader, 15);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 16);
        entities.Collection.Amount = db.GetDecimal(reader, 17);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 18);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 19);
        entities.ObligationType.Code = db.GetString(reader, 20);
        entities.ObligationType.Classification = db.GetString(reader, 21);
        entities.Adjusted.SystemGeneratedIdentifier = db.GetInt32(reader, 22);
        entities.Adjusted.AdjustedInd = db.GetNullableString(reader, 23);
        entities.Adjusted.CrtType = db.GetInt32(reader, 24);
        entities.Adjusted.CstId = db.GetInt32(reader, 25);
        entities.Adjusted.CrvId = db.GetInt32(reader, 26);
        entities.Adjusted.CrdId = db.GetInt32(reader, 27);
        entities.Adjusted.ObgId = db.GetInt32(reader, 28);
        entities.Adjusted.CspNumber = db.GetString(reader, 29);
        entities.Adjusted.CpaType = db.GetString(reader, 30);
        entities.Adjusted.OtrId = db.GetInt32(reader, 31);
        entities.Adjusted.OtrType = db.GetString(reader, 32);
        entities.Adjusted.OtyId = db.GetInt32(reader, 33);
        entities.Adjusted.CollectionAdjustmentDt = db.GetDate(reader, 34);
        entities.Adjusted.CreatedTmst = db.GetDateTime(reader, 35);
        entities.Collection.Populated = true;
        entities.ObligationType.Populated = db.GetNullableInt32(reader, 19) != null
          ;
        entities.Adjusted.Populated = db.GetNullableInt32(reader, 22) != null;
        CheckValid<Collection>("AppliedToCode",
          entities.Collection.AppliedToCode);
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("ConcurrentInd",
          entities.Collection.ConcurrentInd);
        CheckValid<Collection>("ProgramAppliedTo",
          entities.Collection.ProgramAppliedTo);

        if (entities.ObligationType.Populated)
        {
          CheckValid<ObligationType>("Classification",
            entities.ObligationType.Classification);
        }

        if (entities.Adjusted.Populated)
        {
          CheckValid<Collection>("AdjustedInd", entities.Adjusted.AdjustedInd);
        }

        return true;
      },
      () =>
      {
        entities.ObligationType.Populated = false;
        entities.Collection.Populated = false;
        entities.Adjusted.Populated = false;
      });
  }

  private IEnumerable<bool> ReadCseOrganization()
  {
    return ReadEach("ReadCseOrganization",
      null,
      (db, reader) =>
      {
        entities.CseOrganization.Code = db.GetString(reader, 0);
        entities.CseOrganization.Type1 = db.GetString(reader, 1);
        entities.CseOrganization.Populated = true;

        return true;
      },
      () =>
      {
        entities.CseOrganization.Populated = false;
      });
  }

  private IEnumerable<bool> ReadCsePerson()
  {
    return ReadEach("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.ApCsePerson.Number = db.GetString(reader, 0);
        entities.ApCsePerson.Populated = true;

        return true;
      },
      () =>
      {
        entities.ApCsePerson.Populated = false;
      });
  }

  private IEnumerable<bool> ReadCsePersonCollectionCsePersonObligationType1()
  {
    return ReadEach("ReadCsePersonCollectionCsePersonObligationType1",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetDate(command, "collAdjDt", import.ReportEndDate.Date);
        db.
          SetDateTime(command, "createdTmst1", import.ReportStartDate.Timestamp);
        db.SetDateTime(command, "createdTmst2", import.ReportEndDate.Timestamp);
        db.SetDate(command, "date", import.ReportStartDate.Date);
      },
      (db, reader) =>
      {
        entities.ApCsePerson.Number = db.GetString(reader, 0);
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 1);
        entities.Collection.AppliedToCode = db.GetString(reader, 2);
        entities.Collection.CollectionDt = db.GetDate(reader, 3);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 4);
        entities.Collection.ConcurrentInd = db.GetString(reader, 5);
        entities.Collection.CrtType = db.GetInt32(reader, 6);
        entities.Collection.CstId = db.GetInt32(reader, 7);
        entities.Collection.CrvId = db.GetInt32(reader, 8);
        entities.Collection.CrdId = db.GetInt32(reader, 9);
        entities.Collection.ObgId = db.GetInt32(reader, 10);
        entities.Collection.CspNumber = db.GetString(reader, 11);
        entities.Collection.CpaType = db.GetString(reader, 12);
        entities.Collection.OtrId = db.GetInt32(reader, 13);
        entities.Collection.OtrType = db.GetString(reader, 14);
        entities.Collection.OtyId = db.GetInt32(reader, 15);
        entities.Collection.CollectionAdjustmentDt = db.GetDate(reader, 16);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 17);
        entities.Collection.Amount = db.GetDecimal(reader, 18);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 19);
        entities.Supp.Number = db.GetString(reader, 20);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 21);
        entities.ObligationType.Code = db.GetString(reader, 22);
        entities.ObligationType.Classification = db.GetString(reader, 23);
        entities.ChOrArCaseRole.CasNumber = db.GetString(reader, 24);
        entities.ChOrArCaseRole.CspNumber = db.GetString(reader, 25);
        entities.ChOrArCaseRole.Type1 = db.GetString(reader, 26);
        entities.ChOrArCaseRole.Identifier = db.GetInt32(reader, 27);
        entities.ChOrArCaseRole.StartDate = db.GetNullableDate(reader, 28);
        entities.ChOrArCaseRole.EndDate = db.GetNullableDate(reader, 29);
        entities.Adjusted.SystemGeneratedIdentifier = db.GetInt32(reader, 30);
        entities.Adjusted.AdjustedInd = db.GetNullableString(reader, 31);
        entities.Adjusted.CrtType = db.GetInt32(reader, 32);
        entities.Adjusted.CstId = db.GetInt32(reader, 33);
        entities.Adjusted.CrvId = db.GetInt32(reader, 34);
        entities.Adjusted.CrdId = db.GetInt32(reader, 35);
        entities.Adjusted.ObgId = db.GetInt32(reader, 36);
        entities.Adjusted.CspNumber = db.GetString(reader, 37);
        entities.Adjusted.CpaType = db.GetString(reader, 38);
        entities.Adjusted.OtrId = db.GetInt32(reader, 39);
        entities.Adjusted.OtrType = db.GetString(reader, 40);
        entities.Adjusted.OtyId = db.GetInt32(reader, 41);
        entities.Adjusted.CollectionAdjustmentDt = db.GetDate(reader, 42);
        entities.Adjusted.CreatedTmst = db.GetDateTime(reader, 43);
        entities.ApCsePerson.Populated = true;
        entities.Collection.Populated = true;
        entities.Supp.Populated = db.GetNullableString(reader, 20) != null;
        entities.ObligationType.Populated = db.GetNullableInt32(reader, 21) != null
          ;
        entities.ChOrArCaseRole.Populated =
          db.GetNullableString(reader, 24) != null;
        entities.Adjusted.Populated = db.GetNullableInt32(reader, 30) != null;
        CheckValid<Collection>("AppliedToCode",
          entities.Collection.AppliedToCode);
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("ConcurrentInd",
          entities.Collection.ConcurrentInd);
        CheckValid<Collection>("ProgramAppliedTo",
          entities.Collection.ProgramAppliedTo);

        if (entities.ObligationType.Populated)
        {
          CheckValid<ObligationType>("Classification",
            entities.ObligationType.Classification);
        }

        if (entities.ChOrArCaseRole.Populated)
        {
          CheckValid<CaseRole>("Type1", entities.ChOrArCaseRole.Type1);
        }

        if (entities.Adjusted.Populated)
        {
          CheckValid<Collection>("AdjustedInd", entities.Adjusted.AdjustedInd);
        }

        return true;
      },
      () =>
      {
        entities.ObligationType.Populated = false;
        entities.Collection.Populated = false;
        entities.ApCsePerson.Populated = false;
        entities.Adjusted.Populated = false;
        entities.ChOrArCaseRole.Populated = false;
        entities.Supp.Populated = false;
      });
  }

  private IEnumerable<bool> ReadCsePersonCollectionCsePersonObligationType2()
  {
    return ReadEach("ReadCsePersonCollectionCsePersonObligationType2",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetDate(command, "collAdjDt", import.ReportEndDate.Date);
        db.SetDateTime(command, "timestamp1", import.ReportStartDate.Timestamp);
        db.SetDateTime(command, "timestamp2", import.ReportEndDate.Timestamp);
        db.SetDate(command, "date", import.ReportStartDate.Date);
      },
      (db, reader) =>
      {
        entities.ApCsePerson.Number = db.GetString(reader, 0);
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 1);
        entities.Collection.AppliedToCode = db.GetString(reader, 2);
        entities.Collection.CollectionDt = db.GetDate(reader, 3);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 4);
        entities.Collection.ConcurrentInd = db.GetString(reader, 5);
        entities.Collection.CrtType = db.GetInt32(reader, 6);
        entities.Collection.CstId = db.GetInt32(reader, 7);
        entities.Collection.CrvId = db.GetInt32(reader, 8);
        entities.Collection.CrdId = db.GetInt32(reader, 9);
        entities.Collection.ObgId = db.GetInt32(reader, 10);
        entities.Collection.CspNumber = db.GetString(reader, 11);
        entities.Collection.CpaType = db.GetString(reader, 12);
        entities.Collection.OtrId = db.GetInt32(reader, 13);
        entities.Collection.OtrType = db.GetString(reader, 14);
        entities.Collection.OtyId = db.GetInt32(reader, 15);
        entities.Collection.CollectionAdjustmentDt = db.GetDate(reader, 16);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 17);
        entities.Collection.Amount = db.GetDecimal(reader, 18);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 19);
        entities.Supp.Number = db.GetString(reader, 20);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 21);
        entities.ObligationType.Code = db.GetString(reader, 22);
        entities.ObligationType.Classification = db.GetString(reader, 23);
        entities.ChOrArCaseRole.CasNumber = db.GetString(reader, 24);
        entities.ChOrArCaseRole.CspNumber = db.GetString(reader, 25);
        entities.ChOrArCaseRole.Type1 = db.GetString(reader, 26);
        entities.ChOrArCaseRole.Identifier = db.GetInt32(reader, 27);
        entities.ChOrArCaseRole.StartDate = db.GetNullableDate(reader, 28);
        entities.ChOrArCaseRole.EndDate = db.GetNullableDate(reader, 29);
        entities.Adjusted.SystemGeneratedIdentifier = db.GetInt32(reader, 30);
        entities.Adjusted.AdjustedInd = db.GetNullableString(reader, 31);
        entities.Adjusted.CrtType = db.GetInt32(reader, 32);
        entities.Adjusted.CstId = db.GetInt32(reader, 33);
        entities.Adjusted.CrvId = db.GetInt32(reader, 34);
        entities.Adjusted.CrdId = db.GetInt32(reader, 35);
        entities.Adjusted.ObgId = db.GetInt32(reader, 36);
        entities.Adjusted.CspNumber = db.GetString(reader, 37);
        entities.Adjusted.CpaType = db.GetString(reader, 38);
        entities.Adjusted.OtrId = db.GetInt32(reader, 39);
        entities.Adjusted.OtrType = db.GetString(reader, 40);
        entities.Adjusted.OtyId = db.GetInt32(reader, 41);
        entities.Adjusted.CollectionAdjustmentDt = db.GetDate(reader, 42);
        entities.Adjusted.CreatedTmst = db.GetDateTime(reader, 43);
        entities.ApCsePerson.Populated = true;
        entities.Collection.Populated = true;
        entities.Supp.Populated = db.GetNullableString(reader, 20) != null;
        entities.ObligationType.Populated = db.GetNullableInt32(reader, 21) != null
          ;
        entities.ChOrArCaseRole.Populated =
          db.GetNullableString(reader, 24) != null;
        entities.Adjusted.Populated = db.GetNullableInt32(reader, 30) != null;
        CheckValid<Collection>("AppliedToCode",
          entities.Collection.AppliedToCode);
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("ConcurrentInd",
          entities.Collection.ConcurrentInd);
        CheckValid<Collection>("ProgramAppliedTo",
          entities.Collection.ProgramAppliedTo);

        if (entities.ObligationType.Populated)
        {
          CheckValid<ObligationType>("Classification",
            entities.ObligationType.Classification);
        }

        if (entities.ChOrArCaseRole.Populated)
        {
          CheckValid<CaseRole>("Type1", entities.ChOrArCaseRole.Type1);
        }

        if (entities.Adjusted.Populated)
        {
          CheckValid<Collection>("AdjustedInd", entities.Adjusted.AdjustedInd);
        }

        return true;
      },
      () =>
      {
        entities.ObligationType.Populated = false;
        entities.Collection.Populated = false;
        entities.ApCsePerson.Populated = false;
        entities.Adjusted.Populated = false;
        entities.ChOrArCaseRole.Populated = false;
        entities.Supp.Populated = false;
      });
  }

  private IEnumerable<bool> ReadDashboardStagingPriority1()
  {
    return ReadEach("ReadDashboardStagingPriority1",
      (db, command) =>
      {
        db.SetInt32(
          command, "reportMonth", import.DashboardAuditData.ReportMonth);
      },
      (db, reader) =>
      {
        entities.DashboardStagingPriority12.ReportMonth =
          db.GetInt32(reader, 0);
        entities.DashboardStagingPriority12.ReportLevel =
          db.GetString(reader, 1);
        entities.DashboardStagingPriority12.ReportLevelId =
          db.GetString(reader, 2);
        entities.DashboardStagingPriority12.AsOfDate =
          db.GetNullableDate(reader, 3);
        entities.DashboardStagingPriority12.CasesPayingArrearsNumerator =
          db.GetNullableInt32(reader, 4);
        entities.DashboardStagingPriority12.CasesPayingArrearsDenominator =
          db.GetNullableInt32(reader, 5);
        entities.DashboardStagingPriority12.CasesPayingArrearsPercent =
          db.GetNullableDecimal(reader, 6);
        entities.DashboardStagingPriority12.CasesPayingArrearsRank =
          db.GetNullableInt32(reader, 7);
        entities.DashboardStagingPriority12.ContractorNumber =
          db.GetNullableString(reader, 8);
        entities.DashboardStagingPriority12.PrvYrCasesPaidArrearsNumtr =
          db.GetNullableInt32(reader, 9);
        entities.DashboardStagingPriority12.PrvYrCasesPaidArrearsDenom =
          db.GetNullableInt32(reader, 10);
        entities.DashboardStagingPriority12.CasesPayArrearsPrvYrPct =
          db.GetNullableDecimal(reader, 11);
        entities.DashboardStagingPriority12.PctChgBtwenYrsCasesPayArrs =
          db.GetNullableDecimal(reader, 12);
        entities.DashboardStagingPriority12.Populated = true;

        return true;
      },
      () =>
      {
        entities.DashboardStagingPriority12.Populated = false;
      });
  }

  private bool ReadDashboardStagingPriority10()
  {
    entities.DashboardStagingPriority12.Populated = false;

    return Read("ReadDashboardStagingPriority10",
      (db, command) =>
      {
        db.SetInt32(
          command, "reportMonth",
          local.Contractor.Item.Gcontractor.ReportMonth);
        db.SetString(
          command, "reportLevel",
          local.Contractor.Item.Gcontractor.ReportLevel);
        db.SetString(
          command, "reportLevelId",
          local.Contractor.Item.Gcontractor.ReportLevelId);
      },
      (db, reader) =>
      {
        entities.DashboardStagingPriority12.ReportMonth =
          db.GetInt32(reader, 0);
        entities.DashboardStagingPriority12.ReportLevel =
          db.GetString(reader, 1);
        entities.DashboardStagingPriority12.ReportLevelId =
          db.GetString(reader, 2);
        entities.DashboardStagingPriority12.AsOfDate =
          db.GetNullableDate(reader, 3);
        entities.DashboardStagingPriority12.CasesPayingArrearsNumerator =
          db.GetNullableInt32(reader, 4);
        entities.DashboardStagingPriority12.CasesPayingArrearsDenominator =
          db.GetNullableInt32(reader, 5);
        entities.DashboardStagingPriority12.CasesPayingArrearsPercent =
          db.GetNullableDecimal(reader, 6);
        entities.DashboardStagingPriority12.CasesPayingArrearsRank =
          db.GetNullableInt32(reader, 7);
        entities.DashboardStagingPriority12.ContractorNumber =
          db.GetNullableString(reader, 8);
        entities.DashboardStagingPriority12.PrvYrCasesPaidArrearsNumtr =
          db.GetNullableInt32(reader, 9);
        entities.DashboardStagingPriority12.PrvYrCasesPaidArrearsDenom =
          db.GetNullableInt32(reader, 10);
        entities.DashboardStagingPriority12.CasesPayArrearsPrvYrPct =
          db.GetNullableDecimal(reader, 11);
        entities.DashboardStagingPriority12.PctChgBtwenYrsCasesPayArrs =
          db.GetNullableDecimal(reader, 12);
        entities.DashboardStagingPriority12.Populated = true;
      });
  }

  private IEnumerable<bool> ReadDashboardStagingPriority11()
  {
    return ReadEach("ReadDashboardStagingPriority11",
      (db, command) =>
      {
        db.SetInt32(
          command, "reportMonth", import.DashboardAuditData.ReportMonth);
      },
      (db, reader) =>
      {
        entities.DashboardStagingPriority12.ReportMonth =
          db.GetInt32(reader, 0);
        entities.DashboardStagingPriority12.ReportLevel =
          db.GetString(reader, 1);
        entities.DashboardStagingPriority12.ReportLevelId =
          db.GetString(reader, 2);
        entities.DashboardStagingPriority12.AsOfDate =
          db.GetNullableDate(reader, 3);
        entities.DashboardStagingPriority12.CasesPayingArrearsNumerator =
          db.GetNullableInt32(reader, 4);
        entities.DashboardStagingPriority12.CasesPayingArrearsDenominator =
          db.GetNullableInt32(reader, 5);
        entities.DashboardStagingPriority12.CasesPayingArrearsPercent =
          db.GetNullableDecimal(reader, 6);
        entities.DashboardStagingPriority12.CasesPayingArrearsRank =
          db.GetNullableInt32(reader, 7);
        entities.DashboardStagingPriority12.ContractorNumber =
          db.GetNullableString(reader, 8);
        entities.DashboardStagingPriority12.PrvYrCasesPaidArrearsNumtr =
          db.GetNullableInt32(reader, 9);
        entities.DashboardStagingPriority12.PrvYrCasesPaidArrearsDenom =
          db.GetNullableInt32(reader, 10);
        entities.DashboardStagingPriority12.CasesPayArrearsPrvYrPct =
          db.GetNullableDecimal(reader, 11);
        entities.DashboardStagingPriority12.PctChgBtwenYrsCasesPayArrs =
          db.GetNullableDecimal(reader, 12);
        entities.DashboardStagingPriority12.Populated = true;

        return true;
      },
      () =>
      {
        entities.DashboardStagingPriority12.Populated = false;
      });
  }

  private bool ReadDashboardStagingPriority13()
  {
    entities.PreviousYear.Populated = false;

    return Read("ReadDashboardStagingPriority13",
      (db, command) =>
      {
        db.SetString(
          command, "reportLevel",
          entities.DashboardStagingPriority12.ReportLevel);
        db.SetString(
          command, "reportLevelId",
          entities.DashboardStagingPriority12.ReportLevelId);
        db.SetInt32(command, "reportMonth", local.PreviousYear.ReportMonth);
      },
      (db, reader) =>
      {
        entities.PreviousYear.ReportMonth = db.GetInt32(reader, 0);
        entities.PreviousYear.ReportLevel = db.GetString(reader, 1);
        entities.PreviousYear.ReportLevelId = db.GetString(reader, 2);
        entities.PreviousYear.CasesPayingArrearsNumerator =
          db.GetNullableInt32(reader, 3);
        entities.PreviousYear.CasesPayingArrearsDenominator =
          db.GetNullableInt32(reader, 4);
        entities.PreviousYear.Populated = true;
      });
  }

  private IEnumerable<bool> ReadDashboardStagingPriority14()
  {
    return ReadEach("ReadDashboardStagingPriority14",
      (db, command) =>
      {
        db.SetInt32(
          command, "reportMonth", import.DashboardAuditData.ReportMonth);
      },
      (db, reader) =>
      {
        entities.DashboardStagingPriority35.ReportMonth =
          db.GetInt32(reader, 0);
        entities.DashboardStagingPriority35.ReportLevel =
          db.GetString(reader, 1);
        entities.DashboardStagingPriority35.ReportLevelId =
          db.GetString(reader, 2);
        entities.DashboardStagingPriority35.AsOfDate =
          db.GetNullableDate(reader, 3);
        entities.DashboardStagingPriority35.CasesPayingArrearsDenominator =
          db.GetNullableInt32(reader, 4);
        entities.DashboardStagingPriority35.CasesPayingArrearsNumerator =
          db.GetNullableInt32(reader, 5);
        entities.DashboardStagingPriority35.CasesPayingArrearsPercent =
          db.GetNullableDecimal(reader, 6);
        entities.DashboardStagingPriority35.CasesPayingArrearsRank =
          db.GetNullableInt32(reader, 7);
        entities.DashboardStagingPriority35.Populated = true;

        return true;
      },
      () =>
      {
        entities.DashboardStagingPriority35.Populated = false;
      });
  }

  private IEnumerable<bool> ReadDashboardStagingPriority15()
  {
    return ReadEach("ReadDashboardStagingPriority15",
      (db, command) =>
      {
        db.SetInt32(
          command, "reportMonth", import.DashboardAuditData.ReportMonth);
      },
      (db, reader) =>
      {
        entities.DashboardStagingPriority35.ReportMonth =
          db.GetInt32(reader, 0);
        entities.DashboardStagingPriority35.ReportLevel =
          db.GetString(reader, 1);
        entities.DashboardStagingPriority35.ReportLevelId =
          db.GetString(reader, 2);
        entities.DashboardStagingPriority35.AsOfDate =
          db.GetNullableDate(reader, 3);
        entities.DashboardStagingPriority35.CasesPayingArrearsDenominator =
          db.GetNullableInt32(reader, 4);
        entities.DashboardStagingPriority35.CasesPayingArrearsNumerator =
          db.GetNullableInt32(reader, 5);
        entities.DashboardStagingPriority35.CasesPayingArrearsPercent =
          db.GetNullableDecimal(reader, 6);
        entities.DashboardStagingPriority35.CasesPayingArrearsRank =
          db.GetNullableInt32(reader, 7);
        entities.DashboardStagingPriority35.Populated = true;

        return true;
      },
      () =>
      {
        entities.DashboardStagingPriority35.Populated = false;
      });
  }

  private IEnumerable<bool> ReadDashboardStagingPriority16()
  {
    return ReadEach("ReadDashboardStagingPriority16",
      (db, command) =>
      {
        db.SetInt32(
          command, "reportMonth", import.DashboardAuditData.ReportMonth);
      },
      (db, reader) =>
      {
        entities.DashboardStagingPriority12.ReportMonth =
          db.GetInt32(reader, 0);
        entities.DashboardStagingPriority12.ReportLevel =
          db.GetString(reader, 1);
        entities.DashboardStagingPriority12.ReportLevelId =
          db.GetString(reader, 2);
        entities.DashboardStagingPriority12.AsOfDate =
          db.GetNullableDate(reader, 3);
        entities.DashboardStagingPriority12.CasesPayingArrearsNumerator =
          db.GetNullableInt32(reader, 4);
        entities.DashboardStagingPriority12.CasesPayingArrearsDenominator =
          db.GetNullableInt32(reader, 5);
        entities.DashboardStagingPriority12.CasesPayingArrearsPercent =
          db.GetNullableDecimal(reader, 6);
        entities.DashboardStagingPriority12.CasesPayingArrearsRank =
          db.GetNullableInt32(reader, 7);
        entities.DashboardStagingPriority12.ContractorNumber =
          db.GetNullableString(reader, 8);
        entities.DashboardStagingPriority12.PrvYrCasesPaidArrearsNumtr =
          db.GetNullableInt32(reader, 9);
        entities.DashboardStagingPriority12.PrvYrCasesPaidArrearsDenom =
          db.GetNullableInt32(reader, 10);
        entities.DashboardStagingPriority12.CasesPayArrearsPrvYrPct =
          db.GetNullableDecimal(reader, 11);
        entities.DashboardStagingPriority12.PctChgBtwenYrsCasesPayArrs =
          db.GetNullableDecimal(reader, 12);
        entities.DashboardStagingPriority12.Populated = true;

        return true;
      },
      () =>
      {
        entities.DashboardStagingPriority12.Populated = false;
      });
  }

  private IEnumerable<bool> ReadDashboardStagingPriority17()
  {
    return ReadEach("ReadDashboardStagingPriority17",
      (db, command) =>
      {
        db.SetInt32(
          command, "reportMonth", import.DashboardAuditData.ReportMonth);
      },
      (db, reader) =>
      {
        entities.DashboardStagingPriority12.ReportMonth =
          db.GetInt32(reader, 0);
        entities.DashboardStagingPriority12.ReportLevel =
          db.GetString(reader, 1);
        entities.DashboardStagingPriority12.ReportLevelId =
          db.GetString(reader, 2);
        entities.DashboardStagingPriority12.AsOfDate =
          db.GetNullableDate(reader, 3);
        entities.DashboardStagingPriority12.CasesPayingArrearsNumerator =
          db.GetNullableInt32(reader, 4);
        entities.DashboardStagingPriority12.CasesPayingArrearsDenominator =
          db.GetNullableInt32(reader, 5);
        entities.DashboardStagingPriority12.CasesPayingArrearsPercent =
          db.GetNullableDecimal(reader, 6);
        entities.DashboardStagingPriority12.CasesPayingArrearsRank =
          db.GetNullableInt32(reader, 7);
        entities.DashboardStagingPriority12.ContractorNumber =
          db.GetNullableString(reader, 8);
        entities.DashboardStagingPriority12.PrvYrCasesPaidArrearsNumtr =
          db.GetNullableInt32(reader, 9);
        entities.DashboardStagingPriority12.PrvYrCasesPaidArrearsDenom =
          db.GetNullableInt32(reader, 10);
        entities.DashboardStagingPriority12.CasesPayArrearsPrvYrPct =
          db.GetNullableDecimal(reader, 11);
        entities.DashboardStagingPriority12.PctChgBtwenYrsCasesPayArrs =
          db.GetNullableDecimal(reader, 12);
        entities.DashboardStagingPriority12.Populated = true;

        return true;
      },
      () =>
      {
        entities.DashboardStagingPriority12.Populated = false;
      });
  }

  private IEnumerable<bool> ReadDashboardStagingPriority2()
  {
    return ReadEach("ReadDashboardStagingPriority2",
      (db, command) =>
      {
        db.SetInt32(
          command, "reportMonth", import.DashboardAuditData.ReportMonth);
      },
      (db, reader) =>
      {
        entities.DashboardStagingPriority12.ReportMonth =
          db.GetInt32(reader, 0);
        entities.DashboardStagingPriority12.ReportLevel =
          db.GetString(reader, 1);
        entities.DashboardStagingPriority12.ReportLevelId =
          db.GetString(reader, 2);
        entities.DashboardStagingPriority12.AsOfDate =
          db.GetNullableDate(reader, 3);
        entities.DashboardStagingPriority12.CasesPayingArrearsNumerator =
          db.GetNullableInt32(reader, 4);
        entities.DashboardStagingPriority12.CasesPayingArrearsDenominator =
          db.GetNullableInt32(reader, 5);
        entities.DashboardStagingPriority12.CasesPayingArrearsPercent =
          db.GetNullableDecimal(reader, 6);
        entities.DashboardStagingPriority12.CasesPayingArrearsRank =
          db.GetNullableInt32(reader, 7);
        entities.DashboardStagingPriority12.ContractorNumber =
          db.GetNullableString(reader, 8);
        entities.DashboardStagingPriority12.PrvYrCasesPaidArrearsNumtr =
          db.GetNullableInt32(reader, 9);
        entities.DashboardStagingPriority12.PrvYrCasesPaidArrearsDenom =
          db.GetNullableInt32(reader, 10);
        entities.DashboardStagingPriority12.CasesPayArrearsPrvYrPct =
          db.GetNullableDecimal(reader, 11);
        entities.DashboardStagingPriority12.PctChgBtwenYrsCasesPayArrs =
          db.GetNullableDecimal(reader, 12);
        entities.DashboardStagingPriority12.Populated = true;

        return true;
      },
      () =>
      {
        entities.DashboardStagingPriority12.Populated = false;
      });
  }

  private IEnumerable<bool> ReadDashboardStagingPriority3()
  {
    return ReadEach("ReadDashboardStagingPriority3",
      (db, command) =>
      {
        db.SetInt32(
          command, "reportMonth", import.DashboardAuditData.ReportMonth);
      },
      (db, reader) =>
      {
        entities.DashboardStagingPriority12.ReportMonth =
          db.GetInt32(reader, 0);
        entities.DashboardStagingPriority12.ReportLevel =
          db.GetString(reader, 1);
        entities.DashboardStagingPriority12.ReportLevelId =
          db.GetString(reader, 2);
        entities.DashboardStagingPriority12.AsOfDate =
          db.GetNullableDate(reader, 3);
        entities.DashboardStagingPriority12.CasesPayingArrearsNumerator =
          db.GetNullableInt32(reader, 4);
        entities.DashboardStagingPriority12.CasesPayingArrearsDenominator =
          db.GetNullableInt32(reader, 5);
        entities.DashboardStagingPriority12.CasesPayingArrearsPercent =
          db.GetNullableDecimal(reader, 6);
        entities.DashboardStagingPriority12.CasesPayingArrearsRank =
          db.GetNullableInt32(reader, 7);
        entities.DashboardStagingPriority12.ContractorNumber =
          db.GetNullableString(reader, 8);
        entities.DashboardStagingPriority12.PrvYrCasesPaidArrearsNumtr =
          db.GetNullableInt32(reader, 9);
        entities.DashboardStagingPriority12.PrvYrCasesPaidArrearsDenom =
          db.GetNullableInt32(reader, 10);
        entities.DashboardStagingPriority12.CasesPayArrearsPrvYrPct =
          db.GetNullableDecimal(reader, 11);
        entities.DashboardStagingPriority12.PctChgBtwenYrsCasesPayArrs =
          db.GetNullableDecimal(reader, 12);
        entities.DashboardStagingPriority12.Populated = true;

        return true;
      },
      () =>
      {
        entities.DashboardStagingPriority12.Populated = false;
      });
  }

  private IEnumerable<bool> ReadDashboardStagingPriority4()
  {
    return ReadEach("ReadDashboardStagingPriority4",
      (db, command) =>
      {
        db.SetInt32(
          command, "reportMonth", import.DashboardAuditData.ReportMonth);
      },
      (db, reader) =>
      {
        entities.DashboardStagingPriority12.ReportMonth =
          db.GetInt32(reader, 0);
        entities.DashboardStagingPriority12.ReportLevel =
          db.GetString(reader, 1);
        entities.DashboardStagingPriority12.ReportLevelId =
          db.GetString(reader, 2);
        entities.DashboardStagingPriority12.AsOfDate =
          db.GetNullableDate(reader, 3);
        entities.DashboardStagingPriority12.CasesPayingArrearsNumerator =
          db.GetNullableInt32(reader, 4);
        entities.DashboardStagingPriority12.CasesPayingArrearsDenominator =
          db.GetNullableInt32(reader, 5);
        entities.DashboardStagingPriority12.CasesPayingArrearsPercent =
          db.GetNullableDecimal(reader, 6);
        entities.DashboardStagingPriority12.CasesPayingArrearsRank =
          db.GetNullableInt32(reader, 7);
        entities.DashboardStagingPriority12.ContractorNumber =
          db.GetNullableString(reader, 8);
        entities.DashboardStagingPriority12.PrvYrCasesPaidArrearsNumtr =
          db.GetNullableInt32(reader, 9);
        entities.DashboardStagingPriority12.PrvYrCasesPaidArrearsDenom =
          db.GetNullableInt32(reader, 10);
        entities.DashboardStagingPriority12.CasesPayArrearsPrvYrPct =
          db.GetNullableDecimal(reader, 11);
        entities.DashboardStagingPriority12.PctChgBtwenYrsCasesPayArrs =
          db.GetNullableDecimal(reader, 12);
        entities.DashboardStagingPriority12.Populated = true;

        return true;
      },
      () =>
      {
        entities.DashboardStagingPriority12.Populated = false;
      });
  }

  private IEnumerable<bool> ReadDashboardStagingPriority5()
  {
    return ReadEach("ReadDashboardStagingPriority5",
      (db, command) =>
      {
        db.SetInt32(
          command, "reportMonth", import.DashboardAuditData.ReportMonth);
      },
      (db, reader) =>
      {
        entities.DashboardStagingPriority35.ReportMonth =
          db.GetInt32(reader, 0);
        entities.DashboardStagingPriority35.ReportLevel =
          db.GetString(reader, 1);
        entities.DashboardStagingPriority35.ReportLevelId =
          db.GetString(reader, 2);
        entities.DashboardStagingPriority35.AsOfDate =
          db.GetNullableDate(reader, 3);
        entities.DashboardStagingPriority35.CasesPayingArrearsDenominator =
          db.GetNullableInt32(reader, 4);
        entities.DashboardStagingPriority35.CasesPayingArrearsNumerator =
          db.GetNullableInt32(reader, 5);
        entities.DashboardStagingPriority35.CasesPayingArrearsPercent =
          db.GetNullableDecimal(reader, 6);
        entities.DashboardStagingPriority35.CasesPayingArrearsRank =
          db.GetNullableInt32(reader, 7);
        entities.DashboardStagingPriority35.Populated = true;

        return true;
      },
      () =>
      {
        entities.DashboardStagingPriority35.Populated = false;
      });
  }

  private bool ReadDashboardStagingPriority6()
  {
    entities.DashboardStagingPriority12.Populated = false;

    return Read("ReadDashboardStagingPriority6",
      (db, command) =>
      {
        db.SetInt32(command, "reportMonth", local.Statewide.ReportMonth);
        db.SetString(command, "reportLevel", local.Statewide.ReportLevel);
        db.SetString(command, "reportLevelId", local.Statewide.ReportLevelId);
      },
      (db, reader) =>
      {
        entities.DashboardStagingPriority12.ReportMonth =
          db.GetInt32(reader, 0);
        entities.DashboardStagingPriority12.ReportLevel =
          db.GetString(reader, 1);
        entities.DashboardStagingPriority12.ReportLevelId =
          db.GetString(reader, 2);
        entities.DashboardStagingPriority12.AsOfDate =
          db.GetNullableDate(reader, 3);
        entities.DashboardStagingPriority12.CasesPayingArrearsNumerator =
          db.GetNullableInt32(reader, 4);
        entities.DashboardStagingPriority12.CasesPayingArrearsDenominator =
          db.GetNullableInt32(reader, 5);
        entities.DashboardStagingPriority12.CasesPayingArrearsPercent =
          db.GetNullableDecimal(reader, 6);
        entities.DashboardStagingPriority12.CasesPayingArrearsRank =
          db.GetNullableInt32(reader, 7);
        entities.DashboardStagingPriority12.ContractorNumber =
          db.GetNullableString(reader, 8);
        entities.DashboardStagingPriority12.PrvYrCasesPaidArrearsNumtr =
          db.GetNullableInt32(reader, 9);
        entities.DashboardStagingPriority12.PrvYrCasesPaidArrearsDenom =
          db.GetNullableInt32(reader, 10);
        entities.DashboardStagingPriority12.CasesPayArrearsPrvYrPct =
          db.GetNullableDecimal(reader, 11);
        entities.DashboardStagingPriority12.PctChgBtwenYrsCasesPayArrs =
          db.GetNullableDecimal(reader, 12);
        entities.DashboardStagingPriority12.Populated = true;
      });
  }

  private bool ReadDashboardStagingPriority7()
  {
    entities.DashboardStagingPriority12.Populated = false;

    return Read("ReadDashboardStagingPriority7",
      (db, command) =>
      {
        db.SetInt32(command, "reportMonth", local.Local1.Item.G.ReportMonth);
        db.SetString(command, "reportLevel", local.Local1.Item.G.ReportLevel);
        db.
          SetString(command, "reportLevelId", local.Local1.Item.G.ReportLevelId);
      },
      (db, reader) =>
      {
        entities.DashboardStagingPriority12.ReportMonth =
          db.GetInt32(reader, 0);
        entities.DashboardStagingPriority12.ReportLevel =
          db.GetString(reader, 1);
        entities.DashboardStagingPriority12.ReportLevelId =
          db.GetString(reader, 2);
        entities.DashboardStagingPriority12.AsOfDate =
          db.GetNullableDate(reader, 3);
        entities.DashboardStagingPriority12.CasesPayingArrearsNumerator =
          db.GetNullableInt32(reader, 4);
        entities.DashboardStagingPriority12.CasesPayingArrearsDenominator =
          db.GetNullableInt32(reader, 5);
        entities.DashboardStagingPriority12.CasesPayingArrearsPercent =
          db.GetNullableDecimal(reader, 6);
        entities.DashboardStagingPriority12.CasesPayingArrearsRank =
          db.GetNullableInt32(reader, 7);
        entities.DashboardStagingPriority12.ContractorNumber =
          db.GetNullableString(reader, 8);
        entities.DashboardStagingPriority12.PrvYrCasesPaidArrearsNumtr =
          db.GetNullableInt32(reader, 9);
        entities.DashboardStagingPriority12.PrvYrCasesPaidArrearsDenom =
          db.GetNullableInt32(reader, 10);
        entities.DashboardStagingPriority12.CasesPayArrearsPrvYrPct =
          db.GetNullableDecimal(reader, 11);
        entities.DashboardStagingPriority12.PctChgBtwenYrsCasesPayArrs =
          db.GetNullableDecimal(reader, 12);
        entities.DashboardStagingPriority12.Populated = true;
      });
  }

  private bool ReadDashboardStagingPriority8()
  {
    entities.DashboardStagingPriority35.Populated = false;

    return Read("ReadDashboardStagingPriority8",
      (db, command) =>
      {
        db.SetInt32(
          command, "reportMonth", local.DashboardStagingPriority35.ReportMonth);
        db.SetString(
          command, "reportLevel", local.DashboardStagingPriority35.ReportLevel);
        db.SetString(
          command, "reportLevelId",
          local.DashboardStagingPriority35.ReportLevelId);
      },
      (db, reader) =>
      {
        entities.DashboardStagingPriority35.ReportMonth =
          db.GetInt32(reader, 0);
        entities.DashboardStagingPriority35.ReportLevel =
          db.GetString(reader, 1);
        entities.DashboardStagingPriority35.ReportLevelId =
          db.GetString(reader, 2);
        entities.DashboardStagingPriority35.AsOfDate =
          db.GetNullableDate(reader, 3);
        entities.DashboardStagingPriority35.CasesPayingArrearsDenominator =
          db.GetNullableInt32(reader, 4);
        entities.DashboardStagingPriority35.CasesPayingArrearsNumerator =
          db.GetNullableInt32(reader, 5);
        entities.DashboardStagingPriority35.CasesPayingArrearsPercent =
          db.GetNullableDecimal(reader, 6);
        entities.DashboardStagingPriority35.CasesPayingArrearsRank =
          db.GetNullableInt32(reader, 7);
        entities.DashboardStagingPriority35.Populated = true;
      });
  }

  private IEnumerable<bool> ReadDashboardStagingPriority9()
  {
    return ReadEach("ReadDashboardStagingPriority9",
      (db, command) =>
      {
        db.SetInt32(command, "reportMonth", local.PreviousYear.ReportMonth);
      },
      (db, reader) =>
      {
        entities.PreviousYear.ReportMonth = db.GetInt32(reader, 0);
        entities.PreviousYear.ReportLevel = db.GetString(reader, 1);
        entities.PreviousYear.ReportLevelId = db.GetString(reader, 2);
        entities.PreviousYear.CasesPayingArrearsNumerator =
          db.GetNullableInt32(reader, 3);
        entities.PreviousYear.CasesPayingArrearsDenominator =
          db.GetNullableInt32(reader, 4);
        entities.PreviousYear.Populated = true;

        return true;
      },
      () =>
      {
        entities.PreviousYear.Populated = false;
      });
  }

  private IEnumerable<bool> ReadDebtAdjustment1()
  {
    System.Diagnostics.Debug.Assert(entities.Debt.Populated);

    return ReadEach("ReadDebtAdjustment1",
      (db, command) =>
      {
        db.SetDateTime(command, "createdTmst", import.ReportEndDate.Timestamp);
        db.SetInt32(command, "otyTypePrimary", entities.Debt.OtyType);
        db.SetString(command, "otrPType", entities.Debt.Type1);
        db.SetInt32(
          command, "otrPGeneratedId", entities.Debt.SystemGeneratedIdentifier);
        db.SetString(command, "cpaPType", entities.Debt.CpaType);
        db.SetString(command, "cspPNumber", entities.Debt.CspNumber);
        db.SetInt32(command, "obgPGeneratedId", entities.Debt.ObgGeneratedId);
      },
      (db, reader) =>
      {
        entities.DebtAdjustment.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.DebtAdjustment.CspNumber = db.GetString(reader, 1);
        entities.DebtAdjustment.CpaType = db.GetString(reader, 2);
        entities.DebtAdjustment.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.DebtAdjustment.Type1 = db.GetString(reader, 4);
        entities.DebtAdjustment.Amount = db.GetDecimal(reader, 5);
        entities.DebtAdjustment.DebtAdjustmentType = db.GetString(reader, 6);
        entities.DebtAdjustment.DebtAdjustmentDt = db.GetDate(reader, 7);
        entities.DebtAdjustment.CreatedTmst = db.GetDateTime(reader, 8);
        entities.DebtAdjustment.CspSupNumber = db.GetNullableString(reader, 9);
        entities.DebtAdjustment.CpaSupType = db.GetNullableString(reader, 10);
        entities.DebtAdjustment.OtyType = db.GetInt32(reader, 11);
        entities.DebtAdjustment.Populated = true;
        CheckValid<ObligationTransaction>("Type1", entities.DebtAdjustment.Type1);
        CheckValid<ObligationTransaction>("DebtAdjustmentType",
          entities.DebtAdjustment.DebtAdjustmentType);

        return true;
      },
      () =>
      {
        entities.DebtAdjustment.Populated = false;
      });
  }

  private bool ReadDebtAdjustment2()
  {
    entities.DebtAdjustment.Populated = false;

    return Read("ReadDebtAdjustment2",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ApCsePerson.Number);
        db.SetDate(command, "debAdjDt", import.ReportEndDate.Date);
      },
      (db, reader) =>
      {
        entities.DebtAdjustment.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.DebtAdjustment.CspNumber = db.GetString(reader, 1);
        entities.DebtAdjustment.CpaType = db.GetString(reader, 2);
        entities.DebtAdjustment.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.DebtAdjustment.Type1 = db.GetString(reader, 4);
        entities.DebtAdjustment.Amount = db.GetDecimal(reader, 5);
        entities.DebtAdjustment.DebtAdjustmentType = db.GetString(reader, 6);
        entities.DebtAdjustment.DebtAdjustmentDt = db.GetDate(reader, 7);
        entities.DebtAdjustment.CreatedTmst = db.GetDateTime(reader, 8);
        entities.DebtAdjustment.CspSupNumber = db.GetNullableString(reader, 9);
        entities.DebtAdjustment.CpaSupType = db.GetNullableString(reader, 10);
        entities.DebtAdjustment.OtyType = db.GetInt32(reader, 11);
        entities.DebtAdjustment.Populated = true;
        CheckValid<ObligationTransaction>("Type1", entities.DebtAdjustment.Type1);
        CheckValid<ObligationTransaction>("DebtAdjustmentType",
          entities.DebtAdjustment.DebtAdjustmentType);
      });
  }

  private bool ReadDebtAdjustment3()
  {
    System.Diagnostics.Debug.Assert(entities.DebtDetail.Populated);
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.DebtAdjustment.Populated = false;

    return Read("ReadDebtAdjustment3",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
        db.SetDate(command, "debAdjDt", import.ReportEndDate.Date);
        db.SetInt32(command, "otyTypePrimary", entities.DebtDetail.OtyType);
        db.SetInt32(
          command, "obgPGeneratedId", entities.DebtDetail.ObgGeneratedId);
        db.SetString(command, "otrPType", entities.DebtDetail.OtrType);
        db.SetInt32(
          command, "otrPGeneratedId", entities.DebtDetail.OtrGeneratedId);
        db.SetString(command, "cpaPType", entities.DebtDetail.CpaType);
        db.SetString(command, "cspPNumber", entities.DebtDetail.CspNumber);
      },
      (db, reader) =>
      {
        entities.DebtAdjustment.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.DebtAdjustment.CspNumber = db.GetString(reader, 1);
        entities.DebtAdjustment.CpaType = db.GetString(reader, 2);
        entities.DebtAdjustment.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.DebtAdjustment.Type1 = db.GetString(reader, 4);
        entities.DebtAdjustment.Amount = db.GetDecimal(reader, 5);
        entities.DebtAdjustment.DebtAdjustmentType = db.GetString(reader, 6);
        entities.DebtAdjustment.DebtAdjustmentDt = db.GetDate(reader, 7);
        entities.DebtAdjustment.CreatedTmst = db.GetDateTime(reader, 8);
        entities.DebtAdjustment.CspSupNumber = db.GetNullableString(reader, 9);
        entities.DebtAdjustment.CpaSupType = db.GetNullableString(reader, 10);
        entities.DebtAdjustment.OtyType = db.GetInt32(reader, 11);
        entities.DebtAdjustment.Populated = true;
        CheckValid<ObligationTransaction>("Type1", entities.DebtAdjustment.Type1);
        CheckValid<ObligationTransaction>("DebtAdjustmentType",
          entities.DebtAdjustment.DebtAdjustmentType);
      });
  }

  private IEnumerable<bool> ReadDebtDetailObligationObligationTypeCsePerson1()
  {
    return ReadEach("ReadDebtDetailObligationObligationTypeCsePerson1",
      (db, command) =>
      {
        db.SetDateTime(command, "createdTmst", import.ReportEndDate.Timestamp);
        db.SetDate(command, "dueDt", local.CollectionDate.Date);
        db.SetString(command, "cspNumber", entities.ApCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.DebtDetail.CspNumber = db.GetString(reader, 1);
        entities.DebtDetail.CpaType = db.GetString(reader, 2);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 4);
        entities.DebtDetail.OtrType = db.GetString(reader, 5);
        entities.DebtDetail.DueDt = db.GetDate(reader, 6);
        entities.DebtDetail.BalanceDueAmt = db.GetDecimal(reader, 7);
        entities.DebtDetail.CoveredPrdStartDt = db.GetNullableDate(reader, 8);
        entities.DebtDetail.PreconversionProgramCode =
          db.GetNullableString(reader, 9);
        entities.DebtDetail.CreatedTmst = db.GetDateTime(reader, 10);
        entities.Obligation.CpaType = db.GetString(reader, 11);
        entities.Obligation.CspNumber = db.GetString(reader, 12);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 13);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 14);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 14);
        entities.Obligation.PrimarySecondaryCode =
          db.GetNullableString(reader, 15);
        entities.Obligation.CreatedTmst = db.GetDateTime(reader, 16);
        entities.ObligationType.Code = db.GetString(reader, 17);
        entities.ObligationType.Classification = db.GetString(reader, 18);
        entities.Supp.Number = db.GetString(reader, 19);
        entities.DebtDetail.Populated = true;
        entities.Obligation.Populated = true;
        entities.ObligationType.Populated = true;
        entities.Supp.Populated = true;
        CheckValid<Obligation>("PrimarySecondaryCode",
          entities.Obligation.PrimarySecondaryCode);
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);

        return true;
      },
      () =>
      {
        entities.ObligationType.Populated = false;
        entities.DebtDetail.Populated = false;
        entities.Obligation.Populated = false;
        entities.Supp.Populated = false;
      });
  }

  private IEnumerable<bool> ReadDebtDetailObligationObligationTypeCsePerson2()
  {
    return ReadEach("ReadDebtDetailObligationObligationTypeCsePerson2",
      (db, command) =>
      {
        db.SetDateTime(command, "createdTmst", import.ReportEndDate.Timestamp);
        db.SetDate(command, "dueDt", local.CollectionDate.Date);
        db.SetString(command, "cspNumber", entities.ApCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.DebtDetail.CspNumber = db.GetString(reader, 1);
        entities.DebtDetail.CpaType = db.GetString(reader, 2);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 4);
        entities.DebtDetail.OtrType = db.GetString(reader, 5);
        entities.DebtDetail.DueDt = db.GetDate(reader, 6);
        entities.DebtDetail.BalanceDueAmt = db.GetDecimal(reader, 7);
        entities.DebtDetail.CoveredPrdStartDt = db.GetNullableDate(reader, 8);
        entities.DebtDetail.PreconversionProgramCode =
          db.GetNullableString(reader, 9);
        entities.DebtDetail.CreatedTmst = db.GetDateTime(reader, 10);
        entities.Obligation.CpaType = db.GetString(reader, 11);
        entities.Obligation.CspNumber = db.GetString(reader, 12);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 13);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 14);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 14);
        entities.Obligation.PrimarySecondaryCode =
          db.GetNullableString(reader, 15);
        entities.Obligation.CreatedTmst = db.GetDateTime(reader, 16);
        entities.ObligationType.Code = db.GetString(reader, 17);
        entities.ObligationType.Classification = db.GetString(reader, 18);
        entities.Supp.Number = db.GetString(reader, 19);
        entities.AfterFy.SystemGeneratedIdentifier = db.GetInt32(reader, 20);
        entities.AfterFy.AppliedToCode = db.GetString(reader, 21);
        entities.AfterFy.CollectionDt = db.GetDate(reader, 22);
        entities.AfterFy.AdjustedInd = db.GetNullableString(reader, 23);
        entities.AfterFy.ConcurrentInd = db.GetString(reader, 24);
        entities.AfterFy.CrtType = db.GetInt32(reader, 25);
        entities.AfterFy.CstId = db.GetInt32(reader, 26);
        entities.AfterFy.CrvId = db.GetInt32(reader, 27);
        entities.AfterFy.CrdId = db.GetInt32(reader, 28);
        entities.AfterFy.ObgId = db.GetInt32(reader, 29);
        entities.AfterFy.CspNumber = db.GetString(reader, 30);
        entities.AfterFy.CpaType = db.GetString(reader, 31);
        entities.AfterFy.OtrId = db.GetInt32(reader, 32);
        entities.AfterFy.OtrType = db.GetString(reader, 33);
        entities.AfterFy.OtyId = db.GetInt32(reader, 34);
        entities.AfterFy.CollectionAdjustmentDt = db.GetDate(reader, 35);
        entities.AfterFy.CreatedTmst = db.GetDateTime(reader, 36);
        entities.AfterFy.Amount = db.GetDecimal(reader, 37);
        entities.AfterFy.ProgramAppliedTo = db.GetString(reader, 38);
        entities.DebtDetail.Populated = true;
        entities.Obligation.Populated = true;
        entities.ObligationType.Populated = true;
        entities.Supp.Populated = true;
        entities.AfterFy.Populated = db.GetNullableInt32(reader, 20) != null;
        CheckValid<Obligation>("PrimarySecondaryCode",
          entities.Obligation.PrimarySecondaryCode);
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);

        if (entities.AfterFy.Populated)
        {
          CheckValid<Collection>("AppliedToCode", entities.AfterFy.AppliedToCode);
          CheckValid<Collection>("AdjustedInd", entities.AfterFy.AdjustedInd);
          CheckValid<Collection>("ConcurrentInd", entities.AfterFy.ConcurrentInd);
          CheckValid<Collection>("ProgramAppliedTo",
            entities.AfterFy.ProgramAppliedTo);
        }

        return true;
      },
      () =>
      {
        entities.ObligationType.Populated = false;
        entities.DebtDetail.Populated = false;
        entities.Obligation.Populated = false;
        entities.Supp.Populated = false;
        entities.AfterFy.Populated = false;
      });
  }

  private IEnumerable<bool> ReadDebtObligationObligationTypeDebtDetail1()
  {
    return ReadEach("ReadDebtObligationObligationTypeDebtDetail1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ApCsePerson.Number);
        db.SetNullableString(
          command, "cspSupNumber", entities.ChOrArCsePerson.Number);
        db.SetDateTime(command, "createdTmst", import.ReportEndDate.Timestamp);
        db.SetDate(command, "date", import.ReportEndDate.Date);
      },
      (db, reader) =>
      {
        entities.Debt.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.Debt.CspNumber = db.GetString(reader, 1);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.DebtDetail.CspNumber = db.GetString(reader, 1);
        entities.Debt.CpaType = db.GetString(reader, 2);
        entities.Obligation.CpaType = db.GetString(reader, 2);
        entities.DebtDetail.CpaType = db.GetString(reader, 2);
        entities.Debt.SystemGeneratedIdentifier = db.GetInt32(reader, 3);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.Debt.Type1 = db.GetString(reader, 4);
        entities.DebtDetail.OtrType = db.GetString(reader, 4);
        entities.Debt.Amount = db.GetDecimal(reader, 5);
        entities.Debt.CreatedTmst = db.GetDateTime(reader, 6);
        entities.Debt.CspSupNumber = db.GetNullableString(reader, 7);
        entities.Debt.CpaSupType = db.GetNullableString(reader, 8);
        entities.Debt.OtyType = db.GetInt32(reader, 9);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 9);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 9);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 9);
        entities.Obligation.PrimarySecondaryCode =
          db.GetNullableString(reader, 10);
        entities.Obligation.CreatedTmst = db.GetDateTime(reader, 11);
        entities.ObligationType.Code = db.GetString(reader, 12);
        entities.ObligationType.Classification = db.GetString(reader, 13);
        entities.DebtDetail.DueDt = db.GetDate(reader, 14);
        entities.DebtDetail.BalanceDueAmt = db.GetDecimal(reader, 15);
        entities.DebtDetail.CoveredPrdStartDt = db.GetNullableDate(reader, 16);
        entities.DebtDetail.PreconversionProgramCode =
          db.GetNullableString(reader, 17);
        entities.DebtDetail.CreatedTmst = db.GetDateTime(reader, 18);
        entities.Debt.Populated = true;
        entities.Obligation.Populated = true;
        entities.ObligationType.Populated = true;
        entities.DebtDetail.Populated = true;
        CheckValid<ObligationTransaction>("Type1", entities.Debt.Type1);
        CheckValid<Obligation>("PrimarySecondaryCode",
          entities.Obligation.PrimarySecondaryCode);
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);

        return true;
      },
      () =>
      {
        entities.ObligationType.Populated = false;
        entities.DebtDetail.Populated = false;
        entities.Obligation.Populated = false;
        entities.Debt.Populated = false;
      });
  }

  private IEnumerable<bool> ReadDebtObligationObligationTypeDebtDetail2()
  {
    return ReadEach("ReadDebtObligationObligationTypeDebtDetail2",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ApCsePerson.Number);
        db.SetNullableString(
          command, "cspSupNumber", entities.ChOrArCsePerson.Number);
        db.SetDateTime(command, "createdTmst", import.ReportEndDate.Timestamp);
        db.SetDate(command, "date", import.ReportEndDate.Date);
      },
      (db, reader) =>
      {
        entities.Debt.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.Debt.CspNumber = db.GetString(reader, 1);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.DebtDetail.CspNumber = db.GetString(reader, 1);
        entities.Debt.CpaType = db.GetString(reader, 2);
        entities.Obligation.CpaType = db.GetString(reader, 2);
        entities.DebtDetail.CpaType = db.GetString(reader, 2);
        entities.Debt.SystemGeneratedIdentifier = db.GetInt32(reader, 3);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.Debt.Type1 = db.GetString(reader, 4);
        entities.DebtDetail.OtrType = db.GetString(reader, 4);
        entities.Debt.Amount = db.GetDecimal(reader, 5);
        entities.Debt.CreatedTmst = db.GetDateTime(reader, 6);
        entities.Debt.CspSupNumber = db.GetNullableString(reader, 7);
        entities.Debt.CpaSupType = db.GetNullableString(reader, 8);
        entities.Debt.OtyType = db.GetInt32(reader, 9);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 9);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 9);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 9);
        entities.Obligation.PrimarySecondaryCode =
          db.GetNullableString(reader, 10);
        entities.Obligation.CreatedTmst = db.GetDateTime(reader, 11);
        entities.ObligationType.Code = db.GetString(reader, 12);
        entities.ObligationType.Classification = db.GetString(reader, 13);
        entities.DebtDetail.DueDt = db.GetDate(reader, 14);
        entities.DebtDetail.BalanceDueAmt = db.GetDecimal(reader, 15);
        entities.DebtDetail.CoveredPrdStartDt = db.GetNullableDate(reader, 16);
        entities.DebtDetail.PreconversionProgramCode =
          db.GetNullableString(reader, 17);
        entities.DebtDetail.CreatedTmst = db.GetDateTime(reader, 18);
        entities.Debt.Populated = true;
        entities.Obligation.Populated = true;
        entities.ObligationType.Populated = true;
        entities.DebtDetail.Populated = true;
        CheckValid<ObligationTransaction>("Type1", entities.Debt.Type1);
        CheckValid<Obligation>("PrimarySecondaryCode",
          entities.Obligation.PrimarySecondaryCode);
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);

        return true;
      },
      () =>
      {
        entities.ObligationType.Populated = false;
        entities.DebtDetail.Populated = false;
        entities.Obligation.Populated = false;
        entities.Debt.Populated = false;
      });
  }

  private IEnumerable<bool> ReadDebtObligationObligationTypeDebtDetail3()
  {
    return ReadEach("ReadDebtObligationObligationTypeDebtDetail3",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ApCsePerson.Number);
        db.SetNullableString(
          command, "cspSupNumber", entities.ChOrArCsePerson.Number);
        db.SetDateTime(command, "createdTmst", import.ReportEndDate.Timestamp);
        db.SetDate(command, "date", import.ReportEndDate.Date);
      },
      (db, reader) =>
      {
        entities.Debt.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.Debt.CspNumber = db.GetString(reader, 1);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.DebtDetail.CspNumber = db.GetString(reader, 1);
        entities.Debt.CpaType = db.GetString(reader, 2);
        entities.Obligation.CpaType = db.GetString(reader, 2);
        entities.DebtDetail.CpaType = db.GetString(reader, 2);
        entities.Debt.SystemGeneratedIdentifier = db.GetInt32(reader, 3);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.Debt.Type1 = db.GetString(reader, 4);
        entities.DebtDetail.OtrType = db.GetString(reader, 4);
        entities.Debt.Amount = db.GetDecimal(reader, 5);
        entities.Debt.CreatedTmst = db.GetDateTime(reader, 6);
        entities.Debt.CspSupNumber = db.GetNullableString(reader, 7);
        entities.Debt.CpaSupType = db.GetNullableString(reader, 8);
        entities.Debt.OtyType = db.GetInt32(reader, 9);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 9);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 9);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 9);
        entities.Obligation.PrimarySecondaryCode =
          db.GetNullableString(reader, 10);
        entities.Obligation.CreatedTmst = db.GetDateTime(reader, 11);
        entities.ObligationType.Code = db.GetString(reader, 12);
        entities.ObligationType.Classification = db.GetString(reader, 13);
        entities.DebtDetail.DueDt = db.GetDate(reader, 14);
        entities.DebtDetail.BalanceDueAmt = db.GetDecimal(reader, 15);
        entities.DebtDetail.CoveredPrdStartDt = db.GetNullableDate(reader, 16);
        entities.DebtDetail.PreconversionProgramCode =
          db.GetNullableString(reader, 17);
        entities.DebtDetail.CreatedTmst = db.GetDateTime(reader, 18);
        entities.Debt.Populated = true;
        entities.Obligation.Populated = true;
        entities.ObligationType.Populated = true;
        entities.DebtDetail.Populated = true;
        CheckValid<ObligationTransaction>("Type1", entities.Debt.Type1);
        CheckValid<Obligation>("PrimarySecondaryCode",
          entities.Obligation.PrimarySecondaryCode);
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);

        return true;
      },
      () =>
      {
        entities.ObligationType.Populated = false;
        entities.DebtDetail.Populated = false;
        entities.Obligation.Populated = false;
        entities.Debt.Populated = false;
      });
  }

  private IEnumerable<bool>
    ReadDebtObligationObligationTypeDebtDetailDebtAdjustment()
  {
    return ReadEach("ReadDebtObligationObligationTypeDebtDetailDebtAdjustment",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ApCsePerson.Number);
        db.SetNullableString(
          command, "cspSupNumber", entities.ChOrArCsePerson.Number);
        db.SetDate(command, "date", import.ReportEndDate.Date);
        db.
          SetDateTime(command, "createdTmst1", import.ReportStartDate.Timestamp);
        db.SetDateTime(command, "createdTmst2", import.ReportEndDate.Timestamp);
      },
      (db, reader) =>
      {
        entities.Debt.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.Debt.CspNumber = db.GetString(reader, 1);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.DebtDetail.CspNumber = db.GetString(reader, 1);
        entities.Debt.CpaType = db.GetString(reader, 2);
        entities.Obligation.CpaType = db.GetString(reader, 2);
        entities.DebtDetail.CpaType = db.GetString(reader, 2);
        entities.Debt.SystemGeneratedIdentifier = db.GetInt32(reader, 3);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.Debt.Type1 = db.GetString(reader, 4);
        entities.DebtDetail.OtrType = db.GetString(reader, 4);
        entities.Debt.Amount = db.GetDecimal(reader, 5);
        entities.Debt.CreatedTmst = db.GetDateTime(reader, 6);
        entities.Debt.CspSupNumber = db.GetNullableString(reader, 7);
        entities.Debt.CpaSupType = db.GetNullableString(reader, 8);
        entities.Debt.OtyType = db.GetInt32(reader, 9);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 9);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 9);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 9);
        entities.Obligation.PrimarySecondaryCode =
          db.GetNullableString(reader, 10);
        entities.Obligation.CreatedTmst = db.GetDateTime(reader, 11);
        entities.ObligationType.Code = db.GetString(reader, 12);
        entities.ObligationType.Classification = db.GetString(reader, 13);
        entities.DebtDetail.DueDt = db.GetDate(reader, 14);
        entities.DebtDetail.BalanceDueAmt = db.GetDecimal(reader, 15);
        entities.DebtDetail.CoveredPrdStartDt = db.GetNullableDate(reader, 16);
        entities.DebtDetail.PreconversionProgramCode =
          db.GetNullableString(reader, 17);
        entities.DebtDetail.CreatedTmst = db.GetDateTime(reader, 18);
        entities.DebtAdjustment.ObgGeneratedId = db.GetInt32(reader, 19);
        entities.DebtAdjustment.CspNumber = db.GetString(reader, 20);
        entities.DebtAdjustment.CpaType = db.GetString(reader, 21);
        entities.DebtAdjustment.SystemGeneratedIdentifier =
          db.GetInt32(reader, 22);
        entities.DebtAdjustment.Type1 = db.GetString(reader, 23);
        entities.DebtAdjustment.Amount = db.GetDecimal(reader, 24);
        entities.DebtAdjustment.DebtAdjustmentType = db.GetString(reader, 25);
        entities.DebtAdjustment.DebtAdjustmentDt = db.GetDate(reader, 26);
        entities.DebtAdjustment.CreatedTmst = db.GetDateTime(reader, 27);
        entities.DebtAdjustment.CspSupNumber = db.GetNullableString(reader, 28);
        entities.DebtAdjustment.CpaSupType = db.GetNullableString(reader, 29);
        entities.DebtAdjustment.OtyType = db.GetInt32(reader, 30);
        entities.Debt.Populated = true;
        entities.Obligation.Populated = true;
        entities.ObligationType.Populated = true;
        entities.DebtDetail.Populated = true;
        entities.DebtAdjustment.Populated = true;
        CheckValid<ObligationTransaction>("Type1", entities.Debt.Type1);
        CheckValid<Obligation>("PrimarySecondaryCode",
          entities.Obligation.PrimarySecondaryCode);
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);
        CheckValid<ObligationTransaction>("Type1", entities.DebtAdjustment.Type1);
        CheckValid<ObligationTransaction>("DebtAdjustmentType",
          entities.DebtAdjustment.DebtAdjustmentType);

        return true;
      },
      () =>
      {
        entities.ObligationType.Populated = false;
        entities.DebtDetail.Populated = false;
        entities.Obligation.Populated = false;
        entities.DebtAdjustment.Populated = false;
        entities.Debt.Populated = false;
      });
  }

  private IEnumerable<bool> ReadLegalReferralServiceProvider()
  {
    return ReadEach("ReadLegalReferralServiceProvider",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetDateTime(
          command, "createdTimestamp", import.ReportEndDate.Timestamp);
        db.SetNullableDate(command, "statusDate", import.ReportEndDate.Date);
        db.SetNullableDate(
          command, "discontinueDate", import.ReportStartDate.Date);
      },
      (db, reader) =>
      {
        entities.LegalReferral.CasNumber = db.GetString(reader, 0);
        entities.LegalReferral.Identifier = db.GetInt32(reader, 1);
        entities.LegalReferral.StatusDate = db.GetNullableDate(reader, 2);
        entities.LegalReferral.Status = db.GetNullableString(reader, 3);
        entities.LegalReferral.ReferralDate = db.GetDate(reader, 4);
        entities.LegalReferral.CreatedTimestamp = db.GetDateTime(reader, 5);
        entities.LegalReferral.ReferralReason1 = db.GetString(reader, 6);
        entities.LegalReferral.ReferralReason2 = db.GetString(reader, 7);
        entities.LegalReferral.ReferralReason3 = db.GetString(reader, 8);
        entities.LegalReferral.ReferralReason4 = db.GetString(reader, 9);
        entities.LegalReferral.CourtCaseNumber =
          db.GetNullableString(reader, 10);
        entities.LegalReferral.TribunalId = db.GetNullableInt32(reader, 11);
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 12);
        entities.ServiceProvider.UserId = db.GetString(reader, 13);
        entities.ServiceProvider.RoleCode = db.GetNullableString(reader, 14);
        entities.LegalReferral.Populated = true;
        entities.ServiceProvider.Populated = true;

        return true;
      },
      () =>
      {
        entities.ServiceProvider.Populated = false;
        entities.LegalReferral.Populated = false;
      });
  }

  private void UpdateDashboardStagingPriority1()
  {
    var casesPayingArrearsPercent = 0M;

    entities.DashboardStagingPriority35.Populated = false;
    Update("UpdateDashboardStagingPriority1",
      (db, command) =>
      {
        db.SetNullableInt32(command, "casPayingArrDen", 0);
        db.SetNullableInt32(command, "casPayingArrNum", 0);
        db.SetNullableDecimal(
          command, "casPayingArrPer", casesPayingArrearsPercent);
        db.SetNullableInt32(command, "casPayingArrRnk", 0);
        db.SetInt32(
          command, "reportMonth",
          entities.DashboardStagingPriority35.ReportMonth);
        db.SetString(
          command, "reportLevel",
          entities.DashboardStagingPriority35.ReportLevel);
        db.SetString(
          command, "reportLevelId",
          entities.DashboardStagingPriority35.ReportLevelId);
      });

    entities.DashboardStagingPriority35.CasesPayingArrearsDenominator = 0;
    entities.DashboardStagingPriority35.CasesPayingArrearsNumerator = 0;
    entities.DashboardStagingPriority35.CasesPayingArrearsPercent =
      casesPayingArrearsPercent;
    entities.DashboardStagingPriority35.CasesPayingArrearsRank = 0;
    entities.DashboardStagingPriority35.Populated = true;
  }

  private void UpdateDashboardStagingPriority10()
  {
    var casesPayingArrearsRank = local.Temp.CasesPayingArrearsRank ?? 0;

    entities.DashboardStagingPriority12.Populated = false;
    Update("UpdateDashboardStagingPriority10",
      (db, command) =>
      {
        db.SetNullableInt32(command, "casPayingArrRnk", casesPayingArrearsRank);
        db.SetInt32(
          command, "reportMonth",
          entities.DashboardStagingPriority12.ReportMonth);
        db.SetString(
          command, "reportLevel",
          entities.DashboardStagingPriority12.ReportLevel);
        db.SetString(
          command, "reportLevelId",
          entities.DashboardStagingPriority12.ReportLevelId);
      });

    entities.DashboardStagingPriority12.CasesPayingArrearsRank =
      casesPayingArrearsRank;
    entities.DashboardStagingPriority12.Populated = true;
  }

  private void UpdateDashboardStagingPriority2()
  {
    var asOfDate = local.Statewide.AsOfDate;
    var casesPayingArrearsNumerator =
      local.Statewide.CasesPayingArrearsNumerator ?? 0;
    var casesPayingArrearsDenominator =
      local.Statewide.CasesPayingArrearsDenominator ?? 0;
    var casesPayingArrearsPercent =
      local.Statewide.CasesPayingArrearsPercent ?? 0M;
    var casesPayingArrearsRank = local.Statewide.CasesPayingArrearsRank ?? 0;

    entities.DashboardStagingPriority12.Populated = false;
    Update("UpdateDashboardStagingPriority2",
      (db, command) =>
      {
        db.SetNullableDate(command, "asOfDt", asOfDate);
        db.SetNullableInt32(
          command, "casPayingArrNum", casesPayingArrearsNumerator);
        db.SetNullableInt32(
          command, "casPayingArrDen", casesPayingArrearsDenominator);
        db.SetNullableDecimal(
          command, "casPayingArrPer", casesPayingArrearsPercent);
        db.SetNullableInt32(command, "casPayingArrRnk", casesPayingArrearsRank);
        db.SetInt32(
          command, "reportMonth",
          entities.DashboardStagingPriority12.ReportMonth);
        db.SetString(
          command, "reportLevel",
          entities.DashboardStagingPriority12.ReportLevel);
        db.SetString(
          command, "reportLevelId",
          entities.DashboardStagingPriority12.ReportLevelId);
      });

    entities.DashboardStagingPriority12.AsOfDate = asOfDate;
    entities.DashboardStagingPriority12.CasesPayingArrearsNumerator =
      casesPayingArrearsNumerator;
    entities.DashboardStagingPriority12.CasesPayingArrearsDenominator =
      casesPayingArrearsDenominator;
    entities.DashboardStagingPriority12.CasesPayingArrearsPercent =
      casesPayingArrearsPercent;
    entities.DashboardStagingPriority12.CasesPayingArrearsRank =
      casesPayingArrearsRank;
    entities.DashboardStagingPriority12.Populated = true;
  }

  private void UpdateDashboardStagingPriority3()
  {
    var asOfDate = local.Local1.Item.G.AsOfDate;
    var casesPayingArrearsNumerator =
      local.Local1.Item.G.CasesPayingArrearsNumerator ?? 0;
    var casesPayingArrearsDenominator =
      local.Local1.Item.G.CasesPayingArrearsDenominator ?? 0;
    var casesPayingArrearsPercent =
      local.Local1.Item.G.CasesPayingArrearsPercent ?? 0M;
    var casesPayingArrearsRank = local.Local1.Item.G.CasesPayingArrearsRank ?? 0
      ;

    entities.DashboardStagingPriority12.Populated = false;
    Update("UpdateDashboardStagingPriority3",
      (db, command) =>
      {
        db.SetNullableDate(command, "asOfDt", asOfDate);
        db.SetNullableInt32(
          command, "casPayingArrNum", casesPayingArrearsNumerator);
        db.SetNullableInt32(
          command, "casPayingArrDen", casesPayingArrearsDenominator);
        db.SetNullableDecimal(
          command, "casPayingArrPer", casesPayingArrearsPercent);
        db.SetNullableInt32(command, "casPayingArrRnk", casesPayingArrearsRank);
        db.SetInt32(
          command, "reportMonth",
          entities.DashboardStagingPriority12.ReportMonth);
        db.SetString(
          command, "reportLevel",
          entities.DashboardStagingPriority12.ReportLevel);
        db.SetString(
          command, "reportLevelId",
          entities.DashboardStagingPriority12.ReportLevelId);
      });

    entities.DashboardStagingPriority12.AsOfDate = asOfDate;
    entities.DashboardStagingPriority12.CasesPayingArrearsNumerator =
      casesPayingArrearsNumerator;
    entities.DashboardStagingPriority12.CasesPayingArrearsDenominator =
      casesPayingArrearsDenominator;
    entities.DashboardStagingPriority12.CasesPayingArrearsPercent =
      casesPayingArrearsPercent;
    entities.DashboardStagingPriority12.CasesPayingArrearsRank =
      casesPayingArrearsRank;
    entities.DashboardStagingPriority12.Populated = true;
  }

  private void UpdateDashboardStagingPriority4()
  {
    var casesPayingArrearsDenominator =
      (entities.DashboardStagingPriority35.CasesPayingArrearsDenominator ?? 0) +
      (local.DashboardStagingPriority35.CasesPayingArrearsDenominator ?? 0);

    entities.DashboardStagingPriority35.Populated = false;
    Update("UpdateDashboardStagingPriority4",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "casPayingArrDen", casesPayingArrearsDenominator);
        db.SetInt32(
          command, "reportMonth",
          entities.DashboardStagingPriority35.ReportMonth);
        db.SetString(
          command, "reportLevel",
          entities.DashboardStagingPriority35.ReportLevel);
        db.SetString(
          command, "reportLevelId",
          entities.DashboardStagingPriority35.ReportLevelId);
      });

    entities.DashboardStagingPriority35.CasesPayingArrearsDenominator =
      casesPayingArrearsDenominator;
    entities.DashboardStagingPriority35.Populated = true;
  }

  private void UpdateDashboardStagingPriority5()
  {
    var casesPayingArrearsNumerator =
      (entities.DashboardStagingPriority35.CasesPayingArrearsNumerator ?? 0) +
      (local.DashboardStagingPriority35.CasesPayingArrearsNumerator ?? 0);

    entities.DashboardStagingPriority35.Populated = false;
    Update("UpdateDashboardStagingPriority5",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "casPayingArrNum", casesPayingArrearsNumerator);
        db.SetInt32(
          command, "reportMonth",
          entities.DashboardStagingPriority35.ReportMonth);
        db.SetString(
          command, "reportLevel",
          entities.DashboardStagingPriority35.ReportLevel);
        db.SetString(
          command, "reportLevelId",
          entities.DashboardStagingPriority35.ReportLevelId);
      });

    entities.DashboardStagingPriority35.CasesPayingArrearsNumerator =
      casesPayingArrearsNumerator;
    entities.DashboardStagingPriority35.Populated = true;
  }

  private void UpdateDashboardStagingPriority6()
  {
    var asOfDate = local.Contractor.Item.Gcontractor.AsOfDate;
    var casesPayingArrearsNumerator =
      local.Contractor.Item.Gcontractor.CasesPayingArrearsNumerator ?? 0;
    var casesPayingArrearsDenominator =
      local.Contractor.Item.Gcontractor.CasesPayingArrearsDenominator ?? 0;
    var casesPayingArrearsPercent =
      local.Contractor.Item.Gcontractor.CasesPayingArrearsPercent ?? 0M;
    var casesPayingArrearsRank =
      local.Contractor.Item.Gcontractor.CasesPayingArrearsRank ?? 0;
    var prvYrCasesPaidArrearsNumtr =
      local.Contractor.Item.Gcontractor.PrvYrCasesPaidArrearsNumtr ?? 0;
    var prvYrCasesPaidArrearsDenom =
      local.Contractor.Item.Gcontractor.PrvYrCasesPaidArrearsDenom ?? 0;
    var casesPayArrearsPrvYrPct =
      local.Contractor.Item.Gcontractor.CasesPayArrearsPrvYrPct ?? 0M;
    var pctChgBtwenYrsCasesPayArrs =
      local.Contractor.Item.Gcontractor.PctChgBtwenYrsCasesPayArrs ?? 0M;

    entities.DashboardStagingPriority12.Populated = false;
    Update("UpdateDashboardStagingPriority6",
      (db, command) =>
      {
        db.SetNullableDate(command, "asOfDt", asOfDate);
        db.SetNullableInt32(
          command, "casPayingArrNum", casesPayingArrearsNumerator);
        db.SetNullableInt32(
          command, "casPayingArrDen", casesPayingArrearsDenominator);
        db.SetNullableDecimal(
          command, "casPayingArrPer", casesPayingArrearsPercent);
        db.SetNullableInt32(command, "casPayingArrRnk", casesPayingArrearsRank);
        db.SetNullableInt32(
          command, "prvYrPdArNumtr", prvYrCasesPaidArrearsNumtr);
        db.SetNullableInt32(
          command, "prvYrPdArDenom", prvYrCasesPaidArrearsDenom);
        db.
          SetNullableDecimal(command, "payArPrvYrPct", casesPayArrearsPrvYrPct);
        db.SetNullableDecimal(
          command, "pctChgByrArsPd", pctChgBtwenYrsCasesPayArrs);
        db.SetInt32(
          command, "reportMonth",
          entities.DashboardStagingPriority12.ReportMonth);
        db.SetString(
          command, "reportLevel",
          entities.DashboardStagingPriority12.ReportLevel);
        db.SetString(
          command, "reportLevelId",
          entities.DashboardStagingPriority12.ReportLevelId);
      });

    entities.DashboardStagingPriority12.AsOfDate = asOfDate;
    entities.DashboardStagingPriority12.CasesPayingArrearsNumerator =
      casesPayingArrearsNumerator;
    entities.DashboardStagingPriority12.CasesPayingArrearsDenominator =
      casesPayingArrearsDenominator;
    entities.DashboardStagingPriority12.CasesPayingArrearsPercent =
      casesPayingArrearsPercent;
    entities.DashboardStagingPriority12.CasesPayingArrearsRank =
      casesPayingArrearsRank;
    entities.DashboardStagingPriority12.PrvYrCasesPaidArrearsNumtr =
      prvYrCasesPaidArrearsNumtr;
    entities.DashboardStagingPriority12.PrvYrCasesPaidArrearsDenom =
      prvYrCasesPaidArrearsDenom;
    entities.DashboardStagingPriority12.CasesPayArrearsPrvYrPct =
      casesPayArrearsPrvYrPct;
    entities.DashboardStagingPriority12.PctChgBtwenYrsCasesPayArrs =
      pctChgBtwenYrsCasesPayArrs;
    entities.DashboardStagingPriority12.Populated = true;
  }

  private void UpdateDashboardStagingPriority7()
  {
    var casesPayingArrearsPercent = local.Temp.CasesPayingArrearsPercent ?? 0M;
    var prvYrCasesPaidArrearsNumtr = local.Temp.PrvYrCasesPaidArrearsNumtr ?? 0;
    var prvYrCasesPaidArrearsDenom = local.Temp.PrvYrCasesPaidArrearsDenom ?? 0;
    var casesPayArrearsPrvYrPct = local.Temp.CasesPayArrearsPrvYrPct ?? 0M;
    var pctChgBtwenYrsCasesPayArrs = local.Temp.PctChgBtwenYrsCasesPayArrs ?? 0M
      ;

    entities.DashboardStagingPriority12.Populated = false;
    Update("UpdateDashboardStagingPriority7",
      (db, command) =>
      {
        db.SetNullableDecimal(
          command, "casPayingArrPer", casesPayingArrearsPercent);
        db.SetNullableInt32(
          command, "prvYrPdArNumtr", prvYrCasesPaidArrearsNumtr);
        db.SetNullableInt32(
          command, "prvYrPdArDenom", prvYrCasesPaidArrearsDenom);
        db.
          SetNullableDecimal(command, "payArPrvYrPct", casesPayArrearsPrvYrPct);
        db.SetNullableDecimal(
          command, "pctChgByrArsPd", pctChgBtwenYrsCasesPayArrs);
        db.SetInt32(
          command, "reportMonth",
          entities.DashboardStagingPriority12.ReportMonth);
        db.SetString(
          command, "reportLevel",
          entities.DashboardStagingPriority12.ReportLevel);
        db.SetString(
          command, "reportLevelId",
          entities.DashboardStagingPriority12.ReportLevelId);
      });

    entities.DashboardStagingPriority12.CasesPayingArrearsPercent =
      casesPayingArrearsPercent;
    entities.DashboardStagingPriority12.PrvYrCasesPaidArrearsNumtr =
      prvYrCasesPaidArrearsNumtr;
    entities.DashboardStagingPriority12.PrvYrCasesPaidArrearsDenom =
      prvYrCasesPaidArrearsDenom;
    entities.DashboardStagingPriority12.CasesPayArrearsPrvYrPct =
      casesPayArrearsPrvYrPct;
    entities.DashboardStagingPriority12.PctChgBtwenYrsCasesPayArrs =
      pctChgBtwenYrsCasesPayArrs;
    entities.DashboardStagingPriority12.Populated = true;
  }

  private void UpdateDashboardStagingPriority8()
  {
    var casesPayingArrearsPercent =
      local.DashboardStagingPriority35.CasesPayingArrearsPercent ?? 0M;

    entities.DashboardStagingPriority35.Populated = false;
    Update("UpdateDashboardStagingPriority8",
      (db, command) =>
      {
        db.SetNullableDecimal(
          command, "casPayingArrPer", casesPayingArrearsPercent);
        db.SetInt32(
          command, "reportMonth",
          entities.DashboardStagingPriority35.ReportMonth);
        db.SetString(
          command, "reportLevel",
          entities.DashboardStagingPriority35.ReportLevel);
        db.SetString(
          command, "reportLevelId",
          entities.DashboardStagingPriority35.ReportLevelId);
      });

    entities.DashboardStagingPriority35.CasesPayingArrearsPercent =
      casesPayingArrearsPercent;
    entities.DashboardStagingPriority35.Populated = true;
  }

  private void UpdateDashboardStagingPriority9()
  {
    var casesPayingArrearsRank =
      local.DashboardStagingPriority35.CasesPayingArrearsRank ?? 0;

    entities.DashboardStagingPriority35.Populated = false;
    Update("UpdateDashboardStagingPriority9",
      (db, command) =>
      {
        db.SetNullableInt32(command, "casPayingArrRnk", casesPayingArrearsRank);
        db.SetInt32(
          command, "reportMonth",
          entities.DashboardStagingPriority35.ReportMonth);
        db.SetString(
          command, "reportLevel",
          entities.DashboardStagingPriority35.ReportLevel);
        db.SetString(
          command, "reportLevelId",
          entities.DashboardStagingPriority35.ReportLevelId);
      });

    entities.DashboardStagingPriority35.CasesPayingArrearsRank =
      casesPayingArrearsRank;
    entities.DashboardStagingPriority35.Populated = true;
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Local local = new();
  protected readonly Entities entities = new();
#endregion

#region Structures
  /// <summary>
  /// This class defines import view.
  /// </summary>
  [Serializable]
  public class Import
  {
    /// <summary>
    /// A value of DashboardAuditData.
    /// </summary>
    public DashboardAuditData DashboardAuditData
    {
      get => dashboardAuditData ??= new();
      set => dashboardAuditData = value;
    }

    /// <summary>
    /// A value of ProgramCheckpointRestart.
    /// </summary>
    public ProgramCheckpointRestart ProgramCheckpointRestart
    {
      get => programCheckpointRestart ??= new();
      set => programCheckpointRestart = value;
    }

    /// <summary>
    /// A value of ProgramProcessingInfo.
    /// </summary>
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    /// <summary>
    /// A value of ReportEndDate.
    /// </summary>
    public DateWorkArea ReportEndDate
    {
      get => reportEndDate ??= new();
      set => reportEndDate = value;
    }

    /// <summary>
    /// A value of ReportStartDate.
    /// </summary>
    public DateWorkArea ReportStartDate
    {
      get => reportStartDate ??= new();
      set => reportStartDate = value;
    }

    /// <summary>
    /// A value of AuditFlag.
    /// </summary>
    public Common AuditFlag
    {
      get => auditFlag ??= new();
      set => auditFlag = value;
    }

    /// <summary>
    /// A value of Cq66220EffectiveFy.
    /// </summary>
    public Ocse157Verification Cq66220EffectiveFy
    {
      get => cq66220EffectiveFy ??= new();
      set => cq66220EffectiveFy = value;
    }

    private DashboardAuditData? dashboardAuditData;
    private ProgramCheckpointRestart? programCheckpointRestart;
    private ProgramProcessingInfo? programProcessingInfo;
    private DateWorkArea? reportEndDate;
    private DateWorkArea? reportStartDate;
    private Common? auditFlag;
    private Ocse157Verification? cq66220EffectiveFy;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A LocalGroup group.</summary>
    [Serializable]
    public class LocalGroup
    {
      /// <summary>
      /// A value of G.
      /// </summary>
      public DashboardStagingPriority12 G
      {
        get => g ??= new();
        set => g = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private DashboardStagingPriority12? g;
    }

    /// <summary>A ContractorGroup group.</summary>
    [Serializable]
    public class ContractorGroup
    {
      /// <summary>
      /// A value of Gcontractor.
      /// </summary>
      public DashboardStagingPriority12 Gcontractor
      {
        get => gcontractor ??= new();
        set => gcontractor = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private DashboardStagingPriority12? gcontractor;
    }

    /// <summary>
    /// A value of FiscalYear.
    /// </summary>
    public DateWorkArea FiscalYear
    {
      get => fiscalYear ??= new();
      set => fiscalYear = value;
    }

    /// <summary>
    /// A value of PreviousRank.
    /// </summary>
    public DashboardStagingPriority35 PreviousRank
    {
      get => previousRank ??= new();
      set => previousRank = value;
    }

    /// <summary>
    /// A value of NullDashboardStagingPriority35.
    /// </summary>
    public DashboardStagingPriority35 NullDashboardStagingPriority35
    {
      get => nullDashboardStagingPriority35 ??= new();
      set => nullDashboardStagingPriority35 = value;
    }

    /// <summary>
    /// A value of DashboardStagingPriority35.
    /// </summary>
    public DashboardStagingPriority35 DashboardStagingPriority35
    {
      get => dashboardStagingPriority35 ??= new();
      set => dashboardStagingPriority35 = value;
    }

    /// <summary>
    /// A value of Worker.
    /// </summary>
    public DashboardAuditData Worker
    {
      get => worker ??= new();
      set => worker = value;
    }

    /// <summary>
    /// A value of CountCaseAtty.
    /// </summary>
    public Common CountCaseAtty
    {
      get => countCaseAtty ??= new();
      set => countCaseAtty = value;
    }

    /// <summary>
    /// A value of Initialize.
    /// </summary>
    public DashboardStagingPriority12 Initialize
    {
      get => initialize ??= new();
      set => initialize = value;
    }

    /// <summary>
    /// A value of CountCaseWk.
    /// </summary>
    public Common CountCaseWk
    {
      get => countCaseWk ??= new();
      set => countCaseWk = value;
    }

    /// <summary>
    /// A value of Delete.
    /// </summary>
    public DashboardStagingPriority12 Delete
    {
      get => delete ??= new();
      set => delete = value;
    }

    /// <summary>
    /// A value of Initialized.
    /// </summary>
    public DashboardAuditData Initialized
    {
      get => initialized ??= new();
      set => initialized = value;
    }

    /// <summary>
    /// A value of ProgramCheckpointRestart.
    /// </summary>
    public ProgramCheckpointRestart ProgramCheckpointRestart
    {
      get => programCheckpointRestart ??= new();
      set => programCheckpointRestart = value;
    }

    /// <summary>
    /// A value of Statewide.
    /// </summary>
    public DashboardStagingPriority12 Statewide
    {
      get => statewide ??= new();
      set => statewide = value;
    }

    /// <summary>
    /// A value of EabFileHandling.
    /// </summary>
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    /// <summary>
    /// A value of EabReportSend.
    /// </summary>
    public EabReportSend EabReportSend
    {
      get => eabReportSend ??= new();
      set => eabReportSend = value;
    }

    /// <summary>
    /// Gets a value of Local1.
    /// </summary>
    [JsonIgnore]
    public Array<LocalGroup> Local1 => local1 ??= new(LocalGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Local1 for json serialization.
    /// </summary>
    [JsonPropertyName("local1")]
    [Computed]
    public IList<LocalGroup>? Local1_Json
    {
      get => local1;
      set => Local1.Assign(value);
    }

    /// <summary>
    /// A value of Restart.
    /// </summary>
    public Case1 Restart
    {
      get => restart ??= new();
      set => restart = value;
    }

    /// <summary>
    /// A value of Prev.
    /// </summary>
    public Case1 Prev
    {
      get => prev ??= new();
      set => prev = value;
    }

    /// <summary>
    /// A value of RecordsReadSinceCommit.
    /// </summary>
    public Common RecordsReadSinceCommit
    {
      get => recordsReadSinceCommit ??= new();
      set => recordsReadSinceCommit = value;
    }

    /// <summary>
    /// A value of DashboardAuditData.
    /// </summary>
    public DashboardAuditData DashboardAuditData
    {
      get => dashboardAuditData ??= new();
      set => dashboardAuditData = value;
    }

    /// <summary>
    /// A value of CountCase.
    /// </summary>
    public Common CountCase
    {
      get => countCase ??= new();
      set => countCase = value;
    }

    /// <summary>
    /// A value of Program.
    /// </summary>
    public Program Program
    {
      get => program ??= new();
      set => program = value;
    }

    /// <summary>
    /// A value of BalDueOnFyEnd.
    /// </summary>
    public ReportTotals BalDueOnFyEnd
    {
      get => balDueOnFyEnd ??= new();
      set => balDueOnFyEnd = value;
    }

    /// <summary>
    /// A value of TempEndDate.
    /// </summary>
    public DateWorkArea TempEndDate
    {
      get => tempEndDate ??= new();
      set => tempEndDate = value;
    }

    /// <summary>
    /// A value of CollFound.
    /// </summary>
    public Common CollFound
    {
      get => collFound ??= new();
      set => collFound = value;
    }

    /// <summary>
    /// A value of Ap.
    /// </summary>
    public CsePerson Ap
    {
      get => ap ??= new();
      set => ap = value;
    }

    /// <summary>
    /// A value of Supp.
    /// </summary>
    public CsePerson Supp
    {
      get => supp ??= new();
      set => supp = value;
    }

    /// <summary>
    /// A value of NonNa.
    /// </summary>
    public Collection NonNa
    {
      get => nonNa ??= new();
      set => nonNa = value;
    }

    /// <summary>
    /// A value of CollectionDate.
    /// </summary>
    public DateWorkArea CollectionDate
    {
      get => collectionDate ??= new();
      set => collectionDate = value;
    }

    /// <summary>
    /// A value of NullDateWorkArea.
    /// </summary>
    public DateWorkArea NullDateWorkArea
    {
      get => nullDateWorkArea ??= new();
      set => nullDateWorkArea = value;
    }

    /// <summary>
    /// A value of Assistance.
    /// </summary>
    public Common Assistance
    {
      get => assistance ??= new();
      set => assistance = value;
    }

    /// <summary>
    /// A value of Temp.
    /// </summary>
    public DashboardStagingPriority12 Temp
    {
      get => temp ??= new();
      set => temp = value;
    }

    /// <summary>
    /// A value of Common.
    /// </summary>
    public Common Common
    {
      get => common ??= new();
      set => common = value;
    }

    /// <summary>
    /// A value of PrevRank.
    /// </summary>
    public DashboardStagingPriority12 PrevRank
    {
      get => prevRank ??= new();
      set => prevRank = value;
    }

    /// <summary>
    /// A value of PreviousYear.
    /// </summary>
    public DashboardStagingPriority12 PreviousYear
    {
      get => previousYear ??= new();
      set => previousYear = value;
    }

    /// <summary>
    /// A value of Contractor1.
    /// </summary>
    public CseOrganization Contractor1
    {
      get => contractor1 ??= new();
      set => contractor1 = value;
    }

    /// <summary>
    /// Gets a value of Contractor.
    /// </summary>
    [JsonIgnore]
    public Array<ContractorGroup> Contractor => contractor ??= new(
      ContractorGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Contractor for json serialization.
    /// </summary>
    [JsonPropertyName("contractor")]
    [Computed]
    public IList<ContractorGroup>? Contractor_Json
    {
      get => contractor;
      set => Contractor.Assign(value);
    }

    /// <summary>
    /// A value of DetermineContractor.
    /// </summary>
    public DashboardStagingPriority12 DetermineContractor
    {
      get => determineContractor ??= new();
      set => determineContractor = value;
    }

    /// <summary>
    /// A value of Current.
    /// </summary>
    public DashboardStagingPriority12 Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of PrevAtty.
    /// </summary>
    public ServiceProvider PrevAtty
    {
      get => prevAtty ??= new();
      set => prevAtty = value;
    }

    private DateWorkArea? fiscalYear;
    private DashboardStagingPriority35? previousRank;
    private DashboardStagingPriority35? nullDashboardStagingPriority35;
    private DashboardStagingPriority35? dashboardStagingPriority35;
    private DashboardAuditData? worker;
    private Common? countCaseAtty;
    private DashboardStagingPriority12? initialize;
    private Common? countCaseWk;
    private DashboardStagingPriority12? delete;
    private DashboardAuditData? initialized;
    private ProgramCheckpointRestart? programCheckpointRestart;
    private DashboardStagingPriority12? statewide;
    private EabFileHandling? eabFileHandling;
    private EabReportSend? eabReportSend;
    private Array<LocalGroup>? local1;
    private Case1? restart;
    private Case1? prev;
    private Common? recordsReadSinceCommit;
    private DashboardAuditData? dashboardAuditData;
    private Common? countCase;
    private Program? program;
    private ReportTotals? balDueOnFyEnd;
    private DateWorkArea? tempEndDate;
    private Common? collFound;
    private CsePerson? ap;
    private CsePerson? supp;
    private Collection? nonNa;
    private DateWorkArea? collectionDate;
    private DateWorkArea? nullDateWorkArea;
    private Common? assistance;
    private DashboardStagingPriority12? temp;
    private Common? common;
    private DashboardStagingPriority12? prevRank;
    private DashboardStagingPriority12? previousYear;
    private CseOrganization? contractor1;
    private Array<ContractorGroup>? contractor;
    private DashboardStagingPriority12? determineContractor;
    private DashboardStagingPriority12? current;
    private ServiceProvider? prevAtty;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of DisbursementTransactionRln.
    /// </summary>
    public DisbursementTransactionRln DisbursementTransactionRln
    {
      get => disbursementTransactionRln ??= new();
      set => disbursementTransactionRln = value;
    }

    /// <summary>
    /// A value of Debit.
    /// </summary>
    public DisbursementTransaction Debit
    {
      get => debit ??= new();
      set => debit = value;
    }

    /// <summary>
    /// A value of Credit.
    /// </summary>
    public DisbursementTransaction Credit
    {
      get => credit ??= new();
      set => credit = value;
    }

    /// <summary>
    /// A value of PaymentRequest.
    /// </summary>
    public PaymentRequest PaymentRequest
    {
      get => paymentRequest ??= new();
      set => paymentRequest = value;
    }

    /// <summary>
    /// A value of DashboardStagingPriority35.
    /// </summary>
    public DashboardStagingPriority35 DashboardStagingPriority35
    {
      get => dashboardStagingPriority35 ??= new();
      set => dashboardStagingPriority35 = value;
    }

    /// <summary>
    /// A value of WorkerCaseAssignment.
    /// </summary>
    public CaseAssignment WorkerCaseAssignment
    {
      get => workerCaseAssignment ??= new();
      set => workerCaseAssignment = value;
    }

    /// <summary>
    /// A value of WorkerServiceProvider.
    /// </summary>
    public ServiceProvider WorkerServiceProvider
    {
      get => workerServiceProvider ??= new();
      set => workerServiceProvider = value;
    }

    /// <summary>
    /// A value of PreviousYear.
    /// </summary>
    public DashboardStagingPriority12 PreviousYear
    {
      get => previousYear ??= new();
      set => previousYear = value;
    }

    /// <summary>
    /// A value of CseOrganization.
    /// </summary>
    public CseOrganization CseOrganization
    {
      get => cseOrganization ??= new();
      set => cseOrganization = value;
    }

    /// <summary>
    /// A value of DashboardStagingPriority12.
    /// </summary>
    public DashboardStagingPriority12 DashboardStagingPriority12
    {
      get => dashboardStagingPriority12 ??= new();
      set => dashboardStagingPriority12 = value;
    }

    /// <summary>
    /// A value of Case1.
    /// </summary>
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of ObligationType.
    /// </summary>
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
    }

    /// <summary>
    /// A value of ChOrArCsePerson.
    /// </summary>
    public CsePerson ChOrArCsePerson
    {
      get => chOrArCsePerson ??= new();
      set => chOrArCsePerson = value;
    }

    /// <summary>
    /// A value of DebtDetail.
    /// </summary>
    public DebtDetail DebtDetail
    {
      get => debtDetail ??= new();
      set => debtDetail = value;
    }

    /// <summary>
    /// A value of Obligation.
    /// </summary>
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

    /// <summary>
    /// A value of DebtAdjustment.
    /// </summary>
    public ObligationTransaction DebtAdjustment
    {
      get => debtAdjustment ??= new();
      set => debtAdjustment = value;
    }

    /// <summary>
    /// A value of ObligationTransactionRln.
    /// </summary>
    public ObligationTransactionRln ObligationTransactionRln
    {
      get => obligationTransactionRln ??= new();
      set => obligationTransactionRln = value;
    }

    /// <summary>
    /// A value of Debt.
    /// </summary>
    public ObligationTransaction Debt
    {
      get => debt ??= new();
      set => debt = value;
    }

    /// <summary>
    /// A value of Collection.
    /// </summary>
    public Collection Collection
    {
      get => collection ??= new();
      set => collection = value;
    }

    /// <summary>
    /// A value of ApCsePerson.
    /// </summary>
    public CsePerson ApCsePerson
    {
      get => apCsePerson ??= new();
      set => apCsePerson = value;
    }

    /// <summary>
    /// A value of Obligor.
    /// </summary>
    public CsePersonAccount Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
    }

    /// <summary>
    /// A value of Supported.
    /// </summary>
    public CsePersonAccount Supported
    {
      get => supported ??= new();
      set => supported = value;
    }

    /// <summary>
    /// A value of Adjusted.
    /// </summary>
    public Collection Adjusted
    {
      get => adjusted ??= new();
      set => adjusted = value;
    }

    /// <summary>
    /// A value of CashReceiptDetail.
    /// </summary>
    public CashReceiptDetail CashReceiptDetail
    {
      get => cashReceiptDetail ??= new();
      set => cashReceiptDetail = value;
    }

    /// <summary>
    /// A value of CashReceipt.
    /// </summary>
    public CashReceipt CashReceipt
    {
      get => cashReceipt ??= new();
      set => cashReceipt = value;
    }

    /// <summary>
    /// A value of CashReceiptType.
    /// </summary>
    public CashReceiptType CashReceiptType
    {
      get => cashReceiptType ??= new();
      set => cashReceiptType = value;
    }

    /// <summary>
    /// A value of ApCaseRole.
    /// </summary>
    public CaseRole ApCaseRole
    {
      get => apCaseRole ??= new();
      set => apCaseRole = value;
    }

    /// <summary>
    /// A value of ChOrArCaseRole.
    /// </summary>
    public CaseRole ChOrArCaseRole
    {
      get => chOrArCaseRole ??= new();
      set => chOrArCaseRole = value;
    }

    /// <summary>
    /// A value of CaseAssignment.
    /// </summary>
    public CaseAssignment CaseAssignment
    {
      get => caseAssignment ??= new();
      set => caseAssignment = value;
    }

    /// <summary>
    /// A value of Supp.
    /// </summary>
    public CsePerson Supp
    {
      get => supp ??= new();
      set => supp = value;
    }

    /// <summary>
    /// A value of AfterFy.
    /// </summary>
    public Collection AfterFy
    {
      get => afterFy ??= new();
      set => afterFy = value;
    }

    /// <summary>
    /// A value of CollectionType.
    /// </summary>
    public CollectionType CollectionType
    {
      get => collectionType ??= new();
      set => collectionType = value;
    }

    /// <summary>
    /// A value of ServiceProvider.
    /// </summary>
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
    }

    /// <summary>
    /// A value of OfficeServiceProvider.
    /// </summary>
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
    }

    /// <summary>
    /// A value of LegalReferral.
    /// </summary>
    public LegalReferral LegalReferral
    {
      get => legalReferral ??= new();
      set => legalReferral = value;
    }

    /// <summary>
    /// A value of LegalReferralAssignment.
    /// </summary>
    public LegalReferralAssignment LegalReferralAssignment
    {
      get => legalReferralAssignment ??= new();
      set => legalReferralAssignment = value;
    }

    private DisbursementTransactionRln? disbursementTransactionRln;
    private DisbursementTransaction? debit;
    private DisbursementTransaction? credit;
    private PaymentRequest? paymentRequest;
    private DashboardStagingPriority35? dashboardStagingPriority35;
    private CaseAssignment? workerCaseAssignment;
    private ServiceProvider? workerServiceProvider;
    private DashboardStagingPriority12? previousYear;
    private CseOrganization? cseOrganization;
    private DashboardStagingPriority12? dashboardStagingPriority12;
    private Case1? case1;
    private ObligationType? obligationType;
    private CsePerson? chOrArCsePerson;
    private DebtDetail? debtDetail;
    private Obligation? obligation;
    private ObligationTransaction? debtAdjustment;
    private ObligationTransactionRln? obligationTransactionRln;
    private ObligationTransaction? debt;
    private Collection? collection;
    private CsePerson? apCsePerson;
    private CsePersonAccount? obligor;
    private CsePersonAccount? supported;
    private Collection? adjusted;
    private CashReceiptDetail? cashReceiptDetail;
    private CashReceipt? cashReceipt;
    private CashReceiptType? cashReceiptType;
    private CaseRole? apCaseRole;
    private CaseRole? chOrArCaseRole;
    private CaseAssignment? caseAssignment;
    private CsePerson? supp;
    private Collection? afterFy;
    private CollectionType? collectionType;
    private ServiceProvider? serviceProvider;
    private OfficeServiceProvider? officeServiceProvider;
    private LegalReferral? legalReferral;
    private LegalReferralAssignment? legalReferralAssignment;
  }
#endregion
}
