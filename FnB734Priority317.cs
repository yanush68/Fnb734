// Program: FN_B734_PRIORITY_3_17, ID: 945148936, model: 746.
// Short name: SWE03690
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
/// A program: FN_B734_PRIORITY_3_17.
/// </para>
/// <para>
/// Priority 3-17: Aging Report of Unprocessed Legal Referrals by Attorney
/// </para>
/// </summary>
[Serializable]
[Program("SWE03690")]
public partial class FnB734Priority317: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B734_PRIORITY_3_17 program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB734Priority317(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB734Priority317.
  /// </summary>
  public FnB734Priority317(IContext context, Import import, Export export):
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
    // Priority 3-17: Aging Report of Unprocessed Legal Referrals by Attorney
    // -------------------------------------------------------------------------------------
    // Report Level: State, Judicial District, Region, Office, Attorney
    // Report Period: Month
    // 1)	Find all referrals with EST or PAT reason code that is in either OPEN 
    // or SENT status as of refresh date.
    // 2)	Count the number of days from referral creation date until refresh 
    // date.
    // 3)	Aging categories:
    // a.	60  90 days
    // b.	91  120 days
    // c.	121  150 days
    // d.	150 + days
    // -------------------------------------------------------------------------------------
    MoveDashboardAuditData2(import.DashboardAuditData, local.Initialized);
    MoveProgramCheckpointRestart(import.ProgramCheckpointRestart,
      local.ProgramCheckpointRestart);
    local.Begin.Date = import.ReportStartDate.Date;
    local.Begin.Time = StringToTime("00.00.00.000000") ?? default;
    UseFnBuildTimestampFrmDateTime();
    MoveDateWorkArea3(import.ReportEndDate, local.End);

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
    }

    // -- Checkpoint Info
    // Positions   Value
    // ---------   
    // ------------------------------------
    //  001-080    General Checkpoint Info for PRAD
    //  081-088    Dashboard Priority
    //  089-089    Blank
    //  090-116    legal refferal create timestamp
    if (AsChar(import.ProgramCheckpointRestart.RestartInd) == 'Y')
    {
      if (Equal(import.ProgramCheckpointRestart.RestartInfo, 81, 4, "3-17"))
      {
        if (!IsEmpty(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 90, 26)))
        {
          local.BatchTimestampWorkArea.TextTimestamp =
            Substring(import.ProgramCheckpointRestart.RestartInfo, 90, 26);
          UseLeCabConvertTimestamp();
          local.Checkpoint.CreatedTimestamp =
            local.BatchTimestampWorkArea.IefTimestamp;
        }

        // -- Load Judicial District counts.
        if (Lt(local.NullDate.Timestamp, local.Checkpoint.CreatedTimestamp))
        {
          foreach(var _ in ReadDashboardStagingPriority1())
          {
            local.Local1.Index =
              (int)StringToNumber(entities.DashboardStagingPriority35.
                ReportLevelId) - 1;
            local.Local1.CheckSize();

            local.Local1.Update.G.Assign(entities.DashboardStagingPriority35);
          }
        }
        else
        {
          // this is when there is a month in change in the middle of a week. we
          // do not want to double count the results
          foreach(var _ in ReadDashboardStagingPriority1())
          {
            local.Local1.Index =
              (int)StringToNumber(entities.DashboardStagingPriority35.
                ReportLevelId) - 1;
            local.Local1.CheckSize();

            local.Local1.Update.G.Assign(entities.DashboardStagingPriority35);
            local.Local1.Update.G.ReferralAging121To150Days = 0;
            local.Local1.Update.G.ReferralAging151PlusDays = 0;
            local.Local1.Update.G.ReferralAging60To90Days = 0;
            local.Local1.Update.G.ReferralAging91To120Days = 0;
          }

          local.Checkpoint.CreatedTimestamp = import.ReportEndDate.Timestamp;
        }
      }
      else
      {
        local.Checkpoint.CreatedTimestamp = import.ReportEndDate.Timestamp;
      }
    }
    else
    {
      local.Checkpoint.CreatedTimestamp = import.ReportEndDate.Timestamp;
    }

    foreach(var _ in ReadLegalReferralCase())
    {
      if (!IsEmpty(entities.Case1.NoJurisdictionCd))
      {
        continue;
      }

      // -- Re-initialize Judicial District and Office
      local.Initialized.JudicialDistrict = "";
      local.Initialized.Office = 0;
      local.DashboardAuditData.Assign(local.Initialized);
      local.CountCase.Flag = "N";
      local.Checkpoint.CreatedTimestamp =
        entities.LegalReferral.CreatedTimestamp;

      if (Equal(entities.LegalReferral.ReferralReason1, "PAT") || Equal
        (entities.LegalReferral.ReferralReason2, "PAT") || Equal
        (entities.LegalReferral.ReferralReason3, "PAT") || Equal
        (entities.LegalReferral.ReferralReason4, "PAT") || Equal
        (entities.LegalReferral.ReferralReason1, "EST") || Equal
        (entities.LegalReferral.ReferralReason2, "EST") || Equal
        (entities.LegalReferral.ReferralReason3, "EST") || Equal
        (entities.LegalReferral.ReferralReason4, "EST"))
      {
        if (!Equal(entities.LegalReferral.ReferralReason1, "PAT") && !
          Equal(entities.LegalReferral.ReferralReason1, "EST") && !
          IsEmpty(entities.LegalReferral.ReferralReason1))
        {
          continue;
        }

        if (!Equal(entities.LegalReferral.ReferralReason2, "PAT") && !
          Equal(entities.LegalReferral.ReferralReason2, "EST") && !
          IsEmpty(entities.LegalReferral.ReferralReason2))
        {
          continue;
        }

        if (!Equal(entities.LegalReferral.ReferralReason3, "PAT") && !
          Equal(entities.LegalReferral.ReferralReason3, "EST") && !
          IsEmpty(entities.LegalReferral.ReferralReason3))
        {
          continue;
        }

        if (!Equal(entities.LegalReferral.ReferralReason4, "PAT") && !
          Equal(entities.LegalReferral.ReferralReason4, "EST") && !
          IsEmpty(entities.LegalReferral.ReferralReason4))
        {
          continue;
        }

        local.DashboardAuditData.DashboardPriority = "3-17";
        local.CountCase.Flag = "Y";
        local.DashboardAuditData.CaseNumber = entities.Case1.Number;
        local.Case1.Number = entities.Case1.Number;
      }
      else
      {
        continue;
      }

      if (AsChar(local.CountCase.Flag) == 'N')
      {
        // -- Case does not owe arrears.  Skip this case.
        continue;
      }

      local.Convert.Date = Date(entities.LegalReferral.CreatedTimestamp);
      local.DashboardAuditData.DaysReported =
        DaysFromAD(import.ReportEndDate.Date) - DaysFromAD(local.Convert.Date);
      local.DashboardAuditData.LegalReferralDate = local.Convert.Date;
      ReadCaseAssignment();

      if (Lt(entities.CaseAssignment.DiscontinueDate, import.ReportEndDate.Date) &&
        Lt(local.NullDate.Date, entities.CaseAssignment.DiscontinueDate))
      {
        local.CaseAssignment.Date = entities.CaseAssignment.DiscontinueDate;
      }
      else
      {
        local.CaseAssignment.Date = import.ReportEndDate.Date;
      }

      // -- Determine office and judicial district to which case is assigned on 
      // the report period end date.
      UseFnB734DetermineJdFromCase();
      local.DashboardAuditData.CaseNumber = entities.Case1.Number;

      if (AsChar(import.AuditFlag.Flag) == 'Y')
      {
        // -- Log to the dashboard audit table.
        UseFnB734CreateDashboardAudit();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }
      }

      // -- Increment Judicial District Level
      if (!IsEmpty(local.DashboardAuditData.JudicialDistrict))
      {
        local.Local1.Index =
          (int)StringToNumber(local.DashboardAuditData.JudicialDistrict) - 1;
        local.Local1.CheckSize();

        if ((local.DashboardAuditData.DaysReported ?? 0) >= 60 && (
          local.DashboardAuditData.DaysReported ?? 0) <= 90)
        {
          local.Local1.Update.G.ReferralAging60To90Days =
            (local.Local1.Item.G.ReferralAging60To90Days ?? 0) + 1;
        }
        else if ((local.DashboardAuditData.DaysReported ?? 0) >= 91 && (
          local.DashboardAuditData.DaysReported ?? 0) <= 120)
        {
          local.Local1.Update.G.ReferralAging91To120Days =
            (local.Local1.Item.G.ReferralAging91To120Days ?? 0) + 1;
        }
        else if ((local.DashboardAuditData.DaysReported ?? 0) >= 121 && (
          local.DashboardAuditData.DaysReported ?? 0) <= 150)
        {
          local.Local1.Update.G.ReferralAging121To150Days =
            (local.Local1.Item.G.ReferralAging121To150Days ?? 0) + 1;
        }
        else if ((local.DashboardAuditData.DaysReported ?? 0) >= 151)
        {
          local.Local1.Update.G.ReferralAging151PlusDays =
            (local.Local1.Item.G.ReferralAging151PlusDays ?? 0) + 1;
        }
        else
        {
          // if it is less than 60 days we are not counting it
          continue;
        }
      }

      ++local.RecordProcessed.Count;

      if (local.RecordProcessed.Count >= (
        import.ProgramCheckpointRestart.UpdateFrequencyCount ?? 0))
      {
        // -- Save Judicial District counts.
        local.Local1.Index = 0;

        for(var limit = local.Local1.Count; local.Local1.Index < limit; ++
          local.Local1.Index)
        {
          if (!local.Local1.CheckSize())
          {
            break;
          }

          if (ReadDashboardStagingPriority2())
          {
            try
            {
              UpdateDashboardStagingPriority35();
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

        local.Local1.CheckIndex();

        // -- Checkpoint Info
        // Positions   Value
        // ---------   
        // ------------------------------------
        //  001-080    General Checkpoint Info for PRAD
        //  081-088    Dashboard Priority
        //  089-089    Blank
        //  090-116    legal refferal create timestamp
        local.BatchTimestampWorkArea.TextTimestamp = "";
        local.BatchTimestampWorkArea.IefTimestamp =
          local.Checkpoint.CreatedTimestamp;
        UseLeCabConvertTimestamp();
        local.ProgramCheckpointRestart.RestartInfo =
          Substring(import.ProgramCheckpointRestart.RestartInfo, 250, 1, 80) + "3-17    " +
          " " + String
          (local.BatchTimestampWorkArea.TextTimestamp,
          BatchTimestampWorkArea.TextTimestamp_MaxLength);
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

    if (local.RecordProcessed.Count > 0)
    {
      // -- Save Judicial District counts.
      for(local.Local1.Index = 0; local.Local1.Index < local.Local1.Count; ++
        local.Local1.Index)
      {
        if (!local.Local1.CheckSize())
        {
          break;
        }

        if (ReadDashboardStagingPriority2())
        {
          try
          {
            UpdateDashboardStagingPriority35();
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
      }

      local.Local1.CheckIndex();

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
      Substring(import.ProgramCheckpointRestart.RestartInfo, 250, 1, 80) + "3-18     ";
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

  private static void MoveDashboardAuditData3(DashboardAuditData source,
    DashboardAuditData target)
  {
    target.Office = source.Office;
    target.JudicialDistrict = source.JudicialDistrict;
    target.CaseNumber = source.CaseNumber;
  }

  private static void MoveDateWorkArea1(DateWorkArea source, DateWorkArea target)
  {
    target.Date = source.Date;
    target.Time = source.Time;
  }

  private static void MoveDateWorkArea2(DateWorkArea source, DateWorkArea target)
  {
    target.Date = source.Date;
    target.Time = source.Time;
    target.Timestamp = source.Timestamp;
  }

  private static void MoveDateWorkArea3(DateWorkArea source, DateWorkArea target)
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

    MoveDashboardAuditData1(local.DashboardAuditData,
      useImport.DashboardAuditData);

    context.Call(FnB734CreateDashboardAudit.Execute, useImport, useExport);
  }

  private void UseFnB734DetermineJdFromCase()
  {
    var useImport = new FnB734DetermineJdFromCase.Import();
    var useExport = new FnB734DetermineJdFromCase.Export();

    useImport.Case1.Number = entities.Case1.Number;
    useImport.ReportEndDate.Date = local.CaseAssignment.Date;

    context.Call(FnB734DetermineJdFromCase.Execute, useImport, useExport);

    MoveDashboardAuditData3(useExport.DashboardAuditData,
      local.DashboardAuditData);
  }

  private void UseFnBuildTimestampFrmDateTime()
  {
    var useImport = new FnBuildTimestampFrmDateTime.Import();
    var useExport = new FnBuildTimestampFrmDateTime.Export();

    MoveDateWorkArea1(local.Begin, useImport.DateWorkArea);

    context.Call(FnBuildTimestampFrmDateTime.Execute, useImport, useExport);

    MoveDateWorkArea2(useExport.DateWorkArea, local.Begin);
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
    var reportMonth = local.Local1.Item.G.ReportMonth;
    var reportLevel = local.Local1.Item.G.ReportLevel;
    var reportLevelId = local.Local1.Item.G.ReportLevelId;
    var asOfDate = local.Local1.Item.G.AsOfDate;
    var param = 0M;
    var referralAging60To90Days =
      local.Local1.Item.G.ReferralAging60To90Days ?? 0;
    var referralAging91To120Days =
      local.Local1.Item.G.ReferralAging91To120Days ?? 0;
    var referralAging121To150Days =
      local.Local1.Item.G.ReferralAging121To150Days ?? 0;
    var referralAging151PlusDays =
      local.Local1.Item.G.ReferralAging151PlusDays ?? 0;

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
        db.SetNullableDecimal(command, "STypeCollAmt", param);
        db.SetNullableDecimal(command, "STypeCollPer", param);
        db.SetNullableInt32(command, "refAge60To90", referralAging60To90Days);
        db.SetNullableInt32(command, "refAge91To120", referralAging91To120Days);
        db.
          SetNullableInt32(command, "refAge121To150", referralAging121To150Days);
        db.SetNullableInt32(command, "refAge151Plus", referralAging151PlusDays);
      });

    entities.DashboardStagingPriority35.ReportMonth = reportMonth;
    entities.DashboardStagingPriority35.ReportLevel = reportLevel;
    entities.DashboardStagingPriority35.ReportLevelId = reportLevelId;
    entities.DashboardStagingPriority35.AsOfDate = asOfDate;
    entities.DashboardStagingPriority35.ReferralAging60To90Days =
      referralAging60To90Days;
    entities.DashboardStagingPriority35.ReferralAging91To120Days =
      referralAging91To120Days;
    entities.DashboardStagingPriority35.ReferralAging121To150Days =
      referralAging121To150Days;
    entities.DashboardStagingPriority35.ReferralAging151PlusDays =
      referralAging151PlusDays;
    entities.DashboardStagingPriority35.Populated = true;
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
        entities.CaseAssignment.DiscontinueDate = db.GetNullableDate(reader, 0);
        entities.CaseAssignment.CreatedTimestamp = db.GetDateTime(reader, 1);
        entities.CaseAssignment.SpdId = db.GetInt32(reader, 2);
        entities.CaseAssignment.OffId = db.GetInt32(reader, 3);
        entities.CaseAssignment.OspCode = db.GetString(reader, 4);
        entities.CaseAssignment.OspDate = db.GetDate(reader, 5);
        entities.CaseAssignment.CasNo = db.GetString(reader, 6);
        entities.CaseAssignment.Populated = true;
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
        entities.DashboardStagingPriority35.ReportMonth =
          db.GetInt32(reader, 0);
        entities.DashboardStagingPriority35.ReportLevel =
          db.GetString(reader, 1);
        entities.DashboardStagingPriority35.ReportLevelId =
          db.GetString(reader, 2);
        entities.DashboardStagingPriority35.AsOfDate =
          db.GetNullableDate(reader, 3);
        entities.DashboardStagingPriority35.ReferralAging60To90Days =
          db.GetNullableInt32(reader, 4);
        entities.DashboardStagingPriority35.ReferralAging91To120Days =
          db.GetNullableInt32(reader, 5);
        entities.DashboardStagingPriority35.ReferralAging121To150Days =
          db.GetNullableInt32(reader, 6);
        entities.DashboardStagingPriority35.ReferralAging151PlusDays =
          db.GetNullableInt32(reader, 7);
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
        db.SetInt32(command, "reportMonth", local.Local1.Item.G.ReportMonth);
        db.SetString(command, "reportLevel", local.Local1.Item.G.ReportLevel);
        db.
          SetString(command, "reportLevelId", local.Local1.Item.G.ReportLevelId);
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
        entities.DashboardStagingPriority35.ReferralAging60To90Days =
          db.GetNullableInt32(reader, 4);
        entities.DashboardStagingPriority35.ReferralAging91To120Days =
          db.GetNullableInt32(reader, 5);
        entities.DashboardStagingPriority35.ReferralAging121To150Days =
          db.GetNullableInt32(reader, 6);
        entities.DashboardStagingPriority35.ReferralAging151PlusDays =
          db.GetNullableInt32(reader, 7);
        entities.DashboardStagingPriority35.Populated = true;
      });
  }

  private IEnumerable<bool> ReadLegalReferralCase()
  {
    return ReadEachInSeparateTransaction("ReadLegalReferralCase",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTimestamp1", local.Checkpoint.CreatedTimestamp);
        db.SetNullableDate(command, "statusDate", import.ReportEndDate.Date);
        db.SetDateTime(
          command, "createdTimestamp2", import.ReportEndDate.Timestamp);
      },
      (db, reader) =>
      {
        entities.LegalReferral.CasNumber = db.GetString(reader, 0);
        entities.Case1.Number = db.GetString(reader, 0);
        entities.LegalReferral.Identifier = db.GetInt32(reader, 1);
        entities.LegalReferral.StatusDate = db.GetNullableDate(reader, 2);
        entities.LegalReferral.Status = db.GetNullableString(reader, 3);
        entities.LegalReferral.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.LegalReferral.ReferralReason1 = db.GetString(reader, 5);
        entities.LegalReferral.ReferralReason2 = db.GetString(reader, 6);
        entities.LegalReferral.ReferralReason3 = db.GetString(reader, 7);
        entities.LegalReferral.ReferralReason4 = db.GetString(reader, 8);
        entities.Case1.NoJurisdictionCd = db.GetNullableString(reader, 9);
        entities.LegalReferral.Populated = true;
        entities.Case1.Populated = true;

        return true;
      },
      () =>
      {
        entities.Case1.Populated = false;
        entities.LegalReferral.Populated = false;
      });
  }

  private void UpdateDashboardStagingPriority35()
  {
    var asOfDate = local.Local1.Item.G.AsOfDate;
    var referralAging60To90Days =
      local.Local1.Item.G.ReferralAging60To90Days ?? 0;
    var referralAging91To120Days =
      local.Local1.Item.G.ReferralAging91To120Days ?? 0;
    var referralAging121To150Days =
      local.Local1.Item.G.ReferralAging121To150Days ?? 0;
    var referralAging151PlusDays =
      local.Local1.Item.G.ReferralAging151PlusDays ?? 0;

    entities.DashboardStagingPriority35.Populated = false;
    Update("UpdateDashboardStagingPriority35",
      (db, command) =>
      {
        db.SetNullableDate(command, "asOfDate", asOfDate);
        db.SetNullableInt32(command, "refAge60To90", referralAging60To90Days);
        db.SetNullableInt32(command, "refAge91To120", referralAging91To120Days);
        db.
          SetNullableInt32(command, "refAge121To150", referralAging121To150Days);
        db.SetNullableInt32(command, "refAge151Plus", referralAging151PlusDays);
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
    entities.DashboardStagingPriority35.ReferralAging60To90Days =
      referralAging60To90Days;
    entities.DashboardStagingPriority35.ReferralAging91To120Days =
      referralAging91To120Days;
    entities.DashboardStagingPriority35.ReferralAging121To150Days =
      referralAging121To150Days;
    entities.DashboardStagingPriority35.ReferralAging151PlusDays =
      referralAging151PlusDays;
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
    /// A value of AuditFlag.
    /// </summary>
    public Common AuditFlag
    {
      get => auditFlag ??= new();
      set => auditFlag = value;
    }

    private DashboardAuditData? dashboardAuditData;
    private DateWorkArea? reportStartDate;
    private DateWorkArea? reportEndDate;
    private ProgramProcessingInfo? programProcessingInfo;
    private ProgramCheckpointRestart? programCheckpointRestart;
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
      public DashboardStagingPriority35 G
      {
        get => g ??= new();
        set => g = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private DashboardStagingPriority35? g;
    }

    /// <summary>
    /// A value of NullDate.
    /// </summary>
    public DateWorkArea NullDate
    {
      get => nullDate ??= new();
      set => nullDate = value;
    }

    /// <summary>
    /// A value of CaseAssignment.
    /// </summary>
    public DateWorkArea CaseAssignment
    {
      get => caseAssignment ??= new();
      set => caseAssignment = value;
    }

    /// <summary>
    /// A value of Checkpoint.
    /// </summary>
    public LegalReferral Checkpoint
    {
      get => checkpoint ??= new();
      set => checkpoint = value;
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
    /// A value of Convert.
    /// </summary>
    public DateWorkArea Convert
    {
      get => convert ??= new();
      set => convert = value;
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
    /// A value of Begin.
    /// </summary>
    public DateWorkArea Begin
    {
      get => begin ??= new();
      set => begin = value;
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
    /// A value of Case1.
    /// </summary>
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
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
    /// A value of BatchTimestampWorkArea.
    /// </summary>
    public BatchTimestampWorkArea BatchTimestampWorkArea
    {
      get => batchTimestampWorkArea ??= new();
      set => batchTimestampWorkArea = value;
    }

    private DateWorkArea? nullDate;
    private DateWorkArea? caseAssignment;
    private LegalReferral? checkpoint;
    private DashboardStagingPriority35? dashboardStagingPriority35;
    private DateWorkArea? convert;
    private DashboardAuditData? initialized;
    private DateWorkArea? begin;
    private DateWorkArea? end;
    private EabFileHandling? eabFileHandling;
    private EabReportSend? eabReportSend;
    private Array<LocalGroup>? local1;
    private Case1? case1;
    private DashboardAuditData? dashboardAuditData;
    private Common? countCase;
    private Common? recordProcessed;
    private ProgramCheckpointRestart? programCheckpointRestart;
    private BatchTimestampWorkArea? batchTimestampWorkArea;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of CaseAssignment.
    /// </summary>
    public CaseAssignment CaseAssignment
    {
      get => caseAssignment ??= new();
      set => caseAssignment = value;
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
    /// A value of LegalReferral.
    /// </summary>
    public LegalReferral LegalReferral
    {
      get => legalReferral ??= new();
      set => legalReferral = value;
    }

    private CaseAssignment? caseAssignment;
    private DashboardStagingPriority35? dashboardStagingPriority35;
    private CseOrganization? cseOrganization;
    private Case1? case1;
    private LegalReferral? legalReferral;
  }
#endregion
}
