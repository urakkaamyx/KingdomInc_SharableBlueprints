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
        static int Signed(BinaryReader reader) { uint n = Var(reader); return unchecked((int)(n >> 1) ^ -((int)n & 1)); }
    }
}
