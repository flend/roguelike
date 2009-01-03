using System;
using System.Runtime.InteropServices;
using System.Text;

namespace libtcodWrapper
{
    /// <summary>
    /// Represents a 32-bit color to the TCOD API.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Color
    {
        private byte r;
        /// <summary>
        /// Red
        /// </summary>
        public byte Red
        {
            get { return r; }
        }

        private byte g;
        /// <summary>
        /// Green
        /// </summary>
        public byte Green
        {
            get { return g; }
        }

        private byte b;
        /// <summary>
        /// Blue
        /// </summary>
        public byte Blue
        {
            get { return b; }
        }

        /// <summary>
        /// Hue (0.0 - 360.0)
        /// </summary>
        public float Hue
        {
            get
            {
                float h, s, v;
                TCOD_color_get_HSV(this, out h, out s, out v);
                return h;
            }
        }

        /// <summary>
        /// Saturation (0.0 - 1.0)
        /// </summary>
        public float Saturation
        {
            get
            {
                float h, s, v;
                TCOD_color_get_HSV(this, out h, out s, out v);
                return s;
            }
        }

        /// <summary>
        /// Value (0.0 - 1.0)
        /// </summary>
        public float Value
        {
            get
            {
                float h, s, v;
                TCOD_color_get_HSV(this, out h, out s, out v);
                return v;
            }
        }

        private Color(byte red, byte green, byte blue)
        {
            r = red;
            g = green;
            b = blue;
        }

        /// <summary>
        /// Form a Color from RGB components.
        /// </summary>
        /// <param name="red">Red Component (0 - 255)</param>
        /// <param name="green">Green Component (0 - 255)</param>
        /// <param name="blue">Blue Component (0 - 255)</param>
        public static Color FromRGB(byte red, byte green, byte blue)
        {
            return new Color(red, green, blue);
        }
        
        /// <summary>
        /// Form a Color from HSV components.
        /// </summary>
        /// <param name="hue">Hue Component (0.0 - 360.0)</param>
        /// <param name="saturation">Saturation Component (0.0 - 1.0)</param>
        /// <param name="value">Value Component (0.0 - 1.0)</param>
        public static Color FromHSV(float hue, float saturation, float value)
        {
            Color c = new Color(0, 0, 0);
            TCOD_color_set_HSV(ref c, hue, saturation, value);
            return c;
        }

        /// <summary>
        /// Returns HSV value from a Color.
        /// </summary>
        /// <param name="h">Hue Component (0.0 - 360.0)</param>
        /// <param name="s">Saturation Component (0.0 - 1.0)</param>
        /// <param name="v">Value Component (0.0 - 1.0)</param>
        public void GetHSV(out float h, out float s, out float v)
        {
            TCOD_color_get_HSV(this, out h, out s, out v);
        }

        /// <summary>
        /// Determine if two Colors are equal.
        /// </summary>
        /// <param name="obj">Other Color</param>
        /// <returns>Are Equal?</returns>
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            return TCOD_color_equals(this, (Color)obj);
        }

        /// <summary>
        /// Calculate Hash Value of a Color
        /// </summary>
        /// <returns>Hash Value</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// Determine if two Colors are equal.
        /// </summary>
        /// <param name="lhs">Left Hand Size</param>
        /// <param name="rhs">Right Hand Size</param>
        /// <returns>Are Equal?</returns>
        public static bool operator ==(Color lhs, Color rhs)
        {
            return TCOD_color_equals(lhs, rhs);
        }

        /// <summary>
        /// Determine if two Colors are not equal.
        /// </summary>
        /// <param name="lhs">Left Hand Size</param>
        /// <param name="rhs">Right Hand Size</param>
        /// <returns>Are Not Equal?</returns>
        public static bool operator !=(Color lhs, Color rhs)
        {
            return !TCOD_color_equals(lhs, rhs);
        }

        /// <summary>
        /// Add two Colors
        /// </summary>
        /// <param name="lhs">Left Hand Size</param>
        /// <param name="rhs">Right Hand Size</param>
        /// <returns>New Color</returns>
        public static Color operator +(Color lhs, Color rhs)
        {
            return TCOD_color_add(lhs, rhs);
        }

        /// <summary>
        /// Multiply two Colors
        /// </summary>
        /// <param name="lhs">Left Hand Size</param>
        /// <param name="rhs">Right Hand Size</param>
        /// <returns>New Color</returns>
        public static Color operator *(Color lhs, Color rhs)
        {
            return TCOD_color_multiply(lhs, rhs);
        }

        /// <summary>
        /// Multiple a Color by a constant
        /// </summary>
        /// <param name="lhs">Left Hand Size</param>
        /// <param name="rhs">Right Hand Size</param>
        /// <returns>New Color</returns>
        public static Color operator *(Color lhs, float rhs)
        {
            return TCOD_color_multiply_scalar(lhs, rhs);
        }

