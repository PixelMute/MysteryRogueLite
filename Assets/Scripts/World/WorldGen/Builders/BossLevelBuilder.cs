class BossLevelBuilder : LineBuilder
{
    public BossRoom BossRoom { get; set; }
    protected override bool PlaceExit(Room prev, float direction, float pathVariance)
    {
        var res = AttemptToPlaceRoom(prev, BossRoom, direction + Random.Range(-pathVariance, pathVariance));
        if (res != -1)
        {
            PlacedRooms.Add(BossRoom);
            return base.PlaceExit(BossRoom, direction, pathVariance);
        }
        else
        {
            return false;
        }
    }
}

