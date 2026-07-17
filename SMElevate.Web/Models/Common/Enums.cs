namespace SMElevate.Web.Models.Common;

public enum UserType
{
    SME,
    Bank,
    SBP,
    Admin
}

public enum NotificationType
{
    Info,
    Success,
    Warning,
    Error
}

public enum EmailStatus
{
    Pending,
    Sent,
    Failed
}

public enum EmailRecipientType
{
    EndUser,
    Bank,
    Admin
}

public enum SettingCategory
{
    Email,
    Notification,
    OAuth,
    General
}

public static class AppStatus
{
    // Legacy statuses (preserved for backward compatibility)
    public const string Submitted = "Submitted";
    public const string Assigned = "Assigned";
    public const string InProcess = "In Process";
    public const string Approved = "Approved";
    public const string Rejected = "Rejected";
    public const string MoreInformationRequired = "More Information Required";
    public const string Completed = "Completed";

    // V2.2 extended 26-stage lifecycle
    public const string Draft = "Draft";
    public const string ValidationPending = "Validation Pending";
    public const string ValidationFailed = "Validation Failed";
    public const string Validated = "Validated";
    public const string ReferredToBank = "Referred to Bank";
    public const string UnderCreditBureauCheck = "Under Credit Bureau Check";
    public const string UnderRiskAssessment = "Under Risk Assessment";
    public const string UnderCDDComplianceReview = "Under CDD/Compliance Review";
    public const string AdditionalInformationRequired = "Additional Information Required";
    public const string UnderBankDecision = "Under Bank Decision";
    public const string ConditionallyApproved = "Conditionally Approved";
    public const string Declined = "Declined";
    public const string OfferIssued = "Offer Issued";
    public const string OfferAccepted = "Offer Accepted";
    public const string OfferRejected = "Offer Rejected";
    public const string OfferExpired = "Offer Expired";
    public const string PostApprovalFormalities = "Post-Approval Formalities";
    public const string DocumentsPending = "Documents Pending";
    public const string DocumentsCompleted = "Documents Completed";
    public const string ReadyForDisbursement = "Ready for Disbursement";
    public const string Disbursed = "Disbursed";
    public const string UnderMonitoring = "Under Monitoring";
    public const string Closed = "Closed";
    public const string Withdrawn = "Withdrawn";
}

public static class LookupCodes
{
    public const string Status = "STATUS";
    public const string Tenor = "TENOR";
    public const string BusinessNature = "BUSINESS_NATURE";
    public const string BusinessPremise = "BUSINESS_PREMISE";
    public const string BusinessStatus = "BUSINESS_STATUS";
    public const string FacilityType = "FACILITY_TYPE";
    public const string Gender = "GENDER";
    public const string UserTypeLookup = "USER_TYPE";
    public const string IsBusinessRegistered = "IS_BUSINESS_REGISTERED";
    public const string TypeOfFacility = "TYPE_OF_FACILITY";
    public const string IBANOrRaastType = "IBAN_RAAST_TYPE";
    public const string RiskCategory = "RISK_CATEGORY";
    public const string ComplianceStatus = "COMPLIANCE_STATUS";
}

public enum ActorType
{
    EndUser,
    Bank,
    Admin,
    System
}
