using UnityEngine;

namespace LSystem
{
    /// <summary>
    /// Rotate an incoming module and call the next module.
    /// </summary>
    public class Rotate : Module
    {
        [SerializeField]
        [Tooltip("Maximum possible rotation. If max and min are the same there can be no variance.")]
        protected Vector3 eulerAnglesMax;

        [SerializeField]
        [Tooltip("Minimum possible rotation. If max and min are the same there can be no variance.")]
        protected Vector3 eulerAnglesMin;

        public Rotate() :base()
        {
            ethereal = true;
        }

        // Entry point when pre-baking LSystem.
        public override void Bake(ParameterBundle bundle)
        {
            AnyExecute(bundle);
        }

        // Entry point when dynamically generating LSystem.
        public override void Execute(ParameterBundle bundle)
        {
            AnyExecute(bundle);
        }

        // Encapsulates common functionality between Bake and Execute. 
        protected void AnyExecute(ParameterBundle bundle)
        {
            //try get relevant prams.
            bool fatal = false;
            Sentence sentence;
            CharGameObjectDict implementations;
            RuleSet rules;
            if (!GetCoreParameters(bundle, out sentence, out implementations, out rules)) fatal = true;

            // Perform rotation on transform based.
            // Note that many rotations are not easily possible with this method. Needs to be improved.
            Quaternion rotation;
            if (bundle.Get("Rotation", out rotation))
            {
             
                //apply pitch yaw and roll to the rotation
                rotation = rotation * Quaternion.Euler(new Vector3( Random.Range(eulerAnglesMin.x, eulerAnglesMax.x),
                                                        Random.Range(eulerAnglesMin.y, eulerAnglesMax.y),
                                                        Random.Range(eulerAnglesMin.z, eulerAnglesMax.z)));
                bundle.Set("Rotation", rotation);
            }
            else
            {
                Debug.LogWarning("Default parameter 'Rotation' missing. Skipping Rotation", gameObject);
            }
            if (!fatal)
            {
                //Call next module
                if(baked) BakeNextModule(transform, sentence, implementations, rules, bundle);
                else EnqueueProcessNextModule(transform, sentence, implementations, rules, bundle);
            }
            Destroy(gameObject); //once rotation  and next module clled we can destroy the object.
        }
    }
}