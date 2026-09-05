using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using HRAttendance.Business.Interfaces;
using HRAttendance.Business.Services;
using HRAttendance.Data.DTOs.Common;
using HRAttendance.Data.DTOs.Employee;
using HRAttendance.Data.Interfaces;
using HRAttendance.Data.Models.Employee;
using Xunit;

namespace HRAttendance.UnitTests.Services;

public class EmployeeServiceTests
{
    private readonly Mock<IEmployeeRepository> _mockRepo;
    private readonly Mock<IPasswordHasher> _mockHasher;
    private readonly Mock<IFileStorageService> _mockFileStorage;
    private readonly Mock<ILogger<EmployeeService>> _mockLogger;
    private readonly EmployeeService _service;

    public EmployeeServiceTests()
    {
        _mockRepo = new Mock<IEmployeeRepository>();
        _mockHasher = new Mock<IPasswordHasher>();
        _mockFileStorage = new Mock<IFileStorageService>();
        _mockLogger = new Mock<ILogger<EmployeeService>>();

        _service = new EmployeeService(_mockRepo.Object, _mockHasher.Object, _mockFileStorage.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetEmployeeByIdAsync_WhenEmployeeExists_ShouldReturnSuccess()
    {
        // Arrange
        var employeeId = 1;
        var employee = new Employee
        {
            Id = employeeId,
            OrganizationId = 1,
            EmployeeCode = "EMP001",
            FirstName = "John",
            LastName = "Doe",
            IsActive = true
        };

        _mockRepo.Setup(r => r.GetWithDetailsAsync(employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        // Act
        var result = await _service.GetEmployeeByIdAsync(employeeId);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.EmployeeCode.Should().Be("EMP001");
        result.Data.FullName.Should().Be("John Doe");
    }

    [Fact]
    public async Task GetEmployeeByIdAsync_WhenEmployeeDoesNotExist_ShouldReturnFail()
    {
        // Arrange
        _mockRepo.Setup(r => r.GetWithDetailsAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Employee?)null);

        // Act
        var result = await _service.GetEmployeeByIdAsync(999);

        // Assert
        result.Success.Should().BeFalse();
        result.Data.Should().BeNull();
        result.Message.Should().Contain("was not found");
    }
}
