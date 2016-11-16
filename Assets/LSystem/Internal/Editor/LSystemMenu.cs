using UnityEngine;
using UnityEditor;

namespace LSystem
{
    ///<summary>
    /// New seed shortcut.
    ///</summary>
    public class LSystemMenu
    {
        [MenuItem("LSystem/New Seed")]
        private static void NewMenuOption()
        {
            new GameObject("Seed", typeof(Seed));
        }
    }
}