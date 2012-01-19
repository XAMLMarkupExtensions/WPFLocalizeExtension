namespace WPFLocalizeExtension.Extensions
{
    #region Uses
    using System;
    using System.Windows.Markup;
    using WPFLocalizeExtension.BaseExtensions;
    using WPFLocalizeExtension.Engine;
    using System.Windows;
    using System.Reflection;
    using System.ComponentModel;
    using System.Linq;
    using System.Windows.Media.Imaging;
    using System.Drawing;
    using System.Drawing.Imaging;
    #endregion

    public static class RegisterMissingTypeConverters
    {
        private static bool registered = false;

        public static void Register()
        {
            if (registered)
                return;

            TypeDescriptor.AddAttributes(typeof(BitmapSource), new Attribute[] { new TypeConverterAttribute(typeof(BitmapSourceTypeConverter)) });

            registered = true;
        }
    }

    /// <summary>
    /// A type converter class for Bitmap resources that are used in WPF.
    /// </summary>
    public class BitmapSourceTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return ((sourceType != null) && (sourceType.Equals(typeof(System.Drawing.Bitmap))));
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return ((destinationType != null) && (destinationType.Equals(typeof(System.Drawing.Bitmap))));
        }

        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            Bitmap bitmap = value as Bitmap;

            if (bitmap == null)
                return null;

            IntPtr bmpPt = bitmap.GetHbitmap();

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

        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            BitmapSource source = value as BitmapSource;
            
            if (value == null)
                return null;

            Bitmap bmp = new Bitmap(
                source.PixelWidth,
                source.PixelHeight,
                PixelFormat.Format32bppPArgb);

            BitmapData data = bmp.LockBits(
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

    /// <summary>
    /// A generic <c>BaseLocalizeExtension</c>.
    /// </summary>
    [MarkupExtensionReturnType(typeof(object))]
    public class LocExtension : BaseLocalizeExtension<object>
	{
        #region Properties
        /// <summary>
        /// The target property type.
        /// </summary>
        public Type TargetPropertyType { get; private set; }

        /// <summary>
        /// The target property TypeConverter.
        /// </summary>
        public TypeConverter TargetPropertyTypeConverter { get; private set; } 
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="LocExtension"/> class.
        /// </summary>
        public LocExtension() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="LocExtension"/> class.
        /// </summary>
        /// <param name="key">The resource identifier.</param>
        public LocExtension(string key) : base(key) { } 
        #endregion

        protected override bool CanProvideValue(Type resourceType)
        {
            if (this.TargetPropertyType == null)
                return false;

            if (resourceType == null)
                return false;

            // Simplest case - direct assignment possible.
            if (this.TargetPropertyType.Equals(typeof(System.Object)))
                return true;

            // Simple case: The resource equals the target type as for string.
            if (resourceType.Equals(this.TargetPropertyType))
                return true;

            // Register missing type converters - this class will do this only once per appdomain.
            RegisterMissingTypeConverters.Register();

            // Take the default type converter.
            if (this.TargetPropertyTypeConverter == null)
            {
                // Get the type converter.
                this.TargetPropertyTypeConverter = TypeDescriptor.GetConverter(this.TargetPropertyType);
            }

            if (this.TargetPropertyTypeConverter == null)
                return false;

            return this.TargetPropertyTypeConverter.CanConvertFrom(resourceType);
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            object obj = base.ProvideValue(serviceProvider);

            var targetObject = GetTargetObject();
            var targetProperty = GetTargetProperty();

            if ((targetObject != null) && (targetProperty != null))
            {
                this.TargetPropertyType = null;

                if (targetProperty is DependencyProperty)
                {
                    this.TargetPropertyType = (targetProperty as DependencyProperty).PropertyType;
                }
                else if (targetProperty is PropertyInfo)
                {
                    this.TargetPropertyType = (targetProperty as PropertyInfo).PropertyType;
                }
                else
                    return null;

                if (this.TargetPropertyType.Equals(typeof(System.Windows.Media.ImageSource)))
                    this.TargetPropertyType = typeof(BitmapSource);
            }

            // TODO: Delete this for later extension of automatic suffix/prefix based on object and property names/IDs.
            if (obj == null)
                return null;

            if (this.CanProvideValue(obj.GetType()))
                return this.FormatOutput(obj);

            if ((this.TargetPropertyType != null) && (this.TargetPropertyType.IsValueType))
                return Activator.CreateInstance(this.TargetPropertyType);
            else
                return null;
        }

        protected override void HandleNewValue()
        {
            var obj = LocalizeDictionary.Instance.GetLocalizedObject<object>(this.Assembly, this.Dict, this.Key, this.GetForcedCultureOrDefault());

            if (obj == null)
                return;

            if (this.CanProvideValue(obj.GetType()))
                this.SetNewValue(this.FormatOutput(obj));
        }

        protected override object FormatOutput(object input)
        {           
            if (input == null)
                return null;

            if (this.TargetPropertyType.Equals(typeof(System.Object)) || input.GetType().Equals(this.TargetPropertyType))
                return input;

            if (this.TargetPropertyTypeConverter == null)
                return null;
            
            object result = null;

            try
            {
                result = this.TargetPropertyTypeConverter.ConvertFrom(input);
            }
            catch
            {
                result = Activator.CreateInstance(this.TargetPropertyType);
            }

            return result;
        }
    }
}