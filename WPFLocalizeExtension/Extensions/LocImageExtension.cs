using System;
using System.Windows;
using System.Windows.Markup;
using WPFLocalizeExtension.BaseExtensions;
using WPFLocalizeExtension.Engine;

namespace WPFLocalizeExtension.Extensions
{
    /// <summary>
    /// <c>BaseLocalizeExtension</c> for image objects
    /// </summary>
    [MarkupExtensionReturnType(typeof(System.Windows.Media.Imaging.BitmapSource))]
    public class LocImageExtension : BaseLocalizeExtension<System.Windows.Media.Imaging.BitmapSource>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LocImageExtension"/> class.
        /// </summary>
        public LocImageExtension() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="LocImageExtension"/> class.
        /// </summary>
        /// <param name="key">The resource identifier.</param>
        public LocImageExtension(string key) : base(key) { }

        /// <summary>
        /// Provides the Value for the first Binding as <see cref="System.Windows.Media.Imaging.BitmapSource"/>
        /// </summary>
        /// <param name="serviceProvider">
        /// The <see cref="System.Windows.Markup.IProvideValueTarget"/> provided from the <see cref="MarkupExtension"/>
        /// </param>
        /// <returns>The founded item from the .resx directory or null if not founded</returns>
        /// <exception cref="System.InvalidOperationException">
        /// thrown if <paramref name="serviceProvider"/> is not type of <see cref="System.Windows.Markup.IProvideValueTarget"/>
        /// </exception>
        /// <exception cref="System.NotSupportedException">
        /// thrown if the founded object is not type of <see cref="System.Drawing.Bitmap"/>
        /// </exception>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            object obj = base.ProvideValue(serviceProvider);
            
            if (obj == null)
            {
                return null;
            }

            if (this.IsTypeOf(obj.GetType(), typeof(BaseLocalizeExtension<>)))
            {
                return obj;
            }
            
            if (obj.GetType().Equals(typeof(System.Drawing.Bitmap)))
            {
                return this.FormatOutput(obj);
            }

            throw new NotSupportedException(
                string.Format(
                    "ResourceKey '{0}' returns '{1}' which is not type of System.Drawing.Bitmap",
                    this.Key,
                    obj.GetType().FullName));
        }

        /// <summary>
        /// see <c>BaseLocalizeExtension</c>
        /// </summary>
        protected override void HandleNewValue()
        {
            object obj = LocalizeDictionary.Instance.GetLocalizedObject<object>(this.Assembly, this.Dict, this.Key, this.GetForcedCultureOrDefault());
            this.SetNewValue(this.FormatOutput(obj));
        }

        /// <summary>
        /// Creates a <see cref="System.Windows.Media.Imaging.BitmapSource"/> from a <see cref="System.Drawing.Bitmap"/>.
        /// This extension does NOT support a DesignValue.
        /// </summary>
        /// <param name="input">The <see cref="System.Drawing.Bitmap"/> to convert</param>
        /// <returns>The converted <see cref="System.Windows.Media.Imaging.BitmapSource"/></returns>
        protected override object FormatOutput(object input)
        {
            // allocate the memory for the bitmap
            IntPtr bmpPt = ((System.Drawing.Bitmap)input).GetHbitmap();

            // create the bitmapSource
            System.Windows.Media.Imaging.BitmapSource bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                bmpPt,
                IntPtr.Zero,
                Int32Rect.Empty,
                System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());

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
    }
}