using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TP View Data", menuName = "MFPS/ThirdPerson/View Data")]
public class bl_TPViewData : ScriptableObject
{
    public Vector2 ViewPosition;
    public Vector3 ViewRotation;
    [Range(0.1f, 7)] public float DistanceFromPlayer = 5;
    [Range(0.1f, 5)] public float TransitionDuration = 0.5f;

    public AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    public Vector3 GetViewPosition => new Vector3(ViewPosition.x, ViewPosition.y, -DistanceFromPlayer);
}