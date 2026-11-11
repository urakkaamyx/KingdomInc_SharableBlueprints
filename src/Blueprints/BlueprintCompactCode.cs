using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using LightJson;

namespace SharableBlueprints
{
    public static partial class BlueprintCompactCode
    {
        const int Max = 8 * 1024 * 1024;
        static readonly Encoding Utf8 = new UTF8Encoding(false, true);
        static int Gcd(int a, int b) { a = Math.Abs(a); b = Math.Abs(b); while (b != 0) { int r = a % b; a = b; b = r; } return a; }
    }
}
