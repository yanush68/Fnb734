// Program: FN_B734_PRI_4_PYRAMID_REPORT, ID: 945237040, model: 746.
// Short name: SWE03724
using System;
using System.Collections.Generic;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_B734_PRI_4_PYRAMID_REPORT.
/// </para>
/// <para>
/// Priority 4 - Pyramid Report
/// </para>
/// </summary>
[Serializable]
[Program("SWE03724")]
public partial class FnB734Pri4PyramidReport: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B734_PRI_4_PYRAMID_REPORT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB734Pri4PyramidReport(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB734Pri4PyramidReport.
  /// </summary>
  public FnB734Pri4PyramidReport(IContext context, Import import, Export export):
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
    // ---------------------------------------------------------------------------------------------------
    // -------------------------------------------------------------------------------------
    // PRIORITY 4 - Pyramid Report
    // -------------------------------------------------------------------------------------
    // Priority 4 is a Statewide snapshot of all cases open as of the refresh 
    // date.  All counts are case specific.  All statuses are as of the refresh
    // date.  Priority 4 is reported at the Statewide Level only.  The report is
    // produced monthly.
    // Each case is counted once and only once per tier.
    // The total count of all elements in each tier should equal the total 
    // number of cases reported in Tier1.  **Exception is Tier5***
    // -------------------------------------------------------------------------------------
    // -- Only process the Pyramid Report when the report period is through the 
    // end of the month.
    if (Month(import.ReportEndDate.Date) == Month
      (AddDays(import.ReportEndDate.Date, 1)))
    {
      return;
    }

    // -------------------------------------------------------------------------------------
    // Priority 4-1: Tier 1 - Total Number of Cases
    // -------------------------------------------------------------------------------------
    if (!Lt("4-01", import.Restart.DashboardPriority) && !
      Lt("4-01", import.Start.DashboardPriority) && !
      Lt(import.End.DashboardPriority, "4-01"))
    {
      UseFnB734Pri4Level1();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // -------------------------------------------------------------------------------------
    // Priority 4-2: Tier 2.1 - Cases with Current Child Support Owed,
    // 	      Tier 2.2 - Cases with Any Obligation Other than Current Child 
    // Support,
    // 	      Tier 2.3 - Cases with No Obligation
    // -------------------------------------------------------------------------------------
    if (!Lt("4-02", import.Restart.DashboardPriority) && !
      Lt("4-02", import.Start.DashboardPriority) && !
      Lt(import.End.DashboardPriority, "4-02"))
    {
      UseFnB734Pri4Level2();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // -------------------------------------------------------------------------------------
    // Priority 4-3: Tier 3.1 - Paying Cases (Current Child Support Owed),
    // 	      Tier 3.2 - Non Paying Cases (Current Child Support Owed),
    // 	      Tier 3.3 - Paternity Cases (No Obligation),
    // 	      Tier 3.4 - Non Paternity Cases (No Obligation)
    // -------------------------------------------------------------------------------------
    if (!Lt("4-03", import.Restart.DashboardPriority) && !
      Lt("4-03", import.Start.DashboardPriority) && !
      Lt(import.End.DashboardPriority, "4-03"))
    {
      UseFnB734Pri4Level3();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // -------------------------------------------------------------------------------------
    // Priority 4-4: Tier 4.1 - Cases with No Address or Employer,
    // 	      Tier 4.2 - Cases with Active Address Only,
    // 	      Tier 4.3 - Cases with Active Employer Only,
    // 	      Tier 4.4 - Cases with Both an Active Address AND an Active 
    // Employer
    // 	      Tier 5.1.1 - Verified Address
    // 		(Subset of Cases with Active Address Only)
    // 	      Tier 5.1.2 - Non Verified Address
    // 		(Subset of Cases with Active Address Only)
    // 	      Tier 5.2.1 - Verified Employer
    // 		(Subset of Cases with Active Employer Only)
    // 	      Tier 5.2.2 - Non Verified Employer
    // 		(Subset of Cases with Active Employer Only)
    // 	      Tier 5.3.1 - Verified Address and Employer
    // 		(Subset of Cases with Both an Active Address and an Active Employer)
    // 	      Tier 5.3.2 - Non Verified Address or Employer
    // 		(Subset of Cases with Both an Active Address and an Active Employer)
    // -------------------------------------------------------------------------------------
    if (!Lt("4-04", import.Restart.DashboardPriority) && !
      Lt("4-04", import.Start.DashboardPriority) && !
      Lt(import.End.DashboardPriority, "4-04"))
    {
      UseFnB734Pri4Level4And5();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    local.DashboardPyramid.AsOfDate = import.ProgramProcessingInfo.ProcessDate;
    local.DashboardPyramid.ReportMonth = import.DashboardAuditData.ReportMonth;
    local.DashboardPyramid.RunNumber = import.DashboardAuditData.RunNumber;

    // -- Build the Dashboard_Pyramid record from the summarized priority 4 
    // staging records.
    foreach(var _ in ReadDashboardStagingPriority4())
    {
      local.DashboardPyramid.TotalCases =
        (local.DashboardPyramid.TotalCases ?? 0) + 1;

      if (AsChar(entities.DashboardStagingPriority4.CurrentCsInd) == 'Y')
      {
        local.DashboardPyramid.CsCases =
          (local.DashboardPyramid.CsCases ?? 0) + 1;

        if (AsChar(entities.DashboardStagingPriority4.PayingCaseInd) == 'Y')
        {
          local.DashboardPyramid.PayingCases =
            (local.DashboardPyramid.PayingCases ?? 0) + 1;
        }
        else
        {
          local.DashboardPyramid.NopayCases =
            (local.DashboardPyramid.NopayCases ?? 0) + 1;

          if (IsEmpty(entities.DashboardStagingPriority4.AddressVerInd) && IsEmpty
            (entities.DashboardStagingPriority4.EmployerVerInd))
          {
            local.DashboardPyramid.NopayNoaddemp =
              (local.DashboardPyramid.NopayNoaddemp ?? 0) + 1;
          }
          else if (!IsEmpty(entities.DashboardStagingPriority4.AddressVerInd) &&
            !IsEmpty(entities.DashboardStagingPriority4.EmployerVerInd))
          {
            local.DashboardPyramid.NopayAddEmp =
              (local.DashboardPyramid.NopayAddEmp ?? 0) + 1;

            if (AsChar(entities.DashboardStagingPriority4.AddressVerInd) == 'Y'
              && AsChar(entities.DashboardStagingPriority4.EmployerVerInd) == 'Y'
              )
            {
              local.DashboardPyramid.NopayAddEmpV =
                (local.DashboardPyramid.NopayAddEmpV ?? 0) + 1;
            }
            else
            {
              local.DashboardPyramid.NopayAddEmpNv =
                (local.DashboardPyramid.NopayAddEmpNv ?? 0) + 1;
            }
          }
          else if (!IsEmpty(entities.DashboardStagingPriority4.AddressVerInd))
          {
            local.DashboardPyramid.NopayAdd =
              (local.DashboardPyramid.NopayAdd ?? 0) + 1;

            if (AsChar(entities.DashboardStagingPriority4.AddressVerInd) == 'Y')
            {
              local.DashboardPyramid.NopayAddV =
                (local.DashboardPyramid.NopayAddV ?? 0) + 1;
            }
            else
            {
              local.DashboardPyramid.NopayAddNv =
                (local.DashboardPyramid.NopayAddNv ?? 0) + 1;
            }
          }
          else if (!IsEmpty(entities.DashboardStagingPriority4.EmployerVerInd))
          {
            local.DashboardPyramid.NopayEmp =
              (local.DashboardPyramid.NopayEmp ?? 0) + 1;

            if (AsChar(entities.DashboardStagingPriority4.EmployerVerInd) == 'Y'
              )
            {
              local.DashboardPyramid.NopayEmpV =
                (local.DashboardPyramid.NopayEmpV ?? 0) + 1;
            }
            else
            {
              local.DashboardPyramid.NopayEmpNv =
                (local.DashboardPyramid.NopayEmpNv ?? 0) + 1;
            }
          }
        }
      }
      else if (AsChar(entities.DashboardStagingPriority4.OtherObgInd) == 'Y')
      {
        local.DashboardPyramid.NonCsObCases =
          (local.DashboardPyramid.NonCsObCases ?? 0) + 1;
      }
      else
      {
        local.DashboardPyramid.NoobCases =
          (local.DashboardPyramid.NoobCases ?? 0) + 1;

        if (AsChar(entities.DashboardStagingPriority4.PaternityEstInd) == 'N')
        {
          local.DashboardPyramid.NoobPat =
            (local.DashboardPyramid.NoobPat ?? 0) + 1;

          if (IsEmpty(entities.DashboardStagingPriority4.AddressVerInd) && IsEmpty
            (entities.DashboardStagingPriority4.EmployerVerInd))
          {
            local.DashboardPyramid.NoobPatNoaddemp =
              (local.DashboardPyramid.NoobPatNoaddemp ?? 0) + 1;
          }
          else if (!IsEmpty(entities.DashboardStagingPriority4.AddressVerInd) &&
            !IsEmpty(entities.DashboardStagingPriority4.EmployerVerInd))
          {
            local.DashboardPyramid.NoobPatAddemp =
              (local.DashboardPyramid.NoobPatAddemp ?? 0) + 1;

            if (AsChar(entities.DashboardStagingPriority4.AddressVerInd) == 'Y'
              && AsChar(entities.DashboardStagingPriority4.EmployerVerInd) == 'Y'
              )
            {
              local.DashboardPyramid.NoobPatAddempV =
                (local.DashboardPyramid.NoobPatAddempV ?? 0) + 1;
            }
            else
            {
              local.DashboardPyramid.NoobPatAddempNv =
                (local.DashboardPyramid.NoobPatAddempNv ?? 0) + 1;
            }
          }
          else if (!IsEmpty(entities.DashboardStagingPriority4.AddressVerInd))
          {
            local.DashboardPyramid.NoobPatAdd =
              (local.DashboardPyramid.NoobPatAdd ?? 0) + 1;

            if (AsChar(entities.DashboardStagingPriority4.AddressVerInd) == 'Y')
            {
              local.DashboardPyramid.NoobPatAddV =
                (local.DashboardPyramid.NoobPatAddV ?? 0) + 1;
            }
            else
            {
              local.DashboardPyramid.NoobPatAddNv =
                (local.DashboardPyramid.NoobPatAddNv ?? 0) + 1;
            }
          }
          else if (!IsEmpty(entities.DashboardStagingPriority4.EmployerVerInd))
          {
            local.DashboardPyramid.NoobPatEmp =
              (local.DashboardPyramid.NoobPatEmp ?? 0) + 1;

            if (AsChar(entities.DashboardStagingPriority4.EmployerVerInd) == 'Y'
              )
            {
              local.DashboardPyramid.NoobPatEmpV =
                (local.DashboardPyramid.NoobPatEmpV ?? 0) + 1;
            }
            else
            {
              local.DashboardPyramid.NoobPatEmpNv =
                (local.DashboardPyramid.NoobPatEmpNv ?? 0) + 1;
            }
          }
        }
        else
        {
          local.DashboardPyramid.NoobNopat =
            (local.DashboardPyramid.NoobNopat ?? 0) + 1;

          if (IsEmpty(entities.DashboardStagingPriority4.AddressVerInd) && IsEmpty
            (entities.DashboardStagingPriority4.EmployerVerInd))
          {
            local.DashboardPyramid.NoobNopatNoaddem =
              (local.DashboardPyramid.NoobNopatNoaddem ?? 0) + 1;
          }
          else if (!IsEmpty(entities.DashboardStagingPriority4.AddressVerInd) &&
            !IsEmpty(entities.DashboardStagingPriority4.EmployerVerInd))
          {
            local.DashboardPyramid.NoobNopatAdem =
              (local.DashboardPyramid.NoobNopatAdem ?? 0) + 1;

            if (AsChar(entities.DashboardStagingPriority4.AddressVerInd) == 'Y'
              && AsChar(entities.DashboardStagingPriority4.EmployerVerInd) == 'Y'
              )
            {
              local.DashboardPyramid.NoobNopatAdemV =
                (local.DashboardPyramid.NoobNopatAdemV ?? 0) + 1;
            }
            else
            {
              local.DashboardPyramid.NoobNopatAdemNv =
                (local.DashboardPyramid.NoobNopatAdemNv ?? 0) + 1;
            }
          }
          else if (!IsEmpty(entities.DashboardStagingPriority4.AddressVerInd))
          {
            local.DashboardPyramid.NoobNopatAdd =
              (local.DashboardPyramid.NoobNopatAdd ?? 0) + 1;

            if (AsChar(entities.DashboardStagingPriority4.AddressVerInd) == 'Y')
            {
              local.DashboardPyramid.NoobNopatAddV =
                (local.DashboardPyramid.NoobNopatAddV ?? 0) + 1;
            }
            else
            {
              local.DashboardPyramid.NoobNopatAddNv =
                (local.DashboardPyramid.NoobNopatAddNv ?? 0) + 1;
            }
          }
          else if (!IsEmpty(entities.DashboardStagingPriority4.EmployerVerInd))
          {
            local.DashboardPyramid.NoobNopatEmp =
              (local.DashboardPyramid.NoobNopatEmp ?? 0) + 1;

            if (AsChar(entities.DashboardStagingPriority4.EmployerVerInd) == 'Y'
              )
            {
              local.DashboardPyramid.NoobNopatEmpV =
                (local.DashboardPyramid.NoobNopatEmpV ?? 0) + 1;
            }
            else
            {
              local.DashboardPyramid.NoobNopatEmpNv =
                (local.DashboardPyramid.NoobNopatEmpNv ?? 0) + 1;
            }
          }
        }
      }
    }

    try
    {
      CreateDashboardPyramid();
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "DASHBOARD_PYRAMID_AE";

          break;
        case ErrorCode.PermittedValueViolation:
          ExitState = "DASHBOARD_PYRAMID_PV";

          break;
        default:
          throw;
      }
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error creating Dashboard_Pyramid in FN_B734_Pri_4_Pyramid_Report.";
      UseCabErrorReport();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
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

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    context.Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseFnB734Pri4Level1()
  {
    var useImport = new FnB734Pri4Level1.Import();
    var useExport = new FnB734Pri4Level1.Export();

    useImport.ProgramCheckpointRestart.Assign(import.ProgramCheckpointRestart);
    MoveDateWorkArea(import.ReportEndDate, useImport.ReportEndDate);
    MoveDashboardAuditData(import.DashboardAuditData,
      useImport.DashboardAuditData);
    useImport.ProgramProcessingInfo.ProcessDate =
      import.ProgramProcessingInfo.ProcessDate;

    context.Call(FnB734Pri4Level1.Execute, useImport, useExport);
  }

  private void UseFnB734Pri4Level2()
  {
    var useImport = new FnB734Pri4Level2.Import();
    var useExport = new FnB734Pri4Level2.Export();

    useImport.ProgramCheckpointRestart.Assign(import.ProgramCheckpointRestart);
    MoveDateWorkArea(import.ReportStartDate, useImport.ReportStartDate);
    MoveDateWorkArea(import.ReportEndDate, useImport.ReportEndDate);
    MoveDashboardAuditData(import.DashboardAuditData,
      useImport.DashboardAuditData);
    useImport.ProgramProcessingInfo.ProcessDate =
      import.ProgramProcessingInfo.ProcessDate;

    context.Call(FnB734Pri4Level2.Execute, useImport, useExport);
  }

  private void UseFnB734Pri4Level3()
  {
    var useImport = new FnB734Pri4Level3.Import();
    var useExport = new FnB734Pri4Level3.Export();

    useImport.ProgramCheckpointRestart.Assign(import.ProgramCheckpointRestart);
    MoveDateWorkArea(import.ReportStartDate, useImport.ReportStartDate);
    MoveDateWorkArea(import.ReportEndDate, useImport.ReportEndDate);
    MoveDashboardAuditData(import.DashboardAuditData,
      useImport.DashboardAuditData);
    useImport.ProgramProcessingInfo.ProcessDate =
      import.ProgramProcessingInfo.ProcessDate;

    context.Call(FnB734Pri4Level3.Execute, useImport, useExport);
  }

  private void UseFnB734Pri4Level4And5()
  {
    var useImport = new FnB734Pri4Level4And5.Import();
    var useExport = new FnB734Pri4Level4And5.Export();

    useImport.ProgramCheckpointRestart.Assign(import.ProgramCheckpointRestart);
    MoveDateWorkArea(import.ReportStartDate, useImport.ReportStartDate);
    MoveDateWorkArea(import.ReportEndDate, useImport.ReportEndDate);
    MoveDashboardAuditData(import.DashboardAuditData,
      useImport.DashboardAuditData);
    useImport.ProgramProcessingInfo.ProcessDate =
      import.ProgramProcessingInfo.ProcessDate;

    context.Call(FnB734Pri4Level4And5.Execute, useImport, useExport);
  }

  private void CreateDashboardPyramid()
  {
    var reportMonth = local.DashboardPyramid.ReportMonth;
    var runNumber = local.DashboardPyramid.RunNumber;
    var asOfDate = local.DashboardPyramid.AsOfDate;
    var totalCases = local.DashboardPyramid.TotalCases ?? 0;
    var csCases = local.DashboardPyramid.CsCases ?? 0;
    var payingCases = local.DashboardPyramid.PayingCases ?? 0;
    var nopayCases = local.DashboardPyramid.NopayCases ?? 0;
    var nopayNoaddemp = local.DashboardPyramid.NopayNoaddemp ?? 0;
    var nopayAdd = local.DashboardPyramid.NopayAdd ?? 0;
    var nopayAddV = local.DashboardPyramid.NopayAddV ?? 0;
    var nopayAddNv = local.DashboardPyramid.NopayAddNv ?? 0;
    var nopayEmp = local.DashboardPyramid.NopayEmp ?? 0;
    var nopayEmpV = local.DashboardPyramid.NopayEmpV ?? 0;
    var nopayEmpNv = local.DashboardPyramid.NopayEmpNv ?? 0;
    var nopayAddEmp = local.DashboardPyramid.NopayAddEmp ?? 0;
    var nopayAddEmpV = local.DashboardPyramid.NopayAddEmpV ?? 0;
    var nopayAddEmpNv = local.DashboardPyramid.NopayAddEmpNv ?? 0;
    var nonCsObCases = local.DashboardPyramid.NonCsObCases ?? 0;
    var noobCases = local.DashboardPyramid.NoobCases ?? 0;
    var noobPat = local.DashboardPyramid.NoobPat ?? 0;
    var noobPatNoaddemp = local.DashboardPyramid.NoobPatNoaddemp ?? 0;
    var noobPatAdd = local.DashboardPyramid.NoobPatAdd ?? 0;
    var noobPatAddV = local.DashboardPyramid.NoobPatAddV ?? 0;
    var noobPatAddNv = local.DashboardPyramid.NoobPatAddNv ?? 0;
    var noobPatEmp = local.DashboardPyramid.NoobPatEmp ?? 0;
    var noobPatEmpV = local.DashboardPyramid.NoobPatEmpV ?? 0;
    var noobPatEmpNv = local.DashboardPyramid.NoobPatEmpNv ?? 0;
    var noobPatAddemp = local.DashboardPyramid.NoobPatAddemp ?? 0;
    var noobPatAddempV = local.DashboardPyramid.NoobPatAddempV ?? 0;
    var noobPatAddempNv = local.DashboardPyramid.NoobPatAddempNv ?? 0;
    var noobNopat = local.DashboardPyramid.NoobNopat ?? 0;
    var noobNopatNoaddem = local.DashboardPyramid.NoobNopatNoaddem ?? 0;
    var noobNopatAdd = local.DashboardPyramid.NoobNopatAdd ?? 0;
    var noobNopatAddV = local.DashboardPyramid.NoobNopatAddV ?? 0;
    var noobNopatAddNv = local.DashboardPyramid.NoobNopatAddNv ?? 0;
    var noobNopatEmp = local.DashboardPyramid.NoobNopatEmp ?? 0;
    var noobNopatEmpV = local.DashboardPyramid.NoobNopatEmpV ?? 0;
    var noobNopatEmpNv = local.DashboardPyramid.NoobNopatEmpNv ?? 0;
    var noobNopatAdem = local.DashboardPyramid.NoobNopatAdem ?? 0;
    var noobNopatAdemV = local.DashboardPyramid.NoobNopatAdemV ?? 0;
    var noobNopatAdemNv = local.DashboardPyramid.NoobNopatAdemNv ?? 0;

    entities.DashboardPyramid.Populated = false;
    Update("CreateDashboardPyramid",
      (db, command) =>
      {
        db.SetInt32(command, "reportMonth", reportMonth);
        db.SetInt32(command, "runNumber", runNumber);
        db.SetNullableDate(command, "asOfDate", asOfDate);
        db.SetNullableInt32(command, "totalCases", totalCases);
        db.SetNullableInt32(command, "csCases", csCases);
        db.SetNullableInt32(command, "payingCases", payingCases);
        db.SetNullableInt32(command, "nopayCases", nopayCases);
        db.SetNullableInt32(command, "nopayNoaddemp", nopayNoaddemp);
        db.SetNullableInt32(command, "nopayAdd", nopayAdd);
        db.SetNullableInt32(command, "nopayAddV", nopayAddV);
        db.SetNullableInt32(command, "nopayAddNv", nopayAddNv);
        db.SetNullableInt32(command, "nopayEmp", nopayEmp);
        db.SetNullableInt32(command, "nopayEmpV", nopayEmpV);
        db.SetNullableInt32(command, "nopayEmpNv", nopayEmpNv);
        db.SetNullableInt32(command, "nopayAddEmp", nopayAddEmp);
        db.SetNullableInt32(command, "nopayAddEmpV", nopayAddEmpV);
        db.SetNullableInt32(command, "nopayAddEmpNv", nopayAddEmpNv);
        db.SetNullableInt32(command, "nonCsObCases", nonCsObCases);
        db.SetNullableInt32(command, "noobCases", noobCases);
        db.SetNullableInt32(command, "noobPat", noobPat);
        db.SetNullableInt32(command, "noobPatNoaddemp", noobPatNoaddemp);
        db.SetNullableInt32(command, "noobPatAdd", noobPatAdd);
        db.SetNullableInt32(command, "noobPatAddV", noobPatAddV);
        db.SetNullableInt32(command, "noobPatAddNv", noobPatAddNv);
        db.SetNullableInt32(command, "noobPatEmp", noobPatEmp);
        db.SetNullableInt32(command, "noobPatEmpV", noobPatEmpV);
        db.SetNullableInt32(command, "noobPatEmpNv", noobPatEmpNv);
        db.SetNullableInt32(command, "noobPatAddemp", noobPatAddemp);
        db.SetNullableInt32(command, "noobPatAddempV", noobPatAddempV);
        db.SetNullableInt32(command, "noobPatAddempNv", noobPatAddempNv);
        db.SetNullableInt32(command, "noobNopat", noobNopat);
        db.SetNullableInt32(command, "noobNopatNoaddem", noobNopatNoaddem);
        db.SetNullableInt32(command, "noobNopatAdd", noobNopatAdd);
        db.SetNullableInt32(command, "noobNopatAddV", noobNopatAddV);
        db.SetNullableInt32(command, "noobNopatAddNv", noobNopatAddNv);
        db.SetNullableInt32(command, "noobNopatEmp", noobNopatEmp);
        db.SetNullableInt32(command, "noobNopatEmpV", noobNopatEmpV);
        db.SetNullableInt32(command, "noobNopatEmpNv", noobNopatEmpNv);
        db.SetNullableInt32(command, "noobNopatAdem", noobNopatAdem);
        db.SetNullableInt32(command, "noobNopatAdemV", noobNopatAdemV);
        db.SetNullableInt32(command, "noobNopatAdemNv", noobNopatAdemNv);
      });

    entities.DashboardPyramid.ReportMonth = reportMonth;
    entities.DashboardPyramid.RunNumber = runNumber;
    entities.DashboardPyramid.AsOfDate = asOfDate;
    entities.DashboardPyramid.TotalCases = totalCases;
    entities.DashboardPyramid.CsCases = csCases;
    entities.DashboardPyramid.PayingCases = payingCases;
    entities.DashboardPyramid.NopayCases = nopayCases;
    entities.DashboardPyramid.NopayNoaddemp = nopayNoaddemp;
    entities.DashboardPyramid.NopayAdd = nopayAdd;
    entities.DashboardPyramid.NopayAddV = nopayAddV;
    entities.DashboardPyramid.NopayAddNv = nopayAddNv;
    entities.DashboardPyramid.NopayEmp = nopayEmp;
    entities.DashboardPyramid.NopayEmpV = nopayEmpV;
    entities.DashboardPyramid.NopayEmpNv = nopayEmpNv;
    entities.DashboardPyramid.NopayAddEmp = nopayAddEmp;
    entities.DashboardPyramid.NopayAddEmpV = nopayAddEmpV;
    entities.DashboardPyramid.NopayAddEmpNv = nopayAddEmpNv;
    entities.DashboardPyramid.NonCsObCases = nonCsObCases;
    entities.DashboardPyramid.NoobCases = noobCases;
    entities.DashboardPyramid.NoobPat = noobPat;
    entities.DashboardPyramid.NoobPatNoaddemp = noobPatNoaddemp;
    entities.DashboardPyramid.NoobPatAdd = noobPatAdd;
    entities.DashboardPyramid.NoobPatAddV = noobPatAddV;
    entities.DashboardPyramid.NoobPatAddNv = noobPatAddNv;
    entities.DashboardPyramid.NoobPatEmp = noobPatEmp;
    entities.DashboardPyramid.NoobPatEmpV = noobPatEmpV;
    entities.DashboardPyramid.NoobPatEmpNv = noobPatEmpNv;
    entities.DashboardPyramid.NoobPatAddemp = noobPatAddemp;
    entities.DashboardPyramid.NoobPatAddempV = noobPatAddempV;
    entities.DashboardPyramid.NoobPatAddempNv = noobPatAddempNv;
    entities.DashboardPyramid.NoobNopat = noobNopat;
    entities.DashboardPyramid.NoobNopatNoaddem = noobNopatNoaddem;
    entities.DashboardPyramid.NoobNopatAdd = noobNopatAdd;
    entities.DashboardPyramid.NoobNopatAddV = noobNopatAddV;
    entities.DashboardPyramid.NoobNopatAddNv = noobNopatAddNv;
    entities.DashboardPyramid.NoobNopatEmp = noobNopatEmp;
    entities.DashboardPyramid.NoobNopatEmpV = noobNopatEmpV;
    entities.DashboardPyramid.NoobNopatEmpNv = noobNopatEmpNv;
    entities.DashboardPyramid.NoobNopatAdem = noobNopatAdem;
    entities.DashboardPyramid.NoobNopatAdemV = noobNopatAdemV;
    entities.DashboardPyramid.NoobNopatAdemNv = noobNopatAdemNv;
    entities.DashboardPyramid.Populated = true;
  }

  private IEnumerable<bool> ReadDashboardStagingPriority4()
  {
    return ReadEach("ReadDashboardStagingPriority4",
      (db, command) =>
      {
        db.SetInt32(
          command, "reportMonth", import.DashboardAuditData.ReportMonth);
        db.SetInt32(command, "runNumber", import.DashboardAuditData.RunNumber);
      },
      (db, reader) =>
      {
        entities.DashboardStagingPriority4.ReportMonth = db.GetInt32(reader, 0);
        entities.DashboardStagingPriority4.RunNumber = db.GetInt32(reader, 1);
        entities.DashboardStagingPriority4.CaseNumber = db.GetString(reader, 2);
        entities.DashboardStagingPriority4.AsOfDate =
          db.GetNullableDate(reader, 3);
        entities.DashboardStagingPriority4.CurrentCsInd =
          db.GetNullableString(reader, 4);
        entities.DashboardStagingPriority4.OtherObgInd =
          db.GetNullableString(reader, 5);
        entities.DashboardStagingPriority4.CsDueAmt =
          db.GetNullableDecimal(reader, 6);
        entities.DashboardStagingPriority4.CsCollectedAmt =
          db.GetNullableDecimal(reader, 7);
        entities.DashboardStagingPriority4.PayingCaseInd =
          db.GetNullableString(reader, 8);
        entities.DashboardStagingPriority4.PaternityEstInd =
          db.GetNullableString(reader, 9);
        entities.DashboardStagingPriority4.AddressVerInd =
          db.GetNullableString(reader, 10);
        entities.DashboardStagingPriority4.EmployerVerInd =
          db.GetNullableString(reader, 11);
        entities.DashboardStagingPriority4.Populated = true;

        return true;
      },
      () =>
      {
        entities.DashboardStagingPriority4.Populated = false;
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
    /// A value of ProgramProcessingInfo.
    /// </summary>
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
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

    private ProgramProcessingInfo? programProcessingInfo;
    private DashboardAuditData? dashboardAuditData;
    private DateWorkArea? reportStartDate;
    private DateWorkArea? reportEndDate;
    private ProgramCheckpointRestart? programCheckpointRestart;
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

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of DashboardPyramid.
    /// </summary>
    public DashboardPyramid DashboardPyramid
    {
      get => dashboardPyramid ??= new();
      set => dashboardPyramid = value;
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

    private DashboardPyramid? dashboardPyramid;
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
    /// A value of DashboardPyramid.
    /// </summary>
    public DashboardPyramid DashboardPyramid
    {
      get => dashboardPyramid ??= new();
      set => dashboardPyramid = value;
    }

    /// <summary>
    /// A value of DashboardStagingPriority4.
    /// </summary>
    public DashboardStagingPriority4 DashboardStagingPriority4
    {
      get => dashboardStagingPriority4 ??= new();
      set => dashboardStagingPriority4 = value;
    }

    private DashboardPyramid? dashboardPyramid;
    private DashboardStagingPriority4? dashboardStagingPriority4;
  }
#endregion
}
