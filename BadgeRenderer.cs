//
// BadgeRenderer.cs
//
// PintaNumberBadges - a Pinta add-in that stamps numbered circular badges.
//
// Licensed under the MIT License, see README.md.
//

using System;
using System.Globalization;
using Cairo;
using Pango;
using Pinta.Core;
using Color = Cairo.Color;
using Context = Cairo.Context;

namespace PintaNumberBadges;

/// <summary>
/// Paints the badge itself: a circle (or hand-drawn ring) with a centered number.
/// Kept free of tool/GTK state so the same drawing code can be reused
/// (e.g. by a test harness that renders a sample image).
/// </summary>
public static class BadgeRenderer
{
	private const string FontFamily = "Sans";

	// The number should never fill the whole circle, otherwise multi-digit
	// numbers end up touching the outline.
	private const double MaxTextWidthFactor = 1.5;
	private const double MaxTextHeightFactor = 1.5;

	/// <summary>
	/// Draws a badge into <paramref name="g"/>.
	/// </summary>
	/// <param name="g">Target Cairo context (canvas coordinates).</param>
	/// <param name="center">Badge center in canvas coordinates.</param>
	/// <param name="number">Number rendered in the middle of the badge.</param>
	/// <param name="radius">Badge radius in pixels.</param>
	/// <param name="outlineWidth">Width of the ring/outline, 0 = automatic.</param>
	/// <param name="color">
	/// Palette color of the badge: the fill when <paramref name="filled"/> is true,
	/// otherwise the ring and number color (e.g. red marker).
	/// </param>
	/// <param name="filled">Fill the circle; false draws an outline-only ring.</param>
	/// <param name="handDrawn">Use a wobbly, hand-drawn ring and a handwriting font.</param>
	public static void Draw (
		Context g,
		PointD center,
		int number,
		double radius,
		double outlineWidth,
		Color color,
		bool filled = true,
		bool handDrawn = false)
	{
		int seed = HashCode.Combine (number, (int) Math.Round (radius * 100.0), handDrawn ? 1 : 0);

		if (filled) {
			Color contrast = GetContrastColor (color);

			// --- Filled shape
			BuildOutlinePath (g, center, radius, handDrawn, seed);
			g.Save ();
			g.SetSourceColor (color);
			g.Fill ();
			g.Restore ();

			// --- Optional outline ring in the contrasting color
			if (outlineWidth > 0) {
				BuildOutlinePath (g, center, radius, handDrawn, seed);
				g.Save ();
				g.SetSourceColor (contrast);
				g.LineWidth = outlineWidth;
				g.LineCap = LineCap.Round;
				g.LineJoin = LineJoin.Round;
				g.Stroke ();
				g.Restore ();
			}

			DrawNumber (g, center, number, radius, contrast, handDrawn, seed);
		} else {
			// --- Outline-only ring: interior stays transparent, like a marker circle
			double ring_width = outlineWidth > 0
				? outlineWidth
				: Math.Max (2, radius * 0.12);

			BuildOutlinePath (g, center, radius, handDrawn, seed);
			g.Save ();
			g.SetSourceColor (color);
			g.LineWidth = ring_width;
			g.LineCap = LineCap.Round;
			g.LineJoin = LineJoin.Round;
			g.Stroke ();
			g.Restore ();

			DrawNumber (g, center, number, radius, color, handDrawn, seed);
		}
	}

	/// <summary>
	/// Picks black or white depending on the fill color's brightness,
	/// so the number stays readable on any palette color.
	/// </summary>
	public static Color GetContrastColor (Color fill)
	{
		double luminance = 0.2126 * fill.R + 0.7152 * fill.G + 0.0722 * fill.B;
		return luminance > 0.55
			? new Color (0, 0, 0, 1)
			: new Color (1, 1, 1, 1);
	}

	/// <summary>
	/// Returns the area a badge occupies, in canvas coordinates.
	/// Used for invalidating only the modified part of the canvas.
	/// </summary>
	public static RectangleD GetBounds (PointD center, double radius, double outlineWidth)
	{
		// Generous bounds: covers the hand-drawn wobble and the automatic ring width.
		double half = radius * 1.15 + Math.Max (Math.Max (0, outlineWidth), radius * 0.12) + 3;
		return new RectangleD (center.X - half, center.Y - half, 2 * half, 2 * half);
	}

	/// <summary>
	/// Human readable description of the badge center for tooltips.
	/// </summary>
	public static string DescribeCenter (PointD center)
		=> string.Create (
			CultureInfo.InvariantCulture,
			$"({center.X:0}, {center.Y:0})");

	#region Path building

	private static void BuildOutlinePath (Context g, PointD center, double radius, bool handDrawn, int seed)
	{
		if (!handDrawn) {
			g.NewPath ();
			g.Arc (center.X, center.Y, radius, 0, Math.Tau);
			return;
		}

		AppendHandDrawnPath (g, center, radius, new Random (seed));
	}

