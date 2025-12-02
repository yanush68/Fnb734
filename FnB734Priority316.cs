// Program: FN_B734_PRIORITY_3_16, ID: 945148935, model: 746.
// Short name: SWE03689
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
/// A program: FN_B734_PRIORITY_3_16.
/// </para>
/// <para>
/// Priority 3-16 Days From Locate(?) to Service of Process
/// </para>
/// </summary>
[Serializable]
[Program("SWE03689")]
public partial class FnB734Priority316: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B734_PRIORITY_3_16 program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB734Priority316(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB734Priority316.
  /// </summary>
  public FnB734Priority316(IContext context, Import import, Export export):
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
    // Priority 3-16 Days From Locate(?) to Service of Process
    // -------------------------------------------------------------------------------------
    // Caseworker Measure?  Attorney Measure?  Attorney may not get a referral 
    // until well after the locate event (negative impact to attorney).  Once
    // the attorney gets it, they may not request service until well after the
    // referral date (possible negative impact to caseworker).
    // Report Level: State, Judicial District, Region, Office, Supervisor, 
    // Caseworker
    // Report Period: Month
    // 1)	Find all SERV records with a Return Date entered in the current report
    // period.  SERV record must be attached to a P class legal action.
    // Return Date is an indication that service was either successful or
    // document as unsuccessful, both of which meet the federal requirement.
    // 2)	Look for AP/CH combo on LROL for the legal action to find AP.  Then 
    // credit all cases where AP/CH combo is active as of the serve return date.
    // SERV has a Requested Servee field that is manually entered it does
    // not show the KAECSES person record  Have to have a way to tie it to an AP
    // (for locate purposes) and to a case (to report to a caseworker/office/etc
    // ).
    // 3)	Find the most recent qualifying locate event PRIOR TO the Service 
    // Request Date on SERV  (An address could be verified 6/12/12, and service
    // requested on 6/15/12.  A new address is verified on 7/22/12.  We do not
    // want to count the # of days from 7/22/12 to Process of Service.  We want
    // to count the # of days from the initial locate to Process of Service).
    // **NOTE**  Considered counting the # of Days from first NCP locate AFTER 
    // case opening to Process of Service.  This isnt always fair, though  We
    // may have the NCP located 2/12/12, but not get all the necessary paperwork
    // from the non-custodial parent until 3/15/12 we dont necessarily want to
    // start from that 2/12/12 date  Also, in that situation, we may not re-
    // verify the address after we get all the necessary paperwork.  We may just
    // request service with the  existing (1+ month old) address.  Then, were
    // counting # of days from 2/12/12 (even though we werent even thinking
    // about service yet) to Process of Service.  Is this okay?
    // 4)	Qualifying locate events:
    // a)	ADDR- A Verified Date is entered on type R address record.  Clock 
    // starts from Verified Date.
    // b)	FADS- A Verified Date is entered (no type exists) on address record.  
    // Clock starts from Verified Date.
    // c)	INCS- A Return Date is entered with type E and Return Code E.  
    // Clock starts from Return Date.
    // 5)	The qualifying locate event does not have to be active as of refresh
    // date.  (The record can be end-dated)
    // 6)	Count number of days from locate date to SERV Return Date
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
    local.ReportingPeriod.Update.EndDate.Timestamp =
      import.ReportEndDate.Timestamp;
    local.Null1.Date = new DateTime(1, 1, 1);

    // -- Checkpoint Info
    // Positions   Value
    // ---------   
    // ------------------------------------
    //  001-080    General Checkpoint Info for PRAD
    //  081-088    Dashboard Priority
    //  089-089    Blank
    //  090-116    legal action create timestamp
    //  117-117    period on
    if (AsChar(import.ProgramCheckpointRestart.RestartInd) == 'Y')
    {
      if (Equal(import.ProgramCheckpointRestart.RestartInfo, 81, 4, "3-16"))
      {
        if (!IsEmpty(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 90, 26)))
        {
          local.SubscrpitCount.Count =
            (int)StringToNumber(Substring(
              import.ProgramCheckpointRestart.RestartInfo, 250, 117, 1));
          local.BatchTimestampWorkArea.TextTimestamp =
            Substring(import.ProgramCheckpointRestart.RestartInfo, 90, 26);
          UseLeCabConvertTimestamp();
          local.Checkpoint.CreatedTstamp =
            local.BatchTimestampWorkArea.IefTimestamp;
        }
      }
      else
      {
        local.Checkpoint.CreatedTstamp = import.ReportEndDate.Timestamp;
      }
    }
    else
    {
      local.Checkpoint.CreatedTstamp = import.ReportEndDate.Timestamp;
    }

    local.ReportingPeriod.Index = 0;

    for(var limit = local.ReportingPeriod.Count; local.ReportingPeriod.Index < limit
      ; ++local.ReportingPeriod.Index)
    {
      if (!local.ReportingPeriod.CheckSize())
      {
        break;
      }

      local.Begin.Date = local.ReportingPeriod.Item.BeingDate.Date;
      local.Begin.Timestamp = local.ReportingPeriod.Item.BeingDate.Timestamp;
      local.End.Timestamp = local.ReportingPeriod.Item.EndDate.Timestamp;
      local.End.Date = local.ReportingPeriod.Item.EndDate.Date;
      local.Initialized.ReportMonth =
        local.ReportingPeriod.Item.DashboardAuditData.ReportMonth;

      for(local.Local1.Index = 0; local.Local1.Index < local.Local1.Count; ++
        local.Local1.Index)
      {
        if (!local.Local1.CheckSize())
        {
          break;
        }

        local.Local1.Update.G.ReportMonth =
          local.ReportingPeriod.Item.DashboardAuditData.ReportMonth;
      }

      local.Local1.CheckIndex();

      if (local.SubscrpitCount.Count == local.ReportingPeriod.Index + 1)
      {
        local.Checkpoint.CreatedTstamp =
          local.BatchTimestampWorkArea.IefTimestamp;

        if (!IsEmpty(import.ProgramCheckpointRestart.RestartInfo))
        {
          // -- Load Judicial District counts.
          if (Lt(local.Null1.Timestamp, local.Checkpoint.CreatedTstamp))
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
            // this is when there is a month in change in the middle of a week. 
            // we do not want to double count the results
            foreach(var _ in ReadDashboardStagingPriority1())
            {
              local.Local1.Index =
                (int)StringToNumber(entities.DashboardStagingPriority35.
                  ReportLevelId) - 1;
              local.Local1.CheckSize();

              local.Local1.Update.G.Assign(entities.DashboardStagingPriority35);
              local.Local1.Update.G.DaysToReturnOfServiceAvg = 0;
              local.Local1.Update.G.DaysToReturnOfServiceDenom = 0;
              local.Local1.Update.G.DaysToReturnOfSrvcNumer = 0;
            }

            local.Checkpoint.CreatedTstamp = import.ReportEndDate.Timestamp;
          }
        }
      }
      else
      {
        local.Checkpoint.CreatedTstamp =
          local.ReportingPeriod.Item.EndDate.Timestamp;

        for(local.Local1.Index = 0; local.Local1.Index < local.Local1.Count; ++
          local.Local1.Index)
        {
          if (!local.Local1.CheckSize())
          {
            break;
          }

          local.Local1.Update.G.DaysToReturnOfServiceAvg = 0;
          local.Local1.Update.G.DaysToReturnOfServiceDenom = 0;
          local.Local1.Update.G.DaysToReturnOfSrvcNumer = 0;
        }

        local.Local1.CheckIndex();
      }

      foreach(var _ in ReadLegalActionServiceProcessFipsTribunal())
      {
        local.Checkpoint.CreatedTstamp = entities.LegalAction.CreatedTstamp;
        local.Initialized.JudicialDistrict = "";
        local.Initialized.Office = 0;
        local.DashboardAuditData.Assign(local.Initialized);
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

        foreach(var _1 in ReadLegalReferralCase())
        {
          if (!Lt(local.Null1.Date, entities.LegalReferral.ReferralDate))
          {
            continue;
          }

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

            local.DashboardAuditData.CaseNumber = entities.Case1.Number;
            local.Case1.Number = entities.Case1.Number;
            local.DashboardAuditData.DaysReported =
              DaysFromAD(entities.ServiceProcess.ReturnDate) - DaysFromAD
              (entities.LegalReferral.ReferralDate);
            local.DashboardAuditData.LegalReferralDate =
              entities.LegalReferral.ReferralDate;
            local.DashboardAuditData.LegalActionDate =
              entities.LegalAction.FiledDate;
            local.DashboardAuditData.DashboardPriority = "3-16";
            local.DashboardAuditData.StandardNumber =
              entities.LegalAction.StandardNumber;
            local.CountCase.Flag = "Y";

            // -- Determine office and judicial district to which case is 
            // assigned on the report period end date.
            UseFnB734DetermineJdFromOrder();

            if (IsEmpty(local.DashboardAuditData.JudicialDistrict))
            {
              goto ReadEach;
            }

            local.DashboardAuditData.DashboardPriority = "3-16";
            local.DashboardAuditData.StandardNumber =
              entities.LegalAction.StandardNumber;
            local.DashboardAuditData.DebtType =
              entities.LegalActionDetail.NonFinOblgType;
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
                (int)StringToNumber(local.DashboardAuditData.JudicialDistrict) -
                1;
              local.Local1.CheckSize();

              local.Local1.Update.G.DaysToReturnOfSrvcNumer =
                (local.Local1.Item.G.DaysToReturnOfSrvcNumer ?? 0) + (
                  local.DashboardAuditData.DaysReported ?? 0);
              local.Local1.Update.G.DaysToReturnOfServiceDenom =
                (local.Local1.Item.G.DaysToReturnOfServiceDenom ?? 0) + 1;

              if ((local.Local1.Item.G.DaysToReturnOfServiceDenom ?? 0) > 0 && (
                local.Local1.Item.G.DaysToReturnOfSrvcNumer ?? 0) > 0)
              {
                local.Local1.Update.G.DaysToReturnOfServiceAvg =
                  (decimal)(local.Local1.Item.G.DaysToReturnOfSrvcNumer ?? 0) /
                  (local.Local1.Item.G.DaysToReturnOfServiceDenom ?? 0);
              }
              else
              {
                local.Local1.Update.G.DaysToReturnOfServiceAvg = 0;
              }
            }
          }
          else
          {
            continue;
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
          //  090-116    legal action create timestamp
          //  117-117    period on
          local.BatchTimestampWorkArea.TextTimestamp = "";
          local.BatchTimestampWorkArea.IefTimestamp =
            local.Checkpoint.CreatedTstamp;
          UseLeCabConvertTimestamp();
          local.ProgramCheckpointRestart.RestartInfo =
            Substring(import.ProgramCheckpointRestart.RestartInfo, 250, 1, 80) +
            "3-16    " + " " + String
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
      Substring(import.ProgramCheckpointRestart.RestartInfo, 250, 1, 80) + "3-17    ";
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
    useImport.ReportStartDate.Date = local.Begin.Date;
    useImport.ReportEndDate.Date = local.End.Date;

    context.Call(FnB734DetermineJdFromOrder.Execute, useImport, useExport);

    MoveDashboardAuditData3(useExport.DashboardAuditData,
      local.DashboardAuditData);
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
    var daysToReturnOfSrvcNumer =
      local.Local1.Item.G.DaysToReturnOfSrvcNumer ?? 0;
    var daysToReturnOfServiceDenom =
      local.Local1.Item.G.DaysToReturnOfServiceDenom ?? 0;
    var daysToReturnOfServiceAvg =
      local.Local1.Item.G.DaysToReturnOfServiceAvg ?? 0M;

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
        db.SetNullableInt32(command, "retServDaysNmr", daysToReturnOfSrvcNumer);
        db.SetNullableInt32(
          command, "retSrvDaysDnom", daysToReturnOfServiceDenom);
        db.SetNullableDecimal(
          command, "retServDaysAvg", daysToReturnOfServiceAvg);
      });

    entities.DashboardStagingPriority35.ReportMonth = reportMonth;
    entities.DashboardStagingPriority35.ReportLevel = reportLevel;
    entities.DashboardStagingPriority35.ReportLevelId = reportLevelId;
    entities.DashboardStagingPriority35.AsOfDate = asOfDate;
    entities.DashboardStagingPriority35.DaysToReturnOfSrvcNumer =
      daysToReturnOfSrvcNumer;
    entities.DashboardStagingPriority35.DaysToReturnOfServiceDenom =
      daysToReturnOfServiceDenom;
    entities.DashboardStagingPriority35.DaysToReturnOfServiceAvg =
      daysToReturnOfServiceAvg;
    entities.DashboardStagingPriority35.Populated = true;
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
          command, "reportMonth",
          local.ReportingPeriod.Item.DashboardAuditData.ReportMonth);
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
        entities.DashboardStagingPriority35.DaysToReturnOfSrvcNumer =
          db.GetNullableInt32(reader, 4);
        entities.DashboardStagingPriority35.DaysToReturnOfServiceDenom =
          db.GetNullableInt32(reader, 5);
        entities.DashboardStagingPriority35.DaysToReturnOfServiceAvg =
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
        entities.DashboardStagingPriority35.DaysToReturnOfSrvcNumer =
          db.GetNullableInt32(reader, 4);
        entities.DashboardStagingPriority35.DaysToReturnOfServiceDenom =
          db.GetNullableInt32(reader, 5);
        entities.DashboardStagingPriority35.DaysToReturnOfServiceAvg =
          db.GetNullableDecimal(reader, 6);
        entities.DashboardStagingPriority35.Populated = true;
      });
  }

  private IEnumerable<bool> ReadLegalActionServiceProcessFipsTribunal()
  {
    return ReadEachInSeparateTransaction(
      "ReadLegalActionServiceProcessFipsTribunal",
      (db, command) =>
      {
        db.SetDate(command, "date1", local.Begin.Date);
        db.SetDate(command, "date2", local.End.Date);
        db.
          SetDateTime(command, "createdTstamp", local.Checkpoint.CreatedTstamp);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.ServiceProcess.LgaIdentifier = db.GetInt32(reader, 0);
        entities.LegalAction.Classification = db.GetString(reader, 1);
        entities.LegalAction.ActionTaken = db.GetString(reader, 2);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 3);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 4);
        entities.LegalAction.CreatedTstamp = db.GetDateTime(reader, 5);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 6);
        entities.ServiceProcess.ReturnDate = db.GetNullableDate(reader, 7);
        entities.ServiceProcess.Identifier = db.GetInt32(reader, 8);
        entities.Fips.State = db.GetInt32(reader, 9);
        entities.Tribunal.FipState = db.GetNullableInt32(reader, 9);
        entities.Fips.County = db.GetInt32(reader, 10);
        entities.Tribunal.FipCounty = db.GetNullableInt32(reader, 10);
        entities.Fips.Location = db.GetInt32(reader, 11);
        entities.Tribunal.FipLocation = db.GetNullableInt32(reader, 11);
        entities.Fips.CountyDescription = db.GetNullableString(reader, 12);
        entities.Fips.StateAbbreviation = db.GetString(reader, 13);
        entities.Tribunal.Name = db.GetString(reader, 14);
        entities.Tribunal.JudicialDistrict = db.GetString(reader, 15);
        entities.Tribunal.Identifier = db.GetInt32(reader, 16);
        entities.LegalAction.Populated = true;
        entities.ServiceProcess.Populated = true;
        entities.Fips.Populated = db.GetNullableInt32(reader, 9) != null;
        entities.Tribunal.Populated = db.GetNullableString(reader, 14) != null;

        return true;
      },
      () =>
      {
        entities.ServiceProcess.Populated = false;
        entities.LegalAction.Populated = false;
        entities.Fips.Populated = false;
        entities.Tribunal.Populated = false;
      });
  }

  private IEnumerable<bool> ReadLegalReferralCase()
  {
    return ReadEach("ReadLegalReferralCase",
      (db, command) =>
      {
        db.SetDate(command, "referralDate", entities.ServiceProcess.ReturnDate);
        db.
          SetNullableDate(command, "statusDate", entities.LegalAction.FiledDate);
        db.SetNullableInt32(
          command, "lgaIdentifier", entities.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalReferral.CasNumber = db.GetString(reader, 0);
        entities.Case1.Number = db.GetString(reader, 0);
        entities.LegalReferral.Identifier = db.GetInt32(reader, 1);
        entities.LegalReferral.StatusDate = db.GetNullableDate(reader, 2);
        entities.LegalReferral.Status = db.GetNullableString(reader, 3);
        entities.LegalReferral.ReferralDate = db.GetDate(reader, 4);
        entities.LegalReferral.ReferralReason1 = db.GetString(reader, 5);
        entities.LegalReferral.ReferralReason2 = db.GetString(reader, 6);
        entities.LegalReferral.ReferralReason3 = db.GetString(reader, 7);
        entities.LegalReferral.ReferralReason4 = db.GetString(reader, 8);
        entities.LegalReferral.Populated = true;
        entities.Case1.Populated = true;

        return true;
      },
      () =>
      {
        entities.LegalReferral.Populated = false;
        entities.Case1.Populated = false;
      });
  }

  private void UpdateDashboardStagingPriority35()
  {
    var asOfDate = local.Local1.Item.G.AsOfDate;
    var daysToReturnOfSrvcNumer =
      local.Local1.Item.G.DaysToReturnOfSrvcNumer ?? 0;
    var daysToReturnOfServiceDenom =
      local.Local1.Item.G.DaysToReturnOfServiceDenom ?? 0;
    var daysToReturnOfServiceAvg =
      local.Local1.Item.G.DaysToReturnOfServiceAvg ?? 0M;

    entities.DashboardStagingPriority35.Populated = false;
    Update("UpdateDashboardStagingPriority35",
      (db, command) =>
      {
        db.SetNullableDate(command, "asOfDate", asOfDate);
        db.SetNullableInt32(command, "retServDaysNmr", daysToReturnOfSrvcNumer);
        db.SetNullableInt32(
          command, "retSrvDaysDnom", daysToReturnOfServiceDenom);
        db.SetNullableDecimal(
          command, "retServDaysAvg", daysToReturnOfServiceAvg);
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
    entities.DashboardStagingPriority35.DaysToReturnOfSrvcNumer =
      daysToReturnOfSrvcNumer;
    entities.DashboardStagingPriority35.DaysToReturnOfServiceDenom =
      daysToReturnOfServiceDenom;
    entities.DashboardStagingPriority35.DaysToReturnOfServiceAvg =
      daysToReturnOfServiceAvg;
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
    /// A value of Null1.
    /// </summary>
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
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

    private DashboardAuditData? initialized;
    private EabFileHandling? eabFileHandling;
    private EabReportSend? eabReportSend;
    private LegalAction? checkpoint;
    private DateWorkArea? begin;
    private DateWorkAttributes? dateWorkAttributes;
    private DateWorkArea? end;
    private LegalAction? previousRecord;
    private DashboardAuditData? dashboardAuditData;
    private Common? countCase;
    private Common? recordProcessed;
    private ProgramCheckpointRestart? programCheckpointRestart;
    private BatchTimestampWorkArea? batchTimestampWorkArea;
    private Common? subscrpitCount;
    private Array<ReportingPeriodGroup>? reportingPeriod;
    private DateWorkArea? null1;
    private Array<LocalGroup>? local1;
    private Case1? case1;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of LegalReferralCaseRole.
    /// </summary>
    public LegalReferralCaseRole LegalReferralCaseRole
    {
      get => legalReferralCaseRole ??= new();
      set => legalReferralCaseRole = value;
    }

    /// <summary>
    /// A value of ServiceProcess.
    /// </summary>
    public ServiceProcess ServiceProcess
    {
      get => serviceProcess ??= new();
      set => serviceProcess = value;
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
    /// A value of LegalReferral.
    /// </summary>
    public LegalReferral LegalReferral
    {
      get => legalReferral ??= new();
      set => legalReferral = value;
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
    /// A value of LaPersonLaCaseRole.
    /// </summary>
    public LaPersonLaCaseRole LaPersonLaCaseRole
    {
      get => laPersonLaCaseRole ??= new();
      set => laPersonLaCaseRole = value;
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
    /// A value of Ch2Nd.
    /// </summary>
    public CaseRole Ch2Nd
    {
      get => ch2Nd ??= new();
      set => ch2Nd = value;
    }

    private LegalReferralCaseRole? legalReferralCaseRole;
    private ServiceProcess? serviceProcess;
    private CseOrganization? cseOrganization;
    private LegalAction? legalAction;
    private LegalReferral? legalReferral;
    private Case1? case1;
    private CaseRole? caseRole;
    private CsePerson? csePerson;
    private LegalActionPerson? legalActionPerson;
    private Fips? fips;
    private Tribunal? tribunal;
    private LegalActionDetail? legalActionDetail;
    private DashboardStagingPriority35? dashboardStagingPriority35;
    private LaPersonLaCaseRole? laPersonLaCaseRole;
    private LegalActionCaseRole? legalActionCaseRole;
    private CaseRole? ch2Nd;
  }
#endregion
}
