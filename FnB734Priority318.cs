// Program: FN_B734_PRIORITY_3_18, ID: 945148937, model: 746.
// Short name: SWE03691
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
/// A program: FN_B734_PRIORITY_3_18.
/// </para>
/// <para>
/// Priority 3-18: Federal Timeframes- Days from IWO to IWO payment
/// </para>
/// </summary>
[Serializable]
[Program("SWE03691")]
public partial class FnB734Priority318: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B734_PRIORITY_3_18 program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB734Priority318(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB734Priority318.
  /// </summary>
  public FnB734Priority318(IContext context, Import import, Export export):
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
    // Priority 3-18: Federal Timeframes- Days from IWO to IWO payment
    // -------------------------------------------------------------------------------------
    // 45CFR 303.100
    // This will count the average number of days from IWO issuance to an 
    // employer to first I type payment received.
    // Report Level: State, Judicial District, Region, Office
    // Report: Month
    // 1.	Count the first occurrence of an I type payment on a court order with 
    // a received date in the current report period.  To qualify, the previous I
    // type payment on the order must have occurred greater than 40 days ago.
    // 2.	Find most recent ORDIWO2 created for that court order.  An associated 
    // entry must exist on the IWGL screen.  ORDIWO2s with created by the DOL
    // process will be excluded.
    // 3.	If a different I type payment was created in between the ORDIWO2 
    // creation date and the most recent I type payment date, do not count case/
    // order.
    // 4.	Calculate number of days between ORDIWO2 creation date and I type 
    // payment date.  An associated entry must exist on the IWGL screen.
    // 5.	To find office to credit, look for attorney assigned to the ORDIWO2 on
    // the created date.
    // -------------------------------------------------------------------------------------
    MoveDashboardAuditData2(import.DashboardAuditData, local.Initialized);
    MoveProgramCheckpointRestart(import.ProgramCheckpointRestart,
      local.ProgramCheckpointRestart);

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
    //  090-110    cash receipt detail create timestamp
    if (AsChar(import.ProgramCheckpointRestart.RestartInd) == 'Y')
    {
      if (Equal(import.ProgramCheckpointRestart.RestartInfo, 81, 4, "3-18"))
      {
        if (!IsEmpty(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 90, 26)))
        {
          local.BatchTimestampWorkArea.TextTimestamp =
            Substring(import.ProgramCheckpointRestart.RestartInfo, 90, 26);
          UseLeCabConvertTimestamp();
          local.Checkpoint.CreatedTmst =
            local.BatchTimestampWorkArea.IefTimestamp;
        }

        // -- Load Judicial District counts.
        if (Lt(local.Null1.Timestamp, local.Checkpoint.CreatedTmst))
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
            local.Local1.Update.G.DaysToIwoPaymentAverage = 0;
            local.Local1.Update.G.DaysToIwoPaymentDenominator = 0;
            local.Local1.Update.G.DaysToIwoPaymentNumerator = 0;
          }

          local.Checkpoint.CreatedTmst =
            AddMonths(import.ReportEndDate.Timestamp, 6);
        }
      }
      else
      {
        local.Checkpoint.CreatedTmst =
          AddMonths(import.ReportEndDate.Timestamp, 6);
      }
    }
    else
    {
      local.Checkpoint.CreatedTmst =
        AddMonths(import.ReportEndDate.Timestamp, 6);
    }

    foreach(var _ in ReadCashReceiptDetail1())
    {
      local.CashReceipt.Date = entities.CashReceiptDetail.CollectionDate;
      local.CashReceipt.Time = StringToTime("23.59.59.999999") ?? default;
      UseFnBuildTimestampFrmDateTime();
      local.CashReceipt.Timestamp = entities.CashReceiptDetail.CreatedTmst;

      foreach(var _1 in ReadCashReceiptDetail2())
      {
        if (Lt(entities.N2dRead.CreatedTmst,
          entities.CashReceiptDetail.CreatedTmst))
        {
        }
        else if (Lt(entities.CashReceiptDetail.CreatedTmst,
          entities.N2dRead.CreatedTmst))
        {
          // the program has already processed a record for this day for this 
          // obligor and court order number
          goto ReadEach;
        }
      }

      local.LegalActionFound.Flag = "";

      if (ReadLegalAction())
      {
        local.LegalActionFound.Flag = "Y";
      }

      if (AsChar(local.LegalActionFound.Flag) != 'Y')
      {
        continue;
      }

      local.MiniumOrderDt.Date =
        AddDays(entities.CashReceiptDetail.CollectionDate, -40);

      if (ReadCashReceiptDetail3())
      {
        continue;

        // THERE IS ANOTHER 'I' PAYMENT SO SO NOT USE
      }

      ReadLegalActionIncomeSourceCsePerson();

      if (entities.LegalActionIncomeSource.Populated)
      {
        // we can count this record since it has a iwgl record tied to the 
        // current legal action for the obligor
      }
      else
      {
        continue;
      }

      local.Initialized.JudicialDistrict = "";
      local.Initialized.Office = 0;
      local.DashboardAuditData.Assign(local.Initialized);
      local.PreviousRecord.StandardNumber = entities.LegalAction.StandardNumber;
      local.Checkpoint.CreatedTmst = entities.CashReceiptDetail.CreatedTmst;
      local.DashboardAuditData.DebtType =
        entities.LegalActionDetail.NonFinOblgType;
      local.Convert.Date = Date(entities.LegalAction.CreatedTstamp);
      local.DashboardAuditData.DaysReported =
        DaysFromAD(entities.CashReceiptDetail.CollectionDate) - DaysFromAD
        (local.Convert.Date);

      // -- Determine office and judicial district to which case is assigned on 
      // the report period end date.
      UseFnB734DetermineJdFromOrder();

      if (IsEmpty(local.DashboardAuditData.JudicialDistrict))
      {
        continue;
      }

      local.DashboardAuditData.DashboardPriority = "3-18";
      local.DashboardAuditData.StandardNumber =
        entities.LegalAction.StandardNumber;

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

        local.Local1.Update.G.DaysToIwoPaymentDenominator =
          (local.Local1.Item.G.DaysToIwoPaymentDenominator ?? 0) + 1;
        local.Local1.Update.G.DaysToIwoPaymentNumerator =
          (local.DashboardAuditData.DaysReported ?? 0) + (
            local.Local1.Item.G.DaysToIwoPaymentNumerator ?? 0);

        if ((local.Local1.Item.G.DaysToIwoPaymentDenominator ?? 0) > 0)
        {
          local.Local1.Update.G.DaysToIwoPaymentAverage =
            (decimal)(local.Local1.Item.G.DaysToIwoPaymentNumerator ?? 0) / (
              local.Local1.Item.G.DaysToIwoPaymentDenominator ?? 0);
        }
        else
        {
          local.Local1.Update.G.DaysToIwoPaymentAverage = 0;
        }
      }

      ++local.RecordProcessed.Count;

      if (local.RecordProcessed.Count >= (
        import.ProgramCheckpointRestart.UpdateFrequencyCount ?? 0))
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

        // -- Checkpoint Info
        // Positions   Value
        // ---------   
        // ------------------------------------
        //  001-080    General Checkpoint Info for PRAD
        //  081-088    Dashboard Priority
        //  089-089    Blank
        //  090-116    cash reciept detail create timestamp
        local.BatchTimestampWorkArea.TextTimestamp = "";
        local.BatchTimestampWorkArea.IefTimestamp =
          local.Checkpoint.CreatedTmst;
        UseLeCabConvertTimestamp();
        local.ProgramCheckpointRestart.RestartInfo =
          Substring(import.ProgramCheckpointRestart.RestartInfo, 250, 1, 80) + "3-18    " +
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

