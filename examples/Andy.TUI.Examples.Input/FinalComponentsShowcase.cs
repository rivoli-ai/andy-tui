using System;
using Andy.TUI.Declarative;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Declarative.Extensions;
using Andy.TUI.Declarative.Layout;
using Andy.TUI.Declarative.State;
using Andy.TUI.Terminal;

namespace Andy.TUI.Examples.Input;

/// <summary>
/// Showcase for the final four components: Gradient, BigText, Slider, and Badge.
/// </summary>
class FinalComponentsShowcaseApp
{
    // State
    private float _volumeLevel = 50f;
    private float _brightnessLevel = 75f;
    private int _notificationCount = 3;
    private bool _isOnline = true;
    private Binding<float> _volumeBinding;
    private Binding<float> _brightnessBinding;
    
    public FinalComponentsShowcaseApp()
    {
        _volumeBinding = new Binding<float>(
            () => _volumeLevel,
            value => _volumeLevel = value
        );
        
        _brightnessBinding = new Binding<float>(
            () => _brightnessLevel,
            value => _brightnessLevel = value
        );
    }
    
    public ISimpleComponent CreateUI()
    {
        return new VStack(spacing: 1) {
            // Title with BigText
            new BigText("FINAL", BigTextFont.Block, color: Color.Cyan),
            new BigText("SHOWCASE", BigTextFont.Slim, color: Color.Magenta),
            
            new Text(new string('─', 40)).Color(Color.DarkGray),
            
            // Gradient text examples
            new VStack(spacing: 1) {
                new Text("Gradient Examples:").Bold().Color(Color.White),
                new Gradient("Horizontal Gradient Text", Color.Red, Color.Blue),
                new Gradient("Vertical\nGradient\nText", Color.Green, Color.Yellow, GradientDirection.Vertical),
                new Gradient("Diagonal Gradient Effect", Color.Magenta, Color.Cyan, GradientDirection.Diagonal, bold: true)
            },
            
            new Text(new string('─', 40)).Color(Color.DarkGray),
            
            // Slider examples
            new VStack(spacing: 1) {
                new Text("Volume Control:").Bold().Color(Color.White),
                new Slider(_volumeBinding, 0f, 100f, 5f, label: "Volume", thumbColor: Color.Green),
                
                new Text("Brightness:").Bold().Color(Color.White),
                new Slider(
                    _brightnessBinding, 
                    0f, 100f, 10f,
                    label: "Brightness",
                    orientation: SliderOrientation.Horizontal,
                    trackChar: '▬',
                    thumbChar: '●',
                    thumbColor: Color.Yellow,
                    showValue: true,
                    valueFormat: "F0"
                )
            },
            
            new Text(new string('─', 40)).Color(Color.DarkGray),
            
            // Badge examples
            new VStack(spacing: 1) {
                new Text("Badge Examples:").Bold().Color(Color.White),
                new HStack(spacing: 2) {
                    new Badge("NEW", BadgeStyle.Rounded, BadgeVariant.Primary),
                    new Badge("BETA", BadgeStyle.Square, BadgeVariant.Warning),
                    new Badge(_notificationCount.ToString(), BadgeStyle.Count, BadgeVariant.Error),
                    new Badge("", BadgeStyle.Dot, _isOnline ? BadgeVariant.Success : BadgeVariant.Error),
                    new Badge("PRO", BadgeStyle.Pill, BadgeVariant.Info, bold: true)
                },
                
                new HStack(spacing: 2) {
                    new Badge("Custom Color", customColor: Color.Black, customBackgroundColor: Color.Cyan),
                    new Badge("⭐ Featured", BadgeStyle.Rounded, BadgeVariant.Primary),
                    new Badge("v2.0", BadgeStyle.Default, BadgeVariant.Secondary),
                    new Badge("LIVE", BadgeStyle.Square, BadgeVariant.Error, prefix: "🔴 ")
                }
            },
            
            new Text(new string('─', 40)).Color(Color.DarkGray),
            
            // Combined example
            new VStack(spacing: 1) {
                new BigText("STATUS", BigTextFont.Mini, color: Color.Green),
                new HStack(spacing: 2) {
                    new Badge(_isOnline ? "ONLINE" : "OFFLINE", 
                        BadgeStyle.Pill, 
                        _isOnline ? BadgeVariant.Success : BadgeVariant.Error),
                    new Text($"Volume: {_volumeLevel:F0}%"),
                    new Text($"Brightness: {_brightnessLevel:F0}%")
                },
                new Gradient($"System running at {(_volumeLevel + _brightnessLevel) / 2:F0}% capacity", 
                    Color.Yellow, Color.Green, underline: true)
            },
            
            new Spacer(),
            
            // Instructions
            new HStack
            {
                new Text("Tab: Next").Color(Color.DarkGray),
                new Text("Shift+Tab: Previous").Color(Color.DarkGray),
                new Text("Arrows: Adjust sliders").Color(Color.DarkGray),
                new Text("Space: Toggle status").Color(Color.DarkGray),
                new Text("+/-: Notifications").Color(Color.DarkGray),
                new Text("Ctrl+C: Exit").Color(Color.DarkGray)
            }
        };
    }
}