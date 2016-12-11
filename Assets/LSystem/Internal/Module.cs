using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace LSystem
{
    /// <summary>
    /// LSystem symbol implementation base for module components. Core LSystem algorithm
    /// </summary>
    public abstract class Module : MonoBehaviour
    {
        /// <summary>
        /// A unified LSystem symbol implementation execution point with a parameter list for dynamic execution. 
        /// </summary>
        /// <param name="bundle">Bundle containing information collected by previous iterations</param>
        /// <returns>Bundle containing any information that previous Modules may be interested in.</returns>
        public abstract void Execute(ParameterBundle bundle);

        /// <summary>
        /// A unified LSystem symbol implementation execution point with a parameter list for pre-baked execution. 
        /// </summary>
        /// <param name="bundle">Bundle containing information collected by previous iterations</param>
        /// <returns>Bundle containing any information that previous Modules may be interested in.</returns>>
        public abstract void Bake(ParameterBundle bundle);

        // Serialization is required for Instantiate() to copy values. but we don't want the items visible.
        
        // The symbol that represents this module in the seed implementation parameters   
        [HideInInspector]
        [SerializeField]
        protected char symbol;

        // If a module is ethereal we cannot rely on it being alive.
        // It should not be incorporated into the hierarchy and no references that cant be broken should be kept.
        [HideInInspector]
        [SerializeField]
        protected bool ethereal;

        // The previous (Non ethereal) module. required for connecting the dots.
        [HideInInspector]
        [SerializeField]
        protected Module previous;

        // If a module is dead it should not carry on Executing modules. Used with baking.
        [HideInInspector]
        [SerializeField]
        protected bool dead;

        // Is this Module baked? Inherited from previous modules. 
        [HideInInspector]
        [SerializeField]
        protected bool baked;

        //used to compare "prefab" instances at runtime. this is then used to share meshes.
        [HideInInspector]
        [SerializeField]
        protected string prefabIdentifier = null;

        static ModuleSheduler sheduler;

        // Execute next module as a bake (no delay) operation.
        public void BakeNextModule(Transform caller, Sentence sentence, 
                                   SerializableDictionary<char, GameObject> implementation, RuleSet rules, ParameterBundle bundle)
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

        // Execute next module as a IEnumerator (undefined delay based on load) operation.
        public void EnqueueProcessNextModule(Transform caller, Sentence sentence, 
                                             SerializableDictionary<char, GameObject> implementation, RuleSet rules, ParameterBundle bundle)
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

        // Called from the module scheduler when there is time.
        IEnumerator EnumerableProcessNextModule(Module previous, Sentence sentence, 
                                                SerializableDictionary<char, GameObject> implementation, RuleSet rules, ParameterBundle bundle)
        {
            if(!ProcessNextModule(previous, sentence, implementation, rules, bundle, false))
            {
                yield break;
            }
        }

        // Parse the sentence, load symbol implementations and find the next module to execute. 
        // One module is executed at a time, and an internal pointer is adjusted to for the next iteration.
        public bool ProcessNextModule(Module previous, Sentence sentence, 
                                       SerializableDictionary<char, GameObject> implementation, RuleSet rules, ParameterBundle bundle, bool baked)
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

        // Once ProcessNextModule has found an executable module 
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

            if (prefabIdentifier != null) module.prefabIdentifier = prefabIdentifier;
            if (baked) module.Bake(newBundle);
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

        public bool GetPositionParameters(ParameterBundle bundle, out int generation, out Quaternion rotation)
        {
            bool success = true;
            if (!bundle.Get("Generation", out generation))
            {
                success = false;
                Debug.LogError("Default parameter 'Generation' missing.", gameObject);
            }
            if (!bundle.Get("Rotation", out rotation))
            {
                success = false;
                Debug.LogError("Default parameter 'Rotation' missing.", gameObject);
            }
            return success;
        }
    }
}


