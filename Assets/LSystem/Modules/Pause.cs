using UnityEngine;
using System.Collections;

namespace LSystem
{
    public class Pause : Module
    {
        [SerializeField]
        float maxTime;

        [SerializeField]
        float minTime;

        Pause() : base()
        {
            ethereal = true;
        }

        public override void Execute(ParameterBundle bundle)
        {
            StartCoroutine(Wait(bundle));
        }

        IEnumerator Wait(ParameterBundle bundle)
        {
            Sentence sentence;
            CharGameObjectDict implementations;
            RuleSet rules;

            if (GetCoreParameters(bundle, out sentence, out implementations, out rules))
            {
                yield return new WaitForSeconds(Random.Range(minTime, maxTime));
                RegisterrocessNextModule(sentence, implementations, rules, bundle);
            }
            Destroy(gameObject);
        }
    }
}