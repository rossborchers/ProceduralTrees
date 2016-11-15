using UnityEngine;
using System.Collections.Generic;

namespace LSystem
{
    public class Split : Module
    {
        [SerializeField] char splitEndDelimiter = ']';

        Split() :base()
        {
            ethereal = true;
        }

        public override void Execute(ParameterBundle bundle)
        {
            Sentence sentence;
            CharGameObjectDict implementations;
            RuleSet rules;
            if (!GetCoreParameters(bundle, out sentence, out implementations, out rules)) return;

            int bracketCount = 1;
            sentence.PushPosition();
            while (sentence.HasNext())
            {
                char c = sentence.Next();

                if (c == splitEndDelimiter) bracketCount--;
                else if (c == symbol) bracketCount++;

                if (bracketCount == 0) break;
            }

            Sentence split = sentence.PopPositionAndCut();

            RegisterrocessNextModule(sentence, implementations, rules, bundle);
            RegisterrocessNextModule(split, implementations, rules, bundle);
            Destroy(gameObject);
        }
    }
}