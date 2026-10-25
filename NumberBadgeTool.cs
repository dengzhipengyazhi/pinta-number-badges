//
// NumberBadgeTool.cs
//
// PintaNumberBadges - a Pinta add-in that stamps numbered circular badges.
//
// Licensed under the MIT License, see README.md.
//
// The tool follows Pinta's standard tool pattern (see BaseTool):
//   * toolbar widgets are created in OnBuildToolBar()
//   * a live preview is painted into the document's scratch "Tool Layer"
//   * clicking bakes the badge into the current user layer and pushes a
//     SimpleHistoryItem so that Undo works
//

using System;
using Cairo;
using Gdk;
using Gtk;
using Pinta.Core;

namespace PintaNumberBadges;

public sealed class NumberBadgeTool : BaseTool
{
	private const string IconName = "tool-text-symbolic";

	private const string SettingNumber = "NumberBadges.Number";
	private const string SettingAutoIncrement = "NumberBadges.AutoIncrement";
	private const string SettingRadius = "NumberBadges.Radius";
	private const string SettingOutlineWidth = "NumberBadges.OutlineWidth";
	private const string SettingFilled = "NumberBadges.Filled";
	private const string SettingHandDrawn = "NumberBadges.HandDrawn";

	private const int DefaultRadius = 18;
	private const int DefaultOutlineWidth = 2;

	private readonly IPaletteService palette;

	private int number;
	private bool auto_increment;
	private int radius;
	private int outline_width;
	private bool filled;
	private bool hand_drawn;

	// Toolbar widgets (created lazily, once per toolbar build).
	private Gtk.SpinButton? number_spin;
	private Gtk.CheckButton? auto_increment_button;
	private Gtk.SpinButton? radius_spin;
	private Gtk.SpinButton? outline_spin;
	private Gtk.CheckButton? filled_button;
	private Gtk.CheckButton? hand_drawn_button;

	// Guards callbacks while we update widgets programmatically.
	private bool syncing_widgets;

	// Current preview on the tool layer.
	private PointD preview_center;
	private RectangleI? preview_bounds;

	public NumberBadgeTool (IServiceProvider services) : base (services)
	{
		palette = services.GetService<IPaletteService> ();

		// Radius / outline width are stored as ints on purpose:
		// Pinta's settings file only round-trips int, bool and string.
		number = Math.Clamp (Settings.GetSetting (SettingNumber, 1), 0, 9999);
		auto_increment = Settings.GetSetting (SettingAutoIncrement, true);
		radius = Math.Clamp (Settings.GetSetting (SettingRadius, DefaultRadius), 3, 1000);
		outline_width = Math.Clamp (Settings.GetSetting (SettingOutlineWidth, DefaultOutlineWidth), 0, 50);
		filled = Settings.GetSetting (SettingFilled, true);
		hand_drawn = Settings.GetSetting (SettingHandDrawn, false);
	}

	public override string Name
		=> Strings.Get ("Numbered Badge", "数字圆标");

	public override string Icon
		=> IconName;

	public override string StatusBarText
		=> Strings.Get (
			"Click to stamp a numbered badge. Left click uses the primary color, right click the secondary color. Turn off 'Fill' for a marker-style ring; 'Hand' makes the circle and number look hand-drawn.",
			"单击放置带编号的圆标：左键用主色，右键用副色。关闭「填充」得到马克笔圆圈；打开「手绘」让圆和数字呈手绘效果。");

	public override int Priority
		=> 55;

	#region Toolbar

