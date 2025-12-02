// Program: FN_B734_DASHBOARD, ID: 945116410, model: 746.
// Short name: SWEF734B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B734_DASHBOARD.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch, Transaction = "SWEFB734")]
[Program("SWEF734B")]
public partial class FnB734Dashboard: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B734_DASHBOARD program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB734Dashboard(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB734Dashboard.
  /// </summary>
  public FnB734Dashboard(IContext context, Import import, Export export):
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
    // ---
    // 
    // ---
    // ---
    // C
    // S S    D a s h b o a r d
    // 
    // ---
    // ---
    // 
    // ---
    // ---------------------------------------------------------------------------------------------------
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
    // 02/17/17  GVandy	CQ56069		Restarts must use the processing date when the 
    // job
    // 					originally started.
    // 02/04/20  GVandy	CQ66220		Correlate with OCSE157 changes beginning in FY 
    // 2022.
    // 					These changes include only amounts in OCSE157
    // 					Lines 25, 27, and 29 that are both distributed
    // 					and disbursed.  Export a cutoff FY which defaults to
    // 					2022 but can be overridden with a code table value for testing.
    // ---------------------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    UseFnB734DashboardInitialization();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      UseEabExtractExitStateMessage();
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "Error returned from initialization: " + String
        (local.ExitStateWorkArea.Message, ExitStateWorkArea.Message_MaxLength);
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // ------------------------------------------------------------------------------
    // -- Refresh the Service Provider entries in the Dashboard name table.
    // ------------------------------------------------------------------------------
    foreach(var _ in ReadServiceProvider())
    {
      // -- Find the Dashboard name entry for this service provider
      if (ReadDashboardName1())
      {
        // -- Update the name of the service provider
        try
        {
          UpdateDashboardName1();

          continue;
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              local.EabReportSend.RptDetail =
                "Not Unique violation when updating Dashboard_Name entry for User ID " +
                String
                (entities.ServiceProvider.UserId,
                ServiceProvider.UserId_MaxLength);

              break;
            case ErrorCode.PermittedValueViolation:
              local.EabReportSend.RptDetail =
                "Permitted Value violation when updating Dashboard_Name entry for User ID " +
                String
                (entities.ServiceProvider.UserId,
                ServiceProvider.UserId_MaxLength);

              break;
            default:
              throw;
          }
        }
      }
      else
      {
        // -- Create a new entry for the service provider
        try
        {
          CreateDashboardName1();

          continue;
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              local.EabReportSend.RptDetail =
                "Already Exists violation when creating Dashboard_Name entry for User ID " +
                String
                (entities.ServiceProvider.UserId,
                ServiceProvider.UserId_MaxLength);

              break;
            case ErrorCode.PermittedValueViolation:
              local.EabReportSend.RptDetail =
                "Permitted Value violation when creating Dashboard_Name entry for User ID " +
                String
                (entities.ServiceProvider.UserId,
                ServiceProvider.UserId_MaxLength);

              break;
            default:
              throw;
          }
        }
      }

      local.EabFileHandling.Action = "WRITE";
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // ------------------------------------------------------------------------------
    // -- Refresh the Privatization Contractor Firm entries in the Dashboard 
    // name table.
    // ------------------------------------------------------------------------------
    foreach(var _ in ReadCseOrganization())
    {
      // -- Read each dashboard_name
      //    where provider_id = service_provider user_id
      //      and provider_type = 'CX'
      //    when sucessful
      //        Update dashboard_name
      //        set org_or_last_name to cse_organization name
      //        set first_name to spaces
      //        set middle_intitial to spaces
      //    when not found
      //        Create dashboard_name
      //        set provider_id to cse_organization code
      //        set provider_type to 'CX'
      //        set org_or_last_name to cse_organization name
      //        set first_name to spaces
      //        set middle_intitial to spaces
      local.DashboardName.ProviderId = entities.CseOrganization.Code;

      // -- Find the Dashboard name entry for this Privatization Contractor 
      // Firm.
      if (ReadDashboardName2())
      {
        // -- Update the name of the Privatization Contractor Firm
        try
        {
          UpdateDashboardName2();

          continue;
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              local.EabReportSend.RptDetail =
                "Not Unique violation when updating Dashboard_Name entry for Contractor ID " +
                String
                (entities.CseOrganization.Code, CseOrganization.Code_MaxLength);

              break;
            case ErrorCode.PermittedValueViolation:
              local.EabReportSend.RptDetail =
                "Permitted Value violation when updating Dashboard_Name entry for Contractor ID " +
                String
                (entities.CseOrganization.Code, CseOrganization.Code_MaxLength);

              break;
            default:
              throw;
          }
        }
      }
      else
      {
        // -- Create a new entry for the Privatization Contractor Firm
        try
        {
          CreateDashboardName2();

          continue;
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              local.EabReportSend.RptDetail =
                "Already Exists violation when creating Dashboard_Name entry for Contractor ID " +
                String
                (entities.CseOrganization.Code, CseOrganization.Code_MaxLength);

              break;
            case ErrorCode.PermittedValueViolation:
              local.EabReportSend.RptDetail =
                "Permitted Value violation when creating Dashboard_Name entry for Contractor ID " +
                String
                (entities.CseOrganization.Code, CseOrganization.Code_MaxLength);

              break;
            default:
              throw;
          }
        }
      }

      local.EabFileHandling.Action = "WRITE";
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    UseFnB734DeterContractorHistory();

    // ------------------------------------------------------------------------------
    // -- Take an initial checkpoint to save the Dashboard_Name Updates.
    // ------------------------------------------------------------------------------
    UseUpdateCheckpointRstAndCommit();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "Error taking initial checkpoint.";
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    if (AsChar(local.ProgramCheckpointRestart.RestartInd) == 'Y')
    {
      local.StartSubscript.Count = local.RestartGroupSubscript.Count;
    }
    else
    {
      local.StartSubscript.Count = 1;
    }

    // -- Process each report period (i.e. month or partial month) specified on 
    // PPI record.
    local.Local1.Index = local.StartSubscript.Count - 1;

    for(var limit = local.Local1.Count; local.Local1.Index < limit; ++
      local.Local1.Index)
    {
      if (!local.Local1.CheckSize())
      {
        break;
      }

      // -- Set report month to be passed to the priority driver cabs.
      local.DashboardAuditData.ReportMonth =
        local.Local1.Item.GlocalPeriodStart.YearMonth;

      // -------------------------------------------------------------------------------------
      //  Set global checkpoint info to pass to the priority driver cabs.
      //  The first 70 bytes were set in the initialization cab.
      //  Add the Report Group Suscript to the global checkpoint info.
      //  The remainder of the checkpoint info will be set in the individual
      //  cabs processing the priorities.
      //  Checkpoint Info... (first 55 bytes are the same as the PPI info)
      // 	Position  Description
      // 	--------  
      // -----------------------------------------
      // 	001-010   Last Run Date (yyyy-mm-dd)
      // 	011-011   Blank
      // 	012-019   Starting Priority  (format 9-99xxxx)
      // 	020-020   Blank
      // 	021-028   Ending Priority  (format 9-99xxxx)
      // 	029-029   Blank
      // 	030-039   Starting Report Date (yyyy-mm-dd)
      // 	040-040   Blank
      // 	041-050   Ending Report Date (yyyy-mm-dd)
      // 	051-051   Blank
      // 	052-054   Fiscal Year Designation ("FFY" or "SFY")
      // 	055-055   Audit Flag
      // 	056-058   Run Number
      // 	059-059   Blank
      // 	060-069   Original MPPI Process Date (yyyy-mm-dd)
      // 	070-070   Blank
      // 	071-072   Report Group Subscript
      // 	073-080   Blank
      // 	081-088   Priority
      // 	089-089   Blank
      // 	090-250   <priority specific restart info>
      // -------------------------------------------------------------------------------------
      if (IsEmpty(local.Restart.DashboardPriority))
      {
        local.ProgramCheckpointRestart.RestartInfo =
          Substring(local.ProgramCheckpointRestart.RestartInfo, 250, 1, 70) + NumberToString
          (local.Local1.Index + 1, 14, 2);
      }

      if (CharAt(local.Start.DashboardPriority, 1) <= '1' && CharAt
        (local.End.DashboardPriority, 1) >= '1' && CharAt
        (local.Restart.DashboardPriority, 1) <= '1')
      {
        // -------------------------------------------------------------------------------------
        // PRIORITY 1 - OCSE157 Federal Incentive Measures Reporting
        // -------------------------------------------------------------------------------------
        UseFnB734Pri1FederalIncentives();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          break;
        }
      }

      if (CharAt(local.Start.DashboardPriority, 1) <= '2' && CharAt
        (local.End.DashboardPriority, 1) >= '2' && CharAt
        (local.Restart.DashboardPriority, 1) <= '2')
      {
        // -------------------------------------------------------------------------------------
        // PRIORITY 2- Performance Measures by Judicial District
        // -------------------------------------------------------------------------------------
        UseFnB734Pri2PerformanceMetric();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          break;
        }
      }

      if (CharAt(local.Start.DashboardPriority, 1) <= '3' && CharAt
        (local.End.DashboardPriority, 1) >= '3' && CharAt
        (local.Restart.DashboardPriority, 1) <= '3')
      {
        // -------------------------------------------------------------------------------------
        // PRIORITY 3- Key Outputs/Metrics
        // -------------------------------------------------------------------------------------
        UseFnB734Pri3KeyOutputMetrics();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          break;
        }
      }

      if (CharAt(local.Start.DashboardPriority, 1) <= '4' && CharAt
        (local.End.DashboardPriority, 1) >= '4' && CharAt
        (local.Restart.DashboardPriority, 1) <= '4')
      {
        // -------------------------------------------------------------------------------------
        // Priority 4- Pyramid Report
        // -------------------------------------------------------------------------------------
        UseFnB734Pri4PyramidReport();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          break;
        }
      }

      if (CharAt(local.Start.DashboardPriority, 1) <= '5' && CharAt
        (local.End.DashboardPriority, 1) >= '5' && CharAt
        (local.Restart.DashboardPriority, 1) <= '5')
      {
        // -------------------------------------------------------------------------------------
        // Priority 5- Individual Worker and Team Performance
        // -------------------------------------------------------------------------------------
        local.ScriptCount.Count = local.Local1.Index + 1;
        UseFnB734Pri5WorkerAndTeam();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          break;
        }
      }

      // ------------------------------------------------------------------------------
      // -- Set the restart priority value to spaces.  We may have restarted in 
      // a specific
      // -- priority and we don't want this skip any priorities during the next 
      // reporting
      // -- period.
      // ------------------------------------------------------------------------------
      local.Restart.DashboardPriority = "";

      // ------------------------------------------------------------------------------
      // -- Clear data from production tables.
      // ------------------------------------------------------------------------------
      foreach(var _ in ReadDashboardPerformanceMetrics())
      {
        DeleteDashboardPerformanceMetrics();
      }

      foreach(var _ in ReadDashboardOutputMetrics())
      {
        DeleteDashboardOutputMetrics();
      }

      // ------------------------------------------------------------------------------
      // -- Copy staging records to production tables.
      // ------------------------------------------------------------------------------
      foreach(var _ in ReadDashboardStagingPriority12())
      {
        try
        {
          CreateDashboardPerformanceMetrics();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "DASHBOARD_PERFORMANCE_METRICS_AE";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "DASHBOARD_PERFORMANCE_METRICS_PV";

              break;
            default:
              throw;
          }
        }
      }

      foreach(var _ in ReadDashboardStagingPriority35())
      {
        try
        {
          CreateDashboardOutputMetrics();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "DASHBOARD_OUTPUT_METRICS_AE";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "DASHBOARD_OUTPUT_METRICS_PV";

              break;
            default:
              throw;
          }
        }
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        UseEabExtractExitStateMessage();
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail = local.ExitStateWorkArea.Message;
        UseCabErrorReport2();
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      // ------------------------------------------------------------------------------
      // -- Checkpoint after each reporting period.
      // ------------------------------------------------------------------------------
      local.ProgramCheckpointRestart.RestartInd = "Y";

      // -------------------------------------------------------------------------------------
      //  Checkpoint Info... (first 54 bytes are the same as the PPI info)
      // 	Position  Description
      // 	--------  
      // -----------------------------------------
      // 	001-010   Last Run Date (yyyy-mm-dd)
      // 	011-011   Blank
      // 	012-019   Starting Priority  (format 9-99xxxx)
      // 	020-020   Blank
      // 	021-028   Ending Priority  (format 9-99xxxx)
      // 	029-029   Blank
      // 	030-039   Starting Report Date (yyyy-mm-dd)
      // 	040-040   Blank
      // 	041-050   Ending Report Date (yyyy-mm-dd)
      // 	051-051   Blank
      // 	052-054   Fiscal Year Designation ("FFY" or "SFY")
      // 	055-055   Audit Flag
      // 	056-058   Run Number
      // 	059-059   Blank
      // 	060-069   Original MPPI Process Date (yyyy-mm-dd)
      // 	070-070   Blank
      // 	071-072   Report Group Subscript
      // 	073-080   Blank
      // 	081-088   Priority
      // 	089-089   Blank
      // 	090-250   <priority specific restart info>
      // -------------------------------------------------------------------------------------
      local.ProgramCheckpointRestart.RestartInfo =
        Substring(local.ProgramCheckpointRestart.RestartInfo, 250, 1, 70) + NumberToString
        (local.Local1.Index + 2, 14, 2);
      UseUpdateCheckpointRstAndCommit();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail = "Error taking checkpoint.";
        UseCabErrorReport2();
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }
    }

    local.Local1.CheckIndex();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      UseEabExtractExitStateMessage();
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "Error returned from Priority " + NumberToString
        (local.Local1.Index + 1, 14, 2) + ": " + String
        (local.ExitStateWorkArea.Message, ExitStateWorkArea.Message_MaxLength);
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // ------------------------------------------------------------------------------
    // -- Store the processing date in positions 001-010 of the PPI record.
    // -- On the next run we'll use this date as the last run date.
    // ------------------------------------------------------------------------------
    // -- Convert the PPI processing date to text.
    local.DateWorkArea.Date = local.ProgramProcessingInfo.ProcessDate;
    UseCabDate2TextWithHyphens();

    // -- Concat the PPI date into the first 10 bytes of the PPI parameter.
    local.ProgramProcessingInfo.ParameterList =
      String(local.TextWorkArea.Text10, TextWorkArea.Text10_MaxLength) + Substring
      (local.ProgramProcessingInfo.ParameterList, 11, 230);

    // -- Update parameter list.
    if (ReadProgramProcessingInfo())
    {
      try
      {
        UpdateProgramProcessingInfo();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "PROGRAM_PROCESSING_INFO_NU_AB";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "PROGRAM_PROCESSING_INFO_PV_AB";

            break;
          default:
            throw;
        }
      }
    }
    else
    {
      ExitState = "PROGRAM_PROCESSING_INFO_NF_AB";
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error adding PPI date to PPI parameter list as the last run date.  " +
        " ";
      UseCabErrorReport2();

      return;
    }

    // ------------------------------------------------------------------------------
    // -- Take a final checkpoint.
    // ------------------------------------------------------------------------------
    local.ProgramCheckpointRestart.RestartInd = "N";
    local.ProgramCheckpointRestart.RestartInfo = "";
    UseUpdateCheckpointRstAndCommit();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "Error taking final checkpoint.";
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // ------------------------------------------------------------------------------
    // -- Close the control report.
    // ------------------------------------------------------------------------------
    local.EabFileHandling.Action = "CLOSE";
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error closing control report.  Status = " + String
        (local.EabFileHandling.Status, EabFileHandling.Status_MaxLength);
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // ------------------------------------------------------------------------------
    // -- Close the error report.
    // ------------------------------------------------------------------------------
    local.EabFileHandling.Action = "CLOSE";
    UseCabErrorReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    ExitState = "ACO_NI0000_PROCESSING_COMPLETE";
  }

  private static void MoveDashboardAuditData(DashboardAuditData source,
    DashboardAuditData target)
  {
    target.ReportMonth = source.ReportMonth;
    target.RunNumber = source.RunNumber;
  }

  private static void MoveDateWorkArea(DateWorkArea source, DateWorkArea target)
  {
    target.Date = source.Date;
    target.Timestamp = source.Timestamp;
  }

  private static void MoveExport1ToLocal1(FnB734DashboardInitialization.Export.
    ExportGroup source, Local.LocalGroup target)
  {
    target.GlocalPeriodStart.Assign(source.GexportPeriodStart);
    MoveDateWorkArea(source.GexportPeriodEnd, target.GlocalPeriodEnd);
    MoveDateWorkArea(source.GexportFyStart, target.GlocalFyStart);
    MoveDateWorkArea(source.GexportFyEnd, target.GlocalFyEnd);
    target.GlocalAuditRecs.Flag = source.GexportAuditRec.Flag;
  }

  private static void MoveProgramProcessingInfo(ProgramProcessingInfo source,
    ProgramProcessingInfo target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ParameterList = source.ParameterList;
  }

  private void UseCabControlReport()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    context.Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabDate2TextWithHyphens()
  {
    var useImport = new CabDate2TextWithHyphens.Import();
    var useExport = new CabDate2TextWithHyphens.Export();

    useImport.DateWorkArea.Date = local.DateWorkArea.Date;

    context.Call(CabDate2TextWithHyphens.Execute, useImport, useExport);

    local.TextWorkArea.Text10 = useExport.TextWorkArea.Text10;
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    context.Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    context.Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseEabExtractExitStateMessage()
  {
    var useImport = new EabExtractExitStateMessage.Import();
    var useExport = new EabExtractExitStateMessage.Export();

    useExport.ExitStateWorkArea.Message = local.ExitStateWorkArea.Message;

    context.Call(EabExtractExitStateMessage.Execute, useImport, useExport);

    local.ExitStateWorkArea.Message = useExport.ExitStateWorkArea.Message;
  }

  private void UseFnB734DashboardInitialization()
  {
    var useImport = new FnB734DashboardInitialization.Import();
    var useExport = new FnB734DashboardInitialization.Export();

    context.Call(FnB734DashboardInitialization.Execute, useImport, useExport);

    local.RestartGroupSubscript.Count = useExport.RestartGroupSubscript.Count;
    local.Restart.DashboardPriority = useExport.Restart.DashboardPriority;
    local.DashboardAuditData.RunNumber = useExport.DashboardAuditData.RunNumber;
    local.Start.DashboardPriority = useExport.Start.DashboardPriority;
    local.End.DashboardPriority = useExport.End.DashboardPriority;
    local.ProgramCheckpointRestart.Assign(useExport.ProgramCheckpointRestart);
    local.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
    useExport.Export1.CopyTo(local.Local1, MoveExport1ToLocal1);
    local.Cq66220EffectiveFy.FiscalYear =
      useExport.Cq66220EffectiveFy.FiscalYear;
  }

  private void UseFnB734DeterContractorHistory()
  {
    var useImport = new FnB734DeterContractorHistory.Import();
    var useExport = new FnB734DeterContractorHistory.Export();

    MoveProgramProcessingInfo(local.ProgramProcessingInfo,
      useImport.ProgramProcessingInfo);

    context.Call(FnB734DeterContractorHistory.Execute, useImport, useExport);
  }

  private void UseFnB734Pri1FederalIncentives()
  {
    var useImport = new FnB734Pri1FederalIncentives.Import();
    var useExport = new FnB734Pri1FederalIncentives.Export();

    useImport.Restart.DashboardPriority = local.Restart.DashboardPriority;
    MoveDashboardAuditData(local.DashboardAuditData,
      useImport.DashboardAuditData);
    useImport.Start.DashboardPriority = local.Start.DashboardPriority;
    useImport.End.DashboardPriority = local.End.DashboardPriority;
    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);
    useImport.ProgramProcessingInfo.ProcessDate =
      local.ProgramProcessingInfo.ProcessDate;
    MoveDateWorkArea(local.Local1.Item.GlocalPeriodStart, useImport.PeriodStart);
    MoveDateWorkArea(local.Local1.Item.GlocalPeriodEnd, useImport.PeriodEnd);
    MoveDateWorkArea(local.Local1.Item.GlocalFyStart, useImport.FiscalYearStart);
    MoveDateWorkArea(local.Local1.Item.GlocalFyEnd, useImport.FiscalYearEnd);
    useImport.Cq66220EffectiveFy.FiscalYear =
      local.Cq66220EffectiveFy.FiscalYear;
    useImport.AuditRec.Flag = local.Local1.Item.GlocalAuditRecs.Flag;

    context.Call(FnB734Pri1FederalIncentives.Execute, useImport, useExport);
  }

  private void UseFnB734Pri2PerformanceMetric()
  {
    var useImport = new FnB734Pri2PerformanceMetric.Import();
    var useExport = new FnB734Pri2PerformanceMetric.Export();

    useImport.Restart.DashboardPriority = local.Restart.DashboardPriority;
    MoveDashboardAuditData(local.DashboardAuditData,
      useImport.DashboardAuditData);
    useImport.Start.DashboardPriority = local.Start.DashboardPriority;
    useImport.End.DashboardPriority = local.End.DashboardPriority;
    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);
    useImport.ProgramProcessingInfo.ProcessDate =
      local.ProgramProcessingInfo.ProcessDate;
    MoveDateWorkArea(local.Local1.Item.GlocalPeriodStart, useImport.PeriodStart);
    MoveDateWorkArea(local.Local1.Item.GlocalPeriodEnd, useImport.PeriodEnd);
    MoveDateWorkArea(local.Local1.Item.GlocalFyStart, useImport.FiscalYearStart);
    MoveDateWorkArea(local.Local1.Item.GlocalFyEnd, useImport.FiscalYearEnd);

    context.Call(FnB734Pri2PerformanceMetric.Execute, useImport, useExport);
  }

  private void UseFnB734Pri3KeyOutputMetrics()
  {
    var useImport = new FnB734Pri3KeyOutputMetrics.Import();
    var useExport = new FnB734Pri3KeyOutputMetrics.Export();

    useImport.Restart.DashboardPriority = local.Restart.DashboardPriority;
    useImport.StartDashboardAuditData.DashboardPriority =
      local.Start.DashboardPriority;
    useImport.End.DashboardPriority = local.End.DashboardPriority;
    useImport.ProgramProcessingInfo.ProcessDate =
      local.ProgramProcessingInfo.ProcessDate;
    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);
    MoveDateWorkArea(local.Local1.Item.GlocalPeriodEnd, useImport.PeriodEnd);
    MoveDateWorkArea(local.Local1.Item.GlocalPeriodStart,
      useImport.StartDateWorkArea);
    MoveDashboardAuditData(local.DashboardAuditData,
      useImport.DashboardAuditData);
    useImport.AuditFlag.Flag = local.Local1.Item.GlocalAuditRecs.Flag;

    context.Call(FnB734Pri3KeyOutputMetrics.Execute, useImport, useExport);
  }

  private void UseFnB734Pri4PyramidReport()
  {
    var useImport = new FnB734Pri4PyramidReport.Import();
    var useExport = new FnB734Pri4PyramidReport.Export();

    useImport.ProgramProcessingInfo.ProcessDate =
      local.ProgramProcessingInfo.ProcessDate;
    MoveDashboardAuditData(local.DashboardAuditData,
      useImport.DashboardAuditData);
    MoveDateWorkArea(local.Local1.Item.GlocalPeriodStart,
      useImport.ReportStartDate);
    MoveDateWorkArea(local.Local1.Item.GlocalPeriodEnd, useImport.ReportEndDate);
    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);
    useImport.Restart.DashboardPriority = local.Restart.DashboardPriority;
    useImport.Start.DashboardPriority = local.Start.DashboardPriority;
    useImport.End.DashboardPriority = local.End.DashboardPriority;

    context.Call(FnB734Pri4PyramidReport.Execute, useImport, useExport);
  }

  private void UseFnB734Pri5WorkerAndTeam()
  {
    var useImport = new FnB734Pri5WorkerAndTeam.Import();
    var useExport = new FnB734Pri5WorkerAndTeam.Export();

    useImport.Restart.DashboardPriority = local.Restart.DashboardPriority;
    useImport.StartDashboardAuditData.DashboardPriority =
      local.Start.DashboardPriority;
    useImport.End.DashboardPriority = local.End.DashboardPriority;
    useImport.ProgramProcessingInfo.ProcessDate =
      local.ProgramProcessingInfo.ProcessDate;
    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);
    MoveDateWorkArea(local.Local1.Item.GlocalPeriodEnd, useImport.PeriodEnd);
    MoveDateWorkArea(local.Local1.Item.GlocalPeriodStart,
      useImport.StartDateWorkArea);
    MoveDashboardAuditData(local.DashboardAuditData,
      useImport.DashboardAuditData);
    useImport.ScriptCount.Count = local.ScriptCount.Count;
    useImport.AuditFlag.Flag = local.Local1.Item.GlocalAuditRecs.Flag;

    context.Call(FnB734Pri5WorkerAndTeam.Execute, useImport, useExport);
  }

  private void UseUpdateCheckpointRstAndCommit()
  {
    var useImport = new UpdateCheckpointRstAndCommit.Import();
    var useExport = new UpdateCheckpointRstAndCommit.Export();

    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);

    context.Call(UpdateCheckpointRstAndCommit.Execute, useImport, useExport);
  }

  private void CreateDashboardName1()
  {
    var providerId = entities.ServiceProvider.UserId;
    var providerType = "SP";
    var orgOrLastName = entities.ServiceProvider.LastName;
    var firstName = entities.ServiceProvider.FirstName;
    var middleInitial = entities.ServiceProvider.MiddleInitial;

    entities.DashboardName.Populated = false;
    Update("CreateDashboardName1",
      (db, command) =>
      {
        db.SetString(command, "providerId", providerId);
        db.SetString(command, "providerType", providerType);
        db.SetNullableString(command, "orgOrLastName", orgOrLastName);
        db.SetNullableString(command, "firstName", firstName);
        db.SetNullableString(command, "middleInitial", middleInitial);
      });

    entities.DashboardName.ProviderId = providerId;
    entities.DashboardName.ProviderType = providerType;
    entities.DashboardName.OrgOrLastName = orgOrLastName;
    entities.DashboardName.FirstName = firstName;
    entities.DashboardName.MiddleInitial = middleInitial;
    entities.DashboardName.Populated = true;
  }

  private void CreateDashboardName2()
  {
    var providerId = entities.CseOrganization.Code;
    var providerType = "CX";
    var orgOrLastName = entities.CseOrganization.Name;

    entities.DashboardName.Populated = false;
    Update("CreateDashboardName2",
      (db, command) =>
      {
        db.SetString(command, "providerId", providerId);
        db.SetString(command, "providerType", providerType);
        db.SetNullableString(command, "orgOrLastName", orgOrLastName);
        db.SetNullableString(command, "firstName", "");
        db.SetNullableString(command, "middleInitial", "");
      });

    entities.DashboardName.ProviderId = providerId;
    entities.DashboardName.ProviderType = providerType;
    entities.DashboardName.OrgOrLastName = orgOrLastName;
    entities.DashboardName.FirstName = "";
    entities.DashboardName.MiddleInitial = "";
    entities.DashboardName.Populated = true;
  }

  private void CreateDashboardOutputMetrics()
  {
    var reportMonth = entities.DashboardStagingPriority35.ReportMonth;
    var reportLevel = entities.DashboardStagingPriority35.ReportLevel;
    var reportLevelId = entities.DashboardStagingPriority35.ReportLevelId;
    var type1 = "DATA";
    var asOfDate = entities.DashboardStagingPriority35.AsOfDate;
    var casesWithEstReferral =
      entities.DashboardStagingPriority35.CasesWithEstReferral;
    var casesWithEnfReferral =
      entities.DashboardStagingPriority35.CasesWithEnfReferral;
    var fullTimeEquivalent =
      entities.DashboardStagingPriority35.FullTimeEquivalent;
    var newOrdersEstablished =
      entities.DashboardStagingPriority35.NewOrdersEstablished;
    var paternitiesEstablished =
      entities.DashboardStagingPriority35.PaternitiesEstablished;
    var casesOpenedWithOrder =
      entities.DashboardStagingPriority35.CasesOpenedWithOrder;
    var casesOpenedWithoutOrders =
      entities.DashboardStagingPriority35.CasesOpenedWithoutOrders;
    var casesClosedWithOrders =
      entities.DashboardStagingPriority35.CasesClosedWithOrders;
    var casesClosedWithoutOrders =
      entities.DashboardStagingPriority35.CasesClosedWithoutOrders;
    var modifications = entities.DashboardStagingPriority35.Modifications;
    var incomeWithholdingsIssued =
      entities.DashboardStagingPriority35.IncomeWithholdingsIssued;
    var contemptMotionFilings =
      entities.DashboardStagingPriority35.ContemptMotionFilings;
    var contemptOrderFilings =
      entities.DashboardStagingPriority35.ContemptOrderFilings;
    var stypeCollectionAmount =
      entities.DashboardStagingPriority35.StypeCollectionAmount;
    var stypePercentOfTotal =
      entities.DashboardStagingPriority35.StypePercentOfTotal;
    var ftypeCollectionAmount =
      entities.DashboardStagingPriority35.FtypeCollectionAmount;
    var ftypePercentOfTotal =
      entities.DashboardStagingPriority35.FtypePercentOfTotal;
    var itypeCollectionAmount =
      entities.DashboardStagingPriority35.ItypeCollectionAmount;
    var itypePercentOfTotal =
      entities.DashboardStagingPriority35.ItypePercentOfTotal;
    var utypeCollectionAmount =
      entities.DashboardStagingPriority35.UtypeCollectionAmount;
    var utypePercentOfTotal =
      entities.DashboardStagingPriority35.UtypePercentOfTotal;
    var ctypeCollectionAmount =
      entities.DashboardStagingPriority35.CtypeCollectionAmount;
    var ctypePercentOfTotal =
      entities.DashboardStagingPriority35.CtypePercentOfTotal;
    var totalCollectionAmount =
      entities.DashboardStagingPriority35.TotalCollectionAmount;
    var daysToOrderEstblshmntNumer =
      entities.DashboardStagingPriority35.DaysToOrderEstblshmntNumer;
    var daysToOrderEstblshmntDenom =
      entities.DashboardStagingPriority35.DaysToOrderEstblshmntDenom;
    var daysToOrderEstblshmntAvg =
      entities.DashboardStagingPriority35.DaysToOrderEstblshmntAvg;
    var daysToReturnOfSrvcNumer =
      entities.DashboardStagingPriority35.DaysToReturnOfSrvcNumer;
    var daysToReturnOfServiceDenom =
      entities.DashboardStagingPriority35.DaysToReturnOfServiceDenom;
    var daysToReturnOfServiceAvg =
      entities.DashboardStagingPriority35.DaysToReturnOfServiceAvg;
    var referralAging60To90Days =
      entities.DashboardStagingPriority35.ReferralAging60To90Days;
    var referralAging91To120Days =
      entities.DashboardStagingPriority35.ReferralAging91To120Days;
    var referralAging121To150Days =
      entities.DashboardStagingPriority35.ReferralAging121To150Days;
    var referralAging151PlusDays =
      entities.DashboardStagingPriority35.ReferralAging151PlusDays;
    var daysToIwoPaymentNumerator =
      entities.DashboardStagingPriority35.DaysToIwoPaymentNumerator;
    var daysToIwoPaymentDenominator =
      entities.DashboardStagingPriority35.DaysToIwoPaymentDenominator;
    var daysToIwoPaymentAverage =
      entities.DashboardStagingPriority35.DaysToIwoPaymentAverage;
    var referralsToLegalForEst =
      entities.DashboardStagingPriority35.ReferralsToLegalForEst;
    var referralsToLegalForEnf =
      entities.DashboardStagingPriority35.ReferralsToLegalForEnf;
    var caseloadCount = entities.DashboardStagingPriority35.CaseloadCount;
    var casesOpened = entities.DashboardStagingPriority35.CasesOpened;
    var ncpLocatesByAddress =
      entities.DashboardStagingPriority35.NcpLocatesByAddress;
    var ncpLocatesByEmployer =
      entities.DashboardStagingPriority35.NcpLocatesByEmployer;
    var caseClosures = entities.DashboardStagingPriority35.CaseClosures;
    var caseReviews = entities.DashboardStagingPriority35.CaseReviews;
    var petitions = entities.DashboardStagingPriority35.Petitions;
    var casesPayingArrearsDenominator =
      entities.DashboardStagingPriority35.CasesPayingArrearsDenominator;
    var casesPayingArrearsNumerator =
      entities.DashboardStagingPriority35.CasesPayingArrearsNumerator;
    var casesPayingArrearsPercent =
      entities.DashboardStagingPriority35.CasesPayingArrearsPercent;
    var casesPayingArrearsRank =
      entities.DashboardStagingPriority35.CasesPayingArrearsRank;
    var currentSupportPaidFfytdDen =
      entities.DashboardStagingPriority35.CurrentSupportPaidFfytdDen;
    var currentSupportPaidFfytdNum =
      entities.DashboardStagingPriority35.CurrentSupportPaidFfytdNum;
    var currentSupportPaidFfytdPer =
      entities.DashboardStagingPriority35.CurrentSupportPaidFfytdPer;
    var currentSupportPaidFfytdRnk =
      entities.DashboardStagingPriority35.CurrentSupportPaidFfytdRnk;
    var currentSupportPaidMthDen =
      entities.DashboardStagingPriority35.CurrentSupportPaidMthDen;
    var currentSupportPaidMthNum =
      entities.DashboardStagingPriority35.CurrentSupportPaidMthNum;
    var currentSupportPaidMthPer =
      entities.DashboardStagingPriority35.CurrentSupportPaidMthPer;
    var currentSupportPaidMthRnk =
      entities.DashboardStagingPriority35.CurrentSupportPaidMthRnk;

    entities.DashboardOutputMetrics.Populated = false;
    Update("CreateDashboardOutputMetrics",
      (db, command) =>
      {
        db.SetInt32(command, "reportMonth", reportMonth);
        db.SetString(command, "reportLevel", reportLevel);
        db.SetString(command, "reportLevelId", reportLevelId);
        db.SetString(command, "type", type1);
        db.SetNullableDate(command, "asOfDate", asOfDate);
        db.SetNullableInt32(command, "casWEstRef", casesWithEstReferral);
        db.SetNullableInt32(command, "casWEnfRef", casesWithEnfReferral);
        db.SetNullableDecimal(command, "fullTimeEqvlnt", fullTimeEquivalent);
        db.SetNullableInt32(command, "newOrdEst", newOrdersEstablished);
        db.SetNullableInt32(command, "paternitiesEst", paternitiesEstablished);
        db.SetNullableInt32(command, "casesOpnWOrder", casesOpenedWithOrder);
        db.
          SetNullableInt32(command, "casesOpnWoOrder", casesOpenedWithoutOrders);
        db.SetNullableInt32(command, "casesClsWOrder", casesClosedWithOrders);
        db.
          SetNullableInt32(command, "casesClsWoOrder", casesClosedWithoutOrders);
        db.SetNullableInt32(command, "modifications", modifications);
        db.SetNullableInt32(command, "iwIssued", incomeWithholdingsIssued);
        db.SetNullableInt32(command, "cntmptMtnFiled", contemptMotionFilings);
        db.SetNullableInt32(command, "cntmptOrdFiled", contemptOrderFilings);
        db.SetNullableDecimal(command, "STypeCollAmt", stypeCollectionAmount);
        db.SetNullableDecimal(command, "STypeCollPer", stypePercentOfTotal);
        db.SetNullableDecimal(command, "FTypeCollAmt", ftypeCollectionAmount);
        db.SetNullableDecimal(command, "FTypeCollPer", ftypePercentOfTotal);
        db.SetNullableDecimal(command, "ITypeCollAmt", itypeCollectionAmount);
        db.SetNullableDecimal(command, "ITypeCollPer", itypePercentOfTotal);
        db.SetNullableDecimal(command, "UTypeCollAmt", utypeCollectionAmount);
        db.SetNullableDecimal(command, "UTypeCollPer", utypePercentOfTotal);
        db.SetNullableDecimal(command, "CTypeCollAmt", ctypeCollectionAmount);
        db.SetNullableDecimal(command, "CTypeCollPer", ctypePercentOfTotal);
        db.SetNullableDecimal(command, "totalCollAmt", totalCollectionAmount);
        db.
          SetNullableInt32(command, "ordEstDaysNmr", daysToOrderEstblshmntNumer);
        db.SetNullableInt32(
          command, "ordEstDaysDnom", daysToOrderEstblshmntDenom);
        db.
          SetNullableDecimal(command, "ordEstDaysAvg", daysToOrderEstblshmntAvg);
        db.SetNullableInt32(command, "retServDaysNmr", daysToReturnOfSrvcNumer);
        db.SetNullableInt32(
          command, "retSrvDaysDnom", daysToReturnOfServiceDenom);
        db.SetNullableDecimal(
          command, "retServDaysAvg", daysToReturnOfServiceAvg);
        db.SetNullableInt32(command, "refAge60To90", referralAging60To90Days);
        db.SetNullableInt32(command, "refAge91To120", referralAging91To120Days);
        db.
          SetNullableInt32(command, "refAge121To150", referralAging121To150Days);
        db.SetNullableInt32(command, "refAge151Plus", referralAging151PlusDays);
        db.
          SetNullableInt32(command, "iwoPmtDaysNmr", daysToIwoPaymentNumerator);
        db.SetNullableInt32(
          command, "iwoPmtDaysDnom", daysToIwoPaymentDenominator);
        db.
          SetNullableDecimal(command, "iwoPmtDaysAvg", daysToIwoPaymentAverage);
        db.SetNullableInt32(command, "estRefToLegal", referralsToLegalForEst);
        db.SetNullableInt32(command, "enfRefToLegal", referralsToLegalForEnf);
        db.SetNullableInt32(command, "caseloadCount", caseloadCount);
        db.SetNullableInt32(command, "casesOpened", casesOpened);
        db.SetNullableInt32(command, "ncpLocByAdrss", ncpLocatesByAddress);
        db.SetNullableInt32(command, "ncpLocByEmp", ncpLocatesByEmployer);
        db.SetNullableInt32(command, "caseClosures", caseClosures);
        db.SetNullableInt32(command, "caseReviews", caseReviews);
        db.SetNullableInt32(command, "petitions", petitions);
        db.SetNullableInt32(
          command, "casPayingArrDen", casesPayingArrearsDenominator);
        db.SetNullableInt32(
          command, "casPayingArrNum", casesPayingArrearsNumerator);
        db.SetNullableDecimal(
          command, "casPayingArrPer", casesPayingArrearsPercent);
        db.SetNullableInt32(command, "casPayingArrRnk", casesPayingArrearsRank);
        db.SetNullableDecimal(
          command, "curSupPdYtdDen", currentSupportPaidFfytdDen);
        db.SetNullableDecimal(
          command, "curSupPdYtdNum", currentSupportPaidFfytdNum);
        db.SetNullableDecimal(
          command, "curSupPdYtdPer", currentSupportPaidFfytdPer);
        db.SetNullableInt32(
          command, "curSupPdYtdRnk", currentSupportPaidFfytdRnk);
        db.SetNullableDecimal(
          command, "curSupPdMthDen", currentSupportPaidMthDen);
        db.SetNullableDecimal(
          command, "curSupPdMthNum", currentSupportPaidMthNum);
        db.SetNullableDecimal(
          command, "curSupPdMthPer", currentSupportPaidMthPer);
        db.
          SetNullableInt32(command, "curSupPdMthRnk", currentSupportPaidMthRnk);
      });

    entities.DashboardOutputMetrics.ReportMonth = reportMonth;
    entities.DashboardOutputMetrics.ReportLevel = reportLevel;
    entities.DashboardOutputMetrics.ReportLevelId = reportLevelId;
    entities.DashboardOutputMetrics.Type1 = type1;
    entities.DashboardOutputMetrics.AsOfDate = asOfDate;
    entities.DashboardOutputMetrics.CasesWithEstReferral = casesWithEstReferral;
    entities.DashboardOutputMetrics.CasesWithEnfReferral = casesWithEnfReferral;
    entities.DashboardOutputMetrics.FullTimeEquivalent = fullTimeEquivalent;
    entities.DashboardOutputMetrics.NewOrdersEstablished = newOrdersEstablished;
    entities.DashboardOutputMetrics.PaternitiesEstablished =
      paternitiesEstablished;
    entities.DashboardOutputMetrics.CasesOpenedWithOrder = casesOpenedWithOrder;
    entities.DashboardOutputMetrics.CasesOpenedWithoutOrders =
      casesOpenedWithoutOrders;
    entities.DashboardOutputMetrics.CasesClosedWithOrders =
      casesClosedWithOrders;
    entities.DashboardOutputMetrics.CasesClosedWithoutOrders =
      casesClosedWithoutOrders;
    entities.DashboardOutputMetrics.Modifications = modifications;
    entities.DashboardOutputMetrics.IncomeWithholdingsIssued =
      incomeWithholdingsIssued;
    entities.DashboardOutputMetrics.ContemptMotionFilings =
      contemptMotionFilings;
    entities.DashboardOutputMetrics.ContemptOrderFilings = contemptOrderFilings;
    entities.DashboardOutputMetrics.StypeCollectionAmount =
      stypeCollectionAmount;
    entities.DashboardOutputMetrics.StypePercentOfTotal = stypePercentOfTotal;
    entities.DashboardOutputMetrics.FtypeCollectionAmount =
      ftypeCollectionAmount;
    entities.DashboardOutputMetrics.FtypePercentOfTotal = ftypePercentOfTotal;
    entities.DashboardOutputMetrics.ItypeCollectionAmount =
      itypeCollectionAmount;
    entities.DashboardOutputMetrics.ItypePercentOfTotal = itypePercentOfTotal;
    entities.DashboardOutputMetrics.UtypeCollectionAmount =
      utypeCollectionAmount;
    entities.DashboardOutputMetrics.UtypePercentOfTotal = utypePercentOfTotal;
    entities.DashboardOutputMetrics.CtypeCollectionAmount =
      ctypeCollectionAmount;
    entities.DashboardOutputMetrics.CtypePercentOfTotal = ctypePercentOfTotal;
    entities.DashboardOutputMetrics.TotalCollectionAmount =
      totalCollectionAmount;
    entities.DashboardOutputMetrics.DaysToOrderEstblshmntNumer =
      daysToOrderEstblshmntNumer;
    entities.DashboardOutputMetrics.DaysToOrderEstblshmntDenom =
      daysToOrderEstblshmntDenom;
    entities.DashboardOutputMetrics.DaysToOrderEstblshmntAvg =
      daysToOrderEstblshmntAvg;
    entities.DashboardOutputMetrics.DaysToReturnOfSrvcNumer =
      daysToReturnOfSrvcNumer;
    entities.DashboardOutputMetrics.DaysToReturnOfServiceDenom =
      daysToReturnOfServiceDenom;
    entities.DashboardOutputMetrics.DaysToReturnOfServiceAvg =
      daysToReturnOfServiceAvg;
    entities.DashboardOutputMetrics.ReferralAging60To90Days =
      referralAging60To90Days;
    entities.DashboardOutputMetrics.ReferralAging91To120Days =
      referralAging91To120Days;
    entities.DashboardOutputMetrics.ReferralAging121To150Days =
      referralAging121To150Days;
    entities.DashboardOutputMetrics.ReferralAging151PlusDays =
      referralAging151PlusDays;
    entities.DashboardOutputMetrics.DaysToIwoPaymentNumerator =
      daysToIwoPaymentNumerator;
    entities.DashboardOutputMetrics.DaysToIwoPaymentDenominator =
      daysToIwoPaymentDenominator;
    entities.DashboardOutputMetrics.DaysToIwoPaymentAverage =
      daysToIwoPaymentAverage;
    entities.DashboardOutputMetrics.ReferralsToLegalForEst =
      referralsToLegalForEst;
    entities.DashboardOutputMetrics.ReferralsToLegalForEnf =
      referralsToLegalForEnf;
    entities.DashboardOutputMetrics.CaseloadCount = caseloadCount;
    entities.DashboardOutputMetrics.CasesOpened = casesOpened;
    entities.DashboardOutputMetrics.NcpLocatesByAddress = ncpLocatesByAddress;
    entities.DashboardOutputMetrics.NcpLocatesByEmployer = ncpLocatesByEmployer;
    entities.DashboardOutputMetrics.CaseClosures = caseClosures;
    entities.DashboardOutputMetrics.CaseReviews = caseReviews;
    entities.DashboardOutputMetrics.Petitions = petitions;
    entities.DashboardOutputMetrics.CasesPayingArrearsDenominator =
      casesPayingArrearsDenominator;
    entities.DashboardOutputMetrics.CasesPayingArrearsNumerator =
      casesPayingArrearsNumerator;
    entities.DashboardOutputMetrics.CasesPayingArrearsPercent =
      casesPayingArrearsPercent;
    entities.DashboardOutputMetrics.CasesPayingArrearsRank =
      casesPayingArrearsRank;
    entities.DashboardOutputMetrics.CurrentSupportPaidFfytdDen =
      currentSupportPaidFfytdDen;
    entities.DashboardOutputMetrics.CurrentSupportPaidFfytdNum =
      currentSupportPaidFfytdNum;
    entities.DashboardOutputMetrics.CurrentSupportPaidFfytdPer =
      currentSupportPaidFfytdPer;
    entities.DashboardOutputMetrics.CurrentSupportPaidFfytdRnk =
      currentSupportPaidFfytdRnk;
    entities.DashboardOutputMetrics.CurrentSupportPaidMthDen =
      currentSupportPaidMthDen;
    entities.DashboardOutputMetrics.CurrentSupportPaidMthNum =
      currentSupportPaidMthNum;
    entities.DashboardOutputMetrics.CurrentSupportPaidMthPer =
      currentSupportPaidMthPer;
    entities.DashboardOutputMetrics.CurrentSupportPaidMthRnk =
      currentSupportPaidMthRnk;
    entities.DashboardOutputMetrics.Populated = true;
  }

  private void CreateDashboardPerformanceMetrics()
  {
    var reportMonth = entities.DashboardStagingPriority12.ReportMonth;
    var reportLevel = entities.DashboardStagingPriority12.ReportLevel;
    var reportLevelId = entities.DashboardStagingPriority12.ReportLevelId;
    var type1 = "DATA";
    var asOfDate = entities.DashboardStagingPriority12.AsOfDate;
    var casesUnderOrderNumerator =
      entities.DashboardStagingPriority12.CasesUnderOrderNumerator;
    var casesUnderOrderDenominator =
      entities.DashboardStagingPriority12.CasesUnderOrderDenominator;
    var casesUnderOrderPercent =
      entities.DashboardStagingPriority12.CasesUnderOrderPercent;
    var casesUnderOrderRank =
      entities.DashboardStagingPriority12.CasesUnderOrderRank;
    var pepNumerator = entities.DashboardStagingPriority12.PepNumerator;
    var pepDenominator = entities.DashboardStagingPriority12.PepDenominator;
    var pepPercent = entities.DashboardStagingPriority12.PepPercent;
    var casesPayingArrearsNumerator =
      entities.DashboardStagingPriority12.CasesPayingArrearsNumerator;
    var casesPayingArrearsDenominator =
      entities.DashboardStagingPriority12.CasesPayingArrearsDenominator;
    var casesPayingArrearsPercent =
      entities.DashboardStagingPriority12.CasesPayingArrearsPercent;
    var casesPayingArrearsRank =
      entities.DashboardStagingPriority12.CasesPayingArrearsRank;
    var currentSupportPaidMthNum =
      entities.DashboardStagingPriority12.CurrentSupportPaidMthNum;
    var currentSupportPaidMthDen =
      entities.DashboardStagingPriority12.CurrentSupportPaidMthDen;
    var currentSupportPaidMthPer =
      entities.DashboardStagingPriority12.CurrentSupportPaidMthPer;
    var currentSupportPaidMthRnk =
      entities.DashboardStagingPriority12.CurrentSupportPaidMthRnk;
    var currentSupportPaidFfytdNum =
      entities.DashboardStagingPriority12.CurrentSupportPaidFfytdNum;
    var currentSupportPaidFfytdDen =
      entities.DashboardStagingPriority12.CurrentSupportPaidFfytdDen;
    var currentSupportPaidFfytdPer =
      entities.DashboardStagingPriority12.CurrentSupportPaidFfytdPer;
    var currentSupportPaidFfytdRnk =
      entities.DashboardStagingPriority12.CurrentSupportPaidFfytdRnk;
    var collectionsFfytdToPriorMonth =
      entities.DashboardStagingPriority12.CollectionsFfytdToPriorMonth;
    var collectionsFfytdActual =
      entities.DashboardStagingPriority12.CollectionsFfytdActual;
    var collectionsFfytdPriorYear =
      entities.DashboardStagingPriority12.CollectionsFfytdPriorYear;
    var collectionsFfytdPercentChange =
      entities.DashboardStagingPriority12.CollectionsFfytdPercentChange;
    var collectionsFfytdRnk =
      entities.DashboardStagingPriority12.CollectionsFfytdRnk;
    var collectionsInMonthActual =
      entities.DashboardStagingPriority12.CollectionsInMonthActual;
    var collectionsInMonthPriorYear =
      entities.DashboardStagingPriority12.CollectionsInMonthPriorYear;
    var collectionsInMonthPercentChg =
      entities.DashboardStagingPriority12.CollectionsInMonthPercentChg;
    var collectionsInMonthRnk =
      entities.DashboardStagingPriority12.CollectionsInMonthRnk;
    var arrearsDistributedMonthActual =
      entities.DashboardStagingPriority12.ArrearsDistributedMonthActual;
    var arrearsDistributedMonthRnk =
      entities.DashboardStagingPriority12.ArrearsDistributedMonthRnk;
    var arrearsDistributedFfytdActual =
      entities.DashboardStagingPriority12.ArrearsDistributedFfytdActual;
    var arrearsDistrubutedFfytdRnk =
      entities.DashboardStagingPriority12.ArrearsDistrubutedFfytdRnk;
    var arrearsDueActual = entities.DashboardStagingPriority12.ArrearsDueActual;
    var arrearsDueRnk = entities.DashboardStagingPriority12.ArrearsDueRnk;
    var collectionsPerObligCaseNumer =
      entities.DashboardStagingPriority12.CollectionsPerObligCaseNumer;
    var collectionsPerObligCaseDenom =
      entities.DashboardStagingPriority12.CollectionsPerObligCaseDenom;
    var collectionsPerObligCaseAvg =
      entities.DashboardStagingPriority12.CollectionsPerObligCaseAvg;
    var collectionsPerObligCaseRnk =
      entities.DashboardStagingPriority12.CollectionsPerObligCaseRnk;
    var iwoPerObligCaseNumerator =
      entities.DashboardStagingPriority12.IwoPerObligCaseNumerator;
    var iwoPerObligCaseDenominator =
      entities.DashboardStagingPriority12.IwoPerObligCaseDenominator;
    var iwoPerObligCaseAverage =
      entities.DashboardStagingPriority12.IwoPerObligCaseAverage;
    var iwoPerObligCaseRnk =
      entities.DashboardStagingPriority12.IwoPerObligCaseRnk;
    var casesPerFteNumerator =
      entities.DashboardStagingPriority12.CasesPerFteNumerator;
    var casesPerFteDenominator =
      entities.DashboardStagingPriority12.CasesPerFteDenominator;
    var casesPerFteAverage =
      entities.DashboardStagingPriority12.CasesPerFteAverage;
    var casesPerFteRank = entities.DashboardStagingPriority12.CasesPerFteRank;
    var collectionsPerFteNumerator =
      entities.DashboardStagingPriority12.CollectionsPerFteNumerator;
    var collectionsPerFteDenominator =
      entities.DashboardStagingPriority12.CollectionsPerFteDenominator;
    var collectionsPerFteAverage =
      entities.DashboardStagingPriority12.CollectionsPerFteAverage;
    var collectionsPerFteRank =
      entities.DashboardStagingPriority12.CollectionsPerFteRank;
    var casesPayingNumerator =
      entities.DashboardStagingPriority12.CasesPayingNumerator;
    var casesPayingDenominator =
      entities.DashboardStagingPriority12.CasesPayingDenominator;
    var casesPayingPercent =
      entities.DashboardStagingPriority12.CasesPayingPercent;
    var casesPayingRank = entities.DashboardStagingPriority12.CasesPayingRank;
    var pepRank = entities.DashboardStagingPriority12.PepRank;
    var contractorNumber = entities.DashboardStagingPriority12.ContractorNumber;
    var prevYrPepNumerator =
      entities.DashboardStagingPriority12.PrevYrPepNumerator;
    var prevYrPepDenominator =
      entities.DashboardStagingPriority12.PrevYrPepDenominator;
    var prevYrPepPercent = entities.DashboardStagingPriority12.PrevYrPepPercent;
    var percentChgBetweenYrsPep =
      entities.DashboardStagingPriority12.PercentChgBetweenYrsPep;
    var prevYrCaseNumerator =
      entities.DashboardStagingPriority12.PrevYrCaseNumerator;
    var prevYrCaseDenominator =
      entities.DashboardStagingPriority12.PrevYrCaseDenominator;
    var casesUndrOrdrPrevYrPct =
      entities.DashboardStagingPriority12.CasesUndrOrdrPrevYrPct;
    var pctChgBtwenYrsCaseUndrOrdr =
      entities.DashboardStagingPriority12.PctChgBtwenYrsCaseUndrOrdr;
    var prevYrCurSupprtPaidNumtr =
      entities.DashboardStagingPriority12.PrevYrCurSupprtPaidNumtr;
    var prevYrCurSupprtPaidDenom =
      entities.DashboardStagingPriority12.PrevYrCurSupprtPaidDenom;
    var curSupprtPdPrevYrPct =
      entities.DashboardStagingPriority12.CurSupprtPdPrevYrPct;
    var pctChgBtwenYrsCurSuptPd =
      entities.DashboardStagingPriority12.PctChgBtwenYrsCurSuptPd;
    var prvYrCasesPaidArrearsNumtr =
      entities.DashboardStagingPriority12.PrvYrCasesPaidArrearsNumtr;
    var prvYrCasesPaidArrearsDenom =
      entities.DashboardStagingPriority12.PrvYrCasesPaidArrearsDenom;
    var casesPayArrearsPrvYrPct =
      entities.DashboardStagingPriority12.CasesPayArrearsPrvYrPct;
    var pctChgBtwenYrsCasesPayArrs =
      entities.DashboardStagingPriority12.PctChgBtwenYrsCasesPayArrs;

    entities.DashboardPerformanceMetrics.Populated = false;
    Update("CreateDashboardPerformanceMetrics",
      (db, command) =>
      {
        db.SetInt32(command, "reportMonth", reportMonth);
        db.SetString(command, "reportLevel", reportLevel);
        db.SetString(command, "reportLevelId", reportLevelId);
        db.SetString(command, "type", type1);
        db.SetNullableDate(command, "asOfDt", asOfDate);
        db.
          SetNullableInt32(command, "casUnderOrdNum", casesUnderOrderNumerator);
        db.SetNullableInt32(
          command, "casUnderOrdDen", casesUnderOrderDenominator);
        db.
          SetNullableDecimal(command, "casUnderOrdPert", casesUnderOrderPercent);
        db.SetNullableInt32(command, "casUnderOrdRnk", casesUnderOrderRank);
        db.SetNullableInt32(command, "pepNum", pepNumerator);
        db.SetNullableInt32(command, "pepDen", pepDenominator);
        db.SetNullableDecimal(command, "pepPer", pepPercent);
        db.SetNullableInt32(
          command, "casPayingArrNum", casesPayingArrearsNumerator);
        db.SetNullableInt32(
          command, "casPayingArrDen", casesPayingArrearsDenominator);
        db.SetNullableDecimal(
          command, "casPayingArrPer", casesPayingArrearsPercent);
        db.SetNullableInt32(command, "casPayingArrRnk", casesPayingArrearsRank);
        db.SetNullableDecimal(
          command, "curSupPdMthNum", currentSupportPaidMthNum);
        db.SetNullableDecimal(
          command, "curSupPdMthDen", currentSupportPaidMthDen);
        db.SetNullableDecimal(
          command, "curSupPdMthPer", currentSupportPaidMthPer);
        db.
          SetNullableInt32(command, "curSupPdMthRnk", currentSupportPaidMthRnk);
        db.SetNullableDecimal(
          command, "curSupPdYtdNum", currentSupportPaidFfytdNum);
        db.SetNullableDecimal(
          command, "curSupPdYtdDen", currentSupportPaidFfytdDen);
        db.SetNullableDecimal(
          command, "curSupPdYtdPer", currentSupportPaidFfytdPer);
        db.SetNullableInt32(
          command, "curSupPdYtdRnk", currentSupportPaidFfytdRnk);
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
        db.SetNullableDecimal(
          command, "arrDistMthAct", arrearsDistributedMonthActual);
        db.
          SetNullableInt32(command, "arrDistMthRnk", arrearsDistributedMonthRnk);
        db.SetNullableDecimal(
          command, "arrDistYtdAct", arrearsDistributedFfytdActual);
        db.
          SetNullableInt32(command, "arrDistYtdRnk", arrearsDistrubutedFfytdRnk);
        db.SetNullableDecimal(command, "arrDueAct", arrearsDueActual);
        db.SetNullableInt32(command, "arrDueRnk", arrearsDueRnk);
        db.SetNullableDecimal(
          command, "collOblCasNum", collectionsPerObligCaseNumer);
        db.SetNullableDecimal(
          command, "collOblCasDen", collectionsPerObligCaseDenom);
        db.SetNullableDecimal(
          command, "collOblCasAvg", collectionsPerObligCaseAvg);
        db.
          SetNullableInt32(command, "collOblCasRnk", collectionsPerObligCaseRnk);
        db.SetNullableInt32(command, "iwoOblCasNum", iwoPerObligCaseNumerator);
        db.
          SetNullableInt32(command, "iwoOblCasDen", iwoPerObligCaseDenominator);
        db.SetNullableDecimal(command, "iwoOblCasAvg", iwoPerObligCaseAverage);
        db.SetNullableInt32(command, "iwoOblCasRnk", iwoPerObligCaseRnk);
        db.SetNullableInt32(command, "casPerFteNum", casesPerFteNumerator);
        db.SetNullableDecimal(command, "casPerFteDen", casesPerFteDenominator);
        db.SetNullableDecimal(command, "casPerFteAvg", casesPerFteAverage);
        db.SetNullableInt32(command, "casPerFteRnk", casesPerFteRank);
        db.SetNullableDecimal(
          command, "collPerFteNum", collectionsPerFteNumerator);
        db.SetNullableDecimal(
          command, "collPerFteDen", collectionsPerFteDenominator);
        db.
          SetNullableDecimal(command, "collPerFteAvg", collectionsPerFteAverage);
        db.SetNullableInt32(command, "collPerFteRnk", collectionsPerFteRank);
        db.SetNullableInt32(command, "casPayingNum", casesPayingNumerator);
        db.SetNullableInt32(command, "casPayingDen", casesPayingDenominator);
        db.SetNullableDecimal(command, "casPayingPer", casesPayingPercent);
        db.SetNullableInt32(command, "casPayingRnk", casesPayingRank);
        db.SetNullableInt32(command, "pepRank", pepRank);
        db.SetNullableString(command, "contractorNum", contractorNumber);
        db.SetNullableInt32(command, "prvYrPepNumtr", prevYrPepNumerator);
        db.SetNullableInt32(command, "prvYrPepDenom", prevYrPepDenominator);
        db.SetNullableDecimal(command, "prvYrPepPct", prevYrPepPercent);
        db.SetNullableDecimal(command, "pctChgByrPep", percentChgBetweenYrsPep);
        db.SetNullableInt32(command, "prvYrCaseNumtr", prevYrCaseNumerator);
        db.SetNullableInt32(command, "prvYrCaseDenom", prevYrCaseDenominator);
        db.SetNullableDecimal(command, "prvYrCasPctUo", casesUndrOrdrPrevYrPct);
        db.SetNullableDecimal(
          command, "pctChgByrCasUo", pctChgBtwenYrsCaseUndrOrdr);
        db.SetNullableDecimal(
          command, "pvYrSupPdNumtr", prevYrCurSupprtPaidNumtr);
        db.SetNullableDecimal(
          command, "pvYrSupPdDenom", prevYrCurSupprtPaidDenom);
        db.SetNullableDecimal(command, "prvYrCSPdPct", curSupprtPdPrevYrPct);
        db.
          SetNullableDecimal(command, "pctChgByrCsPd", pctChgBtwenYrsCurSuptPd);
        db.SetNullableInt32(
          command, "prvYrPdArNumtr", prvYrCasesPaidArrearsNumtr);
        db.SetNullableInt32(
          command, "prvYrPdArDenom", prvYrCasesPaidArrearsDenom);
        db.
          SetNullableDecimal(command, "payArPrvYrPct", casesPayArrearsPrvYrPct);
        db.SetNullableDecimal(
          command, "pctChgByrArsPd", pctChgBtwenYrsCasesPayArrs);
      });

    entities.DashboardPerformanceMetrics.ReportMonth = reportMonth;
    entities.DashboardPerformanceMetrics.ReportLevel = reportLevel;
    entities.DashboardPerformanceMetrics.ReportLevelId = reportLevelId;
    entities.DashboardPerformanceMetrics.Type1 = type1;
    entities.DashboardPerformanceMetrics.AsOfDate = asOfDate;
    entities.DashboardPerformanceMetrics.CasesUnderOrderNumerator =
      casesUnderOrderNumerator;
    entities.DashboardPerformanceMetrics.CasesUnderOrderDenominator =
      casesUnderOrderDenominator;
    entities.DashboardPerformanceMetrics.CasesUnderOrderPercent =
      casesUnderOrderPercent;
    entities.DashboardPerformanceMetrics.CasesUnderOrderRank =
      casesUnderOrderRank;
    entities.DashboardPerformanceMetrics.PepNumerator = pepNumerator;
    entities.DashboardPerformanceMetrics.PepDenominator = pepDenominator;
    entities.DashboardPerformanceMetrics.PepPercent = pepPercent;
    entities.DashboardPerformanceMetrics.CasesPayingArrearsNumerator =
      casesPayingArrearsNumerator;
    entities.DashboardPerformanceMetrics.CasesPayingArrearsDenominator =
      casesPayingArrearsDenominator;
    entities.DashboardPerformanceMetrics.CasesPayingArrearsPercent =
      casesPayingArrearsPercent;
    entities.DashboardPerformanceMetrics.CasesPayingArrearsRank =
      casesPayingArrearsRank;
    entities.DashboardPerformanceMetrics.CurrentSupportPaidMthNum =
      currentSupportPaidMthNum;
    entities.DashboardPerformanceMetrics.CurrentSupportPaidMthDen =
      currentSupportPaidMthDen;
    entities.DashboardPerformanceMetrics.CurrentSupportPaidMthPer =
      currentSupportPaidMthPer;
    entities.DashboardPerformanceMetrics.CurrentSupportPaidMthRnk =
      currentSupportPaidMthRnk;
    entities.DashboardPerformanceMetrics.CurrentSupportPaidFfytdNum =
      currentSupportPaidFfytdNum;
    entities.DashboardPerformanceMetrics.CurrentSupportPaidFfytdDen =
      currentSupportPaidFfytdDen;
    entities.DashboardPerformanceMetrics.CurrentSupportPaidFfytdPer =
      currentSupportPaidFfytdPer;
    entities.DashboardPerformanceMetrics.CurrentSupportPaidFfytdRnk =
      currentSupportPaidFfytdRnk;
    entities.DashboardPerformanceMetrics.CollectionsFfytdToPriorMonth =
      collectionsFfytdToPriorMonth;
    entities.DashboardPerformanceMetrics.CollectionsFfytdActual =
      collectionsFfytdActual;
    entities.DashboardPerformanceMetrics.CollectionsFfytdPriorYear =
      collectionsFfytdPriorYear;
    entities.DashboardPerformanceMetrics.CollectionsFfytdPercentChange =
      collectionsFfytdPercentChange;
    entities.DashboardPerformanceMetrics.CollectionsFfytdRnk =
      collectionsFfytdRnk;
    entities.DashboardPerformanceMetrics.CollectionsInMonthActual =
      collectionsInMonthActual;
    entities.DashboardPerformanceMetrics.CollectionsInMonthPriorYear =
      collectionsInMonthPriorYear;
    entities.DashboardPerformanceMetrics.CollectionsInMonthPercentChg =
      collectionsInMonthPercentChg;
    entities.DashboardPerformanceMetrics.CollectionsInMonthRnk =
      collectionsInMonthRnk;
    entities.DashboardPerformanceMetrics.ArrearsDistributedMonthActual =
      arrearsDistributedMonthActual;
    entities.DashboardPerformanceMetrics.ArrearsDistributedMonthRnk =
      arrearsDistributedMonthRnk;
    entities.DashboardPerformanceMetrics.ArrearsDistributedFfytdActual =
      arrearsDistributedFfytdActual;
    entities.DashboardPerformanceMetrics.ArrearsDistrubutedFfytdRnk =
      arrearsDistrubutedFfytdRnk;
    entities.DashboardPerformanceMetrics.ArrearsDueActual = arrearsDueActual;
    entities.DashboardPerformanceMetrics.ArrearsDueRnk = arrearsDueRnk;
    entities.DashboardPerformanceMetrics.CollectionsPerObligCaseNumer =
      collectionsPerObligCaseNumer;
    entities.DashboardPerformanceMetrics.CollectionsPerObligCaseDenom =
      collectionsPerObligCaseDenom;
    entities.DashboardPerformanceMetrics.CollectionsPerObligCaseAvg =
      collectionsPerObligCaseAvg;
    entities.DashboardPerformanceMetrics.CollectionsPerObligCaseRnk =
      collectionsPerObligCaseRnk;
    entities.DashboardPerformanceMetrics.IwoPerObligCaseNumerator =
      iwoPerObligCaseNumerator;
    entities.DashboardPerformanceMetrics.IwoPerObligCaseDenominator =
      iwoPerObligCaseDenominator;
    entities.DashboardPerformanceMetrics.IwoPerObligCaseAverage =
      iwoPerObligCaseAverage;
    entities.DashboardPerformanceMetrics.IwoPerObligCaseRnk =
      iwoPerObligCaseRnk;
    entities.DashboardPerformanceMetrics.CasesPerFteNumerator =
      casesPerFteNumerator;
    entities.DashboardPerformanceMetrics.CasesPerFteDenominator =
      casesPerFteDenominator;
    entities.DashboardPerformanceMetrics.CasesPerFteAverage =
      casesPerFteAverage;
    entities.DashboardPerformanceMetrics.CasesPerFteRank = casesPerFteRank;
    entities.DashboardPerformanceMetrics.CollectionsPerFteNumerator =
      collectionsPerFteNumerator;
    entities.DashboardPerformanceMetrics.CollectionsPerFteDenominator =
      collectionsPerFteDenominator;
    entities.DashboardPerformanceMetrics.CollectionsPerFteAverage =
      collectionsPerFteAverage;
    entities.DashboardPerformanceMetrics.CollectionsPerFteRank =
      collectionsPerFteRank;
    entities.DashboardPerformanceMetrics.CasesPayingNumerator =
      casesPayingNumerator;
    entities.DashboardPerformanceMetrics.CasesPayingDenominator =
      casesPayingDenominator;
    entities.DashboardPerformanceMetrics.CasesPayingPercent =
      casesPayingPercent;
    entities.DashboardPerformanceMetrics.CasesPayingRank = casesPayingRank;
    entities.DashboardPerformanceMetrics.PepRank = pepRank;
    entities.DashboardPerformanceMetrics.ContractorNumber = contractorNumber;
    entities.DashboardPerformanceMetrics.PrevYrPepNumerator =
      prevYrPepNumerator;
    entities.DashboardPerformanceMetrics.PrevYrPepDenominator =
      prevYrPepDenominator;
    entities.DashboardPerformanceMetrics.PrevYrPepPercent = prevYrPepPercent;
    entities.DashboardPerformanceMetrics.PercentChgBetweenYrsPep =
      percentChgBetweenYrsPep;
    entities.DashboardPerformanceMetrics.PrevYrCaseNumerator =
      prevYrCaseNumerator;
    entities.DashboardPerformanceMetrics.PrevYrCaseDenominator =
      prevYrCaseDenominator;
    entities.DashboardPerformanceMetrics.CasesUndrOrdrPrevYrPct =
      casesUndrOrdrPrevYrPct;
    entities.DashboardPerformanceMetrics.PctChgBtwenYrsCaseUndrOrdr =
      pctChgBtwenYrsCaseUndrOrdr;
    entities.DashboardPerformanceMetrics.PrevYrCurSupprtPaidNumtr =
      prevYrCurSupprtPaidNumtr;
    entities.DashboardPerformanceMetrics.PrevYrCurSupprtPaidDenom =
      prevYrCurSupprtPaidDenom;
    entities.DashboardPerformanceMetrics.CurSupprtPdPrevYrPct =
      curSupprtPdPrevYrPct;
    entities.DashboardPerformanceMetrics.PctChgBtwenYrsCurSuptPd =
      pctChgBtwenYrsCurSuptPd;
    entities.DashboardPerformanceMetrics.PrvYrCasesPaidArrearsNumtr =
      prvYrCasesPaidArrearsNumtr;
    entities.DashboardPerformanceMetrics.PrvYrCasesPaidArrearsDenom =
      prvYrCasesPaidArrearsDenom;
    entities.DashboardPerformanceMetrics.CasesPayArrearsPrvYrPct =
      casesPayArrearsPrvYrPct;
    entities.DashboardPerformanceMetrics.PctChgBtwenYrsCasesPayArrs =
      pctChgBtwenYrsCasesPayArrs;
    entities.DashboardPerformanceMetrics.Populated = true;
  }

  private void DeleteDashboardOutputMetrics()
  {
    Update("DeleteDashboardOutputMetrics",
      (db, command) =>
      {
        db.SetInt32(
          command, "reportMonth", entities.DashboardOutputMetrics.ReportMonth);
        db.SetString(
          command, "reportLevel", entities.DashboardOutputMetrics.ReportLevel);
        db.SetString(
          command, "reportLevelId",
          entities.DashboardOutputMetrics.ReportLevelId);
        db.SetString(command, "type", entities.DashboardOutputMetrics.Type1);
      });
  }

  private void DeleteDashboardPerformanceMetrics()
  {
    Update("DeleteDashboardPerformanceMetrics",
      (db, command) =>
      {
        db.SetInt32(
          command, "reportMonth",
          entities.DashboardPerformanceMetrics.ReportMonth);
        db.SetString(
          command, "reportLevel",
          entities.DashboardPerformanceMetrics.ReportLevel);
        db.SetString(
          command, "reportLevelId",
          entities.DashboardPerformanceMetrics.ReportLevelId);
        db.
          SetString(command, "type", entities.DashboardPerformanceMetrics.Type1);
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
        entities.CseOrganization.Name = db.GetString(reader, 2);
        entities.CseOrganization.Populated = true;

        return true;
      },
      () =>
      {
        entities.CseOrganization.Populated = false;
      });
  }

  private bool ReadDashboardName1()
  {
    entities.DashboardName.Populated = false;

    return Read("ReadDashboardName1",
      (db, command) =>
      {
        db.SetString(command, "providerId", entities.ServiceProvider.UserId);
      },
      (db, reader) =>
      {
        entities.DashboardName.ProviderId = db.GetString(reader, 0);
        entities.DashboardName.ProviderType = db.GetString(reader, 1);
        entities.DashboardName.OrgOrLastName = db.GetNullableString(reader, 2);
        entities.DashboardName.FirstName = db.GetNullableString(reader, 3);
        entities.DashboardName.MiddleInitial = db.GetNullableString(reader, 4);
        entities.DashboardName.Populated = true;
      });
  }

  private bool ReadDashboardName2()
  {
    entities.DashboardName.Populated = false;

    return Read("ReadDashboardName2",
      (db, command) =>
      {
        db.SetString(command, "providerId", local.DashboardName.ProviderId);
      },
      (db, reader) =>
      {
        entities.DashboardName.ProviderId = db.GetString(reader, 0);
        entities.DashboardName.ProviderType = db.GetString(reader, 1);
        entities.DashboardName.OrgOrLastName = db.GetNullableString(reader, 2);
        entities.DashboardName.FirstName = db.GetNullableString(reader, 3);
        entities.DashboardName.MiddleInitial = db.GetNullableString(reader, 4);
        entities.DashboardName.Populated = true;
      });
  }

  private IEnumerable<bool> ReadDashboardOutputMetrics()
  {
    return ReadEach("ReadDashboardOutputMetrics",
      null,
      (db, reader) =>
      {
        entities.DashboardOutputMetrics.ReportMonth = db.GetInt32(reader, 0);
        entities.DashboardOutputMetrics.ReportLevel = db.GetString(reader, 1);
        entities.DashboardOutputMetrics.ReportLevelId = db.GetString(reader, 2);
        entities.DashboardOutputMetrics.Type1 = db.GetString(reader, 3);
        entities.DashboardOutputMetrics.AsOfDate =
          db.GetNullableDate(reader, 4);
        entities.DashboardOutputMetrics.CasesWithEstReferral =
          db.GetNullableInt32(reader, 5);
        entities.DashboardOutputMetrics.CasesWithEnfReferral =
          db.GetNullableInt32(reader, 6);
        entities.DashboardOutputMetrics.FullTimeEquivalent =
          db.GetNullableDecimal(reader, 7);
        entities.DashboardOutputMetrics.NewOrdersEstablished =
          db.GetNullableInt32(reader, 8);
        entities.DashboardOutputMetrics.PaternitiesEstablished =
          db.GetNullableInt32(reader, 9);
        entities.DashboardOutputMetrics.CasesOpenedWithOrder =
          db.GetNullableInt32(reader, 10);
        entities.DashboardOutputMetrics.CasesOpenedWithoutOrders =
          db.GetNullableInt32(reader, 11);
        entities.DashboardOutputMetrics.CasesClosedWithOrders =
          db.GetNullableInt32(reader, 12);
        entities.DashboardOutputMetrics.CasesClosedWithoutOrders =
          db.GetNullableInt32(reader, 13);
        entities.DashboardOutputMetrics.Modifications =
          db.GetNullableInt32(reader, 14);
        entities.DashboardOutputMetrics.IncomeWithholdingsIssued =
          db.GetNullableInt32(reader, 15);
        entities.DashboardOutputMetrics.ContemptMotionFilings =
          db.GetNullableInt32(reader, 16);
        entities.DashboardOutputMetrics.ContemptOrderFilings =
          db.GetNullableInt32(reader, 17);
        entities.DashboardOutputMetrics.StypeCollectionAmount =
          db.GetNullableDecimal(reader, 18);
        entities.DashboardOutputMetrics.StypePercentOfTotal =
          db.GetNullableDecimal(reader, 19);
        entities.DashboardOutputMetrics.FtypeCollectionAmount =
          db.GetNullableDecimal(reader, 20);
        entities.DashboardOutputMetrics.FtypePercentOfTotal =
          db.GetNullableDecimal(reader, 21);
        entities.DashboardOutputMetrics.ItypeCollectionAmount =
          db.GetNullableDecimal(reader, 22);
        entities.DashboardOutputMetrics.ItypePercentOfTotal =
          db.GetNullableDecimal(reader, 23);
        entities.DashboardOutputMetrics.UtypeCollectionAmount =
          db.GetNullableDecimal(reader, 24);
        entities.DashboardOutputMetrics.UtypePercentOfTotal =
          db.GetNullableDecimal(reader, 25);
        entities.DashboardOutputMetrics.CtypeCollectionAmount =
          db.GetNullableDecimal(reader, 26);
        entities.DashboardOutputMetrics.CtypePercentOfTotal =
          db.GetNullableDecimal(reader, 27);
        entities.DashboardOutputMetrics.TotalCollectionAmount =
          db.GetNullableDecimal(reader, 28);
        entities.DashboardOutputMetrics.DaysToOrderEstblshmntNumer =
          db.GetNullableInt32(reader, 29);
        entities.DashboardOutputMetrics.DaysToOrderEstblshmntDenom =
          db.GetNullableInt32(reader, 30);
        entities.DashboardOutputMetrics.DaysToOrderEstblshmntAvg =
          db.GetNullableDecimal(reader, 31);
        entities.DashboardOutputMetrics.DaysToReturnOfSrvcNumer =
          db.GetNullableInt32(reader, 32);
        entities.DashboardOutputMetrics.DaysToReturnOfServiceDenom =
          db.GetNullableInt32(reader, 33);
        entities.DashboardOutputMetrics.DaysToReturnOfServiceAvg =
          db.GetNullableDecimal(reader, 34);
        entities.DashboardOutputMetrics.ReferralAging60To90Days =
          db.GetNullableInt32(reader, 35);
        entities.DashboardOutputMetrics.ReferralAging91To120Days =
          db.GetNullableInt32(reader, 36);
        entities.DashboardOutputMetrics.ReferralAging121To150Days =
          db.GetNullableInt32(reader, 37);
        entities.DashboardOutputMetrics.ReferralAging151PlusDays =
          db.GetNullableInt32(reader, 38);
        entities.DashboardOutputMetrics.DaysToIwoPaymentNumerator =
          db.GetNullableInt32(reader, 39);
        entities.DashboardOutputMetrics.DaysToIwoPaymentDenominator =
          db.GetNullableInt32(reader, 40);
        entities.DashboardOutputMetrics.DaysToIwoPaymentAverage =
          db.GetNullableDecimal(reader, 41);
        entities.DashboardOutputMetrics.ReferralsToLegalForEst =
          db.GetNullableInt32(reader, 42);
        entities.DashboardOutputMetrics.ReferralsToLegalForEnf =
          db.GetNullableInt32(reader, 43);
        entities.DashboardOutputMetrics.CaseloadCount =
          db.GetNullableInt32(reader, 44);
        entities.DashboardOutputMetrics.CasesOpened =
          db.GetNullableInt32(reader, 45);
        entities.DashboardOutputMetrics.NcpLocatesByAddress =
          db.GetNullableInt32(reader, 46);
        entities.DashboardOutputMetrics.NcpLocatesByEmployer =
          db.GetNullableInt32(reader, 47);
        entities.DashboardOutputMetrics.CaseClosures =
          db.GetNullableInt32(reader, 48);
        entities.DashboardOutputMetrics.CaseReviews =
          db.GetNullableInt32(reader, 49);
        entities.DashboardOutputMetrics.Petitions =
          db.GetNullableInt32(reader, 50);
        entities.DashboardOutputMetrics.CasesPayingArrearsDenominator =
          db.GetNullableInt32(reader, 51);
        entities.DashboardOutputMetrics.CasesPayingArrearsNumerator =
          db.GetNullableInt32(reader, 52);
        entities.DashboardOutputMetrics.CasesPayingArrearsPercent =
          db.GetNullableDecimal(reader, 53);
        entities.DashboardOutputMetrics.CasesPayingArrearsRank =
          db.GetNullableInt32(reader, 54);
        entities.DashboardOutputMetrics.CurrentSupportPaidFfytdDen =
          db.GetNullableDecimal(reader, 55);
        entities.DashboardOutputMetrics.CurrentSupportPaidFfytdNum =
          db.GetNullableDecimal(reader, 56);
        entities.DashboardOutputMetrics.CurrentSupportPaidFfytdPer =
          db.GetNullableDecimal(reader, 57);
        entities.DashboardOutputMetrics.CurrentSupportPaidFfytdRnk =
          db.GetNullableInt32(reader, 58);
        entities.DashboardOutputMetrics.CurrentSupportPaidMthDen =
          db.GetNullableDecimal(reader, 59);
        entities.DashboardOutputMetrics.CurrentSupportPaidMthNum =
          db.GetNullableDecimal(reader, 60);
        entities.DashboardOutputMetrics.CurrentSupportPaidMthPer =
          db.GetNullableDecimal(reader, 61);
        entities.DashboardOutputMetrics.CurrentSupportPaidMthRnk =
          db.GetNullableInt32(reader, 62);
        entities.DashboardOutputMetrics.Populated = true;

        return true;
      },
      () =>
      {
        entities.DashboardOutputMetrics.Populated = false;
      });
  }

  private IEnumerable<bool> ReadDashboardPerformanceMetrics()
  {
    return ReadEach("ReadDashboardPerformanceMetrics",
      null,
      (db, reader) =>
      {
        entities.DashboardPerformanceMetrics.ReportMonth =
          db.GetInt32(reader, 0);
        entities.DashboardPerformanceMetrics.ReportLevel =
          db.GetString(reader, 1);
        entities.DashboardPerformanceMetrics.ReportLevelId =
          db.GetString(reader, 2);
        entities.DashboardPerformanceMetrics.Type1 = db.GetString(reader, 3);
        entities.DashboardPerformanceMetrics.AsOfDate =
          db.GetNullableDate(reader, 4);
        entities.DashboardPerformanceMetrics.CasesUnderOrderNumerator =
          db.GetNullableInt32(reader, 5);
        entities.DashboardPerformanceMetrics.CasesUnderOrderDenominator =
          db.GetNullableInt32(reader, 6);
        entities.DashboardPerformanceMetrics.CasesUnderOrderPercent =
          db.GetNullableDecimal(reader, 7);
        entities.DashboardPerformanceMetrics.CasesUnderOrderRank =
          db.GetNullableInt32(reader, 8);
        entities.DashboardPerformanceMetrics.PepNumerator =
          db.GetNullableInt32(reader, 9);
        entities.DashboardPerformanceMetrics.PepDenominator =
          db.GetNullableInt32(reader, 10);
        entities.DashboardPerformanceMetrics.PepPercent =
          db.GetNullableDecimal(reader, 11);
        entities.DashboardPerformanceMetrics.CasesPayingArrearsNumerator =
          db.GetNullableInt32(reader, 12);
        entities.DashboardPerformanceMetrics.CasesPayingArrearsDenominator =
          db.GetNullableInt32(reader, 13);
        entities.DashboardPerformanceMetrics.CasesPayingArrearsPercent =
          db.GetNullableDecimal(reader, 14);
        entities.DashboardPerformanceMetrics.CasesPayingArrearsRank =
          db.GetNullableInt32(reader, 15);
        entities.DashboardPerformanceMetrics.CurrentSupportPaidMthNum =
          db.GetNullableDecimal(reader, 16);
        entities.DashboardPerformanceMetrics.CurrentSupportPaidMthDen =
          db.GetNullableDecimal(reader, 17);
        entities.DashboardPerformanceMetrics.CurrentSupportPaidMthPer =
          db.GetNullableDecimal(reader, 18);
        entities.DashboardPerformanceMetrics.CurrentSupportPaidMthRnk =
          db.GetNullableInt32(reader, 19);
        entities.DashboardPerformanceMetrics.CurrentSupportPaidFfytdNum =
          db.GetNullableDecimal(reader, 20);
        entities.DashboardPerformanceMetrics.CurrentSupportPaidFfytdDen =
          db.GetNullableDecimal(reader, 21);
        entities.DashboardPerformanceMetrics.CurrentSupportPaidFfytdPer =
          db.GetNullableDecimal(reader, 22);
        entities.DashboardPerformanceMetrics.CurrentSupportPaidFfytdRnk =
          db.GetNullableInt32(reader, 23);
        entities.DashboardPerformanceMetrics.CollectionsFfytdToPriorMonth =
          db.GetNullableDecimal(reader, 24);
        entities.DashboardPerformanceMetrics.CollectionsFfytdActual =
          db.GetNullableDecimal(reader, 25);
        entities.DashboardPerformanceMetrics.CollectionsFfytdPriorYear =
          db.GetNullableDecimal(reader, 26);
        entities.DashboardPerformanceMetrics.CollectionsFfytdPercentChange =
          db.GetNullableDecimal(reader, 27);
        entities.DashboardPerformanceMetrics.CollectionsFfytdRnk =
          db.GetNullableInt32(reader, 28);
        entities.DashboardPerformanceMetrics.CollectionsInMonthActual =
          db.GetNullableDecimal(reader, 29);
        entities.DashboardPerformanceMetrics.CollectionsInMonthPriorYear =
          db.GetNullableDecimal(reader, 30);
        entities.DashboardPerformanceMetrics.CollectionsInMonthPercentChg =
          db.GetNullableDecimal(reader, 31);
        entities.DashboardPerformanceMetrics.CollectionsInMonthRnk =
          db.GetNullableInt32(reader, 32);
        entities.DashboardPerformanceMetrics.ArrearsDistributedMonthActual =
          db.GetNullableDecimal(reader, 33);
        entities.DashboardPerformanceMetrics.ArrearsDistributedMonthRnk =
          db.GetNullableInt32(reader, 34);
        entities.DashboardPerformanceMetrics.ArrearsDistributedFfytdActual =
          db.GetNullableDecimal(reader, 35);
        entities.DashboardPerformanceMetrics.ArrearsDistrubutedFfytdRnk =
          db.GetNullableInt32(reader, 36);
        entities.DashboardPerformanceMetrics.ArrearsDueActual =
          db.GetNullableDecimal(reader, 37);
        entities.DashboardPerformanceMetrics.ArrearsDueRnk =
          db.GetNullableInt32(reader, 38);
        entities.DashboardPerformanceMetrics.CollectionsPerObligCaseNumer =
          db.GetNullableDecimal(reader, 39);
        entities.DashboardPerformanceMetrics.CollectionsPerObligCaseDenom =
          db.GetNullableDecimal(reader, 40);
        entities.DashboardPerformanceMetrics.CollectionsPerObligCaseAvg =
          db.GetNullableDecimal(reader, 41);
        entities.DashboardPerformanceMetrics.CollectionsPerObligCaseRnk =
          db.GetNullableInt32(reader, 42);
        entities.DashboardPerformanceMetrics.IwoPerObligCaseNumerator =
          db.GetNullableInt32(reader, 43);
        entities.DashboardPerformanceMetrics.IwoPerObligCaseDenominator =
          db.GetNullableInt32(reader, 44);
        entities.DashboardPerformanceMetrics.IwoPerObligCaseAverage =
          db.GetNullableDecimal(reader, 45);
        entities.DashboardPerformanceMetrics.IwoPerObligCaseRnk =
          db.GetNullableInt32(reader, 46);
        entities.DashboardPerformanceMetrics.CasesPerFteNumerator =
          db.GetNullableInt32(reader, 47);
        entities.DashboardPerformanceMetrics.CasesPerFteDenominator =
          db.GetNullableDecimal(reader, 48);
        entities.DashboardPerformanceMetrics.CasesPerFteAverage =
          db.GetNullableDecimal(reader, 49);
        entities.DashboardPerformanceMetrics.CasesPerFteRank =
          db.GetNullableInt32(reader, 50);
        entities.DashboardPerformanceMetrics.CollectionsPerFteNumerator =
          db.GetNullableDecimal(reader, 51);
        entities.DashboardPerformanceMetrics.CollectionsPerFteDenominator =
          db.GetNullableDecimal(reader, 52);
        entities.DashboardPerformanceMetrics.CollectionsPerFteAverage =
          db.GetNullableDecimal(reader, 53);
        entities.DashboardPerformanceMetrics.CollectionsPerFteRank =
          db.GetNullableInt32(reader, 54);
        entities.DashboardPerformanceMetrics.CasesPayingNumerator =
          db.GetNullableInt32(reader, 55);
        entities.DashboardPerformanceMetrics.CasesPayingDenominator =
          db.GetNullableInt32(reader, 56);
        entities.DashboardPerformanceMetrics.CasesPayingPercent =
          db.GetNullableDecimal(reader, 57);
        entities.DashboardPerformanceMetrics.CasesPayingRank =
          db.GetNullableInt32(reader, 58);
        entities.DashboardPerformanceMetrics.PepRank =
          db.GetNullableInt32(reader, 59);
        entities.DashboardPerformanceMetrics.ContractorNumber =
          db.GetNullableString(reader, 60);
        entities.DashboardPerformanceMetrics.PrevYrPepNumerator =
          db.GetNullableInt32(reader, 61);
        entities.DashboardPerformanceMetrics.PrevYrPepDenominator =
          db.GetNullableInt32(reader, 62);
        entities.DashboardPerformanceMetrics.PrevYrPepPercent =
          db.GetNullableDecimal(reader, 63);
        entities.DashboardPerformanceMetrics.PercentChgBetweenYrsPep =
          db.GetNullableDecimal(reader, 64);
        entities.DashboardPerformanceMetrics.PrevYrCaseNumerator =
          db.GetNullableInt32(reader, 65);
        entities.DashboardPerformanceMetrics.PrevYrCaseDenominator =
          db.GetNullableInt32(reader, 66);
        entities.DashboardPerformanceMetrics.CasesUndrOrdrPrevYrPct =
          db.GetNullableDecimal(reader, 67);
        entities.DashboardPerformanceMetrics.PctChgBtwenYrsCaseUndrOrdr =
          db.GetNullableDecimal(reader, 68);
        entities.DashboardPerformanceMetrics.PrevYrCurSupprtPaidNumtr =
          db.GetNullableDecimal(reader, 69);
        entities.DashboardPerformanceMetrics.PrevYrCurSupprtPaidDenom =
          db.GetNullableDecimal(reader, 70);
        entities.DashboardPerformanceMetrics.CurSupprtPdPrevYrPct =
          db.GetNullableDecimal(reader, 71);
        entities.DashboardPerformanceMetrics.PctChgBtwenYrsCurSuptPd =
          db.GetNullableDecimal(reader, 72);
        entities.DashboardPerformanceMetrics.PrvYrCasesPaidArrearsNumtr =
          db.GetNullableInt32(reader, 73);
        entities.DashboardPerformanceMetrics.PrvYrCasesPaidArrearsDenom =
          db.GetNullableInt32(reader, 74);
        entities.DashboardPerformanceMetrics.CasesPayArrearsPrvYrPct =
          db.GetNullableDecimal(reader, 75);
        entities.DashboardPerformanceMetrics.PctChgBtwenYrsCasesPayArrs =
          db.GetNullableDecimal(reader, 76);
        entities.DashboardPerformanceMetrics.Populated = true;

        return true;
      },
      () =>
      {
        entities.DashboardPerformanceMetrics.Populated = false;
      });
  }

  private IEnumerable<bool> ReadDashboardStagingPriority12()
  {
    return ReadEach("ReadDashboardStagingPriority12",
      null,
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
        entities.DashboardStagingPriority12.PepNumerator =
          db.GetNullableInt32(reader, 8);
        entities.DashboardStagingPriority12.PepDenominator =
          db.GetNullableInt32(reader, 9);
        entities.DashboardStagingPriority12.PepPercent =
          db.GetNullableDecimal(reader, 10);
        entities.DashboardStagingPriority12.CasesPayingArrearsNumerator =
          db.GetNullableInt32(reader, 11);
        entities.DashboardStagingPriority12.CasesPayingArrearsDenominator =
          db.GetNullableInt32(reader, 12);
        entities.DashboardStagingPriority12.CasesPayingArrearsPercent =
          db.GetNullableDecimal(reader, 13);
        entities.DashboardStagingPriority12.CasesPayingArrearsRank =
          db.GetNullableInt32(reader, 14);
        entities.DashboardStagingPriority12.CurrentSupportPaidMthNum =
          db.GetNullableDecimal(reader, 15);
        entities.DashboardStagingPriority12.CurrentSupportPaidMthDen =
          db.GetNullableDecimal(reader, 16);
        entities.DashboardStagingPriority12.CurrentSupportPaidMthPer =
          db.GetNullableDecimal(reader, 17);
        entities.DashboardStagingPriority12.CurrentSupportPaidMthRnk =
          db.GetNullableInt32(reader, 18);
        entities.DashboardStagingPriority12.CurrentSupportPaidFfytdNum =
          db.GetNullableDecimal(reader, 19);
        entities.DashboardStagingPriority12.CurrentSupportPaidFfytdDen =
          db.GetNullableDecimal(reader, 20);
        entities.DashboardStagingPriority12.CurrentSupportPaidFfytdPer =
          db.GetNullableDecimal(reader, 21);
        entities.DashboardStagingPriority12.CurrentSupportPaidFfytdRnk =
          db.GetNullableInt32(reader, 22);
        entities.DashboardStagingPriority12.CollectionsFfytdToPriorMonth =
          db.GetNullableDecimal(reader, 23);
        entities.DashboardStagingPriority12.CollectionsFfytdActual =
          db.GetNullableDecimal(reader, 24);
        entities.DashboardStagingPriority12.CollectionsFfytdPriorYear =
          db.GetNullableDecimal(reader, 25);
        entities.DashboardStagingPriority12.CollectionsFfytdPercentChange =
          db.GetNullableDecimal(reader, 26);
        entities.DashboardStagingPriority12.CollectionsFfytdRnk =
          db.GetNullableInt32(reader, 27);
        entities.DashboardStagingPriority12.CollectionsInMonthActual =
          db.GetNullableDecimal(reader, 28);
        entities.DashboardStagingPriority12.CollectionsInMonthPriorYear =
          db.GetNullableDecimal(reader, 29);
        entities.DashboardStagingPriority12.CollectionsInMonthPercentChg =
          db.GetNullableDecimal(reader, 30);
        entities.DashboardStagingPriority12.CollectionsInMonthRnk =
          db.GetNullableInt32(reader, 31);
        entities.DashboardStagingPriority12.ArrearsDistributedMonthActual =
          db.GetNullableDecimal(reader, 32);
        entities.DashboardStagingPriority12.ArrearsDistributedMonthRnk =
          db.GetNullableInt32(reader, 33);
        entities.DashboardStagingPriority12.ArrearsDistributedFfytdActual =
          db.GetNullableDecimal(reader, 34);
        entities.DashboardStagingPriority12.ArrearsDistrubutedFfytdRnk =
          db.GetNullableInt32(reader, 35);
        entities.DashboardStagingPriority12.ArrearsDueActual =
          db.GetNullableDecimal(reader, 36);
        entities.DashboardStagingPriority12.ArrearsDueRnk =
          db.GetNullableInt32(reader, 37);
        entities.DashboardStagingPriority12.CollectionsPerObligCaseNumer =
          db.GetNullableDecimal(reader, 38);
        entities.DashboardStagingPriority12.CollectionsPerObligCaseDenom =
          db.GetNullableDecimal(reader, 39);
        entities.DashboardStagingPriority12.CollectionsPerObligCaseAvg =
          db.GetNullableDecimal(reader, 40);
        entities.DashboardStagingPriority12.CollectionsPerObligCaseRnk =
          db.GetNullableInt32(reader, 41);
        entities.DashboardStagingPriority12.IwoPerObligCaseNumerator =
          db.GetNullableInt32(reader, 42);
        entities.DashboardStagingPriority12.IwoPerObligCaseDenominator =
          db.GetNullableInt32(reader, 43);
        entities.DashboardStagingPriority12.IwoPerObligCaseAverage =
          db.GetNullableDecimal(reader, 44);
        entities.DashboardStagingPriority12.IwoPerObligCaseRnk =
          db.GetNullableInt32(reader, 45);
        entities.DashboardStagingPriority12.CasesPerFteNumerator =
          db.GetNullableInt32(reader, 46);
        entities.DashboardStagingPriority12.CasesPerFteDenominator =
          db.GetNullableDecimal(reader, 47);
        entities.DashboardStagingPriority12.CasesPerFteAverage =
          db.GetNullableDecimal(reader, 48);
        entities.DashboardStagingPriority12.CasesPerFteRank =
          db.GetNullableInt32(reader, 49);
        entities.DashboardStagingPriority12.CollectionsPerFteNumerator =
          db.GetNullableDecimal(reader, 50);
        entities.DashboardStagingPriority12.CollectionsPerFteDenominator =
          db.GetNullableDecimal(reader, 51);
        entities.DashboardStagingPriority12.CollectionsPerFteAverage =
          db.GetNullableDecimal(reader, 52);
        entities.DashboardStagingPriority12.CollectionsPerFteRank =
          db.GetNullableInt32(reader, 53);
        entities.DashboardStagingPriority12.CasesPayingNumerator =
          db.GetNullableInt32(reader, 54);
        entities.DashboardStagingPriority12.CasesPayingDenominator =
          db.GetNullableInt32(reader, 55);
        entities.DashboardStagingPriority12.CasesPayingPercent =
          db.GetNullableDecimal(reader, 56);
        entities.DashboardStagingPriority12.CasesPayingRank =
          db.GetNullableInt32(reader, 57);
        entities.DashboardStagingPriority12.PepRank =
          db.GetNullableInt32(reader, 58);
        entities.DashboardStagingPriority12.ContractorNumber =
          db.GetNullableString(reader, 59);
        entities.DashboardStagingPriority12.PrevYrPepNumerator =
          db.GetNullableInt32(reader, 60);
        entities.DashboardStagingPriority12.PrevYrPepDenominator =
          db.GetNullableInt32(reader, 61);
        entities.DashboardStagingPriority12.PrevYrPepPercent =
          db.GetNullableDecimal(reader, 62);
        entities.DashboardStagingPriority12.PercentChgBetweenYrsPep =
          db.GetNullableDecimal(reader, 63);
        entities.DashboardStagingPriority12.PrevYrCaseNumerator =
          db.GetNullableInt32(reader, 64);
        entities.DashboardStagingPriority12.PrevYrCaseDenominator =
          db.GetNullableInt32(reader, 65);
        entities.DashboardStagingPriority12.CasesUndrOrdrPrevYrPct =
          db.GetNullableDecimal(reader, 66);
        entities.DashboardStagingPriority12.PctChgBtwenYrsCaseUndrOrdr =
          db.GetNullableDecimal(reader, 67);
        entities.DashboardStagingPriority12.PrevYrCurSupprtPaidNumtr =
          db.GetNullableDecimal(reader, 68);
        entities.DashboardStagingPriority12.PrevYrCurSupprtPaidDenom =
          db.GetNullableDecimal(reader, 69);
        entities.DashboardStagingPriority12.CurSupprtPdPrevYrPct =
          db.GetNullableDecimal(reader, 70);
        entities.DashboardStagingPriority12.PctChgBtwenYrsCurSuptPd =
          db.GetNullableDecimal(reader, 71);
        entities.DashboardStagingPriority12.PrvYrCasesPaidArrearsNumtr =
          db.GetNullableInt32(reader, 72);
        entities.DashboardStagingPriority12.PrvYrCasesPaidArrearsDenom =
          db.GetNullableInt32(reader, 73);
        entities.DashboardStagingPriority12.CasesPayArrearsPrvYrPct =
          db.GetNullableDecimal(reader, 74);
        entities.DashboardStagingPriority12.PctChgBtwenYrsCasesPayArrs =
          db.GetNullableDecimal(reader, 75);
        entities.DashboardStagingPriority12.Populated = true;

        return true;
      },
      () =>
      {
        entities.DashboardStagingPriority12.Populated = false;
      });
  }

  private IEnumerable<bool> ReadDashboardStagingPriority35()
  {
    return ReadEach("ReadDashboardStagingPriority35",
      null,
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
        entities.DashboardStagingPriority35.CasesWithEstReferral =
          db.GetNullableInt32(reader, 4);
        entities.DashboardStagingPriority35.CasesWithEnfReferral =
          db.GetNullableInt32(reader, 5);
        entities.DashboardStagingPriority35.FullTimeEquivalent =
          db.GetNullableDecimal(reader, 6);
        entities.DashboardStagingPriority35.NewOrdersEstablished =
          db.GetNullableInt32(reader, 7);
        entities.DashboardStagingPriority35.PaternitiesEstablished =
          db.GetNullableInt32(reader, 8);
        entities.DashboardStagingPriority35.CasesOpenedWithOrder =
          db.GetNullableInt32(reader, 9);
        entities.DashboardStagingPriority35.CasesOpenedWithoutOrders =
          db.GetNullableInt32(reader, 10);
        entities.DashboardStagingPriority35.CasesClosedWithOrders =
          db.GetNullableInt32(reader, 11);
        entities.DashboardStagingPriority35.CasesClosedWithoutOrders =
          db.GetNullableInt32(reader, 12);
        entities.DashboardStagingPriority35.Modifications =
          db.GetNullableInt32(reader, 13);
        entities.DashboardStagingPriority35.IncomeWithholdingsIssued =
          db.GetNullableInt32(reader, 14);
        entities.DashboardStagingPriority35.ContemptMotionFilings =
          db.GetNullableInt32(reader, 15);
        entities.DashboardStagingPriority35.ContemptOrderFilings =
          db.GetNullableInt32(reader, 16);
        entities.DashboardStagingPriority35.StypeCollectionAmount =
          db.GetNullableDecimal(reader, 17);
        entities.DashboardStagingPriority35.StypePercentOfTotal =
          db.GetNullableDecimal(reader, 18);
        entities.DashboardStagingPriority35.FtypeCollectionAmount =
          db.GetNullableDecimal(reader, 19);
        entities.DashboardStagingPriority35.FtypePercentOfTotal =
          db.GetNullableDecimal(reader, 20);
        entities.DashboardStagingPriority35.ItypeCollectionAmount =
          db.GetNullableDecimal(reader, 21);
        entities.DashboardStagingPriority35.ItypePercentOfTotal =
          db.GetNullableDecimal(reader, 22);
        entities.DashboardStagingPriority35.UtypeCollectionAmount =
          db.GetNullableDecimal(reader, 23);
        entities.DashboardStagingPriority35.UtypePercentOfTotal =
          db.GetNullableDecimal(reader, 24);
        entities.DashboardStagingPriority35.CtypeCollectionAmount =
          db.GetNullableDecimal(reader, 25);
        entities.DashboardStagingPriority35.CtypePercentOfTotal =
          db.GetNullableDecimal(reader, 26);
        entities.DashboardStagingPriority35.TotalCollectionAmount =
          db.GetNullableDecimal(reader, 27);
        entities.DashboardStagingPriority35.DaysToOrderEstblshmntNumer =
          db.GetNullableInt32(reader, 28);
        entities.DashboardStagingPriority35.DaysToOrderEstblshmntDenom =
          db.GetNullableInt32(reader, 29);
        entities.DashboardStagingPriority35.DaysToOrderEstblshmntAvg =
          db.GetNullableDecimal(reader, 30);
        entities.DashboardStagingPriority35.DaysToReturnOfSrvcNumer =
          db.GetNullableInt32(reader, 31);
        entities.DashboardStagingPriority35.DaysToReturnOfServiceDenom =
          db.GetNullableInt32(reader, 32);
        entities.DashboardStagingPriority35.DaysToReturnOfServiceAvg =
          db.GetNullableDecimal(reader, 33);
        entities.DashboardStagingPriority35.ReferralAging60To90Days =
          db.GetNullableInt32(reader, 34);
        entities.DashboardStagingPriority35.ReferralAging91To120Days =
          db.GetNullableInt32(reader, 35);
        entities.DashboardStagingPriority35.ReferralAging121To150Days =
          db.GetNullableInt32(reader, 36);
        entities.DashboardStagingPriority35.ReferralAging151PlusDays =
          db.GetNullableInt32(reader, 37);
        entities.DashboardStagingPriority35.DaysToIwoPaymentNumerator =
          db.GetNullableInt32(reader, 38);
        entities.DashboardStagingPriority35.DaysToIwoPaymentDenominator =
          db.GetNullableInt32(reader, 39);
        entities.DashboardStagingPriority35.DaysToIwoPaymentAverage =
          db.GetNullableDecimal(reader, 40);
        entities.DashboardStagingPriority35.ReferralsToLegalForEst =
          db.GetNullableInt32(reader, 41);
        entities.DashboardStagingPriority35.ReferralsToLegalForEnf =
          db.GetNullableInt32(reader, 42);
        entities.DashboardStagingPriority35.CaseloadCount =
          db.GetNullableInt32(reader, 43);
        entities.DashboardStagingPriority35.CasesOpened =
          db.GetNullableInt32(reader, 44);
        entities.DashboardStagingPriority35.NcpLocatesByAddress =
          db.GetNullableInt32(reader, 45);
        entities.DashboardStagingPriority35.NcpLocatesByEmployer =
          db.GetNullableInt32(reader, 46);
        entities.DashboardStagingPriority35.CaseClosures =
          db.GetNullableInt32(reader, 47);
        entities.DashboardStagingPriority35.CaseReviews =
          db.GetNullableInt32(reader, 48);
        entities.DashboardStagingPriority35.Petitions =
          db.GetNullableInt32(reader, 49);
        entities.DashboardStagingPriority35.CasesPayingArrearsDenominator =
          db.GetNullableInt32(reader, 50);
        entities.DashboardStagingPriority35.CasesPayingArrearsNumerator =
          db.GetNullableInt32(reader, 51);
        entities.DashboardStagingPriority35.CasesPayingArrearsPercent =
          db.GetNullableDecimal(reader, 52);
        entities.DashboardStagingPriority35.CasesPayingArrearsRank =
          db.GetNullableInt32(reader, 53);
        entities.DashboardStagingPriority35.CurrentSupportPaidFfytdDen =
          db.GetNullableDecimal(reader, 54);
        entities.DashboardStagingPriority35.CurrentSupportPaidFfytdNum =
          db.GetNullableDecimal(reader, 55);
        entities.DashboardStagingPriority35.CurrentSupportPaidFfytdPer =
          db.GetNullableDecimal(reader, 56);
        entities.DashboardStagingPriority35.CurrentSupportPaidFfytdRnk =
          db.GetNullableInt32(reader, 57);
        entities.DashboardStagingPriority35.CurrentSupportPaidMthDen =
          db.GetNullableDecimal(reader, 58);
        entities.DashboardStagingPriority35.CurrentSupportPaidMthNum =
          db.GetNullableDecimal(reader, 59);
        entities.DashboardStagingPriority35.CurrentSupportPaidMthPer =
          db.GetNullableDecimal(reader, 60);
        entities.DashboardStagingPriority35.CurrentSupportPaidMthRnk =
          db.GetNullableInt32(reader, 61);
        entities.DashboardStagingPriority35.Populated = true;

        return true;
      },
      () =>
      {
        entities.DashboardStagingPriority35.Populated = false;
      });
  }

  private bool ReadProgramProcessingInfo()
  {
    entities.ProgramProcessingInfo.Populated = false;

    return Read("ReadProgramProcessingInfo",
      (db, command) =>
      {
        db.SetString(command, "name", local.ProgramProcessingInfo.Name);
        db.SetDateTime(
          command, "createdTimestamp",
          local.ProgramProcessingInfo.CreatedTimestamp);
      },
      (db, reader) =>
      {
        entities.ProgramProcessingInfo.Name = db.GetString(reader, 0);
        entities.ProgramProcessingInfo.CreatedTimestamp =
          db.GetDateTime(reader, 1);
        entities.ProgramProcessingInfo.ParameterList =
          db.GetNullableString(reader, 2);
        entities.ProgramProcessingInfo.Populated = true;
      });
  }

  private IEnumerable<bool> ReadServiceProvider()
  {
    return ReadEach("ReadServiceProvider",
      null,
      (db, reader) =>
      {
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.UserId = db.GetString(reader, 1);
        entities.ServiceProvider.LastName = db.GetString(reader, 2);
        entities.ServiceProvider.FirstName = db.GetString(reader, 3);
        entities.ServiceProvider.MiddleInitial = db.GetString(reader, 4);
        entities.ServiceProvider.Populated = true;

        return true;
      },
      () =>
      {
        entities.ServiceProvider.Populated = false;
      });
  }

  private void UpdateDashboardName1()
  {
    var orgOrLastName = entities.ServiceProvider.LastName;
    var firstName = entities.ServiceProvider.FirstName;
    var middleInitial = entities.ServiceProvider.MiddleInitial;

    entities.DashboardName.Populated = false;
    Update("UpdateDashboardName1",
      (db, command) =>
      {
        db.SetNullableString(command, "orgOrLastName", orgOrLastName);
        db.SetNullableString(command, "firstName", firstName);
        db.SetNullableString(command, "middleInitial", middleInitial);
        db.SetString(command, "providerId", entities.DashboardName.ProviderId);
        db.SetString(
          command, "providerType", entities.DashboardName.ProviderType);
      });

    entities.DashboardName.OrgOrLastName = orgOrLastName;
    entities.DashboardName.FirstName = firstName;
    entities.DashboardName.MiddleInitial = middleInitial;
    entities.DashboardName.Populated = true;
  }

  private void UpdateDashboardName2()
  {
    var orgOrLastName = entities.CseOrganization.Name;

    entities.DashboardName.Populated = false;
    Update("UpdateDashboardName2",
      (db, command) =>
      {
        db.SetNullableString(command, "orgOrLastName", orgOrLastName);
        db.SetNullableString(command, "firstName", "");
        db.SetNullableString(command, "middleInitial", "");
        db.SetString(command, "providerId", entities.DashboardName.ProviderId);
        db.SetString(
          command, "providerType", entities.DashboardName.ProviderType);
      });

    entities.DashboardName.OrgOrLastName = orgOrLastName;
    entities.DashboardName.FirstName = "";
    entities.DashboardName.MiddleInitial = "";
    entities.DashboardName.Populated = true;
  }

  private void UpdateProgramProcessingInfo()
  {
    var parameterList = local.ProgramProcessingInfo.ParameterList ?? "";

    entities.ProgramProcessingInfo.Populated = false;
    Update("UpdateProgramProcessingInfo",
      (db, command) =>
      {
        db.SetNullableString(command, "parameterList", parameterList);
        db.SetString(command, "name", entities.ProgramProcessingInfo.Name);
        db.SetDateTime(
          command, "createdTimestamp",
          entities.ProgramProcessingInfo.CreatedTimestamp);
      });

    entities.ProgramProcessingInfo.ParameterList = parameterList;
    entities.ProgramProcessingInfo.Populated = true;
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
      /// A value of GlocalPeriodStart.
      /// </summary>
      public DateWorkArea GlocalPeriodStart
      {
        get => glocalPeriodStart ??= new();
        set => glocalPeriodStart = value;
      }

      /// <summary>
      /// A value of GlocalPeriodEnd.
      /// </summary>
      public DateWorkArea GlocalPeriodEnd
      {
        get => glocalPeriodEnd ??= new();
        set => glocalPeriodEnd = value;
      }

      /// <summary>
      /// A value of GlocalFyStart.
      /// </summary>
      public DateWorkArea GlocalFyStart
      {
        get => glocalFyStart ??= new();
        set => glocalFyStart = value;
      }

      /// <summary>
      /// A value of GlocalFyEnd.
      /// </summary>
      public DateWorkArea GlocalFyEnd
      {
        get => glocalFyEnd ??= new();
        set => glocalFyEnd = value;
      }

      /// <summary>
      /// A value of GlocalAuditRecs.
      /// </summary>
      public Common GlocalAuditRecs
      {
        get => glocalAuditRecs ??= new();
        set => glocalAuditRecs = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 24;

      private DateWorkArea? glocalPeriodStart;
      private DateWorkArea? glocalPeriodEnd;
      private DateWorkArea? glocalFyStart;
      private DateWorkArea? glocalFyEnd;
      private Common? glocalAuditRecs;
    }

    /// <summary>
    /// A value of Cq66220EffectiveFy.
    /// </summary>
    public Ocse157Verification Cq66220EffectiveFy
    {
      get => cq66220EffectiveFy ??= new();
      set => cq66220EffectiveFy = value;
    }

    /// <summary>
    /// A value of DashboardName.
    /// </summary>
    public DashboardName DashboardName
    {
      get => dashboardName ??= new();
      set => dashboardName = value;
    }

    /// <summary>
    /// A value of RestartGroupSubscript.
    /// </summary>
    public Common RestartGroupSubscript
    {
      get => restartGroupSubscript ??= new();
      set => restartGroupSubscript = value;
    }

    /// <summary>
    /// A value of Restart.
    /// </summary>
    public DashboardAuditData Restart
    {
      get => restart ??= new();
      set => restart = value;
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
    /// A value of Start.
    /// </summary>
    public DashboardAuditData Start
    {
      get => start ??= new();
      set => start = value;
    }

    /// <summary>
    /// A value of End.
    /// </summary>
    public DashboardAuditData End
    {
      get => end ??= new();
      set => end = value;
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
    /// A value of ExitStateWorkArea.
    /// </summary>
    public ExitStateWorkArea ExitStateWorkArea
    {
      get => exitStateWorkArea ??= new();
      set => exitStateWorkArea = value;
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
    /// A value of StartSubscript.
    /// </summary>
    public Common StartSubscript
    {
      get => startSubscript ??= new();
      set => startSubscript = value;
    }

    /// <summary>
    /// A value of ScriptCount.
    /// </summary>
    public Common ScriptCount
    {
      get => scriptCount ??= new();
      set => scriptCount = value;
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
    /// A value of TextWorkArea.
    /// </summary>
    public TextWorkArea TextWorkArea
    {
      get => textWorkArea ??= new();
      set => textWorkArea = value;
    }

    private Ocse157Verification? cq66220EffectiveFy;
    private DashboardName? dashboardName;
    private Common? restartGroupSubscript;
    private DashboardAuditData? restart;
    private DashboardAuditData? dashboardAuditData;
    private DashboardAuditData? start;
    private DashboardAuditData? end;
    private ProgramCheckpointRestart? programCheckpointRestart;
    private ProgramProcessingInfo? programProcessingInfo;
    private Array<LocalGroup>? local1;
    private ExitStateWorkArea? exitStateWorkArea;
    private EabFileHandling? eabFileHandling;
    private EabReportSend? eabReportSend;
    private Common? startSubscript;
    private Common? scriptCount;
    private DateWorkArea? dateWorkArea;
    private TextWorkArea? textWorkArea;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ContractorHistory.
    /// </summary>
    public ContractorHistory ContractorHistory
    {
      get => contractorHistory ??= new();
      set => contractorHistory = value;
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
    /// A value of DashboardName.
    /// </summary>
    public DashboardName DashboardName
    {
      get => dashboardName ??= new();
      set => dashboardName = value;
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
    /// A value of ServiceProvider.
    /// </summary>
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
    }

    /// <summary>
    /// A value of DashboardPerformanceMetrics.
    /// </summary>
    public DashboardPerformanceMetrics DashboardPerformanceMetrics
    {
      get => dashboardPerformanceMetrics ??= new();
      set => dashboardPerformanceMetrics = value;
    }

    /// <summary>
    /// A value of DashboardOutputMetrics.
    /// </summary>
    public DashboardOutputMetrics DashboardOutputMetrics
    {
      get => dashboardOutputMetrics ??= new();
      set => dashboardOutputMetrics = value;
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
    /// A value of DashboardStagingPriority35.
    /// </summary>
    public DashboardStagingPriority35 DashboardStagingPriority35
    {
      get => dashboardStagingPriority35 ??= new();
      set => dashboardStagingPriority35 = value;
    }

    private ContractorHistory? contractorHistory;
    private ProgramProcessingInfo? programProcessingInfo;
    private DashboardName? dashboardName;
    private CseOrganization? cseOrganization;
    private ServiceProvider? serviceProvider;
    private DashboardPerformanceMetrics? dashboardPerformanceMetrics;
    private DashboardOutputMetrics? dashboardOutputMetrics;
    private DashboardStagingPriority12? dashboardStagingPriority12;
    private DashboardStagingPriority35? dashboardStagingPriority35;
  }
#endregion
}
