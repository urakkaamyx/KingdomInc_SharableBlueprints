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
        static int Count(BinaryReader reader) { uint n = Var(reader); if (n > 100000) throw new InvalidDataException("Compact blueprint list is too large."); return (int)n; }
    }
}
