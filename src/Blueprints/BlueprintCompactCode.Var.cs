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
        static void Var(BinaryWriter writer, uint number)
        {
            while (number >= 128) { writer.Write((byte)(number | 128)); number >>= 7; }
            writer.Write((byte)number);
        }
    }
}
