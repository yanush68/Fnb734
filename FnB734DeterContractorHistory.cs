// Program: FN_B734_DETER_CONTRACTOR_HISTORY, ID: 1625343371, model: 746.
// Short name: SWE04000
using System;
using System.Collections.Generic;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B734_DETER_CONTRACTOR_HISTORY.
/// </summary>
[Serializable]
[Program("SWE04000")]
public partial class FnB734DeterContractorHistory: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B734_DETER_CONTRACTOR_HISTORY program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB734DeterContractorHistory(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB734DeterContractorHistory.
  /// </summary>
  public FnB734DeterContractorHistory(IContext context, Import import,
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
    local.LastRunDate.Date =
      AddDays(import.ProgramProcessingInfo.ProcessDate, -7);
    local.DateWorkAttributes.NumericalYear =
      Year(import.ProgramProcessingInfo.ProcessDate);
    local.DateWorkAttributes.NumericalMonth =
      Month(import.ProgramProcessingInfo.ProcessDate);
    local.DateWorkAttributes.TextDate10Char =
      NumberToString(local.DateWorkAttributes.NumericalYear, 12, 4) + "-" + NumberToString
      (local.DateWorkAttributes.NumericalMonth, 14, 2) + "-" + "01";
    local.FirstOfTheMonth.Date =
      AddMonths(StringToDate(local.DateWorkAttributes.TextDate10Char), 1);
    local.EndOfTheMonth.Date = AddDays(local.FirstOfTheMonth.Date, -1);
    local.Max.Date = new DateTime(2099, 12, 31);
    local.Current.Date = Now().Date;
    local.Processed.Code = "";
    local.Processed.Type1 = "";

    foreach(var _ in ReadCseOrganizationCseOrganizationCseOrganizationRelationship())
    {
      if (Equal(entities.JudicalDistrict.Code, local.Processed.Code) && AsChar
        (entities.JudicalDistrict.Type1) == AsChar(local.Processed.Type1))
      {
        foreach(var _1 in ReadContractorHistory1())
        {
          if (Lt(local.Current.Date, entities.ContractorHistory.EffectiveDate))
          {
            // this history record must have been added by mistake so it will be
            // deleted.
            DeleteContractorHistory();
          }
        }

        // can only have one relationship record per judical district, so this 
        // one will be deleted.
        DeleteCseOrganizationRelationship();

        continue;
      }

      MoveCseOrganization(entities.JudicalDistrict, local.Processed);

      if (ReadContractorHistoryCseOrganization())
      {
        local.ContractorHistory.Assign(local.Clear);

        if (!Equal(entities.Contractor2NdRead.Code, entities.Contractor.Code))
        {
          // contractor changed for judical district
          local.ContractorHistory.Assign(entities.ContractorHistory);
          local.ContractorHistory.EndDate = local.EndOfTheMonth.Date;

          try
          {
            UpdateContractorHistory();
            local.Add.Identifier = 0;

            if (ReadContractorHistory2())
            {
              local.Add.Identifier = entities.Read.Identifier + 1;
            }

            if (local.Add.Identifier < 1)
            {
              local.Add.Identifier = 1;
            }

            try
            {
              CreateContractorHistory();
            }
            catch(Exception e1)
            {
              switch(GetErrorCode(e1))
              {
                case ErrorCode.AlreadyExists:
                  ExitState = "ACO_NE0000_CONTRACTOR_HISTORY_AE";

                  return;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "ACO_NE0000_CONTRACTOR_HISTORY_PV";

                  return;
                default:
                  throw;
              }
            }
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "ACO_NE0000_CONTRACTOR_HISTORY_NU";

                return;
              case ErrorCode.PermittedValueViolation:
                ExitState = "ACO_NE0000_CONTRACTOR_HISTORY_PV";

                return;
              default:
                throw;
            }
          }
        }
        else
        {
          // no changes in contractors for judical district
        }

        continue;
      }
    }
  }

  private static void MoveCseOrganization(CseOrganization source,
    CseOrganization target)
  {
    target.Code = source.Code;
    target.Type1 = source.Type1;
  }

  private void CreateContractorHistory()
  {
    var identifier = local.Add.Identifier;
    var createdBy = global.UserId;
    var createdTimestmp = Now();
    var effectiveDate = local.FirstOfTheMonth.Date;
    var endDate = local.Max.Date;
    var fkCktCseOrgatypeCode = entities.Contractor.Type1;
    var fkCktCseOrgaorganztnId = entities.Contractor.Code;
    var fk0CktCseOrgatypeCode = entities.JudicalDistrict.Type1;
    var fk0CktCseOrgaorganztnId = entities.JudicalDistrict.Code;

    entities.Create.Populated = false;
    Update("CreateContractorHistory",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", identifier);
        db.SetNullableString(command, "createdBy", createdBy);
        db.SetNullableDateTime(command, "createdTimestmp", createdTimestmp);
        db.SetNullableDate(command, "effectiveDate", effectiveDate);
        db.SetNullableDate(command, "endDate", endDate);
        db.SetNullableString(command, "lastUpdatedBy", createdBy);
        db.SetNullableDateTime(command, "lastUpdateTimestmp", createdTimestmp);
        db.SetString(command, "fkCktCseOrgatypeCode", fkCktCseOrgatypeCode);
        db.SetString(command, "fkCktCseOrgaorganztnId", fkCktCseOrgaorganztnId);
        db.SetString(command, "fk0CktCseOrgatypeCode", fk0CktCseOrgatypeCode);
        db.
          SetString(command, "fk0CktCseOrgaorganztnId", fk0CktCseOrgaorganztnId);
      });

    entities.Create.Identifier = identifier;
    entities.Create.CreatedBy = createdBy;
    entities.Create.CreatedTimestmp = createdTimestmp;
    entities.Create.EffectiveDate = effectiveDate;
    entities.Create.EndDate = endDate;
    entities.Create.LastUpdatedBy = createdBy;
    entities.Create.LastUpdateTimestmp = createdTimestmp;
    entities.Create.FkCktCseOrgatypeCode = fkCktCseOrgatypeCode;
    entities.Create.FkCktCseOrgaorganztnId = fkCktCseOrgaorganztnId;
    entities.Create.Fk0CktCseOrgatypeCode = fk0CktCseOrgatypeCode;
    entities.Create.Fk0CktCseOrgaorganztnId = fk0CktCseOrgaorganztnId;
    entities.Create.Populated = true;
  }

  private void DeleteContractorHistory()
  {
    Update("DeleteContractorHistory",
      (db, command) =>
      {
        db.
          SetInt32(command, "identifier", entities.ContractorHistory.Identifier);
        db.SetString(
          command, "fkCktCseOrgatypeCode",
          entities.ContractorHistory.FkCktCseOrgatypeCode);
        db.SetString(
          command, "fkCktCseOrgaorganztnId",
          entities.ContractorHistory.FkCktCseOrgaorganztnId);
        db.SetString(
          command, "fk0CktCseOrgatypeCode",
          entities.ContractorHistory.Fk0CktCseOrgatypeCode);
        db.SetString(
          command, "fk0CktCseOrgaorganztnId",
          entities.ContractorHistory.Fk0CktCseOrgaorganztnId);
      });
  }

  private void DeleteCseOrganizationRelationship()
  {
    Update("DeleteCseOrganizationRelationship",
      (db, command) =>
      {
        db.SetString(
          command, "cogParentCode",
          entities.CseOrganizationRelationship.CogParentCode);
        db.SetString(
          command, "cogParentType",
          entities.CseOrganizationRelationship.CogParentType);
        db.SetString(
          command, "cogChildCode",
          entities.CseOrganizationRelationship.CogChildCode);
        db.SetString(
          command, "cogChildType",
          entities.CseOrganizationRelationship.CogChildType);
      });
  }

  private IEnumerable<bool> ReadContractorHistory1()
  {
    return ReadEach("ReadContractorHistory1",
      (db, command) =>
      {
        db.SetString(
          command, "fk0CktCseOrgatypeCode", entities.JudicalDistrict.Type1);
        db.SetString(
          command, "fk0CktCseOrgaorganztnId", entities.JudicalDistrict.Code);
        db.
          SetString(command, "fkCktCseOrgatypeCode", entities.Contractor.Type1);
        db.
          SetString(command, "fkCktCseOrgaorganztnId", entities.Contractor.Code);
      },
      (db, reader) =>
      {
        entities.ContractorHistory.Identifier = db.GetInt32(reader, 0);
        entities.ContractorHistory.CreatedBy = db.GetNullableString(reader, 1);
        entities.ContractorHistory.CreatedTimestmp =
          db.GetNullableDateTime(reader, 2);
        entities.ContractorHistory.EffectiveDate =
          db.GetNullableDate(reader, 3);
        entities.ContractorHistory.EndDate = db.GetNullableDate(reader, 4);
        entities.ContractorHistory.LastUpdatedBy =
          db.GetNullableString(reader, 5);
        entities.ContractorHistory.LastUpdateTimestmp =
          db.GetNullableDateTime(reader, 6);
        entities.ContractorHistory.FkCktCseOrgatypeCode =
          db.GetString(reader, 7);
        entities.ContractorHistory.FkCktCseOrgaorganztnId =
          db.GetString(reader, 8);
        entities.ContractorHistory.Fk0CktCseOrgatypeCode =
          db.GetString(reader, 9);
        entities.ContractorHistory.Fk0CktCseOrgaorganztnId =
          db.GetString(reader, 10);
        entities.ContractorHistory.Populated = true;

        return true;
      },
      () =>
      {
        entities.ContractorHistory.Populated = false;
      });
  }

  private bool ReadContractorHistory2()
  {
    entities.Read.Populated = false;

    return Read("ReadContractorHistory2",
      (db, command) =>
      {
        db.
          SetString(command, "fkCktCseOrgaorganztnId", entities.Contractor.Code);
        db.SetString(
          command, "fk0CktCseOrgaorganztnId", entities.JudicalDistrict.Code);
      },
      (db, reader) =>
      {
        entities.Read.Identifier = db.GetInt32(reader, 0);
        entities.Read.CreatedBy = db.GetNullableString(reader, 1);
        entities.Read.CreatedTimestmp = db.GetNullableDateTime(reader, 2);
        entities.Read.EffectiveDate = db.GetNullableDate(reader, 3);
        entities.Read.EndDate = db.GetNullableDate(reader, 4);
        entities.Read.LastUpdatedBy = db.GetNullableString(reader, 5);
        entities.Read.LastUpdateTimestmp = db.GetNullableDateTime(reader, 6);
        entities.Read.FkCktCseOrgatypeCode = db.GetString(reader, 7);
        entities.Read.FkCktCseOrgaorganztnId = db.GetString(reader, 8);
        entities.Read.Fk0CktCseOrgatypeCode = db.GetString(reader, 9);
        entities.Read.Fk0CktCseOrgaorganztnId = db.GetString(reader, 10);
        entities.Read.Populated = true;
      });
  }

  private bool ReadContractorHistoryCseOrganization()
  {
    entities.Contractor2NdRead.Populated = false;
    entities.ContractorHistory.Populated = false;

    return Read("ReadContractorHistoryCseOrganization",
      (db, command) =>
      {
        db.SetString(
          command, "fk0CktCseOrgatypeCode", entities.JudicalDistrict.Type1);
        db.SetString(
          command, "fk0CktCseOrgaorganztnId", entities.JudicalDistrict.Code);
      },
      (db, reader) =>
      {
        entities.ContractorHistory.Identifier = db.GetInt32(reader, 0);
        entities.ContractorHistory.CreatedBy = db.GetNullableString(reader, 1);
        entities.ContractorHistory.CreatedTimestmp =
          db.GetNullableDateTime(reader, 2);
        entities.ContractorHistory.EffectiveDate =
          db.GetNullableDate(reader, 3);
        entities.ContractorHistory.EndDate = db.GetNullableDate(reader, 4);
        entities.ContractorHistory.LastUpdatedBy =
          db.GetNullableString(reader, 5);
        entities.ContractorHistory.LastUpdateTimestmp =
          db.GetNullableDateTime(reader, 6);
        entities.ContractorHistory.FkCktCseOrgatypeCode =
          db.GetString(reader, 7);
        entities.Contractor2NdRead.Type1 = db.GetString(reader, 7);
        entities.ContractorHistory.FkCktCseOrgaorganztnId =
          db.GetString(reader, 8);
        entities.Contractor2NdRead.Code = db.GetString(reader, 8);
        entities.ContractorHistory.Fk0CktCseOrgatypeCode =
          db.GetString(reader, 9);
        entities.ContractorHistory.Fk0CktCseOrgaorganztnId =
          db.GetString(reader, 10);
        entities.Contractor2NdRead.Name = db.GetString(reader, 11);
        entities.ContractorHistory.Populated = true;
        entities.Contractor2NdRead.Populated = true;
      });
  }

  private IEnumerable<bool>
    ReadCseOrganizationCseOrganizationCseOrganizationRelationship()
  {
    return ReadEach(
      "ReadCseOrganizationCseOrganizationCseOrganizationRelationship",
      null,
      (db, reader) =>
      {
        entities.JudicalDistrict.Code = db.GetString(reader, 0);
        entities.CseOrganizationRelationship.CogParentCode =
          db.GetString(reader, 0);
        entities.JudicalDistrict.Type1 = db.GetString(reader, 1);
        entities.CseOrganizationRelationship.CogParentType =
          db.GetString(reader, 1);
        entities.Contractor.Code = db.GetString(reader, 2);
        entities.CseOrganizationRelationship.CogChildCode =
          db.GetString(reader, 2);
        entities.Contractor.Type1 = db.GetString(reader, 3);
        entities.CseOrganizationRelationship.CogChildType =
          db.GetString(reader, 3);
        entities.Contractor.Name = db.GetString(reader, 4);
        entities.CseOrganizationRelationship.ReasonCode =
          db.GetString(reader, 5);
        entities.CseOrganizationRelationship.CreatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.JudicalDistrict.Populated = true;
        entities.Contractor.Populated = true;
        entities.CseOrganizationRelationship.Populated = true;

        return true;
      },
      () =>
      {
        entities.Contractor.Populated = false;
        entities.JudicalDistrict.Populated = false;
        entities.CseOrganizationRelationship.Populated = false;
      });
  }

  private void UpdateContractorHistory()
  {
    System.Diagnostics.Debug.Assert(entities.ContractorHistory.Populated);

    var endDate = local.ContractorHistory.EndDate;
    var lastUpdatedBy = global.UserId;
    var lastUpdateTimestmp = Now();

    entities.ContractorHistory.Populated = false;
    Update("UpdateContractorHistory",
      (db, command) =>
      {
        db.SetNullableDate(command, "endDate", endDate);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdateTimestmp", lastUpdateTimestmp);
        db.
          SetInt32(command, "identifier", entities.ContractorHistory.Identifier);
        db.SetString(
          command, "fkCktCseOrgatypeCode",
          entities.ContractorHistory.FkCktCseOrgatypeCode);
        db.SetString(
          command, "fkCktCseOrgaorganztnId",
          entities.ContractorHistory.FkCktCseOrgaorganztnId);
        db.SetString(
          command, "fk0CktCseOrgatypeCode",
          entities.ContractorHistory.Fk0CktCseOrgatypeCode);
        db.SetString(
          command, "fk0CktCseOrgaorganztnId",
          entities.ContractorHistory.Fk0CktCseOrgaorganztnId);
      });

    entities.ContractorHistory.EndDate = endDate;
    entities.ContractorHistory.LastUpdatedBy = lastUpdatedBy;
    entities.ContractorHistory.LastUpdateTimestmp = lastUpdateTimestmp;
    entities.ContractorHistory.Populated = true;
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
    /// A value of Processed.
    /// </summary>
    public CseOrganization Processed
    {
      get => processed ??= new();
      set => processed = value;
    }

    /// <summary>
    /// A value of Add.
    /// </summary>
    public ContractorHistory Add
    {
      get => add ??= new();
      set => add = value;
    }

    /// <summary>
    /// A value of Clear.
    /// </summary>
    public ContractorHistory Clear
    {
      get => clear ??= new();
      set => clear = value;
    }

    /// <summary>
    /// A value of ContractorHistory.
    /// </summary>
    public ContractorHistory ContractorHistory
    {
      get => contractorHistory ??= new();
      set => contractorHistory = value;
    }

    /// <summary>
    /// A value of Max.
    /// </summary>
    public DateWorkArea Max
    {
      get => max ??= new();
      set => max = value;
    }

    /// <summary>
    /// A value of DateWorkAttributes.
    /// </summary>
    public DateWorkAttributes DateWorkAttributes
    {
      get => dateWorkAttributes ??= new();
      set => dateWorkAttributes = value;
    }

    /// <summary>
    /// A value of FirstOfTheMonth.
    /// </summary>
    public DateWorkArea FirstOfTheMonth
    {
      get => firstOfTheMonth ??= new();
      set => firstOfTheMonth = value;
    }

    /// <summary>
    /// A value of EndOfTheMonth.
    /// </summary>
    public DateWorkArea EndOfTheMonth
    {
      get => endOfTheMonth ??= new();
      set => endOfTheMonth = value;
    }

    /// <summary>
    /// A value of Work.
    /// </summary>
    public DateWorkArea Work
    {
      get => work ??= new();
      set => work = value;
    }

    /// <summary>
    /// A value of LastRunDate.
    /// </summary>
    public DateWorkArea LastRunDate
    {
      get => lastRunDate ??= new();
      set => lastRunDate = value;
    }

    /// <summary>
    /// A value of Current.
    /// </summary>
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of TextWorkArea.
    /// </summary>
    public TextWorkArea TextWorkArea
    {
      get => textWorkArea ??= new();
      set => textWorkArea = value;
    }

    private CseOrganization? processed;
    private ContractorHistory? add;
    private ContractorHistory? clear;
    private ContractorHistory? contractorHistory;
    private DateWorkArea? max;
    private DateWorkAttributes? dateWorkAttributes;
    private DateWorkArea? firstOfTheMonth;
    private DateWorkArea? endOfTheMonth;
    private DateWorkArea? work;
    private DateWorkArea? lastRunDate;
    private DateWorkArea? current;
    private TextWorkArea? textWorkArea;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Read.
    /// </summary>
    public ContractorHistory Read
    {
      get => read ??= new();
      set => read = value;
    }

    /// <summary>
    /// A value of ContractorRead.
    /// </summary>
    public CseOrganization ContractorRead
    {
      get => contractorRead ??= new();
      set => contractorRead = value;
    }

    /// <summary>
    /// A value of JudicalDistrictRead.
    /// </summary>
    public CseOrganization JudicalDistrictRead
    {
      get => judicalDistrictRead ??= new();
      set => judicalDistrictRead = value;
    }

    /// <summary>
    /// A value of Create.
    /// </summary>
    public ContractorHistory Create
    {
      get => create ??= new();
      set => create = value;
    }

    /// <summary>
    /// A value of Contractor2NdRead.
    /// </summary>
    public CseOrganization Contractor2NdRead
    {
      get => contractor2NdRead ??= new();
      set => contractor2NdRead = value;
    }

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

    private ContractorHistory? read;
    private CseOrganization? contractorRead;
    private CseOrganization? judicalDistrictRead;
    private ContractorHistory? create;
    private CseOrganization? contractor2NdRead;
    private ContractorHistory? contractorHistory;
    private CseOrganization? contractor;
    private CseOrganization? judicalDistrict;
    private CseOrganizationRelationship? cseOrganizationRelationship;
  }
#endregion
}
