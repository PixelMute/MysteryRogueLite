class SpecialRoom : Room
{
    public SpecialRoom() : base(new RoomInfo()
    {
        MinWidth = 5,
        MinHeight = 5,
        MaxWidth = 10,
        MaxHeight = 10,
        MinConnections = 1,
        MaxConnections = 1,
    })
    { }

    public override void Paint(Level level)
    {
        var painter = new RoomPainter(level, this);
        painter.PaintFloorArea(Bounds.Left + 1, Bounds.Right - 2, Bounds.Bottom + 1, Bounds.Top - 2);
    }
}

