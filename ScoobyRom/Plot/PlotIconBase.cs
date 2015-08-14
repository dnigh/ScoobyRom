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
		public const int DefaultWidth = 128;
		public const int DefaultHeight = 128;
		public const int Padding = 2;

		// ImageFormat:
		// Png uses transparent background; Bmp & Gif use black background; ImageFormat.Tiff adds unneeded Exif
		// MemoryBmp makes PNG on Linux!
		// should be fast as only used temporarily for conversion
		static readonly System.Drawing.Imaging.ImageFormat imageFormat = System.Drawing.Imaging.ImageFormat.Bmp;

		protected int width, height, padding;
		protected System.Drawing.Rectangle bounds;
		protected Gdk.Pixbuf constDataIcon;

		// reuse objects where possible to improve performance
		protected readonly NPlot.PlotSurface2D plotSurface = new NPlot.PlotSurface2D ();
		protected readonly System.Drawing.Bitmap bitmap_cache;
		protected System.IO.MemoryStream memoryStream;

		public PlotIconBase (int width, int height)
		{
			this.width = width;
			this.height = height;
			this.bounds = new System.Drawing.Rectangle (0, 0, width, height);

			// could also use pre-defined wrapper with internal bitmap: NPlot.Bitmap.PlotSurface2D
			this.bitmap_cache = new System.Drawing.Bitmap (width, height);

			// black/transparent (depending on image format) frame
			this.padding = Padding;
		}

		// reuse icon, very useful for performance as many tables have const values
		public Gdk.Pixbuf ConstDataIcon {
			get {
				if (constDataIcon == null) {
					//					Gtk.Image image = new Gtk.Image ();
					//					missingDataPic = image.RenderIcon (Gtk.Stock.MissingImage, Gtk.IconSize.SmallToolbar, null);
					//					image.Dispose ();

					// bits per sample must be 8!
					constDataIcon = new Gdk.Pixbuf (Gdk.Colorspace.Rgb, false, 8, width, height);
					// RGBA
					constDataIcon.Fill (0xAAAAAAFF);
				}
				return constDataIcon;
			}
		}

		// free some KiB depending on icon size and image format
		public void CleanupTemp ()
		{
			memoryStream.Dispose ();
			memoryStream = null;
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

			if (memoryStream == null)
				memoryStream = new System.IO.MemoryStream (MemoryStreamCapacity);
			memoryStream.Position = 0;
			bitmap_cache.Save (memoryStream, imageFormat);

			memoryStream.Position = 0;
			// TODO create Pixbuf directly from bitmap if possible, avoiding MemoryStream; no better solution found so far
			return new Gdk.Pixbuf (memoryStream);

			// return PixbufFromBitmap (bitmap_cache);
		}

		// working but sensitive to internal bitmap data formats
		// speed not tested yet, probably not worth using this vs. SaveTo+LoadFrom MemoryStream method
		// original source: http://mono.1490590.n4.nabble.com/Current-way-of-creating-a-Pixbuf-from-an-RGB-Array-td1545766.html
//		public Gdk.Pixbuf PixbufFromBitmap (Bitmap bitmap)
//		{
//			int width = bitmap.Width;
//			int height = bitmap.Height;
//
//			System.Drawing.Imaging.BitmapData bitmapData = bitmap.LockBits (new System.Drawing.Rectangle (0, 0, width, height), System.Drawing.Imaging.ImageLockMode.ReadOnly,
//				                                               System.Drawing.Imaging.PixelFormat.Format24bppRgb);
//			System.IntPtr scan = bitmapData.Scan0;
//			int size = width * height * 3;
//			byte[] pixbufData = new byte[size];
//
//			unsafe {
//				byte* p = (byte*)scan;
//				for (int y = 0; y < height; y++) {
//					for (int x = 0; x < width; x++) {
//						int i = (y * width + x) * 3;
//						pixbufData [i] = p [i + 2];
//						pixbufData [i + 1] = p [i + 1];
//						pixbufData [i + 2] = p [i];
//					}
//				}
//
//				// original: colors are wrong!
////				for (int i = 0; i < size; i++)
////					pixbufData [i] = p [i];
//			}
//
//			Gdk.Pixbuf pixbuf = new Gdk.Pixbuf (pixbufData, false, 8, width, height, bitmapData.Stride);
//			bitmap.UnlockBits (bitmapData);
//			return pixbuf;
//		}
	}
}