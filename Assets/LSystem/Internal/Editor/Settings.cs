using UnityEngine;
using UnityEditor;
using System.Collections;

namespace LSystem
{
    public class SettingsWindow : EditorWindow
    {
        //TODO:

        //wrap coroutine so that there are a maximum number of modules executed per frame no matter what, and a queue stores delegates and executes them the next frame.

        int maxModulesPerFrame;

        [MenuItem("LSystem/Settings")]
        static void Init()
        {
            // Get existing open window or if none, make a new one:
            SettingsWindow window = (SettingsWindow)GetWindow(typeof(SettingsWindow));
            window.Setup();
            window.Show();

           
        }

        void Setup()
        {
            maxModulesPerFrame = PlayerPrefs.GetInt("LSystem_MaxModulesPerFrame", 100);
        }

        void OnGUI()
        {
            int before = maxModulesPerFrame;
            maxModulesPerFrame = EditorGUILayout.IntField("Max Modules Per Frame", maxModulesPerFrame);
            if(before != maxModulesPerFrame)PlayerPrefs.SetInt("LSystem_MaxModulesPerFrame", maxModulesPerFrame);
        }
    }
}