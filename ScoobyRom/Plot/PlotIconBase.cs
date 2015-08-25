// PlotIconBase.cs: Common functionality for drawing icons using NPlot.

/* Copyright (C) 2011-2015 SubaruDieselCrew
 *
 * This file is part of ScoobyRom.
 *
 * ScoobyRom is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * ScoobyRom is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with ScoobyRom.  If not, see <http://www.gnu.org/licenses/>.
 */

// measured on Linux x64, mono, Release build 2015-08: ~600 ms vs. ~680 ms using MemoryStream
// --> raw conversion not worth the effort, also depends on internal bitmap data representation
// Small performance difference does not matter as background task can be used.
//#define BitmapToPixbufConversionRaw

using System.Drawing;
using NPlot;
using Gdk;

namespace ScoobyRom
{
	/// <summary>
	/// Common functionality for drawing icons using NPlot.
	/// Methods are not thread safe!
	/// </summary>
	public abstract class PlotIconBase
	{
		protected const int MemoryStreamCapacity = 2048;
		public const int Padding = 2;

		#if !BitmapToPixbufConversionRaw
		// for conversion purposes only: System.Drawing.Bitmap -> Gdk.Pixbuf

		protected System.IO.MemoryStream memoryStream;

		// Png uses transparent background; Bmp & Gif use black background; ImageFormat.Tiff adds unneeded Exif
		// MemoryBmp makes PNG on Linux!
		// should be fast as only used temporarily for conversion
		// Linux x64 tested 2015-08: ImageFormat.Bmp is fastest
		static readonly System.Drawing.Imaging.ImageFormat imageFormat = System.Drawing.Imaging.ImageFormat.Bmp;
		#endif

		protected int width, height, padding;
		protected System.Drawing.Rectangle bounds;
		protected Gdk.Pixbuf constDataIcon;

		// reuse objects where possible to improve performance
		protected readonly NPlot.PlotSurface2D plotSurface = new NPlot.PlotSurface2D ();
		protected readonly System.Drawing.Bitmap bitmap_cache;


		public PlotIconBase (int width, int height)
		{
			this.width = width;
			this.height = height;
			this.bounds = new System.Drawing.Rectangle (0, 0, width, height);

			// could also use pre-defined wrapper with internal bitmap: NPlot.Bitmap.PlotSurface2D
			this.bitmap_cache = new System.Drawing.Bitmap (width, height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

			// black/transparent (depending on image format) frame
			this.padding = Padding;
		}

		abstract public Gdk.Pixbuf CreateIcon (Subaru.Tables.Table table);

		// reuse icon, very useful for performance as many tables have const values
		public Gdk.Pixbuf ConstDataIcon {
			get {
				if (constDataIcon == null) {
					//					Gtk.Image image = new Gtk.Image ();
					//					missingDataPic = image.RenderIcon (Gtk.Stock.MissingImage, Gtk.IconSize.SmallToolbar, null);
					//					image.Dispose ();

					// bits per sample must be 8!
//					constDataIcon = new Gdk.Pixbuf (Gdk.Colorspace.Rgb, false, 8, width, height);
					// RGBA
//					constDataIcon.Fill (0xAAAAAAFF);

					using (var surface = new Cairo.ImageSurface (Cairo.Format.Rgb24, width, height)) {
						using (Cairo.Context cr = new Cairo.Context (surface)) {
							cr.SetSourceRGB (0.7, 0.7, 0.7);
							cr.Paint ();
							cr.SetSourceRGB (1, 0, 0);
							//cr.MoveTo (0.2 * width, 0.6 * height);
//							cr.ShowText ("const");
//							cr.Stroke ();

							using (var layout = Pango.CairoHelper.CreateLayout (cr)) {
								layout.FontDescription = Pango.FontDescription.FromString ("Sans 12");
								layout.SetText ("const");
								int lwidth, lheight;
								layout.GetPixelSize (out lwidth, out lheight);
								// 0, 0 = left top
								cr.MoveTo (0.5 * (width - lwidth), 0.5 * (height - lheight));
								Pango.CairoHelper.ShowLayout (cr, layout);
							}


						}
						constDataIcon = new Gdk.Pixbuf (surface.Data, Gdk.Colorspace.Rgb, true, 8, width, height, surface.Stride, null);
					}


				}
				return constDataIcon;
			}
		}

		// free some KiB depending on icon size and image format
		public void CleanupTemp ()
		{
			#if !BitmapToPixbufConversionRaw
			memoryStream.Dispose ();
			memoryStream = null;
			#endif
		}

		protected Gdk.Pixbuf DrawAndConvert ()
		{
			// Things like Padding needs to be set each time after Clear()
			plotSurface.Padding = padding;

			using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage (bitmap_cache)) {
				plotSurface.Draw (g, bounds);
			}

			// NPlot library uses System.Drawing (.NET Base Class Library) types
			// have to convert result (System.Drawing.Bitmap) to Gdk.Pixbuf for Gtk usage

			#if BitmapToPixbufConversionRaw
			return PixbufFromBitmap (bitmap_cache);
			#else
			if (memoryStream == null)
				memoryStream = new System.IO.MemoryStream (MemoryStreamCapacity);
			memoryStream.Position = 0;
			bitmap_cache.Save (memoryStream, imageFormat);
			memoryStream.Position = 0;
			return new Gdk.Pixbuf (memoryStream);
			#endif
		}

		#if BitmapToPixbufConversionRaw
		// working but sensitive to internal bitmap data format
		// http://mono.1490590.n4.nabble.com/Current-way-of-creating-a-Pixbuf-from-an-RGB-Array-td1545766.html
		// https://stackoverflow.com/questions/19187737/converting-a-bgr-bitmap-to-rgb
		public Gdk.Pixbuf PixbufFromBitmap (Bitmap bitmap)
		{
			int width = bitmap.Width;
			int height = bitmap.Height;

			System.Drawing.Imaging.BitmapData bitmapData = bitmap.LockBits (new System.Drawing.Rectangle (0, 0, width, height), System.Drawing.Imaging.ImageLockMode.ReadOnly,
				                                               System.Drawing.Imaging.PixelFormat.Format24bppRgb);
			int size = width * height * 3;
			byte[] rawData = new byte[size];
			System.Runtime.InteropServices.Marshal.Copy (bitmapData.Scan0, rawData, 0, size);
			bitmap.UnlockBits (bitmapData);

			// BGR (Microsoft's internal format) to RGB conversion necessary
			for (int i = 0; i < size; i += 3) {
				byte tmp = rawData [i];
				rawData [i] = rawData [i + 2];
				rawData [i + 2] = tmp;
			}

			Gdk.Pixbuf pixbuf = new Gdk.Pixbuf (rawData, Colorspace.Rgb, false, 8, width, height, bitmapData.Stride);
			return pixbuf;
		}

		#endif
	}
}