using System;
using System.Collections.Generic;
using UnityEngine;

public class Builder
{
    protected Room entrance;
    protected Room exit;

    protected List<Room> singleConnections;
    protected List<Room> multiConnections;
    protected List<Room> branchable;
    protected List<int> branchConnectionLength = new List<int> { 0, 0, 1, 1, 2 };
    protected List<int> pathConnectionLength = new List<int> { 0, 1, 1, 1, 2 };
    protected List<int> pathLengthJitters = new List<int> { 0, 1, 1 };
    protected float pathLengthPercentage = .5f;

    public List<Room> Rooms { get; set; }
    public List<Room> PlacedRooms;

    private enum Direction
    {
        LEFT,
        TOP,
        RIGHT,
        BOTTOM
    }

    public virtual bool Build(List<Room> rooms)
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
        PlacedRooms.Add(entrance);
        branchable.Add(entrance);

        int roomsOnPath = (int)(multiConnections.Count * pathLengthPercentage) + pathLengthJitters.PickRandom();
        roomsOnPath = Math.Min(roomsOnPath, multiConnections.Count);
        float pathVariance = 15;

        var curr = entrance;
        float res;
        int rejected = 0;
        Debug.Log($"Number of rooms on path: {roomsOnPath}");
        for (int i = 0; i < roomsOnPath; i++)
        {
            for (int j = 0; j < pathConnectionLength.PickRandom(); j++)
            {
                var connectionRoom = new ConnectionRoom();
                res = AttemptToPlaceRoom(curr, connectionRoom, direction + Random.Range(-pathVariance, pathVariance));
                if (res != -1)
                {
                    PlacedRooms.Add(connectionRoom);
                    branchable.Add(connectionRoom);
                    curr = connectionRoom;
                }
                else
                {
                    rejected++;
                }
            }
            var next = multiConnections[i];
            res = AttemptToPlaceRoom(curr, next, direction + Random.Range(-pathVariance, pathVariance));
            if (res != -1)
            {
                PlacedRooms.Add(next);
                branchable.Add(next);
                curr = next;
            }
            else
            {
                rejected++;
            }
        }
        Debug.Log($"Number of rooms on path rejected: {rejected} out of {roomsOnPath}");
        res = AttemptToPlaceRoom(curr, exit, direction + Random.Range(-pathVariance, pathVariance));
        if (res != -1)
        {
            PlacedRooms.Add(exit);
        }
        else
        {
            return false;
        }

        var roomsToBranch = GetRoomsToBranch(roomsOnPath);
        CreateBranches(roomsToBranch);

        FindNeighbours();

