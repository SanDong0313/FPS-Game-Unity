using System;
using System.Collections.Generic;
using UnityEngine;

namespace MFPS.InputManager
{
    [Serializable]
    public class ButtonData
    {
        public string KeyName;
        [TextArea(1, 2)] public string Description;
        [KeyFinder] public KeyCode PrimaryKey = KeyCode.None;
        [KeyFinder] public KeyCode AlternativeKey = KeyCode.None;

        public string PrimaryAxis = "";
        public string AlternativeAxis = "";

        public bool PrimaryIsAxis = false;
        public bool AlternativeIsAxis = false;

        public float AxisValue = 1;

        private int downState = 0;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool isButtonDown()
        {
            bool isTrue = !PrimaryIsAxis ? Input.GetKeyDown(PrimaryKey) : isAxisTrue(PrimaryAxis);
            if (isTrue)
            {
                if (downState == 1)
                    isTrue = false;
                else if (downState == 0)
                    downState = 1;
            }
            else if (!isTrue) { downState = 0; }

            if (isTrue) return isTrue;
            isTrue = !AlternativeIsAxis ? Input.GetKeyDown(AlternativeKey) : isAxisTrue(AlternativeAxis);
            return isTrue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool isButton()
        {
            bool isTrue = !PrimaryIsAxis ? Input.GetKey(PrimaryKey) : isAxisTrue(PrimaryAxis);
            if (isTrue) return isTrue;
            isTrue = !AlternativeIsAxis ? Input.GetKey(AlternativeKey) : isAxisTrue(AlternativeAxis);
            return isTrue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool isButtonUp()
        {
            bool isTrue = !PrimaryIsAxis ? Input.GetKeyUp(PrimaryKey) : isAxisTrue(PrimaryAxis);
            if (isTrue) { downState = 0; return isTrue; }
            isTrue = !AlternativeIsAxis ? Input.GetKeyUp(AlternativeKey) : isAxisTrue(AlternativeAxis);
            return isTrue;
        }

        private bool isAxisTrue(string axisName)
        {
            if (string.IsNullOrEmpty(axisName)) return false;
            return Input.GetAxis(axisName) == AxisValue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string GetInputName()
        {
            if (PrimaryIsAxis) return PrimaryAxis;
            else return PrimaryKey.ToString();
        }
    }
}