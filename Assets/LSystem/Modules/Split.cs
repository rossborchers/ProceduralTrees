using UnityEngine;
using System.Collections.Generic;
using System;

namespace LSystem
{
    public class Split : Module
    {
        [SerializeField] char splitEndDelimiter = ']';

        Split() :base()
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

            if (baked)
            {
                BakeNextModule(transform, sentence, implementations, rules, bundle);
                BakeNextModule(transform, split, implementations, rules, bundle);
            }
            else
            {
                EnqueueProcessNextModule(transform, sentence, implementations, rules, bundle);
                EnqueueProcessNextModule(transform, split, implementations, rules, bundle);
            }

            Destroy(gameObject);
        }

    }
}