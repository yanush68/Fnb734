// Program: FN_B734_PRIORITY_5_26, ID: 1902564405, model: 746.
// Short name: SWE03757
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
/// A program: FN_B734_PRIORITY_5_26.
/// </para>
/// <para>
/// Priority 3-12: Contempt Order Filings
/// </para>
/// </summary>
[Serializable]
[Program("SWE03757")]
public partial class FnB734Priority526: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B734_PRIORITY_5_26 program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB734Priority526(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB734Priority526.
  /// </summary>
  public FnB734Priority526(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // -------------------------------------------------------------------------------------
    // Priority 5-26: Contempt Order Filings
    // -------------------------------------------------------------------------------------
    // Count of all court orders with an Order for Contempt filed within the 
    // report period.  Caseworker statistics roll up into an Office, Into a
    // Judicial District.
    // All out of state orders will be excluded.
    // Report Level:  Caseworker
    // Report Period: Month
    // Contempt Order Filings by Caseworker
    // 1)	Count O class legal action with file date entered in current report 
    // period.
    // 2)	Count only following action taken: CONTEMPJ
    // 3)	Credit caseworker who created the Contempt order
    // -------------------------------------------------------------------------------------
    MoveDashboardAuditData2(import.DashboardAuditData,
      local.InitializedDashboardAuditData);
    MoveProgramCheckpointRestart(import.ProgramCheckpointRestart,
      local.ProgramCheckpointRestart);
    local.ReportingPeriod.Index = -1;

    ++local.ReportingPeriod.Index;
    local.ReportingPeriod.CheckSize();

    local.ReportingPeriod.Update.BeingDate.Timestamp =
      import.ReportStartDate.Timestamp;
    local.Begin.Year = Year(import.ReportStartDate.Date);
    local.Begin.Month = Month(import.ReportStartDate.Date);
    local.Begin.Day = 1;
    local.DateWorkAttributes.TextYear = NumberToString(local.Begin.Year, 12, 4);
    local.DateWorkAttributes.TextMonth =
      NumberToString(local.Begin.Month, 14, 2);
    local.DateWorkAttributes.TextDay = NumberToString(local.Begin.Day, 14, 2);
    local.DateWorkAttributes.TextDate10Char =
      String(local.DateWorkAttributes.TextYear,
      DateWorkAttributes.TextYear_MaxLength) + "-" + String
      (local.DateWorkAttributes.TextMonth,
      DateWorkAttributes.TextMonth_MaxLength) + "-" + String
      (local.DateWorkAttributes.TextDay, DateWorkAttributes.TextDay_MaxLength);
    local.ReportingPeriod.Update.BeingDate.Date =
      StringToDate(local.DateWorkAttributes.TextDate10Char);
    local.ReportingPeriod.Update.DashboardAuditData.ReportMonth =
      (int)StringToNumber(String(
        local.DateWorkAttributes.TextYear,
      DateWorkAttributes.TextYear_MaxLength) +
      String(local.DateWorkAttributes.TextMonth,
      DateWorkAttributes.TextMonth_MaxLength));
    local.ReportingPeriod.Update.EndDate.Date = import.ReportEndDate.Date;
    local.ReportingPeriod.Update.EndDate.Timestamp = Now();

    ++local.ReportingPeriod.Index;
    local.ReportingPeriod.CheckSize();

    local.ReportingPeriod.Update.BeingDate.Timestamp =
      AddMonths(import.ReportStartDate.Timestamp, -1);
    local.Begin.Year = Year(AddMonths(import.ReportStartDate.Date, -1));
    local.Begin.Month = Month(AddMonths(import.ReportStartDate.Date, -1));
    local.Begin.Day = 1;
    local.DateWorkAttributes.TextYear = NumberToString(local.Begin.Year, 12, 4);
    local.DateWorkAttributes.TextMonth =
      NumberToString(local.Begin.Month, 14, 2);
    local.DateWorkAttributes.TextDay = NumberToString(local.Begin.Day, 14, 2);
    local.DateWorkAttributes.TextDate10Char =
      String(local.DateWorkAttributes.TextYear,
      DateWorkAttributes.TextYear_MaxLength) + "-" + String
      (local.DateWorkAttributes.TextMonth,
      DateWorkAttributes.TextMonth_MaxLength) + "-" + String
      (local.DateWorkAttributes.TextDay, DateWorkAttributes.TextDay_MaxLength);
    local.ReportingPeriod.Update.BeingDate.Date =
      StringToDate(local.DateWorkAttributes.TextDate10Char);
    local.ReportingPeriod.Update.DashboardAuditData.ReportMonth =
      (int)StringToNumber(String(
        local.DateWorkAttributes.TextYear,
      DateWorkAttributes.TextYear_MaxLength) +
      String(local.DateWorkAttributes.TextMonth,
      DateWorkAttributes.TextMonth_MaxLength));
    local.ReportingPeriod.Update.EndDate.Date =
      AddDays(AddMonths(local.ReportingPeriod.Item.BeingDate.Date, 1), -1);
    local.ReportingPeriod.Update.EndDate.Timestamp = Now();

    ++local.ReportingPeriod.Index;
    local.ReportingPeriod.CheckSize();

    local.ReportingPeriod.Update.BeingDate.Timestamp =
      AddMonths(import.ReportStartDate.Timestamp, -2);
    local.Begin.Year = Year(AddMonths(import.ReportStartDate.Date, -2));
    local.Begin.Month = Month(AddMonths(import.ReportStartDate.Date, -2));
    local.Begin.Day = 1;
    local.DateWorkAttributes.TextYear = NumberToString(local.Begin.Year, 12, 4);
    local.DateWorkAttributes.TextMonth =
      NumberToString(local.Begin.Month, 14, 2);
    local.DateWorkAttributes.TextDay = NumberToString(local.Begin.Day, 14, 2);
    local.DateWorkAttributes.TextDate10Char =
      String(local.DateWorkAttributes.TextYear,
      DateWorkAttributes.TextYear_MaxLength) + "-" + String
      (local.DateWorkAttributes.TextMonth,
      DateWorkAttributes.TextMonth_MaxLength) + "-" + String
      (local.DateWorkAttributes.TextDay, DateWorkAttributes.TextDay_MaxLength);
    local.ReportingPeriod.Update.BeingDate.Date =
      StringToDate(local.DateWorkAttributes.TextDate10Char);
    local.ReportingPeriod.Update.DashboardAuditData.ReportMonth =
      (int)StringToNumber(String(
        local.DateWorkAttributes.TextYear,
      DateWorkAttributes.TextYear_MaxLength) +
      String(local.DateWorkAttributes.TextMonth,
      DateWorkAttributes.TextMonth_MaxLength));
    local.ReportingPeriod.Update.EndDate.Date =
      AddDays(AddMonths(local.ReportingPeriod.Item.BeingDate.Date, 1), -1);
    local.ReportingPeriod.Update.EndDate.Timestamp = Now();

    // -- Checkpoint Info
    // Positions   Value
    // ---------   
    // ------------------------------------
    //  001-080    General Checkpoint Info for PRAD
    //  081-088    Dashboard Priority
    //  089-089    Blank
    //  090-116    legal action standard number
    //  117-117    period on
    if (AsChar(import.ProgramCheckpointRestart.RestartInd) == 'Y')
    {
      if (Equal(import.ProgramCheckpointRestart.RestartInfo, 81, 4, "5-26"))
      {
        local.SubscrpitCount.Count =
          (int)StringToNumber(Substring(
            import.ProgramCheckpointRestart.RestartInfo, 250, 117, 1));

        if (!IsEmpty(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 90, 26)))
        {
          local.BatchTimestampWorkArea.TextTimestamp =
            Substring(import.ProgramCheckpointRestart.RestartInfo, 90, 26);
          UseLeCabConvertTimestamp();
          local.CheckpointLegalAction.CreatedTstamp =
            local.BatchTimestampWorkArea.IefTimestamp;
        }

        if (Lt(local.Null1.Timestamp, local.CheckpointLegalAction.CreatedTstamp) ||
          local.SubscrpitCount.Count > 0)
        {
        }
        else
        {
          // this is when there is a month in change in the middle of a week. we
          // do not want to double count the results
          local.CheckpointLegalAction.CreatedTstamp =
            AddMonths(import.ReportEndDate.Timestamp, 6);
          local.ReportingPeriod.Index = 0;

          for(var limit = local.ReportingPeriod.Count; local
            .ReportingPeriod.Index < limit; ++local.ReportingPeriod.Index)
          {
            if (!local.ReportingPeriod.CheckSize())
            {
              break;
            }

            if (local.ReportingPeriod.Index == 0)
            {
              local.Begin.Date = local.ReportingPeriod.Item.BeingDate.Date;
              local.End.Date = local.ReportingPeriod.Item.EndDate.Date;
              local.InitializedDashboardAuditData.ReportMonth =
                local.ReportingPeriod.Item.DashboardAuditData.ReportMonth;
              local.DashboardStagingPriority35.ReportMonth =
                local.ReportingPeriod.Item.DashboardAuditData.ReportMonth;

              if (local.SubscrpitCount.Count == local.ReportingPeriod.Index + 1)
              {
              }
              else
              {
                local.CheckpointLegalAction.CreatedTstamp =
                  AddMonths(import.ReportEndDate.Timestamp, 6);
              }
            }
            else if (local.ReportingPeriod.Index == 1)
            {
              if (import.ScriptCount.Count != 1)
              {
                // only process the first period since the other periods have 
                // already been done in a earlier call to this cab
                goto Test;
              }

              local.Begin.Date = local.ReportingPeriod.Item.BeingDate.Date;
              local.End.Date = local.ReportingPeriod.Item.EndDate.Date;
              local.InitializedDashboardAuditData.ReportMonth =
                local.ReportingPeriod.Item.DashboardAuditData.ReportMonth;
              local.DashboardStagingPriority35.ReportMonth =
                local.ReportingPeriod.Item.DashboardAuditData.ReportMonth;

              if (local.SubscrpitCount.Count == local.ReportingPeriod.Index + 1)
              {
              }
              else
              {
                local.CheckpointLegalAction.CreatedTstamp =
                  AddMonths(import.ReportEndDate.Timestamp, 6);
              }
            }
            else if (local.ReportingPeriod.Index == 2)
            {
              if (import.ScriptCount.Count != 1)
              {
                // only process the first period since the other periods have 
                // already been done in a earlier call to this cab
                goto Test;
              }

              local.Begin.Date = local.ReportingPeriod.Item.BeingDate.Date;
              local.End.Date = local.ReportingPeriod.Item.EndDate.Date;
              local.InitializedDashboardAuditData.ReportMonth =
                local.ReportingPeriod.Item.DashboardAuditData.ReportMonth;
              local.DashboardStagingPriority35.ReportMonth =
                local.ReportingPeriod.Item.DashboardAuditData.ReportMonth;

              if (local.SubscrpitCount.Count == local.ReportingPeriod.Index + 1)
              {
              }
              else
              {
                local.CheckpointLegalAction.CreatedTstamp =
                  AddMonths(import.ReportEndDate.Timestamp, 6);
              }
            }

            foreach(var _ in ReadDashboardStagingPriority1())
            {
              // need to clear the previcously determined totals before the 
              // program begins or the numbers will not be correct, they will
              // reflect previous run numbers also
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

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                local.EabFileHandling.Action = "WRITE";
                local.EabReportSend.RptDetail =
                  "Error creating/updating Dashboard_Staging_Priority_3_5.";
                UseCabErrorReport();
                ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                return;
              }
            }
          }

          local.ReportingPeriod.CheckIndex();
        }
      }
      else
      {
        local.CheckpointLegalAction.CreatedTstamp =
          AddMonths(import.ReportEndDate.Timestamp, 6);
        local.ReportingPeriod.Index = 0;

        for(var limit = local.ReportingPeriod.Count; local
          .ReportingPeriod.Index < limit; ++local.ReportingPeriod.Index)
        {
          if (!local.ReportingPeriod.CheckSize())
          {
            break;
          }

          if (local.ReportingPeriod.Index == 0)
          {
            local.Begin.Date = local.ReportingPeriod.Item.BeingDate.Date;
            local.End.Date = local.ReportingPeriod.Item.EndDate.Date;
            local.InitializedDashboardAuditData.ReportMonth =
              local.ReportingPeriod.Item.DashboardAuditData.ReportMonth;
            local.DashboardStagingPriority35.ReportMonth =
              local.ReportingPeriod.Item.DashboardAuditData.ReportMonth;

            if (local.SubscrpitCount.Count == local.ReportingPeriod.Index + 1)
            {
            }
            else
            {
              local.CheckpointLegalAction.CreatedTstamp =
                AddMonths(import.ReportEndDate.Timestamp, 6);
            }
          }
          else if (local.ReportingPeriod.Index == 1)
          {
            if (import.ScriptCount.Count != 1)
            {
              // only process the first period since the other periods have 
              // already been done in a earlier call to this cab
              goto Test;
            }

            local.Begin.Date = local.ReportingPeriod.Item.BeingDate.Date;
            local.End.Date = local.ReportingPeriod.Item.EndDate.Date;
            local.InitializedDashboardAuditData.ReportMonth =
              local.ReportingPeriod.Item.DashboardAuditData.ReportMonth;
            local.DashboardStagingPriority35.ReportMonth =
              local.ReportingPeriod.Item.DashboardAuditData.ReportMonth;

            if (local.SubscrpitCount.Count == local.ReportingPeriod.Index + 1)
            {
            }
            else
            {
              local.CheckpointLegalAction.CreatedTstamp =
                AddMonths(import.ReportEndDate.Timestamp, 6);
            }
          }
          else if (local.ReportingPeriod.Index == 2)
          {
            if (import.ScriptCount.Count != 1)
            {
              // only process the first period since the other periods have 
              // already been done in a earlier call to this cab
              goto Test;
            }

            local.Begin.Date = local.ReportingPeriod.Item.BeingDate.Date;
            local.End.Date = local.ReportingPeriod.Item.EndDate.Date;
            local.InitializedDashboardAuditData.ReportMonth =
              local.ReportingPeriod.Item.DashboardAuditData.ReportMonth;
            local.DashboardStagingPriority35.ReportMonth =
              local.ReportingPeriod.Item.DashboardAuditData.ReportMonth;

            if (local.SubscrpitCount.Count == local.ReportingPeriod.Index + 1)
            {
            }
            else
            {
              local.CheckpointLegalAction.CreatedTstamp =
                AddMonths(import.ReportEndDate.Timestamp, 6);
            }
          }

          foreach(var _ in ReadDashboardStagingPriority1())
          {
            // need to clear the previcously determined totals before the 
            // program begins or the numbers will not be correct, they will
            // reflect previous run numbers also
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

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              local.EabFileHandling.Action = "WRITE";
              local.EabReportSend.RptDetail =
                "Error creating/updating Dashboard_Staging_Priority_3_5.";
              UseCabErrorReport();
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              return;
            }
          }
        }

        local.ReportingPeriod.CheckIndex();
      }
    }
    else
    {
      local.CheckpointLegalAction.CreatedTstamp =
        AddMonths(import.ReportEndDate.Timestamp, 6);
      local.ReportingPeriod.Index = 0;

      for(var limit = local.ReportingPeriod.Count; local
        .ReportingPeriod.Index < limit; ++local.ReportingPeriod.Index)
      {
        if (!local.ReportingPeriod.CheckSize())
        {
          break;
        }

        if (local.ReportingPeriod.Index == 0)
        {
          local.Begin.Date = local.ReportingPeriod.Item.BeingDate.Date;
          local.End.Date = local.ReportingPeriod.Item.EndDate.Date;
          local.InitializedDashboardAuditData.ReportMonth =
            local.ReportingPeriod.Item.DashboardAuditData.ReportMonth;
          local.DashboardStagingPriority35.ReportMonth =
            local.ReportingPeriod.Item.DashboardAuditData.ReportMonth;

          if (local.SubscrpitCount.Count == local.ReportingPeriod.Index + 1)
          {
          }
          else
          {
            local.CheckpointLegalAction.CreatedTstamp =
              AddMonths(import.ReportEndDate.Timestamp, 6);
          }
        }
        else if (local.ReportingPeriod.Index == 1)
        {
          if (import.ScriptCount.Count != 1)
          {
            // only process the first period since the other periods have 
            // already been done in a earlier call to this cab
            goto Test;
          }

          local.Begin.Date = local.ReportingPeriod.Item.BeingDate.Date;
          local.End.Date = local.ReportingPeriod.Item.EndDate.Date;
          local.InitializedDashboardAuditData.ReportMonth =
            local.ReportingPeriod.Item.DashboardAuditData.ReportMonth;
          local.DashboardStagingPriority35.ReportMonth =
            local.ReportingPeriod.Item.DashboardAuditData.ReportMonth;

          if (local.SubscrpitCount.Count == local.ReportingPeriod.Index + 1)
          {
          }
          else
          {
            local.CheckpointLegalAction.CreatedTstamp =
              AddMonths(import.ReportEndDate.Timestamp, 6);
          }
        }
        else if (local.ReportingPeriod.Index == 2)
        {
          if (import.ScriptCount.Count != 1)
          {
            // only process the first period since the other periods have 
            // already been done in a earlier call to this cab
            goto Test;
          }

          local.Begin.Date = local.ReportingPeriod.Item.BeingDate.Date;
          local.End.Date = local.ReportingPeriod.Item.EndDate.Date;
          local.InitializedDashboardAuditData.ReportMonth =
            local.ReportingPeriod.Item.DashboardAuditData.ReportMonth;
          local.DashboardStagingPriority35.ReportMonth =
            local.ReportingPeriod.Item.DashboardAuditData.ReportMonth;

          if (local.SubscrpitCount.Count == local.ReportingPeriod.Index + 1)
          {
          }
          else
          {
            local.CheckpointLegalAction.CreatedTstamp =
              AddMonths(import.ReportEndDate.Timestamp, 6);
          }
        }

        foreach(var _ in ReadDashboardStagingPriority1())
        {
          // need to clear the previcously determined totals before the program 
          // begins or the numbers will not be correct, they will reflect
          // previous run numbers also
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

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail =
              "Error creating/updating Dashboard_Staging_Priority_3_5.";
            UseCabErrorReport();
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }
        }
      }

      local.ReportingPeriod.CheckIndex();
    }

