// Program: FN_B734_PRIORITY_5_17, ID: 945148975, model: 746.
// Short name: SWE03709
using System;
using System.Collections.Generic;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_B734_PRIORITY_5_17.
/// </para>
/// <para>
/// Priority 3-17: Aging Report of Unprocessed Legal Referrals by Attorney
/// </para>
/// </summary>
[Serializable]
[Program("SWE03709")]
public partial class FnB734Priority517: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B734_PRIORITY_5_17 program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB734Priority517(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB734Priority517.
  /// </summary>
  public FnB734Priority517(IContext context, Import import, Export export):
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
    // Priority 5-17: Aging Report of Unprocessed Legal Referrals by Attorney
    // -------------------------------------------------------------------------------------
    // Report Level: Attorney
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
    MoveDashboardAuditData2(import.DashboardAuditData,
      local.InitializedDashboardAuditData);
    MoveProgramCheckpointRestart(import.ProgramCheckpointRestart,
      local.ProgramCheckpointRestart);

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
      if (Equal(import.ProgramCheckpointRestart.RestartInfo, 81, 4, "5-17"))
      {
        if (!IsEmpty(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 90, 26)))
        {
          local.BatchTimestampWorkArea.TextTimestamp =
            Substring(import.ProgramCheckpointRestart.RestartInfo, 90, 26);
          UseLeCabConvertTimestamp();
          local.CheckpointLegalReferral.CreatedTimestamp =
            local.BatchTimestampWorkArea.IefTimestamp;
        }

        if (Lt(local.Null1.Timestamp,
          local.CheckpointLegalReferral.CreatedTimestamp))
        {
        }
        else
        {
          // this is when there is a month in change in the middle of a week. we
          // do not want to double count the results
          local.CheckpointLegalReferral.CreatedTimestamp =
            import.ReportEndDate.Timestamp;

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
      }
      else
      {
        local.CheckpointLegalReferral.CreatedTimestamp =
          import.ReportEndDate.Timestamp;

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
    }
    else
    {
      local.CheckpointLegalReferral.CreatedTimestamp =
        import.ReportEndDate.Timestamp;

      foreach(var _ in ReadDashboardStagingPriority1())
      {
        // need to clear the previcously determined totals before the program 
        // begins or the numbers will not be correct, they will reflect previous
        // run numbers also
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

    foreach(var _ in ReadLegalReferralCase())
    {
      if (!IsEmpty(entities.Case1.NoJurisdictionCd))
      {
        continue;
      }

      local.DashboardAuditData.Assign(local.InitializedDashboardAuditData);
      local.DashboardStagingPriority35.Assign(
        local.InitializedDashboardStagingPriority35);
      local.CountCase.Flag = "N";
      local.CheckpointLegalReferral.CreatedTimestamp =
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

      local.DashboardStagingPriority35.ReportLevelId = "";

      if (ReadLegalReferralAssignmentServiceProvider())
      {
        local.DashboardStagingPriority35.ReportLevelId =
          entities.ServiceProvider.UserId;
      }

      if (IsEmpty(local.DashboardStagingPriority35.ReportLevelId))
      {
        continue;
      }

      local.DashboardStagingPriority35.ReferralAging60To90Days = 0;
      local.DashboardStagingPriority35.ReferralAging91To120Days = 0;
      local.DashboardStagingPriority35.ReferralAging121To150Days = 0;
      local.DashboardStagingPriority35.ReferralAging151PlusDays = 0;
      local.Convert.Date = Date(entities.LegalReferral.CreatedTimestamp);
      local.DashboardAuditData.DaysReported =
        DaysFromAD(import.ReportEndDate.Date) - DaysFromAD(local.Convert.Date);

      if ((local.DashboardAuditData.DaysReported ?? 0) >= 60 && (
        local.DashboardAuditData.DaysReported ?? 0) <= 90)
      {
        local.DashboardStagingPriority35.ReferralAging60To90Days =
          (local.DashboardStagingPriority35.ReferralAging60To90Days ?? 0) + 1;
      }
      else if ((local.DashboardAuditData.DaysReported ?? 0) >= 91 && (
        local.DashboardAuditData.DaysReported ?? 0) <= 120)
      {
        local.DashboardStagingPriority35.ReferralAging91To120Days =
          (local.DashboardStagingPriority35.ReferralAging91To120Days ?? 0) + 1;
      }
      else if ((local.DashboardAuditData.DaysReported ?? 0) >= 121 && (
        local.DashboardAuditData.DaysReported ?? 0) <= 150)
      {
        local.DashboardStagingPriority35.ReferralAging121To150Days =
          (local.DashboardStagingPriority35.ReferralAging121To150Days ?? 0) + 1
          ;
      }
      else if ((local.DashboardAuditData.DaysReported ?? 0) >= 151)
      {
        local.DashboardStagingPriority35.ReferralAging151PlusDays =
          (local.DashboardStagingPriority35.ReferralAging151PlusDays ?? 0) + 1;
      }
      else
      {
        // if it is less than 60 days we are not counting it
        continue;
      }

      local.DashboardAuditData.LegalReferralDate = local.Convert.Date;
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

      local.DashboardStagingPriority35.ReportMonth =
        local.DashboardAuditData.ReportMonth;
      local.DashboardAuditData.CaseNumber = entities.Case1.Number;
      local.Case1.Number = entities.Case1.Number;
      local.DashboardAuditData.LegalReferralNumber =
        entities.LegalReferral.Identifier;
      local.DashboardAuditData.DashboardPriority = "5-17";
      local.DashboardAuditData.CaseDate = entities.Case1.StatusDate;
      local.DashboardAuditData.WorkerId =
        local.DashboardStagingPriority35.ReportLevelId;

      if (AsChar(import.AuditFlag.Flag) == 'Y')
      {
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
        //  090-116    legal refferal create timestamp
        local.BatchTimestampWorkArea.TextTimestamp = "";
        local.BatchTimestampWorkArea.IefTimestamp =
          local.CheckpointLegalReferral.CreatedTimestamp;
        UseLeCabConvertTimestamp();
        local.ProgramCheckpointRestart.RestartInfo =
          Substring(import.ProgramCheckpointRestart.RestartInfo, 250, 1, 80) + "5-17    " +
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
      Substring(import.ProgramCheckpointRestart.RestartInfo, 250, 1, 80) + "5-18     ";
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
    var reportMonth = import.DashboardAuditData.ReportMonth;
    var reportLevel = local.DashboardStagingPriority35.ReportLevel;
    var reportLevelId = local.DashboardStagingPriority35.ReportLevelId;
    var asOfDate = local.DashboardStagingPriority35.AsOfDate;
    var param = 0M;
    var referralAging60To90Days =
      local.DashboardStagingPriority35.ReferralAging60To90Days ?? 0;
    var referralAging91To120Days =
      local.DashboardStagingPriority35.ReferralAging91To120Days ?? 0;
    var referralAging121To150Days =
      local.DashboardStagingPriority35.ReferralAging121To150Days ?? 0;
    var referralAging151PlusDays =
      local.DashboardStagingPriority35.ReferralAging151PlusDays ?? 0;

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
        db.SetInt32(
          command, "reportMonth", import.DashboardAuditData.ReportMonth);
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

  private bool ReadLegalReferralAssignmentServiceProvider()
  {
    System.Diagnostics.Debug.Assert(entities.LegalReferral.Populated);
    entities.LegalReferralAssignment.Populated = false;
    entities.ServiceProvider.Populated = false;

    return Read("ReadLegalReferralAssignmentServiceProvider",
      (db, command) =>
      {
        db.SetInt32(command, "lgrId", entities.LegalReferral.Identifier);
        db.SetString(command, "casNo", entities.LegalReferral.CasNumber);
        db.SetDate(command, "effectiveDate", import.ReportEndDate.Date);
      },
      (db, reader) =>
      {
        entities.LegalReferralAssignment.ReasonCode = db.GetString(reader, 0);
        entities.LegalReferralAssignment.EffectiveDate = db.GetDate(reader, 1);
        entities.LegalReferralAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 2);
        entities.LegalReferralAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 3);
        entities.LegalReferralAssignment.SpdId = db.GetInt32(reader, 4);
        entities.LegalReferralAssignment.OffId = db.GetInt32(reader, 5);
        entities.LegalReferralAssignment.OspCode = db.GetString(reader, 6);
        entities.LegalReferralAssignment.OspDate = db.GetDate(reader, 7);
        entities.LegalReferralAssignment.CasNo = db.GetString(reader, 8);
        entities.LegalReferralAssignment.LgrId = db.GetInt32(reader, 9);
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 10);
        entities.ServiceProvider.UserId = db.GetString(reader, 11);
        entities.ServiceProvider.RoleCode = db.GetNullableString(reader, 12);
        entities.LegalReferralAssignment.Populated = true;
        entities.ServiceProvider.Populated = true;
      });
  }

  private IEnumerable<bool> ReadLegalReferralCase()
  {
    return ReadEachInSeparateTransaction("ReadLegalReferralCase",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTimestamp1",
          local.CheckpointLegalReferral.CreatedTimestamp);
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
        entities.LegalReferral.ReferralDate = db.GetDate(reader, 4);
        entities.LegalReferral.CreatedTimestamp = db.GetDateTime(reader, 5);
        entities.LegalReferral.ReferralReason1 = db.GetString(reader, 6);
        entities.LegalReferral.ReferralReason2 = db.GetString(reader, 7);
        entities.LegalReferral.ReferralReason3 = db.GetString(reader, 8);
        entities.LegalReferral.ReferralReason4 = db.GetString(reader, 9);
        entities.LegalReferral.CourtCaseNumber =
          db.GetNullableString(reader, 10);
        entities.LegalReferral.TribunalId = db.GetNullableInt32(reader, 11);
        entities.Case1.Status = db.GetNullableString(reader, 12);
        entities.Case1.StatusDate = db.GetNullableDate(reader, 13);
        entities.Case1.CseOpenDate = db.GetNullableDate(reader, 14);
        entities.Case1.CreatedTimestamp = db.GetDateTime(reader, 15);
        entities.Case1.InterstateCaseId = db.GetNullableString(reader, 16);
        entities.Case1.NoJurisdictionCd = db.GetNullableString(reader, 17);
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

  private void UpdateDashboardStagingPriority1()
  {
    entities.DashboardStagingPriority35.Populated = false;
    Update("UpdateDashboardStagingPriority1",
      (db, command) =>
      {
        db.SetNullableInt32(command, "refAge60To90", 0);
        db.SetNullableInt32(command, "refAge91To120", 0);
        db.SetNullableInt32(command, "refAge121To150", 0);
        db.SetNullableInt32(command, "refAge151Plus", 0);
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

    entities.DashboardStagingPriority35.ReferralAging60To90Days = 0;
    entities.DashboardStagingPriority35.ReferralAging91To120Days = 0;
    entities.DashboardStagingPriority35.ReferralAging121To150Days = 0;
    entities.DashboardStagingPriority35.ReferralAging151PlusDays = 0;
    entities.DashboardStagingPriority35.Populated = true;
  }

  private void UpdateDashboardStagingPriority2()
  {
    var asOfDate = local.DashboardStagingPriority35.AsOfDate;
    var referralAging60To90Days =
      (local.DashboardStagingPriority35.ReferralAging60To90Days ?? 0) +
      (entities.DashboardStagingPriority35.ReferralAging60To90Days ?? 0);
    var referralAging91To120Days =
      (local.DashboardStagingPriority35.ReferralAging91To120Days ?? 0) +
      (entities.DashboardStagingPriority35.ReferralAging91To120Days ?? 0);
    var referralAging121To150Days =
      (local.DashboardStagingPriority35.ReferralAging121To150Days ?? 0) +
      (entities.DashboardStagingPriority35.ReferralAging121To150Days ?? 0);
    var referralAging151PlusDays =
      (local.DashboardStagingPriority35.ReferralAging151PlusDays ?? 0) +
      (entities.DashboardStagingPriority35.ReferralAging151PlusDays ?? 0);

    entities.DashboardStagingPriority35.Populated = false;
    Update("UpdateDashboardStagingPriority2",
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
    /// <summary>
    /// A value of Null1.
    /// </summary>
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    /// <summary>
    /// A value of CheckpointLegalReferral.
    /// </summary>
    public LegalReferral CheckpointLegalReferral
    {
      get => checkpointLegalReferral ??= new();
      set => checkpointLegalReferral = value;
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
    /// A value of InitializedDashboardAuditData.
    /// </summary>
    public DashboardAuditData InitializedDashboardAuditData
    {
      get => initializedDashboardAuditData ??= new();
      set => initializedDashboardAuditData = value;
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

    /// <summary>
    /// A value of CheckpointLegalAction.
    /// </summary>
    public LegalAction CheckpointLegalAction
    {
      get => checkpointLegalAction ??= new();
      set => checkpointLegalAction = value;
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
    private LegalReferral? checkpointLegalReferral;
    private DashboardStagingPriority35? dashboardStagingPriority35;
    private DateWorkArea? convert;
    private DashboardAuditData? initializedDashboardAuditData;
    private DateWorkArea? begin;
    private DateWorkArea? end;
    private EabFileHandling? eabFileHandling;
    private EabReportSend? eabReportSend;
    private Case1? case1;
    private DashboardAuditData? dashboardAuditData;
    private Common? countCase;
    private Common? recordProcessed;
    private ProgramCheckpointRestart? programCheckpointRestart;
    private BatchTimestampWorkArea? batchTimestampWorkArea;
    private LegalAction? checkpointLegalAction;
    private DashboardStagingPriority35? initializedDashboardStagingPriority35;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of LegalReferralAssignment.
    /// </summary>
    public LegalReferralAssignment LegalReferralAssignment
    {
      get => legalReferralAssignment ??= new();
      set => legalReferralAssignment = value;
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

    /// <summary>
    /// A value of ServiceProvider.
    /// </summary>
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
    }

    /// <summary>
    /// A value of OfficeServiceProvider.
    /// </summary>
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
    }

    private LegalReferralAssignment? legalReferralAssignment;
    private DashboardStagingPriority35? dashboardStagingPriority35;
    private Case1? case1;
    private LegalReferral? legalReferral;
    private ServiceProvider? serviceProvider;
    private OfficeServiceProvider? officeServiceProvider;
  }
#endregion
}
