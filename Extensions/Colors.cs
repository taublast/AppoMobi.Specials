using System;
using System.Drawing;

namespace AppoMobi.Specials;

public static class ColorExtensions
{
	// Convert an HLS value into an RGB value.

	public static Color RgbFromHsl(double Hue, double Saturation, double Luminance)
	{
		var fChroma = (1.0 - Math.Abs(2.0 * Luminance - 1.0)) * Saturation;
		var fHue = Hue / 60.0;
		var fHueMod2 = fHue;
		while (fHueMod2 >= 2.0) fHueMod2 -= 2.0;
		var fTemp = fChroma * (1.0 - Math.Abs(fHueMod2 - 1.0));

		double fRed = 0, fGreen = 0, fBlue = 0;
		if (fHue < 1.0)
		{
			fRed = fChroma;
			fGreen = fTemp;
			fBlue = 0;
		}
		else if (fHue < 2.0)
		{
			fRed = fTemp;
			fGreen = fChroma;
			fBlue = 0;
		}
		else if (fHue < 3.0)
		{
			fRed = 0;
			fGreen = fChroma;
			fBlue = fTemp;
		}
		else if (fHue < 4.0)
		{
			fRed = 0;
			fGreen = fTemp;
			fBlue = fChroma;
		}
		else if (fHue < 5.0)
		{
			fRed = fTemp;
			fGreen = 0;
			fBlue = fChroma;
		}
		else if (fHue < 6.0)
		{
			fRed = fChroma;
			fGreen = 0;
			fBlue = fTemp;
		}
		else
		{
			fRed = 0;
			fGreen = 0;
			fBlue = 0;
		}

		var fMin = Luminance - 0.5 * fChroma;
		fRed += fMin;
		fGreen += fMin;
		fBlue += fMin;

		fRed *= 255.0;
		fGreen *= 255.0;
		fBlue *= 255.0;

		int iRed = 0, iGreen = 0, iBlue = 0;
		// the default seems to be to truncate rather than round
		iRed = Convert.ToInt32(Math.Truncate(fRed));
		iGreen = Convert.ToInt32(Math.Truncate(fGreen));
		iBlue = Convert.ToInt32(Math.Truncate(fBlue));
		if (iRed < 0) iRed = 0;
		if (iRed > 255) iRed = 255;
		if (iGreen < 0) iGreen = 0;
		if (iGreen > 255) iGreen = 255;
		if (iBlue < 0) iBlue = 0;
		if (iBlue > 255) iBlue = 255;

		return Color.FromArgb(iRed, iGreen, iBlue);
	}

	public static string HexFromHsl(double Hue, double Saturation, double Luminance)
	{
		var fChroma = (1.0 - Math.Abs(2.0 * Luminance - 1.0)) * Saturation;
		var fHue = Hue / 60.0;
		var fHueMod2 = fHue;
		while (fHueMod2 >= 2.0) fHueMod2 -= 2.0;
		var fTemp = fChroma * (1.0 - Math.Abs(fHueMod2 - 1.0));

		double fRed = 0, fGreen = 0, fBlue = 0;
		if (fHue < 1.0)
		{
			fRed = fChroma;
			fGreen = fTemp;
			fBlue = 0;
		}
		else if (fHue < 2.0)
		{
			fRed = fTemp;
			fGreen = fChroma;
			fBlue = 0;
		}
		else if (fHue < 3.0)
		{
			fRed = 0;
			fGreen = fChroma;
			fBlue = fTemp;
		}
		else if (fHue < 4.0)
		{
			fRed = 0;
			fGreen = fTemp;
			fBlue = fChroma;
		}
		else if (fHue < 5.0)
		{
			fRed = fTemp;
			fGreen = 0;
			fBlue = fChroma;
		}
		else if (fHue < 6.0)
		{
			fRed = fChroma;
			fGreen = 0;
			fBlue = fTemp;
		}
		else
		{
			fRed = 0;
			fGreen = 0;
			fBlue = 0;
		}

		var fMin = Luminance - 0.5 * fChroma;
		fRed += fMin;
		fGreen += fMin;
		fBlue += fMin;

		fRed *= 255.0;
		fGreen *= 255.0;
		fBlue *= 255.0;

		int iRed = 0, iGreen = 0, iBlue = 0;
		// the default seems to be to truncate rather than round
		iRed = Convert.ToInt32(Math.Truncate(fRed));
		iGreen = Convert.ToInt32(Math.Truncate(fGreen));
		iBlue = Convert.ToInt32(Math.Truncate(fBlue));
		if (iRed < 0) iRed = 0;
		if (iRed > 255) iRed = 255;
		if (iGreen < 0) iGreen = 0;
		if (iGreen > 255) iGreen = 255;
		if (iBlue < 0) iBlue = 0;
		if (iBlue > 255) iBlue = 255;

		return Color.FromArgb(iRed, iGreen, iBlue).ToHexString();
	}

