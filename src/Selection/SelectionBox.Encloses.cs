using System;

namespace SharableBlueprints
{
    public readonly partial struct SelectionBox
    {
        // Game positions are the top-left corner, with positive width/downward height.
        public bool Encloses(int x, int y, int width, int height)
        {
            return HasArea && width > 0 && height > 0 && x >= Left && y <= Top
                && (long)x + width <= Right && (long)y - height >= Bottom;
        }
    }
}
