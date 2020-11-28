public class StandardRoom : Room
{
    public StandardRoom() : base(new RoomInfo()
    {
        MaxConnections = 16,
        MinWidth = 6,
        MaxWidth = 12,
        MinHeight = 6,
        MaxHeight = 12,
        RoomType = RoomType.STANDARD,
    })
    { }

    public override void PaintDoors(Level level)
    {
        base.PaintDoors(level);
        var painter = new RoomPainter(level, this);
        foreach (var door in ConnectionLocations)
        {
            if (door.x == Bounds.Right || door.x == Bounds.Left)
            {
                var xValue = door.x == Bounds.Right ? door.x - 1 : door.x;
                if (door.y <= Bounds.Top - 2)
                {
                    painter.PaintTopWall(xValue, xValue + 1, door.y + 1);
                }
                if (door.y > Bounds.Bottom + 1)
                {
                    if (door.x == Bounds.Right)
                    {
                        painter.PaintTerrainTile(xValue, door.y - 1, Painter.TurnLeftTiles.PickRandom());
                    }
                    else
                    {
                        painter.PaintTerrainTile(xValue, door.y - 1, Painter.TurnRightTiles.PickRandom());
                    }
                }
                else if (door.y == Bounds.Bottom + 1)
                {
                    painter.PaintBottomWall(xValue, xValue + 1, door.y - 1);
                }
            }
            else if (door.y == Bounds.Bottom)
            {
                if (door.x < Bounds.Right - 2)
                {
                    painter.PaintTerrainTile(door.x + 1, door.y, Painter.TurnLeftTiles.PickRandom());
                }
                else if (door.x == Bounds.Right - 2)
                {
                    painter.PaintRightWall(door.y, door.y + 1, door.x + 1);
                }
                if (door.x > Bounds.Left + 1)
                {
                    painter.PaintTerrainTile(door.x - 1, door.y, Painter.TurnRightTiles.PickRandom());
                }
                else if (door.x == Bounds.Left + 1)
                {
                    painter.PaintLeftWall(door.y, door.y + 1, door.x - 1);
                }
            }
        }
    }

    public override void Paint(Level level)
    {
        var painter = new RoomPainter(level, this);
        painter.PaintBottomWall(Bounds.Left, Bounds.Right - 1, Bounds.Bottom);
        painter.PaintTopWall(Bounds.Left, Bounds.Right - 1, Bounds.Top - 1);
        painter.PaintLeftWall(Bounds.Bottom, Bounds.Top, Bounds.Left);
        painter.PaintRightWall(Bounds.Bottom, Bounds.Top, Bounds.Right - 1);
        painter.PaintFloorArea(Bounds.Left + 1, Bounds.Right - 1, Bounds.Bottom + 1, Bounds.Top - 1);
        painter.PaintBottomLeftCorner(Bounds.Left, Bounds.Bottom);
        painter.PaintBottomRightCorner(Bounds.Right - 1, Bounds.Bottom);
    }
}

