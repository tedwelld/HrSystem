using System.ComponentModel.DataAnnotations;
using HrSystem.Data;
using HrSystem.Data.EntityModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Api.Controllers;

public class CreateCompanyDto
{
    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(250)]
    public string Address { get; set; } = string.Empty;

    [MaxLength(100)]
    public string City { get; set; } = string.Empty;

    [MaxLength(100)]
    public string Country { get; set; } = string.Empty;

    [MaxLength(30)]
    public string Phone { get; set; } = string.Empty;

    [MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;
}

[ApiController]
[Route("api/companies")]
[Authorize]
public class CompaniesController(HrSystemDbContext dbContext) : ControllerBase
{
    private readonly HrSystemDbContext _dbContext = dbContext;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var items = await _dbContext.Companies
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new
            {
                x.Id,
                x.Name,
                x.Address,
                x.City,
                x.Country,
                x.Phone,
                x.Email,
                x.Description
            })
            .ToListAsync();

        return Ok(items);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateCompanyDto dto)
    {
        var company = new Company
        {
            Name = dto.Name.Trim(),
            Address = dto.Address.Trim(),
            City = dto.City.Trim(),
            Country = dto.Country.Trim(),
            Phone = dto.Phone.Trim(),
            Email = dto.Email.Trim(),
            Description = dto.Description.Trim(),
            CreatedAtUtc = DateTime.UtcNow
        };

        _dbContext.Companies.Add(company);
        await _dbContext.SaveChangesAsync();

        return Ok(company);
    }
}
