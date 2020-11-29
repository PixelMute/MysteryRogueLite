using System;
using System.Collections.Generic;

class LoopBuilder : Builder
{
    //These methods allow for the adjusting of the shape of the loop
    //by default the loop is a perfect circle, but it can be adjusted

    //increasing the exponent will increase the the curvature, making the loop more oval shaped.
    private int curveExponent = 0;

    //This is a percentage (range 0-1) of the intensity of the curve function
    // 0 makes for a perfect linear curve (circle)
    // 1 means the curve is completely determined by the curve exponent
    private float curveIntensity = 1;

    //Adjusts the starting point along the loop.
    // a common example, setting to 0.25 will make for a short fat oval instead of a long one.
    private float curveOffset = 0;

    public void SetLoopShape(int exponent, float intensity, float offset)
    {
        this.curveExponent = Math.Abs(exponent);
        curveIntensity = intensity % 1f;
        curveOffset = offset % 0.5f;
    }

    public LoopBuilder(int curveExponent, float curveIntensity, float curveOffset)
    {
        this.curveExponent = Math.Abs(curveExponent);
        this.curveIntensity = curveIntensity % 1f;
        this.curveOffset = curveOffset % 0.5f;
    }

    public LoopBuilder() { }

    private float TargetAngle(float percentAlong)
    {
        percentAlong += curveOffset;
        return 360f * (float)(
                        curveIntensity * CurveEquation(percentAlong)
                        + (1 - curveIntensity) * (percentAlong)
                        - curveOffset);
    }

    private double CurveEquation(double x)
    {
        return Math.Pow(4, 2 * curveExponent)
                * (Math.Pow((x % 0.5f) - 0.25, 2 * curveExponent + 1))
                + 0.25 + 0.5 * Math.Floor(2 * x);
    }

    public override bool Build(List<Room> rooms)
    {
        this.Rooms = rooms;
        InitBuilder();

        if (entrance == null || exit == null)
        {
            return false;
        }

        float startAngle = Random.RandomDirection();
        entrance.SetSize();
        entrance.Bounds.SetPosition(0, 0);
        PlacedRooms.Add(entrance);
        branchable.Add(entrance);

        var loop = new List<Room>();
        int roomsOnPath = (int)(multiConnections.Count * pathLengthPercentage) + pathLengthJitters.PickRandom();
        roomsOnPath = Math.Min(roomsOnPath, multiConnections.Count);

        for (int i = 0; i <= roomsOnPath; i++)
        {
            if (i == 0)
            {
                loop.Add(entrance);
            }
            else
            {
                loop.Add(multiConnections[0]);
                multiConnections.RemoveAt(0);
            }
            for (int j = 0; j < pathConnectionLength.PickRandom(); j++)
            {
                var newRoom = new ConnectionRoom();
                loop.Add(newRoom);
            }
        }

        loop.Insert((loop.Count + 1) / 2, exit);

        var prev = entrance;
        float targetAngle;
        for (int i = 1; i < loop.Count; i++)
        {
            var curr = loop[i];
            targetAngle = startAngle + TargetAngle(i / (float)loop.Count);
            var res = AttemptToPlaceRoom(prev, curr, targetAngle);
            if (res != -1)
            {
                PlacedRooms.Add(curr);
                prev = curr;
                branchable.Add(curr);
            }
            else if (i != loop.Count - 1 && !(curr is ConnectionRoom))
            {
                //Little chance dependant, could be better
                return false;
            }
        }

        //While not connected with the entrance, place connection rooms to connect
        //Also chance dependent. Should find better way to handle
        while (!prev.Connect(entrance))
        {

            var c = new ConnectionRoom();
            var angleToEntrance = GetAngleBetweenRooms(prev, entrance);
            if (AttemptToPlaceRoom(prev, c, angleToEntrance) == -1)
            {
                return false;
            }
            loop.Add(c);
            PlacedRooms.Add(c);
            branchable.Add(c);
            prev = c;
        }

        UnityEngine.Debug.Log($"There are {loop.Count} rooms in the loop");

        //Pass in 0 because we remove the rooms from multiconnection
        var roomsToBranch = GetRoomsToBranch(0);
        CreateBranches(roomsToBranch);

        FindNeighbours();

        return true;
    }
}

