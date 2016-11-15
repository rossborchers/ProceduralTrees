using UnityEngine;
using System.Collections.Generic;
using System;

namespace LSystem
{
    /// <summary>
    /// Initialize an LSystem.
    /// </summary>
    public class Seed : Module
    {
        public enum GenMode
        {
            PreEdgeRewrite,
            IterativeNodeRewrite
        }

        public bool IsRoot { get { return isRoot; } private set { isRoot = value; } }
        [SerializeField]
        protected bool isRoot;

        public string Axiom { get { return axiom; } set { axiom = value; } }
        [SerializeField]
        protected string axiom;

        public bool InheritHeading { get { return isRoot; } private set { isRoot = value; } }
        [SerializeField] protected bool inheritHeading;

        public GenMode GenerateMode { get { return generateMode; } private set { generateMode = value; } }
        [SerializeField]
        protected GenMode generateMode;

        public int Iterations { get { return iterations; } private set { iterations = value; } }
        [SerializeField]
        protected int iterations;

        public bool IterativeGrowth { get { return iterativeGrowth; } private set { iterativeGrowth = value; } }
        [SerializeField]
        protected bool iterativeGrowth = true;

        [SerializeField]
        protected bool bakePrefab = false;

        public ParameterBundle StartingParameters { get { return startingParameters; } private set { } }
        [SerializeField] protected ParameterBundle startingParameters = new ParameterBundle();

        public RuleSet Rules { get { return rules; } private set { } }
        [SerializeField] protected RuleSet rules = new RuleSet();

        public CharGameObjectDict Implementations { get { return implementations; } private set { } }
        [SerializeField] protected CharGameObjectDict implementations = new CharGameObjectDict();

        protected bool executed;
        protected ParameterBundle returnBundle = new ParameterBundle();

        void Start()
        {
            if (isRoot)
            {
                Execute(startingParameters);
            }

            if(bakePrefab)
            {

            }
        }

        public override void Execute(ParameterBundle bundle)
        {
            Profiler.BeginSample("LSystem.Seed.Execute");

            Sentence sentence = new Sentence(axiom);
            if(generateMode == GenMode.PreEdgeRewrite)
            {
                //Pre calculate final sentence.
                for (int i = 0; i < iterations; i++)
                {
                    sentence = rules.NextGeneration(sentence);
                }
                rules.Fertile = false;
                bundle.SetOrPut("Iterations", 0);
            }
            else // if(preGrow == GenMode.IterateNodeRewrite)
            {
                rules.Fertile = true;
                bundle.SetOrPut("Iterations", iterations);
            }

            bundle.SetOrPut("Generation", 0);
            bundle.SetOrPut("Sentence", sentence);
            bundle.SetOrPut("Implementations", implementations);
            bundle.SetOrPut("RuleSet", rules);
           
            if (!bundle.Exists("Position")) bundle.Put("Position", transform.position);

            if (!inheritHeading) bundle.SetOrPut("Heading", transform.up);
            else if (!bundle.Exists("Heading")) bundle.Put("Heading", transform.up);

            //execute modules in the axiom
            if (!executed)
            {
                executed = true;
                EnqueueProcessNextModule(transform, sentence, implementations, rules, bundle);
            }
            Profiler.EndSample();
        }
    }
}
