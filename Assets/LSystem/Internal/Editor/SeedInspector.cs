using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using System;
using System.Linq;
using System.IO;

namespace LSystem
{
    ///<summary>
    /// Custom Inspector for seed. Responsible for drawing SerializeDictionaries.
    ///</summary>
    [CanEditMultipleObjects]
    [CustomEditor(typeof(Seed))]
    public class SeedInspector : Editor
    {
        public void OnEnable()
        {
            serIsRoot = serializedObject.FindProperty("isRoot");
            serInheritRotation = serializedObject.FindProperty("inheritRotation");
            serAxiom = serializedObject.FindProperty("axiom");

            serGenerateMode = serializedObject.FindProperty("generateMode");
            serIterations = serializedObject.FindProperty("iterations");

            serBaked = serializedObject.FindProperty("baked");
            serPrefabIdentifier = serializedObject.FindProperty("prefabIdentifier");

            serBakedRotationMax = serializedObject.FindProperty("bakedRotationMax");
            serBakedRotationMin = serializedObject.FindProperty("bakedRotationMin");
            serBakedScaleMax = serializedObject.FindProperty("bakedScaleMax");
            serBakedScaleMin = serializedObject.FindProperty("bakedScaleMin");

            serBakedScaleOnSpawn = serializedObject.FindProperty("bakedScaleOnSpawn");
            serBakedScaleTime = serializedObject.FindProperty("bakedScaleTime");
        }

        public override void OnInspectorGUI()
        {
            // Avoid setting values when animating, since during record the changes will automatically keyframe.
            EditorWindow window = EditorWindow.focusedWindow;
            if (window != null && window.GetType().Name == "AnimationWindow") 
            {
                return;
            }

            serializedObject.Update();

            // Can multi edit
            EditorGUILayout.PropertyField(serIsRoot);
            EditorGUILayout.PropertyField(serInheritRotation);
            EditorGUILayout.PropertyField(serAxiom);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(serGenerateMode);
            EditorGUILayout.PropertyField(serIterations);
           
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(serBaked);
            if(serBaked.boolValue)
            {
                EditorGUI.indentLevel++;
              
                EditorGUILayout.PropertyField(serBakedRotationMax);
                EditorGUILayout.PropertyField(serBakedRotationMin);
                EditorGUILayout.PropertyField(serBakedScaleMax);
                EditorGUILayout.PropertyField(serBakedScaleMin);
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(serPrefabIdentifier);
                EditorGUILayout.Space();

                EditorGUILayout.PropertyField(serBakedScaleOnSpawn);
                if (serBakedScaleOnSpawn.boolValue)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(serBakedScaleTime);
                    EditorGUI.indentLevel--;
                }

                EditorGUI.indentLevel--;
            }

            serializedObject.ApplyModifiedProperties();

            //Can't multi edit
            Undo.RecordObject((Seed)target, "Seed Inspector modified");

            DrawStartingParams();

            DrawRules();

            DrawImplementations();


            if(PrefabUtility.GetPrefabParent(target) != null)
            {
                EditorGUILayout.HelpBox("Unapplied prefab changes will be lost on play or editor close.", MessageType.Warning);
            }
        }

