using System;
using System.Runtime.CompilerServices;

namespace Andy.TUI.Diagnostics;

/// <summary>
/// Extension methods for enhanced logging capabilities.
/// </summary>
public static class LoggingExtensions
{
    /// <summary>
    /// Logs method entry with parameters.
    /// </summary>
    public static void LogMethodEntry(
        this ILogger logger,
        object? param1 = null,
        object? param2 = null,
        object? param3 = null,
        [CallerMemberName] string? methodName = null)
    {
        if (param1 == null)
        {
            logger.Debug($"→ {methodName}()");
        }
        else if (param2 == null)
        {
            logger.Debug($"→ {methodName}({param1})");
        }
        else if (param3 == null)
        {
            logger.Debug($"→ {methodName}({param1}, {param2})");
        }
        else
        {
            logger.Debug($"→ {methodName}({param1}, {param2}, {param3})");
        }
    }
    
    /// <summary>
    /// Logs method exit with optional return value.
    /// </summary>
    public static T LogMethodExit<T>(
        this ILogger logger,
        T returnValue,
        [CallerMemberName] string? methodName = null)
    {
        logger.Debug($"← {methodName} = {returnValue}");
        return returnValue;
    }
    
    /// <summary>
    /// Logs method exit without return value.
    /// </summary>
    public static void LogMethodExit(
        this ILogger logger,
        [CallerMemberName] string? methodName = null)
    {
        logger.Debug($"← {methodName}");
    }
    
    /// <summary>
    /// Logs a state change.
    /// </summary>
    public static void LogStateChange(
        this ILogger logger,
        string property,
        object? oldValue,
        object? newValue)
    {
        logger.Debug($"State: {property} changed from '{oldValue}' to '{newValue}'");
    }
    
    /// <summary>
    /// Logs an event occurrence.
    /// </summary>
    public static void LogEvent(
        this ILogger logger,
        string eventName,
        object? data = null)
    {
        if (data != null)
        {
            logger.Debug($"Event: {eventName} → {data}");
        }
        else
        {
            logger.Debug($"Event: {eventName}");
        }
    }
    
    /// <summary>
    /// Logs a key input event.
    /// </summary>
    public static void LogKeyInput(
        this ILogger logger,
        ConsoleKeyInfo key,
        string? context = null)
    {
        var modifiers = "";
        if (key.Modifiers.HasFlag(ConsoleModifiers.Control)) modifiers += "Ctrl+";
        if (key.Modifiers.HasFlag(ConsoleModifiers.Alt)) modifiers += "Alt+";
        if (key.Modifiers.HasFlag(ConsoleModifiers.Shift)) modifiers += "Shift+";
        
        var keyStr = $"{modifiers}{key.Key}";
        if (key.KeyChar != '\0' && !char.IsControl(key.KeyChar))
        {
            keyStr += $" ('{key.KeyChar}')";
        }
        
        if (context != null)
        {
            logger.Debug($"KeyInput: {keyStr} @ {context}");
        }
        else
        {
            logger.Debug($"KeyInput: {keyStr}");
        }
    }
    
    /// <summary>
    /// Logs a focus change.
    /// </summary>
    public static void LogFocusChange(
        this ILogger logger,
        string? oldFocus,
        string? newFocus,
        string? reason = null)
    {
        if (reason != null)
        {
            logger.Debug($"Focus: '{oldFocus}' → '{newFocus}' (reason: {reason})");
        }
        else
        {
            logger.Debug($"Focus: '{oldFocus}' → '{newFocus}'");
        }
    }
    
    /// <summary>
    /// Logs a render operation.
    /// </summary>
    public static void LogRender(
        this ILogger logger,
        string component,
        int? dirtyRegions = null,
        long? elapsedMs = null)
    {
        var details = new System.Collections.Generic.List<string>();
        if (dirtyRegions.HasValue)
            details.Add($"regions={dirtyRegions}");
        if (elapsedMs.HasValue)
            details.Add($"time={elapsedMs}ms");
            
        if (details.Count > 0)
        {
            logger.Debug($"Render: {component} [{string.Join(", ", details)}]");
        }
        else
        {
            logger.Debug($"Render: {component}");
        }
    }
    
    /// <summary>
    /// Logs a tree diff operation.
    /// </summary>
    public static void LogTreeDiff(
        this ILogger logger,
        int additions,
        int deletions,
        int updates,
        long? elapsedMs = null)
    {
        var msg = $"TreeDiff: +{additions} -{deletions} ~{updates}";
        if (elapsedMs.HasValue)
        {
            msg += $" ({elapsedMs}ms)";
        }
        logger.Debug(msg);
    }
    
    /// <summary>
    /// Logs buffer invalidation.
    /// </summary>
    public static void LogInvalidation(
        this ILogger logger,
        string reason,
        int x,
        int y,
        int width,
        int height)
    {
        logger.Debug($"Invalidate: {reason} @ ({x},{y}) {width}x{height}");
    }
}