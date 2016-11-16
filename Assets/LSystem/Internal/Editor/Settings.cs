using UnityEngine;
using UnityEditor;
using System.Collections;

namespace LSystem
{
    public class SettingsWindow : EditorWindow
    {
        int maxModulesPerFrame;

        [MenuItem("LSystem/Settings")]
        static void Init()
        {
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