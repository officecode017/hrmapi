using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using HRAttendance.Business.Interfaces.Payroll.Statutory;
using HRAttendance.Data;
using HRAttendance.Data.Models.Payroll;

namespace HRAttendance.Business.Services.Payroll.Statutory;

public class StatutoryRuleProvider : IStatutoryRuleProvider
{
    private readonly ApplicationDbContext _context;

    public StatutoryRuleProvider(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<StatutoryRule?> GetActiveRuleAsync(int organizationId, StatutoryRuleType type, DateOnly date)
    {
        return await _context.StatutoryRules
            .Where(r => r.OrganizationId == organizationId 
                     && r.RuleType == type 
                     && r.IsActive
                     && r.EffectiveFrom <= date
                     && (r.EffectiveTo == null || r.EffectiveTo.Value >= date))
            .OrderByDescending(r => r.Version)
            .FirstOrDefaultAsync();
    }

    public async Task<PFConfig> GetPFConfigAsync(int organizationId, DateOnly date)
    {
        var rule = await GetActiveRuleAsync(organizationId, StatutoryRuleType.ProvidentFund, date);
        if (rule != null && !string.IsNullOrWhiteSpace(rule.ConfigurationJson) && rule.ConfigurationJson != "{}")
        {
            try
            {
                var config = JsonSerializer.Deserialize<PFConfig>(rule.ConfigurationJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (config != null) return config;
            }
            catch { }
        }
        return new PFConfig(); // Standard Indian Statutory defaults
    }

    public async Task<ESIConfig> GetESIConfigAsync(int organizationId, DateOnly date)
    {
        var rule = await GetActiveRuleAsync(organizationId, StatutoryRuleType.ESI, date);
        if (rule != null && !string.IsNullOrWhiteSpace(rule.ConfigurationJson) && rule.ConfigurationJson != "{}")
        {
            try
            {
                var config = JsonSerializer.Deserialize<ESIConfig>(rule.ConfigurationJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (config != null) return config;
            }
            catch { }
        }
        return new ESIConfig(); // Standard Indian Statutory defaults
    }

    public async Task<PTConfig> GetPTConfigAsync(int organizationId, string stateCode, DateOnly date)
    {
        var rule = await GetActiveRuleAsync(organizationId, StatutoryRuleType.ProfessionalTax, date);
        if (rule != null && !string.IsNullOrWhiteSpace(rule.ConfigurationJson) && rule.ConfigurationJson != "{}")
        {
            try
            {
                var config = JsonSerializer.Deserialize<PTConfig>(rule.ConfigurationJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (config != null) return config;
            }
            catch { }
        }

        // Standard Maharashtra / Default State brackets if not overridden
        var defaultBrackets = new List<PTBracket>
        {
            new(0m, 7500m, 0m),
            new(7501m, 10000m, 175m),
            new(10001m, decimal.MaxValue, 200m, FebruaryTax: 300m)
        };

        return new PTConfig(stateCode.ToUpperInvariant(), defaultBrackets);
    }

    public async Task<TDSConfig> GetTDSConfigAsync(int organizationId, DateOnly date)
    {
        var rule = await GetActiveRuleAsync(organizationId, StatutoryRuleType.TDS, date);
        if (rule != null && !string.IsNullOrWhiteSpace(rule.ConfigurationJson) && rule.ConfigurationJson != "{}")
        {
            try
            {
                var config = JsonSerializer.Deserialize<TDSConfig>(rule.ConfigurationJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (config != null) return config;
            }
            catch { }
        }
        return new TDSConfig();
    }
}
