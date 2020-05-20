#region Copyright information
// <copyright file="BitmapSourceTypeConverter.cs">
//     Licensed under Microsoft Public License (Ms-PL)
//     https://github.com/XAMLMarkupExtensions/WPFLocalizationExtension/blob/master/LICENSE
// </copyright>
// <author>Bernhard Millauer</author>
// <author>Uwe Mayer</author>
#endregion

namespace WPFLocalizeExtension.TypeConverters
{
    #region Usings
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Media.Imaging;
    #endregion

    /// <summary>
    /// A type converter class for Bitmap resources that are used in WPF.
    /// </summary>
    public class BitmapSourceTypeConverter : TypeConverter
    {
        /// <inheritdoc/>
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(Bitmap);
        }

        /// <inheritdoc/>
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return destinationType == typeof(Bitmap);
        }

        /// <inheritdoc/>
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (!(value is Bitmap bitmap))
                return null;

            var bmpPt = bitmap.GetHbitmap();

            // create the bitmapSource
            var bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                bmpPt,
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            // freeze the bitmap to avoid hooking events to the bitmap
            bitmapSource.Freeze();

            // free memory
            DeleteObject(bmpPt);

            // return bitmapSource
            return bitmapSource;
        }

        /// <summary>
        /// Frees memory of a pointer.
        /// </summary>
        /// <param name="o">Object to remove from memory.</param>
        /// <returns>0 if the removing was success, otherwise another number.</returns>
        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        private static extern int DeleteObject(IntPtr o);

        /// <inheritdoc/>
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            var source = value as BitmapSource;

            if (value == null)
                return null;

            var bmp = new Bitmap(
                source.PixelWidth,
                source.PixelHeight,
                PixelFormat.Format32bppPArgb);

            var data = bmp.LockBits(
                new Rectangle(System.Drawing.Point.Empty, bmp.Size),
                ImageLockMode.WriteOnly,
                PixelFormat.Format32bppPArgb);

            source.CopyPixels(
                Int32Rect.Empty,
                data.Scan0,
                data.Height * data.Stride,
                data.Stride);

            bmp.UnlockBits(data);

            return bmp;
        }
    }
}