        protected void DrawStartingParams()
        {
            Dictionary<string, object> dict = ((Seed)target).StartingParameters.StoreDictionary;
            List<string> keys = dict.Keys.ToList();
            List<object> values = dict.Values.ToList();
            
            if ((startingParamsFoldout = EditorGUILayout.Foldout(startingParamsFoldout, "Starting Parameters")))
            {
                Rect controlRect;

                List<string> keysToRemove = new List<string>();
                List<object> valuesToRemove = new List<object>();

                for (int i = 0; i < dict.Keys.Count; i++)
                {
                    controlRect = EditorGUILayout.GetControlRect(false);
                    Rect keyRect = new Rect(controlRect.position, new Vector2(controlRect.size.x / 2, controlRect.size.y));
                    Rect valueRect = new Rect(new Vector2(controlRect.position.x + controlRect.size.x / 2, controlRect.position.y),
                                              new Vector2(controlRect.size.x / 2 - 25, controlRect.size.y));
                    Rect removeRect = new Rect(new Vector2(controlRect.position.x + controlRect.size.x - 20, controlRect.position.y),
                                            new Vector2(20, controlRect.size.y));
                    string newKey = EditorGUI.TextField(keyRect, keys[i]);
                    //Make sure key is unique.
                    if (newKey != keys[i])
                    {
                        for (int j = 0; j < keys.Count; j++)
                        {
                            if (keys[j] == newKey && i != j)
                            {
                                keys[j] = GenerateGUIDString(10);
                            }
                        }
                    }

                    keys[i] = newKey;
                    values[i] = DrawObject(valueRect, values[i]);

                    if (!EditorGUI.Toggle(removeRect, true))
                    {
                        keysToRemove.Add(keys[i]);
                        valuesToRemove.Add(values[i]);
                    }
                }
                foreach (string s in keysToRemove) keys.Remove(s);
                foreach (object o in valuesToRemove) values.Remove(o);

                controlRect = EditorGUILayout.GetControlRect(false);
                Rect popupRect = new Rect(controlRect.position, new Vector2(controlRect.size.x / 4 * 3, controlRect.size.y));
                Rect addRect = new Rect(new Vector2(controlRect.position.x + controlRect.size.x / 4 * 3, controlRect.position.y),
                                          new Vector2(controlRect.size.x / 4, controlRect.size.y));
                startingParamAddSelectedIndex = EditorGUI.Popup(popupRect, startingParamAddSelectedIndex,
                                                         new string[] { "Integer", "Float", "Boolean","Vector2",
                                                                       "Vector3","Vector4","AnimationCurve", "Color" });
                if (GUI.Button(addRect, "Add"))
                {
                    switch (startingParamAddSelectedIndex)
                    {
                        case 0:
                            int intValue = 0;
                            values.Add(intValue);
                            keys.Add(GenerateGUIDString(10));
                            break;
                        case 1:
                            float floatVlaue = 0f;
                            values.Add(floatVlaue);
                            keys.Add(GenerateGUIDString(10));
                            break;
                        case 2:
                            bool boolValue = false;
                            values.Add(boolValue);
                            keys.Add(GenerateGUIDString(10));
                            break;
                        case 3:
                            Vector2 vec2Value = Vector2.zero;
                            values.Add(vec2Value);
                            keys.Add(GenerateGUIDString(10));
                            break;
                        case 4:
                            Vector3 vec3Value = Vector3.zero;
                            values.Add(vec3Value);
                            keys.Add(GenerateGUIDString(10));
                            break;
                        case 5:
                            Vector4 vec4Value = Vector4.zero;
                            values.Add(vec4Value);
                            keys.Add(GenerateGUIDString(10));
                            break;
                        case 6:
                            AnimationCurve curveValue = new AnimationCurve();
                            values.Add(curveValue);
                            keys.Add(GenerateGUIDString(10));
                            break;
                        case 7:
                            Color colorValue = Color.white;
                            values.Add(colorValue);
                            keys.Add(GenerateGUIDString(10));
                            break;
                        default:
                            Debug.LogError("starting parameter popup index is not valid.");
                            break;
                    }
                    
                } //Add Button
                EditorGUILayout.Space();

                Dictionary<string, object> returnDict = new Dictionary<string, object>();
                for(int i = 0; i < keys.Count; i++)
                {
                 
                    returnDict.Add(keys[i], values[i]);
                }
                ((Seed)target).StartingParameters.StoreDictionary = returnDict;
            }
        }

