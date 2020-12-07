using System;
using System.Collections.Generic;


public class LineBuilder : Builder
{
    protected virtual bool PlaceExit(Room prev, float direction, float pathVariance)
    {
        var res = AttemptToPlaceRoom(prev, exit, direction + SeededRandom.Range(-pathVariance, pathVariance));
        if (res != -1)
        {
            PlacedRooms.Add(exit);
            return true;
        }
        else
        {
            return false;
        }
    }

    public override bool Build(List<Room> rooms)
    {
        this.Rooms = rooms;
        InitBuilder();

        if (entrance == null || exit == null)
        {
            return false;
        }

        float direction = SeededRandom.RandomDirection();
        entrance.SetSize();
        entrance.Bounds.SetPosition(0, 0);
        PlacedRooms.Add(entrance);
        branchable.Add(entrance);

        int roomsOnPath = (int)(multiConnections.Count * pathLengthPercentage) + SeededRandom.PickRandom(pathLengthJitters);
        roomsOnPath = Math.Min(roomsOnPath, multiConnections.Count);
        float pathVariance = 15;

        var curr = entrance;
        float res;
        int rejected = 0;
        for (int i = 0; i < roomsOnPath; i++)
        {
            for (int j = 0; j < SeededRandom.PickRandom(pathConnectionLength); j++)
            {
                var connectionRoom = new ConnectionRoom();
                res = AttemptToPlaceRoom(curr, connectionRoom, direction + SeededRandom.Range(-pathVariance, pathVariance));
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
            res = AttemptToPlaceRoom(curr, next, direction + SeededRandom.Range(-pathVariance, pathVariance));
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
        for (int j = 0; j < SeededRandom.PickRandom(pathConnectionLength); j++)
        {
            var connectionRoom = new ConnectionRoom();
            res = AttemptToPlaceRoom(curr, connectionRoom, direction + SeededRandom.Range(-pathVariance, pathVariance));
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

        if (!PlaceExit(curr, direction, pathVariance))
        {
            return false;
        }

        var roomsToBranch = GetRoomsToBranch(roomsOnPath);
        CreateBranches(roomsToBranch);

        FindNeighbours();

        return true;
    }
}

