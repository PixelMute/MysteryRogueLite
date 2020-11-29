class LargeSqaureRoom : StandardRoom
{
    public LargeSqaureRoom() : base()
    {
        Info.MinWidth = 12;
        Info.MaxWidth = 12;
        Info.MinHeight = 12;
        Info.MaxHeight = 12;
    }

    public override void Paint(Level level)
    {
        var painter = new RoomPainter(level, this);
        PaintOutsideWalls(level);
        var newRect = new RogueRect(Bounds);
        newRect.Left += 1;
        newRect.Right = newRect.Left + 2;
        newRect.Top -= 2;
        newRect.Bottom += 1;
        painter.PaintFloorArea(newRect);
        newRect.Right = Bounds.Right - 2;
        newRect.Left = newRect.Right - 2;
        painter.PaintFloorArea(newRect);
        newRect = new RogueRect(Bounds);
        newRect.Bottom += 1;
        newRect.Top = newRect.Bottom + 2;
        newRect.Left += 1;
        newRect.Right -= 2;
        painter.PaintFloorArea(newRect);
        newRect.Top = Bounds.Top - 2;
        newRect.Bottom = newRect.Top - 2;
        painter.PaintFloorArea(newRect);
        painter.AutoPaintWalls();
    }
}

