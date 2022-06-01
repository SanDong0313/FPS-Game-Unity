using UnityEngine;

public abstract class bl_PlayerReferencesCommon : MonoBehaviour
{
    /// <summary>
    /// 
    /// </summary>
    public abstract Collider[] AllColliders 
    { 
        get; 
    }

    /// <summary>
    /// 
    /// </summary>
    public abstract Animator PlayerAnimator 
    { 
        get; 
        set; 
    }

    /// <summary>
    /// 
    /// </summary>
    public abstract Transform BotAimTarget 
    { 
        get; 
        set; 
    }

    /// <summary>
    /// 
    /// </summary>
    public abstract Team PlayerTeam
    {
        get;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="list"></param>
    /// <param name="ignore"></param>
    public abstract void IgnoreColliders(Collider[] list, bool ignore);
}