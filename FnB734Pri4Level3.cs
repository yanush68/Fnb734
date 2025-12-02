// Program: FN_B734_PRI_4_LEVEL_3, ID: 945237095, model: 746.
// Short name: SWE03727
using System;
using System.Collections.Generic;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B734_PRI_4_LEVEL_3.
/// </summary>
[Serializable]
[Program("SWE03727")]
public partial class FnB734Pri4Level3: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B734_PRI_4_LEVEL_3 program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB734Pri4Level3(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB734Pri4Level3.
  /// </summary>
  public FnB734Pri4Level3(IContext context, Import import, Export export):
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
    // 02/03/14  GVandy	CQ42584		Correct Divide by Zero error.
    // ---------------------------------------------------------------------------------------------------
    // -------------------------------------------------------------------------------------
    // Priority 4: Tier 3 - Case Payment and Paternity Status
    // -------------------------------------------------------------------------------------
    // Tier 3.1- Paying Cases (Current Child Support Owed)
    // Paying Cases are defined as a case where at least 75% of the combined 
    // current child
    // support due for the last 3 months has been collected (distributed) as 
    // current child
    // support.
    // $ Amount of Current Child Support Due
    // 	1) Debt details- accruing CS Obligation type- due date in report month.
    // 	     a) Due date within report month and the due date >= to earliest CSE
    // open date
    // 	     b) Skip debts that are due before earliest case role date
    // 	2) Count debt amounts only for primary obligation in primary/secondary 
    // situation.
    // 	3) For joint/several situations, divide the obligation equally among 
    // obligors.
    // 	4) Look for original debt detail amount.
    // 	5) Ignore adjustments done to those debt details.
    // 	6) For each case reported in Tier 2.1, find all find all AP/CH 
    // combinations that
    // 	   overlapped during the report time frame (three months).
    // 	7) Find any qualifying debts that were due on a date (due date) when the
    // AP/CH
    // 	   were active together on the case.  All qualifying debts found will be
    // attributed
    // 	   to the case.  This is the total dollar amount of current child 
    // support due for
    // 	   the case for the three month report period.
    // Current Child Support Collected
    // 	1) Collections created (distributed) that applied to current CS support 
    // due in
    // 	   report period.
    // 	2) In primary/secondary, credit only the primary.
    // 	3) For joint/several identify which AP made the payment.
    // 	4) Bypass FcrtRec and FDIR (REIP) cash receipt types.  Bypass adjusted 
    // collections
    // 	   where collection adjusted in report period.
    // 	5) Include CSENet incoming Interstate collection types.
    // 	6) Count for persons with both active and inactive case roles.
    // 	7) Credit case(s) where the AP/CH were active together on the debt 
    // detail due date for
    // 	   the debt where the collection applied.
    // Tier 3.2- Non Paying Cases (Current Child Support Owed)
    // Non Paying Cases are defined as cases where less than 75% of the combined
    // current
    // child has been collected (distributed) in at least two out of the last 
    // three months.
    // This is a count of all cases reported in Tier 2.1 minus all cases 
    // reported in Tier
    // 3.1.
    // 	1) Count all cases reported in Tier 2.1
    // 	2) Subtract all cases reported in Tier 3.1
    // Tier 3.3-  Paternity Cases (No Obligation)
    // This is a count of all cases from Tier 2.3 where at least one active 
    // child (on
    // refresh date) does not have paternity established.  These cases are a 
    // subset of Tier
    // 2.3.  NOTE: Not all cases with at least one active child without 
    // paternity
    // established will be counted on this line.  Example: 2 kids on case.  One 
    // kid has
    // paternity established and current support owed.  The second kid does not 
    // have
    // paternity established.  The case will not count here as the case was not 
    // counted in
    // Tier 2.3.
    // 	1) Of cases reported in Tier 2.3, count those where:
    // 	2) Paternity Established (CPAT) = N for at least one active child on the
    // case
    // 	   (at report month end).
    // Tier 3.4- Non Paternity Cases (No Obligation)
    // This is a count of all cases from Tier 2.4 where all active children have
    // paternity
    // established.
    // 	1) Count all cases reported in Tier 2.3
    // 	2) Subtract cases reported in Tier 3.3
    // -------------------------------------------------------------------------------------
    MoveProgramCheckpointRestart(import.ProgramCheckpointRestart,
      local.ProgramCheckpointRestart);

    // ------------------------------------------------------------------------------
    // -- Determine if we're restarting and set appropriate restart information.
    // ------------------------------------------------------------------------------
    if (AsChar(import.ProgramCheckpointRestart.RestartInd) == 'Y' && Equal
      (import.ProgramCheckpointRestart.RestartInfo, 81, 8, "4-03    "))
    {
      // -- Checkpoint Info
      // Positions   Value
      // ---------   
      // ------------------------------------
      //  001-080    General Checkpoint Info for PRAD
      //  081-088    Dashboard Priority
      //  089-089    Blank
      //  090-099    AP Person Number
      //  100-100    Blank
      //  101-101    Restart section ("A" for Current Support Due, "B" for 
      // Current Support Collected)
      local.RestartAp.Number =
        Substring(import.ProgramCheckpointRestart.RestartInfo, 90, 10);
      local.RestartSection.Text1 =
        Substring(import.ProgramCheckpointRestart.RestartInfo, 101, 1);

      switch(AsChar(local.RestartSection.Text1))
      {
        case 'A':
          local.ProcessCsDue.Flag = "Y";
          local.ProcessCsCollected.Flag = "Y";

          break;
        case 'B':
          local.ProcessCsDue.Flag = "N";
          local.ProcessCsCollected.Flag = "Y";

          break;
        case 'Z':
          local.ProcessCsDue.Flag = "N";
          local.ProcessCsCollected.Flag = "N";

          break;
        default:
          local.ProcessCsDue.Flag = "Y";
          local.ProcessCsCollected.Flag = "Y";

          break;
      }
    }
    else
    {
      local.RestartAp.Number = "";
      local.RestartSection.Text1 = "";
      local.ProcessCsDue.Flag = "Y";
      local.ProcessCsCollected.Flag = "Y";
    }

    if (ReadObligationType())
    {
      MoveObligationType(entities.Cs, local.Cs);
    }
    else
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error reading CS obligation type in fn_b734_pri_4_level_3.";
      UseCabErrorReport();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // ------------------------------------------------------------------------------
    // -- Tier 3.1 - Paying Cases (Current Child Support Owed)
    // -- Tier 3.2 - Non Paying Cases (Current Child Support Owed)
    // ------------------------------------------------------------------------------
    // -- Setup a local report period start view to be import report period 
    // start - 2 months.
    local.ReportStartDate.Date = AddMonths(import.ReportStartDate.Date, -2);

    if (AsChar(local.ProcessCsDue.Flag) == 'Y')
    {
      local.PrevAp.Number = "";

      // -- Read each current child support debt due during the reporting 
      // period.
      foreach(var _ in ReadDebtDebtDetailCsePersonCsePersonObligation())
      {
        if (Month(entities.DebtDetail.CreatedTmst) > Month
          (entities.DebtDetail.DueDt) && Year
          (entities.DebtDetail.CreatedTmst) == Year
          (entities.DebtDetail.DueDt) || Year
          (entities.DebtDetail.CreatedTmst) > Year(entities.DebtDetail.DueDt))
        {
          // -- The created date of the debt must be prior to the end of the 
          // month in which the
          //    debt was due.  Otherwise, there was no opportunity for a payment
          // to apply as
          //    current support against the debt.
          continue;
        }

        if (!Equal(entities.ApCsePerson.Number, local.PrevAp.Number))
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
            //  090-099    AP Person Number
            //  100-100    Blank
            //  101-101    Restart section    ("A" for Current Support Due,
            // 				"B" for Current Support Collected
            // 				"Z" for all other sections)
            local.ProgramCheckpointRestart.RestartInd = "Y";
            local.ProgramCheckpointRestart.RestartInfo =
              Substring(import.ProgramCheckpointRestart.RestartInfo, 250, 1, 80) +
              "4-03    " + " " + String
              (local.PrevAp.Number, CsePerson.Number_MaxLength) + " A";
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

        local.PrevAp.Number = entities.ApCsePerson.Number;
        ++local.RecordsReadSinceCommit.Count;

        if (AsChar(entities.Obligation.PrimarySecondaryCode) == 'J')
        {
          // -- For Joint and Several debts divide the debt amount between the 
          // two obligors.
          local.Debt.Amount = entities.Debt.Amount / 2;
        }
        else
        {
          local.Debt.Amount = entities.Debt.Amount;
        }

        // -- Find case(s) to which the current child support should be 
        // attributed.
        foreach(var _1 in ReadCase())
        {
          // -- Increment the current support amount due for this case.
          if (ReadDashboardStagingPriority1())
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
                "Error updating Dashboard_Staging_Priority_4 in FN_B734_Pri_4_Level_3.";
              UseCabErrorReport();
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              return;
            }
          }
          else
          {
            // -- Continue. (The case number may not have been included as an 
            // open case)
          }
        }
      }
    }

    if (AsChar(local.ProcessCsCollected.Flag) == 'Y')
    {
      if (AsChar(local.RestartSection.Text1) != 'B')
      {
        local.RestartAp.Number = "";
      }

      local.PrevAp.Number = "";

      // -- Read each collection applied as current child support for debts due 
      // within the 3 month report period.
      foreach(var _ in ReadCollectionCsePersonDebtDetailCsePerson())
      {
        if (!Equal(entities.ApCsePerson.Number, local.PrevAp.Number))
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
            //  090-099    AP Person Number
            //  100-100    Blank
            //  101-101    Restart section ("A" for Current Support Due, "B" for
            // Current Support Collected)
            local.ProgramCheckpointRestart.RestartInd = "Y";
            local.ProgramCheckpointRestart.RestartInfo =
              Substring(import.ProgramCheckpointRestart.RestartInfo, 250, 1, 80) +
              "4-03    " + " " + String
              (local.PrevAp.Number, CsePerson.Number_MaxLength) + " B";
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

        local.PrevAp.Number = entities.ApCsePerson.Number;
        ++local.RecordsReadSinceCommit.Count;

        // -- Find case(s) to which the current child support should be 
        // attributed.
        foreach(var _1 in ReadCase())
        {
          // -- Increment the current support amount due for this case.
          if (ReadDashboardStagingPriority1())
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
                "Error updating Dashboard_Staging_Priority_4 in FN_B734_Pri_4_Level_3.";
              UseCabErrorReport();
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              return;
            }
          }
          else
          {
            // -- Continue. (The case number may not have been included as an 
            // open case)
          }
        }
      }
    }

    // -- Find all cases with current_cs_ind = Y.  Then determine if 75% of 
    // current support
    //    was paid during the last 3 months.
    foreach(var _ in ReadDashboardStagingPriority2())
    {
      ++local.RecordsReadSinceCommit.Count;

      // 02/03/14 GVandy  CQ42584  Correct Divide by Zero error.
      if ((entities.DashboardStagingPriority4.CsDueAmt ?? 0M) == 0)
      {
        local.DashboardStagingPriority4.PayingCaseInd = "N";
      }
      else if ((entities.DashboardStagingPriority4.CsCollectedAmt ?? 0M) / (
        entities.DashboardStagingPriority4.CsDueAmt ?? 0M) >= 0.75M)
      {
        local.DashboardStagingPriority4.PayingCaseInd = "Y";
      }
      else
      {
        local.DashboardStagingPriority4.PayingCaseInd = "N";
      }

      try
      {
        UpdateDashboardStagingPriority3();
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
          "Error updating Dashboard_Staging_Priority_4 in FN_B734_Pri_4_Level_3.";
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
        //  090-099    AP Person Number
        //  100-100    Blank
        //  101-101    Restart section    ("A" for Current Support Due,
        // 				"B" for Current Support Collected
        // 				"Z" for all other sections)
        local.ProgramCheckpointRestart.RestartInd = "Y";

        // -- The AP Person Number is deliberately set to "9999999999".
        local.ProgramCheckpointRestart.RestartInfo =
          Substring(import.ProgramCheckpointRestart.RestartInfo, 250, 1, 80) + "4-03    " +
          " " + "9999999999 Z";
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
    // -- Tier 3.3 - Paternity Cases (No Obligation)
    // -- Tier 3.4 - Non Paternity Cases (No Obligation)
    // ------------------------------------------------------------------------------
    // -- Find all cases with current_cs_ind = N and other_obg_ind = N.  Then 
    // determine if
    //    paternity is established for all children on the case.
    foreach(var _ in ReadDashboardStagingPriority3())
    {
      ++local.RecordsReadSinceCommit.Count;

      // -- Determine if paternity is an issue for this case.
      if (ReadCsePerson())
      {
        local.DashboardStagingPriority4.PaternityEstInd = "N";
      }
      else
      {
        local.DashboardStagingPriority4.PaternityEstInd = "Y";
      }

      try
      {
        UpdateDashboardStagingPriority5();
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
          "Error updating Dashboard_Staging_Priority_4 in FN_B734_Pri_4_Level_3.";
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
        //  090-099    AP Person Number
        //  100-100    Blank
        //  101-101    Restart section    ("A" for Current Support Due,
        // 				"B" for Current Support Collected
        // 				"Z" for all other sections)
        local.ProgramCheckpointRestart.RestartInd = "Y";

        // -- The AP Person Number is deliberately set to "9999999999".
        local.ProgramCheckpointRestart.RestartInfo =
          Substring(import.ProgramCheckpointRestart.RestartInfo, 250, 1, 80) + "4-03    " +
          " " + "9999999999 Z";
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
      Substring(import.ProgramCheckpointRestart.RestartInfo, 250, 1, 80) + "4-04    ";
    UseUpdateCheckpointRstAndCommit();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "Error taking checkpoint.";
      UseCabErrorReport();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }
  }

  private static void MoveObligationType(ObligationType source,
    ObligationType target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
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

  private IEnumerable<bool> ReadCase()
  {
    return ReadEach("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "cspNumber1", entities.ApCsePerson.Number);
        db.SetNullableDate(command, "startDate", entities.DebtDetail.DueDt);
        db.SetString(command, "cspNumber2", entities.ChCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.NoJurisdictionCd = db.GetNullableString(reader, 1);
        entities.Case1.Populated = true;

        return true;
      },
      () =>
      {
        entities.Case1.Populated = false;
      });
  }

  private IEnumerable<bool> ReadCollectionCsePersonDebtDetailCsePerson()
  {
    return ReadEachInSeparateTransaction(
      "ReadCollectionCsePersonDebtDetailCsePerson",
      (db, command) =>
      {
        db.SetDate(command, "date1", local.ReportStartDate.Date);
        db.SetDate(command, "date2", import.ReportEndDate.Date);
        db.SetDateTime(command, "createdTmst", import.ReportEndDate.Timestamp);
        db.SetInt32(command, "otyType", local.Cs.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", local.RestartAp.Number);
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.AppliedToCode = db.GetString(reader, 1);
        entities.Collection.CollectionDt = db.GetDate(reader, 2);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 3);
        entities.Collection.ConcurrentInd = db.GetString(reader, 4);
        entities.Collection.CrtType = db.GetInt32(reader, 5);
        entities.Collection.CstId = db.GetInt32(reader, 6);
        entities.Collection.CrvId = db.GetInt32(reader, 7);
        entities.Collection.CrdId = db.GetInt32(reader, 8);
        entities.Collection.ObgId = db.GetInt32(reader, 9);
        entities.Collection.CspNumber = db.GetString(reader, 10);
        entities.ApCsePerson.Number = db.GetString(reader, 10);
        entities.DebtDetail.CspNumber = db.GetString(reader, 10);
        entities.Collection.CpaType = db.GetString(reader, 11);
        entities.Collection.OtrId = db.GetInt32(reader, 12);
        entities.Collection.OtrType = db.GetString(reader, 13);
        entities.Collection.OtyId = db.GetInt32(reader, 14);
        entities.Collection.CollectionAdjustmentDt = db.GetDate(reader, 15);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 16);
        entities.Collection.Amount = db.GetDecimal(reader, 17);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 18);
        entities.Collection.CourtOrderAppliedTo =
          db.GetNullableString(reader, 19);
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 20);
        entities.DebtDetail.CpaType = db.GetString(reader, 21);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 22);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 23);
        entities.DebtDetail.OtrType = db.GetString(reader, 24);
        entities.DebtDetail.DueDt = db.GetDate(reader, 25);
        entities.DebtDetail.CreatedTmst = db.GetDateTime(reader, 26);
        entities.ChCsePerson.Number = db.GetString(reader, 27);
        entities.ChCsePerson.Type1 = db.GetString(reader, 28);

        if (AsChar(entities.ChCsePerson.Type1) == 'C')
        {
          entities.ChCsePerson.PaternityEstablishedIndicator =
            db.GetNullableString(reader, 29);
          entities.ChCsePerson.DatePaternEstab = db.GetDate(reader, 30);
        }
        else
        {
          entities.ChCsePerson.PaternityEstablishedIndicator = "";
          entities.ChCsePerson.DatePaternEstab = null;
        }

        entities.Collection.Populated = true;
        entities.ApCsePerson.Populated = true;
        entities.DebtDetail.Populated = true;
        entities.ChCsePerson.Populated = true;
        CheckValid<Collection>("AppliedToCode",
          entities.Collection.AppliedToCode);
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("ConcurrentInd",
          entities.Collection.ConcurrentInd);
        CheckValid<Collection>("ProgramAppliedTo",
          entities.Collection.ProgramAppliedTo);
        CheckValid<CsePerson>("Type1", entities.ChCsePerson.Type1);

        return true;
      },
      () =>
      {
        entities.Collection.Populated = false;
        entities.ChCsePerson.Populated = false;
        entities.ApCsePerson.Populated = false;
        entities.DebtDetail.Populated = false;
      });
  }

  private bool ReadCsePerson()
  {
    entities.ChCsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetDate(command, "datePaternEstab", import.ReportEndDate.Date);
        db.SetString(
          command, "casNumber", entities.DashboardStagingPriority4.CaseNumber);
      },
      (db, reader) =>
      {
        entities.ChCsePerson.Number = db.GetString(reader, 0);
        entities.ChCsePerson.Type1 = db.GetString(reader, 1);

        if (AsChar(entities.ChCsePerson.Type1) == 'C')
        {
          entities.ChCsePerson.PaternityEstablishedIndicator =
            db.GetNullableString(reader, 2);
          entities.ChCsePerson.DatePaternEstab = db.GetDate(reader, 3);
        }
        else
        {
          entities.ChCsePerson.PaternityEstablishedIndicator = "";
          entities.ChCsePerson.DatePaternEstab = null;
        }

        entities.ChCsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.ChCsePerson.Type1);
      });
  }

  private bool ReadDashboardStagingPriority1()
  {
    entities.DashboardStagingPriority4.Populated = false;

    return Read("ReadDashboardStagingPriority1",
      (db, command) =>
      {
        db.SetInt32(command, "runNumber", import.DashboardAuditData.RunNumber);
        db.SetInt32(
          command, "reportMonth", import.DashboardAuditData.ReportMonth);
        db.SetString(command, "caseNumber", entities.Case1.Number);
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
      });
  }

  private IEnumerable<bool> ReadDashboardStagingPriority2()
  {
    return ReadEachInSeparateTransaction("ReadDashboardStagingPriority2",
      (db, command) =>
      {
        db.SetInt32(command, "runNumber", import.DashboardAuditData.RunNumber);
        db.SetInt32(
          command, "reportMonth", import.DashboardAuditData.ReportMonth);
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

  private IEnumerable<bool> ReadDashboardStagingPriority3()
  {
    return ReadEachInSeparateTransaction("ReadDashboardStagingPriority3",
      (db, command) =>
      {
        db.SetInt32(command, "runNumber", import.DashboardAuditData.RunNumber);
        db.SetInt32(
          command, "reportMonth", import.DashboardAuditData.ReportMonth);
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

  private IEnumerable<bool> ReadDebtDebtDetailCsePersonCsePersonObligation()
  {
    return ReadEachInSeparateTransaction(
      "ReadDebtDebtDetailCsePersonCsePersonObligation",
      (db, command) =>
      {
        db.SetDate(command, "date1", local.ReportStartDate.Date);
        db.SetDate(command, "date2", import.ReportEndDate.Date);
        db.SetDateTime(command, "createdTmst", import.ReportEndDate.Timestamp);
        db.SetInt32(
          command, "dtyGeneratedId", local.Cs.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", local.RestartAp.Number);
      },
      (db, reader) =>
      {
        entities.Debt.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Debt.CspNumber = db.GetString(reader, 1);
        entities.DebtDetail.CspNumber = db.GetString(reader, 1);
        entities.ApCsePerson.Number = db.GetString(reader, 1);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.Debt.CpaType = db.GetString(reader, 2);
        entities.DebtDetail.CpaType = db.GetString(reader, 2);
        entities.Obligation.CpaType = db.GetString(reader, 2);
        entities.Debt.SystemGeneratedIdentifier = db.GetInt32(reader, 3);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.Debt.Type1 = db.GetString(reader, 4);
        entities.DebtDetail.OtrType = db.GetString(reader, 4);
        entities.Debt.Amount = db.GetDecimal(reader, 5);
        entities.Debt.CreatedTmst = db.GetDateTime(reader, 6);
        entities.Debt.OtyType = db.GetInt32(reader, 9);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 9);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 9);
        entities.DebtDetail.DueDt = db.GetDate(reader, 10);
        entities.DebtDetail.CreatedTmst = db.GetDateTime(reader, 11);
        entities.ChCsePerson.Number = db.GetString(reader, 12);
        entities.ChCsePerson.Type1 = db.GetString(reader, 13);
        entities.Obligation.PrimarySecondaryCode =
          db.GetNullableString(reader, 16);

        if (Equal(entities.Debt.Type1, "DE"))
        {
          entities.Debt.CspSupNumber = db.GetNullableString(reader, 7);
          entities.Debt.CpaSupType = db.GetNullableString(reader, 8);
        }
        else
        {
          entities.Debt.CspSupNumber = null;
          entities.Debt.CpaSupType = null;
        }

        if (AsChar(entities.ChCsePerson.Type1) == 'C')
        {
          entities.ChCsePerson.PaternityEstablishedIndicator =
            db.GetNullableString(reader, 14);
          entities.ChCsePerson.DatePaternEstab = db.GetDate(reader, 15);
        }
        else
        {
          entities.ChCsePerson.PaternityEstablishedIndicator = "";
          entities.ChCsePerson.DatePaternEstab = null;
        }

        entities.Debt.Populated = true;
        entities.DebtDetail.Populated = true;
        entities.ApCsePerson.Populated = true;
        entities.ChCsePerson.Populated = true;
        entities.Obligation.Populated = true;
        CheckValid<ObligationTransaction>("Type1", entities.Debt.Type1);
        CheckValid<CsePerson>("Type1", entities.ChCsePerson.Type1);
        CheckValid<Obligation>("PrimarySecondaryCode",
          entities.Obligation.PrimarySecondaryCode);

        return true;
      },
      () =>
      {
        entities.ChCsePerson.Populated = false;
        entities.ApCsePerson.Populated = false;
        entities.DebtDetail.Populated = false;
        entities.Obligation.Populated = false;
        entities.Debt.Populated = false;
      });
  }

  private bool ReadObligationType()
  {
    entities.Cs.Populated = false;

    return Read("ReadObligationType",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDt", import.ProgramProcessingInfo.ProcessDate);
      },
      (db, reader) =>
      {
        entities.Cs.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Cs.Code = db.GetString(reader, 1);
        entities.Cs.EffectiveDt = db.GetDate(reader, 2);
        entities.Cs.DiscontinueDt = db.GetNullableDate(reader, 3);
        entities.Cs.Populated = true;
      });
  }

  private void UpdateDashboardStagingPriority1()
  {
    var csDueAmt =
      (entities.DashboardStagingPriority4.CsDueAmt ?? 0M) + local.Debt.Amount;

    entities.DashboardStagingPriority4.Populated = false;
    Update("UpdateDashboardStagingPriority1",
      (db, command) =>
      {
        db.SetNullableDecimal(command, "csDueAmt", csDueAmt);
        db.SetInt32(
          command, "reportMonth",
          entities.DashboardStagingPriority4.ReportMonth);
        db.SetInt32(
          command, "runNumber", entities.DashboardStagingPriority4.RunNumber);
        db.SetString(
          command, "caseNumber", entities.DashboardStagingPriority4.CaseNumber);
      });

    entities.DashboardStagingPriority4.CsDueAmt = csDueAmt;
    entities.DashboardStagingPriority4.Populated = true;
  }

  private void UpdateDashboardStagingPriority2()
  {
    var csCollectedAmt = (entities.DashboardStagingPriority4.CsCollectedAmt ?? 0M
      ) + entities.Collection.Amount;

    entities.DashboardStagingPriority4.Populated = false;
    Update("UpdateDashboardStagingPriority2",
      (db, command) =>
      {
        db.SetNullableDecimal(command, "csCollectedAmt", csCollectedAmt);
        db.SetInt32(
          command, "reportMonth",
          entities.DashboardStagingPriority4.ReportMonth);
        db.SetInt32(
          command, "runNumber", entities.DashboardStagingPriority4.RunNumber);
        db.SetString(
          command, "caseNumber", entities.DashboardStagingPriority4.CaseNumber);
      });

    entities.DashboardStagingPriority4.CsCollectedAmt = csCollectedAmt;
    entities.DashboardStagingPriority4.Populated = true;
  }

  private void UpdateDashboardStagingPriority3()
  {
    var payingCaseInd = local.DashboardStagingPriority4.PayingCaseInd ?? "";

    entities.DashboardStagingPriority4.Populated = false;
    Update("UpdateDashboardStagingPriority3",
      (db, command) =>
      {
        db.SetNullableString(command, "payingCaseInd", payingCaseInd);
        db.SetInt32(
          command, "reportMonth",
          entities.DashboardStagingPriority4.ReportMonth);
        db.SetInt32(
          command, "runNumber", entities.DashboardStagingPriority4.RunNumber);
        db.SetString(
          command, "caseNumber", entities.DashboardStagingPriority4.CaseNumber);
      });

    entities.DashboardStagingPriority4.PayingCaseInd = payingCaseInd;
    entities.DashboardStagingPriority4.Populated = true;
  }

  private void UpdateDashboardStagingPriority5()
  {
    var paternityEstInd = local.DashboardStagingPriority4.PaternityEstInd ?? "";

    entities.DashboardStagingPriority4.Populated = false;
    Update("UpdateDashboardStagingPriority5",
      (db, command) =>
      {
        db.SetNullableString(command, "paternityEstInd", paternityEstInd);
        db.SetInt32(
          command, "reportMonth",
          entities.DashboardStagingPriority4.ReportMonth);
        db.SetInt32(
          command, "runNumber", entities.DashboardStagingPriority4.RunNumber);
        db.SetString(
          command, "caseNumber", entities.DashboardStagingPriority4.CaseNumber);
      });

    entities.DashboardStagingPriority4.PaternityEstInd = paternityEstInd;
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
    /// A value of Cs.
    /// </summary>
    public ObligationType Cs
    {
      get => cs ??= new();
      set => cs = value;
    }

    /// <summary>
    /// A value of ProcessCsCollected.
    /// </summary>
    public Common ProcessCsCollected
    {
      get => processCsCollected ??= new();
      set => processCsCollected = value;
    }

    /// <summary>
    /// A value of ProcessCsDue.
    /// </summary>
    public Common ProcessCsDue
    {
      get => processCsDue ??= new();
      set => processCsDue = value;
    }

    /// <summary>
    /// A value of RestartSection.
    /// </summary>
    public WorkArea RestartSection
    {
      get => restartSection ??= new();
      set => restartSection = value;
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
    /// A value of Debt.
    /// </summary>
    public ObligationTransaction Debt
    {
      get => debt ??= new();
      set => debt = value;
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
    /// A value of CaseUnderOrder.
    /// </summary>
    public Common CaseUnderOrder
    {
      get => caseUnderOrder ??= new();
      set => caseUnderOrder = value;
    }

    /// <summary>
    /// A value of RestartAp.
    /// </summary>
    public CsePerson RestartAp
    {
      get => restartAp ??= new();
      set => restartAp = value;
    }

    /// <summary>
    /// A value of PrevAp.
    /// </summary>
    public CsePerson PrevAp
    {
      get => prevAp ??= new();
      set => prevAp = value;
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

    /// <summary>
    /// A value of TbdLocalReportEndDate.
    /// </summary>
    public DateWorkArea TbdLocalReportEndDate
    {
      get => tbdLocalReportEndDate ??= new();
      set => tbdLocalReportEndDate = value;
    }

    private ObligationType? cs;
    private Common? processCsCollected;
    private Common? processCsDue;
    private WorkArea? restartSection;
    private DateWorkArea? reportStartDate;
    private ObligationTransaction? debt;
    private DashboardStagingPriority4? dashboardStagingPriority4;
    private Case1? case1;
    private Common? caseUnderOrder;
    private CsePerson? restartAp;
    private CsePerson? prevAp;
    private ProgramCheckpointRestart? programCheckpointRestart;
    private Common? recordsReadSinceCommit;
    private EabFileHandling? eabFileHandling;
    private EabReportSend? eabReportSend;
    private DateWorkArea? tbdLocalReportEndDate;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ObligationType.
    /// </summary>
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
    }

    /// <summary>
    /// A value of Collection.
    /// </summary>
    public Collection Collection
    {
      get => collection ??= new();
      set => collection = value;
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
    /// A value of Supported.
    /// </summary>
    public CsePersonAccount Supported
    {
      get => supported ??= new();
      set => supported = value;
    }

    /// <summary>
    /// A value of Obligor.
    /// </summary>
    public CsePersonAccount Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
    }

    /// <summary>
    /// A value of DebtDetail.
    /// </summary>
    public DebtDetail DebtDetail
    {
      get => debtDetail ??= new();
      set => debtDetail = value;
    }

    /// <summary>
    /// A value of Cs.
    /// </summary>
    public ObligationType Cs
    {
      get => cs ??= new();
      set => cs = value;
    }

    /// <summary>
    /// A value of Obligation.
    /// </summary>
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

    /// <summary>
    /// A value of Debt.
    /// </summary>
    public ObligationTransaction Debt
    {
      get => debt ??= new();
      set => debt = value;
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
    /// A value of CashReceiptDetail.
    /// </summary>
    public CashReceiptDetail CashReceiptDetail
    {
      get => cashReceiptDetail ??= new();
      set => cashReceiptDetail = value;
    }

    /// <summary>
    /// A value of CashReceipt.
    /// </summary>
    public CashReceipt CashReceipt
    {
      get => cashReceipt ??= new();
      set => cashReceipt = value;
    }

    /// <summary>
    /// A value of CashReceiptType.
    /// </summary>
    public CashReceiptType CashReceiptType
    {
      get => cashReceiptType ??= new();
      set => cashReceiptType = value;
    }

    private ObligationType? obligationType;
    private Collection? collection;
    private CaseRole? chCaseRole;
    private CaseRole? apCaseRole;
    private CsePerson? chCsePerson;
    private CsePerson? apCsePerson;
    private CsePersonAccount? supported;
    private CsePersonAccount? obligor;
    private DebtDetail? debtDetail;
    private ObligationType? cs;
    private Obligation? obligation;
    private ObligationTransaction? debt;
    private DashboardStagingPriority4? dashboardStagingPriority4;
    private Case1? case1;
    private CashReceiptDetail? cashReceiptDetail;
    private CashReceipt? cashReceipt;
    private CashReceiptType? cashReceiptType;
  }
#endregion
}
