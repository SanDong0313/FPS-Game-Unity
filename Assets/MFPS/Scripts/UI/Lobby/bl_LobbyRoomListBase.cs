using Photon.Realtime;
using UnityEngine;
using System.Collections.Generic;

public abstract class bl_LobbyRoomListBase : MonoBehaviour
{

    /// <summary>
    /// 
    /// </summary>
    /// <param name="rooms"></param>
    public abstract void SetRoomList(List<RoomInfo> rooms);

    /// <summary>
    /// 
    /// </summary>
    private static bl_LobbyRoomListBase _instance;
    public static bl_LobbyRoomListBase Instance
    {
        get
        {
            if (_instance == null) _instance = FindObjectOfType<bl_LobbyRoomListBase>();
            if (_instance == null && bl_LobbyUI.Instance != null) _instance = bl_LobbyUI.Instance.GetComponentInChildren<bl_LobbyRoomListBase>(true);
            return _instance;
        }
    }
}
