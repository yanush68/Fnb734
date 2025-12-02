// Program: FN_B734_DETER_CONTRACTOR_FROM_JD, ID: 1902433565, model: 746.
// Short name: SWE03732
using System;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B734_DETER_CONTRACTOR_FROM_JD.
/// </summary>
[Serializable]
[Program("SWE03732")]
public partial class FnB734DeterContractorFromJd: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B734_DETER_CONTRACTOR_FROM_JD program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB734DeterContractorFromJd(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB734DeterContractorFromJd.
  /// </summary>
  public FnB734DeterContractorFromJd(IContext context, Import import,
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
    if (ReadContractorHistoryCseOrganizationCseOrganization())
    {
      MoveCseOrganization(entities.Contractor, export.Contractor);
    }

    if (IsEmpty(export.Contractor.Code))
    {
      // --  Write to error report...
      UseCabDate2TextWithHyphens();
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "FN_B734_DETER_CONTRACTOR_FROM_JD - Error finding contractor for judical district " +
        String(import.DashboardAuditData.JudicialDistrict ?? "", 2) + ".  Rpt End Date = " +
        String(local.TextWorkArea.Text10, TextWorkArea.Text10_MaxLength);
      UseCabErrorReport();
    }
  }

  private static void MoveCseOrganization(CseOrganization source,
    CseOrganization target)
  {
    target.Code = source.Code;
    target.Name = source.Name;
  }

  private void UseCabDate2TextWithHyphens()
  {
    var useImport = new CabDate2TextWithHyphens.Import();
    var useExport = new CabDate2TextWithHyphens.Export();

    useImport.DateWorkArea.Date = import.ReportEndDate.Date;

    context.Call(CabDate2TextWithHyphens.Execute, useImport, useExport);

    local.TextWorkArea.Text10 = useExport.TextWorkArea.Text10;
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

  private bool ReadContractorHistoryCseOrganizationCseOrganization()
  {
    entities.ContractorHistory.Populated = false;
    entities.Contractor.Populated = false;
    entities.JudicalDistrict.Populated = false;

    return Read("ReadContractorHistoryCseOrganizationCseOrganization",
      (db, command) =>
      {
        db.SetNullableDate(command, "endDate", import.ReportEndDate.Date);
        db.SetString(
          command, "fk0CktCseOrgaorganztnId",
          import.DashboardAuditData.JudicialDistrict ?? "");
      },
      (db, reader) =>
      {
        entities.ContractorHistory.Identifier = db.GetInt32(reader, 0);
        entities.ContractorHistory.EffectiveDate =
          db.GetNullableDate(reader, 1);
        entities.ContractorHistory.EndDate = db.GetNullableDate(reader, 2);
        entities.ContractorHistory.FkCktCseOrgatypeCode =
          db.GetString(reader, 3);
        entities.Contractor.Type1 = db.GetString(reader, 3);
        entities.ContractorHistory.FkCktCseOrgaorganztnId =
          db.GetString(reader, 4);
        entities.Contractor.Code = db.GetString(reader, 4);
        entities.ContractorHistory.Fk0CktCseOrgatypeCode =
          db.GetString(reader, 5);
        entities.JudicalDistrict.Type1 = db.GetString(reader, 5);
        entities.ContractorHistory.Fk0CktCseOrgaorganztnId =
          db.GetString(reader, 6);
        entities.JudicalDistrict.Code = db.GetString(reader, 6);
        entities.Contractor.Name = db.GetString(reader, 7);
        entities.ContractorHistory.Populated = true;
        entities.JudicalDistrict.Populated = true;
        entities.Contractor.Populated = true;
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
    /// A value of DashboardAuditData.
    /// </summary>
    public DashboardAuditData DashboardAuditData
    {
      get => dashboardAuditData ??= new();
      set => dashboardAuditData = value;
    }

    /// <summary>
    /// A value of ReportEndDate.
    /// </summary>
    public DateWorkArea ReportEndDate
    {
      get => reportEndDate ??= new();
      set => reportEndDate = value;
    }

    private DashboardAuditData? dashboardAuditData;
    private DateWorkArea? reportEndDate;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Contractor.
    /// </summary>
    public CseOrganization Contractor
    {
      get => contractor ??= new();
      set => contractor = value;
    }

    private CseOrganization? contractor;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of TextWorkArea.
    /// </summary>
    public TextWorkArea TextWorkArea
    {
      get => textWorkArea ??= new();
      set => textWorkArea = value;
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

    private TextWorkArea? textWorkArea;
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
    /// A value of ContractorHistory.
    /// </summary>
    public ContractorHistory ContractorHistory
    {
      get => contractorHistory ??= new();
      set => contractorHistory = value;
    }

    /// <summary>
    /// A value of Contractor.
    /// </summary>
    public CseOrganization Contractor
    {
      get => contractor ??= new();
      set => contractor = value;
    }

    /// <summary>
    /// A value of JudicalDistrict.
    /// </summary>
    public CseOrganization JudicalDistrict
    {
      get => judicalDistrict ??= new();
      set => judicalDistrict = value;
    }

    /// <summary>
    /// A value of CseOrganizationRelationship.
    /// </summary>
    public CseOrganizationRelationship CseOrganizationRelationship
    {
      get => cseOrganizationRelationship ??= new();
      set => cseOrganizationRelationship = value;
    }

    private ContractorHistory? contractorHistory;
    private CseOrganization? contractor;
    private CseOrganization? judicalDistrict;
    private CseOrganizationRelationship? cseOrganizationRelationship;
  }
#endregion
}
