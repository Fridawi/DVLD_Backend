using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DVLD.INFRASTRUCTURE.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApplicationTypes",
                columns: table => new
                {
                    ApplicationTypeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Fees = table.Column<decimal>(type: "smallmoney", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationTypes", x => x.ApplicationTypeID);
                });

            migrationBuilder.CreateTable(
                name: "Countries",
                columns: table => new
                {
                    CountryID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CountryName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Countries", x => x.CountryID);
                });

            migrationBuilder.CreateTable(
                name: "LicenseClasses",
                columns: table => new
                {
                    LicenseClassID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClassName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ClassDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    MinimumAllowedAge = table.Column<byte>(type: "tinyint", nullable: false),
                    DefaultValidityLength = table.Column<byte>(type: "tinyint", nullable: false),
                    ClassFees = table.Column<decimal>(type: "smallmoney", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LicenseClasses", x => x.LicenseClassID);
                });

            migrationBuilder.CreateTable(
                name: "TestTypes",
                columns: table => new
                {
                    TestTypeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Fees = table.Column<decimal>(type: "smallmoney", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestTypes", x => x.TestTypeID);
                });

            migrationBuilder.CreateTable(
                name: "People",
                columns: table => new
                {
                    PersonID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SecondName = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ThirdName = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    NationalNo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "datetime", nullable: false),
                    Gendor = table.Column<byte>(type: "tinyint", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    NationalityCountryID = table.Column<int>(type: "int", nullable: false),
                    ImagePath = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_People", x => x.PersonID);
                    table.ForeignKey(
                        name: "FK_People_Countries_NationalityCountryID",
                        column: x => x.NationalityCountryID,
                        principalTable: "Countries",
                        principalColumn: "CountryID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Drivers",
                columns: table => new
                {
                    DriverID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PersonID = table.Column<int>(type: "int", nullable: false),
                    CreatedByUserID = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "smalldatetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Drivers", x => x.DriverID);
                    table.ForeignKey(
                        name: "FK_Drivers_People_PersonID",
                        column: x => x.PersonID,
                        principalTable: "People",
                        principalColumn: "PersonID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserName = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Password = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PersonID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserID);
                    table.ForeignKey(
                        name: "FK_Users_People_PersonID",
                        column: x => x.PersonID,
                        principalTable: "People",
                        principalColumn: "PersonID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Applications",
                columns: table => new
                {
                    ApplicationID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApplicantPersonID = table.Column<int>(type: "int", nullable: false),
                    ApplicationDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    ApplicationTypeID = table.Column<int>(type: "int", nullable: false),
                    ApplicationStatus = table.Column<byte>(type: "tinyint", nullable: false),
                    LastStatusDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    PaidFees = table.Column<decimal>(type: "smallmoney", nullable: false),
                    CreatedByUserID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Applications", x => x.ApplicationID);
                    table.ForeignKey(
                        name: "FK_Applications_ApplicationTypes_ApplicationTypeID",
                        column: x => x.ApplicationTypeID,
                        principalTable: "ApplicationTypes",
                        principalColumn: "ApplicationTypeID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Applications_People_ApplicantPersonID",
                        column: x => x.ApplicantPersonID,
                        principalTable: "People",
                        principalColumn: "PersonID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Applications_Users_CreatedByUserID",
                        column: x => x.CreatedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID");
                });

            migrationBuilder.CreateTable(
                name: "Licenses",
                columns: table => new
                {
                    LicenseID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApplicationID = table.Column<int>(type: "int", nullable: false),
                    DriverID = table.Column<int>(type: "int", nullable: false),
                    LicenseClassID = table.Column<int>(type: "int", nullable: false),
                    IssueDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    ExpirationDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PaidFees = table.Column<decimal>(type: "smallmoney", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IssueReason = table.Column<byte>(type: "tinyint", nullable: false),
                    CreatedByUserID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Licenses", x => x.LicenseID);
                    table.ForeignKey(
                        name: "FK_Licenses_Applications_ApplicationID",
                        column: x => x.ApplicationID,
                        principalTable: "Applications",
                        principalColumn: "ApplicationID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Licenses_Drivers_DriverID",
                        column: x => x.DriverID,
                        principalTable: "Drivers",
                        principalColumn: "DriverID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Licenses_LicenseClasses_LicenseClassID",
                        column: x => x.LicenseClassID,
                        principalTable: "LicenseClasses",
                        principalColumn: "LicenseClassID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LocalDrivingLicenseApplications",
                columns: table => new
                {
                    LocalDrivingLicenseApplicationID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApplicationID = table.Column<int>(type: "int", nullable: false),
                    LicenseClassID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocalDrivingLicenseApplications", x => x.LocalDrivingLicenseApplicationID);
                    table.ForeignKey(
                        name: "FK_LocalDrivingLicenseApplications_Applications_ApplicationID",
                        column: x => x.ApplicationID,
                        principalTable: "Applications",
                        principalColumn: "ApplicationID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LocalDrivingLicenseApplications_LicenseClasses_LicenseClassID",
                        column: x => x.LicenseClassID,
                        principalTable: "LicenseClasses",
                        principalColumn: "LicenseClassID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DetainedLicenses",
                columns: table => new
                {
                    DetainID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LicenseID = table.Column<int>(type: "int", nullable: false),
                    DetainDate = table.Column<DateTime>(type: "smalldatetime", nullable: false),
                    FineFees = table.Column<decimal>(type: "smallmoney", nullable: false),
                    CreatedByUserID = table.Column<int>(type: "int", nullable: false),
                    IsReleased = table.Column<bool>(type: "bit", nullable: false),
                    ReleaseDate = table.Column<DateTime>(type: "smalldatetime", nullable: true),
                    ReleasedByUserID = table.Column<int>(type: "int", nullable: true),
                    ReleaseApplicationID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetainedLicenses", x => x.DetainID);
                    table.ForeignKey(
                        name: "FK_DetainedLicenses_Applications_ReleaseApplicationID",
                        column: x => x.ReleaseApplicationID,
                        principalTable: "Applications",
                        principalColumn: "ApplicationID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DetainedLicenses_Licenses_LicenseID",
                        column: x => x.LicenseID,
                        principalTable: "Licenses",
                        principalColumn: "LicenseID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DetainedLicenses_Users_CreatedByUserID",
                        column: x => x.CreatedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DetainedLicenses_Users_ReleasedByUserID",
                        column: x => x.ReleasedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InternationalLicenses",
                columns: table => new
                {
                    InternationalLicenseID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApplicationID = table.Column<int>(type: "int", nullable: false),
                    DriverID = table.Column<int>(type: "int", nullable: false),
                    IssuedUsingLocalLicenseID = table.Column<int>(type: "int", nullable: false),
                    IssueDate = table.Column<DateTime>(type: "smalldatetime", nullable: false),
                    ExpirationDate = table.Column<DateTime>(type: "smalldatetime", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedByUserID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InternationalLicenses", x => x.InternationalLicenseID);
                    table.ForeignKey(
                        name: "FK_InternationalLicenses_Applications_ApplicationID",
                        column: x => x.ApplicationID,
                        principalTable: "Applications",
                        principalColumn: "ApplicationID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InternationalLicenses_Drivers_DriverID",
                        column: x => x.DriverID,
                        principalTable: "Drivers",
                        principalColumn: "DriverID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InternationalLicenses_Licenses_IssuedUsingLocalLicenseID",
                        column: x => x.IssuedUsingLocalLicenseID,
                        principalTable: "Licenses",
                        principalColumn: "LicenseID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InternationalLicenses_Users_CreatedByUserID",
                        column: x => x.CreatedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TestAppointments",
                columns: table => new
                {
                    TestAppointmentID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TestTypeID = table.Column<int>(type: "int", nullable: false),
                    LocalDrivingLicenseApplicationID = table.Column<int>(type: "int", nullable: false),
                    AppointmentDate = table.Column<DateTime>(type: "smalldatetime", nullable: false),
                    PaidFees = table.Column<decimal>(type: "smallmoney", nullable: false),
                    CreatedByUserID = table.Column<int>(type: "int", nullable: false),
                    IsLocked = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    RetakeTestApplicationID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestAppointments", x => x.TestAppointmentID);
                    table.ForeignKey(
                        name: "FK_TestAppointments_Applications_RetakeTestApplicationID",
                        column: x => x.RetakeTestApplicationID,
                        principalTable: "Applications",
                        principalColumn: "ApplicationID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TestAppointments_LocalDrivingLicenseApplications_LocalDrivingLicenseApplicationID",
                        column: x => x.LocalDrivingLicenseApplicationID,
                        principalTable: "LocalDrivingLicenseApplications",
                        principalColumn: "LocalDrivingLicenseApplicationID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TestAppointments_Users_CreatedByUserID",
                        column: x => x.CreatedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Tests",
                columns: table => new
                {
                    TestID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TestAppointmentID = table.Column<int>(type: "int", nullable: false),
                    TestResult = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedByUserID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tests", x => x.TestID);
                    table.ForeignKey(
                        name: "FK_Tests_TestAppointments_TestAppointmentID",
                        column: x => x.TestAppointmentID,
                        principalTable: "TestAppointments",
                        principalColumn: "TestAppointmentID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "ApplicationTypes",
                columns: new[] { "ApplicationTypeID", "Fees", "Title" },
                values: new object[,]
                {
                    { 1, 15m, "New Local Driving License Service" },
                    { 2, 7m, "Renew Driving License Service" },
                    { 3, 10m, "Replacement for a Lost Driving License" },
                    { 4, 5m, "Replacement for a Damaged Driving License" },
                    { 5, 15m, "Release Detained Driving Licsense" },
                    { 6, 51m, "New International License" },
                    { 7, 5m, "Retake Test" }
                });

            migrationBuilder.InsertData(
                table: "Countries",
                columns: new[] { "CountryID", "CountryName" },
                values: new object[,]
                {
                    { 1, "Afghanistan" },
                    { 2, "Albania" },
                    { 3, "Algeria" },
                    { 4, "Andorra" },
                    { 5, "Angola" },
                    { 6, "Antigua and Barbuda" },
                    { 7, "Argentina" },
                    { 8, "Armenia" },
                    { 9, "Australia" },
                    { 10, "Austria" },
                    { 11, "Azerbaijan" },
                    { 12, "Bahamas" },
                    { 13, "Bahrain" },
                    { 14, "Bangladesh" },
                    { 15, "Barbados" },
                    { 16, "Belarus" },
                    { 17, "Belgium" },
                    { 18, "Belize" },
                    { 19, "Benin" },
                    { 20, "Bhutan" },
                    { 21, "Bolivia" },
                    { 22, "Bosnia and Herzegovina" },
                    { 23, "Botswana" },
                    { 24, "Brazil" },
                    { 25, "Brunei" },
                    { 26, "Bulgaria" },
                    { 27, "Burkina Faso" },
                    { 28, "Burundi" },
                    { 29, "Cabo Verde" },
                    { 30, "Cambodia" },
                    { 31, "Cameroon" },
                    { 32, "Canada" },
                    { 33, "Central African Republic" },
                    { 34, "Chad" },
                    { 35, "Chile" },
                    { 36, "China" },
                    { 37, "Colombia" },
                    { 38, "Comoros" },
                    { 39, "Congo (Congo-Brazzaville)" },
                    { 40, "Costa Rica" },
                    { 41, "Croatia" },
                    { 42, "Cuba" },
                    { 43, "Cyprus" },
                    { 44, "Czechia (Czech Republic)" },
                    { 45, "Democratic Republic of the Congo" },
                    { 46, "Denmark" },
                    { 47, "Djibouti" },
                    { 48, "Dominica" },
                    { 49, "Dominican Republic" },
                    { 50, "Ecuador" },
                    { 51, "Egypt" },
                    { 52, "El Salvador" },
                    { 53, "Equatorial Guinea" },
                    { 54, "Eritrea" },
                    { 55, "Estonia" },
                    { 56, "Eswatini (fmr. \"Swaziland\")" },
                    { 57, "Ethiopia" },
                    { 58, "Fiji" },
                    { 59, "Finland" },
                    { 60, "France" },
                    { 61, "Gabon" },
                    { 62, "Gambia" },
                    { 63, "Georgia" },
                    { 64, "Germany" },
                    { 65, "Ghana" },
                    { 66, "Greece" },
                    { 67, "Grenada" },
                    { 68, "Guatemala" },
                    { 69, "Guinea" },
                    { 70, "Guinea-Bissau" },
                    { 71, "Guyana" },
                    { 72, "Haiti" },
                    { 73, "Holy See" },
                    { 74, "Honduras" },
                    { 75, "Hungary" },
                    { 76, "Iceland" },
                    { 77, "India" },
                    { 78, "Indonesia" },
                    { 79, "Iran" },
                    { 80, "Iraq" },
                    { 81, "Ireland" },
                    { 82, "Italy" },
                    { 83, "Ivory Coast" },
                    { 84, "Jamaica" },
                    { 85, "Japan" },
                    { 86, "Jordan" },
                    { 87, "Kazakhstan" },
                    { 88, "Kenya" },
                    { 89, "Kiribati" },
                    { 90, "Kuwait" },
                    { 91, "Kyrgyzstan" },
                    { 92, "Laos" },
                    { 93, "Latvia" },
                    { 94, "Lebanon" },
                    { 95, "Lesotho" },
                    { 96, "Liberia" },
                    { 97, "Libya" },
                    { 98, "Liechtenstein" },
                    { 99, "Lithuania" },
                    { 100, "Luxembourg" },
                    { 101, "Madagascar" },
                    { 102, "Malawi" },
                    { 103, "Malaysia" },
                    { 104, "Maldives" },
                    { 105, "Mali" },
                    { 106, "Malta" },
                    { 107, "Marshall Islands" },
                    { 108, "Mauritania" },
                    { 109, "Mauritius" },
                    { 110, "Mexico" },
                    { 111, "Micronesia" },
                    { 112, "Moldova" },
                    { 113, "Monaco" },
                    { 114, "Mongolia" },
                    { 115, "Montenegro" },
                    { 116, "Morocco" },
                    { 117, "Mozambique" },
                    { 118, "Namibia" },
                    { 119, "Nauru" },
                    { 120, "Nepal" },
                    { 121, "Netherlands" },
                    { 122, "New Zealand" },
                    { 123, "Nicaragua" },
                    { 124, "Niger" },
                    { 125, "Nigeria" },
                    { 126, "North Macedonia" },
                    { 127, "Norway" },
                    { 128, "Oman" },
                    { 129, "Pakistan" },
                    { 130, "Palau" },
                    { 131, "Palestine State" },
                    { 132, "Panama" },
                    { 133, "Papua New Guinea" },
                    { 134, "Paraguay" },
                    { 135, "Peru" },
                    { 136, "Philippines" },
                    { 137, "Poland" },
                    { 138, "Portugal" },
                    { 139, "Qatar" },
                    { 140, "Romania" },
                    { 141, "Russia" },
                    { 142, "Rwanda" },
                    { 143, "Saint Kitts and Nevis" },
                    { 144, "Saint Lucia" },
                    { 145, "Saint Vincent and the Grenadines" },
                    { 146, "Samoa" },
                    { 147, "San Marino" },
                    { 148, "Sao Tome and Principe" },
                    { 149, "Saudi Arabia" },
                    { 150, "Senegal" },
                    { 151, "Serbia" },
                    { 152, "Seychelles" },
                    { 153, "Sierra Leone" },
                    { 154, "Singapore" },
                    { 155, "Slovakia" },
                    { 156, "Slovenia" },
                    { 157, "Solomon Islands" },
                    { 158, "Somalia" },
                    { 159, "South Africa" },
                    { 160, "South Sudan" },
                    { 161, "Spain" },
                    { 162, "Sri Lanka" },
                    { 163, "Sudan" },
                    { 164, "Suriname" },
                    { 165, "Sweden" },
                    { 166, "Switzerland" },
                    { 167, "Syria" },
                    { 168, "Tajikistan" },
                    { 169, "Tanzania" },
                    { 170, "Thailand" },
                    { 171, "Timor-Leste" },
                    { 172, "Togo" },
                    { 173, "Tonga" },
                    { 174, "Trinidad and Tobago" },
                    { 175, "Tunisia" },
                    { 176, "Turkey" },
                    { 177, "Turkmenistan" },
                    { 178, "Tuvalu" },
                    { 179, "Uganda" },
                    { 180, "Ukraine" },
                    { 181, "United Arab Emirates" },
                    { 182, "United Kingdom" },
                    { 183, "United States of America" },
                    { 184, "Uruguay" },
                    { 185, "Uzbekistan" },
                    { 186, "Vanuatu" },
                    { 187, "Venezuela" },
                    { 188, "Vietnam" },
                    { 189, "Yemen" },
                    { 190, "Zambia" },
                    { 191, "Zimbabwe" }
                });

            migrationBuilder.InsertData(
                table: "LicenseClasses",
                columns: new[] { "LicenseClassID", "ClassDescription", "ClassFees", "ClassName", "DefaultValidityLength", "MinimumAllowedAge" },
                values: new object[,]
                {
                    { 1, "Small motorcycles with engine capacity less than 125cc.", 15m, "Class 1 - Small Motorcycle", (byte)5, (byte)16 },
                    { 2, "Motorcycles with engine capacity more than 125cc.", 30m, "Class 2 - Heavy Motorcycle", (byte)5, (byte)18 },
                    { 3, "Standard cars and small pickups. (Most Common)", 20m, "Class 3 - Ordinary driving license", (byte)10, (byte)18 },
                    { 4, "Vehicles used for commercial purposes like Taxis and small buses.", 200m, "Class 4 - Commercial", (byte)10, (byte)21 },
                    { 5, "Agricultural tractors and specialized machinery.", 50m, "Class 5 - Agricultural", (byte)10, (byte)18 },
                    { 6, "Trucks with total weight between 3.5 and 7.5 tons.", 250m, "Class 6 - Small and Medium Truck", (byte)10, (byte)21 },
                    { 7, "Large trucks and trailers with weight exceeding 7.5 tons.", 300m, "Class 7 - Heavy Truck", (byte)10, (byte)21 }
                });

            migrationBuilder.InsertData(
                table: "TestTypes",
                columns: new[] { "TestTypeID", "Description", "Fees", "Title" },
                values: new object[,]
                {
                    { 1, "Eye vision examination", 10m, "Vision Test" },
                    { 2, "Theoretical driving rules test", 20m, "Written Test" },
                    { 3, "Practical driving test on the road", 30m, "Street Test" }
                });

            migrationBuilder.InsertData(
                table: "People",
                columns: new[] { "PersonID", "Address", "DateOfBirth", "Email", "FirstName", "Gendor", "ImagePath", "LastName", "NationalNo", "NationalityCountryID", "Phone", "SecondName", "ThirdName" },
                values: new object[,]
                {
                    { 1, "123 Amman St, Al-Abdali District", new DateTime(2005, 8, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "jone@example.com", "jone", (byte)0, null, "", "N2746744", 80, "0791234567", "max", "alax" },
                    { 2, "123 Amman St, Al-Abdali District", new DateTime(1998, 8, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "ben@example.com", "ben", (byte)0, "e0acd4bc-4e9a-4ef2-b345-fb90bd39ba84.avif", "", "N253234", 80, "0791234567", "park", "jake" },
                    { 3, "123 Amman St, Al-Abdali District", new DateTime(2005, 8, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "Better@example.com", "Better", (byte)0, "ddeb3e90-c473-41ad-bdaf-519fb194cb98.avif", "", "N2742234", 80, "0791234567", "Make", "Maksuel" },
                    { 4, "123 Amman St, Al-Abdali District", new DateTime(1998, 8, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "Alxander@example.com", "Alxander", (byte)0, "425d43ef-7226-459a-8556-0e71bbf9633d.avif", "met", "N1224437", 80, "0791234567", "Karter", "max" },
                    { 5, "123 Amman St, Al-Abdali District", new DateTime(1998, 8, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "jonson@example.com", "jonson", (byte)0, "c76dd4de-31c9-4b22-b7d5-521212046c51.avif", "ben", "N1224227", 80, "0791234567", "wllet", "marker" },
                    { 6, "123 Amman St, Al-Abdali District", new DateTime(2005, 8, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "Adim@example.com", "Adim", (byte)0, null, "", "N2586744", 80, "0791234567", "Elson", "max" },
                    { 7, "123 Amman St, Al-Abdali District", new DateTime(2005, 8, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "Rafile@example.com", "Rafile", (byte)0, null, "", "N274264", 80, "0791234567", "gurge", "carter" }
                });

            migrationBuilder.InsertData(
                table: "Drivers",
                columns: new[] { "DriverID", "CreatedByUserID", "CreatedDate", "PersonID" },
                values: new object[,]
                {
                    { 1, 2, new DateTime(2026, 2, 1, 10, 30, 0, 0, DateTimeKind.Unspecified), 1 },
                    { 2, 3, new DateTime(2026, 2, 1, 10, 30, 0, 0, DateTimeKind.Unspecified), 2 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Applications_ApplicantPersonID",
                table: "Applications",
                column: "ApplicantPersonID");

            migrationBuilder.CreateIndex(
                name: "IX_Applications_ApplicationTypeID",
                table: "Applications",
                column: "ApplicationTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_Applications_CreatedByUserID",
                table: "Applications",
                column: "CreatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationTypes_Title",
                table: "ApplicationTypes",
                column: "Title",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DetainedLicenses_CreatedByUserID",
                table: "DetainedLicenses",
                column: "CreatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_DetainedLicenses_LicenseID",
                table: "DetainedLicenses",
                column: "LicenseID");

            migrationBuilder.CreateIndex(
                name: "IX_DetainedLicenses_ReleaseApplicationID",
                table: "DetainedLicenses",
                column: "ReleaseApplicationID");

            migrationBuilder.CreateIndex(
                name: "IX_DetainedLicenses_ReleasedByUserID",
                table: "DetainedLicenses",
                column: "ReleasedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_Drivers_PersonID",
                table: "Drivers",
                column: "PersonID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InternationalLicenses_ApplicationID",
                table: "InternationalLicenses",
                column: "ApplicationID");

            migrationBuilder.CreateIndex(
                name: "IX_InternationalLicenses_CreatedByUserID",
                table: "InternationalLicenses",
                column: "CreatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_InternationalLicenses_DriverID",
                table: "InternationalLicenses",
                column: "DriverID");

            migrationBuilder.CreateIndex(
                name: "IX_InternationalLicenses_IssuedUsingLocalLicenseID",
                table: "InternationalLicenses",
                column: "IssuedUsingLocalLicenseID");

            migrationBuilder.CreateIndex(
                name: "IX_LicenseClasses_ClassName",
                table: "LicenseClasses",
                column: "ClassName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Licenses_ApplicationID",
                table: "Licenses",
                column: "ApplicationID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Licenses_DriverID",
                table: "Licenses",
                column: "DriverID");

            migrationBuilder.CreateIndex(
                name: "IX_Licenses_LicenseClassID",
                table: "Licenses",
                column: "LicenseClassID");

            migrationBuilder.CreateIndex(
                name: "IX_LocalDrivingLicenseApplications_ApplicationID",
                table: "LocalDrivingLicenseApplications",
                column: "ApplicationID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LocalDrivingLicenseApplications_LicenseClassID",
                table: "LocalDrivingLicenseApplications",
                column: "LicenseClassID");

            migrationBuilder.CreateIndex(
                name: "IX_People_NationalityCountryID",
                table: "People",
                column: "NationalityCountryID");

            migrationBuilder.CreateIndex(
                name: "IX_People_NationalNo",
                table: "People",
                column: "NationalNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TestAppointments_CreatedByUserID",
                table: "TestAppointments",
                column: "CreatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_TestAppointments_LocalDrivingLicenseApplicationID",
                table: "TestAppointments",
                column: "LocalDrivingLicenseApplicationID");

            migrationBuilder.CreateIndex(
                name: "IX_TestAppointments_RetakeTestApplicationID",
                table: "TestAppointments",
                column: "RetakeTestApplicationID");

            migrationBuilder.CreateIndex(
                name: "IX_Tests_TestAppointmentID",
                table: "Tests",
                column: "TestAppointmentID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TestTypes_Title",
                table: "TestTypes",
                column: "Title",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_PersonID",
                table: "Users",
                column: "PersonID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_UserName",
                table: "Users",
                column: "UserName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DetainedLicenses");

            migrationBuilder.DropTable(
                name: "InternationalLicenses");

            migrationBuilder.DropTable(
                name: "Tests");

            migrationBuilder.DropTable(
                name: "TestTypes");

            migrationBuilder.DropTable(
                name: "Licenses");

            migrationBuilder.DropTable(
                name: "TestAppointments");

            migrationBuilder.DropTable(
                name: "Drivers");

            migrationBuilder.DropTable(
                name: "LocalDrivingLicenseApplications");

            migrationBuilder.DropTable(
                name: "Applications");

            migrationBuilder.DropTable(
                name: "LicenseClasses");

            migrationBuilder.DropTable(
                name: "ApplicationTypes");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "People");

            migrationBuilder.DropTable(
                name: "Countries");
        }
    }
}