	public static string ToHexString(this Color color)
	{
		var b = color.R;
		var str = b.ToString("X2");
		b = color.G;
		var str2 = b.ToString("X2");
		b = color.B;
		return "#" + str + str2 + b.ToString("X2");
	}


	public static void HlsToRgb(double h, double l, double s, out int r, out int g, out int b)
  {
		double p2;
		if (l <= 0.5) p2 = l * (1 + s);
		else p2 = l + s - l * s;

		var p1 = 2 * l - p2;
		double double_r, double_g, double_b;
		if (s == 0)
		{
			double_r = l;
			double_g = l;
			double_b = l;
		}
		else
		{
			double_r = QqhToRgb(p1, p2, h + 120);
			double_g = QqhToRgb(p1, p2, h);
			double_b = QqhToRgb(p1, p2, h - 120);
		}

		// Convert RGB to the 0 to 255 range.
		r = (int)(double_r * 255.0);
		g = (int)(double_g * 255.0);
		b = (int)(double_b * 255.0);
	}

	private static double QqhToRgb(double q1, double q2, double hue)
  {
		if (hue > 360) hue -= 360;
		else if (hue < 0) hue += 360;

		if (hue < 60) return q1 + (q2 - q1) * hue / 60;
		if (hue < 180) return q2;
		if (hue < 240) return q1 + (q2 - q1) * (240 - hue) / 60;
		return q1;
	}
	// Convert an RGB value into an HLS value.

	public static void RgbToHls(int r, int g, int b, out double h, out double l, out double s)
  {
		// Convert RGB to a 0.0 to 1.0 range.
		var double_r = r / 255.0;
		var double_g = g / 255.0;
		var double_b = b / 255.0;

		// Get the maximum and minimum RGB components.
		var max = double_r;
		if (max < double_g) max = double_g;
		if (max < double_b) max = double_b;

		var min = double_r;
		if (min > double_g) min = double_g;
		if (min > double_b) min = double_b;

		var diff = max - min;
		l = (max + min) / 2;
		if (Math.Abs(diff) < 0.00001)
		{
			s = 0;
			h = 0; // H is really undefined.
		}
		else
		{
			if (l <= 0.5) s = diff / (max + min);
			else s = diff / (2 - max - min);

			var r_dist = (max - double_r) / diff;
			var g_dist = (max - double_g) / diff;
			var b_dist = (max - double_b) / diff;

			if (double_r == max) h = b_dist - g_dist;
			else if (double_g == max) h = 2 + r_dist - b_dist;
			else h = 4 + g_dist - r_dist;

			h = h * 60;
			if (h < 0) h += 360;
		}
	}


	public static string MakeLighter(this string hex, int percent)
  {
		// strip the leading # if it's there
		hex = hex.Replace("#", "");

		var r = Convert.ToUInt16(hex.Substring(0, 2), 16);
		var g = Convert.ToUInt16(hex.Substring(2, 2), 16);
		var b = Convert.ToUInt16(hex.Substring(4, 2), 16);

		var rr = (0 | ((1 << 8) + r + (256 - r) * percent / 100)).ToString("X").Substring(1);
		var gg = (0 | ((1 << 8) + g + (256 - g) * percent / 100)).ToString("X").Substring(1);
		var bb = (0 | ((1 << 8) + b + (256 - b) * percent / 100)).ToString("X").Substring(1);

		return '#' + rr + gg + bb; //889fao
	}

	public static string MakeDarker(this string hex, int percent)
  {
		// strip the leading # if it's there
		percent = 100 - percent;
		hex = hex.Replace("#", "");

		var r = Convert.ToUInt16(hex.Substring(0, 2), 16);
		var g = Convert.ToUInt16(hex.Substring(2, 2), 16);
		var b = Convert.ToUInt16(hex.Substring(4, 2), 16);

		var rr = (r * percent / 100).ToString("X2");
		var gg = (g * percent / 100).ToString("X2");
		var bb = (b * percent / 100).ToString("X2");


		return '#' + rr + gg + bb;
		//(r * percent / 100).ToString("X") +
		//(g * percent / 100).ToString("X") +
		//(b * percent / 100).ToString("X");
	}
}