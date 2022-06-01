using Photon.Realtime;
using System;
using UnityEngine;
using UnityEngine.UI;

public abstract class bl_ScoreboardPopUpMenuBase : MonoBehaviour
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class MenuOptions
    {
        public string Title;

        [HideInInspector] public Button OptionButton;
    }

    public static Action<int> onScoreboardMenuAction;
    public static Player TargetPlayer;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="active"></param>
    /// <returns></returns>
    public abstract bl_ScoreboardPopUpMenuBase SetActive(bool active);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
    public abstract bl_ScoreboardPopUpMenuBase SetTargetPlayer(Player player);
}