	/// <summary>
	/// Builds a closed, slightly irregular path that looks penciled by hand:
	/// jittered radius, a slight ellipse/rotation, and a stroke that overlaps
	/// its own start point instead of closing perfectly.
	/// </summary>
	private static void AppendHandDrawnPath (Context g, PointD center, double radius, Random rng)
	{
		const int segments = 13;

		double start_angle = rng.NextDouble () * Math.Tau;
		double sweep = Math.Tau + 0.14 + rng.NextDouble () * 0.20;
		double y_scale = 0.92 + rng.NextDouble () * 0.12;
		double rotation = (rng.NextDouble () - 0.5) * 0.16;
		double cos_rot = Math.Cos (rotation);
		double sin_rot = Math.Sin (rotation);

		PointD[] points = new PointD[segments + 1];

		for (int i = 0; i <= segments; ++i) {
			double t = (double) i / segments;
			double angle = start_angle + t * sweep;
			double r = radius * (1.0 + (rng.NextDouble () - 0.5) * 0.10);
			double x = Math.Cos (angle) * r;
			double y = Math.Sin (angle) * r * y_scale;

			points[i] = new PointD (
				center.X + x * cos_rot - y * sin_rot,
				center.Y + x * sin_rot + y * cos_rot);
		}

		g.NewPath ();
		g.MoveTo (points[0].X, points[0].Y);

		// Catmull-Rom spline converted to cubic Bezier segments.
		for (int i = 0; i < segments; ++i) {
			PointD p0 = i == 0 ? points[0] : points[i - 1];
			PointD p1 = points[i];
			PointD p2 = points[i + 1];
			PointD p3 = i + 2 <= segments ? points[i + 2] : points[segments];

			PointD c1 = new (
				p1.X + (p2.X - p0.X) / 6.0,
				p1.Y + (p2.Y - p0.Y) / 6.0);
			PointD c2 = new (
				p2.X - (p3.X - p1.X) / 6.0,
				p2.Y - (p3.Y - p1.Y) / 6.0);

			g.CurveTo (c1.X, c1.Y, c2.X, c2.Y, p2.X, p2.Y);
		}
	}

	#endregion

	#region Number

	private static void DrawNumber (Context g, PointD center, int number, double radius, Color color, bool handDrawn, int seed)
	{
		string text = number.ToString (CultureInfo.InvariantCulture);

		// Create the layout from the Cairo context: this keeps the renderer
		// independent from GTK widgets while still using Pinta's Pango stack.
		using Layout layout = PangoCairo.Functions.CreateLayout (g);

		string family = handDrawn ? GetHandwritingFamily () : FontFamily;
		Pango.Weight weight = handDrawn ? Weight.Normal : Weight.Bold;

		double font_size = Math.Clamp (radius * (handDrawn ? 1.35 : 1.25), 6, 2000);
		SetFont (layout, text, family, weight, font_size);
		layout.GetPixelExtents (out RectangleI ink, out _);

		double max_width = radius * MaxTextWidthFactor;
		double max_height = radius * MaxTextHeightFactor;

		if (ink.Width > max_width || ink.Height > max_height) {
			double scale = Math.Min (
				max_width / Math.Max (1, ink.Width),
				max_height / Math.Max (1, ink.Height));

			font_size = Math.Max (6, font_size * scale * 0.98);
			SetFont (layout, text, family, weight, font_size);
			layout.GetPixelExtents (out ink, out _);
		}

		g.Save ();
		g.SetSourceColor (color);

		if (handDrawn) {
			// A tiny rotation makes the number look placed by hand.
			double angle = (new Random (seed ^ 0x5EED).NextDouble () - 0.5) * 0.14;
			g.Translate (center.X, center.Y);
			g.Rotate (angle);
			g.MoveTo (-(ink.Width / 2.0) - ink.X, -(ink.Height / 2.0) - ink.Y);
		} else {
			g.MoveTo (
				center.X - (ink.Width / 2.0) - ink.X,
				center.Y - (ink.Height / 2.0) - ink.Y);
		}

		PangoCairo.Functions.ShowLayout (g, layout);
		g.Restore ();
	}

	private static void SetFont (Layout layout, string text, string family, Weight weight, double font_size)
	{
		string size = font_size.ToString ("0.##", CultureInfo.InvariantCulture);
		string weight_name = weight == Weight.Bold ? "Bold" : "Normal";

		layout.SetFontDescription (
			Pango.FontDescription.FromString ($"{family} {weight_name} {size}"));
		layout.SetText (text, -1);
	}

	/// <summary>
	/// A handwriting-ish font that is normally present on each platform.
	/// If the font is missing, Pango falls back to the default sans font.
	/// </summary>
	private static string GetHandwritingFamily ()
	{
		if (OperatingSystem.IsWindows ())
			return "Segoe Print";

		if (OperatingSystem.IsMacOS ())
			return "Bradley Hand";

		// Many Linux distributions ship Comic Sans MS through the MS core fonts;
		// otherwise Pango falls back to Sans.
		return "Comic Sans MS";
	}

	#endregion
}
