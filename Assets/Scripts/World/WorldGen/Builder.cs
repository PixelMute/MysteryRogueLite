using System;
using System.Collections.Generic;
using UnityEngine;

public class Builder
{
    private Room entrance;
    private Room exit;

    private List<Room> singleConnections;
    private List<Room> multiConnnections;

    public List<Room> Rooms { get; set; }
    private List<Room> placedRooms;

    private enum Direction
    {
        LEFT,
        TOP,
        RIGHT,
        BOTTOM
    }

    public bool Build(List<Room> rooms)
    {
        this.Rooms = rooms;
        InitBuilder();

        if (entrance == null || exit == null)
        {
            return false;
        }

        float direction = Random.RandomDirection();

        entrance.SetSize();
        entrance.Bounds.SetPosition(0, 0);

        int roomsOnPath = multiConnnections.Count;
        float pathVariance = 15;

        var curr = entrance;
        float res;
        for (int i = 0; i < roomsOnPath; i++)
        {
            var connectionRoom = new ConnectionRoom();
            res = AttemptToPlaceRoom(curr, connectionRoom, direction + Random.Range(-pathVariance, pathVariance));
            if (res != -1)
            {
                rooms.Add(connectionRoom);
                placedRooms.Add(connectionRoom);
                curr = connectionRoom;
            }
            var next = multiConnnections[i];
            res = AttemptToPlaceRoom(curr, next, direction + Random.Range(-pathVariance, pathVariance));
            if (res != -1)
            {
                placedRooms.Add(next);
                curr = next;
            }
        }
        var next = exit;
        res = AttemptToPlaceRoom(curr, next, direction + Random.Range(-pathVariance, pathVariance));
        if (res != -1)
        {
            placedRooms.Add(next);
        }

        FindNeighbours();

        return true;
    }

    protected void FindNeighbours()
    {
        for (int i = 0; i < Rooms.Count - 1; i++)
        {
            for (int j = i + 1; j < Rooms.Count; j++)
            {
                Rooms[i].Connect(Rooms[j]);
            }
        }
    }

    private float AttemptToPlaceRoom(Room previousRoom, Room nextRoom, float angle)
    {
        angle %= 360f;
        if (angle < 0)
            angle += 360;

        var prevCenter = previousRoom.Bounds.Center;

        var m = (float)Math.Tan(angle.DegreesToRadians());
        var b = prevCenter.y - m * prevCenter.x;

        var direction = GetEdgeThatWillConnect(previousRoom.Bounds, m, b);
        var start = IntersectRectAndLine(previousRoom.Bounds, m, b, direction);
        var freeSpace = FindFreeSpace(start, Math.Max(nextRoom.Info.MaxWidth, nextRoom.Info.MaxHeight));
        if (freeSpace == null || !nextRoom.SetSizeWithLimit(freeSpace.Width + 1, freeSpace.Height + 1))
        {
            //If there is no free space or the free space is too small for the room
            return -1;
        }
        var idealCenter = GetIdealCenter(previousRoom.Bounds, nextRoom.Bounds, m, b, direction);
        nextRoom.Bounds.SetCenter(idealCenter.x, idealCenter.y);

        //perform connection bounds and target checking, move the room if necessary
        if (direction == Direction.TOP || direction == Direction.BOTTOM)
        {
            if (nextRoom.Bounds.Right < previousRoom.Bounds.Left + 2) nextRoom.Bounds.Shift(previousRoom.Bounds.Left + 2 - nextRoom.Bounds.Right, 0);
            else if (nextRoom.Bounds.Left > previousRoom.Bounds.Right - 2) nextRoom.Bounds.Shift(previousRoom.Bounds.Right - 2 - nextRoom.Bounds.Left, 0);

            if (nextRoom.Bounds.Right > freeSpace.Right) nextRoom.Bounds.Shift(freeSpace.Right - nextRoom.Bounds.Right, 0);
            else if (nextRoom.Bounds.Left < freeSpace.Left) nextRoom.Bounds.Shift(freeSpace.Left - nextRoom.Bounds.Left, 0);
        }
        else
        {
            if (nextRoom.Bounds.Bottom < previousRoom.Bounds.Top + 2) nextRoom.Bounds.Shift(0, previousRoom.Bounds.Top + 2 - nextRoom.Bounds.Bottom);
            else if (nextRoom.Bounds.Top > previousRoom.Bounds.Bottom - 2) nextRoom.Bounds.Shift(0, previousRoom.Bounds.Bottom - 2 - nextRoom.Bounds.Top);

            if (nextRoom.Bounds.Bottom > freeSpace.Bottom) nextRoom.Bounds.Shift(0, freeSpace.Bottom - nextRoom.Bounds.Bottom);
            else if (nextRoom.Bounds.Top < freeSpace.Top) nextRoom.Bounds.Shift(0, freeSpace.Top - nextRoom.Bounds.Top);
        }
        if (!nextRoom.Bounds.IsInside(freeSpace))
        {
            //Something went wrong
            return -1;
        }

        if (nextRoom.Connect(previousRoom))
        {
            return GetAngleBetweenRooms(nextRoom, previousRoom);
        }
        return -1;
    }

