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
        static void Signed(BinaryWriter writer, int n) => Var(writer, unchecked((uint)((n << 1) ^ (n >> 31))));
    }
}
