// Program: FN_B734_PRIORITY_1_9, ID: 945132078, model: 746.
// Short name: SWE03090
using System;
using System.Collections.Generic;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_B734_PRIORITY_1_9.
/// </para>
/// <para>
/// Priority 1-9: Collections Per Obligated Case
/// </para>
/// </summary>
[Serializable]
[Program("SWE03090")]
public partial class FnB734Priority19: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B734_PRIORITY_1_9 program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB734Priority19(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB734Priority19.
  /// </summary>
  public FnB734Priority19(IContext context, Import import, Export export):
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
    // ---------------------------------------------------------------------------------------------------
    // -------------------------------------------------------------------------------------
    // Priority 1-9: Collections Per Obligated Case (Current Reporting Period-
    // Monthly)
    // -------------------------------------------------------------------------------------
    // This calculation will be an average.
    // Obligated is defined as a financial order and obligation owed to the case
    // that is open during the reporting period.
    // Report Level: State, Judicial District
    // Report Period: Month
    // Numerator
    // 	1) See the rules in Priority 1.6.
    // Denominator
    // 	1) See the rules in Priority 1.1 (Numerator)
    // -------------------------------------------------------------------------------------
    MoveProgramCheckpointRestart(import.ProgramCheckpointRestart,
      local.ProgramCheckpointRestart);

    // -- The as_of_date is deliberately not being set during updates because 
    // this priority
    // -- is calculated using values from previously calculated priorities.  The
    // as_of_date
    // -- would have been set when those priorities were calculated.
    // ------------------------------------------------------------------------------
    // -- Calculate the Numerator, Denominator, and Average using values 
    // previously
    // -- stored during processing for collections in month and cases under 
    // order
    // -- numerator.
    // ------------------------------------------------------------------------------
    foreach(var _ in ReadDashboardStagingPriority1())
    {
      local.Temp.CollectionsPerObligCaseNumer =
        entities.DashboardStagingPriority12.CollectionsInMonthActual;
      local.Temp.CollectionsPerObligCaseDenom =
        entities.DashboardStagingPriority12.CasesUnderOrderNumerator;

      if ((local.Temp.CollectionsPerObligCaseDenom ?? 0M) == 0)
      {
        local.Temp.CollectionsPerObligCaseAvg = 0;
      }
      else
      {
        local.Temp.CollectionsPerObligCaseAvg =
          Math.Round((local.Temp.CollectionsPerObligCaseNumer ?? 0M) /
          (local.Temp.CollectionsPerObligCaseDenom ?? 0M), 2,
          MidpointRounding.AwayFromZero);
      }

      try
      {
        UpdateDashboardStagingPriority1();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "DASHBOARD_STAGING_PRI_1_2_NU";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "DASHBOARD_STAGING_PRI_1_2_PV";

            break;
          default:
            throw;
        }
      }
    }

    local.Common.Count = 0;
    local.PrevRank.CollectionsPerObligCaseAvg = 0;
    local.Temp.CollectionsPerObligCaseRnk = 1;

    // ------------------------------------------------------------------------------
    // -- Calculate Judicial District Ranking.
    // ------------------------------------------------------------------------------
    foreach(var _ in ReadDashboardStagingPriority2())
    {
      ++local.Common.Count;

      if ((entities.DashboardStagingPriority12.CollectionsPerObligCaseAvg ?? 0M) ==
        (local.PrevRank.CollectionsPerObligCaseAvg ?? 0M))
      {
        // -- The ranking for this judicial district is tied with the previous 
        // judicial district.
        // -- This JD gets the same ranking already in the local_temp 
        // case_under_order_rank.
      }
      else
      {
        local.Temp.CollectionsPerObligCaseRnk = local.Common.Count;
      }

      try
      {
        UpdateDashboardStagingPriority2();
        local.PrevRank.CollectionsPerObligCaseAvg =
          entities.DashboardStagingPriority12.CollectionsPerObligCaseAvg;
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "DASHBOARD_STAGING_PRI_1_2_NU";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "DASHBOARD_STAGING_PRI_1_2_PV";

            break;
          default:
            throw;
        }
      }
    }

    local.Common.Count = 0;
    local.PrevRank.CollectionsPerObligCaseAvg = 0;
    local.Temp.CollectionsPerObligCaseRnk = 1;

    // ------------------------------------------------------------------------------
    // -- Calculate Contractor Ranking.
    // ------------------------------------------------------------------------------
    foreach(var _ in ReadDashboardStagingPriority3())
    {
      ++local.Common.Count;

      if ((entities.DashboardStagingPriority12.CollectionsPerObligCaseAvg ?? 0M) ==
        (local.PrevRank.CollectionsPerObligCaseAvg ?? 0M))
      {
        // -- The ranking for this judicial district is tied with the previous 
        // judicial district.
        // -- This JD gets the same ranking already in the local_temp 
        // case_under_order_rank.
      }
      else
      {
        local.Temp.CollectionsPerObligCaseRnk = local.Common.Count;
      }

      try
      {
        UpdateDashboardStagingPriority2();
        local.PrevRank.CollectionsPerObligCaseAvg =
          entities.DashboardStagingPriority12.CollectionsPerObligCaseAvg;
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "DASHBOARD_STAGING_PRI_1_2_NU";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "DASHBOARD_STAGING_PRI_1_2_PV";

            break;
          default:
            throw;
        }
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
      Substring(import.ProgramCheckpointRestart.RestartInfo, 250, 1, 80) + "1-10    ";
    UseUpdateCheckpointRstAndCommit();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "Error taking checkpoint.";
      UseCabErrorReport();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }
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

  private void UseUpdateCheckpointRstAndCommit()
  {
    var useImport = new UpdateCheckpointRstAndCommit.Import();
    var useExport = new UpdateCheckpointRstAndCommit.Export();

    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);

    context.Call(UpdateCheckpointRstAndCommit.Execute, useImport, useExport);
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
        entities.DashboardStagingPriority12.ReportMonth =
          db.GetInt32(reader, 0);
        entities.DashboardStagingPriority12.ReportLevel =
          db.GetString(reader, 1);
        entities.DashboardStagingPriority12.ReportLevelId =
          db.GetString(reader, 2);
        entities.DashboardStagingPriority12.AsOfDate =
          db.GetNullableDate(reader, 3);
        entities.DashboardStagingPriority12.CasesUnderOrderNumerator =
          db.GetNullableInt32(reader, 4);
        entities.DashboardStagingPriority12.CollectionsInMonthActual =
          db.GetNullableDecimal(reader, 5);
        entities.DashboardStagingPriority12.CollectionsPerObligCaseNumer =
          db.GetNullableDecimal(reader, 6);
        entities.DashboardStagingPriority12.CollectionsPerObligCaseDenom =
          db.GetNullableDecimal(reader, 7);
        entities.DashboardStagingPriority12.CollectionsPerObligCaseAvg =
          db.GetNullableDecimal(reader, 8);
        entities.DashboardStagingPriority12.CollectionsPerObligCaseRnk =
          db.GetNullableInt32(reader, 9);
        entities.DashboardStagingPriority12.Populated = true;

        return true;
      },
      () =>
      {
        entities.DashboardStagingPriority12.Populated = false;
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
        entities.DashboardStagingPriority12.ReportMonth =
          db.GetInt32(reader, 0);
        entities.DashboardStagingPriority12.ReportLevel =
          db.GetString(reader, 1);
        entities.DashboardStagingPriority12.ReportLevelId =
          db.GetString(reader, 2);
        entities.DashboardStagingPriority12.AsOfDate =
          db.GetNullableDate(reader, 3);
        entities.DashboardStagingPriority12.CasesUnderOrderNumerator =
          db.GetNullableInt32(reader, 4);
        entities.DashboardStagingPriority12.CollectionsInMonthActual =
          db.GetNullableDecimal(reader, 5);
        entities.DashboardStagingPriority12.CollectionsPerObligCaseNumer =
          db.GetNullableDecimal(reader, 6);
        entities.DashboardStagingPriority12.CollectionsPerObligCaseDenom =
          db.GetNullableDecimal(reader, 7);
        entities.DashboardStagingPriority12.CollectionsPerObligCaseAvg =
          db.GetNullableDecimal(reader, 8);
        entities.DashboardStagingPriority12.CollectionsPerObligCaseRnk =
          db.GetNullableInt32(reader, 9);
        entities.DashboardStagingPriority12.Populated = true;

        return true;
      },
      () =>
      {
        entities.DashboardStagingPriority12.Populated = false;
      });
  }

  private IEnumerable<bool> ReadDashboardStagingPriority3()
  {
    return ReadEach("ReadDashboardStagingPriority3",
      (db, command) =>
      {
        db.SetInt32(
          command, "reportMonth", import.DashboardAuditData.ReportMonth);
      },
      (db, reader) =>
      {
        entities.DashboardStagingPriority12.ReportMonth =
          db.GetInt32(reader, 0);
        entities.DashboardStagingPriority12.ReportLevel =
          db.GetString(reader, 1);
        entities.DashboardStagingPriority12.ReportLevelId =
          db.GetString(reader, 2);
        entities.DashboardStagingPriority12.AsOfDate =
          db.GetNullableDate(reader, 3);
        entities.DashboardStagingPriority12.CasesUnderOrderNumerator =
          db.GetNullableInt32(reader, 4);
        entities.DashboardStagingPriority12.CollectionsInMonthActual =
          db.GetNullableDecimal(reader, 5);
        entities.DashboardStagingPriority12.CollectionsPerObligCaseNumer =
          db.GetNullableDecimal(reader, 6);
        entities.DashboardStagingPriority12.CollectionsPerObligCaseDenom =
          db.GetNullableDecimal(reader, 7);
        entities.DashboardStagingPriority12.CollectionsPerObligCaseAvg =
          db.GetNullableDecimal(reader, 8);
        entities.DashboardStagingPriority12.CollectionsPerObligCaseRnk =
          db.GetNullableInt32(reader, 9);
        entities.DashboardStagingPriority12.Populated = true;

        return true;
      },
      () =>
      {
        entities.DashboardStagingPriority12.Populated = false;
      });
  }

  private void UpdateDashboardStagingPriority1()
  {
    var collectionsPerObligCaseNumer =
      local.Temp.CollectionsPerObligCaseNumer ?? 0M;
    var collectionsPerObligCaseDenom =
      local.Temp.CollectionsPerObligCaseDenom ?? 0M;
    var collectionsPerObligCaseAvg = local.Temp.CollectionsPerObligCaseAvg ?? 0M
      ;

    entities.DashboardStagingPriority12.Populated = false;
    Update("UpdateDashboardStagingPriority1",
      (db, command) =>
      {
        db.SetNullableDecimal(
          command, "collOblCasNum", collectionsPerObligCaseNumer);
        db.SetNullableDecimal(
          command, "collOblCasDen", collectionsPerObligCaseDenom);
        db.SetNullableDecimal(
          command, "collOblCasAvg", collectionsPerObligCaseAvg);
        db.SetInt32(
          command, "reportMonth",
          entities.DashboardStagingPriority12.ReportMonth);
        db.SetString(
          command, "reportLevel",
          entities.DashboardStagingPriority12.ReportLevel);
        db.SetString(
          command, "reportLevelId",
          entities.DashboardStagingPriority12.ReportLevelId);
      });

    entities.DashboardStagingPriority12.CollectionsPerObligCaseNumer =
      collectionsPerObligCaseNumer;
    entities.DashboardStagingPriority12.CollectionsPerObligCaseDenom =
      collectionsPerObligCaseDenom;
    entities.DashboardStagingPriority12.CollectionsPerObligCaseAvg =
      collectionsPerObligCaseAvg;
    entities.DashboardStagingPriority12.Populated = true;
  }

  private void UpdateDashboardStagingPriority2()
  {
    var collectionsPerObligCaseRnk = local.Temp.CollectionsPerObligCaseRnk ?? 0;

    entities.DashboardStagingPriority12.Populated = false;
    Update("UpdateDashboardStagingPriority2",
      (db, command) =>
      {
        db.
          SetNullableInt32(command, "collOblCasRnk", collectionsPerObligCaseRnk);
        db.SetInt32(
          command, "reportMonth",
          entities.DashboardStagingPriority12.ReportMonth);
        db.SetString(
          command, "reportLevel",
          entities.DashboardStagingPriority12.ReportLevel);
        db.SetString(
          command, "reportLevelId",
          entities.DashboardStagingPriority12.ReportLevelId);
      });

    entities.DashboardStagingPriority12.CollectionsPerObligCaseRnk =
      collectionsPerObligCaseRnk;
    entities.DashboardStagingPriority12.Populated = true;
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
    /// A value of DashboardAuditData.
    /// </summary>
    public DashboardAuditData DashboardAuditData
    {
      get => dashboardAuditData ??= new();
      set => dashboardAuditData = value;
    }

    private ProgramProcessingInfo? programProcessingInfo;
    private ProgramCheckpointRestart? programCheckpointRestart;
    private DashboardAuditData? dashboardAuditData;
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
    /// A value of Common.
    /// </summary>
    public Common Common
    {
      get => common ??= new();
      set => common = value;
    }

    /// <summary>
    /// A value of PrevRank.
    /// </summary>
    public DashboardStagingPriority12 PrevRank
    {
      get => prevRank ??= new();
      set => prevRank = value;
    }

    /// <summary>
    /// A value of Temp.
    /// </summary>
    public DashboardStagingPriority12 Temp
    {
      get => temp ??= new();
      set => temp = value;
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

    private Common? common;
    private DashboardStagingPriority12? prevRank;
    private DashboardStagingPriority12? temp;
    private ProgramCheckpointRestart? programCheckpointRestart;
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
    /// A value of DashboardStagingPriority12.
    /// </summary>
    public DashboardStagingPriority12 DashboardStagingPriority12
    {
      get => dashboardStagingPriority12 ??= new();
      set => dashboardStagingPriority12 = value;
    }

    private DashboardStagingPriority12? dashboardStagingPriority12;
  }
#endregion
}
