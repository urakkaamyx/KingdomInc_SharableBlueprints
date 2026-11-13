using KingdomFoundation.Api;
using System;

namespace SharableBlueprints
{
    public readonly struct SelectionBox
    {
        public readonly int Left, Bottom, Right, Top;
        public long Width => (long)Right - Left;
        public long Height => (long)Top - Bottom;
        public bool HasArea => Width >= 5 && Height >= 5;

        public SelectionBox(int x1, int y1, int x2, int y2)
        {
            Left = Math.Min(x1, x2);
            Right = Math.Max(x1, x2);
            Bottom = Math.Min(y1, y2);
            Top = Math.Max(y1, y2);
        }

        // Game positions are the top-left corner, with positive width/downward height.
        public bool Encloses(int x, int y, int width, int height)
        {
            return HasArea && width > 0 && height > 0 && x >= Left && y <= Top && (long)x + width <= Right && (long)y - height >= Bottom;
        }

        public bool EnclosesContainer(int x, int y, int width, int height)
        {
            // Containers have decorative borders. Allow a small margin without
            // capturing a much larger ancestor or a container merely crossed by the box.
            const int borderTolerance = 32; // Half of the game's 64-unit placement grid.
            return HasArea && width > 0 && height > 0 && x >= (long)Left - borderTolerance && y <= (long)Top + borderTolerance && (long)x + width <= (long)Right + borderTolerance && (long)y - height >= (long)Bottom - borderTolerance;
        }

        public SelectionBox IncludeFrame(int x, int y, int width, int height)
        {
            return new SelectionBox(Math.Min(Left, x), Math.Min(Bottom, checked(y - height)), Math.Max(Right, checked(x + width)), Math.Max(Top, y));
        }
    }
}
