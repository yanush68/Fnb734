// Program: FN_B734_PRIORITY_1_1, ID: 945117536, model: 746.
// Short name: SWE03079
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
/// A program: FN_B734_PRIORITY_1_1.
/// </para>
/// <para>
/// Priority 1-1: Cases With Support Orders
/// </para>
/// </summary>
[Serializable]
[Program("SWE03079")]
public partial class FnB734Priority11: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B734_PRIORITY_1_1 program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB734Priority11(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB734Priority11.
  /// </summary>
  public FnB734Priority11(IContext context, Import import, Export export):
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
    // 10/21/15  DDupree	CQ46954		Add a rollup to the contractor level.  Also 
    // add % change
    // 					from previous year values.
    // ---------------------------------------------------------------------------------------------------
    // -------------------------------------------------------------------------------------
    // Priority 1-1: % Cases Under Order
    // -------------------------------------------------------------------------------------
    // The total number of cases with an order divided by total number of cases 
    // (OCSE157 Line2/ OCSE157 Line1)
    // Report Level: State, Judicial District
    // Report Period: Month (Fiscal Year-to-date calculation)
    // Numerator:
    // Number of Cases with Order
    // 	1) Cases open at the end of the reporting period with a J or O class 
    // legal
    // 	   action and a legal detail with no end date and an obligation type of 
    // CRCH,
    // 	   CS, MS, AJ, MJ-NA, MJ-NAI, MJ-NC, MJ-NF, ZCS, HIC or UM.
    // 	2) Must read for AP/CH combination (defined on LROL screen for non-
    // financial
    // 	   legal details and LOPS screen for financial legal details.)
    // 	3) Case roles can be open or closed.
    // 	4) Any case where AP/CH overlap/were active at the same time.  Case 
    // roles do
    // 	   not have to have been open during the reporting period.
    // 	5) Read for any J or O class legal action.
    // 	6) Do not read for open legal detail on financial obligations (financial
    // 	   obligations must be obligated)
    // 	7) Do count if non-financial legal detail is ended, but the end date is 
    // in
    // 	   the future (after report period end).  *Current OCSE 157 logic doesn
    // t
    // 	   evaluate the end date.  We are not using for Dashboard coding.  End 
    // date
    // 	   can be entered at any time and legal detail will still be counted.
    // 	8) Do count if obligation was created any time during or prior to the
    // 	   reporting period.
    // 	9) Read Legal Action Case Role for HIC & UM legal details
    // 	10) Count case if there was a cash or medical support order at one time,
    // but
    // 	    there is no money owed now, or the medical support is no longer in
    // 	    effect.
    // 	11) EP should not be considered for this line.
    // 	12) The legal action must have a filed date.
    // Denominator:
    // Number of Open Cases
    // 	1) Cases open at the end of the reporting period (should match case 
    // count
    // 	   from CASL for each caseworker within a specific office
    // 	   Exception: cases with no jurisdiction will be excluded).
    // -------------------------------------------------------------------------------------
    MoveDashboardAuditData1(import.DashboardAuditData, local.Initialized);
    MoveProgramCheckpointRestart(import.ProgramCheckpointRestart,
      local.ProgramCheckpointRestart);

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
      local.Local1.Update.G.ContractorNumber =
        local.ContractorCseOrganization.Code;

      if (local.Contractor.Count < 1)
      {
        ++local.Contractor.Index;
        local.Contractor.CheckSize();

        local.Contractor.Update.Gcontractor.ContractorNumber =
          local.ContractorCseOrganization.Code;
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

          if (Equal(local.ContractorCseOrganization.Code,
            local.Contractor.Item.Gcontractor.ContractorNumber))
          {
            goto ReadEach1;
          }
        }

        local.Contractor.CheckIndex();

        local.Contractor.Index = local.Contractor.Count;
        local.Contractor.CheckSize();

        local.Contractor.Update.Gcontractor.ContractorNumber =
          local.ContractorCseOrganization.Code;
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

    // ------------------------------------------------------------------------------
    // -- Determine if we're restarting and set appropriate restart information.
    // ------------------------------------------------------------------------------
    if (AsChar(import.ProgramCheckpointRestart.RestartInd) == 'Y' && Equal
      (import.ProgramCheckpointRestart.RestartInfo, 81, 8, "1-01    "))
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
          local.Statewide.CasesUnderOrderDenominator = 0;
          local.Statewide.CasesUnderOrderNumerator = 0;
          local.Statewide.CasesUnderOrderPercent = 0;
          local.Statewide.CasesUnderOrderRank = 0;
        }

        // -- Load Judicial District counts.
        foreach(var _ in ReadDashboardStagingPriority4())
        {
          local.Local1.Index =
            (int)StringToNumber(entities.DashboardStagingPriority12.
              ReportLevelId) - 1;
          local.Local1.CheckSize();

          local.Local1.Update.G.Assign(entities.DashboardStagingPriority12);
          local.Local1.Update.G.CasesUnderOrderDenominator = 0;
          local.Local1.Update.G.CasesUnderOrderNumerator = 0;
          local.Local1.Update.G.CasesUnderOrderPercent = 0;
          local.Local1.Update.G.CasesUnderOrderRank = 0;
        }
      }
    }
    else
    {
      local.Restart.Number = "";
    }

    // ------------------------------------------------------------------------------
    // -- Read each open case.
    // ------------------------------------------------------------------------------
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
        if (local.RecordsReadSinceCommit.Count > (
          import.ProgramCheckpointRestart.ReadFrequencyCount ?? 0))
        {
          // -- Save statewide counts.
          if (ReadDashboardStagingPriority5())
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
          for(local.Local1.Index = 0; local.Local1.Index < local.Local1.Count; ++
            local.Local1.Index)
          {
            if (!local.Local1.CheckSize())
            {
              break;
            }

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
            "1-01    " + " " + String
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
      local.DashboardAuditData.Assign(local.Initialized);

      // -- Determine office and judicial district to which case is assigned on 
      // the report period end date.
      UseFnB734DetermineJdFromCase();

      // -------------------------------------------------------------------------------------
      // --  D E N O M I N A T O R  (Number of Open Cases) (OCSE157 Line 1)
      // -------------------------------------------------------------------------------------
      // -- Include case in the Priority 1-1 denominator (Number of Open Cases).
      // -- This is the same as OCSE157 Line 1.
      // -- Increment Statewide Level
      local.Statewide.CasesUnderOrderDenominator =
        (local.Statewide.CasesUnderOrderDenominator ?? 0) + 1;

      // -- Increment Judicial District Level
      if (!IsEmpty(local.DashboardAuditData.JudicialDistrict))
      {
        local.Local1.Index =
          (int)StringToNumber(local.DashboardAuditData.JudicialDistrict) - 1;
        local.Local1.CheckSize();

        local.Local1.Update.G.CasesUnderOrderDenominator =
          (local.Local1.Item.G.CasesUnderOrderDenominator ?? 0) + 1;

        // add set statements her for contractor's attributes
        local.DashboardAuditData.ContractorNumber =
          local.Local1.Item.G.ContractorNumber;
      }

      if (AsChar(import.AuditFlag.Flag) == 'Y')
      {
        // -- Log to the dashboard audit table.
        local.DashboardAuditData.DashboardPriority = "1-1(D)";
        UseFnB734CreateDashboardAudit();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }
      }

      // -------------------------------------------------------------------------------------
      // -- N U M E R A T O R  (Number of Cases with an Order) (OCSE157 Line 2)
      // -------------------------------------------------------------------------------------
      local.NonFinLdet.Flag = "N";
      local.FinLdet.Flag = "N";
      local.AccrualInstrFound.Flag = "N";

      // ----------------------------------------------------------------------
      // For current case, read all valid AP/CH combos - active or not.
      // Date checks will ensure we read overlapping AP/CH roles only.
      // If fin ldet found, count in line 2. Don't count case in 2c.
      // If non-fin ldet found, count in line 2. We still need to check if
      // fin LDET exists. This is necessary for line 2c.
      // -----------------------------------------------------------------------
      foreach(var _1 in ReadCaseRoleCsePersonCaseRoleCsePerson())
      {
        // ----------------------------------------------------------------------
        // Using LROL, read J-class HIC or UM ldet - active or not.
        // Skip Legal Actions created after the end of FY.
        // Skip LDETs created after the end of FY.
        // Also include LDETs created in previous FYs.
        // ----------------------------------------------------------------------
        if (AsChar(local.NonFinLdet.Flag) == 'N')
        {
          if (ReadLegalActionDetailLegalAction())
          {
            local.NonFinLdet.Flag = "Y";

            if (AsChar(local.FinLdet.Flag) == 'Y')
            {
              // ----------------------------------------------------------------------
              // We found a fin and non-fin LDET for this case.
              // No further processing is necessary for this case.
              // ----------------------------------------------------------------------
              break;
            }

            local.DashboardAuditData.DashboardPriority = "1-1(N)#1";
            local.DashboardAuditData.SuppCspNumber =
              entities.ChCsePerson.Number;
            local.DashboardAuditData.PayorCspNumber =
              entities.ApCsePerson.Number;
            local.DashboardAuditData.StandardNumber =
              entities.LegalAction.StandardNumber;
            local.DashboardAuditData.DebtType =
              entities.LegalActionDetail.NonFinOblgType;

            break;
          }
        }

        // ----------------------------------------------------------------------
        // Using LOPS, read all J-class fin LDETs - active or not.
        // Read for Obligations with specific ob types.
        // Skip Legal Actions created after the end of FY.
        // Skip LDETs created after the end of FY.
        // Also include LDETs created in previous FYs.
        // ----------------------------------------------------------------------
        if (AsChar(local.FinLdet.Flag) == 'N')
        {
          foreach(var _2 in ReadLegalActionDetailLegalActionObligationType())
          {
            if (entities.ObligationType.Populated)
            {
              // -------------------------------------------------------------------
              // We found a finance LDET with desired Ob types.
              // Now check if Accrual Instructions were 'ever' setup for Current
              // Obligation.
              // Qualify by supported person.
              // --------------------------------------------------------------------
              if (AsChar(entities.ObligationType.Classification) == 'A')
              {
                if (ReadAccrualInstructions())
                {
                  local.FinLdet.Flag = "Y";
                  local.DashboardAuditData.DashboardPriority = "1-1(N)#2";
                  local.DashboardAuditData.SuppCspNumber =
                    entities.ChCsePerson.Number;
                  local.DashboardAuditData.PayorCspNumber =
                    entities.ApCsePerson.Number;
                  local.DashboardAuditData.StandardNumber =
                    entities.LegalAction.StandardNumber;
                  local.DashboardAuditData.DebtType =
                    entities.ObligationType.Code;

                  // ----------------------------------------------------------------------
                  // We found a fin-LDET for this case.
                  // No further processing is necessary for this case.
                  // ----------------------------------------------------------------------
                  goto ReadEach2;
                }
              }

              // -------------------------------------------------------------------
              // We got here because Accrual Instructions were never setup
              // on current Obligation.
              // Now check if debt was 'ever' owed on this obligation.
              // -------------------------------------------------------------------
              // ----------------------------------------------
              // Qualify Debts by Supp person. 7/18/01
              // Only read debts created before FY end.
              // ----------------------------------------------
              foreach(var _3 in ReadDebtDebtDetail())
              {
                // -----------------------------------------------
                // Skip MJ AF/FC.
                // -----------------------------------------------
                if (Equal(entities.ObligationType.Code, "MJ"))
                {
                  // -----------------------------------------------
                  // CAB defaults Coll date to Current date. So don't pass 
                  // anything.
                  // -----------------------------------------------
                  UseFnDeterminePgmForDebtDetail();

                  if (Equal(local.Program.Code, "AF") || Equal
                    (local.Program.Code, "AFI") || Equal
                    (local.Program.Code, "FC") || Equal
                    (local.Program.Code, "FCI"))
                  {
                    // -----------------------------------------------
                    // Skip this debt detail.
                    // -----------------------------------------------
                    continue;
                  }
                }

                local.DashboardAuditData.DashboardPriority = "1-1(N)#3";
                local.DashboardAuditData.SuppCspNumber =
                  entities.ChCsePerson.Number;
                local.DashboardAuditData.PayorCspNumber =
                  entities.ApCsePerson.Number;
                local.DashboardAuditData.StandardNumber =
                  entities.LegalAction.StandardNumber;
                local.DashboardAuditData.DebtType =
                  entities.ObligationType.Code;
                local.DashboardAuditData.DebtDueDate =
                  entities.DebtDetail.DueDt;
                local.DashboardAuditData.DebtBalanceDue =
                  entities.DebtDetail.BalanceDueAmt;
                local.FinLdet.Flag = "Y";

                // ----------------------------------------------------------------------
                // We found a fin LDET for this case.
                // No further processing is necessary for this case.
                // ----------------------------------------------------------------------
                goto ReadEach2;
              }
            }
          }
        }
      }

ReadEach2:

      if (AsChar(local.FinLdet.Flag) == 'N' && AsChar
        (local.NonFinLdet.Flag) == 'N')
      {
        // -- Case is not under order.  Go to next case.
      }
      else
      {
        // -- Case is under order.  Count in the Priority 1-1 Numerator and log 
        // to the Dashboard Audit Table.
        // -- Increment Statewide Level
        local.Statewide.CasesUnderOrderNumerator =
          (local.Statewide.CasesUnderOrderNumerator ?? 0) + 1;

        // -- Increment Judicial District Level
        if (!IsEmpty(local.DashboardAuditData.JudicialDistrict))
        {
          local.Local1.Index =
            (int)StringToNumber(local.DashboardAuditData.JudicialDistrict) - 1;
          local.Local1.CheckSize();

          local.Local1.Update.G.CasesUnderOrderNumerator =
            (local.Local1.Item.G.CasesUnderOrderNumerator ?? 0) + 1;
        }

        if (AsChar(import.AuditFlag.Flag) == 'Y')
        {
          // -- Log to the dashboard audit table.
          UseFnB734CreateDashboardAudit();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            return;
          }
        }
      }
    }

    // ------------------------------------------------------------------------------
    // -- Store final Statewide counts.
    // ------------------------------------------------------------------------------
    if (ReadDashboardStagingPriority5())
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
          local.Contractor.Update.Gcontractor.CasesUnderOrderDenominator =
            (local.Contractor.Item.Gcontractor.CasesUnderOrderDenominator ?? 0) +
            (local.Local1.Item.G.CasesUnderOrderDenominator ?? 0);
          local.Contractor.Update.Gcontractor.CasesUnderOrderNumerator =
            (local.Contractor.Item.Gcontractor.CasesUnderOrderNumerator ?? 0) +
            (local.Local1.Item.G.CasesUnderOrderNumerator ?? 0);

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
    foreach(var _ in ReadDashboardStagingPriority7())
    {
      local.DashboardAuditData.JudicialDistrict =
        entities.PreviousYear.ReportLevelId;
      UseFnB734DeterContractorFromJd();

      // -- Add previous years Cases Under Order values to appropriate 
      // contractor.
      for(local.Contractor.Index = 0; local.Contractor.Index < local
        .Contractor.Count; ++local.Contractor.Index)
      {
        if (!local.Contractor.CheckSize())
        {
          break;
        }

        if (Equal(local.Contractor.Item.Gcontractor.ContractorNumber,
          local.ContractorCseOrganization.Code))
        {
          local.Contractor.Update.Gcontractor.PrevYrCaseNumerator =
            (local.Contractor.Item.Gcontractor.PrevYrCaseNumerator ?? 0) + (
              entities.PreviousYear.CasesUnderOrderNumerator ?? 0);
          local.Contractor.Update.Gcontractor.PrevYrCaseDenominator =
            (local.Contractor.Item.Gcontractor.PrevYrCaseDenominator ?? 0) + (
              entities.PreviousYear.CasesUnderOrderDenominator ?? 0);

          goto ReadEach3;
        }
      }

      local.Contractor.CheckIndex();

ReadEach3:
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

      if (ReadDashboardStagingPriority8())
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
          CreateDashboardStagingPriority3();
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
    // Under
    // -- Order Percent, Previous Year Cases Under Order Percent, and Percent 
    // Change
    // -- from the Previous Year.
    // ------------------------------------------------------------------------------
    foreach(var _ in ReadDashboardStagingPriority9())
    {
      local.Temp.Assign(entities.DashboardStagingPriority12);

      if ((local.Temp.CasesUnderOrderDenominator ?? 0) == 0)
      {
        local.Temp.CasesUnderOrderPercent = 0;
      }
      else
      {
        local.Temp.CasesUnderOrderPercent =
          Math.Round((decimal)(local.Temp.CasesUnderOrderNumerator ?? 0) /
          (local.Temp.CasesUnderOrderDenominator ?? 0), 3,
          MidpointRounding.AwayFromZero);
      }

      // -- Read for the previous year Cases Under Order values for all but the 
      // contractor level.
      // -- The contractor level previous year values were calculated and stored
      // earlier.
      if (!Equal(entities.DashboardStagingPriority12.ReportLevel, "XJ"))
      {
        if (ReadDashboardStagingPriority10())
        {
          local.Temp.PrevYrCaseNumerator =
            entities.PreviousYear.CasesUnderOrderNumerator;
          local.Temp.PrevYrCaseDenominator =
            entities.PreviousYear.CasesUnderOrderDenominator;
        }
        else
        {
          local.Temp.PrevYrCaseNumerator = 0;
          local.Temp.PrevYrCaseDenominator = 0;
        }
      }

      // -- Calculate Previous Year Cases Under Order percent.
      if ((local.Temp.PrevYrCaseDenominator ?? 0) == 0)
      {
        local.Temp.CasesUndrOrdrPrevYrPct = 0;
      }
      else
      {
        local.Temp.CasesUndrOrdrPrevYrPct =
          Math.Round((decimal)(local.Temp.PrevYrCaseNumerator ?? 0) /
          (local.Temp.PrevYrCaseDenominator ?? 0), 3,
          MidpointRounding.AwayFromZero);
      }

      // -- Calculate percent change between Current Year Cases Under Order 
      // percent
      //    and Previous Year Cases Under Order percent.
      if ((local.Temp.CasesUndrOrdrPrevYrPct ?? 0M) == 0)
      {
        local.Temp.PctChgBtwenYrsCaseUndrOrdr = 0;
      }
      else
      {
        local.Temp.PctChgBtwenYrsCaseUndrOrdr =
          Math.Round(((local.Temp.CasesUnderOrderPercent ?? 0M) - (
            local.Temp.CasesUndrOrdrPrevYrPct ?? 0M
          )) /
          (local.Temp.CasesUndrOrdrPrevYrPct ?? 0M), 3,
          MidpointRounding.AwayFromZero);
      }

      try
      {
        UpdateDashboardStagingPriority4();
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

    local.Common.Count = 0;
    local.PrevRank.CasesUnderOrderPercent = 0;
    local.Temp.CasesUnderOrderRank = 1;

    // ------------------------------------------------------------------------------
    // -- Calculate Judicial District Ranking.
    // ------------------------------------------------------------------------------
    foreach(var _ in ReadDashboardStagingPriority11())
    {
      ++local.Common.Count;

      if ((entities.DashboardStagingPriority12.CasesUnderOrderPercent ?? 0M) ==
        (local.PrevRank.CasesUnderOrderPercent ?? 0M))
      {
        // -- The ranking for this judicial district is tied with the previous 
        // judicial district.
        // -- This JD gets the same ranking already in the local_temp 
        // case_under_order_rank.
      }
      else
      {
        local.Temp.CasesUnderOrderRank = local.Common.Count;
      }

      try
      {
        UpdateDashboardStagingPriority5();
        local.PrevRank.CasesUnderOrderPercent =
          entities.DashboardStagingPriority12.CasesUnderOrderPercent;
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

    local.Common.Count = 0;
    local.PrevRank.CasesUnderOrderPercent = 0;
    local.Temp.CasesUnderOrderRank = 1;

    // ------------------------------------------------------------------------------
    // -- Calculate Contractor Ranking.
    // ------------------------------------------------------------------------------
    foreach(var _ in ReadDashboardStagingPriority13())
    {
      ++local.Common.Count;

      if ((entities.DashboardStagingPriority12.CasesUnderOrderPercent ?? 0M) ==
        (local.PrevRank.CasesUnderOrderPercent ?? 0M))
      {
        // -- The ranking for this judicial district is tied with the previous 
        // judicial district.
        // -- This JD gets the same ranking already in the local_temp 
        // case_under_order_rank.
      }
      else
      {
        local.Temp.CasesUnderOrderRank = local.Common.Count;
      }

      try
      {
        UpdateDashboardStagingPriority5();
        local.PrevRank.CasesUnderOrderPercent =
          entities.DashboardStagingPriority12.CasesUnderOrderPercent;
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
      Substring(import.ProgramCheckpointRestart.RestartInfo, 250, 1, 80) + "1-02    ";
    UseUpdateCheckpointRstAndCommit();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "Error taking checkpoint.";
      UseCabErrorReport();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }
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
    target.RunNumber = source.RunNumber;
  }

  private static void MoveDashboardAuditData2(DashboardAuditData source,
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

  private void UseFnB734CreateDashboardAudit()
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

    MoveCseOrganization(useExport.Contractor, local.ContractorCseOrganization);
  }

  private void UseFnB734DetermineJdFromCase()
  {
    var useImport = new FnB734DetermineJdFromCase.Import();
    var useExport = new FnB734DetermineJdFromCase.Export();

    useImport.Case1.Number = entities.Case1.Number;
    useImport.ReportEndDate.Date = import.ReportEndDate.Date;

    context.Call(FnB734DetermineJdFromCase.Execute, useImport, useExport);

    MoveDashboardAuditData2(useExport.DashboardAuditData,
      local.DashboardAuditData);
  }

  private void UseFnDeterminePgmForDebtDetail()
  {
    var useImport = new FnDeterminePgmForDebtDetail.Import();
    var useExport = new FnDeterminePgmForDebtDetail.Export();

    useImport.SupportedPerson.Number = entities.ChCsePerson.Number;
    MoveObligationType(entities.ObligationType, useImport.ObligationType);

    MoveDebtDetail(entities.DebtDetail, useImport.DebtDetail);

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
    var casesUnderOrderNumerator = local.Statewide.CasesUnderOrderNumerator ?? 0
      ;
    var casesUnderOrderDenominator =
      local.Statewide.CasesUnderOrderDenominator ?? 0;
    var casesUnderOrderPercent = local.Statewide.CasesUnderOrderPercent ?? 0M;
    var casesUnderOrderRank = local.Statewide.CasesUnderOrderRank ?? 0;
    var param = 0M;

    entities.DashboardStagingPriority12.Populated = false;
    Update("CreateDashboardStagingPriority1",
      (db, command) =>
      {
        db.SetInt32(command, "reportMonth", reportMonth);
        db.SetString(command, "reportLevel", reportLevel);
        db.SetString(command, "reportLevelId", reportLevelId);
        db.SetNullableDate(command, "asOfDt", asOfDate);
        db.
          SetNullableInt32(command, "casUnderOrdNum", casesUnderOrderNumerator);
        db.SetNullableInt32(
          command, "casUnderOrdDen", casesUnderOrderDenominator);
        db.
          SetNullableDecimal(command, "casUnderOrdPer", casesUnderOrderPercent);
        db.SetNullableInt32(command, "casUnderOrdRnk", casesUnderOrderRank);
        db.SetNullableInt32(command, "pepNum", 0);
        db.SetNullableDecimal(command, "pepPer", param);
        db.SetNullableDecimal(command, "curSupPdMthNum", param);
        db.SetNullableDecimal(command, "casPerFteDen", param);
        db.SetNullableString(command, "contractorNum", "");
        db.SetNullableInt32(command, "prvYrCaseNumtr", 0);
        db.SetNullableInt32(command, "prvYrCaseDenom", 0);
        db.SetNullableDecimal(command, "prvYrCasPctUo", param);
        db.SetNullableDecimal(command, "pctChgByrCasUo", param);
      });

    entities.DashboardStagingPriority12.ReportMonth = reportMonth;
    entities.DashboardStagingPriority12.ReportLevel = reportLevel;
    entities.DashboardStagingPriority12.ReportLevelId = reportLevelId;
    entities.DashboardStagingPriority12.AsOfDate = asOfDate;
    entities.DashboardStagingPriority12.CasesUnderOrderNumerator =
      casesUnderOrderNumerator;
    entities.DashboardStagingPriority12.CasesUnderOrderDenominator =
      casesUnderOrderDenominator;
    entities.DashboardStagingPriority12.CasesUnderOrderPercent =
      casesUnderOrderPercent;
    entities.DashboardStagingPriority12.CasesUnderOrderRank =
      casesUnderOrderRank;
    entities.DashboardStagingPriority12.ContractorNumber = "";
    entities.DashboardStagingPriority12.PrevYrCaseNumerator = 0;
    entities.DashboardStagingPriority12.PrevYrCaseDenominator = 0;
    entities.DashboardStagingPriority12.CasesUndrOrdrPrevYrPct = param;
    entities.DashboardStagingPriority12.PctChgBtwenYrsCaseUndrOrdr = param;
    entities.DashboardStagingPriority12.Populated = true;
  }

  private void CreateDashboardStagingPriority2()
  {
    var reportMonth = local.Local1.Item.G.ReportMonth;
    var reportLevel = local.Local1.Item.G.ReportLevel;
    var reportLevelId = local.Local1.Item.G.ReportLevelId;
    var asOfDate = local.Local1.Item.G.AsOfDate;
    var casesUnderOrderNumerator =
      local.Local1.Item.G.CasesUnderOrderNumerator ?? 0;
    var casesUnderOrderDenominator =
      local.Local1.Item.G.CasesUnderOrderDenominator ?? 0;
    var casesUnderOrderPercent = local.Local1.Item.G.CasesUnderOrderPercent ?? 0M
      ;
    var casesUnderOrderRank = local.Local1.Item.G.CasesUnderOrderRank ?? 0;
    var param = 0M;
    var contractorNumber = local.Local1.Item.G.ContractorNumber ?? "";

    entities.DashboardStagingPriority12.Populated = false;
    Update("CreateDashboardStagingPriority2",
      (db, command) =>
      {
        db.SetInt32(command, "reportMonth", reportMonth);
        db.SetString(command, "reportLevel", reportLevel);
        db.SetString(command, "reportLevelId", reportLevelId);
        db.SetNullableDate(command, "asOfDt", asOfDate);
        db.
          SetNullableInt32(command, "casUnderOrdNum", casesUnderOrderNumerator);
        db.SetNullableInt32(
          command, "casUnderOrdDen", casesUnderOrderDenominator);
        db.
          SetNullableDecimal(command, "casUnderOrdPer", casesUnderOrderPercent);
        db.SetNullableInt32(command, "casUnderOrdRnk", casesUnderOrderRank);
        db.SetNullableInt32(command, "pepNum", 0);
        db.SetNullableDecimal(command, "pepPer", param);
        db.SetNullableDecimal(command, "curSupPdMthNum", param);
        db.SetNullableDecimal(command, "casPerFteDen", param);
        db.SetNullableString(command, "contractorNum", contractorNumber);
        db.SetNullableInt32(command, "prvYrCaseNumtr", 0);
        db.SetNullableInt32(command, "prvYrCaseDenom", 0);
        db.SetNullableDecimal(command, "prvYrCasPctUo", param);
        db.SetNullableDecimal(command, "pctChgByrCasUo", param);
      });

    entities.DashboardStagingPriority12.ReportMonth = reportMonth;
    entities.DashboardStagingPriority12.ReportLevel = reportLevel;
    entities.DashboardStagingPriority12.ReportLevelId = reportLevelId;
    entities.DashboardStagingPriority12.AsOfDate = asOfDate;
    entities.DashboardStagingPriority12.CasesUnderOrderNumerator =
      casesUnderOrderNumerator;
    entities.DashboardStagingPriority12.CasesUnderOrderDenominator =
      casesUnderOrderDenominator;
    entities.DashboardStagingPriority12.CasesUnderOrderPercent =
      casesUnderOrderPercent;
    entities.DashboardStagingPriority12.CasesUnderOrderRank =
      casesUnderOrderRank;
    entities.DashboardStagingPriority12.ContractorNumber = contractorNumber;
    entities.DashboardStagingPriority12.PrevYrCaseNumerator = 0;
    entities.DashboardStagingPriority12.PrevYrCaseDenominator = 0;
    entities.DashboardStagingPriority12.CasesUndrOrdrPrevYrPct = param;
    entities.DashboardStagingPriority12.PctChgBtwenYrsCaseUndrOrdr = param;
    entities.DashboardStagingPriority12.Populated = true;
  }

  private void CreateDashboardStagingPriority3()
  {
    var reportMonth = local.Contractor.Item.Gcontractor.ReportMonth;
    var reportLevel = local.Contractor.Item.Gcontractor.ReportLevel;
    var reportLevelId = local.Contractor.Item.Gcontractor.ReportLevelId;
    var asOfDate = local.Contractor.Item.Gcontractor.AsOfDate;
    var casesUnderOrderNumerator =
      local.Contractor.Item.Gcontractor.CasesUnderOrderNumerator ?? 0;
    var casesUnderOrderDenominator =
      local.Contractor.Item.Gcontractor.CasesUnderOrderDenominator ?? 0;
    var casesUnderOrderPercent =
      local.Contractor.Item.Gcontractor.CasesUnderOrderPercent ?? 0M;
    var param = 0M;
    var contractorNumber =
      local.Contractor.Item.Gcontractor.ContractorNumber ?? "";
    var prevYrCaseNumerator =
      local.Contractor.Item.Gcontractor.PrevYrCaseNumerator ?? 0;
    var prevYrCaseDenominator =
      local.Contractor.Item.Gcontractor.PrevYrCaseDenominator ?? 0;
    var casesUndrOrdrPrevYrPct =
      local.Contractor.Item.Gcontractor.CasesUndrOrdrPrevYrPct ?? 0M;
    var pctChgBtwenYrsCaseUndrOrdr =
      local.Contractor.Item.Gcontractor.PctChgBtwenYrsCaseUndrOrdr ?? 0M;

    entities.DashboardStagingPriority12.Populated = false;
    Update("CreateDashboardStagingPriority3",
      (db, command) =>
      {
        db.SetInt32(command, "reportMonth", reportMonth);
        db.SetString(command, "reportLevel", reportLevel);
        db.SetString(command, "reportLevelId", reportLevelId);
        db.SetNullableDate(command, "asOfDt", asOfDate);
        db.
          SetNullableInt32(command, "casUnderOrdNum", casesUnderOrderNumerator);
        db.SetNullableInt32(
          command, "casUnderOrdDen", casesUnderOrderDenominator);
        db.
          SetNullableDecimal(command, "casUnderOrdPer", casesUnderOrderPercent);
        db.SetNullableInt32(command, "casUnderOrdRnk", 0);
        db.SetNullableInt32(command, "pepNum", 0);
        db.SetNullableDecimal(command, "pepPer", param);
        db.SetNullableDecimal(command, "curSupPdMthNum", param);
        db.SetNullableDecimal(command, "casPerFteDen", param);
        db.SetNullableString(command, "contractorNum", contractorNumber);
        db.SetNullableInt32(command, "prvYrCaseNumtr", prevYrCaseNumerator);
        db.SetNullableInt32(command, "prvYrCaseDenom", prevYrCaseDenominator);
        db.SetNullableDecimal(command, "prvYrCasPctUo", casesUndrOrdrPrevYrPct);
        db.SetNullableDecimal(
          command, "pctChgByrCasUo", pctChgBtwenYrsCaseUndrOrdr);
      });

    entities.DashboardStagingPriority12.ReportMonth = reportMonth;
    entities.DashboardStagingPriority12.ReportLevel = reportLevel;
    entities.DashboardStagingPriority12.ReportLevelId = reportLevelId;
    entities.DashboardStagingPriority12.AsOfDate = asOfDate;
    entities.DashboardStagingPriority12.CasesUnderOrderNumerator =
      casesUnderOrderNumerator;
    entities.DashboardStagingPriority12.CasesUnderOrderDenominator =
      casesUnderOrderDenominator;
    entities.DashboardStagingPriority12.CasesUnderOrderPercent =
      casesUnderOrderPercent;
    entities.DashboardStagingPriority12.CasesUnderOrderRank = 0;
    entities.DashboardStagingPriority12.ContractorNumber = contractorNumber;
    entities.DashboardStagingPriority12.PrevYrCaseNumerator =
      prevYrCaseNumerator;
    entities.DashboardStagingPriority12.PrevYrCaseDenominator =
      prevYrCaseDenominator;
    entities.DashboardStagingPriority12.CasesUndrOrdrPrevYrPct =
      casesUndrOrdrPrevYrPct;
    entities.DashboardStagingPriority12.PctChgBtwenYrsCaseUndrOrdr =
      pctChgBtwenYrsCaseUndrOrdr;
    entities.DashboardStagingPriority12.Populated = true;
  }

  private bool ReadAccrualInstructions()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.AccrualInstructions.Populated = false;

    return Read("ReadAccrualInstructions",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
        db.SetNullableString(
          command, "cspSupNumber", entities.ChCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.AccrualInstructions.OtrType = db.GetString(reader, 0);
        entities.AccrualInstructions.OtyId = db.GetInt32(reader, 1);
        entities.AccrualInstructions.ObgGeneratedId = db.GetInt32(reader, 2);
        entities.AccrualInstructions.CspNumber = db.GetString(reader, 3);
        entities.AccrualInstructions.CpaType = db.GetString(reader, 4);
        entities.AccrualInstructions.OtrGeneratedId = db.GetInt32(reader, 5);
        entities.AccrualInstructions.AsOfDt = db.GetDate(reader, 6);
        entities.AccrualInstructions.DiscontinueDt =
          db.GetNullableDate(reader, 7);
        entities.AccrualInstructions.LastAccrualDt =
          db.GetNullableDate(reader, 8);
        entities.AccrualInstructions.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCaseCaseAssignment()
  {
    return ReadEachInSeparateTransaction("ReadCaseCaseAssignment",
      (db, command) =>
      {
        db.SetDate(command, "effectiveDate", import.ReportEndDate.Date);
        db.SetString(command, "numb", local.Restart.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.CaseAssignment.CasNo = db.GetString(reader, 0);
        entities.Case1.NoJurisdictionCd = db.GetNullableString(reader, 1);
        entities.CaseAssignment.EffectiveDate = db.GetDate(reader, 2);
        entities.CaseAssignment.DiscontinueDate = db.GetNullableDate(reader, 3);
        entities.CaseAssignment.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.CaseAssignment.SpdId = db.GetInt32(reader, 5);
        entities.CaseAssignment.OffId = db.GetInt32(reader, 6);
        entities.CaseAssignment.OspCode = db.GetString(reader, 7);
        entities.CaseAssignment.OspDate = db.GetDate(reader, 8);
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
        entities.ChCaseRole.CasNumber = db.GetString(reader, 7);
        entities.ChCaseRole.CspNumber = db.GetString(reader, 8);
        entities.ChCsePerson.Number = db.GetString(reader, 8);
        entities.ChCaseRole.Type1 = db.GetString(reader, 9);
        entities.ChCaseRole.Identifier = db.GetInt32(reader, 10);
        entities.ChCaseRole.StartDate = db.GetNullableDate(reader, 11);
        entities.ChCaseRole.EndDate = db.GetNullableDate(reader, 12);

        if (Equal(entities.ApCaseRole.Type1, "CH"))
        {
          entities.ApCaseRole.DateOfEmancipation =
            db.GetNullableDate(reader, 6);
          entities.ChCaseRole.DateOfEmancipation =
            db.GetNullableDate(reader, 13);
        }
        else
        {
          entities.ApCaseRole.DateOfEmancipation = null;
          entities.ChCaseRole.DateOfEmancipation = null;
        }

        entities.ApCaseRole.Populated = true;
        entities.ApCsePerson.Populated = true;
        entities.ChCaseRole.Populated = true;
        entities.ChCsePerson.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ApCaseRole.Type1);
        CheckValid<CaseRole>("Type1", entities.ChCaseRole.Type1);

        return true;
      },
      () =>
      {
        entities.ChCsePerson.Populated = false;
        entities.ApCsePerson.Populated = false;
        entities.ApCaseRole.Populated = false;
        entities.ChCaseRole.Populated = false;
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
        entities.DashboardStagingPriority12.CasesUnderOrderNumerator =
          db.GetNullableInt32(reader, 4);
        entities.DashboardStagingPriority12.CasesUnderOrderDenominator =
          db.GetNullableInt32(reader, 5);
        entities.DashboardStagingPriority12.CasesUnderOrderPercent =
          db.GetNullableDecimal(reader, 6);
        entities.DashboardStagingPriority12.CasesUnderOrderRank =
          db.GetNullableInt32(reader, 7);
        entities.DashboardStagingPriority12.ContractorNumber =
          db.GetNullableString(reader, 8);
        entities.DashboardStagingPriority12.PrevYrCaseNumerator =
          db.GetNullableInt32(reader, 9);
        entities.DashboardStagingPriority12.PrevYrCaseDenominator =
          db.GetNullableInt32(reader, 10);
        entities.DashboardStagingPriority12.CasesUndrOrdrPrevYrPct =
          db.GetNullableDecimal(reader, 11);
        entities.DashboardStagingPriority12.PctChgBtwenYrsCaseUndrOrdr =
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
    entities.PreviousYear.Populated = false;

    return Read("ReadDashboardStagingPriority10",
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
        entities.PreviousYear.CasesUnderOrderNumerator =
          db.GetNullableInt32(reader, 3);
        entities.PreviousYear.CasesUnderOrderDenominator =
          db.GetNullableInt32(reader, 4);
        entities.PreviousYear.CasesUnderOrderPercent =
          db.GetNullableDecimal(reader, 5);
        entities.PreviousYear.Populated = true;
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
        entities.DashboardStagingPriority12.CasesUnderOrderNumerator =
          db.GetNullableInt32(reader, 4);
        entities.DashboardStagingPriority12.CasesUnderOrderDenominator =
          db.GetNullableInt32(reader, 5);
        entities.DashboardStagingPriority12.CasesUnderOrderPercent =
          db.GetNullableDecimal(reader, 6);
        entities.DashboardStagingPriority12.CasesUnderOrderRank =
          db.GetNullableInt32(reader, 7);
        entities.DashboardStagingPriority12.ContractorNumber =
          db.GetNullableString(reader, 8);
        entities.DashboardStagingPriority12.PrevYrCaseNumerator =
          db.GetNullableInt32(reader, 9);
        entities.DashboardStagingPriority12.PrevYrCaseDenominator =
          db.GetNullableInt32(reader, 10);
        entities.DashboardStagingPriority12.CasesUndrOrdrPrevYrPct =
          db.GetNullableDecimal(reader, 11);
        entities.DashboardStagingPriority12.PctChgBtwenYrsCaseUndrOrdr =
          db.GetNullableDecimal(reader, 12);
        entities.DashboardStagingPriority12.Populated = true;

        return true;
      },
      () =>
      {
        entities.DashboardStagingPriority12.Populated = false;
      });
  }

  private IEnumerable<bool> ReadDashboardStagingPriority13()
  {
    return ReadEach("ReadDashboardStagingPriority13",
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
        entities.DashboardStagingPriority12.CasesUnderOrderNumerator =
          db.GetNullableInt32(reader, 4);
        entities.DashboardStagingPriority12.CasesUnderOrderDenominator =
          db.GetNullableInt32(reader, 5);
        entities.DashboardStagingPriority12.CasesUnderOrderPercent =
          db.GetNullableDecimal(reader, 6);
        entities.DashboardStagingPriority12.CasesUnderOrderRank =
          db.GetNullableInt32(reader, 7);
        entities.DashboardStagingPriority12.ContractorNumber =
          db.GetNullableString(reader, 8);
        entities.DashboardStagingPriority12.PrevYrCaseNumerator =
          db.GetNullableInt32(reader, 9);
        entities.DashboardStagingPriority12.PrevYrCaseDenominator =
          db.GetNullableInt32(reader, 10);
        entities.DashboardStagingPriority12.CasesUndrOrdrPrevYrPct =
          db.GetNullableDecimal(reader, 11);
        entities.DashboardStagingPriority12.PctChgBtwenYrsCaseUndrOrdr =
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
        entities.DashboardStagingPriority12.CasesUnderOrderNumerator =
          db.GetNullableInt32(reader, 4);
        entities.DashboardStagingPriority12.CasesUnderOrderDenominator =
          db.GetNullableInt32(reader, 5);
        entities.DashboardStagingPriority12.CasesUnderOrderPercent =
          db.GetNullableDecimal(reader, 6);
        entities.DashboardStagingPriority12.CasesUnderOrderRank =
          db.GetNullableInt32(reader, 7);
        entities.DashboardStagingPriority12.ContractorNumber =
          db.GetNullableString(reader, 8);
        entities.DashboardStagingPriority12.PrevYrCaseNumerator =
          db.GetNullableInt32(reader, 9);
        entities.DashboardStagingPriority12.PrevYrCaseDenominator =
          db.GetNullableInt32(reader, 10);
        entities.DashboardStagingPriority12.CasesUndrOrdrPrevYrPct =
          db.GetNullableDecimal(reader, 11);
        entities.DashboardStagingPriority12.PctChgBtwenYrsCaseUndrOrdr =
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
        entities.DashboardStagingPriority12.CasesUnderOrderNumerator =
          db.GetNullableInt32(reader, 4);
        entities.DashboardStagingPriority12.CasesUnderOrderDenominator =
          db.GetNullableInt32(reader, 5);
        entities.DashboardStagingPriority12.CasesUnderOrderPercent =
          db.GetNullableDecimal(reader, 6);
        entities.DashboardStagingPriority12.CasesUnderOrderRank =
          db.GetNullableInt32(reader, 7);
        entities.DashboardStagingPriority12.ContractorNumber =
          db.GetNullableString(reader, 8);
        entities.DashboardStagingPriority12.PrevYrCaseNumerator =
          db.GetNullableInt32(reader, 9);
        entities.DashboardStagingPriority12.PrevYrCaseDenominator =
          db.GetNullableInt32(reader, 10);
        entities.DashboardStagingPriority12.CasesUndrOrdrPrevYrPct =
          db.GetNullableDecimal(reader, 11);
        entities.DashboardStagingPriority12.PctChgBtwenYrsCaseUndrOrdr =
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
        entities.DashboardStagingPriority12.CasesUnderOrderNumerator =
          db.GetNullableInt32(reader, 4);
        entities.DashboardStagingPriority12.CasesUnderOrderDenominator =
          db.GetNullableInt32(reader, 5);
        entities.DashboardStagingPriority12.CasesUnderOrderPercent =
          db.GetNullableDecimal(reader, 6);
        entities.DashboardStagingPriority12.CasesUnderOrderRank =
          db.GetNullableInt32(reader, 7);
        entities.DashboardStagingPriority12.ContractorNumber =
          db.GetNullableString(reader, 8);
        entities.DashboardStagingPriority12.PrevYrCaseNumerator =
          db.GetNullableInt32(reader, 9);
        entities.DashboardStagingPriority12.PrevYrCaseDenominator =
          db.GetNullableInt32(reader, 10);
        entities.DashboardStagingPriority12.CasesUndrOrdrPrevYrPct =
          db.GetNullableDecimal(reader, 11);
        entities.DashboardStagingPriority12.PctChgBtwenYrsCaseUndrOrdr =
          db.GetNullableDecimal(reader, 12);
        entities.DashboardStagingPriority12.Populated = true;

        return true;
      },
      () =>
      {
        entities.DashboardStagingPriority12.Populated = false;
      });
  }

  private bool ReadDashboardStagingPriority5()
  {
    entities.DashboardStagingPriority12.Populated = false;

    return Read("ReadDashboardStagingPriority5",
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
        entities.DashboardStagingPriority12.CasesUnderOrderNumerator =
          db.GetNullableInt32(reader, 4);
        entities.DashboardStagingPriority12.CasesUnderOrderDenominator =
          db.GetNullableInt32(reader, 5);
        entities.DashboardStagingPriority12.CasesUnderOrderPercent =
          db.GetNullableDecimal(reader, 6);
        entities.DashboardStagingPriority12.CasesUnderOrderRank =
          db.GetNullableInt32(reader, 7);
        entities.DashboardStagingPriority12.ContractorNumber =
          db.GetNullableString(reader, 8);
        entities.DashboardStagingPriority12.PrevYrCaseNumerator =
          db.GetNullableInt32(reader, 9);
        entities.DashboardStagingPriority12.PrevYrCaseDenominator =
          db.GetNullableInt32(reader, 10);
        entities.DashboardStagingPriority12.CasesUndrOrdrPrevYrPct =
          db.GetNullableDecimal(reader, 11);
        entities.DashboardStagingPriority12.PctChgBtwenYrsCaseUndrOrdr =
          db.GetNullableDecimal(reader, 12);
        entities.DashboardStagingPriority12.Populated = true;
      });
  }

  private bool ReadDashboardStagingPriority6()
  {
    entities.DashboardStagingPriority12.Populated = false;

    return Read("ReadDashboardStagingPriority6",
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
        entities.DashboardStagingPriority12.CasesUnderOrderNumerator =
          db.GetNullableInt32(reader, 4);
        entities.DashboardStagingPriority12.CasesUnderOrderDenominator =
          db.GetNullableInt32(reader, 5);
        entities.DashboardStagingPriority12.CasesUnderOrderPercent =
          db.GetNullableDecimal(reader, 6);
        entities.DashboardStagingPriority12.CasesUnderOrderRank =
          db.GetNullableInt32(reader, 7);
        entities.DashboardStagingPriority12.ContractorNumber =
          db.GetNullableString(reader, 8);
        entities.DashboardStagingPriority12.PrevYrCaseNumerator =
          db.GetNullableInt32(reader, 9);
        entities.DashboardStagingPriority12.PrevYrCaseDenominator =
          db.GetNullableInt32(reader, 10);
        entities.DashboardStagingPriority12.CasesUndrOrdrPrevYrPct =
          db.GetNullableDecimal(reader, 11);
        entities.DashboardStagingPriority12.PctChgBtwenYrsCaseUndrOrdr =
          db.GetNullableDecimal(reader, 12);
        entities.DashboardStagingPriority12.Populated = true;
      });
  }

  private IEnumerable<bool> ReadDashboardStagingPriority7()
  {
    return ReadEach("ReadDashboardStagingPriority7",
      (db, command) =>
      {
        db.SetInt32(command, "reportMonth", local.PreviousYear.ReportMonth);
      },
      (db, reader) =>
      {
        entities.PreviousYear.ReportMonth = db.GetInt32(reader, 0);
        entities.PreviousYear.ReportLevel = db.GetString(reader, 1);
        entities.PreviousYear.ReportLevelId = db.GetString(reader, 2);
        entities.PreviousYear.CasesUnderOrderNumerator =
          db.GetNullableInt32(reader, 3);
        entities.PreviousYear.CasesUnderOrderDenominator =
          db.GetNullableInt32(reader, 4);
        entities.PreviousYear.CasesUnderOrderPercent =
          db.GetNullableDecimal(reader, 5);
        entities.PreviousYear.Populated = true;

        return true;
      },
      () =>
      {
        entities.PreviousYear.Populated = false;
      });
  }

  private bool ReadDashboardStagingPriority8()
  {
    entities.DashboardStagingPriority12.Populated = false;

    return Read("ReadDashboardStagingPriority8",
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
        entities.DashboardStagingPriority12.CasesUnderOrderNumerator =
          db.GetNullableInt32(reader, 4);
        entities.DashboardStagingPriority12.CasesUnderOrderDenominator =
          db.GetNullableInt32(reader, 5);
        entities.DashboardStagingPriority12.CasesUnderOrderPercent =
          db.GetNullableDecimal(reader, 6);
        entities.DashboardStagingPriority12.CasesUnderOrderRank =
          db.GetNullableInt32(reader, 7);
        entities.DashboardStagingPriority12.ContractorNumber =
          db.GetNullableString(reader, 8);
        entities.DashboardStagingPriority12.PrevYrCaseNumerator =
          db.GetNullableInt32(reader, 9);
        entities.DashboardStagingPriority12.PrevYrCaseDenominator =
          db.GetNullableInt32(reader, 10);
        entities.DashboardStagingPriority12.CasesUndrOrdrPrevYrPct =
          db.GetNullableDecimal(reader, 11);
        entities.DashboardStagingPriority12.PctChgBtwenYrsCaseUndrOrdr =
          db.GetNullableDecimal(reader, 12);
        entities.DashboardStagingPriority12.Populated = true;
      });
  }

  private IEnumerable<bool> ReadDashboardStagingPriority9()
  {
    return ReadEach("ReadDashboardStagingPriority9",
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
        entities.DashboardStagingPriority12.CasesUnderOrderNumerator =
          db.GetNullableInt32(reader, 4);
        entities.DashboardStagingPriority12.CasesUnderOrderDenominator =
          db.GetNullableInt32(reader, 5);
        entities.DashboardStagingPriority12.CasesUnderOrderPercent =
          db.GetNullableDecimal(reader, 6);
        entities.DashboardStagingPriority12.CasesUnderOrderRank =
          db.GetNullableInt32(reader, 7);
        entities.DashboardStagingPriority12.ContractorNumber =
          db.GetNullableString(reader, 8);
        entities.DashboardStagingPriority12.PrevYrCaseNumerator =
          db.GetNullableInt32(reader, 9);
        entities.DashboardStagingPriority12.PrevYrCaseDenominator =
          db.GetNullableInt32(reader, 10);
        entities.DashboardStagingPriority12.CasesUndrOrdrPrevYrPct =
          db.GetNullableDecimal(reader, 11);
        entities.DashboardStagingPriority12.PctChgBtwenYrsCaseUndrOrdr =
          db.GetNullableDecimal(reader, 12);
        entities.DashboardStagingPriority12.Populated = true;

        return true;
      },
      () =>
      {
        entities.DashboardStagingPriority12.Populated = false;
      });
  }

  private IEnumerable<bool> ReadDebtDebtDetail()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);

    return ReadEach("ReadDebtDebtDetail",
      (db, command) =>
      {
        db.SetDateTime(command, "createdTmst", import.ReportEndDate.Timestamp);
        db.SetNullableString(
          command, "cspSupNumber", entities.ChCsePerson.Number);
        db.SetInt32(command, "otyType", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
      },
      (db, reader) =>
      {
        entities.Debt.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.Debt.CspNumber = db.GetString(reader, 1);
        entities.DebtDetail.CspNumber = db.GetString(reader, 1);
        entities.Debt.CpaType = db.GetString(reader, 2);
        entities.DebtDetail.CpaType = db.GetString(reader, 2);
        entities.Debt.SystemGeneratedIdentifier = db.GetInt32(reader, 3);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.Debt.Type1 = db.GetString(reader, 4);
        entities.DebtDetail.OtrType = db.GetString(reader, 4);
        entities.Debt.CreatedTmst = db.GetDateTime(reader, 5);
        entities.Debt.OtyType = db.GetInt32(reader, 8);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 8);
        entities.DebtDetail.DueDt = db.GetDate(reader, 9);
        entities.DebtDetail.BalanceDueAmt = db.GetDecimal(reader, 10);
        entities.DebtDetail.RetiredDt = db.GetNullableDate(reader, 11);
        entities.DebtDetail.CoveredPrdStartDt = db.GetNullableDate(reader, 12);
        entities.DebtDetail.PreconversionProgramCode =
          db.GetNullableString(reader, 13);
        entities.DebtDetail.CreatedTmst = db.GetDateTime(reader, 14);

        if (Equal(entities.Debt.Type1, "DE"))
        {
          entities.Debt.CspSupNumber = db.GetNullableString(reader, 6);
          entities.Debt.CpaSupType = db.GetNullableString(reader, 7);
        }
        else
        {
          entities.Debt.CspSupNumber = null;
          entities.Debt.CpaSupType = null;
        }

        entities.Debt.Populated = true;
        entities.DebtDetail.Populated = true;
        CheckValid<ObligationTransaction>("Type1", entities.Debt.Type1);

        return true;
      },
      () =>
      {
        entities.Debt.Populated = false;
        entities.DebtDetail.Populated = false;
      });
  }

  private bool ReadLegalActionDetailLegalAction()
  {
    System.Diagnostics.Debug.Assert(entities.ApCaseRole.Populated);
    System.Diagnostics.Debug.Assert(entities.ChCaseRole.Populated);
    entities.LegalAction.Populated = false;
    entities.LegalActionDetail.Populated = false;

    return Read("ReadLegalActionDetailLegalAction",
      (db, command) =>
      {
        db.SetNullableDate(command, "filedDt", local.Null1.Date);
        db.
          SetDateTime(command, "createdTstamp", import.ReportEndDate.Timestamp);
        db.SetInt32(command, "croIdentifier1", entities.ApCaseRole.Identifier);
        db.SetString(command, "croType1", entities.ApCaseRole.Type1);
        db.SetString(command, "cspNumber1", entities.ApCaseRole.CspNumber);
        db.SetString(command, "casNumber1", entities.ApCaseRole.CasNumber);
        db.SetInt32(command, "croIdentifier2", entities.ChCaseRole.Identifier);
        db.SetString(command, "croType2", entities.ChCaseRole.Type1);
        db.SetString(command, "cspNumber2", entities.ChCaseRole.CspNumber);
        db.SetString(command, "casNumber2", entities.ChCaseRole.CasNumber);
      },
      (db, reader) =>
      {
        entities.LegalActionDetail.LgaIdentifier = db.GetInt32(reader, 0);
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalActionDetail.Number = db.GetInt32(reader, 1);
        entities.LegalActionDetail.EndDate = db.GetNullableDate(reader, 2);
        entities.LegalActionDetail.EffectiveDate = db.GetDate(reader, 3);
        entities.LegalActionDetail.CreatedTstamp = db.GetDateTime(reader, 4);
        entities.LegalActionDetail.DetailType = db.GetString(reader, 6);
        entities.LegalAction.Classification = db.GetString(reader, 7);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 8);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 9);
        entities.LegalAction.CreatedTstamp = db.GetDateTime(reader, 10);

        if (AsChar(entities.LegalActionDetail.DetailType) == 'N')
        {
          entities.LegalActionDetail.NonFinOblgType =
            db.GetNullableString(reader, 5);
        }
        else
        {
          entities.LegalActionDetail.NonFinOblgType = "";
        }

        entities.LegalActionDetail.Populated = true;
        entities.LegalAction.Populated = true;
        CheckValid<LegalActionDetail>("DetailType",
          entities.LegalActionDetail.DetailType);
      });
  }

  private IEnumerable<bool> ReadLegalActionDetailLegalActionObligationType()
  {
    return ReadEach("ReadLegalActionDetailLegalActionObligationType",
      (db, command) =>
      {
        db.SetNullableDate(command, "filedDt", local.Null1.Date);
        db.
          SetDateTime(command, "createdTstamp", import.ReportEndDate.Timestamp);
        db.
          SetNullableString(command, "cspNumber1", entities.ApCsePerson.Number);
        db.
          SetNullableString(command, "cspNumber2", entities.ChCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.LegalActionDetail.LgaIdentifier = db.GetInt32(reader, 0);
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalActionDetail.Number = db.GetInt32(reader, 1);
        entities.LegalActionDetail.EndDate = db.GetNullableDate(reader, 2);
        entities.LegalActionDetail.EffectiveDate = db.GetDate(reader, 3);
        entities.LegalActionDetail.CreatedTstamp = db.GetDateTime(reader, 4);
        entities.LegalActionDetail.DetailType = db.GetString(reader, 6);
        entities.LegalAction.Classification = db.GetString(reader, 7);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 8);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 9);
        entities.LegalAction.CreatedTstamp = db.GetDateTime(reader, 10);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 11);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 11);
        entities.ObligationType.Code = db.GetString(reader, 12);
        entities.ObligationType.Classification = db.GetString(reader, 13);
        entities.Obligation.CpaType = db.GetString(reader, 14);
        entities.Obligation.CspNumber = db.GetString(reader, 15);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 16);
        entities.Obligation.CreatedTmst = db.GetDateTime(reader, 17);
        entities.Obligation.LgaIdentifier = db.GetNullableInt32(reader, 18);
        entities.Obligation.LadNumber = db.GetNullableInt32(reader, 19);

        if (AsChar(entities.LegalActionDetail.DetailType) == 'N')
        {
          entities.LegalActionDetail.NonFinOblgType =
            db.GetNullableString(reader, 5);
        }
        else
        {
          entities.LegalActionDetail.NonFinOblgType = "";
        }

        entities.LegalActionDetail.Populated = true;
        entities.LegalAction.Populated = true;
        entities.ObligationType.Populated = true;
        entities.Obligation.Populated = true;
        CheckValid<LegalActionDetail>("DetailType",
          entities.LegalActionDetail.DetailType);
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);

        return true;
      },
      () =>
      {
        entities.LegalAction.Populated = false;
        entities.LegalActionDetail.Populated = false;
        entities.ObligationType.Populated = false;
        entities.Obligation.Populated = false;
      });
  }

  private void UpdateDashboardStagingPriority1()
  {
    var asOfDate = local.Statewide.AsOfDate;
    var casesUnderOrderNumerator = local.Statewide.CasesUnderOrderNumerator ?? 0
      ;
    var casesUnderOrderDenominator =
      local.Statewide.CasesUnderOrderDenominator ?? 0;
    var casesUnderOrderPercent = local.Statewide.CasesUnderOrderPercent ?? 0M;
    var casesUnderOrderRank = local.Statewide.CasesUnderOrderRank ?? 0;

    entities.DashboardStagingPriority12.Populated = false;
    Update("UpdateDashboardStagingPriority1",
      (db, command) =>
      {
        db.SetNullableDate(command, "asOfDt", asOfDate);
        db.
          SetNullableInt32(command, "casUnderOrdNum", casesUnderOrderNumerator);
        db.SetNullableInt32(
          command, "casUnderOrdDen", casesUnderOrderDenominator);
        db.
          SetNullableDecimal(command, "casUnderOrdPer", casesUnderOrderPercent);
        db.SetNullableInt32(command, "casUnderOrdRnk", casesUnderOrderRank);
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
    entities.DashboardStagingPriority12.CasesUnderOrderNumerator =
      casesUnderOrderNumerator;
    entities.DashboardStagingPriority12.CasesUnderOrderDenominator =
      casesUnderOrderDenominator;
    entities.DashboardStagingPriority12.CasesUnderOrderPercent =
      casesUnderOrderPercent;
    entities.DashboardStagingPriority12.CasesUnderOrderRank =
      casesUnderOrderRank;
    entities.DashboardStagingPriority12.Populated = true;
  }

  private void UpdateDashboardStagingPriority2()
  {
    var asOfDate = local.Local1.Item.G.AsOfDate;
    var casesUnderOrderNumerator =
      local.Local1.Item.G.CasesUnderOrderNumerator ?? 0;
    var casesUnderOrderDenominator =
      local.Local1.Item.G.CasesUnderOrderDenominator ?? 0;
    var casesUnderOrderPercent = local.Local1.Item.G.CasesUnderOrderPercent ?? 0M
      ;
    var casesUnderOrderRank = local.Local1.Item.G.CasesUnderOrderRank ?? 0;
    var contractorNumber = local.Local1.Item.G.ContractorNumber ?? "";

    entities.DashboardStagingPriority12.Populated = false;
    Update("UpdateDashboardStagingPriority2",
      (db, command) =>
      {
        db.SetNullableDate(command, "asOfDt", asOfDate);
        db.
          SetNullableInt32(command, "casUnderOrdNum", casesUnderOrderNumerator);
        db.SetNullableInt32(
          command, "casUnderOrdDen", casesUnderOrderDenominator);
        db.
          SetNullableDecimal(command, "casUnderOrdPer", casesUnderOrderPercent);
        db.SetNullableInt32(command, "casUnderOrdRnk", casesUnderOrderRank);
        db.SetNullableString(command, "contractorNum", contractorNumber);
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
    entities.DashboardStagingPriority12.CasesUnderOrderNumerator =
      casesUnderOrderNumerator;
    entities.DashboardStagingPriority12.CasesUnderOrderDenominator =
      casesUnderOrderDenominator;
    entities.DashboardStagingPriority12.CasesUnderOrderPercent =
      casesUnderOrderPercent;
    entities.DashboardStagingPriority12.CasesUnderOrderRank =
      casesUnderOrderRank;
    entities.DashboardStagingPriority12.ContractorNumber = contractorNumber;
    entities.DashboardStagingPriority12.Populated = true;
  }

  private void UpdateDashboardStagingPriority3()
  {
    var asOfDate = local.Local1.Item.G.AsOfDate;
    var casesUnderOrderNumerator =
      local.Contractor.Item.Gcontractor.CasesUnderOrderNumerator ?? 0;
    var casesUnderOrderDenominator =
      local.Contractor.Item.Gcontractor.CasesUnderOrderDenominator ?? 0;
    var casesUnderOrderPercent =
      local.Contractor.Item.Gcontractor.CasesUnderOrderPercent ?? 0M;
    var contractorNumber =
      local.Contractor.Item.Gcontractor.ContractorNumber ?? "";
    var prevYrCaseNumerator =
      local.Contractor.Item.Gcontractor.PrevYrCaseNumerator ?? 0;
    var prevYrCaseDenominator =
      local.Contractor.Item.Gcontractor.PrevYrCaseDenominator ?? 0;
    var casesUndrOrdrPrevYrPct =
      local.Contractor.Item.Gcontractor.CasesUndrOrdrPrevYrPct ?? 0M;
    var pctChgBtwenYrsCaseUndrOrdr =
      local.Contractor.Item.Gcontractor.PctChgBtwenYrsCaseUndrOrdr ?? 0M;

    entities.DashboardStagingPriority12.Populated = false;
    Update("UpdateDashboardStagingPriority3",
      (db, command) =>
      {
        db.SetNullableDate(command, "asOfDt", asOfDate);
        db.
          SetNullableInt32(command, "casUnderOrdNum", casesUnderOrderNumerator);
        db.SetNullableInt32(
          command, "casUnderOrdDen", casesUnderOrderDenominator);
        db.
          SetNullableDecimal(command, "casUnderOrdPer", casesUnderOrderPercent);
        db.SetNullableString(command, "contractorNum", contractorNumber);
        db.SetNullableInt32(command, "prvYrCaseNumtr", prevYrCaseNumerator);
        db.SetNullableInt32(command, "prvYrCaseDenom", prevYrCaseDenominator);
        db.SetNullableDecimal(command, "prvYrCasPctUo", casesUndrOrdrPrevYrPct);
        db.SetNullableDecimal(
          command, "pctChgByrCasUo", pctChgBtwenYrsCaseUndrOrdr);
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
    entities.DashboardStagingPriority12.CasesUnderOrderNumerator =
      casesUnderOrderNumerator;
    entities.DashboardStagingPriority12.CasesUnderOrderDenominator =
      casesUnderOrderDenominator;
    entities.DashboardStagingPriority12.CasesUnderOrderPercent =
      casesUnderOrderPercent;
    entities.DashboardStagingPriority12.ContractorNumber = contractorNumber;
    entities.DashboardStagingPriority12.PrevYrCaseNumerator =
      prevYrCaseNumerator;
    entities.DashboardStagingPriority12.PrevYrCaseDenominator =
      prevYrCaseDenominator;
    entities.DashboardStagingPriority12.CasesUndrOrdrPrevYrPct =
      casesUndrOrdrPrevYrPct;
    entities.DashboardStagingPriority12.PctChgBtwenYrsCaseUndrOrdr =
      pctChgBtwenYrsCaseUndrOrdr;
    entities.DashboardStagingPriority12.Populated = true;
  }

  private void UpdateDashboardStagingPriority4()
  {
    var casesUnderOrderPercent = local.Temp.CasesUnderOrderPercent ?? 0M;
    var prevYrCaseNumerator = local.Temp.PrevYrCaseNumerator ?? 0;
    var prevYrCaseDenominator = local.Temp.PrevYrCaseDenominator ?? 0;
    var casesUndrOrdrPrevYrPct = local.Temp.CasesUndrOrdrPrevYrPct ?? 0M;
    var pctChgBtwenYrsCaseUndrOrdr = local.Temp.PctChgBtwenYrsCaseUndrOrdr ?? 0M
      ;

    entities.DashboardStagingPriority12.Populated = false;
    Update("UpdateDashboardStagingPriority4",
      (db, command) =>
      {
        db.
          SetNullableDecimal(command, "casUnderOrdPer", casesUnderOrderPercent);
        db.SetNullableInt32(command, "prvYrCaseNumtr", prevYrCaseNumerator);
        db.SetNullableInt32(command, "prvYrCaseDenom", prevYrCaseDenominator);
        db.SetNullableDecimal(command, "prvYrCasPctUo", casesUndrOrdrPrevYrPct);
        db.SetNullableDecimal(
          command, "pctChgByrCasUo", pctChgBtwenYrsCaseUndrOrdr);
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

    entities.DashboardStagingPriority12.CasesUnderOrderPercent =
      casesUnderOrderPercent;
    entities.DashboardStagingPriority12.PrevYrCaseNumerator =
      prevYrCaseNumerator;
    entities.DashboardStagingPriority12.PrevYrCaseDenominator =
      prevYrCaseDenominator;
    entities.DashboardStagingPriority12.CasesUndrOrdrPrevYrPct =
      casesUndrOrdrPrevYrPct;
    entities.DashboardStagingPriority12.PctChgBtwenYrsCaseUndrOrdr =
      pctChgBtwenYrsCaseUndrOrdr;
    entities.DashboardStagingPriority12.Populated = true;
  }

  private void UpdateDashboardStagingPriority5()
  {
    var casesUnderOrderRank = local.Temp.CasesUnderOrderRank ?? 0;

    entities.DashboardStagingPriority12.Populated = false;
    Update("UpdateDashboardStagingPriority5",
      (db, command) =>
      {
        db.SetNullableInt32(command, "casUnderOrdRnk", casesUnderOrderRank);
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

    entities.DashboardStagingPriority12.CasesUnderOrderRank =
      casesUnderOrderRank;
    entities.DashboardStagingPriority12.Populated = true;
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
    /// A value of ProgramProcessingInfo.
    /// </summary>
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
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
    /// A value of DashboardAuditData.
    /// </summary>
    public DashboardAuditData DashboardAuditData
    {
      get => dashboardAuditData ??= new();
      set => dashboardAuditData = value;
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
    /// A value of AuditFlag.
    /// </summary>
    public Common AuditFlag
    {
      get => auditFlag ??= new();
      set => auditFlag = value;
    }

    private ProgramProcessingInfo? programProcessingInfo;
    private ProgramCheckpointRestart? programCheckpointRestart;
    private DashboardAuditData? dashboardAuditData;
    private DateWorkArea? reportEndDate;
    private Common? auditFlag;
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

    /// <summary>
    /// A value of PreviousYr.
    /// </summary>
    public DashboardStagingPriority12 PreviousYr
    {
      get => previousYr ??= new();
      set => previousYr = value;
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
    /// A value of DetermineContractor.
    /// </summary>
    public DashboardStagingPriority12 DetermineContractor
    {
      get => determineContractor ??= new();
      set => determineContractor = value;
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
    /// A value of ContractorCseOrganization.
    /// </summary>
    public CseOrganization ContractorCseOrganization
    {
      get => contractorCseOrganization ??= new();
      set => contractorCseOrganization = value;
    }

    /// <summary>
    /// A value of ContractorDashboardStagingPriority12.
    /// </summary>
    public DashboardStagingPriority12 ContractorDashboardStagingPriority12
    {
      get => contractorDashboardStagingPriority12 ??= new();
      set => contractorDashboardStagingPriority12 = value;
    }

    /// <summary>
    /// A value of Restarting.
    /// </summary>
    public Common Restarting
    {
      get => restarting ??= new();
      set => restarting = value;
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
    /// A value of Temp.
    /// </summary>
    public DashboardStagingPriority12 Temp
    {
      get => temp ??= new();
      set => temp = value;
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
    /// A value of Initialized.
    /// </summary>
    public DashboardAuditData Initialized
    {
      get => initialized ??= new();
      set => initialized = value;
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
    /// A value of NonFinLdet.
    /// </summary>
    public Common NonFinLdet
    {
      get => nonFinLdet ??= new();
      set => nonFinLdet = value;
    }

    /// <summary>
    /// A value of FinLdet.
    /// </summary>
    public Common FinLdet
    {
      get => finLdet ??= new();
      set => finLdet = value;
    }

    /// <summary>
    /// A value of AccrualInstrFound.
    /// </summary>
    public Common AccrualInstrFound
    {
      get => accrualInstrFound ??= new();
      set => accrualInstrFound = value;
    }

    /// <summary>
    /// A value of Null1.
    /// </summary>
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
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
    /// A value of ProgramCheckpointRestart.
    /// </summary>
    public ProgramCheckpointRestart ProgramCheckpointRestart
    {
      get => programCheckpointRestart ??= new();
      set => programCheckpointRestart = value;
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

    private DashboardStagingPriority12? previousYr;
    private DashboardStagingPriority12? current;
    private DashboardStagingPriority12? determineContractor;
    private DashboardStagingPriority12? previousYear;
    private Array<ContractorGroup>? contractor;
    private CseOrganization? contractorCseOrganization;
    private DashboardStagingPriority12? contractorDashboardStagingPriority12;
    private Common? restarting;
    private Common? common;
    private DashboardStagingPriority12? prevRank;
    private DashboardStagingPriority12? temp;
    private DashboardStagingPriority12? statewide;
    private DashboardAuditData? initialized;
    private Array<LocalGroup>? local1;
    private Common? recordsReadSinceCommit;
    private DashboardAuditData? dashboardAuditData;
    private Common? nonFinLdet;
    private Common? finLdet;
    private Common? accrualInstrFound;
    private DateWorkArea? null1;
    private Program? program;
    private Case1? restart;
    private Case1? prev;
    private ProgramCheckpointRestart? programCheckpointRestart;
    private EabFileHandling? eabFileHandling;
    private EabReportSend? eabReportSend;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of DashboardStagingPriority12.
    /// </summary>
    public DashboardStagingPriority12 DashboardStagingPriority12
    {
      get => dashboardStagingPriority12 ??= new();
      set => dashboardStagingPriority12 = value;
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
    /// A value of Case1.
    /// </summary>
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of ChCsePerson.
    /// </summary>
    public CsePerson ChCsePerson
    {
      get => chCsePerson ??= new();
      set => chCsePerson = value;
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
    /// A value of LegalAction.
    /// </summary>
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// A value of LegalActionDetail.
    /// </summary>
    public LegalActionDetail LegalActionDetail
    {
      get => legalActionDetail ??= new();
      set => legalActionDetail = value;
    }

    /// <summary>
    /// A value of ApLegalActionCaseRole.
    /// </summary>
    public LegalActionCaseRole ApLegalActionCaseRole
    {
      get => apLegalActionCaseRole ??= new();
      set => apLegalActionCaseRole = value;
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
    /// A value of ChLegalActionCaseRole.
    /// </summary>
    public LegalActionCaseRole ChLegalActionCaseRole
    {
      get => chLegalActionCaseRole ??= new();
      set => chLegalActionCaseRole = value;
    }

    /// <summary>
    /// A value of ChCaseRole.
    /// </summary>
    public CaseRole ChCaseRole
    {
      get => chCaseRole ??= new();
      set => chCaseRole = value;
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
    /// A value of AccrualInstructions.
    /// </summary>
    public AccrualInstructions AccrualInstructions
    {
      get => accrualInstructions ??= new();
      set => accrualInstructions = value;
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
    /// A value of Obligation.
    /// </summary>
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
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
    /// A value of DebtDetail.
    /// </summary>
    public DebtDetail DebtDetail
    {
      get => debtDetail ??= new();
      set => debtDetail = value;
    }

    /// <summary>
    /// A value of Obligor.
    /// </summary>
    public LegalActionPerson Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
    }

    /// <summary>
    /// A value of Supp.
    /// </summary>
    public LegalActionPerson Supp
    {
      get => supp ??= new();
      set => supp = value;
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
    /// A value of PreviousYear.
    /// </summary>
    public DashboardStagingPriority12 PreviousYear
    {
      get => previousYear ??= new();
      set => previousYear = value;
    }

    private DashboardStagingPriority12? dashboardStagingPriority12;
    private CseOrganization? cseOrganization;
    private Case1? case1;
    private CsePerson? chCsePerson;
    private CsePerson? apCsePerson;
    private LegalAction? legalAction;
    private LegalActionDetail? legalActionDetail;
    private LegalActionCaseRole? apLegalActionCaseRole;
    private CaseRole? apCaseRole;
    private LegalActionCaseRole? chLegalActionCaseRole;
    private CaseRole? chCaseRole;
    private ObligationType? obligationType;
    private AccrualInstructions? accrualInstructions;
    private ObligationTransaction? debt;
    private Obligation? obligation;
    private CsePersonAccount? supported;
    private DebtDetail? debtDetail;
    private LegalActionPerson? obligor;
    private LegalActionPerson? supp;
    private CaseAssignment? caseAssignment;
    private DashboardStagingPriority12? previousYear;
  }
#endregion
}
