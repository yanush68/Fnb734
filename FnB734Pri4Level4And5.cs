// Program: FN_B734_PRI_4_LEVEL_4_AND_5, ID: 945237096, model: 746.
// Short name: SWE03728
using System;
using System.Collections.Generic;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B734_PRI_4_LEVEL_4_AND_5.
/// </summary>
[Serializable]
[Program("SWE03728")]
public partial class FnB734Pri4Level4And5: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B734_PRI_4_LEVEL_4_AND_5 program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB734Pri4Level4And5(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB734Pri4Level4And5.
  /// </summary>
  public FnB734Pri4Level4And5(IContext context, Import import, Export export):
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
    // 	
    // 03/12/13  GVandy	CQ42238		Determine address and employer status for all
    // 			Segment A	pyramid categores.
    // ---------------------------------------------------------------------------------------------------
    // -------------------------------------------------------------------------------------
    // Priority 4: Tier  - Address and Employer Status
    // -------------------------------------------------------------------------------------
    // Definitions for Tier 4.4 (to be used in specific priorities in Tier4)
    // Active Address
    // 	1) At least one active (non-end dated) address (ADDR and FADS) exists 
    // for the
    // 	   AP as of the report period end date.
    // 	2) Address record can be Residential or Mailing.
    // 	3) Credit all cases where AP has an active case role as of report period
    // end
    // 	   date.
    // Active Employer
    // 	1) At least one active (non-end dated) employer (INCS) exists for the 
    // NCP as
    // 	   of the report period end date.
    // 	2) Count only records with one of the following types: E(mployment),
    // 	   M(ilitary), O(ther).  Not counting R(esource) type.
    // 	3) Credit all cases where AP has an active case role as of report period
    // end
    // 	   date.
    // Tier 4.1- Cases with No Address or Employer
    // This is a count of all cases where no active AP has an active address or 
    // employer as
    // of the report period end date.  If multiple active APs exist, and at 
    // least one has an
    // active address or employer, the case will not count in this priority.  If
    // multiple
    // APs exist, we can usually begin establishment action, etc. when only one 
    // is located.
    // Exceptions: when an AP has Ross Hearing rights and must be notified of 
    // pending
    // actions.
    // Tier 4.2- Cases with Active Address Only
    // This is a count of all cases where at least one active AP has an active 
    // address, but
    // no active APs have an active employer.
    // Tier 4.3- Cases with Active Employer Only
    // This is a count of all cases where at least one active AP has an active 
    // employer, but
    // no active APs have an active address.
    // Tier 4.4- Cases with Both an Active Address AND an Active Employer
    // This is a count of all cases where at least one active AP has an active 
    // address, and
    // at least one active AP has an active employer.  It does not have to be 
    // the same AP.
    // Tier 5.1.1- Verified Address (Subset of Cases with Active Address Only)
    // 	1) At least one active AP on the case has an active (non-end dated) 
    // address
    // 	   WITH a Verified Date entered.
    // Tier 5.1.2- Non Verified Address (Subset of Cases with Active Address 
    // Only)
    // 	1) No active APs on the case have an active (non-end dated) address with
    // a
    // 	   Verified Date entered.  These would be ADDR records where a Send Date
    // is
    // 	   entered, but no Verified Date has yet been entered.
    // Tier 5.2.1- Verified Employer (Subset of Cases with Active Employer Only)
    // 	1) At least one active AP on the case has an active (non-end dated) 
    // employer
    // 	   with a Return Date entered and a qualifying combination (Type/ Return
    // 	   Code):
    // 		a. E(mployment)/ E(mployed)
    // 		b. M(ilitary)/ A(ctive)
    // 		c. M(ilitary)/ R(etired)
    // 		d. O(ther)/ V(erified)
    // Tier 5.2.1- Non Verified Employer (Subset of Cases with Active Employer 
    // Only)
    // 	1) No active APs on the case have an active (non-end dated) employer 
    // with a
    // 	   Return Date entered and a qualifying combination (Type/Return Code):
    // 		a. E(mployment)/ E(mployed)
    // 		b. M(ilitary)/ A(ctive)
    // 		c. M(ilitary)/ R(etired)
    // 		d. O(ther)/ V(erified)
    // Tier 5.3.1- Verified Address and Employer (Subset of Cases with Both an 
    // Active
    // Address and an Active Employer)
    // 	1) At least one active AP on the case has an active (non-end dated) 
    // address
    // 	   WITH a Verified Date entered.
    // 	AND
    // 	2) At least one active AP on the case has an active (non-end dated) 
    // employer
    // 	   with a Return Date entered and a qualifying combination (Type/ Return
    // 	   Code):
    // 		e. E(mployment)/ E(mployed)
    // 		f. M(ilitary)/ A(ctive)
    // 		g. M(ilitary)/ R(etired)
    // 		h. O(ther)/ V(erified)
    // 	NOTE- It does not have to be the same AP.  Example: Two APs known to 
    // case.
    // 	One has a Verified Address and no employer, the other has a Verified 
    // Employer
    // 	and no address.  The case would count in Tier 5.3.1.
    // Tier 5.3.2- Non-Verified Address or Employer (Cases with Both an Active 
    // Address and an Active Employer)
    // This is a count of all cases from Tier 4.4 where either no AP has a 
    // Verified Address and/or no AP has a Verified Employer (as defined above).
    // 	1) Take all cases reported in Tier 4.4
    // 	2) Subtract all cases reported in Tier 5.3.1
    // -------------------------------------------------------------------------------------
    MoveProgramCheckpointRestart(import.ProgramCheckpointRestart,
      local.ProgramCheckpointRestart);

    // ------------------------------------------------------------------------------
    // -- Determine if we're restarting and set appropriate restart information.
    // ------------------------------------------------------------------------------
    if (AsChar(import.ProgramCheckpointRestart.RestartInd) == 'Y' && Equal
      (import.ProgramCheckpointRestart.RestartInfo, 81, 8, "4-04    "))
    {
      // -- Checkpoint Info
      // Positions   Value
      // ---------   
      // ------------------------------------
      //  001-080    General Checkpoint Info for PRAD
      //  081-088    Dashboard Priority
      //  089-089    Blank
      //  090-099    Case Number
      local.Restart.Number =
        Substring(import.ProgramCheckpointRestart.RestartInfo, 90, 10);
    }
    else
    {
      local.Restart.Number = "";
    }

    // ------------------------------------------------------------------------------
    // -- Find all cases with requiring address and employer status.  These are 
    // all
    // -- non-paying current support cases, all no obligation paternity cases 
    // and all
    // -- no obligation non paternity cases.
    // ------------------------------------------------------------------------------
    foreach(var _ in ReadDashboardStagingPriority4())
    {
      ++local.RecordsReadSinceCommit.Count;
      local.DashboardStagingPriority4.AddressVerInd = "";
      local.DashboardStagingPriority4.EmployerVerInd = "";

      // -- Determine if an AP on the case has an Address.
      foreach(var _1 in ReadCsePersonAddress())
      {
        if (!Lt(import.ReportEndDate.Date,
          entities.CsePersonAddress.VerifiedDate) && Lt
          (local.Null1.Date, entities.CsePersonAddress.VerifiedDate))
        {
          local.DashboardStagingPriority4.AddressVerInd = "Y";
        }
        else
        {
          local.DashboardStagingPriority4.AddressVerInd = "N";
        }

        if (Lt(import.ReportEndDate.Date, entities.CsePersonAddress.VerifiedDate))
        {
          // -- Check the next address record to see if it is verified.
        }
        else
        {
          break;
        }
      }

      local.EmployerVerified.Flag = "";
      local.EmployerNonVerified.Flag = "";

      // -- Determine if an AP on the case has an Employer.
      foreach(var _1 in ReadIncomeSource())
      {
        if (!Lt(import.ReportEndDate.Date, entities.IncomeSource.ReturnDt) && Lt
          (local.Null1.Date, entities.IncomeSource.ReturnDt))
        {
          if (AsChar(entities.IncomeSource.Type1) == 'E' && AsChar
            (entities.IncomeSource.ReturnCd) == 'E' || AsChar
            (entities.IncomeSource.Type1) == 'M' && AsChar
            (entities.IncomeSource.ReturnCd) == 'A' || AsChar
            (entities.IncomeSource.Type1) == 'M' && AsChar
            (entities.IncomeSource.ReturnCd) == 'R' || AsChar
            (entities.IncomeSource.Type1) == 'O' && AsChar
            (entities.IncomeSource.ReturnCd) == 'V')
          {
            local.EmployerVerified.Flag = "Y";

            break;
          }
          else
          {
            // -- The return code indicates that the AP is not "employed".
          }
        }
        else
        {
          local.EmployerNonVerified.Flag = "Y";
        }
      }

      if (AsChar(local.EmployerVerified.Flag) == 'Y')
      {
        local.DashboardStagingPriority4.EmployerVerInd = "Y";
      }
      else if (AsChar(local.EmployerNonVerified.Flag) == 'Y')
      {
        local.DashboardStagingPriority4.EmployerVerInd = "N";
      }
      else
      {
        local.DashboardStagingPriority4.EmployerVerInd = "";
      }

      try
      {
        UpdateDashboardStagingPriority4();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "DASHBOARD_STAGING_PRI_4_NU";

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
          "Error updating Dashboard_Staging_Priority_4 in FN_B734_Pri_4_Level_4_and_5.";
        UseCabErrorReport();
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

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
        //  090-099    Case Number
        local.ProgramCheckpointRestart.RestartInd = "Y";
        local.ProgramCheckpointRestart.RestartInfo =
          Substring(import.ProgramCheckpointRestart.RestartInfo, 250, 1, 80) + "4-04    " +
          " " + String
          (entities.DashboardStagingPriority4.CaseNumber,
          DashboardStagingPriority4.CaseNumber_MaxLength);
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
      Substring(import.ProgramCheckpointRestart.RestartInfo, 250, 1, 80) + "4-05    ";
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

  private IEnumerable<bool> ReadCsePersonAddress()
  {
    return ReadEach("ReadCsePersonAddress",
      (db, command) =>
      {
        db.SetNullableDateTime(
          command, "createdTimestamp", import.ReportEndDate.Timestamp);
        db.SetNullableDate(command, "endDate", import.ReportEndDate.Date);
        db.SetString(
          command, "casNumber", entities.DashboardStagingPriority4.CaseNumber);
      },
      (db, reader) =>
      {
        entities.CsePersonAddress.Identifier = db.GetDateTime(reader, 0);
        entities.CsePersonAddress.CspNumber = db.GetString(reader, 1);
        entities.CsePersonAddress.VerifiedDate = db.GetNullableDate(reader, 2);
        entities.CsePersonAddress.EndDate = db.GetNullableDate(reader, 3);
        entities.CsePersonAddress.CreatedTimestamp =
          db.GetNullableDateTime(reader, 4);
        entities.CsePersonAddress.Populated = true;

        return true;
      },
      () =>
      {
        entities.CsePersonAddress.Populated = false;
      });
  }

  private IEnumerable<bool> ReadDashboardStagingPriority4()
  {
    return ReadEachInSeparateTransaction("ReadDashboardStagingPriority4",
      (db, command) =>
      {
        db.SetInt32(command, "runNumber", import.DashboardAuditData.RunNumber);
        db.SetInt32(
          command, "reportMonth", import.DashboardAuditData.ReportMonth);
        db.SetString(command, "caseNumber", local.Restart.Number);
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
        entities.DashboardStagingPriority4.PayingCaseInd =
          db.GetNullableString(reader, 6);
        entities.DashboardStagingPriority4.AddressVerInd =
          db.GetNullableString(reader, 7);
        entities.DashboardStagingPriority4.EmployerVerInd =
          db.GetNullableString(reader, 8);
        entities.DashboardStagingPriority4.Populated = true;

        return true;
      },
      () =>
      {
        entities.DashboardStagingPriority4.Populated = false;
      });
  }

  private IEnumerable<bool> ReadIncomeSource()
  {
    return ReadEach("ReadIncomeSource",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTimestamp", import.ReportEndDate.Timestamp);
        db.SetNullableDate(command, "endDt", import.ReportEndDate.Date);
        db.SetString(
          command, "casNumber", entities.DashboardStagingPriority4.CaseNumber);
      },
      (db, reader) =>
      {
        entities.IncomeSource.Identifier = db.GetDateTime(reader, 0);
        entities.IncomeSource.Type1 = db.GetString(reader, 1);
        entities.IncomeSource.ReturnDt = db.GetNullableDate(reader, 2);
        entities.IncomeSource.ReturnCd = db.GetNullableString(reader, 3);
        entities.IncomeSource.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.IncomeSource.CspINumber = db.GetString(reader, 5);
        entities.IncomeSource.StartDt = db.GetNullableDate(reader, 6);
        entities.IncomeSource.EndDt = db.GetNullableDate(reader, 7);
        entities.IncomeSource.Populated = true;
        CheckValid<IncomeSource>("Type1", entities.IncomeSource.Type1);

        return true;
      },
      () =>
      {
        entities.IncomeSource.Populated = false;
      });
  }

  private void UpdateDashboardStagingPriority4()
  {
    var addressVerInd = local.DashboardStagingPriority4.AddressVerInd ?? "";
    var employerVerInd = local.DashboardStagingPriority4.EmployerVerInd ?? "";

    entities.DashboardStagingPriority4.Populated = false;
    Update("UpdateDashboardStagingPriority4",
      (db, command) =>
      {
        db.SetNullableString(command, "addressVerInd", addressVerInd);
        db.SetNullableString(command, "employerVerInd", employerVerInd);
        db.SetInt32(
          command, "reportMonth",
          entities.DashboardStagingPriority4.ReportMonth);
        db.SetInt32(
          command, "runNumber", entities.DashboardStagingPriority4.RunNumber);
        db.SetString(
          command, "caseNumber", entities.DashboardStagingPriority4.CaseNumber);
      });

    entities.DashboardStagingPriority4.AddressVerInd = addressVerInd;
    entities.DashboardStagingPriority4.EmployerVerInd = employerVerInd;
    entities.DashboardStagingPriority4.Populated = true;
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
    private DateWorkArea? reportStartDate;
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
    /// A value of EmployerNonVerified.
    /// </summary>
    public Common EmployerNonVerified
    {
      get => employerNonVerified ??= new();
      set => employerNonVerified = value;
    }

    /// <summary>
    /// A value of EmployerVerified.
    /// </summary>
    public Common EmployerVerified
    {
      get => employerVerified ??= new();
      set => employerVerified = value;
    }

    /// <summary>
    /// A value of Null1.
    /// </summary>
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
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
    /// A value of Restart.
    /// </summary>
    public Case1 Restart
    {
      get => restart ??= new();
      set => restart = value;
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

    private Common? employerNonVerified;
    private Common? employerVerified;
    private DateWorkArea? null1;
    private DashboardStagingPriority4? dashboardStagingPriority4;
    private Case1? restart;
    private ProgramCheckpointRestart? programCheckpointRestart;
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
    /// A value of IncomeSource.
    /// </summary>
    public IncomeSource IncomeSource
    {
      get => incomeSource ??= new();
      set => incomeSource = value;
    }

    /// <summary>
    /// A value of CsePersonAddress.
    /// </summary>
    public CsePersonAddress CsePersonAddress
    {
      get => csePersonAddress ??= new();
      set => csePersonAddress = value;
    }

    /// <summary>
    /// A value of ChCaseRole.
    /// </summary>
    public CaseRole ChCaseRole
    {
      get => chCaseRole ??= new();
      set => chCaseRole = value;
    }

    /// <summary>
    /// A value of ApCaseRole.
    /// </summary>
    public CaseRole ApCaseRole
    {
      get => apCaseRole ??= new();
      set => apCaseRole = value;
    }

    /// <summary>
    /// A value of ChCsePerson.
    /// </summary>
    public CsePerson ChCsePerson
    {
      get => chCsePerson ??= new();
      set => chCsePerson = value;
    }

    /// <summary>
    /// A value of ApCsePerson.
    /// </summary>
    public CsePerson ApCsePerson
    {
      get => apCsePerson ??= new();
      set => apCsePerson = value;
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

    private IncomeSource? incomeSource;
    private CsePersonAddress? csePersonAddress;
    private CaseRole? chCaseRole;
    private CaseRole? apCaseRole;
    private CsePerson? chCsePerson;
    private CsePerson? apCsePerson;
    private DashboardStagingPriority4? dashboardStagingPriority4;
    private Case1? case1;
  }
#endregion
}
