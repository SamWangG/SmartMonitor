using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.International.Converters.TraditionalChineseToSimplifiedConverter;
namespace MonitorCore
{
    class ChineseConvert
    {
        public static string SimplifiedToTraditional(string Text)
        {
            return ChineseConverter.Convert(Text, ChineseConversionDirection.SimplifiedToTraditional);
        }

        public static string TraditionalToSimplified(string Text)
        {
            return ChineseConverter.Convert(Text, ChineseConversionDirection.TraditionalToSimplified);
        }
    }
}
