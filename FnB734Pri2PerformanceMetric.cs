// Program: FN_B734_PRI_2_PERFORMANCE_METRIC, ID: 945146758, model: 746.
// Short name: SWE03095
using System;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B734_PRI_2_PERFORMANCE_METRIC.
/// </summary>
[Serializable]
[Program("SWE03095")]
public partial class FnB734Pri2PerformanceMetric: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B734_PRI_2_PERFORMANCE_METRIC program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB734Pri2PerformanceMetric(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB734Pri2PerformanceMetric.
  /// </summary>
  public FnB734Pri2PerformanceMetric(IContext context, Import import,
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
    // 03/22/13  GVandy	CQ36547		Initial Development.
    // 			Segment B	
    // 07/02/13  GVandy			Remove Priority 2-13.
    // ---------------------------------------------------------------------------------------------------
    // -------------------------------------------------------------------------------------
    // PRIORITY 2 - Performance Measures by Judicial District
    // -------------------------------------------------------------------------------------
    // Priority 1 is dedicated to capturing performance measures at the Judicial
    // District
    // level.  All data elements in Priority 2 also exist in Priority 1.  The 
    // expectation is
    // that Priority 2 will provide a more accurate tool by which to measure the
    // performance
    // of each Judicial District.
    // With the exception of Priority 2-13, all Priority 2 measurements are done
    // in the
    // Priority 1 action block.  For example, fn_b734_priority_1_1 calculates 
    // both
    // Priority 1-1 and Priority 2-1.
    // -------------------------------------------------------------------------------------
    // -------------------------------------------------------------------------------------
    // Priority 2-13: Percent of Cases Paying
    // -------------------------------------------------------------------------------------
    if (!Lt("2-13", import.Restart.DashboardPriority) && !
      Lt("2-13", import.Start.DashboardPriority) && !
      Lt(import.End.DashboardPriority, "2-13"))
    {
      // Priority 2-13 was removed from dashboard after it successfully 
      // completed acceptance testing.
      // Prod Fix testing revealed significant performance issues due to how the
      // business rules specify this measurement is calculated.
      // USE fn_b734_priority_2_13
      //     WHICH IMPORTS: Entity View import dashboard_audit_data
      // 				TO Entity View import dashboard_audit_data
      //                    Entity View import program_checkpoint_restart
      // 				TO Entity View import program_checkpoint_restart
      //                    Entity View import program_processing_info
      // 				TO Entity View import program_processing_info
      //                    Work View   import_fiscal_year_start date_work_area
      // 				TO Work View   import_report_start_date date_work_area
      //                    Work View   import_period_end date_work_area
      // 				TO Work View   import_report_end_date date_work_area
      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
    }
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
    /// A value of FiscalYearEnd.
    /// </summary>
    public DateWorkArea FiscalYearEnd
    {
      get => fiscalYearEnd ??= new();
      set => fiscalYearEnd = value;
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
    /// A value of DashboardAuditData.
    /// </summary>
    public DashboardAuditData DashboardAuditData
    {
      get => dashboardAuditData ??= new();
      set => dashboardAuditData = value;
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

    private DateWorkArea? fiscalYearEnd;
    private DateWorkArea? fiscalYearStart;
    private ProgramProcessingInfo? programProcessingInfo;
    private ProgramCheckpointRestart? programCheckpointRestart;
    private DateWorkArea? periodStart;
    private DateWorkArea? periodEnd;
    private DashboardAuditData? dashboardAuditData;
    private DashboardAuditData? restart;
    private DashboardAuditData? start;
    private DashboardAuditData? end;
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
