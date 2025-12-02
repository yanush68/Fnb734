// Program: FN_B734_PRI_4_LEVEL_1, ID: 945237091, model: 746.
// Short name: SWE03725
using System;
using System.Collections.Generic;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B734_PRI_4_LEVEL_1.
/// </summary>
[Serializable]
[Program("SWE03725")]
public partial class FnB734Pri4Level1: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B734_PRI_4_LEVEL_1 program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB734Pri4Level1(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB734Pri4Level1.
  /// </summary>
  public FnB734Pri4Level1(IContext context, Import import, Export export):
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
    // 03/12/14  GVandy	CQ42238		Add Judicial District, Caseworker ID, and 
    // Contractor
    // 					ID to the Pyramid staging table to support new
    // 					Pyramid case list reports.
    // ---------------------------------------------------------------------------------------------------
    // -------------------------------------------------------------------------------------
    // Priority 4: Tier 1- Total Number of Cases
    // -------------------------------------------------------------------------------------
    // Tier 1- Total Number of Cases
    // 1)	Count all cases in O status as of the report period end date.
    // -------------------------------------------------------------------------------------
    MoveProgramCheckpointRestart(import.ProgramCheckpointRestart,
      local.ProgramCheckpointRestart);

    // ------------------------------------------------------------------------------
    // -- Determine if we're restarting and set appropriate restart information.
    // ------------------------------------------------------------------------------
    if (AsChar(import.ProgramCheckpointRestart.RestartInd) == 'Y' && Equal
      (import.ProgramCheckpointRestart.RestartInfo, 81, 8, "4-01    "))
    {
      // -- Checkpoint Info
      // Positions   Value
      // ---------   
      // ------------------------------------
      //  001-080    General Checkpoint Info for PRAD
      //  081-088    Dashboard Priority
      //  089-089    Blank
      //  090-099    CSE Case Number
      local.Restart.Number =
        Substring(import.ProgramCheckpointRestart.RestartInfo, 90, 10);
    }
    else
    {
      local.Restart.Number = "";
    }

    // ------------------------------------------------------------------------------
    // -- Read each open case.
    // ------------------------------------------------------------------------------
    foreach(var _ in ReadCaseCaseAssignmentServiceProvider())
    {
      if (Equal(entities.Case1.Number, local.Prev.Number))
      {
        continue;
      }
      else
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
          //  090-099    CSE Case Number
          local.ProgramCheckpointRestart.RestartInd = "Y";
          local.ProgramCheckpointRestart.RestartInfo =
            Substring(import.ProgramCheckpointRestart.RestartInfo, 250, 1, 80) +
            "4-01    " + " " + String
            (local.Prev.Number, Case1.Number_MaxLength);
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

      local.Prev.Number = entities.Case1.Number;
      ++local.RecordsReadSinceCommit.Count;

      // -- Determine judicial district to which case is assigned on the report 
      // period end date.
      UseFnB734DetermineJdFromCase();

      // -- Determine contractor from the judicial district to which they are 
      // assigned on the report period end date.
      UseFnB734DeterContractorFromJd();

      // -- Determine caseworker to which case is assigned on the report period 
      // end date.
      if (entities.ServiceProvider.Populated)
      {
        local.ServiceProvider.UserId = entities.ServiceProvider.UserId;
      }
      else
      {
        local.ServiceProvider.UserId = "";
      }

      // -------------------------------------------------------------------------------------
      // --  Total Number of Cases
      // -------------------------------------------------------------------------------------
      try
      {
        CreateDashboardStagingPriority4();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "DASHBOARD_STAGING_PRI_4_AE";

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
          "Error creating Dashboard_Staging_Priority_4 in FN_B734_Pri_4_Level_1.";
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
      Substring(import.ProgramCheckpointRestart.RestartInfo, 250, 1, 80) + "4-02    ";
    UseUpdateCheckpointRstAndCommit();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "Error taking checkpoint.";
      UseCabErrorReport();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }
  }

  private static void MoveCseOrganization(CseOrganization source,
    CseOrganization target)
  {
    target.Code = source.Code;
    target.Name = source.Name;
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

  private void UseFnB734DeterContractorFromJd()
  {
    var useImport = new FnB734DeterContractorFromJd.Import();
    var useExport = new FnB734DeterContractorFromJd.Export();

    useImport.ReportEndDate.Date = import.ReportEndDate.Date;
    useImport.DashboardAuditData.JudicialDistrict =
      local.DashboardAuditData.JudicialDistrict;

    context.Call(FnB734DeterContractorFromJd.Execute, useImport, useExport);

    MoveCseOrganization(useExport.Contractor, local.Contractor);
  }

  private void UseFnB734DetermineJdFromCase()
  {
    var useImport = new FnB734DetermineJdFromCase.Import();
    var useExport = new FnB734DetermineJdFromCase.Export();

    useImport.Case1.Number = entities.Case1.Number;
    useImport.ReportEndDate.Date = import.ReportEndDate.Date;

    context.Call(FnB734DetermineJdFromCase.Execute, useImport, useExport);

    local.DashboardAuditData.Assign(useExport.DashboardAuditData);
  }

  private void UseUpdateCheckpointRstAndCommit()
  {
    var useImport = new UpdateCheckpointRstAndCommit.Import();
    var useExport = new UpdateCheckpointRstAndCommit.Export();

    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);

    context.Call(UpdateCheckpointRstAndCommit.Execute, useImport, useExport);
  }

  private void CreateDashboardStagingPriority4()
  {
    var reportMonth = import.DashboardAuditData.ReportMonth;
    var runNumber = import.DashboardAuditData.RunNumber;
    var caseNumber = entities.Case1.Number;
    var asOfDate = import.ProgramProcessingInfo.ProcessDate;
    var csDueAmt = 0M;
    var workerId = local.ServiceProvider.UserId;
    var judicialDistrict = local.DashboardAuditData.JudicialDistrict ?? "";
    var contractorNumber = local.Contractor.Code;

    entities.DashboardStagingPriority4.Populated = false;
    Update("CreateDashboardStagingPriority4",
      (db, command) =>
      {
        db.SetInt32(command, "reportMonth", reportMonth);
        db.SetInt32(command, "runNumber", runNumber);
        db.SetString(command, "caseNumber", caseNumber);
        db.SetNullableDate(command, "asOfDate", asOfDate);
        db.SetNullableString(command, "currentCsInd", "");
        db.SetNullableString(command, "otherObgInd", "");
        db.SetNullableDecimal(command, "csDueAmt", csDueAmt);
        db.SetNullableDecimal(command, "csCollectedAmt", csDueAmt);
        db.SetNullableString(command, "payingCaseInd", "");
        db.SetNullableString(command, "paternityEstInd", "");
        db.SetNullableString(command, "addressVerInd", "");
        db.SetNullableString(command, "employerVerInd", "");
        db.SetString(command, "workerId", workerId);
        db.SetString(command, "judicialDistrict", judicialDistrict);
        db.SetNullableString(command, "contractorNum", contractorNumber);
      });

    entities.DashboardStagingPriority4.ReportMonth = reportMonth;
    entities.DashboardStagingPriority4.RunNumber = runNumber;
    entities.DashboardStagingPriority4.CaseNumber = caseNumber;
    entities.DashboardStagingPriority4.AsOfDate = asOfDate;
    entities.DashboardStagingPriority4.CurrentCsInd = "";
    entities.DashboardStagingPriority4.OtherObgInd = "";
    entities.DashboardStagingPriority4.CsDueAmt = csDueAmt;
    entities.DashboardStagingPriority4.CsCollectedAmt = csDueAmt;
    entities.DashboardStagingPriority4.PayingCaseInd = "";
    entities.DashboardStagingPriority4.PaternityEstInd = "";
    entities.DashboardStagingPriority4.AddressVerInd = "";
    entities.DashboardStagingPriority4.EmployerVerInd = "";
    entities.DashboardStagingPriority4.WorkerId = workerId;
    entities.DashboardStagingPriority4.JudicialDistrict = judicialDistrict;
    entities.DashboardStagingPriority4.ContractorNumber = contractorNumber;
    entities.DashboardStagingPriority4.Populated = true;
  }

  private IEnumerable<bool> ReadCaseCaseAssignmentServiceProvider()
  {
    return ReadEachInSeparateTransaction(
      "ReadCaseCaseAssignmentServiceProvider",
      (db, command) =>
      {
        db.SetDate(command, "effectiveDate", import.ReportEndDate.Date);
        db.SetString(command, "numb", local.Restart.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.CaseAssignment.CasNo = db.GetString(reader, 0);
        entities.Case1.NoJurisdictionCd = db.GetNullableString(reader, 1);
        entities.CaseAssignment.EffectiveDate = db.GetDate(reader, 2);
        entities.CaseAssignment.DiscontinueDate = db.GetNullableDate(reader, 3);
        entities.CaseAssignment.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.CaseAssignment.SpdId = db.GetInt32(reader, 5);
        entities.CaseAssignment.OffId = db.GetInt32(reader, 6);
        entities.CaseAssignment.OspCode = db.GetString(reader, 7);
        entities.CaseAssignment.OspDate = db.GetDate(reader, 8);
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 9);
        entities.ServiceProvider.UserId = db.GetString(reader, 10);
        entities.Case1.Populated = true;
        entities.CaseAssignment.Populated = true;
        entities.ServiceProvider.Populated = db.GetNullableInt32(reader, 9) != null
          ;

        return true;
      },
      () =>
      {
        entities.ServiceProvider.Populated = false;
        entities.Case1.Populated = false;
        entities.CaseAssignment.Populated = false;
      });
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
    /// A value of Contractor.
    /// </summary>
    public CseOrganization Contractor
    {
      get => contractor ??= new();
      set => contractor = value;
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
    /// A value of Restart.
    /// </summary>
    public Case1 Restart
    {
      get => restart ??= new();
      set => restart = value;
    }

    /// <summary>
    /// A value of Prev.
    /// </summary>
    public Case1 Prev
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

    private CseOrganization? contractor;
    private ServiceProvider? serviceProvider;
    private DashboardAuditData? dashboardAuditData;
    private ProgramCheckpointRestart? programCheckpointRestart;
    private Case1? restart;
    private Case1? prev;
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

    /// <summary>
    /// A value of CaseAssignment.
    /// </summary>
    public CaseAssignment CaseAssignment
    {
      get => caseAssignment ??= new();
      set => caseAssignment = value;
    }

    private ServiceProvider? serviceProvider;
    private OfficeServiceProvider? officeServiceProvider;
    private DashboardStagingPriority4? dashboardStagingPriority4;
    private Case1? case1;
    private CaseAssignment? caseAssignment;
  }
#endregion
}
