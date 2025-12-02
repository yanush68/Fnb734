// Program: FN_B734_CREATE_DASHBOARD_AUDIT, ID: 945118847, model: 746.
// Short name: SWE03083
using System;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B734_CREATE_DASHBOARD_AUDIT.
/// </summary>
[Serializable]
[Program("SWE03083")]
public partial class FnB734CreateDashboardAudit: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B734_CREATE_DASHBOARD_AUDIT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB734CreateDashboardAudit(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB734CreateDashboardAudit.
  /// </summary>
  public FnB734CreateDashboardAudit(IContext context, Import import,
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
    for(var i = 0; i < 5; ++i)
    {
      ExitState = "ACO_NN0000_ALL_OK";

      try
      {
        CreateDashboardAuditData();

        break;
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "DASHBOARD_AUDIT_AE";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "DASHBOARD_AUDIT_PV";

            break;
          default:
            throw;
        }
      }
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      UseEabExtractExitStateMessage();
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = local.ExitStateWorkArea.Message;
      UseCabErrorReport();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }
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

  private void UseEabExtractExitStateMessage()
  {
    var useImport = new EabExtractExitStateMessage.Import();
    var useExport = new EabExtractExitStateMessage.Export();

    useExport.ExitStateWorkArea.Message = local.ExitStateWorkArea.Message;

    context.Call(EabExtractExitStateMessage.Execute, useImport, useExport);

    local.ExitStateWorkArea.Message = useExport.ExitStateWorkArea.Message;
  }

  private void CreateDashboardAuditData()
  {
    var reportMonth = import.DashboardAuditData.ReportMonth;
    var dashboardPriority = import.DashboardAuditData.DashboardPriority;
    var runNumber = import.DashboardAuditData.RunNumber;
    var createdTimestamp = Now();
    var office = import.DashboardAuditData.Office ?? 0;
    var judicialDistrict = import.DashboardAuditData.JudicialDistrict ?? "";
    var workerId = import.DashboardAuditData.WorkerId ?? "";
    var caseNumber = import.DashboardAuditData.CaseNumber ?? "";
    var standardNumber = import.DashboardAuditData.StandardNumber ?? "";
    var payorCspNumber = import.DashboardAuditData.PayorCspNumber ?? "";
    var suppCspNumber = import.DashboardAuditData.SuppCspNumber ?? "";
    var fte = import.DashboardAuditData.Fte ?? 0;
    var collectionAmount = import.DashboardAuditData.CollectionAmount ?? 0M;
    var collAppliedToCd = import.DashboardAuditData.CollAppliedToCd ?? "";
    var collectionCreatedDate = import.DashboardAuditData.CollectionCreatedDate;
    var collectionType = import.DashboardAuditData.CollectionType ?? "";
    var debtBalanceDue = import.DashboardAuditData.DebtBalanceDue ?? 0M;
    var debtDueDate = import.DashboardAuditData.DebtDueDate;
    var debtType = import.DashboardAuditData.DebtType ?? "";
    var legalActionDate = import.DashboardAuditData.LegalActionDate;
    var legalReferralDate = import.DashboardAuditData.LegalReferralDate;
    var legalReferralNumber = import.DashboardAuditData.LegalReferralNumber ?? 0
      ;
    var daysReported = import.DashboardAuditData.DaysReported ?? 0;
    var verifiedDate = import.DashboardAuditData.VerifiedDate;
    var caseDate = import.DashboardAuditData.CaseDate;
    var reviewDate = import.DashboardAuditData.ReviewDate;
    var contractorNumber = import.DashboardAuditData.ContractorNumber ?? "";

    entities.DashboardAuditData.Populated = false;
    Update("CreateDashboardAuditData",
      (db, command) =>
      {
        db.SetInt32(command, "reportMonth", reportMonth);
        db.SetString(command, "dashboardPriority", dashboardPriority);
        db.SetInt32(command, "runNumber", runNumber);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableInt32(command, "office", office);
        db.SetNullableString(command, "judicialDistrict", judicialDistrict);
        db.SetNullableString(command, "workerId", workerId);
        db.SetNullableString(command, "caseNumber", caseNumber);
        db.SetNullableString(command, "standardNumber", standardNumber);
        db.SetNullableString(command, "payorCspNumber", payorCspNumber);
        db.SetNullableString(command, "suppCspNumber", suppCspNumber);
        db.SetNullableInt32(command, "fte", fte);
        db.SetNullableDecimal(command, "collectionAmt", collectionAmount);
        db.SetNullableString(command, "collAppliedToCd", collAppliedToCd);
        db.SetNullableDate(command, "collCreatedDt", collectionCreatedDate);
        db.SetNullableString(command, "collType", collectionType);
        db.SetNullableDecimal(command, "debtBalanceDue", debtBalanceDue);
        db.SetNullableDate(command, "debtDueDt", debtDueDate);
        db.SetNullableString(command, "debtType", debtType);
        db.SetNullableDate(command, "legalActionDt", legalActionDate);
        db.SetNullableDate(command, "legalRefDt", legalReferralDate);
        db.SetNullableInt32(command, "legalRefNo", legalReferralNumber);
        db.SetNullableInt32(command, "daysReported", daysReported);
        db.SetNullableDate(command, "verifiedDt", verifiedDate);
        db.SetNullableDate(command, "caseDt", caseDate);
        db.SetNullableDate(command, "reviewDt", reviewDate);
        db.SetNullableString(command, "contractorNum", contractorNumber);
      });

    entities.DashboardAuditData.ReportMonth = reportMonth;
    entities.DashboardAuditData.DashboardPriority = dashboardPriority;
    entities.DashboardAuditData.RunNumber = runNumber;
    entities.DashboardAuditData.CreatedTimestamp = createdTimestamp;
    entities.DashboardAuditData.Office = office;
    entities.DashboardAuditData.JudicialDistrict = judicialDistrict;
    entities.DashboardAuditData.WorkerId = workerId;
    entities.DashboardAuditData.CaseNumber = caseNumber;
    entities.DashboardAuditData.StandardNumber = standardNumber;
    entities.DashboardAuditData.PayorCspNumber = payorCspNumber;
    entities.DashboardAuditData.SuppCspNumber = suppCspNumber;
    entities.DashboardAuditData.Fte = fte;
    entities.DashboardAuditData.CollectionAmount = collectionAmount;
    entities.DashboardAuditData.CollAppliedToCd = collAppliedToCd;
    entities.DashboardAuditData.CollectionCreatedDate = collectionCreatedDate;
    entities.DashboardAuditData.CollectionType = collectionType;
    entities.DashboardAuditData.DebtBalanceDue = debtBalanceDue;
    entities.DashboardAuditData.DebtDueDate = debtDueDate;
    entities.DashboardAuditData.DebtType = debtType;
    entities.DashboardAuditData.LegalActionDate = legalActionDate;
    entities.DashboardAuditData.LegalReferralDate = legalReferralDate;
    entities.DashboardAuditData.LegalReferralNumber = legalReferralNumber;
    entities.DashboardAuditData.DaysReported = daysReported;
    entities.DashboardAuditData.VerifiedDate = verifiedDate;
    entities.DashboardAuditData.CaseDate = caseDate;
    entities.DashboardAuditData.ReviewDate = reviewDate;
    entities.DashboardAuditData.ContractorNumber = contractorNumber;
    entities.DashboardAuditData.Populated = true;
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
    /// A value of ExitStateWorkArea.
    /// </summary>
    public ExitStateWorkArea ExitStateWorkArea
    {
      get => exitStateWorkArea ??= new();
      set => exitStateWorkArea = value;
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

    private ExitStateWorkArea? exitStateWorkArea;
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
    /// A value of DashboardAuditData.
    /// </summary>
    public DashboardAuditData DashboardAuditData
    {
      get => dashboardAuditData ??= new();
      set => dashboardAuditData = value;
    }

    private DashboardAuditData? dashboardAuditData;
  }
#endregion
}
