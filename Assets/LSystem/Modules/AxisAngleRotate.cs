using UnityEngine;
using System.Collections.Generic;
using System;

namespace LSystem
{
    public class AxisAngleRotate : Module
    {
        [SerializeField]
        protected Vector3 axis;

        [SerializeField]
        protected float angleMin;

        [SerializeField]
        protected float angleMax;

        AxisAngleRotate() : base()
        {
            ethereal = true;
        }

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
                heading = Quaternion.AngleAxis(UnityEngine.Random.Range(angleMin, angleMax), axis) * heading;
                bundle.Set("Heading", heading);
            }
            else
            {
                Debug.LogWarning("Default parameter 'Heading' missing. Skipping Rotation", gameObject);
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