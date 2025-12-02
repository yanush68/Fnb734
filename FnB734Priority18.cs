// Program: FN_B734_PRIORITY_1_8, ID: 945132077, model: 746.
// Short name: SWE03089
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
/// A program: FN_B734_PRIORITY_1_8.
/// </para>
/// <para>
/// Priority 1-8: Arrears Due
/// </para>
/// </summary>
[Serializable]
[Program("SWE03089")]
public partial class FnB734Priority18: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B734_PRIORITY_1_8 program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB734Priority18(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB734Priority18.
  /// </summary>
  public FnB734Priority18(IContext context, Import import, Export export):
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
    // 03/22/13  GVandy	CQ36547		Initial Development.
    // 			Segment B	
    // 					Splitting amounts between Joint & Several
    // 					obligors resulted in dollar amounts variance when
    // 					compared to OCSE157.  Decision was made to
    // 					NOT split amounts between obligors.
    // ---------------------------------------------------------------------------------------------------
    // -------------------------------------------------------------------------------------
    // Priority 1-8: Arrears Due (End of Report Period)
    // -------------------------------------------------------------------------------------
    // 1 Column
    // 	1. Arrears Due as of refresh date
    // Report Level: State, Judicial District
    // Report Period: Month
    // Definition of Arrears Due (using OCSE157 Line26)
    // 	1) Accruing Debts- Due date less than first day of the current month and
    // 	   balance still owed as of refresh date.
    // 	2) Non-accruing debts- bypass Fees, Recoveries, 718Bs and MJs (AF, AFI, 
    // FC,
    // 	   FCI).  For all others, capture the amount due on refresh date.
    // 	3) In joint/several, count the debt only once.
    // 	4) Exclude all incoming interstate obligations (AFI, FCI, NAI).
    // 	5) In primary/secondary, credit only the primary.
    // -------------------------------------------------------------------------------------
    MoveDashboardAuditData2(import.DashboardAuditData, local.Initialized);
    MoveProgramCheckpointRestart(import.ProgramCheckpointRestart,
      local.ProgramCheckpointRestart);

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
        local.Contractor.Update.Gcontractor.ReportLevelId =
          local.Local1.Item.G.ContractorNumber;
        local.Contractor.Update.Gcontractor.ReportMonth =
          import.DashboardAuditData.ReportMonth;
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
            goto ReadEach;
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

ReadEach:
      ;
    }

    // ------------------------------------------------------------------------------
    // -- Determine if we're restarting and set appropriate restart information.
    // ------------------------------------------------------------------------------
    if (AsChar(import.ProgramCheckpointRestart.RestartInd) == 'Y' && Equal
      (import.ProgramCheckpointRestart.RestartInfo, 81, 8, "1-08    "))
    {
      // -- Checkpoint Info
      // Positions   Value
      // ---------   
      // ------------------------------------
      //  001-080    General Checkpoint Info for PRAD
      //  081-088    Dashboard Priority
      //  089-089    Blank
      //  090-099    AP Person Number
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
        foreach(var _ in ReadDashboardStagingPriority1())
        {
          local.Statewide.Assign(entities.DashboardStagingPriority12);
          local.Statewide.ArrearsDueActual = 0;
          local.Statewide.ArrearsDueRnk = 0;
        }

        // -- Load Judicial District counts.
        foreach(var _ in ReadDashboardStagingPriority2())
        {
          local.Local1.Index =
            (int)StringToNumber(entities.DashboardStagingPriority12.
              ReportLevelId) - 1;
          local.Local1.CheckSize();

          local.Local1.Update.G.Assign(entities.DashboardStagingPriority12);
          local.Local1.Update.G.ArrearsDueActual = 0;
          local.Local1.Update.G.ArrearsDueRnk = 0;
        }
      }
    }
    else
    {
      local.Restart.Number = "";
    }

    // -- This logic is copied from ocse157 Line 26.
    foreach(var _ in ReadObligorCsePersonCollection())
    {
      if (Equal(entities.ApCsePerson.Number, local.PrevAp.Number))
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
          if (ReadDashboardStagingPriority3())
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

            if (ReadDashboardStagingPriority4())
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
          //  090-099    AP Person Number
          local.ProgramCheckpointRestart.RestartInd = "Y";
          local.ProgramCheckpointRestart.RestartInfo =
            Substring(import.ProgramCheckpointRestart.RestartInfo, 250, 1, 80) +
            "1-08    " + " " + String
            (local.PrevAp.Number, CsePerson.Number_MaxLength);
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

      local.PrevAp.Number = entities.ApCsePerson.Number;
      ++local.RecordsReadSinceCommit.Count;
      MoveDashboardAuditData1(local.Initialized, local.DashboardAuditData);
      local.ActivityAfterFyEnd.Flag = "N";

      // --------------------------------------------------------
      // 10/8/2001
      // - Also read 'C'urrent collections.
      // - Include Concurrent collections.
      // --------------------------------------------------------
      if (entities.AfterFy.Populated)
      {
        local.ActivityAfterFyEnd.Flag = "Y";
      }
      else if (ReadDebtAdjustment1())
      {
        local.ActivityAfterFyEnd.Flag = "Y";
      }
      else
      {
        // ---------------------
        // Not an error.
        // ---------------------
      }

      local.PrevSupp.Number = "";

      // --------------------------------------------
      // No need to check for overlapping roles.
      // Read Each will fetch distinct rows.
      // --------------------------------------------
      foreach(var _1 in ReadCsePersonSupported())
      {
        if (Equal(entities.Supp.Number, local.PrevSupp.Number))
        {
          continue;
        }

        local.PrevSupp.Number = entities.Supp.Number;
        ++local.RecordsReadSinceCommit.Count;

        // --------------------------------------------
        // Clear local views.
        // --------------------------------------------
        MoveDashboardAuditData1(local.Initialized, local.DashboardAuditData);

        // -------------------------------------------------------------------
        // Step #1. - Read debts where bal is due on 'run date'.
        // -------------------------------------------------------------------
        // -------------------------------------------------------------------
        // -READ debts with bal due
        // -Accruing debts must be due atleast 1 month before FY end.
        // -Non accruing debts are due upon creation.(Due Dt is irrelevant)
        // -Skip Fees, Recoveries
        // -Skip debts created after FY end.
        // -------------------------------------------------------------------
        // -------------------------------------------------------------------
        // -Exclude incoming interstate obligations. 04/14/08 CQ2461.
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
          // In a J/S situation, include the first obligation.
          // -------------------------------------------------------------------
          if (AsChar(entities.Obligation.PrimarySecondaryCode) == 'J')
          {
            if (!ReadObligationRln())
            {
              // -------------------------------------------------------------------
              // This must be the second obligation. Skip.
              // -------------------------------------------------------------------
              continue;
            }
          }

          // -------------------------------------------------------------------
          // -Skip MJ AF, MJ FC, MJ AFI, MJ FCI.
          // -------------------------------------------------------------------
          if (Equal(entities.ObligationType.Code, "MJ"))
          {
            UseFnDeterminePgmForDebtDetail();

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

          MoveDashboardAuditData1(local.Initialized, local.DashboardAuditData);
          local.DashboardAuditData.DebtBalanceDue =
            entities.DebtDetail.BalanceDueAmt;

          // -- Increment Statewide Level
          local.Statewide.ArrearsDueActual =
            (local.Statewide.ArrearsDueActual ?? 0M) + (
              local.DashboardAuditData.DebtBalanceDue ?? 0M);

          // -- Determine Judicial District...
          UseFnB734DetermineJdFromOrder();

          // -- Increment Judicial District Level
          if (!IsEmpty(local.DashboardAuditData.JudicialDistrict))
          {
            local.Local1.Index =
              (int)StringToNumber(local.DashboardAuditData.JudicialDistrict) - 1
              ;
            local.Local1.CheckSize();

            local.Local1.Update.G.ArrearsDueActual =
              (local.Local1.Item.G.ArrearsDueActual ?? 0M) + (
                local.DashboardAuditData.DebtBalanceDue ?? 0M);
          }

          // -- Log to the audit table.
          local.DashboardAuditData.DashboardPriority = "1-8(#1A)";
          local.DashboardAuditData.DebtDueDate = entities.DebtDetail.DueDt;
          local.DashboardAuditData.SuppCspNumber = entities.Supp.Number;
          local.DashboardAuditData.PayorCspNumber = entities.ApCsePerson.Number;
          local.DashboardAuditData.DebtType = entities.ObligationType.Code;

          if (AsChar(import.AuditFlag.Flag) == 'Y')
          {
            UseFnB734CreateDashboardAudit();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }
          }

          // --------------------------------------------
          // Process Debt Adj after FY end.
          // --------------------------------------------
          foreach(var _3 in ReadDebtAdjustment2())
          {
            // ------------------------------------------------------------------
            // 'I' type adj increases balance_due. So we need to 'subtract'
            // this amt from balance at run time to get balance due on 9/30.
            // Similarily, we need to add 'D' type adj amount.
            // ------------------------------------------------------------------
            if (AsChar(entities.DebtAdjustment.DebtAdjustmentType) == 'I')
            {
              local.DashboardAuditData.DebtBalanceDue =
                -entities.DebtAdjustment.Amount;
            }
            else
            {
              local.DashboardAuditData.DebtBalanceDue =
                entities.DebtAdjustment.Amount;
            }

            // -- Increment Statewide Level
            local.Statewide.ArrearsDueActual =
              (local.Statewide.ArrearsDueActual ?? 0M) + (
                local.DashboardAuditData.DebtBalanceDue ?? 0M);

            // -- Increment Judicial District Level
            if (!IsEmpty(local.DashboardAuditData.JudicialDistrict))
            {
              local.Local1.Index =
                (int)StringToNumber(local.DashboardAuditData.JudicialDistrict) -
                1;
              local.Local1.CheckSize();

              local.Local1.Update.G.ArrearsDueActual =
                (local.Local1.Item.G.ArrearsDueActual ?? 0M) + (
                  local.DashboardAuditData.DebtBalanceDue ?? 0M);
            }

            // -- Log to the audit table.
            local.DashboardAuditData.DashboardPriority = "1-8(#1B)";

            if (AsChar(import.AuditFlag.Flag) == 'Y')
            {
              UseFnB734CreateDashboardAudit();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                return;
              }
            }

            // ---------------------------------------------
            // End of Debt Adjustment READ EACH.
            // --------------------------------------------
          }

          // -------------------------------------------------------------------
          // Process collections and coll adj after FY end.
          // Read non-concurrent collections only.
          // -------------------------------------------------------------------
          // --------------------------------------------------------
          // 10/8/2001
          // - Also read 'C'urrent collections.
          // - Include Concurrent collections.
          // --------------------------------------------------------
          foreach(var _3 in ReadCollection1())
          {
            // ---------------------------------------------
            // Maintain running totals for this person.
            // --------------------------------------------
            if (Lt(import.ReportEndDate.Timestamp,
              entities.Collection.CreatedTmst))
            {
              local.DashboardAuditData.CollectionAmount =
                entities.Collection.Amount;
            }
            else
            {
              local.DashboardAuditData.CollectionAmount =
                -entities.Collection.Amount;
            }

            // -- Increment Statewide Level
            local.Statewide.ArrearsDueActual =
              (local.Statewide.ArrearsDueActual ?? 0M) + (
                local.DashboardAuditData.CollectionAmount ?? 0M);

            // -- Increment Judicial District Level
            if (!IsEmpty(local.DashboardAuditData.JudicialDistrict))
            {
              local.Local1.Index =
                (int)StringToNumber(local.DashboardAuditData.JudicialDistrict) -
                1;
              local.Local1.CheckSize();

              local.Local1.Update.G.ArrearsDueActual =
                (local.Local1.Item.G.ArrearsDueActual ?? 0M) + (
                  local.DashboardAuditData.CollectionAmount ?? 0M);
            }

            // -- Log to the audit table.
            local.DashboardAuditData.DebtBalanceDue = 0;
            local.DashboardAuditData.CollAppliedToCd =
              entities.Collection.AppliedToCode;
            local.DashboardAuditData.CollectionCreatedDate =
              Date(entities.Collection.CreatedTmst);
            local.DashboardAuditData.DashboardPriority = "1-8(#1C)";

            if (AsChar(import.AuditFlag.Flag) == 'Y')
            {
              UseFnB734CreateDashboardAudit();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                return;
              }
            }

            // --------------------------------------------
            // End of Collection READ EACH.
            // --------------------------------------------
          }

          // --------------------------------------------
          // End of non-zero bal Debt READ EACH.
          // --------------------------------------------
        }

        // -------------------------------------------------------------------
        // Step # 2. - Look for debts with 'Zero' bal due but where a
        // Collection is applied after FY end.
        // -------------------------------------------------------------------
        // -----------------------------------------------------------------------
        // Only do this if there is any collection/debt activity
        // for AP after FY end. There is no point in spinning through all
        // Zero debts if there is no activity at all for this AP.
        // -----------------------------------------------------------------------
        if (AsChar(local.ActivityAfterFyEnd.Flag) == 'Y')
        {
          // --------------------------------------------------------
          // 10/8/2001
          // - Also read 'C'urrent collections.
          // - Include Concurrent collections.
          // --------------------------------------------------------
          // -------------------------------------------------------------------
          // -Exclude incoming interstate obligations. 04/14/08 CQ2461.
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
            // In a J/S situation, include the first obligation.
            // -------------------------------------------------------------------
            if (AsChar(entities.Obligation.PrimarySecondaryCode) == 'J')
            {
              if (!ReadObligationRln())
              {
                // -------------------------------------------------------------------
                // This must be the second obligation. Skip.
                // -------------------------------------------------------------------
                continue;
              }
            }

            // -------------------------------------------------------------------
            // -Skip MJ AF, MJ FC, MJ AFI, MJ FCI.
            // -------------------------------------------------------------------
            if (Equal(entities.ObligationType.Code, "MJ"))
            {
              UseFnDeterminePgmForDebtDetail();

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

            MoveDashboardAuditData1(local.Initialized, local.DashboardAuditData);
            local.DashboardAuditData.DebtDueDate = entities.DebtDetail.DueDt;
            local.DashboardAuditData.SuppCspNumber = entities.Supp.Number;
            local.DashboardAuditData.PayorCspNumber =
              entities.ApCsePerson.Number;
            local.DashboardAuditData.DebtType = entities.ObligationType.Code;

            // -- Determine Judicial District...
            UseFnB734DetermineJdFromOrder();

            // --------------------------------------------
            // Process Debt Adj after FY end.
            // --------------------------------------------
            foreach(var _3 in ReadDebtAdjustment2())
            {
              // ------------------------------------------------------------------
              // 'I' type adj increases balance_due. So we need to 'subtract'
              // this amt from balance at run time to get balance due on 9/30.
              // Similarily, we need to add 'D' type adj amount.
              // ------------------------------------------------------------------
              if (AsChar(entities.DebtAdjustment.DebtAdjustmentType) == 'I')
              {
                local.DashboardAuditData.DebtBalanceDue =
                  -entities.DebtAdjustment.Amount;
              }
              else
              {
                local.DashboardAuditData.DebtBalanceDue =
                  entities.DebtAdjustment.Amount;
              }

              // -- Increment Statewide Level
              local.Statewide.ArrearsDueActual =
                (local.Statewide.ArrearsDueActual ?? 0M) + (
                  local.DashboardAuditData.DebtBalanceDue ?? 0M);

              // -- Increment Judicial District Level
              if (!IsEmpty(local.DashboardAuditData.JudicialDistrict))
              {
                local.Local1.Index =
                  (int)StringToNumber(local.DashboardAuditData.JudicialDistrict) -
                  1;
                local.Local1.CheckSize();

                local.Local1.Update.G.ArrearsDueActual =
                  (local.Local1.Item.G.ArrearsDueActual ?? 0M) + (
                    local.DashboardAuditData.DebtBalanceDue ?? 0M);
              }

              // -- Log to the audit table.
              local.DashboardAuditData.DashboardPriority = "1-8(#2A)";

              if (AsChar(import.AuditFlag.Flag) == 'Y')
              {
                UseFnB734CreateDashboardAudit();

                if (!IsExitState("ACO_NN0000_ALL_OK"))
                {
                  return;
                }
              }

              // ---------------------------------------------
              // End of Debt Adjustment READ EACH.
              // --------------------------------------------
            }

            // -------------------------------------------------------------------
            // Process collections and coll adj after FY end.
            // Read non-concurrent collections only.
            // -------------------------------------------------------------------
            // --------------------------------------------------------
            // 10/8/2001
            // - Also read 'C'urrent collections.
            // - Include Concurrent collections.
            // --------------------------------------------------------
            foreach(var _3 in ReadCollection2())
            {
              // ---------------------------------------------
              // Maintain running totals for this person.
              // --------------------------------------------
              if (Lt(import.ReportEndDate.Timestamp,
                entities.Collection.CreatedTmst))
              {
                local.DashboardAuditData.CollectionAmount =
                  entities.Collection.Amount;
              }
              else
              {
                local.DashboardAuditData.CollectionAmount =
                  -entities.Collection.Amount;
              }

              // -- Increment Statewide Level
              local.Statewide.ArrearsDueActual =
                (local.Statewide.ArrearsDueActual ?? 0M) + (
                  local.DashboardAuditData.CollectionAmount ?? 0M);

              // -- Increment Judicial District Level
              if (!IsEmpty(local.DashboardAuditData.JudicialDistrict))
              {
                local.Local1.Index =
                  (int)StringToNumber(local.DashboardAuditData.JudicialDistrict) -
                  1;
                local.Local1.CheckSize();

                local.Local1.Update.G.ArrearsDueActual =
                  (local.Local1.Item.G.ArrearsDueActual ?? 0M) + (
                    local.DashboardAuditData.CollectionAmount ?? 0M);
              }

              // -- Log to the audit table.
              local.DashboardAuditData.DebtBalanceDue = 0;
              local.DashboardAuditData.CollAppliedToCd =
                entities.Collection.AppliedToCode;
              local.DashboardAuditData.CollectionCreatedDate =
                Date(entities.Collection.CreatedTmst);
              local.DashboardAuditData.DashboardPriority = "1-8(#2B)";

              if (AsChar(import.AuditFlag.Flag) == 'Y')
              {
                UseFnB734CreateDashboardAudit();

                if (!IsExitState("ACO_NN0000_ALL_OK"))
                {
                  return;
                }
              }

              // --------------------------------------------
              // End of Collection READ EACH.
              // --------------------------------------------
            }

            // --------------------------------------------
            // End of Debt w/Coll READ EACH.
            // --------------------------------------------
          }

          // -------------------------------------------------------------------
          // Step # 3. - Look for debts with 'Zero' bal due but where a
          // D-type adjustment is made to debt after FY end.
          // -------------------------------------------------------------------
          // -------------------------------------------------------------------
          // -Exclude incoming interstate obligations. 04/14/08 CQ2461.
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
            // In a J/S situation, include the first obligation.
            // -------------------------------------------------------------------
            if (AsChar(entities.Obligation.PrimarySecondaryCode) == 'J')
            {
              if (!ReadObligationRln())
              {
                // -------------------------------------------------------------------
                // This must be the second obligation. Skip.
                // -------------------------------------------------------------------
                continue;
              }
            }

            // -------------------------------------------------------------------
            // -Skip MJ AF, MJ FC, MJ AFI, MJ FCI.
            // -------------------------------------------------------------------
            if (Equal(entities.ObligationType.Code, "MJ"))
            {
              UseFnDeterminePgmForDebtDetail();

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

            // ---------------------------------------------------
            // If a collection is applied to debt after FY end,
            // then debt must have been processed in Step # 2.
            // ---------------------------------------------------
            // --------------------------------------------------------
            // 10/8/2001
            // - Also read 'C'urrent collections.
            // - Include Concurrent collections.
            // --------------------------------------------------------
            if (ReadCollection3())
            {
              continue;
            }

            MoveDashboardAuditData1(local.Initialized, local.DashboardAuditData);
            local.DashboardAuditData.DebtDueDate = entities.DebtDetail.DueDt;
            local.DashboardAuditData.SuppCspNumber = entities.Supp.Number;
            local.DashboardAuditData.PayorCspNumber =
              entities.ApCsePerson.Number;
            local.DashboardAuditData.DebtType = entities.ObligationType.Code;

            // -- Determine Judicial District...
            UseFnB734DetermineJdFromOrder();

            // --------------------------------------------
            // Process Debt Adj after FY end.
            // --------------------------------------------
            foreach(var _3 in ReadDebtAdjustment2())
            {
              // ------------------------------------------------------------------
              // 'I' type adj increases balance_due. So we need to 'subtract'
              // this amt from balance at run time to get balance due on 9/30.
              // Similarily, we need to add 'D' type adj amount.
              // ------------------------------------------------------------------
              if (AsChar(entities.DebtAdjustment.DebtAdjustmentType) == 'I')
              {
                local.DashboardAuditData.DebtBalanceDue =
                  -entities.DebtAdjustment.Amount;
              }
              else
              {
                local.DashboardAuditData.DebtBalanceDue =
                  entities.DebtAdjustment.Amount;
              }

              // -- Increment Statewide Level
              local.Statewide.ArrearsDueActual =
                (local.Statewide.ArrearsDueActual ?? 0M) + (
                  local.DashboardAuditData.DebtBalanceDue ?? 0M);

              // -- Increment Judicial District Level
              if (!IsEmpty(local.DashboardAuditData.JudicialDistrict))
              {
                local.Local1.Index =
                  (int)StringToNumber(local.DashboardAuditData.JudicialDistrict) -
                  1;
                local.Local1.CheckSize();

                local.Local1.Update.G.ArrearsDueActual =
                  (local.Local1.Item.G.ArrearsDueActual ?? 0M) + (
                    local.DashboardAuditData.DebtBalanceDue ?? 0M);
              }

              // -- Log to the audit table.
              local.DashboardAuditData.DashboardPriority = "1-8(#3A)";

              if (AsChar(import.AuditFlag.Flag) == 'Y')
              {
                UseFnB734CreateDashboardAudit();

                if (!IsExitState("ACO_NN0000_ALL_OK"))
                {
                  return;
                }
              }

              // --------------------------------------------
              // End of Debt Adjustment READ EACH.
              // --------------------------------------------
            }

            // -------------------------------------------------------------------
            // Process collection adjustments after FY end.
            // Read non-concurrent collections only.
            // -------------------------------------------------------------------
            // --------------------------------------------------------
            // 10/8/2001
            // - Also read 'C'urrent collections.
            // - Include Concurrent collections.
            // --------------------------------------------------------
            foreach(var _3 in ReadCollection4())
            {
              // ---------------------------------------------
              // Maintain running totals for this person.
              // --------------------------------------------
              local.DashboardAuditData.CollectionAmount =
                -entities.Collection.Amount;

              // -- Increment Statewide Level
              local.Statewide.ArrearsDueActual =
                (local.Statewide.ArrearsDueActual ?? 0M) + (
                  local.DashboardAuditData.CollectionAmount ?? 0M);

              // -- Increment Judicial District Level
              if (!IsEmpty(local.DashboardAuditData.JudicialDistrict))
              {
                local.Local1.Index =
                  (int)StringToNumber(local.DashboardAuditData.JudicialDistrict) -
                  1;
                local.Local1.CheckSize();

                local.Local1.Update.G.ArrearsDueActual =
                  (local.Local1.Item.G.ArrearsDueActual ?? 0M) + (
                    local.DashboardAuditData.CollectionAmount ?? 0M);
              }

              // -- Log to the audit table.
              local.DashboardAuditData.DebtBalanceDue = 0;
              local.DashboardAuditData.CollAppliedToCd =
                entities.Collection.AppliedToCode;
              local.DashboardAuditData.CollectionCreatedDate =
                Date(entities.Collection.CreatedTmst);
              local.DashboardAuditData.DashboardPriority = "1-8(#3B)";

              if (AsChar(import.AuditFlag.Flag) == 'Y')
              {
                UseFnB734CreateDashboardAudit();

                if (!IsExitState("ACO_NN0000_ALL_OK"))
                {
                  return;
                }
              }

              // --------------------------------------------
              // End of Collection READ EACH.
              // --------------------------------------------
            }

            // --------------------------------------------
            // End of Debt w/ debt adj  READ EACH.
            // --------------------------------------------
          }
        }

        // -------------------------------------------------------
        // *** Finished processing all Debts for this person. ***
        // -------------------------------------------------------
        // --------------------------------------------
        // End of Supp Person READ EACH.
        // --------------------------------------------
      }

      // --------------------------------------------
      // End of driving READ EACH.
      // --------------------------------------------
    }

    // ------------------------------------------------------------------------------
    // -- Store final Statewide counts.
    // ------------------------------------------------------------------------------
    if (ReadDashboardStagingPriority3())
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
    for(local.Local1.Index = 0; local.Local1.Index < local.Local1.Count; ++
      local.Local1.Index)
    {
      if (!local.Local1.CheckSize())
      {
        break;
      }

      if (ReadDashboardStagingPriority4())
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
          local.Contractor.Update.Gcontractor.ArrearsDueActual =
            (local.Contractor.Item.Gcontractor.ArrearsDueActual ?? 0M) + (
              local.Local1.Item.G.ArrearsDueActual ?? 0M);

          goto Next;
        }
      }

      local.Contractor.CheckIndex();

