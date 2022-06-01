#define MFPS_19
//#define MFPS_182
using MFPSEditor;
using System.Collections;
using UnityEngine;
using MFPS.ThirdPerson;
#if UNITY_POST_PROCESSING_STACK_V2
using UnityEngine.Rendering.PostProcessing;
#endif

public class bl_PlayerCameraSwitcher : bl_MonoBehaviour
{

#region Public members
    [ScriptableDrawer] public bl_TPViewData viewState;
    [ScriptableDrawer] public bl_TPViewData viewAimState;

    public float collisionOffset = 0.2f;
    public int collisionUpdateFrequency = 15; 
#endregion

#region Private Variables
    private Transform playerCamera, cameraParent;
    private Vector3 defaultFPPosition;
    private Vector3 defaultFPRotation;
    private bl_PlayerSettings playerSettings;
    private Camera weaponCamera;
    private bl_CameraShakerBase cameraShaker;
    private bl_CameraRayBase cameraRay;
    private bl_PlayerNetwork playerNetwork;
    private TPViewOverrideState m_viewOverrideState = TPViewOverrideState.None;
    public bl_TPViewData m_overrideViewState = null;
#if UNITY_POST_PROCESSING_STACK_V2
    private PostProcessLayer weaponCameraEffects;
    private PostProcessLayer playerCameraEffects;
#endif
    private bool isAiming = false;
    private bool initialized = false;
    private bool cacheReferences = false;
    private bl_PlayerReferences playerReferences;
    private Vector3 tpCameraDirection;
    private RaycastHit cameraCollisionRay;
    private bool isCollisioning = false;
    private Ray dirRay;
#if MFPS_182
    private KeyCode SwitchViewKey = KeyCode.P;
#endif
#endregion

#region Public properties
    public MPlayerViewMode playerViewMode { get; set; } = MPlayerViewMode.FirstPerson;
    public bool CanManualySwitchView { get; set; } = true;
    public bl_TPViewData CurrentViewData { get; set; } 
#endregion

