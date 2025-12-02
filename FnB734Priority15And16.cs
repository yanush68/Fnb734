// Program: FN_B734_PRIORITY_1_5_AND_1_6, ID: 945132075, model: 746.
// Short name: SWE03087
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
/// A program: FN_B734_PRIORITY_1_5_AND_1_6.
/// </para>
/// <para>
/// Priority 1-5: Total Collections Federal Fiscal YTD Over Prior Year
/// and
/// Priority 1-6: Total Collections in Month to Prior Year Same Month
/// </para>
/// </summary>
[Serializable]
[Program("SWE03087")]
public partial class FnB734Priority15And16: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B734_PRIORITY_1_5_AND_1_6 program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB734Priority15And16(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB734Priority15And16.
  /// </summary>
  public FnB734Priority15And16(IContext context, Import import, Export export):
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
    // 07/31/13  GVandy	CQ41079		Correct fiscal year to prior month calculation.
    // ---------------------------------------------------------------------------------------------------
    // -------------------------------------------------------------------------------------
    // Priority 1-5: Total Collections FFYTD over Prior Year
    // Priority 1-6: Total Collections in Month to Prior Year Same Month
    // -------------------------------------------------------------------------------------
    // Priority 1-5: Total Collections federal fiscal YTD Over Prior Year
    // Note: The YTD calculation will not be a new calculation each time the job
    // runs.  The YTD is simply the summation of all prior months totals plus
    // the current report month to date total.
    // Report Level: State, Judicial District
    // Report Period: Month (Fiscal year-to-date calculation)
    // and
    // Priority 1-6: Total Collections in Month to Prior Year Same Month
    // Report Level: State, Judicial District
    // Report Period: Month
    // For priorities 1-5 and 1-6, prior period comparisons will be taken 
    // directly from the end month, end month(YTD) Dashboard numbers reported
    // Statewide and for each JD for the prior year.
    // Example:
    // Current Run Date = 9/10/13
    // Current Report Period is September month (through 9/10/13) and YTD (
    // through 9/10/13).
    // Prior Year Report Period is End Month September 2012 and YTD through 
    // September 2012.
    // The dashboard will display a disclaimer that total collections include 
    // Fees, 718Bs, Incoming Interstate and MJs (AF, AFI, FC and FCI).
    // 	1) Note:  These same rules are used in Priority 1.9, 1.12, 3.13.
    // 	   Collections created (distributed) during report period.  Applied to
    // 	   current support, voluntaries, gift or arrears.
    // 	2) Bypass concurrent (primary/secondary - count only primary).
    // 	   Joint/several - count the collection only once.
    // 	3) Bypass FcrtRec and FDIR (REIP) cash receipt types.
    // 	4) Bypass adjusted collections where collection adjusted in report 
    // period.
    // 	5) Bypass Recoveries.
    // 	6) Count negative collection where adjustment occurred in report period 
    // to a
    // 	   collection created in a prior report period.
    // 	7) Exclude CSENet collection types.
    // 	8) Count for persons with both active and inactive case roles.
    // 	9) Include all collections that have applied to Incoming Interstate 
    // arrears
    // 	   (NAI, AFI, FCI arrears).
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

    // ------------------------------------------------------------------------------
    // -- Determine if we're restarting and set appropriate restart information.
    // ------------------------------------------------------------------------------
    if (AsChar(import.ProgramCheckpointRestart.RestartInd) == 'Y' && Equal
      (import.ProgramCheckpointRestart.RestartInfo, 81, 8, "1-05    "))
    {
      // -- Checkpoint Info
      // Positions   Value
      // ---------   
      // ------------------------------------
      //  001-080    General Checkpoint Info for PRAD
      //  081-088    Dashboard Priority
      //  089-089    Blank
      //  090-098    Collection System Generated Identifier
      if (!IsEmpty(Substring(import.ProgramCheckpointRestart.RestartInfo, 90, 9)))
      {
        local.Restart.SystemGeneratedIdentifier =
          (int)StringToNumber(Substring(
            import.ProgramCheckpointRestart.RestartInfo, 250, 90, 9));
      }

      if (local.Restart.SystemGeneratedIdentifier > 0)
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
          local.Statewide.CollectionsFfytdActual = 0;
          local.Statewide.CollectionsFfytdPercentChange = 0;
          local.Statewide.CollectionsFfytdPriorYear = 0;
          local.Statewide.CollectionsFfytdRnk = 0;
          local.Statewide.CollectionsFfytdToPriorMonth = 0;
          local.Statewide.CollectionsInMonthActual = 0;
          local.Statewide.CollectionsInMonthPercentChg = 0;
          local.Statewide.CollectionsInMonthPriorYear = 0;
          local.Statewide.CollectionsInMonthRnk = 0;
        }

        // -- Load Judicial District counts.
        foreach(var _ in ReadDashboardStagingPriority2())
        {
          local.Local1.Index =
            (int)StringToNumber(entities.DashboardStagingPriority12.
              ReportLevelId) - 1;
          local.Local1.CheckSize();

          local.Local1.Update.G.Assign(entities.DashboardStagingPriority12);
          local.Local1.Update.G.CollectionsFfytdActual = 0;
          local.Local1.Update.G.CollectionsFfytdPercentChange = 0;
          local.Local1.Update.G.CollectionsFfytdPriorYear = 0;
          local.Local1.Update.G.CollectionsFfytdRnk = 0;
          local.Local1.Update.G.CollectionsFfytdToPriorMonth = 0;
          local.Local1.Update.G.CollectionsInMonthActual = 0;
          local.Local1.Update.G.CollectionsInMonthPercentChg = 0;
          local.Local1.Update.G.CollectionsInMonthPriorYear = 0;
          local.Local1.Update.G.CollectionsInMonthRnk = 0;
        }
      }
    }
    else
    {
      local.Restart.SystemGeneratedIdentifier = 0;
    }

    // -------------------------------------------------------------------
    // Read Each is sorted in Asc order of Supp Person #.
    // -------------------------------------------------------------------
    foreach(var _ in ReadCollectionObligationTypeCsePersonCollectionType())
    {
      if (entities.CollectionType.Populated)
      {
        // -- Skip CSENet collections.
        if (entities.CollectionType.SequentialIdentifier == 27 || entities
          .CollectionType.SequentialIdentifier == 28 || entities
          .CollectionType.SequentialIdentifier == 29)
        {
          continue;
        }
      }

      if (entities.Collection.SystemGeneratedIdentifier == local
        .Prev.SystemGeneratedIdentifier)
      {
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
          //  090-098    Collection System Generated Identifier
          local.ProgramCheckpointRestart.RestartInd = "Y";
          local.ProgramCheckpointRestart.RestartInfo =
            Substring(import.ProgramCheckpointRestart.RestartInfo, 250, 1, 80) +
            "1-05    " + " " + NumberToString
            (local.Prev.SystemGeneratedIdentifier, 7, 9);
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

      local.Prev.SystemGeneratedIdentifier =
        entities.Collection.SystemGeneratedIdentifier;
      ++local.RecordsReadSinceCommit.Count;
      MoveDashboardAuditData1(local.Initialized, local.DashboardAuditData);

      // -------------------------------------------------------------------------------------
      // -- Include collection in the in month amount.
      // -------------------------------------------------------------------------------------
      if (Lt(entities.Collection.CreatedTmst, import.ReportStartDate.Timestamp))
      {
        // -----------------------------------------------------------------
        // This must be an adjustment to a collection created in prev report 
        // period.
        // -----------------------------------------------------------------
        local.DashboardAuditData.CollectionAmount = -entities.Collection.Amount;
      }
      else
      {
        local.DashboardAuditData.CollectionAmount = entities.Collection.Amount;
      }

      // -- Increment Statewide Level
      local.Statewide.CollectionsInMonthActual =
        (local.Statewide.CollectionsInMonthActual ?? 0M) + (
          local.DashboardAuditData.CollectionAmount ?? 0M);

      // -- Determine Judicial District...
      if (AsChar(entities.ObligationType.Classification) == 'F')
      {
        if (!ReadLegalActionDetail())
        {
          goto Test;
        }

        if (ReadFipsTribunal())
        {
          if (entities.Fips.State == 20)
          {
            // -- For Fees on Kansas Orders use the county of order to determine
            // Judicial District.
            UseFnB734DetermineJdFromOrder();

            goto Test;
          }
        }

        // -- For Fees on non Kansas orders, use the case entered on LOPS for 
        // the Obligor to determine the Judicial District.
        if (!ReadCase())
        {
          goto Test;
        }

        if (ReadCaseAssignment())
        {
          if (Lt(entities.CaseAssignment.DiscontinueDate,
            import.ReportEndDate.Date))
          {
            local.TempDateWorkArea.Date =
              entities.CaseAssignment.DiscontinueDate;
          }
          else
          {
            local.TempDateWorkArea.Date = import.ReportEndDate.Date;
          }
        }

        UseFnB734DetermineJdFromCase();
      }
      else
      {
        // -- For non Fees, use the order to determine Judicial District.
        UseFnB734DetermineJdFromOrder();
      }

Test:

      // -- Increment Judicial District Level
      if (!IsEmpty(local.DashboardAuditData.JudicialDistrict))
      {
        local.Local1.Index =
          (int)StringToNumber(local.DashboardAuditData.JudicialDistrict) - 1;
        local.Local1.CheckSize();

        // -- Increment In-Month Judicial District Level
        local.Local1.Update.G.CollectionsInMonthActual =
          (local.Local1.Item.G.CollectionsInMonthActual ?? 0M) + (
            local.DashboardAuditData.CollectionAmount ?? 0M);
      }

      // -- Log to the audit table.
      local.DashboardAuditData.DashboardPriority = "1-5" + String
        (local.ReportingAbbreviation.Text2, TextWorkArea.Text2_MaxLength);
      local.DashboardAuditData.CollectionCreatedDate =
        Date(entities.Collection.CreatedTmst);
      local.DashboardAuditData.CollAppliedToCd =
        entities.Collection.AppliedToCode;

      if (AsChar(entities.ObligationType.Classification) != 'F')
      {
        if (ReadCsePerson())
        {
          local.DashboardAuditData.SuppCspNumber = entities.Supp.Number;
        }
      }

      local.DashboardAuditData.PayorCspNumber = entities.Ap.Number;

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
          local.Contractor.Update.Gcontractor.CollectionsInMonthActual =
            (local.Contractor.Item.Gcontractor.CollectionsInMonthActual ?? 0M) +
            (local.Local1.Item.G.CollectionsInMonthActual ?? 0M);
          local.Contractor.Update.Gcontractor.CollectionsInMonthPriorYear =
            (local.Contractor.Item.Gcontractor.CollectionsInMonthPriorYear ?? 0M)
            + (local.Local1.Item.G.CollectionsInMonthPriorYear ?? 0M);

          goto Next;
        }
      }

      local.Contractor.CheckIndex();

