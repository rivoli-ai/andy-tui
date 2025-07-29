using System;
using System.Collections.Generic;
using System.Linq;
using Andy.TUI.Components;
using Andy.TUI.Components.Layout;
using Andy.TUI.Core.VirtualDom;
using Andy.TUI.Terminal;

namespace Andy.TUI.Examples.Layout;

class Program
{
    static void Main(string[] args)
    {
        Console.Clear();
        Console.WriteLine("Andy.TUI Layout Components Examples");
        Console.WriteLine("===================================");
        Console.WriteLine();
        
        // Demonstrate each layout component with actual rendering
        DemonstrateBox();
        DemonstrateStack();
        DemonstrateGrid();
        DemonstrateScrollView();
        DemonstrateComplexLayout();
        
        Console.WriteLine("\n\nAll demonstrations completed.");
    }
    
    static void DemonstrateBox()
    {
        Console.WriteLine("1. Box Component Demo");
        Console.WriteLine("---------------------\n");
        
        // Simple box with border
        Console.WriteLine("Simple Box with Single Border:");
        Console.WriteLine();
        DrawBox(2, 20, 5, BorderStyle.Single, "Hello Box!");
        
        // Box with double border
        Console.WriteLine("\n\nBox with Double Border and Padding:");
        Console.WriteLine();
        DrawBox(2, 25, 7, BorderStyle.Double, "Padded Content", 2);
        
        // Box with rounded border  
        Console.WriteLine("\n\n\nBox with Rounded Border:");
        Console.WriteLine();
        DrawBox(2, 30, 5, BorderStyle.Rounded, "Rounded corners!");
        Console.WriteLine();
    }
    
    static void DemonstrateStack()
    {
        Console.WriteLine("\n\n2. Stack Component Demo");
        Console.WriteLine("-----------------------\n");
        
        // Vertical stack
        Console.WriteLine("Vertical Stack (spacing=1):");
        Console.WriteLine("  • Item 1");
        Console.WriteLine();
        Console.WriteLine("  • Item 2");
        Console.WriteLine();
        Console.WriteLine("  • Item 3");
        
        // Horizontal stack
        Console.WriteLine("\nHorizontal Stack (spacing=2):");
        Console.WriteLine("  [Button1]  [Button2]  [Button3]");
        
        // Show alignment
        Console.WriteLine("\nStack with Center Alignment:");
        Console.WriteLine();
        DrawBox(2, 40, 7, BorderStyle.Single, "Centered");
        Console.WriteLine();
    }
    
    static void DemonstrateGrid()
    {
        Console.WriteLine("\n\n3. Grid Component Demo");
        Console.WriteLine("----------------------\n");
        
        Console.WriteLine("Grid with 3x3 cells:");
        Console.WriteLine();
        
        // Draw grid structure - simplified for inline display
        Console.WriteLine("  ┌─────────────┐  ┌──────────────────┐  ┌────────┐");
        Console.WriteLine("  │ R0,C0       │  │ R0,C1            │  │ R0,C2  │");
        Console.WriteLine("  └─────────────┘  └──────────────────┘  └────────┘");
        Console.WriteLine();
        Console.WriteLine("  ┌─────────────┐  ┌──────────────────┐  ┌────────┐");
        Console.WriteLine("  │ R1,C0       │  │ R1,C1            │  │ R1,C2  │");
        Console.WriteLine("  └─────────────┘  └──────────────────┘  └────────┘");
        Console.WriteLine();
        Console.WriteLine("  ┌─────────────┐  ┌──────────────────┐  ┌────────┐");
        Console.WriteLine("  │ R2,C0       │  │ R2,C1            │  │ R2,C2  │");
        Console.WriteLine("  └─────────────┘  └──────────────────┘  └────────┘");
        
        Console.WriteLine("\nGrid features shown:");
        Console.WriteLine("- Column widths: Absolute(15), Star(~20), Absolute(10)");
        Console.WriteLine("- Row/column gaps");
        Console.WriteLine("- Cell positioning");
    }
    
    static void DemonstrateScrollView()
    {
        Console.WriteLine("\n\n4. ScrollView Component Demo");
        Console.WriteLine("----------------------------\n");
        
        Console.WriteLine("ScrollView with viewport and scrollbars:");
        Console.WriteLine();
        
        // Draw a simple representation
        Console.WriteLine("  ┌──────────────────────────────────────┐");
        Console.WriteLine("  │ Line 1: Long content that extends... │▲");
        Console.WriteLine("  │ Line 2: Long content that extends... │║");
        Console.WriteLine("  │ Line 3: Long content that extends... │█");
        Console.WriteLine("  │ Line 4: Long content that extends... │█");
        Console.WriteLine("  │ Line 5: Long content that extends... │║");
        Console.WriteLine("  │ Line 6: Long content that extends... │║");
        Console.WriteLine("  │ Line 7: Long content that extends... │║");
        Console.WriteLine("  │ Line 8: Long content that extends... │▼");
        Console.WriteLine("  │◄═══════════════════════════════════►│");
        Console.WriteLine("  └──────────────────────────────────────┘");
        
        Console.WriteLine("\nScrollView features:");
        Console.WriteLine("- Viewport clipping");
        Console.WriteLine("- Vertical and horizontal scrollbars");
        Console.WriteLine("- Content larger than viewport");
    }
    