        protected void DrawRules()
        {
            RuleSet ruleSet = ((Seed)target).Rules;
            List<char> keys = ruleSet.Rules.KeyList;
            List<string> values = ruleSet.Rules.ValueList;

            if (values.Count != keys.Count)
            {
                Debug.LogError("RuleSet value count (" + values.Count + ") not equal to key count(" + keys.Count + ")");
                keys.Clear();
                values.Clear();
            }

            if ((ruleSetFoldout = EditorGUILayout.Foldout(ruleSetFoldout, "Rules")))
            {
                Rect controlRect;
                for (int i = 0; i < keys.Count; i++)
                {
                    controlRect = EditorGUILayout.GetControlRect(false);
                    Rect keyRect = new Rect(controlRect.position, new Vector2(controlRect.size.x / 2, controlRect.size.y));
                    Rect valueRect = new Rect(new Vector2(controlRect.position.x + controlRect.size.x / 2, controlRect.position.y),
                                              new Vector2(controlRect.size.x / 2 - 25, controlRect.size.y));
                    Rect removeRect = new Rect(new Vector2(controlRect.position.x + controlRect.size.x - 25, controlRect.position.y),
                                            new Vector2(25, controlRect.size.y));

                    string newKeyStr = EditorGUI.TextField(keyRect, "" + keys[i]);
                    char newKey = (char)33;
                    if(newKeyStr.Length > 0) newKey = newKeyStr.ToCharArray()[0];

                    //Make sure key is unique.
                    if (newKey != keys[i])
                    {
                        int max = 33;
                        bool hit = false;
                        for (int j = 0; j < keys.Count; j++)
                        {
                            if (keys[j] == newKey && i != j)
                            {
                                hit = true;
                            }

                            if (keys[j] > max) max = keys[j];
                        }
                        if (hit) newKey = (char)++max;
                    }

                    keys[i] = newKey;

                    values[i] = EditorGUI.TextField(valueRect, values[i]);
                    if (!EditorGUI.Toggle(removeRect, true))
                    {
                        keys.RemoveAt(i);
                        values.RemoveAt(i);
                    }
                }

                controlRect = EditorGUILayout.GetControlRect(false);
                Rect addRect = new Rect(new Vector2(controlRect.position.x + controlRect.size.x / 4 * 3, controlRect.position.y), 
                    new Vector2(controlRect.size.x / 4, controlRect.size.y));
                if (GUI.Button(addRect,"Add"))
                {
                    //to avoid duplicate keys
                    char newKey = (char)33;
                    int max = 33;
                    for (int i = 0; i < keys.Count; i++)
                    {
                        if (keys[i] > max) max = keys[i];
                    }
                    newKey = (char)++max;
                    keys.Add(newKey);
                    values.Add("");
                } //Add Button          
                EditorGUILayout.Space();
            }

        }

        protected void DrawImplementations()
        {
            CharGameObjectDict implementations = ((Seed)target).Implementations;
            List<char> keys = implementations.KeyList;
            List<GameObject> values = implementations.ValueList;
           

            if (values.Count != keys.Count)
            {
                Debug.LogError("Implementations value count (" + values.Count + ") not equal to key count(" + keys.Count + ")");
                keys.Clear();
                values.Clear();
            }

            if ((implementationFoldout = EditorGUILayout.Foldout(implementationFoldout, "Implementations")))
            {
                Rect controlRect;
                for (int i = 0; i < keys.Count; i++)
                {
                    controlRect = EditorGUILayout.GetControlRect(false);
                    Rect keyRect = new Rect(controlRect.position, new Vector2(controlRect.size.x / 2, controlRect.size.y));
                    Rect valueRect = new Rect(new Vector2(controlRect.position.x + controlRect.size.x / 2, controlRect.position.y),
                                              new Vector2(controlRect.size.x / 2 - 25, controlRect.size.y));
                    Rect removeRect = new Rect(new Vector2(controlRect.position.x + controlRect.size.x - 25, controlRect.position.y),
                                            new Vector2(25, controlRect.size.y));
                    string newKeyStr = EditorGUI.TextField(keyRect, "" + keys[i]);
                    char newKey = (char)33;
                    if (newKeyStr.Length > 0) newKey = newKeyStr.ToCharArray()[0];
                    //Make sure key is unique.
                    if (newKey != keys[i])
                    {
                        int max = 33;
                        bool hit = false;
                        for (int j = 0; j < keys.Count; j++)
                        {
                            if (keys[j] == newKey && i != j)
                            {
                                hit = true;
                            }

                            if (keys[j] > max) max = keys[j];
                        }
                        if (hit) newKey = (char)++max;
                    }

                    keys[i] = newKey;

                    values[i] = (GameObject)EditorGUI.ObjectField(valueRect, values[i], values[i].GetType(), false);
                    if (!EditorGUI.Toggle(removeRect, true))
                    {
                        keys.RemoveAt(i);
                        values.RemoveAt(i);
                    }
                }


                string[] allPaths = Directory.GetDirectories(Application.dataPath, "LSystem", SearchOption.AllDirectories);
                if (allPaths.Length > 1) Debug.LogWarning("There is more than one LSystem directory in the project, Prefab root may be calculated incorrectly.");
                if (allPaths.Length <= 0)
                {
                    Debug.LogError("Cannot find LSystem Directory.");
                }

                DrawNewImplementations(keys, values, allPaths);
                DrawExistingImplementations(keys, values, allPaths);

                EditorGUILayout.Space();
            } 
        }

