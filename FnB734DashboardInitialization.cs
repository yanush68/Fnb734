// Program: FN_B734_DASHBOARD_INITIALIZATION, ID: 945116549, model: 746.
// Short name: SWE03077
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B734_DASHBOARD_INITIALIZATION.
/// </summary>
[Serializable]
[Program("SWE03077")]
public partial class FnB734DashboardInitialization: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B734_DASHBOARD_INITIALIZATION program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB734DashboardInitialization(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB734DashboardInitialization.
  /// </summary>
  public FnB734DashboardInitialization(IContext context, Import import,
    Export export):
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
    // 02/22/13  GVandy	CQ36547		Initial Development
    // 04/29/16  GVandy	CQ51409		Put report_month on Dummy placeholder audit 
    // records.
    // 02/17/17  GVandy	CQ56069		Restarts must use the processing date when the 
    // job
    // 					originally started.
    // 10/30/17  DDupree	CQ?????		Default audit logging to only the final weekly
    // 					run of the report month.
    // 11/17/17  GVandy	CQ60506		Correct audit logging logic.
    // 02/04/20  GVandy	CQ66220		Correlate with OCSE157 changes beginning in FY 
    // 2022.
    // 					These changes include only amounts in OCSE157
    // 					Lines 25, 27, and 29 that are both distributed
    // 					and disbursed.  Export a cutoff FY which defaults to
    // 					2022 but can be overridden with a code table value for testing.
    // ---------------------------------------------------------------------------------------------------
    // ------------------------------------------------------------------------------
    // -- Read the PPI record.
    // ------------------------------------------------------------------------------
    export.ProgramProcessingInfo.Name = global.UserId;
    UseReadProgramProcessingInfo();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // 02/04/20 GVandy CQ66220  Beginning in FY 2022, include only amounts that 
    // are both distributed
    // and disbursed.  Export a cutoff FY which defaults to 2022 but can be 
    // overridden with a code
    // table value for testing.
    if (ReadCodeValue())
    {
      export.Cq66220EffectiveFy.FiscalYear =
        (int?)StringToNumber(Substring(
          entities.CodeValue.Cdvalue, CodeValue.Cdvalue_MaxLength, 1, 4));
    }
    else
    {
      export.Cq66220EffectiveFy.FiscalYear = 2022;
    }

    // ------------------------------------------------------------------------------
    // -- Read for restart info.
    // ------------------------------------------------------------------------------
    export.ProgramCheckpointRestart.ProgramName = global.UserId;
    UseReadPgmCheckpointRestart();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // ------------------------------------------------------------------------------
    // -- This program may be restarted over several days at the end of each 
    // month
    // -- and the MPPI process date may have changed since the program 
    // originally
    // -- started.  If a restart occurs we will use the original process date 
    // saved
    // -- in the checkpoint info.
    // ------------------------------------------------------------------------------
    if (AsChar(export.ProgramCheckpointRestart.RestartInd) == 'Y')
    {
      // ------------------------------------------------------------------------------
      // -- Extract the process date.
      // ------------------------------------------------------------------------------
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
      export.ProgramProcessingInfo.ProcessDate =
        StringToDate(Substring(
          export.ProgramCheckpointRestart.RestartInfo, 250, 60, 10));
    }

    // ------------------------------------------------------------------------------
    // -- Open the error report.
    // ------------------------------------------------------------------------------
    local.EabFileHandling.Action = "OPEN";
    local.EabReportSend.ProcessDate = export.ProgramProcessingInfo.ProcessDate;
    local.EabReportSend.ProgramName = global.UserId;
    UseCabErrorReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_ERROR_RPT_A";

      return;
    }

    // ------------------------------------------------------------------------------
    // -- Open the control report.
    // ------------------------------------------------------------------------------
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error opening Control Report. Status = " + String
        (local.EabFileHandling.Status, EabFileHandling.Status_MaxLength);
      UseCabErrorReport1();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // ------------------------------------------------------------------------------
    // -- Determine if we're restarting.
    // ------------------------------------------------------------------------------
    if (AsChar(export.ProgramCheckpointRestart.RestartInd) == 'Y')
    {
      // ------------------------------------------------------------------------------
      // -- Extract the restart information.
      // ------------------------------------------------------------------------------
      // -------------------------------------------------------------------------------------
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
      export.DashboardAuditData.RunNumber =
        (int)StringToNumber(Substring(
          export.ProgramCheckpointRestart.RestartInfo, 250, 56, 3));
      export.RestartGroupSubscript.Count =
        (int)StringToNumber(Substring(
          export.ProgramCheckpointRestart.RestartInfo, 250, 71, 2));
      export.Restart.DashboardPriority =
        Substring(export.ProgramCheckpointRestart.RestartInfo, 81, 8);

      // -- Overlay the PPI info with the checkpointed info.
      // -- This will insure that we get the correct values for the restart.
      export.ProgramProcessingInfo.ParameterList =
        Substring(export.ProgramCheckpointRestart.RestartInfo, 250, 1, 55);
    }
    else
    {
      // -- Set export run number to max run number plus 1.
      ReadDashboardAuditData();
      ++export.DashboardAuditData.RunNumber;

      // -- Create a dummy entry in the audit table to hold this new run number.
      local.DashboardAuditData.ReportMonth = Now().Date.Year * 100 + Now
        ().Date.Month;

      try
      {
        CreateDashboardAuditData();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "DASHBOARD_AUDIT_AE";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "DASHBOARD_AUDIT_PV";

            break;
          default:
            throw;
        }
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "Error creating dashboard_audit_date in fn_b734_dashboard_initialization.";
        UseCabErrorReport1();
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }
    }

    // ------------------------------------------------------------------------------
    // -- Extract the PPI information.
    // ------------------------------------------------------------------------------
    // -----------------------------------------------------
    // Parameter List
    // Position  Description
    // --------  
    // -----------------------------------------
    // 001-010   Last Run Date (yyyy-mm-dd)
    // 011-011   Blank
    // 012-019   Starting Priority  (format 9-99xxxx)
    // 020-020   Blank
    // 021-028   Ending Priority  (format 9-99xxxx)
    // 029-029   Blank
    // 030-039   Starting Report Date (yyyy-mm-dd)
    // 040-040   Blank
    // 041-050   Ending Report Date (yyyy-mm-dd)
    // 051-051   Blank
    // 052-054   Fiscal Year Designation ("FFY" or "SFY")
    // 055-055   Audit Flag
    // -----------------------------------------------------
    if (!IsEmpty(Substring(export.ProgramProcessingInfo.ParameterList, 1, 10)) &&
      !Equal(export.ProgramProcessingInfo.ParameterList, 1, 10, "YYYY-MM-DD"))
    {
      local.LastRunDate.Date =
        StringToDate(
          Substring(export.ProgramProcessingInfo.ParameterList, 1, 10));
    }
    else
    {
      // -- Default to first day of PPI month.
      local.LastRunDate.Date =
        AddDays(export.ProgramProcessingInfo.ProcessDate,
        -(Day(export.ProgramProcessingInfo.ProcessDate) - 1));
    }

    if (!IsEmpty(Substring(export.ProgramProcessingInfo.ParameterList, 12, 8)))
    {
      export.Start.DashboardPriority =
        Substring(export.ProgramProcessingInfo.ParameterList, 12, 8);
    }
    else
    {
      export.Start.DashboardPriority = "0-00";
    }

    if (!IsEmpty(Substring(export.ProgramProcessingInfo.ParameterList, 21, 8)))
    {
      export.End.DashboardPriority =
        Substring(export.ProgramProcessingInfo.ParameterList, 21, 8);
    }
    else
    {
      export.End.DashboardPriority = "9-99";
    }

    if (!IsEmpty(Substring(export.ProgramProcessingInfo.ParameterList, 30, 10)) &&
      !Equal(export.ProgramProcessingInfo.ParameterList, 30, 10, "YYYY-MM-DD"))
    {
      local.EarliestPeriodStart.Date =
        StringToDate(Substring(
          export.ProgramProcessingInfo.ParameterList, 30, 10));

      // -- Insure starting date is the first day of a month.
      if (Day(local.EarliestPeriodStart.Date) != 1)
      {
        local.EarliestPeriodStart.Date =
          AddDays(local.EarliestPeriodStart.Date,
          -(Day(local.EarliestPeriodStart.Date) - 1));
      }
    }
    else
    {
      // -- Default to be first day of Month(last run date + 1 day).
      local.EarliestPeriodStart.Date = AddDays(local.LastRunDate.Date, 1);
      local.EarliestPeriodStart.Date =
        AddDays(local.EarliestPeriodStart.Date,
        -(Day(local.EarliestPeriodStart.Date) - 1));
    }

    if (!IsEmpty(Substring(export.ProgramProcessingInfo.ParameterList, 41, 10)) &&
      !Equal(export.ProgramProcessingInfo.ParameterList, 41, 10, "YYYY-MM-DD"))
    {
      local.LatestPeriodEnd.Date =
        StringToDate(Substring(
          export.ProgramProcessingInfo.ParameterList, 41, 10));
    }
    else
    {
      // -- Default to the last day of the PPI processing date month.
      local.LatestPeriodEnd.Date = export.ProgramProcessingInfo.ProcessDate;
      local.LatestPeriodEnd.Date =
        AddDays(local.LatestPeriodEnd.Date, -(Day(local.LatestPeriodEnd.Date) -
        1));
      local.LatestPeriodEnd.Date =
        AddDays(AddMonths(local.LatestPeriodEnd.Date, 1), -1);
    }

    // -- Ensure ending date is less or equal to processing date.
    if (Lt(export.ProgramProcessingInfo.ProcessDate, local.LatestPeriodEnd.Date))
    {
      local.LatestPeriodEnd.Date = export.ProgramProcessingInfo.ProcessDate;
    }

    if (!IsEmpty(Substring(export.ProgramProcessingInfo.ParameterList, 52, 3)))
    {
      local.FiscalYearDesignation.Text3 =
        Substring(export.ProgramProcessingInfo.ParameterList, 52, 3);
    }
    else
    {
      local.FiscalYearDesignation.Text3 = "FFY";
    }

    local.Audit.Flag =
      Substring(export.ProgramProcessingInfo.ParameterList, 55, 1);

    // ------------------------------------------------------------------------------
    // -- Create the export group reporting periods.
    // ------------------------------------------------------------------------------
    local.Start.Date = local.EarliestPeriodStart.Date;
    export.Export1.Index = -1;

    do
    {
      ++export.Export1.Index;
      export.Export1.CheckSize();

      if (export.Export1.Index >= Export.ExportGroup.Capacity)
      {
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "Reporting Periods Exceed Maximum of " + NumberToString
          (Export.ExportGroup.Capacity, 14, 2);
        UseCabErrorReport1();
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        break;
      }

      // -- Determine starting and ending dates and timestamps for the report 
      // period.
      local.End.Date = AddDays(AddMonths(local.Start.Date, 1), -1);

      if (Lt(export.ProgramProcessingInfo.ProcessDate, local.End.Date))
      {
        local.End.Date = export.ProgramProcessingInfo.ProcessDate;
      }

      if (Lt(local.LatestPeriodEnd.Date, local.End.Date))
      {
        // -- This IF fixes the scenario where an end date is entered on the 
        // MPPI parameter
        //    list.  We don't want the reporting period to exceed that end date.
        local.End.Date = local.LatestPeriodEnd.Date;
      }

      UseFnBuildReportTimeframe1();

      // -- Determine if audit logging should be enabled for the reporting 
      // period.
      switch(AsChar(local.Audit.Flag))
      {
        case 'Y':
          // -- Auditing is on for all report months.
          export.Export1.Update.GexportAuditRec.Flag = "Y";

          break;
        case 'N':
          // -- Auditing is off for all report months.
          export.Export1.Update.GexportAuditRec.Flag = "N";

          break;
        default:
          // -- By default, auditing is on for only the final weekly run of a 
          // report month.
          if (Year(AddDays(export.ProgramProcessingInfo.ProcessDate, 1)) > Year
            (export.Export1.Item.GexportPeriodEnd.Date) || Year
            (AddDays(export.ProgramProcessingInfo.ProcessDate, 1)) == Year
            (export.Export1.Item.GexportPeriodEnd.Date) && Month
            (AddDays(export.ProgramProcessingInfo.ProcessDate, 1)) > Month
            (export.Export1.Item.GexportPeriodEnd.Date))
          {
            // -- Auditing is on for the final run of the month.
            export.Export1.Update.GexportAuditRec.Flag = "Y";
          }
          else
          {
            // -- Auditing is off for all but the final run of the month.
            export.Export1.Update.GexportAuditRec.Flag = "N";
          }

          break;
      }

      // -- Determine FY start and end dates/timestamps for this report period.
      switch(TrimEnd(local.FiscalYearDesignation.Text3))
      {
        case "FFY":
          local.Start.Date =
            IntToDate(Year(export.Export1.Item.GexportPeriodStart.Date) * 10000
            + 1001);

          if (Lt(export.Export1.Item.GexportPeriodStart.Date, local.Start.Date))
          {
            local.Start.Date = AddYears(local.Start.Date, -1);
          }

          break;
        case "SFY":
          local.Start.Date =
            IntToDate(Year(export.Export1.Item.GexportPeriodStart.Date) * 10000
            + 701);

          if (Lt(export.Export1.Item.GexportPeriodStart.Date, local.Start.Date))
          {
            local.Start.Date = AddYears(local.Start.Date, -1);
          }

          break;
        default:
          break;
      }

      local.End.Date = AddDays(AddYears(local.Start.Date, 1), -1);
      UseFnBuildReportTimeframe2();

      // -- Increment starting date by 1 month.
      local.Start.Date =
        AddMonths(export.Export1.Item.GexportPeriodStart.Date, 1);
    }
    while(Lt(export.Export1.Item.GexportPeriodEnd.Date,
      local.LatestPeriodEnd.Date));

    // ------------------------------------------------------------------------------
    // -- Log CQ66220 Effective FY to the control report.
    // ------------------------------------------------------------------------------
    for(local.Common.Count = 1; local.Common.Count <= 4; ++local.Common.Count)
    {
      if (local.Common.Count == 2)
      {
        local.EabReportSend.RptDetail = "CQ66220 Effective FY     : " + NumberToString
          (export.Cq66220EffectiveFy.FiscalYear ?? 0, 12, 4);
      }
      else
      {
        local.EabReportSend.RptDetail = "";
      }

      local.EabFileHandling.Action = "WRITE";
      UseCabControlReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "Error encountered writing CQ66220 FY to the control report.";
        UseCabErrorReport1();
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }
    }

    // ------------------------------------------------------------------------------
    // -- Log restart data to the control report.
    // ------------------------------------------------------------------------------
    if (AsChar(export.ProgramCheckpointRestart.RestartInd) == 'Y')
    {
      for(local.Common.Count = 1; local.Common.Count <= 10; ++
        local.Common.Count)
      {
        switch(local.Common.Count)
        {
          case 1:
            local.EabReportSend.RptDetail = "Program is Restarting at...";

            break;
          case 2:
            local.ProcessDate.Date = export.ProgramProcessingInfo.ProcessDate;
            UseCabDate2TextWithHyphens1();
            local.EabReportSend.RptDetail = "     Processing Date: " + String
              (local.TextWorkArea.Text10, TextWorkArea.Text10_MaxLength);

            break;
          case 3:
            local.EabReportSend.RptDetail = "       Report Period: " + NumberToString
              (export.RestartGroupSubscript.Count, 14, 2);

            break;
          case 4:
            local.EabReportSend.RptDetail = "       Priority     : " + String
              (export.Restart.DashboardPriority,
              DashboardAuditData.DashboardPriority_MaxLength);

            break;
          case 5:
            local.EabReportSend.RptDetail = "       Restart Data...";

            break;
          case 6:
            local.EabReportSend.RptDetail = "        Pos 001-080 : " + Substring
              (export.ProgramCheckpointRestart.RestartInfo, 250, 1, 80);

            break;
          case 7:
            local.EabReportSend.RptDetail = "        Pos 081-160 : " + Substring
              (export.ProgramCheckpointRestart.RestartInfo, 250, 81, 80);

            break;
          case 8:
            local.EabReportSend.RptDetail = "        Pos 161-240 : " + Substring
              (export.ProgramCheckpointRestart.RestartInfo, 250, 161, 80);

            break;
          default:
            local.EabReportSend.RptDetail = "";

            break;
        }

        local.EabFileHandling.Action = "WRITE";
        UseCabControlReport1();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "Error encountered writing Restat Info to the control report.";
          UseCabErrorReport1();
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }
      }
    }

    // ------------------------------------------------------------------------------
    // -- Log PPI data to the control report.
    // ------------------------------------------------------------------------------
    for(local.Common.Count = 1; local.Common.Count <= 11; ++local.Common.Count)
    {
      switch(local.Common.Count)
      {
        case 1:
          local.EabReportSend.RptDetail = "PPI Parameter Info...";

          break;
        case 2:
          local.EabReportSend.RptDetail = "       Starting Priority : " + String
            (export.Start.DashboardPriority,
            DashboardAuditData.DashboardPriority_MaxLength);

          break;
        case 3:
          local.EabReportSend.RptDetail = "       Ending Priority   : " + String
            (export.End.DashboardPriority,
            DashboardAuditData.DashboardPriority_MaxLength);

          break;
        case 4:
          UseCabDate2TextWithHyphens4();
          local.EabReportSend.RptDetail = "       Starting Rpt Date : " + String
            (local.TextWorkArea.Text10, TextWorkArea.Text10_MaxLength);

          break;
        case 5:
          UseCabDate2TextWithHyphens3();
          local.EabReportSend.RptDetail = "       Ending Rpt Date   : " + String
            (local.TextWorkArea.Text10, TextWorkArea.Text10_MaxLength);

          break;
        case 6:
          local.EabReportSend.RptDetail = "       Fiscal Year       : " + String
            (local.FiscalYearDesignation.Text3, WorkArea.Text3_MaxLength);

          break;
        case 7:
          if (IsEmpty(local.Audit.Flag))
          {
            local.EabReportSend.RptDetail = "       Audit Flag        : " + "EOM Only (Default)";
          }
          else
          {
            local.EabReportSend.RptDetail = "       Audit Flag        : " + String
              (local.Audit.Flag, Common.Flag_MaxLength);
          }

          break;
        case 8:
          local.EabReportSend.RptDetail = "       Run Number        : " + NumberToString
            (export.DashboardAuditData.RunNumber, 13, 3);

          break;
        case 9:
          UseCabDate2TextWithHyphens2();
          local.EabReportSend.RptDetail = "       Last Run Date     : " + String
            (local.TextWorkArea.Text10, TextWorkArea.Text10_MaxLength);

          break;
        default:
          local.EabReportSend.RptDetail = "";

          break;
      }

      local.EabFileHandling.Action = "WRITE";
      UseCabControlReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "Error encountered writing PPI Parms to the control report.";
        UseCabErrorReport1();
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }
    }

    // ------------------------------------------------------------------------------
    // -- Ensure ending date is greater or equal to starting date.
    // ------------------------------------------------------------------------------
    if (Lt(local.LatestPeriodEnd.Date, local.EarliestPeriodStart.Date))
    {
      local.EabReportSend.RptDetail =
        "ERROR... Starting Report Date Must Be Prior to Ending Report Date.  See Control Report.";
      local.EabFileHandling.Action = "WRITE";
      UseCabErrorReport1();
      local.EabReportSend.RptDetail =
        "ERROR... Starting Report Date Must Be Prior to Ending Report Date.";
      UseCabControlReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "Error encountered writing PPI Parms to the control report.";
        UseCabErrorReport1();
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // ------------------------------------------------------------------------------
    // -- Log Report Period Info to the control report.
    // ------------------------------------------------------------------------------
    export.Export1.Index = 0;

    for(var limit = export.Export1.Count; export.Export1.Index < limit; ++
      export.Export1.Index)
    {
      if (!export.Export1.CheckSize())
      {
        break;
      }

      for(local.Common.Count = 1; local.Common.Count <= 8; ++local.Common.Count)
      {
        switch(local.Common.Count)
        {
          case 1:
            local.EabReportSend.RptDetail = "Report Period:  " + NumberToString
              (export.Export1.Index + 1, 14, 2) + "   Year/Month: " + NumberToString
              (export.Export1.Item.GexportPeriodStart.YearMonth, 10, 6);
            local.EabReportSend.RptDetail =
              TrimEnd(local.EabReportSend.RptDetail) + "   Audit Data Created: " +
              String
              (export.Export1.Item.GexportAuditRec.Flag, Common.Flag_MaxLength);

            break;
          case 2:
            local.EabReportSend.RptDetail = "";

            break;
          case 3:
            local.EabReportSend.RptDetail =
              "                     From Date    From Timestamp               To Date      To Timestamp";

            break;
          case 4:
            local.EabReportSend.RptDetail =
              "                     ----------   --------------------------   ----------   --------------------------";

            break;
          case 5:
            UseCabDate2TextWithHyphens5();
            local.EabReportSend.RptDetail = "     Report Period   " + String
              (local.TextWorkArea.Text10, TextWorkArea.Text10_MaxLength);
            local.BatchTimestampWorkArea.IefTimestamp =
              export.Export1.Item.GexportPeriodStart.Timestamp;
            local.BatchTimestampWorkArea.TextTimestamp = "";
            UseLeCabConvertTimestamp();
            local.EabReportSend.RptDetail =
              TrimEnd(local.EabReportSend.RptDetail) + "   " + String
              (local.BatchTimestampWorkArea.TextTimestamp,
              BatchTimestampWorkArea.TextTimestamp_MaxLength);
            UseCabDate2TextWithHyphens6();
            local.EabReportSend.RptDetail =
              TrimEnd(local.EabReportSend.RptDetail) + "   " + String
              (local.TextWorkArea.Text10, TextWorkArea.Text10_MaxLength);
            local.BatchTimestampWorkArea.IefTimestamp =
              export.Export1.Item.GexportPeriodEnd.Timestamp;
            local.BatchTimestampWorkArea.TextTimestamp = "";
            UseLeCabConvertTimestamp();
            local.EabReportSend.RptDetail =
              TrimEnd(local.EabReportSend.RptDetail) + "   " + String
              (local.BatchTimestampWorkArea.TextTimestamp,
              BatchTimestampWorkArea.TextTimestamp_MaxLength);

            break;
          case 6:
            UseCabDate2TextWithHyphens7();
            local.EabReportSend.RptDetail = "       Fiscal Year   " + String
              (local.TextWorkArea.Text10, TextWorkArea.Text10_MaxLength);
            local.BatchTimestampWorkArea.IefTimestamp =
              export.Export1.Item.GexportFyStart.Timestamp;
            local.BatchTimestampWorkArea.TextTimestamp = "";
            UseLeCabConvertTimestamp();
            local.EabReportSend.RptDetail =
              TrimEnd(local.EabReportSend.RptDetail) + "   " + String
              (local.BatchTimestampWorkArea.TextTimestamp,
              BatchTimestampWorkArea.TextTimestamp_MaxLength);
            UseCabDate2TextWithHyphens8();
            local.EabReportSend.RptDetail =
              TrimEnd(local.EabReportSend.RptDetail) + "   " + String
              (local.TextWorkArea.Text10, TextWorkArea.Text10_MaxLength);
            local.BatchTimestampWorkArea.IefTimestamp =
              export.Export1.Item.GexportFyEnd.Timestamp;
            local.BatchTimestampWorkArea.TextTimestamp = "";
            UseLeCabConvertTimestamp();
            local.EabReportSend.RptDetail =
              TrimEnd(local.EabReportSend.RptDetail) + "   " + String
              (local.BatchTimestampWorkArea.TextTimestamp,
              BatchTimestampWorkArea.TextTimestamp_MaxLength);

            break;
          default:
            local.EabReportSend.RptDetail = "";

            break;
        }

        local.EabFileHandling.Action = "WRITE";
        UseCabControlReport1();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "Error encountered writing PPI Parms to the control report.";
          UseCabErrorReport1();
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }
      }
    }

    export.Export1.CheckIndex();

    if (AsChar(export.ProgramCheckpointRestart.RestartInd) != 'Y')
    {
      // -------------------------------------------------------------------------------------
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
      export.ProgramCheckpointRestart.RestartInfo =
        export.ProgramProcessingInfo.ParameterList;

      // -- Add run number to the global checkpoint info.
      export.ProgramCheckpointRestart.RestartInfo =
        Substring(export.ProgramCheckpointRestart.RestartInfo, 250, 1, 55) + NumberToString
        (export.DashboardAuditData.RunNumber, 13, 3);

      // -- Add processing date to the global checkpoint info.
      local.ProcessDate.Date = export.ProgramProcessingInfo.ProcessDate;
      UseCabDate2TextWithHyphens1();
      export.ProgramCheckpointRestart.RestartInfo =
        Substring(export.ProgramCheckpointRestart.RestartInfo, 250, 1, 59) + String
        (local.TextWorkArea.Text10, TextWorkArea.Text10_MaxLength);
    }
  }

  private static void MoveBatchTimestampWorkArea(BatchTimestampWorkArea source,
    BatchTimestampWorkArea target)
  {
    target.IefTimestamp = source.IefTimestamp;
    target.TextTimestamp = source.TextTimestamp;
  }

  private static void MoveDateWorkArea(DateWorkArea source, DateWorkArea target)
  {
    target.Date = source.Date;
    target.Timestamp = source.Timestamp;
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private static void MoveProgramCheckpointRestart(
    ProgramCheckpointRestart source, ProgramCheckpointRestart target)
  {
    target.UpdateFrequencyCount = source.UpdateFrequencyCount;
    target.ReadFrequencyCount = source.ReadFrequencyCount;
    target.CheckpointCount = source.CheckpointCount;
    target.LastCheckpointTimestamp = source.LastCheckpointTimestamp;
    target.RestartInd = source.RestartInd;
    target.RestartInfo = source.RestartInfo;
  }

  private void UseCabControlReport1()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    context.Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport2()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    context.Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabDate2TextWithHyphens1()
  {
    var useImport = new CabDate2TextWithHyphens.Import();
    var useExport = new CabDate2TextWithHyphens.Export();

    useImport.DateWorkArea.Date = local.ProcessDate.Date;

    context.Call(CabDate2TextWithHyphens.Execute, useImport, useExport);

    local.TextWorkArea.Text10 = useExport.TextWorkArea.Text10;
  }

  private void UseCabDate2TextWithHyphens2()
  {
    var useImport = new CabDate2TextWithHyphens.Import();
    var useExport = new CabDate2TextWithHyphens.Export();

    useImport.DateWorkArea.Date = local.LastRunDate.Date;

    context.Call(CabDate2TextWithHyphens.Execute, useImport, useExport);

    local.TextWorkArea.Text10 = useExport.TextWorkArea.Text10;
  }

  private void UseCabDate2TextWithHyphens3()
  {
    var useImport = new CabDate2TextWithHyphens.Import();
    var useExport = new CabDate2TextWithHyphens.Export();

    useImport.DateWorkArea.Date = local.LatestPeriodEnd.Date;

    context.Call(CabDate2TextWithHyphens.Execute, useImport, useExport);

    local.TextWorkArea.Text10 = useExport.TextWorkArea.Text10;
  }

  private void UseCabDate2TextWithHyphens4()
  {
    var useImport = new CabDate2TextWithHyphens.Import();
    var useExport = new CabDate2TextWithHyphens.Export();

    useImport.DateWorkArea.Date = local.EarliestPeriodStart.Date;

    context.Call(CabDate2TextWithHyphens.Execute, useImport, useExport);

    local.TextWorkArea.Text10 = useExport.TextWorkArea.Text10;
  }

  private void UseCabDate2TextWithHyphens5()
  {
    var useImport = new CabDate2TextWithHyphens.Import();
    var useExport = new CabDate2TextWithHyphens.Export();

    useImport.DateWorkArea.Date = export.Export1.Item.GexportPeriodStart.Date;

    context.Call(CabDate2TextWithHyphens.Execute, useImport, useExport);

    local.TextWorkArea.Text10 = useExport.TextWorkArea.Text10;
  }

  private void UseCabDate2TextWithHyphens6()
  {
    var useImport = new CabDate2TextWithHyphens.Import();
    var useExport = new CabDate2TextWithHyphens.Export();

    useImport.DateWorkArea.Date = export.Export1.Item.GexportPeriodEnd.Date;

    context.Call(CabDate2TextWithHyphens.Execute, useImport, useExport);

    local.TextWorkArea.Text10 = useExport.TextWorkArea.Text10;
  }

  private void UseCabDate2TextWithHyphens7()
  {
    var useImport = new CabDate2TextWithHyphens.Import();
    var useExport = new CabDate2TextWithHyphens.Export();

    useImport.DateWorkArea.Date = export.Export1.Item.GexportFyStart.Date;

    context.Call(CabDate2TextWithHyphens.Execute, useImport, useExport);

    local.TextWorkArea.Text10 = useExport.TextWorkArea.Text10;
  }

  private void UseCabDate2TextWithHyphens8()
  {
    var useImport = new CabDate2TextWithHyphens.Import();
    var useExport = new CabDate2TextWithHyphens.Export();

    useImport.DateWorkArea.Date = export.Export1.Item.GexportFyEnd.Date;

    context.Call(CabDate2TextWithHyphens.Execute, useImport, useExport);

    local.TextWorkArea.Text10 = useExport.TextWorkArea.Text10;
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    context.Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    context.Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseFnBuildReportTimeframe1()
  {
    var useImport = new FnBuildReportTimeframe.Import();
    var useExport = new FnBuildReportTimeframe.Export();

    useImport.Start.Date = local.Start.Date;
    useImport.End.Date = local.End.Date;

    context.Call(FnBuildReportTimeframe.Execute, useImport, useExport);

    export.Export1.Update.GexportPeriodStart.Assign(useExport.Start);
    MoveDateWorkArea(useExport.End, export.Export1.Update.GexportPeriodEnd);
  }

  private void UseFnBuildReportTimeframe2()
  {
    var useImport = new FnBuildReportTimeframe.Import();
    var useExport = new FnBuildReportTimeframe.Export();

    useImport.Start.Date = local.Start.Date;
    useImport.End.Date = local.End.Date;

    context.Call(FnBuildReportTimeframe.Execute, useImport, useExport);

    MoveDateWorkArea(useExport.Start, export.Export1.Update.GexportFyStart);
    MoveDateWorkArea(useExport.End, export.Export1.Update.GexportFyEnd);
  }

  private void UseLeCabConvertTimestamp()
  {
    var useImport = new LeCabConvertTimestamp.Import();
    var useExport = new LeCabConvertTimestamp.Export();

    MoveBatchTimestampWorkArea(local.BatchTimestampWorkArea,
      useImport.BatchTimestampWorkArea);

    context.Call(LeCabConvertTimestamp.Execute, useImport, useExport);

    MoveBatchTimestampWorkArea(useExport.BatchTimestampWorkArea,
      local.BatchTimestampWorkArea);
  }

  private void UseReadPgmCheckpointRestart()
  {
    var useImport = new ReadPgmCheckpointRestart.Import();
    var useExport = new ReadPgmCheckpointRestart.Export();

    useImport.ProgramCheckpointRestart.ProgramName =
      export.ProgramCheckpointRestart.ProgramName;

    context.Call(ReadPgmCheckpointRestart.Execute, useImport, useExport);

    MoveProgramCheckpointRestart(useExport.ProgramCheckpointRestart,
      export.ProgramCheckpointRestart);
  }

  private void UseReadProgramProcessingInfo()
  {
    var useImport = new ReadProgramProcessingInfo.Import();
    var useExport = new ReadProgramProcessingInfo.Export();

    useImport.ProgramProcessingInfo.Name = export.ProgramProcessingInfo.Name;

    context.Call(ReadProgramProcessingInfo.Execute, useImport, useExport);

    export.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
  }

  private void CreateDashboardAuditData()
  {
    var reportMonth = local.DashboardAuditData.ReportMonth;
    var dashboardPriority = "0-00";
    var runNumber = export.DashboardAuditData.RunNumber;
    var createdTimestamp = Now();

    entities.New1.Populated = false;
    Update("CreateDashboardAuditData",
      (db, command) =>
      {
        db.SetInt32(command, "reportMonth", reportMonth);
        db.SetString(command, "dashboardPriority", dashboardPriority);
        db.SetInt32(command, "runNumber", runNumber);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableInt32(command, "office", 0);
        db.SetNullableString(command, "judicialDistrict", "");
        db.SetNullableString(command, "workerId", "");
        db.SetNullableString(command, "caseNumber", "");
        db.SetNullableString(command, "standardNumber", "");
        db.SetNullableInt32(command, "fte", 0);
        db.SetNullableDecimal(command, "collectionAmt", 0M);
        db.SetNullableString(command, "collAppliedToCd", "");
        db.SetNullableDate(command, "collCreatedDt", null);
        db.SetNullableString(command, "debtType", "");
        db.SetNullableInt32(command, "legalRefNo", 0);
      });

    entities.New1.ReportMonth = reportMonth;
    entities.New1.DashboardPriority = dashboardPriority;
    entities.New1.RunNumber = runNumber;
    entities.New1.CreatedTimestamp = createdTimestamp;
    entities.New1.Populated = true;
  }

  private bool ReadCodeValue()
  {
    entities.CodeValue.Populated = false;

    return Read("ReadCodeValue",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate", export.ProgramProcessingInfo.ProcessDate);
      },
      (db, reader) =>
      {
        entities.CodeValue.Id = db.GetInt32(reader, 0);
        entities.CodeValue.CodId = db.GetNullableInt32(reader, 1);
        entities.CodeValue.Cdvalue = db.GetString(reader, 2);
        entities.CodeValue.EffectiveDate = db.GetDate(reader, 3);
        entities.CodeValue.ExpirationDate = db.GetDate(reader, 4);
        entities.CodeValue.Populated = true;
      });
  }

  private bool ReadDashboardAuditData()
  {
    return Read("ReadDashboardAuditData",
      null,
      (db, reader) =>
      {
        export.DashboardAuditData.RunNumber = db.GetInt32(reader, 0);
      });
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
    /// <summary>A ExportGroup group.</summary>
    [Serializable]
    public class ExportGroup
    {
      /// <summary>
      /// A value of GexportPeriodStart.
      /// </summary>
      public DateWorkArea GexportPeriodStart
      {
        get => gexportPeriodStart ??= new();
        set => gexportPeriodStart = value;
      }

      /// <summary>
      /// A value of GexportPeriodEnd.
      /// </summary>
      public DateWorkArea GexportPeriodEnd
      {
        get => gexportPeriodEnd ??= new();
        set => gexportPeriodEnd = value;
      }

      /// <summary>
      /// A value of GexportFyStart.
      /// </summary>
      public DateWorkArea GexportFyStart
      {
        get => gexportFyStart ??= new();
        set => gexportFyStart = value;
      }

      /// <summary>
      /// A value of GexportFyEnd.
      /// </summary>
      public DateWorkArea GexportFyEnd
      {
        get => gexportFyEnd ??= new();
        set => gexportFyEnd = value;
      }

      /// <summary>
      /// A value of GexportAuditRec.
      /// </summary>
      public Common GexportAuditRec
      {
        get => gexportAuditRec ??= new();
        set => gexportAuditRec = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 24;

      private DateWorkArea? gexportPeriodStart;
      private DateWorkArea? gexportPeriodEnd;
      private DateWorkArea? gexportFyStart;
      private DateWorkArea? gexportFyEnd;
      private Common? gexportAuditRec;
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
    /// A value of End.
    /// </summary>
    public DashboardAuditData End
    {
      get => end ??= new();
      set => end = value;
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
    /// Gets a value of Export1.
    /// </summary>
    [JsonIgnore]
    public Array<ExportGroup> Export1 =>
      export1 ??= new(ExportGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Export1 for json serialization.
    /// </summary>
    [JsonPropertyName("export1")]
    [Computed]
    public IList<ExportGroup>? Export1_Json
    {
      get => export1;
      set => Export1.Assign(value);
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
    private Common? restartGroupSubscript;
    private DashboardAuditData? restart;
    private DashboardAuditData? end;
    private DashboardAuditData? start;
    private ProgramProcessingInfo? programProcessingInfo;
    private ProgramCheckpointRestart? programCheckpointRestart;
    private Array<ExportGroup>? export1;
    private Ocse157Verification? cq66220EffectiveFy;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Audit.
    /// </summary>
    public Common Audit
    {
      get => audit ??= new();
      set => audit = value;
    }

    /// <summary>
    /// A value of ProcessDate.
    /// </summary>
    public DateWorkArea ProcessDate
    {
      get => processDate ??= new();
      set => processDate = value;
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
    /// A value of Blank.
    /// </summary>
    public WorkArea Blank
    {
      get => blank ??= new();
      set => blank = value;
    }

    /// <summary>
    /// A value of BatchTimestampWorkArea.
    /// </summary>
    public BatchTimestampWorkArea BatchTimestampWorkArea
    {
      get => batchTimestampWorkArea ??= new();
      set => batchTimestampWorkArea = value;
    }

    /// <summary>
    /// A value of LastRunDate.
    /// </summary>
    public DateWorkArea LastRunDate
    {
      get => lastRunDate ??= new();
      set => lastRunDate = value;
    }

    /// <summary>
    /// A value of TextWorkArea.
    /// </summary>
    public TextWorkArea TextWorkArea
    {
      get => textWorkArea ??= new();
      set => textWorkArea = value;
    }

    /// <summary>
    /// A value of End.
    /// </summary>
    public DateWorkArea End
    {
      get => end ??= new();
      set => end = value;
    }

    /// <summary>
    /// A value of Start.
    /// </summary>
    public DateWorkArea Start
    {
      get => start ??= new();
      set => start = value;
    }

    /// <summary>
    /// A value of FiscalYearDesignation.
    /// </summary>
    public WorkArea FiscalYearDesignation
    {
      get => fiscalYearDesignation ??= new();
      set => fiscalYearDesignation = value;
    }

    /// <summary>
    /// A value of LatestPeriodEnd.
    /// </summary>
    public DateWorkArea LatestPeriodEnd
    {
      get => latestPeriodEnd ??= new();
      set => latestPeriodEnd = value;
    }

    /// <summary>
    /// A value of EarliestPeriodStart.
    /// </summary>
    public DateWorkArea EarliestPeriodStart
    {
      get => earliestPeriodStart ??= new();
      set => earliestPeriodStart = value;
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
    /// A value of Status.
    /// </summary>
    public EabFileHandling Status
    {
      get => status ??= new();
      set => status = value;
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
    /// A value of ExitStateWorkArea.
    /// </summary>
    public ExitStateWorkArea ExitStateWorkArea
    {
      get => exitStateWorkArea ??= new();
      set => exitStateWorkArea = value;
    }

    /// <summary>
    /// A value of Common.
    /// </summary>
    public Common Common
    {
      get => common ??= new();
      set => common = value;
    }

    private Common? audit;
    private DateWorkArea? processDate;
    private DashboardAuditData? dashboardAuditData;
    private WorkArea? blank;
    private BatchTimestampWorkArea? batchTimestampWorkArea;
    private DateWorkArea? lastRunDate;
    private TextWorkArea? textWorkArea;
    private DateWorkArea? end;
    private DateWorkArea? start;
    private WorkArea? fiscalYearDesignation;
    private DateWorkArea? latestPeriodEnd;
    private DateWorkArea? earliestPeriodStart;
    private EabReportSend? eabReportSend;
    private EabFileHandling? status;
    private EabFileHandling? eabFileHandling;
    private ExitStateWorkArea? exitStateWorkArea;
    private Common? common;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of New1.
    /// </summary>
    public DashboardAuditData New1
    {
      get => new1 ??= new();
      set => new1 = value;
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
    /// A value of CodeValue.
    /// </summary>
    public CodeValue CodeValue
    {
      get => codeValue ??= new();
      set => codeValue = value;
    }

    /// <summary>
    /// A value of Code.
    /// </summary>
    public Code Code
    {
      get => code ??= new();
      set => code = value;
    }

    private DashboardAuditData? new1;
    private DashboardAuditData? dashboardAuditData;
    private CodeValue? codeValue;
    private Code? code;
  }
#endregion
}
