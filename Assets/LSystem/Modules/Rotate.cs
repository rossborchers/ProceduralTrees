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
            if (!bundle.Get("Sentence", out sentence)) { fatal = true; Debug.LogError("Default parameter 'Sentence' missing.", gameObject); }
            if (!bundle.Get("Implementations", out implementations)) { fatal = true; Debug.LogError("Default parameter 'Implementations' missing.", gameObject); }
            if (!bundle.Get("RuleSet", out rules)) { fatal = true; Debug.LogError("Default parameter 'RuleSet' missing.", gameObject); }

            Vector3 heading;
            if (bundle.Get("Heading", out heading))
            {
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
                ProcessNextModule(sentence, implementations, rules, bundle);
            }
            Destroy(gameObject);
        }
    }
}