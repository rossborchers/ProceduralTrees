using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Enables dictionary serialization when non generic types derive from this.
/// Stores serialized list and converts it to and from a dictionary.
/// </summary>
/// <typeparam name="TKey"></typeparam>
/// <typeparam name="TValue"></typeparam>
[Serializable]
public class SerializableDictionary<TKey, TValue> : ISerializationCallbackReceiver, IEnumerable
{
    public List<TKey> KeyList { get { return keyList; } set { keyList = value; } }
    [SerializeField] protected List<TKey> keyList = new List<TKey>();

    public List<TValue> ValueList { get { return valueList; } set { valueList = value;} }
    [SerializeField] protected List<TValue> valueList = new List<TValue>();

    Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>();

    public int Count { get { return dictionary.Count; } private set { } }

    public IEqualityComparer<TKey> Comparer { get { return dictionary.Comparer; }  private set{}}

    [SerializeField]
    protected bool serialized;

    //Dictionary emulation 
    public SerializableDictionary()
    {

    }

    public SerializableDictionary(int count, IEqualityComparer<TKey> comparer)
    {
        dictionary = new Dictionary<TKey, TValue>(count, comparer);
    }
        
    public bool TryGetValue(TKey key, out TValue value)
    {
        return dictionary.TryGetValue(key, out value);
    }

    public bool ContainsKey(TKey key)
    {
        return dictionary.ContainsKey(key);
    }

    public void Add(TKey key, TValue value)
    {
        dictionary.Add(key, value);
    }

    public TValue this[TKey key]
    {
        get
        {
            return dictionary[key];
        }
        set
        {
            dictionary[key] = value;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return new SerializableDictionaryEnum(dictionary);
    }

    public void Clear()
    {
        dictionary.Clear();
        KeyList.Clear();
        valueList.Clear();
    }

    public class SerializableDictionaryEnum : IEnumerator
    {
        public SerializableDictionaryEnum(Dictionary<TKey, TValue> dictionary)
        {
            this.dictionary = dictionary;
            this.enumerator = dictionary.GetEnumerator();
        }

        Dictionary<TKey, TValue> dictionary;
        Dictionary<TKey, TValue>.Enumerator enumerator;

        object IEnumerator.Current
        {
            get
            {
                return enumerator.Current;
            }
        }

        public bool MoveNext()
        {
            return enumerator.MoveNext();
        }

        public void Reset()
        {
            enumerator = dictionary.GetEnumerator();
        }
    }
    
    public void OnBeforeSerialize()
    {
        if (serialized) return;
        serialized = true;

        keyList.Clear();
        valueList.Clear();
        foreach (KeyValuePair<TKey, TValue> pair in dictionary)
        {
            keyList.Add(pair.Key);
            valueList.Add(pair.Value);
        }
    }

    // Load dictionary from lists
    public void OnAfterDeserialize()
    {
        if (serialized) 
        {
            dictionary.Clear();
            for (int i = 0; i < keyList.Count; i++)
            {
                dictionary.Add(keyList[i], valueList[i]);
            }
            serialized = false;
        }
    }

}

[Serializable]
public class StringObjectDict : SerializableDictionary<string, object>
{
    public StringObjectDict() : base() { }
    public StringObjectDict(int count, IEqualityComparer<string> comparer) : base(count, comparer) { }
}
[Serializable] public class CharGameObjectDict : SerializableDictionary<char, GameObject> { }
[Serializable] public class CharStringDict : SerializableDictionary<char, string> { }
[Serializable] public class StringIntDict : SerializableDictionary<string, int> { }
[Serializable] public class StringFloatDict : SerializableDictionary<string, float> { }
[Serializable] public class StringBoolDict : SerializableDictionary<string, bool> { }
[Serializable] public class StringVec2Dict : SerializableDictionary<string, Vector2> { }
[Serializable] public class StringVec3Dict : SerializableDictionary<string, Vector3> { }
[Serializable] public class StringVec4Dict : SerializableDictionary<string, Vector4> { }
[Serializable] public class StringAnimationCurveDict : SerializableDictionary<string, AnimationCurve> { }
[Serializable] public class StringColorDict : SerializableDictionary<string, Color> { }



