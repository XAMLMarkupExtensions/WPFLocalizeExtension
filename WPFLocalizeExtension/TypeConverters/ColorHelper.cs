#region Copyright information
// <copyright file="ColorHelper.cs">
//     Licensed under Microsoft Public License (Ms-PL)
//     http://wpflocalizeextension.codeplex.com/license
// </copyright>
// <author>Uwe Mayer</author>
#endregion

#if WINDOWS_PHONE
namespace WP7LocalizeExtension.TypeConverters
#else
namespace SLLocalizeExtension.TypeConverters
#endif
{
    using System;
    using System.Windows.Media;

    /// <summary>
    /// A class with color names.
    /// </summary>
    public static class ColorHelper
    {
        /// <summary>
        /// Create a color from a 4 byte uint.
        /// </summary>
        /// <param name="code">The color code.</param>
        /// <returns>The color</returns>
        public static Color FromArgb(UInt32 code)
        {
            Color c = new Color();

            c.A = (byte)(0x000000FF & (code >> 24));
            c.R = (byte)(0x000000FF & (code >> 16));
            c.G = (byte)(0x000000FF & (code >> 8));
            c.B = (byte)(0x000000FF & code);

            return c;
        }

        /// <summary>
        /// Select a color based on common names.
        /// </summary>
        /// <param name="name">The color name.</param>
        /// <returns>The color.</returns>
        public static Color FromName(string name)
        {
            switch (name)
            {
                case "Aliceblue": return ColorHelper.FromArgb(0xFFF0F8FF);
                case "AntiqueWhite": return ColorHelper.FromArgb(0xFFFAEBD7);
                case "Aqua": return ColorHelper.FromArgb(0xFF00FFFF);
                case "Aquamarine": return ColorHelper.FromArgb(0xFF7FFFD4);
                case "Azure": return ColorHelper.FromArgb(0xFFF0FFFF);
                case "Beige": return ColorHelper.FromArgb(0xFFF5F5DC);
                case "Bisque": return ColorHelper.FromArgb(0xFFFFE4C4);
                case "Black": return ColorHelper.FromArgb(0xFF000000);
                case "BlanchedAlmond": return ColorHelper.FromArgb(0xFFFFEBCD);
                case "Blue": return ColorHelper.FromArgb(0xFF0000FF);
                case "BlueViolet": return ColorHelper.FromArgb(0xFF8A2BE2);
                case "Brown": return ColorHelper.FromArgb(0xFFA52A2A);
                case "BurlyWood": return ColorHelper.FromArgb(0xFFDEB887);
                case "CadetBlue": return ColorHelper.FromArgb(0xFF5F9EA0);
                case "Chartreuse": return ColorHelper.FromArgb(0xFF7FFF00);
                case "Chocolate": return ColorHelper.FromArgb(0xFFD2691E);
                case "Coral": return ColorHelper.FromArgb(0xFFFF7F50);
                case "CornflowerBlue": return ColorHelper.FromArgb(0xFF6495ED);
                case "Cornsilk": return ColorHelper.FromArgb(0xFFFFF8DC);
                case "Crimson": return ColorHelper.FromArgb(0xFFDC143C);
                case "Cyan": return ColorHelper.FromArgb(0xFF00FFFF);
                case "DarkBlue": return ColorHelper.FromArgb(0xFF00008B);
                case "DarkCyan": return ColorHelper.FromArgb(0xFF008B8B);
                case "DarkGoldenrod": return ColorHelper.FromArgb(0xFFB8860B);
                case "DarkGray": return ColorHelper.FromArgb(0xFFA9A9A9);
                case "DarkGreen": return ColorHelper.FromArgb(0xFF006400);
                case "DarkKhaki": return ColorHelper.FromArgb(0xFFBDB76B);
                case "DarkMagenta": return ColorHelper.FromArgb(0xFF8B008B);
                case "DarkOliveGreen": return ColorHelper.FromArgb(0xFF556B2F);
                case "DarkOrange": return ColorHelper.FromArgb(0xFFFF8C00);
                case "DarkOrchid": return ColorHelper.FromArgb(0xFF9932CC);
                case "DarkRed": return ColorHelper.FromArgb(0xFF8B0000);
                case "DarkSalmon": return ColorHelper.FromArgb(0xFFE9967A);
                case "DarkSeaGreen": return ColorHelper.FromArgb(0xFF8FBC8F);
                case "DarkSlateBlue": return ColorHelper.FromArgb(0xFF483D8B);
                case "DarkSlateGray": return ColorHelper.FromArgb(0xFF2F4F4F);
                case "DarkTurquoise": return ColorHelper.FromArgb(0xFF00CED1);
                case "DarkViolet": return ColorHelper.FromArgb(0xFF9400D3);
                case "DeepPink": return ColorHelper.FromArgb(0xFFFF1493);
                case "DeepSkyBlue": return ColorHelper.FromArgb(0xFF00BFFF);
                case "DimGray": return ColorHelper.FromArgb(0xFF696969);
                case "DodgerBlue": return ColorHelper.FromArgb(0xFF1E90FF);
                case "Firebrick": return ColorHelper.FromArgb(0xFFB22222);
                case "FloralWhite": return ColorHelper.FromArgb(0xFFFFFAF0);
                case "ForestGreen": return ColorHelper.FromArgb(0xFF228B22);
                case "Gainsboro": return ColorHelper.FromArgb(0xFFDCDCDC);
                case "GhostWhite": return ColorHelper.FromArgb(0xFFF8F8FF);
                case "Gold": return ColorHelper.FromArgb(0xFFFFD700);
                case "Goldenrod": return ColorHelper.FromArgb(0xFFDAA520);
                case "Gray": return ColorHelper.FromArgb(0xFF808080);
                case "Green": return ColorHelper.FromArgb(0xFF008000);
                case "GreenYellow": return ColorHelper.FromArgb(0xFFADFF2F);
                case "Honeydew": return ColorHelper.FromArgb(0xFFF0FFF0);
                case "HotPink": return ColorHelper.FromArgb(0xFFFF69B4);
                case "IndianRed": return ColorHelper.FromArgb(0xFFCD5C5C);
                case "Indigo": return ColorHelper.FromArgb(0xFF4B0082);
                case "Ivory": return ColorHelper.FromArgb(0xFFFFFFF0);
                case "Khaki": return ColorHelper.FromArgb(0xFFF0E68C);
                case "Lavender": return ColorHelper.FromArgb(0xFFE6E6FA);
                case "LavenderBlush": return ColorHelper.FromArgb(0xFFFFF0F5);
                case "LawnGreen": return ColorHelper.FromArgb(0xFF7CFC00);
                case "LemonChiffon": return ColorHelper.FromArgb(0xFFFFFACD);
                case "LightBlue": return ColorHelper.FromArgb(0xFFADD8E6);
                case "LightCoral": return ColorHelper.FromArgb(0xFFF08080);
                case "LightCyan": return ColorHelper.FromArgb(0xFFE0FFFF);
                case "LightGoldenrodYellow": return ColorHelper.FromArgb(0xFFFAFAD2);
                case "LightGray": return ColorHelper.FromArgb(0xFFD3D3D3);
                case "LightGreen": return ColorHelper.FromArgb(0xFF90EE90);
                case "LightPink": return ColorHelper.FromArgb(0xFFFFB6C1);
                case "LightSalmon": return ColorHelper.FromArgb(0xFFFFA07A);
                case "LightSeaGreen": return ColorHelper.FromArgb(0xFF20B2AA);
                case "LightSkyBlue": return ColorHelper.FromArgb(0xFF87CEFA);
                case "LightSlateGray": return ColorHelper.FromArgb(0xFF778899);
                case "LightSteelBlue": return ColorHelper.FromArgb(0xFFB0C4DE);
                case "LightYellow": return ColorHelper.FromArgb(0xFFFFFFE0);
                case "Lime": return ColorHelper.FromArgb(0xFF00FF00);
                case "LimeGreen": return ColorHelper.FromArgb(0xFF32CD32);
                case "Linen": return ColorHelper.FromArgb(0xFFFAF0E6);
                case "Magenta": return ColorHelper.FromArgb(0xFFFF00FF);
                case "Maroon": return ColorHelper.FromArgb(0xFF800000);
                case "MediumAquamarine": return ColorHelper.FromArgb(0xFF66CDAA);
                case "MediumBlue": return ColorHelper.FromArgb(0xFF0000CD);
                case "MediumOrchid": return ColorHelper.FromArgb(0xFFBA55D3);
                case "MediumPurple": return ColorHelper.FromArgb(0xFF9370DB);
                case "MediumSeaGreen": return ColorHelper.FromArgb(0xFF3CB371);
                case "MediumSlateBlue": return ColorHelper.FromArgb(0xFF7B68EE);
                case "MediumSpringGreen": return ColorHelper.FromArgb(0xFF00FA9A);
                case "MediumTurquoise": return ColorHelper.FromArgb(0xFF48D1CC);
                case "MediumVioletRed": return ColorHelper.FromArgb(0xFFC71585);
                case "MidnightBlue": return ColorHelper.FromArgb(0xFF191970);
                case "MintCream": return ColorHelper.FromArgb(0xFFF5FFFA);
                case "MistyRose": return ColorHelper.FromArgb(0xFFFFE4E1);
                case "Moccasin": return ColorHelper.FromArgb(0xFFFFE4B5);
                case "NavajoWhite": return ColorHelper.FromArgb(0xFFFFDEAD);
                case "Navy": return ColorHelper.FromArgb(0xFF000080);
                case "OldLace": return ColorHelper.FromArgb(0xFFFDF5E6);
                case "Olive": return ColorHelper.FromArgb(0xFF808000);
                case "OliveDrab": return ColorHelper.FromArgb(0xFF6B8E23);
                case "Orange": return ColorHelper.FromArgb(0xFFFFA500);
                case "OrangeRed": return ColorHelper.FromArgb(0xFFFF4500);
                case "Orchid": return ColorHelper.FromArgb(0xFFDA70D6);
                case "PaleGoldenrod": return ColorHelper.FromArgb(0xFFEEE8AA);
                case "PaleGreen": return ColorHelper.FromArgb(0xFF98FB98);
                case "PaleTurquoise": return ColorHelper.FromArgb(0xFFAFEEEE);
                case "PaleVioletRed": return ColorHelper.FromArgb(0xFFDB7093);
                case "PapayaWhip": return ColorHelper.FromArgb(0xFFFFEFD5);
                case "PeachPuff": return ColorHelper.FromArgb(0xFFFFDAB9);
                case "Peru": return ColorHelper.FromArgb(0xFFCD853F);
                case "Pink": return ColorHelper.FromArgb(0xFFFFC0CB);
                case "Plum": return ColorHelper.FromArgb(0xFFDDA0DD);
                case "PowderBlue": return ColorHelper.FromArgb(0xFFB0E0E6);
                case "Purple": return ColorHelper.FromArgb(0xFF800080);
                case "Red": return ColorHelper.FromArgb(0xFFFF0000);
                case "RosyBrown": return ColorHelper.FromArgb(0xFFBC8F8F);
                case "RoyalBlue": return ColorHelper.FromArgb(0xFF4169E1);
                case "SaddleBrown": return ColorHelper.FromArgb(0xFF8B4513);
                case "Salmon": return ColorHelper.FromArgb(0xFFFA8072);
                case "SandyBrown": return ColorHelper.FromArgb(0xFFF4A460);
                case "SeaGreen": return ColorHelper.FromArgb(0xFF2E8B57);
                case "SeaShell": return ColorHelper.FromArgb(0xFFFFF5EE);
                case "Sienna": return ColorHelper.FromArgb(0xFFA0522D);
                case "Silver": return ColorHelper.FromArgb(0xFFC0C0C0);
                case "SkyBlue": return ColorHelper.FromArgb(0xFF87CEEB);
                case "SlateBlue": return ColorHelper.FromArgb(0xFF6A5ACD);
                case "SlateGray": return ColorHelper.FromArgb(0xFF708090);
                case "Snow": return ColorHelper.FromArgb(0xFFFFFAFA);
                case "SpringGreen": return ColorHelper.FromArgb(0xFF00FF7F);
                case "SteelBlue": return ColorHelper.FromArgb(0xFF4682B4);
                case "Tan": return ColorHelper.FromArgb(0xFFD2B48C);
                case "Teal": return ColorHelper.FromArgb(0xFF008080);
                case "Thistle": return ColorHelper.FromArgb(0xFFD8BFD8);
                case "Tomato": return ColorHelper.FromArgb(0xFFFF6347);
                case "Transparent": return ColorHelper.FromArgb(0x00FFFFFF);
                case "Turquoise": return ColorHelper.FromArgb(0xFF40E0D0);
                case "Violet": return ColorHelper.FromArgb(0xFFEE82EE);
                case "Wheat": return ColorHelper.FromArgb(0xFFF5DEB3);
                case "White": return ColorHelper.FromArgb(0xFFFFFFFF);
                case "WhiteSmoke": return ColorHelper.FromArgb(0xFFF5F5F5);
                case "Yellow": return ColorHelper.FromArgb(0xFFFFFF00);
                case "YellowGreen": return ColorHelper.FromArgb(0xFF9ACD32);
            }

            return new Color();
        }
    }
}