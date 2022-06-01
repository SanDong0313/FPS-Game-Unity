using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using MFPSEditor;
using MFPS.ThirdPerson;
using MFPS.Runtime.UI;

public class MFPSThirdPersonViewAddon
{
    private const string DEFINE_KEY = "MFPSTPV";

#if !MFPSTPV
    [MenuItem("MFPS/Addons/Third Person/Enable")]
    private static void Enable()
    {
        EditorUtils.SetEnabled(DEFINE_KEY, true);
    }
#endif

#if MFPSTPV
    [MenuItem("MFPS/Addons/Third Person/Disable")]
    private static void Disable()
    {
        EditorUtils.SetEnabled(DEFINE_KEY, false);
    }
#endif

    [MenuItem("MFPS/Addons/Third Person/Integrate")]
    private static void Instegrate()
    {
        SetupPlayer(bl_GameData.Instance.Player1.gameObject);
        SetupPlayer(bl_GameData.Instance.Player2.gameObject);

#if PSELECTOR
        var allPlayers = bl_PlayerSelector.Data.AllPlayers;
        foreach(var p in allPlayers)
        {
            SetupPlayer(p.Prefab);
        }
#endif
        AddonIntegrationWizard.ShowSuccessIntegrationLog(null, "Third Person View");
    }

    static void SetupPlayer(GameObject player)
    {
        if (player == null) return;
        bl_PlayerCameraSwitcher pcs = player.GetComponent<bl_PlayerCameraSwitcher>();
        if (pcs != null) return;

        pcs = player.AddComponent<bl_PlayerCameraSwitcher>();
        pcs.viewState = bl_CameraViewSettings.Instance.customViews[2];
        pcs.viewAimState = bl_CameraViewSettings.Instance.customViews[3];

        EditorUtility.SetDirty(pcs);
        EditorUtility.SetDirty(player);
    }

    [MenuItem("MFPS/Addons/Third Person/Integrate Settings")]
    private static void InstegrateUI()
    {
        var reference = bl_UIReferences.Instance.GetComponentInChildren<bl_LoadoutDropdown>(true);
        if(reference == null)
        {
            Debug.LogWarning("Integration failed due missing references.");
            return;
        }

        var parent = reference.transform.parent.parent;
        var prefab = AssetDatabase.LoadAssetAtPath("Assets/Addons/ThirdPersonView/Prefabs/UI/Setting Row (Camera View).prefab", typeof(GameObject)) as GameObject;
        var instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
        instance.transform.SetParent(parent, false);
        instance.transform.SetAsLastSibling();
        EditorUtility.SetDirty(bl_UIReferences.Instance);
        EditorUtility.SetDirty(instance);
        AddonIntegrationWizard.ShowSuccessIntegrationLog(instance, "Third Person View Setting");
    }

    [MenuItem("MFPS/Addons/Third Person/Integrate Settings",true)]
    private static bool InstegrateUIVerification()
    {
        var gm = bl_UIReferences.Instance;
        if (gm == null) return false;
        if (gm.GetComponentInChildren<bl_CameraViewUISetting>(true) != null) return false;
        return true;
    }
}