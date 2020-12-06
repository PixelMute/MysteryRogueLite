using System;
using System.Collections.Generic;
using UnityEngine;

public class RogueRect
{
    public int Left { get; set; }
    public int Right { get; set; }
    public int Top { get; set; }
    public int Bottom { get; set; }

    public int Width { get { return Right - Left; } }
    public int Height { get { return Top - Bottom; } }

    public RogueRect() { }
    public RogueRect(Vector2Int point, Vector2Int otherPoint)
    {
        Left = Math.Min(otherPoint.x, point.x);
        Right = Math.Max(otherPoint.x, point.x);
        Top = Math.Max(otherPoint.y, point.y);
        Bottom = Math.Min(otherPoint.y, point.y);
    }

    public RogueRect(RogueRect other)
    {
        Left = other.Left;
        Right = other.Right;
        Top = other.Top;
        Bottom = other.Bottom;
    }

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
        Set(x - Width / 2, x + Width / 2 + (Width % 2 == 0 ? 0 : 1), y - Height / 2, y + Height / 2 + (Height % 2 == 0 ? 0 : 1));
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
    public bool IsColliding(RogueRect other)
    {
        return !(Math.Max(Left, other.Left) >= Math.Min(Right, other.Right) || Math.Min(Top, other.Top) <= Math.Max(Bottom, other.Bottom));
    }

    /// <summary>
    /// Returns true if this rectangle is completely inside the other rectangle
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public bool IsInside(RogueRect other)
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
    public RogueRect Intersect(RogueRect other)
    {
        var result = new RogueRect();
        result.Left = Math.Max(Left, other.Left);
        result.Right = Math.Min(Right, other.Right);
        result.Top = Math.Min(Top, other.Top);
        result.Bottom = Math.Max(Bottom, other.Bottom);
        if (result.Width < 0 || result.Height < 0)
        {
            return null;
        }
        return result;
    }

    /// <summary>
    /// Gets all the points of this rectangle
    /// </summary>
    /// <returns></returns>
    public List<Vector2Int> GetPoints()
    {
        var res = new List<Vector2Int>();
        for (int i = Left; i <= Right; i++)
        {
            for (int j = Bottom; j <= Top; j++)
            {
                res.Add(new Vector2Int(i, j));
            }
        }
        return res;
    }
}


