using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

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

        public abstract void Bake(ParameterBundle bundle);

        [SerializeField]
        protected char symbol;

        [SerializeField]
        protected bool ethereal;

        [SerializeField]
        protected Module previous;

        [SerializeField]
        protected bool dead;

        [SerializeField]
        protected bool baked;

        [SerializeField]
        protected string prefabIdentifier = null;

        static ModuleSheduler sheduler;

        public void BakeNextModule(Transform caller, Sentence sentence, SerializableDictionary<char, GameObject> implementation, RuleSet rules, ParameterBundle bundle)
        {
            Module previous;
            if (this.ethereal)
            {
                // Since this module is ethereal we have to set the parent to this modules parent
                // Note that this behavior is recursive up to the first non ethereal module or null.
                previous = caller.parent.GetComponent<Module>();
            }
            else
            {
                previous = caller.GetComponent<Module>();
            }
            ProcessNextModule(previous, sentence, implementation, rules, bundle, true);
        }

        public void EnqueueProcessNextModule(Transform caller, Sentence sentence, SerializableDictionary<char, GameObject> implementation, RuleSet rules, ParameterBundle bundle)
        {
            if(sheduler == null)
            {
                sheduler = FindObjectOfType<ModuleSheduler>();
                if(sheduler == null)
                {
                    GameObject shedulerObject = new GameObject("LSystemModuleSheduler");
                    sheduler = shedulerObject.AddComponent<ModuleSheduler>();
                    shedulerObject.hideFlags = HideFlags.HideInHierarchy;
                }
            }

            Module previous;
            if (this.ethereal)
            {
                // Since this module is ethereal we have to set the parent to this modules parent
                // Note that this behavior is recursive up to the first non ethereal module or null.
                previous = caller.parent.GetComponent<Module>();
            }
            else
            {
                previous = caller.GetComponent<Module>();
            }

            sheduler.EnqueueProcessNextModule(EnumerableProcessNextModule(previous, sentence, implementation, rules, bundle));
        }

        IEnumerator EnumerableProcessNextModule(Module previous, Sentence sentence, SerializableDictionary<char, GameObject> implementation, RuleSet rules, ParameterBundle bundle)
        {
            if(!ProcessNextModule(previous, sentence, implementation, rules, bundle, false))
            {
                yield break;
            }
        }

        public bool ProcessNextModule(Module previous, Sentence sentence, SerializableDictionary<char, GameObject> implementation, RuleSet rules, ParameterBundle bundle, bool baked)
        {
            if (dead) return false;
            Profiler.BeginSample("LSystem.Module.ProcessNextModule");

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
                                return false; 
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
                if (symbol == '\0') return false;  //Sentence is empty! Caused if rules do not generate anything from previous 

            } while (!implementation.TryGetValue(symbol, out module));
            KeyValuePair<GameObject, Sentence> newPair = new KeyValuePair<GameObject, Sentence>(module, sentence);

            Profiler.EndSample();

            ExecuteModule(previous, newPair, bundle, symbol, baked);

            return true;
        }

        public void ExecuteModule(Module previous, KeyValuePair<GameObject, Sentence> moduleSentancePair, ParameterBundle bundle, char symbol, bool baked)
        {
            if(previous == null) return; // if object is destroyed externally

            Profiler.BeginSample("LSystem.Module.ExecuteModule");

            if (moduleSentancePair.Key == null) return;

            ParameterBundle newBundle = new ParameterBundle(bundle);
            newBundle.SetOrPut("Sentence", moduleSentancePair.Value);

            GameObject moduleInstance = UnityEngine.Object.Instantiate(moduleSentancePair.Key);
            Module module = moduleInstance.GetComponent<Module>();


            module.previous = previous;
            moduleInstance.transform.SetParent(previous.transform, true);
        
            module.symbol = symbol;

            //Seeds are baked separately so their baked value must not be overwritten.
            //Is there a way to avoid this special case in module? 
            if(module.GetType() != typeof(Seed)) module.baked = baked;

            Profiler.EndSample();

            if (baked)
            {
                if (prefabIdentifier == null) prefabIdentifier = gameObject.name;
                module.Bake(newBundle);
            }
            else module.Execute(newBundle);
        }

        protected void AssignPrevious(Module next, Module previous)
        {
            next.previous = previous;
        }

      

        protected void SetPrefabIdentifier(Module module)
        {
            module.prefabIdentifier = gameObject.name;
        }

        public void Kill(Module toKill)
        {
            toKill.dead = true;
        }
        public bool Dead()
        {
            return dead;
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


