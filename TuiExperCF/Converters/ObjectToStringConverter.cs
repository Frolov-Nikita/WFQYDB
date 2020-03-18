using Binding.Converters;
using System;
using System.Collections.Generic;
using System.Text;

namespace TuiExperCF.Converters
{
    public class ObjectToStringConverter : IBindingConverter
    {
        public Type FirstType => typeof(object);

        public Type SecondType => typeof(string);

        public ConversionResult Convert(object first) => 
            new ConversionResult(first?.ToString()??"");

        public ConversionResult ConvertBack(object second)
        {
            throw new NotImplementedException();
        }
    }
}
