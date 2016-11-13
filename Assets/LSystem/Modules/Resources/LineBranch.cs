using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;

namespace LSystem
{
    public class LineBranch : Module 
    {
        [SerializeField]
        protected float localGrowMultiplier = 1f;

        [SerializeField]
        protected float generationCutoff = 4;

        [SerializeField]
        protected float startGrowSize = 1f;

        [SerializeField]
        protected float maxGrowSpeed = 4f;

        [SerializeField]
        protected float minGrowSpeed = 1f;

        [SerializeField]
        protected float sizeMultiplier = 0.85f;

        protected Vector3 end;
       
        public override void Execute(ParameterBundle bundle)
        {
            Vector3 position, heading;
            Sentence sentence;
            CharGameObjectDict implementations;
            int generation;
            RuleSet rules;

            if (!GetCoreParameters(bundle, out sentence, out implementations, out rules)
             || !GetPositionParameters(bundle, out generation, out position, out heading)) return;
            if ((generation > generationCutoff)) return;

            float size;
            if(bundle.Get("GrowSize", out size))
            {
                startGrowSize = size;
            }

            transform.position = position;
            end = position;

          
            StartCoroutine(Grow(sentence, heading, rules, implementations, generation, UnityEngine.Random.Range(minGrowSpeed, maxGrowSpeed), bundle));
        }

        IEnumerator Grow(Sentence sentence, Vector3 heading, RuleSet rules, CharGameObjectDict implementations, int generation, float growSpeed, ParameterBundle bundle)
        {
            transform.up = ((transform.position + heading) - transform.position).normalized;
            while(Vector3.Distance(transform.position, end) < startGrowSize)
            {
                end += heading * Mathf.Min(heading.magnitude * Time.deltaTime * growSpeed * localGrowMultiplier, startGrowSize);
                yield return null;
            }

            bundle.Set("Position", end);
            bundle.SetOrPut("GrowSize", startGrowSize * sizeMultiplier);
           
            ProcessNextModule(sentence, implementations, rules, bundle);
        }

        void OnDrawGizmos()
        {
            Gizmos.DrawLine(transform.position, end);
        }
    }

}
