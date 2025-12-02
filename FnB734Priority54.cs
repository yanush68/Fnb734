// Program: FN_B734_PRIORITY_5_4, ID: 945148967, model: 746.
// Short name: SWE03714
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
/// A program: FN_B734_PRIORITY_5_4.
/// </para>
/// <para>
/// Priority 3-4: Paternities Established
/// </para>
/// </summary>
[Serializable]
[Program("SWE03714")]
public partial class FnB734Priority54: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B734_PRIORITY_5_4 program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB734Priority54(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB734Priority54.
  /// </summary>
  public FnB734Priority54(IContext context, Import import, Export export):
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
    // Priority 5-4: Paternities Established
    // -------------------------------------------------------------------------------------
    // This is a count of all children with paternity established by court order
    // in the review period (by attorney and caseworker).  Caseworker/Attorney
    // stats roll up into an Office, Into a Judicial District.
    // County of order needs to be applied to the county pull.
    // All out of state orders will be excluded.
    // 5-4.1 Report Level:Caseworker
    // Report Period: Month
    // Paternities Established by Caseworker
    // 1)	Case open at any time during report period.
    // 2)	J class legal action with file date entered in current report period.
    // 3)	Count only Legal Action Established by Child Support (CS) or Court 
    // Trustee (CT)
    // 4)	Look for Establish Paternity (EP) legal detail.
    // 5)	Count each child (S Role on LOPS on EP legal detail).  Multiple 
    // children will be counted if multiple children exist on LOPS.
    // 6)	Use AP/CH combination (LOPS), find case where AP/CH combination is 
    // active on legal action file date.  Credit caseworker/office assigned to
    // case (ASIN) on legal action file.
    // 5-4.2 Report Level:Attorney
    // Report Period: Month
    // Paternities Established by Attorney
    // 1)	Case open at any time during report period.
    // 2)	J class legal action with file date entered in current report period.
    // 3)	Count only Legal Action Established by CS or CT
    // 4)	Look for EP legal detail.
    // 5)	Count each child (S Role on LOPS on EP legal detail).  Multiple 
    // children will be counted if multiple children exist on LOPS.
    // 6)	Credit attorney/office assigned to legal action on file date (ASIN).  
    // Each unique qualifying court order will only be credited to one attorney
    // as only one attorney will be active on the file date.
    // -------------------------------------------------------------------------------------
    MoveDashboardAuditData2(import.DashboardAuditData,
      local.InitializedDashboardAuditData);
    MoveProgramCheckpointRestart(import.ProgramCheckpointRestart,
      local.ProgramCheckpointRestart);
    local.DashboardStagingPriority35.AsOfDate =
      import.ProgramProcessingInfo.ProcessDate;
    local.DashboardStagingPriority35.ReportMonth =
      import.DashboardAuditData.ReportMonth;
    local.ReportingPeriod.Index = -1;

    ++local.ReportingPeriod.Index;
    local.ReportingPeriod.CheckSize();

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

    ++local.ReportingPeriod.Index;
    local.ReportingPeriod.CheckSize();

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

    ++local.ReportingPeriod.Index;
    local.ReportingPeriod.CheckSize();

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

    // -- Checkpoint Info
    // Positions   Value
    // ---------   
    // ------------------------------------
    //  001-080    General Checkpoint Info for PRAD
    //  081-088    Dashboard Priority
    //  089-089    Blank
    //  090-109    Standard Number
    //  110-117    Filed date
    //  118-118    report period
    local.SubscrpitCount.Count = 0;

    if (AsChar(import.ProgramCheckpointRestart.RestartInd) == 'Y')
    {
      if (Equal(import.ProgramCheckpointRestart.RestartInfo, 81, 4, "5-04"))
      {
        local.Checkpoint.StandardNumber =
          Substring(import.ProgramCheckpointRestart.RestartInfo, 90, 20);

        if (!IsEmpty(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 110, 8)))
        {
          local.Checkpoint.FiledDate =
            IntToDate((int)StringToNumber(Substring(
              import.ProgramCheckpointRestart.RestartInfo, 250, 110, 8)));
        }

        local.SubscrpitCount.Count =
          (int)StringToNumber(Substring(
            import.ProgramCheckpointRestart.RestartInfo, 250, 118, 1));

        if (local.SubscrpitCount.Count > 0 || !
          IsEmpty(local.Checkpoint.StandardNumber) || Lt
          (local.Null1.Date, local.Checkpoint.FiledDate))
        {
        }
        else
        {
          // this is when there is a month in change in the middle of a week. we
          // do not want to double count the results
          local.Checkpoint.StandardNumber = "";
          local.Checkpoint.FiledDate = new DateTime(1, 1, 1);
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
                local.Checkpoint.FiledDate = new DateTime(1, 1, 1);
                local.Checkpoint.StandardNumber = "";
              }
            }
            else if (local.ReportingPeriod.Index == 1)
            {
              if (import.ScriptCount.Count != 1)
              {
                // only process the first period since the other periods have 
                // already been done in a earlier call to this cab
                goto Test1;
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
                local.Checkpoint.StandardNumber = "";
                local.Checkpoint.FiledDate = new DateTime(1, 1, 1);
              }
            }
            else if (local.ReportingPeriod.Index == 2)
            {
              if (import.ScriptCount.Count != 1)
              {
                // only process the first period since the other periods have 
                // already been done in a earlier call to this cab
                goto Test1;
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
                local.Checkpoint.FiledDate = new DateTime(1, 1, 1);
                local.Checkpoint.StandardNumber = "";
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
        local.Checkpoint.StandardNumber = "";
        local.Checkpoint.FiledDate = new DateTime(1, 1, 1);
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
              local.Checkpoint.FiledDate = new DateTime(1, 1, 1);
              local.Checkpoint.StandardNumber = "";
            }
          }
          else if (local.ReportingPeriod.Index == 1)
          {
            if (import.ScriptCount.Count != 1)
            {
              // only process the first period since the other periods have 
              // already been done in a earlier call to this cab
              goto Test1;
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
              local.Checkpoint.StandardNumber = "";
              local.Checkpoint.FiledDate = new DateTime(1, 1, 1);
            }
          }
          else if (local.ReportingPeriod.Index == 2)
          {
            if (import.ScriptCount.Count != 1)
            {
              // only process the first period since the other periods have 
              // already been done in a earlier call to this cab
              goto Test1;
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
              local.Checkpoint.FiledDate = new DateTime(1, 1, 1);
              local.Checkpoint.StandardNumber = "";
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
      local.Checkpoint.StandardNumber = "";
      local.Checkpoint.FiledDate = new DateTime(1, 1, 1);
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
            local.Checkpoint.FiledDate = new DateTime(1, 1, 1);
            local.Checkpoint.StandardNumber = "";
          }
        }
        else if (local.ReportingPeriod.Index == 1)
        {
          if (import.ScriptCount.Count != 1)
          {
            // only process the first period since the other periods have 
            // already been done in a earlier call to this cab
            goto Test1;
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
            local.Checkpoint.StandardNumber = "";
            local.Checkpoint.FiledDate = new DateTime(1, 1, 1);
          }
        }
        else if (local.ReportingPeriod.Index == 2)
        {
          if (import.ScriptCount.Count != 1)
          {
            // only process the first period since the other periods have 
            // already been done in a earlier call to this cab
            goto Test1;
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
            local.Checkpoint.FiledDate = new DateTime(1, 1, 1);
            local.Checkpoint.StandardNumber = "";
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

Test1:

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
        local.End.Date = local.ReportingPeriod.Item.EndDate.Date;
        local.InitializedDashboardAuditData.ReportMonth =
          local.ReportingPeriod.Item.DashboardAuditData.ReportMonth;
        local.InitializedDashboardStagingPriority35.ReportMonth =
          local.ReportingPeriod.Item.DashboardAuditData.ReportMonth;

        if (local.SubscrpitCount.Count == local.ReportingPeriod.Index + 1)
        {
        }
        else
        {
          local.Checkpoint.FiledDate = new DateTime(1, 1, 1);
          local.Checkpoint.StandardNumber = "";
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

        local.Begin.Date = local.ReportingPeriod.Item.BeingDate.Date;
        local.End.Date = local.ReportingPeriod.Item.EndDate.Date;
        local.InitializedDashboardAuditData.ReportMonth =
          local.ReportingPeriod.Item.DashboardAuditData.ReportMonth;
        local.InitializedDashboardStagingPriority35.ReportMonth =
          local.ReportingPeriod.Item.DashboardAuditData.ReportMonth;

        if (local.SubscrpitCount.Count == local.ReportingPeriod.Index + 1)
        {
        }
        else
        {
          local.Checkpoint.StandardNumber = "";
          local.Checkpoint.FiledDate = new DateTime(1, 1, 1);
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

        local.Begin.Date = local.ReportingPeriod.Item.BeingDate.Date;
        local.End.Date = local.ReportingPeriod.Item.EndDate.Date;
        local.InitializedDashboardAuditData.ReportMonth =
          local.ReportingPeriod.Item.DashboardAuditData.ReportMonth;
        local.InitializedDashboardStagingPriority35.ReportMonth =
          local.ReportingPeriod.Item.DashboardAuditData.ReportMonth;

        if (local.SubscrpitCount.Count == local.ReportingPeriod.Index + 1)
        {
        }
        else
        {
          local.Checkpoint.FiledDate = new DateTime(1, 1, 1);
          local.Checkpoint.StandardNumber = "";
        }
      }

      foreach(var _ in ReadLegalActionLaDetNonfinancialFipsTribunal())
      {
        if (Equal(local.PreviousRecord.StandardNumber,
          entities.LegalAction.StandardNumber))
        {
          continue;
        }

        local.InitializedDashboardAuditData.JudicialDistrict = "";
        local.InitializedDashboardAuditData.Office = 0;
        local.DashboardAuditData.Assign(local.InitializedDashboardAuditData);
        local.DashboardStagingPriority35.Assign(
          local.InitializedDashboardStagingPriority35);
        local.PreviousRecord.StandardNumber =
          entities.LegalAction.StandardNumber;
        local.Checkpoint.StandardNumber = entities.LegalAction.StandardNumber;
        local.Checkpoint.FiledDate = entities.LegalAction.FiledDate;

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

        local.NumberOfChildren.Count = 0;
        local.DashboardAuditData.DashboardPriority = "5-4.1";
        local.DashboardAuditData.StandardNumber =
          entities.LegalAction.StandardNumber;
        local.DashboardAuditData.LegalActionDate =
          entities.LegalAction.FiledDate;
        local.DashboardAuditData.DebtType =
          entities.LaDetNonfinancial.NonFinOblgType;

        foreach(var _1 in ReadCsePerson())
        {
          local.DashboardAuditData.SuppCspNumber = entities.Supp.Number;
          ++local.NumberOfChildren.Count;
          local.CountCase.Flag = "";

          foreach(var _2 in ReadCsePersonCase())
          {
            if (entities.Case1.Populated)
            {
              local.CountCase.Flag = "Y";

              break;
            }
          }

          if (AsChar(local.CountCase.Flag) != 'Y')
          {
            continue;
          }

          if (AsChar(local.CountCase.Flag) == 'Y')
          {
            local.DashboardStagingPriority35.ReportLevelId = "";

            if (ReadCaseAssignmentServiceProvider())
            {
              local.DashboardStagingPriority35.ReportLevelId =
                entities.ServiceProvider.UserId;
            }

            if (IsEmpty(local.DashboardStagingPriority35.ReportLevelId))
            {
              goto Test2;
            }

            // -- Case does not owe arrears.  Skip this case.
            local.DashboardStagingPriority35.AsOfDate =
              import.ProgramProcessingInfo.ProcessDate;
            local.DashboardStagingPriority35.ReportLevel = "CW";
            local.DashboardStagingPriority35.ReportLevelId =
              entities.ServiceProvider.UserId;
            local.DashboardStagingPriority35.PaternitiesEstablished = 1;
            local.DashboardAuditData.DashboardPriority = "5-4.1";
            local.DashboardAuditData.CaseNumber = entities.Case1.Number;
            local.DashboardAuditData.CaseDate = entities.Case1.StatusDate;
            local.DashboardAuditData.WorkerId =
              local.DashboardStagingPriority35.ReportLevelId;
            local.DashboardAuditData.SuppCspNumber =
              entities.ChCsePerson.Number;
            local.DashboardAuditData.PayorCspNumber =
              entities.ApCsePerson.Number;

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
          }

Test2:

          if (AsChar(local.CountCase.Flag) == 'Y')
          {
          }

          if (ReadLegalActionAssigmentServiceProvider())
          {
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
            local.DashboardStagingPriority35.PaternitiesEstablished = 1;
            local.DashboardAuditData.DashboardPriority = "5-4.2";
            local.DashboardAuditData.StandardNumber =
              entities.LegalAction.StandardNumber;
            local.DashboardAuditData.LegalActionDate =
              entities.LegalAction.FiledDate;
            local.DashboardAuditData.CaseNumber = entities.Case1.Number;
            local.DashboardAuditData.CaseDate = entities.Case1.StatusDate;
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

            local.CountCase.Flag = "Y";
          }
        }

        if (AsChar(local.CountCase.Flag) != 'Y')
        {
          continue;
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
          //  090-109    Standard number
          //  110-117    Filed date
          //  118-118    Period number
          local.ProgramCheckpointRestart.RestartInfo =
            Substring(import.ProgramCheckpointRestart.RestartInfo, 250, 1, 80) +
            "5-04     " + String
            (entities.LegalAction.StandardNumber,
            LegalAction.StandardNumber_MaxLength) + NumberToString
            (DateToInt(entities.LegalAction.FiledDate), 8, 8);
          local.ProgramCheckpointRestart.RestartInfo =
            Substring(local.ProgramCheckpointRestart.RestartInfo, 250, 1, 117) +
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
      Substring(import.ProgramCheckpointRestart.RestartInfo, 250, 1, 80) + "5-05    ";
    UseUpdateCheckpointRstAndCommit();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "Error taking checkpoint.";
      UseCabErrorReport();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }
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
    var paternitiesEstablished =
      local.DashboardStagingPriority35.PaternitiesEstablished ?? 0;

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
        db.SetNullableInt32(command, "paternitiesEst", paternitiesEstablished);
        db.SetNullableDecimal(command, "STypeCollAmt", param);
        db.SetNullableDecimal(command, "STypeCollPer", param);
      });

    entities.DashboardStagingPriority35.ReportMonth = reportMonth;
    entities.DashboardStagingPriority35.ReportLevel = reportLevel;
    entities.DashboardStagingPriority35.ReportLevelId = reportLevelId;
    entities.DashboardStagingPriority35.AsOfDate = asOfDate;
    entities.DashboardStagingPriority35.PaternitiesEstablished =
      paternitiesEstablished;
    entities.DashboardStagingPriority35.Populated = true;
  }

  private bool ReadCaseAssignmentServiceProvider()
  {
    entities.ServiceProvider.Populated = false;
    entities.CaseAssignment.Populated = false;

    return Read("ReadCaseAssignmentServiceProvider",
      (db, command) =>
      {
        db.SetString(command, "casNo", entities.Case1.Number);
        db.SetDate(command, "effectiveDate", entities.LegalAction.FiledDate);
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
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 9);
        entities.ServiceProvider.UserId = db.GetString(reader, 10);
        entities.ServiceProvider.RoleCode = db.GetNullableString(reader, 11);
        entities.CaseAssignment.Populated = true;
        entities.ServiceProvider.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCsePerson()
  {
    System.Diagnostics.Debug.Assert(entities.LaDetNonfinancial.Populated);

    return ReadEach("ReadCsePerson",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "ladRNumber", entities.LaDetNonfinancial.Number);
        db.SetNullableInt32(
          command, "lgaRIdentifier", entities.LaDetNonfinancial.LgaIdentifier);
        db.SetNullableDate(command, "endDt", local.Begin.Date);
      },
      (db, reader) =>
      {
        entities.Supp.Number = db.GetString(reader, 0);
        entities.Supp.Populated = true;

        return true;
      },
      () =>
      {
        entities.Supp.Populated = false;
      });
  }

  private IEnumerable<bool> ReadCsePersonCase()
  {
    System.Diagnostics.Debug.Assert(entities.LaDetNonfinancial.Populated);

    return ReadEach("ReadCsePersonCase",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "ladRNumber", entities.LaDetNonfinancial.Number);
        db.SetNullableInt32(
          command, "lgaRIdentifier", entities.LaDetNonfinancial.LgaIdentifier);
        db.
          SetNullableDate(command, "startDate", entities.LegalAction.FiledDate);
        db.SetString(command, "cspNumber", entities.Supp.Number);
      },
      (db, reader) =>
      {
        entities.ApCsePerson.Number = db.GetString(reader, 0);
        entities.Case1.Number = db.GetString(reader, 1);
        entities.Case1.Status = db.GetNullableString(reader, 2);
        entities.Case1.StatusDate = db.GetNullableDate(reader, 3);
        entities.Case1.CseOpenDate = db.GetNullableDate(reader, 4);
        entities.Case1.CreatedTimestamp = db.GetDateTime(reader, 5);
        entities.Case1.InterstateCaseId = db.GetNullableString(reader, 6);
        entities.Case1.NoJurisdictionCd = db.GetNullableString(reader, 7);
        entities.ApCsePerson.Populated = true;
        entities.Case1.Populated = true;

        return true;
      },
      () =>
      {
        entities.Case1.Populated = false;
        entities.ApCsePerson.Populated = false;
      });
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
        entities.DashboardStagingPriority35.PaternitiesEstablished =
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
        entities.DashboardStagingPriority35.PaternitiesEstablished =
          db.GetNullableInt32(reader, 4);
        entities.DashboardStagingPriority35.Populated = true;
      });
  }

  private bool ReadLegalActionAssigmentServiceProvider()
  {
    entities.ServiceProvider.Populated = false;
    entities.LegalActionAssigment.Populated = false;

    return Read("ReadLegalActionAssigmentServiceProvider",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "lgaIdentifier", entities.LegalAction.Identifier);
        db.SetDate(command, "effectiveDt", entities.LegalAction.FiledDate);
      },
      (db, reader) =>
      {
        entities.LegalActionAssigment.LgaIdentifier =
          db.GetNullableInt32(reader, 0);
        entities.LegalActionAssigment.OspEffectiveDate =
          db.GetNullableDate(reader, 1);
        entities.LegalActionAssigment.OspRoleCode =
          db.GetNullableString(reader, 2);
        entities.LegalActionAssigment.OffGeneratedId =
          db.GetNullableInt32(reader, 3);
        entities.LegalActionAssigment.SpdGeneratedId =
          db.GetNullableInt32(reader, 4);
        entities.LegalActionAssigment.EffectiveDate = db.GetDate(reader, 5);
        entities.LegalActionAssigment.DiscontinueDate =
          db.GetNullableDate(reader, 6);
        entities.LegalActionAssigment.ReasonCode = db.GetString(reader, 7);
        entities.LegalActionAssigment.CreatedTimestamp =
          db.GetDateTime(reader, 8);
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 9);
        entities.ServiceProvider.UserId = db.GetString(reader, 10);
        entities.ServiceProvider.RoleCode = db.GetNullableString(reader, 11);
        entities.LegalActionAssigment.Populated = true;
        entities.ServiceProvider.Populated = true;
      });
  }

  private IEnumerable<bool> ReadLegalActionLaDetNonfinancialFipsTribunal()
  {
    return ReadEachInSeparateTransaction(
      "ReadLegalActionLaDetNonfinancialFipsTribunal",
      (db, command) =>
      {
        db.SetDate(command, "date1", local.Begin.Date);
        db.SetDate(command, "date2", local.End.Date);
        db.SetNullableString(
          command, "standardNo", local.Checkpoint.StandardNumber ?? "");
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LaDetNonfinancial.LgaIdentifier = db.GetInt32(reader, 0);
        entities.LegalAction.Classification = db.GetString(reader, 1);
        entities.LegalAction.ActionTaken = db.GetString(reader, 2);
        entities.LegalAction.Type1 = db.GetString(reader, 3);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 4);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 5);
        entities.LegalAction.EndDate = db.GetNullableDate(reader, 6);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 7);
        entities.LegalAction.CreatedTstamp = db.GetDateTime(reader, 8);
        entities.LegalAction.EstablishmentCode =
          db.GetNullableString(reader, 9);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 10);
        entities.LaDetNonfinancial.Number = db.GetInt32(reader, 11);
        entities.LaDetNonfinancial.NonFinOblgType =
          db.GetNullableString(reader, 12);
        entities.LaDetNonfinancial.DetailType = db.GetString(reader, 13);
        entities.Fips.State = db.GetInt32(reader, 14);
        entities.Tribunal.FipState = db.GetNullableInt32(reader, 14);
        entities.Fips.County = db.GetInt32(reader, 15);
        entities.Tribunal.FipCounty = db.GetNullableInt32(reader, 15);
        entities.Fips.Location = db.GetInt32(reader, 16);
        entities.Tribunal.FipLocation = db.GetNullableInt32(reader, 16);
        entities.Fips.CountyDescription = db.GetNullableString(reader, 17);
        entities.Fips.StateAbbreviation = db.GetString(reader, 18);
        entities.Tribunal.Name = db.GetString(reader, 19);
        entities.Tribunal.JudicialDistrict = db.GetString(reader, 20);
        entities.Tribunal.Identifier = db.GetInt32(reader, 21);
        entities.LegalAction.Populated = true;
        entities.LaDetNonfinancial.Populated = true;
        entities.Fips.Populated = db.GetNullableInt32(reader, 14) != null;
        entities.Tribunal.Populated = db.GetNullableString(reader, 19) != null;
        CheckValid<LegalActionDetail>("DetailType",
          entities.LaDetNonfinancial.DetailType);

        return true;
      },
      () =>
      {
        entities.LegalAction.Populated = false;
        entities.Fips.Populated = false;
        entities.Tribunal.Populated = false;
        entities.LaDetNonfinancial.Populated = false;
      });
  }

  private void UpdateDashboardStagingPriority1()
  {
    entities.DashboardStagingPriority35.Populated = false;
    Update("UpdateDashboardStagingPriority1",
      (db, command) =>
      {
        db.SetNullableInt32(command, "paternitiesEst", 0);
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

    entities.DashboardStagingPriority35.PaternitiesEstablished = 0;
    entities.DashboardStagingPriority35.Populated = true;
  }

  private void UpdateDashboardStagingPriority2()
  {
    var asOfDate = local.DashboardStagingPriority35.AsOfDate;
    var paternitiesEstablished =
      (entities.DashboardStagingPriority35.PaternitiesEstablished ?? 0) + 1;

    entities.DashboardStagingPriority35.Populated = false;
    Update("UpdateDashboardStagingPriority2",
      (db, command) =>
      {
        db.SetNullableDate(command, "asOfDate", asOfDate);
        db.SetNullableInt32(command, "paternitiesEst", paternitiesEstablished);
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
    entities.DashboardStagingPriority35.PaternitiesEstablished =
      paternitiesEstablished;
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
    /// A value of ProgramProcessingInfo.
    /// </summary>
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
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

    private DateWorkArea? reportEndDate;
    private DateWorkArea? reportStartDate;
    private ProgramCheckpointRestart? programCheckpointRestart;
    private ProgramProcessingInfo? programProcessingInfo;
    private DashboardAuditData? dashboardAuditData;
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
    /// A value of Ch.
    /// </summary>
    public CsePerson Ch
    {
      get => ch ??= new();
      set => ch = value;
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
    /// A value of SubscrpitCount.
    /// </summary>
    public Common SubscrpitCount
    {
      get => subscrpitCount ??= new();
      set => subscrpitCount = value;
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
    /// A value of NumberOfChildren.
    /// </summary>
    public Common NumberOfChildren
    {
      get => numberOfChildren ??= new();
      set => numberOfChildren = value;
    }

    /// <summary>
    /// A value of Checkpoint.
    /// </summary>
    public LegalAction Checkpoint
    {
      get => checkpoint ??= new();
      set => checkpoint = value;
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
    /// A value of DashboardAuditData.
    /// </summary>
    public DashboardAuditData DashboardAuditData
    {
      get => dashboardAuditData ??= new();
      set => dashboardAuditData = value;
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
    /// A value of BatchTimestampWorkArea.
    /// </summary>
    public BatchTimestampWorkArea BatchTimestampWorkArea
    {
      get => batchTimestampWorkArea ??= new();
      set => batchTimestampWorkArea = value;
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
    /// A value of CountCase.
    /// </summary>
    public Common CountCase
    {
      get => countCase ??= new();
      set => countCase = value;
    }

    /// <summary>
    /// A value of InitializedDashboardStagingPriority35.
    /// </summary>
    public DashboardStagingPriority35 InitializedDashboardStagingPriority35
    {
      get => initializedDashboardStagingPriority35 ??= new();
      set => initializedDashboardStagingPriority35 = value;
    }

    private DateWorkArea? null1;
    private CsePerson? ch;
    private CsePerson? ap;
    private Common? subscrpitCount;
    private Array<ReportingPeriodGroup>? reportingPeriod;
    private Common? numberOfChildren;
    private LegalAction? checkpoint;
    private DateWorkArea? begin;
    private DateWorkAttributes? dateWorkAttributes;
    private DateWorkArea? end;
    private LegalAction? previousRecord;
    private Common? recordProcessed;
    private ProgramCheckpointRestart? programCheckpointRestart;
    private EabFileHandling? eabFileHandling;
    private EabReportSend? eabReportSend;
    private DashboardAuditData? dashboardAuditData;
    private DashboardAuditData? initializedDashboardAuditData;
    private BatchTimestampWorkArea? batchTimestampWorkArea;
    private DashboardStagingPriority35? dashboardStagingPriority35;
    private Common? countCase;
    private DashboardStagingPriority35? initializedDashboardStagingPriority35;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ChLegalActionPerson.
    /// </summary>
    public LegalActionPerson ChLegalActionPerson
    {
      get => chLegalActionPerson ??= new();
      set => chLegalActionPerson = value;
    }

    /// <summary>
    /// A value of ChLaPersonLaCaseRole.
    /// </summary>
    public LaPersonLaCaseRole ChLaPersonLaCaseRole
    {
      get => chLaPersonLaCaseRole ??= new();
      set => chLaPersonLaCaseRole = value;
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
    /// A value of DashboardStagingPriority35.
    /// </summary>
    public DashboardStagingPriority35 DashboardStagingPriority35
    {
      get => dashboardStagingPriority35 ??= new();
      set => dashboardStagingPriority35 = value;
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
    /// A value of ApLegalActionPerson.
    /// </summary>
    public LegalActionPerson ApLegalActionPerson
    {
      get => apLegalActionPerson ??= new();
      set => apLegalActionPerson = value;
    }

    /// <summary>
    /// A value of LaDetNonfinancial.
    /// </summary>
    public LegalActionDetail LaDetNonfinancial
    {
      get => laDetNonfinancial ??= new();
      set => laDetNonfinancial = value;
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
    /// A value of ApLaPersonLaCaseRole.
    /// </summary>
    public LaPersonLaCaseRole ApLaPersonLaCaseRole
    {
      get => apLaPersonLaCaseRole ??= new();
      set => apLaPersonLaCaseRole = value;
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
    /// A value of ChOrAr.
    /// </summary>
    public CaseRole ChOrAr
    {
      get => chOrAr ??= new();
      set => chOrAr = value;
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
    /// A value of ApCsePerson.
    /// </summary>
    public CsePerson ApCsePerson
    {
      get => apCsePerson ??= new();
      set => apCsePerson = value;
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
    /// A value of ServiceProvider.
    /// </summary>
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
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
    /// A value of ApLegalActionCaseRole.
    /// </summary>
    public LegalActionCaseRole ApLegalActionCaseRole
    {
      get => apLegalActionCaseRole ??= new();
      set => apLegalActionCaseRole = value;
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
    /// A value of ChLegalActionCaseRole.
    /// </summary>
    public LegalActionCaseRole ChLegalActionCaseRole
    {
      get => chLegalActionCaseRole ??= new();
      set => chLegalActionCaseRole = value;
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
    /// A value of OfficeServiceProvider.
    /// </summary>
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
    }

    /// <summary>
    /// A value of LegalActionAssigment.
    /// </summary>
    public LegalActionAssigment LegalActionAssigment
    {
      get => legalActionAssigment ??= new();
      set => legalActionAssigment = value;
    }

    private LegalActionPerson? chLegalActionPerson;
    private LaPersonLaCaseRole? chLaPersonLaCaseRole;
    private LegalActionDetail? legalActionDetail;
    private DashboardStagingPriority35? dashboardStagingPriority35;
    private LegalAction? legalAction;
    private Fips? fips;
    private Tribunal? tribunal;
    private LegalActionPerson? apLegalActionPerson;
    private LegalActionDetail? laDetNonfinancial;
    private CsePerson? supp;
    private LaPersonLaCaseRole? apLaPersonLaCaseRole;
    private LegalActionCaseRole? legalActionCaseRole;
    private CaseRole? chOrAr;
    private Case1? case1;
    private CsePerson? apCsePerson;
    private CsePerson? chCsePerson;
    private ServiceProvider? serviceProvider;
    private CaseRole? apCaseRole;
    private LegalActionCaseRole? apLegalActionCaseRole;
    private CaseRole? chCaseRole;
    private LegalActionCaseRole? chLegalActionCaseRole;
    private CaseAssignment? caseAssignment;
    private OfficeServiceProvider? officeServiceProvider;
    private LegalActionAssigment? legalActionAssigment;
  }
#endregion
}
