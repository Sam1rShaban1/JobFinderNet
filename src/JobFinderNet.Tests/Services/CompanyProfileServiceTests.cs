using Moq;
using JobFinderNet.Core.DTOs;
using JobFinderNet.Core.Models;
using JobFinderNet.Core.Interfaces.Repositories;
using JobFinderNet.Infrastructure.Services;
using JobFinderNet.Tests.Helpers;

namespace JobFinderNet.Tests.Services;

public class CompanyProfileServiceTests
{
    private readonly Mock<ICompanyProfileRepository> _mockRepo;
    private readonly CompanyProfileService _service;

    public CompanyProfileServiceTests()
    {
        _mockRepo = new Mock<ICompanyProfileRepository>();
        _service = new CompanyProfileService(_mockRepo.Object);
    }

    [Fact]
    public async Task GetByIdAsync_Existing_ReturnsDto()
    {
        var profile = new CompanyProfile
        {
            Id = 1, Name = "Acme", LogoUrl = "/logo.png", Description = "Desc",
            Website = "https://acme.com", Size = "50-100", Industry = "Tech",
            IsVerified = true, Jobs = new List<Job> { TestDbContextFactory.CreateTestJob(1, "Job1", "emp1") }
        };
        _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(profile);

        var result = await _service.GetByIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal("Acme", result!.Name);
        Assert.Equal(1, result.OpenRoles);
    }

    [Fact]
    public async Task GetByIdAsync_NotFound_ReturnsNull()
    {
        _mockRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((CompanyProfile?)null);

        var result = await _service.GetByIdAsync(99);

        Assert.Null(result);
    }

    [Fact]
    public async Task SearchAsync_ReturnsMappedResults()
    {
        var companies = new List<CompanyProfile>
        {
            new() { Id = 1, Name = "Acme", Industry = "Tech", Jobs = new List<Job>() }
        };
        _mockRepo.Setup(r => r.SearchAsync("acme", 20)).ReturnsAsync(companies);

        var result = await _service.SearchAsync("acme");

        Assert.Single(result);
        Assert.Equal("Acme", result[0].Name);
    }

    [Fact]
    public async Task GetMyCompanyAsync_ReturnsCompanyForUser()
    {
        var profile = new CompanyProfile { Id = 1, Name = "Acme", ClaimedByUserId = "u1" };
        _mockRepo.Setup(r => r.GetByClaimedUserIdAsync("u1")).ReturnsAsync(profile);

        var result = await _service.GetMyCompanyAsync("u1");

        Assert.NotNull(result);
        Assert.Equal("Acme", result!.Name);
    }

    [Fact]
    public async Task GetMyCompanyAsync_NoCompany_ReturnsNull()
    {
        _mockRepo.Setup(r => r.GetByClaimedUserIdAsync("u1")).ReturnsAsync((CompanyProfile?)null);

        var result = await _service.GetMyCompanyAsync("u1");

        Assert.Null(result);
    }

    [Fact]
    public async Task ClaimCompanyAsync_NewCompany_CreatesAndClaims()
    {
        _mockRepo.Setup(r => r.GetByNameAsync("NewCo")).ReturnsAsync((CompanyProfile?)null);
        _mockRepo.Setup(r => r.AddAsync(It.IsAny<CompanyProfile>())).Returns(Task.CompletedTask);
        _mockRepo.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var dto = new CreateCompanyProfileDto { Name = "NewCo", Industry = "Tech" };

        var result = await _service.ClaimCompanyAsync("u1", dto);

        Assert.Equal("NewCo", result.Name);
        Assert.Equal("u1", result.ClaimedByUserId);
        Assert.False(result.IsVerified);
    }

    [Fact]
    public async Task ClaimCompanyAsync_ExistingUnclaimed_ClaimsIt()
    {
        var existing = new CompanyProfile { Id = 1, Name = "Existing", ClaimedByUserId = null };
        _mockRepo.Setup(r => r.GetByNameAsync("Existing")).ReturnsAsync(existing);
        _mockRepo.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var dto = new CreateCompanyProfileDto { Name = "Existing" };

        var result = await _service.ClaimCompanyAsync("u1", dto);

        Assert.Equal("u1", result.ClaimedByUserId);
    }

    [Fact]
    public async Task ClaimCompanyAsync_AlreadyClaimed_ThrowsInvalidOperation()
    {
        var existing = new CompanyProfile { Id = 1, Name = "Claimed", ClaimedByUserId = "other" };
        _mockRepo.Setup(r => r.GetByNameAsync("Claimed")).ReturnsAsync(existing);

        var dto = new CreateCompanyProfileDto { Name = "Claimed" };

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.ClaimCompanyAsync("u1", dto));
    }

    [Fact]
    public async Task UpdateCompanyAsync_Owner_UpdatesSuccessfully()
    {
        var company = new CompanyProfile { Id = 1, ClaimedByUserId = "u1", Name = "Old" };
        _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(company);
        _mockRepo.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var dto = new CreateCompanyProfileDto { Name = "New", Description = "Updated" };

        var result = await _service.UpdateCompanyAsync(1, "u1", dto);

        Assert.NotNull(result);
        Assert.Equal("Updated", result!.Description);
    }

    [Fact]
    public async Task UpdateCompanyAsync_NotOwner_ThrowsUnauthorized()
    {
        var company = new CompanyProfile { Id = 1, ClaimedByUserId = "other", Name = "Old" };
        _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(company);

        var dto = new CreateCompanyProfileDto { Name = "New" };

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.UpdateCompanyAsync(1, "u1", dto));
    }

    [Fact]
    public async Task UpdateCompanyAsync_NotFound_ReturnsNull()
    {
        _mockRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((CompanyProfile?)null);

        var result = await _service.UpdateCompanyAsync(99, "u1", new CreateCompanyProfileDto { Name = "X" });

        Assert.Null(result);
    }
}
