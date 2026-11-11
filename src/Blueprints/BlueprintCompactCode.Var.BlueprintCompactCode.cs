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
        static uint Var(BinaryReader reader)
        {
            uint result = 0;
            for (int shift = 0; shift < 35; shift += 7)
            {
                byte part = reader.ReadByte();
                if (shift == 28 && part > 15) throw new InvalidDataException("Invalid compact blueprint integer.");
                result |= (uint)(part & 127) << shift;
                if (part < 128) return result;
            }
            throw new InvalidDataException("Invalid compact blueprint integer.");
        }
    }
}
