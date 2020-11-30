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

    public override void Decorate(Level level)
    {
        var painter = new RoomPainter(level, this);
        level.DecorativeTileMap.SpawnTorch(Bounds.Left + 4, Bounds.Bottom + 4);
        level.DecorativeTileMap.SpawnTorch(Bounds.Left + 7, Bounds.Bottom + 4);
        //level.DecorativeTileMap.SpawnSideTorch(Bounds.Left + 3, Bounds.Bottom + 4, false);
        //level.DecorativeTileMap.SpawnSideTorch(Bounds.Left + 8, Bounds.Bottom + 4, true);
        //level.DecorativeTileMap.SpawnSideTorch(Bounds.Left + 3, Bounds.Bottom + 7, false);
        //level.DecorativeTileMap.SpawnSideTorch(Bounds.Left + 8, Bounds.Bottom + 7, true);
        if (!painter.IsFloor(Bounds.Left + 3, Bounds.Top - 1))
        {
            level.DecorativeTileMap.SpawnTorch(Bounds.Left + 3, Bounds.Top - 1);
        }
        if (!painter.IsFloor(Bounds.Right - 4, Bounds.Top - 1))
        {
            level.DecorativeTileMap.SpawnTorch(Bounds.Right - 4, Bounds.Top - 1);
        }
        if (!painter.IsFloor(Bounds.Left, Bounds.Bottom + 3))
        {
            level.DecorativeTileMap.SpawnSideTorch(Bounds.Left + 1, Bounds.Bottom + 3, true);
        }
        if (!painter.IsFloor(Bounds.Left, Bounds.Top - 4))
        {
            level.DecorativeTileMap.SpawnSideTorch(Bounds.Left + 1, Bounds.Top - 4, true);
        }
        if (!painter.IsFloor(Bounds.Right - 1, Bounds.Bottom + 3))
        {
            level.DecorativeTileMap.SpawnSideTorch(Bounds.Right - 2, Bounds.Bottom + 3, false);
        }
        if (!painter.IsFloor(Bounds.Right - 1, Bounds.Top - 4))
        {
            level.DecorativeTileMap.SpawnSideTorch(Bounds.Right - 2, Bounds.Top - 4, false);
        }
    }
}

