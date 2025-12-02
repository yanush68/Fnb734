// Program: FN_B734_PRI_5_WORKER_AND_TEAM, ID: 945148110, model: 746.
// Short name: SWE03703
using System;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_B734_PRI_5_WORKER_AND_TEAM.
/// </para>
/// <para>
/// PRIORITY 3- Key Outputs/Metrics
/// </para>
/// </summary>
[Serializable]
[Program("SWE03703")]
public partial class FnB734Pri5WorkerAndTeam: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B734_PRI_5_WORKER_AND_TEAM program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB734Pri5WorkerAndTeam(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB734Pri5WorkerAndTeam.
  /// </summary>
  public FnB734Pri5WorkerAndTeam(IContext context, Import import, Export export):
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
    // 03/20/13  DDupre	CQ36547		Initial Development.
    // 				
    // ---------------------------------------------------------------------------------------------------
    // -------------------------------------------------------------------------------------
    // PRIORITY 5- Key worker and team performance
    // -------------------------------------------------------------------------------------
    // Priority 5 is dedicated to capturing performacne measures at the 
    // caseworker/attorney level.
    // -------------------------------------------------------------------------------------
    // -------------------------------------------------------------------------------------
    // -------------------------------------------------------------------------------------
    // -------------------------------------------------------------------------------------
    // -------------------------------------------------------------------------------------
    // Priority 5-1: Caseload Counts
    // -------------------------------------------------------------------------------------
    if (!Lt("5-01", import.Restart.DashboardPriority) && !
      Lt("5-01", import.StartDashboardAuditData.DashboardPriority) && !
      Lt(import.End.DashboardPriority, "5-01"))
    {
      UseFnB734Priority51();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // -------------------------------------------------------------------------------------
    // Priority 5-3: New Orders Established by Attorney
    // -------------------------------------------------------------------------------------
    if (!Lt("5-03", import.Restart.DashboardPriority) && !
      Lt("5-03", import.StartDashboardAuditData.DashboardPriority) && !
      Lt(import.End.DashboardPriority, "5-03"))
    {
      UseFnB734Priority53();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // -------------------------------------------------------------------------------------
    // Priority 5-4: Paternities Established
    // -------------------------------------------------------------------------------------
    if (!Lt("5-04", import.Restart.DashboardPriority) && !
      Lt("5-04", import.StartDashboardAuditData.DashboardPriority) && !
      Lt(import.End.DashboardPriority, "5-04"))
    {
      UseFnB734Priority54();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // -------------------------------------------------------------------------------------
    // Priority 5-5: Cases Opened
    // -------------------------------------------------------------------------------------
    if (!Lt("5-05", import.Restart.DashboardPriority) && !
      Lt("5-05", import.StartDashboardAuditData.DashboardPriority) && !
      Lt(import.End.DashboardPriority, "5-05"))
    {
      UseFnB734Priority55();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // -------------------------------------------------------------------------------------
    // Priority 5-9: Modifications
    // -------------------------------------------------------------------------------------
    if (!Lt("5-09", import.Restart.DashboardPriority) && !
      Lt("5-09", import.StartDashboardAuditData.DashboardPriority) && !
      Lt(import.End.DashboardPriority, "5-09"))
    {
      UseFnB734Priority59();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // -------------------------------------------------------------------------------------
    // Priority 5-10: Income Withholdings Issued
    // -------------------------------------------------------------------------------------
    if (!Lt("5-10", import.Restart.DashboardPriority) && !
      Lt("5-10", import.StartDashboardAuditData.DashboardPriority) && !
      Lt(import.End.DashboardPriority, "5-10"))
    {
      UseFnB734Priority510();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // -------------------------------------------------------------------------------------
    // Priority 5-11: Contempt Motion Filings
    // -------------------------------------------------------------------------------------
    if (!Lt("5-11", import.Restart.DashboardPriority) && !
      Lt("5-11", import.StartDashboardAuditData.DashboardPriority) && !
      Lt(import.End.DashboardPriority, "5-11"))
    {
      UseFnB734Priority511();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // -------------------------------------------------------------------------------------
    // Priority 5-12: Contempt Order Filings
    // -------------------------------------------------------------------------------------
    if (!Lt("5-12", import.Restart.DashboardPriority) && !
      Lt("5-12", import.StartDashboardAuditData.DashboardPriority) && !
      Lt(import.End.DashboardPriority, "5-12"))
    {
      UseFnB734Priority512();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // -------------------------------------------------------------------------------------
    // Priority 5-13: Collections by Type
    // -------------------------------------------------------------------------------------
    if (!Lt("5-13", import.Restart.DashboardPriority) && !
      Lt("5-13", import.StartDashboardAuditData.DashboardPriority) && !
      Lt(import.End.DashboardPriority, "5-13"))
    {
      UseFnB734Priority513();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // -------------------------------------------------------------------------------------
    // Priority 5-15: Federal Timeframes- Days from referral to order 
    // establishment by referral Attorney
    // -------------------------------------------------------------------------------------
    if (!Lt("5-15", import.Restart.DashboardPriority) && !
      Lt("5-15", import.StartDashboardAuditData.DashboardPriority) && !
      Lt(import.End.DashboardPriority, "5-15"))
    {
      UseFnB734Priority515();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // -------------------------------------------------------------------------------------
    // Priority 5-16 Days From Locate(?) to Service of Process
    // -------------------------------------------------------------------------------------
    if (!Lt("5-16", import.Restart.DashboardPriority) && !
      Lt("5-16", import.StartDashboardAuditData.DashboardPriority) && !
      Lt(import.End.DashboardPriority, "5-16"))
    {
      UseFnB734Priority516();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // -------------------------------------------------------------------------------------
    // Priority 5-17: Aging Report of Unprocessed Legal Referrals by Attorney
    // -------------------------------------------------------------------------------------
    if (!Lt("5-17", import.Restart.DashboardPriority) && !
      Lt("5-17", import.StartDashboardAuditData.DashboardPriority) && !
      Lt(import.End.DashboardPriority, "5-17"))
    {
      UseFnB734Priority517();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // -------------------------------------------------------------------------------------
    // Priority 5-18: Federal Timeframes- Days from IWO to IWO payment
    // -------------------------------------------------------------------------------------
    if (!Lt("5-18", import.Restart.DashboardPriority) && !
      Lt("5-18", import.StartDashboardAuditData.DashboardPriority) && !
      Lt(import.End.DashboardPriority, "5-18"))
    {
      UseFnB734Priority518();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // -------------------------------------------------------------------------------------
    // Priority 5-19: NCP Locates by Address
    // -------------------------------------------------------------------------------------
    if (!Lt("5-19", import.Restart.DashboardPriority) && !
      Lt("5-19", import.StartDashboardAuditData.DashboardPriority) && !
      Lt(import.End.DashboardPriority, "5-19"))
    {
      UseFnB734Priority519();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // -------------------------------------------------------------------------------------
    // Priority 5-20: NCP Locates by Employer
    // -------------------------------------------------------------------------------------
    if (!Lt("5-20", import.Restart.DashboardPriority) && !
      Lt("5-20", import.StartDashboardAuditData.DashboardPriority) && !
      Lt(import.End.DashboardPriority, "5-20"))
    {
      UseFnB734Priority520();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // -------------------------------------------------------------------------------------
    // Priority 5-21: Referrals to Legal for Establishment
    // -------------------------------------------------------------------------------------
    if (!Lt("5-21", import.Restart.DashboardPriority) && !
      Lt("5-21", import.StartDashboardAuditData.DashboardPriority) && !
      Lt(import.End.DashboardPriority, "5-21"))
    {
      UseFnB734Priority521();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // -------------------------------------------------------------------------------------
    // Priority 5-22: Referrals to Legal for Enforcement
    // -------------------------------------------------------------------------------------
    if (!Lt("5-22", import.Restart.DashboardPriority) && !
      Lt("5-22", import.StartDashboardAuditData.DashboardPriority) && !
      Lt(import.End.DashboardPriority, "5-22"))
    {
      UseFnB734Priority522();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // -------------------------------------------------------------------------------------
    // Priority 5-23: Case Closure
    // -------------------------------------------------------------------------------------
    if (!Lt("5-23", import.Restart.DashboardPriority) && !
      Lt("5-23", import.StartDashboardAuditData.DashboardPriority) && !
      Lt(import.End.DashboardPriority, "5-23"))
    {
      UseFnB734Priority523();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // -------------------------------------------------------------------------------------
    // Priority 5-24: Case Reviews
    // -------------------------------------------------------------------------------------
    if (!Lt("5-24", import.Restart.DashboardPriority) && !
      Lt("5-24", import.StartDashboardAuditData.DashboardPriority) && !
      Lt(import.End.DashboardPriority, "5-24"))
    {
      UseFnB734Priority524();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // -------------------------------------------------------------------------------------
    // Priority 5-25: Contempt Motion Filings by worker
    // -------------------------------------------------------------------------------------
    if (!Lt("5-25", import.Restart.DashboardPriority) && !
      Lt("5-25", import.StartDashboardAuditData.DashboardPriority) && !
      Lt(import.End.DashboardPriority, "5-25"))
    {
      UseFnB734Priority525();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // -------------------------------------------------------------------------------------
    // Priority 5-26: Contempt Order Filings by worker
    // -------------------------------------------------------------------------------------
    if (!Lt("5-26", import.Restart.DashboardPriority) && !
      Lt("5-26", import.StartDashboardAuditData.DashboardPriority) && !
      Lt(import.End.DashboardPriority, "5-26"))
    {
      UseFnB734Priority526();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // -------------------------------------------------------------------------------------
    // Priority 5-27: Petitions filings by worker
    // -------------------------------------------------------------------------------------
    if (!Lt("5-27", import.Restart.DashboardPriority) && !
      Lt("5-27", import.StartDashboardAuditData.DashboardPriority) && !
      Lt(import.End.DashboardPriority, "5-27"))
    {
      UseFnB734Priority527();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
    }
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

  private void UseFnB734Priority51()
  {
    var useImport = new FnB734Priority51.Import();
    var useExport = new FnB734Priority51.Export();

    useImport.ProgramCheckpointRestart.Assign(import.ProgramCheckpointRestart);
    useImport.ReportStartDate.Date = import.StartDateWorkArea.Date;
    MoveDateWorkArea(import.PeriodEnd, useImport.ReportEndDate);
    useImport.ProgramProcessingInfo.ProcessDate =
      import.ProgramProcessingInfo.ProcessDate;
    MoveDashboardAuditData(import.DashboardAuditData,
      useImport.DashboardAuditData);
    useImport.AuditFlag.Flag = import.AuditFlag.Flag;

    context.Call(FnB734Priority51.Execute, useImport, useExport);
  }

  private void UseFnB734Priority510()
  {
    var useImport = new FnB734Priority510.Import();
    var useExport = new FnB734Priority510.Export();

    MoveDashboardAuditData(import.DashboardAuditData,
      useImport.DashboardAuditData);
    useImport.ProgramProcessingInfo.ProcessDate =
      import.ProgramProcessingInfo.ProcessDate;
    MoveDateWorkArea(import.PeriodEnd, useImport.ReportEndDate);
    MoveDateWorkArea(import.StartDateWorkArea, useImport.ReportStartDate);
    useImport.ProgramCheckpointRestart.Assign(import.ProgramCheckpointRestart);
    useImport.ScriptCount.Count = import.ScriptCount.Count;
    useImport.AuditFlag.Flag = import.AuditFlag.Flag;

    context.Call(FnB734Priority510.Execute, useImport, useExport);
  }

  private void UseFnB734Priority511()
  {
    var useImport = new FnB734Priority511.Import();
    var useExport = new FnB734Priority511.Export();

    MoveDashboardAuditData(import.DashboardAuditData,
      useImport.DashboardAuditData);
    useImport.ProgramProcessingInfo.ProcessDate =
      import.ProgramProcessingInfo.ProcessDate;
    MoveDateWorkArea(import.PeriodEnd, useImport.ReportEndDate);
    MoveDateWorkArea(import.StartDateWorkArea, useImport.ReportStartDate);
    useImport.ProgramCheckpointRestart.Assign(import.ProgramCheckpointRestart);
    useImport.ScriptCount.Count = import.ScriptCount.Count;
    useImport.AuditFlag.Flag = import.AuditFlag.Flag;

    context.Call(FnB734Priority511.Execute, useImport, useExport);
  }

  private void UseFnB734Priority512()
  {
    var useImport = new FnB734Priority512.Import();
    var useExport = new FnB734Priority512.Export();

    MoveDashboardAuditData(import.DashboardAuditData,
      useImport.DashboardAuditData);
    useImport.ProgramProcessingInfo.ProcessDate =
      import.ProgramProcessingInfo.ProcessDate;
    MoveDateWorkArea(import.PeriodEnd, useImport.ReportEndDate);
    MoveDateWorkArea(import.StartDateWorkArea, useImport.ReportStartDate);
    useImport.ProgramCheckpointRestart.Assign(import.ProgramCheckpointRestart);
    useImport.ScriptCount.Count = import.ScriptCount.Count;
    useImport.AuditFlag.Flag = import.AuditFlag.Flag;

    context.Call(FnB734Priority512.Execute, useImport, useExport);
  }

  private void UseFnB734Priority513()
  {
    var useImport = new FnB734Priority513.Import();
    var useExport = new FnB734Priority513.Export();

    MoveDashboardAuditData(import.DashboardAuditData,
      useImport.DashboardAuditData);
    useImport.ProgramCheckpointRestart.Assign(import.ProgramCheckpointRestart);
    useImport.ProgramProcessingInfo.ProcessDate =
      import.ProgramProcessingInfo.ProcessDate;
    MoveDateWorkArea(import.PeriodEnd, useImport.ReportEndDate);
    MoveDateWorkArea(import.StartDateWorkArea, useImport.ReportStartDate);
    useImport.AuditFlag.Flag = import.AuditFlag.Flag;

    context.Call(FnB734Priority513.Execute, useImport, useExport);
  }

  private void UseFnB734Priority515()
  {
    var useImport = new FnB734Priority515.Import();
    var useExport = new FnB734Priority515.Export();

    MoveDashboardAuditData(import.DashboardAuditData,
      useImport.DashboardAuditData);
    useImport.ProgramProcessingInfo.ProcessDate =
      import.ProgramProcessingInfo.ProcessDate;
    MoveDateWorkArea(import.PeriodEnd, useImport.ReportEndDate);
    MoveDateWorkArea(import.StartDateWorkArea, useImport.ReportStartDate);
    useImport.ProgramCheckpointRestart.Assign(import.ProgramCheckpointRestart);
    useImport.ScriptCount.Count = import.ScriptCount.Count;
    useImport.AuditFlag.Flag = import.AuditFlag.Flag;

    context.Call(FnB734Priority515.Execute, useImport, useExport);
  }

  private void UseFnB734Priority516()
  {
    var useImport = new FnB734Priority516.Import();
    var useExport = new FnB734Priority516.Export();

    MoveDashboardAuditData(import.DashboardAuditData,
      useImport.DashboardAuditData);
    useImport.ProgramProcessingInfo.ProcessDate =
      import.ProgramProcessingInfo.ProcessDate;
    MoveDateWorkArea(import.PeriodEnd, useImport.ReportEndDate);
    MoveDateWorkArea(import.StartDateWorkArea, useImport.ReportStartDate);
    useImport.ProgramCheckpointRestart.Assign(import.ProgramCheckpointRestart);
    useImport.ScriptCount.Count = import.ScriptCount.Count;
    useImport.AuditFlag.Flag = import.AuditFlag.Flag;

    context.Call(FnB734Priority516.Execute, useImport, useExport);
  }

  private void UseFnB734Priority517()
  {
    var useImport = new FnB734Priority517.Import();
    var useExport = new FnB734Priority517.Export();

    MoveDashboardAuditData(import.DashboardAuditData,
      useImport.DashboardAuditData);
    useImport.ReportStartDate.Date = import.StartDateWorkArea.Date;
    MoveDateWorkArea(import.PeriodEnd, useImport.ReportEndDate);
    useImport.ProgramProcessingInfo.ProcessDate =
      import.ProgramProcessingInfo.ProcessDate;
    useImport.ProgramCheckpointRestart.Assign(import.ProgramCheckpointRestart);
    useImport.AuditFlag.Flag = import.AuditFlag.Flag;

    context.Call(FnB734Priority517.Execute, useImport, useExport);
  }

  private void UseFnB734Priority518()
  {
    var useImport = new FnB734Priority518.Import();
    var useExport = new FnB734Priority518.Export();

    MoveDashboardAuditData(import.DashboardAuditData,
      useImport.DashboardAuditData);
    useImport.ProgramProcessingInfo.ProcessDate =
      import.ProgramProcessingInfo.ProcessDate;
    MoveDateWorkArea(import.PeriodEnd, useImport.ReportEndDate);
    MoveDateWorkArea(import.StartDateWorkArea, useImport.ReportStartDate);
    useImport.ProgramCheckpointRestart.Assign(import.ProgramCheckpointRestart);
    useImport.AuditFlag.Flag = import.AuditFlag.Flag;

    context.Call(FnB734Priority518.Execute, useImport, useExport);
  }

  private void UseFnB734Priority519()
  {
    var useImport = new FnB734Priority519.Import();
    var useExport = new FnB734Priority519.Export();

    MoveDashboardAuditData(import.DashboardAuditData,
      useImport.DashboardAuditData);
    useImport.ReportStartDate.Date = import.StartDateWorkArea.Date;
    MoveDateWorkArea(import.PeriodEnd, useImport.ReportEndDate);
    useImport.ProgramProcessingInfo.ProcessDate =
      import.ProgramProcessingInfo.ProcessDate;
    useImport.ProgramCheckpointRestart.Assign(import.ProgramCheckpointRestart);
    useImport.AuditFlag.Flag = import.AuditFlag.Flag;

    context.Call(FnB734Priority519.Execute, useImport, useExport);
  }

  private void UseFnB734Priority520()
  {
    var useImport = new FnB734Priority520.Import();
    var useExport = new FnB734Priority520.Export();

    MoveDashboardAuditData(import.DashboardAuditData,
      useImport.DashboardAuditData);
    useImport.ReportStartDate.Date = import.StartDateWorkArea.Date;
    MoveDateWorkArea(import.PeriodEnd, useImport.ReportEndDate);
    useImport.ProgramProcessingInfo.ProcessDate =
      import.ProgramProcessingInfo.ProcessDate;
    useImport.ProgramCheckpointRestart.Assign(import.ProgramCheckpointRestart);
    useImport.AuditFlag.Flag = import.AuditFlag.Flag;

    context.Call(FnB734Priority520.Execute, useImport, useExport);
  }

  private void UseFnB734Priority521()
  {
    var useImport = new FnB734Priority521.Import();
    var useExport = new FnB734Priority521.Export();

    MoveDashboardAuditData(import.DashboardAuditData,
      useImport.DashboardAuditData);
    MoveDateWorkArea(import.StartDateWorkArea, useImport.ReportStartDate);
    MoveDateWorkArea(import.PeriodEnd, useImport.ReportEndDate);
    useImport.ProgramProcessingInfo.ProcessDate =
      import.ProgramProcessingInfo.ProcessDate;
    useImport.ProgramCheckpointRestart.Assign(import.ProgramCheckpointRestart);
    useImport.AuditFlag.Flag = import.AuditFlag.Flag;

    context.Call(FnB734Priority521.Execute, useImport, useExport);
  }

  private void UseFnB734Priority522()
  {
    var useImport = new FnB734Priority522.Import();
    var useExport = new FnB734Priority522.Export();

    MoveDashboardAuditData(import.DashboardAuditData,
      useImport.DashboardAuditData);
    MoveDateWorkArea(import.StartDateWorkArea, useImport.ReportStartDate);
    MoveDateWorkArea(import.PeriodEnd, useImport.ReportEndDate);
    useImport.ProgramProcessingInfo.ProcessDate =
      import.ProgramProcessingInfo.ProcessDate;
    useImport.ProgramCheckpointRestart.Assign(import.ProgramCheckpointRestart);
    useImport.AuditFlag.Flag = import.AuditFlag.Flag;

    context.Call(FnB734Priority522.Execute, useImport, useExport);
  }

  private void UseFnB734Priority523()
  {
    var useImport = new FnB734Priority523.Import();
    var useExport = new FnB734Priority523.Export();

    MoveDashboardAuditData(import.DashboardAuditData,
      useImport.DashboardAuditData);
    useImport.ProgramCheckpointRestart.Assign(import.ProgramCheckpointRestart);
    useImport.ProgramProcessingInfo.ProcessDate =
      import.ProgramProcessingInfo.ProcessDate;
    MoveDateWorkArea(import.PeriodEnd, useImport.ReportEndDate);
    MoveDateWorkArea(import.StartDateWorkArea, useImport.ReportStartDate);
    useImport.AuditFlag.Flag = import.AuditFlag.Flag;

    context.Call(FnB734Priority523.Execute, useImport, useExport);
  }

  private void UseFnB734Priority524()
  {
    var useImport = new FnB734Priority524.Import();
    var useExport = new FnB734Priority524.Export();

    MoveDashboardAuditData(import.DashboardAuditData,
      useImport.DashboardAuditData);
    useImport.ReportStartDate.Date = import.StartDateWorkArea.Date;
    MoveDateWorkArea(import.PeriodEnd, useImport.ReportEndDate);
    useImport.ProgramProcessingInfo.ProcessDate =
      import.ProgramProcessingInfo.ProcessDate;
    useImport.ProgramCheckpointRestart.Assign(import.ProgramCheckpointRestart);
    useImport.AuditFlag.Flag = import.AuditFlag.Flag;

    context.Call(FnB734Priority524.Execute, useImport, useExport);
  }

  private void UseFnB734Priority525()
  {
    var useImport = new FnB734Priority525.Import();
    var useExport = new FnB734Priority525.Export();

    MoveDashboardAuditData(import.DashboardAuditData,
      useImport.DashboardAuditData);
    useImport.ProgramProcessingInfo.ProcessDate =
      import.ProgramProcessingInfo.ProcessDate;
    MoveDateWorkArea(import.PeriodEnd, useImport.ReportEndDate);
    MoveDateWorkArea(import.StartDateWorkArea, useImport.ReportStartDate);
    useImport.ProgramCheckpointRestart.Assign(import.ProgramCheckpointRestart);
    useImport.ScriptCount.Count = import.ScriptCount.Count;
    useImport.AuditFlag.Flag = import.AuditFlag.Flag;

    context.Call(FnB734Priority525.Execute, useImport, useExport);
  }

  private void UseFnB734Priority526()
  {
    var useImport = new FnB734Priority526.Import();
    var useExport = new FnB734Priority526.Export();

    MoveDashboardAuditData(import.DashboardAuditData,
      useImport.DashboardAuditData);
    useImport.ProgramProcessingInfo.ProcessDate =
      import.ProgramProcessingInfo.ProcessDate;
    MoveDateWorkArea(import.PeriodEnd, useImport.ReportEndDate);
    MoveDateWorkArea(import.StartDateWorkArea, useImport.ReportStartDate);
    useImport.ProgramCheckpointRestart.Assign(import.ProgramCheckpointRestart);
    useImport.ScriptCount.Count = import.ScriptCount.Count;
    useImport.AuditFlag.Flag = import.AuditFlag.Flag;

    context.Call(FnB734Priority526.Execute, useImport, useExport);
  }

  private void UseFnB734Priority527()
  {
    var useImport = new FnB734Priority527.Import();
    var useExport = new FnB734Priority527.Export();

    MoveDashboardAuditData(import.DashboardAuditData,
      useImport.DashboardAuditData);
    useImport.ProgramProcessingInfo.ProcessDate =
      import.ProgramProcessingInfo.ProcessDate;
    MoveDateWorkArea(import.PeriodEnd, useImport.ReportEndDate);
    MoveDateWorkArea(import.StartDateWorkArea, useImport.ReportStartDate);
    useImport.ProgramCheckpointRestart.Assign(import.ProgramCheckpointRestart);
    useImport.ScriptCount.Count = import.ScriptCount.Count;
    useImport.AuditFlag.Flag = import.AuditFlag.Flag;

    context.Call(FnB734Priority527.Execute, useImport, useExport);
  }

  private void UseFnB734Priority53()
  {
    var useImport = new FnB734Priority53.Import();
    var useExport = new FnB734Priority53.Export();

    useImport.ProgramProcessingInfo.ProcessDate =
      import.ProgramProcessingInfo.ProcessDate;
    MoveDashboardAuditData(import.DashboardAuditData,
      useImport.DashboardAuditData);
    MoveDateWorkArea(import.PeriodEnd, useImport.ReportEndDate);
    MoveDateWorkArea(import.StartDateWorkArea, useImport.ReportStartDate);
    useImport.ProgramCheckpointRestart.Assign(import.ProgramCheckpointRestart);
    useImport.ScriptCount.Count = import.ScriptCount.Count;
    useImport.AuditFlag.Flag = import.AuditFlag.Flag;

    context.Call(FnB734Priority53.Execute, useImport, useExport);
  }

  private void UseFnB734Priority54()
  {
    var useImport = new FnB734Priority54.Import();
    var useExport = new FnB734Priority54.Export();

    MoveDateWorkArea(import.PeriodEnd, useImport.ReportEndDate);
    MoveDateWorkArea(import.StartDateWorkArea, useImport.ReportStartDate);
    useImport.ProgramCheckpointRestart.Assign(import.ProgramCheckpointRestart);
    useImport.ProgramProcessingInfo.ProcessDate =
      import.ProgramProcessingInfo.ProcessDate;
    MoveDashboardAuditData(import.DashboardAuditData,
      useImport.DashboardAuditData);
    useImport.ScriptCount.Count = import.ScriptCount.Count;
    useImport.AuditFlag.Flag = import.AuditFlag.Flag;

    context.Call(FnB734Priority54.Execute, useImport, useExport);
  }

  private void UseFnB734Priority55()
  {
    var useImport = new FnB734Priority55.Import();
    var useExport = new FnB734Priority55.Export();

    MoveDashboardAuditData(import.DashboardAuditData,
      useImport.DashboardAuditData);
    useImport.ProgramCheckpointRestart.Assign(import.ProgramCheckpointRestart);
    useImport.ProgramProcessingInfo.ProcessDate =
      import.ProgramProcessingInfo.ProcessDate;
    MoveDateWorkArea(import.PeriodEnd, useImport.ReportEndDate);
    MoveDateWorkArea(import.StartDateWorkArea, useImport.ReportStartDate);
    useImport.AuditFlag.Flag = import.AuditFlag.Flag;

    context.Call(FnB734Priority55.Execute, useImport, useExport);
  }

  private void UseFnB734Priority59()
  {
    var useImport = new FnB734Priority59.Import();
    var useExport = new FnB734Priority59.Export();

    MoveDashboardAuditData(import.DashboardAuditData,
      useImport.DashboardAuditData);
    useImport.ReportStartDate.Date = import.StartDateWorkArea.Date;
    MoveDateWorkArea(import.PeriodEnd, useImport.ReportEndDate);
    useImport.ProgramProcessingInfo.ProcessDate =
      import.ProgramProcessingInfo.ProcessDate;
    useImport.ProgramCheckpointRestart.Assign(import.ProgramCheckpointRestart);
    useImport.AuditFlag.Flag = import.AuditFlag.Flag;

    context.Call(FnB734Priority59.Execute, useImport, useExport);
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
#endregion

#region Structures
  /// <summary>
  /// This class defines import view.
  /// </summary>
  [Serializable]
  public class Import
  {
    /// <summary>
    /// A value of Restart.
    /// </summary>
    public DashboardAuditData Restart
    {
      get => restart ??= new();
      set => restart = value;
    }

    /// <summary>
    /// A value of StartDashboardAuditData.
    /// </summary>
    public DashboardAuditData StartDashboardAuditData
    {
      get => startDashboardAuditData ??= new();
      set => startDashboardAuditData = value;
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
    /// A value of PeriodEnd.
    /// </summary>
    public DateWorkArea PeriodEnd
    {
      get => periodEnd ??= new();
      set => periodEnd = value;
    }

    /// <summary>
    /// A value of StartDateWorkArea.
    /// </summary>
    public DateWorkArea StartDateWorkArea
    {
      get => startDateWorkArea ??= new();
      set => startDateWorkArea = value;
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

    private DashboardAuditData? restart;
    private DashboardAuditData? startDashboardAuditData;
    private DashboardAuditData? end;
    private ProgramProcessingInfo? programProcessingInfo;
    private ProgramCheckpointRestart? programCheckpointRestart;
    private DateWorkArea? periodEnd;
    private DateWorkArea? startDateWorkArea;
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
#endregion
}
