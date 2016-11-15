using UnityEngine;
using System.Collections.Generic;

namespace LSystem
{
    public class Rotate : Module
    {
        [SerializeField]
        protected Vector3 eulerAnglesMax;

        [SerializeField]
        protected Vector3 eulerAnglesMin;

        [SerializeField]
        protected bool clearPrevious = false;

        Rotate() :base()
        {
            ethereal = true;
        }

        public override void Execute(ParameterBundle bundle)
        {
            bool fatal = false;
            Sentence sentence;
            CharGameObjectDict implementations;
            RuleSet rules;
            if (!GetCoreParameters(bundle, out sentence, out implementations, out rules)) fatal = true;

            Vector3 heading;
            if (bundle.Get("Heading", out heading))
            {
                if (clearPrevious)
                {
                    heading = Vector3.up;
                }

                heading = Quaternion.Euler(new Vector3(Random.Range(eulerAnglesMin.x, eulerAnglesMax.x),
                                                                 Random.Range(eulerAnglesMin.y, eulerAnglesMax.y),
                                                                 Random.Range(eulerAnglesMin.z, eulerAnglesMax.z))) * heading;
                bundle.Set("Heading", heading);
            }
            else
            {
                Debug.LogWarning("Default parameter 'Heading' missing. Skipping Rotation", gameObject);
            }
            if(!fatal)
            {
                RegisterrocessNextModule(sentence, implementations, rules, bundle);
            }
            Destroy(gameObject);
        }
    }
}