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
        static int Unit(BinaryReader reader) { uint n = Var(reader); if (n == 0 || n > 20000000) throw new InvalidDataException("Invalid coordinate unit."); return (int)n; }
    }
}