ReadEach:
      ;
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
      Substring(import.ProgramCheckpointRestart.RestartInfo, 250, 1, 80) + "3-21     ";
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
    target.StandardNumber = source.StandardNumber;
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

  private void UseFnB734DetermineJdFromOrder()
  {
    var useImport = new FnB734DetermineJdFromOrder.Import();
    var useExport = new FnB734DetermineJdFromOrder.Export();

    useImport.PersistentLegalAction.Assign(entities.LegalAction);
    useImport.ReportEndDate.Date = import.ReportEndDate.Date;
    useImport.ReportStartDate.Date = import.ReportStartDate.Date;

    context.Call(FnB734DetermineJdFromOrder.Execute, useImport, useExport);

    MoveDashboardAuditData3(useExport.DashboardAuditData,
      local.DashboardAuditData);
  }

  private void UseFnBuildTimestampFrmDateTime()
  {
    var useImport = new FnBuildTimestampFrmDateTime.Import();
    var useExport = new FnBuildTimestampFrmDateTime.Export();

    MoveDateWorkArea1(local.CashReceipt, useImport.DateWorkArea);

    context.Call(FnBuildTimestampFrmDateTime.Execute, useImport, useExport);

    MoveDateWorkArea2(useExport.DateWorkArea, local.CashReceipt);
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
    var daysToIwoPaymentNumerator =
      local.Local1.Item.G.DaysToIwoPaymentNumerator ?? 0;
    var daysToIwoPaymentDenominator =
      local.Local1.Item.G.DaysToIwoPaymentDenominator ?? 0;
    var daysToIwoPaymentAverage =
      local.Local1.Item.G.DaysToIwoPaymentAverage ?? 0M;

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
        db.
          SetNullableInt32(command, "iwoPmtDaysNmr", daysToIwoPaymentNumerator);
        db.SetNullableInt32(
          command, "iwoPmtDaysDnom", daysToIwoPaymentDenominator);
        db.
          SetNullableDecimal(command, "iwoPmtDaysAvg", daysToIwoPaymentAverage);
      });

    entities.DashboardStagingPriority35.ReportMonth = reportMonth;
    entities.DashboardStagingPriority35.ReportLevel = reportLevel;
    entities.DashboardStagingPriority35.ReportLevelId = reportLevelId;
    entities.DashboardStagingPriority35.AsOfDate = asOfDate;
    entities.DashboardStagingPriority35.DaysToIwoPaymentNumerator =
      daysToIwoPaymentNumerator;
    entities.DashboardStagingPriority35.DaysToIwoPaymentDenominator =
      daysToIwoPaymentDenominator;
    entities.DashboardStagingPriority35.DaysToIwoPaymentAverage =
      daysToIwoPaymentAverage;
    entities.DashboardStagingPriority35.Populated = true;
  }

  private IEnumerable<bool> ReadCashReceiptDetail1()
  {
    return ReadEachInSeparateTransaction("ReadCashReceiptDetail1",
      (db, command) =>
      {
        db.SetDate(command, "date1", import.ReportStartDate.Date);
        db.SetDate(command, "date2", import.ReportEndDate.Date);
        db.SetDateTime(command, "createdTmst", local.Checkpoint.CreatedTmst);
      },
      (db, reader) =>
      {
        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetail.CourtOrderNumber =
          db.GetNullableString(reader, 4);
        entities.CashReceiptDetail.CollectionDate = db.GetDate(reader, 5);
        entities.CashReceiptDetail.ObligorPersonNumber =
          db.GetNullableString(reader, 6);
        entities.CashReceiptDetail.CreatedTmst = db.GetDateTime(reader, 7);
        entities.CashReceiptDetail.CltIdentifier =
          db.GetNullableInt32(reader, 8);
        entities.CashReceiptDetail.Populated = true;

        return true;
      },
      () =>
      {
        entities.CashReceiptDetail.Populated = false;
      });
  }

  private IEnumerable<bool> ReadCashReceiptDetail2()
  {
    return ReadEach("ReadCashReceiptDetail2",
      (db, command) =>
      {
        db.SetNullableString(
          command, "oblgorPrsnNbr",
          entities.CashReceiptDetail.ObligorPersonNumber);
        db.SetNullableString(
          command, "courtOrderNumber",
          entities.CashReceiptDetail.CourtOrderNumber);
        db.SetDate(
          command, "collectionDate", entities.CashReceiptDetail.CollectionDate);
      },
      (db, reader) =>
      {
        entities.N2dRead.CrvIdentifier = db.GetInt32(reader, 0);
        entities.N2dRead.CstIdentifier = db.GetInt32(reader, 1);
        entities.N2dRead.CrtIdentifier = db.GetInt32(reader, 2);
        entities.N2dRead.SequentialIdentifier = db.GetInt32(reader, 3);
        entities.N2dRead.CourtOrderNumber = db.GetNullableString(reader, 4);
        entities.N2dRead.CollectionDate = db.GetDate(reader, 5);
        entities.N2dRead.ObligorPersonNumber = db.GetNullableString(reader, 6);
        entities.N2dRead.CreatedTmst = db.GetDateTime(reader, 7);
        entities.N2dRead.CltIdentifier = db.GetNullableInt32(reader, 8);
        entities.N2dRead.Populated = true;

        return true;
      },
      () =>
      {
        entities.N2dRead.Populated = false;
      });
  }

  private bool ReadCashReceiptDetail3()
  {
    entities.N2dRead.Populated = false;

    return Read("ReadCashReceiptDetail3",
      (db, command) =>
      {
        db.SetNullableString(
          command, "oblgorPrsnNbr",
          entities.CashReceiptDetail.ObligorPersonNumber);
        db.SetNullableString(
          command, "courtOrderNumber",
          entities.CashReceiptDetail.CourtOrderNumber);
        db.SetDate(
          command, "collectionDate1",
          entities.CashReceiptDetail.CollectionDate);
        db.SetDate(command, "collectionDate2", local.MiniumOrderDt.Date);
        db.SetDateTime(
          command, "createdTstamp", entities.LegalAction.CreatedTstamp);
      },
      (db, reader) =>
      {
        entities.N2dRead.CrvIdentifier = db.GetInt32(reader, 0);
        entities.N2dRead.CstIdentifier = db.GetInt32(reader, 1);
        entities.N2dRead.CrtIdentifier = db.GetInt32(reader, 2);
        entities.N2dRead.SequentialIdentifier = db.GetInt32(reader, 3);
        entities.N2dRead.CourtOrderNumber = db.GetNullableString(reader, 4);
        entities.N2dRead.CollectionDate = db.GetDate(reader, 5);
        entities.N2dRead.ObligorPersonNumber = db.GetNullableString(reader, 6);
        entities.N2dRead.CreatedTmst = db.GetDateTime(reader, 7);
        entities.N2dRead.CltIdentifier = db.GetNullableInt32(reader, 8);
        entities.N2dRead.Populated = true;
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
        entities.DashboardStagingPriority35.DaysToIwoPaymentNumerator =
          db.GetNullableInt32(reader, 4);
        entities.DashboardStagingPriority35.DaysToIwoPaymentDenominator =
          db.GetNullableInt32(reader, 5);
        entities.DashboardStagingPriority35.DaysToIwoPaymentAverage =
          db.GetNullableDecimal(reader, 6);
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
        entities.DashboardStagingPriority35.DaysToIwoPaymentNumerator =
          db.GetNullableInt32(reader, 4);
        entities.DashboardStagingPriority35.DaysToIwoPaymentDenominator =
          db.GetNullableInt32(reader, 5);
        entities.DashboardStagingPriority35.DaysToIwoPaymentAverage =
          db.GetNullableDecimal(reader, 6);
        entities.DashboardStagingPriority35.Populated = true;
      });
  }

  private bool ReadLegalAction()
  {
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction",
      (db, command) =>
      {
        db.SetNullableString(
          command, "standardNo", entities.CashReceiptDetail.CourtOrderNumber);
        db.SetNullableDate(command, "endDate", import.ReportStartDate.Date);
        db.SetString(
          command, "cspNumber", entities.CashReceiptDetail.ObligorPersonNumber);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.ActionTaken = db.GetString(reader, 1);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 2);
        entities.LegalAction.CreatedBy = db.GetString(reader, 3);
        entities.LegalAction.CreatedTstamp = db.GetDateTime(reader, 4);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 5);
        entities.LegalAction.Populated = true;
      });
  }

  private bool ReadLegalActionIncomeSourceCsePerson()
  {
    entities.CsePerson.Populated = false;
    entities.LegalActionIncomeSource.Populated = false;

    return Read("ReadLegalActionIncomeSourceCsePerson",
      (db, command) =>
      {
        db.SetInt32(command, "lgaIdentifier", entities.LegalAction.Identifier);
        db.SetString(
          command, "cspINumber",
          entities.CashReceiptDetail.ObligorPersonNumber);
      },
      (db, reader) =>
      {
        entities.LegalActionIncomeSource.CspNumber = db.GetString(reader, 0);
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.LegalActionIncomeSource.LgaIdentifier = db.GetInt32(reader, 1);
        entities.LegalActionIncomeSource.IsrIdentifier =
          db.GetDateTime(reader, 2);
        entities.LegalActionIncomeSource.EndDate =
          db.GetNullableDate(reader, 3);
        entities.LegalActionIncomeSource.Identifier = db.GetInt32(reader, 4);
        entities.LegalActionIncomeSource.Populated = true;
        entities.CsePerson.Populated = true;
      });
  }

  private void UpdateDashboardStagingPriority35()
  {
    var asOfDate = local.Local1.Item.G.AsOfDate;
    var daysToIwoPaymentNumerator =
      local.Local1.Item.G.DaysToIwoPaymentNumerator ?? 0;
    var daysToIwoPaymentDenominator =
      local.Local1.Item.G.DaysToIwoPaymentDenominator ?? 0;
    var daysToIwoPaymentAverage =
      local.Local1.Item.G.DaysToIwoPaymentAverage ?? 0M;

    entities.DashboardStagingPriority35.Populated = false;
    Update("UpdateDashboardStagingPriority35",
      (db, command) =>
      {
        db.SetNullableDate(command, "asOfDate", asOfDate);
        db.
          SetNullableInt32(command, "iwoPmtDaysNmr", daysToIwoPaymentNumerator);
        db.SetNullableInt32(
          command, "iwoPmtDaysDnom", daysToIwoPaymentDenominator);
        db.
          SetNullableDecimal(command, "iwoPmtDaysAvg", daysToIwoPaymentAverage);
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
    entities.DashboardStagingPriority35.DaysToIwoPaymentNumerator =
      daysToIwoPaymentNumerator;
    entities.DashboardStagingPriority35.DaysToIwoPaymentDenominator =
      daysToIwoPaymentDenominator;
    entities.DashboardStagingPriority35.DaysToIwoPaymentAverage =
      daysToIwoPaymentAverage;
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
    /// A value of Null1.
    /// </summary>
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    /// <summary>
    /// A value of CashReceipt.
    /// </summary>
    public DateWorkArea CashReceipt
    {
      get => cashReceipt ??= new();
      set => cashReceipt = value;
    }

    /// <summary>
    /// A value of Checkpoint.
    /// </summary>
    public CashReceiptDetail Checkpoint
    {
      get => checkpoint ??= new();
      set => checkpoint = value;
    }

    /// <summary>
    /// A value of LegalActionFound.
    /// </summary>
    public Common LegalActionFound
    {
      get => legalActionFound ??= new();
      set => legalActionFound = value;
    }

    /// <summary>
    /// A value of MiniumOrderDt.
    /// </summary>
    public DateWorkArea MiniumOrderDt
    {
      get => miniumOrderDt ??= new();
      set => miniumOrderDt = value;
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
    /// A value of CheckpointDelete.
    /// </summary>
    public LegalAction CheckpointDelete
    {
      get => checkpointDelete ??= new();
      set => checkpointDelete = value;
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
    /// A value of Convert.
    /// </summary>
    public DateWorkArea Convert
    {
      get => convert ??= new();
      set => convert = value;
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

    private DateWorkArea? null1;
    private DateWorkArea? cashReceipt;
    private CashReceiptDetail? checkpoint;
    private Common? legalActionFound;
    private DateWorkArea? miniumOrderDt;
    private DashboardAuditData? initialized;
    private EabFileHandling? eabFileHandling;
    private EabReportSend? eabReportSend;
    private LegalAction? checkpointDelete;
    private DateWorkArea? begin;
    private DateWorkAttributes? dateWorkAttributes;
    private DateWorkArea? end;
    private BatchTimestampWorkArea? batchTimestampWorkArea;
    private DashboardAuditData? dashboardAuditData;
    private LegalAction? previousRecord;
    private Common? recordProcessed;
    private ProgramCheckpointRestart? programCheckpointRestart;
    private DateWorkArea? convert;
    private Array<LocalGroup>? local1;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of N2dRead.
    /// </summary>
    public CashReceiptDetail N2dRead
    {
      get => n2dRead ??= new();
      set => n2dRead = value;
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
    /// A value of CashReceiptDetailStatHistory.
    /// </summary>
    public CashReceiptDetailStatHistory CashReceiptDetailStatHistory
    {
      get => cashReceiptDetailStatHistory ??= new();
      set => cashReceiptDetailStatHistory = value;
    }

    /// <summary>
    /// A value of CashReceiptDetailStatus.
    /// </summary>
    public CashReceiptDetailStatus CashReceiptDetailStatus
    {
      get => cashReceiptDetailStatus ??= new();
      set => cashReceiptDetailStatus = value;
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
    /// A value of CashReceiptDetail.
    /// </summary>
    public CashReceiptDetail CashReceiptDetail
    {
      get => cashReceiptDetail ??= new();
      set => cashReceiptDetail = value;
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
    /// A value of CsePerson.
    /// </summary>
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
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
    /// A value of DashboardStagingPriority35.
    /// </summary>
    public DashboardStagingPriority35 DashboardStagingPriority35
    {
      get => dashboardStagingPriority35 ??= new();
      set => dashboardStagingPriority35 = value;
    }

    /// <summary>
    /// A value of LegalActionIncomeSource.
    /// </summary>
    public LegalActionIncomeSource LegalActionIncomeSource
    {
      get => legalActionIncomeSource ??= new();
      set => legalActionIncomeSource = value;
    }

    /// <summary>
    /// A value of Employment.
    /// </summary>
    public IncomeSource Employment
    {
      get => employment ??= new();
      set => employment = value;
    }

    /// <summary>
    /// A value of Employer.
    /// </summary>
    public Employer Employer
    {
      get => employer ??= new();
      set => employer = value;
    }

    private CaseRole? caseRole;
    private LegalActionCaseRole? legalActionCaseRole;
    private LaPersonLaCaseRole? laPersonLaCaseRole;
    private CashReceiptDetail? n2dRead;
    private Collection? collection;
    private CashReceiptDetailStatHistory? cashReceiptDetailStatHistory;
    private CashReceiptDetailStatus? cashReceiptDetailStatus;
    private CollectionType? collectionType;
    private CashReceiptDetail? cashReceiptDetail;
    private CseOrganization? cseOrganization;
    private LegalAction? legalAction;
    private CsePerson? csePerson;
    private LegalActionPerson? legalActionPerson;
    private LegalActionDetail? legalActionDetail;
    private DashboardStagingPriority35? dashboardStagingPriority35;
    private LegalActionIncomeSource? legalActionIncomeSource;
    private IncomeSource? employment;
    private Employer? employer;
  }
#endregion
}
