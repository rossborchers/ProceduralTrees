using UnityEngine;
using System;

namespace LSystem
{
    ///<summary>
    /// Stores rules (char -> string) relationships and uses them to evolve sentences.
    ///</summary>
    [Serializable]
    public class RuleSet
    {
        public bool Fertile { get { return fertile; }  set { fertile = value; } }
        [SerializeField]
        protected bool fertile = true;

        public CharStringDict Rules { get { return rules; } private set { } }
        [SerializeField]
        protected CharStringDict rules = new CharStringDict();

        public Sentence NextGeneration(Sentence before)
        {
            if (!fertile) return new Sentence("");

            string after = "";
            char[] arr = before.ToCharArray();
            foreach(char c in arr)
            {
                string result;
                if (rules.TryGetValue(c, out result))
                {
                    after += result;
                }
                else after += c;
            }
            return new Sentence(after);
        }
    }

}
