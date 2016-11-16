using System;
using System.Collections.Generic;
using UnityEngine;

namespace LSystem
{
    /// <summary>
    /// Allow LSystem modules to exchange information without knowledge about the others implementation.
    /// </summary>
    [Serializable]
    public class ParameterBundle : ISerializationCallbackReceiver
    {
        // Unity wont serialize object references as their subclasses, so we serialize them in a temp object
        [Serializable]
        public class SerializedObjectStore 
        {
            [SerializeField]
            public StringIntDict intValues = new StringIntDict();
            [SerializeField]
            public StringFloatDict floatValues = new StringFloatDict();
            [SerializeField]
            public StringBoolDict boolValues = new StringBoolDict();
            [SerializeField]
            public StringVec2Dict vec2Values = new StringVec2Dict();
            [SerializeField]
            public StringVec3Dict vec3Values = new StringVec3Dict();
            [SerializeField]
            public StringVec4Dict vec4Values = new StringVec4Dict();
            [SerializeField]
            public StringAnimationCurveDict animCurveValues = new StringAnimationCurveDict();
            [SerializeField]
            public StringColorDict colorValues = new StringColorDict();

            public void Clear()
            {
                intValues.Clear();
                floatValues.Clear();
                boolValues.Clear();
                vec2Values.Clear();
                vec3Values.Clear();
                vec4Values.Clear();
                animCurveValues.Clear();
                colorValues.Clear();
            }
        }

        [SerializeField]
        SerializedObjectStore store = new SerializedObjectStore();

        [SerializeField] protected bool serialized;

        [NonSerialized]
        protected Dictionary<string, object> parameters = new Dictionary<string, object>();

        public ParameterBundle()
        {
        }

        public int Count { get { return parameters.Count; } private set { } }

        public ParameterBundle(ParameterBundle original)
        {
            //Make new dictionary and add values
            parameters = new Dictionary<string, object>(original.parameters.Count, original.parameters.Comparer);
            foreach(KeyValuePair<string, object> pair in original.parameters)
            {
                parameters.Add(pair.Key, pair.Value);
            }
        }

        public bool Exists(string key)
        {
            return parameters.ContainsKey(key);
        }

        public bool Get<T>(string key, out T value)
        {
            object baseInstance;
            if(parameters.TryGetValue(key, out baseInstance))
            {
                if (baseInstance.GetType() == typeof(T))
                {
                    value = (T)baseInstance;
                    return true;
                }
            }
            value = default(T);
            return false;
        }

        public bool Put<T>(string key, T value)
        {
            if (value != null && !parameters.ContainsKey(key))
            {
                parameters.Add(key, value);
                return true;
            }
            return false;
        }

        public bool Set<T>(string key, T value)
        {
            if (value != null && parameters.ContainsKey(key))
            {
                if (parameters[key].GetType() == typeof(T))
                {
                    parameters[key] = value;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// If the Key value Pair exists it is set to value, else its added to the bundle.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool SetOrPut<T>(string key, T value)
        {
            bool result = false;
            if (Set(key, value)) result = true;
            else if (Put(key, value)) result = true;
            return result;
        }

        /// <summary>
        /// Merge multiple ParameterBundles into one. Note that parameters with the same key will override each other and parameters with the same key but different value will be ignored
        /// </summary>
        /// <param name="bundles"></param>
        /// <returns></returns>
        public static ParameterBundle Merge(List<ParameterBundle> bundles)
        {
            ParameterBundle merged = new ParameterBundle();
            foreach(ParameterBundle bundle in bundles)
            {
                foreach(KeyValuePair<string, object> pair in bundle.parameters)
                {
                    if (!merged.Set(pair.Key, pair.Value))
                    {
                        merged.Put(pair.Key, pair.Value);
                    }
                }
            }
            return merged;
        }

        public Dictionary<string, object> StoreDictionary
        {
            get
            {
                return LoadFromStore(store);
            }
            set
            {
                store = SaveToStore(value); 
            }
        }
       
        public void OnBeforeSerialize()
        {
            if (serialized) return;
            serialized = true;

            store.Clear();
            store = SaveToStore(parameters); 
        }

        // Load dictionary from lists
        public void OnAfterDeserialize()
        {
            if (serialized)
            {
                parameters.Clear();
                parameters = LoadFromStore(store);
                serialized = false;
            }
        }

        Dictionary<string, object> LoadFromStore(SerializedObjectStore store)
        {
            Dictionary<string, object> merged = new Dictionary<string, object>();
            foreach (KeyValuePair<string, int> pair in store.intValues) merged.Add(pair.Key, pair.Value);
            foreach (KeyValuePair<string, float> pair in store.floatValues) merged.Add(pair.Key, pair.Value);
            foreach (KeyValuePair<string, bool> pair in store.boolValues) merged.Add(pair.Key, pair.Value);
            foreach (KeyValuePair<string, Vector2> pair in store.vec2Values) merged.Add(pair.Key, pair.Value);
            foreach (KeyValuePair<string, Vector3> pair in store.vec3Values) merged.Add(pair.Key, pair.Value);
            foreach (KeyValuePair<string, Vector4> pair in store.vec4Values) merged.Add(pair.Key, pair.Value);
            foreach (KeyValuePair<string, AnimationCurve> pair in store.animCurveValues) merged.Add(pair.Key, pair.Value);
            foreach (KeyValuePair<string, Color> pair in store.colorValues) merged.Add(pair.Key, pair.Value);
            return merged;
        }

        SerializedObjectStore SaveToStore(Dictionary<string, object> dict)
        {
            SerializedObjectStore store = new SerializedObjectStore();
            foreach (KeyValuePair<string, object> pair in dict)
            {
                Type type = pair.Value.GetType();
                switch (type.Name)
                {
                    case "Int32":
                        store.intValues.Add(pair.Key, (int)pair.Value);
                        break;
                    case "Single":
                        store.floatValues.Add(pair.Key, (float)pair.Value);
                        break;
                    case "Boolean":
                        store.boolValues.Add(pair.Key, (bool)pair.Value);
                        break;
                    case "Vector2":
                        store.vec2Values.Add(pair.Key, (Vector2)pair.Value);
                        break;
                    case "Vector3":
                        store.vec3Values.Add(pair.Key, (Vector3)pair.Value);
                        break;
                    case "Vector4":
                        store.vec4Values.Add(pair.Key, (Vector4)pair.Value);
                        break;
                    case "AnimationCurve":
                        store.animCurveValues.Add(pair.Key, (AnimationCurve)pair.Value);
                        break;
                    case "Color":
                        store.colorValues.Add(pair.Key, (Color)pair.Value);
                        break;
                }
            }
            return store;
        }
    }
}
