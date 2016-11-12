
using UnityEngine;
using System.Collections.Generic;

namespace LSystem
{
    /// <summary>
    /// Abstracts common module logic for reuse.
    /// </summary>
    public static class ModuleUtil
    {
        /// <summary>
        /// Parse a LSystem sentence to find its next implementations <see cref="Module"/>s.
        /// </summary>
        /// <param name="sentance"></param>
        /// <param name="implementation"></param>
        /// <returns></returns>
        public static List<KeyValuePair<GameObject, Sentence>> CreateNextModules(Sentence sentance, SerializableDictionary<char, GameObject> implementation)
        {
            List<KeyValuePair<GameObject, Sentence>> modules = new List<KeyValuePair<GameObject, Sentence>>();

            int position = sentance.Position();
            char c = sentance.Next();

            // Branching code. Is there no way to implement this as a ScriptModule?
            while(c == '[')
            {
                string newSentanceStr = "";
                char[]  chars = sentance.ToCharArray();
                int start = position;
                int end = position;
                for (int i = position + 1; i < chars.Length && chars[i] != ']'; i++)
                {
                    if (chars[i] != ']')
                    {
                        newSentanceStr += chars[i];
                    }
                    end = i;
                }
                sentance.Remove(start, start - end + 1);

                List<KeyValuePair<GameObject, Sentence>> branchModules = CreateNextModules(new Sentence(newSentanceStr), implementation);
                foreach(KeyValuePair<GameObject, Sentence> pair in branchModules)
                {
                    modules.Add(pair);
                }

                c = sentance.PeekNext();
            }

            //normal module
            GameObject module;
            if(implementation.TryGetValue(c, out module))
            {
                modules.Add(new KeyValuePair<GameObject, Sentence>(module, sentance));
            }
            return modules;
        }


        /// <summary>
        /// Execute and pass a <see cref="ParameterBundle"/> to all modules in the list 
        /// </summary>
        /// <param name="modules">List of GameObjects with modules to call <see cref="Module.Execute(ParameterBundle)"/> on.</param>
        /// <param name="bundle">Bundle to pass to <see cref="Module.Execute(ParameterBundle)"/> </param>
        /// <returns></returns>
        public static List<ParameterBundle> ExecuteList(List<KeyValuePair<GameObject, Sentence>> moduleSentancePairs, ParameterBundle bundle)
        {
            List<ParameterBundle> bundles = new List<ParameterBundle>();
            foreach (KeyValuePair<GameObject, Sentence> pair in moduleSentancePairs)
            {
                ParameterBundle returnBundle = ExecuteModule(pair, bundle);
                if (returnBundle != null) bundles.Add(returnBundle);
            }
            return bundles;
        }

        public static ParameterBundle ExecuteModule(KeyValuePair<GameObject, Sentence> moduleSentancePair, ParameterBundle bundle)
        {
            ParameterBundle newBundle = new ParameterBundle(bundle);
            newBundle.SetOrPut("Sentence", moduleSentancePair.Value);

            GameObject moduleInstance = GameObject.Instantiate(moduleSentancePair.Key);
            return moduleInstance.GetComponent<Module>().Execute(newBundle);
        }
    }
}