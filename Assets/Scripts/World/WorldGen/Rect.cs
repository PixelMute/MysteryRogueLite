using System;
using UnityEngine;

public class Rect
{
    public int Left { get; set; }
    public int Right { get; set; }
    public int Top { get; set; }
    public int Bottom { get; set; }

    public int Width { get { return Right - Left; } }
    public int Height { get { return Top - Bottom; } }

    /// <summary>
    /// Sets the rect to have the new bounds
    /// </summary>
    /// <param name="left">Coordinate of the left edge</param>
    /// <param name="right">Coordinate of the right edge</param>
    /// <param name="bottom">Coordinate of the bottom edge</param>
    /// <param name="top">Coordinate of the top edge</param>
    public void Set(int left, int right, int bottom, int top)
    {
        Left = left;
        Right = right;
        Top = top;
        Bottom = bottom;
    }

    /// <summary>
    /// Sets the bottom left corner to be the new x and y. Keeps the same width and height
    /// </summary>
    /// <param name="x">Bottom left x coor</param>
    /// <param name="y">Bottom left y coor</param>
    public void SetPosition(int x, int y)
    {
        Set(x, x + (Right - Left), y, y + (Top - Bottom));
    }

    /// <summary>
    /// Sets the center of this rect to be the new x and y. Keeps the same height and width
    /// </summary>
    /// <param name="x">X coodinate of the new center</param>
    /// <param name="y">Y coodinate of the new center</param>
    public void SetCenter(int x, int y)
    {
        Set(x - Width / 2, x + Width / 2, y - Height / 2, y + Height / 2);
    }

    /// <summary>
    /// Resizes the rect. Keeps the bottom left coordinate at the same location
    /// </summary>
    /// <param name="width">New width of the rectangle</param>
    /// <param name="height">New height of the rectangle</param>
    public void Resize(int width, int height)
    {
        Set(Left, Left + width, Bottom, Bottom + height);
    }

    /// <summary>
    /// Gets the center of this rectangle
    /// </summary>
    public Vector2 Center { get { return new Vector2((Left + Right) / 2f, (Top + Bottom) / 2f); } }

    /// <summary>
    /// Returns true if the rectangles are colliding. Touching is not considered colliding
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public bool IsColliding(Rect other)
    {
        return !(Math.Max(Left, other.Left) >= Math.Min(Right, other.Right) || Math.Max(Top, other.Top) >= Math.Min(Bottom, other.Bottom));
    }

    /// <summary>
    /// Returns true if this rectangle is completely inside the other rectangle
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public bool IsInside(Rect other)
    {
        return Left >= other.Left && Right <= other.Right && Top <= other.Top && Bottom >= other.Bottom;
    }

    /// <summary>
    /// Shifts the rectangle in the x dim by x and in the y dim by y
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    public void Shift(int x, int y)
    {
        Set(Left + x, Right + x, Bottom + y, Top + y);
    }

    /// <summary>
    /// Gets the rectangle that is the intersection between these rectangles
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public Rect Intersect(Rect other)
    {
        Rect result = new Rect();
        result.Left = Math.Max(Left, other.Left);
        result.Right = Math.Min(Right, other.Right);
        result.Top = Math.Max(Top, other.Top);
        result.Bottom = Math.Min(Bottom, other.Bottom);
        return result;
    }
}