    private float GetAngleBetweenRooms(Room one, Room two)
    {
        var centerOne = one.Bounds.Center;
        var centerTwo = two.Bounds.Center;
        float xDiff = centerOne.x - centerTwo.x;
        float yDiff = centerOne.y - centerTwo.y;
        return ((float)Math.Atan2(yDiff, xDiff)).RadiansToDegrees();
    }

    private Direction GetEdgeThatWillConnect(Rect rectangle, float m, float b)
    {
        var width = (float)rectangle.Width;
        var height = (float)rectangle.Height;
        if (-height / 2f <= m * width / 2 && m * width / 2 <= height)
        {
            if (m <= 90 || m >= 270)
            {
                return Direction.RIGHT;
            }
            return Direction.LEFT;
        }
        else
        {
            if (m > 180)
            {
                return Direction.TOP;
            }
            return Direction.BOTTOM;
        }
    }

    private Vector2Int GetIdealCenter(Rect prev, Rect next, float m, float b, Direction direction)
    {
        //find the ideal center for this new room using the line equation and known dimensions
        var targetCenter = new Vector2Int();
        if (direction == Direction.RIGHT)
        {
            targetCenter.x = prev.Right + (next.Width - 1) / 2;
            targetCenter.y = (int)(m * targetCenter.x + b);
        }
        else if (direction == Direction.LEFT)
        {
            targetCenter.x = prev.Left - (next.Width - 1) / 2;
            targetCenter.y = (int)(m * targetCenter.x + b);
        }
        else if (direction == Direction.TOP)
        {
            targetCenter.y = prev.Top - (next.Height - 1) / 2;
            targetCenter.x = (int)((targetCenter.y - b) / m);
        }
        else
        {
            targetCenter.y = prev.Bottom + (next.Height - 1) / 2;
            targetCenter.x = (int)((targetCenter.y - b) / m);
        }
        var width = (float)prev.Width;
        var height = (float)prev.Height;
        return targetCenter;
    }

    private void InitBuilder()
    {
        singleConnections = new List<Room>();
        multiConnnections = new List<Room>();
        placedRooms = new List<Room>();

        foreach (var room in Rooms)
        {
            room.Reset();
            if (room.Info.RoomType == RoomType.ENTRANCE)
            {
                entrance = room;
            }
            else if (room.Info.RoomType == RoomType.EXIT)
            {
                exit = room;
            }
            else if (room.Info.MaxConnections > 1)
            {
                multiConnnections.Add(room);
            }
            else if (room.Info.MaxConnections == 1)
            {
                singleConnections.Add(room);
            }
        }
    }

