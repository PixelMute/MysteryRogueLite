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


    public override void Paint(Level level)
    {
        var painter = new RoomPainter(level, this);
        PaintOutsideWalls(level);
        painter.PaintFloorArea(Bounds.Left + 1, Bounds.Right - 2, Bounds.Bottom + 1, Bounds.Top - 2);
    }
}

