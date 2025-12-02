// Program: FN_B734_DETERMINE_JD_FROM_CASE, ID: 945118846, model: 746.
// Short name: SWE03082
using System;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B734_DETERMINE_JD_FROM_CASE.
/// </summary>
[Serializable]
[Program("SWE03082")]
public partial class FnB734DetermineJdFromCase: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B734_DETERMINE_JD_FROM_CASE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB734DetermineJdFromCase(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB734DetermineJdFromCase.
  /// </summary>
  public FnB734DetermineJdFromCase(IContext context, Import import,
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
    export.DashboardAuditData.CaseNumber = import.Case1.Number;

    if (ReadCseOrganizationOffice())
    {
      export.DashboardAuditData.JudicialDistrict =
        entities.JudicialDistrict.Code;
      export.DashboardAuditData.Office = entities.Office.SystemGeneratedId;
    }
    else
    {
      // --  Write to error report...
      UseCabDate2TextWithHyphens();
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "FN_B734_DETERMINE_JD_FROM_CASE - Error finding office/judicial district for case " +
        String(import.Case1.Number, Case1.Number_MaxLength) + ".  Rpt End Date = " +
        String(local.TextWorkArea.Text10, TextWorkArea.Text10_MaxLength);
      UseCabErrorReport();
    }
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

  private bool ReadCseOrganizationOffice()
  {
    entities.JudicialDistrict.Populated = false;
    entities.Office.Populated = false;

    return Read("ReadCseOrganizationOffice",
      (db, command) =>
      {
        db.SetDate(command, "effectiveDate", import.ReportEndDate.Date);
        db.SetString(command, "casNo", import.Case1.Number);
      },
      (db, reader) =>
      {
        entities.JudicialDistrict.Code = db.GetString(reader, 0);
        entities.JudicialDistrict.Type1 = db.GetString(reader, 1);
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 2);
        entities.Office.CogTypeCode = db.GetNullableString(reader, 3);
        entities.Office.CogCode = db.GetNullableString(reader, 4);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 5);
        entities.JudicialDistrict.Populated = true;
        entities.Office.Populated = true;
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
    /// A value of Case1.
    /// </summary>
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of ReportEndDate.
    /// </summary>
    public DateWorkArea ReportEndDate
    {
      get => reportEndDate ??= new();
      set => reportEndDate = value;
    }

    private Case1? case1;
    private DateWorkArea? reportEndDate;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of DashboardAuditData.
    /// </summary>
    public DashboardAuditData DashboardAuditData
    {
      get => dashboardAuditData ??= new();
      set => dashboardAuditData = value;
    }

    private DashboardAuditData? dashboardAuditData;
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
    /// A value of Case1.
    /// </summary>
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
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
    /// A value of JudicialDistrict.
    /// </summary>
    public CseOrganization JudicialDistrict
    {
      get => judicialDistrict ??= new();
      set => judicialDistrict = value;
    }

    /// <summary>
    /// A value of County.
    /// </summary>
    public CseOrganization County
    {
      get => county ??= new();
      set => county = value;
    }

    /// <summary>
    /// A value of CseOrganizationRelationship.
    /// </summary>
    public CseOrganizationRelationship CseOrganizationRelationship
    {
      get => cseOrganizationRelationship ??= new();
      set => cseOrganizationRelationship = value;
    }

    /// <summary>
    /// A value of Office.
    /// </summary>
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    /// <summary>
    /// A value of CaseAssignment.
    /// </summary>
    public CaseAssignment CaseAssignment
    {
      get => caseAssignment ??= new();
      set => caseAssignment = value;
    }

    private Case1? case1;
    private OfficeServiceProvider? officeServiceProvider;
    private CseOrganization? judicialDistrict;
    private CseOrganization? county;
    private CseOrganizationRelationship? cseOrganizationRelationship;
    private Office? office;
    private CaseAssignment? caseAssignment;
  }
#endregion
}
