using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;

namespace LSystem
{
    public class LineBranch : Module 
    {
        Vector3 end;

        public override ParameterBundle Execute(ParameterBundle bundle)
        {
            bool fatal = false;
            Vector3 position, heading;
            Sentence sentence;
            CharGameObjectDict implementations;
            float growSpeed;
            RuleSet rules;
            if (!bundle.Get("Sentence", out sentence)) fatal = true;
            if (!bundle.Get("Implementations", out implementations)) fatal = true;
            if (!bundle.Get("RuleSet", out rules)) fatal = true;
            if (!bundle.Get("Position", out position)) fatal = true;
            if (!bundle.Get("Heading", out heading)) fatal = true;
            if (!bundle.Get("GrowSpeed", out growSpeed))
            {
                growSpeed = 1;
                Debug.LogWarning("float GrowSpeed parameter missing in LineBranch. Please make sure to include it");
            }

            ParameterBundle returnBundle = new ParameterBundle();
            if(!fatal)
            {
                transform.position = position;
                end = position;

                StartCoroutine(Grow(sentence, heading, rules, implementations, growSpeed, bundle));
            }
            else
            {
                Debug.LogError("Default parameter missing!", gameObject);
            }
            return returnBundle;
        }

        void OnDrawGizmos()
        {
            Gizmos.DrawLine(transform.position, end);
        }

        IEnumerator Grow(Sentence sentence, Vector3 heading, RuleSet rules, CharGameObjectDict implementations, float growSpeed, ParameterBundle bundle)
        {
            while(Vector3.Distance(transform.position, end) < 1)
            {
                end += heading * Time.deltaTime * growSpeed;
                yield return null;
            }

            // Once done:
            if(!sentence.HasNext()) sentence = rules.NextGeneration(sentence);
            List<KeyValuePair<GameObject, Sentence>> modules = ModuleUtil.CreateNextModules(sentence, implementations);
            ModuleUtil.ExecuteList(modules, bundle);
        }
    }

}
