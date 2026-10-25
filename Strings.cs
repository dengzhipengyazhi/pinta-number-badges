//
// Strings.cs
//
// PintaNumberBadges - a Pinta add-in that stamps numbered circular badges.
//
// Licensed under the MIT License, see README.md.
//

using System;
using System.Globalization;

namespace PintaNumberBadges;

/// <summary>
/// Minimal bilingual (English / Simplified Chinese) string helper.
/// Pinta's own translations live in Pinta's gettext domain, which an add-in
/// cannot reasonably extend, so the add-in ships its two most useful
/// languages directly. Add more languages here if needed.
/// </summary>
internal static class Strings
{
	private static readonly bool use_chinese =
		CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.Equals ("zh", StringComparison.OrdinalIgnoreCase);

	public static string Get (string english, string chinese)
		=> use_chinese ? chinese : english;
}
