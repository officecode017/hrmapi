using System.Text;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using HRAttendance.Business.Interfaces.Documents;
using HRAttendance.Business.Interfaces.Payroll;
using HRAttendance.Business.Services.Documents;
using HRAttendance.Data;
using HRAttendance.Data.Models.Document;
using HRAttendance.Data.Models.Employee;
using HRAttendance.Data.Models.Organization;
using HRAttendance.Data.Models.Payroll;

namespace HRAttendance.UnitTests.Documents;

public class EmployeeDocumentServiceTests
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<IAzureBlobStorageService> _blobStorageMock;
    private readonly Mock<IPayslipPdfGenerator> _pdfGeneratorMock;
    private readonly Mock<ILogger<EmployeeDocumentService>> _loggerMock;
    private readonly EmployeeDocumentService _service;

    public EmployeeDocumentServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _blobStorageMock = new Mock<IAzureBlobStorageService>();
        _pdfGeneratorMock = new Mock<IPayslipPdfGenerator>();
        _loggerMock = new Mock<ILogger<EmployeeDocumentService>>();

        _service = new EmployeeDocumentService(
            _context,
            _blobStorageMock.Object,
            _pdfGeneratorMock.Object,
            _loggerMock.Object
        );

        // Seed Organization and Employee
        _context.Organizations.Add(new Organization
        {
            Id = 1,
            Name = "Acme Corp"
        });

        _context.Employees.Add(new Employee
        {
            Id = 10,
            OrganizationId = 1,
            EmployeeCode = "EMP010",
            FirstName = "John",
            LastName = "Doe"
        });

        _context.SaveChanges();
    }

    [Fact]
    public async Task EnsureSystemFoldersExistAsync_CreatesFinanceSalarySlipsPersonalAndContractsFolders()
    {
        // Act
        await _service.EnsureSystemFoldersExistAsync(1, 10);

        // Assert
        var folders = await _context.EmployeeFolders.Where(f => f.EmployeeId == 10).ToListAsync();
        folders.Should().HaveCount(4);

        var finance = folders.FirstOrDefault(f => f.Name == "Finance & Payroll");
        finance.Should().NotBeNull();
        finance!.IsSystemFolder.Should().BeTrue();
        finance.FolderType.Should().Be(FolderType.Finance);

        var salarySlips = folders.FirstOrDefault(f => f.Name == "Salary Slips");
        salarySlips.Should().NotBeNull();
        salarySlips!.ParentFolderId.Should().Be(finance.Id);
        salarySlips.IsSystemFolder.Should().BeTrue();

        folders.Any(f => f.Name == "Personal & KYC" && f.IsSystemFolder).Should().BeTrue();
        folders.Any(f => f.Name == "Contracts & Agreements" && f.IsSystemFolder).Should().BeTrue();
    }

    [Fact]
    public async Task CreateFolderAsync_WithValidName_CreatesCustomFolder()
    {
        // Arrange
        await _service.EnsureSystemFoldersExistAsync(1, 10);

        // Act
        var folder = await _service.CreateFolderAsync(1, 10, null, "Medical Records", 1);

        // Assert
        folder.Should().NotBeNull();
        folder.Name.Should().Be("Medical Records");
        folder.IsSystemFolder.Should().BeFalse();
        folder.Path.Should().Be("/Medical Records");
    }

    [Fact]
    public async Task CreateFolderAsync_WithDuplicateName_ThrowsInvalidOperationException()
    {
        // Arrange
        await _service.EnsureSystemFoldersExistAsync(1, 10);
        await _service.CreateFolderAsync(1, 10, null, "Certificates", 1);

        // Act & Assert
        var act = async () => await _service.CreateFolderAsync(1, 10, null, "Certificates", 1);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already exists*");
    }

    [Fact]
    public async Task RenameFolderAsync_SystemFolder_ThrowsInvalidOperationException()
    {
        // Arrange
        await _service.EnsureSystemFoldersExistAsync(1, 10);
        var finance = await _context.EmployeeFolders.FirstAsync(f => f.Name == "Finance & Payroll");

        // Act & Assert
        var act = async () => await _service.RenameFolderAsync(1, finance.Id, "Finance Renamed", 1);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*System folders cannot be renamed*");
    }

    [Fact]
    public async Task DeleteFolderAsync_SystemFolder_ThrowsInvalidOperationException()
    {
        // Arrange
        await _service.EnsureSystemFoldersExistAsync(1, 10);
        var salarySlips = await _context.EmployeeFolders.FirstAsync(f => f.Name == "Salary Slips");

        // Act & Assert
        var act = async () => await _service.DeleteFolderAsync(1, salarySlips.Id, 1);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*System default folders cannot be deleted*");
    }

    [Fact]
    public async Task UploadDocumentAsync_UploadsBlobAndCreatesRecord()
    {
        // Arrange
        await _service.EnsureSystemFoldersExistAsync(1, 10);
        var personal = await _context.EmployeeFolders.FirstAsync(f => f.Name == "Personal & KYC");

        _blobStorageMock.Setup(b => b.UploadBlobAsync(
            It.IsAny<string>(), 
            It.IsAny<Stream>(), 
            It.IsAny<string>(), 
            It.IsAny<CancellationToken>()))
            .ReturnsAsync("https://mockstorage.blob.core.windows.net/container/sample.pdf");

        var fileBytes = Encoding.UTF8.GetBytes("Dummy file content");
        using var stream = new MemoryStream(fileBytes);

        // Act
        var doc = await _service.UploadDocumentAsync(
            organizationId: 1,
            employeeId: 10,
            folderId: personal.Id,
            title: "Passport Copy",
            fileName: "passport_scan.pdf",
            contentType: "application/pdf",
            fileStream: stream,
            uploadedByUserId: 1
        );

        // Assert
        doc.Should().NotBeNull();
        doc.Title.Should().Be("Passport Copy");
        doc.OriginalFileName.Should().Be("passport_scan.pdf");
        doc.FileSizeBytes.Should().Be(fileBytes.Length);
        doc.IsSystemLocked.Should().BeFalse();

        _blobStorageMock.Verify(b => b.UploadBlobAsync(
            It.Is<string>(p => p.Contains("org-1/emp-10")),
            It.IsAny<Stream>(),
            "application/pdf",
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ArchivePublishedPayslipAsync_RendersPdfAndSavesToSalarySlipsFolder()
    {
        // Arrange
        var period = new PayrollPeriod
        {
            Id = 5,
            OrganizationId = 1,
            Year = 2026,
            Month = 9,
            Description = "September 2026",
            RunType = PayrollRunType.Regular
        };
        _context.PayrollPeriods.Add(period);

        var payrollEmp = new PayrollEmployee
        {
            Id = 50,
            OrganizationId = 1,
            EmployeeId = 10,
            PayrollPeriodId = 5,
            Period = period
        };
        _context.PayrollEmployees.Add(payrollEmp);

        var payslip = new Payslip
        {
            Id = 100,
            EmployeeId = 10,
            PayrollEmployeeId = 50,
            PayrollEmployee = payrollEmp,
            PayslipNumber = "PS-202609-0010",
            Status = PayslipStatus.Published
        };
        _context.Payslips.Add(payslip);
        await _context.SaveChangesAsync();

        var dummyPdf = Encoding.UTF8.GetBytes("%PDF-1.4 Mock Payslip PDF Bytes");
        _pdfGeneratorMock.Setup(p => p.GeneratePayslipPdfAsync(100, It.IsAny<CancellationToken>()))
            .ReturnsAsync(dummyPdf);

        _blobStorageMock.Setup(b => b.UploadBlobAsync(
            It.IsAny<string>(), 
            It.IsAny<Stream>(), 
            It.IsAny<string>(), 
            It.IsAny<CancellationToken>()))
            .ReturnsAsync("https://mockblob/payslip.pdf");

        // Act
        var result = await _service.ArchivePublishedPayslipAsync(100);

        // Assert
        result.Should().BeTrue();

        var salarySlipsFolder = await _context.EmployeeFolders.FirstAsync(f => f.EmployeeId == 10 && f.Name == "Salary Slips");
        var archivedDoc = await _context.EmployeeDocuments.FirstOrDefaultAsync(d => d.FolderId == salarySlipsFolder.Id && d.SourceType == DocumentSourceType.Payslip);

        archivedDoc.Should().NotBeNull();
        archivedDoc!.RelatedEntityId.Should().Be(100);
        archivedDoc.IsSystemLocked.Should().BeTrue();
        archivedDoc.Title.Should().Contain("September 2026");
        archivedDoc.OriginalFileName.Should().Contain("PS-202609-0010");

        // And verify attempting to delete locked payslip throws
        var deleteAct = async () => await _service.DeleteDocumentAsync(1, archivedDoc.Id, 1);
        await deleteAct.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*System-generated documents*cannot be deleted*");
    }
}
