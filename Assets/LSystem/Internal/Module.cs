using UnityEngine;
using System.Collections.Generic;


namespace LSystem
{
    /// <summary>
    /// LSystem symbol implementation base for modules that are components.
    /// </summary>
    public abstract class Module : MonoBehaviour
    {
        /// <summary>
        /// A unified LSystem symbol implementation execution point with a parameter list. 
        /// </summary>
        /// <param name="bundle">Bundle containing information collected by previous iterations</param>
        /// <returns>Bundle containing any information that previous Modules may be interested in.</returns>
        public abstract void Execute(ParameterBundle bundle);

        protected char symbol;

        protected bool ethereal;

        public void ProcessNextModule(Sentence sentence, SerializableDictionary<char, GameObject> implementation, RuleSet rules, ParameterBundle bundle)
        {
            GameObject module;
            char symbol;
            do
            {
                if (!sentence.HasNext())
                {
                    sentence = rules.NextGeneration(sentence);

                    if (!bundle.Set("Sentence", sentence))
                    {
                        Debug.LogError("Cannot set 'Sentence' parameter in GetAndExecuteModule", gameObject);
                    }

                    int generation;
                    if (bundle.Get("Generation", out generation))
                    {
                        generation++;
                        if (!bundle.Set("Generation", generation))
                        {
                            Debug.LogError("Cannot set 'Generation' parameter in GetAndExecuteModule", gameObject);
                        }
                    }
                    else Debug.LogError("Cannot get 'Generation' parameter in GetAndExecuteModule", gameObject);
                }
                symbol = sentence.Next();
                if (symbol == '\0') return; //Sentence is empty! Caused if rules do not generate anything from previous

            } while (!implementation.TryGetValue(symbol, out module));

            KeyValuePair < GameObject, Sentence > newPair = new KeyValuePair<GameObject, Sentence>(module, sentence);
            ExecuteModule(newPair, bundle, symbol);
        }

        public void ExecuteModule(KeyValuePair<GameObject, Sentence> moduleSentancePair, ParameterBundle bundle, char symbol)
        {
            if (moduleSentancePair.Key == null) return;

            ParameterBundle newBundle = new ParameterBundle(bundle);
            newBundle.SetOrPut("Sentence", moduleSentancePair.Value);

            GameObject moduleInstance = Object.Instantiate(moduleSentancePair.Key);
            if (this.ethereal)moduleInstance.transform.SetParent(transform.parent, true);
            else moduleInstance.transform.SetParent(transform, true);
            Module module = moduleInstance.GetComponent<Module>();
            module.symbol = symbol;
            module.Execute(newBundle);
        }

        // Retrial for parameters common to most LSystems are wrapped here for convenience.
        public bool GetCoreParameters(ParameterBundle bundle, out Sentence sentence, out CharGameObjectDict implementations, out RuleSet rules)
        {
            bool success = true;
            if (!bundle.Get("Sentence", out sentence))
            {
                success = false;
                Debug.LogError("Default parameter 'Sentence' missing.", gameObject);
            }
            if (!bundle.Get("Implementations", out implementations))
            {
                success = false;
                Debug.LogError("Default parameter 'Implementations' missing.", gameObject);
            }
            if (!bundle.Get("RuleSet", out rules))
            {
                success = false;
                Debug.LogError("Default parameter 'RuleSet' missing.", gameObject);
            }
            return success;
        }

        public bool GetPositionParameters(ParameterBundle bundle, out int generation, out Vector3 position, out Vector3 heading)
        {
            bool success = true;
            if (!bundle.Get("Generation", out generation))
            {
                success = false;
                Debug.LogError("Default parameter 'Generation' missing.", gameObject);
            }
            if (!bundle.Get("Position", out position))
            {
                success = false;
                Debug.LogError("Default parameter 'Position' missing.", gameObject);
            }
            if (!bundle.Get("Heading", out heading))
            {
                success = false;
                Debug.LogError("Default parameter 'Heading' missing.", gameObject);
            }
            return success;
        }
    }
}