Next:
      ;
    }

    local.Local1.CheckIndex();

    foreach(var _ in ReadDashboardStagingPriority5())
    {
      if (Equal(entities.DashboardStagingPriority12.ReportLevel, "XJ"))
      {
        continue;
      }

      MoveDashboardStagingPriority1(entities.DashboardStagingPriority12,
        local.OtherDashboardStagingPriority12);

      // ------------------------------------------------------------------------------
      // -- Determine the amount from start of fiscal year through the previous 
      // month.
      // ------------------------------------------------------------------------------
      // -- Determine the prior month.
      local.OtherDateWorkArea.Date =
        AddMonths(IntToDate(import.DashboardAuditData.ReportMonth * 100 + 1), -1);

      if (Lt(local.OtherDateWorkArea.Date, import.FiscalYearStartDate.Date))
      {
        // 07/31/13 GVandy CQ41079  Correct fiscal year to prior month 
        // calculation.
        // -- The prior month is in the previous fiscal year.
        // -- Set the collections_fy_to_prior_month to zero.
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
        local.OtherDashboardStagingPriority12.ReportMonth =
          Year(local.OtherDateWorkArea.Date) * 100 + Month
          (local.OtherDateWorkArea.Date);

        // -- Read the prior month staging record.
        if (ReadDashboardStagingPriority6())
        {
          // -- Set the collections_fy_to_prior_month to the FYTD_actual amount 
          // from the prior month.
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
        else
        {
          // -- Set the collections_fy_to_prior_month to zero.
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
      }

      // ------------------------------------------------------------------------------
      // -- Determine the amount from fiscal year to date in the prior year and
      // -- in month in prior year.
      // ------------------------------------------------------------------------------
      // -- Determine the prior year.
      local.OtherDateWorkArea.Date =
        AddYears(IntToDate(import.DashboardAuditData.ReportMonth * 100 + 1), -1);
      local.OtherDashboardStagingPriority12.ReportMonth =
        Year(local.OtherDateWorkArea.Date) * 100 + Month
        (local.OtherDateWorkArea.Date);

      // -- Read the prior year staging record.
      if (ReadDashboardStagingPriority6())
      {
        // -- Set the collections_fytd_prior_year to the FYTD_actual amount from
        // the prior year.
        // -- Set the collection_in_month_prior_year to the in_month_actual 
        // amount from the prior year.
        try
        {
          UpdateDashboardStagingPriority5();
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
        // -- Set the collections_fytd_prior_year to zero.
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

      // -- Set the collections_fytd to the FYTD_to_prior_month + 
      // in_month_actual amount.
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

    // ------------------------------------------------------------------------------
    // -- Rollup current year Judicial District counts to the Contractor Level.
    // ------------------------------------------------------------------------------
    foreach(var _ in ReadDashboardStagingPriority2())
    {
      local.DashboardAuditData.JudicialDistrict =
        entities.DashboardStagingPriority12.ReportLevelId;
      UseFnB734DeterContractorFromJd();

      // Here the program will add all the current jd amounts into the correct 
      // contractor
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
          local.Contractor.Update.Gcontractor.CollectionsFfytdPriorYear =
            (local.Contractor.Item.Gcontractor.CollectionsFfytdPriorYear ?? 0M) +
            (
              entities.DashboardStagingPriority12.CollectionsFfytdPriorYear ?? 0M
            );
          local.Contractor.Update.Gcontractor.CollectionsFfytdToPriorMonth =
            (local.Contractor.Item.Gcontractor.CollectionsFfytdToPriorMonth ?? 0M
            ) + (
              entities.DashboardStagingPriority12.
              CollectionsFfytdToPriorMonth ?? 0M);
          local.Contractor.Update.Gcontractor.CollectionsInMonthPriorYear =
            (local.Contractor.Item.Gcontractor.CollectionsInMonthPriorYear ?? 0M)
            + (
              entities.DashboardStagingPriority12.
              CollectionsInMonthPriorYear ?? 0M);
          local.Contractor.Update.Gcontractor.CollectionsFfytdActual =
            (local.Contractor.Item.Gcontractor.CollectionsFfytdActual ?? 0M) + (
              entities.DashboardStagingPriority12.CollectionsFfytdActual ?? 0M
            );

          goto ReadEach2;
        }
      }

      local.Contractor.CheckIndex();

ReadEach2:
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

      if (ReadDashboardStagingPriority7())
      {
        try
        {
          UpdateDashboardStagingPriority8();
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
    // -- Calculate the Statewide, Judicial District and Contractor Percent 
    // Change.
    // ------------------------------------------------------------------------------
    foreach(var _ in ReadDashboardStagingPriority5())
    {
      local.TempDashboardStagingPriority12.Assign(
        entities.DashboardStagingPriority12);

      if ((local.TempDashboardStagingPriority12.CollectionsInMonthPriorYear ?? 0M
        ) == 0)
      {
        local.TempDashboardStagingPriority12.CollectionsInMonthPercentChg = 0;
      }
      else
      {
        local.TempDashboardStagingPriority12.CollectionsInMonthPercentChg =
          Math.Round((
            local.TempDashboardStagingPriority12.CollectionsInMonthActual ?? 0M
          ) / (
            local.TempDashboardStagingPriority12.
            CollectionsInMonthPriorYear ?? 0M
          ) - 1, 3, MidpointRounding.AwayFromZero);
      }

      if ((local.TempDashboardStagingPriority12.CollectionsFfytdPriorYear ?? 0M) ==
        0)
      {
        local.TempDashboardStagingPriority12.CollectionsFfytdPercentChange = 0;
      }
      else
      {
        local.TempDashboardStagingPriority12.CollectionsFfytdPercentChange =
          Math.Round((
            local.TempDashboardStagingPriority12.CollectionsFfytdActual ?? 0M
          ) / (
            local.TempDashboardStagingPriority12.CollectionsFfytdPriorYear ?? 0M
          ) - 1, 3, MidpointRounding.AwayFromZero);
      }

      try
      {
        UpdateDashboardStagingPriority9();
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
    local.PrevRank.CollectionsInMonthPercentChg = 0;
    local.TempDashboardStagingPriority12.CollectionsInMonthRnk = 1;

    // ------------------------------------------------------------------------------
    // -- Calculate Judicial District Ranking (in month).
    // ------------------------------------------------------------------------------
    foreach(var _ in ReadDashboardStagingPriority8())
    {
      ++local.Common.Count;

      if ((entities.DashboardStagingPriority12.CollectionsInMonthPercentChg ?? 0M
        ) == (local.PrevRank.CollectionsInMonthPercentChg ?? 0M))
      {
        // -- The ranking for this judicial district is tied with the previous 
        // judicial district.
        // -- This JD gets the same ranking already in the local_temp 
        // case_under_order_rank.
      }
      else
      {
        local.TempDashboardStagingPriority12.CollectionsInMonthRnk =
          local.Common.Count;
      }

      try
      {
        UpdateDashboardStagingPriority10();
        MoveDashboardStagingPriority2(entities.DashboardStagingPriority12,
          local.PrevRank);
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
    local.PrevRank.CollectionsInMonthPercentChg = 0;
    local.TempDashboardStagingPriority12.CollectionsInMonthRnk = 1;

    // ------------------------------------------------------------------------------
    // -- Calculate Contractor Ranking (in month).
    // ------------------------------------------------------------------------------
    foreach(var _ in ReadDashboardStagingPriority9())
    {
      ++local.Common.Count;

      if ((entities.DashboardStagingPriority12.CollectionsInMonthPercentChg ?? 0M
        ) == (local.PrevRank.CollectionsInMonthPercentChg ?? 0M))
      {
        // -- The ranking for this judicial district is tied with the previous 
        // judicial district.
        // -- This JD gets the same ranking already in the local_temp 
        // case_under_order_rank.
      }
      else
      {
        local.TempDashboardStagingPriority12.CollectionsInMonthRnk =
          local.Common.Count;
      }

      try
      {
        UpdateDashboardStagingPriority10();
        MoveDashboardStagingPriority2(entities.DashboardStagingPriority12,
          local.PrevRank);
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
    local.PrevRank.CollectionsFfytdPercentChange = 0;
    local.TempDashboardStagingPriority12.CollectionsFfytdRnk = 1;

    // ------------------------------------------------------------------------------
    // -- Calculate Judicial District Ranking (Fiscal Year To Date).
    // ------------------------------------------------------------------------------
    foreach(var _ in ReadDashboardStagingPriority10())
    {
      ++local.Common.Count;

      if ((entities.DashboardStagingPriority12.
        CollectionsFfytdPercentChange ?? 0M) == (
          local.PrevRank.CollectionsFfytdPercentChange ?? 0M))
      {
        // -- The ranking for this judicial district is tied with the previous 
        // judicial district.
        // -- This JD gets the same ranking already in the local_temp 
        // case_under_order_rank.
      }
      else
      {
        local.TempDashboardStagingPriority12.CollectionsFfytdRnk =
          local.Common.Count;
      }

      try
      {
        UpdateDashboardStagingPriority11();
        MoveDashboardStagingPriority2(entities.DashboardStagingPriority12,
          local.PrevRank);
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
    local.PrevRank.CollectionsFfytdPercentChange = 0;
    local.TempDashboardStagingPriority12.CollectionsFfytdRnk = 1;

    // ------------------------------------------------------------------------------
    // -- Calculate Contractor Ranking (Fiscal Year To Date).
    // ------------------------------------------------------------------------------
    foreach(var _ in ReadDashboardStagingPriority11())
    {
      ++local.Common.Count;

      if ((entities.DashboardStagingPriority12.
        CollectionsFfytdPercentChange ?? 0M) == (
          local.PrevRank.CollectionsFfytdPercentChange ?? 0M))
      {
        // -- The ranking for this judicial district is tied with the previous 
        // judicial district.
        // -- This JD gets the same ranking already in the local_temp 
        // case_under_order_rank.
      }
      else
      {
        local.TempDashboardStagingPriority12.CollectionsFfytdRnk =
          local.Common.Count;
      }

      try
      {
        UpdateDashboardStagingPriority11();
        MoveDashboardStagingPriority2(entities.DashboardStagingPriority12,
          local.PrevRank);
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
      Substring(import.ProgramCheckpointRestart.RestartInfo, 250, 1, 80) + "1-07    ";
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
  }

  private static void MoveDashboardAuditData4(DashboardAuditData source,
    DashboardAuditData target)
  {
    target.Office = source.Office;
    target.JudicialDistrict = source.JudicialDistrict;
    target.CaseNumber = source.CaseNumber;
    target.StandardNumber = source.StandardNumber;
  }

  private static void MoveDashboardStagingPriority1(
    DashboardStagingPriority12 source, DashboardStagingPriority12 target)
  {
    target.ReportMonth = source.ReportMonth;
    target.ReportLevel = source.ReportLevel;
    target.ReportLevelId = source.ReportLevelId;
  }

  private static void MoveDashboardStagingPriority2(
    DashboardStagingPriority12 source, DashboardStagingPriority12 target)
  {
    target.CollectionsFfytdPercentChange = source.CollectionsFfytdPercentChange;
    target.CollectionsInMonthPercentChg = source.CollectionsInMonthPercentChg;
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

  private void UseFnB734DetermineJdFromCase()
  {
    var useImport = new FnB734DetermineJdFromCase.Import();
    var useExport = new FnB734DetermineJdFromCase.Export();

    useImport.Case1.Number = entities.Case1.Number;
    useImport.ReportEndDate.Date = local.TempDateWorkArea.Date;

    context.Call(FnB734DetermineJdFromCase.Execute, useImport, useExport);

    MoveDashboardAuditData3(useExport.DashboardAuditData,
      local.DashboardAuditData);
  }

  private void UseFnB734DetermineJdFromOrder()
  {
    var useImport = new FnB734DetermineJdFromOrder.Import();
    var useExport = new FnB734DetermineJdFromOrder.Export();

    useImport.PersistentCollection.Assign(entities.Collection);
    useImport.ReportStartDate.Date = import.ReportStartDate.Date;
    useImport.ReportEndDate.Date = import.ReportEndDate.Date;

    context.Call(FnB734DetermineJdFromOrder.Execute, useImport, useExport);

    MoveDashboardAuditData4(useExport.DashboardAuditData,
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
    var collectionsFfytdToPriorMonth =
      local.Statewide.CollectionsFfytdToPriorMonth ?? 0M;
    var collectionsFfytdActual = local.Statewide.CollectionsFfytdActual ?? 0M;
    var collectionsFfytdPriorYear =
      local.Statewide.CollectionsFfytdPriorYear ?? 0M;
    var collectionsFfytdPercentChange =
      local.Statewide.CollectionsFfytdPercentChange ?? 0M;
    var collectionsFfytdRnk = local.Statewide.CollectionsFfytdRnk ?? 0;
    var collectionsInMonthActual = local.Statewide.CollectionsInMonthActual ?? 0M
      ;
    var collectionsInMonthPriorYear =
      local.Statewide.CollectionsInMonthPriorYear ?? 0M;
    var collectionsInMonthPercentChg =
      local.Statewide.CollectionsInMonthPercentChg ?? 0M;
    var collectionsInMonthRnk = local.Statewide.CollectionsInMonthRnk ?? 0;

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
        db.SetNullableDecimal(
          command, "collYtdToPriMo", collectionsFfytdToPriorMonth);
        db.SetNullableDecimal(command, "collYtdAct", collectionsFfytdActual);
        db.
          SetNullableDecimal(command, "collYtdPriYr", collectionsFfytdPriorYear);
        db.SetNullableDecimal(
          command, "collYtdPerChg", collectionsFfytdPercentChange);
        db.SetNullableInt32(command, "collYtdRnk", collectionsFfytdRnk);
        db.
          SetNullableDecimal(command, "collInMthAct", collectionsInMonthActual);
        db.SetNullableDecimal(
          command, "collInMthPriYr", collectionsInMonthPriorYear);
        db.SetNullableDecimal(
          command, "collInMthPerCh", collectionsInMonthPercentChg);
        db.SetNullableInt32(command, "collInMthRnk", collectionsInMonthRnk);
        db.SetNullableDecimal(command, "casPerFteDen", param);
        db.SetNullableString(command, "contractorNum", "");
      });

    entities.DashboardStagingPriority12.ReportMonth = reportMonth;
    entities.DashboardStagingPriority12.ReportLevel = reportLevel;
    entities.DashboardStagingPriority12.ReportLevelId = reportLevelId;
    entities.DashboardStagingPriority12.AsOfDate = asOfDate;
    entities.DashboardStagingPriority12.CollectionsFfytdToPriorMonth =
      collectionsFfytdToPriorMonth;
    entities.DashboardStagingPriority12.CollectionsFfytdActual =
      collectionsFfytdActual;
    entities.DashboardStagingPriority12.CollectionsFfytdPriorYear =
      collectionsFfytdPriorYear;
    entities.DashboardStagingPriority12.CollectionsFfytdPercentChange =
      collectionsFfytdPercentChange;
    entities.DashboardStagingPriority12.CollectionsFfytdRnk =
      collectionsFfytdRnk;
    entities.DashboardStagingPriority12.CollectionsInMonthActual =
      collectionsInMonthActual;
    entities.DashboardStagingPriority12.CollectionsInMonthPriorYear =
      collectionsInMonthPriorYear;
    entities.DashboardStagingPriority12.CollectionsInMonthPercentChg =
      collectionsInMonthPercentChg;
    entities.DashboardStagingPriority12.CollectionsInMonthRnk =
      collectionsInMonthRnk;
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
    var collectionsFfytdToPriorMonth =
      local.Local1.Item.G.CollectionsFfytdToPriorMonth ?? 0M;
    var collectionsFfytdActual = local.Local1.Item.G.CollectionsFfytdActual ?? 0M
      ;
    var collectionsFfytdPriorYear =
      local.Local1.Item.G.CollectionsFfytdPriorYear ?? 0M;
    var collectionsFfytdPercentChange =
      local.Local1.Item.G.CollectionsFfytdPercentChange ?? 0M;
    var collectionsFfytdRnk = local.Local1.Item.G.CollectionsFfytdRnk ?? 0;
    var collectionsInMonthActual =
      local.Local1.Item.G.CollectionsInMonthActual ?? 0M;
    var collectionsInMonthPriorYear =
      local.Local1.Item.G.CollectionsInMonthPriorYear ?? 0M;
    var collectionsInMonthPercentChg =
      local.Local1.Item.G.CollectionsInMonthPercentChg ?? 0M;
    var collectionsInMonthRnk = local.Local1.Item.G.CollectionsInMonthRnk ?? 0;

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
        db.SetNullableDecimal(
          command, "collYtdToPriMo", collectionsFfytdToPriorMonth);
        db.SetNullableDecimal(command, "collYtdAct", collectionsFfytdActual);
        db.
          SetNullableDecimal(command, "collYtdPriYr", collectionsFfytdPriorYear);
        db.SetNullableDecimal(
          command, "collYtdPerChg", collectionsFfytdPercentChange);
        db.SetNullableInt32(command, "collYtdRnk", collectionsFfytdRnk);
        db.
          SetNullableDecimal(command, "collInMthAct", collectionsInMonthActual);
        db.SetNullableDecimal(
          command, "collInMthPriYr", collectionsInMonthPriorYear);
        db.SetNullableDecimal(
          command, "collInMthPerCh", collectionsInMonthPercentChg);
        db.SetNullableInt32(command, "collInMthRnk", collectionsInMonthRnk);
        db.SetNullableDecimal(command, "casPerFteDen", param);
        db.SetNullableString(command, "contractorNum", "");
      });

    entities.DashboardStagingPriority12.ReportMonth = reportMonth;
    entities.DashboardStagingPriority12.ReportLevel = reportLevel;
    entities.DashboardStagingPriority12.ReportLevelId = reportLevelId;
    entities.DashboardStagingPriority12.AsOfDate = asOfDate;
    entities.DashboardStagingPriority12.CollectionsFfytdToPriorMonth =
      collectionsFfytdToPriorMonth;
    entities.DashboardStagingPriority12.CollectionsFfytdActual =
      collectionsFfytdActual;
    entities.DashboardStagingPriority12.CollectionsFfytdPriorYear =
      collectionsFfytdPriorYear;
    entities.DashboardStagingPriority12.CollectionsFfytdPercentChange =
      collectionsFfytdPercentChange;
    entities.DashboardStagingPriority12.CollectionsFfytdRnk =
      collectionsFfytdRnk;
    entities.DashboardStagingPriority12.CollectionsInMonthActual =
      collectionsInMonthActual;
    entities.DashboardStagingPriority12.CollectionsInMonthPriorYear =
      collectionsInMonthPriorYear;
    entities.DashboardStagingPriority12.CollectionsInMonthPercentChg =
      collectionsInMonthPercentChg;
    entities.DashboardStagingPriority12.CollectionsInMonthRnk =
      collectionsInMonthRnk;
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
    var collectionsFfytdToPriorMonth =
      local.Contractor.Item.Gcontractor.CollectionsFfytdToPriorMonth ?? 0M;
    var collectionsFfytdActual =
      local.Contractor.Item.Gcontractor.CollectionsFfytdActual ?? 0M;
    var collectionsFfytdPriorYear =
      local.Contractor.Item.Gcontractor.CollectionsFfytdPriorYear ?? 0M;
    var collectionsFfytdPercentChange =
      local.Contractor.Item.Gcontractor.CollectionsFfytdPercentChange ?? 0M;
    var collectionsFfytdRnk =
      local.Contractor.Item.Gcontractor.CollectionsFfytdRnk ?? 0;
    var collectionsInMonthActual =
      local.Contractor.Item.Gcontractor.CollectionsInMonthActual ?? 0M;
    var collectionsInMonthPriorYear =
      local.Contractor.Item.Gcontractor.CollectionsInMonthPriorYear ?? 0M;
    var collectionsInMonthPercentChg =
      local.Contractor.Item.Gcontractor.CollectionsInMonthPercentChg ?? 0M;
    var collectionsInMonthRnk =
      local.Contractor.Item.Gcontractor.CollectionsInMonthRnk ?? 0;
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
        db.SetNullableDecimal(
          command, "collYtdToPriMo", collectionsFfytdToPriorMonth);
        db.SetNullableDecimal(command, "collYtdAct", collectionsFfytdActual);
        db.
          SetNullableDecimal(command, "collYtdPriYr", collectionsFfytdPriorYear);
        db.SetNullableDecimal(
          command, "collYtdPerChg", collectionsFfytdPercentChange);
        db.SetNullableInt32(command, "collYtdRnk", collectionsFfytdRnk);
        db.
          SetNullableDecimal(command, "collInMthAct", collectionsInMonthActual);
        db.SetNullableDecimal(
          command, "collInMthPriYr", collectionsInMonthPriorYear);
        db.SetNullableDecimal(
          command, "collInMthPerCh", collectionsInMonthPercentChg);
        db.SetNullableInt32(command, "collInMthRnk", collectionsInMonthRnk);
        db.SetNullableDecimal(command, "casPerFteDen", param);
        db.SetNullableString(command, "contractorNum", contractorNumber);
      });

    entities.DashboardStagingPriority12.ReportMonth = reportMonth;
    entities.DashboardStagingPriority12.ReportLevel = reportLevel;
    entities.DashboardStagingPriority12.ReportLevelId = reportLevelId;
    entities.DashboardStagingPriority12.AsOfDate = asOfDate;
    entities.DashboardStagingPriority12.CollectionsFfytdToPriorMonth =
      collectionsFfytdToPriorMonth;
    entities.DashboardStagingPriority12.CollectionsFfytdActual =
      collectionsFfytdActual;
    entities.DashboardStagingPriority12.CollectionsFfytdPriorYear =
      collectionsFfytdPriorYear;
    entities.DashboardStagingPriority12.CollectionsFfytdPercentChange =
      collectionsFfytdPercentChange;
    entities.DashboardStagingPriority12.CollectionsFfytdRnk =
      collectionsFfytdRnk;
    entities.DashboardStagingPriority12.CollectionsInMonthActual =
      collectionsInMonthActual;
    entities.DashboardStagingPriority12.CollectionsInMonthPriorYear =
      collectionsInMonthPriorYear;
    entities.DashboardStagingPriority12.CollectionsInMonthPercentChg =
      collectionsInMonthPercentChg;
    entities.DashboardStagingPriority12.CollectionsInMonthRnk =
      collectionsInMonthRnk;
    entities.DashboardStagingPriority12.ContractorNumber = contractorNumber;
    entities.DashboardStagingPriority12.Populated = true;
  }

  private bool ReadCase()
  {
    System.Diagnostics.Debug.Assert(entities.LegalActionDetail.Populated);
    entities.Case1.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "ladRNumber", entities.LegalActionDetail.Number);
        db.SetNullableInt32(
          command, "lgaRIdentifier", entities.LegalActionDetail.LgaIdentifier);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCaseAssignment()
  {
    entities.CaseAssignment.Populated = false;

    return Read("ReadCaseAssignment",
      (db, command) =>
      {
        db.SetString(command, "casNo", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.CaseAssignment.ReasonCode = db.GetString(reader, 0);
        entities.CaseAssignment.EffectiveDate = db.GetDate(reader, 1);
        entities.CaseAssignment.DiscontinueDate = db.GetNullableDate(reader, 2);
        entities.CaseAssignment.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.CaseAssignment.SpdId = db.GetInt32(reader, 4);
        entities.CaseAssignment.OffId = db.GetInt32(reader, 5);
        entities.CaseAssignment.OspCode = db.GetString(reader, 6);
        entities.CaseAssignment.OspDate = db.GetDate(reader, 7);
        entities.CaseAssignment.CasNo = db.GetString(reader, 8);
        entities.CaseAssignment.Populated = true;
      });
  }

  private IEnumerable<bool>
    ReadCollectionObligationTypeCsePersonCollectionType()
  {
    return ReadEachInSeparateTransaction(
      "ReadCollectionObligationTypeCsePersonCollectionType",
      (db, command) =>
      {
        db.
          SetDateTime(command, "createdTmst1", import.ReportStartDate.Timestamp);
        db.SetDateTime(command, "createdTmst2", import.ReportEndDate.Timestamp);
        db.SetDate(command, "collAdjDt", import.ReportEndDate.Date);
        db.SetDate(command, "date", import.ReportStartDate.Date);
        db.SetInt32(command, "collId", local.Restart.SystemGeneratedIdentifier);
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
        entities.Ap.Number = db.GetString(reader, 10);
        entities.Collection.CpaType = db.GetString(reader, 11);
        entities.Collection.OtrId = db.GetInt32(reader, 12);
        entities.Collection.OtrType = db.GetString(reader, 13);
        entities.Collection.OtyId = db.GetInt32(reader, 14);
        entities.Collection.CollectionAdjustmentDt = db.GetDate(reader, 15);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 16);
        entities.Collection.Amount = db.GetDecimal(reader, 17);
        entities.Collection.CourtOrderAppliedTo =
          db.GetNullableString(reader, 18);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 19);
        entities.ObligationType.Code = db.GetString(reader, 20);
        entities.ObligationType.Classification = db.GetString(reader, 21);
        entities.CollectionType.SequentialIdentifier = db.GetInt32(reader, 22);
        entities.Collection.Populated = true;
        entities.ObligationType.Populated = true;
        entities.Ap.Populated = true;
        entities.CollectionType.Populated = db.GetNullableInt32(reader, 22) != null
          ;
        CheckValid<Collection>("AppliedToCode",
          entities.Collection.AppliedToCode);
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("ConcurrentInd",
          entities.Collection.ConcurrentInd);
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);

        return true;
      },
      () =>
      {
        entities.CollectionType.Populated = false;
        entities.Collection.Populated = false;
        entities.Ap.Populated = false;
        entities.ObligationType.Populated = false;
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

  private bool ReadCsePerson()
  {
    System.Diagnostics.Debug.Assert(entities.Collection.Populated);
    entities.Supp.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.Collection.OtyId);
        db.SetString(command, "obTrnTyp", entities.Collection.OtrType);
        db.SetInt32(command, "obTrnId", entities.Collection.OtrId);
        db.SetString(command, "cpaType", entities.Collection.CpaType);
        db.SetString(command, "cspNumber", entities.Collection.CspNumber);
        db.SetInt32(command, "obgGeneratedId", entities.Collection.ObgId);
      },
      (db, reader) =>
      {
        entities.Supp.Number = db.GetString(reader, 0);
        entities.Supp.Populated = true;
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
        entities.DashboardStagingPriority12.CollectionsFfytdToPriorMonth =
          db.GetNullableDecimal(reader, 4);
        entities.DashboardStagingPriority12.CollectionsFfytdActual =
          db.GetNullableDecimal(reader, 5);
        entities.DashboardStagingPriority12.CollectionsFfytdPriorYear =
          db.GetNullableDecimal(reader, 6);
        entities.DashboardStagingPriority12.CollectionsFfytdPercentChange =
          db.GetNullableDecimal(reader, 7);
        entities.DashboardStagingPriority12.CollectionsFfytdRnk =
          db.GetNullableInt32(reader, 8);
        entities.DashboardStagingPriority12.CollectionsInMonthActual =
          db.GetNullableDecimal(reader, 9);
        entities.DashboardStagingPriority12.CollectionsInMonthPriorYear =
          db.GetNullableDecimal(reader, 10);
        entities.DashboardStagingPriority12.CollectionsInMonthPercentChg =
          db.GetNullableDecimal(reader, 11);
        entities.DashboardStagingPriority12.CollectionsInMonthRnk =
          db.GetNullableInt32(reader, 12);
        entities.DashboardStagingPriority12.ContractorNumber =
          db.GetNullableString(reader, 13);
        entities.DashboardStagingPriority12.Populated = true;

        return true;
      },
      () =>
      {
        entities.DashboardStagingPriority12.Populated = false;
      });
  }

  private IEnumerable<bool> ReadDashboardStagingPriority10()
  {
    return ReadEach("ReadDashboardStagingPriority10",
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
        entities.DashboardStagingPriority12.CollectionsFfytdToPriorMonth =
          db.GetNullableDecimal(reader, 4);
        entities.DashboardStagingPriority12.CollectionsFfytdActual =
          db.GetNullableDecimal(reader, 5);
        entities.DashboardStagingPriority12.CollectionsFfytdPriorYear =
          db.GetNullableDecimal(reader, 6);
        entities.DashboardStagingPriority12.CollectionsFfytdPercentChange =
          db.GetNullableDecimal(reader, 7);
        entities.DashboardStagingPriority12.CollectionsFfytdRnk =
          db.GetNullableInt32(reader, 8);
        entities.DashboardStagingPriority12.CollectionsInMonthActual =
          db.GetNullableDecimal(reader, 9);
        entities.DashboardStagingPriority12.CollectionsInMonthPriorYear =
          db.GetNullableDecimal(reader, 10);
        entities.DashboardStagingPriority12.CollectionsInMonthPercentChg =
          db.GetNullableDecimal(reader, 11);
        entities.DashboardStagingPriority12.CollectionsInMonthRnk =
          db.GetNullableInt32(reader, 12);
        entities.DashboardStagingPriority12.ContractorNumber =
          db.GetNullableString(reader, 13);
        entities.DashboardStagingPriority12.Populated = true;

        return true;
      },
      () =>
      {
        entities.DashboardStagingPriority12.Populated = false;
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
        entities.DashboardStagingPriority12.CollectionsFfytdToPriorMonth =
          db.GetNullableDecimal(reader, 4);
        entities.DashboardStagingPriority12.CollectionsFfytdActual =
          db.GetNullableDecimal(reader, 5);
        entities.DashboardStagingPriority12.CollectionsFfytdPriorYear =
          db.GetNullableDecimal(reader, 6);
        entities.DashboardStagingPriority12.CollectionsFfytdPercentChange =
          db.GetNullableDecimal(reader, 7);
        entities.DashboardStagingPriority12.CollectionsFfytdRnk =
          db.GetNullableInt32(reader, 8);
        entities.DashboardStagingPriority12.CollectionsInMonthActual =
          db.GetNullableDecimal(reader, 9);
        entities.DashboardStagingPriority12.CollectionsInMonthPriorYear =
          db.GetNullableDecimal(reader, 10);
        entities.DashboardStagingPriority12.CollectionsInMonthPercentChg =
          db.GetNullableDecimal(reader, 11);
        entities.DashboardStagingPriority12.CollectionsInMonthRnk =
          db.GetNullableInt32(reader, 12);
        entities.DashboardStagingPriority12.ContractorNumber =
          db.GetNullableString(reader, 13);
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
        entities.DashboardStagingPriority12.CollectionsFfytdToPriorMonth =
          db.GetNullableDecimal(reader, 4);
        entities.DashboardStagingPriority12.CollectionsFfytdActual =
          db.GetNullableDecimal(reader, 5);
        entities.DashboardStagingPriority12.CollectionsFfytdPriorYear =
          db.GetNullableDecimal(reader, 6);
        entities.DashboardStagingPriority12.CollectionsFfytdPercentChange =
          db.GetNullableDecimal(reader, 7);
        entities.DashboardStagingPriority12.CollectionsFfytdRnk =
          db.GetNullableInt32(reader, 8);
        entities.DashboardStagingPriority12.CollectionsInMonthActual =
          db.GetNullableDecimal(reader, 9);
        entities.DashboardStagingPriority12.CollectionsInMonthPriorYear =
          db.GetNullableDecimal(reader, 10);
        entities.DashboardStagingPriority12.CollectionsInMonthPercentChg =
          db.GetNullableDecimal(reader, 11);
        entities.DashboardStagingPriority12.CollectionsInMonthRnk =
          db.GetNullableInt32(reader, 12);
        entities.DashboardStagingPriority12.ContractorNumber =
          db.GetNullableString(reader, 13);
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
        entities.DashboardStagingPriority12.CollectionsFfytdToPriorMonth =
          db.GetNullableDecimal(reader, 4);
        entities.DashboardStagingPriority12.CollectionsFfytdActual =
          db.GetNullableDecimal(reader, 5);
        entities.DashboardStagingPriority12.CollectionsFfytdPriorYear =
          db.GetNullableDecimal(reader, 6);
        entities.DashboardStagingPriority12.CollectionsFfytdPercentChange =
          db.GetNullableDecimal(reader, 7);
        entities.DashboardStagingPriority12.CollectionsFfytdRnk =
          db.GetNullableInt32(reader, 8);
        entities.DashboardStagingPriority12.CollectionsInMonthActual =
          db.GetNullableDecimal(reader, 9);
        entities.DashboardStagingPriority12.CollectionsInMonthPriorYear =
          db.GetNullableDecimal(reader, 10);
        entities.DashboardStagingPriority12.CollectionsInMonthPercentChg =
          db.GetNullableDecimal(reader, 11);
        entities.DashboardStagingPriority12.CollectionsInMonthRnk =
          db.GetNullableInt32(reader, 12);
        entities.DashboardStagingPriority12.ContractorNumber =
          db.GetNullableString(reader, 13);
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
        entities.DashboardStagingPriority12.CollectionsFfytdToPriorMonth =
          db.GetNullableDecimal(reader, 4);
        entities.DashboardStagingPriority12.CollectionsFfytdActual =
          db.GetNullableDecimal(reader, 5);
        entities.DashboardStagingPriority12.CollectionsFfytdPriorYear =
          db.GetNullableDecimal(reader, 6);
        entities.DashboardStagingPriority12.CollectionsFfytdPercentChange =
          db.GetNullableDecimal(reader, 7);
        entities.DashboardStagingPriority12.CollectionsFfytdRnk =
          db.GetNullableInt32(reader, 8);
        entities.DashboardStagingPriority12.CollectionsInMonthActual =
          db.GetNullableDecimal(reader, 9);
        entities.DashboardStagingPriority12.CollectionsInMonthPriorYear =
          db.GetNullableDecimal(reader, 10);
        entities.DashboardStagingPriority12.CollectionsInMonthPercentChg =
          db.GetNullableDecimal(reader, 11);
        entities.DashboardStagingPriority12.CollectionsInMonthRnk =
          db.GetNullableInt32(reader, 12);
        entities.DashboardStagingPriority12.ContractorNumber =
          db.GetNullableString(reader, 13);
        entities.DashboardStagingPriority12.Populated = true;
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
        entities.DashboardStagingPriority12.ReportMonth =
          db.GetInt32(reader, 0);
        entities.DashboardStagingPriority12.ReportLevel =
          db.GetString(reader, 1);
        entities.DashboardStagingPriority12.ReportLevelId =
          db.GetString(reader, 2);
        entities.DashboardStagingPriority12.AsOfDate =
          db.GetNullableDate(reader, 3);
        entities.DashboardStagingPriority12.CollectionsFfytdToPriorMonth =
          db.GetNullableDecimal(reader, 4);
        entities.DashboardStagingPriority12.CollectionsFfytdActual =
          db.GetNullableDecimal(reader, 5);
        entities.DashboardStagingPriority12.CollectionsFfytdPriorYear =
          db.GetNullableDecimal(reader, 6);
        entities.DashboardStagingPriority12.CollectionsFfytdPercentChange =
          db.GetNullableDecimal(reader, 7);
        entities.DashboardStagingPriority12.CollectionsFfytdRnk =
          db.GetNullableInt32(reader, 8);
        entities.DashboardStagingPriority12.CollectionsInMonthActual =
          db.GetNullableDecimal(reader, 9);
        entities.DashboardStagingPriority12.CollectionsInMonthPriorYear =
          db.GetNullableDecimal(reader, 10);
        entities.DashboardStagingPriority12.CollectionsInMonthPercentChg =
          db.GetNullableDecimal(reader, 11);
        entities.DashboardStagingPriority12.CollectionsInMonthRnk =
          db.GetNullableInt32(reader, 12);
        entities.DashboardStagingPriority12.ContractorNumber =
          db.GetNullableString(reader, 13);
        entities.DashboardStagingPriority12.Populated = true;

        return true;
      },
      () =>
      {
        entities.DashboardStagingPriority12.Populated = false;
      });
  }

  private bool ReadDashboardStagingPriority6()
  {
    entities.Other.Populated = false;

    return Read("ReadDashboardStagingPriority6",
      (db, command) =>
      {
        db.SetString(
          command, "reportLevel",
          local.OtherDashboardStagingPriority12.ReportLevel);
        db.SetString(
          command, "reportLevelId",
          local.OtherDashboardStagingPriority12.ReportLevelId);
        db.SetInt32(
          command, "reportMonth",
          local.OtherDashboardStagingPriority12.ReportMonth);
      },
      (db, reader) =>
      {
        entities.Other.ReportMonth = db.GetInt32(reader, 0);
        entities.Other.ReportLevel = db.GetString(reader, 1);
        entities.Other.ReportLevelId = db.GetString(reader, 2);
        entities.Other.CollectionsFfytdActual =
          db.GetNullableDecimal(reader, 3);
        entities.Other.CollectionsInMonthActual =
          db.GetNullableDecimal(reader, 4);
        entities.Other.Populated = true;
      });
  }

  private bool ReadDashboardStagingPriority7()
  {
    entities.DashboardStagingPriority12.Populated = false;

    return Read("ReadDashboardStagingPriority7",
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
        entities.DashboardStagingPriority12.CollectionsFfytdToPriorMonth =
          db.GetNullableDecimal(reader, 4);
        entities.DashboardStagingPriority12.CollectionsFfytdActual =
          db.GetNullableDecimal(reader, 5);
        entities.DashboardStagingPriority12.CollectionsFfytdPriorYear =
          db.GetNullableDecimal(reader, 6);
        entities.DashboardStagingPriority12.CollectionsFfytdPercentChange =
          db.GetNullableDecimal(reader, 7);
        entities.DashboardStagingPriority12.CollectionsFfytdRnk =
          db.GetNullableInt32(reader, 8);
        entities.DashboardStagingPriority12.CollectionsInMonthActual =
          db.GetNullableDecimal(reader, 9);
        entities.DashboardStagingPriority12.CollectionsInMonthPriorYear =
          db.GetNullableDecimal(reader, 10);
        entities.DashboardStagingPriority12.CollectionsInMonthPercentChg =
          db.GetNullableDecimal(reader, 11);
        entities.DashboardStagingPriority12.CollectionsInMonthRnk =
          db.GetNullableInt32(reader, 12);
        entities.DashboardStagingPriority12.ContractorNumber =
          db.GetNullableString(reader, 13);
        entities.DashboardStagingPriority12.Populated = true;
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
        entities.DashboardStagingPriority12.CollectionsFfytdToPriorMonth =
          db.GetNullableDecimal(reader, 4);
        entities.DashboardStagingPriority12.CollectionsFfytdActual =
          db.GetNullableDecimal(reader, 5);
        entities.DashboardStagingPriority12.CollectionsFfytdPriorYear =
          db.GetNullableDecimal(reader, 6);
        entities.DashboardStagingPriority12.CollectionsFfytdPercentChange =
          db.GetNullableDecimal(reader, 7);
        entities.DashboardStagingPriority12.CollectionsFfytdRnk =
          db.GetNullableInt32(reader, 8);
        entities.DashboardStagingPriority12.CollectionsInMonthActual =
          db.GetNullableDecimal(reader, 9);
        entities.DashboardStagingPriority12.CollectionsInMonthPriorYear =
          db.GetNullableDecimal(reader, 10);
        entities.DashboardStagingPriority12.CollectionsInMonthPercentChg =
          db.GetNullableDecimal(reader, 11);
        entities.DashboardStagingPriority12.CollectionsInMonthRnk =
          db.GetNullableInt32(reader, 12);
        entities.DashboardStagingPriority12.ContractorNumber =
          db.GetNullableString(reader, 13);
        entities.DashboardStagingPriority12.Populated = true;

        return true;
      },
      () =>
      {
        entities.DashboardStagingPriority12.Populated = false;
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
        entities.DashboardStagingPriority12.CollectionsFfytdToPriorMonth =
          db.GetNullableDecimal(reader, 4);
        entities.DashboardStagingPriority12.CollectionsFfytdActual =
          db.GetNullableDecimal(reader, 5);
        entities.DashboardStagingPriority12.CollectionsFfytdPriorYear =
          db.GetNullableDecimal(reader, 6);
        entities.DashboardStagingPriority12.CollectionsFfytdPercentChange =
          db.GetNullableDecimal(reader, 7);
        entities.DashboardStagingPriority12.CollectionsFfytdRnk =
          db.GetNullableInt32(reader, 8);
        entities.DashboardStagingPriority12.CollectionsInMonthActual =
          db.GetNullableDecimal(reader, 9);
        entities.DashboardStagingPriority12.CollectionsInMonthPriorYear =
          db.GetNullableDecimal(reader, 10);
        entities.DashboardStagingPriority12.CollectionsInMonthPercentChg =
          db.GetNullableDecimal(reader, 11);
        entities.DashboardStagingPriority12.CollectionsInMonthRnk =
          db.GetNullableInt32(reader, 12);
        entities.DashboardStagingPriority12.ContractorNumber =
          db.GetNullableString(reader, 13);
        entities.DashboardStagingPriority12.Populated = true;

        return true;
      },
      () =>
      {
        entities.DashboardStagingPriority12.Populated = false;
      });
  }

  private bool ReadFipsTribunal()
  {
    System.Diagnostics.Debug.Assert(entities.LegalActionDetail.Populated);
    entities.Fips.Populated = false;
    entities.Tribunal.Populated = false;

    return Read("ReadFipsTribunal",
      (db, command) =>
      {
        db.SetInt32(
          command, "legalActionId", entities.LegalActionDetail.LgaIdentifier);
      },
      (db, reader) =>
      {
        entities.Fips.State = db.GetInt32(reader, 0);
        entities.Tribunal.FipState = db.GetNullableInt32(reader, 0);
        entities.Fips.County = db.GetInt32(reader, 1);
        entities.Tribunal.FipCounty = db.GetNullableInt32(reader, 1);
        entities.Fips.Location = db.GetInt32(reader, 2);
        entities.Tribunal.FipLocation = db.GetNullableInt32(reader, 2);
        entities.Tribunal.JudicialDistrict = db.GetString(reader, 3);
        entities.Tribunal.Identifier = db.GetInt32(reader, 4);
        entities.Fips.Populated = true;
        entities.Tribunal.Populated = true;
      });
  }

  private bool ReadLegalActionDetail()
  {
    System.Diagnostics.Debug.Assert(entities.Collection.Populated);
    entities.LegalActionDetail.Populated = false;

    return Read("ReadLegalActionDetail",
      (db, command) =>
      {
        db.SetString(command, "otrType", entities.Collection.OtrType);
        db.SetInt32(command, "dtyGeneratedId", entities.Collection.OtyId);
        db.SetInt32(command, "obId", entities.Collection.ObgId);
        db.SetString(command, "cspNumber", entities.Collection.CspNumber);
        db.SetString(command, "cpaType", entities.Collection.CpaType);
      },
      (db, reader) =>
      {
        entities.LegalActionDetail.LgaIdentifier = db.GetInt32(reader, 0);
        entities.LegalActionDetail.Number = db.GetInt32(reader, 1);
        entities.LegalActionDetail.DetailType = db.GetString(reader, 2);
        entities.LegalActionDetail.Populated = true;
        CheckValid<LegalActionDetail>("DetailType",
          entities.LegalActionDetail.DetailType);
      });
  }

  private void UpdateDashboardStagingPriority1()
  {
    var asOfDate = local.Statewide.AsOfDate;
    var collectionsFfytdToPriorMonth =
      local.Statewide.CollectionsFfytdToPriorMonth ?? 0M;
    var collectionsFfytdActual = local.Statewide.CollectionsFfytdActual ?? 0M;
    var collectionsFfytdPriorYear =
      local.Statewide.CollectionsFfytdPriorYear ?? 0M;
    var collectionsFfytdPercentChange =
      local.Statewide.CollectionsFfytdPercentChange ?? 0M;
    var collectionsFfytdRnk = local.Statewide.CollectionsFfytdRnk ?? 0;
    var collectionsInMonthActual = local.Statewide.CollectionsInMonthActual ?? 0M
      ;
    var collectionsInMonthPriorYear =
      local.Statewide.CollectionsInMonthPriorYear ?? 0M;
    var collectionsInMonthPercentChg =
      local.Statewide.CollectionsInMonthPercentChg ?? 0M;
    var collectionsInMonthRnk = local.Statewide.CollectionsInMonthRnk ?? 0;

    entities.DashboardStagingPriority12.Populated = false;
    Update("UpdateDashboardStagingPriority1",
      (db, command) =>
      {
        db.SetNullableDate(command, "asOfDt", asOfDate);
        db.SetNullableDecimal(
          command, "collYtdToPriMo", collectionsFfytdToPriorMonth);
        db.SetNullableDecimal(command, "collYtdAct", collectionsFfytdActual);
        db.
          SetNullableDecimal(command, "collYtdPriYr", collectionsFfytdPriorYear);
        db.SetNullableDecimal(
          command, "collYtdPerChg", collectionsFfytdPercentChange);
        db.SetNullableInt32(command, "collYtdRnk", collectionsFfytdRnk);
        db.
          SetNullableDecimal(command, "collInMthAct", collectionsInMonthActual);
        db.SetNullableDecimal(
          command, "collInMthPriYr", collectionsInMonthPriorYear);
        db.SetNullableDecimal(
          command, "collInMthPerCh", collectionsInMonthPercentChg);
        db.SetNullableInt32(command, "collInMthRnk", collectionsInMonthRnk);
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
    entities.DashboardStagingPriority12.CollectionsFfytdToPriorMonth =
      collectionsFfytdToPriorMonth;
    entities.DashboardStagingPriority12.CollectionsFfytdActual =
      collectionsFfytdActual;
    entities.DashboardStagingPriority12.CollectionsFfytdPriorYear =
      collectionsFfytdPriorYear;
    entities.DashboardStagingPriority12.CollectionsFfytdPercentChange =
      collectionsFfytdPercentChange;
    entities.DashboardStagingPriority12.CollectionsFfytdRnk =
      collectionsFfytdRnk;
    entities.DashboardStagingPriority12.CollectionsInMonthActual =
      collectionsInMonthActual;
    entities.DashboardStagingPriority12.CollectionsInMonthPriorYear =
      collectionsInMonthPriorYear;
    entities.DashboardStagingPriority12.CollectionsInMonthPercentChg =
      collectionsInMonthPercentChg;
    entities.DashboardStagingPriority12.CollectionsInMonthRnk =
      collectionsInMonthRnk;
    entities.DashboardStagingPriority12.Populated = true;
  }

  private void UpdateDashboardStagingPriority10()
  {
    var collectionsInMonthRnk =
      local.TempDashboardStagingPriority12.CollectionsInMonthRnk ?? 0;

    entities.DashboardStagingPriority12.Populated = false;
    Update("UpdateDashboardStagingPriority10",
      (db, command) =>
      {
        db.SetNullableInt32(command, "collInMthRnk", collectionsInMonthRnk);
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

    entities.DashboardStagingPriority12.CollectionsInMonthRnk =
      collectionsInMonthRnk;
    entities.DashboardStagingPriority12.Populated = true;
  }

  private void UpdateDashboardStagingPriority11()
  {
    var collectionsFfytdRnk =
      local.TempDashboardStagingPriority12.CollectionsFfytdRnk ?? 0;

    entities.DashboardStagingPriority12.Populated = false;
    Update("UpdateDashboardStagingPriority11",
      (db, command) =>
      {
        db.SetNullableInt32(command, "collYtdRnk", collectionsFfytdRnk);
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

    entities.DashboardStagingPriority12.CollectionsFfytdRnk =
      collectionsFfytdRnk;
    entities.DashboardStagingPriority12.Populated = true;
  }

  private void UpdateDashboardStagingPriority2()
  {
    var asOfDate = local.Local1.Item.G.AsOfDate;
    var collectionsFfytdToPriorMonth =
      local.Local1.Item.G.CollectionsFfytdToPriorMonth ?? 0M;
    var collectionsFfytdActual = local.Local1.Item.G.CollectionsFfytdActual ?? 0M
      ;
    var collectionsFfytdPriorYear =
      local.Local1.Item.G.CollectionsFfytdPriorYear ?? 0M;
    var collectionsFfytdPercentChange =
      local.Local1.Item.G.CollectionsFfytdPercentChange ?? 0M;
    var collectionsFfytdRnk = local.Local1.Item.G.CollectionsFfytdRnk ?? 0;
    var collectionsInMonthActual =
      local.Local1.Item.G.CollectionsInMonthActual ?? 0M;
    var collectionsInMonthPriorYear =
      local.Local1.Item.G.CollectionsInMonthPriorYear ?? 0M;
    var collectionsInMonthPercentChg =
      local.Local1.Item.G.CollectionsInMonthPercentChg ?? 0M;
    var collectionsInMonthRnk = local.Local1.Item.G.CollectionsInMonthRnk ?? 0;

    entities.DashboardStagingPriority12.Populated = false;
    Update("UpdateDashboardStagingPriority2",
      (db, command) =>
      {
        db.SetNullableDate(command, "asOfDt", asOfDate);
        db.SetNullableDecimal(
          command, "collYtdToPriMo", collectionsFfytdToPriorMonth);
        db.SetNullableDecimal(command, "collYtdAct", collectionsFfytdActual);
        db.
          SetNullableDecimal(command, "collYtdPriYr", collectionsFfytdPriorYear);
        db.SetNullableDecimal(
          command, "collYtdPerChg", collectionsFfytdPercentChange);
        db.SetNullableInt32(command, "collYtdRnk", collectionsFfytdRnk);
        db.
          SetNullableDecimal(command, "collInMthAct", collectionsInMonthActual);
        db.SetNullableDecimal(
          command, "collInMthPriYr", collectionsInMonthPriorYear);
        db.SetNullableDecimal(
          command, "collInMthPerCh", collectionsInMonthPercentChg);
        db.SetNullableInt32(command, "collInMthRnk", collectionsInMonthRnk);
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
    entities.DashboardStagingPriority12.CollectionsFfytdToPriorMonth =
      collectionsFfytdToPriorMonth;
    entities.DashboardStagingPriority12.CollectionsFfytdActual =
      collectionsFfytdActual;
    entities.DashboardStagingPriority12.CollectionsFfytdPriorYear =
      collectionsFfytdPriorYear;
    entities.DashboardStagingPriority12.CollectionsFfytdPercentChange =
      collectionsFfytdPercentChange;
    entities.DashboardStagingPriority12.CollectionsFfytdRnk =
      collectionsFfytdRnk;
    entities.DashboardStagingPriority12.CollectionsInMonthActual =
      collectionsInMonthActual;
    entities.DashboardStagingPriority12.CollectionsInMonthPriorYear =
      collectionsInMonthPriorYear;
    entities.DashboardStagingPriority12.CollectionsInMonthPercentChg =
      collectionsInMonthPercentChg;
    entities.DashboardStagingPriority12.CollectionsInMonthRnk =
      collectionsInMonthRnk;
    entities.DashboardStagingPriority12.Populated = true;
  }

  private void UpdateDashboardStagingPriority3()
  {
    var collectionsFfytdToPriorMonth = 0M;

    entities.DashboardStagingPriority12.Populated = false;
    Update("UpdateDashboardStagingPriority3",
      (db, command) =>
      {
        db.SetNullableDecimal(
          command, "collYtdToPriMo", collectionsFfytdToPriorMonth);
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

    entities.DashboardStagingPriority12.CollectionsFfytdToPriorMonth =
      collectionsFfytdToPriorMonth;
    entities.DashboardStagingPriority12.Populated = true;
  }

  private void UpdateDashboardStagingPriority4()
  {
    var collectionsFfytdToPriorMonth = entities.Other.CollectionsFfytdActual;

    entities.DashboardStagingPriority12.Populated = false;
    Update("UpdateDashboardStagingPriority4",
      (db, command) =>
      {
        db.SetNullableDecimal(
          command, "collYtdToPriMo", collectionsFfytdToPriorMonth);
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

    entities.DashboardStagingPriority12.CollectionsFfytdToPriorMonth =
      collectionsFfytdToPriorMonth;
    entities.DashboardStagingPriority12.Populated = true;
  }

  private void UpdateDashboardStagingPriority5()
  {
    var collectionsFfytdPriorYear = entities.Other.CollectionsFfytdActual;
    var collectionsInMonthPriorYear = entities.Other.CollectionsInMonthActual;

    entities.DashboardStagingPriority12.Populated = false;
    Update("UpdateDashboardStagingPriority5",
      (db, command) =>
      {
        db.
          SetNullableDecimal(command, "collYtdPriYr", collectionsFfytdPriorYear);
        db.SetNullableDecimal(
          command, "collInMthPriYr", collectionsInMonthPriorYear);
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

    entities.DashboardStagingPriority12.CollectionsFfytdPriorYear =
      collectionsFfytdPriorYear;
    entities.DashboardStagingPriority12.CollectionsInMonthPriorYear =
      collectionsInMonthPriorYear;
    entities.DashboardStagingPriority12.Populated = true;
  }

  private void UpdateDashboardStagingPriority6()
  {
    var collectionsFfytdPriorYear = 0M;

    entities.DashboardStagingPriority12.Populated = false;
    Update("UpdateDashboardStagingPriority6",
      (db, command) =>
      {
        db.
          SetNullableDecimal(command, "collYtdPriYr", collectionsFfytdPriorYear);
        db.SetNullableDecimal(
          command, "collInMthPriYr", collectionsFfytdPriorYear);
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

    entities.DashboardStagingPriority12.CollectionsFfytdPriorYear =
      collectionsFfytdPriorYear;
    entities.DashboardStagingPriority12.CollectionsInMonthPriorYear =
      collectionsFfytdPriorYear;
    entities.DashboardStagingPriority12.Populated = true;
  }

  private void UpdateDashboardStagingPriority7()
  {
    var collectionsFfytdActual =
      (entities.DashboardStagingPriority12.CollectionsFfytdToPriorMonth ?? 0M) +
      (entities.DashboardStagingPriority12.CollectionsInMonthActual ?? 0M);

    entities.DashboardStagingPriority12.Populated = false;
    Update("UpdateDashboardStagingPriority7",
      (db, command) =>
      {
        db.SetNullableDecimal(command, "collYtdAct", collectionsFfytdActual);
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

    entities.DashboardStagingPriority12.CollectionsFfytdActual =
      collectionsFfytdActual;
    entities.DashboardStagingPriority12.Populated = true;
  }

  private void UpdateDashboardStagingPriority8()
  {
    var asOfDate = local.Contractor.Item.Gcontractor.AsOfDate;
    var collectionsFfytdToPriorMonth =
      local.Contractor.Item.Gcontractor.CollectionsFfytdToPriorMonth ?? 0M;
    var collectionsFfytdActual =
      local.Contractor.Item.Gcontractor.CollectionsFfytdActual ?? 0M;
    var collectionsFfytdPriorYear =
      local.Contractor.Item.Gcontractor.CollectionsFfytdPriorYear ?? 0M;
    var collectionsFfytdPercentChange =
      local.Contractor.Item.Gcontractor.CollectionsFfytdPercentChange ?? 0M;
    var collectionsFfytdRnk =
      local.Contractor.Item.Gcontractor.CollectionsFfytdRnk ?? 0;
    var collectionsInMonthActual =
      local.Contractor.Item.Gcontractor.CollectionsInMonthActual ?? 0M;
    var collectionsInMonthPriorYear =
      local.Contractor.Item.Gcontractor.CollectionsInMonthPriorYear ?? 0M;
    var collectionsInMonthPercentChg =
      local.Contractor.Item.Gcontractor.CollectionsInMonthPercentChg ?? 0M;
    var collectionsInMonthRnk =
      local.Contractor.Item.Gcontractor.CollectionsInMonthRnk ?? 0;

    entities.DashboardStagingPriority12.Populated = false;
    Update("UpdateDashboardStagingPriority8",
      (db, command) =>
      {
        db.SetNullableDate(command, "asOfDt", asOfDate);
        db.SetNullableDecimal(
          command, "collYtdToPriMo", collectionsFfytdToPriorMonth);
        db.SetNullableDecimal(command, "collYtdAct", collectionsFfytdActual);
        db.
          SetNullableDecimal(command, "collYtdPriYr", collectionsFfytdPriorYear);
        db.SetNullableDecimal(
          command, "collYtdPerChg", collectionsFfytdPercentChange);
        db.SetNullableInt32(command, "collYtdRnk", collectionsFfytdRnk);
        db.
          SetNullableDecimal(command, "collInMthAct", collectionsInMonthActual);
        db.SetNullableDecimal(
          command, "collInMthPriYr", collectionsInMonthPriorYear);
        db.SetNullableDecimal(
          command, "collInMthPerCh", collectionsInMonthPercentChg);
        db.SetNullableInt32(command, "collInMthRnk", collectionsInMonthRnk);
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
    entities.DashboardStagingPriority12.CollectionsFfytdToPriorMonth =
      collectionsFfytdToPriorMonth;
    entities.DashboardStagingPriority12.CollectionsFfytdActual =
      collectionsFfytdActual;
    entities.DashboardStagingPriority12.CollectionsFfytdPriorYear =
      collectionsFfytdPriorYear;
    entities.DashboardStagingPriority12.CollectionsFfytdPercentChange =
      collectionsFfytdPercentChange;
    entities.DashboardStagingPriority12.CollectionsFfytdRnk =
      collectionsFfytdRnk;
    entities.DashboardStagingPriority12.CollectionsInMonthActual =
      collectionsInMonthActual;
    entities.DashboardStagingPriority12.CollectionsInMonthPriorYear =
      collectionsInMonthPriorYear;
    entities.DashboardStagingPriority12.CollectionsInMonthPercentChg =
      collectionsInMonthPercentChg;
    entities.DashboardStagingPriority12.CollectionsInMonthRnk =
      collectionsInMonthRnk;
    entities.DashboardStagingPriority12.Populated = true;
  }

  private void UpdateDashboardStagingPriority9()
  {
    var collectionsFfytdPercentChange =
      local.TempDashboardStagingPriority12.CollectionsFfytdPercentChange ?? 0M;
    var collectionsInMonthPercentChg =
      local.TempDashboardStagingPriority12.CollectionsInMonthPercentChg ?? 0M;

    entities.DashboardStagingPriority12.Populated = false;
    Update("UpdateDashboardStagingPriority9",
      (db, command) =>
      {
        db.SetNullableDecimal(
          command, "collYtdPerChg", collectionsFfytdPercentChange);
        db.SetNullableDecimal(
          command, "collInMthPerCh", collectionsInMonthPercentChg);
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

    entities.DashboardStagingPriority12.CollectionsFfytdPercentChange =
      collectionsFfytdPercentChange;
    entities.DashboardStagingPriority12.CollectionsInMonthPercentChg =
      collectionsInMonthPercentChg;
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
    /// A value of FiscalYearStartDate.
    /// </summary>
    public DateWorkArea FiscalYearStartDate
    {
      get => fiscalYearStartDate ??= new();
      set => fiscalYearStartDate = value;
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
    private DateWorkArea? fiscalYearStartDate;
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
    /// A value of TempDateWorkArea.
    /// </summary>
    public DateWorkArea TempDateWorkArea
    {
      get => tempDateWorkArea ??= new();
      set => tempDateWorkArea = value;
    }

    /// <summary>
    /// A value of Restart.
    /// </summary>
    public Collection Restart
    {
      get => restart ??= new();
      set => restart = value;
    }

    /// <summary>
    /// A value of Prev.
    /// </summary>
    public Collection Prev
    {
      get => prev ??= new();
      set => prev = value;
    }

    /// <summary>
    /// A value of OtherDateWorkArea.
    /// </summary>
    public DateWorkArea OtherDateWorkArea
    {
      get => otherDateWorkArea ??= new();
      set => otherDateWorkArea = value;
    }

    /// <summary>
    /// A value of OtherDashboardStagingPriority12.
    /// </summary>
    public DashboardStagingPriority12 OtherDashboardStagingPriority12
    {
      get => otherDashboardStagingPriority12 ??= new();
      set => otherDashboardStagingPriority12 = value;
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
    /// A value of ReportingAbbreviation.
    /// </summary>
    public TextWorkArea ReportingAbbreviation
    {
      get => reportingAbbreviation ??= new();
      set => reportingAbbreviation = value;
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
    /// A value of Null1.
    /// </summary>
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    /// <summary>
    /// A value of DateWorkArea.
    /// </summary>
    public DateWorkArea DateWorkArea
    {
      get => dateWorkArea ??= new();
      set => dateWorkArea = value;
    }

    /// <summary>
    /// A value of TempDashboardStagingPriority12.
    /// </summary>
    public DashboardStagingPriority12 TempDashboardStagingPriority12
    {
      get => tempDashboardStagingPriority12 ??= new();
      set => tempDashboardStagingPriority12 = value;
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

    private DateWorkArea? tempDateWorkArea;
    private Collection? restart;
    private Collection? prev;
    private DateWorkArea? otherDateWorkArea;
    private DashboardStagingPriority12? otherDashboardStagingPriority12;
    private DashboardAuditData? initialized;
    private ProgramCheckpointRestart? programCheckpointRestart;
    private DashboardStagingPriority12? statewide;
    private EabFileHandling? eabFileHandling;
    private EabReportSend? eabReportSend;
    private Array<LocalGroup>? local1;
    private TextWorkArea? reportingAbbreviation;
    private Common? recordsReadSinceCommit;
    private DashboardAuditData? dashboardAuditData;
    private DateWorkArea? null1;
    private DateWorkArea? dateWorkArea;
    private DashboardStagingPriority12? tempDashboardStagingPriority12;
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
    /// A value of Fips.
    /// </summary>
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
    }

    /// <summary>
    /// A value of Tribunal.
    /// </summary>
    public Tribunal Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
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
    /// A value of LaPersonLaCaseRole.
    /// </summary>
    public LaPersonLaCaseRole LaPersonLaCaseRole
    {
      get => laPersonLaCaseRole ??= new();
      set => laPersonLaCaseRole = value;
    }

    /// <summary>
    /// A value of LegalActionPerson.
    /// </summary>
    public LegalActionPerson LegalActionPerson
    {
      get => legalActionPerson ??= new();
      set => legalActionPerson = value;
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
    /// A value of CollectionType.
    /// </summary>
    public CollectionType CollectionType
    {
      get => collectionType ??= new();
      set => collectionType = value;
    }

    /// <summary>
    /// A value of Other.
    /// </summary>
    public DashboardStagingPriority12 Other
    {
      get => other ??= new();
      set => other = value;
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
    /// A value of Supp.
    /// </summary>
    public CsePerson Supp
    {
      get => supp ??= new();
      set => supp = value;
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
    /// A value of Ap.
    /// </summary>
    public CsePerson Ap
    {
      get => ap ??= new();
      set => ap = value;
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
    /// A value of LegalAction.
    /// </summary>
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
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
    /// A value of ObligationType.
    /// </summary>
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
    }

    private Fips? fips;
    private Tribunal? tribunal;
    private CaseAssignment? caseAssignment;
    private Case1? case1;
    private CaseRole? caseRole;
    private LegalActionCaseRole? legalActionCaseRole;
    private LaPersonLaCaseRole? laPersonLaCaseRole;
    private LegalActionPerson? legalActionPerson;
    private LegalActionDetail? legalActionDetail;
    private CollectionType? collectionType;
    private DashboardStagingPriority12? other;
    private CseOrganization? cseOrganization;
    private DashboardStagingPriority12? dashboardStagingPriority12;
    private CsePerson? supp;
    private Collection? collection;
    private CsePerson? ap;
    private ObligationTransaction? debt;
    private Obligation? obligation;
    private CsePersonAccount? obligor;
    private CsePersonAccount? supported;
    private CashReceiptDetail? cashReceiptDetail;
    private CashReceipt? cashReceipt;
    private CashReceiptType? cashReceiptType;
    private LegalAction? legalAction;
    private DebtDetail? debtDetail;
    private ObligationType? obligationType;
  }
#endregion
}
