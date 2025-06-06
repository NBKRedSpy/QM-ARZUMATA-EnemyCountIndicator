using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

internal static class Helpers
{
    // Convert colors to hex and log them
    public static string FaceColorToHex(Color color) => $"#{color.r:F0}{color.g:F0}{color.b:F0}";
    public static string AlphaAwareColorToHex(Color color) => $"#{(int)(color.r * 255):X2}{(int)(color.g * 255):X2}{(int)(color.b * 255):X2}{(int)(color.a * 255):X2}";

    public static Color HexStringToUnityColor(string hex)
    {
        if (string.IsNullOrEmpty(hex) || !hex.StartsWith("#"))
        {
            throw new ArgumentException("Invalid color format", nameof(hex));
        }

        // Check the length of the input string to determine if it has an alpha channel
        if (hex.Length == 7)  // No alpha channel provided, assume full opacity
        {
            hex = "#FF" + hex.Substring(0);
        }
        else if (hex.Length != 9)
        {
            throw new ArgumentException("Invalid color format", nameof(hex));
        }

        // Parse the R, G, B, and A values from the hex string
        int r = int.Parse(hex.Substring(1, 2), System.Globalization.NumberStyles.HexNumber);
        int g = int.Parse(hex.Substring(3, 2), System.Globalization.NumberStyles.HexNumber);
        int b = int.Parse(hex.Substring(5, 2), System.Globalization.NumberStyles.HexNumber);
        int a = int.Parse(hex.Substring(7, 2), System.Globalization.NumberStyles.HexNumber);

        // Convert to normalized float values
        return new Color(r / 255f, g / 255f, b / 255f, a / 255f);
    }
}