        /// <summary>
        /// Multiple a Color by a constant
        /// </summary>
        /// <param name="lhs">Left Hand Size</param>
        /// <param name="rhs">Right Hand Size</param>
        /// <returns>New Color</returns>
        public static Color operator *(Color lhs, double rhs)
        {
            return TCOD_color_multiply_scalar(lhs, (float)rhs);
        }

        /// <summary>
        /// Divide each component of a color by a give constant
        /// </summary>
        /// <param name="lhs">Left Hand Side Color</param>
        /// <param name="rhs">Right Hand Side Constant</param>
        /// <returns>New Color</returns>
        public static Color operator /(Color lhs, int rhs)
        {
            return new Color((byte)(lhs.r / rhs), (byte)(lhs.g / rhs), (byte)(lhs.b / rhs));
        }

        /// <summary>
        /// Divide each component of a color by a give constant
        /// </summary>
        /// <param name="lhs">Left Hand Side Color</param>
        /// <param name="rhs">Right Hand Side Constant</param>
        /// <returns>New Color</returns>
        public static Color operator /(Color lhs, float rhs)
        {
            return new Color((byte)((float)lhs.r / rhs), (byte)((float)lhs.g / rhs), (byte)((float)lhs.b / rhs));
        }

        /// <summary>
        /// Divide each component of a color by a give constant
        /// </summary>
        /// <param name="lhs">Left Hand Side Color</param>
        /// <param name="rhs">Right Hand Side Constant</param>
        /// <returns>New Color</returns>
        public static Color operator /(Color lhs, double rhs)
        {
            return new Color((byte)((double)lhs.r / rhs), (byte)((double)lhs.g / rhs), (byte)((double)lhs.b / rhs));
        }

        /// <summary>
        /// Subtract each component of a color from another, flooring to zero.
        /// </summary>
        /// <param name="lhs">Left Hand Side</param>
        /// <param name="rhs">Right Hand Side</param>
        /// <returns>New Color</returns>
        public static Color operator -(Color lhs, Color rhs)
        {
            return new Color(SubFloor(lhs.r, rhs.r), SubFloor(lhs.g, rhs.g), SubFloor(lhs.b, rhs.b));
        }

        private static byte SubFloor(byte lhs, byte rhs)
        {
            if (lhs < rhs)
                return 0;
            else
                return (byte)(lhs - rhs);
        }

        /// <summary>
        /// Interpolate (lerp) a Color with another Color
        /// </summary>
        /// <param name="c1">First Color</param>
        /// <param name="c2">Second Color</param>
        /// <param name="coef">Interpolate Coefficient</param>
        /// <returns>New Color</returns>
        public static Color Interpolate(Color c1, Color c2, float coef)
        {
            Color ret =  new Color(); 
            ret.r=(byte)(c1.r+(c2.r-c1.r)*coef);
            ret.g=(byte)(c1.g+(c2.g-c1.g)*coef);
            ret.b=(byte)(c1.b+(c2.b-c1.b)*coef);
            return ret;
        }

        /// <summary>
        /// Interpolate (lerp) a Color with another Color
        /// </summary>
        /// <param name="c1">First Color</param>
        /// <param name="c2">Second Color</param>
        /// <param name="coef">Interpolate Coefficient</param>
        /// <returns>New Color</returns>
        public static Color Interpolate(Color c1, Color c2, double coef)
        {
            Color ret =  new Color(); 
            ret.r=(byte)(c1.r+(c2.r-c1.r)*coef);
            ret.g=(byte)(c1.g+(c2.g-c1.g)*coef);
            ret.b=(byte)(c1.b+(c2.b-c1.b)*coef);
            return ret;
        }

        [DllImport(DLLName.name)]
        private extern static Color TCOD_color_add(Color c1, Color c2);

        [DllImport(DLLName.name)]
        private extern static Color TCOD_color_multiply(Color c1, Color c2);

        [DllImport(DLLName.name)]
        private extern static bool TCOD_color_equals(Color c1, Color c2);

        [DllImport(DLLName.name)]
        private extern static Color TCOD_color_multiply_scalar (Color c1, float value);

        // 0<= h < 360, 0 <= s <= 1, 0 <= v <= 1 
        [DllImport(DLLName.name)]
        private extern static void TCOD_color_set_HSV(ref Color c, float h, float s, float v);

