// Program: FN_B734_PRI_3_KEY_OUTPUT_METRICS, ID: 945148091, model: 746.
// Short name: SWE03676
using System;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_B734_PRI_3_KEY_OUTPUT_METRICS.
/// </para>
/// <para>
/// PRIORITY 3- Key Outputs/Metrics
/// </para>
/// </summary>
[Serializable]
[Program("SWE03676")]
public partial class FnB734Pri3KeyOutputMetrics: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B734_PRI_3_KEY_OUTPUT_METRICS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB734Pri3KeyOutputMetrics(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB734Pri3KeyOutputMetrics.
  /// </summary>
  public FnB734Pri3KeyOutputMetrics(IContext context, Import import,
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
    // 03/20/13  DDupre	CQ36547		Initial Development.
    // 				
    // ---------------------------------------------------------------------------------------------------
    // -------------------------------------------------------------------------------------
    // PRIORITY 3- Key Outputs/Metrics
    // -------------------------------------------------------------------------------------
    // Priority 3 is dedicated to reporting individual caseworker/attorney key 
    // outputs for performance reviews.
    // -------------------------------------------------------------------------------------
    // -------------------------------------------------------------------------------------
    // -------------------------------------------------------------------------------------
    // -------------------------------------------------------------------------------------
    // -------------------------------------------------------------------------------------
    // Priority 3-1: Caseload Counts
    // -------------------------------------------------------------------------------------
    if (!Lt("3-01", import.Restart.DashboardPriority) && !
      Lt("3-01", import.StartDashboardAuditData.DashboardPriority) && !
      Lt(import.End.DashboardPriority, "3-01"))
    {
      UseFnB734Priority31();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // -------------------------------------------------------------------------------------
    // Priority 3-2: Staffing
    // -------------------------------------------------------------------------------------
    if (!Lt("3-02", import.Restart.DashboardPriority) && !
      Lt("3-02", import.StartDashboardAuditData.DashboardPriority) && !
      Lt(import.End.DashboardPriority, "3-02"))
    {
      UseFnB734Priority32();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // -------------------------------------------------------------------------------------
    // Priority 3-3: New Orders Established
    // -------------------------------------------------------------------------------------
    if (!Lt("3-03", import.Restart.DashboardPriority) && !
      Lt("3-03", import.StartDashboardAuditData.DashboardPriority) && !
      Lt(import.End.DashboardPriority, "3-03"))
    {
      UseFnB734Priority33();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // -------------------------------------------------------------------------------------
    // Priority 3-4: Paternities Established
    // -------------------------------------------------------------------------------------
    if (!Lt("3-04", import.Restart.DashboardPriority) && !
      Lt("3-04", import.StartDashboardAuditData.DashboardPriority) && !
      Lt(import.End.DashboardPriority, "3-04"))
    {
      UseFnB734Priority34();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // -------------------------------------------------------------------------------------
    // Priority 3-5: New Cases Opened With Orders
    // Priority 3-6: New Cases Opened Without Orders
    // -------------------------------------------------------------------------------------
    if (!Lt("3-05", import.Restart.DashboardPriority) && !
      Lt("3-05", import.StartDashboardAuditData.DashboardPriority) && !
      Lt(import.End.DashboardPriority, "3-05"))
    {
      UseFnB734Priority35And36();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // -------------------------------------------------------------------------------------
    // Priority 3-7: Cases Closed With Orders
    // Priority 3-8: Cases closed Without Orders
    // -------------------------------------------------------------------------------------
    if (!Lt("3-07", import.Restart.DashboardPriority) && !
      Lt("3-07", import.StartDashboardAuditData.DashboardPriority) && !
      Lt(import.End.DashboardPriority, "3-07"))
    {
      UseFnB734Priority37And38();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // -------------------------------------------------------------------------------------
    // Priority 3-9: Modifications
    // -------------------------------------------------------------------------------------
    if (!Lt("3-09", import.Restart.DashboardPriority) && !
      Lt("3-09", import.StartDashboardAuditData.DashboardPriority) && !
      Lt(import.End.DashboardPriority, "3-09"))
    {
      UseFnB734Priority39();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // -------------------------------------------------------------------------------------
    // Priority 3-10: Income Withholdings Issued
    // -------------------------------------------------------------------------------------
    if (!Lt("3-10", import.Restart.DashboardPriority) && !
      Lt("3-10", import.StartDashboardAuditData.DashboardPriority) && !
      Lt(import.End.DashboardPriority, "3-10"))
    {
      UseFnB734Priority310();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // -------------------------------------------------------------------------------------
    // Priority 3-11: Contempt Motion Filings
    // -------------------------------------------------------------------------------------
    if (!Lt("3-11", import.Restart.DashboardPriority) && !
      Lt("3-11", import.StartDashboardAuditData.DashboardPriority) && !
      Lt(import.End.DashboardPriority, "3-11"))
    {
      UseFnB734Priority311();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // -------------------------------------------------------------------------------------
    // Priority 3-12: Contempt Order Filings
    // -------------------------------------------------------------------------------------
    if (!Lt("3-12", import.Restart.DashboardPriority) && !
      Lt("3-12", import.StartDashboardAuditData.DashboardPriority) && !
      Lt(import.End.DashboardPriority, "3-12"))
    {
      UseFnB734Priority312();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // -------------------------------------------------------------------------------------
    // Priority 3-13: Collections by Type
    // -------------------------------------------------------------------------------------
    if (!Lt("3-13", import.Restart.DashboardPriority) && !
      Lt("3-13", import.StartDashboardAuditData.DashboardPriority) && !
      Lt(import.End.DashboardPriority, "3-13"))
    {
      UseFnB734Priority313();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // -------------------------------------------------------------------------------------
    // Priority 3-15: Federal Timeframes- Days from referral to order 
    // establishment by referral Attorney
    // -------------------------------------------------------------------------------------
    if (!Lt("3-15", import.Restart.DashboardPriority) && !
      Lt("3-15", import.StartDashboardAuditData.DashboardPriority) && !
      Lt(import.End.DashboardPriority, "3-15"))
    {
      UseFnB734Priority315();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // -------------------------------------------------------------------------------------
    // Priority 3-16 Days From Locate(?) to Service of Process
    // -------------------------------------------------------------------------------------
    if (!Lt("3-16", import.Restart.DashboardPriority) && !
      Lt("3-16", import.StartDashboardAuditData.DashboardPriority) && !
      Lt(import.End.DashboardPriority, "3-16"))
    {
      UseFnB734Priority316();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // -------------------------------------------------------------------------------------
    // Priority 3-17: Aging Report of Unprocessed Legal Referrals by Attorney
    // -------------------------------------------------------------------------------------
    if (!Lt("3-17", import.Restart.DashboardPriority) && !
      Lt("3-17", import.StartDashboardAuditData.DashboardPriority) && !
      Lt(import.End.DashboardPriority, "3-17"))
    {
      UseFnB734Priority317();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // -------------------------------------------------------------------------------------
    // Priority 3-18: Federal Timeframes- Days from IWO to IWO payment
    // -------------------------------------------------------------------------------------
    if (!Lt("3-18", import.Restart.DashboardPriority) && !
      Lt("3-18", import.StartDashboardAuditData.DashboardPriority) && !
      Lt(import.End.DashboardPriority, "3-18"))
    {
      UseFnB734Priority318();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // -------------------------------------------------------------------------------------
    // Priority 3-21: Referrals to Legal for Establishment
    // -------------------------------------------------------------------------------------
    if (!Lt("3-21", import.Restart.DashboardPriority) && !
      Lt("3-21", import.StartDashboardAuditData.DashboardPriority) && !
      Lt(import.End.DashboardPriority, "3-21"))
    {
      UseFnB734Priority321();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // -------------------------------------------------------------------------------------
    // Priority 3-22: Referrals to Legal for Enforcement
    // -------------------------------------------------------------------------------------
    if (!Lt("3-22", import.Restart.DashboardPriority) && !
      Lt("3-22", import.StartDashboardAuditData.DashboardPriority) && !
      Lt(import.End.DashboardPriority, "3-22"))
    {
      UseFnB734Priority322();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
    }

    // -------------------------------------------------------------------------------------
    // -------------------------------------------------------------------------------------
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

  private void UseFnB734Priority31()
  {
    var useImport = new FnB734Priority31.Import();
    var useExport = new FnB734Priority31.Export();

    useImport.ProgramCheckpointRestart.Assign(import.ProgramCheckpointRestart);
    useImport.ReportStartDate.Date = import.StartDateWorkArea.Date;
    MoveDateWorkArea(import.PeriodEnd, useImport.ReportEndDate);
    useImport.ProgramProcessingInfo.ProcessDate =
      import.ProgramProcessingInfo.ProcessDate;
    MoveDashboardAuditData(import.DashboardAuditData,
      useImport.DashboardAuditData);
    useImport.AuditFlag.Flag = import.AuditFlag.Flag;

    context.Call(FnB734Priority31.Execute, useImport, useExport);
  }

  private void UseFnB734Priority310()
  {
    var useImport = new FnB734Priority310.Import();
    var useExport = new FnB734Priority310.Export();

    MoveDashboardAuditData(import.DashboardAuditData,
      useImport.DashboardAuditData);
    useImport.ProgramProcessingInfo.ProcessDate =
      import.ProgramProcessingInfo.ProcessDate;
    MoveDateWorkArea(import.PeriodEnd, useImport.ReportEndDate);
    MoveDateWorkArea(import.StartDateWorkArea, useImport.ReportStartDate);
    useImport.ProgramCheckpointRestart.Assign(import.ProgramCheckpointRestart);
    useImport.AuditFlag.Flag = import.AuditFlag.Flag;

    context.Call(FnB734Priority310.Execute, useImport, useExport);
  }

  private void UseFnB734Priority311()
  {
    var useImport = new FnB734Priority311.Import();
    var useExport = new FnB734Priority311.Export();

    MoveDashboardAuditData(import.DashboardAuditData,
      useImport.DashboardAuditData);
    useImport.ProgramProcessingInfo.ProcessDate =
      import.ProgramProcessingInfo.ProcessDate;
    MoveDateWorkArea(import.PeriodEnd, useImport.ReportEndDate);
    MoveDateWorkArea(import.StartDateWorkArea, useImport.ReportStartDate);
    useImport.ProgramCheckpointRestart.Assign(import.ProgramCheckpointRestart);
    useImport.AuditFlag.Flag = import.AuditFlag.Flag;

    context.Call(FnB734Priority311.Execute, useImport, useExport);
  }

  private void UseFnB734Priority312()
  {
    var useImport = new FnB734Priority312.Import();
    var useExport = new FnB734Priority312.Export();

    MoveDashboardAuditData(import.DashboardAuditData,
      useImport.DashboardAuditData);
    useImport.ProgramProcessingInfo.ProcessDate =
      import.ProgramProcessingInfo.ProcessDate;
    MoveDateWorkArea(import.PeriodEnd, useImport.ReportEndDate);
    MoveDateWorkArea(import.StartDateWorkArea, useImport.ReportStartDate);
    useImport.ProgramCheckpointRestart.Assign(import.ProgramCheckpointRestart);
    useImport.AuditFlag.Flag = import.AuditFlag.Flag;

    context.Call(FnB734Priority312.Execute, useImport, useExport);
  }

  private void UseFnB734Priority313()
  {
    var useImport = new FnB734Priority313.Import();
    var useExport = new FnB734Priority313.Export();

    MoveDashboardAuditData(import.DashboardAuditData,
      useImport.DashboardAuditData);
    useImport.ProgramCheckpointRestart.Assign(import.ProgramCheckpointRestart);
    useImport.ProgramProcessingInfo.ProcessDate =
      import.ProgramProcessingInfo.ProcessDate;
    MoveDateWorkArea(import.PeriodEnd, useImport.ReportEndDate);
    MoveDateWorkArea(import.StartDateWorkArea, useImport.ReportStartDate);
    useImport.AuditFlag.Flag = import.AuditFlag.Flag;

    context.Call(FnB734Priority313.Execute, useImport, useExport);
  }

  private void UseFnB734Priority315()
  {
    var useImport = new FnB734Priority315.Import();
    var useExport = new FnB734Priority315.Export();

    MoveDashboardAuditData(import.DashboardAuditData,
      useImport.DashboardAuditData);
    useImport.ProgramProcessingInfo.ProcessDate =
      import.ProgramProcessingInfo.ProcessDate;
    MoveDateWorkArea(import.PeriodEnd, useImport.ReportEndDate);
    MoveDateWorkArea(import.StartDateWorkArea, useImport.ReportStartDate);
    useImport.ProgramCheckpointRestart.Assign(import.ProgramCheckpointRestart);
    useImport.AuditFlag.Flag = import.AuditFlag.Flag;

    context.Call(FnB734Priority315.Execute, useImport, useExport);
  }

  private void UseFnB734Priority316()
  {
    var useImport = new FnB734Priority316.Import();
    var useExport = new FnB734Priority316.Export();

    MoveDashboardAuditData(import.DashboardAuditData,
      useImport.DashboardAuditData);
    useImport.ProgramProcessingInfo.ProcessDate =
      import.ProgramProcessingInfo.ProcessDate;
    MoveDateWorkArea(import.PeriodEnd, useImport.ReportEndDate);
    MoveDateWorkArea(import.StartDateWorkArea, useImport.ReportStartDate);
    useImport.ProgramCheckpointRestart.Assign(import.ProgramCheckpointRestart);
    useImport.AuditFlag.Flag = import.AuditFlag.Flag;

    context.Call(FnB734Priority316.Execute, useImport, useExport);
  }

  private void UseFnB734Priority317()
  {
    var useImport = new FnB734Priority317.Import();
    var useExport = new FnB734Priority317.Export();

    MoveDashboardAuditData(import.DashboardAuditData,
      useImport.DashboardAuditData);
    useImport.ReportStartDate.Date = import.StartDateWorkArea.Date;
    MoveDateWorkArea(import.PeriodEnd, useImport.ReportEndDate);
    useImport.ProgramProcessingInfo.ProcessDate =
      import.ProgramProcessingInfo.ProcessDate;
    useImport.ProgramCheckpointRestart.Assign(import.ProgramCheckpointRestart);
    useImport.AuditFlag.Flag = import.AuditFlag.Flag;

    context.Call(FnB734Priority317.Execute, useImport, useExport);
  }

  private void UseFnB734Priority318()
  {
    var useImport = new FnB734Priority318.Import();
    var useExport = new FnB734Priority318.Export();

    MoveDashboardAuditData(import.DashboardAuditData,
      useImport.DashboardAuditData);
    useImport.ProgramProcessingInfo.ProcessDate =
      import.ProgramProcessingInfo.ProcessDate;
    MoveDateWorkArea(import.PeriodEnd, useImport.ReportEndDate);
    MoveDateWorkArea(import.StartDateWorkArea, useImport.ReportStartDate);
    useImport.ProgramCheckpointRestart.Assign(import.ProgramCheckpointRestart);
    useImport.AuditFlag.Flag = import.AuditFlag.Flag;

    context.Call(FnB734Priority318.Execute, useImport, useExport);
  }

  private void UseFnB734Priority32()
  {
    var useImport = new FnB734Priority32.Import();
    var useExport = new FnB734Priority32.Export();

    useImport.ProgramCheckpointRestart.Assign(import.ProgramCheckpointRestart);
    MoveDashboardAuditData(import.DashboardAuditData,
      useImport.DashboardAuditData);
    useImport.ReportStartDate.Date = import.StartDateWorkArea.Date;
    MoveDateWorkArea(import.PeriodEnd, useImport.ReportEndDate);
    useImport.ProgramProcessingInfo.ProcessDate =
      import.ProgramProcessingInfo.ProcessDate;
    useImport.AuditFlag.Flag = import.AuditFlag.Flag;

    context.Call(FnB734Priority32.Execute, useImport, useExport);
  }

  private void UseFnB734Priority321()
  {
    var useImport = new FnB734Priority321.Import();
    var useExport = new FnB734Priority321.Export();

    MoveDashboardAuditData(import.DashboardAuditData,
      useImport.DashboardAuditData);
    MoveDateWorkArea(import.StartDateWorkArea, useImport.ReportStartDate);
    MoveDateWorkArea(import.PeriodEnd, useImport.ReportEndDate);
    useImport.ProgramProcessingInfo.ProcessDate =
      import.ProgramProcessingInfo.ProcessDate;
    useImport.ProgramCheckpointRestart.Assign(import.ProgramCheckpointRestart);
    useImport.AuditFlag.Flag = import.AuditFlag.Flag;

    context.Call(FnB734Priority321.Execute, useImport, useExport);
  }

  private void UseFnB734Priority322()
  {
    var useImport = new FnB734Priority322.Import();
    var useExport = new FnB734Priority322.Export();

    MoveDashboardAuditData(import.DashboardAuditData,
      useImport.DashboardAuditData);
    MoveDateWorkArea(import.StartDateWorkArea, useImport.ReportStartDate);
    MoveDateWorkArea(import.PeriodEnd, useImport.ReportEndDate);
    useImport.ProgramProcessingInfo.ProcessDate =
      import.ProgramProcessingInfo.ProcessDate;
    useImport.ProgramCheckpointRestart.Assign(import.ProgramCheckpointRestart);
    useImport.AuditFlag.Flag = import.AuditFlag.Flag;

    context.Call(FnB734Priority322.Execute, useImport, useExport);
  }

  private void UseFnB734Priority33()
  {
    var useImport = new FnB734Priority33.Import();
    var useExport = new FnB734Priority33.Export();

    useImport.ProgramProcessingInfo.ProcessDate =
      import.ProgramProcessingInfo.ProcessDate;
    MoveDashboardAuditData(import.DashboardAuditData,
      useImport.DashboardAuditData);
    MoveDateWorkArea(import.PeriodEnd, useImport.ReportEndDate);
    MoveDateWorkArea(import.StartDateWorkArea, useImport.ReportStartDate);
    useImport.ProgramCheckpointRestart.Assign(import.ProgramCheckpointRestart);
    useImport.AuditFlag.Flag = import.AuditFlag.Flag;

    context.Call(FnB734Priority33.Execute, useImport, useExport);
  }

  private void UseFnB734Priority34()
  {
    var useImport = new FnB734Priority34.Import();
    var useExport = new FnB734Priority34.Export();

    MoveDateWorkArea(import.PeriodEnd, useImport.ReportEndDate);
    MoveDateWorkArea(import.StartDateWorkArea, useImport.ReportStartDate);
    useImport.ProgramCheckpointRestart.Assign(import.ProgramCheckpointRestart);
    useImport.ProgramProcessingInfo.ProcessDate =
      import.ProgramProcessingInfo.ProcessDate;
    MoveDashboardAuditData(import.DashboardAuditData,
      useImport.DashboardAuditData);
    useImport.AuditFlag.Flag = import.AuditFlag.Flag;

    context.Call(FnB734Priority34.Execute, useImport, useExport);
  }

  private void UseFnB734Priority35And36()
  {
    var useImport = new FnB734Priority35And36.Import();
    var useExport = new FnB734Priority35And36.Export();

    MoveDashboardAuditData(import.DashboardAuditData,
      useImport.DashboardAuditData);
    useImport.ProgramCheckpointRestart.Assign(import.ProgramCheckpointRestart);
    useImport.ProgramProcessingInfo.ProcessDate =
      import.ProgramProcessingInfo.ProcessDate;
    MoveDateWorkArea(import.PeriodEnd, useImport.ReportEndDate);
    MoveDateWorkArea(import.StartDateWorkArea, useImport.ReportStartDate);
    useImport.AuditFlag.Flag = import.AuditFlag.Flag;

    context.Call(FnB734Priority35And36.Execute, useImport, useExport);
  }

  private void UseFnB734Priority37And38()
  {
    var useImport = new FnB734Priority37And38.Import();
    var useExport = new FnB734Priority37And38.Export();

    MoveDashboardAuditData(import.DashboardAuditData,
      useImport.DashboardAuditData);
    useImport.ProgramCheckpointRestart.Assign(import.ProgramCheckpointRestart);
    useImport.ProgramProcessingInfo.ProcessDate =
      import.ProgramProcessingInfo.ProcessDate;
    MoveDateWorkArea(import.PeriodEnd, useImport.ReportEndDate);
    MoveDateWorkArea(import.StartDateWorkArea, useImport.ReportStartDate);
    useImport.AuditFlag.Flag = import.AuditFlag.Flag;

    context.Call(FnB734Priority37And38.Execute, useImport, useExport);
  }

  private void UseFnB734Priority39()
  {
    var useImport = new FnB734Priority39.Import();
    var useExport = new FnB734Priority39.Export();

    MoveDashboardAuditData(import.DashboardAuditData,
      useImport.DashboardAuditData);
    useImport.ReportStartDate.Date = import.StartDateWorkArea.Date;
    MoveDateWorkArea(import.PeriodEnd, useImport.ReportEndDate);
    useImport.ProgramProcessingInfo.ProcessDate =
      import.ProgramProcessingInfo.ProcessDate;
    useImport.ProgramCheckpointRestart.Assign(import.ProgramCheckpointRestart);
    useImport.AuditFlag.Flag = import.AuditFlag.Flag;

    context.Call(FnB734Priority39.Execute, useImport, useExport);
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
