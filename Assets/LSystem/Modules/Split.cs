using UnityEngine;

namespace LSystem
{
    /// <summary>
    /// Splits are responsible for forking modules and enabling geometric forks. 
    /// They are often conceptualized using turtle drawing as a position push(eg '[' ) and pop(eg ']') stack operations, however this implementation executes them in parallel.
    /// </summary>
    public class Split : Module
    {
        [SerializeField]
        [Tooltip("The delimiter that ends a split.")]
        char splitEndDelimiter = ']';

        Split() :base()
        {
            ethereal = true;
        }

        // Entry point when pre-baking LSystem.
        public override void Bake(ParameterBundle bundle)
        {
            AnyExecute(bundle);
        }

        // Entry point when dynamically executing LSystem.
        public override void Execute(ParameterBundle bundle)
        {
            AnyExecute(bundle);
        }

        // Encapsulates common functionality between Bake and Execute. 
        protected void AnyExecute(ParameterBundle bundle)
        {
            Sentence sentence;
            CharGameObjectDict implementations;
            RuleSet rules;
            if (!GetCoreParameters(bundle, out sentence, out implementations, out rules)) return;

            // Count brackets to determine which sentence piece to pull into a new fork
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

            //call next recursion
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