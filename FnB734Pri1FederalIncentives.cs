// Program: FN_B734_PRI_1_FEDERAL_INCENTIVES, ID: 945116568, model: 746.
// Short name: SWE03078
using System;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_B734_PRI_1_FEDERAL_INCENTIVES.
/// </para>
/// <para>
/// PRIORITY 1- OCSE157 Federal Incentive Measures Reporting
/// </para>
/// </summary>
[Serializable]
[Program("SWE03078")]
public partial class FnB734Pri1FederalIncentives: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B734_PRI_1_FEDERAL_INCENTIVES program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB734Pri1FederalIncentives(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB734Pri1FederalIncentives.
  /// </summary>
  public FnB734Pri1FederalIncentives(IContext context, Import import,
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
    // 02/20/13  GVandy	CQ36547		Initial Development.  Priority 1-1, 1-3, and 1-
    // 4.
    // 			Segment A
    // 02/04/20  GVandy	CQ66220		Correlate with OCSE157 changes beginning in FY 
    // 2022.
    // 					These changes include only amounts in OCSE157
    // 					Lines 25, 27, and 29 that are both distributed
    // 					and disbursed.  Export a cutoff FY which defaults to
    // 					2022 but can be overridden with a code table value for testing.
    // ---------------------------------------------------------------------------------------------------
    // -------------------------------------------------------------------------------------
    // PRIORITY 1- OCSE157 Federal Incentive Measures Reporting
    // -------------------------------------------------------------------------------------
    // Priority 1 is dedicated to reporting federal incentive measures at the 
    // state and
    // Judicial District levels.  All data reported at the Judicial District 
    // Level will use
    // the Business Rules defined in Priority 2.  The Business Rules as defined 
    // in Priority
    // 1 will report data at the statewide level.
    // **Data reported at the statewide level should mirror the OCSE157 report 
    // with few
    // exceptions**
    // Report Levels- Identifies all the possible report levels that exist and 
    // the hierarchy
    // of these levels.
    // Each case is only counted ONCE (attributed to caseworker/office 
    // assignment).
    // OCSE157 Line1- Cases open as of (report period) end.  If the case closure
    // date is the
    // last day of the (report period) then the case is counted.
    // -------------------------------------------------------------------------------------
    // -------------------------------------------------------------------------------------
    // Priority 1-1: Cases With Support Orders
    // -------------------------------------------------------------------------------------
    if (!Lt("1-01", import.Restart.DashboardPriority) && !
      Lt("1-01", import.Start.DashboardPriority) && !
      Lt(import.End.DashboardPriority, "1-01"))
    {
      UseFnB734Priority11();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // -------------------------------------------------------------------------------------
    // Priority 1-2: IV-D Paternity Establishment Percentage (PEP)
    // -------------------------------------------------------------------------------------
    if (!Lt("1-02", import.Restart.DashboardPriority) && !
      Lt("1-02", import.Start.DashboardPriority) && !
      Lt(import.End.DashboardPriority, "1-02"))
    {
      UseFnB734Priority12();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // -------------------------------------------------------------------------------------
    // Priority 1-3: Percent of Cases Paying on Arrears
    // -------------------------------------------------------------------------------------
    if (!Lt("1-03", import.Restart.DashboardPriority) && !
      Lt("1-03", import.Start.DashboardPriority) && !
      Lt(import.End.DashboardPriority, "1-03"))
    {
      // -- The reporting period start date = FY start date
      //        reporting period end date = report period end date (PPI/As Of 
      // date)
      UseFnB734Priority13();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // -------------------------------------------------------------------------------------
    // Priority 1-4: Collections on Current Support
    // -------------------------------------------------------------------------------------
    if (!Lt("1-04", import.Restart.DashboardPriority) && !
      Lt("1-04", import.Start.DashboardPriority) && !
      Lt(import.End.DashboardPriority, "1-04"))
    {
      UseFnB734Priority14();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // -------------------------------------------------------------------------------------
    // Priority 1-5: Comparison of Total Collections federal fiscal YTD Over 
    // Prior Year
    // and
    // Priority 1-6: Comparison of Total Collections in Month to Prior Year Same
    // Month
    // -------------------------------------------------------------------------------------
    if (!Lt("1-05", import.Restart.DashboardPriority) && !
      Lt("1-05", import.Start.DashboardPriority) && !
      Lt(import.End.DashboardPriority, "1-05"))
    {
      UseFnB734Priority15And16();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // -------------------------------------------------------------------------------------
    // Priority 1-7: Arrears Distributed (Report month, FFYTD)
    // -------------------------------------------------------------------------------------
    if (!Lt("1-07", import.Restart.DashboardPriority) && !
      Lt("1-07", import.Start.DashboardPriority) && !
      Lt(import.End.DashboardPriority, "1-07"))
    {
      UseFnB734Priority17();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // -------------------------------------------------------------------------------------
    // Priority 1-8: Arrears Due (End of Report Period)
    // -------------------------------------------------------------------------------------
    if (!Lt("1-08", import.Restart.DashboardPriority) && !
      Lt("1-08", import.Start.DashboardPriority) && !
      Lt(import.End.DashboardPriority, "1-08"))
    {
      UseFnB734Priority18();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // -------------------------------------------------------------------------------------
    // Priority 1-9: Collections Per Obligated Case (Current Reporting Period-
    // Monthly)
    // -------------------------------------------------------------------------------------
    if (!Lt("1-09", import.Restart.DashboardPriority) && !
      Lt("1-09", import.Start.DashboardPriority) && !
      Lt(import.End.DashboardPriority, "1-09"))
    {
      UseFnB734Priority19();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // -------------------------------------------------------------------------------------
    // Priority 1-10: Income Withholdings Per Obligated Case
    // -------------------------------------------------------------------------------------
    if (!Lt("1-10", import.Restart.DashboardPriority) && !
      Lt("1-10", import.Start.DashboardPriority) && !
      Lt(import.End.DashboardPriority, "1-10"))
    {
      UseFnB734Priority110();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // -------------------------------------------------------------------------------------
    // Priority 1-11: Cases Per FTE (full-time equivalent)
    // -------------------------------------------------------------------------------------
    if (!Lt("1-11", import.Restart.DashboardPriority) && !
      Lt("1-11", import.Start.DashboardPriority) && !
      Lt(import.End.DashboardPriority, "1-11"))
    {
      UseFnB734Priority111();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // -------------------------------------------------------------------------------------
    // Priority 1-12: Collections Per FTE (full-time equivalent)
    // -------------------------------------------------------------------------------------
    if (!Lt("1-12", import.Restart.DashboardPriority) && !
      Lt("1-12", import.Start.DashboardPriority) && !
      Lt(import.End.DashboardPriority, "1-12"))
    {
      UseFnB734Priority112();

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

  private void UseFnB734Priority11()
  {
    var useImport = new FnB734Priority11.Import();
    var useExport = new FnB734Priority11.Export();

    useImport.ProgramProcessingInfo.ProcessDate =
      import.ProgramProcessingInfo.ProcessDate;
    useImport.ProgramCheckpointRestart.Assign(import.ProgramCheckpointRestart);
    MoveDashboardAuditData(import.DashboardAuditData,
      useImport.DashboardAuditData);
    MoveDateWorkArea(import.PeriodEnd, useImport.ReportEndDate);
    useImport.AuditFlag.Flag = import.AuditRec.Flag;

    context.Call(FnB734Priority11.Execute, useImport, useExport);
  }

  private void UseFnB734Priority110()
  {
    var useImport = new FnB734Priority110.Import();
    var useExport = new FnB734Priority110.Export();

    MoveDashboardAuditData(import.DashboardAuditData,
      useImport.DashboardAuditData);
    useImport.ProgramCheckpointRestart.Assign(import.ProgramCheckpointRestart);
    useImport.ProgramProcessingInfo.ProcessDate =
      import.ProgramProcessingInfo.ProcessDate;
    MoveDateWorkArea(import.PeriodStart, useImport.ReportStartDate);
    MoveDateWorkArea(import.PeriodEnd, useImport.ReportEndDate);
    useImport.AuditFlag.Flag = import.AuditRec.Flag;

    context.Call(FnB734Priority110.Execute, useImport, useExport);
  }

  private void UseFnB734Priority111()
  {
    var useImport = new FnB734Priority111.Import();
    var useExport = new FnB734Priority111.Export();

    MoveDashboardAuditData(import.DashboardAuditData,
      useImport.DashboardAuditData);
    useImport.ProgramCheckpointRestart.Assign(import.ProgramCheckpointRestart);
    useImport.ProgramProcessingInfo.ProcessDate =
      import.ProgramProcessingInfo.ProcessDate;
    MoveDateWorkArea(import.PeriodStart, useImport.ReportStartDate);
    MoveDateWorkArea(import.PeriodEnd, useImport.ReportEndDate);

    context.Call(FnB734Priority111.Execute, useImport, useExport);
  }

  private void UseFnB734Priority112()
  {
    var useImport = new FnB734Priority112.Import();
    var useExport = new FnB734Priority112.Export();

    MoveDashboardAuditData(import.DashboardAuditData,
      useImport.DashboardAuditData);
    useImport.ProgramCheckpointRestart.Assign(import.ProgramCheckpointRestart);
    useImport.ProgramProcessingInfo.ProcessDate =
      import.ProgramProcessingInfo.ProcessDate;
    MoveDateWorkArea(import.PeriodStart, useImport.ReportStartDate);
    MoveDateWorkArea(import.PeriodEnd, useImport.ReportEndDate);

    context.Call(FnB734Priority112.Execute, useImport, useExport);
  }

  private void UseFnB734Priority12()
  {
    var useImport = new FnB734Priority12.Import();
    var useExport = new FnB734Priority12.Export();

    MoveDashboardAuditData(import.DashboardAuditData,
      useImport.DashboardAuditData);
    useImport.ProgramCheckpointRestart.Assign(import.ProgramCheckpointRestart);
    useImport.ProgramProcessingInfo.ProcessDate =
      import.ProgramProcessingInfo.ProcessDate;
    MoveDateWorkArea(import.FiscalYearStart, useImport.ReportStartDate);
    MoveDateWorkArea(import.PeriodEnd, useImport.ReportEndDate);
    MoveDateWorkArea(import.FiscalYearStart, useImport.FiscalYearStartDate);
    MoveDateWorkArea(import.FiscalYearEnd, useImport.FiscalYearEndDate);
    useImport.AuditFlag.Flag = import.AuditRec.Flag;

    context.Call(FnB734Priority12.Execute, useImport, useExport);
  }

  private void UseFnB734Priority13()
  {
    var useImport = new FnB734Priority13.Import();
    var useExport = new FnB734Priority13.Export();

    MoveDashboardAuditData(import.DashboardAuditData,
      useImport.DashboardAuditData);
    useImport.ProgramCheckpointRestart.Assign(import.ProgramCheckpointRestart);
    useImport.ProgramProcessingInfo.ProcessDate =
      import.ProgramProcessingInfo.ProcessDate;
    MoveDateWorkArea(import.PeriodEnd, useImport.ReportEndDate);
    MoveDateWorkArea(import.FiscalYearStart, useImport.ReportStartDate);
    useImport.Cq66220EffectiveFy.FiscalYear =
      import.Cq66220EffectiveFy.FiscalYear;
    useImport.AuditFlag.Flag = import.AuditRec.Flag;

    context.Call(FnB734Priority13.Execute, useImport, useExport);
  }

  private void UseFnB734Priority14()
  {
    var useImport = new FnB734Priority14.Import();
    var useExport = new FnB734Priority14.Export();

    MoveDashboardAuditData(import.DashboardAuditData,
      useImport.DashboardAuditData);
    useImport.ProgramCheckpointRestart.Assign(import.ProgramCheckpointRestart);
    useImport.ProgramProcessingInfo.ProcessDate =
      import.ProgramProcessingInfo.ProcessDate;
    MoveDateWorkArea(import.PeriodStart, useImport.ReportStartDate);
    MoveDateWorkArea(import.PeriodEnd, useImport.ReportEndDate);
    MoveDateWorkArea(import.FiscalYearStart, useImport.FiscalYearStart);
    useImport.Cq66220EffectiveFy.FiscalYear =
      import.Cq66220EffectiveFy.FiscalYear;
    useImport.AuditFlag.Flag = import.AuditRec.Flag;

    context.Call(FnB734Priority14.Execute, useImport, useExport);
  }

  private void UseFnB734Priority15And16()
  {
    var useImport = new FnB734Priority15And16.Import();
    var useExport = new FnB734Priority15And16.Export();

    MoveDashboardAuditData(import.DashboardAuditData,
      useImport.DashboardAuditData);
    useImport.ProgramCheckpointRestart.Assign(import.ProgramCheckpointRestart);
    useImport.ProgramProcessingInfo.ProcessDate =
      import.ProgramProcessingInfo.ProcessDate;
    MoveDateWorkArea(import.PeriodStart, useImport.ReportStartDate);
    MoveDateWorkArea(import.PeriodEnd, useImport.ReportEndDate);
    MoveDateWorkArea(import.FiscalYearStart, useImport.FiscalYearStartDate);
    useImport.AuditFlag.Flag = import.AuditRec.Flag;

    context.Call(FnB734Priority15And16.Execute, useImport, useExport);
  }

  private void UseFnB734Priority17()
  {
    var useImport = new FnB734Priority17.Import();
    var useExport = new FnB734Priority17.Export();

    MoveDashboardAuditData(import.DashboardAuditData,
      useImport.DashboardAuditData);
    useImport.ProgramCheckpointRestart.Assign(import.ProgramCheckpointRestart);
    useImport.ProgramProcessingInfo.ProcessDate =
      import.ProgramProcessingInfo.ProcessDate;
    MoveDateWorkArea(import.PeriodStart, useImport.ReportStartDate);
    MoveDateWorkArea(import.PeriodEnd, useImport.ReportEndDate);
    MoveDateWorkArea(import.FiscalYearStart, useImport.FiscalYearStart);
    useImport.Cq66220EffectiveFy.FiscalYear =
      import.Cq66220EffectiveFy.FiscalYear;
    useImport.AuditFlag.Flag = import.AuditRec.Flag;

    context.Call(FnB734Priority17.Execute, useImport, useExport);
  }

  private void UseFnB734Priority18()
  {
    var useImport = new FnB734Priority18.Import();
    var useExport = new FnB734Priority18.Export();

    MoveDashboardAuditData(import.DashboardAuditData,
      useImport.DashboardAuditData);
    useImport.ProgramCheckpointRestart.Assign(import.ProgramCheckpointRestart);
    useImport.ProgramProcessingInfo.ProcessDate =
      import.ProgramProcessingInfo.ProcessDate;
    MoveDateWorkArea(import.PeriodEnd, useImport.ReportEndDate);
    MoveDateWorkArea(import.PeriodStart, useImport.ReportStartDate);
    useImport.AuditFlag.Flag = import.AuditRec.Flag;

    context.Call(FnB734Priority18.Execute, useImport, useExport);
  }

  private void UseFnB734Priority19()
  {
    var useImport = new FnB734Priority19.Import();
    var useExport = new FnB734Priority19.Export();

    useImport.ProgramProcessingInfo.ProcessDate =
      import.ProgramProcessingInfo.ProcessDate;
    useImport.ProgramCheckpointRestart.Assign(import.ProgramCheckpointRestart);
    MoveDashboardAuditData(import.DashboardAuditData,
      useImport.DashboardAuditData);

    context.Call(FnB734Priority19.Execute, useImport, useExport);
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
    /// A value of Start.
    /// </summary>
    public DashboardAuditData Start
    {
      get => start ??= new();
      set => start = value;
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
    /// A value of Restart.
    /// </summary>
    public DashboardAuditData Restart
    {
      get => restart ??= new();
      set => restart = value;
    }

    /// <summary>
    /// A value of PeriodStart.
    /// </summary>
    public DateWorkArea PeriodStart
    {
      get => periodStart ??= new();
      set => periodStart = value;
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
    /// A value of FiscalYearStart.
    /// </summary>
    public DateWorkArea FiscalYearStart
    {
      get => fiscalYearStart ??= new();
      set => fiscalYearStart = value;
    }

    /// <summary>
    /// A value of FiscalYearEnd.
    /// </summary>
    public DateWorkArea FiscalYearEnd
    {
      get => fiscalYearEnd ??= new();
      set => fiscalYearEnd = value;
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
    /// A value of AuditRec.
    /// </summary>
    public Common AuditRec
    {
      get => auditRec ??= new();
      set => auditRec = value;
    }

    /// <summary>
    /// A value of Cq66220EffectiveFy.
    /// </summary>
    public Ocse157Verification Cq66220EffectiveFy
    {
      get => cq66220EffectiveFy ??= new();
      set => cq66220EffectiveFy = value;
    }

    private DashboardAuditData? dashboardAuditData;
    private ProgramProcessingInfo? programProcessingInfo;
    private DashboardAuditData? start;
    private DashboardAuditData? end;
    private DashboardAuditData? restart;
    private DateWorkArea? periodStart;
    private DateWorkArea? periodEnd;
    private DateWorkArea? fiscalYearStart;
    private DateWorkArea? fiscalYearEnd;
    private ProgramCheckpointRestart? programCheckpointRestart;
    private Common? auditRec;
    private Ocse157Verification? cq66220EffectiveFy;
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
