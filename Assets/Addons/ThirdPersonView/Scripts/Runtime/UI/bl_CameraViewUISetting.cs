using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MFPS.Runtime.Settings;

namespace MFPS.ThirdPerson
{
    public class bl_CameraViewUISetting : MonoBehaviour
    {
        public bl_SingleSettingsBinding settingsBinding;
        public static MPlayerViewMode SelectedViewMode = MPlayerViewMode.None;

        /// <summary>
        /// 
        /// </summary>
        private void Awake()
        {
            if (bl_CameraViewSettings.Instance.gamePlayerView == MFPSGamePlayerView.FirstPersonOnly || bl_CameraViewSettings.Instance.gamePlayerView == MFPSGamePlayerView.ThirdPersonOnly)
            {
                    gameObject.SetActive(false);
                    return;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnEnable()
        {
            bl_EventHandler.onGameSettingsChange += OnSettingsChanged;
            this.InvokeAfter(0.5f, () =>
             {
                 settingsBinding.currentOption = (int)bl_CameraViewSettings.Instance.CurrentViewMode;
                 settingsBinding.ApplyCurrentValue();
             });
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnDisable()
        {
            bl_EventHandler.onGameSettingsChange -= OnSettingsChanged;
        }

        /// <summary>
        /// 
        /// </summary>
        void OnSettingsChanged()
        {
          /*  if (bl_CameraViewSettings.Instance.CurrentViewMode == SelectedViewMode) return;
            bl_CameraViewSettings.Instance.CurrentViewMode = SelectedViewMode;*/
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public void OnChange(int value)
        {
            SelectedViewMode = (MPlayerViewMode)value;
        }
    }
}