Test:

    local.ReportingPeriod.Index = 0;

    for(var limit = local.ReportingPeriod.Count; local.ReportingPeriod.Index < limit
      ; ++local.ReportingPeriod.Index)
    {
      if (!local.ReportingPeriod.CheckSize())
      {
        break;
      }

      if (local.SubscrpitCount.Count > 0 && local.SubscrpitCount.Count > local
        .ReportingPeriod.Index + 1)
      {
        continue;
      }

      if (local.ReportingPeriod.Index == 0)
      {
        local.Begin.Date = local.ReportingPeriod.Item.BeingDate.Date;
        local.Begin.Timestamp = local.ReportingPeriod.Item.BeingDate.Timestamp;
        local.End.Timestamp = local.ReportingPeriod.Item.EndDate.Timestamp;
        local.End.Date = local.ReportingPeriod.Item.EndDate.Date;
        local.InitializedDashboardAuditData.ReportMonth =
          local.ReportingPeriod.Item.DashboardAuditData.ReportMonth;

        if (local.SubscrpitCount.Count == local.ReportingPeriod.Index + 1)
        {
        }
        else
        {
          local.CheckpointLegalAction.CreatedTstamp =
            AddMonths(import.ReportEndDate.Timestamp, 6);
        }
      }
      else if (local.ReportingPeriod.Index == 1)
      {
        if (import.ScriptCount.Count != 1)
        {
          // only process the first period since the other periods have already 
          // been done in a earlier call to this cab
          break;
        }

        local.Begin.Timestamp = local.ReportingPeriod.Item.BeingDate.Timestamp;
        local.End.Timestamp = local.ReportingPeriod.Item.EndDate.Timestamp;
        local.Begin.Date = local.ReportingPeriod.Item.BeingDate.Date;
        local.End.Date = local.ReportingPeriod.Item.EndDate.Date;
        local.InitializedDashboardAuditData.ReportMonth =
          local.ReportingPeriod.Item.DashboardAuditData.ReportMonth;

        if (local.SubscrpitCount.Count == local.ReportingPeriod.Index + 1)
        {
          local.CheckpointLegalAction.CreatedTstamp =
            local.BatchTimestampWorkArea.IefTimestamp;
        }
        else
        {
          local.CheckpointLegalAction.CreatedTstamp =
            AddMonths(import.ReportEndDate.Timestamp, 6);
        }
      }
      else if (local.ReportingPeriod.Index == 2)
      {
        if (import.ScriptCount.Count != 1)
        {
          // only process the first period since the other periods have already 
          // been done in a earlier call to this cab
          break;
        }

        local.Begin.Timestamp = local.ReportingPeriod.Item.BeingDate.Timestamp;
        local.End.Timestamp = local.ReportingPeriod.Item.EndDate.Timestamp;
        local.Begin.Date = local.ReportingPeriod.Item.BeingDate.Date;
        local.End.Date = local.ReportingPeriod.Item.EndDate.Date;
        local.InitializedDashboardAuditData.ReportMonth =
          local.ReportingPeriod.Item.DashboardAuditData.ReportMonth;

        if (local.SubscrpitCount.Count == local.ReportingPeriod.Index + 1)
        {
        }
        else
        {
          local.CheckpointLegalAction.CreatedTstamp =
            AddMonths(import.ReportEndDate.Timestamp, 6);
        }
      }

      foreach(var _ in ReadLegalActionFipsTribunal())
      {
        local.CheckpointLegalAction.CreatedTstamp =
          entities.LegalAction.CreatedTstamp;
        local.InitializedDashboardAuditData.JudicialDistrict = "";
        local.InitializedDashboardAuditData.Office = 0;
        local.DashboardAuditData.Assign(local.InitializedDashboardAuditData);
        local.DashboardStagingPriority35.Assign(
          local.InitializedDashboardStagingPriority35);
        local.PreviousRecord.StandardNumber =
          entities.LegalAction.StandardNumber;

        if (entities.Fips.Populated)
        {
          if (entities.Fips.State != 20)
          {
            continue;
          }
        }
        else
        {
          continue;
        }

        local.DashboardStagingPriority35.ReportLevelId =
          entities.LegalAction.CreatedBy;

        if (IsEmpty(local.DashboardStagingPriority35.ReportLevelId))
        {
          continue;
        }

        local.DashboardStagingPriority35.AsOfDate =
          import.ProgramProcessingInfo.ProcessDate;
        local.DashboardStagingPriority35.ReportLevel = "CW";
        local.DashboardStagingPriority35.ReportMonth =
          local.DashboardAuditData.ReportMonth;
        local.DashboardStagingPriority35.ContemptMotionFilings =
          (local.DashboardStagingPriority35.ContemptMotionFilings ?? 0) + 1;
        local.DashboardAuditData.StandardNumber =
          entities.LegalAction.StandardNumber;
        local.DashboardAuditData.LegalActionDate =
          entities.LegalAction.FiledDate;
        local.DashboardAuditData.DashboardPriority = "5-26";
        local.DashboardAuditData.StandardNumber =
          entities.LegalAction.StandardNumber;
        local.DashboardAuditData.LegalActionDate =
          entities.LegalAction.FiledDate;
        local.DashboardAuditData.WorkerId =
          local.DashboardStagingPriority35.ReportLevelId;

        if (AsChar(import.AuditFlag.Flag) == 'Y')
        {
          // -- Log to the dashboard audit table.
          UseFnB734CreateDashboardAudit();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            return;
          }
        }

        if (ReadDashboardStagingPriority2())
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
            CreateDashboardStagingPriority35();
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
            "Error creating/updating Dashboard_Staging_Priority_3_5.";
          UseCabErrorReport();
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }

        ++local.RecordProcessed.Count;

        if (local.RecordProcessed.Count >= (
          import.ProgramCheckpointRestart.UpdateFrequencyCount ?? 0))
        {
          // -- Checkpoint Info
          // Positions   Value
          // ---------   
          // ------------------------------------
          //  001-080    General Checkpoint Info for PRAD
          //  081-088    Dashboard Priority
          //  089-089    Blank
          //  090-116    legal action create timestamp
          //  117-117    period on
          local.BatchTimestampWorkArea.TextTimestamp = "";
          local.BatchTimestampWorkArea.IefTimestamp =
            local.CheckpointLegalAction.CreatedTstamp;
          UseLeCabConvertTimestamp();
          local.ProgramCheckpointRestart.RestartInfo =
            Substring(import.ProgramCheckpointRestart.RestartInfo, 250, 1, 80) +
            "5-26    " + " " + String
            (local.BatchTimestampWorkArea.TextTimestamp,
            BatchTimestampWorkArea.TextTimestamp_MaxLength);
          local.ProgramCheckpointRestart.RestartInfo =
            Substring(local.ProgramCheckpointRestart.RestartInfo, 250, 1, 116) +
            NumberToString(local.ReportingPeriod.Index + 1, 15, 1);
          local.ProgramCheckpointRestart.RestartInd = "Y";
          local.ProgramCheckpointRestart.CheckpointCount = 0;
          UseUpdateCheckpointRstAndCommit();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail = "Error taking checkpoint.";
            UseCabErrorReport();
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          local.RecordProcessed.Count = 0;
        }
      }
    }

    local.ReportingPeriod.CheckIndex();

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
      Substring(import.ProgramCheckpointRestart.RestartInfo, 250, 1, 80) + "5-27     ";
    UseUpdateCheckpointRstAndCommit();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "Error taking checkpoint.";
      UseCabErrorReport();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }
  }

  private static void MoveBatchTimestampWorkArea(BatchTimestampWorkArea source,
    BatchTimestampWorkArea target)
  {
    target.IefTimestamp = source.IefTimestamp;
    target.TextTimestamp = source.TextTimestamp;
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

    MoveDashboardAuditData1(local.DashboardAuditData,
      useImport.DashboardAuditData);

    context.Call(FnB734CreateDashboardAudit.Execute, useImport, useExport);
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

  private void UseUpdateCheckpointRstAndCommit()
  {
    var useImport = new UpdateCheckpointRstAndCommit.Import();
    var useExport = new UpdateCheckpointRstAndCommit.Export();

    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);

    context.Call(UpdateCheckpointRstAndCommit.Execute, useImport, useExport);
  }

  private void CreateDashboardStagingPriority35()
  {
    var reportMonth = local.DashboardStagingPriority35.ReportMonth;
    var reportLevel = local.DashboardStagingPriority35.ReportLevel;
    var reportLevelId = local.DashboardStagingPriority35.ReportLevelId;
    var asOfDate = local.DashboardStagingPriority35.AsOfDate;
    var param = 0M;
    var contemptOrderFilings =
      local.DashboardStagingPriority35.ContemptMotionFilings ?? 0;

    entities.DashboardStagingPriority35.Populated = false;
    Update("CreateDashboardStagingPriority35",
      (db, command) =>
      {
        db.SetInt32(command, "reportMonth", reportMonth);
        db.SetString(command, "reportLevel", reportLevel);
        db.SetString(command, "reportLevelId", reportLevelId);
        db.SetNullableDate(command, "asOfDate", asOfDate);
        db.SetNullableInt32(command, "casWEstRef", 0);
        db.SetNullableDecimal(command, "fullTimeEqvlnt", param);
        db.SetNullableInt32(command, "cntmptOrdFiled", contemptOrderFilings);
        db.SetNullableDecimal(command, "STypeCollAmt", param);
        db.SetNullableDecimal(command, "STypeCollPer", param);
      });

    entities.DashboardStagingPriority35.ReportMonth = reportMonth;
    entities.DashboardStagingPriority35.ReportLevel = reportLevel;
    entities.DashboardStagingPriority35.ReportLevelId = reportLevelId;
    entities.DashboardStagingPriority35.AsOfDate = asOfDate;
    entities.DashboardStagingPriority35.ContemptOrderFilings =
      contemptOrderFilings;
    entities.DashboardStagingPriority35.Populated = true;
  }

  private IEnumerable<bool> ReadDashboardStagingPriority1()
  {
    return ReadEach("ReadDashboardStagingPriority1",
      (db, command) =>
      {
        db.SetInt32(
          command, "reportMonth", local.DashboardStagingPriority35.ReportMonth);
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
        entities.DashboardStagingPriority35.ContemptOrderFilings =
          db.GetNullableInt32(reader, 4);
        entities.DashboardStagingPriority35.Populated = true;

        return true;
      },
      () =>
      {
        entities.DashboardStagingPriority35.Populated = false;
      });
  }

  private bool ReadDashboardStagingPriority2()
  {
    entities.DashboardStagingPriority35.Populated = false;

    return Read("ReadDashboardStagingPriority2",
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
        entities.DashboardStagingPriority35.ContemptOrderFilings =
          db.GetNullableInt32(reader, 4);
        entities.DashboardStagingPriority35.Populated = true;
      });
  }

  private IEnumerable<bool> ReadLegalActionFipsTribunal()
  {
    return ReadEachInSeparateTransaction("ReadLegalActionFipsTribunal",
      (db, command) =>
      {
        db.SetDate(command, "date1", local.Begin.Date);
        db.SetDate(command, "date2", local.End.Date);
        db.SetDateTime(
          command, "createdTstamp", local.CheckpointLegalAction.CreatedTstamp);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.Classification = db.GetString(reader, 1);
        entities.LegalAction.ActionTaken = db.GetString(reader, 2);
        entities.LegalAction.Type1 = db.GetString(reader, 3);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 4);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 5);
        entities.LegalAction.EndDate = db.GetNullableDate(reader, 6);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 7);
        entities.LegalAction.CreatedBy = db.GetString(reader, 8);
        entities.LegalAction.CreatedTstamp = db.GetDateTime(reader, 9);
        entities.LegalAction.EstablishmentCode =
          db.GetNullableString(reader, 10);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 11);
        entities.Fips.State = db.GetInt32(reader, 12);
        entities.Tribunal.FipState = db.GetNullableInt32(reader, 12);
        entities.Fips.County = db.GetInt32(reader, 13);
        entities.Tribunal.FipCounty = db.GetNullableInt32(reader, 13);
        entities.Fips.Location = db.GetInt32(reader, 14);
        entities.Tribunal.FipLocation = db.GetNullableInt32(reader, 14);
        entities.Fips.CountyDescription = db.GetNullableString(reader, 15);
        entities.Fips.StateAbbreviation = db.GetString(reader, 16);
        entities.Tribunal.Name = db.GetString(reader, 17);
        entities.Tribunal.JudicialDistrict = db.GetString(reader, 18);
        entities.Tribunal.Identifier = db.GetInt32(reader, 19);
        entities.LegalAction.Populated = true;
        entities.Fips.Populated = db.GetNullableInt32(reader, 12) != null;
        entities.Tribunal.Populated = db.GetNullableString(reader, 17) != null;

        return true;
      },
      () =>
      {
        entities.LegalAction.Populated = false;
        entities.Fips.Populated = false;
        entities.Tribunal.Populated = false;
      });
  }

  private void UpdateDashboardStagingPriority1()
  {
    entities.DashboardStagingPriority35.Populated = false;
    Update("UpdateDashboardStagingPriority1",
      (db, command) =>
      {
        db.SetNullableInt32(command, "cntmptOrdFiled", 0);
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

    entities.DashboardStagingPriority35.ContemptOrderFilings = 0;
    entities.DashboardStagingPriority35.Populated = true;
  }

  private void UpdateDashboardStagingPriority2()
  {
    var asOfDate = local.DashboardStagingPriority35.AsOfDate;
    var contemptOrderFilings =
      (entities.DashboardStagingPriority35.ContemptOrderFilings ?? 0) + 1;

    entities.DashboardStagingPriority35.Populated = false;
    Update("UpdateDashboardStagingPriority2",
      (db, command) =>
      {
        db.SetNullableDate(command, "asOfDate", asOfDate);
        db.SetNullableInt32(command, "cntmptOrdFiled", contemptOrderFilings);
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

    entities.DashboardStagingPriority35.AsOfDate = asOfDate;
    entities.DashboardStagingPriority35.ContemptOrderFilings =
      contemptOrderFilings;
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
    /// A value of ProgramCheckpointRestart.
    /// </summary>
    public ProgramCheckpointRestart ProgramCheckpointRestart
    {
      get => programCheckpointRestart ??= new();
      set => programCheckpointRestart = value;
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
    /// A value of AuditFlag.
    /// </summary>
    public Common AuditFlag
    {
      get => auditFlag ??= new();
      set => auditFlag = value;
    }

    private DashboardAuditData? dashboardAuditData;
    private ProgramProcessingInfo? programProcessingInfo;
    private DateWorkArea? reportEndDate;
    private DateWorkArea? reportStartDate;
    private ProgramCheckpointRestart? programCheckpointRestart;
    private Common? scriptCount;
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
    /// <summary>A ReportingPeriodGroup group.</summary>
    [Serializable]
    public class ReportingPeriodGroup
    {
      /// <summary>
      /// A value of EndDate.
      /// </summary>
      public DateWorkArea EndDate
      {
        get => endDate ??= new();
        set => endDate = value;
      }

      /// <summary>
      /// A value of BeingDate.
      /// </summary>
      public DateWorkArea BeingDate
      {
        get => beingDate ??= new();
        set => beingDate = value;
      }

      /// <summary>
      /// A value of DashboardAuditData.
      /// </summary>
      public DashboardAuditData DashboardAuditData
      {
        get => dashboardAuditData ??= new();
        set => dashboardAuditData = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 3;

      private DateWorkArea? endDate;
      private DateWorkArea? beingDate;
      private DashboardAuditData? dashboardAuditData;
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
    /// A value of InitializedDashboardAuditData.
    /// </summary>
    public DashboardAuditData InitializedDashboardAuditData
    {
      get => initializedDashboardAuditData ??= new();
      set => initializedDashboardAuditData = value;
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
    /// A value of CheckpointLegalAction.
    /// </summary>
    public LegalAction CheckpointLegalAction
    {
      get => checkpointLegalAction ??= new();
      set => checkpointLegalAction = value;
    }

    /// <summary>
    /// A value of Begin.
    /// </summary>
    public DateWorkArea Begin
    {
      get => begin ??= new();
      set => begin = value;
    }

    /// <summary>
    /// A value of DateWorkAttributes.
    /// </summary>
    public DateWorkAttributes DateWorkAttributes
    {
      get => dateWorkAttributes ??= new();
      set => dateWorkAttributes = value;
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
    /// A value of BatchTimestampWorkArea.
    /// </summary>
    public BatchTimestampWorkArea BatchTimestampWorkArea
    {
      get => batchTimestampWorkArea ??= new();
      set => batchTimestampWorkArea = value;
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
    /// A value of PreviousRecord.
    /// </summary>
    public LegalAction PreviousRecord
    {
      get => previousRecord ??= new();
      set => previousRecord = value;
    }

    /// <summary>
    /// A value of RecordProcessed.
    /// </summary>
    public Common RecordProcessed
    {
      get => recordProcessed ??= new();
      set => recordProcessed = value;
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
    /// Gets a value of ReportingPeriod.
    /// </summary>
    [JsonIgnore]
    public Array<ReportingPeriodGroup> ReportingPeriod =>
      reportingPeriod ??= new(ReportingPeriodGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of ReportingPeriod for json serialization.
    /// </summary>
    [JsonPropertyName("reportingPeriod")]
    [Computed]
    public IList<ReportingPeriodGroup>? ReportingPeriod_Json
    {
      get => reportingPeriod;
      set => ReportingPeriod.Assign(value);
    }

    /// <summary>
    /// A value of SubscrpitCount.
    /// </summary>
    public Common SubscrpitCount
    {
      get => subscrpitCount ??= new();
      set => subscrpitCount = value;
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
    /// A value of InitializedDashboardStagingPriority35.
    /// </summary>
    public DashboardStagingPriority35 InitializedDashboardStagingPriority35
    {
      get => initializedDashboardStagingPriority35 ??= new();
      set => initializedDashboardStagingPriority35 = value;
    }

    /// <summary>
    /// A value of CheckpointLegalReferral.
    /// </summary>
    public LegalReferral CheckpointLegalReferral
    {
      get => checkpointLegalReferral ??= new();
      set => checkpointLegalReferral = value;
    }

    private DateWorkArea? null1;
    private DashboardAuditData? initializedDashboardAuditData;
    private EabFileHandling? eabFileHandling;
    private EabReportSend? eabReportSend;
    private LegalAction? checkpointLegalAction;
    private DateWorkArea? begin;
    private DateWorkAttributes? dateWorkAttributes;
    private DateWorkArea? end;
    private BatchTimestampWorkArea? batchTimestampWorkArea;
    private DashboardAuditData? dashboardAuditData;
    private LegalAction? previousRecord;
    private Common? recordProcessed;
    private ProgramCheckpointRestart? programCheckpointRestart;
    private Array<ReportingPeriodGroup>? reportingPeriod;
    private Common? subscrpitCount;
    private DashboardStagingPriority35? dashboardStagingPriority35;
    private DashboardStagingPriority35? initializedDashboardStagingPriority35;
    private LegalReferral? checkpointLegalReferral;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of DashboardStagingPriority35.
    /// </summary>
    public DashboardStagingPriority35 DashboardStagingPriority35
    {
      get => dashboardStagingPriority35 ??= new();
      set => dashboardStagingPriority35 = value;
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
    /// A value of ServiceProvider.
    /// </summary>
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
    }

    /// <summary>
    /// A value of LegalActionAssigment.
    /// </summary>
    public LegalActionAssigment LegalActionAssigment
    {
      get => legalActionAssigment ??= new();
      set => legalActionAssigment = value;
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

    private DashboardStagingPriority35? dashboardStagingPriority35;
    private CseOrganization? cseOrganization;
    private LegalAction? legalAction;
    private LegalActionDetail? legalActionDetail;
    private ServiceProvider? serviceProvider;
    private LegalActionAssigment? legalActionAssigment;
    private OfficeServiceProvider? officeServiceProvider;
    private Fips? fips;
    private Tribunal? tribunal;
  }
#endregion
}
