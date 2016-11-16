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

        public Vector3 BakedRotationMax { get { return bakedRotationMax; } private set { bakedRotationMax = value; } }
        [SerializeField]
        protected Vector3 bakedRotationMax = Vector3.zero;

        public Vector3 BakedRotationMin { get { return bakedRotationMin; } private set { bakedRotationMin = value; } }
        [SerializeField]
        protected Vector3 bakedRotationMin = Vector3.zero;

        public float BakedScaleMax { get { return bakedScaleMax; } private set { bakedScaleMax = value; } }
        [SerializeField]
        protected float bakedScaleMax = 1;

        public float BakedScaleMin { get { return bakedScaleMin; } private set { bakedScaleMin = value; } }
        [SerializeField]
        protected float bakedScaleMin = 1;

        public ParameterBundle StartingParameters { get { return startingParameters; } private set { } }
        [SerializeField] protected ParameterBundle startingParameters = new ParameterBundle();

        public RuleSet Rules { get { return rules; } private set { } }
        [SerializeField] protected RuleSet rules = new RuleSet();

        public CharGameObjectDict Implementations { get { return implementations; } private set { } }
        [SerializeField] protected CharGameObjectDict implementations = new CharGameObjectDict();

        protected bool executed;
        protected ParameterBundle returnBundle = new ParameterBundle();

        protected static Dictionary<string, GameObject> bakedProtoypes = new Dictionary<string, GameObject>();

        void Start()
        {
            if (isRoot)
            {
                if (baked) Bake(startingParameters);
                else Execute(startingParameters);
            }
        }

        public override void Bake(ParameterBundle bundle)
        {
            if (dead) return;

            //TODO: this has the meshes so they will still be destroyed when this is!
            Vector3 heading;
            if (!bundle.Get("Heading", out heading))
            {
                heading = Vector3.up;
            }

           

            // try get existing instance
            GameObject prototypeInstance;
            if (bakedProtoypes.TryGetValue(prefabIdentifier, out prototypeInstance))
            {
                GameObject instance = (GameObject)Instantiate(prototypeInstance, previous.transform.position, Quaternion.identity, previous.transform);

                instance.transform.up = heading;
                instance.SetActive(true);
            }
            else
            {
                //TODO: This lags like crazy. Not suitable for complex objects. Move logic to Editor and save as prefab!
                AnyExecute(bundle); //bake

                //this object contains the original mesh data so it needs to become the prototypical instance. we create a copy of it to continue.
                //Be free!
                GameObject instance = (GameObject)Instantiate(gameObject, Vector3.zero, Quaternion.identity);
               

                transform.position = Vector3.zero;
                transform.rotation = Quaternion.identity;
                transform.parent = null;

                gameObject.name = "PrototypeInstance_" + prefabIdentifier;
                Kill(this); //the prototype must not be able to create new instances(they will do the same... etc)
                //gameObject.hideFlags = HideFlags.HideInHierarchy;

                bakedProtoypes.Add(prefabIdentifier, gameObject);
                gameObject.SetActive(false);
            }
        }

        public override void Execute(ParameterBundle bundle)
        {
            if (dead) return;
            AnyExecute(bundle); 
        }

        public void AnyExecute(ParameterBundle bundle)
        {
            Profiler.BeginSample("LSystem.Seed.Execute");

            if (previous != null)
            {
                transform.position = previous.transform.position;
            }

            Sentence sentence = new Sentence(axiom);
            if (generateMode == GenMode.PreEdgeRewrite)
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

            if (!bundle.Exists("Position")) bundle.Put("Position", transform.position); //TODO: REMOVE. Is this used anywhere?

           

            if (!inheritHeading) bundle.SetOrPut("Heading", transform.up);
            else if (!bundle.Exists("Heading")) bundle.Put("Heading", transform.up);

            //execute modules in the axiom
            if (!executed)
            {
                executed = true;

                if(baked) BakeNextModule(transform, sentence, implementations, rules, bundle);
                else EnqueueProcessNextModule(transform, sentence, implementations, rules, bundle);
            }
            Profiler.EndSample();
        }
    }
}
