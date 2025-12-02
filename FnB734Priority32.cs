// Program: FN_B734_PRIORITY_3_2, ID: 945148925, model: 746.
// Short name: SWE03693
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
/// A program: FN_B734_PRIORITY_3_2.
/// </para>
/// <para>
/// Priority 3-2: Staffing
/// </para>
/// </summary>
[Serializable]
[Program("SWE03693")]
public partial class FnB734Priority32: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B734_PRIORITY_3_2 program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB734Priority32(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB734Priority32.
  /// </summary>
  public FnB734Priority32(IContext context, Import import, Export export):
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
    // Priority 3-2: Staffing
    // -------------------------------------------------------------------------------------
    // This will be manually entered  the data source will be Ashley Dexters 
    // FTE Report.  Will point to the excel spreadsheet.
    // Report Level: State, Judicial District, Region, Office
    // Report Period: Month
    // 1)	Location of the spreadsheet must be static.
    // 2)	Format of the spreadsheet must be static.
    // Question for Ashley:
    // How often is the spreadsheet updated?  The assumption is Monthly.
    // -------------------------------------------------------------------------------------
    MoveProgramCheckpointRestart(import.ProgramCheckpointRestart,
      local.ProgramCheckpointRestart);
    MoveDashboardAuditData2(import.DashboardAuditData, local.Initialized);

    foreach(var _ in ReadCseOrganization())
    {
      if (Verify(entities.JudicialDistrict.Code, "0123456789") != 0)
      {
        continue;
      }

      local.Initialized.JudicialDistrict = "";
      local.Initialized.Office = 0;
      local.DashboardAuditData.Assign(local.Initialized);

      local.Fte.Index = (int)StringToNumber(entities.JudicialDistrict.Code) - 1;
      local.Fte.CheckSize();

      local.DashboardStagingPriority35.ReportLevelId =
        NumberToString(local.Fte.Index + 1, 14, 2);

      foreach(var _1 in ReadOfficeOfficeStaffing())
      {
        if (entities.OfficeStaffing.Populated)
        {
          local.Fte.Update.G.FullTimeEquivalent =
            (local.Fte.Item.G.FullTimeEquivalent ?? 0M) + (
              entities.OfficeStaffing.FullTimeEquivalent ?? 0M);
          local.DashboardAuditData.Office = entities.Office2.SystemGeneratedId;
          local.DashboardAuditData.DashboardPriority = "3-2";
          local.DashboardAuditData.Fte =
            (int?)entities.OfficeStaffing.FullTimeEquivalent;
          local.DashboardAuditData.JudicialDistrict =
            local.DashboardStagingPriority35.ReportLevelId;

          if (AsChar(import.AuditFlag.Flag) == 'Y')
          {
            // -- Log to the dashboard audit table.
            UseFnB734CreateDashboardAudit();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }
          }
        }
      }
    }

    // -- Save Judicial District counts.
    for(local.Fte.Index = 0; local.Fte.Index < local.Fte.Count; ++
      local.Fte.Index)
    {
      if (!local.Fte.CheckSize())
      {
        break;
      }

      local.DashboardStagingPriority35.ReportLevelId =
        NumberToString(local.Fte.Index + 1, 14, 2);

      if (ReadDashboardStagingPriority35())
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

    local.Fte.CheckIndex();

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
    local.ProgramCheckpointRestart.RestartInd = "Y";
    local.ProgramCheckpointRestart.RestartInfo = "";
    local.ProgramCheckpointRestart.RestartInfo =
      Substring(import.ProgramCheckpointRestart.RestartInfo, 250, 1, 80) + "3-03     ";
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

  private void UseUpdateCheckpointRstAndCommit()
  {
    var useImport = new UpdateCheckpointRstAndCommit.Import();
    var useExport = new UpdateCheckpointRstAndCommit.Export();

    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);

    context.Call(UpdateCheckpointRstAndCommit.Execute, useImport, useExport);
  }

  private void CreateDashboardStagingPriority35()
  {
    var reportMonth = import.DashboardAuditData.ReportMonth;
    var reportLevel = "JD";
    var reportLevelId = local.DashboardStagingPriority35.ReportLevelId;
    var asOfDate = import.ProgramProcessingInfo.ProcessDate;
    var fullTimeEquivalent = 0M;

    entities.DashboardStagingPriority35.Populated = false;
    Update("CreateDashboardStagingPriority35",
      (db, command) =>
      {
        db.SetInt32(command, "reportMonth", reportMonth);
        db.SetString(command, "reportLevel", reportLevel);
        db.SetString(command, "reportLevelId", reportLevelId);
        db.SetNullableDate(command, "asOfDate", asOfDate);
        db.SetNullableInt32(command, "casWEstRef", 0);
        db.SetNullableDecimal(command, "fullTimeEqvlnt", fullTimeEquivalent);
        db.SetNullableDecimal(command, "STypeCollAmt", fullTimeEquivalent);
        db.SetNullableDecimal(command, "STypeCollPer", fullTimeEquivalent);
      });

    entities.DashboardStagingPriority35.ReportMonth = reportMonth;
    entities.DashboardStagingPriority35.ReportLevel = reportLevel;
    entities.DashboardStagingPriority35.ReportLevelId = reportLevelId;
    entities.DashboardStagingPriority35.AsOfDate = asOfDate;
    entities.DashboardStagingPriority35.FullTimeEquivalent = fullTimeEquivalent;
    entities.DashboardStagingPriority35.Populated = true;
  }

  private IEnumerable<bool> ReadCseOrganization()
  {
    return ReadEach("ReadCseOrganization",
      null,
      (db, reader) =>
      {
        entities.JudicialDistrict.Code = db.GetString(reader, 0);
        entities.JudicialDistrict.Type1 = db.GetString(reader, 1);
        entities.JudicialDistrict.Name = db.GetString(reader, 2);
        entities.JudicialDistrict.Populated = true;

        return true;
      },
      () =>
      {
        entities.JudicialDistrict.Populated = false;
      });
  }

  private bool ReadDashboardStagingPriority35()
  {
    entities.DashboardStagingPriority35.Populated = false;

    return Read("ReadDashboardStagingPriority35",
      (db, command) =>
      {
        db.SetInt32(
          command, "reportMonth", import.DashboardAuditData.ReportMonth);
        db.SetString(
          command, "reportLevelId",
          local.DashboardStagingPriority35.ReportLevelId);
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
        entities.DashboardStagingPriority35.FullTimeEquivalent =
          db.GetNullableDecimal(reader, 4);
        entities.DashboardStagingPriority35.Populated = true;
      });
  }

  private IEnumerable<bool> ReadOfficeOfficeStaffing()
  {
    return ReadEach("ReadOfficeOfficeStaffing",
      (db, command) =>
      {
        db.SetDate(command, "effectiveDate", import.ReportEndDate.Date);
        db.SetString(command, "cogChildType", entities.JudicialDistrict.Type1);
        db.SetString(command, "cogChildCode", entities.JudicialDistrict.Code);
        db.
          SetInt32(command, "yearMonth", import.DashboardAuditData.ReportMonth);
      },
      (db, reader) =>
      {
        entities.Office2.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.Office2.CogTypeCode = db.GetNullableString(reader, 1);
        entities.Office2.CogCode = db.GetNullableString(reader, 2);
        entities.Office2.EffectiveDate = db.GetDate(reader, 3);
        entities.Office2.DiscontinueDate = db.GetNullableDate(reader, 4);
        entities.Office2.OffOffice = db.GetNullableInt32(reader, 5);
        entities.OfficeStaffing.YearMonth = db.GetInt32(reader, 6);
        entities.OfficeStaffing.FullTimeEquivalent =
          db.GetNullableDecimal(reader, 7);
        entities.OfficeStaffing.OffGeneratedId = db.GetInt32(reader, 8);
        entities.Office2.Populated = true;
        entities.OfficeStaffing.Populated = db.GetNullableInt32(reader, 6) != null
          ;

        return true;
      },
      () =>
      {
        entities.Office2.Populated = false;
        entities.OfficeStaffing.Populated = false;
      });
  }

  private void UpdateDashboardStagingPriority35()
  {
    var asOfDate = import.ProgramProcessingInfo.ProcessDate;
    var fullTimeEquivalent = local.Fte.Item.G.FullTimeEquivalent ?? 0M;

    entities.DashboardStagingPriority35.Populated = false;
    Update("UpdateDashboardStagingPriority35",
      (db, command) =>
      {
        db.SetNullableDate(command, "asOfDate", asOfDate);
        db.SetNullableDecimal(command, "fullTimeEqvlnt", fullTimeEquivalent);
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
    entities.DashboardStagingPriority35.FullTimeEquivalent = fullTimeEquivalent;
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
    /// A value of ProgramProcessingInfo.
    /// </summary>
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    /// <summary>
    /// A value of AuditFlag.
    /// </summary>
    public Common AuditFlag
    {
      get => auditFlag ??= new();
      set => auditFlag = value;
    }

    private ProgramCheckpointRestart? programCheckpointRestart;
    private DashboardAuditData? dashboardAuditData;
    private DateWorkArea? reportStartDate;
    private DateWorkArea? reportEndDate;
    private ProgramProcessingInfo? programProcessingInfo;
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
    /// <summary>A FteGroup group.</summary>
    [Serializable]
    public class FteGroup
    {
      /// <summary>
      /// A value of G.
      /// </summary>
      public OfficeStaffing G
      {
        get => g ??= new();
        set => g = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private OfficeStaffing? g;
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
    /// Gets a value of Fte.
    /// </summary>
    [JsonIgnore]
    public Array<FteGroup> Fte => fte ??= new(FteGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Fte for json serialization.
    /// </summary>
    [JsonPropertyName("fte")]
    [Computed]
    public IList<FteGroup>? Fte_Json
    {
      get => fte;
      set => Fte.Assign(value);
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
    /// A value of Case1.
    /// </summary>
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of DashboardAuditData.
    /// </summary>
    public DashboardAuditData DashboardAuditData
    {
      get => dashboardAuditData ??= new();
      set => dashboardAuditData = value;
    }

    private DashboardStagingPriority35? dashboardStagingPriority35;
    private Array<FteGroup>? fte;
    private ProgramCheckpointRestart? programCheckpointRestart;
    private DashboardAuditData? initialized;
    private EabFileHandling? eabFileHandling;
    private EabReportSend? eabReportSend;
    private Case1? case1;
    private DashboardAuditData? dashboardAuditData;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of CseOrganizationRelationship.
    /// </summary>
    public CseOrganizationRelationship CseOrganizationRelationship
    {
      get => cseOrganizationRelationship ??= new();
      set => cseOrganizationRelationship = value;
    }

    /// <summary>
    /// A value of Office1.
    /// </summary>
    public CseOrganization Office1
    {
      get => office1 ??= new();
      set => office1 = value;
    }

    /// <summary>
    /// A value of JudicialDistrict.
    /// </summary>
    public CseOrganization JudicialDistrict
    {
      get => judicialDistrict ??= new();
      set => judicialDistrict = value;
    }

    /// <summary>
    /// A value of Office2.
    /// </summary>
    public Office Office2
    {
      get => office2 ??= new();
      set => office2 = value;
    }

    /// <summary>
    /// A value of OfficeStaffing.
    /// </summary>
    public OfficeStaffing OfficeStaffing
    {
      get => officeStaffing ??= new();
      set => officeStaffing = value;
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
    /// A value of DashboardStagingPriority35.
    /// </summary>
    public DashboardStagingPriority35 DashboardStagingPriority35
    {
      get => dashboardStagingPriority35 ??= new();
      set => dashboardStagingPriority35 = value;
    }

    private CseOrganizationRelationship? cseOrganizationRelationship;
    private CseOrganization? office1;
    private CseOrganization? judicialDistrict;
    private Office? office2;
    private OfficeStaffing? officeStaffing;
    private CseOrganization? cseOrganization;
    private DashboardStagingPriority35? dashboardStagingPriority35;
  }
#endregion
}
