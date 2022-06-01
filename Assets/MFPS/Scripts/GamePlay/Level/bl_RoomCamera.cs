using UnityEngine;
using UnityEngine.Animations;

public class bl_RoomCamera : bl_RoomCameraBase
{
    #region Public members
    [Header("Auto Rotation")]
    [LovattoToogle] public bool autoRotation = true;
    public Axis rotationDirection = Axis.X;
    public float rotationSpeed = 4;

    [Header("Fly Camera")]
    public float cameraSensitivity = 90;
    public float climbSpeed = 4;
    public float normalMoveSpeed = 10;
    public float slowMoveFactor = 0.25f;
    public float fastMoveFactor = 3; 
    #endregion

    #region Private members
    private float rotationX = 0.0f;
    private float rotationY = 0.0f;
    private Transform m_Transform;
    private Quaternion origiRotation;
    private Vector3 originPosition;
    private CameraMode currentCameraMode = CameraMode.MapPreview; 
    #endregion

    /// <summary>
    /// 
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        m_Transform = transform;
        origiRotation = m_Transform.localRotation;
        originPosition = m_Transform.localPosition;       
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnEnable()
    {
        base.OnEnable();
        Camera.SetupCurrent(GetComponent<Camera>());
        m_Transform = transform;
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnUpdate()
    {
        FlyMovement();
        Rotate();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cameraMode"></param>
    public override void SetCameraMode(CameraMode cameraMode)
    {
        currentCameraMode = cameraMode;
    }

    /// <summary>
    /// 
    /// </summary>
    void Rotate()
    {
        if (!autoRotation || currentCameraMode != CameraMode.MapPreview || rotationDirection == Axis.None) return;

        Vector3 dir = Vector3.up;
        if (rotationDirection == Axis.X) dir = Vector3.right;
        else if (rotationDirection == Axis.Z) dir = Vector3.forward;

        m_Transform.Rotate(dir * Time.deltaTime * rotationSpeed);
    }

    /// <summary>
    /// 
    /// </summary>
    void FlyMovement()
    {
        if (currentCameraMode != CameraMode.Spectator) return;

        rotationX += bl_GameInput.MouseX * cameraSensitivity * Time.deltaTime;
        rotationY += bl_GameInput.MouseY * cameraSensitivity * Time.deltaTime;
        rotationY = Mathf.Clamp(rotationY, -90, 90);

        m_Transform.localRotation = Quaternion.AngleAxis(rotationX, Vector3.up);
        m_Transform.localRotation *= Quaternion.AngleAxis(rotationY, Vector3.left);

        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            m_Transform.position += m_Transform.forward * (normalMoveSpeed * fastMoveFactor) * bl_GameInput.Vertical * Time.deltaTime;
            m_Transform.position += m_Transform.right * (normalMoveSpeed * fastMoveFactor) * bl_GameInput.Horizontal * Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
        {
            m_Transform.position += m_Transform.forward * (normalMoveSpeed * slowMoveFactor) * bl_GameInput.Vertical * Time.deltaTime;
            m_Transform.position += m_Transform.right * (normalMoveSpeed * slowMoveFactor) * bl_GameInput.Horizontal * Time.deltaTime;
        }
        else
        {
            m_Transform.position += m_Transform.forward * normalMoveSpeed * bl_GameInput.Vertical * Time.deltaTime;
            m_Transform.position += m_Transform.right * normalMoveSpeed * bl_GameInput.Horizontal * Time.deltaTime;
        }


        if (Input.GetKey(KeyCode.Q)) { m_Transform.position += m_Transform.up * climbSpeed * Time.deltaTime; }
        if (Input.GetKey(KeyCode.E)) { m_Transform.position -= m_Transform.up * climbSpeed * Time.deltaTime; }

        if (bl_GameInput.Jump())
        {
            bl_UtilityHelper.LockCursor((bl_RoomMenu.Instance.isCursorLocked == false) ? true : false);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="active"></param>
    public override void SetActive(bool active)
    {
        gameObject.SetActive(active);
    }

    /// <summary>
    /// 
    /// </summary>
    public override void ResetCamera()
    {
        currentCameraMode = CameraMode.MapPreview;
        m_Transform.localRotation = origiRotation;
        m_Transform.localPosition = originPosition;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override CameraMode GetCameraMode()
    {
        return currentCameraMode;
    }
}