        [DllImport(DLLName.name)]
        private extern static void TCOD_color_get_HSV(Color c, out float h, out float s, out float v);
    }

	/// <summary>
	/// A listing of the System.Drawing.Color preset colors, converted
	/// to the TCOD Color format.
	/// </summary>
	public static class ColorPresets
	{
		/// <summary>
		/// The color AliceBlue; RGB (240, 248, 255)
		/// </summary>
		public static readonly Color AliceBlue = Color.FromRGB(240, 248, 255);
		/// <summary>
		/// The color AntiqueWhite; RGB (250, 235, 215)
		/// </summary>
		public static readonly Color AntiqueWhite = Color.FromRGB(250, 235, 215);
		/// <summary>
		/// The color Aqua; RGB (0, 255, 255)
		/// </summary>
		public static readonly Color Aqua = Color.FromRGB(0, 255, 255);
		/// <summary>
		/// The color Aquamarine; RGB (127, 255, 212)
		/// </summary>
		public static readonly Color Aquamarine = Color.FromRGB(127, 255, 212);
		/// <summary>
		/// The color Azure; RGB (240, 255, 255)
		/// </summary>
		public static readonly Color Azure = Color.FromRGB(240, 255, 255);
		/// <summary>
		/// The color Beige; RGB (245, 245, 220)
		/// </summary>
		public static readonly Color Beige = Color.FromRGB(245, 245, 220);
		/// <summary>
		/// The color Bisque; RGB (255, 228, 196)
		/// </summary>
		public static readonly Color Bisque = Color.FromRGB(255, 228, 196);
		/// <summary>
		/// The color Black; RGB (0, 0, 0)
		/// </summary>
		public static readonly Color Black = Color.FromRGB(0, 0, 0);
		/// <summary>
		/// The color BlanchedAlmond; RGB (255, 235, 205)
		/// </summary>
		public static readonly Color BlanchedAlmond = Color.FromRGB(255, 235, 205);
		/// <summary>
		/// The color Blue; RGB (0, 0, 255)
		/// </summary>
		public static readonly Color Blue = Color.FromRGB(0, 0, 255);
		/// <summary>
		/// The color BlueViolet; RGB (138, 43, 226)
		/// </summary>
		public static readonly Color BlueViolet = Color.FromRGB(138, 43, 226);
		/// <summary>
		/// The color Brown; RGB (165, 42, 42)
		/// </summary>
		public static readonly Color Brown = Color.FromRGB(165, 42, 42);
		/// <summary>
		/// The color BurlyWood; RGB (222, 184, 135)
		/// </summary>
		public static readonly Color BurlyWood = Color.FromRGB(222, 184, 135);
		/// <summary>
		/// The color CadetBlue; RGB (95, 158, 160)
		/// </summary>
		public static readonly Color CadetBlue = Color.FromRGB(95, 158, 160);
		/// <summary>
		/// The color Chartreuse; RGB (127, 255, 0)
		/// </summary>
		public static readonly Color Chartreuse = Color.FromRGB(127, 255, 0);
		/// <summary>
		/// The color Chocolate; RGB (210, 105, 30)
		/// </summary>
		public static readonly Color Chocolate = Color.FromRGB(210, 105, 30);
		/// <summary>
		/// The color Coral; RGB (255, 127, 80)
		/// </summary>
		public static readonly Color Coral = Color.FromRGB(255, 127, 80);
		/// <summary>
		/// The color CornflowerBlue; RGB (100, 149, 237)
		/// </summary>
		public static readonly Color CornflowerBlue = Color.FromRGB(100, 149, 237);
		/// <summary>
		/// The color Cornsilk; RGB (255, 248, 220)
		/// </summary>
		public static readonly Color Cornsilk = Color.FromRGB(255, 248, 220);
		/// <summary>
		/// The color Crimson; RGB (220, 20, 60)
		/// </summary>
		public static readonly Color Crimson = Color.FromRGB(220, 20, 60);
		/// <summary>
		/// The color Cyan; RGB (0, 255, 255)
		/// </summary>
		public static readonly Color Cyan = Color.FromRGB(0, 255, 255);
		/// <summary>
		/// The color DarkBlue; RGB (0, 0, 139)
		/// </summary>
		public static readonly Color DarkBlue = Color.FromRGB(0, 0, 139);
		/// <summary>
		/// The color DarkCyan; RGB (0, 139, 139)
		/// </summary>
		public static readonly Color DarkCyan = Color.FromRGB(0, 139, 139);
		/// <summary>
		/// The color DarkGoldenrod; RGB (184, 134, 11)
		/// </summary>
		public static readonly Color DarkGoldenrod = Color.FromRGB(184, 134, 11);
		/// <summary>
		/// The color DarkGray; RGB (169, 169, 169)
		/// </summary>
		public static readonly Color DarkGray = Color.FromRGB(169, 169, 169);
		/// <summary>
		/// The color DarkGreen; RGB (0, 100, 0)
		/// </summary>
		public static readonly Color DarkGreen = Color.FromRGB(0, 100, 0);
		/// <summary>
		/// The color DarkKhaki; RGB (189, 183, 107)
		/// </summary>
		public static readonly Color DarkKhaki = Color.FromRGB(189, 183, 107);
		/// <summary>
		/// The color DarkMagenta; RGB (139, 0, 139)
		/// </summary>
		public static readonly Color DarkMagenta = Color.FromRGB(139, 0, 139);
		/// <summary>
		/// The color DarkOliveGreen; RGB (85, 107, 47)
		/// </summary>
		public static readonly Color DarkOliveGreen = Color.FromRGB(85, 107, 47);
		/// <summary>
		/// The color DarkOrange; RGB (255, 140, 0)
		/// </summary>
		public static readonly Color DarkOrange = Color.FromRGB(255, 140, 0);
		/// <summary>
		/// The color DarkOrchid; RGB (153, 50, 204)
		/// </summary>
		public static readonly Color DarkOrchid = Color.FromRGB(153, 50, 204);
		/// <summary>
		/// The color DarkRed; RGB (139, 0, 0)
		/// </summary>
		public static readonly Color DarkRed = Color.FromRGB(139, 0, 0);
		/// <summary>
		/// The color DarkSalmon; RGB (233, 150, 122)
		/// </summary>
		public static readonly Color DarkSalmon = Color.FromRGB(233, 150, 122);
		/// <summary>
		/// The color DarkSeaGreen; RGB (143, 188, 139)
		/// </summary>
		public static readonly Color DarkSeaGreen = Color.FromRGB(143, 188, 139);
		/// <summary>
		/// The color DarkSlateBlue; RGB (72, 61, 139)
		/// </summary>
		public static readonly Color DarkSlateBlue = Color.FromRGB(72, 61, 139);
		/// <summary>
		/// The color DarkSlateGray; RGB (47, 79, 79)
		/// </summary>
		public static readonly Color DarkSlateGray = Color.FromRGB(47, 79, 79);
		/// <summary>
		/// The color DarkTurquoise; RGB (0, 206, 209)
		/// </summary>
		public static readonly Color DarkTurquoise = Color.FromRGB(0, 206, 209);
		/// <summary>
		/// The color DarkViolet; RGB (148, 0, 211)
		/// </summary>
		public static readonly Color DarkViolet = Color.FromRGB(148, 0, 211);
		/// <summary>
		/// The color DeepPink; RGB (255, 20, 147)
		/// </summary>
		public static readonly Color DeepPink = Color.FromRGB(255, 20, 147);
		/// <summary>
		/// The color DeepSkyBlue; RGB (0, 191, 255)
		/// </summary>
		public static readonly Color DeepSkyBlue = Color.FromRGB(0, 191, 255);
		/// <summary>
		/// The color DimGray; RGB (105, 105, 105)
		/// </summary>
		public static readonly Color DimGray = Color.FromRGB(105, 105, 105);
		/// <summary>
		/// The color DodgerBlue; RGB (30, 144, 255)
		/// </summary>
		public static readonly Color DodgerBlue = Color.FromRGB(30, 144, 255);
		/// <summary>
		/// The color Firebrick; RGB (178, 34, 34)
		/// </summary>
		public static readonly Color Firebrick = Color.FromRGB(178, 34, 34);
		/// <summary>
		/// The color FloralWhite; RGB (255, 250, 240)
		/// </summary>
		public static readonly Color FloralWhite = Color.FromRGB(255, 250, 240);
		/// <summary>
		/// The color ForestGreen; RGB (34, 139, 34)
		/// </summary>
		public static readonly Color ForestGreen = Color.FromRGB(34, 139, 34);
		/// <summary>
		/// The color Fuchsia; RGB (255, 0, 255)
		/// </summary>
		public static readonly Color Fuchsia = Color.FromRGB(255, 0, 255);
		/// <summary>
		/// The color Gainsboro; RGB (220, 220, 220)
		/// </summary>
		public static readonly Color Gainsboro = Color.FromRGB(220, 220, 220);
		/// <summary>
		/// The color GhostWhite; RGB (248, 248, 255)
		/// </summary>
		public static readonly Color GhostWhite = Color.FromRGB(248, 248, 255);
		/// <summary>
		/// The color Gold; RGB (255, 215, 0)
		/// </summary>
		public static readonly Color Gold = Color.FromRGB(255, 215, 0);
		/// <summary>
		/// The color Goldenrod; RGB (218, 165, 32)
		/// </summary>
		public static readonly Color Goldenrod = Color.FromRGB(218, 165, 32);
		/// <summary>
		/// The color Gray; RGB (128, 128, 128)
		/// </summary>
		public static readonly Color Gray = Color.FromRGB(128, 128, 128);
		/// <summary>
		/// The color Green; RGB (0, 128, 0)
		/// </summary>
		public static readonly Color Green = Color.FromRGB(0, 128, 0);
		/// <summary>
		/// The color GreenYellow; RGB (173, 255, 47)
		/// </summary>
		public static readonly Color GreenYellow = Color.FromRGB(173, 255, 47);
		/// <summary>
		/// The color Honeydew; RGB (240, 255, 240)
		/// </summary>
		public static readonly Color Honeydew = Color.FromRGB(240, 255, 240);
		/// <summary>
		/// The color HotPink; RGB (255, 105, 180)
		/// </summary>
		public static readonly Color HotPink = Color.FromRGB(255, 105, 180);
		/// <summary>
		/// The color IndianRed; RGB (205, 92, 92)
		/// </summary>
		public static readonly Color IndianRed = Color.FromRGB(205, 92, 92);
		/// <summary>
		/// The color Indigo; RGB (75, 0, 130)
		/// </summary>
		public static readonly Color Indigo = Color.FromRGB(75, 0, 130);
		/// <summary>
		/// The color Ivory; RGB (255, 255, 240)
		/// </summary>
		public static readonly Color Ivory = Color.FromRGB(255, 255, 240);
		/// <summary>
		/// The color Khaki; RGB (240, 230, 140)
		/// </summary>
		public static readonly Color Khaki = Color.FromRGB(240, 230, 140);
		/// <summary>
		/// The color Lavender; RGB (230, 230, 250)
		/// </summary>
		public static readonly Color Lavender = Color.FromRGB(230, 230, 250);
		/// <summary>
		/// The color LavenderBlush; RGB (255, 240, 245)
		/// </summary>
		public static readonly Color LavenderBlush = Color.FromRGB(255, 240, 245);
		/// <summary>
		/// The color LawnGreen; RGB (124, 252, 0)
		/// </summary>
		public static readonly Color LawnGreen = Color.FromRGB(124, 252, 0);
		/// <summary>
		/// The color LemonChiffon; RGB (255, 250, 205)
		/// </summary>
		public static readonly Color LemonChiffon = Color.FromRGB(255, 250, 205);
		/// <summary>
		/// The color LightBlue; RGB (173, 216, 230)
		/// </summary>
		public static readonly Color LightBlue = Color.FromRGB(173, 216, 230);
		/// <summary>
		/// The color LightCoral; RGB (240, 128, 128)
		/// </summary>
		public static readonly Color LightCoral = Color.FromRGB(240, 128, 128);
		/// <summary>
		/// The color LightCyan; RGB (224, 255, 255)
		/// </summary>
		public static readonly Color LightCyan = Color.FromRGB(224, 255, 255);
		/// <summary>
		/// The color LightGoldenrodYellow; RGB (250, 250, 210)
		/// </summary>
		public static readonly Color LightGoldenrodYellow = Color.FromRGB(250, 250, 210);
		/// <summary>
		/// The color LightGray; RGB (211, 211, 211)
		/// </summary>
		public static readonly Color LightGray = Color.FromRGB(211, 211, 211);
		/// <summary>
		/// The color LightGreen; RGB (144, 238, 144)
		/// </summary>
		public static readonly Color LightGreen = Color.FromRGB(144, 238, 144);
		/// <summary>
		/// The color LightPink; RGB (255, 182, 193)
		/// </summary>
		public static readonly Color LightPink = Color.FromRGB(255, 182, 193);
		/// <summary>
		/// The color LightSalmon; RGB (255, 160, 122)
		/// </summary>
		public static readonly Color LightSalmon = Color.FromRGB(255, 160, 122);
		/// <summary>
		/// The color LightSeaGreen; RGB (32, 178, 170)
		/// </summary>
		public static readonly Color LightSeaGreen = Color.FromRGB(32, 178, 170);
		/// <summary>
		/// The color LightSkyBlue; RGB (135, 206, 250)
		/// </summary>
		public static readonly Color LightSkyBlue = Color.FromRGB(135, 206, 250);
		/// <summary>
		/// The color LightSlateGray; RGB (119, 136, 153)
		/// </summary>
		public static readonly Color LightSlateGray = Color.FromRGB(119, 136, 153);
		/// <summary>
		/// The color LightSteelBlue; RGB (176, 196, 222)
		/// </summary>
		public static readonly Color LightSteelBlue = Color.FromRGB(176, 196, 222);
		/// <summary>
		/// The color LightYellow; RGB (255, 255, 224)
		/// </summary>
		public static readonly Color LightYellow = Color.FromRGB(255, 255, 224);
		/// <summary>
		/// The color Lime; RGB (0, 255, 0)
		/// </summary>
		public static readonly Color Lime = Color.FromRGB(0, 255, 0);
		/// <summary>
		/// The color LimeGreen; RGB (50, 205, 50)
		/// </summary>
		public static readonly Color LimeGreen = Color.FromRGB(50, 205, 50);
		/// <summary>
		/// The color Linen; RGB (250, 240, 230)
		/// </summary>
		public static readonly Color Linen = Color.FromRGB(250, 240, 230);
		/// <summary>
		/// The color Magenta; RGB (255, 0, 255)
		/// </summary>
		public static readonly Color Magenta = Color.FromRGB(255, 0, 255);
		/// <summary>
		/// The color Maroon; RGB (128, 0, 0)
		/// </summary>
		public static readonly Color Maroon = Color.FromRGB(128, 0, 0);
		/// <summary>
		/// The color MediumAquamarine; RGB (102, 205, 170)
		/// </summary>
		public static readonly Color MediumAquamarine = Color.FromRGB(102, 205, 170);
		/// <summary>
		/// The color MediumBlue; RGB (0, 0, 205)
		/// </summary>
		public static readonly Color MediumBlue = Color.FromRGB(0, 0, 205);
		/// <summary>
		/// The color MediumOrchid; RGB (186, 85, 211)
		/// </summary>
		public static readonly Color MediumOrchid = Color.FromRGB(186, 85, 211);
		/// <summary>
		/// The color MediumPurple; RGB (147, 112, 219)
		/// </summary>
		public static readonly Color MediumPurple = Color.FromRGB(147, 112, 219);
		/// <summary>
		/// The color MediumSeaGreen; RGB (60, 179, 113)
		/// </summary>
		public static readonly Color MediumSeaGreen = Color.FromRGB(60, 179, 113);
		/// <summary>
		/// The color MediumSlateBlue; RGB (123, 104, 238)
		/// </summary>
		public static readonly Color MediumSlateBlue = Color.FromRGB(123, 104, 238);
		/// <summary>
		/// The color MediumSpringGreen; RGB (0, 250, 154)
		/// </summary>
		public static readonly Color MediumSpringGreen = Color.FromRGB(0, 250, 154);
		/// <summary>
		/// The color MediumTurquoise; RGB (72, 209, 204)
		/// </summary>
		public static readonly Color MediumTurquoise = Color.FromRGB(72, 209, 204);
		/// <summary>
		/// The color MediumVioletRed; RGB (199, 21, 133)
		/// </summary>
		public static readonly Color MediumVioletRed = Color.FromRGB(199, 21, 133);
		/// <summary>
		/// The color MidnightBlue; RGB (25, 25, 112)
		/// </summary>
		public static readonly Color MidnightBlue = Color.FromRGB(25, 25, 112);
		/// <summary>
		/// The color MintCream; RGB (245, 255, 250)
		/// </summary>
		public static readonly Color MintCream = Color.FromRGB(245, 255, 250);
		/// <summary>
		/// The color MistyRose; RGB (255, 228, 225)
		/// </summary>
		public static readonly Color MistyRose = Color.FromRGB(255, 228, 225);
		/// <summary>
		/// The color Moccasin; RGB (255, 228, 181)
		/// </summary>
		public static readonly Color Moccasin = Color.FromRGB(255, 228, 181);
		/// <summary>
		/// The color NavajoWhite; RGB (255, 222, 173)
		/// </summary>
		public static readonly Color NavajoWhite = Color.FromRGB(255, 222, 173);
		/// <summary>
		/// The color Navy; RGB (0, 0, 128)
		/// </summary>
		public static readonly Color Navy = Color.FromRGB(0, 0, 128);
		/// <summary>
		/// The color OldLace; RGB (253, 245, 230)
		/// </summary>
		public static readonly Color OldLace = Color.FromRGB(253, 245, 230);
		/// <summary>
		/// The color Olive; RGB (128, 128, 0)
		/// </summary>
		public static readonly Color Olive = Color.FromRGB(128, 128, 0);
		/// <summary>
		/// The color OliveDrab; RGB (107, 142, 35)
		/// </summary>
		public static readonly Color OliveDrab = Color.FromRGB(107, 142, 35);
		/// <summary>
		/// The color Orange; RGB (255, 165, 0)
		/// </summary>
		public static readonly Color Orange = Color.FromRGB(255, 165, 0);
		/// <summary>
		/// The color OrangeRed; RGB (255, 69, 0)
		/// </summary>
		public static readonly Color OrangeRed = Color.FromRGB(255, 69, 0);
		/// <summary>
		/// The color Orchid; RGB (218, 112, 214)
		/// </summary>
		public static readonly Color Orchid = Color.FromRGB(218, 112, 214);
		/// <summary>
		/// The color PaleGoldenrod; RGB (238, 232, 170)
		/// </summary>
		public static readonly Color PaleGoldenrod = Color.FromRGB(238, 232, 170);
		/// <summary>
		/// The color PaleGreen; RGB (152, 251, 152)
		/// </summary>
		public static readonly Color PaleGreen = Color.FromRGB(152, 251, 152);
		/// <summary>
		/// The color PaleTurquoise; RGB (175, 238, 238)
		/// </summary>
		public static readonly Color PaleTurquoise = Color.FromRGB(175, 238, 238);
		/// <summary>
		/// The color PaleVioletRed; RGB (219, 112, 147)
		/// </summary>
		public static readonly Color PaleVioletRed = Color.FromRGB(219, 112, 147);
		/// <summary>
		/// The color PapayaWhip; RGB (255, 239, 213)
		/// </summary>
		public static readonly Color PapayaWhip = Color.FromRGB(255, 239, 213);
		/// <summary>
		/// The color PeachPuff; RGB (255, 218, 185)
		/// </summary>
		public static readonly Color PeachPuff = Color.FromRGB(255, 218, 185);
		/// <summary>
		/// The color Peru; RGB (205, 133, 63)
		/// </summary>
		public static readonly Color Peru = Color.FromRGB(205, 133, 63);
		/// <summary>
		/// The color Pink; RGB (255, 192, 203)
		/// </summary>
		public static readonly Color Pink = Color.FromRGB(255, 192, 203);
		/// <summary>
		/// The color Plum; RGB (221, 160, 221)
		/// </summary>
		public static readonly Color Plum = Color.FromRGB(221, 160, 221);
		/// <summary>
		/// The color PowderBlue; RGB (176, 224, 230)
		/// </summary>
		public static readonly Color PowderBlue = Color.FromRGB(176, 224, 230);
		/// <summary>
		/// The color Purple; RGB (128, 0, 128)
		/// </summary>
		public static readonly Color Purple = Color.FromRGB(128, 0, 128);
		/// <summary>
		/// The color Red; RGB (255, 0, 0)
		/// </summary>
		public static readonly Color Red = Color.FromRGB(255, 0, 0);
		/// <summary>
		/// The color RosyBrown; RGB (188, 143, 143)
		/// </summary>
		public static readonly Color RosyBrown = Color.FromRGB(188, 143, 143);
		/// <summary>
		/// The color RoyalBlue; RGB (65, 105, 225)
		/// </summary>
		public static readonly Color RoyalBlue = Color.FromRGB(65, 105, 225);
		/// <summary>
		/// The color SaddleBrown; RGB (139, 69, 19)
		/// </summary>
		public static readonly Color SaddleBrown = Color.FromRGB(139, 69, 19);
		/// <summary>
		/// The color Salmon; RGB (250, 128, 114)
		/// </summary>
		public static readonly Color Salmon = Color.FromRGB(250, 128, 114);
		/// <summary>
		/// The color SandyBrown; RGB (244, 164, 96)
		/// </summary>
		public static readonly Color SandyBrown = Color.FromRGB(244, 164, 96);
		/// <summary>
		/// The color SeaGreen; RGB (46, 139, 87)
		/// </summary>
		public static readonly Color SeaGreen = Color.FromRGB(46, 139, 87);
		/// <summary>
		/// The color SeaShell; RGB (255, 245, 238)
		/// </summary>
		public static readonly Color SeaShell = Color.FromRGB(255, 245, 238);
		/// <summary>
		/// The color Sienna; RGB (160, 82, 45)
		/// </summary>
		public static readonly Color Sienna = Color.FromRGB(160, 82, 45);
		/// <summary>
		/// The color Silver; RGB (192, 192, 192)
		/// </summary>
		public static readonly Color Silver = Color.FromRGB(192, 192, 192);
		/// <summary>
		/// The color SkyBlue; RGB (135, 206, 235)
		/// </summary>
		public static readonly Color SkyBlue = Color.FromRGB(135, 206, 235);
		/// <summary>
		/// The color SlateBlue; RGB (106, 90, 205)
		/// </summary>
		public static readonly Color SlateBlue = Color.FromRGB(106, 90, 205);
		/// <summary>
		/// The color SlateGray; RGB (112, 128, 144)
		/// </summary>
		public static readonly Color SlateGray = Color.FromRGB(112, 128, 144);
		/// <summary>
		/// The color Snow; RGB (255, 250, 250)
		/// </summary>
		public static readonly Color Snow = Color.FromRGB(255, 250, 250);
		/// <summary>
		/// The color SpringGreen; RGB (0, 255, 127)
		/// </summary>
		public static readonly Color SpringGreen = Color.FromRGB(0, 255, 127);
		/// <summary>
		/// The color SteelBlue; RGB (70, 130, 180)
		/// </summary>
		public static readonly Color SteelBlue = Color.FromRGB(70, 130, 180);
		/// <summary>
		/// The color Tan; RGB (210, 180, 140)
		/// </summary>
		public static readonly Color Tan = Color.FromRGB(210, 180, 140);
		/// <summary>
		/// The color Teal; RGB (0, 128, 128)
		/// </summary>
		public static readonly Color Teal = Color.FromRGB(0, 128, 128);
		/// <summary>
		/// The color Thistle; RGB (216, 191, 216)
		/// </summary>
		public static readonly Color Thistle = Color.FromRGB(216, 191, 216);
		/// <summary>
		/// The color Tomato; RGB (255, 99, 71)
		/// </summary>
		public static readonly Color Tomato = Color.FromRGB(255, 99, 71);
		/// <summary>
		/// The color Turquoise; RGB (64, 224, 208)
		/// </summary>
		public static readonly Color Turquoise = Color.FromRGB(64, 224, 208);
		/// <summary>
		/// The color Violet; RGB (238, 130, 238)
		/// </summary>
		public static readonly Color Violet = Color.FromRGB(238, 130, 238);
		/// <summary>
		/// The color Wheat; RGB (245, 222, 179)
		/// </summary>
		public static readonly Color Wheat = Color.FromRGB(245, 222, 179);
		/// <summary>
		/// The color White; RGB (255, 255, 255)
		/// </summary>
		public static readonly Color White = Color.FromRGB(255, 255, 255);
		/// <summary>
		/// The color WhiteSmoke; RGB (245, 245, 245)
		/// </summary>
		public static readonly Color WhiteSmoke = Color.FromRGB(245, 245, 245);
		/// <summary>
		/// The color Yellow; RGB (255, 255, 0)
		/// </summary>
		public static readonly Color Yellow = Color.FromRGB(255, 255, 0);
		/// <summary>
		/// The color YellowGreen; RGB (154, 205, 50)
		/// </summary>
		public static readonly Color YellowGreen = Color.FromRGB(154, 205, 50);
	}

 	/// <summary>
	/// A listing of the default TCOD preset colors.
	/// </summary>
    public static class TCODColorPresets
    {
        /// <summary>
        /// The color Black; RGB (0, 0, 0)
        /// </summary>
        public static readonly Color Black = Color.FromRGB(0, 0, 0);

        /// <summary>
        /// The color DarkGray; RGB (96, 96, 96)
        /// </summary>
        public static readonly Color DarkGray = Color.FromRGB(96, 96, 96);

        /// <summary>
        /// The color Gray; RGB (196, 196, 196)
        /// </summary>
        public static readonly Color Gray = Color.FromRGB(196, 196, 196);

        /// <summary>
        /// The color White; RGB (255, 255, 255)
        /// </summary>
        public static readonly Color White = Color.FromRGB(255, 255, 255);

        /// <summary>
        /// The color DarkBlue; RGB (40, 40, 128)
        /// </summary>
        public static readonly Color DarkBlue = Color.FromRGB(40, 40, 128);

        /// <summary>
        /// The color BrightBlue; RGB (120, 120, 255)
        /// </summary>
        public static readonly Color BrightBlue = Color.FromRGB(120, 120, 255);

        /// <summary>
        /// The color DarkRed; RGB (128, 0, 0)
        /// </summary>
        public static readonly Color DarkRed = Color.FromRGB(128, 0, 0);

        /// <summary>
        /// The color NormalRed; RGB (255, 0, 0)
        /// </summary>
        public static readonly Color Red = Color.FromRGB(255, 0, 0);

        /// <summary>
        /// The color BrightRed; RGB (255, 100, 50)
        /// </summary>
        public static readonly Color BrightRed = Color.FromRGB(255, 100, 50);

        /// <summary>
        /// The color Brown; RGB (32, 16, 0)
        /// </summary>
        public static readonly Color Brown = Color.FromRGB(32, 16, 0);

        /// <summary>
        /// The color BrightYellow; RGB (255, 255, 150)
        /// </summary>
        public static readonly Color BrightYellow = Color.FromRGB(255, 255, 150);

        /// <summary>
        /// The color Yellow; RGB (255, 255, 0)
        /// </summary>
        public static readonly Color Yellow = Color.FromRGB(255, 255, 0);

        /// <summary>
        /// The color DarkYellow; RGB (164, 164, 0)
        /// </summary>
        public static readonly Color DarkYellow = Color.FromRGB(164, 164, 0);

        /// <summary>
        /// The color BrightGreen; RGB (0, 255, 0)
        /// </summary>
        public static readonly Color BrightGreen = Color.FromRGB(0, 255, 0);

        /// <summary>
        /// The color Green; RGB (0, 220, 0)
        /// </summary>
        public static readonly Color Green = Color.FromRGB(0, 220, 0);

        /// <summary>
        /// The color DarkGreen; RGB (0, 128, 0)
        /// </summary>
        public static readonly Color DarkGreen = Color.FromRGB(0, 128, 0);

        /// <summary>
        /// The color Orange; RGB (255, 150, 0)
        /// </summary>
        public static readonly Color Orange = Color.FromRGB(255, 150, 0);

        /// <summary>
        /// The color Silver; RGB (203, 203, 203)
        /// </summary>
        public static readonly Color Silver = Color.FromRGB(203, 203, 203);

        /// <summary>
        /// The color Gold; RGB (255, 255, 102)
        /// </summary>
        public static readonly Color Gold = Color.FromRGB(255, 255, 102);

        /// <summary>
        /// The color Purple; RGB (204, 51, 153)
        /// </summary>
        public static readonly Color Purple = Color.FromRGB(204, 51, 153);

        /// <summary>
        /// The color DarkPurple; RGB (51, 0, 51)
        /// </summary>
        public static readonly Color DarkPurple = Color.FromRGB(51, 0, 51);
    }
}