        return true;
    }

    protected List<Room> GetRoomsToBranch(int numRoomsOnPath)
    {
        var res = new List<Room>();
        for (int i = numRoomsOnPath; i < multiConnections.Count; i++)
        {
            res.Add(multiConnections[i]);
        }
        res.AddRange(singleConnections);
        return res;
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

    protected void CreateBranches(List<Room> roomsToBranch)
    {
        Debug.Log($"Number of rooms to branch: {roomsToBranch.Count}");
        var currentBranch = new List<Room>();
        var i = 0;
        while (i < roomsToBranch.Count)
        {
            currentBranch.Clear();
            var room = roomsToBranch[i];
            var curr = branchable.PickRandom();
            var numConnections = branchConnectionLength.PickRandom();
            currentBranch = TryCreateBranch(room, curr, numConnections);
            if (currentBranch.Count != numConnections + 1)
            {
                continue;
            }

            foreach (var branched in currentBranch)
            {
                if (branched.Info.MaxConnections > 1 && Random.RandBool(.3f))
                {
                    if (branched is StandardRoom)
                    {
                        for (int j = 0; j < Random.Range(1, 4); j++)
                        {
                            branchable.Add(branched);
                        }
                    }
                    else
                    {
                        branchable.Add(branched);
                    }
                }
            }
            i++;
        }
    }

    private List<Room> TryCreateBranch(Room roomToBranch, Room currentRoom, int numConnRooms)
    {
        int tries;
        float angle;
        var currentBranch = new List<Room>();
        for (int j = 0; j < numConnRooms; j++)
        {
            var connRoom = new ConnectionRoom();
            tries = 3;
            do
            {
                tries--;
                angle = AttemptToPlaceRoom(currentRoom, connRoom, Random.RandomDirection());
            } while (tries > 0 && angle == -1);

            if (angle == -1)    //If we failed to place the connecting room, remove all rooms on this branch
            {
                foreach (var branchRoom in currentBranch)
                {
                    branchRoom.Reset();
                    PlacedRooms.Remove(branchRoom);
                }
                currentBranch.Clear();
                return currentBranch;
            }
            else
            {
                currentRoom = connRoom;
                currentBranch.Add(connRoom);
                PlacedRooms.Add(connRoom);
            }
        }

        tries = 10;
        do
        {
            tries--;
            angle = AttemptToPlaceRoom(currentRoom, roomToBranch, Random.RandomDirection());
        } while (tries > 0 && angle == -1);

        if (angle == -1)
        {
            roomToBranch.Reset();
            foreach (var branchRoom in currentBranch)
            {
                branchRoom.Reset();
                PlacedRooms.Remove(branchRoom);
            }
            currentBranch.Clear();
            return currentBranch;
        }
        else
        {
            currentBranch.Add(roomToBranch);
            PlacedRooms.Add(roomToBranch);
        }
        return currentBranch;
    }

    protected float AttemptToPlaceRoom(Room previousRoom, Room nextRoom, float angle)
    {
        angle %= 360f;
        if (angle < 0)
            angle += 360;

        var prevCenter = previousRoom.Bounds.Center;

        var m = (float)Math.Tan(angle.DegreesToRadians());
        var b = prevCenter.y - m * prevCenter.x;

        var direction = GetEdgeThatWillConnect(previousRoom.Bounds, m, b, angle);
        var start = IntersectRogueRectAndLine(previousRoom.Bounds, m, b, direction);

        //cap it to a valid connection point for most rooms
        if (direction == Direction.TOP || direction == Direction.BOTTOM)
        {
            start.x = start.x.Gate(previousRoom.Bounds.Left + 2, previousRoom.Bounds.Right - 2);
        }
        else
        {
            start.y = start.y.Gate(previousRoom.Bounds.Bottom + 2, previousRoom.Bounds.Top - 2);
        }

        var startingSpace = GetStartingFreeSpace(start, direction, Math.Max(nextRoom.Info.MaxWidth, nextRoom.Info.MaxHeight));
        var freeSpace = FindFreeSpace(startingSpace, start);
        if (freeSpace == null || !nextRoom.SetSizeWithLimit(freeSpace.Width, freeSpace.Height))
        {
            if (freeSpace == null)
            {
                //Debug.Log("Reason for rejection: free space was null");
            }
            else
            {
                //Debug.Log("Reason for rejection: free space too small for room");
            }
            //If there is no free space or the free space is too small for the room
            return -1;
        }
        var idealCenter = GetIdealCenter(previousRoom.Bounds, nextRoom.Bounds, m, b, direction);
        nextRoom.Bounds.SetCenter(idealCenter.x, idealCenter.y);

        //perform connection bounds and target checking, move the room if necessary
        if (direction == Direction.TOP || direction == Direction.BOTTOM)
        {
            if (nextRoom.Bounds.Right < previousRoom.Bounds.Left + 3)
                nextRoom.Bounds.Shift(previousRoom.Bounds.Left + 3 - nextRoom.Bounds.Right, 0);
            else if (nextRoom.Bounds.Left > previousRoom.Bounds.Right - 3)
                nextRoom.Bounds.Shift(previousRoom.Bounds.Right - 3 - nextRoom.Bounds.Left, 0);

            if (nextRoom.Bounds.Right > freeSpace.Right)
                nextRoom.Bounds.Shift(freeSpace.Right - nextRoom.Bounds.Right, 0);
            else if (nextRoom.Bounds.Left < freeSpace.Left)
                nextRoom.Bounds.Shift(freeSpace.Left - nextRoom.Bounds.Left, 0);
        }
        else
        {
            if (nextRoom.Bounds.Bottom > previousRoom.Bounds.Top - 3)
                nextRoom.Bounds.Shift(0, previousRoom.Bounds.Top - 3 - nextRoom.Bounds.Bottom);
            else if (nextRoom.Bounds.Top < previousRoom.Bounds.Bottom + 3)
                nextRoom.Bounds.Shift(0, previousRoom.Bounds.Bottom + 3 - nextRoom.Bounds.Top);

            if (nextRoom.Bounds.Bottom < freeSpace.Bottom)
                nextRoom.Bounds.Shift(0, freeSpace.Bottom - nextRoom.Bounds.Bottom);
            else if (nextRoom.Bounds.Top > freeSpace.Top)
                nextRoom.Bounds.Shift(0, freeSpace.Top - nextRoom.Bounds.Top);
        }
        if (!nextRoom.Bounds.IsInside(freeSpace))
        {
            //Debug.Log("Reason for rejection: Room not inside of free space");
            //Something went wrong
            return -1;
        }

        if (nextRoom.Connect(previousRoom))
        {
            return GetAngleBetweenRooms(nextRoom, previousRoom);
        }
        //Debug.Log("Reason for rejection: Couldn't connect to previous room");
        return -1;
    }

    private RogueRect GetStartingFreeSpace(Vector2Int start, Direction direction, int maxSize)
    {
        return new RogueRect()
        {
            Left = direction == Direction.RIGHT ? start.x : start.x - maxSize,
            Right = direction == Direction.LEFT ? start.x : start.x + maxSize,
            Top = direction == Direction.BOTTOM ? start.y : start.y + maxSize,
            Bottom = direction == Direction.TOP ? start.y : start.y - maxSize,
        };
    }

    protected float GetAngleBetweenRooms(Room one, Room two)
    {
        var centerOne = one.Bounds.Center;
        var centerTwo = two.Bounds.Center;
        float xDiff = centerTwo.x - centerOne.x;
        float yDiff = centerTwo.y - centerOne.y;
        return ((float)Math.Atan2(yDiff, xDiff)).RadiansToDegrees();
    }

    private Direction GetEdgeThatWillConnect(RogueRect RogueRectangle, float m, float b, float angle)
    {
        var width = (float)RogueRectangle.Width;
        var height = (float)RogueRectangle.Height;
        if (-height / 2f <= m * width / 2 && m * width / 2 <= height)
        {
            if (angle <= 90 || angle >= 270)
            {
                return Direction.RIGHT;
            }
            return Direction.LEFT;
        }
        else
        {
            if (angle <= 180)
            {
                return Direction.TOP;
            }
            return Direction.BOTTOM;
        }
    }

    private Vector2Int GetIdealCenter(RogueRect prev, RogueRect next, float m, float b, Direction Direction)
    {
        //find the ideal center for this new room using the line equation and known dimensions
        var targetCenter = new Vector2Int();
        if (Direction == Direction.RIGHT)
        {
            targetCenter.x = prev.Right + (next.Width) / 2;
            targetCenter.y = (int)(m * targetCenter.x + b);
        }
        else if (Direction == Direction.LEFT)
        {
            targetCenter.x = prev.Left - (next.Width) / 2;
            targetCenter.y = (int)(m * targetCenter.x + b);
        }
        else if (Direction == Direction.TOP)
        {
            targetCenter.y = prev.Top + (next.Height) / 2;
            targetCenter.x = (int)((targetCenter.y - b) / m);
        }
        else
        {
            targetCenter.y = prev.Bottom - (next.Height) / 2;
            targetCenter.x = (int)((targetCenter.y - b) / m);
        }
        var width = (float)prev.Width;
        var height = (float)prev.Height;
        return targetCenter;
    }

    protected void InitBuilder()
    {
        singleConnections = new List<Room>();
        multiConnections = new List<Room>();
        PlacedRooms = new List<Room>();
        branchable = new List<Room>();

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
                multiConnections.Add(room);
            }
            else if (room.Info.MaxConnections == 1)
            {
                singleConnections.Add(room);
            }
        }
    }

    private Vector2Int IntersectRogueRectAndLine(RogueRect RogueRectangle, float m, float b, Direction Direction)
    {
        if (Direction == Direction.RIGHT)
        {
            return new Vector2Int(RogueRectangle.Right, (int)Math.Round(m * RogueRectangle.Right + b));
        }
        else if (Direction == Direction.LEFT)
        {
            return new Vector2Int(RogueRectangle.Left, (int)Math.Round(m * RogueRectangle.Left + b));
        }
        else if (Direction == Direction.TOP)
        {
            return new Vector2Int((int)Math.Round((RogueRectangle.Top - b) / m), RogueRectangle.Top);
        }
        else
        {
            return new Vector2Int((int)Math.Round((RogueRectangle.Bottom - b) / m), RogueRectangle.Bottom);
        }
    }

    private RogueRect FindFreeSpace(RogueRect space, Vector2Int start)
    {
        var collisions = new List<Room>(PlacedRooms);
        do
        {
            //Remove all rooms that we aren't currently colliding with or currently inside or that are inside us
            collisions.RemoveAll(room => !room.Bounds.IsColliding(space) && !room.Bounds.IsInside(space) && !space.IsInside(room.Bounds));

            if (collisions.Count != 0)
            {
                var closestRoom = GetClosestRoom(collisions, start);
                if (closestRoom == null)
                {
                    return null;
                }

                var intersection = closestRoom.Bounds.Intersect(space);
                if (intersection.Width >= intersection.Height)
                {
                    if (intersection.Right == space.Right || Math.Abs(space.Right - intersection.Left) <= Math.Abs(space.Left - intersection.Right))
                    {
                        space.Right = intersection.Left;
                    }
                    else
                    {
                        space.Left = intersection.Right;
                    }
                }
                else
                {
                    if (intersection.Top == space.Top || Math.Abs(space.Top - intersection.Bottom) <= Math.Abs(space.Bottom - intersection.Top))
                    {
                        space.Top = intersection.Bottom;
                    }
                    else
                    {
                        space.Bottom = intersection.Top;
                    }
                }
                ////We multiply by height and width here in order to take into effect the current size when determing what way to cut off the RogueRect
                //int wDiff = int.MaxValue;
                //var closestRogueRect = closestRoom.Bounds;
                //if (closestRogueRect.Left >= start.x)
                //{
                //    wDiff = (space.Right - closestRogueRect.Left);
                //}
                //else if (closestRogueRect.Right <= start.x)
                //{
                //    wDiff = (closestRogueRect.Right - space.Left);
                //}

                //var hDiff = int.MaxValue;
                //if (closestRogueRect.Top >= start.y)
                //{
                //    hDiff = (space.Bottom - closestRogueRect.Top);
                //}
                //else if (closestRogueRect.Bottom <= start.y)
                //{
                //    hDiff = (closestRogueRect.Bottom - space.Top);
                //}

                ////reduce by as little as possible to resolve the collision
                //if (wDiff < hDiff || (wDiff == hDiff && Random.Range(0, 2) == 0))
                //{
                //    if (closestRogueRect.Left >= start.x && closestRogueRect.Left < space.Right)
                //        space.Right = closestRogueRect.Left;
                //    if (closestRogueRect.Right <= start.x && closestRogueRect.Right > space.Left)
                //        space.Left = closestRogueRect.Right;
                //}
                //else
                //{
                //    if (closestRogueRect.Top >= start.y && closestRogueRect.Top < space.Bottom)
                //        space.Bottom = closestRogueRect.Top;
                //    if (closestRogueRect.Bottom <= start.y && closestRogueRect.Bottom > space.Top)
                //        space.Top = closestRogueRect.Bottom;
                //}
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
            var RogueRect = room.Bounds;
            var curDiff = 0;
            if (start.x <= RogueRect.Left)
            {
                inside = false;
                curDiff += RogueRect.Left - start.x;
            }
            else if (start.x >= RogueRect.Right)
            {
                inside = false;
                curDiff += start.x - RogueRect.Right;
            }

            if (start.y <= RogueRect.Bottom)
            {
                inside = false;
                curDiff += RogueRect.Bottom - start.y;
            }
            else if (start.y >= RogueRect.Top)
            {
                inside = false;
                curDiff += start.y - RogueRect.Top;
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

