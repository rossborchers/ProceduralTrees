using UnityEngine;
using System.Collections.Generic;
using System.Collections;

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
        protected Module previous;
        protected bool dead;

        public static int MaximumNewModulesPerFrame{ get; set; }

        Queue<IEnumerator> nextModuleProcessors = new Queue<IEnumerator>();
        public void Update()
        {

        }

        public void RegisterrocessNextModule(Sentence sentence, SerializableDictionary<char, GameObject> implementation, RuleSet rules, ParameterBundle bundle)
        {
            ProcessNextModule(sentence, implementation, rules, bundle);
        }

        void ProcessNextModule(Sentence sentence, SerializableDictionary<char, GameObject> implementation, RuleSet rules, ParameterBundle bundle)
        {
            Profiler.BeginSample("LSystem.Module.ProcessNextModule");

            if (dead) return;

            GameObject module;
            char symbol = '\0';
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
                    int iterations;
                    if (bundle.Get("Generation", out generation))
                    {
                        generation++;
                        if (bundle.Get("Iterations", out iterations))
                        {
                            if (generation > iterations)
                            {
                                //Max iterations reached.
                                return;
                            }
                        }
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
            KeyValuePair<GameObject, Sentence> newPair = new KeyValuePair<GameObject, Sentence>(module, sentence);

            Profiler.EndSample();

            ExecuteModule(newPair, bundle, symbol);
        }


        public void ExecuteModule(KeyValuePair<GameObject, Sentence> moduleSentancePair, ParameterBundle bundle, char symbol)
        {
            Profiler.BeginSample("LSystem.Module.ExecuteModule");

            if (moduleSentancePair.Key == null) return;

            ParameterBundle newBundle = new ParameterBundle(bundle);
            newBundle.SetOrPut("Sentence", moduleSentancePair.Value);

            GameObject moduleInstance = Object.Instantiate(moduleSentancePair.Key);
            Module module = moduleInstance.GetComponent<Module>();
            if (this.ethereal)
            {
                // Since this module is ethereal we have to set the parent to this modules parent
                // Note that this behavior is recursive up to the first non ethereal module or null.
                module.previous = transform.parent.GetComponent<Module>();
                moduleInstance.transform.SetParent(transform.parent, true);
            }
            else
            {
                moduleInstance.transform.SetParent(transform, true);
                module.previous = transform.GetComponent<Module>();
            }
            
            module.symbol = symbol;

            Profiler.EndSample();

            module.Execute(newBundle);
        }

        protected void AssignPrevious(Module next, Module previous)
        {
            next.previous = previous;
        }

        protected void Kill(Module toKill)
        {
            toKill.dead = true;
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

        public bool GetPositionParameters(ParameterBundle bundle, out int generation, out Vector3 heading)
        {
            bool success = true;
            if (!bundle.Get("Generation", out generation))
            {
                success = false;
                Debug.LogError("Default parameter 'Generation' missing.", gameObject);
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


