using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SMElevate.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddV22LifecycleAndModules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "EffectiveDate",
                table: "SchemeForms",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FormStatus",
                table: "SchemeForms",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "PublishedAt",
                table: "SchemeForms",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VersionNumber",
                table: "SchemeForms",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ActorType",
                table: "LoanRequestStatusHistories",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IpAddress",
                table: "LoanRequestStatusHistories",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReasonCode",
                table: "LoanRequestStatusHistories",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AccountValidationMessage",
                table: "LoanRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AccountValidationStatus",
                table: "LoanRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CaseId",
                table: "LoanRequests",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ConsentDate",
                table: "LoanRequests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ConsentGiven",
                table: "LoanRequests",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ConsentIpAddress",
                table: "LoanRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConsentVersion",
                table: "LoanRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IdentifiedBankId",
                table: "LoanRequests",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDraft",
                table: "LoanRequests",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PreferredIdentifierType",
                table: "LoanRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AdditionalInfoRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LoanRequestId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RequiredDocuments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ApplicantResponse = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResponseDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RespondedByUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdditionalInfoRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdditionalInfoRequests_LoanRequests_LoanRequestId",
                        column: x => x.LoanRequestId,
                        principalTable: "LoanRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AdditionalInfoRequests_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AdditionalInfoRequests_Users_RespondedByUserId",
                        column: x => x.RespondedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ApplicationMonitorings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LoanRequestId = table.Column<int>(type: "int", nullable: false),
                    MonitoringStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NextReviewDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationMonitorings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApplicationMonitorings_LoanRequests_LoanRequestId",
                        column: x => x.LoanRequestId,
                        principalTable: "LoanRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ApplicationMonitorings_Users_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "BankAssessments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LoanRequestId = table.Column<int>(type: "int", nullable: false),
                    AssessmentType = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CheckDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResultSummary = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ScorecardReference = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Result = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RiskCategory = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AssessmentDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CDDStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    KYCStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AMLStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SanctionsScreeningStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PEPScreeningStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ComplianceResult = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CompletionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AttachmentPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankAssessments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BankAssessments_LoanRequests_LoanRequestId",
                        column: x => x.LoanRequestId,
                        principalTable: "LoanRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BankAssessments_Users_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ConditionalOffers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OfferNumber = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoanRequestId = table.Column<int>(type: "int", nullable: false),
                    OfferVersion = table.Column<int>(type: "int", nullable: false),
                    IssueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FacilityType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApprovedAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TenorMonths = table.Column<int>(type: "int", nullable: false),
                    PricingSummary = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TermsAndConditions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConditionsPrecedent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OfferLetterPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConditionalOffers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConditionalOffers_LoanRequests_LoanRequestId",
                        column: x => x.LoanRequestId,
                        principalTable: "LoanRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ConditionalOffers_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DeclineReasonCodes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeclineReasonCodes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Disbursements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LoanRequestId = table.Column<int>(type: "int", nullable: false),
                    DisbursementStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ApprovedAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DisbursedAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    ValueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DisbursementAccount = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BankReferenceNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Disbursements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Disbursements_LoanRequests_LoanRequestId",
                        column: x => x.LoanRequestId,
                        principalTable: "LoanRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Disbursements_Users_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PostApprovalChecklists",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LoanRequestId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostApprovalChecklists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PostApprovalChecklists_LoanRequests_LoanRequestId",
                        column: x => x.LoanRequestId,
                        principalTable: "LoanRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PostApprovalChecklists_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[UserExternalLogins]') AND type = 'U')
BEGIN
    CREATE TABLE [UserExternalLogins] (
        [Id] int NOT NULL IDENTITY,
        [UserId] int NOT NULL,
        [Provider] nvarchar(450) NOT NULL,
        [ProviderUserId] nvarchar(450) NOT NULL,
        [ProviderEmail] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [LastLoginAt] datetime2 NULL,
        CONSTRAINT [PK_UserExternalLogins] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_UserExternalLogins_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
    );
