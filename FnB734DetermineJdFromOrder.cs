// Program: FN_B734_DETERMINE_JD_FROM_ORDER, ID: 945119058, model: 746.
// Short name: SWE03085
using System;
using System.Collections.Generic;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B734_DETERMINE_JD_FROM_ORDER.
/// </summary>
[Serializable]
[Program("SWE03085")]
public partial class FnB734DetermineJdFromOrder: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B734_DETERMINE_JD_FROM_ORDER program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB734DetermineJdFromOrder(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB734DetermineJdFromOrder.
  /// </summary>
  public FnB734DetermineJdFromOrder(IContext context, Import import,
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
    // ---------------------------------------------------------------------------------------------------
    //                                     
    // C H A N G E    L O G
    // ---------------------------------------------------------------------------------------------------
    // Date      Developer     Request #	Description
    // --------  ----------    ----------	
    // -----------------------------------------------------------
    // 02/20/13  GVandy	CQ36547		Initial Development.  Priority 1-1, 1-3, and 1-
    // 4.
    // 			Segment A	
    // 04/18/13  GVandy	CQ36547		Add support for determining order directly from
    // 					legal action.
    // 06/07/13  GVandy			Add support for finding judicial district from AP/
    // Supported
    // 					on the Debt or Collection rather than by tribunal.  This
    // 					was added to support priority 2-13.  Added a new import
    // 					flag to support this option.
    // ---------------------------------------------------------------------------------------------------
    // -------------------------------------------------------------------------------------
    // Find Judicial District by County of Order.
    // -------------------------------------------------------------------------------------
    // 	1) If collections are on a KS order, credit the county of order.
    // 	2) If collections are on a non-KS order, look for AP/Supported Person
    // 	   combination on debt detail where money applied to credit case.
    // 	3) Find the first applicable case that was open at some point during the
    // 	   report period.  If no case is found that was open during the report
    // 	   period, the collection will error out.
    // 	4) Look for AP/ Supported Person active on a case on the debt detail due
    // 	   date.  If none found, look for earliest instance of AP/ Supported 
    // Person
    // 	   active on the case following debt detail due date (only necessary if
    // 	   collection on non-KS order).   (For Future, Gift and Voluntary 
    // payments,
    // 	   use collection created time stamp instead of debt detail due date.)
    // 		a) If AP/ Supported Person combination is active on two or more cases
    // 		   on the debt detail due date, credit the case where the combination
    // 		   became active most recently.
    // 		b) If AP/ Supported Person combination is not active on the debt due
    // 		   date or any time afterwards, look for case with most recent
    // 		   instance of AP/ Supported Person combination being active prior to
    // 		   the debt detail due date.
    // -------------------------------------------------------------------------------------
    if (import.PersistentCollection.Populated)
    {
      if (!ReadObligationDebt())
      {
        // --  Write to error report...
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "FN_B734_Determine_JD_From_Order.  Error finding obligation.";
        local.EabReportSend.RptDetail =
          TrimEnd(local.EabReportSend.RptDetail) + "  Collection Sys Gen ID " +
          NumberToString
          (import.PersistentCollection.SystemGeneratedIdentifier, 7, 9);
        UseCabErrorReport();

        return;
      }
    }
    else if (import.PersistentDebt.Populated)
    {
      if (!ReadObligation())
      {
        // --  Write to error report...
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "FN_B734_Determine_JD_From_Order.  Error finding obligation.";
        local.EabReportSend.RptDetail =
          TrimEnd(local.EabReportSend.RptDetail) + "  Debt Sys Gen ID " + NumberToString
          (import.PersistentDebt.SystemGeneratedIdentifier, 7, 9);
        UseCabErrorReport();

        return;
      }
    }
    else if (import.PersistentLegalAction.Populated)
    {
      if (!ReadTribunal())
      {
        // --  Write to error report...
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "FN_B734_Determine_JD_From_Order.  Error finding Tribunal for Legal Action.";
        local.EabReportSend.RptDetail =
          TrimEnd(local.EabReportSend.RptDetail) + "  Legal Action ID " + NumberToString
          (import.PersistentLegalAction.Identifier, 7, 9);
        UseCabErrorReport();

        return;
      }

      if (ReadFips())
      {
        if (entities.Fips.State == 20 && !
          IsEmpty(entities.Tribunal.JudicialDistrict))
        {
          // -- Court order is a Kansas Order.  Use Judicial District in which 
          // the Tribunal resides.
          export.DashboardAuditData.JudicialDistrict =
            entities.Tribunal.JudicialDistrict;
        }
      }
      else
      {
        // -- Continue.  Foreign tribunals are not related to FIPS records.
      }

      // -- No further processing when determine JD from legal action.
      return;
    }

    if (AsChar(import.UseApSupportedOnly.Flag) == 'Y')
    {
      // -- Use the AP/Support to find case instead of using the Tribunal 
      // judicial district.  Continue.
    }
    else if (ReadLegalActionTribunalFips())
    {
      if (entities.Fips.Populated)
      {
        if (entities.Fips.State == 20 && !
          IsEmpty(entities.Tribunal.JudicialDistrict))
        {
          // -- Court order is a Kansas Order.  Use Judicial District in which 
          // the Tribunal resides.
          export.DashboardAuditData.JudicialDistrict =
            entities.Tribunal.JudicialDistrict;
          export.DashboardAuditData.StandardNumber =
            entities.LegalAction.StandardNumber;

          return;
        }
      }
      else
      {
        // -- Continue.  Foreign tribunals are not related to FIPS records.
      }
    }
    else
    {
      if (ReadObligationType1())
      {
        if (AsChar(entities.ObligationType.Classification) == 'V')
        {
          // -- Voluntary collections may not be associated to legal actions.
          // -- Determine Judicial District using case.
          goto Test;
        }
      }
      else
      {
        // --  Write to error report...
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "FN_B734_Determine_JD_From_Order.  Error finding obligation type.";

        if (import.PersistentCollection.Populated)
        {
          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + "  Collection Sys Gen ID " +
            NumberToString
            (import.PersistentCollection.SystemGeneratedIdentifier, 7, 9);
        }
        else if (import.PersistentDebt.Populated)
        {
          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + "  Debt Sys Gen ID " + NumberToString
            (import.PersistentDebt.SystemGeneratedIdentifier, 7, 9);
        }

        UseCabErrorReport();

        return;
      }

      // --  Write to error report...
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "FN_B734_Determine_JD_From_Order.  Error finding Legal Action/Tribunal.";

      if (import.PersistentCollection.Populated)
      {
        local.EabReportSend.RptDetail =
          TrimEnd(local.EabReportSend.RptDetail) + "  Collection Sys Gen ID " +
          NumberToString
          (import.PersistentCollection.SystemGeneratedIdentifier, 7, 9);
      }
      else if (import.PersistentDebt.Populated)
      {
        local.EabReportSend.RptDetail =
          TrimEnd(local.EabReportSend.RptDetail) + "  Debt Sys Gen ID " + NumberToString
          (import.PersistentDebt.SystemGeneratedIdentifier, 7, 9);
      }

      UseCabErrorReport();

      return;
    }

Test:

    // -- Determine Judicial District by using Case.
    if (!ReadCsePerson1())
    {
      // --  Write to error report...
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "FN_B734_Determine_JD_From_Order.  Error finding Obligor.";

      if (import.PersistentCollection.Populated)
      {
        local.EabReportSend.RptDetail =
          TrimEnd(local.EabReportSend.RptDetail) + "  Collection Sys Gen ID " +
          NumberToString
          (import.PersistentCollection.SystemGeneratedIdentifier, 7, 9);
      }
      else if (import.PersistentDebt.Populated)
      {
        local.EabReportSend.RptDetail =
          TrimEnd(local.EabReportSend.RptDetail) + "  Debt Sys Gen ID " + NumberToString
          (import.PersistentDebt.SystemGeneratedIdentifier, 7, 9);
      }

      UseCabErrorReport();

      return;
    }

    if (import.PersistentCollection.Populated)
    {
      if (!ReadCsePerson2())
      {
        // --  Write to error report...
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "FN_B734_Determine_JD_From_Order.  Error finding Supported.";
        local.EabReportSend.RptDetail =
          TrimEnd(local.EabReportSend.RptDetail) + "  Collection Sys Gen ID " +
          NumberToString
          (import.PersistentCollection.SystemGeneratedIdentifier, 7, 9);
        UseCabErrorReport();

        return;
      }

      if (!ReadObligationType2())
      {
        // --  Write to error report...
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "FN_B734_Determine_JD_From_Order.  Error finding Obligation Type.";
        local.EabReportSend.RptDetail =
          TrimEnd(local.EabReportSend.RptDetail) + "  Collection Sys Gen ID " +
          NumberToString
          (import.PersistentCollection.SystemGeneratedIdentifier, 7, 9);
        UseCabErrorReport();

        return;
      }

      if (!ReadDebtDetail1())
      {
        // --  Write to error report...
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "FN_B734_Determine_JD_From_Order.  Error finding Debt Detail.";
        local.EabReportSend.RptDetail =
          TrimEnd(local.EabReportSend.RptDetail) + "  Collection Sys Gen ID " +
          NumberToString
          (import.PersistentCollection.SystemGeneratedIdentifier, 7, 9);
        UseCabErrorReport();

        return;
      }

      if (AsChar(import.PersistentCollection.AppliedToCode) == 'G' || AsChar
        (entities.ObligationType.Classification) == 'V' || Lt
        (import.ReportEndDate.Date, entities.DebtDetail.DueDt))
      {
        // -- For Future, Gift, and Voluntary collections, determine the case 
        // using
        // -- the collection created date.
        local.CaseDate.Date = Date(import.PersistentCollection.CreatedTmst);
      }
      else
      {
        // -- For all others, determine case using the debt detail due date.
        local.CaseDate.Date = entities.DebtDetail.DueDt;
      }
    }
    else if (import.PersistentDebt.Populated)
    {
      if (!ReadCsePerson3())
      {
        // --  Write to error report...
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "FN_B734_Determine_JD_From_Order.  Error finding Supported.";
        local.EabReportSend.RptDetail =
          TrimEnd(local.EabReportSend.RptDetail) + "  Debt Sys Gen ID " + NumberToString
          (import.PersistentDebt.SystemGeneratedIdentifier, 7, 9);
        UseCabErrorReport();

        return;
      }

      if (!ReadDebtDetail2())
      {
        // --  Write to error report...
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "FN_B734_Determine_JD_From_Order.  Error finding Debt Detail.";
        local.EabReportSend.RptDetail =
          TrimEnd(local.EabReportSend.RptDetail) + "  Debt Sys Gen ID " + NumberToString
          (import.PersistentDebt.SystemGeneratedIdentifier, 7, 9);
        UseCabErrorReport();

        return;
      }

      // -- Determine case using the debt detail due date.
      local.CaseDate.Date = entities.DebtDetail.DueDt;
    }

    // -- Find a case where the AP/Supported have overlapping roles on the debt 
    // due date
    // -- (or collection create date for Gift, Voluntaries, and Future 
    // collections).
    local.Overlap.Date = new DateTime(1, 1, 1);

    foreach(var _ in ReadCaseCaseRoleCaseRoleCaseAssignment1())
    {
      if (!entities.CaseAssignment.Populated)
      {
        // -- Case was not open during the report period.  Look for another 
        // case.
        continue;
      }

      if (Lt(entities.ApCaseRole.StartDate, entities.SupportedCaseRole.StartDate))
      {
        local.DateWorkArea.Date = entities.SupportedCaseRole.StartDate;
      }
      else
      {
        local.DateWorkArea.Date = entities.ApCaseRole.StartDate;
      }

      if (Lt(local.Overlap.Date, local.DateWorkArea.Date))
      {
        // -- Use case where AP/Supported combo became active most recently.
        local.Overlap.Date = local.DateWorkArea.Date;
        local.Case1.Number = entities.Case1.Number;
      }
    }

    if (IsEmpty(local.Case1.Number))
    {
      // -- Find case where the AP/Supported have the earliest overlapping roles
      // following debt due date.
      local.Overlap.Date = new DateTime(2099, 12, 31);

      foreach(var _ in ReadCaseCaseRoleCaseRoleCaseAssignment2())
      {
        if (!entities.CaseAssignment.Populated)
        {
          // -- Case was not open during the report period.  Look for another 
          // case.
          continue;
        }

        if (Lt(entities.ApCaseRole.StartDate,
          entities.SupportedCaseRole.StartDate))
        {
          local.DateWorkArea.Date = entities.SupportedCaseRole.StartDate;
        }
        else
        {
          local.DateWorkArea.Date = entities.ApCaseRole.StartDate;
        }

        if (Lt(local.DateWorkArea.Date, local.Overlap.Date))
        {
          // -- Use case where AP/Supported combo became active most recently.
          local.Overlap.Date = local.DateWorkArea.Date;
          local.Case1.Number = entities.Case1.Number;
        }
      }
    }

    if (IsEmpty(local.Case1.Number))
    {
      // -- Find case where the AP/Supported have the most recent overlapping 
      // roles prior to the debt due date.
      local.Overlap.Date = new DateTime(1, 1, 1);

      foreach(var _ in ReadCaseCaseRoleCaseRoleCaseAssignment3())
      {
        if (!entities.CaseAssignment.Populated)
        {
          // -- Case was not open during the report period.  Look for another 
          // case.
          continue;
        }

        if (Lt(entities.ApCaseRole.StartDate,
          entities.SupportedCaseRole.StartDate))
        {
          local.DateWorkArea.Date = entities.SupportedCaseRole.StartDate;
        }
        else
        {
          local.DateWorkArea.Date = entities.ApCaseRole.StartDate;
        }

        if (Lt(local.Overlap.Date, local.DateWorkArea.Date))
        {
          // -- Use case where AP/Supported combo became active most recently.
          local.Overlap.Date = local.DateWorkArea.Date;
          local.Case1.Number = entities.Case1.Number;
        }
      }
    }

    if (IsEmpty(local.Case1.Number))
    {
      // -- If no overlapping case roles found on an open case then write to the
      // error report.
      UseCabDate2TextWithHyphens2();
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "FN_B734_DETERMINE_JD_FROM_ORDER - Error finding common active case for AP " +
        String(entities.ApCsePerson.Number, CsePerson.Number_MaxLength) + " Supported " +
        String
        (entities.SupportedCsePerson.Number, CsePerson.Number_MaxLength) + ".  Debt Due Date " +
        String(local.TextWorkArea.Text10, TextWorkArea.Text10_MaxLength);
      UseCabErrorReport();

      return;
    }

    ReadCaseAssignmentOffice();

    if (ReadCseOrganization())
    {
      export.DashboardAuditData.StandardNumber =
        entities.LegalAction.StandardNumber;
      export.DashboardAuditData.JudicialDistrict =
        entities.JudicialDistrict.Code;
      export.DashboardAuditData.Office = entities.Office.SystemGeneratedId;
      export.DashboardAuditData.CaseNumber = local.Case1.Number;
    }
    else
    {
      // --  Write to error report...
      UseCabDate2TextWithHyphens1();
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "FN_B734_DETERMINE_JD_FROM_ORDER - Error finding judicial district for case " +
        String(local.Case1.Number, Case1.Number_MaxLength) + " in office " + NumberToString
        (entities.Office.SystemGeneratedId, 13, 3);
      UseCabErrorReport();
    }
  }

  private void UseCabDate2TextWithHyphens1()
  {
    var useImport = new CabDate2TextWithHyphens.Import();
    var useExport = new CabDate2TextWithHyphens.Export();

    useImport.DateWorkArea.Date = import.ReportEndDate.Date;

    context.Call(CabDate2TextWithHyphens.Execute, useImport, useExport);

    local.TextWorkArea.Text10 = useExport.TextWorkArea.Text10;
  }

  private void UseCabDate2TextWithHyphens2()
  {
    var useImport = new CabDate2TextWithHyphens.Import();
    var useExport = new CabDate2TextWithHyphens.Export();

    useImport.DateWorkArea.Date = local.CaseDate.Date;

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

  private bool ReadCaseAssignmentOffice()
  {
    entities.CaseAssignment.Populated = false;
    entities.Office.Populated = false;

    return Read("ReadCaseAssignmentOffice",
      (db, command) =>
      {
        db.SetString(command, "casNo", local.Case1.Number);
        db.SetNullableDate(
          command, "discontinueDate", import.ReportStartDate.Date);
        db.SetDate(command, "effectiveDate", import.ReportEndDate.Date);
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
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 8);
        entities.Office.CogTypeCode = db.GetNullableString(reader, 9);
        entities.Office.CogCode = db.GetNullableString(reader, 10);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 11);
        entities.CaseAssignment.Populated = true;
        entities.Office.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCaseCaseRoleCaseRoleCaseAssignment1()
  {
    return ReadEach("ReadCaseCaseRoleCaseRoleCaseAssignment1",
      (db, command) =>
      {
        db.SetNullableDate(command, "startDate", local.CaseDate.Date);
        db.SetString(command, "cspNumber1", entities.ApCsePerson.Number);
        db.SetString(command, "cspNumber2", entities.SupportedCsePerson.Number);
        db.SetDate(command, "effectiveDate", import.ReportEndDate.Date);
        db.SetNullableDate(
          command, "discontinueDate", import.ReportStartDate.Date);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.ApCaseRole.CasNumber = db.GetString(reader, 0);
        entities.ApCaseRole.CspNumber = db.GetString(reader, 1);
        entities.ApCaseRole.Type1 = db.GetString(reader, 2);
        entities.ApCaseRole.Identifier = db.GetInt32(reader, 3);
        entities.ApCaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.ApCaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.SupportedCaseRole.CasNumber = db.GetString(reader, 6);
        entities.SupportedCaseRole.CspNumber = db.GetString(reader, 7);
        entities.SupportedCaseRole.Type1 = db.GetString(reader, 8);
        entities.SupportedCaseRole.Identifier = db.GetInt32(reader, 9);
        entities.SupportedCaseRole.StartDate = db.GetNullableDate(reader, 10);
        entities.SupportedCaseRole.EndDate = db.GetNullableDate(reader, 11);
        entities.CaseAssignment.EffectiveDate = db.GetDate(reader, 12);
        entities.CaseAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 13);
        entities.CaseAssignment.CreatedTimestamp = db.GetDateTime(reader, 14);
        entities.CaseAssignment.SpdId = db.GetInt32(reader, 15);
        entities.CaseAssignment.OffId = db.GetInt32(reader, 16);
        entities.CaseAssignment.OspCode = db.GetString(reader, 17);
        entities.CaseAssignment.OspDate = db.GetDate(reader, 18);
        entities.CaseAssignment.CasNo = db.GetString(reader, 19);
        entities.Case1.Populated = true;
        entities.ApCaseRole.Populated = true;
        entities.SupportedCaseRole.Populated = true;
        entities.CaseAssignment.Populated = db.GetNullableInt32(reader, 15) != null
          ;
        CheckValid<CaseRole>("Type1", entities.ApCaseRole.Type1);
        CheckValid<CaseRole>("Type1", entities.SupportedCaseRole.Type1);

        return true;
      },
      () =>
      {
        entities.CaseAssignment.Populated = false;
        entities.Case1.Populated = false;
        entities.ApCaseRole.Populated = false;
        entities.SupportedCaseRole.Populated = false;
      });
  }

  private IEnumerable<bool> ReadCaseCaseRoleCaseRoleCaseAssignment2()
  {
    return ReadEach("ReadCaseCaseRoleCaseRoleCaseAssignment2",
      (db, command) =>
      {
        db.SetNullableDate(command, "endDate", local.CaseDate.Date);
        db.SetString(command, "cspNumber1", entities.ApCsePerson.Number);
        db.SetString(command, "cspNumber2", entities.SupportedCsePerson.Number);
        db.SetDate(command, "effectiveDate", import.ReportEndDate.Date);
        db.SetNullableDate(
          command, "discontinueDate", import.ReportStartDate.Date);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.ApCaseRole.CasNumber = db.GetString(reader, 0);
        entities.ApCaseRole.CspNumber = db.GetString(reader, 1);
        entities.ApCaseRole.Type1 = db.GetString(reader, 2);
        entities.ApCaseRole.Identifier = db.GetInt32(reader, 3);
        entities.ApCaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.ApCaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.SupportedCaseRole.CasNumber = db.GetString(reader, 6);
        entities.SupportedCaseRole.CspNumber = db.GetString(reader, 7);
        entities.SupportedCaseRole.Type1 = db.GetString(reader, 8);
        entities.SupportedCaseRole.Identifier = db.GetInt32(reader, 9);
        entities.SupportedCaseRole.StartDate = db.GetNullableDate(reader, 10);
        entities.SupportedCaseRole.EndDate = db.GetNullableDate(reader, 11);
        entities.CaseAssignment.EffectiveDate = db.GetDate(reader, 12);
        entities.CaseAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 13);
        entities.CaseAssignment.CreatedTimestamp = db.GetDateTime(reader, 14);
        entities.CaseAssignment.SpdId = db.GetInt32(reader, 15);
        entities.CaseAssignment.OffId = db.GetInt32(reader, 16);
        entities.CaseAssignment.OspCode = db.GetString(reader, 17);
        entities.CaseAssignment.OspDate = db.GetDate(reader, 18);
        entities.CaseAssignment.CasNo = db.GetString(reader, 19);
        entities.Case1.Populated = true;
        entities.ApCaseRole.Populated = true;
        entities.SupportedCaseRole.Populated = true;
        entities.CaseAssignment.Populated = db.GetNullableInt32(reader, 15) != null
          ;
        CheckValid<CaseRole>("Type1", entities.ApCaseRole.Type1);
        CheckValid<CaseRole>("Type1", entities.SupportedCaseRole.Type1);

        return true;
      },
      () =>
      {
        entities.CaseAssignment.Populated = false;
        entities.Case1.Populated = false;
        entities.ApCaseRole.Populated = false;
        entities.SupportedCaseRole.Populated = false;
      });
  }

  private IEnumerable<bool> ReadCaseCaseRoleCaseRoleCaseAssignment3()
  {
    return ReadEach("ReadCaseCaseRoleCaseRoleCaseAssignment3",
      (db, command) =>
      {
        db.SetNullableDate(command, "startDate", local.CaseDate.Date);
        db.SetString(command, "cspNumber1", entities.ApCsePerson.Number);
        db.SetString(command, "cspNumber2", entities.SupportedCsePerson.Number);
        db.SetDate(command, "effectiveDate", import.ReportEndDate.Date);
        db.SetNullableDate(
          command, "discontinueDate", import.ReportStartDate.Date);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.ApCaseRole.CasNumber = db.GetString(reader, 0);
        entities.ApCaseRole.CspNumber = db.GetString(reader, 1);
        entities.ApCaseRole.Type1 = db.GetString(reader, 2);
        entities.ApCaseRole.Identifier = db.GetInt32(reader, 3);
        entities.ApCaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.ApCaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.SupportedCaseRole.CasNumber = db.GetString(reader, 6);
        entities.SupportedCaseRole.CspNumber = db.GetString(reader, 7);
        entities.SupportedCaseRole.Type1 = db.GetString(reader, 8);
        entities.SupportedCaseRole.Identifier = db.GetInt32(reader, 9);
        entities.SupportedCaseRole.StartDate = db.GetNullableDate(reader, 10);
        entities.SupportedCaseRole.EndDate = db.GetNullableDate(reader, 11);
        entities.CaseAssignment.EffectiveDate = db.GetDate(reader, 12);
        entities.CaseAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 13);
        entities.CaseAssignment.CreatedTimestamp = db.GetDateTime(reader, 14);
        entities.CaseAssignment.SpdId = db.GetInt32(reader, 15);
        entities.CaseAssignment.OffId = db.GetInt32(reader, 16);
        entities.CaseAssignment.OspCode = db.GetString(reader, 17);
        entities.CaseAssignment.OspDate = db.GetDate(reader, 18);
        entities.CaseAssignment.CasNo = db.GetString(reader, 19);
        entities.Case1.Populated = true;
        entities.ApCaseRole.Populated = true;
        entities.SupportedCaseRole.Populated = true;
        entities.CaseAssignment.Populated = db.GetNullableInt32(reader, 15) != null
          ;
        CheckValid<CaseRole>("Type1", entities.ApCaseRole.Type1);
        CheckValid<CaseRole>("Type1", entities.SupportedCaseRole.Type1);

        return true;
      },
      () =>
      {
        entities.CaseAssignment.Populated = false;
        entities.Case1.Populated = false;
        entities.ApCaseRole.Populated = false;
        entities.SupportedCaseRole.Populated = false;
      });
  }

  private bool ReadCseOrganization()
  {
    System.Diagnostics.Debug.Assert(entities.Office.Populated);
    entities.JudicialDistrict.Populated = false;

    return Read("ReadCseOrganization",
      (db, command) =>
      {
        db.SetString(command, "cogParentType", entities.Office.CogTypeCode);
        db.SetString(command, "cogParentCode", entities.Office.CogCode);
      },
      (db, reader) =>
      {
        entities.JudicialDistrict.Code = db.GetString(reader, 0);
        entities.JudicialDistrict.Type1 = db.GetString(reader, 1);
        entities.JudicialDistrict.Populated = true;
      });
  }

  private bool ReadCsePerson1()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.ApCsePerson.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
      },
      (db, reader) =>
      {
        entities.ApCsePerson.Number = db.GetString(reader, 0);
        entities.ApCsePerson.Populated = true;
      });
  }

  private bool ReadCsePerson2()
  {
    System.Diagnostics.Debug.Assert(entities.Debt.Populated);
    entities.SupportedCsePerson.Populated = false;

    return Read("ReadCsePerson2",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.Debt.CspSupNumber);
        db.SetNullableString(command, "cpaSupType", entities.Debt.CpaSupType);
      },
      (db, reader) =>
      {
        entities.SupportedCsePerson.Number = db.GetString(reader, 0);
        entities.SupportedCsePerson.Populated = true;
      });
  }

  private bool ReadCsePerson3()
  {
    System.Diagnostics.Debug.Assert(import.PersistentDebt.Populated);
    entities.SupportedCsePerson.Populated = false;

    return Read("ReadCsePerson3",
      (db, command) =>
      {
        db.SetString(command, "numb", import.PersistentDebt.CspSupNumber);
        db.SetNullableString(
          command, "cpaSupType", import.PersistentDebt.CpaSupType);
      },
      (db, reader) =>
      {
        entities.SupportedCsePerson.Number = db.GetString(reader, 0);
        entities.SupportedCsePerson.Populated = true;
      });
  }

  private bool ReadDebtDetail1()
  {
    System.Diagnostics.Debug.Assert(entities.Debt.Populated);
    entities.DebtDetail.Populated = false;

    return Read("ReadDebtDetail1",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.Debt.OtyType);
        db.SetInt32(command, "obgGeneratedId", entities.Debt.ObgGeneratedId);
        db.SetString(command, "otrType", entities.Debt.Type1);
        db.SetInt32(
          command, "otrGeneratedId", entities.Debt.SystemGeneratedIdentifier);
        db.SetString(command, "cpaType", entities.Debt.CpaType);
        db.SetString(command, "cspNumber", entities.Debt.CspNumber);
      },
      (db, reader) =>
      {
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.DebtDetail.CspNumber = db.GetString(reader, 1);
        entities.DebtDetail.CpaType = db.GetString(reader, 2);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 4);
        entities.DebtDetail.OtrType = db.GetString(reader, 5);
        entities.DebtDetail.DueDt = db.GetDate(reader, 6);
        entities.DebtDetail.Populated = true;
      });
  }

  private bool ReadDebtDetail2()
  {
    System.Diagnostics.Debug.Assert(import.PersistentDebt.Populated);
    entities.DebtDetail.Populated = false;

    return Read("ReadDebtDetail2",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", import.PersistentDebt.OtyType);
        db.SetInt32(
          command, "obgGeneratedId", import.PersistentDebt.ObgGeneratedId);
        db.SetString(command, "otrType", import.PersistentDebt.Type1);
        db.SetInt32(
          command, "otrGeneratedId",
          import.PersistentDebt.SystemGeneratedIdentifier);
        db.SetString(command, "cpaType", import.PersistentDebt.CpaType);
        db.SetString(command, "cspNumber", import.PersistentDebt.CspNumber);
      },
      (db, reader) =>
      {
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.DebtDetail.CspNumber = db.GetString(reader, 1);
        entities.DebtDetail.CpaType = db.GetString(reader, 2);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 4);
        entities.DebtDetail.OtrType = db.GetString(reader, 5);
        entities.DebtDetail.DueDt = db.GetDate(reader, 6);
        entities.DebtDetail.Populated = true;
      });
  }

  private bool ReadFips()
  {
    System.Diagnostics.Debug.Assert(entities.Tribunal.Populated);
    entities.Fips.Populated = false;

    return Read("ReadFips",
      (db, command) =>
      {
        db.SetInt32(command, "location", entities.Tribunal.FipLocation ?? 0);
        db.SetInt32(command, "county", entities.Tribunal.FipCounty ?? 0);
        db.SetInt32(command, "state", entities.Tribunal.FipState ?? 0);
      },
      (db, reader) =>
      {
        entities.Fips.State = db.GetInt32(reader, 0);
        entities.Fips.County = db.GetInt32(reader, 1);
        entities.Fips.Location = db.GetInt32(reader, 2);
        entities.Fips.Populated = true;
      });
  }

  private bool ReadLegalActionTribunalFips()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.Fips.Populated = false;
    entities.LegalAction.Populated = false;
    entities.Tribunal.Populated = false;

    return Read("ReadLegalActionTribunalFips",
      (db, command) =>
      {
        db.SetInt32(command, "legalActionId", entities.Obligation.LgaId ?? 0);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 1);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 2);
        entities.Tribunal.Identifier = db.GetInt32(reader, 2);
        entities.Tribunal.FipLocation = db.GetNullableInt32(reader, 3);
        entities.Tribunal.JudicialDistrict = db.GetString(reader, 4);
        entities.Tribunal.FipCounty = db.GetNullableInt32(reader, 5);
        entities.Tribunal.FipState = db.GetNullableInt32(reader, 6);
        entities.Fips.State = db.GetInt32(reader, 7);
        entities.Fips.County = db.GetInt32(reader, 8);
        entities.Fips.Location = db.GetInt32(reader, 9);
        entities.LegalAction.Populated = true;
        entities.Tribunal.Populated = true;
        entities.Fips.Populated = db.GetNullableInt32(reader, 7) != null;
      });
  }

  private bool ReadObligation()
  {
    System.Diagnostics.Debug.Assert(import.PersistentDebt.Populated);
    entities.Obligation.Populated = false;

    return Read("ReadObligation",
      (db, command) =>
      {
        db.SetInt32(command, "dtyGeneratedId", import.PersistentDebt.OtyType);
        db.SetInt32(command, "obId", import.PersistentDebt.ObgGeneratedId);
        db.SetString(command, "cspNumber", import.PersistentDebt.CspNumber);
        db.SetString(command, "cpaType", import.PersistentDebt.CpaType);
      },
      (db, reader) =>
      {
        entities.Obligation.CpaType = db.GetString(reader, 0);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.Obligation.LgaId = db.GetNullableInt32(reader, 4);
        entities.Obligation.Populated = true;
      });
  }

  private bool ReadObligationDebt()
  {
    System.Diagnostics.Debug.Assert(import.PersistentCollection.Populated);
    entities.Debt.Populated = false;
    entities.Obligation.Populated = false;

    return Read("ReadObligationDebt",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", import.PersistentCollection.OtyId);
        db.SetString(command, "obTrnTyp", import.PersistentCollection.OtrType);
        db.SetInt32(command, "obTrnId", import.PersistentCollection.OtrId);
        db.SetString(command, "cpaType", import.PersistentCollection.CpaType);
        db.
          SetString(command, "cspNumber", import.PersistentCollection.CspNumber);
        db.
          SetInt32(command, "obgGeneratedId", import.PersistentCollection.ObgId);
      },
      (db, reader) =>
      {
        entities.Obligation.CpaType = db.GetString(reader, 0);
        entities.Debt.CpaType = db.GetString(reader, 0);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.Debt.CspNumber = db.GetString(reader, 1);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Debt.ObgGeneratedId = db.GetInt32(reader, 2);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.Debt.OtyType = db.GetInt32(reader, 3);
        entities.Obligation.LgaId = db.GetNullableInt32(reader, 4);
        entities.Debt.SystemGeneratedIdentifier = db.GetInt32(reader, 5);
        entities.Debt.Type1 = db.GetString(reader, 6);

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

        entities.Obligation.Populated = true;
        entities.Debt.Populated = true;
        CheckValid<ObligationTransaction>("Type1", entities.Debt.Type1);
      });
  }

  private bool ReadObligationType1()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.ObligationType.Populated = false;

    return Read("ReadObligationType1",
      (db, command) =>
      {
        db.SetInt32(command, "debtTypId", entities.Obligation.DtyGeneratedId);
      },
      (db, reader) =>
      {
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ObligationType.Classification = db.GetString(reader, 1);
        entities.ObligationType.Populated = true;
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);
      });
  }

  private bool ReadObligationType2()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.ObligationType.Populated = false;

    return Read("ReadObligationType2",
      (db, command) =>
      {
        db.SetInt32(command, "debtTypId", entities.Obligation.DtyGeneratedId);
      },
      (db, reader) =>
      {
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ObligationType.Classification = db.GetString(reader, 1);
        entities.ObligationType.Populated = true;
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);
      });
  }

  private bool ReadTribunal()
  {
    System.Diagnostics.Debug.Assert(import.PersistentLegalAction.Populated);
    entities.Tribunal.Populated = false;

    return Read("ReadTribunal",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier", import.PersistentLegalAction.TrbId ?? 0);
      },
      (db, reader) =>
      {
        entities.Tribunal.FipLocation = db.GetNullableInt32(reader, 0);
        entities.Tribunal.JudicialDistrict = db.GetString(reader, 1);
        entities.Tribunal.Identifier = db.GetInt32(reader, 2);
        entities.Tribunal.FipCounty = db.GetNullableInt32(reader, 3);
        entities.Tribunal.FipState = db.GetNullableInt32(reader, 4);
        entities.Tribunal.Populated = true;
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
    /// A value of PersistentDebt.
    /// </summary>
    public ObligationTransaction PersistentDebt
    {
      get => persistentDebt ??= new();
      set => persistentDebt = value;
    }

    /// <summary>
    /// A value of PersistentCollection.
    /// </summary>
    public Collection PersistentCollection
    {
      get => persistentCollection ??= new();
      set => persistentCollection = value;
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
    /// A value of PersistentLegalAction.
    /// </summary>
    public LegalAction PersistentLegalAction
    {
      get => persistentLegalAction ??= new();
      set => persistentLegalAction = value;
    }

    /// <summary>
    /// A value of UseApSupportedOnly.
    /// </summary>
    public Common UseApSupportedOnly
    {
      get => useApSupportedOnly ??= new();
      set => useApSupportedOnly = value;
    }

    private ObligationTransaction? persistentDebt;
    private Collection? persistentCollection;
    private DateWorkArea? reportStartDate;
    private DateWorkArea? reportEndDate;
    private LegalAction? persistentLegalAction;
    private Common? useApSupportedOnly;
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
    /// A value of DateWorkArea.
    /// </summary>
    public DateWorkArea DateWorkArea
    {
      get => dateWorkArea ??= new();
      set => dateWorkArea = value;
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
    /// A value of Overlap.
    /// </summary>
    public DateWorkArea Overlap
    {
      get => overlap ??= new();
      set => overlap = value;
    }

    /// <summary>
    /// A value of CaseDate.
    /// </summary>
    public DateWorkArea CaseDate
    {
      get => caseDate ??= new();
      set => caseDate = value;
    }

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

    private DateWorkArea? dateWorkArea;
    private Case1? case1;
    private DateWorkArea? overlap;
    private DateWorkArea? caseDate;
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
    /// A value of Fips.
    /// </summary>
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
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
    /// A value of CaseAssignment.
    /// </summary>
    public CaseAssignment CaseAssignment
    {
      get => caseAssignment ??= new();
      set => caseAssignment = value;
    }

    /// <summary>
    /// A value of ObligationType.
    /// </summary>
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
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
    /// A value of Case1.
    /// </summary>
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
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
    /// A value of SupportedCaseRole.
    /// </summary>
    public CaseRole SupportedCaseRole
    {
      get => supportedCaseRole ??= new();
      set => supportedCaseRole = value;
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
    /// A value of SupportedCsePerson.
    /// </summary>
    public CsePerson SupportedCsePerson
    {
      get => supportedCsePerson ??= new();
      set => supportedCsePerson = value;
    }

    /// <summary>
    /// A value of LegalAction.
    /// </summary>
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
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
    /// A value of Tribunal.
    /// </summary>
    public Tribunal Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
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
    /// A value of Office.
    /// </summary>
    public Office Office
    {
      get => office ??= new();
      set => office = value;
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
    /// A value of County.
    /// </summary>
    public CseOrganization County
    {
      get => county ??= new();
      set => county = value;
    }

    /// <summary>
    /// A value of OfficeServiceProvider.
    /// </summary>
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
    }

    private Fips? fips;
    private ObligationTransaction? debt;
    private CaseAssignment? caseAssignment;
    private ObligationType? obligationType;
    private DebtDetail? debtDetail;
    private CsePersonAccount? supported;
    private CsePersonAccount? obligor;
    private Case1? case1;
    private CaseRole? apCaseRole;
    private CaseRole? supportedCaseRole;
    private CsePerson? apCsePerson;
    private CsePerson? supportedCsePerson;
    private LegalAction? legalAction;
    private Obligation? obligation;
    private Tribunal? tribunal;
    private CseOrganization? judicialDistrict;
    private Office? office;
    private CseOrganizationRelationship? cseOrganizationRelationship;
    private CseOrganization? county;
    private OfficeServiceProvider? officeServiceProvider;
  }
#endregion
}
