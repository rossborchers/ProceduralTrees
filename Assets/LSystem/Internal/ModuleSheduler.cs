using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace LSystem
{
    ///<summary>
    /// Runs a maximum number of module executions per frame to avoid performance drops with lots of branching  
    ///</summary>
    public class ModuleSheduler : MonoBehaviour
    {
        public int MaxNewModulesPerFrame { get { return maxNewModulesPerFrame; } set { maxNewModulesPerFrame = value; } }
        [SerializeField]
        protected int maxNewModulesPerFrame;

        protected void Awake()
        {
            maxNewModulesPerFrame = PlayerPrefs.GetInt("LSystem_MaxModulesPerFrame", 100);
        }

        Queue<IEnumerator> nextModuleProcessors = new Queue<IEnumerator>();
        protected void Update()
        {
            int modulesProcessed = 0;
            int numModules = nextModuleProcessors.Count;
            while (numModules > 0 && modulesProcessed < maxNewModulesPerFrame)
            {
                IEnumerator next = nextModuleProcessors.Dequeue();
                if (next.MoveNext())
                {
                    if (next.Current is YieldInstruction)
                    {
                        nextModuleProcessors.Enqueue(next);
                    }
                    else
                    {
                        Debug.Log("Unsupported return value from IEnumerator", gameObject);
                    }
                }
                numModules--;
                modulesProcessed++;
            }
        }

        public void EnqueueProcessNextModule(IEnumerator processor)
        {
            nextModuleProcessors.Enqueue(processor);
        }
    }
}