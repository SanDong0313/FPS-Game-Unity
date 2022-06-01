using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Photon.Pun;

[CustomEditor(typeof(bl_PlayerCameraSwitcher))]
public class bl_PlayerCameraSwitcherEditor : Editor
{
    bl_PlayerCameraSwitcher script;
    GUIContent redIcon;
    int recordingState = 0;
    bool isRecording = false;
    Vector3 defaultPosition, defaultRotation;

    private void OnEnable()
    {
        script = (bl_PlayerCameraSwitcher)target;
        redIcon = EditorGUIUtility.IconContent("d_redLight");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUI.BeginChangeCheck();
        GUI.enabled = recordingState == 0 || recordingState == 1;
        var vsp = serializedObject.FindProperty("viewState");
        EditorGUILayout.PropertyField(vsp);
        Rect r = GUILayoutUtility.GetLastRect();
        if (script.gameObject.activeInHierarchy)
        {       
            r.x += r.width - 30;
            r.width = 25;
            r.y = vsp.isExpanded ? r.y - 26 : r.y + 5;
            r.height = EditorGUIUtility.singleLineHeight;

            if (GUI.Button(r, redIcon, EditorStyles.toolbarButton))
            {
                recordingState = 1;
                OnRecord();
            }
        }
        GUI.enabled = true;



        GUI.enabled = recordingState == 0 || recordingState == 2;
        var vas = serializedObject.FindProperty("viewAimState");
        EditorGUILayout.PropertyField(vas);
        if (script.gameObject.activeInHierarchy)
        {
            r = GUILayoutUtility.GetLastRect();
            r.x += r.width - 30;
            r.width = 25;
            r.y = vas.isExpanded ? r.y - 26 : r.y + 5;
            r.height = EditorGUIUtility.singleLineHeight;

            if (GUI.Button(r, redIcon, EditorStyles.toolbarButton))
            {
                recordingState = 2;
                OnRecord();
            }
        }
        GUI.enabled = true;
        GUILayout.Space(EditorGUIUtility.singleLineHeight);
        EditorGUILayout.BeginVertical("box");
        script.collisionOffset = EditorGUILayout.Slider("Collision Offset", script.collisionOffset, 0.01f, 0.7f);
        script.collisionUpdateFrequency = EditorGUILayout.IntSlider("Collision Update Frequency", script.collisionUpdateFrequency, 1, 30);
        EditorGUILayout.EndVertical();
        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
        }
    }

    void OnRecord()
    {
        isRecording = !isRecording;
        var references = script.GetComponent<bl_PlayerReferences>();
        Transform t = references.playerCamera.transform;
        ActiveEditorTracker.sharedTracker.isLocked = isRecording;
        if (isRecording)
        {
            defaultPosition = t.localPosition;
            defaultRotation = t.localEulerAngles;
            references.gunManager.gameObject.SetActive(false);
            if(recordingState == 1)
            {
                t.localPosition = new Vector3(script.viewState.ViewPosition.x, script.viewState.ViewPosition.y, -script.viewState.DistanceFromPlayer);
                t.localEulerAngles = script.viewState.ViewRotation;
            }
            else
            {
                t.localPosition = new Vector3(script.viewAimState.ViewPosition.x, script.viewAimState.ViewPosition.y, -script.viewAimState.DistanceFromPlayer);
                t.localEulerAngles = script.viewAimState.ViewRotation;
            }
            Selection.activeTransform = t;
        }
        else
        {
            Selection.activeObject = script.transform;
            if (recordingState == 1)
            {
                script.viewState.ViewPosition = new Vector2(t.localPosition.x, t.localPosition.y);
                script.viewState.DistanceFromPlayer = Mathf.Abs(t.localPosition.z);
                script.viewState.ViewRotation = t.localEulerAngles;
            }
            else
            {
                script.viewAimState.ViewPosition = new Vector2(t.localPosition.x, t.localPosition.y);
                script.viewAimState.DistanceFromPlayer = Mathf.Abs(t.localPosition.z);
                script.viewAimState.ViewRotation = t.localEulerAngles;
            }
            t.localPosition = defaultPosition;
            t.localEulerAngles = defaultRotation;
            references.gunManager.gameObject.SetActive(true);
            recordingState = 0;
        }
        serializedObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(target);
        EditorUtility.SetDirty(script.viewState);
        EditorUtility.SetDirty(script.viewAimState);
    }
}