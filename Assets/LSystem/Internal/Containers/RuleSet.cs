using UnityEngine;
using System;

namespace LSystem
{
    [Serializable]
    public class CharStringDict : SerializableDictionary<char, string>{}

    [Serializable]
    public class RuleSet
    {
        public CharStringDict Rules { get { return rules; } private set { } }
        [SerializeField]
        protected CharStringDict rules = new CharStringDict();

        public Sentence NextGeneration(Sentence before)
        {
            string after = "";
            char[] arr = before.ToCharArray();
            foreach(char c in arr)
            {
                string result;
                if (rules.TryGetValue(c, out result))
                {
                    after += result;
                }
            }
            return new Sentence(after);
        }
    }

}
