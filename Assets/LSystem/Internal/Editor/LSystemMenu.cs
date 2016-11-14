using UnityEngine;
using UnityEditor;

namespace LSystem
{
    public class LSystemMenu
    {
        [MenuItem("LSystem/New Seed")]
        private static void NewMenuOption()
        {
            new GameObject("Seed", typeof(Seed));
        }
    }
}