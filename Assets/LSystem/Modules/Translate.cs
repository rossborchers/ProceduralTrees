using UnityEngine;
using System.Collections.Generic;
using System;

namespace LSystem
{
    public class Translate : Module
    {
        [SerializeField]
        protected float distanceMax;

        [SerializeField]
        protected float distanceMin;

        public override void Bake(ParameterBundle bundle)
        {
            AnyExecute(bundle);
        }

        public override void Execute(ParameterBundle bundle)
        {
            AnyExecute(bundle);
        }

        protected void AnyExecute(ParameterBundle bundle)
        {
            bool fatal = false;
            Sentence sentence;
            CharGameObjectDict implementations;
            RuleSet rules;
            if (!GetCoreParameters(bundle, out sentence, out implementations, out rules)) fatal = true;

            Vector3 heading;
            if (bundle.Get("Heading", out heading))
            {
                Debug.Log(UnityEngine.Random.Range(distanceMin, distanceMax) * heading);
                transform.Translate(UnityEngine.Random.Range(distanceMin, distanceMax) * heading);
            }
            else
            {
                Debug.LogWarning("Default parameter 'Heading' missing. Skipping Translation", gameObject);
            }
            if (!fatal)
            {
                if(baked) BakeNextModule(transform, sentence, implementations, rules, bundle);
                else EnqueueProcessNextModule(transform, sentence, implementations, rules, bundle);
            }
            Destroy(gameObject);
        }
    }
}