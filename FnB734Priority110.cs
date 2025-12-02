// Program: FN_B734_PRIORITY_1_10, ID: 945132081, model: 746.
// Short name: SWE03091
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
/// A program: FN_B734_PRIORITY_1_10.
/// </para>
/// <para>
/// Priority 1-10: Income Withholdings Per Obligated Case
/// </para>
/// </summary>
[Serializable]
[Program("SWE03091")]
public partial class FnB734Priority110: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B734_PRIORITY_1_10 program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB734Priority110(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB734Priority110.
  /// </summary>
  public FnB734Priority110(IContext context, Import import, Export export):
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
    // ---------------------------------------------------------------------------------------------------
    // -------------------------------------------------------------------------------------
    // Priority 1-10: Income Withholdings Per Obligated Case
    // -------------------------------------------------------------------------------------
    // This is a measure of ORD-IWO2 (Notice to Withhold Incomes) generated in 
    // the report period per case.
    // Report Level: State, Judicial District
    // Report Period: Month
    // Numerator
    // Income Withholdings Issued by Caseworker
    // 	1) Case open at any time during report period.
    // 	2) Count I class legal action with CREATED DATE entered in current 
    // report
    // 	   period.  (usually not a file date entered on these legal actions)
    // 	3) Count only following action taken: ORDIWO2 (ORD/NOTICE WITHHOLD 
    // INCOME CS)
    // 	4) An associated entry must exist on the IWGL screen.
    // 	5) Credit each unique legal action only once.
    // Denominator
    // 	1)	Refer to Priority 1.1 (Numerator).
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
      (import.ProgramCheckpointRestart.RestartInfo, 81, 8, "1-10    "))
    {
      // -- Checkpoint Info
      // Positions   Value
      // ---------   
      // ------------------------------------
      //  001-080    General Checkpoint Info for PRAD
      //  081-088    Dashboard Priority
      //  089-089    Blank
      //  090-098    Legal Action Identifier
      if (!IsEmpty(Substring(import.ProgramCheckpointRestart.RestartInfo, 90, 9)))
      {
        local.Restart.Identifier =
          (int)StringToNumber(Substring(
            import.ProgramCheckpointRestart.RestartInfo, 250, 90, 9));
      }

      if (local.Restart.Identifier > 0)
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
          local.Statewide.IwoPerObligCaseAverage = 0;
          local.Statewide.IwoPerObligCaseDenominator = 0;
          local.Statewide.IwoPerObligCaseNumerator = 0;
          local.Statewide.IwoPerObligCaseRnk = 0;
        }

        // -- Load Judicial District counts.
        foreach(var _ in ReadDashboardStagingPriority2())
        {
          local.Local1.Index =
            (int)StringToNumber(entities.DashboardStagingPriority12.
              ReportLevelId) - 1;
          local.Local1.CheckSize();

          local.Local1.Update.G.Assign(entities.DashboardStagingPriority12);
          local.Local1.Update.G.IwoPerObligCaseAverage = 0;
          local.Local1.Update.G.IwoPerObligCaseDenominator = 0;
          local.Local1.Update.G.IwoPerObligCaseNumerator = 0;
          local.Local1.Update.G.IwoPerObligCaseRnk = 0;
        }
      }
    }
    else
    {
      local.Restart.Identifier = 0;
    }

    foreach(var _ in ReadLegalActionLegalActionIncomeSource())
    {
      // -- Skip the legal action if no IWGL record exists.
      if (!entities.LegalActionIncomeSource.Populated)
      {
        continue;
      }

      // -- Skip the legal action if no associated case was open during the 
      // reporting period.
      foreach(var _1 in ReadCaseCaseAssignment())
      {
        if (entities.CaseAssignment.Populated)
        {
          break;
        }
      }

      if (!entities.Case1.Populated)
      {
        continue;
      }

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
        //  090-098    Legal Action Identifier
        local.ProgramCheckpointRestart.RestartInd = "Y";
        local.ProgramCheckpointRestart.RestartInfo =
          Substring(import.ProgramCheckpointRestart.RestartInfo, 250, 1, 80) + "1-10    " +
          " " + NumberToString(local.Prev.Identifier, 7, 9);
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

      local.Prev.Identifier = entities.LegalAction.Identifier;
      ++local.RecordsReadSinceCommit.Count;
      MoveDashboardAuditData1(local.Initialized, local.DashboardAuditData);

      // -- Increment Statewide Level
      local.Statewide.IwoPerObligCaseNumerator =
        (local.Statewide.IwoPerObligCaseNumerator ?? 0) + 1;

      // -- Determine Judicial District...
      UseFnB734DetermineJdFromOrder();

      // -- Increment Judicial District Level
      if (!IsEmpty(local.DashboardAuditData.JudicialDistrict))
      {
        local.Local1.Index =
          (int)StringToNumber(local.DashboardAuditData.JudicialDistrict) - 1;
        local.Local1.CheckSize();

        // -- Increment In-Month Judicial District Level
        local.Local1.Update.G.IwoPerObligCaseNumerator =
          (local.Local1.Item.G.IwoPerObligCaseNumerator ?? 0) + 1;
      }

      // -- Log to the audit table.
      local.DashboardAuditData.DashboardPriority = "1-10(N)";
      local.DashboardAuditData.LegalActionDate =
        Date(entities.LegalAction.CreatedTstamp);
      local.DashboardAuditData.StandardNumber =
        entities.LegalAction.StandardNumber;

      if (AsChar(import.AuditFlag.Flag) == 'Y')
      {
        UseFnB734CreateDashboardAudit();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }
      }
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
          local.Contractor.Update.Gcontractor.IwoPerObligCaseDenominator =
            (local.Contractor.Item.Gcontractor.IwoPerObligCaseDenominator ?? 0) +
            (local.Local1.Item.G.IwoPerObligCaseDenominator ?? 0);
          local.Contractor.Update.Gcontractor.IwoPerObligCaseNumerator =
            (local.Contractor.Item.Gcontractor.IwoPerObligCaseNumerator ?? 0) +
            (local.Local1.Item.G.IwoPerObligCaseNumerator ?? 0);

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

    // ------------------------------------------------------------------------------
    // -- Calculate the Denominator, and Average using values previously
    // -- stored during processing for cases under order numerator.
    // ------------------------------------------------------------------------------
    foreach(var _ in ReadDashboardStagingPriority6())
    {
      local.Temp.IwoPerObligCaseNumerator =
        entities.DashboardStagingPriority12.IwoPerObligCaseNumerator;
      local.Temp.IwoPerObligCaseDenominator =
        entities.DashboardStagingPriority12.CasesUnderOrderNumerator;

      if ((local.Temp.IwoPerObligCaseDenominator ?? 0) == 0)
      {
        local.Temp.IwoPerObligCaseAverage = 0;
      }
      else
      {
        local.Temp.IwoPerObligCaseAverage =
          Math.Round((decimal)(local.Temp.IwoPerObligCaseNumerator ?? 0) /
          (local.Temp.IwoPerObligCaseDenominator ?? 0), 2,
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
    local.PrevRank.IwoPerObligCaseAverage = 0;
    local.Temp.IwoPerObligCaseRnk = 1;

    // ------------------------------------------------------------------------------
    // -- Calculate Judicial District Ranking (in month).
    // ------------------------------------------------------------------------------
    foreach(var _ in ReadDashboardStagingPriority7())
    {
      ++local.Common.Count;

      if ((entities.DashboardStagingPriority12.IwoPerObligCaseAverage ?? 0M) ==
        (local.PrevRank.IwoPerObligCaseAverage ?? 0M))
      {
        // -- The ranking for this judicial district is tied with the previous 
        // judicial district.
        // -- This JD gets the same ranking already in the local_temp 
        // case_under_order_rank.
      }
      else
      {
        local.Temp.IwoPerObligCaseRnk = local.Common.Count;
      }

      try
      {
        UpdateDashboardStagingPriority5();
        local.PrevRank.IwoPerObligCaseAverage =
          entities.DashboardStagingPriority12.IwoPerObligCaseAverage;
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
    local.PrevRank.IwoPerObligCaseAverage = 0;
    local.Temp.IwoPerObligCaseRnk = 1;

    // ------------------------------------------------------------------------------
    // -- Calculate Contractor Ranking (in month).
    // ------------------------------------------------------------------------------
    foreach(var _ in ReadDashboardStagingPriority8())
    {
      ++local.Common.Count;

      if ((entities.DashboardStagingPriority12.IwoPerObligCaseAverage ?? 0M) ==
        (local.PrevRank.IwoPerObligCaseAverage ?? 0M))
      {
        // -- The ranking for this judicial district is tied with the previous 
        // judicial district.
        // -- This JD gets the same ranking already in the local_temp 
        // case_under_order_rank.
      }
      else
      {
        local.Temp.IwoPerObligCaseRnk = local.Common.Count;
      }

      try
      {
        UpdateDashboardStagingPriority5();
        local.PrevRank.IwoPerObligCaseAverage =
          entities.DashboardStagingPriority12.IwoPerObligCaseAverage;
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
    local.ProgramCheckpointRestart.RestartInd = "Y";

    // -- Checkpoint Info
    // Positions   Value
    // ---------   
    // ------------------------------------
    //  001-080    General Checkpoint Info for PRAD
    //  081-088    Dashboard Priority
    local.ProgramCheckpointRestart.RestartInd = "Y";
    local.ProgramCheckpointRestart.RestartInfo = "";
    local.ProgramCheckpointRestart.RestartInfo =
      Substring(import.ProgramCheckpointRestart.RestartInfo, 250, 1, 80) + "1-11    ";
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

    useImport.PersistentLegalAction.Assign(entities.LegalAction);
    useImport.ReportStartDate.Date = import.ReportStartDate.Date;
    useImport.ReportEndDate.Date = import.ReportEndDate.Date;

    context.Call(FnB734DetermineJdFromOrder.Execute, useImport, useExport);

    MoveDashboardAuditData3(useExport.DashboardAuditData,
      local.DashboardAuditData);
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
    var iwoPerObligCaseNumerator = local.Statewide.IwoPerObligCaseNumerator ?? 0
      ;
    var iwoPerObligCaseDenominator =
      local.Statewide.IwoPerObligCaseDenominator ?? 0;
    var iwoPerObligCaseAverage = local.Statewide.IwoPerObligCaseAverage ?? 0M;
    var iwoPerObligCaseRnk = local.Statewide.IwoPerObligCaseRnk ?? 0;

    entities.DashboardStagingPriority12.Populated = false;
    Update("CreateDashboardStagingPriority1",
      (db, command) =>
      {
        db.SetInt32(command, "reportMonth", reportMonth);
        db.SetString(command, "reportLevel", reportLevel);
        db.SetString(command, "reportLevelId", reportLevelId);
        db.SetNullableDate(command, "asOfDt", asOfDate);
        db.SetNullableInt32(command, "casUnderOrdNum", 0);
        db.SetNullableInt32(command, "casUnderOrdDen", 0);
        db.SetNullableDecimal(command, "casUnderOrdPer", param);
        db.SetNullableDecimal(command, "curSupPdMthNum", param);
        db.SetNullableInt32(command, "iwoOblCasNum", iwoPerObligCaseNumerator);
        db.
          SetNullableInt32(command, "iwoOblCasDen", iwoPerObligCaseDenominator);
        db.SetNullableDecimal(command, "iwoOblCasAvg", iwoPerObligCaseAverage);
        db.SetNullableInt32(command, "iwoOblCasRnk", iwoPerObligCaseRnk);
        db.SetNullableDecimal(command, "casPerFteDen", param);
        db.SetNullableString(command, "contractorNum", "");
      });

    entities.DashboardStagingPriority12.ReportMonth = reportMonth;
    entities.DashboardStagingPriority12.ReportLevel = reportLevel;
    entities.DashboardStagingPriority12.ReportLevelId = reportLevelId;
    entities.DashboardStagingPriority12.AsOfDate = asOfDate;
    entities.DashboardStagingPriority12.CasesUnderOrderNumerator = 0;
    entities.DashboardStagingPriority12.IwoPerObligCaseNumerator =
      iwoPerObligCaseNumerator;
    entities.DashboardStagingPriority12.IwoPerObligCaseDenominator =
      iwoPerObligCaseDenominator;
    entities.DashboardStagingPriority12.IwoPerObligCaseAverage =
      iwoPerObligCaseAverage;
    entities.DashboardStagingPriority12.IwoPerObligCaseRnk = iwoPerObligCaseRnk;
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
    var iwoPerObligCaseNumerator =
      local.Local1.Item.G.IwoPerObligCaseNumerator ?? 0;
    var iwoPerObligCaseDenominator =
      local.Local1.Item.G.IwoPerObligCaseDenominator ?? 0;
    var iwoPerObligCaseAverage = local.Local1.Item.G.IwoPerObligCaseAverage ?? 0M
      ;
    var iwoPerObligCaseRnk = local.Local1.Item.G.IwoPerObligCaseRnk ?? 0;

    entities.DashboardStagingPriority12.Populated = false;
    Update("CreateDashboardStagingPriority2",
      (db, command) =>
      {
        db.SetInt32(command, "reportMonth", reportMonth);
        db.SetString(command, "reportLevel", reportLevel);
        db.SetString(command, "reportLevelId", reportLevelId);
        db.SetNullableDate(command, "asOfDt", asOfDate);
        db.SetNullableInt32(command, "casUnderOrdNum", 0);
        db.SetNullableInt32(command, "casUnderOrdDen", 0);
        db.SetNullableDecimal(command, "casUnderOrdPer", param);
        db.SetNullableDecimal(command, "curSupPdMthNum", param);
        db.SetNullableInt32(command, "iwoOblCasNum", iwoPerObligCaseNumerator);
        db.
          SetNullableInt32(command, "iwoOblCasDen", iwoPerObligCaseDenominator);
        db.SetNullableDecimal(command, "iwoOblCasAvg", iwoPerObligCaseAverage);
        db.SetNullableInt32(command, "iwoOblCasRnk", iwoPerObligCaseRnk);
        db.SetNullableDecimal(command, "casPerFteDen", param);
        db.SetNullableString(command, "contractorNum", "");
      });

    entities.DashboardStagingPriority12.ReportMonth = reportMonth;
    entities.DashboardStagingPriority12.ReportLevel = reportLevel;
    entities.DashboardStagingPriority12.ReportLevelId = reportLevelId;
    entities.DashboardStagingPriority12.AsOfDate = asOfDate;
    entities.DashboardStagingPriority12.CasesUnderOrderNumerator = 0;
    entities.DashboardStagingPriority12.IwoPerObligCaseNumerator =
      iwoPerObligCaseNumerator;
    entities.DashboardStagingPriority12.IwoPerObligCaseDenominator =
      iwoPerObligCaseDenominator;
    entities.DashboardStagingPriority12.IwoPerObligCaseAverage =
      iwoPerObligCaseAverage;
    entities.DashboardStagingPriority12.IwoPerObligCaseRnk = iwoPerObligCaseRnk;
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
    var iwoPerObligCaseNumerator =
      local.Contractor.Item.Gcontractor.IwoPerObligCaseNumerator ?? 0;
    var iwoPerObligCaseDenominator =
      local.Contractor.Item.Gcontractor.IwoPerObligCaseDenominator ?? 0;
    var iwoPerObligCaseAverage =
      local.Contractor.Item.Gcontractor.IwoPerObligCaseAverage ?? 0M;
    var iwoPerObligCaseRnk =
      local.Contractor.Item.Gcontractor.IwoPerObligCaseRnk ?? 0;
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
        db.SetNullableInt32(command, "casUnderOrdDen", 0);
        db.SetNullableDecimal(command, "casUnderOrdPer", param);
        db.SetNullableDecimal(command, "curSupPdMthNum", param);
        db.SetNullableInt32(command, "iwoOblCasNum", iwoPerObligCaseNumerator);
        db.
          SetNullableInt32(command, "iwoOblCasDen", iwoPerObligCaseDenominator);
        db.SetNullableDecimal(command, "iwoOblCasAvg", iwoPerObligCaseAverage);
        db.SetNullableInt32(command, "iwoOblCasRnk", iwoPerObligCaseRnk);
        db.SetNullableDecimal(command, "casPerFteDen", param);
        db.SetNullableString(command, "contractorNum", contractorNumber);
      });

    entities.DashboardStagingPriority12.ReportMonth = reportMonth;
    entities.DashboardStagingPriority12.ReportLevel = reportLevel;
    entities.DashboardStagingPriority12.ReportLevelId = reportLevelId;
    entities.DashboardStagingPriority12.AsOfDate = asOfDate;
    entities.DashboardStagingPriority12.CasesUnderOrderNumerator = 0;
    entities.DashboardStagingPriority12.IwoPerObligCaseNumerator =
      iwoPerObligCaseNumerator;
    entities.DashboardStagingPriority12.IwoPerObligCaseDenominator =
      iwoPerObligCaseDenominator;
    entities.DashboardStagingPriority12.IwoPerObligCaseAverage =
      iwoPerObligCaseAverage;
    entities.DashboardStagingPriority12.IwoPerObligCaseRnk = iwoPerObligCaseRnk;
    entities.DashboardStagingPriority12.ContractorNumber = contractorNumber;
    entities.DashboardStagingPriority12.Populated = true;
  }

  private IEnumerable<bool> ReadCaseCaseAssignment()
  {
    return ReadEach("ReadCaseCaseAssignment",
      (db, command) =>
      {
        db.SetInt32(command, "lgaId", entities.LegalAction.Identifier);
        db.SetNullableDate(
          command, "discontinueDate", import.ReportStartDate.Date);
        db.SetDate(command, "effectiveDate", import.ReportEndDate.Date);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.CaseAssignment.ReasonCode = db.GetString(reader, 1);
        entities.CaseAssignment.EffectiveDate = db.GetDate(reader, 2);
        entities.CaseAssignment.DiscontinueDate = db.GetNullableDate(reader, 3);
        entities.CaseAssignment.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.CaseAssignment.SpdId = db.GetInt32(reader, 5);
        entities.CaseAssignment.OffId = db.GetInt32(reader, 6);
        entities.CaseAssignment.OspCode = db.GetString(reader, 7);
        entities.CaseAssignment.OspDate = db.GetDate(reader, 8);
        entities.CaseAssignment.CasNo = db.GetString(reader, 9);
        entities.Case1.Populated = true;
        entities.CaseAssignment.Populated = db.GetNullableString(reader, 1) != null
          ;

        return true;
      },
      () =>
      {
        entities.CaseAssignment.Populated = false;
        entities.Case1.Populated = false;
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
        entities.DashboardStagingPriority12.IwoPerObligCaseNumerator =
          db.GetNullableInt32(reader, 5);
        entities.DashboardStagingPriority12.IwoPerObligCaseDenominator =
          db.GetNullableInt32(reader, 6);
        entities.DashboardStagingPriority12.IwoPerObligCaseAverage =
          db.GetNullableDecimal(reader, 7);
        entities.DashboardStagingPriority12.IwoPerObligCaseRnk =
          db.GetNullableInt32(reader, 8);
        entities.DashboardStagingPriority12.ContractorNumber =
          db.GetNullableString(reader, 9);
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
        entities.DashboardStagingPriority12.IwoPerObligCaseNumerator =
          db.GetNullableInt32(reader, 5);
        entities.DashboardStagingPriority12.IwoPerObligCaseDenominator =
          db.GetNullableInt32(reader, 6);
        entities.DashboardStagingPriority12.IwoPerObligCaseAverage =
          db.GetNullableDecimal(reader, 7);
        entities.DashboardStagingPriority12.IwoPerObligCaseRnk =
          db.GetNullableInt32(reader, 8);
        entities.DashboardStagingPriority12.ContractorNumber =
          db.GetNullableString(reader, 9);
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
        entities.DashboardStagingPriority12.CasesUnderOrderNumerator =
          db.GetNullableInt32(reader, 4);
        entities.DashboardStagingPriority12.IwoPerObligCaseNumerator =
          db.GetNullableInt32(reader, 5);
        entities.DashboardStagingPriority12.IwoPerObligCaseDenominator =
          db.GetNullableInt32(reader, 6);
        entities.DashboardStagingPriority12.IwoPerObligCaseAverage =
          db.GetNullableDecimal(reader, 7);
        entities.DashboardStagingPriority12.IwoPerObligCaseRnk =
          db.GetNullableInt32(reader, 8);
        entities.DashboardStagingPriority12.ContractorNumber =
          db.GetNullableString(reader, 9);
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
        entities.DashboardStagingPriority12.CasesUnderOrderNumerator =
          db.GetNullableInt32(reader, 4);
        entities.DashboardStagingPriority12.IwoPerObligCaseNumerator =
          db.GetNullableInt32(reader, 5);
        entities.DashboardStagingPriority12.IwoPerObligCaseDenominator =
          db.GetNullableInt32(reader, 6);
        entities.DashboardStagingPriority12.IwoPerObligCaseAverage =
          db.GetNullableDecimal(reader, 7);
        entities.DashboardStagingPriority12.IwoPerObligCaseRnk =
          db.GetNullableInt32(reader, 8);
        entities.DashboardStagingPriority12.ContractorNumber =
          db.GetNullableString(reader, 9);
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
        entities.DashboardStagingPriority12.CasesUnderOrderNumerator =
          db.GetNullableInt32(reader, 4);
        entities.DashboardStagingPriority12.IwoPerObligCaseNumerator =
          db.GetNullableInt32(reader, 5);
        entities.DashboardStagingPriority12.IwoPerObligCaseDenominator =
          db.GetNullableInt32(reader, 6);
        entities.DashboardStagingPriority12.IwoPerObligCaseAverage =
          db.GetNullableDecimal(reader, 7);
        entities.DashboardStagingPriority12.IwoPerObligCaseRnk =
          db.GetNullableInt32(reader, 8);
        entities.DashboardStagingPriority12.ContractorNumber =
          db.GetNullableString(reader, 9);
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
        entities.DashboardStagingPriority12.CasesUnderOrderNumerator =
          db.GetNullableInt32(reader, 4);
        entities.DashboardStagingPriority12.IwoPerObligCaseNumerator =
          db.GetNullableInt32(reader, 5);
        entities.DashboardStagingPriority12.IwoPerObligCaseDenominator =
          db.GetNullableInt32(reader, 6);
        entities.DashboardStagingPriority12.IwoPerObligCaseAverage =
          db.GetNullableDecimal(reader, 7);
        entities.DashboardStagingPriority12.IwoPerObligCaseRnk =
          db.GetNullableInt32(reader, 8);
        entities.DashboardStagingPriority12.ContractorNumber =
          db.GetNullableString(reader, 9);
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
        entities.DashboardStagingPriority12.CasesUnderOrderNumerator =
          db.GetNullableInt32(reader, 4);
        entities.DashboardStagingPriority12.IwoPerObligCaseNumerator =
          db.GetNullableInt32(reader, 5);
        entities.DashboardStagingPriority12.IwoPerObligCaseDenominator =
          db.GetNullableInt32(reader, 6);
        entities.DashboardStagingPriority12.IwoPerObligCaseAverage =
          db.GetNullableDecimal(reader, 7);
        entities.DashboardStagingPriority12.IwoPerObligCaseRnk =
          db.GetNullableInt32(reader, 8);
        entities.DashboardStagingPriority12.ContractorNumber =
          db.GetNullableString(reader, 9);
        entities.DashboardStagingPriority12.Populated = true;

        return true;
      },
      () =>
      {
        entities.DashboardStagingPriority12.Populated = false;
      });
  }

  private IEnumerable<bool> ReadDashboardStagingPriority8()
  {
    return ReadEach("ReadDashboardStagingPriority8",
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
        entities.DashboardStagingPriority12.IwoPerObligCaseNumerator =
          db.GetNullableInt32(reader, 5);
        entities.DashboardStagingPriority12.IwoPerObligCaseDenominator =
          db.GetNullableInt32(reader, 6);
        entities.DashboardStagingPriority12.IwoPerObligCaseAverage =
          db.GetNullableDecimal(reader, 7);
        entities.DashboardStagingPriority12.IwoPerObligCaseRnk =
          db.GetNullableInt32(reader, 8);
        entities.DashboardStagingPriority12.ContractorNumber =
          db.GetNullableString(reader, 9);
        entities.DashboardStagingPriority12.Populated = true;

        return true;
      },
      () =>
      {
        entities.DashboardStagingPriority12.Populated = false;
      });
  }

  private IEnumerable<bool> ReadLegalActionLegalActionIncomeSource()
  {
    return ReadEachInSeparateTransaction(
      "ReadLegalActionLegalActionIncomeSource",
      (db, command) =>
      {
        db.SetDateTime(command, "timestamp1", import.ReportStartDate.Timestamp);
        db.SetDateTime(command, "timestamp2", import.ReportEndDate.Timestamp);
        db.SetInt32(command, "legalActionId", local.Restart.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.ActionTaken = db.GetString(reader, 1);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 2);
        entities.LegalAction.CreatedTstamp = db.GetDateTime(reader, 3);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 4);
        entities.LegalActionIncomeSource.CspNumber = db.GetString(reader, 5);
        entities.LegalActionIncomeSource.LgaIdentifier = db.GetInt32(reader, 6);
        entities.LegalActionIncomeSource.IsrIdentifier =
          db.GetDateTime(reader, 7);
        entities.LegalActionIncomeSource.Identifier = db.GetInt32(reader, 8);
        entities.LegalAction.Populated = true;
        entities.LegalActionIncomeSource.Populated =
          db.GetNullableString(reader, 5) != null;

        return true;
      },
      () =>
      {
        entities.LegalActionIncomeSource.Populated = false;
        entities.LegalAction.Populated = false;
      });
  }

  private void UpdateDashboardStagingPriority1()
  {
    var asOfDate = local.Statewide.AsOfDate;
    var iwoPerObligCaseNumerator = local.Statewide.IwoPerObligCaseNumerator ?? 0
      ;
    var iwoPerObligCaseDenominator =
      local.Statewide.IwoPerObligCaseDenominator ?? 0;
    var iwoPerObligCaseAverage = local.Statewide.IwoPerObligCaseAverage ?? 0M;
    var iwoPerObligCaseRnk = local.Statewide.IwoPerObligCaseRnk ?? 0;

    entities.DashboardStagingPriority12.Populated = false;
    Update("UpdateDashboardStagingPriority1",
      (db, command) =>
      {
        db.SetNullableDate(command, "asOfDt", asOfDate);
        db.SetNullableInt32(command, "iwoOblCasNum", iwoPerObligCaseNumerator);
        db.
          SetNullableInt32(command, "iwoOblCasDen", iwoPerObligCaseDenominator);
        db.SetNullableDecimal(command, "iwoOblCasAvg", iwoPerObligCaseAverage);
        db.SetNullableInt32(command, "iwoOblCasRnk", iwoPerObligCaseRnk);
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
    entities.DashboardStagingPriority12.IwoPerObligCaseNumerator =
      iwoPerObligCaseNumerator;
    entities.DashboardStagingPriority12.IwoPerObligCaseDenominator =
      iwoPerObligCaseDenominator;
    entities.DashboardStagingPriority12.IwoPerObligCaseAverage =
      iwoPerObligCaseAverage;
    entities.DashboardStagingPriority12.IwoPerObligCaseRnk = iwoPerObligCaseRnk;
    entities.DashboardStagingPriority12.Populated = true;
  }

  private void UpdateDashboardStagingPriority2()
  {
    var asOfDate = local.Local1.Item.G.AsOfDate;
    var iwoPerObligCaseNumerator =
      local.Local1.Item.G.IwoPerObligCaseNumerator ?? 0;
    var iwoPerObligCaseDenominator =
      local.Local1.Item.G.IwoPerObligCaseDenominator ?? 0;
    var iwoPerObligCaseAverage = local.Local1.Item.G.IwoPerObligCaseAverage ?? 0M
      ;
    var iwoPerObligCaseRnk = local.Local1.Item.G.IwoPerObligCaseRnk ?? 0;

    entities.DashboardStagingPriority12.Populated = false;
    Update("UpdateDashboardStagingPriority2",
      (db, command) =>
      {
        db.SetNullableDate(command, "asOfDt", asOfDate);
        db.SetNullableInt32(command, "iwoOblCasNum", iwoPerObligCaseNumerator);
        db.
          SetNullableInt32(command, "iwoOblCasDen", iwoPerObligCaseDenominator);
        db.SetNullableDecimal(command, "iwoOblCasAvg", iwoPerObligCaseAverage);
        db.SetNullableInt32(command, "iwoOblCasRnk", iwoPerObligCaseRnk);
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
    entities.DashboardStagingPriority12.IwoPerObligCaseNumerator =
      iwoPerObligCaseNumerator;
    entities.DashboardStagingPriority12.IwoPerObligCaseDenominator =
      iwoPerObligCaseDenominator;
    entities.DashboardStagingPriority12.IwoPerObligCaseAverage =
      iwoPerObligCaseAverage;
    entities.DashboardStagingPriority12.IwoPerObligCaseRnk = iwoPerObligCaseRnk;
    entities.DashboardStagingPriority12.Populated = true;
  }

  private void UpdateDashboardStagingPriority3()
  {
    var asOfDate = local.Contractor.Item.Gcontractor.AsOfDate;
    var iwoPerObligCaseNumerator =
      local.Contractor.Item.Gcontractor.IwoPerObligCaseNumerator ?? 0;
    var iwoPerObligCaseDenominator =
      local.Contractor.Item.Gcontractor.IwoPerObligCaseDenominator ?? 0;
    var iwoPerObligCaseAverage =
      local.Contractor.Item.Gcontractor.IwoPerObligCaseAverage ?? 0M;
    var iwoPerObligCaseRnk =
      local.Contractor.Item.Gcontractor.IwoPerObligCaseRnk ?? 0;

    entities.DashboardStagingPriority12.Populated = false;
    Update("UpdateDashboardStagingPriority3",
      (db, command) =>
      {
        db.SetNullableDate(command, "asOfDt", asOfDate);
        db.SetNullableInt32(command, "iwoOblCasNum", iwoPerObligCaseNumerator);
        db.
          SetNullableInt32(command, "iwoOblCasDen", iwoPerObligCaseDenominator);
        db.SetNullableDecimal(command, "iwoOblCasAvg", iwoPerObligCaseAverage);
        db.SetNullableInt32(command, "iwoOblCasRnk", iwoPerObligCaseRnk);
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
    entities.DashboardStagingPriority12.IwoPerObligCaseNumerator =
      iwoPerObligCaseNumerator;
    entities.DashboardStagingPriority12.IwoPerObligCaseDenominator =
      iwoPerObligCaseDenominator;
    entities.DashboardStagingPriority12.IwoPerObligCaseAverage =
      iwoPerObligCaseAverage;
    entities.DashboardStagingPriority12.IwoPerObligCaseRnk = iwoPerObligCaseRnk;
    entities.DashboardStagingPriority12.Populated = true;
  }

  private void UpdateDashboardStagingPriority4()
  {
    var iwoPerObligCaseDenominator = local.Temp.IwoPerObligCaseDenominator ?? 0;
    var iwoPerObligCaseAverage = local.Temp.IwoPerObligCaseAverage ?? 0M;

    entities.DashboardStagingPriority12.Populated = false;
    Update("UpdateDashboardStagingPriority4",
      (db, command) =>
      {
        db.
          SetNullableInt32(command, "iwoOblCasDen", iwoPerObligCaseDenominator);
        db.SetNullableDecimal(command, "iwoOblCasAvg", iwoPerObligCaseAverage);
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

    entities.DashboardStagingPriority12.IwoPerObligCaseDenominator =
      iwoPerObligCaseDenominator;
    entities.DashboardStagingPriority12.IwoPerObligCaseAverage =
      iwoPerObligCaseAverage;
    entities.DashboardStagingPriority12.Populated = true;
  }

  private void UpdateDashboardStagingPriority5()
  {
    var iwoPerObligCaseRnk = local.Temp.IwoPerObligCaseRnk ?? 0;

    entities.DashboardStagingPriority12.Populated = false;
    Update("UpdateDashboardStagingPriority5",
      (db, command) =>
      {
        db.SetNullableInt32(command, "iwoOblCasRnk", iwoPerObligCaseRnk);
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

    entities.DashboardStagingPriority12.IwoPerObligCaseRnk = iwoPerObligCaseRnk;
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
    /// A value of ReportStartDate.
    /// </summary>
    public DateWorkArea ReportStartDate
    {
      get => reportStartDate ??= new();
      set => reportStartDate = value;
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

    private DashboardAuditData? dashboardAuditData;
    private ProgramCheckpointRestart? programCheckpointRestart;
    private ProgramProcessingInfo? programProcessingInfo;
    private DateWorkArea? reportStartDate;
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
    /// A value of Prev.
    /// </summary>
    public LegalAction Prev
    {
      get => prev ??= new();
      set => prev = value;
    }

    /// <summary>
    /// A value of Restart.
    /// </summary>
    public LegalAction Restart
    {
      get => restart ??= new();
      set => restart = value;
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

    private LegalAction? prev;
    private LegalAction? restart;
    private DashboardAuditData? initialized;
    private ProgramCheckpointRestart? programCheckpointRestart;
    private DashboardStagingPriority12? statewide;
    private EabFileHandling? eabFileHandling;
    private EabReportSend? eabReportSend;
    private Array<LocalGroup>? local1;
    private Common? recordsReadSinceCommit;
    private DashboardAuditData? dashboardAuditData;
    private DashboardStagingPriority12? temp;
    private Common? common;
    private DashboardStagingPriority12? prevRank;
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
    /// A value of LegalActionIncomeSource.
    /// </summary>
    public LegalActionIncomeSource LegalActionIncomeSource
    {
      get => legalActionIncomeSource ??= new();
      set => legalActionIncomeSource = value;
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
    /// A value of Case1.
    /// </summary>
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of CaseRole.
    /// </summary>
    public CaseRole CaseRole
    {
      get => caseRole ??= new();
      set => caseRole = value;
    }

    /// <summary>
    /// A value of LegalActionCaseRole.
    /// </summary>
    public LegalActionCaseRole LegalActionCaseRole
    {
      get => legalActionCaseRole ??= new();
      set => legalActionCaseRole = value;
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
    /// A value of LegalAction.
    /// </summary>
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    private LegalActionIncomeSource? legalActionIncomeSource;
    private CaseAssignment? caseAssignment;
    private Case1? case1;
    private CaseRole? caseRole;
    private LegalActionCaseRole? legalActionCaseRole;
    private CseOrganization? cseOrganization;
    private DashboardStagingPriority12? dashboardStagingPriority12;
    private LegalAction? legalAction;
  }
#endregion
}