    /// <summary>
    /// 
    /// </summary>
    protected override void Awake()
    {
        if (!photonView.IsMine || bl_CameraViewSettings.Instance.gamePlayerView == MFPSGamePlayerView.FirstPersonOnly) { enabled = false; return; }
        else
        base.Awake();

        if (!cacheReferences)
        {
            #if MFPS_182
            SwitchViewKey = bl_CameraViewSettings.Instance.SwitchViewKey;
            #endif
            playerReferences = GetComponent<bl_PlayerReferences>();
            playerSettings = playerReferences.playerSettings;
            playerNetwork = playerReferences.playerNetwork;
            playerCamera = playerReferences.playerCamera.transform;
            weaponCamera = playerReferences.weaponCamera;
            cameraParent = playerCamera.parent;
            cameraShaker = playerReferences.cameraShaker;
            cameraRay = playerReferences.cameraRay;
            defaultFPPosition = playerCamera.localPosition;
            defaultFPRotation = playerCamera.localEulerAngles;
#if UNITY_POST_PROCESSING_STACK_V2
            weaponCameraEffects = weaponCamera.GetComponent<PostProcessLayer>();
#endif
            cacheReferences = true;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void Start()
    {
        SetDefaultView();
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnEnable()
    {
        if (photonView.IsMine)
        {
            base.OnEnable();
            bl_EventHandler.onChangeWeapon += OnLocalChangeWeapon;
            bl_EventHandler.onLocalAimChanged += OnLocalAimChanged;
            bl_EventHandler.onLocalPlayerFire += OnLocalFire;
            bl_EventHandler.onGameSettingsChange += OnSettingsChanged;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnDisable()
    {
        if (photonView.IsMine)
        {
            base.OnDisable();
            bl_EventHandler.onChangeWeapon -= OnLocalChangeWeapon;
            bl_EventHandler.onLocalAimChanged -= OnLocalAimChanged;
            bl_EventHandler.onLocalPlayerFire -= OnLocalFire;
            bl_EventHandler.onGameSettingsChange -= OnSettingsChanged;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void SetDefaultView()
    {
        if (initialized) return;

        var gpv = bl_CameraViewSettings.Instance.CurrentViewMode;
        if (gpv == MPlayerViewMode.ThirdPerson)
        {
            playerViewMode = MPlayerViewMode.ThirdPerson;
            SetupThirdPerson();
            cameraRay.ExtraRayDistance = viewState.DistanceFromPlayer;
            playerCamera.localPosition = viewState.GetViewPosition;
            playerCamera.localRotation = Quaternion.Euler(viewState.ViewRotation);
            cameraShaker?.SetCurrentAsOriginPosition();
            CurrentViewData = viewState;
        }
        initialized = true;
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnUpdate()
    {
        if (!CanManualySwitchView) return;
        if (bl_CameraViewSettings.Instance.gamePlayerView == MFPSGamePlayerView.ThirdPersonOnly || bl_CameraViewSettings.Instance.gamePlayerView == MFPSGamePlayerView.FirstPersonOnly)
            return;

#if MFPS_19
        if (bl_GameInput.ChangeView()) SwitchView();
#elif MFPS_182
        if (bl_GameInput.GetButton(SwitchViewKey, GameInputType.Down)) SwitchView();
#else
        if (Input.GetKeyDown(SwitchViewKey))SwitchView();
#endif
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnFixedUpdate()
    {
        CameraCollision();
    }

    /// <summary>
    /// Change the view mode to the given one
    /// </summary>
    /// <param name="newViewMode"></param>
    public void SetViewMode(MPlayerViewMode newViewMode)
    {
        if (playerViewMode == newViewMode) return;

        playerViewMode = newViewMode;
        bl_CameraViewSettings.Instance.CurrentViewMode = playerViewMode;
        DoTransitionToCurrentView();
    }

    /// <summary>
    /// 
    /// </summary>
    public void SwitchView()
    {
        if (playerViewMode == MPlayerViewMode.FirstPerson)
            playerViewMode = MPlayerViewMode.ThirdPerson;
        else playerViewMode = MPlayerViewMode.FirstPerson;

        bl_CameraViewSettings.Instance.CurrentViewMode = playerViewMode;
        DoTransitionToCurrentView();
    }

    /// <summary>
    /// 
    /// </summary>
    public void DoTransitionToCurrentView()
    {
        StopAllCoroutines();
        StartCoroutine(DoViewTransition());
    }

    /// <summary>
    /// 
    /// </summary>
    private void CameraCollision()
    {
        if (playerViewMode != MPlayerViewMode.ThirdPerson) return;
        if (!bl_CameraViewSettings.Instance.detectCameraCollision) return;
        if (Time.frameCount % collisionUpdateFrequency != 0 || CurrentViewData == null) return;

        Vector3 origin = cameraParent.TransformPoint(defaultFPPosition);
        Vector3 target = cameraParent.TransformPoint(CurrentViewData.GetViewPosition);
        tpCameraDirection = target - origin;
        dirRay = new Ray(origin, tpCameraDirection);
        float distance = bl_UtilityHelper.Distance(origin, playerCamera.position);

        if (Physics.SphereCast(dirRay, collisionOffset, out cameraCollisionRay, distance, bl_CameraViewSettings.Instance.collisionMask, QueryTriggerInteraction.Ignore))
        {
            float p = cameraCollisionRay.distance / CameraDistance();
            playerCamera.position = Vector3.Lerp(origin, target, p);
            if (!isCollisioning)
            {
                StopAllCoroutines();//stop all the transitions
                isCollisioning = true;
            }
        }
        else
        {
            //if previously was colliding with something
            if (isCollisioning)
            {
                TransitionToCurrent();//back to the view position
                isCollisioning = false;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void TransitionTo(bl_TPViewData newViewState, TPViewOverrideState overrideState = TPViewOverrideState.OverrideSingle)
    {
        m_viewOverrideState = overrideState;
        m_overrideViewState = newViewState;

        if (!initialized) SetDefaultView();

        if (playerViewMode == MPlayerViewMode.FirstPerson || isAiming) return;

        StartCoroutine(DoTransitionTo(newViewState));
    }

    /// <summary>
    /// Make transition to the default view position
    /// </summary>
    public void TransitionToDefault()
    {
        m_overrideViewState = null;
        m_viewOverrideState = TPViewOverrideState.None;

        if (playerViewMode == MPlayerViewMode.FirstPerson || isAiming) return;

        StartCoroutine(DoTransitionTo(viewState));
    }

    /// <summary>
    /// Make transition to the current state
    /// </summary>
    public void TransitionToCurrent()
    {
        if (CurrentViewData == null) return;

        TransitionTo(CurrentViewData);
    }

    /// <summary>
    /// Setup the player for use in Third person mode.
    /// </summary>
    void SetupThirdPerson()
    {
        playerSettings.RemoteObjects.SetActive(true);
        weaponCamera.enabled = false;
        //active the current TP Weapon
        int gunID = playerReferences.gunManager.GetCurrentGunID;
        OnLocalChangeWeapon(gunID);
        bl_CrosshairBase.Instance.AllowFade(false);
        ((bl_PlayerAnimations)playerReferences.playerAnimations).useFootSteps = false;
        playerReferences.playerAnimations.GetComponent<bl_PlayerRagdollBase>().SetActiveRagdollPhysics(false);

#if UNITY_POST_PROCESSING_STACK_V2
        if (weaponCameraEffects != null)
        {
            if (playerCameraEffects == null) playerCameraEffects = playerCamera.GetComponent<PostProcessLayer>();
            if (playerCameraEffects == null)
            {
                playerCameraEffects = playerCamera.gameObject.AddComponent<PostProcessLayer>();
                playerCameraEffects.Init(bl_CameraViewSettings.Instance.postProcessResources);
                playerCameraEffects.volumeLayer = weaponCameraEffects.volumeLayer;
                playerCameraEffects.antialiasingMode = weaponCameraEffects.antialiasingMode;
                playerCameraEffects.fastApproximateAntialiasing.fastMode = weaponCameraEffects.fastApproximateAntialiasing.fastMode;
                playerCameraEffects.volumeTrigger = playerCamera;
            }
            playerCameraEffects.enabled = true;
            weaponCameraEffects.enabled = false;
        }
#endif
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    IEnumerator DoTransitionTo(bl_TPViewData newViewState)
    {
        OnViewChanged();
        Vector3 origin = playerCamera.localPosition;
        Quaternion originRot = Quaternion.Euler(playerCamera.localEulerAngles);
        Vector3 targetPosition = newViewState.GetViewPosition;
        Quaternion targetRot = Quaternion.Euler(newViewState.ViewRotation);
        cameraRay.ExtraRayDistance = newViewState.DistanceFromPlayer;
        CurrentViewData = newViewState;

        float d = 0;
        float t = 0;
        while (d < 1)
        {
            d += Time.deltaTime / newViewState.TransitionDuration;
            t = newViewState.transitionCurve.Evaluate(d);
            playerCamera.localPosition = Vector3.Lerp(origin, targetPosition, t);
            playerCamera.localRotation = Quaternion.Slerp(originRot, targetRot, t);
            cameraShaker?.SetCurrentAsOriginPosition();
            yield return null;
        }
        cameraShaker?.SetCurrentAsOriginPosition();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    IEnumerator DoViewTransition()
    {
        OnViewChanged();
        Vector3 origin = playerCamera.localPosition;
        Quaternion originRot = Quaternion.Euler(playerCamera.localEulerAngles);
        Vector3 targetPosition = defaultFPPosition;
        Quaternion targetRot = Quaternion.Euler(defaultFPRotation);

        bl_TPViewData toData = viewState;
        if(m_viewOverrideState == TPViewOverrideState.OverrideDefault || m_viewOverrideState == TPViewOverrideState.OverrideAll)
        {
            toData = m_overrideViewState;
        }
        CurrentViewData = toData;

        if (playerViewMode == MPlayerViewMode.ThirdPerson)
        {
            SetupThirdPerson();
            cameraRay.ExtraRayDistance = toData.DistanceFromPlayer;
            targetPosition = toData.GetViewPosition;
            targetRot = Quaternion.Euler(toData.ViewRotation);
        }
        else
        {
            cameraRay.ExtraRayDistance = 0;
            bl_CrosshairBase.Instance.AllowFade(true);
        }

        float d = 0;
        float t = 0;
        while (d < 1)
        {
            d += Time.deltaTime / toData.TransitionDuration;
            t = toData.transitionCurve.Evaluate(d);
            playerCamera.localPosition = Vector3.Lerp(origin, targetPosition, t);
            playerCamera.localRotation = Quaternion.Slerp(originRot, targetRot, t);
            cameraShaker?.SetCurrentAsOriginPosition();
            yield return null;
        }
        //update the camera shaker origin position
        cameraShaker?.SetCurrentAsOriginPosition();

        if (playerViewMode == MPlayerViewMode.FirstPerson)
        {
            playerSettings.RemoteObjects.SetActive(false);
            weaponCamera.enabled = true;
            playerReferences.gunManager.GetCurrentWeapon()?.ResetDefaultMuzzlePoint();

#if UNITY_POST_PROCESSING_STACK_V2
            if (playerCameraEffects != null) playerCameraEffects.enabled = false;
            if (weaponCameraEffects != null) weaponCameraEffects.enabled = true;
#endif
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    IEnumerator DoAimTransition(bool aiming)
    {
        OnViewChanged();
        bl_TPViewData toData = viewState;
        if (m_viewOverrideState == TPViewOverrideState.OverrideDefault || m_viewOverrideState == TPViewOverrideState.OverrideAll)
        {
            toData = m_overrideViewState;
        }
        CurrentViewData = toData;

        Vector3 origin = playerCamera.localPosition;
        Vector3 targetPosition = toData.GetViewPosition;
        Quaternion originRot = Quaternion.Euler(playerCamera.localEulerAngles);
        Quaternion targetRot = Quaternion.Euler(toData.ViewRotation);

        if (aiming)
        {
            targetPosition = new Vector3(viewAimState.ViewPosition.x, viewAimState.ViewPosition.y, -viewAimState.DistanceFromPlayer);
            targetRot = Quaternion.Euler(viewAimState.ViewRotation);
        }

        float d = 0;
        float t = 0;
        while (d < 1)
        {
            d += Time.deltaTime / viewAimState.TransitionDuration;
            t = bl_CameraViewSettings.Instance.aimTransitionCurve.Evaluate(d);
            playerCamera.localPosition = Vector3.Lerp(origin, targetPosition, t);
            playerCamera.localRotation = Quaternion.Slerp(originRot, targetRot, t);
            cameraShaker?.SetCurrentAsOriginPosition();
            yield return null;
        }
        cameraShaker?.SetCurrentAsOriginPosition();
    }

    /// <summary>
    /// 
    /// </summary>
    void OnLocalFire(int gunID)
    {
        if (playerViewMode == MPlayerViewMode.FirstPerson) return;
        if (playerNetwork.CurrenGun == null) return;

        playerNetwork.CurrenGun.PlayMuzzleflash();
    }

    /// <summary>
    /// 
    /// </summary>
    void OnLocalChangeWeapon(int gunID)
    {
        if (playerViewMode == MPlayerViewMode.FirstPerson) return;
        //if the player is with hands only
        if(gunID == -1)
        {
            playerNetwork.SetWeaponBlocked(1);
            return;
        }

        playerNetwork.networkGunID = gunID;
        playerNetwork.CurrentTPVGun();
        //change the bullet origin point from FP to TP weapon
        bl_NetworkGun ng = playerNetwork.CurrenGun;
        if (ng != null && ng.MuzzleFlash != null)
        {
            playerReferences.gunManager.GetCurrentWeapon()?.OverrideMuzzlePoint(ng.MuzzleFlash.transform);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void OnLocalAimChanged(bool _isAiming)
    {
        isAiming = _isAiming;
        if (playerViewMode == MPlayerViewMode.FirstPerson) return;

        if (isAiming && playerReferences.gunManager.CurrentGun != null
            && playerReferences.gunManager.CurrentGun.Info.Type == GunType.Sniper)
        {
            bl_CrosshairBase.Instance.AllowFade(true);
        }
        else bl_CrosshairBase.Instance.AllowFade(false);


        StopAllCoroutines();
        StartCoroutine(DoAimTransition(isAiming));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public float CameraDistance()
    {
        if (CurrentViewData == null) return 0;
        return CurrentViewData.DistanceFromPlayer;
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnViewChanged()
    {
        var currentWeapon = playerReferences.gunManager.CurrentGun;
        if(currentWeapon != null)
        {
            currentWeapon.SetWeaponRendersActive(!bl_CameraViewSettings.IsThirdPerson());
        }
        if (playerViewMode == MPlayerViewMode.FirstPerson)
        {
            playerReferences.gunManager.GetCurrentWeapon()?.ResetDefaultMuzzlePoint();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void OnSettingsChanged()
    {
        if (bl_CameraViewUISetting.SelectedViewMode == MPlayerViewMode.None) return;
        if (bl_CameraViewSettings.Instance.CurrentViewMode == bl_CameraViewUISetting.SelectedViewMode) return;

        SetViewMode(bl_CameraViewUISetting.SelectedViewMode);
    }
}