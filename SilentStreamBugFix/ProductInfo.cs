using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SilentStreamBugFix
{
    public class ProductInfo
    {
        public const string Name = "SilentStreamBugFix";
        public const string Version = "1.1.0";
        public const string FullName = Name + " v" + Version;
        public const string Company = "marcosbozzani";
        public const string Description = "Fixes no short sounds and cut off in the beginning of sounds when using HDMI audio";
        public static string Guid => "{04AD4EDC-3418-486E-AC09-AD91C6297141}";
    }
}
