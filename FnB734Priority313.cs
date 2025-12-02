// Program: FN_B734_PRIORITY_3_13, ID: 945148933, model: 746.
// Short name: SWE03686
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
/// A program: FN_B734_PRIORITY_3_13.
/// </para>
/// <para>
/// Priority 3-13: Collections by Type
/// </para>
/// </summary>
[Serializable]
[Program("SWE03686")]
public partial class FnB734Priority313: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B734_PRIORITY_3_13 program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB734Priority313(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB734Priority313.
  /// </summary>
  public FnB734Priority313(IContext context, Import import, Export export):
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
    // Priority 3-13: Collections by Type
    // -------------------------------------------------------------------------------------
    // Report Level: State, Judicial District, Region, Office, Supervisor, 
    // Caseworker
    // Report Period: Month
    // Use Definition of Collections from Priorities 1-5 and 1-6 and also use 
    // the following rules:
    // 1)	Credit collections to caseworker assigned to the case as of the 
    // refresh date.
    // 2)	Sort by collection types: S, F, I, U, C.
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

    if (AsChar(import.ProgramCheckpointRestart.RestartInd) == 'Y')
    {
      if (Equal(import.ProgramCheckpointRestart.RestartInfo, 81, 4, "3-13"))
      {
        local.Restart.SystemGeneratedIdentifier =
          (int)StringToNumber(Substring(
            import.ProgramCheckpointRestart.RestartInfo, 250, 90, 9));

        // -- Load Judicial District counts.
        if (local.Restart.SystemGeneratedIdentifier > 0)
        {
          foreach(var _ in ReadDashboardStagingPriority1())
          {
            local.Local1.Index =
              (int)StringToNumber(entities.DashboardStagingPriority35.
                ReportLevelId) - 1;
            local.Local1.CheckSize();

            MoveDashboardStagingPriority35(entities.DashboardStagingPriority35,
              local.Local1.Update.G);
          }
        }
        else
        {
          // this is when there is a month in change in the middle of a week. we
          // do not want to double count the results
          foreach(var _ in ReadDashboardStagingPriority2())
          {
            local.Local1.Index =
              (int)StringToNumber(entities.DashboardStagingPriority35.
                ReportLevelId) - 1;
            local.Local1.CheckSize();

            MoveDashboardStagingPriority35(entities.DashboardStagingPriority35,
              local.Local1.Update.G);
            local.Local1.Update.G.CtypeCollectionAmount = 0;
            local.Local1.Update.G.CtypePercentOfTotal = 0;
            local.Local1.Update.G.FtypeCollectionAmount = 0;
            local.Local1.Update.G.FtypePercentOfTotal = 0;
            local.Local1.Update.G.ItypeCollectionAmount = 0;
            local.Local1.Update.G.ItypePercentOfTotal = 0;
            local.Local1.Update.G.StypeCollectionAmount = 0;
            local.Local1.Update.G.StypePercentOfTotal = 0;
            local.Local1.Update.G.UtypeCollectionAmount = 0;
            local.Local1.Update.G.UtypePercentOfTotal = 0;
            local.Local1.Update.G.TotalCollectionAmount = 0;
          }
        }
      }
      else
      {
        local.Restart.SystemGeneratedIdentifier = 0;
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
          // -- Save Judicial District counts.
          for(local.Local1.Index = 0; local.Local1.Index < local.Local1.Count; ++
            local.Local1.Index)
          {
            if (!local.Local1.CheckSize())
            {
              break;
            }

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
          //  090-098    Collection System Generated Identifier
          local.ProgramCheckpointRestart.RestartInd = "Y";
          local.ProgramCheckpointRestart.RestartInfo =
            Substring(import.ProgramCheckpointRestart.RestartInfo, 250, 1, 80) +
            "3-13    " + " " + NumberToString
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
      local.DashboardAuditData.Assign(local.Initialized);

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
            UseFnB734DetermineJdFromOrder1();
            local.UseApSupportedOnly.Flag = "Y";
            UseFnB734DetermineJdFromOrder2();
            local.DashboardAuditData.JudicialDistrict =
              local.Hold.JudicialDistrict;
            local.DashboardAuditData.Office = local.Hold.Office ?? 0;
            local.DashboardAuditData.StandardNumber = local.Hold.StandardNumber;

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
            local.Temp.Date = entities.CaseAssignment.DiscontinueDate;
          }
          else
          {
            local.Temp.Date = import.ReportEndDate.Date;
          }
        }

        UseFnB734DetermineJdFromCase();

        if (IsEmpty(local.DashboardAuditData.CaseNumber))
        {
          local.DashboardAuditData.CaseNumber = entities.Case1.Number;
        }
      }
      else
      {
        // -- For non Fees, use the order to determine Judicial District.
        UseFnB734DetermineJdFromOrder1();
        local.UseApSupportedOnly.Flag = "Y";
        UseFnB734DetermineJdFromOrder2();
        local.DashboardAuditData.JudicialDistrict = local.Hold.JudicialDistrict;
        local.DashboardAuditData.Office = local.Hold.Office ?? 0;
        local.DashboardAuditData.StandardNumber = local.Hold.StandardNumber;
      }

Test:

      // -- Increment Judicial District Level
      if (!IsEmpty(local.DashboardAuditData.JudicialDistrict))
      {
        local.Local1.Index =
          (int)StringToNumber(local.DashboardAuditData.JudicialDistrict) - 1;
        local.Local1.CheckSize();

        // -- Increment In-Month Judicial District Level
        // @@@@@  This is where you need to keep track of the collection amounts
        // by type...
        switch(TrimEnd(entities.CollectionType.Code))
        {
          case "S":
            local.Local1.Update.G.StypeCollectionAmount =
              (local.Local1.Item.G.StypeCollectionAmount ?? 0M) + (
                local.DashboardAuditData.CollectionAmount ?? 0M);

            // set g_local dashboard_staging_priority_35 
            // s_type_collection_amount to g_local dashboard_staging_priority_35
            // s_type_collection_amount + local dashboard_audit_data
            // collection_amount
            break;
          case "F":
            local.Local1.Update.G.FtypeCollectionAmount =
              (local.Local1.Item.G.FtypeCollectionAmount ?? 0M) + (
                local.DashboardAuditData.CollectionAmount ?? 0M);

            // set g_local dashboard_staging_priority_35 
            // f_type_collection_amount to g_local dashboard_staging_priority_35
            // f_type_collection_amount + local dashboard_audit_data
            // collection_amount
            break;
          case "I":
            local.Local1.Update.G.ItypeCollectionAmount =
              (local.Local1.Item.G.ItypeCollectionAmount ?? 0M) + (
                local.DashboardAuditData.CollectionAmount ?? 0M);

            // set g_local dashboard_staging_priority_35 
            // i_type_collection_amount to g_local dashboard_staging_priority_35
            // i_type_collection_amount + local dashboard_audit_data
            // collection_amount
            break;
          case "U":
            local.Local1.Update.G.UtypeCollectionAmount =
              (local.Local1.Item.G.UtypeCollectionAmount ?? 0M) + (
                local.DashboardAuditData.CollectionAmount ?? 0M);

            // set g_local dashboard_staging_priority_35 
            // u_type_collection_amount to g_local dashboard_staging_priority_35
            // u_type_collection_amount + local dashboard_audit_data
            // collection_amount
            break;
          case "C":
            local.Local1.Update.G.CtypeCollectionAmount =
              (local.Local1.Item.G.CtypeCollectionAmount ?? 0M) + (
                local.DashboardAuditData.CollectionAmount ?? 0M);

            // set g_local dashboard_staging_priority_35 
            // c_type_collection_amount to g_local dashboard_staging_priority_35
            // c_type_collection_amount + local dashboard_audit_data
            // collection_amount
            break;
          default:
            break;
        }

        local.Local1.Update.G.TotalCollectionAmount =
          (local.Local1.Item.G.TotalCollectionAmount ?? 0M) + (
            local.DashboardAuditData.CollectionAmount ?? 0M);

        // set g_local dashboard_staging_priority_35 total_collection_amount to 
        // g_local dashboard_staging_priority_35 total_collection_amount + local
        // dashboard_audit_data collection_amount
      }
      else
      {
        continue;
      }

      // -- Log to the audit table.
      local.DashboardAuditData.DashboardPriority = "3-13" + String
        (local.ReportingAbbreviation.Text2, TextWorkArea.Text2_MaxLength);
      local.DashboardAuditData.CollectionCreatedDate =
        Date(entities.Collection.CreatedTmst);
      local.DashboardAuditData.CollAppliedToCd =
        entities.Collection.AppliedToCode;
      local.DashboardAuditData.CollectionType = entities.CollectionType.Code;

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
    // -- Store final Judicial District counts.
    // ------------------------------------------------------------------------------
    // -- Save Judicial District counts.
    for(local.Local1.Index = 0; local.Local1.Index < local.Local1.Count; ++
      local.Local1.Index)
    {
      if (!local.Local1.CheckSize())
      {
        break;
      }

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

    // @@@@@@  Here you need to calculate the X_type_percent_of_total for 
    // judicial districts.
    // ------------------------------------------------------------------------------
    // -- Calculate the Judicial District Percent Change.
    // ------------------------------------------------------------------------------
    foreach(var _ in ReadDashboardStagingPriority1())
    {
      MoveDashboardStagingPriority35(entities.DashboardStagingPriority35,
        local.DashboardStagingPriority35);

      if ((local.DashboardStagingPriority35.CtypeCollectionAmount ?? 0M) == 0)
      {
        local.DashboardStagingPriority35.CtypePercentOfTotal = 0;
      }
      else
      {
        local.DashboardStagingPriority35.CtypePercentOfTotal =
          (local.DashboardStagingPriority35.CtypeCollectionAmount ?? 0M) / (
            local.DashboardStagingPriority35.TotalCollectionAmount ?? 0M);
      }

      if ((local.DashboardStagingPriority35.FtypeCollectionAmount ?? 0M) == 0)
      {
        local.DashboardStagingPriority35.FtypePercentOfTotal = 0;
      }
      else
      {
        local.DashboardStagingPriority35.FtypePercentOfTotal =
          (local.DashboardStagingPriority35.FtypeCollectionAmount ?? 0M) / (
            local.DashboardStagingPriority35.TotalCollectionAmount ?? 0M);
      }

      if ((local.DashboardStagingPriority35.ItypeCollectionAmount ?? 0M) == 0)
      {
        local.DashboardStagingPriority35.ItypePercentOfTotal = 0;
      }
      else
      {
        local.DashboardStagingPriority35.ItypePercentOfTotal =
          (local.DashboardStagingPriority35.ItypeCollectionAmount ?? 0M) / (
            local.DashboardStagingPriority35.TotalCollectionAmount ?? 0M);
      }

      if ((local.DashboardStagingPriority35.StypeCollectionAmount ?? 0M) == 0)
      {
        local.DashboardStagingPriority35.StypePercentOfTotal = 0;
      }
      else
      {
        local.DashboardStagingPriority35.StypePercentOfTotal =
          (local.DashboardStagingPriority35.StypeCollectionAmount ?? 0M) / (
            local.DashboardStagingPriority35.TotalCollectionAmount ?? 0M);
      }

      if ((local.DashboardStagingPriority35.UtypeCollectionAmount ?? 0M) == 0)
      {
        local.DashboardStagingPriority35.UtypePercentOfTotal = 0;
      }
      else
      {
        local.DashboardStagingPriority35.UtypePercentOfTotal =
          (local.DashboardStagingPriority35.UtypeCollectionAmount ?? 0M) / (
            local.DashboardStagingPriority35.TotalCollectionAmount ?? 0M);
      }

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

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error creating/updating Dashboard_Staging_Priority_3_5.";
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
      Substring(import.ProgramCheckpointRestart.RestartInfo, 250, 1, 80) + "3-15    ";
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

  private static void MoveDashboardStagingPriority35(
    DashboardStagingPriority35 source, DashboardStagingPriority35 target)
  {
    target.ReportMonth = source.ReportMonth;
    target.ReportLevel = source.ReportLevel;
    target.ReportLevelId = source.ReportLevelId;
    target.AsOfDate = source.AsOfDate;
    target.StypeCollectionAmount = source.StypeCollectionAmount;
    target.StypePercentOfTotal = source.StypePercentOfTotal;
    target.FtypeCollectionAmount = source.FtypeCollectionAmount;
    target.FtypePercentOfTotal = source.FtypePercentOfTotal;
    target.ItypeCollectionAmount = source.ItypeCollectionAmount;
    target.ItypePercentOfTotal = source.ItypePercentOfTotal;
    target.UtypeCollectionAmount = source.UtypeCollectionAmount;
    target.UtypePercentOfTotal = source.UtypePercentOfTotal;
    target.CtypeCollectionAmount = source.CtypeCollectionAmount;
    target.CtypePercentOfTotal = source.CtypePercentOfTotal;
    target.TotalCollectionAmount = source.TotalCollectionAmount;
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
    useImport.ReportEndDate.Date = local.Temp.Date;

    context.Call(FnB734DetermineJdFromCase.Execute, useImport, useExport);

    MoveDashboardAuditData3(useExport.DashboardAuditData,
      local.DashboardAuditData);
  }

  private void UseFnB734DetermineJdFromOrder1()
  {
    var useImport = new FnB734DetermineJdFromOrder.Import();
    var useExport = new FnB734DetermineJdFromOrder.Export();

    useImport.PersistentCollection.Assign(entities.Collection);
    useImport.ReportEndDate.Date = import.ReportEndDate.Date;
    useImport.ReportStartDate.Date = import.ReportStartDate.Date;

    context.Call(FnB734DetermineJdFromOrder.Execute, useImport, useExport);

    local.Hold.Assign(useExport.DashboardAuditData);
  }

  private void UseFnB734DetermineJdFromOrder2()
  {
    var useImport = new FnB734DetermineJdFromOrder.Import();
    var useExport = new FnB734DetermineJdFromOrder.Export();

    useImport.PersistentCollection.Assign(entities.Collection);
    useImport.ReportEndDate.Date = import.ReportEndDate.Date;
    useImport.ReportStartDate.Date = import.ReportStartDate.Date;
    useImport.UseApSupportedOnly.Flag = local.UseApSupportedOnly.Flag;

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

  private void CreateDashboardStagingPriority35()
  {
    var reportMonth = local.Local1.Item.G.ReportMonth;
    var reportLevel = local.Local1.Item.G.ReportLevel;
    var reportLevelId = local.Local1.Item.G.ReportLevelId;
    var asOfDate = local.Local1.Item.G.AsOfDate;
    var param = 0M;
    var stypeCollectionAmount = local.Local1.Item.G.StypeCollectionAmount ?? 0M;
    var ftypeCollectionAmount = local.Local1.Item.G.FtypeCollectionAmount ?? 0M;
    var itypeCollectionAmount = local.Local1.Item.G.ItypeCollectionAmount ?? 0M;
    var utypeCollectionAmount = local.Local1.Item.G.UtypeCollectionAmount ?? 0M;
    var ctypeCollectionAmount = local.Local1.Item.G.CtypeCollectionAmount ?? 0M;
    var totalCollectionAmount = local.Local1.Item.G.TotalCollectionAmount ?? 0M;

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
        db.SetNullableDecimal(command, "STypeCollAmt", stypeCollectionAmount);
        db.SetNullableDecimal(command, "STypeCollPer", param);
        db.SetNullableDecimal(command, "FTypeCollAmt", ftypeCollectionAmount);
        db.SetNullableDecimal(command, "FTypeCollPer", param);
        db.SetNullableDecimal(command, "ITypeCollAmt", itypeCollectionAmount);
        db.SetNullableDecimal(command, "ITypeCollPer", param);
        db.SetNullableDecimal(command, "UTypeCollAmt", utypeCollectionAmount);
        db.SetNullableDecimal(command, "UTypeCollPer", param);
        db.SetNullableDecimal(command, "CTypeCollAmt", ctypeCollectionAmount);
        db.SetNullableDecimal(command, "CTypeCollPer", param);
        db.SetNullableDecimal(command, "totalCollAmt", totalCollectionAmount);
        db.SetNullableDecimal(command, "ordEstDaysAvg", param);
        db.SetNullableDecimal(command, "curSupPdYtdDen", param);
      });

    entities.DashboardStagingPriority35.ReportMonth = reportMonth;
    entities.DashboardStagingPriority35.ReportLevel = reportLevel;
    entities.DashboardStagingPriority35.ReportLevelId = reportLevelId;
    entities.DashboardStagingPriority35.AsOfDate = asOfDate;
    entities.DashboardStagingPriority35.StypeCollectionAmount =
      stypeCollectionAmount;
    entities.DashboardStagingPriority35.StypePercentOfTotal = param;
    entities.DashboardStagingPriority35.FtypeCollectionAmount =
      ftypeCollectionAmount;
    entities.DashboardStagingPriority35.FtypePercentOfTotal = param;
    entities.DashboardStagingPriority35.ItypeCollectionAmount =
      itypeCollectionAmount;
    entities.DashboardStagingPriority35.ItypePercentOfTotal = param;
    entities.DashboardStagingPriority35.UtypeCollectionAmount =
      utypeCollectionAmount;
    entities.DashboardStagingPriority35.UtypePercentOfTotal = param;
    entities.DashboardStagingPriority35.CtypeCollectionAmount =
      ctypeCollectionAmount;
    entities.DashboardStagingPriority35.CtypePercentOfTotal = param;
    entities.DashboardStagingPriority35.TotalCollectionAmount =
      totalCollectionAmount;
    entities.DashboardStagingPriority35.Populated = true;
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
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 2);
        entities.Collection.ConcurrentInd = db.GetString(reader, 3);
        entities.Collection.CrtType = db.GetInt32(reader, 4);
        entities.Collection.CstId = db.GetInt32(reader, 5);
        entities.Collection.CrvId = db.GetInt32(reader, 6);
        entities.Collection.CrdId = db.GetInt32(reader, 7);
        entities.Collection.ObgId = db.GetInt32(reader, 8);
        entities.Collection.CspNumber = db.GetString(reader, 9);
        entities.Ap.Number = db.GetString(reader, 9);
        entities.Collection.CpaType = db.GetString(reader, 10);
        entities.Collection.OtrId = db.GetInt32(reader, 11);
        entities.Collection.OtrType = db.GetString(reader, 12);
        entities.Collection.OtyId = db.GetInt32(reader, 13);
        entities.Collection.CollectionAdjustmentDt = db.GetDate(reader, 14);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 15);
        entities.Collection.Amount = db.GetDecimal(reader, 16);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 17);
        entities.ObligationType.Classification = db.GetString(reader, 18);
        entities.CollectionType.SequentialIdentifier = db.GetInt32(reader, 19);
        entities.CollectionType.Code = db.GetString(reader, 20);
        entities.Collection.Populated = true;
        entities.ObligationType.Populated = true;
        entities.Ap.Populated = true;
        entities.CollectionType.Populated = db.GetNullableInt32(reader, 19) != null
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
        entities.ObligationType.Populated = false;
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
        entities.DashboardStagingPriority35.ReportMonth =
          db.GetInt32(reader, 0);
        entities.DashboardStagingPriority35.ReportLevel =
          db.GetString(reader, 1);
        entities.DashboardStagingPriority35.ReportLevelId =
          db.GetString(reader, 2);
        entities.DashboardStagingPriority35.AsOfDate =
          db.GetNullableDate(reader, 3);
        entities.DashboardStagingPriority35.StypeCollectionAmount =
          db.GetNullableDecimal(reader, 4);
        entities.DashboardStagingPriority35.StypePercentOfTotal =
          db.GetNullableDecimal(reader, 5);
        entities.DashboardStagingPriority35.FtypeCollectionAmount =
          db.GetNullableDecimal(reader, 6);
        entities.DashboardStagingPriority35.FtypePercentOfTotal =
          db.GetNullableDecimal(reader, 7);
        entities.DashboardStagingPriority35.ItypeCollectionAmount =
          db.GetNullableDecimal(reader, 8);
        entities.DashboardStagingPriority35.ItypePercentOfTotal =
          db.GetNullableDecimal(reader, 9);
        entities.DashboardStagingPriority35.UtypeCollectionAmount =
          db.GetNullableDecimal(reader, 10);
        entities.DashboardStagingPriority35.UtypePercentOfTotal =
          db.GetNullableDecimal(reader, 11);
        entities.DashboardStagingPriority35.CtypeCollectionAmount =
          db.GetNullableDecimal(reader, 12);
        entities.DashboardStagingPriority35.CtypePercentOfTotal =
          db.GetNullableDecimal(reader, 13);
        entities.DashboardStagingPriority35.TotalCollectionAmount =
          db.GetNullableDecimal(reader, 14);
        entities.DashboardStagingPriority35.Populated = true;

        return true;
      },
      () =>
      {
        entities.DashboardStagingPriority35.Populated = false;
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
        entities.DashboardStagingPriority35.ReportMonth =
          db.GetInt32(reader, 0);
        entities.DashboardStagingPriority35.ReportLevel =
          db.GetString(reader, 1);
        entities.DashboardStagingPriority35.ReportLevelId =
          db.GetString(reader, 2);
        entities.DashboardStagingPriority35.AsOfDate =
          db.GetNullableDate(reader, 3);
        entities.DashboardStagingPriority35.StypeCollectionAmount =
          db.GetNullableDecimal(reader, 4);
        entities.DashboardStagingPriority35.StypePercentOfTotal =
          db.GetNullableDecimal(reader, 5);
        entities.DashboardStagingPriority35.FtypeCollectionAmount =
          db.GetNullableDecimal(reader, 6);
        entities.DashboardStagingPriority35.FtypePercentOfTotal =
          db.GetNullableDecimal(reader, 7);
        entities.DashboardStagingPriority35.ItypeCollectionAmount =
          db.GetNullableDecimal(reader, 8);
        entities.DashboardStagingPriority35.ItypePercentOfTotal =
          db.GetNullableDecimal(reader, 9);
        entities.DashboardStagingPriority35.UtypeCollectionAmount =
          db.GetNullableDecimal(reader, 10);
        entities.DashboardStagingPriority35.UtypePercentOfTotal =
          db.GetNullableDecimal(reader, 11);
        entities.DashboardStagingPriority35.CtypeCollectionAmount =
          db.GetNullableDecimal(reader, 12);
        entities.DashboardStagingPriority35.CtypePercentOfTotal =
          db.GetNullableDecimal(reader, 13);
        entities.DashboardStagingPriority35.TotalCollectionAmount =
          db.GetNullableDecimal(reader, 14);
        entities.DashboardStagingPriority35.Populated = true;

        return true;
      },
      () =>
      {
        entities.DashboardStagingPriority35.Populated = false;
      });
  }

  private bool ReadDashboardStagingPriority3()
  {
    entities.DashboardStagingPriority35.Populated = false;

    return Read("ReadDashboardStagingPriority3",
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
        entities.DashboardStagingPriority35.StypeCollectionAmount =
          db.GetNullableDecimal(reader, 4);
        entities.DashboardStagingPriority35.StypePercentOfTotal =
          db.GetNullableDecimal(reader, 5);
        entities.DashboardStagingPriority35.FtypeCollectionAmount =
          db.GetNullableDecimal(reader, 6);
        entities.DashboardStagingPriority35.FtypePercentOfTotal =
          db.GetNullableDecimal(reader, 7);
        entities.DashboardStagingPriority35.ItypeCollectionAmount =
          db.GetNullableDecimal(reader, 8);
        entities.DashboardStagingPriority35.ItypePercentOfTotal =
          db.GetNullableDecimal(reader, 9);
        entities.DashboardStagingPriority35.UtypeCollectionAmount =
          db.GetNullableDecimal(reader, 10);
        entities.DashboardStagingPriority35.UtypePercentOfTotal =
          db.GetNullableDecimal(reader, 11);
        entities.DashboardStagingPriority35.CtypeCollectionAmount =
          db.GetNullableDecimal(reader, 12);
        entities.DashboardStagingPriority35.CtypePercentOfTotal =
          db.GetNullableDecimal(reader, 13);
        entities.DashboardStagingPriority35.TotalCollectionAmount =
          db.GetNullableDecimal(reader, 14);
        entities.DashboardStagingPriority35.Populated = true;
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
        entities.Tribunal.Identifier = db.GetInt32(reader, 3);
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
    var asOfDate = local.Local1.Item.G.AsOfDate;
    var stypeCollectionAmount = local.Local1.Item.G.StypeCollectionAmount ?? 0M;
    var ftypeCollectionAmount = local.Local1.Item.G.FtypeCollectionAmount ?? 0M;
    var itypeCollectionAmount = local.Local1.Item.G.ItypeCollectionAmount ?? 0M;
    var utypeCollectionAmount = local.Local1.Item.G.UtypeCollectionAmount ?? 0M;
    var ctypeCollectionAmount = local.Local1.Item.G.CtypeCollectionAmount ?? 0M;
    var totalCollectionAmount = local.Local1.Item.G.TotalCollectionAmount ?? 0M;

    entities.DashboardStagingPriority35.Populated = false;
    Update("UpdateDashboardStagingPriority1",
      (db, command) =>
      {
        db.SetNullableDate(command, "asOfDate", asOfDate);
        db.SetNullableDecimal(command, "STypeCollAmt", stypeCollectionAmount);
        db.SetNullableDecimal(command, "FTypeCollAmt", ftypeCollectionAmount);
        db.SetNullableDecimal(command, "ITypeCollAmt", itypeCollectionAmount);
        db.SetNullableDecimal(command, "UTypeCollAmt", utypeCollectionAmount);
        db.SetNullableDecimal(command, "CTypeCollAmt", ctypeCollectionAmount);
        db.SetNullableDecimal(command, "totalCollAmt", totalCollectionAmount);
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
    entities.DashboardStagingPriority35.StypeCollectionAmount =
      stypeCollectionAmount;
    entities.DashboardStagingPriority35.FtypeCollectionAmount =
      ftypeCollectionAmount;
    entities.DashboardStagingPriority35.ItypeCollectionAmount =
      itypeCollectionAmount;
    entities.DashboardStagingPriority35.UtypeCollectionAmount =
      utypeCollectionAmount;
    entities.DashboardStagingPriority35.CtypeCollectionAmount =
      ctypeCollectionAmount;
    entities.DashboardStagingPriority35.TotalCollectionAmount =
      totalCollectionAmount;
    entities.DashboardStagingPriority35.Populated = true;
  }

  private void UpdateDashboardStagingPriority2()
  {
    var stypePercentOfTotal =
      local.DashboardStagingPriority35.StypePercentOfTotal ?? 0M;
    var ftypePercentOfTotal =
      local.DashboardStagingPriority35.FtypePercentOfTotal ?? 0M;
    var itypePercentOfTotal =
      local.DashboardStagingPriority35.ItypePercentOfTotal ?? 0M;
    var utypePercentOfTotal =
      local.DashboardStagingPriority35.UtypePercentOfTotal ?? 0M;
    var ctypePercentOfTotal =
      local.DashboardStagingPriority35.CtypePercentOfTotal ?? 0M;

    entities.DashboardStagingPriority35.Populated = false;
    Update("UpdateDashboardStagingPriority2",
      (db, command) =>
      {
        db.SetNullableDecimal(command, "STypeCollPer", stypePercentOfTotal);
        db.SetNullableDecimal(command, "FTypeCollPer", ftypePercentOfTotal);
        db.SetNullableDecimal(command, "ITypeCollPer", itypePercentOfTotal);
        db.SetNullableDecimal(command, "UTypeCollPer", utypePercentOfTotal);
        db.SetNullableDecimal(command, "CTypeCollPer", ctypePercentOfTotal);
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

    entities.DashboardStagingPriority35.StypePercentOfTotal =
      stypePercentOfTotal;
    entities.DashboardStagingPriority35.FtypePercentOfTotal =
      ftypePercentOfTotal;
    entities.DashboardStagingPriority35.ItypePercentOfTotal =
      itypePercentOfTotal;
    entities.DashboardStagingPriority35.UtypePercentOfTotal =
      utypePercentOfTotal;
    entities.DashboardStagingPriority35.CtypePercentOfTotal =
      ctypePercentOfTotal;
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
    private DateWorkArea? reportEndDate;
    private DateWorkArea? reportStartDate;
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
    /// A value of Hold.
    /// </summary>
    public DashboardAuditData Hold
    {
      get => hold ??= new();
      set => hold = value;
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
    /// A value of SubscrpitCount.
    /// </summary>
    public Common SubscrpitCount
    {
      get => subscrpitCount ??= new();
      set => subscrpitCount = value;
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
    /// A value of Checkpoint.
    /// </summary>
    public LegalAction Checkpoint
    {
      get => checkpoint ??= new();
      set => checkpoint = value;
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
    public DateWorkArea Temp
    {
      get => temp ??= new();
      set => temp = value;
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
    /// A value of Common.
    /// </summary>
    public Common Common
    {
      get => common ??= new();
      set => common = value;
    }

    /// <summary>
    /// A value of UseApSupportedOnly.
    /// </summary>
    public Common UseApSupportedOnly
    {
      get => useApSupportedOnly ??= new();
      set => useApSupportedOnly = value;
    }

    private DashboardAuditData? hold;
    private DashboardStagingPriority35? dashboardStagingPriority35;
    private DashboardAuditData? initialized;
    private ProgramCheckpointRestart? programCheckpointRestart;
    private EabFileHandling? eabFileHandling;
    private EabReportSend? eabReportSend;
    private Array<LocalGroup>? local1;
    private Common? subscrpitCount;
    private BatchTimestampWorkArea? batchTimestampWorkArea;
    private LegalAction? checkpoint;
    private Collection? restart;
    private Collection? prev;
    private Common? recordsReadSinceCommit;
    private DashboardAuditData? dashboardAuditData;
    private DateWorkArea? temp;
    private TextWorkArea? reportingAbbreviation;
    private Common? common;
    private Common? useApSupportedOnly;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of CseOrganization.
    /// </summary>
    public CseOrganization CseOrganization
    {
      get => cseOrganization ??= new();
      set => cseOrganization = value;
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
    /// A value of Collection.
    /// </summary>
    public Collection Collection
    {
      get => collection ??= new();
      set => collection = value;
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
    /// A value of Obligation.
    /// </summary>
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
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
    /// A value of LegalAction.
    /// </summary>
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
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
    /// A value of CaseAssignment.
    /// </summary>
    public CaseAssignment CaseAssignment
    {
      get => caseAssignment ??= new();
      set => caseAssignment = value;
    }

    /// <summary>
    /// A value of ObligationType.
    /// </summary>
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
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
    /// A value of Supported.
    /// </summary>
    public CsePersonAccount Supported
    {
      get => supported ??= new();
      set => supported = value;
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
    /// A value of Obligor.
    /// </summary>
    public CsePersonAccount Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
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

    private CseOrganization? cseOrganization;
    private DashboardStagingPriority35? dashboardStagingPriority35;
    private CollectionType? collectionType;
    private CashReceiptDetail? cashReceiptDetail;
    private Collection? collection;
    private LegalActionDetail? legalActionDetail;
    private Obligation? obligation;
    private ObligationTransaction? debt;
    private Fips? fips;
    private Tribunal? tribunal;
    private LegalAction? legalAction;
    private Case1? case1;
    private CaseRole? caseRole;
    private LegalActionCaseRole? legalActionCaseRole;
    private LaPersonLaCaseRole? laPersonLaCaseRole;
    private LegalActionPerson? legalActionPerson;
    private CaseAssignment? caseAssignment;
    private ObligationType? obligationType;
    private CsePerson? supp;
    private CsePersonAccount? supported;
    private CsePerson? ap;
    private CsePersonAccount? obligor;
    private CashReceipt? cashReceipt;
    private CashReceiptType? cashReceiptType;
  }
#endregion
}
