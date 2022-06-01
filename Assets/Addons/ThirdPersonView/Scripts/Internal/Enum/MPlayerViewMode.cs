namespace MFPS.ThirdPerson
{
    using System;
    public enum MPlayerViewMode
    {
        FirstPerson = 0,
        ThirdPerson = 1,
        None,
    }

    [Serializable]
    public enum MFPSGamePlayerView
    {
        FirstPersonOnly,
        ThirdPersonOnly,
        FirstPersonDefault,
        ThirdPersonDefault,
    }

    public enum TPViewOverrideState
    {
        OverrideDefault,
        OverrideAll,
        OverrideSingle,
        None,
    }
}