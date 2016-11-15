using UnityEngine;
using UnityEditor;
using System.Collections;

public class SettingsWindow : EditorWindow
{
    //TODO:

    //wrap coroutine so that there are a maximum number of modules executed per frame no matter what, and a queue stores delegates and executes them the next frame.

    [MenuItem("LSystem/Settings")]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        SettingsWindow window = (SettingsWindow)GetWindow(typeof(SettingsWindow));
        window.Show();
    }

    void OnGUI()
    {
       // Module.MaximumNewModulesPerFrame = EditorGUILayout.IntField("Maximum New Modules Per Frame", Module.MaximumNewModulesPerFrame);

    }
}
