// Program: FN_B734_PRI_4_LEVEL_2, ID: 945237094, model: 746.
// Short name: SWE03726
using System;
using System.Collections.Generic;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B734_PRI_4_LEVEL_2.
/// </summary>
[Serializable]
[Program("SWE03726")]
public partial class FnB734Pri4Level2: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B734_PRI_4_LEVEL_2 program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB734Pri4Level2(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB734Pri4Level2.
  /// </summary>
  public FnB734Pri4Level2(IContext context, Import import, Export export):
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
    // 09/05/13  GVandy	CQ36547		Initial Development.  Priority 4 (Pyramid 
    // Report)
    // 			Segment E	
    // ---------------------------------------------------------------------------------------------------
    // -------------------------------------------------------------------------------------
    // Priority 4: Tier 2 - Case Obligation Status
    // -------------------------------------------------------------------------------------
    // Tier 2.1- Cases with Current Child Support Owed
    // This is a count of all cases with a current child support obligation (an 
    // accruing child support debt detail in the current month).
    // 	1) Debt details- accruing Obligation type- due date in reporting period.
    // 		a) Due date within report period and the due date >= to earliest CSE 
    // open date
    // 		b) Skip debts that are due before earliest case role date
    // 	2) Count debt amounts only for primary obligation in primary/secondary 
    // situation.
    // 	3) For joint/several situations, divide the obligation equally among 
    // obligors.
    // 	4) Look for original debt detail amount.
    // 	5) Ignore adjustments done to those debt details.
    // 	6) For each case reported in Tier 1, find all AP/CH combinations that 
    // overlapped
    // 	   during the report month.
    // 	7) Find any qualifying debts that were due on a date (due date) when the
    // AP/CH
    // 	   were active together on the case.  If a qualifying debt is found, the
    // case will be
    // 	   reported in this line.
    // Tier 2.2- Cases with Any Obligation Other than Current Child Support
    // This is a count of all cases that meet the federal definition of a case 
    // under order minus those cases counted in Tier 2.1.
    // Cases Under Order
    // 	1) Cases open on refresh date with a J or O class legal action and a 
    // legal detail with
    // 	   no end date and an obligation type of CRCH, CS, MS, AJ, MJ-NA, MJ-
    // NAI, ZCS, HIC or UM.
    // 	2) Must read for AP/CH combination (defined on LROL screen for non-
    // financial legal
    // 	   details and LOPS screen for financial legal details)
    // 	3) Case roles can be open or closed.
    // 	4) Case roles do not have to have been open during the reporting period.
    // 	5) Read for any J or O class legal action.
    // 	6) Do not read for open legal detail on financial obligations (financial
    // obligations
    // 	   must be obligated)
    // 	7) Do count if non-financial legal detail is ended, but the end date is 
    // in the
    // 	   future (after report period end).
    // 	8) Do count if obligation was created any time during or prior to the 
    // reporting
    // 	   period.
    // 	9) Read Legal Action Case Role for HIC & UM legal details
    // 	10) Count case if there was a cash or medical support order at one time,
    // but there is
    // 	   no money owed now, or the medical support is no longer in effect.
    // 	11) EP should not be considered for this line.
    // 	12) The legal action must have a filed date.
    // 	13) Subtract all cases counted in Tier 2.1
    // Tier 2.3- Cases with No Obligation
    // These are all open cases that did not meet the criteria to be counted in 
    // Tiers 2.1 or 2.2 (cases that do not meet the federal definition of a 
    // Case Under Order).
    // 	1) Count all cases in O status as of refresh date.
    // 	2) Subtract cases included in counts for Tiers 2.1 and 2.2.
    // -------------------------------------------------------------------------------------
    MoveProgramCheckpointRestart(import.ProgramCheckpointRestart,
      local.ProgramCheckpointRestart);

    // ------------------------------------------------------------------------------
    // -- Determine if we're restarting and set appropriate restart information.
    // ------------------------------------------------------------------------------
    if (AsChar(import.ProgramCheckpointRestart.RestartInd) == 'Y' && Equal
      (import.ProgramCheckpointRestart.RestartInfo, 81, 8, "4-02    "))
    {
      // -- Checkpoint Info
      // Positions   Value
      // ---------   
      // ------------------------------------
      //  001-080    General Checkpoint Info for PRAD
      //  081-088    Dashboard Priority
      //  089-089    Blank
      //  090-099    AP Person Number
      local.RestartAp.Number =
        Substring(import.ProgramCheckpointRestart.RestartInfo, 90, 10);
    }
    else
    {
      local.RestartAp.Number = "";
    }

    if (!ReadObligationType())
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error reading CS obligation type in fn_b734_pri_4_level_2.";
      UseCabErrorReport();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // ------------------------------------------------------------------------------
    // -- Tier 2.1- Cases with Current Child Support Owed
    // ------------------------------------------------------------------------------
    // -- Read each current child support debt due during the reporting period.
    foreach(var _ in ReadDebtDebtDetailCsePersonCsePerson())
    {
      if (!Equal(entities.ApCsePerson.Number, local.PrevAp.Number))
      {
        // ------------------------------------------------------------------------------
        // -- Checkpoint saving all the info needed for restarting.
        // ------------------------------------------------------------------------------
        if (local.RecordsReadSinceCommit.Count > (
          import.ProgramCheckpointRestart.ReadFrequencyCount ?? 0))
        {
          // -- Checkpoint Info
          // Positions   Value
          // ---------   
          // ------------------------------------
          //  001-080    General Checkpoint Info for PRAD
          //  081-088    Dashboard Priority
          //  089-089    Blank
          //  090-099    AP Person Number
          local.ProgramCheckpointRestart.RestartInd = "Y";
          local.ProgramCheckpointRestart.RestartInfo =
            Substring(import.ProgramCheckpointRestart.RestartInfo, 250, 1, 80) +
            "4-02    " + " " + String
            (local.PrevAp.Number, CsePerson.Number_MaxLength);
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

      local.PrevAp.Number = entities.ApCsePerson.Number;
      ++local.RecordsReadSinceCommit.Count;

      // -- Find case(s) to which the current child support should be 
      // attributed.
      foreach(var _1 in ReadCase())
      {
        // -- Set the current child support indicator for the case.
        if (ReadDashboardStagingPriority1())
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
                ExitState = "DASHBOARD_STAGING_PRI_4_NU";

                break;
              case ErrorCode.PermittedValueViolation:
                ExitState = "DASHBOARD_STAGING_PRI_4_PV";

                break;
              default:
                throw;
            }
          }

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail =
              "Error updating Dashboard_Staging_Priority_4 in FN_B734_Pri_4_Level_2.";
            UseCabErrorReport();
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }
        }
        else
        {
          // -- Continue. (Either the case number was not included as an open 
          // case or we
          //    already updated it to indicate current cs was due).
        }
      }
    }

    // ------------------------------------------------------------------------------
    // -- Tier 2.2- Cases with Any Obligation Other than Current Child Support
    // ------------------------------------------------------------------------------
    // -- Find all cases with current_cs_ind = spaces.  Then determine if the 
    // case is under order.
    foreach(var _ in ReadDashboardStagingPriority2())
    {
      ++local.RecordsReadSinceCommit.Count;
      local.Case1.Number = entities.DashboardStagingPriority4.CaseNumber;

      // -- Determine if case is under order.
      UseFnB734CaseUnderOrder();

      try
      {
        UpdateDashboardStagingPriority2();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "DASHBOARD_STAGING_PRI_4_NU";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "DASHBOARD_STAGING_PRI_4_PV";

            break;
          default:
            throw;
        }
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "Error updating Dashboard_Staging_Priority_4 in FN_B734_Pri_4_Level_2.";
        UseCabErrorReport();
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      // ------------------------------------------------------------------------------
      // -- Checkpoint saving all the info needed for restarting.
      // ------------------------------------------------------------------------------
      if (local.RecordsReadSinceCommit.Count > (
        import.ProgramCheckpointRestart.ReadFrequencyCount ?? 0))
      {
        // -- Checkpoint Info
        // Positions   Value
        // ---------   
        // ------------------------------------
        //  001-080    General Checkpoint Info for PRAD
        //  081-088    Dashboard Priority
        //  089-089    Blank
        //  090-099    AP Person Number
        local.ProgramCheckpointRestart.RestartInd = "Y";

        // -- The AP Person Number is deliberately set to "9999999999" so if we 
        // restart
        //    then the Tier 2.1 logic will not find any APs to process and 
        // processing will
        //    fall back into Tier 2.2.
        local.ProgramCheckpointRestart.RestartInfo =
          Substring(import.ProgramCheckpointRestart.RestartInfo, 250, 1, 80) + "4-02    " +
          " " + "9999999999";
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

    // ------------------------------------------------------------------------------
    // -- Tier 2.3- Cases with No Obligation
    // ------------------------------------------------------------------------------
    // -- No processing is required for Tier 2.3.
    //    A case with no obligation is indicated when 
    // dashboard_staging_priority_4
    //    current_cs_ind = 'N' and other_obg_ind = 'N".
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
      Substring(import.ProgramCheckpointRestart.RestartInfo, 250, 1, 80) + "4-03    ";
    UseUpdateCheckpointRstAndCommit();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "Error taking checkpoint.";
      UseCabErrorReport();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }
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

  private void UseFnB734CaseUnderOrder()
  {
    var useImport = new FnB734CaseUnderOrder.Import();
    var useExport = new FnB734CaseUnderOrder.Export();

    MoveDateWorkArea(import.ReportEndDate, useImport.ReportEndDate);
    useImport.Case1.Number = local.Case1.Number;

    context.Call(FnB734CaseUnderOrder.Execute, useImport, useExport);

    local.CaseUnderOrder.Flag = useExport.CaseUnderOrder.Flag;
  }

  private void UseUpdateCheckpointRstAndCommit()
  {
    var useImport = new UpdateCheckpointRstAndCommit.Import();
    var useExport = new UpdateCheckpointRstAndCommit.Export();

    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);

    context.Call(UpdateCheckpointRstAndCommit.Execute, useImport, useExport);
  }

  private IEnumerable<bool> ReadCase()
  {
    return ReadEach("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "cspNumber1", entities.ApCsePerson.Number);
        db.SetNullableDate(command, "startDate", entities.DebtDetail.DueDt);
        db.SetString(command, "cspNumber2", entities.ChCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.NoJurisdictionCd = db.GetNullableString(reader, 1);
        entities.Case1.Populated = true;

        return true;
      },
      () =>
      {
        entities.Case1.Populated = false;
      });
  }

  private bool ReadDashboardStagingPriority1()
  {
    entities.DashboardStagingPriority4.Populated = false;

    return Read("ReadDashboardStagingPriority1",
      (db, command) =>
      {
        db.SetInt32(command, "runNumber", import.DashboardAuditData.RunNumber);
        db.SetInt32(
          command, "reportMonth", import.DashboardAuditData.ReportMonth);
        db.SetString(command, "caseNumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.DashboardStagingPriority4.ReportMonth = db.GetInt32(reader, 0);
        entities.DashboardStagingPriority4.RunNumber = db.GetInt32(reader, 1);
        entities.DashboardStagingPriority4.CaseNumber = db.GetString(reader, 2);
        entities.DashboardStagingPriority4.AsOfDate =
          db.GetNullableDate(reader, 3);
        entities.DashboardStagingPriority4.CurrentCsInd =
          db.GetNullableString(reader, 4);
        entities.DashboardStagingPriority4.OtherObgInd =
          db.GetNullableString(reader, 5);
        entities.DashboardStagingPriority4.CsDueAmt =
          db.GetNullableDecimal(reader, 6);
        entities.DashboardStagingPriority4.CsCollectedAmt =
          db.GetNullableDecimal(reader, 7);
        entities.DashboardStagingPriority4.PayingCaseInd =
          db.GetNullableString(reader, 8);
        entities.DashboardStagingPriority4.PaternityEstInd =
          db.GetNullableString(reader, 9);
        entities.DashboardStagingPriority4.AddressVerInd =
          db.GetNullableString(reader, 10);
        entities.DashboardStagingPriority4.EmployerVerInd =
          db.GetNullableString(reader, 11);
        entities.DashboardStagingPriority4.Populated = true;
      });
  }

  private IEnumerable<bool> ReadDashboardStagingPriority2()
  {
    return ReadEachInSeparateTransaction("ReadDashboardStagingPriority2",
      (db, command) =>
      {
        db.SetInt32(command, "runNumber", import.DashboardAuditData.RunNumber);
        db.SetInt32(
          command, "reportMonth", import.DashboardAuditData.ReportMonth);
      },
      (db, reader) =>
      {
        entities.DashboardStagingPriority4.ReportMonth = db.GetInt32(reader, 0);
        entities.DashboardStagingPriority4.RunNumber = db.GetInt32(reader, 1);
        entities.DashboardStagingPriority4.CaseNumber = db.GetString(reader, 2);
        entities.DashboardStagingPriority4.AsOfDate =
          db.GetNullableDate(reader, 3);
        entities.DashboardStagingPriority4.CurrentCsInd =
          db.GetNullableString(reader, 4);
        entities.DashboardStagingPriority4.OtherObgInd =
          db.GetNullableString(reader, 5);
        entities.DashboardStagingPriority4.CsDueAmt =
          db.GetNullableDecimal(reader, 6);
        entities.DashboardStagingPriority4.CsCollectedAmt =
          db.GetNullableDecimal(reader, 7);
        entities.DashboardStagingPriority4.PayingCaseInd =
          db.GetNullableString(reader, 8);
        entities.DashboardStagingPriority4.PaternityEstInd =
          db.GetNullableString(reader, 9);
        entities.DashboardStagingPriority4.AddressVerInd =
          db.GetNullableString(reader, 10);
        entities.DashboardStagingPriority4.EmployerVerInd =
          db.GetNullableString(reader, 11);
        entities.DashboardStagingPriority4.Populated = true;

        return true;
      },
      () =>
      {
        entities.DashboardStagingPriority4.Populated = false;
      });
  }

  private IEnumerable<bool> ReadDebtDebtDetailCsePersonCsePerson()
  {
    return ReadEachInSeparateTransaction("ReadDebtDebtDetailCsePersonCsePerson",
      (db, command) =>
      {
        db.SetDate(command, "date1", import.ReportStartDate.Date);
        db.SetDate(command, "date2", import.ReportEndDate.Date);
        db.SetDateTime(command, "createdTmst", import.ReportEndDate.Timestamp);
        db.SetInt32(
          command, "dtyGeneratedId", entities.Cs.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", local.RestartAp.Number);
      },
      (db, reader) =>
      {
        entities.Debt.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.Debt.CspNumber = db.GetString(reader, 1);
        entities.DebtDetail.CspNumber = db.GetString(reader, 1);
        entities.ApCsePerson.Number = db.GetString(reader, 1);
        entities.Debt.CpaType = db.GetString(reader, 2);
        entities.DebtDetail.CpaType = db.GetString(reader, 2);
        entities.Debt.SystemGeneratedIdentifier = db.GetInt32(reader, 3);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.Debt.Type1 = db.GetString(reader, 4);
        entities.DebtDetail.OtrType = db.GetString(reader, 4);
        entities.Debt.CreatedTmst = db.GetDateTime(reader, 5);
        entities.ChCsePerson.Number = db.GetString(reader, 6);
        entities.Debt.OtyType = db.GetInt32(reader, 8);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 8);
        entities.DebtDetail.DueDt = db.GetDate(reader, 9);
        entities.DebtDetail.CreatedTmst = db.GetDateTime(reader, 10);

        if (Equal(entities.Debt.Type1, "DE"))
        {
          entities.Debt.CspSupNumber = db.GetNullableString(reader, 6);
          entities.Debt.CpaSupType = db.GetNullableString(reader, 7);
        }
        else
        {
          entities.Debt.CspSupNumber = null;
          entities.Debt.CpaSupType = null;
        }

        entities.Debt.Populated = true;
        entities.DebtDetail.Populated = true;
        entities.ApCsePerson.Populated = true;
        entities.ChCsePerson.Populated = true;
        CheckValid<ObligationTransaction>("Type1", entities.Debt.Type1);

        return true;
      },
      () =>
      {
        entities.ChCsePerson.Populated = false;
        entities.ApCsePerson.Populated = false;
        entities.DebtDetail.Populated = false;
        entities.Debt.Populated = false;
      });
  }

  private bool ReadObligationType()
  {
    entities.Cs.Populated = false;

    return Read("ReadObligationType",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDt", import.ProgramProcessingInfo.ProcessDate);
      },
      (db, reader) =>
      {
        entities.Cs.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Cs.Code = db.GetString(reader, 1);
        entities.Cs.EffectiveDt = db.GetDate(reader, 2);
        entities.Cs.DiscontinueDt = db.GetNullableDate(reader, 3);
        entities.Cs.Populated = true;
      });
  }

  private void UpdateDashboardStagingPriority1()
  {
    var currentCsInd = "Y";

    entities.DashboardStagingPriority4.Populated = false;
    Update("UpdateDashboardStagingPriority1",
      (db, command) =>
      {
        db.SetNullableString(command, "currentCsInd", currentCsInd);
        db.SetInt32(
          command, "reportMonth",
          entities.DashboardStagingPriority4.ReportMonth);
        db.SetInt32(
          command, "runNumber", entities.DashboardStagingPriority4.RunNumber);
        db.SetString(
          command, "caseNumber", entities.DashboardStagingPriority4.CaseNumber);
      });

    entities.DashboardStagingPriority4.CurrentCsInd = currentCsInd;
    entities.DashboardStagingPriority4.Populated = true;
  }

  private void UpdateDashboardStagingPriority2()
  {
    var currentCsInd = "N";
    var otherObgInd = local.CaseUnderOrder.Flag;

    entities.DashboardStagingPriority4.Populated = false;
    Update("UpdateDashboardStagingPriority2",
      (db, command) =>
      {
        db.SetNullableString(command, "currentCsInd", currentCsInd);
        db.SetNullableString(command, "otherObgInd", otherObgInd);
        db.SetInt32(
          command, "reportMonth",
          entities.DashboardStagingPriority4.ReportMonth);
        db.SetInt32(
          command, "runNumber", entities.DashboardStagingPriority4.RunNumber);
        db.SetString(
          command, "caseNumber", entities.DashboardStagingPriority4.CaseNumber);
      });

    entities.DashboardStagingPriority4.CurrentCsInd = currentCsInd;
    entities.DashboardStagingPriority4.OtherObgInd = otherObgInd;
    entities.DashboardStagingPriority4.Populated = true;
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
    /// A value of ProgramCheckpointRestart.
    /// </summary>
    public ProgramCheckpointRestart ProgramCheckpointRestart
    {
      get => programCheckpointRestart ??= new();
      set => programCheckpointRestart = value;
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

    private ProgramCheckpointRestart? programCheckpointRestart;
    private DateWorkArea? reportStartDate;
    private DateWorkArea? reportEndDate;
    private DashboardAuditData? dashboardAuditData;
    private ProgramProcessingInfo? programProcessingInfo;
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
    /// A value of Case1.
    /// </summary>
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of CaseUnderOrder.
    /// </summary>
    public Common CaseUnderOrder
    {
      get => caseUnderOrder ??= new();
      set => caseUnderOrder = value;
    }

    /// <summary>
    /// A value of RestartAp.
    /// </summary>
    public CsePerson RestartAp
    {
      get => restartAp ??= new();
      set => restartAp = value;
    }

    /// <summary>
    /// A value of PrevAp.
    /// </summary>
    public CsePerson PrevAp
    {
      get => prevAp ??= new();
      set => prevAp = value;
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
    /// A value of RecordsReadSinceCommit.
    /// </summary>
    public Common RecordsReadSinceCommit
    {
      get => recordsReadSinceCommit ??= new();
      set => recordsReadSinceCommit = value;
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

    private Case1? case1;
    private Common? caseUnderOrder;
    private CsePerson? restartAp;
    private CsePerson? prevAp;
    private ProgramCheckpointRestart? programCheckpointRestart;
    private Common? recordsReadSinceCommit;
    private EabFileHandling? eabFileHandling;
    private EabReportSend? eabReportSend;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ChCaseRole.
    /// </summary>
    public CaseRole ChCaseRole
    {
      get => chCaseRole ??= new();
      set => chCaseRole = value;
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
    /// A value of ChCsePerson.
    /// </summary>
    public CsePerson ChCsePerson
    {
      get => chCsePerson ??= new();
      set => chCsePerson = value;
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
    /// A value of Supported.
    /// </summary>
    public CsePersonAccount Supported
    {
      get => supported ??= new();
      set => supported = value;
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
    /// A value of DebtDetail.
    /// </summary>
    public DebtDetail DebtDetail
    {
      get => debtDetail ??= new();
      set => debtDetail = value;
    }

    /// <summary>
    /// A value of Cs.
    /// </summary>
    public ObligationType Cs
    {
      get => cs ??= new();
      set => cs = value;
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
    /// A value of DashboardStagingPriority4.
    /// </summary>
    public DashboardStagingPriority4 DashboardStagingPriority4
    {
      get => dashboardStagingPriority4 ??= new();
      set => dashboardStagingPriority4 = value;
    }

    /// <summary>
    /// A value of Case1.
    /// </summary>
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    private CaseRole? chCaseRole;
    private CaseRole? apCaseRole;
    private CsePerson? chCsePerson;
    private CsePerson? apCsePerson;
    private CsePersonAccount? supported;
    private CsePersonAccount? obligor;
    private DebtDetail? debtDetail;
    private ObligationType? cs;
    private Obligation? obligation;
    private ObligationTransaction? debt;
    private DashboardStagingPriority4? dashboardStagingPriority4;
    private Case1? case1;
  }
#endregion
}
