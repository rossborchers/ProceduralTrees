using UnityEngine;
using System.Collections.Generic;
using System;

namespace LSystem
{
    [Serializable]
    public class StringObjectDict : SerializableDictionary<string, object> { }
    [Serializable]
    public class CharGameObjectDict : SerializableDictionary<char, GameObject> { }

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

        public string StartHeading { get { return startHeading; } set { startHeading = value; } }
        [SerializeField]
        protected string startHeading;

        public StringObjectDict StartingParameters { get { return startingParameters; } private set { } }
        [SerializeField]
        protected StringObjectDict startingParameters = new StringObjectDict();

        public RuleSet Rules { get { return rules; } private set { } }
        [SerializeField]
        protected RuleSet rules = new RuleSet();

        public CharGameObjectDict Implementations { get { return implementations; } private set { } }
        [SerializeField]
        protected CharGameObjectDict implementations = new CharGameObjectDict();

        protected bool executed;
        protected ParameterBundle returnBundle = new ParameterBundle();

        void Start()
        {
            if (isRoot)
            {
                ParameterBundle bundle = new ParameterBundle();
                Execute(bundle);
            }
        }

        public override ParameterBundle Execute(ParameterBundle bundle)
        {
            bundle.SetOrPut("StartingParameters", startingParameters);
            bundle.SetOrPut("Implementations", implementations);
            bundle.SetOrPut("RuleSet", rules);
            bundle.SetOrPut("Iterations", 0);

            Sentence sentence = new Sentence(axiom);

            bundle.SetOrPut("Sentence", sentence);

            Vector3 position;
            if (bundle.Get("Position", out position))
            {
                //TODO? 
            }
            else
            {
                bundle.Put("Position", transform.position);
            }
            if(!inheritHeading)
            {
                bundle.SetOrPut("Heading", startHeading);
            }
            else if(!bundle.Exists("Heading"))
            {
                bundle.Put("Heading", startHeading);
            }

            //execute modules in the axiom
            if (!executed)
            {
                executed = true;
                List<KeyValuePair<GameObject, Sentence>> modules = ModuleUtil.CreateNextModules(sentence, implementations);
                returnBundle = ParameterBundle.Merge(ModuleUtil.ExecuteList(modules, bundle));
            }
            return returnBundle;
        }
    }
}
