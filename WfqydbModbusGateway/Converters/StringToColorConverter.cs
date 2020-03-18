using Binding.Converters;
using ConsoleFramework.Core;
using System;

namespace WfqydbModbusGateway.Converters
{
    public class StringToColorConverter : IBindingConverter
    {
        public Type FirstType => typeof(string);

        public Type SecondType => typeof(Color);

        public ConversionResult Convert(object first)
        {
            var ret = Color.Gray;

            if (Enum.TryParse(typeof(Color), (string)first, true, out object r))
                ret = (Color)r;

            return new ConversionResult(ret);
        }

        public ConversionResult ConvertBack(object second) =>
            new ConversionResult(second?.ToString() ?? nameof(Color.Black));
    }
}
