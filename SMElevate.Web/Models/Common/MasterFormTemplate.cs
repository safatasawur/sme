namespace SMElevate.Web.Models.Common;

public record MasterFieldDef(
    string SectionName,
    int SectionOrder,
    string FieldName,
    string FieldLabel,
    string FieldType,
    int DisplayOrder,
    string? LookupKey,
    bool DefaultRequired = true
);

public static class MasterFormTemplate
{
    public static readonly IReadOnlyList<MasterFieldDef> Fields = new MasterFieldDef[]
    {
        // ── Business Details ─────────────────────────────────────────────────
        new("Business Details", 1, "NameOfBusiness",       "Name of Business",              "text",             1,  null),
        new("Business Details", 1, "ContactPerson",        "Contact Person",                "text",             2,  null),
        new("Business Details", 1, "CellOrLandlineNo",     "Cell / Landline No.",           "text",             3,  null),
        new("Business Details", 1, "NTNNo",                "NTN No. (If applicable)",       "text",             4,  null,                           false),
        new("Business Details", 1, "BusinessAddress",      "Business Address",              "textarea",         5,  null),
        new("Business Details", 1, "AnnualSales",          "Annual Sales (Rs.)",            "number",           6,  null),
        new("Business Details", 1, "YearOfEstablishment",  "Year of Establishment",         "number",           7,  null),
        new("Business Details", 1, "NoOfEmployees",        "No. of Employees",              "number",           8,  null),
        new("Business Details", 1, "BusinessPremise",      "Business Premise",              "radio",            9,  LookupCodes.BusinessPremise),
        new("Business Details", 1, "IsBusinessRegistered", "Business Registration",         "radio",            10, LookupCodes.IsBusinessRegistered),
        new("Business Details", 1, "RegistrationAuthority","Registration Authority",        "text",             11, null,                           false),
        new("Business Details", 1, "BusinessStatus",       "Business Status",               "select",           12, LookupCodes.BusinessStatus),
        new("Business Details", 1, "BusinessNature",       "Business Nature",               "select",           13, LookupCodes.BusinessNature),
        new("Business Details", 1, "BusinessDescription",  "Business Description",          "textarea",         14, null),

        // ── Shareholding Details ─────────────────────────────────────────────
        new("Shareholding Details", 2, "Shareholders", "Individual / Shareholding Details", "shareholder_group", 1, null, false),

        // ── Facility Requested ───────────────────────────────────────────────
        new("Facility Requested", 3, "FacilityRequested", "Facility Requested",             "select",           1,  LookupCodes.FacilityType),
        new("Facility Requested", 3, "TypeOfFacility",    "Type of Facility",               "select",           2,  LookupCodes.TypeOfFacility),
        new("Facility Requested", 3, "Amount",            "Amount (Rs.)",                   "number",           3,  null),
        new("Facility Requested", 3, "Tenor",             "Tenor (Months)",                 "number",           4,  null),

        // ── Bank Details ─────────────────────────────────────────────────────
        new("Bank Details", 4, "AssignedBankId",   "Select Bank",              "bank_select", 1, null),
        new("Bank Details", 4, "IBANOrRaastType",  "Payment Identifier Type",  "select",      2, LookupCodes.IBANOrRaastType),
        new("Bank Details", 4, "IBANOrRaastValue", "IBAN / RAAST ID",          "text",        3, null),
    };
}