Next:
      ;
    }

    local.Local1.CheckIndex();

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

      if (ReadDashboardStagingPriority5())
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

    local.Common.Count = 0;
    local.PrevRank.ArrearsDueActual = 0;
    local.Temp.ArrearsDueRnk = 1;

    // ------------------------------------------------------------------------------
    // -- Calculate Judicial District Ranking.
    // ------------------------------------------------------------------------------
    foreach(var _ in ReadDashboardStagingPriority6())
    {
      ++local.Common.Count;

      if ((entities.DashboardStagingPriority12.ArrearsDueActual ?? 0M) == (
        local.PrevRank.ArrearsDueActual ?? 0M))
      {
        // -- The ranking for this judicial district is tied with the previous 
        // judicial district.
        // -- This JD gets the same ranking already in the local_temp 
        // case_under_order_rank.
      }
      else
      {
        local.Temp.ArrearsDueRnk = local.Common.Count;
      }

      try
      {
        UpdateDashboardStagingPriority4();
        local.PrevRank.ArrearsDueActual =
          entities.DashboardStagingPriority12.ArrearsDueActual;
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
    local.PrevRank.ArrearsDueActual = 0;
    local.Temp.ArrearsDueRnk = 1;

    // ------------------------------------------------------------------------------
    // -- Calculate Contractor Ranking.
    // ------------------------------------------------------------------------------
    foreach(var _ in ReadDashboardStagingPriority7())
    {
      ++local.Common.Count;

      if ((entities.DashboardStagingPriority12.ArrearsDueActual ?? 0M) == (
        local.PrevRank.ArrearsDueActual ?? 0M))
      {
        // -- The ranking for this judicial district is tied with the previous 
        // judicial district.
        // -- This JD gets the same ranking already in the local_temp 
        // case_under_order_rank.
      }
      else
      {
        local.Temp.ArrearsDueRnk = local.Common.Count;
      }

      try
      {
        UpdateDashboardStagingPriority4();
        local.PrevRank.ArrearsDueActual =
          entities.DashboardStagingPriority12.ArrearsDueActual;
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
      Substring(import.ProgramCheckpointRestart.RestartInfo, 250, 1, 80) + "1-09    ";
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
    target.StandardNumber = source.StandardNumber;
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

    MoveCseOrganization(useExport.Contractor, local.Contractor1);
  }

  private void UseFnB734DetermineJdFromOrder()
  {
    var useImport = new FnB734DetermineJdFromOrder.Import();
    var useExport = new FnB734DetermineJdFromOrder.Export();

    useImport.PersistentDebt.Assign(entities.Debt);
    useImport.ReportEndDate.Date = import.ReportEndDate.Date;
    useImport.ReportStartDate.Date = import.ReportStartDate.Date;

    context.Call(FnB734DetermineJdFromOrder.Execute, useImport, useExport);

    MoveDashboardAuditData3(useExport.DashboardAuditData,
      local.DashboardAuditData);
  }

  private void UseFnDeterminePgmForDebtDetail()
  {
    var useImport = new FnDeterminePgmForDebtDetail.Import();
    var useExport = new FnDeterminePgmForDebtDetail.Export();

    MoveObligationType(entities.ObligationType, useImport.ObligationType);
    MoveDebtDetail(entities.DebtDetail, useImport.DebtDetail);
    useImport.Obligation.OrderTypeCode = entities.Obligation.OrderTypeCode;
    useImport.SupportedPerson.Number = entities.Supp.Number;

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
    var arrearsDueActual = local.Statewide.ArrearsDueActual ?? 0M;
    var arrearsDueRnk = local.Statewide.ArrearsDueRnk ?? 0;

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
        db.SetNullableDecimal(command, "curSupPdMthNum", param);
        db.SetNullableDecimal(command, "arrDueAct", arrearsDueActual);
        db.SetNullableInt32(command, "arrDueRnk", arrearsDueRnk);
        db.SetNullableDecimal(command, "casPerFteDen", param);
        db.SetNullableString(command, "contractorNum", "");
      });

    entities.DashboardStagingPriority12.ReportMonth = reportMonth;
    entities.DashboardStagingPriority12.ReportLevel = reportLevel;
    entities.DashboardStagingPriority12.ReportLevelId = reportLevelId;
    entities.DashboardStagingPriority12.AsOfDate = asOfDate;
    entities.DashboardStagingPriority12.ArrearsDueActual = arrearsDueActual;
    entities.DashboardStagingPriority12.ArrearsDueRnk = arrearsDueRnk;
    entities.DashboardStagingPriority12.ContractorNumber = "";
    entities.DashboardStagingPriority12.Populated = true;
  }

  private void CreateDashboardStagingPriority2()
  {
    var reportMonth = local.Local1.Item.G.ReportMonth;
    var reportLevel = local.Local1.Item.G.ReportLevel;
    var reportLevelId = local.Local1.Item.G.ReportLevelId;
    var asOfDate = local.Local1.Item.G.AsOfDate;
    var param = 0M;
    var arrearsDueActual = local.Local1.Item.G.ArrearsDueActual ?? 0M;
    var arrearsDueRnk = local.Local1.Item.G.ArrearsDueRnk ?? 0;

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
        db.SetNullableDecimal(command, "curSupPdMthNum", param);
        db.SetNullableDecimal(command, "arrDueAct", arrearsDueActual);
        db.SetNullableInt32(command, "arrDueRnk", arrearsDueRnk);
        db.SetNullableDecimal(command, "casPerFteDen", param);
        db.SetNullableString(command, "contractorNum", "");
      });

    entities.DashboardStagingPriority12.ReportMonth = reportMonth;
    entities.DashboardStagingPriority12.ReportLevel = reportLevel;
    entities.DashboardStagingPriority12.ReportLevelId = reportLevelId;
    entities.DashboardStagingPriority12.AsOfDate = asOfDate;
    entities.DashboardStagingPriority12.ArrearsDueActual = arrearsDueActual;
    entities.DashboardStagingPriority12.ArrearsDueRnk = arrearsDueRnk;
    entities.DashboardStagingPriority12.ContractorNumber = "";
    entities.DashboardStagingPriority12.Populated = true;
  }

  private void CreateDashboardStagingPriority3()
  {
    var reportMonth = local.Contractor.Item.Gcontractor.ReportMonth;
    var reportLevel = local.Contractor.Item.Gcontractor.ReportLevel;
    var reportLevelId = local.Contractor.Item.Gcontractor.ReportLevelId;
    var asOfDate = local.Contractor.Item.Gcontractor.AsOfDate;
    var param = 0M;
    var arrearsDueActual =
      local.Contractor.Item.Gcontractor.ArrearsDueActual ?? 0M;
    var arrearsDueRnk = local.Contractor.Item.Gcontractor.ArrearsDueRnk ?? 0;
    var contractorNumber =
      local.Contractor.Item.Gcontractor.ContractorNumber ?? "";

    entities.DashboardStagingPriority12.Populated = false;
    Update("CreateDashboardStagingPriority3",
      (db, command) =>
      {
        db.SetInt32(command, "reportMonth", reportMonth);
        db.SetString(command, "reportLevel", reportLevel);
        db.SetString(command, "reportLevelId", reportLevelId);
        db.SetNullableDate(command, "asOfDt", asOfDate);
        db.SetNullableInt32(command, "casUnderOrdNum", 0);
        db.SetNullableDecimal(command, "casUnderOrdPer", param);
        db.SetNullableDecimal(command, "curSupPdMthNum", param);
        db.SetNullableDecimal(command, "arrDueAct", arrearsDueActual);
        db.SetNullableInt32(command, "arrDueRnk", arrearsDueRnk);
        db.SetNullableDecimal(command, "casPerFteDen", param);
        db.SetNullableString(command, "contractorNum", contractorNumber);
      });

    entities.DashboardStagingPriority12.ReportMonth = reportMonth;
    entities.DashboardStagingPriority12.ReportLevel = reportLevel;
    entities.DashboardStagingPriority12.ReportLevelId = reportLevelId;
    entities.DashboardStagingPriority12.AsOfDate = asOfDate;
    entities.DashboardStagingPriority12.ArrearsDueActual = arrearsDueActual;
    entities.DashboardStagingPriority12.ArrearsDueRnk = arrearsDueRnk;
    entities.DashboardStagingPriority12.ContractorNumber = contractorNumber;
    entities.DashboardStagingPriority12.Populated = true;
  }

  private IEnumerable<bool> ReadCollection1()
  {
    System.Diagnostics.Debug.Assert(entities.Debt.Populated);

    return ReadEach("ReadCollection1",
      (db, command) =>
      {
        db.SetInt32(command, "otyId", entities.Debt.OtyType);
        db.SetString(command, "otrType", entities.Debt.Type1);
        db.SetInt32(command, "otrId", entities.Debt.SystemGeneratedIdentifier);
        db.SetString(command, "cpaType", entities.Debt.CpaType);
        db.SetString(command, "cspNumber", entities.Debt.CspNumber);
        db.SetInt32(command, "obgId", entities.Debt.ObgGeneratedId);
        db.SetDateTime(command, "createdTmst", import.ReportEndDate.Timestamp);
        db.SetDate(command, "collAdjDt", import.ReportEndDate.Date);
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

  private IEnumerable<bool> ReadCollection2()
  {
    System.Diagnostics.Debug.Assert(entities.Debt.Populated);

    return ReadEach("ReadCollection2",
      (db, command) =>
      {
        db.SetInt32(command, "otyId", entities.Debt.OtyType);
        db.SetString(command, "otrType", entities.Debt.Type1);
        db.SetInt32(command, "otrId", entities.Debt.SystemGeneratedIdentifier);
        db.SetString(command, "cpaType", entities.Debt.CpaType);
        db.SetString(command, "cspNumber", entities.Debt.CspNumber);
        db.SetInt32(command, "obgId", entities.Debt.ObgGeneratedId);
        db.SetDateTime(command, "createdTmst", import.ReportEndDate.Timestamp);
        db.SetDate(command, "collAdjDt", import.ReportEndDate.Date);
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

  private bool ReadCollection3()
  {
    System.Diagnostics.Debug.Assert(entities.Debt.Populated);
    entities.Collection.Populated = false;

    return Read("ReadCollection3",
      (db, command) =>
      {
        db.SetInt32(command, "otyId", entities.Debt.OtyType);
        db.SetString(command, "otrType", entities.Debt.Type1);
        db.SetInt32(command, "otrId", entities.Debt.SystemGeneratedIdentifier);
        db.SetString(command, "cpaType", entities.Debt.CpaType);
        db.SetString(command, "cspNumber", entities.Debt.CspNumber);
        db.SetInt32(command, "obgId", entities.Debt.ObgGeneratedId);
        db.SetDateTime(command, "createdTmst", import.ReportEndDate.Timestamp);
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
      });
  }

  private IEnumerable<bool> ReadCollection4()
  {
    System.Diagnostics.Debug.Assert(entities.Debt.Populated);

    return ReadEach("ReadCollection4",
      (db, command) =>
      {
        db.SetInt32(command, "otyId", entities.Debt.OtyType);
        db.SetString(command, "otrType", entities.Debt.Type1);
        db.SetInt32(command, "otrId", entities.Debt.SystemGeneratedIdentifier);
        db.SetString(command, "cpaType", entities.Debt.CpaType);
        db.SetString(command, "cspNumber", entities.Debt.CspNumber);
        db.SetInt32(command, "obgId", entities.Debt.ObgGeneratedId);
        db.SetDateTime(command, "createdTmst", import.ReportEndDate.Timestamp);
        db.SetDate(command, "collAdjDt", import.ReportEndDate.Date);
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

  private IEnumerable<bool> ReadCsePersonSupported()
  {
    return ReadEach("ReadCsePersonSupported",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ApCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Supp.Number = db.GetString(reader, 0);
        entities.Supported.CspNumber = db.GetString(reader, 0);
        entities.Supported.Type1 = db.GetString(reader, 1);
        entities.Supp.Populated = true;
        entities.Supported.Populated = true;
        CheckValid<CsePersonAccount>("Type1", entities.Supported.Type1);

        return true;
      },
      () =>
      {
        entities.Supported.Populated = false;
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
        entities.DashboardStagingPriority12.ArrearsDueActual =
          db.GetNullableDecimal(reader, 4);
        entities.DashboardStagingPriority12.ArrearsDueRnk =
          db.GetNullableInt32(reader, 5);
        entities.DashboardStagingPriority12.ContractorNumber =
          db.GetNullableString(reader, 6);
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
        entities.DashboardStagingPriority12.ArrearsDueActual =
          db.GetNullableDecimal(reader, 4);
        entities.DashboardStagingPriority12.ArrearsDueRnk =
          db.GetNullableInt32(reader, 5);
        entities.DashboardStagingPriority12.ContractorNumber =
          db.GetNullableString(reader, 6);
        entities.DashboardStagingPriority12.Populated = true;

        return true;
      },
      () =>
      {
        entities.DashboardStagingPriority12.Populated = false;
      });
  }

  private bool ReadDashboardStagingPriority3()
  {
    entities.DashboardStagingPriority12.Populated = false;

    return Read("ReadDashboardStagingPriority3",
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
        entities.DashboardStagingPriority12.ArrearsDueActual =
          db.GetNullableDecimal(reader, 4);
        entities.DashboardStagingPriority12.ArrearsDueRnk =
          db.GetNullableInt32(reader, 5);
        entities.DashboardStagingPriority12.ContractorNumber =
          db.GetNullableString(reader, 6);
        entities.DashboardStagingPriority12.Populated = true;
      });
  }

  private bool ReadDashboardStagingPriority4()
  {
    entities.DashboardStagingPriority12.Populated = false;

    return Read("ReadDashboardStagingPriority4",
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
        entities.DashboardStagingPriority12.ArrearsDueActual =
          db.GetNullableDecimal(reader, 4);
        entities.DashboardStagingPriority12.ArrearsDueRnk =
          db.GetNullableInt32(reader, 5);
        entities.DashboardStagingPriority12.ContractorNumber =
          db.GetNullableString(reader, 6);
        entities.DashboardStagingPriority12.Populated = true;
      });
  }

  private bool ReadDashboardStagingPriority5()
  {
    entities.DashboardStagingPriority12.Populated = false;

    return Read("ReadDashboardStagingPriority5",
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
        entities.DashboardStagingPriority12.ArrearsDueActual =
          db.GetNullableDecimal(reader, 4);
        entities.DashboardStagingPriority12.ArrearsDueRnk =
          db.GetNullableInt32(reader, 5);
        entities.DashboardStagingPriority12.ContractorNumber =
          db.GetNullableString(reader, 6);
        entities.DashboardStagingPriority12.Populated = true;
      });
  }

  private IEnumerable<bool> ReadDashboardStagingPriority6()
  {
    return ReadEach("ReadDashboardStagingPriority6",
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
        entities.DashboardStagingPriority12.ArrearsDueActual =
          db.GetNullableDecimal(reader, 4);
        entities.DashboardStagingPriority12.ArrearsDueRnk =
          db.GetNullableInt32(reader, 5);
        entities.DashboardStagingPriority12.ContractorNumber =
          db.GetNullableString(reader, 6);
        entities.DashboardStagingPriority12.Populated = true;

        return true;
      },
      () =>
      {
        entities.DashboardStagingPriority12.Populated = false;
      });
  }

  private IEnumerable<bool> ReadDashboardStagingPriority7()
  {
    return ReadEach("ReadDashboardStagingPriority7",
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
        entities.DashboardStagingPriority12.ArrearsDueActual =
          db.GetNullableDecimal(reader, 4);
        entities.DashboardStagingPriority12.ArrearsDueRnk =
          db.GetNullableInt32(reader, 5);
        entities.DashboardStagingPriority12.ContractorNumber =
          db.GetNullableString(reader, 6);
        entities.DashboardStagingPriority12.Populated = true;

        return true;
      },
      () =>
      {
        entities.DashboardStagingPriority12.Populated = false;
      });
  }

  private bool ReadDebtAdjustment1()
  {
    System.Diagnostics.Debug.Assert(entities.Obligor.Populated);
    entities.DebtAdjustment.Populated = false;

    return Read("ReadDebtAdjustment1",
      (db, command) =>
      {
        db.SetString(command, "cpaType", entities.Obligor.Type1);
        db.SetString(command, "cspNumber", entities.Obligor.CspNumber);
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

  private IEnumerable<bool> ReadDebtAdjustment2()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    System.Diagnostics.Debug.Assert(entities.Debt.Populated);

    return ReadEach("ReadDebtAdjustment2",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
        db.SetDate(command, "debAdjDt", import.ReportEndDate.Date);
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

  private IEnumerable<bool> ReadDebtObligationObligationTypeDebtDetail1()
  {
    System.Diagnostics.Debug.Assert(entities.Obligor.Populated);
    System.Diagnostics.Debug.Assert(entities.Supported.Populated);

    return ReadEach("ReadDebtObligationObligationTypeDebtDetail1",
      (db, command) =>
      {
        db.SetString(command, "cpaType", entities.Obligor.Type1);
        db.SetString(command, "cspNumber", entities.Obligor.CspNumber);
        db.SetNullableString(command, "cpaSupType", entities.Supported.Type1);
        db.SetNullableString(
          command, "cspSupNumber", entities.Supported.CspNumber);
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
        entities.Obligation.OrderTypeCode = db.GetString(reader, 12);
        entities.ObligationType.Code = db.GetString(reader, 13);
        entities.ObligationType.Classification = db.GetString(reader, 14);
        entities.DebtDetail.DueDt = db.GetDate(reader, 15);
        entities.DebtDetail.BalanceDueAmt = db.GetDecimal(reader, 16);
        entities.DebtDetail.CoveredPrdStartDt = db.GetNullableDate(reader, 17);
        entities.DebtDetail.PreconversionProgramCode =
          db.GetNullableString(reader, 18);
        entities.DebtDetail.CreatedTmst = db.GetDateTime(reader, 19);
        entities.Debt.Populated = true;
        entities.Obligation.Populated = true;
        entities.ObligationType.Populated = true;
        entities.DebtDetail.Populated = true;
        CheckValid<ObligationTransaction>("Type1", entities.Debt.Type1);
        CheckValid<Obligation>("PrimarySecondaryCode",
          entities.Obligation.PrimarySecondaryCode);
        CheckValid<Obligation>("OrderTypeCode",
          entities.Obligation.OrderTypeCode);
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
    System.Diagnostics.Debug.Assert(entities.Obligor.Populated);
    System.Diagnostics.Debug.Assert(entities.Supported.Populated);

    return ReadEach("ReadDebtObligationObligationTypeDebtDetail2",
      (db, command) =>
      {
        db.SetString(command, "cpaType", entities.Obligor.Type1);
        db.SetString(command, "cspNumber", entities.Obligor.CspNumber);
        db.SetNullableString(command, "cpaSupType", entities.Supported.Type1);
        db.SetNullableString(
          command, "cspSupNumber", entities.Supported.CspNumber);
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
        entities.Obligation.OrderTypeCode = db.GetString(reader, 12);
        entities.ObligationType.Code = db.GetString(reader, 13);
        entities.ObligationType.Classification = db.GetString(reader, 14);
        entities.DebtDetail.DueDt = db.GetDate(reader, 15);
        entities.DebtDetail.BalanceDueAmt = db.GetDecimal(reader, 16);
        entities.DebtDetail.CoveredPrdStartDt = db.GetNullableDate(reader, 17);
        entities.DebtDetail.PreconversionProgramCode =
          db.GetNullableString(reader, 18);
        entities.DebtDetail.CreatedTmst = db.GetDateTime(reader, 19);
        entities.Debt.Populated = true;
        entities.Obligation.Populated = true;
        entities.ObligationType.Populated = true;
        entities.DebtDetail.Populated = true;
        CheckValid<ObligationTransaction>("Type1", entities.Debt.Type1);
        CheckValid<Obligation>("PrimarySecondaryCode",
          entities.Obligation.PrimarySecondaryCode);
        CheckValid<Obligation>("OrderTypeCode",
          entities.Obligation.OrderTypeCode);
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
    System.Diagnostics.Debug.Assert(entities.Obligor.Populated);
    System.Diagnostics.Debug.Assert(entities.Supported.Populated);

    return ReadEach("ReadDebtObligationObligationTypeDebtDetail3",
      (db, command) =>
      {
        db.SetString(command, "cpaType", entities.Obligor.Type1);
        db.SetString(command, "cspNumber", entities.Obligor.CspNumber);
        db.SetNullableString(command, "cpaSupType", entities.Supported.Type1);
        db.SetNullableString(
          command, "cspSupNumber", entities.Supported.CspNumber);
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
        entities.Obligation.OrderTypeCode = db.GetString(reader, 12);
        entities.ObligationType.Code = db.GetString(reader, 13);
        entities.ObligationType.Classification = db.GetString(reader, 14);
        entities.DebtDetail.DueDt = db.GetDate(reader, 15);
        entities.DebtDetail.BalanceDueAmt = db.GetDecimal(reader, 16);
        entities.DebtDetail.CoveredPrdStartDt = db.GetNullableDate(reader, 17);
        entities.DebtDetail.PreconversionProgramCode =
          db.GetNullableString(reader, 18);
        entities.DebtDetail.CreatedTmst = db.GetDateTime(reader, 19);
        entities.Debt.Populated = true;
        entities.Obligation.Populated = true;
        entities.ObligationType.Populated = true;
        entities.DebtDetail.Populated = true;
        CheckValid<ObligationTransaction>("Type1", entities.Debt.Type1);
        CheckValid<Obligation>("PrimarySecondaryCode",
          entities.Obligation.PrimarySecondaryCode);
        CheckValid<Obligation>("OrderTypeCode",
          entities.Obligation.OrderTypeCode);
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

  private bool ReadObligationRln()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.ObligationRln.Populated = false;

    return Read("ReadObligationRln",
      (db, command) =>
      {
        db.SetInt32(command, "otyFirstId", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgFGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspFNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaFType", entities.Obligation.CpaType);
      },
      (db, reader) =>
      {
        entities.ObligationRln.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.ObligationRln.CspNumber = db.GetString(reader, 1);
        entities.ObligationRln.CpaType = db.GetString(reader, 2);
        entities.ObligationRln.ObgFGeneratedId = db.GetInt32(reader, 3);
        entities.ObligationRln.CspFNumber = db.GetString(reader, 4);
        entities.ObligationRln.CpaFType = db.GetString(reader, 5);
        entities.ObligationRln.OrrGeneratedId = db.GetInt32(reader, 6);
        entities.ObligationRln.OtySecondId = db.GetInt32(reader, 7);
        entities.ObligationRln.OtyFirstId = db.GetInt32(reader, 8);
        entities.ObligationRln.Description = db.GetString(reader, 9);
        entities.ObligationRln.Populated = true;
      });
  }

  private IEnumerable<bool> ReadObligorCsePersonCollection()
  {
    return ReadEachInSeparateTransaction("ReadObligorCsePersonCollection",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", local.Restart.Number);
        db.SetDateTime(command, "createdTmst", import.ReportEndDate.Timestamp);
      },
      (db, reader) =>
      {
        entities.Obligor.CspNumber = db.GetString(reader, 0);
        entities.ApCsePerson.Number = db.GetString(reader, 0);
        entities.Obligor.Type1 = db.GetString(reader, 1);
        entities.AfterFy.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.AfterFy.AppliedToCode = db.GetString(reader, 3);
        entities.AfterFy.CollectionDt = db.GetDate(reader, 4);
        entities.AfterFy.AdjustedInd = db.GetNullableString(reader, 5);
        entities.AfterFy.ConcurrentInd = db.GetString(reader, 6);
        entities.AfterFy.CrtType = db.GetInt32(reader, 7);
        entities.AfterFy.CstId = db.GetInt32(reader, 8);
        entities.AfterFy.CrvId = db.GetInt32(reader, 9);
        entities.AfterFy.CrdId = db.GetInt32(reader, 10);
        entities.AfterFy.ObgId = db.GetInt32(reader, 11);
        entities.AfterFy.CspNumber = db.GetString(reader, 12);
        entities.AfterFy.CpaType = db.GetString(reader, 13);
        entities.AfterFy.OtrId = db.GetInt32(reader, 14);
        entities.AfterFy.OtrType = db.GetString(reader, 15);
        entities.AfterFy.OtyId = db.GetInt32(reader, 16);
        entities.AfterFy.CollectionAdjustmentDt = db.GetDate(reader, 17);
        entities.AfterFy.CreatedTmst = db.GetDateTime(reader, 18);
        entities.AfterFy.Amount = db.GetDecimal(reader, 19);
        entities.AfterFy.ProgramAppliedTo = db.GetString(reader, 20);
        entities.Obligor.Populated = true;
        entities.ApCsePerson.Populated = true;
        entities.AfterFy.Populated = db.GetNullableInt32(reader, 2) != null;
        CheckValid<CsePersonAccount>("Type1", entities.Obligor.Type1);

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
        entities.ApCsePerson.Populated = false;
        entities.Obligor.Populated = false;
        entities.AfterFy.Populated = false;
      });
  }

  private void UpdateDashboardStagingPriority1()
  {
    var asOfDate = local.Statewide.AsOfDate;
    var arrearsDueActual = local.Statewide.ArrearsDueActual ?? 0M;
    var arrearsDueRnk = local.Statewide.ArrearsDueRnk ?? 0;

    entities.DashboardStagingPriority12.Populated = false;
    Update("UpdateDashboardStagingPriority1",
      (db, command) =>
      {
        db.SetNullableDate(command, "asOfDt", asOfDate);
        db.SetNullableDecimal(command, "arrDueAct", arrearsDueActual);
        db.SetNullableInt32(command, "arrDueRnk", arrearsDueRnk);
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
    entities.DashboardStagingPriority12.ArrearsDueActual = arrearsDueActual;
    entities.DashboardStagingPriority12.ArrearsDueRnk = arrearsDueRnk;
    entities.DashboardStagingPriority12.Populated = true;
  }

  private void UpdateDashboardStagingPriority2()
  {
    var asOfDate = local.Local1.Item.G.AsOfDate;
    var arrearsDueActual = local.Local1.Item.G.ArrearsDueActual ?? 0M;
    var arrearsDueRnk = local.Local1.Item.G.ArrearsDueRnk ?? 0;

    entities.DashboardStagingPriority12.Populated = false;
    Update("UpdateDashboardStagingPriority2",
      (db, command) =>
      {
        db.SetNullableDate(command, "asOfDt", asOfDate);
        db.SetNullableDecimal(command, "arrDueAct", arrearsDueActual);
        db.SetNullableInt32(command, "arrDueRnk", arrearsDueRnk);
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
    entities.DashboardStagingPriority12.ArrearsDueActual = arrearsDueActual;
    entities.DashboardStagingPriority12.ArrearsDueRnk = arrearsDueRnk;
    entities.DashboardStagingPriority12.Populated = true;
  }

  private void UpdateDashboardStagingPriority3()
  {
    var asOfDate = local.Contractor.Item.Gcontractor.AsOfDate;
    var arrearsDueActual =
      local.Contractor.Item.Gcontractor.ArrearsDueActual ?? 0M;
    var arrearsDueRnk = local.Contractor.Item.Gcontractor.ArrearsDueRnk ?? 0;
    var contractorNumber =
      local.Contractor.Item.Gcontractor.ContractorNumber ?? "";

    entities.DashboardStagingPriority12.Populated = false;
    Update("UpdateDashboardStagingPriority3",
      (db, command) =>
      {
        db.SetNullableDate(command, "asOfDt", asOfDate);
        db.SetNullableDecimal(command, "arrDueAct", arrearsDueActual);
        db.SetNullableInt32(command, "arrDueRnk", arrearsDueRnk);
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
    entities.DashboardStagingPriority12.ArrearsDueActual = arrearsDueActual;
    entities.DashboardStagingPriority12.ArrearsDueRnk = arrearsDueRnk;
    entities.DashboardStagingPriority12.ContractorNumber = contractorNumber;
    entities.DashboardStagingPriority12.Populated = true;
  }

  private void UpdateDashboardStagingPriority4()
  {
    var arrearsDueRnk = local.Temp.ArrearsDueRnk ?? 0;

    entities.DashboardStagingPriority12.Populated = false;
    Update("UpdateDashboardStagingPriority4",
      (db, command) =>
      {
        db.SetNullableInt32(command, "arrDueRnk", arrearsDueRnk);
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

    entities.DashboardStagingPriority12.ArrearsDueRnk = arrearsDueRnk;
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

    private DashboardAuditData? dashboardAuditData;
    private ProgramCheckpointRestart? programCheckpointRestart;
    private ProgramProcessingInfo? programProcessingInfo;
    private DateWorkArea? reportEndDate;
    private DateWorkArea? reportStartDate;
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
    /// A value of Program.
    /// </summary>
    public Program Program
    {
      get => program ??= new();
      set => program = value;
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
    /// A value of PrevAp.
    /// </summary>
    public CsePerson PrevAp
    {
      get => prevAp ??= new();
      set => prevAp = value;
    }

    /// <summary>
    /// A value of ActivityAfterFyEnd.
    /// </summary>
    public Common ActivityAfterFyEnd
    {
      get => activityAfterFyEnd ??= new();
      set => activityAfterFyEnd = value;
    }

    /// <summary>
    /// A value of PrevSupp.
    /// </summary>
    public CsePerson PrevSupp
    {
      get => prevSupp ??= new();
      set => prevSupp = value;
    }

    /// <summary>
    /// A value of Restart.
    /// </summary>
    public CsePerson Restart
    {
      get => restart ??= new();
      set => restart = value;
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

    private DashboardAuditData? initialized;
    private ProgramCheckpointRestart? programCheckpointRestart;
    private DashboardStagingPriority12? statewide;
    private EabFileHandling? eabFileHandling;
    private EabReportSend? eabReportSend;
    private Array<LocalGroup>? local1;
    private Common? recordsReadSinceCommit;
    private DashboardAuditData? dashboardAuditData;
    private Program? program;
    private DashboardStagingPriority12? temp;
    private Common? common;
    private DashboardStagingPriority12? prevRank;
    private CsePerson? prevAp;
    private Common? activityAfterFyEnd;
    private CsePerson? prevSupp;
    private CsePerson? restart;
    private CseOrganization? contractor1;
    private Array<ContractorGroup>? contractor;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of ChOrAr.
    /// </summary>
    public CaseRole ChOrAr
    {
      get => chOrAr ??= new();
      set => chOrAr = value;
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
    /// A value of ObligationRln.
    /// </summary>
    public ObligationRln ObligationRln
    {
      get => obligationRln ??= new();
      set => obligationRln = value;
    }

    private CseOrganization? cseOrganization;
    private DashboardStagingPriority12? dashboardStagingPriority12;
    private Case1? case1;
    private ObligationType? obligationType;
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
    private CaseRole? chOrAr;
    private CsePerson? supp;
    private Collection? afterFy;
    private ObligationRln? obligationRln;
  }
#endregion
}