        protected void DrawNewImplementations(List<char> keys, List<GameObject> values, string[] allPaths)
        {
            // New implementation 
            // http://stackoverflow.com/questions/857705/get-all-derived-types-of-a-type#answer-17680332 
            // Resources fails with polymorphism so we have to do some reflection.
            if (allModuleTypes == null)
            {
                Type[] allModuleTypesArr = (from domainAssembly in AppDomain.CurrentDomain.GetAssemblies()
                                            from assemblyType in domainAssembly.GetTypes()
                                            where typeof(Module).IsAssignableFrom(assemblyType)
                                            select assemblyType).ToArray();
                allModuleTypes = new List<Type>(allModuleTypesArr);
            }

            List<string> types = new List<string>();
            for (int i = 0; i < allModuleTypes.Count; i++)
            {
                if (allModuleTypes[i].IsAbstract || allModuleTypes[i].IsInterface)
                {
                    allModuleTypes.Remove(allModuleTypes[i]);
                }
                else types.Add(allModuleTypes[i].Name);
            }


            Rect controlRect = EditorGUILayout.GetControlRect(false);
            Rect popupRect = new Rect(controlRect.position, new Vector2(controlRect.size.x / 4 * 3, controlRect.size.y));
            Rect addRect = new Rect(new Vector2(controlRect.position.x + controlRect.size.x / 4 * 3, controlRect.position.y),
                                      new Vector2(controlRect.size.x / 4, controlRect.size.y));

            implementationAddNewIndex = EditorGUI.Popup(popupRect, "New", implementationAddNewIndex, types.ToArray());
            if (GUI.Button(addRect, "Add"))
            {
                if (implementationAddNewIndex < allModuleTypes.Count)
                {
                    Type type = allModuleTypes[implementationAddNewIndex];
                    if (allPaths.Length > 0)
                    {
                        string path = allPaths[0].Remove(0, Application.dataPath.Length + 1);
                        string guidStr = GenerateGUIDString(5);
                        string prefabName = type.Name +"_"+ guidStr;
                        UnityEngine.Object prefab = PrefabUtility.CreateEmptyPrefab("Assets/" + path + "/Resources/ModulePrefabs/" + prefabName + ".prefab");
                        GameObject go = new GameObject("TempPrefabInstance", type);
                        values.Add(PrefabUtility.ReplacePrefab(go, prefab, ReplacePrefabOptions.ConnectToPrefab));
                        DestroyImmediate(go);
                        keys.Add((char)(33 + keys.Count));
                    }
                }
            }
        }

