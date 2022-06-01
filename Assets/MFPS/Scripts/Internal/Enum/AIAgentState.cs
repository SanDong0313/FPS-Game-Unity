namespace MFPS.Runtime.AI
{
    public enum AIAgentState
    {
        Idle = 0,
        Patroling = 1,
        Following = 2,
        Covering = 3,
        Looking = 4,
        Searching = 5,
        HoldingPosition = 6,
    }

    public enum AIAgentBehave
    {
        Agressive = 0,
        Pasive = 1,
        Protective = 2,
    }

    public enum AIAgentCoverArea
    {
        ToPoint = 0,
        ToTarget = 1,
        ToRandomPoint = 2,
    }

    public enum AITargetOutRangeBehave
    {
        SearchNewNearestTarget,
        KeepFollowingBasedOnState,
    }

    public enum AIWeaponAccuracy
    {
        Pro,
        Casual,
        Novice,
    }

    public enum AILookAt
    {
        Target,
        Path,
        HitDirection,
        PathToTarget,
    }

    public enum AIRemoteCallType : byte
    {
        DestroyBot = 0,
        SyncTarget = 1,
        CrouchState = 2,
        Respawn = 3,
    }
}