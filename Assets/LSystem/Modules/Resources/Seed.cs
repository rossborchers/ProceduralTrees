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
        public bool IsRoot { get { return isRoot; } private set { isRoot = value; } }
        [SerializeField]
        protected bool isRoot;

        public string Axiom { get { return axiom; } set { axiom = value; } }
        [SerializeField]
        protected string axiom;

        public bool InheritHeading { get { return isRoot; } private set { isRoot = value; } }
        [SerializeField] protected bool inheritHeading;

        public bool PreGrow { get { return preGrow; } private set { preGrow = value; } }
        [SerializeField]
        protected bool preGrow;

        public int PreGrowIterations { get { return preGrowIterations; } private set { preGrowIterations = value; } }
        [SerializeField]
        protected int preGrowIterations;

        public bool IterativeGrowth { get { return iterativeGrowth; } private set { iterativeGrowth = value; } }
        [SerializeField]
        protected bool iterativeGrowth = true;

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
        }

        public override void Execute(ParameterBundle bundle)
        {
            Sentence sentence = new Sentence(axiom);
            if(preGrow)
            {
                for(int i = 0; i < preGrowIterations; i++)
                {
                    sentence = rules.NextGeneration(sentence);
                }
            }

            if (iterativeGrowth) rules.Fertile = true;
            else rules.Fertile = false;

            bundle.SetOrPut("Sentence", sentence);
            bundle.SetOrPut("Implementations", implementations);
            bundle.SetOrPut("RuleSet", rules);
            bundle.SetOrPut("Generation", 0);

            if (!bundle.Exists("Position")) bundle.Put("Position", transform.position);

            if (!inheritHeading) bundle.SetOrPut("Heading", transform.up);
            else if (!bundle.Exists("Heading")) bundle.Put("Heading", transform.up);

            //execute modules in the axiom
            if (!executed)
            {
                executed = true;
                ProcessNextModule(sentence, implementations, rules, bundle);
            }
        }
    }
}