        protected void DrawExistingImplementations(List<char> keys, List<GameObject> values, string[] allPaths)
        {
            // Existing implementation 
            List<string> names = new List<string>();
            List<GameObject> prefabs = new List<GameObject>();
            if (allPaths.Length > 0)
            {
                GameObject[] prefabsArr = Resources.LoadAll<GameObject>("ModulePrefabs");
                prefabs = new List<GameObject>(prefabsArr);
                for (int i = 0; i < prefabs.Count; i++)
                {
                    if (prefabs[i].GetComponent<Module>() != null)
                    {
                        names.Add(prefabs[i].name);
                    }
                    else prefabs.RemoveAt(i);
                }
            }

            if (names.Count > 0)
            {
                Rect controlRect = EditorGUILayout.GetControlRect(false);
                Rect popupRect = new Rect(controlRect.position, new Vector2(controlRect.size.x / 4 * 3, controlRect.size.y));
                Rect addRect = new Rect(new Vector2(controlRect.position.x + controlRect.size.x / 4 * 3, controlRect.position.y),
                                          new Vector2(controlRect.size.x / 4, controlRect.size.y));

                implementationAddExistingIndex = EditorGUI.Popup(popupRect, "Existing", implementationAddExistingIndex, names.ToArray());

                if (GUI.Button(addRect, "Add"))
                {
                    if (implementationAddExistingIndex < prefabs.Count)
                    {
                        if (allPaths.Length > 0)
                        {
                            GameObject existing = prefabs[implementationAddExistingIndex];
                            if (existing != null)
                            {
                                values.Add(existing);
                                keys.Add((char)(33 + keys.Count));
                            }
                            else
                            {
                                Debug.LogWarning("Cannot find " + names[implementationAddExistingIndex]);
                            }
                        }
                    }

                }
            }
        }

        protected object DrawObject(Rect rect, object objectToDraw)
        {
            Type type = objectToDraw.GetType();
            if (type == typeof(int))
            {
                return EditorGUI.IntField(rect, (int)objectToDraw);
            }
            else if (type == typeof(float))
            {
                return EditorGUI.FloatField(rect, (float)objectToDraw);
            }
            else if (type == typeof(bool))
            {
                return EditorGUI.Toggle(rect, (bool)objectToDraw);
            }
            else if (type == typeof(string))
            {
                return EditorGUI.TextField(rect, (string)objectToDraw);
            }
            else if (type == typeof(Vector2))
            {
                return EditorGUI.Vector2Field(rect, "", (Vector2)objectToDraw);
            }
            else if (type == typeof(Vector3))
            {
                return EditorGUI.Vector3Field(rect, "", (Vector3)objectToDraw);
            }
            else if (type == typeof(Vector4))
            {
                return EditorGUI.Vector4Field(rect, "", (Vector4)objectToDraw);
            }
            else if (type == typeof(AnimationCurve))
            {
                return EditorGUI.CurveField(rect, (AnimationCurve)objectToDraw);
            }
            else if (type == typeof(Color))
            {
                return EditorGUI.ColorField(rect, (Color)objectToDraw);
            }
            else
            {
                EditorGUI.LabelField(rect, "Cant Draw Type!");
                return objectToDraw;
            }
        }

        protected string GenerateGUIDString(int length)
        {
            Guid g = Guid.NewGuid();
            return Convert.ToBase64String(g.ToByteArray()).Replace("=", "").Replace("/", "").Replace("+", "").Substring(0, length);
        }

        protected List<Type> allModuleTypes= null;

        protected int startingParamAddSelectedIndex;

        protected static bool startingParamsFoldout;
        protected static bool ruleSetFoldout;
        protected static bool implementationFoldout;

        protected List<KeyValuePair<string, Type>> modules;

        protected int implementationAddNewIndex;
        protected int implementationAddExistingIndex;

        protected SerializedProperty serIsRoot;
        protected SerializedProperty serInheritRotation;
        protected SerializedProperty serAxiom;

        protected SerializedProperty serGenerateMode;
        protected SerializedProperty serIterations;

        protected SerializedProperty serBaked;
        protected SerializedProperty serPrefabIdentifier;
        protected SerializedProperty serBakedRotationMax;
        protected SerializedProperty serBakedRotationMin;
        protected SerializedProperty serBakedScaleMax;
        protected SerializedProperty serBakedScaleMin;

        protected SerializedProperty serBakedScaleOnSpawn;
        protected SerializedProperty serBakedScaleTime;
    }
}