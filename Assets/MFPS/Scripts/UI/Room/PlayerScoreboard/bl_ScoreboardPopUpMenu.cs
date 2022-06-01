using MFPS.Internal;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

public class bl_ScoreboardPopUpMenu : bl_ScoreboardPopUpMenuBase
{
    public MenuOptions[] options;
    public UIListHandler listHandler;
    [SerializeField] private GameObject content = null;


    /// <summary>
    /// 
    /// </summary>
    private void OnEnable()
    {
        PrepareMenu();
        ((RectTransform)transform).position = Input.mousePosition;
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnDisable()
    {
        TargetPlayer = null;
    }

    /// <summary>
    /// 
    /// </summary>
    public override bl_ScoreboardPopUpMenuBase SetActive(bool active)
    {
        content.SetActive(active);
        return this;
    }

    /// <summary>
    /// 
    /// </summary>
    public override bl_ScoreboardPopUpMenuBase SetTargetPlayer(Player player)
    {
        TargetPlayer = player;
        return this;
    }

    /// <summary>
    /// 
    /// </summary>
    public void PrepareMenu()
    {
        if (listHandler.IsInitialize) return;

        for (int i = 0; i < options.Length; i++)
        {
            var btn = listHandler.InstatiateAndGet<Button>();
            btn.onClick.AddListener(() => { OnOptionClicked(i); });
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="option"></param>
    public void OnOptionClicked(int option)
    {
        // The kick event is handled in bl_KickVotationUI.cs
        onScoreboardMenuAction?.Invoke(option);
        SetActive(false);
    }
}