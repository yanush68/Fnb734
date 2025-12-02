// Program: FN_B734_PRIORITY_1_7, ID: 945132076, model: 746.
// Short name: SWE03088
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
/// A program: FN_B734_PRIORITY_1_7.
/// </para>
/// <para>
/// Priority 1-7: Arrears Distributed
/// </para>
/// </summary>
[Serializable]
[Program("SWE03088")]
public partial class FnB734Priority17: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B734_PRIORITY_1_7 program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB734Priority17(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB734Priority17.
  /// </summary>
  public FnB734Priority17(IContext context, Import import, Export export):
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
    // 02/04/20  GVandy	CQ66220		Correlate with OCSE157 changes beginning in FY 
    // 2022.
    // 					These changes include only amounts in OCSE157
    // 					Lines 25, 27, and 29 that are both distributed
    // 					and disbursed.  Export a cutoff FY which defaults to
    // 					2022 but can be overridden with a code table value for testing.
    // ---------------------------------------------------------------------------------------------------
    // -------------------------------------------------------------------------------------
    // Priority 1-7: Arrears Distributed (Report month, FFYTD)
    // -------------------------------------------------------------------------------------
    // 2 Columns:
    // 	1. Arrears distributed report month
    // 	2. Arrears distributed FFYTD
    // Report Level: State, Judicial District
    // Report Period: Month and Month/Fiscal year-to-date calculation
    // Definition of Arrears Distributed (using OCSE157 Line27)
    // 	1) Collections created (distributed) during report period.  Applied to 
    // A arrears.
    // 	2) Bypass concurrent (primary/secondary- count primary only)
    // 	3) For joint/several situations, count the collection only once.
    // 	4) Bypass FcrtRec and FDIR (REIP) cash receipt types.
    // 	5) Bypass collections created and adjusted in report period.
    // 	6) Count negative collection where adjustment occurred in report period 
    // to a
    // 	   collection created in a prior report period.
    // 	7) Bypass fees, recoveries, 718Bs and MJs (AF, AFI, FC, FCI).
    // 	8) Exclude all collections that have applied as Incoming Interstate
    // 	   (NAI, AFI, FCI).
    // -------------------------------------------------------------------------------------
    MoveDashboardAuditData2(import.DashboardAuditData, local.Initialized);
    MoveProgramCheckpointRestart(import.ProgramCheckpointRestart,
      local.ProgramCheckpointRestart);

    // -- Determine the federal fiscal year.
    local.FiscalYear.Year = Year(AddMonths(import.FiscalYearStart.Date, 3));

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
      (import.ProgramCheckpointRestart.RestartInfo, 81, 8, "1-07    "))
    {
      if (local.FiscalYear.Year < (import.Cq66220EffectiveFy.FiscalYear ?? 0))
      {
        // -- Checkpoint Info
        // Positions   Value
        // ---------   
        // ------------------------------------
        //  001-080    General Checkpoint Info for PRAD
        //  081-088    Dashboard Priority
        //  089-089    Blank
        //  090-098    Collection System Generated ID
        //  099-099    Blank
        //  100-100    Local Period Count
        if (!IsEmpty(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 90, 9)))
        {
          local.RestartCollection.SystemGeneratedIdentifier =
            (int)StringToNumber(Substring(
              import.ProgramCheckpointRestart.RestartInfo, 250, 90, 9));
        }
      }
      else if (CharAt(import.ProgramCheckpointRestart.RestartInfo, 102) == 'B')
      {
        // -- Checkpoint Info
        // Positions   Value
        // ---------   
        // ------------------------------------
        //  001-080    General Checkpoint Info for PRAD
        //  081-088    Dashboard Priority
        //  089-089    Blank
        //  090-098    Payment Request ID
        //  099-099    Blank
        //  100-100    Local Period Count
        //  101-101    Blank
        //  102-102    "B" (indicating to restart in part 2)
        local.RestartCollection.SystemGeneratedIdentifier = 999999999;
        local.RestartPaymentRequest.SystemGeneratedIdentifier =
          (int)StringToNumber(Substring(
            import.ProgramCheckpointRestart.RestartInfo, 250, 90, 9));
      }
      else
      {
        // -- Checkpoint Info
        // Positions   Value
        // ---------   
        // ------------------------------------
        //  001-080    General Checkpoint Info for PRAD
        //  081-088    Dashboard Priority
        //  089-089    Blank
        //  090-098    Collection System Generated ID
        //  099-099    Blank
        //  100-100    Local Period Count
        local.RestartCollection.SystemGeneratedIdentifier =
          (int)StringToNumber(Substring(
            import.ProgramCheckpointRestart.RestartInfo, 250, 90, 9));
        local.RestartPaymentRequest.SystemGeneratedIdentifier = 0;
      }

      if (IsEmpty(Substring(import.ProgramCheckpointRestart.RestartInfo, 100, 1)))
      {
        local.PeriodStart.Count = 1;
      }
      else
      {
        local.PeriodStart.Count =
          (int)StringToNumber(Substring(
            import.ProgramCheckpointRestart.RestartInfo, 250, 100, 1));
      }

      if (!IsEmpty(
        Substring(import.ProgramCheckpointRestart.RestartInfo, 90, 10)))
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
          local.Statewide.ArrearsDistributedFfytdActual = 0;
          local.Statewide.ArrearsDistributedMonthActual = 0;
          local.Statewide.ArrearsDistributedMonthRnk = 0;
          local.Statewide.ArrearsDistrubutedFfytdRnk = 0;
        }

        // -- Load Judicial District counts.
        foreach(var _ in ReadDashboardStagingPriority2())
        {
          local.Local1.Index =
            (int)StringToNumber(entities.DashboardStagingPriority12.
              ReportLevelId) - 1;
          local.Local1.CheckSize();

          local.Local1.Update.G.Assign(entities.DashboardStagingPriority12);
          local.Local1.Update.G.ArrearsDistributedFfytdActual = 0;
          local.Local1.Update.G.ArrearsDistributedMonthActual = 0;
          local.Local1.Update.G.ArrearsDistributedMonthRnk = 0;
          local.Local1.Update.G.ArrearsDistrubutedFfytdRnk = 0;
        }
      }
    }
    else
    {
      local.RestartCollection.SystemGeneratedIdentifier = 0;
      local.RestartPaymentRequest.SystemGeneratedIdentifier = 0;
      local.PeriodStart.Count = 1;
    }

    for(local.Period.Count = local.PeriodStart.Count; local.Period.Count <= 2; ++
      local.Period.Count)
    {
      switch(local.Period.Count)
      {
        case 1:
          // -- Calculate in-month current support due and collected.
          MoveDateWorkArea(import.ReportStartDate, local.ReportStartDate);
          MoveDateWorkArea(import.ReportEndDate, local.ReportEndDate);

          // -- Set local audit priority value to "IM" (In-Month).  This will be
          // concatenated to
          //    the end of the audit record priority number to indicate in which
          // period the record
          //    was included.
          local.ReportingAbbreviation.Text2 = "IM";

          break;
        case 2:
          // -- Calculate Fiscal Year to Date current support due and collected.
          MoveDateWorkArea(import.FiscalYearStart, local.ReportStartDate);
          MoveDateWorkArea(import.ReportEndDate, local.ReportEndDate);

          // -- Set local audit priority value to "FY" (Fiscal Year to date).  
          // This will be
          //    concatenated to the end of the audit record priority number to 
          // indicate in which
          //    period the record was included.
          local.ReportingAbbreviation.Text2 = "FY";

          break;
        default:
          break;
      }

      if (local.FiscalYear.Year < (import.Cq66220EffectiveFy.FiscalYear ?? 0))
      {
        // -------------------------------------------------------------------
        // -Read Arrears collections
        // -Skip Fees, Recoveries, 718B, MJ AF
        // -Read colls 'created during' FY and un-adj at the end of FY
        // -Read colls 'adjusted during' FY but created in prev FYs
        // -Skip Concurrent colls
        // -Skip direct payments. (CRT= 2 or 7)
        // -------------------------------------------------------------------
        // -------------------------------------------------------------------
        // -Exclude incoming interstate collections.  04/14/08 CQ#2461
        // -------------------------------------------------------------------
        // -------------------------------------------------------------------
        // Comments on READ EACH.
        // -Generates 3 table join on collection, ob_trn, ob_type
        // -Redundant created_tmst qualification is to aid performance
        // -------------------------------------------------------------------
        foreach(var _ in ReadCollectionCsePersonCsePerson1())
        {
          if (!entities.Supp.Populated)
          {
            continue;
          }

          if (!entities.Ap.Populated)
          {
            continue;
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
              for(local.Local1.Index = 0; local.Local1.Index < local
                .Local1.Count; ++local.Local1.Index)
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
              //  099-099    Blank
              //  100-100    Local Period Count
              local.ProgramCheckpointRestart.RestartInd = "Y";
              local.ProgramCheckpointRestart.RestartInfo =
                Substring(import.ProgramCheckpointRestart.RestartInfo, 250, 1,
                80) + "1-07    " + " " + NumberToString
                (local.Prev.SystemGeneratedIdentifier, 7, 9) + " " + NumberToString
                (local.Period.Count, 15, 1);
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
          MoveDashboardAuditData1(local.Initialized, local.DashboardAuditData);
          ++local.RecordsReadSinceCommit.Count;

          if (Lt(entities.Collection.CreatedTmst,
            local.ReportStartDate.Timestamp))
          {
            // -----------------------------------------------------------------
            // This must be an adjustment to a collection created in prev report
            // period.
            // -----------------------------------------------------------------
            local.DashboardAuditData.CollectionAmount =
              -entities.Collection.Amount;
          }
          else
          {
            local.DashboardAuditData.CollectionAmount =
              entities.Collection.Amount;
          }

          // -- Increment Statewide Level
          switch(local.Period.Count)
          {
            case 1:
              // -- Increment In-Month Statewide Level
              local.Statewide.ArrearsDistributedMonthActual =
                (local.Statewide.ArrearsDistributedMonthActual ?? 0M) + (
                  local.DashboardAuditData.CollectionAmount ?? 0M);

              break;
            case 2:
              // -- Increment Fiscal Year to date Statewide Level
              local.Statewide.ArrearsDistributedFfytdActual =
                (local.Statewide.ArrearsDistributedFfytdActual ?? 0M) + (
                  local.DashboardAuditData.CollectionAmount ?? 0M);

              break;
            default:
              break;
          }

          // -- Determine Judicial District...
          UseFnB734DetermineJdFromOrder();

          // -- Increment Judicial District Level
          if (!IsEmpty(local.DashboardAuditData.JudicialDistrict))
          {
            local.Local1.Index =
              (int)StringToNumber(local.DashboardAuditData.JudicialDistrict) - 1
              ;
            local.Local1.CheckSize();

            switch(local.Period.Count)
            {
              case 1:
                // -- Increment In-Month Judicial District Level
                local.Local1.Update.G.ArrearsDistributedMonthActual =
                  (local.Local1.Item.G.ArrearsDistributedMonthActual ?? 0M) + (
                    local.DashboardAuditData.CollectionAmount ?? 0M);

                break;
              case 2:
                // -- Increment Fiscal Year to date Judicial District Level
                local.Local1.Update.G.ArrearsDistributedFfytdActual =
                  (local.Local1.Item.G.ArrearsDistributedFfytdActual ?? 0M) + (
                    local.DashboardAuditData.CollectionAmount ?? 0M);

                break;
              default:
                break;
            }
          }

          // -- Log to the audit table.
          local.DashboardAuditData.DashboardPriority = "1-7" + String
            (local.ReportingAbbreviation.Text2, TextWorkArea.Text2_MaxLength);
          local.DashboardAuditData.CollectionCreatedDate =
            Date(entities.Collection.CreatedTmst);
          local.DashboardAuditData.CollAppliedToCd =
            entities.Collection.AppliedToCode;
          local.DashboardAuditData.SuppCspNumber = entities.Supp.Number;
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
      }
      else
      {
        // 2/04/20 GVandy  CQ66220  Beginning in FY 2022, include only amounts 
        // that are both
        // distributed and disbursed.
        // -------------------------------------------------------------------
        // -Read Arrears collections
        // -Skip Fees, Recoveries, 718B, MJ AF
        // -Read colls 'created during' FY and un-adj at the end of FY
        // -Read colls 'adjusted during' FY but created in prev FYs
        // -Skip Concurrent colls
        // -Skip direct payments. (CRT= 2 or 7)
        // -------------------------------------------------------------------
        // -------------------------------------------------------------------
        // -Exclude incoming interstate collections.  04/14/08 CQ#2461
        // -------------------------------------------------------------------
        // -------------------------------------------------------------------
        // Comments on READ EACH.
        // -Generates 3 table join on collection, ob_trn, ob_type
        // -Redundant created_tmst qualification is to aid performance
        // -------------------------------------------------------------------
        foreach(var _ in ReadCollectionCsePersonCsePerson2())
        {
          if (!entities.Supp.Populated)
          {
            continue;
          }

          if (!entities.Ap.Populated)
          {
            continue;
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
              for(local.Local1.Index = 0; local.Local1.Index < local
                .Local1.Count; ++local.Local1.Index)
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
              //  099-099    Blank
              //  100-100    Local Period Count
              local.ProgramCheckpointRestart.RestartInd = "Y";
              local.ProgramCheckpointRestart.RestartInfo =
                Substring(import.ProgramCheckpointRestart.RestartInfo, 250, 1,
                80) + "1-07    " + " " + NumberToString
                (local.Prev.SystemGeneratedIdentifier, 7, 9) + " " + NumberToString
                (local.Period.Count, 15, 1);
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
          MoveDashboardAuditData1(local.Initialized, local.DashboardAuditData);
          ++local.RecordsReadSinceCommit.Count;

          if (Lt(entities.Collection.CreatedTmst,
            local.ReportStartDate.Timestamp))
          {
            // -----------------------------------------------------------------
            // This must be an adjustment to a collection created in prev report
            // period.
            // -----------------------------------------------------------------
            local.DashboardAuditData.CollectionAmount =
              -entities.Collection.Amount;
          }
          else
          {
            local.DashboardAuditData.CollectionAmount =
              entities.Collection.Amount;
          }

          // -- Increment Statewide Level
          switch(local.Period.Count)
          {
            case 1:
              // -- Increment In-Month Statewide Level
              local.Statewide.ArrearsDistributedMonthActual =
                (local.Statewide.ArrearsDistributedMonthActual ?? 0M) + (
                  local.DashboardAuditData.CollectionAmount ?? 0M);

              break;
            case 2:
              // -- Increment Fiscal Year to date Statewide Level
              local.Statewide.ArrearsDistributedFfytdActual =
                (local.Statewide.ArrearsDistributedFfytdActual ?? 0M) + (
                  local.DashboardAuditData.CollectionAmount ?? 0M);

              break;
            default:
              break;
          }

          // -- Determine Judicial District...
          UseFnB734DetermineJdFromOrder();

          // -- Increment Judicial District Level
          if (!IsEmpty(local.DashboardAuditData.JudicialDistrict))
          {
            local.Local1.Index =
              (int)StringToNumber(local.DashboardAuditData.JudicialDistrict) - 1
              ;
            local.Local1.CheckSize();

            switch(local.Period.Count)
            {
              case 1:
                // -- Increment In-Month Judicial District Level
                local.Local1.Update.G.ArrearsDistributedMonthActual =
                  (local.Local1.Item.G.ArrearsDistributedMonthActual ?? 0M) + (
                    local.DashboardAuditData.CollectionAmount ?? 0M);

                break;
              case 2:
                // -- Increment Fiscal Year to date Judicial District Level
                local.Local1.Update.G.ArrearsDistributedFfytdActual =
                  (local.Local1.Item.G.ArrearsDistributedFfytdActual ?? 0M) + (
                    local.DashboardAuditData.CollectionAmount ?? 0M);

                break;
              default:
                break;
            }
          }

          // -- Log to the audit table.
          local.DashboardAuditData.DashboardPriority = "1-7" + String
            (local.ReportingAbbreviation.Text2, TextWorkArea.Text2_MaxLength);
          local.DashboardAuditData.CollectionCreatedDate =
            Date(entities.Collection.CreatedTmst);
          local.DashboardAuditData.CollAppliedToCd =
            entities.Collection.AppliedToCode;
          local.DashboardAuditData.SuppCspNumber = entities.Supp.Number;
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

        foreach(var _ in ReadPaymentRequest())
        {
          ++local.RecordsReadSinceCommit.Count;

          // 2/04/20 GVandy  CQ66220  Beginning in FY 2022, include only amounts
          // that are both
          // distributed and disbursed.  We are now gathering the arrears 
          // collections that are
          // disbursed to a NCP or their designated payee.
          // -------------------------------------------------------------------
          // -Read Arrears collections
          // -Skip Fees, Recoveries, 718B, MJ AF
          // -Read colls 'created during' FY and un-adj at the end of FY
          // -Read colls 'adjusted during' FY but created in prev FYs
          // -Skip Concurrent colls
          // -Skip direct payments. (CRT= 2 or 7)
          // -------------------------------------------------------------------
          // -------------------------------------------------------------------
          // -Exclude incoming interstate collections.  04/14/08 CQ#2461
          // -------------------------------------------------------------------
          // -------------------------------------------------------------------
          // Comments on READ EACH.
          // -Generates 3 table join on collection, ob_trn, ob_type
          // -Redundant created_tmst qualification is to aid performance
          // -------------------------------------------------------------------
          local.Prev.SystemGeneratedIdentifier = 0;

          // -- Note that this READ EACH is set to read DISTINCT collections.
          foreach(var _1 in ReadCollectionCsePersonCsePersonDisbursementTransaction())
          {
            MoveDashboardAuditData1(local.Initialized, local.DashboardAuditData);
            ++local.RecordsReadSinceCommit.Count;
            local.DashboardAuditData.CollectionAmount = entities.Credit.Amount;

            // -- Increment Statewide Level
            switch(local.Period.Count)
            {
              case 1:
                // -- Increment In-Month Statewide Level
                local.Statewide.ArrearsDistributedMonthActual =
                  (local.Statewide.ArrearsDistributedMonthActual ?? 0M) + (
                    local.DashboardAuditData.CollectionAmount ?? 0M);

                break;
              case 2:
                // -- Increment Fiscal Year to date Statewide Level
                local.Statewide.ArrearsDistributedFfytdActual =
                  (local.Statewide.ArrearsDistributedFfytdActual ?? 0M) + (
                    local.DashboardAuditData.CollectionAmount ?? 0M);

                break;
              default:
                break;
            }

            // -- Determine Judicial District...
            UseFnB734DetermineJdFromOrder();

            // -- Increment Judicial District Level
            if (!IsEmpty(local.DashboardAuditData.JudicialDistrict))
            {
              local.Local1.Index =
                (int)StringToNumber(local.DashboardAuditData.JudicialDistrict) -
                1;
              local.Local1.CheckSize();

              switch(local.Period.Count)
              {
                case 1:
                  // -- Increment In-Month Judicial District Level
                  local.Local1.Update.G.ArrearsDistributedMonthActual =
                    (local.Local1.Item.G.ArrearsDistributedMonthActual ?? 0M) +
                    (local.DashboardAuditData.CollectionAmount ?? 0M);

                  break;
                case 2:
                  // -- Increment Fiscal Year to date Judicial District Level
                  local.Local1.Update.G.ArrearsDistributedFfytdActual =
                    (local.Local1.Item.G.ArrearsDistributedFfytdActual ?? 0M) +
                    (local.DashboardAuditData.CollectionAmount ?? 0M);

                  break;
                default:
                  break;
              }
            }

            // -- Log to the audit table.
            local.DashboardAuditData.DashboardPriority = "1-7" + String
              (local.ReportingAbbreviation.Text2, TextWorkArea.Text2_MaxLength);
            local.DashboardAuditData.CollectionCreatedDate =
              Date(entities.Collection.CreatedTmst);
            local.DashboardAuditData.CollAppliedToCd =
              entities.Collection.AppliedToCode;
            local.DashboardAuditData.SuppCspNumber = entities.Supp.Number;
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
            for(local.Local1.Index = 0; local.Local1.Index < local
              .Local1.Count; ++local.Local1.Index)
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
            //  090-098    Payment Request ID
            //  099-099    Blank
            //  100-100    Local Period Count
            //  101-101    Blank
            //  102-102    "B" (indicating to restart in part 2)
            local.ProgramCheckpointRestart.RestartInd = "Y";
            local.ProgramCheckpointRestart.RestartInfo =
              Substring(import.ProgramCheckpointRestart.RestartInfo, 250, 1, 80) +
              "1-07    " + " " + NumberToString
              (entities.PaymentRequest.SystemGeneratedIdentifier, 7, 9) + " " +
              NumberToString(local.Period.Count, 15, 1);
            local.ProgramCheckpointRestart.RestartInfo =
              TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + " B";
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
      }

      // Set restart collection id back to zero so that no records are skipped 
      // on the next iteration of the FOR loop.
      local.RestartCollection.SystemGeneratedIdentifier = 0;
      local.RestartPaymentRequest.SystemGeneratedIdentifier = 0;
      local.Prev.SystemGeneratedIdentifier = 0;
    }

    // ------------------------------------------------------------------------------
    // -- Store final Statewide counts.
    // ------------------------------------------------------------------------------
    if (ReadDashboardStagingPriority3())
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
        try
        {
          CreateDashboardStagingPriority4();
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
          local.Contractor.Update.Gcontractor.ArrearsDistributedFfytdActual =
            (local.Contractor.Item.Gcontractor.
              ArrearsDistributedFfytdActual ?? 0M) + (
              local.Local1.Item.G.ArrearsDistributedFfytdActual ?? 0M);
          local.Contractor.Update.Gcontractor.ArrearsDistributedMonthActual =
            (local.Contractor.Item.Gcontractor.
              ArrearsDistributedMonthActual ?? 0M) + (
              local.Local1.Item.G.ArrearsDistributedMonthActual ?? 0M);

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

    local.Common.Count = 0;
    local.PrevRank.ArrearsDistributedMonthActual = 0;
    local.Temp.ArrearsDistributedMonthRnk = 1;

    // ------------------------------------------------------------------------------
    // -- Calculate Judicial District Ranking (in month).
    // ------------------------------------------------------------------------------
    foreach(var _ in ReadDashboardStagingPriority6())
    {
      ++local.Common.Count;

      if ((entities.DashboardStagingPriority12.
        ArrearsDistributedMonthActual ?? 0M) == (
          local.PrevRank.ArrearsDistributedMonthActual ?? 0M))
      {
        // -- The ranking for this judicial district is tied with the previous 
        // judicial district.
        // -- This JD gets the same ranking already in the local_temp 
        // case_under_order_rank.
      }
      else
      {
        local.Temp.ArrearsDistributedMonthRnk = local.Common.Count;
      }

      try
      {
        UpdateDashboardStagingPriority6();
        MoveDashboardStagingPriority12(entities.DashboardStagingPriority12,
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
    local.PrevRank.ArrearsDistributedMonthActual = 0;
    local.Temp.ArrearsDistributedMonthRnk = 1;

    // ------------------------------------------------------------------------------
    // -- Calculate Contractor's Ranking (in month).
    // ------------------------------------------------------------------------------
    foreach(var _ in ReadDashboardStagingPriority7())
    {
      ++local.Common.Count;

      if ((entities.DashboardStagingPriority12.
        ArrearsDistributedMonthActual ?? 0M) == (
          local.PrevRank.ArrearsDistributedMonthActual ?? 0M))
      {
        // -- The ranking for this contractor is tied with the previous 
        // contractor.
        // -- This contractor gets the same ranking already in the local_temp 
        // case_under_order_rank.
      }
      else
      {
        local.Temp.ArrearsDistributedMonthRnk = local.Common.Count;
      }

      try
      {
        UpdateDashboardStagingPriority6();
        MoveDashboardStagingPriority12(entities.DashboardStagingPriority12,
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
    local.PrevRank.ArrearsDistributedFfytdActual = 0;
    local.Temp.ArrearsDistrubutedFfytdRnk = 1;

    // ------------------------------------------------------------------------------
    // -- Calculate Judicial District Ranking (Fiscal Year To Date).
    // ------------------------------------------------------------------------------
    foreach(var _ in ReadDashboardStagingPriority8())
    {
      ++local.Common.Count;

      if ((entities.DashboardStagingPriority12.
        ArrearsDistributedFfytdActual ?? 0M) == (
          local.PrevRank.ArrearsDistributedFfytdActual ?? 0M))
      {
        // -- The ranking for this judicial district is tied with the previous 
        // judicial district.
        // -- This JD gets the same ranking already in the local_temp 
        // case_under_order_rank.
      }
      else
      {
        local.Temp.ArrearsDistrubutedFfytdRnk = local.Common.Count;
      }

      try
      {
        UpdateDashboardStagingPriority7();
        MoveDashboardStagingPriority12(entities.DashboardStagingPriority12,
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
    local.PrevRank.ArrearsDistributedFfytdActual = 0;
    local.Temp.ArrearsDistrubutedFfytdRnk = 1;

    // ------------------------------------------------------------------------------
    // -- Calculate Contractor's Ranking (Fiscal Year To Date).
    // ------------------------------------------------------------------------------
    foreach(var _ in ReadDashboardStagingPriority9())
    {
      ++local.Common.Count;

      if ((entities.DashboardStagingPriority12.
        ArrearsDistributedFfytdActual ?? 0M) == (
          local.PrevRank.ArrearsDistributedFfytdActual ?? 0M))
      {
        // -- The ranking for this contractor is tied with the previous 
        // contractor.
        // -- This contractor gets the same ranking already in the local_temp 
        // case_under_order_rank.
      }
      else
      {
        local.Temp.ArrearsDistrubutedFfytdRnk = local.Common.Count;
      }

      try
      {
        UpdateDashboardStagingPriority7();
        MoveDashboardStagingPriority12(entities.DashboardStagingPriority12,
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
      Substring(import.ProgramCheckpointRestart.RestartInfo, 250, 1, 80) + "1-08    ";
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

  private static void MoveDashboardStagingPriority12(
    DashboardStagingPriority12 source, DashboardStagingPriority12 target)
  {
    target.ArrearsDistributedMonthActual = source.ArrearsDistributedMonthActual;
    target.ArrearsDistributedFfytdActual = source.ArrearsDistributedFfytdActual;
  }

  private static void MoveDateWorkArea(DateWorkArea source, DateWorkArea target)
  {
    target.Date = source.Date;
    target.Timestamp = source.Timestamp;
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

    useImport.PersistentCollection.Assign(entities.Collection);
    useImport.ReportStartDate.Date = local.ReportStartDate.Date;
    useImport.ReportEndDate.Date = local.ReportEndDate.Date;

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
    var arrearsDistributedMonthActual =
      local.Statewide.ArrearsDistributedMonthActual ?? 0M;
    var arrearsDistributedMonthRnk =
      local.Statewide.ArrearsDistributedMonthRnk ?? 0;
    var arrearsDistributedFfytdActual =
      local.Statewide.ArrearsDistributedFfytdActual ?? 0M;
    var arrearsDistrubutedFfytdRnk =
      local.Statewide.ArrearsDistrubutedFfytdRnk ?? 0;

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
          command, "arrDistMthAct", arrearsDistributedMonthActual);
        db.
          SetNullableInt32(command, "arrDistMthRnk", arrearsDistributedMonthRnk);
        db.SetNullableDecimal(
          command, "arrDistYtdAct", arrearsDistributedFfytdActual);
        db.
          SetNullableInt32(command, "arrDistYtdRnk", arrearsDistrubutedFfytdRnk);
        db.SetNullableDecimal(command, "casPerFteDen", param);
        db.SetNullableString(command, "contractorNum", "");
      });

    entities.DashboardStagingPriority12.ReportMonth = reportMonth;
    entities.DashboardStagingPriority12.ReportLevel = reportLevel;
    entities.DashboardStagingPriority12.ReportLevelId = reportLevelId;
    entities.DashboardStagingPriority12.AsOfDate = asOfDate;
    entities.DashboardStagingPriority12.ArrearsDistributedMonthActual =
      arrearsDistributedMonthActual;
    entities.DashboardStagingPriority12.ArrearsDistributedMonthRnk =
      arrearsDistributedMonthRnk;
    entities.DashboardStagingPriority12.ArrearsDistributedFfytdActual =
      arrearsDistributedFfytdActual;
    entities.DashboardStagingPriority12.ArrearsDistrubutedFfytdRnk =
      arrearsDistrubutedFfytdRnk;
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
    var arrearsDistributedMonthActual =
      local.Local1.Item.G.ArrearsDistributedMonthActual ?? 0M;
    var arrearsDistributedMonthRnk =
      local.Local1.Item.G.ArrearsDistributedMonthRnk ?? 0;
    var arrearsDistributedFfytdActual =
      local.Local1.Item.G.ArrearsDistributedFfytdActual ?? 0M;
    var arrearsDistrubutedFfytdRnk =
      local.Local1.Item.G.ArrearsDistrubutedFfytdRnk ?? 0;

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
          command, "arrDistMthAct", arrearsDistributedMonthActual);
        db.
          SetNullableInt32(command, "arrDistMthRnk", arrearsDistributedMonthRnk);
        db.SetNullableDecimal(
          command, "arrDistYtdAct", arrearsDistributedFfytdActual);
        db.
          SetNullableInt32(command, "arrDistYtdRnk", arrearsDistrubutedFfytdRnk);
        db.SetNullableDecimal(command, "casPerFteDen", param);
        db.SetNullableString(command, "contractorNum", "");
      });

    entities.DashboardStagingPriority12.ReportMonth = reportMonth;
    entities.DashboardStagingPriority12.ReportLevel = reportLevel;
    entities.DashboardStagingPriority12.ReportLevelId = reportLevelId;
    entities.DashboardStagingPriority12.AsOfDate = asOfDate;
    entities.DashboardStagingPriority12.ArrearsDistributedMonthActual =
      arrearsDistributedMonthActual;
    entities.DashboardStagingPriority12.ArrearsDistributedMonthRnk =
      arrearsDistributedMonthRnk;
    entities.DashboardStagingPriority12.ArrearsDistributedFfytdActual =
      arrearsDistributedFfytdActual;
    entities.DashboardStagingPriority12.ArrearsDistrubutedFfytdRnk =
      arrearsDistrubutedFfytdRnk;
    entities.DashboardStagingPriority12.ContractorNumber = "";
    entities.DashboardStagingPriority12.Populated = true;
  }

  private void CreateDashboardStagingPriority3()
  {
    var reportMonth = local.Statewide.ReportMonth;
    var reportLevel = local.Statewide.ReportLevel;
    var reportLevelId = local.Statewide.ReportLevelId;
    var asOfDate = local.Statewide.AsOfDate;
    var param = 0M;
    var arrearsDistributedMonthActual =
      local.Statewide.ArrearsDistributedMonthActual ?? 0M;
    var arrearsDistributedMonthRnk =
      local.Statewide.ArrearsDistributedMonthRnk ?? 0;
    var arrearsDistributedFfytdActual =
      local.Statewide.ArrearsDistributedFfytdActual ?? 0M;
    var arrearsDistrubutedFfytdRnk =
      local.Statewide.ArrearsDistrubutedFfytdRnk ?? 0;

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
          command, "arrDistMthAct", arrearsDistributedMonthActual);
        db.
          SetNullableInt32(command, "arrDistMthRnk", arrearsDistributedMonthRnk);
        db.SetNullableDecimal(
          command, "arrDistYtdAct", arrearsDistributedFfytdActual);
        db.
          SetNullableInt32(command, "arrDistYtdRnk", arrearsDistrubutedFfytdRnk);
        db.SetNullableDecimal(command, "casPerFteDen", param);
        db.SetNullableString(command, "contractorNum", "");
      });

    entities.DashboardStagingPriority12.ReportMonth = reportMonth;
    entities.DashboardStagingPriority12.ReportLevel = reportLevel;
    entities.DashboardStagingPriority12.ReportLevelId = reportLevelId;
    entities.DashboardStagingPriority12.AsOfDate = asOfDate;
    entities.DashboardStagingPriority12.ArrearsDistributedMonthActual =
      arrearsDistributedMonthActual;
    entities.DashboardStagingPriority12.ArrearsDistributedMonthRnk =
      arrearsDistributedMonthRnk;
    entities.DashboardStagingPriority12.ArrearsDistributedFfytdActual =
      arrearsDistributedFfytdActual;
    entities.DashboardStagingPriority12.ArrearsDistrubutedFfytdRnk =
      arrearsDistrubutedFfytdRnk;
    entities.DashboardStagingPriority12.ContractorNumber = "";
    entities.DashboardStagingPriority12.Populated = true;
  }

  private void CreateDashboardStagingPriority4()
  {
    var reportMonth = local.Local1.Item.G.ReportMonth;
    var reportLevel = local.Local1.Item.G.ReportLevel;
    var reportLevelId = local.Local1.Item.G.ReportLevelId;
    var asOfDate = local.Local1.Item.G.AsOfDate;
    var param = 0M;
    var arrearsDistributedMonthActual =
      local.Local1.Item.G.ArrearsDistributedMonthActual ?? 0M;
    var arrearsDistributedMonthRnk =
      local.Local1.Item.G.ArrearsDistributedMonthRnk ?? 0;
    var arrearsDistributedFfytdActual =
      local.Local1.Item.G.ArrearsDistributedFfytdActual ?? 0M;
    var arrearsDistrubutedFfytdRnk =
      local.Local1.Item.G.ArrearsDistrubutedFfytdRnk ?? 0;

    entities.DashboardStagingPriority12.Populated = false;
    Update("CreateDashboardStagingPriority4",
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
          command, "arrDistMthAct", arrearsDistributedMonthActual);
        db.
          SetNullableInt32(command, "arrDistMthRnk", arrearsDistributedMonthRnk);
        db.SetNullableDecimal(
          command, "arrDistYtdAct", arrearsDistributedFfytdActual);
        db.
          SetNullableInt32(command, "arrDistYtdRnk", arrearsDistrubutedFfytdRnk);
        db.SetNullableDecimal(command, "casPerFteDen", param);
        db.SetNullableString(command, "contractorNum", "");
      });

    entities.DashboardStagingPriority12.ReportMonth = reportMonth;
    entities.DashboardStagingPriority12.ReportLevel = reportLevel;
    entities.DashboardStagingPriority12.ReportLevelId = reportLevelId;
    entities.DashboardStagingPriority12.AsOfDate = asOfDate;
    entities.DashboardStagingPriority12.ArrearsDistributedMonthActual =
      arrearsDistributedMonthActual;
    entities.DashboardStagingPriority12.ArrearsDistributedMonthRnk =
      arrearsDistributedMonthRnk;
    entities.DashboardStagingPriority12.ArrearsDistributedFfytdActual =
      arrearsDistributedFfytdActual;
    entities.DashboardStagingPriority12.ArrearsDistrubutedFfytdRnk =
      arrearsDistrubutedFfytdRnk;
    entities.DashboardStagingPriority12.ContractorNumber = "";
    entities.DashboardStagingPriority12.Populated = true;
  }

  private void CreateDashboardStagingPriority5()
  {
    var reportMonth = local.Contractor.Item.Gcontractor.ReportMonth;
    var reportLevel = local.Contractor.Item.Gcontractor.ReportLevel;
    var reportLevelId = local.Contractor.Item.Gcontractor.ReportLevelId;
    var asOfDate = local.Contractor.Item.Gcontractor.AsOfDate;
    var param = 0M;
    var arrearsDistributedMonthActual =
      local.Contractor.Item.Gcontractor.ArrearsDistributedMonthActual ?? 0M;
    var arrearsDistributedMonthRnk =
      local.Contractor.Item.Gcontractor.ArrearsDistributedMonthRnk ?? 0;
    var arrearsDistributedFfytdActual =
      local.Contractor.Item.Gcontractor.ArrearsDistributedFfytdActual ?? 0M;
    var arrearsDistrubutedFfytdRnk =
      local.Contractor.Item.Gcontractor.ArrearsDistrubutedFfytdRnk ?? 0;
    var contractorNumber =
      local.Contractor.Item.Gcontractor.ContractorNumber ?? "";

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
        db.SetNullableDecimal(command, "curSupPdMthNum", param);
        db.SetNullableDecimal(
          command, "arrDistMthAct", arrearsDistributedMonthActual);
        db.
          SetNullableInt32(command, "arrDistMthRnk", arrearsDistributedMonthRnk);
        db.SetNullableDecimal(
          command, "arrDistYtdAct", arrearsDistributedFfytdActual);
        db.
          SetNullableInt32(command, "arrDistYtdRnk", arrearsDistrubutedFfytdRnk);
        db.SetNullableDecimal(command, "casPerFteDen", param);
        db.SetNullableString(command, "contractorNum", contractorNumber);
      });

    entities.DashboardStagingPriority12.ReportMonth = reportMonth;
    entities.DashboardStagingPriority12.ReportLevel = reportLevel;
    entities.DashboardStagingPriority12.ReportLevelId = reportLevelId;
    entities.DashboardStagingPriority12.AsOfDate = asOfDate;
    entities.DashboardStagingPriority12.ArrearsDistributedMonthActual =
      arrearsDistributedMonthActual;
    entities.DashboardStagingPriority12.ArrearsDistributedMonthRnk =
      arrearsDistributedMonthRnk;
    entities.DashboardStagingPriority12.ArrearsDistributedFfytdActual =
      arrearsDistributedFfytdActual;
    entities.DashboardStagingPriority12.ArrearsDistrubutedFfytdRnk =
      arrearsDistrubutedFfytdRnk;
    entities.DashboardStagingPriority12.ContractorNumber = contractorNumber;
    entities.DashboardStagingPriority12.Populated = true;
  }

  private IEnumerable<bool> ReadCollectionCsePersonCsePerson1()
  {
    return ReadEachInSeparateTransaction("ReadCollectionCsePersonCsePerson1",
      (db, command) =>
      {
        db.
          SetDateTime(command, "createdTmst1", local.ReportStartDate.Timestamp);
        db.SetDateTime(command, "createdTmst2", local.ReportEndDate.Timestamp);
        db.SetDate(command, "collAdjDt", local.ReportEndDate.Date);
        db.SetDate(command, "date", local.ReportStartDate.Date);
        db.SetInt32(
          command, "collId", local.RestartCollection.SystemGeneratedIdentifier);
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
        entities.Collection.CourtOrderAppliedTo =
          db.GetNullableString(reader, 19);
        entities.Supp.Number = db.GetString(reader, 20);
        entities.Ap.Number = db.GetString(reader, 21);
        entities.Collection.Populated = true;
        entities.Supp.Populated = db.GetNullableString(reader, 20) != null;
        entities.Ap.Populated = db.GetNullableString(reader, 21) != null;
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
        entities.Supp.Populated = false;
        entities.Collection.Populated = false;
        entities.Ap.Populated = false;
      });
  }

  private IEnumerable<bool> ReadCollectionCsePersonCsePerson2()
  {
    return ReadEachInSeparateTransaction("ReadCollectionCsePersonCsePerson2",
      (db, command) =>
      {
        db.
          SetDateTime(command, "createdTmst1", local.ReportStartDate.Timestamp);
        db.SetDateTime(command, "createdTmst2", local.ReportEndDate.Timestamp);
        db.SetDate(command, "collAdjDt", local.ReportEndDate.Date);
        db.SetDate(command, "date", local.ReportStartDate.Date);
        db.SetInt32(
          command, "collId", local.RestartCollection.SystemGeneratedIdentifier);
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
        entities.Collection.CourtOrderAppliedTo =
          db.GetNullableString(reader, 19);
        entities.Supp.Number = db.GetString(reader, 20);
        entities.Ap.Number = db.GetString(reader, 21);
        entities.Collection.Populated = true;
        entities.Supp.Populated = db.GetNullableString(reader, 20) != null;
        entities.Ap.Populated = db.GetNullableString(reader, 21) != null;
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
        entities.Supp.Populated = false;
        entities.Collection.Populated = false;
        entities.Ap.Populated = false;
      });
  }

  private IEnumerable<bool>
    ReadCollectionCsePersonCsePersonDisbursementTransaction()
  {
    return ReadEach("ReadCollectionCsePersonCsePersonDisbursementTransaction",
      (db, command) =>
      {
        db.SetDateTime(command, "createdTmst", import.ReportEndDate.Timestamp);
        db.SetNullableInt32(
          command, "prqGeneratedId",
          entities.PaymentRequest.SystemGeneratedIdentifier);
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
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 18);
        entities.Collection.CourtOrderAppliedTo =
          db.GetNullableString(reader, 19);
        entities.Supp.Number = db.GetString(reader, 20);
        entities.Credit.CpaType = db.GetString(reader, 21);
        entities.Credit.CspNumber = db.GetString(reader, 22);
        entities.Credit.SystemGeneratedIdentifier = db.GetInt32(reader, 23);
        entities.Credit.Type1 = db.GetString(reader, 24);
        entities.Credit.Amount = db.GetDecimal(reader, 25);

        if (AsChar(entities.Credit.Type1) == 'C')
        {
          entities.Credit.ColId = db.GetNullableInt32(reader, 0);
          entities.Credit.CrtId = db.GetNullableInt32(reader, 5);
          entities.Credit.CstId = db.GetNullableInt32(reader, 6);
          entities.Credit.CrvId = db.GetNullableInt32(reader, 7);
          entities.Credit.CrdId = db.GetNullableInt32(reader, 8);
          entities.Credit.ObgId = db.GetNullableInt32(reader, 9);
          entities.Credit.CspNumberDisb = db.GetNullableString(reader, 10);
          entities.Credit.CpaTypeDisb = db.GetNullableString(reader, 11);
          entities.Credit.OtrId = db.GetNullableInt32(reader, 12);
          entities.Credit.OtrTypeDisb = db.GetNullableString(reader, 13);
          entities.Credit.OtyId = db.GetNullableInt32(reader, 14);
        }
        else
        {
          entities.Credit.ColId = null;
          entities.Credit.CrtId = null;
          entities.Credit.CstId = null;
          entities.Credit.CrvId = null;
          entities.Credit.CrdId = null;
          entities.Credit.ObgId = null;
          entities.Credit.CspNumberDisb = null;
          entities.Credit.CpaTypeDisb = null;
          entities.Credit.OtrId = null;
          entities.Credit.OtrTypeDisb = null;
          entities.Credit.OtyId = null;
        }

        entities.Collection.Populated = true;
        entities.Supp.Populated = true;
        entities.Ap.Populated = true;
        entities.Credit.Populated = true;
        CheckValid<Collection>("AppliedToCode",
          entities.Collection.AppliedToCode);
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("ConcurrentInd",
          entities.Collection.ConcurrentInd);
        CheckValid<Collection>("ProgramAppliedTo",
          entities.Collection.ProgramAppliedTo);
        CheckValid<DisbursementTransaction>("Type1", entities.Credit.Type1);

        return true;
      },
      () =>
      {
        entities.Credit.Populated = false;
        entities.Supp.Populated = false;
        entities.Collection.Populated = false;
        entities.Ap.Populated = false;
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
        entities.DashboardStagingPriority12.ArrearsDistributedMonthActual =
          db.GetNullableDecimal(reader, 4);
        entities.DashboardStagingPriority12.ArrearsDistributedMonthRnk =
          db.GetNullableInt32(reader, 5);
        entities.DashboardStagingPriority12.ArrearsDistributedFfytdActual =
          db.GetNullableDecimal(reader, 6);
        entities.DashboardStagingPriority12.ArrearsDistrubutedFfytdRnk =
          db.GetNullableInt32(reader, 7);
        entities.DashboardStagingPriority12.ContractorNumber =
          db.GetNullableString(reader, 8);
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
        entities.DashboardStagingPriority12.ArrearsDistributedMonthActual =
          db.GetNullableDecimal(reader, 4);
        entities.DashboardStagingPriority12.ArrearsDistributedMonthRnk =
          db.GetNullableInt32(reader, 5);
        entities.DashboardStagingPriority12.ArrearsDistributedFfytdActual =
          db.GetNullableDecimal(reader, 6);
        entities.DashboardStagingPriority12.ArrearsDistrubutedFfytdRnk =
          db.GetNullableInt32(reader, 7);
        entities.DashboardStagingPriority12.ContractorNumber =
          db.GetNullableString(reader, 8);
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
        entities.DashboardStagingPriority12.ArrearsDistributedMonthActual =
          db.GetNullableDecimal(reader, 4);
        entities.DashboardStagingPriority12.ArrearsDistributedMonthRnk =
          db.GetNullableInt32(reader, 5);
        entities.DashboardStagingPriority12.ArrearsDistributedFfytdActual =
          db.GetNullableDecimal(reader, 6);
        entities.DashboardStagingPriority12.ArrearsDistrubutedFfytdRnk =
          db.GetNullableInt32(reader, 7);
        entities.DashboardStagingPriority12.ContractorNumber =
          db.GetNullableString(reader, 8);
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
        entities.DashboardStagingPriority12.ArrearsDistributedMonthActual =
          db.GetNullableDecimal(reader, 4);
        entities.DashboardStagingPriority12.ArrearsDistributedMonthRnk =
          db.GetNullableInt32(reader, 5);
        entities.DashboardStagingPriority12.ArrearsDistributedFfytdActual =
          db.GetNullableDecimal(reader, 6);
        entities.DashboardStagingPriority12.ArrearsDistrubutedFfytdRnk =
          db.GetNullableInt32(reader, 7);
        entities.DashboardStagingPriority12.ContractorNumber =
          db.GetNullableString(reader, 8);
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
        entities.DashboardStagingPriority12.ArrearsDistributedMonthActual =
          db.GetNullableDecimal(reader, 4);
        entities.DashboardStagingPriority12.ArrearsDistributedMonthRnk =
          db.GetNullableInt32(reader, 5);
        entities.DashboardStagingPriority12.ArrearsDistributedFfytdActual =
          db.GetNullableDecimal(reader, 6);
        entities.DashboardStagingPriority12.ArrearsDistrubutedFfytdRnk =
          db.GetNullableInt32(reader, 7);
        entities.DashboardStagingPriority12.ContractorNumber =
          db.GetNullableString(reader, 8);
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
        entities.DashboardStagingPriority12.ArrearsDistributedMonthActual =
          db.GetNullableDecimal(reader, 4);
        entities.DashboardStagingPriority12.ArrearsDistributedMonthRnk =
          db.GetNullableInt32(reader, 5);
        entities.DashboardStagingPriority12.ArrearsDistributedFfytdActual =
          db.GetNullableDecimal(reader, 6);
        entities.DashboardStagingPriority12.ArrearsDistrubutedFfytdRnk =
          db.GetNullableInt32(reader, 7);
        entities.DashboardStagingPriority12.ContractorNumber =
          db.GetNullableString(reader, 8);
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
        entities.DashboardStagingPriority12.ArrearsDistributedMonthActual =
          db.GetNullableDecimal(reader, 4);
        entities.DashboardStagingPriority12.ArrearsDistributedMonthRnk =
          db.GetNullableInt32(reader, 5);
        entities.DashboardStagingPriority12.ArrearsDistributedFfytdActual =
          db.GetNullableDecimal(reader, 6);
        entities.DashboardStagingPriority12.ArrearsDistrubutedFfytdRnk =
          db.GetNullableInt32(reader, 7);
        entities.DashboardStagingPriority12.ContractorNumber =
          db.GetNullableString(reader, 8);
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
        entities.DashboardStagingPriority12.ArrearsDistributedMonthActual =
          db.GetNullableDecimal(reader, 4);
        entities.DashboardStagingPriority12.ArrearsDistributedMonthRnk =
          db.GetNullableInt32(reader, 5);
        entities.DashboardStagingPriority12.ArrearsDistributedFfytdActual =
          db.GetNullableDecimal(reader, 6);
        entities.DashboardStagingPriority12.ArrearsDistrubutedFfytdRnk =
          db.GetNullableInt32(reader, 7);
        entities.DashboardStagingPriority12.ContractorNumber =
          db.GetNullableString(reader, 8);
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
        entities.DashboardStagingPriority12.ArrearsDistributedMonthActual =
          db.GetNullableDecimal(reader, 4);
        entities.DashboardStagingPriority12.ArrearsDistributedMonthRnk =
          db.GetNullableInt32(reader, 5);
        entities.DashboardStagingPriority12.ArrearsDistributedFfytdActual =
          db.GetNullableDecimal(reader, 6);
        entities.DashboardStagingPriority12.ArrearsDistrubutedFfytdRnk =
          db.GetNullableInt32(reader, 7);
        entities.DashboardStagingPriority12.ContractorNumber =
          db.GetNullableString(reader, 8);
        entities.DashboardStagingPriority12.Populated = true;

        return true;
      },
      () =>
      {
        entities.DashboardStagingPriority12.Populated = false;
      });
  }

  private IEnumerable<bool> ReadPaymentRequest()
  {
    return ReadEachInSeparateTransaction("ReadPaymentRequest",
      (db, command) =>
      {
        db.SetDateTime(command, "timestamp1", local.ReportStartDate.Timestamp);
        db.SetDateTime(command, "timestamp2", local.ReportEndDate.Timestamp);
        db.SetInt32(
          command, "paymentRequestId",
          local.RestartPaymentRequest.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.PaymentRequest.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.PaymentRequest.ProcessDate = db.GetDate(reader, 1);
        entities.PaymentRequest.Amount = db.GetDecimal(reader, 2);
        entities.PaymentRequest.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.PaymentRequest.Classification = db.GetString(reader, 4);
        entities.PaymentRequest.Type1 = db.GetString(reader, 5);
        entities.PaymentRequest.PrqRGeneratedId =
          db.GetNullableInt32(reader, 6);
        entities.PaymentRequest.InterstateInd = db.GetNullableString(reader, 7);
        entities.PaymentRequest.RecoupmentIndKpc =
          db.GetNullableString(reader, 8);
        entities.PaymentRequest.Populated = true;
        CheckValid<PaymentRequest>("Type1", entities.PaymentRequest.Type1);

        return true;
      },
      () =>
      {
        entities.PaymentRequest.Populated = false;
      });
  }

  private void UpdateDashboardStagingPriority1()
  {
    var asOfDate = local.Statewide.AsOfDate;
    var arrearsDistributedMonthActual =
      local.Statewide.ArrearsDistributedMonthActual ?? 0M;
    var arrearsDistributedMonthRnk =
      local.Statewide.ArrearsDistributedMonthRnk ?? 0;
    var arrearsDistributedFfytdActual =
      local.Statewide.ArrearsDistributedFfytdActual ?? 0M;
    var arrearsDistrubutedFfytdRnk =
      local.Statewide.ArrearsDistrubutedFfytdRnk ?? 0;

    entities.DashboardStagingPriority12.Populated = false;
    Update("UpdateDashboardStagingPriority1",
      (db, command) =>
      {
        db.SetNullableDate(command, "asOfDt", asOfDate);
        db.SetNullableDecimal(
          command, "arrDistMthAct", arrearsDistributedMonthActual);
        db.
          SetNullableInt32(command, "arrDistMthRnk", arrearsDistributedMonthRnk);
        db.SetNullableDecimal(
          command, "arrDistYtdAct", arrearsDistributedFfytdActual);
        db.
          SetNullableInt32(command, "arrDistYtdRnk", arrearsDistrubutedFfytdRnk);
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
    entities.DashboardStagingPriority12.ArrearsDistributedMonthActual =
      arrearsDistributedMonthActual;
    entities.DashboardStagingPriority12.ArrearsDistributedMonthRnk =
      arrearsDistributedMonthRnk;
    entities.DashboardStagingPriority12.ArrearsDistributedFfytdActual =
      arrearsDistributedFfytdActual;
    entities.DashboardStagingPriority12.ArrearsDistrubutedFfytdRnk =
      arrearsDistrubutedFfytdRnk;
    entities.DashboardStagingPriority12.Populated = true;
  }

  private void UpdateDashboardStagingPriority2()
  {
    var asOfDate = local.Local1.Item.G.AsOfDate;
    var arrearsDistributedMonthActual =
      local.Local1.Item.G.ArrearsDistributedMonthActual ?? 0M;
    var arrearsDistributedMonthRnk =
      local.Local1.Item.G.ArrearsDistributedMonthRnk ?? 0;
    var arrearsDistributedFfytdActual =
      local.Local1.Item.G.ArrearsDistributedFfytdActual ?? 0M;
    var arrearsDistrubutedFfytdRnk =
      local.Local1.Item.G.ArrearsDistrubutedFfytdRnk ?? 0;

    entities.DashboardStagingPriority12.Populated = false;
    Update("UpdateDashboardStagingPriority2",
      (db, command) =>
      {
        db.SetNullableDate(command, "asOfDt", asOfDate);
        db.SetNullableDecimal(
          command, "arrDistMthAct", arrearsDistributedMonthActual);
        db.
          SetNullableInt32(command, "arrDistMthRnk", arrearsDistributedMonthRnk);
        db.SetNullableDecimal(
          command, "arrDistYtdAct", arrearsDistributedFfytdActual);
        db.
          SetNullableInt32(command, "arrDistYtdRnk", arrearsDistrubutedFfytdRnk);
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
    entities.DashboardStagingPriority12.ArrearsDistributedMonthActual =
      arrearsDistributedMonthActual;
    entities.DashboardStagingPriority12.ArrearsDistributedMonthRnk =
      arrearsDistributedMonthRnk;
    entities.DashboardStagingPriority12.ArrearsDistributedFfytdActual =
      arrearsDistributedFfytdActual;
    entities.DashboardStagingPriority12.ArrearsDistrubutedFfytdRnk =
      arrearsDistrubutedFfytdRnk;
    entities.DashboardStagingPriority12.Populated = true;
  }

  private void UpdateDashboardStagingPriority3()
  {
    var asOfDate = local.Statewide.AsOfDate;
    var arrearsDistributedMonthActual =
      local.Statewide.ArrearsDistributedMonthActual ?? 0M;
    var arrearsDistributedMonthRnk =
      local.Statewide.ArrearsDistributedMonthRnk ?? 0;
    var arrearsDistributedFfytdActual =
      local.Statewide.ArrearsDistributedFfytdActual ?? 0M;
    var arrearsDistrubutedFfytdRnk =
      local.Statewide.ArrearsDistrubutedFfytdRnk ?? 0;

    entities.DashboardStagingPriority12.Populated = false;
    Update("UpdateDashboardStagingPriority3",
      (db, command) =>
      {
        db.SetNullableDate(command, "asOfDt", asOfDate);
        db.SetNullableDecimal(
          command, "arrDistMthAct", arrearsDistributedMonthActual);
        db.
          SetNullableInt32(command, "arrDistMthRnk", arrearsDistributedMonthRnk);
        db.SetNullableDecimal(
          command, "arrDistYtdAct", arrearsDistributedFfytdActual);
        db.
          SetNullableInt32(command, "arrDistYtdRnk", arrearsDistrubutedFfytdRnk);
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
    entities.DashboardStagingPriority12.ArrearsDistributedMonthActual =
      arrearsDistributedMonthActual;
    entities.DashboardStagingPriority12.ArrearsDistributedMonthRnk =
      arrearsDistributedMonthRnk;
    entities.DashboardStagingPriority12.ArrearsDistributedFfytdActual =
      arrearsDistributedFfytdActual;
    entities.DashboardStagingPriority12.ArrearsDistrubutedFfytdRnk =
      arrearsDistrubutedFfytdRnk;
    entities.DashboardStagingPriority12.Populated = true;
  }

  private void UpdateDashboardStagingPriority4()
  {
    var asOfDate = local.Local1.Item.G.AsOfDate;
    var arrearsDistributedMonthActual =
      local.Local1.Item.G.ArrearsDistributedMonthActual ?? 0M;
    var arrearsDistributedMonthRnk =
      local.Local1.Item.G.ArrearsDistributedMonthRnk ?? 0;
    var arrearsDistributedFfytdActual =
      local.Local1.Item.G.ArrearsDistributedFfytdActual ?? 0M;
    var arrearsDistrubutedFfytdRnk =
      local.Local1.Item.G.ArrearsDistrubutedFfytdRnk ?? 0;

    entities.DashboardStagingPriority12.Populated = false;
    Update("UpdateDashboardStagingPriority4",
      (db, command) =>
      {
        db.SetNullableDate(command, "asOfDt", asOfDate);
        db.SetNullableDecimal(
          command, "arrDistMthAct", arrearsDistributedMonthActual);
        db.
          SetNullableInt32(command, "arrDistMthRnk", arrearsDistributedMonthRnk);
        db.SetNullableDecimal(
          command, "arrDistYtdAct", arrearsDistributedFfytdActual);
        db.
          SetNullableInt32(command, "arrDistYtdRnk", arrearsDistrubutedFfytdRnk);
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
    entities.DashboardStagingPriority12.ArrearsDistributedMonthActual =
      arrearsDistributedMonthActual;
    entities.DashboardStagingPriority12.ArrearsDistributedMonthRnk =
      arrearsDistributedMonthRnk;
    entities.DashboardStagingPriority12.ArrearsDistributedFfytdActual =
      arrearsDistributedFfytdActual;
    entities.DashboardStagingPriority12.ArrearsDistrubutedFfytdRnk =
      arrearsDistrubutedFfytdRnk;
    entities.DashboardStagingPriority12.Populated = true;
  }

  private void UpdateDashboardStagingPriority5()
  {
    var asOfDate = local.Contractor.Item.Gcontractor.AsOfDate;
    var arrearsDistributedMonthActual =
      local.Contractor.Item.Gcontractor.ArrearsDistributedMonthActual ?? 0M;
    var arrearsDistributedMonthRnk =
      local.Contractor.Item.Gcontractor.ArrearsDistributedMonthRnk ?? 0;
    var arrearsDistributedFfytdActual =
      local.Contractor.Item.Gcontractor.ArrearsDistributedFfytdActual ?? 0M;
    var arrearsDistrubutedFfytdRnk =
      local.Contractor.Item.Gcontractor.ArrearsDistrubutedFfytdRnk ?? 0;

    entities.DashboardStagingPriority12.Populated = false;
    Update("UpdateDashboardStagingPriority5",
      (db, command) =>
      {
        db.SetNullableDate(command, "asOfDt", asOfDate);
        db.SetNullableDecimal(
          command, "arrDistMthAct", arrearsDistributedMonthActual);
        db.
          SetNullableInt32(command, "arrDistMthRnk", arrearsDistributedMonthRnk);
        db.SetNullableDecimal(
          command, "arrDistYtdAct", arrearsDistributedFfytdActual);
        db.
          SetNullableInt32(command, "arrDistYtdRnk", arrearsDistrubutedFfytdRnk);
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
    entities.DashboardStagingPriority12.ArrearsDistributedMonthActual =
      arrearsDistributedMonthActual;
    entities.DashboardStagingPriority12.ArrearsDistributedMonthRnk =
      arrearsDistributedMonthRnk;
    entities.DashboardStagingPriority12.ArrearsDistributedFfytdActual =
      arrearsDistributedFfytdActual;
    entities.DashboardStagingPriority12.ArrearsDistrubutedFfytdRnk =
      arrearsDistrubutedFfytdRnk;
    entities.DashboardStagingPriority12.Populated = true;
  }

  private void UpdateDashboardStagingPriority6()
  {
    var arrearsDistributedMonthRnk = local.Temp.ArrearsDistributedMonthRnk ?? 0;

    entities.DashboardStagingPriority12.Populated = false;
    Update("UpdateDashboardStagingPriority6",
      (db, command) =>
      {
        db.
          SetNullableInt32(command, "arrDistMthRnk", arrearsDistributedMonthRnk);
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

    entities.DashboardStagingPriority12.ArrearsDistributedMonthRnk =
      arrearsDistributedMonthRnk;
    entities.DashboardStagingPriority12.Populated = true;
  }

  private void UpdateDashboardStagingPriority7()
  {
    var arrearsDistrubutedFfytdRnk = local.Temp.ArrearsDistrubutedFfytdRnk ?? 0;

    entities.DashboardStagingPriority12.Populated = false;
    Update("UpdateDashboardStagingPriority7",
      (db, command) =>
      {
        db.
          SetNullableInt32(command, "arrDistYtdRnk", arrearsDistrubutedFfytdRnk);
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

    entities.DashboardStagingPriority12.ArrearsDistrubutedFfytdRnk =
      arrearsDistrubutedFfytdRnk;
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
    /// A value of FiscalYearStart.
    /// </summary>
    public DateWorkArea FiscalYearStart
    {
      get => fiscalYearStart ??= new();
      set => fiscalYearStart = value;
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
    private DateWorkArea? reportStartDate;
    private DateWorkArea? reportEndDate;
    private DateWorkArea? fiscalYearStart;
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
    /// A value of Prev.
    /// </summary>
    public Collection Prev
    {
      get => prev ??= new();
      set => prev = value;
    }

    /// <summary>
    /// A value of RestartCollection.
    /// </summary>
    public Collection RestartCollection
    {
      get => restartCollection ??= new();
      set => restartCollection = value;
    }

    /// <summary>
    /// A value of PrevPeriod.
    /// </summary>
    public Common PrevPeriod
    {
      get => prevPeriod ??= new();
      set => prevPeriod = value;
    }

    /// <summary>
    /// A value of PeriodStart.
    /// </summary>
    public Common PeriodStart
    {
      get => periodStart ??= new();
      set => periodStart = value;
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
    /// A value of ReportingAbbreviation.
    /// </summary>
    public TextWorkArea ReportingAbbreviation
    {
      get => reportingAbbreviation ??= new();
      set => reportingAbbreviation = value;
    }

    /// <summary>
    /// A value of Period.
    /// </summary>
    public Common Period
    {
      get => period ??= new();
      set => period = value;
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

    /// <summary>
    /// A value of FiscalYear.
    /// </summary>
    public DateWorkArea FiscalYear
    {
      get => fiscalYear ??= new();
      set => fiscalYear = value;
    }

    /// <summary>
    /// A value of RestartPaymentRequest.
    /// </summary>
    public PaymentRequest RestartPaymentRequest
    {
      get => restartPaymentRequest ??= new();
      set => restartPaymentRequest = value;
    }

    private Collection? prev;
    private Collection? restartCollection;
    private Common? prevPeriod;
    private Common? periodStart;
    private DashboardAuditData? initialized;
    private ProgramCheckpointRestart? programCheckpointRestart;
    private DashboardStagingPriority12? statewide;
    private EabFileHandling? eabFileHandling;
    private EabReportSend? eabReportSend;
    private Array<LocalGroup>? local1;
    private DateWorkArea? reportStartDate;
    private DateWorkArea? reportEndDate;
    private TextWorkArea? reportingAbbreviation;
    private Common? period;
    private Common? recordsReadSinceCommit;
    private DashboardAuditData? dashboardAuditData;
    private DashboardStagingPriority12? temp;
    private Common? common;
    private DashboardStagingPriority12? prevRank;
    private CseOrganization? contractor1;
    private Array<ContractorGroup>? contractor;
    private DateWorkArea? fiscalYear;
    private PaymentRequest? restartPaymentRequest;
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
    /// A value of Credit.
    /// </summary>
    public DisbursementTransaction Credit
    {
      get => credit ??= new();
      set => credit = value;
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
    /// A value of PaymentRequest.
    /// </summary>
    public PaymentRequest PaymentRequest
    {
      get => paymentRequest ??= new();
      set => paymentRequest = value;
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
    /// A value of ObligationType.
    /// </summary>
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
    }

    private DisbursementTransactionRln? disbursementTransactionRln;
    private DisbursementTransaction? credit;
    private DisbursementTransaction? debit;
    private PaymentRequest? paymentRequest;
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
    private ObligationType? obligationType;
  }
#endregion
}
