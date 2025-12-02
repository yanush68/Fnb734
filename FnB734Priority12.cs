// Program: FN_B734_PRIORITY_1_2, ID: 945132074, model: 746.
// Short name: SWE03086
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
/// A program: FN_B734_PRIORITY_1_2.
/// </para>
/// <para>
/// Priority 1-2: IV-D Paternity Establishment Percentage (PEP)
/// </para>
/// </summary>
[Serializable]
[Program("SWE03086")]
public partial class FnB734Priority12: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B734_PRIORITY_1_2 program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB734Priority12(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB734Priority12.
  /// </summary>
  public FnB734Priority12(IContext context, Import import, Export export):
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
    // 10/21/15  DDupree	CQ46954		Add a rollup to the contractor level.  Also 
    // add % change
    // 					from previous year values.
    // ---------------------------------------------------------------------------------------------------
    // -------------------------------------------------------------------------------------
    // Priority 1-2: IV-D Paternity Establishment Percentage (PEP)
    // -------------------------------------------------------------------------------------
    // Total children in IV-D caseload w/ paternity established at end of 
    // current FFY
    // divided by total children in IV-D cases that are born out of wedlock at 
    // end of prior
    // FFY.  (OCSE157 Line6/ OCSE157 Line5 of the preceding FFY [Line5a])
    // Report Level: State, Judicial District
    // Report Period: Month (Fiscal year-to-date calculation)
    // Numerator (Current FFY):
    // Children in IV-D Cases Open during or at the End of the FFY with 
    // Paternity
    // Established or Acknowledged
    // 	1) Case open at any time during report period.
    // 	2) Paternity established and BOW on CPAT = Y.
    // 	3) Exclude children in cases where good cause has been claimed as of 
    // report
    // 	   period end, GC code only.  Do not include pending code. If a child is
    // 	   active during the Report Period on any case where GC is not claimed, 
    // then
    // 	   the child will not be excluded.
    // 	4) Child case role on case open at any time during the report period.
    // 	5) Include children emancipated at any time during the report period.
    // 	6) Exclude previously emancipated children. (exclude children 
    // emancipated
    // 	   prior to report period)
    // 	7) Count children only once.
    // 	8) Case role active at some point during the report period.
    // Denominator (data from prior FFY):  Note:  - The denominator will not be 
    // calculated.
    // It will be taken straight from the OCSE157 run and entered directly in 
    // the table.
    // Children in IV-D Cases at the End of the PRIOR FFY Who Were Born out of 
    // Wedlock.
    // 	1) Case open at the end of the report period.
    // 	2) Child case role on case open at any time during report period.
    // 	3) Include children emancipated at any time during the report period.
    // 	4) Exclude previously emancipated children. (Children that emancipated 
    // prior
    // 	   to FY start)
    // 	5) Count children only once.
    // 	6) Case role active at some point during the report period.
    // 	7) BOW on CPAT = Y
    // 	8) Exclude children in cases where good cause has been claimed as of 
    // report
    // 	   period end, GC code only.  Do not include pending.
    // -------------------------------------------------------------------------------------
    MoveDashboardAuditData2(import.DashboardAuditData, local.Initialized);
    MoveProgramCheckpointRestart(import.ProgramCheckpointRestart,
      local.ProgramCheckpointRestart);

    // -- Determine Previous Year report month.
    local.PreviousYear.ReportMonth = import.DashboardAuditData.ReportMonth - 100
      ;

    // --  Initialize Statewide view
    local.Statewide.AsOfDate = import.ProgramProcessingInfo.ProcessDate;
    local.Statewide.ReportLevel = "ST";
    local.Statewide.ReportLevelId = "KS";
    local.Statewide.ReportMonth = import.DashboardAuditData.ReportMonth;
    local.Contractor.Index = -1;
    local.Contractor.Count = 0;

    // --  Initialize Judicial District group view
    foreach(var _ in ReadCseOrganization())
    {
      if (Verify(entities.CseOrganization.Code, "0123456789") != 0)
      {
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "Judical District code not numeric.  JD Code = " + String
          (entities.CseOrganization.Code, CseOrganization.Code_MaxLength);
        UseCabErrorReport();
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      local.Local1.Index =
        (int)StringToNumber(entities.CseOrganization.Code) - 1;
      local.Local1.CheckSize();

      local.Local1.Update.G.AsOfDate = import.ProgramProcessingInfo.ProcessDate;
      local.Local1.Update.G.ReportLevel = "JD";
      local.Local1.Update.G.ReportLevelId = entities.CseOrganization.Code;
      local.Local1.Update.G.ReportMonth = import.DashboardAuditData.ReportMonth;

      // -- Determine contractor from the judicial district to which they are 
      // assigned on the report period end date.
      local.DashboardAuditData.JudicialDistrict = entities.CseOrganization.Code;
      UseFnB734DeterContractorFromJd();
      local.Local1.Update.G.ContractorNumber = local.Contractor1.Code;

      if (local.Contractor.Count < 1)
      {
        ++local.Contractor.Index;
        local.Contractor.CheckSize();

        local.Contractor.Update.Gcontractor.ContractorNumber =
          local.Contractor1.Code;
        local.Contractor.Update.Gcontractor.ReportLevel = "XJ";
        local.Contractor.Update.Gcontractor.ReportMonth =
          import.DashboardAuditData.ReportMonth;
        local.Contractor.Update.Gcontractor.ReportLevelId =
          local.Local1.Item.G.ContractorNumber;
        local.Contractor.Update.Gcontractor.AsOfDate =
          import.ProgramProcessingInfo.ProcessDate;
      }
      else
      {
        for(local.Contractor.Index = 0; local.Contractor.Index < local
          .Contractor.Count; ++local.Contractor.Index)
        {
          if (!local.Contractor.CheckSize())
          {
            break;
          }

          if (Equal(local.Contractor1.Code,
            local.Contractor.Item.Gcontractor.ContractorNumber))
          {
            goto ReadEach1;
          }
        }

        local.Contractor.CheckIndex();

        local.Contractor.Index = local.Contractor.Count;
        local.Contractor.CheckSize();

        local.Contractor.Update.Gcontractor.ContractorNumber =
          local.Contractor1.Code;
        local.Contractor.Update.Gcontractor.ReportLevel = "XJ";
        local.Contractor.Update.Gcontractor.ReportLevelId =
          local.Local1.Item.G.ContractorNumber;
        local.Contractor.Update.Gcontractor.ReportMonth =
          import.DashboardAuditData.ReportMonth;
        local.Contractor.Update.Gcontractor.AsOfDate =
          import.ProgramProcessingInfo.ProcessDate;
      }

ReadEach1:
      ;
    }

    // ------------------------------------------------------------------------------
    // -- Determine if we're restarting and set appropriate restart information.
    // ------------------------------------------------------------------------------
    if (AsChar(import.ProgramCheckpointRestart.RestartInd) == 'Y' && Equal
      (import.ProgramCheckpointRestart.RestartInfo, 81, 8, "1-02    "))
    {
      // -- Checkpoint Info
      // Positions   Value
      // ---------   
      // ------------------------------------
      //  001-080    General Checkpoint Info for PRAD
      //  081-088    Dashboard Priority
      //  089-089    Blank
      //  090-099    CSE Person Number
      //  100-100    Blank
      //  101-101    Restart Section (N=Numerator, D=Denominator)
      local.RestartSection.Text1 =
        Substring(import.ProgramCheckpointRestart.RestartInfo, 101, 1);

      switch(AsChar(local.RestartSection.Text1))
      {
        case ' ':
          break;
        case 'N':
          local.RestartNumerator.Number =
            Substring(import.ProgramCheckpointRestart.RestartInfo, 90, 10);

          break;
        case 'D':
          local.RestartDenominator.Number =
            Substring(import.ProgramCheckpointRestart.RestartInfo, 90, 10);

          break;
        default:
          break;
      }

      if (!IsEmpty(local.RestartSection.Text1) || !
        IsEmpty(local.RestartDenominator.Number) || !
        IsEmpty(local.RestartNumerator.Number))
      {
        // -- Load statewide counts.
        foreach(var _ in ReadDashboardStagingPriority1())
        {
          MoveDashboardStagingPriority12(entities.DashboardStagingPriority12,
            local.Statewide);
        }

        // -- Load Judicial District counts.
        foreach(var _ in ReadDashboardStagingPriority2())
        {
          local.Local1.Index =
            (int)StringToNumber(entities.DashboardStagingPriority12.
              ReportLevelId) - 1;
          local.Local1.CheckSize();

          local.Local1.Update.G.Assign(entities.DashboardStagingPriority12);
        }
      }
      else
      {
        // -- Load statewide counts.
        foreach(var _ in ReadDashboardStagingPriority1())
        {
          MoveDashboardStagingPriority12(entities.DashboardStagingPriority12,
            local.Statewide);
          local.Statewide.PepDenominator = 0;
          local.Statewide.PepNumerator = 0;
          local.Statewide.PepPercent = 0;
          local.Statewide.PepRank = 0;
        }

        // -- Load Judicial District counts.
        foreach(var _ in ReadDashboardStagingPriority2())
        {
          local.Local1.Index =
            (int)StringToNumber(entities.DashboardStagingPriority12.
              ReportLevelId) - 1;
          local.Local1.CheckSize();

          local.Local1.Update.G.Assign(entities.DashboardStagingPriority12);
          local.Local1.Update.G.PepDenominator = 0;
          local.Local1.Update.G.PepNumerator = 0;
          local.Local1.Update.G.PepRank = 0;
          local.Local1.Update.G.PepPercent = 0;
        }
      }
    }
    else
    {
      local.RestartNumerator.Number = "";
    }

    if (IsEmpty(local.RestartSection.Text1) || AsChar
      (local.RestartSection.Text1) == 'N')
    {
      // -------------------------------------------------------------------------------------
      // -- N U M E R A T O R (Children with Paternity Established or 
      // Acknowledged) (OCSE157 Line 6)
      // -------------------------------------------------------------------------------------
      foreach(var _ in ReadCsePerson())
      {
        // ------------------------------------------------------------------------------
        // -- Checkpoint saving all the info needed for restarting.
        // ------------------------------------------------------------------------------
        if (local.RecordsReadSinceCommit.Count >= (
          import.ProgramCheckpointRestart.ReadFrequencyCount ?? 0))
        {
          // -- Save the Statewide counts.
          if (ReadDashboardStagingPriority3())
          {
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
          else
          {
            try
            {
              CreateDashboardStagingPriority1();
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  ExitState = "DASHBOARD_STAGING_PRI_1_2_AE";

                  break;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "DASHBOARD_STAGING_PRI_1_2_PV";

                  break;
                default:
                  throw;
              }
            }
          }

          // -- Save Judicial District counts.
          for(local.Local1.Index = 0; local.Local1.Index < local.Local1.Count; ++
            local.Local1.Index)
          {
            if (!local.Local1.CheckSize())
            {
              break;
            }

            if (ReadDashboardStagingPriority4())
            {
              try
              {
                UpdateDashboardStagingPriority2();
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
            else
            {
              try
              {
                CreateDashboardStagingPriority2();
              }
              catch(Exception e)
              {
                switch(GetErrorCode(e))
                {
                  case ErrorCode.AlreadyExists:
                    ExitState = "DASHBOARD_STAGING_PRI_1_2_AE";

                    break;
                  case ErrorCode.PermittedValueViolation:
                    ExitState = "DASHBOARD_STAGING_PRI_1_2_PV";

                    break;
                  default:
                    throw;
                }
              }
            }
          }

          local.Local1.CheckIndex();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail =
              "Error creating/updating Dashboard_Staging_Priority_1_2.";
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
          //  089-089    Blank
          //  090-099    CSE Person Number
          //  100-100    Blank
          //  101-101    Restart Section (N=Numerator, D=Denominator)
          local.ProgramCheckpointRestart.RestartInd = "Y";
          local.ProgramCheckpointRestart.RestartInfo =
            Substring(import.ProgramCheckpointRestart.RestartInfo, 250, 1, 80) +
            "1-02    " + " " + String
            (local.Prev.Number, CsePerson.Number_MaxLength) + " N";
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

        local.Prev.Number = entities.CsePerson.Number;
        ++local.RecordsReadSinceCommit.Count;

        // -- Initialize group view which will contain the judical districts in 
        // which the child has a case role.
        for(local.ChildInJds.Index = 0; local.ChildInJds.Index < local
          .ChildInJds.Count; ++local.ChildInJds.Index)
        {
          if (!local.ChildInJds.CheckSize())
          {
            break;
          }

          local.ChildInJds.Update.GlocalChildInJd.Count = 0;
        }

        local.ChildInJds.CheckIndex();
        local.ChildInJds.Count = 0;
        local.ChildCaseCount.Count = 0;

        // -------------------------------------------------------------------
        // Read CH case roles that are active at some point during FY.
        // -------------------------------------------------------------------
        foreach(var _1 in ReadCaseRoleCase1())
        {
          // -----------------------------------------------------
          // Skip case if there is no juridiction.
          // -----------------------------------------------------
          if (!IsEmpty(entities.Case1.NoJurisdictionCd))
          {
            continue;
          }

          // -----------------------------------------------------
          // Skip child if emancipation date is set and is before the start of 
          // FY.
          // -----------------------------------------------------
          if (Lt(local.NullDate.Date, entities.CaseRole.DateOfEmancipation) && Lt
            (entities.CaseRole.DateOfEmancipation, import.ReportStartDate.Date))
          {
            continue;
          }

          // ----------------------------------------------------
          // Skip if case is not open at some point during the FY.
          // ----------------------------------------------------
          ReadCaseAssignment1();

          if (!entities.CaseAssignment.Populated)
          {
            continue;
          }

          // ---------------------------------------------------------
          // Exclude children where Good Cause is active for AR as of end of FY.
          // --------------------------------------------------------
          // ---------------------------------------------------------
          // 6/17/2001
          // If child is active on muliple cases during FY, then count child
          // if there is atleast one case with no active Good Cause as of
          // the end of FY.
          // Read below looks for current Case only. This is okay since
          // we will parse through this logic again for next Case.
          // --------------------------------------------------------
          // ---------------------------------------------------------
          // 07/31/2001
          // Only read GC code to determine good cause.
          // --------------------------------------------------------
          // ---------------------------------------------------------------------------
          // Possible values for Good Cause Code are.
          // PE-Good Cause Pending
          // GC-Good Cause
          // CO-Cooperating
          // Users 'never' end GC records when establishing CO
          // records. Infact, there are no 'closed' entries on Good Cause
          // table as of 8/2.
          // So, to determine if Good Cause is active, look for active GC
          // records where there is no CO created after the GC record.
          // --------------------------------------------------------------------------
          foreach(var _2 in ReadGoodCauseCaseRoleGoodCause1())
          {
            // ---------------------------------------------------------------------
            // Ensure there is no CO record that is created after the GC
            // record but before FY end.
            // ---------------------------------------------------------------------
            if (entities.Next.Populated)
            {
              continue;
            }
            else
            {
              // ---------------------------------------------------------------------
              // So, GC must be still active.
              // ---------------------------------------------------------------------
            }

            goto ReadEach2;
          }

          // -----------------------------------------------------------
          // All conditions are satisifed. Count Child.
          // -----------------------------------------------------------
          ++local.ChildCaseCount.Count;
          MoveDashboardAuditData1(local.Initialized, local.DashboardAuditData);
          local.DashboardAuditData.DashboardPriority = "1-2(N)";
          local.DashboardAuditData.SuppCspNumber = entities.CsePerson.Number;

          // -- Increment Statewide Level
          if (local.ChildCaseCount.Count == 1)
          {
            // -- Count the child only once at the statewide level.
            local.Statewide.PepNumerator =
              (local.Statewide.PepNumerator ?? 0) + 1;
          }

          // -- Determine Judicial District...
          if (!Lt(import.ReportEndDate.Date,
            entities.CaseAssignment.DiscontinueDate))
          {
            // -- Pass the case assignment end date to the Determine JD cab so 
            // it
            // -- will find the JD the case belonged to on the closure date.
            local.TempEndDate.Date = entities.CaseAssignment.DiscontinueDate;
          }
          else
          {
            local.TempEndDate.Date = import.ReportEndDate.Date;
          }

          UseFnB734DetermineJdFromCase();

          // -- Increment Judicial District Level
          if (!IsEmpty(local.DashboardAuditData.JudicialDistrict))
          {
            // -- Keep a running count of how many cases the child has in each 
            // judicial district.
            local.ChildInJds.Index =
              (int)StringToNumber(local.DashboardAuditData.JudicialDistrict) - 1
              ;
            local.ChildInJds.CheckSize();

            local.ChildInJds.Update.GlocalChildInJd.Count =
              local.ChildInJds.Item.GlocalChildInJd.Count + 1;

            if (local.ChildInJds.Item.GlocalChildInJd.Count == 1)
            {
              // -- Count each Child only once per JD.
              local.Local1.Index =
                (int)StringToNumber(local.DashboardAuditData.JudicialDistrict) -
                1;
              local.Local1.CheckSize();

              local.Local1.Update.G.PepNumerator =
                (local.Local1.Item.G.PepNumerator ?? 0) + 1;

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

ReadEach2:
          ;
        }
      }
    }

    // -------------------------------------------------------------------------------------
    // --  D E N O M I N A T O R  (Children born out of wedlock at end of prior 
    // FFY) (OCSE157 Line 5a)
    // -------------------------------------------------------------------------------------
    // -------------------------------------------------------------------------------------
    // We will already have a count of all children that reported in this line (
    // denominator)
    // in the OCSE157 tables.  Rather than recode the above denominator rules, 
    // we would:
    // 	1) Count all children that reported in Line5 for the most recent OCSE157
    // 	   report.  Those are the same children that should be counted in the
    // 	   denominator here.
    // 	2) For each child, count the child once on each case that was open on 
    // the
    // 	   last day of the prior FFY where the child had an active case role 
    // during
    // 	   the prior FFY.  The case cannot have had good cause set on the last 
    // day of
    // 	   the prior FFY.  Count each child only once per JD, but the child may 
    // be
    // 	   counted in multiple JDs.  Credit the office/JD where the case is 
    // assigned
    // 	   on the last day of the prior FFY.
    // -------------------------------------------------------------------------------------
    // -- Determine prior fiscal year start and end dates.
    local.PriorFiscalYrStartDate.Date =
      AddYears(import.FiscalYearStartDate.Date, -1);
    local.PriorFiscalYrStartDate.Timestamp =
      AddYears(import.FiscalYearStartDate.Timestamp, -1);
    local.PriorFiscalYrEndDate.Date =
      AddYears(import.FiscalYearEndDate.Date, -1);
    local.PriorFiscalYrEndDate.Timestamp =
      AddYears(import.FiscalYearEndDate.Timestamp, -1);

    // -- Find the most recent run numer for OCSE157 line 05 for the prior 
    // fiscal year.
    local.Ocse157Data.FiscalYear = Year(import.FiscalYearStartDate.Timestamp);
    local.Ocse157Data.LineNumber = "05";

    if (ReadOcse157Data())
    {
      local.Ocse157Data.Assign(entities.Ocse157Data);
    }

    // -- Log the OCSE157 run information to the control report.
    local.EabFileHandling.Action = "WRITE";
    local.EabReportSend.RptDetail =
      "Priority 1-2 Denominator using OCSE157 Fiscal Year " + NumberToString
      (local.Ocse157Data.FiscalYear ?? 0, 12, 4) + " Run Number " + NumberToString
      (local.Ocse157Data.RunNumber ?? 0, 12, 4) + " Line 5 count " + NumberToString
      (local.Ocse157Data.Number ?? 0L, 4, 12);
    UseCabControlReport();

    // -- Read each child reported in the OCSE157 Line 05.
    foreach(var _ in ReadOcse157VerificationCsePerson())
    {
      ++local.Ocse157Verifi.Count;

      if (!entities.CsePerson.Populated)
      {
        continue;
      }

      // ------------------------------------------------------------------------------
      // -- Checkpoint saving all the info needed for restarting.
      // ------------------------------------------------------------------------------
      if (local.RecordsReadSinceCommit.Count >= (
        import.ProgramCheckpointRestart.ReadFrequencyCount ?? 0))
      {
        // -- Save the Statewide counts.
        if (ReadDashboardStagingPriority3())
        {
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
        else
        {
          try
          {
            CreateDashboardStagingPriority1();
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "DASHBOARD_STAGING_PRI_1_2_AE";

                break;
              case ErrorCode.PermittedValueViolation:
                ExitState = "DASHBOARD_STAGING_PRI_1_2_PV";

                break;
              default:
                throw;
            }
          }
        }

        // -- Save Judicial District counts.
        for(local.Local1.Index = 0; local.Local1.Index < local.Local1.Count; ++
          local.Local1.Index)
        {
          if (!local.Local1.CheckSize())
          {
            break;
          }

          if (ReadDashboardStagingPriority4())
          {
            try
            {
              UpdateDashboardStagingPriority2();
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
          else
          {
            try
            {
              CreateDashboardStagingPriority2();
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  ExitState = "DASHBOARD_STAGING_PRI_1_2_AE";

                  break;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "DASHBOARD_STAGING_PRI_1_2_PV";

                  break;
                default:
                  throw;
              }
            }
          }
        }

        local.Local1.CheckIndex();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "Error creating/updating Dashboard_Staging_Priority_1_2.";
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
        //  089-089    Blank
        //  090-099    CSE Person Number
        //  100-100    Blank
        //  101-101    Restart Section (N=Numerator, D=Denominator)
        local.ProgramCheckpointRestart.RestartInd = "Y";
        local.ProgramCheckpointRestart.RestartInfo =
          Substring(import.ProgramCheckpointRestart.RestartInfo, 250, 1, 80) + "1-02    " +
          " " + String(local.Prev.Number, CsePerson.Number_MaxLength) + " D";
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

      local.Prev.Number = entities.CsePerson.Number;
      ++local.RecordsReadSinceCommit.Count;

      // -- Initialize group view which will contain the judical districts in 
      // which the child has a case role.
      for(local.ChildInJds.Index = 0; local.ChildInJds.Index < local
        .ChildInJds.Count; ++local.ChildInJds.Index)
      {
        if (!local.ChildInJds.CheckSize())
        {
          break;
        }

        local.ChildInJds.Update.GlocalChildInJd.Count = 0;
      }

      local.ChildInJds.CheckIndex();
      local.ChildInJds.Count = 0;
      local.ChildCaseCount.Count = 0;

      // -------------------------------------------------------------------
      // Read CH case roles that are active at some point during FY.
      // -------------------------------------------------------------------
      foreach(var _1 in ReadCaseRoleCase2())
      {
        // -----------------------------------------------------
        // Skip case if there is no juridiction.
        // -----------------------------------------------------
        if (!IsEmpty(entities.Case1.NoJurisdictionCd))
        {
          continue;
        }

        // -----------------------------------------------------
        // Skip child if emancipation date is set and is before the start of FY.
        // -----------------------------------------------------
        if (Lt(local.NullDate.Date, entities.CaseRole.DateOfEmancipation) && Lt
          (entities.CaseRole.DateOfEmancipation,
          local.PriorFiscalYrStartDate.Date))
        {
          continue;
        }

        // ----------------------------------------------------
        // Skip if case is not open at some point during the FY.
        // ----------------------------------------------------
        ReadCaseAssignment2();

        if (!entities.CaseAssignment.Populated)
        {
          continue;
        }

        // ---------------------------------------------------------
        // Exclude children where Good Cause is active for AR as of end of FY.
        // --------------------------------------------------------
        // ---------------------------------------------------------
        // 6/17/2001
        // If child is active on muliple cases during FY, then count child
        // if there is atleast one case with no active Good Cause as of
        // the end of FY.
        // Read below looks for current Case only. This is okay since
        // we will parse through this logic again for next Case.
        // --------------------------------------------------------
        // ---------------------------------------------------------
        // 07/31/2001
        // Only read GC code to determine good cause.
        // --------------------------------------------------------
        // ---------------------------------------------------------------------------
        // Possible values for Good Cause Code are.
        // PE-Good Cause Pending
        // GC-Good Cause
        // CO-Cooperating
        // Users 'never' end GC records when establishing CO
        // records. Infact, there are no 'closed' entries on Good Cause
        // table as of 8/2.
        // So, to determine if Good Cause is active, look for active GC
        // records where there is no CO created after the GC record.
        // --------------------------------------------------------------------------
        foreach(var _2 in ReadGoodCauseCaseRoleGoodCause2())
        {
          // ---------------------------------------------------------------------
          // Ensure there is no CO record that is created after the GC
          // record but before FY end.
          // ---------------------------------------------------------------------
          if (entities.Next.Populated)
          {
            continue;
          }
          else
          {
            // ---------------------------------------------------------------------
            // So, GC must be still active.
            // ---------------------------------------------------------------------
          }

          goto ReadEach3;
        }

        // -----------------------------------------------------------
        // All conditions are satisifed. Count Child.
        // -----------------------------------------------------------
        ++local.ChildCaseCount.Count;
        MoveDashboardAuditData1(local.Initialized, local.DashboardAuditData);
        local.DashboardAuditData.DashboardPriority = "1-2(D)";
        local.DashboardAuditData.SuppCspNumber = entities.CsePerson.Number;

        // -- Increment Statewide Level
        if (local.ChildCaseCount.Count == 1)
        {
          // -- Count the child only once at the statewide level.
          local.Statewide.PepDenominator =
            (local.Statewide.PepDenominator ?? 0) + 1;
        }

        // -- Determine Judicial District...
        if (!Lt(local.PriorFiscalYrEndDate.Date,
          entities.CaseAssignment.DiscontinueDate))
        {
          // -- Pass the case assignment end date to the Determine JD cab so it
          // -- will find the JD the case belonged to on the closure date.
          local.TempEndDate.Date = entities.CaseAssignment.DiscontinueDate;
        }
        else
        {
          local.TempEndDate.Date = local.PriorFiscalYrEndDate.Date;
        }

        UseFnB734DetermineJdFromCase();

        // -- Increment Judicial District Level
        if (!IsEmpty(local.DashboardAuditData.JudicialDistrict))
        {
          // -- Keep a running count of how many cases the child has in each 
          // judicial district.
          local.ChildInJds.Index =
            (int)StringToNumber(local.DashboardAuditData.JudicialDistrict) - 1;
          local.ChildInJds.CheckSize();

          local.ChildInJds.Update.GlocalChildInJd.Count =
            local.ChildInJds.Item.GlocalChildInJd.Count + 1;

          if (local.ChildInJds.Item.GlocalChildInJd.Count == 1)
          {
            // -- Count each Child only once per JD.
            local.Local1.Index =
              (int)StringToNumber(local.DashboardAuditData.JudicialDistrict) - 1
              ;
            local.Local1.CheckSize();

            local.Local1.Update.G.PepDenominator =
              (local.Local1.Item.G.PepDenominator ?? 0) + 1;

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

ReadEach3:
        ;
      }
    }

    // -- Log the number of OCSE157_verifi records found to the control report.
    local.EabFileHandling.Action = "WRITE";
    local.EabReportSend.RptDetail =
      "Priority 1-2 Denominator using OCSE157 Fiscal Year " + NumberToString
      (local.Ocse157Data.FiscalYear ?? 0, 12, 4) + " Run Number " + NumberToString
      (local.Ocse157Data.RunNumber ?? 0, 12, 4) + " Line 5 children found " + NumberToString
      (local.Ocse157Verifi.Count, 4, 12);
    UseCabControlReport();

    // ------------------------------------------------------------------------------
    // -- Store final Statewide counts.
    // ------------------------------------------------------------------------------
    if (ReadDashboardStagingPriority3())
    {
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
    else
    {
      try
      {
        CreateDashboardStagingPriority1();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "DASHBOARD_STAGING_PRI_1_2_AE";

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
    // -- Store final Judicial District counts.
    // ------------------------------------------------------------------------------
    for(local.Local1.Index = 0; local.Local1.Index < local.Local1.Count; ++
      local.Local1.Index)
    {
      if (!local.Local1.CheckSize())
      {
        break;
      }

      if (ReadDashboardStagingPriority4())
      {
        try
        {
          UpdateDashboardStagingPriority2();
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
      else
      {
        try
        {
          CreateDashboardStagingPriority2();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "DASHBOARD_STAGING_PRI_1_2_AE";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "DASHBOARD_STAGING_PRI_1_2_PV";

              break;
            default:
              throw;
          }
        }
      }
    }

    local.Local1.CheckIndex();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error creating/updating Dashboard_Staging_Priority_1_2.";
      UseCabErrorReport();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // ------------------------------------------------------------------------------
    // -- Rollup current year Judicial District counts to the Contractor Level.
    // ------------------------------------------------------------------------------
    for(local.Local1.Index = 0; local.Local1.Index < local.Local1.Count; ++
      local.Local1.Index)
    {
      if (!local.Local1.CheckSize())
      {
        break;
      }

      // -- Add all the current jd amounts into the correct contractor
      for(local.Contractor.Index = 0; local.Contractor.Index < local
        .Contractor.Count; ++local.Contractor.Index)
      {
        if (!local.Contractor.CheckSize())
        {
          break;
        }

        if (Equal(local.Local1.Item.G.ContractorNumber,
          local.Contractor.Item.Gcontractor.ContractorNumber))
        {
          local.Contractor.Update.Gcontractor.PepDenominator =
            (local.Contractor.Item.Gcontractor.PepDenominator ?? 0) + (
              local.Local1.Item.G.PepDenominator ?? 0);
          local.Contractor.Update.Gcontractor.PepNumerator =
            (local.Contractor.Item.Gcontractor.PepNumerator ?? 0) + (
              local.Local1.Item.G.PepNumerator ?? 0);

          goto Next;
        }
      }

      local.Contractor.CheckIndex();

Next:
      ;
    }

    local.Local1.CheckIndex();

    // ------------------------------------------------------------------------------
    // --                    I M P O R T A N T     N O T E
    // --
    // -- The logic below is rolling up previous year JD data to the Contractor 
    // level
    // -- based on the Contractor hierarchy as it exists TODAY, not the 
    // hierarchy as
    // -- it existed last year.  Therefore, if the contractor hierarchy has 
    // changed
    // -- then the values derived in this logic will not match the reported 
    // Dashboard
    // -- values for the previous year for the contractor.
    // ------------------------------------------------------------------------------
    // ------------------------------------------------------------------------------
    // -- Rollup previous year Judicial District counts to the Contractor level.
    // ------------------------------------------------------------------------------
    foreach(var _ in ReadDashboardStagingPriority5())
    {
      local.DashboardAuditData.JudicialDistrict =
        entities.PreviousYear.ReportLevelId;
      UseFnB734DeterContractorFromJd();

      // -- Add previous years PEP values to appropriate contractor.
      for(local.Contractor.Index = 0; local.Contractor.Index < local
        .Contractor.Count; ++local.Contractor.Index)
      {
        if (!local.Contractor.CheckSize())
        {
          break;
        }

        if (Equal(local.Contractor.Item.Gcontractor.ContractorNumber,
          local.Contractor1.Code))
        {
          local.Contractor.Update.Gcontractor.PrevYrPepNumerator =
            (local.Contractor.Item.Gcontractor.PrevYrPepNumerator ?? 0) + (
              entities.PreviousYear.PepNumerator ?? 0);
          local.Contractor.Update.Gcontractor.PrevYrPepDenominator =
            (local.Contractor.Item.Gcontractor.PrevYrPepDenominator ?? 0) + (
              entities.PreviousYear.PepDenominator ?? 0);

          goto ReadEach4;
        }
      }

      local.Contractor.CheckIndex();

ReadEach4:
      ;
    }

    // ------------------------------------------------------------------------------
    // -- Store final Contractor counts.
    // ------------------------------------------------------------------------------
    for(local.Contractor.Index = 0; local.Contractor.Index < local
      .Contractor.Count; ++local.Contractor.Index)
    {
      if (!local.Contractor.CheckSize())
      {
        break;
      }

      if (ReadDashboardStagingPriority6())
      {
        try
        {
          UpdateDashboardStagingPriority3();
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
      else
      {
        try
        {
          CreateDashboardStagingPriority3();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "DASHBOARD_STAGING_PRI_1_2_AE";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "DASHBOARD_STAGING_PRI_1_2_PV";

              break;
            default:
              throw;
          }
        }
      }
    }

    local.Contractor.CheckIndex();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error creating/updating Dashboard_Staging_Priority_1_2.";
      UseCabErrorReport();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // ------------------------------------------------------------------------------
    // -- Calculate the Statewide, Judicial District and Contractor's PEP 
    // Percent,
    // -- Previous Year PEP Percent, and Percent Change from the Previous Year.
    // ------------------------------------------------------------------------------
    foreach(var _ in ReadDashboardStagingPriority7())
    {
      local.Temp.Assign(entities.DashboardStagingPriority12);

      // -- Calculate Current Year PEP percent.
      if ((local.Temp.PepDenominator ?? 0) == 0)
      {
        local.Temp.PepPercent = 0;
      }
      else
      {
        local.Temp.PepPercent =
          Math.Round((decimal)(local.Temp.PepNumerator ?? 0) /
          (local.Temp.PepDenominator ?? 0), 3, MidpointRounding.AwayFromZero);
      }

      // -- Read for the previous year PEP values for all but the contractor 
      // level.
      // -- The contractor level previous year values were calculated and stored
      // earlier.
      if (!Equal(entities.DashboardStagingPriority12.ReportLevel, "XJ"))
      {
        if (ReadDashboardStagingPriority8())
        {
          local.Temp.PrevYrPepNumerator = entities.PreviousYear.PepNumerator;
          local.Temp.PrevYrPepDenominator =
            entities.PreviousYear.PepDenominator;
        }
        else
        {
          local.Temp.PrevYrPepNumerator = 0;
          local.Temp.PrevYrPepDenominator = 0;
        }
      }

      // -- Calculate Previous Year PEP percent.
      if ((local.Temp.PrevYrPepDenominator ?? 0) == 0)
      {
        local.Temp.PrevYrPepPercent = 0;
      }
      else
      {
        local.Temp.PrevYrPepPercent =
          Math.Round((decimal)(local.Temp.PrevYrPepNumerator ?? 0) /
          (local.Temp.PrevYrPepDenominator ?? 0), 3,
          MidpointRounding.AwayFromZero);
      }

      // -- Calculate percent change between Current Year PEP percent and 
      // Previous Year PEP percent.
      if ((local.Temp.PrevYrPepPercent ?? 0M) == 0)
      {
        local.Temp.PercentChgBetweenYrsPep = 0;
      }
      else
      {
        local.Temp.PercentChgBetweenYrsPep =
          Math.Round(((local.Temp.PepPercent ?? 0M) - (
            local.Temp.PrevYrPepPercent ?? 0M
          )) /
          (local.Temp.PrevYrPepPercent ?? 0M), 3,
          MidpointRounding.AwayFromZero);
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
    local.PrevRank.PepPercent = 0;
    local.Temp.PepRank = 1;

    // ------------------------------------------------------------------------------
    // -- Calculate Judicial District Ranking.
    // ------------------------------------------------------------------------------
    foreach(var _ in ReadDashboardStagingPriority9())
    {
      ++local.Common.Count;

      if ((entities.DashboardStagingPriority12.PepPercent ?? 0M) == (
        local.PrevRank.PepPercent ?? 0M))
      {
        // -- The ranking for this judicial district is tied with the previous 
        // judicial district.
        // -- This JD gets the same ranking already in the local_temp 
        // case_under_order_rank.
      }
      else
      {
        local.Temp.PepRank = local.Common.Count;
      }

      try
      {
        UpdateDashboardStagingPriority5();
        local.PrevRank.PepPercent =
          entities.DashboardStagingPriority12.PepPercent;
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

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error creating/updating Dashboard_Staging_Priority_1_2.";
      UseCabErrorReport();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.Common.Count = 0;
    local.PrevRank.PepPercent = 0;
    local.Temp.PepRank = 1;

    // ------------------------------------------------------------------------------
    // -- Calculate Contractor Ranking.
    // ------------------------------------------------------------------------------
    foreach(var _ in ReadDashboardStagingPriority10())
    {
      ++local.Common.Count;

      if ((entities.DashboardStagingPriority12.PepPercent ?? 0M) == (
        local.PrevRank.PepPercent ?? 0M))
      {
        // -- The ranking for this judicial district is tied with the previous 
        // judicial district.
        // -- This JD gets the same ranking already in the local_temp 
        // case_under_order_rank.
      }
      else
      {
        local.Temp.PepRank = local.Common.Count;
      }

      try
      {
        UpdateDashboardStagingPriority5();
        local.PrevRank.PepPercent =
          entities.DashboardStagingPriority12.PepPercent;
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

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error creating/updating Dashboard_Staging_Priority_1_2.";
      UseCabErrorReport();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // ------------------------------------------------------------------------------
    // -- Take a final checkpoint for restarting at the next priority.
    // ------------------------------------------------------------------------------
    local.ProgramCheckpointRestart.RestartInd = "Y";

    // -- Checkpoint Info
    // Positions   Value
    // ---------   
    // ------------------------------------
    //  001-080    General Checkpoint Info for PRAD
    //  081-088    Dashboard Priority
    local.ProgramCheckpointRestart.RestartInd = "Y";
    local.ProgramCheckpointRestart.RestartInfo = "";
    local.ProgramCheckpointRestart.RestartInfo =
      Substring(import.ProgramCheckpointRestart.RestartInfo, 250, 1, 80) + "1-03    ";
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

  private static void MoveDashboardAuditData3(DashboardAuditData source,
    DashboardAuditData target)
  {
    target.Office = source.Office;
    target.JudicialDistrict = source.JudicialDistrict;
    target.CaseNumber = source.CaseNumber;
  }

  private static void MoveDashboardStagingPriority12(
    DashboardStagingPriority12 source, DashboardStagingPriority12 target)
  {
    target.ReportMonth = source.ReportMonth;
    target.ReportLevel = source.ReportLevel;
    target.ReportLevelId = source.ReportLevelId;
    target.AsOfDate = source.AsOfDate;
    target.PepNumerator = source.PepNumerator;
    target.PepDenominator = source.PepDenominator;
    target.PepPercent = source.PepPercent;
    target.PepRank = source.PepRank;
    target.PrevYrPepNumerator = source.PrevYrPepNumerator;
    target.PrevYrPepDenominator = source.PrevYrPepDenominator;
    target.PrevYrPepPercent = source.PrevYrPepPercent;
    target.PercentChgBetweenYrsPep = source.PercentChgBetweenYrsPep;
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

  private void UseCabControlReport()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    context.Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
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

    useImport.DashboardAuditData.Assign(local.DashboardAuditData);

    context.Call(FnB734CreateDashboardAudit.Execute, useImport, useExport);
  }

  private void UseFnB734DeterContractorFromJd()
  {
    var useImport = new FnB734DeterContractorFromJd.Import();
    var useExport = new FnB734DeterContractorFromJd.Export();

    useImport.ReportEndDate.Date = import.ReportEndDate.Date;
    useImport.DashboardAuditData.JudicialDistrict =
      local.DashboardAuditData.JudicialDistrict;

    context.Call(FnB734DeterContractorFromJd.Execute, useImport, useExport);

    MoveCseOrganization(useExport.Contractor, local.Contractor1);
  }

  private void UseFnB734DetermineJdFromCase()
  {
    var useImport = new FnB734DetermineJdFromCase.Import();
    var useExport = new FnB734DetermineJdFromCase.Export();

    useImport.Case1.Number = entities.Case1.Number;
    useImport.ReportEndDate.Date = local.TempEndDate.Date;

    context.Call(FnB734DetermineJdFromCase.Execute, useImport, useExport);

    MoveDashboardAuditData3(useExport.DashboardAuditData,
      local.DashboardAuditData);
  }

  private void UseUpdateCheckpointRstAndCommit()
  {
    var useImport = new UpdateCheckpointRstAndCommit.Import();
    var useExport = new UpdateCheckpointRstAndCommit.Export();

    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);

    context.Call(UpdateCheckpointRstAndCommit.Execute, useImport, useExport);
  }

  private void CreateDashboardStagingPriority1()
  {
    var reportMonth = local.Statewide.ReportMonth;
    var reportLevel = local.Statewide.ReportLevel;
    var reportLevelId = local.Statewide.ReportLevelId;
    var asOfDate = local.Statewide.AsOfDate;
    var param = 0M;
    var pepNumerator = local.Statewide.PepNumerator ?? 0;
    var pepDenominator = local.Statewide.PepDenominator ?? 0;
    var pepPercent = local.Statewide.PepPercent ?? 0M;
    var pepRank = local.Statewide.PepRank ?? 0;

    entities.DashboardStagingPriority12.Populated = false;
    Update("CreateDashboardStagingPriority1",
      (db, command) =>
      {
        db.SetInt32(command, "reportMonth", reportMonth);
        db.SetString(command, "reportLevel", reportLevel);
        db.SetString(command, "reportLevelId", reportLevelId);
        db.SetNullableDate(command, "asOfDt", asOfDate);
        db.SetNullableInt32(command, "casUnderOrdNum", 0);
        db.SetNullableDecimal(command, "casUnderOrdPer", param);
        db.SetNullableInt32(command, "pepNum", pepNumerator);
        db.SetNullableInt32(command, "pepDen", pepDenominator);
        db.SetNullableDecimal(command, "pepPer", pepPercent);
        db.SetNullableDecimal(command, "curSupPdMthNum", param);
        db.SetNullableDecimal(command, "casPerFteDen", param);
        db.SetNullableInt32(command, "pepRank", pepRank);
        db.SetNullableString(command, "contractorNum", "");
        db.SetNullableInt32(command, "prvYrPepNumtr", 0);
        db.SetNullableInt32(command, "prvYrPepDenom", 0);
        db.SetNullableDecimal(command, "prvYrPepPct", param);
        db.SetNullableDecimal(command, "pctChgByrPep", param);
      });

    entities.DashboardStagingPriority12.ReportMonth = reportMonth;
    entities.DashboardStagingPriority12.ReportLevel = reportLevel;
    entities.DashboardStagingPriority12.ReportLevelId = reportLevelId;
    entities.DashboardStagingPriority12.AsOfDate = asOfDate;
    entities.DashboardStagingPriority12.PepNumerator = pepNumerator;
    entities.DashboardStagingPriority12.PepDenominator = pepDenominator;
    entities.DashboardStagingPriority12.PepPercent = pepPercent;
    entities.DashboardStagingPriority12.PepRank = pepRank;
    entities.DashboardStagingPriority12.ContractorNumber = "";
    entities.DashboardStagingPriority12.PrevYrPepNumerator = 0;
    entities.DashboardStagingPriority12.PrevYrPepDenominator = 0;
    entities.DashboardStagingPriority12.PrevYrPepPercent = param;
    entities.DashboardStagingPriority12.PercentChgBetweenYrsPep = param;
    entities.DashboardStagingPriority12.Populated = true;
  }

  private void CreateDashboardStagingPriority2()
  {
    var reportMonth = local.Local1.Item.G.ReportMonth;
    var reportLevel = local.Local1.Item.G.ReportLevel;
    var reportLevelId = local.Local1.Item.G.ReportLevelId;
    var asOfDate = local.Local1.Item.G.AsOfDate;
    var param = 0M;
    var pepNumerator = local.Local1.Item.G.PepNumerator ?? 0;
    var pepDenominator = local.Local1.Item.G.PepDenominator ?? 0;
    var pepPercent = local.Local1.Item.G.PepPercent ?? 0M;
    var pepRank = local.Local1.Item.G.PepRank ?? 0;

    entities.DashboardStagingPriority12.Populated = false;
    Update("CreateDashboardStagingPriority2",
      (db, command) =>
      {
        db.SetInt32(command, "reportMonth", reportMonth);
        db.SetString(command, "reportLevel", reportLevel);
        db.SetString(command, "reportLevelId", reportLevelId);
        db.SetNullableDate(command, "asOfDt", asOfDate);
        db.SetNullableInt32(command, "casUnderOrdNum", 0);
        db.SetNullableDecimal(command, "casUnderOrdPer", param);
        db.SetNullableInt32(command, "pepNum", pepNumerator);
        db.SetNullableInt32(command, "pepDen", pepDenominator);
        db.SetNullableDecimal(command, "pepPer", pepPercent);
        db.SetNullableDecimal(command, "curSupPdMthNum", param);
        db.SetNullableDecimal(command, "casPerFteDen", param);
        db.SetNullableInt32(command, "pepRank", pepRank);
        db.SetNullableString(command, "contractorNum", "");
        db.SetNullableInt32(command, "prvYrPepNumtr", 0);
        db.SetNullableInt32(command, "prvYrPepDenom", 0);
        db.SetNullableDecimal(command, "prvYrPepPct", param);
        db.SetNullableDecimal(command, "pctChgByrPep", param);
      });

    entities.DashboardStagingPriority12.ReportMonth = reportMonth;
    entities.DashboardStagingPriority12.ReportLevel = reportLevel;
    entities.DashboardStagingPriority12.ReportLevelId = reportLevelId;
    entities.DashboardStagingPriority12.AsOfDate = asOfDate;
    entities.DashboardStagingPriority12.PepNumerator = pepNumerator;
    entities.DashboardStagingPriority12.PepDenominator = pepDenominator;
    entities.DashboardStagingPriority12.PepPercent = pepPercent;
    entities.DashboardStagingPriority12.PepRank = pepRank;
    entities.DashboardStagingPriority12.ContractorNumber = "";
    entities.DashboardStagingPriority12.PrevYrPepNumerator = 0;
    entities.DashboardStagingPriority12.PrevYrPepDenominator = 0;
    entities.DashboardStagingPriority12.PrevYrPepPercent = param;
    entities.DashboardStagingPriority12.PercentChgBetweenYrsPep = param;
    entities.DashboardStagingPriority12.Populated = true;
  }

  private void CreateDashboardStagingPriority3()
  {
    var reportMonth = local.Contractor.Item.Gcontractor.ReportMonth;
    var reportLevel = local.Contractor.Item.Gcontractor.ReportLevel;
    var reportLevelId = local.Contractor.Item.Gcontractor.ReportLevelId;
    var asOfDate = local.Contractor.Item.Gcontractor.AsOfDate;
    var param = 0M;
    var pepNumerator = local.Contractor.Item.Gcontractor.PepNumerator ?? 0;
    var pepDenominator = local.Contractor.Item.Gcontractor.PepDenominator ?? 0;
    var pepPercent = local.Contractor.Item.Gcontractor.PepPercent ?? 0M;
    var pepRank = local.Contractor.Item.Gcontractor.PepRank ?? 0;
    var contractorNumber =
      local.Contractor.Item.Gcontractor.ContractorNumber ?? "";
    var prevYrPepNumerator =
      local.Contractor.Item.Gcontractor.PrevYrPepNumerator ?? 0;
    var prevYrPepDenominator =
      local.Contractor.Item.Gcontractor.PrevYrPepDenominator ?? 0;
    var prevYrPepPercent =
      local.Contractor.Item.Gcontractor.PrevYrPepPercent ?? 0M;
    var percentChgBetweenYrsPep =
      local.Contractor.Item.Gcontractor.PercentChgBetweenYrsPep ?? 0M;

    entities.DashboardStagingPriority12.Populated = false;
    Update("CreateDashboardStagingPriority3",
      (db, command) =>
      {
        db.SetInt32(command, "reportMonth", reportMonth);
        db.SetString(command, "reportLevel", reportLevel);
        db.SetString(command, "reportLevelId", reportLevelId);
        db.SetNullableDate(command, "asOfDt", asOfDate);
        db.SetNullableInt32(command, "casUnderOrdNum", 0);
        db.SetNullableDecimal(command, "casUnderOrdPer", param);
        db.SetNullableInt32(command, "pepNum", pepNumerator);
        db.SetNullableInt32(command, "pepDen", pepDenominator);
        db.SetNullableDecimal(command, "pepPer", pepPercent);
        db.SetNullableDecimal(command, "curSupPdMthNum", param);
        db.SetNullableDecimal(command, "casPerFteDen", param);
        db.SetNullableInt32(command, "pepRank", pepRank);
        db.SetNullableString(command, "contractorNum", contractorNumber);
        db.SetNullableInt32(command, "prvYrPepNumtr", prevYrPepNumerator);
        db.SetNullableInt32(command, "prvYrPepDenom", prevYrPepDenominator);
        db.SetNullableDecimal(command, "prvYrPepPct", prevYrPepPercent);
        db.SetNullableDecimal(command, "pctChgByrPep", percentChgBetweenYrsPep);
      });

    entities.DashboardStagingPriority12.ReportMonth = reportMonth;
    entities.DashboardStagingPriority12.ReportLevel = reportLevel;
    entities.DashboardStagingPriority12.ReportLevelId = reportLevelId;
    entities.DashboardStagingPriority12.AsOfDate = asOfDate;
    entities.DashboardStagingPriority12.PepNumerator = pepNumerator;
    entities.DashboardStagingPriority12.PepDenominator = pepDenominator;
    entities.DashboardStagingPriority12.PepPercent = pepPercent;
    entities.DashboardStagingPriority12.PepRank = pepRank;
    entities.DashboardStagingPriority12.ContractorNumber = contractorNumber;
    entities.DashboardStagingPriority12.PrevYrPepNumerator = prevYrPepNumerator;
    entities.DashboardStagingPriority12.PrevYrPepDenominator =
      prevYrPepDenominator;
    entities.DashboardStagingPriority12.PrevYrPepPercent = prevYrPepPercent;
    entities.DashboardStagingPriority12.PercentChgBetweenYrsPep =
      percentChgBetweenYrsPep;
    entities.DashboardStagingPriority12.Populated = true;
  }

  private bool ReadCaseAssignment1()
  {
    entities.CaseAssignment.Populated = false;

    return Read("ReadCaseAssignment1",
      (db, command) =>
      {
        db.SetString(command, "casNo", entities.Case1.Number);
        db.SetDate(command, "effectiveDate", import.ReportEndDate.Date);
        db.SetNullableDate(
          command, "discontinueDate", import.ReportStartDate.Date);
      },
      (db, reader) =>
      {
        entities.CaseAssignment.EffectiveDate = db.GetDate(reader, 0);
        entities.CaseAssignment.DiscontinueDate = db.GetNullableDate(reader, 1);
        entities.CaseAssignment.CreatedTimestamp = db.GetDateTime(reader, 2);
        entities.CaseAssignment.SpdId = db.GetInt32(reader, 3);
        entities.CaseAssignment.OffId = db.GetInt32(reader, 4);
        entities.CaseAssignment.OspCode = db.GetString(reader, 5);
        entities.CaseAssignment.OspDate = db.GetDate(reader, 6);
        entities.CaseAssignment.CasNo = db.GetString(reader, 7);
        entities.CaseAssignment.Populated = true;
      });
  }

  private bool ReadCaseAssignment2()
  {
    entities.CaseAssignment.Populated = false;

    return Read("ReadCaseAssignment2",
      (db, command) =>
      {
        db.SetString(command, "casNo", entities.Case1.Number);
        db.SetDate(command, "effectiveDate", local.PriorFiscalYrEndDate.Date);
        db.SetNullableDate(
          command, "discontinueDate", local.PriorFiscalYrStartDate.Date);
      },
      (db, reader) =>
      {
        entities.CaseAssignment.EffectiveDate = db.GetDate(reader, 0);
        entities.CaseAssignment.DiscontinueDate = db.GetNullableDate(reader, 1);
        entities.CaseAssignment.CreatedTimestamp = db.GetDateTime(reader, 2);
        entities.CaseAssignment.SpdId = db.GetInt32(reader, 3);
        entities.CaseAssignment.OffId = db.GetInt32(reader, 4);
        entities.CaseAssignment.OspCode = db.GetString(reader, 5);
        entities.CaseAssignment.OspDate = db.GetDate(reader, 6);
        entities.CaseAssignment.CasNo = db.GetString(reader, 7);
        entities.CaseAssignment.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCaseRoleCase1()
  {
    return ReadEach("ReadCaseRoleCase1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetNullableDate(command, "startDate", import.ReportEndDate.Date);
        db.SetNullableDate(command, "endDate", import.ReportStartDate.Date);
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.Case1.Number = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.Case1.InterstateCaseId = db.GetNullableString(reader, 7);
        entities.Case1.NoJurisdictionCd = db.GetNullableString(reader, 8);

        if (Equal(entities.CaseRole.Type1, "CH"))
        {
          entities.CaseRole.DateOfEmancipation = db.GetNullableDate(reader, 6);
        }
        else
        {
          entities.CaseRole.DateOfEmancipation = null;
        }

        entities.CaseRole.Populated = true;
        entities.Case1.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);

        return true;
      },
      () =>
      {
        entities.Case1.Populated = false;
        entities.CaseRole.Populated = false;
      });
  }

  private IEnumerable<bool> ReadCaseRoleCase2()
  {
    return ReadEach("ReadCaseRoleCase2",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.
          SetNullableDate(command, "startDate", local.PriorFiscalYrEndDate.Date);
        db.
          SetNullableDate(command, "endDate", local.PriorFiscalYrStartDate.Date);
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.Case1.Number = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.Case1.InterstateCaseId = db.GetNullableString(reader, 7);
        entities.Case1.NoJurisdictionCd = db.GetNullableString(reader, 8);

        if (Equal(entities.CaseRole.Type1, "CH"))
        {
          entities.CaseRole.DateOfEmancipation = db.GetNullableDate(reader, 6);
        }
        else
        {
          entities.CaseRole.DateOfEmancipation = null;
        }

        entities.CaseRole.Populated = true;
        entities.Case1.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);

        return true;
      },
      () =>
      {
        entities.Case1.Populated = false;
        entities.CaseRole.Populated = false;
      });
  }

  private IEnumerable<bool> ReadCseOrganization()
  {
    return ReadEach("ReadCseOrganization",
      null,
      (db, reader) =>
      {
        entities.CseOrganization.Code = db.GetString(reader, 0);
        entities.CseOrganization.Type1 = db.GetString(reader, 1);
        entities.CseOrganization.Populated = true;

        return true;
      },
      () =>
      {
        entities.CseOrganization.Populated = false;
      });
  }

  private IEnumerable<bool> ReadCsePerson()
  {
    return ReadEachInSeparateTransaction("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", local.RestartNumerator.Number);
        db.SetNullableDate(command, "startDate", import.ReportEndDate.Date);
        db.SetNullableDate(command, "endDate", import.ReportStartDate.Date);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);

        if (AsChar(entities.CsePerson.Type1) == 'C')
        {
          entities.CsePerson.BornOutOfWedlock = db.GetNullableString(reader, 2);
          entities.CsePerson.PaternityEstablishedIndicator =
            db.GetNullableString(reader, 3);
        }
        else
        {
          entities.CsePerson.BornOutOfWedlock = "";
          entities.CsePerson.PaternityEstablishedIndicator = "";
        }

        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);

        return true;
      },
      () =>
      {
        entities.CsePerson.Populated = false;
      });
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
        entities.DashboardStagingPriority12.PepNumerator =
          db.GetNullableInt32(reader, 4);
        entities.DashboardStagingPriority12.PepDenominator =
          db.GetNullableInt32(reader, 5);
        entities.DashboardStagingPriority12.PepPercent =
          db.GetNullableDecimal(reader, 6);
        entities.DashboardStagingPriority12.PepRank =
          db.GetNullableInt32(reader, 7);
        entities.DashboardStagingPriority12.ContractorNumber =
          db.GetNullableString(reader, 8);
        entities.DashboardStagingPriority12.PrevYrPepNumerator =
          db.GetNullableInt32(reader, 9);
        entities.DashboardStagingPriority12.PrevYrPepDenominator =
          db.GetNullableInt32(reader, 10);
        entities.DashboardStagingPriority12.PrevYrPepPercent =
          db.GetNullableDecimal(reader, 11);
        entities.DashboardStagingPriority12.PercentChgBetweenYrsPep =
          db.GetNullableDecimal(reader, 12);
        entities.DashboardStagingPriority12.Populated = true;

        return true;
      },
      () =>
      {
        entities.DashboardStagingPriority12.Populated = false;
      });
  }

  private IEnumerable<bool> ReadDashboardStagingPriority10()
  {
    return ReadEach("ReadDashboardStagingPriority10",
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
        entities.DashboardStagingPriority12.PepNumerator =
          db.GetNullableInt32(reader, 4);
        entities.DashboardStagingPriority12.PepDenominator =
          db.GetNullableInt32(reader, 5);
        entities.DashboardStagingPriority12.PepPercent =
          db.GetNullableDecimal(reader, 6);
        entities.DashboardStagingPriority12.PepRank =
          db.GetNullableInt32(reader, 7);
        entities.DashboardStagingPriority12.ContractorNumber =
          db.GetNullableString(reader, 8);
        entities.DashboardStagingPriority12.PrevYrPepNumerator =
          db.GetNullableInt32(reader, 9);
        entities.DashboardStagingPriority12.PrevYrPepDenominator =
          db.GetNullableInt32(reader, 10);
        entities.DashboardStagingPriority12.PrevYrPepPercent =
          db.GetNullableDecimal(reader, 11);
        entities.DashboardStagingPriority12.PercentChgBetweenYrsPep =
          db.GetNullableDecimal(reader, 12);
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
        entities.DashboardStagingPriority12.PepNumerator =
          db.GetNullableInt32(reader, 4);
        entities.DashboardStagingPriority12.PepDenominator =
          db.GetNullableInt32(reader, 5);
        entities.DashboardStagingPriority12.PepPercent =
          db.GetNullableDecimal(reader, 6);
        entities.DashboardStagingPriority12.PepRank =
          db.GetNullableInt32(reader, 7);
        entities.DashboardStagingPriority12.ContractorNumber =
          db.GetNullableString(reader, 8);
        entities.DashboardStagingPriority12.PrevYrPepNumerator =
          db.GetNullableInt32(reader, 9);
        entities.DashboardStagingPriority12.PrevYrPepDenominator =
          db.GetNullableInt32(reader, 10);
        entities.DashboardStagingPriority12.PrevYrPepPercent =
          db.GetNullableDecimal(reader, 11);
        entities.DashboardStagingPriority12.PercentChgBetweenYrsPep =
          db.GetNullableDecimal(reader, 12);
        entities.DashboardStagingPriority12.Populated = true;

        return true;
      },
      () =>
      {
        entities.DashboardStagingPriority12.Populated = false;
      });
  }

  private bool ReadDashboardStagingPriority3()
  {
    entities.DashboardStagingPriority12.Populated = false;

    return Read("ReadDashboardStagingPriority3",
      (db, command) =>
      {
        db.SetInt32(command, "reportMonth", local.Statewide.ReportMonth);
        db.SetString(command, "reportLevel", local.Statewide.ReportLevel);
        db.SetString(command, "reportLevelId", local.Statewide.ReportLevelId);
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
        entities.DashboardStagingPriority12.PepNumerator =
          db.GetNullableInt32(reader, 4);
        entities.DashboardStagingPriority12.PepDenominator =
          db.GetNullableInt32(reader, 5);
        entities.DashboardStagingPriority12.PepPercent =
          db.GetNullableDecimal(reader, 6);
        entities.DashboardStagingPriority12.PepRank =
          db.GetNullableInt32(reader, 7);
        entities.DashboardStagingPriority12.ContractorNumber =
          db.GetNullableString(reader, 8);
        entities.DashboardStagingPriority12.PrevYrPepNumerator =
          db.GetNullableInt32(reader, 9);
        entities.DashboardStagingPriority12.PrevYrPepDenominator =
          db.GetNullableInt32(reader, 10);
        entities.DashboardStagingPriority12.PrevYrPepPercent =
          db.GetNullableDecimal(reader, 11);
        entities.DashboardStagingPriority12.PercentChgBetweenYrsPep =
          db.GetNullableDecimal(reader, 12);
        entities.DashboardStagingPriority12.Populated = true;
      });
  }

  private bool ReadDashboardStagingPriority4()
  {
    entities.DashboardStagingPriority12.Populated = false;

    return Read("ReadDashboardStagingPriority4",
      (db, command) =>
      {
        db.SetInt32(command, "reportMonth", local.Local1.Item.G.ReportMonth);
        db.SetString(command, "reportLevel", local.Local1.Item.G.ReportLevel);
        db.
          SetString(command, "reportLevelId", local.Local1.Item.G.ReportLevelId);
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
        entities.DashboardStagingPriority12.PepNumerator =
          db.GetNullableInt32(reader, 4);
        entities.DashboardStagingPriority12.PepDenominator =
          db.GetNullableInt32(reader, 5);
        entities.DashboardStagingPriority12.PepPercent =
          db.GetNullableDecimal(reader, 6);
        entities.DashboardStagingPriority12.PepRank =
          db.GetNullableInt32(reader, 7);
        entities.DashboardStagingPriority12.ContractorNumber =
          db.GetNullableString(reader, 8);
        entities.DashboardStagingPriority12.PrevYrPepNumerator =
          db.GetNullableInt32(reader, 9);
        entities.DashboardStagingPriority12.PrevYrPepDenominator =
          db.GetNullableInt32(reader, 10);
        entities.DashboardStagingPriority12.PrevYrPepPercent =
          db.GetNullableDecimal(reader, 11);
        entities.DashboardStagingPriority12.PercentChgBetweenYrsPep =
          db.GetNullableDecimal(reader, 12);
        entities.DashboardStagingPriority12.Populated = true;
      });
  }

  private IEnumerable<bool> ReadDashboardStagingPriority5()
  {
    return ReadEach("ReadDashboardStagingPriority5",
      (db, command) =>
      {
        db.SetInt32(command, "reportMonth", local.PreviousYear.ReportMonth);
      },
      (db, reader) =>
      {
        entities.PreviousYear.ReportMonth = db.GetInt32(reader, 0);
        entities.PreviousYear.ReportLevel = db.GetString(reader, 1);
        entities.PreviousYear.ReportLevelId = db.GetString(reader, 2);
        entities.PreviousYear.PepNumerator = db.GetNullableInt32(reader, 3);
        entities.PreviousYear.PepDenominator = db.GetNullableInt32(reader, 4);
        entities.PreviousYear.PepPercent = db.GetNullableDecimal(reader, 5);
        entities.PreviousYear.Populated = true;

        return true;
      },
      () =>
      {
        entities.PreviousYear.Populated = false;
      });
  }

  private bool ReadDashboardStagingPriority6()
  {
    entities.DashboardStagingPriority12.Populated = false;

    return Read("ReadDashboardStagingPriority6",
      (db, command) =>
      {
        db.SetInt32(
          command, "reportMonth",
          local.Contractor.Item.Gcontractor.ReportMonth);
        db.SetString(
          command, "reportLevel",
          local.Contractor.Item.Gcontractor.ReportLevel);
        db.SetString(
          command, "reportLevelId",
          local.Contractor.Item.Gcontractor.ReportLevelId);
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
        entities.DashboardStagingPriority12.PepNumerator =
          db.GetNullableInt32(reader, 4);
        entities.DashboardStagingPriority12.PepDenominator =
          db.GetNullableInt32(reader, 5);
        entities.DashboardStagingPriority12.PepPercent =
          db.GetNullableDecimal(reader, 6);
        entities.DashboardStagingPriority12.PepRank =
          db.GetNullableInt32(reader, 7);
        entities.DashboardStagingPriority12.ContractorNumber =
          db.GetNullableString(reader, 8);
        entities.DashboardStagingPriority12.PrevYrPepNumerator =
          db.GetNullableInt32(reader, 9);
        entities.DashboardStagingPriority12.PrevYrPepDenominator =
          db.GetNullableInt32(reader, 10);
        entities.DashboardStagingPriority12.PrevYrPepPercent =
          db.GetNullableDecimal(reader, 11);
        entities.DashboardStagingPriority12.PercentChgBetweenYrsPep =
          db.GetNullableDecimal(reader, 12);
        entities.DashboardStagingPriority12.Populated = true;
      });
  }

  private IEnumerable<bool> ReadDashboardStagingPriority7()
  {
    return ReadEach("ReadDashboardStagingPriority7",
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
        entities.DashboardStagingPriority12.PepNumerator =
          db.GetNullableInt32(reader, 4);
        entities.DashboardStagingPriority12.PepDenominator =
          db.GetNullableInt32(reader, 5);
        entities.DashboardStagingPriority12.PepPercent =
          db.GetNullableDecimal(reader, 6);
        entities.DashboardStagingPriority12.PepRank =
          db.GetNullableInt32(reader, 7);
        entities.DashboardStagingPriority12.ContractorNumber =
          db.GetNullableString(reader, 8);
        entities.DashboardStagingPriority12.PrevYrPepNumerator =
          db.GetNullableInt32(reader, 9);
        entities.DashboardStagingPriority12.PrevYrPepDenominator =
          db.GetNullableInt32(reader, 10);
        entities.DashboardStagingPriority12.PrevYrPepPercent =
          db.GetNullableDecimal(reader, 11);
        entities.DashboardStagingPriority12.PercentChgBetweenYrsPep =
          db.GetNullableDecimal(reader, 12);
        entities.DashboardStagingPriority12.Populated = true;

        return true;
      },
      () =>
      {
        entities.DashboardStagingPriority12.Populated = false;
      });
  }

  private bool ReadDashboardStagingPriority8()
  {
    entities.PreviousYear.Populated = false;

    return Read("ReadDashboardStagingPriority8",
      (db, command) =>
      {
        db.SetString(
          command, "reportLevel",
          entities.DashboardStagingPriority12.ReportLevel);
        db.SetString(
          command, "reportLevelId",
          entities.DashboardStagingPriority12.ReportLevelId);
        db.SetInt32(command, "reportMonth", local.PreviousYear.ReportMonth);
      },
      (db, reader) =>
      {
        entities.PreviousYear.ReportMonth = db.GetInt32(reader, 0);
        entities.PreviousYear.ReportLevel = db.GetString(reader, 1);
        entities.PreviousYear.ReportLevelId = db.GetString(reader, 2);
        entities.PreviousYear.PepNumerator = db.GetNullableInt32(reader, 3);
        entities.PreviousYear.PepDenominator = db.GetNullableInt32(reader, 4);
        entities.PreviousYear.PepPercent = db.GetNullableDecimal(reader, 5);
        entities.PreviousYear.Populated = true;
      });
  }

  private IEnumerable<bool> ReadDashboardStagingPriority9()
  {
    return ReadEach("ReadDashboardStagingPriority9",
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
        entities.DashboardStagingPriority12.PepNumerator =
          db.GetNullableInt32(reader, 4);
        entities.DashboardStagingPriority12.PepDenominator =
          db.GetNullableInt32(reader, 5);
        entities.DashboardStagingPriority12.PepPercent =
          db.GetNullableDecimal(reader, 6);
        entities.DashboardStagingPriority12.PepRank =
          db.GetNullableInt32(reader, 7);
        entities.DashboardStagingPriority12.ContractorNumber =
          db.GetNullableString(reader, 8);
        entities.DashboardStagingPriority12.PrevYrPepNumerator =
          db.GetNullableInt32(reader, 9);
        entities.DashboardStagingPriority12.PrevYrPepDenominator =
          db.GetNullableInt32(reader, 10);
        entities.DashboardStagingPriority12.PrevYrPepPercent =
          db.GetNullableDecimal(reader, 11);
        entities.DashboardStagingPriority12.PercentChgBetweenYrsPep =
          db.GetNullableDecimal(reader, 12);
        entities.DashboardStagingPriority12.Populated = true;

        return true;
      },
      () =>
      {
        entities.DashboardStagingPriority12.Populated = false;
      });
  }

  private IEnumerable<bool> ReadGoodCauseCaseRoleGoodCause1()
  {
    return ReadEach("ReadGoodCauseCaseRoleGoodCause1",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableDate(command, "effectiveDate", import.ReportEndDate.Date);
        db.SetDateTime(
          command, "createdTimestamp", import.ReportEndDate.Timestamp);
      },
      (db, reader) =>
      {
        entities.GoodCause.Code = db.GetNullableString(reader, 0);
        entities.GoodCause.EffectiveDate = db.GetNullableDate(reader, 1);
        entities.GoodCause.DiscontinueDate = db.GetNullableDate(reader, 2);
        entities.GoodCause.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.GoodCause.CasNumber = db.GetString(reader, 4);
        entities.Ar.CasNumber = db.GetString(reader, 4);
        entities.GoodCause.CspNumber = db.GetString(reader, 5);
        entities.Ar.CspNumber = db.GetString(reader, 5);
        entities.GoodCause.CroType = db.GetString(reader, 6);
        entities.Ar.Type1 = db.GetString(reader, 6);
        entities.GoodCause.CroIdentifier = db.GetInt32(reader, 7);
        entities.Ar.Identifier = db.GetInt32(reader, 7);
        entities.Ar.StartDate = db.GetNullableDate(reader, 8);
        entities.Ar.EndDate = db.GetNullableDate(reader, 9);
        entities.Next.Code = db.GetNullableString(reader, 10);
        entities.Next.EffectiveDate = db.GetNullableDate(reader, 11);
        entities.Next.DiscontinueDate = db.GetNullableDate(reader, 12);
        entities.Next.CreatedTimestamp = db.GetDateTime(reader, 13);
        entities.Next.CasNumber = db.GetString(reader, 14);
        entities.Next.CspNumber = db.GetString(reader, 15);
        entities.Next.CroType = db.GetString(reader, 16);
        entities.Next.CroIdentifier = db.GetInt32(reader, 17);
        entities.GoodCause.Populated = true;
        entities.Ar.Populated = true;
        entities.Next.Populated = db.GetNullableString(reader, 14) != null;
        CheckValid<CaseRole>("Type1", entities.Ar.Type1);

        return true;
      },
      () =>
      {
        entities.Next.Populated = false;
        entities.Ar.Populated = false;
        entities.GoodCause.Populated = false;
      });
  }

  private IEnumerable<bool> ReadGoodCauseCaseRoleGoodCause2()
  {
    return ReadEach("ReadGoodCauseCaseRoleGoodCause2",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableDate(
          command, "effectiveDate", local.PriorFiscalYrEndDate.Date);
        db.SetDateTime(
          command, "createdTimestamp", local.PriorFiscalYrEndDate.Timestamp);
      },
      (db, reader) =>
      {
        entities.GoodCause.Code = db.GetNullableString(reader, 0);
        entities.GoodCause.EffectiveDate = db.GetNullableDate(reader, 1);
        entities.GoodCause.DiscontinueDate = db.GetNullableDate(reader, 2);
        entities.GoodCause.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.GoodCause.CasNumber = db.GetString(reader, 4);
        entities.Ar.CasNumber = db.GetString(reader, 4);
        entities.GoodCause.CspNumber = db.GetString(reader, 5);
        entities.Ar.CspNumber = db.GetString(reader, 5);
        entities.GoodCause.CroType = db.GetString(reader, 6);
        entities.Ar.Type1 = db.GetString(reader, 6);
        entities.GoodCause.CroIdentifier = db.GetInt32(reader, 7);
        entities.Ar.Identifier = db.GetInt32(reader, 7);
        entities.Ar.StartDate = db.GetNullableDate(reader, 8);
        entities.Ar.EndDate = db.GetNullableDate(reader, 9);
        entities.Next.Code = db.GetNullableString(reader, 10);
        entities.Next.EffectiveDate = db.GetNullableDate(reader, 11);
        entities.Next.DiscontinueDate = db.GetNullableDate(reader, 12);
        entities.Next.CreatedTimestamp = db.GetDateTime(reader, 13);
        entities.Next.CasNumber = db.GetString(reader, 14);
        entities.Next.CspNumber = db.GetString(reader, 15);
        entities.Next.CroType = db.GetString(reader, 16);
        entities.Next.CroIdentifier = db.GetInt32(reader, 17);
        entities.GoodCause.Populated = true;
        entities.Ar.Populated = true;
        entities.Next.Populated = db.GetNullableString(reader, 14) != null;
        CheckValid<CaseRole>("Type1", entities.Ar.Type1);

        return true;
      },
      () =>
      {
        entities.Next.Populated = false;
        entities.Ar.Populated = false;
        entities.GoodCause.Populated = false;
      });
  }

  private bool ReadOcse157Data()
  {
    entities.Ocse157Data.Populated = false;

    return Read("ReadOcse157Data",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "fiscalYear", local.Ocse157Data.FiscalYear ?? 0);
        db.SetNullableString(
          command, "lineNumber", local.Ocse157Data.LineNumber ?? "");
      },
      (db, reader) =>
      {
        entities.Ocse157Data.FiscalYear = db.GetNullableInt32(reader, 0);
        entities.Ocse157Data.RunNumber = db.GetNullableInt32(reader, 1);
        entities.Ocse157Data.LineNumber = db.GetNullableString(reader, 2);
        entities.Ocse157Data.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.Ocse157Data.Number = db.GetNullableInt64(reader, 4);
        entities.Ocse157Data.Populated = true;
      });
  }

  private IEnumerable<bool> ReadOcse157VerificationCsePerson()
  {
    return ReadEachInSeparateTransaction("ReadOcse157VerificationCsePerson",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "fiscalYear", local.Ocse157Data.FiscalYear ?? 0);
        db.SetNullableInt32(
          command, "runNumber", local.Ocse157Data.RunNumber ?? 0);
        db.SetNullableString(
          command, "lineNumber", local.Ocse157Data.LineNumber ?? "");
        db.SetNullableString(
          command, "suppPersonNumber", local.RestartDenominator.Number);
      },
      (db, reader) =>
      {
        entities.Ocse157Verification.FiscalYear =
          db.GetNullableInt32(reader, 0);
        entities.Ocse157Verification.RunNumber = db.GetNullableInt32(reader, 1);
        entities.Ocse157Verification.LineNumber =
          db.GetNullableString(reader, 2);
        entities.Ocse157Verification.CreatedTimestamp =
          db.GetDateTime(reader, 3);
        entities.Ocse157Verification.SuppPersonNumber =
          db.GetNullableString(reader, 4);
        entities.CsePerson.Number = db.GetString(reader, 5);
        entities.CsePerson.Type1 = db.GetString(reader, 6);

        if (AsChar(entities.CsePerson.Type1) == 'C')
        {
          entities.CsePerson.BornOutOfWedlock = db.GetNullableString(reader, 7);
          entities.CsePerson.PaternityEstablishedIndicator =
            db.GetNullableString(reader, 8);
        }
        else
        {
          entities.CsePerson.BornOutOfWedlock = "";
          entities.CsePerson.PaternityEstablishedIndicator = "";
        }

        entities.Ocse157Verification.Populated = true;
        entities.CsePerson.Populated = db.GetNullableString(reader, 5) != null;

        if (entities.CsePerson.Populated)
        {
          CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
        }

        return true;
      },
      () =>
      {
        entities.Ocse157Verification.Populated = false;
        entities.CsePerson.Populated = false;
      });
  }

  private void UpdateDashboardStagingPriority1()
  {
    var asOfDate = local.Statewide.AsOfDate;
    var pepNumerator = local.Statewide.PepNumerator ?? 0;
    var pepDenominator = local.Statewide.PepDenominator ?? 0;
    var pepPercent = local.Statewide.PepPercent ?? 0M;
    var pepRank = local.Statewide.PepRank ?? 0;

    entities.DashboardStagingPriority12.Populated = false;
    Update("UpdateDashboardStagingPriority1",
      (db, command) =>
      {
        db.SetNullableDate(command, "asOfDt", asOfDate);
        db.SetNullableInt32(command, "pepNum", pepNumerator);
        db.SetNullableInt32(command, "pepDen", pepDenominator);
        db.SetNullableDecimal(command, "pepPer", pepPercent);
        db.SetNullableInt32(command, "pepRank", pepRank);
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

    entities.DashboardStagingPriority12.AsOfDate = asOfDate;
    entities.DashboardStagingPriority12.PepNumerator = pepNumerator;
    entities.DashboardStagingPriority12.PepDenominator = pepDenominator;
    entities.DashboardStagingPriority12.PepPercent = pepPercent;
    entities.DashboardStagingPriority12.PepRank = pepRank;
    entities.DashboardStagingPriority12.Populated = true;
  }

  private void UpdateDashboardStagingPriority2()
  {
    var asOfDate = local.Local1.Item.G.AsOfDate;
    var pepNumerator = local.Local1.Item.G.PepNumerator ?? 0;
    var pepDenominator = local.Local1.Item.G.PepDenominator ?? 0;
    var pepPercent = local.Local1.Item.G.PepPercent ?? 0M;
    var pepRank = local.Local1.Item.G.PepRank ?? 0;

    entities.DashboardStagingPriority12.Populated = false;
    Update("UpdateDashboardStagingPriority2",
      (db, command) =>
      {
        db.SetNullableDate(command, "asOfDt", asOfDate);
        db.SetNullableInt32(command, "pepNum", pepNumerator);
        db.SetNullableInt32(command, "pepDen", pepDenominator);
        db.SetNullableDecimal(command, "pepPer", pepPercent);
        db.SetNullableInt32(command, "pepRank", pepRank);
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

    entities.DashboardStagingPriority12.AsOfDate = asOfDate;
    entities.DashboardStagingPriority12.PepNumerator = pepNumerator;
    entities.DashboardStagingPriority12.PepDenominator = pepDenominator;
    entities.DashboardStagingPriority12.PepPercent = pepPercent;
    entities.DashboardStagingPriority12.PepRank = pepRank;
    entities.DashboardStagingPriority12.Populated = true;
  }

  private void UpdateDashboardStagingPriority3()
  {
    var asOfDate = local.Contractor.Item.Gcontractor.AsOfDate;
    var pepNumerator = local.Contractor.Item.Gcontractor.PepNumerator ?? 0;
    var pepDenominator = local.Contractor.Item.Gcontractor.PepDenominator ?? 0;
    var pepPercent = local.Contractor.Item.Gcontractor.PepPercent ?? 0M;
    var pepRank = local.Contractor.Item.Gcontractor.PepRank ?? 0;
    var prevYrPepNumerator =
      local.Contractor.Item.Gcontractor.PrevYrPepNumerator ?? 0;
    var prevYrPepDenominator =
      local.Contractor.Item.Gcontractor.PrevYrPepDenominator ?? 0;
    var prevYrPepPercent =
      local.Contractor.Item.Gcontractor.PrevYrPepPercent ?? 0M;
    var percentChgBetweenYrsPep =
      local.Contractor.Item.Gcontractor.PercentChgBetweenYrsPep ?? 0M;

    entities.DashboardStagingPriority12.Populated = false;
    Update("UpdateDashboardStagingPriority3",
      (db, command) =>
      {
        db.SetNullableDate(command, "asOfDt", asOfDate);
        db.SetNullableInt32(command, "pepNum", pepNumerator);
        db.SetNullableInt32(command, "pepDen", pepDenominator);
        db.SetNullableDecimal(command, "pepPer", pepPercent);
        db.SetNullableInt32(command, "pepRank", pepRank);
        db.SetNullableInt32(command, "prvYrPepNumtr", prevYrPepNumerator);
        db.SetNullableInt32(command, "prvYrPepDenom", prevYrPepDenominator);
        db.SetNullableDecimal(command, "prvYrPepPct", prevYrPepPercent);
        db.SetNullableDecimal(command, "pctChgByrPep", percentChgBetweenYrsPep);
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

    entities.DashboardStagingPriority12.AsOfDate = asOfDate;
    entities.DashboardStagingPriority12.PepNumerator = pepNumerator;
    entities.DashboardStagingPriority12.PepDenominator = pepDenominator;
    entities.DashboardStagingPriority12.PepPercent = pepPercent;
    entities.DashboardStagingPriority12.PepRank = pepRank;
    entities.DashboardStagingPriority12.PrevYrPepNumerator = prevYrPepNumerator;
    entities.DashboardStagingPriority12.PrevYrPepDenominator =
      prevYrPepDenominator;
    entities.DashboardStagingPriority12.PrevYrPepPercent = prevYrPepPercent;
    entities.DashboardStagingPriority12.PercentChgBetweenYrsPep =
      percentChgBetweenYrsPep;
    entities.DashboardStagingPriority12.Populated = true;
  }

  private void UpdateDashboardStagingPriority4()
  {
    var pepPercent = local.Temp.PepPercent ?? 0M;
    var prevYrPepNumerator = local.Temp.PrevYrPepNumerator ?? 0;
    var prevYrPepDenominator = local.Temp.PrevYrPepDenominator ?? 0;
    var prevYrPepPercent = local.Temp.PrevYrPepPercent ?? 0M;
    var percentChgBetweenYrsPep = local.Temp.PercentChgBetweenYrsPep ?? 0M;

    entities.DashboardStagingPriority12.Populated = false;
    Update("UpdateDashboardStagingPriority4",
      (db, command) =>
      {
        db.SetNullableDecimal(command, "pepPer", pepPercent);
        db.SetNullableInt32(command, "prvYrPepNumtr", prevYrPepNumerator);
        db.SetNullableInt32(command, "prvYrPepDenom", prevYrPepDenominator);
        db.SetNullableDecimal(command, "prvYrPepPct", prevYrPepPercent);
        db.SetNullableDecimal(command, "pctChgByrPep", percentChgBetweenYrsPep);
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

    entities.DashboardStagingPriority12.PepPercent = pepPercent;
    entities.DashboardStagingPriority12.PrevYrPepNumerator = prevYrPepNumerator;
    entities.DashboardStagingPriority12.PrevYrPepDenominator =
      prevYrPepDenominator;
    entities.DashboardStagingPriority12.PrevYrPepPercent = prevYrPepPercent;
    entities.DashboardStagingPriority12.PercentChgBetweenYrsPep =
      percentChgBetweenYrsPep;
    entities.DashboardStagingPriority12.Populated = true;
  }

  private void UpdateDashboardStagingPriority5()
  {
    var pepRank = local.Temp.PepRank ?? 0;

    entities.DashboardStagingPriority12.Populated = false;
    Update("UpdateDashboardStagingPriority5",
      (db, command) =>
      {
        db.SetNullableInt32(command, "pepRank", pepRank);
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

    entities.DashboardStagingPriority12.PepRank = pepRank;
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
    /// A value of ProgramProcessingInfo.
    /// </summary>
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
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
    /// A value of FiscalYearStartDate.
    /// </summary>
    public DateWorkArea FiscalYearStartDate
    {
      get => fiscalYearStartDate ??= new();
      set => fiscalYearStartDate = value;
    }

    /// <summary>
    /// A value of FiscalYearEndDate.
    /// </summary>
    public DateWorkArea FiscalYearEndDate
    {
      get => fiscalYearEndDate ??= new();
      set => fiscalYearEndDate = value;
    }

    /// <summary>
    /// A value of AuditFlag.
    /// </summary>
    public Common AuditFlag
    {
      get => auditFlag ??= new();
      set => auditFlag = value;
    }

    private DashboardAuditData? dashboardAuditData;
    private ProgramCheckpointRestart? programCheckpointRestart;
    private ProgramProcessingInfo? programProcessingInfo;
    private DateWorkArea? reportStartDate;
    private DateWorkArea? reportEndDate;
    private DateWorkArea? fiscalYearStartDate;
    private DateWorkArea? fiscalYearEndDate;
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
    /// <summary>A ChildInJdsGroup group.</summary>
    [Serializable]
    public class ChildInJdsGroup
    {
      /// <summary>
      /// A value of GlocalChildInJd.
      /// </summary>
      public Common GlocalChildInJd
      {
        get => glocalChildInJd ??= new();
        set => glocalChildInJd = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private Common? glocalChildInJd;
    }

    /// <summary>A LocalGroup group.</summary>
    [Serializable]
    public class LocalGroup
    {
      /// <summary>
      /// A value of G.
      /// </summary>
      public DashboardStagingPriority12 G
      {
        get => g ??= new();
        set => g = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private DashboardStagingPriority12? g;
    }

    /// <summary>A ContractorGroup group.</summary>
    [Serializable]
    public class ContractorGroup
    {
      /// <summary>
      /// A value of Gcontractor.
      /// </summary>
      public DashboardStagingPriority12 Gcontractor
      {
        get => gcontractor ??= new();
        set => gcontractor = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private DashboardStagingPriority12? gcontractor;
    }

    /// <summary>
    /// A value of RestartDenominator.
    /// </summary>
    public CsePerson RestartDenominator
    {
      get => restartDenominator ??= new();
      set => restartDenominator = value;
    }

    /// <summary>
    /// A value of RestartNumerator.
    /// </summary>
    public CsePerson RestartNumerator
    {
      get => restartNumerator ??= new();
      set => restartNumerator = value;
    }

    /// <summary>
    /// A value of RestartSection.
    /// </summary>
    public TextWorkArea RestartSection
    {
      get => restartSection ??= new();
      set => restartSection = value;
    }

    /// <summary>
    /// A value of PriorFiscalYrStartDate.
    /// </summary>
    public DateWorkArea PriorFiscalYrStartDate
    {
      get => priorFiscalYrStartDate ??= new();
      set => priorFiscalYrStartDate = value;
    }

    /// <summary>
    /// A value of PriorFiscalYrEndDate.
    /// </summary>
    public DateWorkArea PriorFiscalYrEndDate
    {
      get => priorFiscalYrEndDate ??= new();
      set => priorFiscalYrEndDate = value;
    }

    /// <summary>
    /// A value of Ocse157Verifi.
    /// </summary>
    public Common Ocse157Verifi
    {
      get => ocse157Verifi ??= new();
      set => ocse157Verifi = value;
    }

    /// <summary>
    /// A value of Ocse157Data.
    /// </summary>
    public Ocse157Data Ocse157Data
    {
      get => ocse157Data ??= new();
      set => ocse157Data = value;
    }

    /// <summary>
    /// Gets a value of ChildInJds.
    /// </summary>
    [JsonIgnore]
    public Array<ChildInJdsGroup> ChildInJds => childInJds ??= new(
      ChildInJdsGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of ChildInJds for json serialization.
    /// </summary>
    [JsonPropertyName("childInJds")]
    [Computed]
    public IList<ChildInJdsGroup>? ChildInJds_Json
    {
      get => childInJds;
      set => ChildInJds.Assign(value);
    }

    /// <summary>
    /// A value of ChildCaseCount.
    /// </summary>
    public Common ChildCaseCount
    {
      get => childCaseCount ??= new();
      set => childCaseCount = value;
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
    /// A value of ProgramCheckpointRestart.
    /// </summary>
    public ProgramCheckpointRestart ProgramCheckpointRestart
    {
      get => programCheckpointRestart ??= new();
      set => programCheckpointRestart = value;
    }

    /// <summary>
    /// A value of Statewide.
    /// </summary>
    public DashboardStagingPriority12 Statewide
    {
      get => statewide ??= new();
      set => statewide = value;
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
    /// Gets a value of Local1.
    /// </summary>
    [JsonIgnore]
    public Array<LocalGroup> Local1 => local1 ??= new(LocalGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Local1 for json serialization.
    /// </summary>
    [JsonPropertyName("local1")]
    [Computed]
    public IList<LocalGroup>? Local1_Json
    {
      get => local1;
      set => Local1.Assign(value);
    }

    /// <summary>
    /// A value of Prev.
    /// </summary>
    public CsePerson Prev
    {
      get => prev ??= new();
      set => prev = value;
    }

    /// <summary>
    /// A value of NullDate.
    /// </summary>
    public DateWorkArea NullDate
    {
      get => nullDate ??= new();
      set => nullDate = value;
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
    /// A value of DashboardAuditData.
    /// </summary>
    public DashboardAuditData DashboardAuditData
    {
      get => dashboardAuditData ??= new();
      set => dashboardAuditData = value;
    }

    /// <summary>
    /// A value of TempEndDate.
    /// </summary>
    public DateWorkArea TempEndDate
    {
      get => tempEndDate ??= new();
      set => tempEndDate = value;
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
    /// A value of PreviousYear.
    /// </summary>
    public DashboardStagingPriority12 PreviousYear
    {
      get => previousYear ??= new();
      set => previousYear = value;
    }

    /// <summary>
    /// A value of Contractor1.
    /// </summary>
    public CseOrganization Contractor1
    {
      get => contractor1 ??= new();
      set => contractor1 = value;
    }

    /// <summary>
    /// Gets a value of Contractor.
    /// </summary>
    [JsonIgnore]
    public Array<ContractorGroup> Contractor => contractor ??= new(
      ContractorGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Contractor for json serialization.
    /// </summary>
    [JsonPropertyName("contractor")]
    [Computed]
    public IList<ContractorGroup>? Contractor_Json
    {
      get => contractor;
      set => Contractor.Assign(value);
    }

    private CsePerson? restartDenominator;
    private CsePerson? restartNumerator;
    private TextWorkArea? restartSection;
    private DateWorkArea? priorFiscalYrStartDate;
    private DateWorkArea? priorFiscalYrEndDate;
    private Common? ocse157Verifi;
    private Ocse157Data? ocse157Data;
    private Array<ChildInJdsGroup>? childInJds;
    private Common? childCaseCount;
    private DashboardAuditData? initialized;
    private ProgramCheckpointRestart? programCheckpointRestart;
    private DashboardStagingPriority12? statewide;
    private EabFileHandling? eabFileHandling;
    private EabReportSend? eabReportSend;
    private Array<LocalGroup>? local1;
    private CsePerson? prev;
    private DateWorkArea? nullDate;
    private Common? recordsReadSinceCommit;
    private DashboardAuditData? dashboardAuditData;
    private DateWorkArea? tempEndDate;
    private DashboardStagingPriority12? temp;
    private Common? common;
    private DashboardStagingPriority12? prevRank;
    private DashboardStagingPriority12? previousYear;
    private CseOrganization? contractor1;
    private Array<ContractorGroup>? contractor;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of PreviousYear.
    /// </summary>
    public DashboardStagingPriority12 PreviousYear
    {
      get => previousYear ??= new();
      set => previousYear = value;
    }

    /// <summary>
    /// A value of Ocse157Data.
    /// </summary>
    public Ocse157Data Ocse157Data
    {
      get => ocse157Data ??= new();
      set => ocse157Data = value;
    }

    /// <summary>
    /// A value of Ocse157Verification.
    /// </summary>
    public Ocse157Verification Ocse157Verification
    {
      get => ocse157Verification ??= new();
      set => ocse157Verification = value;
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
    /// A value of DashboardStagingPriority12.
    /// </summary>
    public DashboardStagingPriority12 DashboardStagingPriority12
    {
      get => dashboardStagingPriority12 ??= new();
      set => dashboardStagingPriority12 = value;
    }

    /// <summary>
    /// A value of CsePerson.
    /// </summary>
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
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
    /// A value of CaseRole.
    /// </summary>
    public CaseRole CaseRole
    {
      get => caseRole ??= new();
      set => caseRole = value;
    }

    /// <summary>
    /// A value of CaseAssignment.
    /// </summary>
    public CaseAssignment CaseAssignment
    {
      get => caseAssignment ??= new();
      set => caseAssignment = value;
    }

    /// <summary>
    /// A value of Next.
    /// </summary>
    public GoodCause Next
    {
      get => next ??= new();
      set => next = value;
    }

    /// <summary>
    /// A value of Ar.
    /// </summary>
    public CaseRole Ar
    {
      get => ar ??= new();
      set => ar = value;
    }

    /// <summary>
    /// A value of GoodCause.
    /// </summary>
    public GoodCause GoodCause
    {
      get => goodCause ??= new();
      set => goodCause = value;
    }

    private DashboardStagingPriority12? previousYear;
    private Ocse157Data? ocse157Data;
    private Ocse157Verification? ocse157Verification;
    private CseOrganization? cseOrganization;
    private DashboardStagingPriority12? dashboardStagingPriority12;
    private CsePerson? csePerson;
    private Case1? case1;
    private CaseRole? caseRole;
    private CaseAssignment? caseAssignment;
    private GoodCause? next;
    private CaseRole? ar;
    private GoodCause? goodCause;
  }
#endregion
}