    private Vector2Int IntersectRectAndLine(Rect rectangle, float m, float b, Direction direction)
    {
        if (direction == Direction.RIGHT)
        {
            return new Vector2Int(rectangle.Right, (int)Math.Round(m * rectangle.Right + b));
        }
        else if (direction == Direction.LEFT)
        {
            return new Vector2Int(rectangle.Left, (int)Math.Round(m * rectangle.Left + b));
        }
        else if (direction == Direction.TOP)
        {
            return new Vector2Int((int)Math.Round((rectangle.Top - b) / m), rectangle.Top);
        }
        else
        {
            return new Vector2Int((int)Math.Round((rectangle.Bottom - b) / m), rectangle.Bottom);
        }
    }

    private Rect FindFreeSpace(Vector2Int start, int maxSize)
    {
        var space = new Rect();
        space.Set(start.x - maxSize, start.x + maxSize, start.y - maxSize, start.y + maxSize);

        var collisions = new List<Room>(placedRooms);
        do
        {
            //Remove all rooms that we aren't currently colliding with
            collisions.RemoveAll(room => !room.Bounds.IsColliding(space));

            if (collisions.Count != 0)
            {
                var closestRoom = GetClosestRoom(collisions, start);
                if (closestRoom == null)
                {
                    return null;
                }
                //We multiply by height and width here in order to take into effect the current size when determing what way to cut off the rect
                int wDiff = int.MaxValue;
                var closestRect = closestRoom.Bounds;
                if (closestRect.Left >= start.x)
                {
                    wDiff = (space.Right - closestRect.Left) * (space.Height + 1);
                }
                else if (closestRect.Right <= start.x)
                {
                    wDiff = (closestRect.Right - space.Left) * (space.Height + 1);
                }

                var hDiff = int.MaxValue;
                if (closestRect.Top >= start.y)
                {
                    hDiff = (space.Bottom - closestRect.Top) * (space.Width + 1);
                }
                else if (closestRect.Bottom <= start.y)
                {
                    hDiff = (closestRect.Bottom - space.Top) * (space.Width + 1);
                }

                //reduce by as little as possible to resolve the collision
                if (wDiff < hDiff || (wDiff == hDiff && Random.Range(0, 2) == 0))
                {
                    if (closestRect.Left >= start.x && closestRect.Left < space.Right) space.Right = closestRect.Left;
                    if (closestRect.Right <= start.x && closestRect.Right > space.Left) space.Left = closestRect.Right;
                }
                else
                {
                    if (closestRect.Top >= start.y && closestRect.Top < space.Bottom) space.Bottom = closestRect.Top;
                    if (closestRect.Bottom <= start.y && closestRect.Bottom > space.Top) space.Top = closestRect.Bottom;
                }
                collisions.Remove(closestRoom);
            }

        } while (collisions.Count != 0);
        return space;
    }

    /// <summary>
    /// Finds the closest room to the start point. Returns null if there are no rooms or if the start point is inside a room
    /// </summary>
    /// <param name="rooms"></param>
    /// <param name="start"></param>
    /// <returns></returns>
    private Room GetClosestRoom(List<Room> rooms, Vector2Int start)
    {
        var inside = true;
        int closestDiff = int.MaxValue;
        Room closestRoom = null;
        foreach (var room in rooms)
        {
            var rect = room.Bounds;
            var curDiff = 0;
            if (start.x <= rect.Left)
            {
                inside = false;
                curDiff += rect.Left - start.x;
            }
            else if (start.x >= rect.Right)
            {
                inside = false;
                curDiff += start.x - rect.Right;
            }

            if (start.y <= rect.Bottom)
            {
                inside = false;
                curDiff += rect.Bottom - start.y;
            }
            else if (start.y >= rect.Top)
            {
                inside = false;
                curDiff += start.y - rect.Top;
            }

            if (inside)
            {
                return null;
            }

            if (curDiff < closestDiff)
            {
                closestDiff = curDiff;
                closestRoom = room;
            }
        }
        return closestRoom;
    }
}

