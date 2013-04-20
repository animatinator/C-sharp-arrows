using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using ArrowDataBinding.Arrows;

namespace ArrowDataBinding.Bindings
{
    public class ArrowValueConverter : IValueConverter
    {
        public IArrow arrow { get; set; }

        public ArrowValueConverter(IArrow arrow)
        {
            this.arrow = arrow;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // TODO: Type checking for ArrowValueConverter
            if (true)//(value is targetType?)
            {
                return arrow.Invoke(value);
            }

            throw new Exception("Wrong type");  // TODO: Proper exceptions for ArrowValueConverter
        }

        public virtual object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // TODO: Can't do converters for invertible arrows yet; need an invertible arrow interface
    //public class InvertibleArrowValueConverter<A, B> : ArrowValueConverter<A, B>
    //{
    //    private InvertibleArrow<A, B> arrow;

    //    public InvertibleArrowValueConverter(InvertibleArrow<A, B> arrow)
    //        : base(arrow)
    //    {
    //        this.arrow = arrow;
    //    }

    //    public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        if (value is B)
    //        {
    //            return arrow.Invert().Invoke((B)value);
    //        }

    //        else throw new Exception("Wrong type");  // TODO: Proper exceptions for InvertibleArrowValueConverter
    //    }
    //}
}