END");

            migrationBuilder.CreateTable(
                name: "WorkflowStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StatusCode = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    StatusName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsInitial = table.Column<bool>(type: "bit", nullable: false),
                    IsFinal = table.Column<bool>(type: "bit", nullable: false),
                    AllowedActorTypes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ColorClass = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowTransitions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FromStatusCode = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ToStatusCode = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AllowedActorTypes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ActionLabel = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RequiresRemarks = table.Column<bool>(type: "bit", nullable: false),
                    RequiresReasonCode = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowTransitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ConditionalOfferResponses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConditionalOfferId = table.Column<int>(type: "int", nullable: false),
                    ResponseType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ResponseDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RespondedByUserId = table.Column<int>(type: "int", nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OfferVersion = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConditionalOfferResponses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConditionalOfferResponses_ConditionalOffers_ConditionalOfferId",
                        column: x => x.ConditionalOfferId,
                        principalTable: "ConditionalOffers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ConditionalOfferResponses_Users_RespondedByUserId",
                        column: x => x.RespondedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "BankDecisions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LoanRequestId = table.Column<int>(type: "int", nullable: false),
                    DecisionType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DecisionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DecisionRemarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeclineReasonCodeId = table.Column<int>(type: "int", nullable: true),
                    ApprovedFacilityType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApprovedAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    ApprovedTenorMonths = table.Column<int>(type: "int", nullable: true),
                    AdditionalConditions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MadeByUserId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankDecisions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BankDecisions_DeclineReasonCodes_DeclineReasonCodeId",
                        column: x => x.DeclineReasonCodeId,
                        principalTable: "DeclineReasonCodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_BankDecisions_LoanRequests_LoanRequestId",
                        column: x => x.LoanRequestId,
                        principalTable: "LoanRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BankDecisions_Users_MadeByUserId",
                        column: x => x.MadeByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PostApprovalChecklistItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChecklistId = table.Column<int>(type: "int", nullable: false),
                    ItemName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ItemDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    DocumentRequired = table.Column<bool>(type: "bit", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SubmittedDocumentPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SubmittedByUserId = table.Column<int>(type: "int", nullable: true),
                    VerificationStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VerifiedByUserId = table.Column<int>(type: "int", nullable: true),
                    VerifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    VerificationRemarks = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostApprovalChecklistItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PostApprovalChecklistItems_PostApprovalChecklists_ChecklistId",
                        column: x => x.ChecklistId,
                        principalTable: "PostApprovalChecklists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PostApprovalChecklistItems_Users_SubmittedByUserId",
                        column: x => x.SubmittedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PostApprovalChecklistItems_Users_VerifiedByUserId",
                        column: x => x.VerifiedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "ApplicationDocuments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LoanRequestId = table.Column<int>(type: "int", nullable: false),
                    DocumentType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AdditionalInfoRequestId = table.Column<int>(type: "int", nullable: true),
                    ChecklistItemId = table.Column<int>(type: "int", nullable: true),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OriginalFileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UploadedByUserId = table.Column<int>(type: "int", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApplicationDocuments_AdditionalInfoRequests_AdditionalInfoRequestId",
                        column: x => x.AdditionalInfoRequestId,
                        principalTable: "AdditionalInfoRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_ApplicationDocuments_LoanRequests_LoanRequestId",
                        column: x => x.LoanRequestId,
                        principalTable: "LoanRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ApplicationDocuments_PostApprovalChecklistItems_ChecklistItemId",
                        column: x => x.ChecklistItemId,
                        principalTable: "PostApprovalChecklistItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_ApplicationDocuments_Users_UploadedByUserId",
                        column: x => x.UploadedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_LoanRequests_CaseId",
                table: "LoanRequests",
                column: "CaseId",
                unique: true,
                filter: "[CaseId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_LoanRequests_IdentifiedBankId",
                table: "LoanRequests",
                column: "IdentifiedBankId");

            migrationBuilder.CreateIndex(
                name: "IX_AdditionalInfoRequests_CreatedByUserId",
                table: "AdditionalInfoRequests",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AdditionalInfoRequests_LoanRequestId",
                table: "AdditionalInfoRequests",
                column: "LoanRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_AdditionalInfoRequests_RespondedByUserId",
                table: "AdditionalInfoRequests",
                column: "RespondedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationDocuments_AdditionalInfoRequestId",
                table: "ApplicationDocuments",
                column: "AdditionalInfoRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationDocuments_ChecklistItemId",
                table: "ApplicationDocuments",
                column: "ChecklistItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationDocuments_LoanRequestId",
                table: "ApplicationDocuments",
                column: "LoanRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationDocuments_UploadedByUserId",
                table: "ApplicationDocuments",
                column: "UploadedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationMonitorings_LoanRequestId",
                table: "ApplicationMonitorings",
                column: "LoanRequestId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationMonitorings_UpdatedByUserId",
                table: "ApplicationMonitorings",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_BankAssessments_LoanRequestId_AssessmentType",
                table: "BankAssessments",
                columns: new[] { "LoanRequestId", "AssessmentType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BankAssessments_UpdatedByUserId",
                table: "BankAssessments",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_BankDecisions_DeclineReasonCodeId",
                table: "BankDecisions",
                column: "DeclineReasonCodeId");

            migrationBuilder.CreateIndex(
                name: "IX_BankDecisions_LoanRequestId",
                table: "BankDecisions",
                column: "LoanRequestId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BankDecisions_MadeByUserId",
                table: "BankDecisions",
                column: "MadeByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ConditionalOfferResponses_ConditionalOfferId",
                table: "ConditionalOfferResponses",
                column: "ConditionalOfferId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConditionalOfferResponses_RespondedByUserId",
                table: "ConditionalOfferResponses",
                column: "RespondedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ConditionalOffers_CreatedByUserId",
                table: "ConditionalOffers",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ConditionalOffers_LoanRequestId",
                table: "ConditionalOffers",
                column: "LoanRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_ConditionalOffers_OfferNumber",
                table: "ConditionalOffers",
                column: "OfferNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeclineReasonCodes_Code",
                table: "DeclineReasonCodes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Disbursements_LoanRequestId",
                table: "Disbursements",
                column: "LoanRequestId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Disbursements_UpdatedByUserId",
                table: "Disbursements",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PostApprovalChecklistItems_ChecklistId",
                table: "PostApprovalChecklistItems",
                column: "ChecklistId");

            migrationBuilder.CreateIndex(
                name: "IX_PostApprovalChecklistItems_SubmittedByUserId",
                table: "PostApprovalChecklistItems",
                column: "SubmittedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PostApprovalChecklistItems_VerifiedByUserId",
                table: "PostApprovalChecklistItems",
                column: "VerifiedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PostApprovalChecklists_CreatedByUserId",
                table: "PostApprovalChecklists",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PostApprovalChecklists_LoanRequestId",
                table: "PostApprovalChecklists",
                column: "LoanRequestId");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_UserExternalLogins_Provider_ProviderUserId' AND object_id = OBJECT_ID('[UserExternalLogins]'))
    CREATE UNIQUE INDEX [IX_UserExternalLogins_Provider_ProviderUserId] ON [UserExternalLogins] ([Provider], [ProviderUserId]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_UserExternalLogins_UserId_Provider' AND object_id = OBJECT_ID('[UserExternalLogins]'))
    CREATE UNIQUE INDEX [IX_UserExternalLogins_UserId_Provider] ON [UserExternalLogins] ([UserId], [Provider]);");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowStatuses_StatusCode",
                table: "WorkflowStatuses",
                column: "StatusCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowTransitions_FromStatusCode_ToStatusCode",
                table: "WorkflowTransitions",
                columns: new[] { "FromStatusCode", "ToStatusCode" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_LoanRequests_Banks_IdentifiedBankId",
                table: "LoanRequests",
                column: "IdentifiedBankId",
                principalTable: "Banks",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LoanRequests_Banks_IdentifiedBankId",
                table: "LoanRequests");

            migrationBuilder.DropTable(
                name: "ApplicationDocuments");

            migrationBuilder.DropTable(
                name: "ApplicationMonitorings");

            migrationBuilder.DropTable(
                name: "BankAssessments");

            migrationBuilder.DropTable(
                name: "BankDecisions");

            migrationBuilder.DropTable(
                name: "ConditionalOfferResponses");

            migrationBuilder.DropTable(
                name: "Disbursements");

            migrationBuilder.Sql("IF OBJECT_ID(N'[UserExternalLogins]', 'U') IS NOT NULL DROP TABLE [UserExternalLogins];");

            migrationBuilder.DropTable(
                name: "WorkflowStatuses");

            migrationBuilder.DropTable(
                name: "WorkflowTransitions");

            migrationBuilder.DropTable(
                name: "AdditionalInfoRequests");

            migrationBuilder.DropTable(
                name: "PostApprovalChecklistItems");

            migrationBuilder.DropTable(
                name: "DeclineReasonCodes");

            migrationBuilder.DropTable(
                name: "ConditionalOffers");

            migrationBuilder.DropTable(
                name: "PostApprovalChecklists");

            migrationBuilder.DropIndex(
                name: "IX_LoanRequests_CaseId",
                table: "LoanRequests");

            migrationBuilder.DropIndex(
                name: "IX_LoanRequests_IdentifiedBankId",
                table: "LoanRequests");

            migrationBuilder.DropColumn(
                name: "EffectiveDate",
                table: "SchemeForms");

            migrationBuilder.DropColumn(
                name: "FormStatus",
                table: "SchemeForms");

            migrationBuilder.DropColumn(
                name: "PublishedAt",
                table: "SchemeForms");

            migrationBuilder.DropColumn(
                name: "VersionNumber",
                table: "SchemeForms");

            migrationBuilder.DropColumn(
                name: "ActorType",
                table: "LoanRequestStatusHistories");

            migrationBuilder.DropColumn(
                name: "IpAddress",
                table: "LoanRequestStatusHistories");

            migrationBuilder.DropColumn(
                name: "ReasonCode",
                table: "LoanRequestStatusHistories");

            migrationBuilder.DropColumn(
                name: "AccountValidationMessage",
                table: "LoanRequests");

            migrationBuilder.DropColumn(
                name: "AccountValidationStatus",
                table: "LoanRequests");

            migrationBuilder.DropColumn(
                name: "CaseId",
                table: "LoanRequests");

            migrationBuilder.DropColumn(
                name: "ConsentDate",
                table: "LoanRequests");

            migrationBuilder.DropColumn(
                name: "ConsentGiven",
                table: "LoanRequests");

            migrationBuilder.DropColumn(
                name: "ConsentIpAddress",
                table: "LoanRequests");

            migrationBuilder.DropColumn(
                name: "ConsentVersion",
                table: "LoanRequests");

            migrationBuilder.DropColumn(
                name: "IdentifiedBankId",
                table: "LoanRequests");

            migrationBuilder.DropColumn(
                name: "IsDraft",
                table: "LoanRequests");

            migrationBuilder.DropColumn(
                name: "PreferredIdentifierType",
                table: "LoanRequests");
        }
    }
}
