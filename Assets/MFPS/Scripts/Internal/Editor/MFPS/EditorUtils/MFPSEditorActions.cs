using UnityEngine;
using UnityEditor;

public static class MFPSEditorActions
{
 
    [MenuItem("MFPS/Actions/Reset default server")]
    static void ResetDefaultServer()
    {
        PlayerPrefs.DeleteKey(PropertiesKeys.GetUniqueKey("preferredregion"));
    }
}