	protected override void OnBuildToolBar (Gtk.Box toolbar)
	{
		toolbar.Append (CreateLabel (Strings.Get ("Number", "编号")));

		Gtk.SpinButton number_button = number_spin = GtkExtensions.CreateToolBarSpinButton (0, 9999, 1, number);
		number_button.Digits = 0;
		number_button.Numeric = true;
		number_button.TooltipText = Strings.Get (
			"Number that will be drawn inside the next badge",
			"下一个圆标中要绘制的编号");
		number_button.OnValueChanged += (_, _) => {
			if (syncing_widgets) return;
			number = Math.Clamp (number_button.GetValueAsInt (), 0, 9999);
			RefreshPreview ();
		};
		toolbar.Append (number_button);

		Gtk.CheckButton auto_button = auto_increment_button = Gtk.CheckButton.NewWithLabel (Strings.Get ("Auto", "自动递增"));
		auto_button.Active = auto_increment;
		auto_button.TooltipText = Strings.Get (
			"Increase the number automatically after each badge",
			"每放置一个圆标后自动把编号 +1");
		auto_button.OnToggled += (_, _) => {
			if (syncing_widgets) return;
			auto_increment = auto_button.Active;
		};
		toolbar.Append (auto_button);

		toolbar.Append (GtkExtensions.CreateToolBarSeparator ());

		toolbar.Append (CreateLabel (Strings.Get ("Radius", "半径")));

		Gtk.SpinButton radius_button = radius_spin = GtkExtensions.CreateToolBarSpinButton (3, 1000, 1, radius);
		radius_button.Digits = 0;
		radius_button.Numeric = true;
		radius_button.TooltipText = Strings.Get ("Badge radius in pixels", "圆标半径（像素）");
		radius_button.OnValueChanged += (_, _) => {
			if (syncing_widgets) return;
			radius = Math.Clamp (radius_button.GetValueAsInt (), 3, 1000);
			RefreshPreview ();
		};
		toolbar.Append (radius_button);

		toolbar.Append (CreateLabel (Strings.Get ("Width", "线宽")));

		Gtk.SpinButton outline_button = outline_spin = GtkExtensions.CreateToolBarSpinButton (0, 50, 1, outline_width);
		outline_button.Digits = 0;
		outline_button.Numeric = true;
		outline_button.TooltipText = Strings.Get (
			"Outline / ring width in pixels; 0 uses an automatic width when 'Fill' is off",
			"描边 / 圆圈线宽（像素）；关闭「填充」时填 0 会自动取合适线宽");
		outline_button.OnValueChanged += (_, _) => {
			if (syncing_widgets) return;
			outline_width = Math.Clamp (outline_button.GetValueAsInt (), 0, 50);
			RefreshPreview ();
		};
		toolbar.Append (outline_button);

		toolbar.Append (GtkExtensions.CreateToolBarSeparator ());

		Gtk.CheckButton fill_button = filled_button = Gtk.CheckButton.NewWithLabel (Strings.Get ("Fill", "填充"));
		fill_button.Active = filled;
		fill_button.TooltipText = Strings.Get (
			"Fill the circle with the palette color; turn off for a marker-style ring",
			"用调色板颜色填充圆圈；关闭后只画圆圈（内部透明）");
		fill_button.OnToggled += (_, _) => {
			if (syncing_widgets) return;
			filled = fill_button.Active;
			RefreshPreview ();
		};
		toolbar.Append (fill_button);

		Gtk.CheckButton hand_button = hand_drawn_button = Gtk.CheckButton.NewWithLabel (Strings.Get ("Hand", "手绘"));
		hand_button.Active = hand_drawn;
		hand_button.TooltipText = Strings.Get (
			"Draw a wobbly, hand-drawn circle with a handwriting-style number",
			"画略微不规则的手绘圆圈，并用系统手写字体绘制数字");
		hand_button.OnToggled += (_, _) => {
			if (syncing_widgets) return;
			hand_drawn = hand_button.Active;
			RefreshPreview ();
		};
		toolbar.Append (hand_button);
	}

	private static Gtk.Label CreateLabel (string text)
	{
		Gtk.Label label = Gtk.Label.New (text);
		label.AddCssClass (AdwaitaStyles.DimLabel);
		label.MarginStart = 6;
		label.MarginEnd = 6;
		return label;
	}

	#endregion

	#region Mouse / keyboard

	protected override void OnActivated (Document? document)
		=> ClearPreview (document);

	protected override void OnMouseDown (Document document, ToolMouseEventArgs e)
	{
		if (!document.Workspace.PointInCanvas (e.PointDouble))
			return;

		if (e.MouseButton != MouseButton.Left && e.MouseButton != MouseButton.Right)
			return;

		Color color = e.MouseButton == MouseButton.Right
			? palette.SecondaryColor
			: palette.PrimaryColor;

		// Snapshot the layer before modifying it, for undo support.
		ImageSurface old_surface = document.Layers.CurrentUserLayer.Surface.Clone ();

		ClearPreview (document);

		using (Context g = document.CreateClippedContext ()) {
			g.Antialias = Antialias.Gray;
			BadgeRenderer.Draw (
				g,
				e.PointDouble,
				number,
				radius,
				outline_width,
				color,
				filled,
				hand_drawn);
		}

		document.History.PushNewItem (
			new SimpleHistoryItem (Icon, Name, old_surface, document.Layers.CurrentUserLayerIndex));

		if (auto_increment)
			AdvanceNumber ();

		ShowPreview (document, e.PointDouble);
	}

