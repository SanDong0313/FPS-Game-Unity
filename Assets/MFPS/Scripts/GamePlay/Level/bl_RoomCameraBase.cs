using UnityEngine;

public abstract class bl_RoomCameraBase : bl_MonoBehaviour
{
    /// <summary>
    /// 
    /// </summary>
    public enum CameraMode
    {
        MapPreview,
        Spectator
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public abstract CameraMode GetCameraMode();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cameraMode"></param>
    public abstract void SetCameraMode(CameraMode cameraMode);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="active"></param>
    public abstract void SetActive(bool active);

    /// <summary>
    /// 
    /// </summary>
    public abstract void ResetCamera();


    private static bl_RoomCameraBase _instance;
    public static bl_RoomCameraBase Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<bl_RoomCameraBase>();
            }
            return _instance;
        }
    }
}
