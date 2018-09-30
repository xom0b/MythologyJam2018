using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GUIDebugLog : MonoBehaviour
{
    public static bool showDebug = false;
    public static string debugLog;

    public static void Log(string s)
    {
        debugLog += "/n" + s;
    }

    private void OnGUI()
    {
        if (showDebug)
        {
            GUI.TextArea(new Rect(10, 10, Screen.width - 10, Screen.height - 10), debugLog);
        }
    }
}