	protected override void OnMouseMove (Document document, ToolMouseEventArgs e)
	{
		if (!document.Workspace.PointInCanvas (e.PointDouble)) {
			ClearPreview (document);
			return;
		}

		ShowPreview (document, e.PointDouble);
	}

	protected override bool OnKeyDown (Document document, ToolKeyEventArgs e)
	{
		switch (e.Key.Value) {
			case Gdk.Constants.KEY_bracketleft:
				SetRadius (Math.Max (3, radius - 2));
				return true;
			case Gdk.Constants.KEY_bracketright:
				SetRadius (Math.Min (1000, radius + 2));
				return true;
			default:
				return base.OnKeyDown (document, e);
		}
	}

	protected override void OnDeactivated (Document? document, BaseTool? newTool)
		=> ClearPreview (document);

	protected override void OnCommit (Document? document)
		=> ClearPreview (document);

	#endregion

	#region Settings

	protected override void OnSaveSettings (ISettingsService settings)
	{
		base.OnSaveSettings (settings);

		if (number_spin is not null)
			number = Math.Clamp (number_spin.GetValueAsInt (), 0, 9999);
		if (radius_spin is not null)
			radius = Math.Clamp (radius_spin.GetValueAsInt (), 3, 1000);
		if (outline_spin is not null)
			outline_width = Math.Clamp (outline_spin.GetValueAsInt (), 0, 50);
		if (auto_increment_button is not null)
			auto_increment = auto_increment_button.Active;
		if (filled_button is not null)
			filled = filled_button.Active;
		if (hand_drawn_button is not null)
			hand_drawn = hand_drawn_button.Active;

		settings.PutSetting (SettingNumber, number);
		settings.PutSetting (SettingAutoIncrement, auto_increment);
		settings.PutSetting (SettingRadius, radius);
		settings.PutSetting (SettingOutlineWidth, outline_width);
		settings.PutSetting (SettingFilled, filled);
		settings.PutSetting (SettingHandDrawn, hand_drawn);
	}

	#endregion

	#region Helpers

	private void AdvanceNumber ()
	{
		number = number >= 9999 ? 0 : number + 1;

		if (number_spin is null || Math.Abs (number_spin.Value - number) < 0.5)
			return;

		syncing_widgets = true;
		number_spin.Value = number;
		syncing_widgets = false;
	}

	private void SetRadius (int new_radius)
	{
		radius = Math.Clamp (new_radius, 3, 1000);

		if (radius_spin is not null) {
			syncing_widgets = true;
			radius_spin.Value = radius;
			syncing_widgets = false;
		}

		RefreshPreview ();
	}

	/// <summary>
	/// Clears the scratch tool layer and repaints the badge preview on it.
	/// The tool layer is only used for display; it is excluded when the image
	/// is flattened/saved (DocumentLayers.GetFlattenedImage).
	/// </summary>
	private void ShowPreview (Document document, PointD center)
	{
		Layer tool_layer = document.Layers.ToolLayer;

		tool_layer.Clear ();

		using (Context g = document.CreateClippedToolContext ()) {
			g.Antialias = Antialias.Gray;
			BadgeRenderer.Draw (
				g,
				center,
				number,
				radius,
				outline_width,
				palette.PrimaryColor,
				filled,
				hand_drawn);
		}

		tool_layer.Hidden = false;

		RectangleI new_bounds = document.ClampToImageSize (
			BadgeRenderer.GetBounds (center, radius, outline_width).ToInt ());

		if (preview_bounds is RectangleI old_bounds)
			document.Workspace.Invalidate (old_bounds);

		preview_center = center;
		preview_bounds = new_bounds;

		document.Workspace.Invalidate (new_bounds);
	}

	private void ClearPreview (Document? document)
	{
		if (document is null || preview_bounds is null)
			return;

		document.Layers.ToolLayer.Clear ();
		document.Layers.ToolLayer.Hidden = true;
		document.Workspace.Invalidate (preview_bounds.Value);
		preview_bounds = null;
	}

	private void RefreshPreview ()
	{
		if (preview_bounds is null)
			return;

		if (!PintaCore.Workspace.HasOpenDocuments)
			return;

		ShowPreview (PintaCore.Workspace.ActiveDocument, preview_center);
	}

	#endregion
}
