using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace HeatSource.View.SolutionAtrrBean
{
    // This is a special type converter which will be associated with the Employee class.
    // It converts an Employee object to string representation for use in a property grid.
    internal class BaseInfoConverter : ExpandableObjectConverter
    {
        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destType)
        {
            if (destType == typeof(string) && value is BaseInfo)
            {
                // Cast the value to an Employee type
                BaseInfo bi = (BaseInfo)value;
                // its top level value
                return bi.Location;
            }
            return base.ConvertTo(context, culture, value, destType);
        }
    }

    internal class CustomAttrCollectionConverter : ExpandableObjectConverter
    {
        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destType)
        {
            if (destType == typeof(string) && value is CustomAttrCollection)
            {
                return "fuck";
            }
            return base.ConvertTo(context, culture, value, destType);
        }
    }
}