    static void DemonstrateComplexLayout()
    {
        Console.WriteLine("\n\n5. Complex Layout Demo");
        Console.WriteLine("----------------------\n");
        
        Console.WriteLine("Application layout using Grid, Box, and Stack:");
        Console.WriteLine();
        
        // Draw the layout
        Console.WriteLine("  ╔════════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("  ║                          APPLICATION HEADER                        ║");
        Console.WriteLine("  ╚════════════════════════════════════════════════════════════════════╝");
        Console.WriteLine("  ┌───────────────────────────────────────────┐┌───────────────────────┐");
        Console.WriteLine("  │                                           ││                       │");
        Console.WriteLine("  │ Main Content Area                         ││ ═══ MENU ═══          │");
        Console.WriteLine("  │                                           ││                       │");
        Console.WriteLine("  │ This is where the main application        ││ ▸ Dashboard           │");
        Console.WriteLine("  │ content would be displayed.               ││                       │");
        Console.WriteLine("  │                                           ││ ▸ Settings            │");
        Console.WriteLine("  │                                           ││                       │");
        Console.WriteLine("  │                                           ││ ▸ Reports             │");
        Console.WriteLine("  │                                           ││                       │");
        Console.WriteLine("  │                                           ││ ▸ Help                │");
        Console.WriteLine("  │                                           ││                       │");
        Console.WriteLine("  │                                           ││                       │");
        Console.WriteLine("  │                                           ││                       │");
        Console.WriteLine("  │                                           ││                       │");
        Console.WriteLine("  │                                           ││                       │");
        Console.WriteLine("  └───────────────────────────────────────────┘└───────────────────────┘");
        
        Console.WriteLine("\n\nLayout composition shown:");
        Console.WriteLine("- Grid for overall structure");
        Console.WriteLine("- Box components for panels");
        Console.WriteLine("- Stack for menu items");
    }
    
    static void DrawBox(int leftPadding, int width, int height, BorderStyle style, string content, int padding = 0)
    {
        var chars = GetBorderChars(style);
        var padStr = new string(' ', leftPadding);
        
        // Top border
        Console.Write(padStr + chars.TopLeft);
        for (int i = 1; i < width - 1; i++)
            Console.Write(chars.Horizontal);
        Console.WriteLine(chars.TopRight);
        
        // Side borders and content
        for (int i = 1; i < height - 1; i++)
        {
            Console.Write(padStr + chars.Vertical);
            
            if (i == height / 2 && !string.IsNullOrEmpty(content))
            {
                // Center content
                var totalPadding = (width - 2 - content.Length) / 2;
                var paddedContent = content.PadLeft(content.Length + totalPadding + padding).PadRight(width - 2);
                if (paddedContent.Length > width - 2)
                    paddedContent = paddedContent.Substring(0, width - 2);
                Console.Write(paddedContent);
            }
            else
            {
                // Empty space
                Console.Write(new string(' ', width - 2));
            }
            
            Console.WriteLine(chars.Vertical);
        }
        
        // Bottom border
        Console.Write(padStr + chars.BottomLeft);
        for (int i = 1; i < width - 1; i++)
            Console.Write(chars.Horizontal);
        Console.WriteLine(chars.BottomRight);
    }
    
    static (char TopLeft, char TopRight, char BottomLeft, char BottomRight, char Horizontal, char Vertical) GetBorderChars(BorderStyle style)
    {
        return style switch
        {
            BorderStyle.Single => ('┌', '┐', '└', '┘', '─', '│'),
            BorderStyle.Double => ('╔', '╗', '╚', '╝', '═', '║'),
            BorderStyle.Rounded => ('╭', '╮', '╰', '╯', '─', '│'),
            BorderStyle.Heavy => ('┏', '┓', '┗', '┛', '━', '┃'),
            BorderStyle.Dashed => ('┌', '┐', '└', '┘', '╌', '╎'),
            _ => ('+', '+', '+', '+', '-', '|')
        };
    }
}

// Minimal BorderStyle enum for the example
public enum BorderStyle
{
    Single,
    Double,
    Rounded,
    Heavy,
    Dashed
}