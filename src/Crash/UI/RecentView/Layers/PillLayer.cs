using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Eto.Drawing;
using Eto.Forms;

namespace Crash.UI.RecentView.Layers
{
	public class PillLayer
	{

		private const string httpRegex = "(http|https)://";
		private const string portRegex = ":[0-9]+";
		private const string pathRegex = "/[^/]+";
		internal static readonly string[] separator = new[] { "." };

		//private const string extensionRegex = "\\.[a-zA-Z]+";

		public static void RenderAddress(PaintEventArgs args, string text, Eto.Drawing.RectangleF container, bool includeCursor = false)
		{
			args.Graphics.SaveTransform();

			args.Graphics.TranslateTransform(container.Left, container.Top);

			var font = SystemFonts.Default(container.Height / 2f);
			var brush = new SolidBrush(Palette.White);

			float padding = container.Height * 0.10f;

			List<string> parts = new();
			if (!string.IsNullOrEmpty(text))
			{
				var fullSize = args.Graphics.MeasureString(font, text);
				var fullBox = new RectangleF(padding / 2f, padding, fullSize.Width + (padding * 2f), container.Height - (padding * 2f));
				args.Graphics.DrawText(font, brush, fullBox, text, alignment: FormattedTextAlignment.Center);
				return;

				var httpText = Regex.Match(text, httpRegex).Value;
				// var pathText = Regex.Match(text, pathRegex).Value;
				var portText = Regex.Match(text, portRegex).Value;
				var wwwText = Regex.Match(text, "www\\.").Value;

				var urlBits = text;
				if (!string.IsNullOrEmpty(httpText))
					urlBits = text.Replace(httpText, string.Empty);

				// if (!string.IsNullOrEmpty(pathText))
				//	urlBits = urlBits.Replace(pathText, string.Empty);

				if (!string.IsNullOrEmpty(portText))
					urlBits = urlBits.Replace(portText, string.Empty);

				if (!string.IsNullOrEmpty(wwwText))
					urlBits = urlBits.Replace(wwwText, string.Empty);

				if (!string.IsNullOrEmpty(urlBits))
				{
					parts = urlBits.Split(separator, StringSplitOptions.RemoveEmptyEntries)
					.Where(p => !string.IsNullOrEmpty(p))
					.Select(p => $".{p}")
					.ToList();
					parts[0] = parts[0].Substring(1);
				}

				if (!string.IsNullOrEmpty(httpText))
					parts.Insert(0, httpText.Replace("https:", "🔐 https:").Replace("http:", "🔓 http:"));

				if (!string.IsNullOrEmpty(portText))
					parts.Add(portText);

				// if (!string.IsNullOrEmpty(pathText))
				// 	parts.Add(pathText);


				foreach (var part in parts)
				{
					args.Graphics.TranslateTransform(padding, 0);

					var size = args.Graphics.MeasureString(font, part);

					var pillSize = new RectangleF(padding / 2f, padding, size.Width + (padding * 2f), container.Height - (padding * 2f));

					var roundedRect = GraphicsPath.GetRoundRect(pillSize, 6f);
					// args.Graphics.FillPath(Palette.Yellow, roundedRect);
					args.Graphics.DrawText(font, brush, pillSize, part, alignment: FormattedTextAlignment.Center);

					args.Graphics.TranslateTransform(pillSize.Width, 0);
				}
			}

			if (includeCursor)
			{
				if (parts.Count == 0)
				{
					var size = args.Graphics.MeasureString(font, " ");
					args.Graphics.TranslateTransform(size.Width + padding, 0);
				}

				// TODO : Animate
				args.Graphics.FillRectangle(Palette.Black, new RectangleF(0, padding, 4, container.Height - (padding * 2)));
			}

			args.Graphics.RestoreTransform();
		}

	}
}
