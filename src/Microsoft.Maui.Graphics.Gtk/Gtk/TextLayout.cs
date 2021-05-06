using System;
using Cairo;
using Microsoft.Maui.Graphics.Extras;

namespace Microsoft.Maui.Graphics.Native.Gtk {

	public class TextLayout : IDisposable {

		private Context _context;

		public TextLayout(Context context) {
			_context = context;
		}

		public Context Context => _context;

		public string FontFamily { get; set; }

		public Pango.Weight Weight { get; set; } = Pango.Weight.Normal;

		public Pango.Style Style { get; set; } = Pango.Style.Normal;

		public int PangoFontSize { get; set; } = -1;

		private Pango.Layout? _layout;
		private bool _layoutOwned = false;

		public TextFlow TextFlow { get; set; }

		public HorizontalAlignment HorizontalAlignment { get; set; }

		public VerticalAlignment VerticalAlignment { get; set; }

		public LineBreakMode LineBreakMode { get; set; } = LineBreakMode.EndTruncation;

		public Pango.Layout Layout {
			get {
				if (_layout == null) {
					_layout = Pango.CairoHelper.CreateLayout(Context);
					_layoutOwned = true;
				}

				if (_layout.FontDescription != FontDescription) {
					_layout.FontDescription = FontDescription;
				}

				// allign & justify per Size
				// _layout.Alignment = HorizontalAlignment.ToPango();
				// _layout.Justify = HorizontalAlignment.HasFlag(HorizontalAlignment.Justified);
				_layout.Wrap = LineBreakMode.ToPangoWrap();
				_layout.Ellipsize = LineBreakMode.ToPangoEllipsize();

				// _layout.SingleParagraphMode = true;
				return _layout;
			}
			set {
				_layout = value;
				_layoutOwned = false;
			}
		}

		private Pango.FontDescription? _fontDescription;
		private bool _fontDescriptionOwned = false;

		public Pango.FontDescription FontDescription {
			get {
				if (PangoFontSize == -1) {
					PangoFontSize = NativeFontService.Instance.SystemFontDescription.Size;
				}

				if (FontFamily == default) {
					FontFamily = NativeFontService.Instance.SystemFontDescription.Family;
				}

				if (_fontDescription == null) {
					_fontDescription = new Pango.FontDescription {
						Family = FontFamily,
						Weight = Weight,
						Style = Style,
						Size = PangoFontSize
					};

					_fontDescriptionOwned = true;

				}

				return _fontDescription;
			}
			set {
				_fontDescription = value;
				_fontDescriptionOwned = false;
			}
		}

		public Cairo.Color TextColor { get; set; }

		public void Dispose() {
			if (_fontDescriptionOwned) {
				_fontDescription?.Dispose();
			}

			if (_layoutOwned) {
				_layout?.Dispose();
			}
		}

		public (int width, int height) GetPixelSize(string text, int desiredWidth = -1) {

			if (desiredWidth > 0) {
				Layout.Width = desiredWidth;
			}

			Layout.SetText(text);
			Layout.GetPixelSize(out var textWidth, out var textHeight);

			return (textWidth, textHeight);
		}

		void Draw() {
			Context.SetSourceRGBA(TextColor.R, TextColor.G, TextColor.B, TextColor.A);
			Pango.CairoHelper.ShowLayout(Context, Layout);
		}

		float GetX(float x, int width) {
			if (HorizontalAlignment == HorizontalAlignment.Left)
				return x;

			if (HorizontalAlignment == HorizontalAlignment.Right)
				return x - width;

			if (HorizontalAlignment == HorizontalAlignment.Center)
				return x - width / 2;

			return x;
		}

		public void DrawString(string value, float x, float y) {

			Context.Save();

			Layout.SetText(value);
			Layout.GetPixelSize(out var textWidth, out var textHeight);

			if (Layout.IsWrapped || Layout.IsEllipsized) {
				Layout.Width = textWidth;
			}

			Context.MoveTo(GetX(x, textWidth), y - Layout.Baseline.ScaledFromPango());
			Draw();
			Context.Restore();

		}

	}

}