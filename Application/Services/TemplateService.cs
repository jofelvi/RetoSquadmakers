using Microsoft.Extensions.Logging;
using retoSquadmakers.Domain.Entities;
using retoSquadmakers.Domain.Repositories;
using retoSquadmakers.Domain.Services;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace retoSquadmakers.Application.Services;

public class TemplateService : ITemplateService
{
    private readonly INotificationTemplateRepository _templateRepository;
    private readonly ILogger<TemplateService> _logger;

    public TemplateService(INotificationTemplateRepository templateRepository, ILogger<TemplateService> logger)
    {
        _templateRepository = templateRepository;
        _logger = logger;
    }

    public async Task<string> RenderTemplateAsync(string templateId, Dictionary<string, object> data)
    {
        try
        {
            // Try to find template by ID and type (Email first, then others)
            var template = await _templateRepository.GetByTemplateIdAsync(templateId, "Email") ??
                          await _templateRepository.GetByTemplateIdAsync(templateId, "SMS") ??
                          await _templateRepository.GetByTemplateIdAsync(templateId, "Push");

            if (template == null)
            {
                _logger.LogWarning("Template not found: {TemplateId}", templateId);
                throw new InvalidOperationException($"Template '{templateId}' not found");
            }

            if (!template.IsActive)
            {
                _logger.LogWarning("Template is inactive: {TemplateId}", templateId);
                throw new InvalidOperationException($"Template '{templateId}' is inactive");
            }

            var renderedContent = ReplaceTemplateVariables(template.Content, data);
            
            _logger.LogDebug("Template rendered successfully: {TemplateId}", templateId);
            return renderedContent;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rendering template {TemplateId}: {Error}", templateId, ex.Message);
            throw;
        }
    }

    public async Task<NotificationTemplate?> GetTemplateAsync(string templateId, string type)
    {
        return await _templateRepository.GetByTemplateIdAsync(templateId, type);
    }

    public async Task<NotificationTemplate> CreateTemplateAsync(NotificationTemplate template)
    {
        // Validate template
        ValidateTemplate(template);

        // Check if template ID already exists
        var existingTemplate = await _templateRepository.GetByTemplateIdAsync(template.TemplateId, template.Type);
        if (existingTemplate != null)
        {
            throw new InvalidOperationException($"Template with ID '{template.TemplateId}' and type '{template.Type}' already exists");
        }

        template.CreatedAt = DateTime.UtcNow;
        return await _templateRepository.CreateAsync(template);
    }

    public async Task<NotificationTemplate> UpdateTemplateAsync(NotificationTemplate template)
    {
        ValidateTemplate(template);

        var existingTemplate = await _templateRepository.GetByTemplateIdAsync(template.TemplateId, template.Type);
        if (existingTemplate == null)
        {
            throw new InvalidOperationException($"Template with ID '{template.TemplateId}' and type '{template.Type}' not found");
        }

        existingTemplate.Name = template.Name;
        existingTemplate.Subject = template.Subject;
        existingTemplate.Content = template.Content;
        existingTemplate.IsActive = template.IsActive;
        existingTemplate.UpdatedAt = DateTime.UtcNow;

        return await _templateRepository.UpdateAsync(existingTemplate);
    }

    public async Task<bool> DeleteTemplateAsync(string templateId)
    {
        // Find template across all types
        var templates = await _templateRepository.GetAllAsync();
        var template = templates.FirstOrDefault(t => t.TemplateId == templateId);

        if (template == null)
            return false;

        await _templateRepository.DeleteAsync(template.Id);
        return true;
    }

    public async Task<IEnumerable<NotificationTemplate>> GetTemplatesAsync(string? type = null)
    {
        if (string.IsNullOrEmpty(type))
        {
            return await _templateRepository.GetAllAsync();
        }

        return await _templateRepository.GetByTypeAsync(type);
    }

    private static void ValidateTemplate(NotificationTemplate template)
    {
        if (string.IsNullOrWhiteSpace(template.TemplateId))
            throw new ArgumentException("Template ID is required");

        if (string.IsNullOrWhiteSpace(template.Name))
            throw new ArgumentException("Template name is required");

        if (string.IsNullOrWhiteSpace(template.Type))
            throw new ArgumentException("Template type is required");

        if (string.IsNullOrWhiteSpace(template.Content))
            throw new ArgumentException("Template content is required");

        var validTypes = new[] { "Email", "SMS", "Push" };
        if (!validTypes.Contains(template.Type))
            throw new ArgumentException($"Template type must be one of: {string.Join(", ", validTypes)}");

        // Validate template variables syntax
        ValidateTemplateVariables(template.Content);
    }

    private static void ValidateTemplateVariables(string content)
    {
        var regex = new Regex(@"\{\{(\w+)\}\}", RegexOptions.Compiled);
        var matches = regex.Matches(content);

        // Check for proper variable syntax
        foreach (Match match in matches)
        {
            var variableName = match.Groups[1].Value;
            if (string.IsNullOrWhiteSpace(variableName))
            {
                throw new ArgumentException($"Invalid template variable syntax: {match.Value}");
            }
        }
    }

    private static string ReplaceTemplateVariables(string template, Dictionary<string, object> data)
    {
        if (data == null || data.Count == 0)
            return template;

        var result = template;
        var regex = new Regex(@"\{\{(\w+)\}\}", RegexOptions.Compiled);

        result = regex.Replace(result, match =>
        {
            var variableName = match.Groups[1].Value;
            
            if (data.TryGetValue(variableName, out var value))
            {
                return value?.ToString() ?? string.Empty;
            }

            // Return the original placeholder if variable not found
            return match.Value;
        });

        return result;
    }
}