using System;

namespace SharableBlueprints
{
    public readonly partial struct SelectionBox
    {
        public readonly int Left, Bottom, Right, Top;
        public long Width => (long)Right - Left;
        public long Height => (long)Top - Bottom;
        public bool HasArea => Width >= 5 && Height >= 5;
        public SelectionBox(int x1, int y1, int x2, int y2)
        {
            Left = Math.Min(x1, x2); Right = Math.Max(x1, x2);
            Bottom = Math.Min(y1, y2); Top = Math.Max(y1, y2);
        }
    }
}
