using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace LSystem
{
    /// <summary>
    /// Dynamic leaf generation module with batching support.
    /// </summary> 
    [ExecuteInEditMode]
    public class Leaf : Module
    {
        [SerializeField]
        [Tooltip("Number of segments for one horizontal size of the leaf. (total segments will be x 2)")]
        protected int lateralSegments = 1;

        [SerializeField]
        [Tooltip("Number of vertical segments on the leaf.")]
        protected int medialSegments = 2;

        [SerializeField]
        [Tooltip("Total vertical size of leaf.")]
        protected float startMedialSize = 1;

        [SerializeField]
        [Tooltip("Half horizontal size of leaf.")]
        protected float startLateralSize = 1;

        [SerializeField]
        [Tooltip("Local Mesh offset.")]
        protected Vector3 offset = Vector3.zero;

        [SerializeField]
        [Tooltip("Start growing time. This will be updated recursively")]
        protected float startGrowTime = 1;

        [SerializeField]
        [Tooltip("Shape of the leaf")]
        protected AnimationCurve contour = new AnimationCurve(new Keyframe[] { new Keyframe(0, 1, 0, 0), new Keyframe(1, 1, 0, 0) });

        [SerializeField]
        [Tooltip("Rotation around the local horizontal axis for each point on the curve. 1 = 90*")]
        protected AnimationCurve medialRotation = new AnimationCurve(new Keyframe[] { new Keyframe(0, 0, 0, 0), new Keyframe(1, 0, 0, 0) });

        [SerializeField]
        [Tooltip("Rotation around the local vertical axis. 1 = 90*")]
        protected AnimationCurve lateralRotation = new AnimationCurve(new Keyframe[] { new Keyframe(0, 0, 0, 0), new Keyframe(1, 0, 0, 0) });

        [SerializeField]
        [Tooltip("Coefficient multiply to medial size by per leaf.")]
        protected float medialSizeChangeCoefficient = 1f;

        [SerializeField]
        [Tooltip("Coefficient multiply to lateral size by per leaf.")]
        protected float lateralSizeChangeCoefficient = 1f;

        [SerializeField]
        [Tooltip("Coefficient to multiply grow time by per leaf.")]
        protected float growTimeChangeCoefficient = 1f;

        [SerializeField]
        protected bool setStaticOnComplete = true;

        [SerializeField]
        [Tooltip("Should the leaf grow over time or start full size?")]
        protected bool animate = true;

        [SerializeField]
        [Tooltip("Setting this to true will make the leaf responsive to any variable changes at the cost of redrawing each frame.")]
        protected bool continuousUpdate = false;

        [SerializeField]
        [Tooltip("Material to apply to leaf renderer.")]
        protected Material leafMaterial;

        [SerializeField]
        [Tooltip("Modulate animation position while in edit mode.")]
        [Range(0,1)]
        protected float editorTimeValue = 1f;

        protected MeshFilter filter;
        protected MeshRenderer leafRenderer;
        protected Mesh mesh;

        // stores shared meshes for each baked instance with the same prefabIdentifier. 
        static Dictionary<string, MeshFilter> bakedPrefabFilters = new Dictionary<string, MeshFilter>();

        // Entry point when pre-baking LSystem.
        public override void Bake(ParameterBundle bundle)
        {
            if ((filter = gameObject.GetComponent<MeshFilter>()) == null)
            {
                filter = gameObject.AddComponent<MeshFilter>();
            }
            if ((leafRenderer = gameObject.GetComponent<MeshRenderer>()) == null)
            {
                leafRenderer = gameObject.AddComponent<MeshRenderer>();
            }
            leafRenderer.material = leafMaterial;

            //Get parameters
            Quaternion rotation;
            Sentence sentence;
            CharGameObjectDict implementations;
            int generation;
            RuleSet rules;

            // Check valid state for growing
            if (!GetCoreParameters(bundle, out sentence, out implementations, out rules)
             || !GetPositionParameters(bundle, out generation, out rotation)) return;

            transform.position = previous.transform.position;
            transform.rotation = rotation;

            MeshFilter sharedFilter;
            if(bakedPrefabFilters.TryGetValue(prefabIdentifier, out sharedFilter))
            {
                filter.mesh = sharedFilter.sharedMesh;
            }
            else
            {
                UpdateMesh( 1, offset);
                bakedPrefabFilters.Add(prefabIdentifier, filter);
            }
            if (setStaticOnComplete) gameObject.isStatic = true;

            BakeNextModule(transform, sentence, implementations, rules, bundle);
        }

        // Entry point when dynamically executing LSystem.
        public override void Execute(ParameterBundle bundle)
        {
            if ((filter = gameObject.GetComponent<MeshFilter>()) == null)
            {
                filter = gameObject.AddComponent<MeshFilter>();
            }
            if ((leafRenderer = gameObject.GetComponent<MeshRenderer>()) == null)
            {
                leafRenderer = gameObject.AddComponent<MeshRenderer>();
            }
            leafRenderer.material = leafMaterial;

            if (gameObject.activeSelf)
            {
                StartCoroutine(Grow(bundle));
            }
        }

        // Run the update loop to grow the mesh when not baked
        IEnumerator Grow(ParameterBundle bundle)
        {
            //Get parameters
            Quaternion rotation;
            Sentence sentence;
            CharGameObjectDict implementations;
            int generation;
            RuleSet rules;

            // Check valid state for growing
            if (!GetCoreParameters(bundle, out sentence, out implementations, out rules)
             || !GetPositionParameters(bundle, out generation, out rotation)) yield break;

            float medialSize,  lateralSize, growTime;
            if (bundle.Get("LeafMedialSize", out medialSize)) startMedialSize = medialSize;
            if (bundle.Get("LeafLateralSize", out lateralSize)) startLateralSize = lateralSize;
            if (bundle.Get("LeafGrowTime", out growTime)) startGrowTime = growTime;

            transform.position = previous.transform.position;
            transform.rotation = rotation;

            float time = 1f;
            if (animate) time = 0f;
            while (time < startGrowTime)
            {
                time += Time.deltaTime;
                UpdateMesh(time / startGrowTime, offset);
                yield return null;
            }

            bundle.SetOrPut("LeafMedialSize", startMedialSize * medialSizeChangeCoefficient);
            bundle.SetOrPut("LeafLateralSize", startLateralSize * lateralSizeChangeCoefficient);
            bundle.SetOrPut("LeafGrowTime", startGrowTime * growTimeChangeCoefficient);

            if (setStaticOnComplete) gameObject.isStatic = true;

            EnqueueProcessNextModule(transform, sentence, implementations, rules, bundle);

            if(continuousUpdate)
            {
                while(true)
                {
                    UpdateMesh(1f, offset);
                    yield return null;
                }
            }
        }

        //Editor only mesh rendering code for visual feedback
        #if UNITY_EDITOR
        // Called in edit mode
        protected void Update()
        {
            if ((filter = gameObject.GetComponent<MeshFilter>()) == null)
            {
                filter = gameObject.AddComponent<MeshFilter>();
            }
            if ((leafRenderer = gameObject.GetComponent<MeshRenderer>()) == null)
            {
                leafRenderer = gameObject.AddComponent<MeshRenderer>();
            }
            leafRenderer.material = leafMaterial;

            if (Application.isPlaying) return;

            UpdateMesh(editorTimeValue, offset);
        }
        #endif


        // Draw the leaf mesh.
        void UpdateMesh(float normalizedTime, Vector3 offset)
        {
            /*  
             *  Positions and distances between verts.
             *  Indecies are calculated from verts 6, 5, 7, 9, 1, 0, 2, 4 with triangles towards the top left
             *  
             *  Lateral Segments: 2
             *  Medial Segments:  2
             *  
             *  Width:  5
             *  height: 2
             * 
             *  [13]---2---[11]----1----[10]---2----[12]---2---[14]  
             *   |           |           |           |           |
             *   5           5           5           5           5
                 |           |           |           |           |
             *  [8]----2----[6]----1----[5]----2----[7]----2----[9]  
             *   |           |           |           |           |
             *   5           5           5           5           5
             *   |           |           |           |           |
             *  [3]----2----[1]----1----[0]----2----[2]----2---[4]    
             *        
             */

            //Validate parameters
            normalizedTime = Mathf.Max(Mathf.Min(1f, normalizedTime));
            if (medialSegments < 1)
            {
                Debug.LogWarning("Leaf cannot have less than 1 medial segments. Skipping mesh generation", gameObject);
                return;
            }
            else if (lateralSegments < 1)
            {
                Debug.LogWarning("Leaf cannot have less than 1 lateral segments. Skipping mesh generation", gameObject);
                return;
            }

            //Ensure no mesh leakage in edit mode
            Mesh mesh;
            if (!Application.isPlaying)
            {
                if(filter.sharedMesh == null)
                {
                    filter.sharedMesh = new Mesh();
                }
                mesh = filter.sharedMesh;
            }
            else mesh = filter.mesh;
            mesh.Clear();

            // How big is the leaf currently?
            float localMedSize = startMedialSize * normalizedTime;
            float localLatSize = startLateralSize * normalizedTime;

            //calculate num verts and build arrays.
            int vertexNumMed = medialSegments + 1;
            int vertexNumLat = lateralSegments + lateralSegments + 1;

            int numVerts = vertexNumLat * vertexNumMed;
            int numIndecies = (numVerts - (vertexNumLat + medialSegments)) * 6;

            Vector3[] verts = new Vector3[numVerts];
            Vector3[] normals = new Vector3[numVerts];
            Vector2[] uvs = new Vector2[numVerts];
            int[] indecies = new int[numIndecies];

            // Use similar system to L system. (Stepwize calculation of rotations)
            Vector3 medialHeading = Vector3.up;
            Vector3 lateralHeading = Vector3.right;

            Vector3 lateralPosLeft;
            Vector3 lastLateralPosLeft;

            Vector3 lateralPosRight;
            Vector3 lastLateralPosRight;

            Vector3 segmentCenter = Vector3.zero;
            Vector3 lastCenter = segmentCenter;
           
            int indexPos = 0;
            int topIndexCutoff = vertexNumLat * (vertexNumMed - 1);

            for (int i = 0; i < numVerts; i+= vertexNumLat)
            {
                //Calculate center (medial) pos and heading

                float normalizedMedPos = (i / vertexNumLat) / (float)medialSegments;
                float medRotationCoeff = medialRotation.Evaluate(normalizedTime * normalizedMedPos);

                medialHeading = Quaternion.AngleAxis(90f * medRotationCoeff, Vector3.right) * medialHeading;

                segmentCenter = lastCenter + (localMedSize / medialSegments) * medialHeading;
                lastCenter = segmentCenter;

                verts[i] = offset + segmentCenter;
                normals[i] = Quaternion.AngleAxis(90, Vector3.right) * medialHeading;
                uvs[i] = Vector3.one;

                if (i < topIndexCutoff) indexPos = UpdateIndecies(indexPos, i, 1, vertexNumLat, ref indecies); 

                int invalidLatIndexPos = i + vertexNumLat - 2;
                int positiveLatEndPos = lateralSegments * 2 - 1;

                lateralHeading = Vector3.right;
                lastLateralPosLeft = Vector3.zero;
                lastLateralPosRight = Vector3.zero;
                for (int j = 1; j < positiveLatEndPos + 1; j+=2)
                {
                    //We calculate one lateral size and invert the local X axis

                    float normalizedLateralPos = ((j == 1) ? 1 : j - 1) / (float)lateralSegments;

                    //Calculate contour and lateral rotation
                    float contourValue = contour.Evaluate(normalizedMedPos);
                    float lateralRotationCoeff = lateralRotation.Evaluate(normalizedLateralPos * normalizedTime);

                    lateralHeading = Quaternion.Euler(0, 90f * lateralRotationCoeff, 0) * lateralHeading;
                    Vector3 finalHeadingLeft = Quaternion.AngleAxis(Vector3.Angle(Vector3.up, medialHeading), Vector3.right) * lateralHeading;
                    Vector3 finalHeadingRight = new Vector3(finalHeadingLeft.x, -finalHeadingLeft.y, -finalHeadingLeft.z);

                    int rightVertIndex = i + j;
                    int leftVertIndex = i + j + 1;

                    lateralPosLeft = lastLateralPosLeft + (localLatSize / 2 * contourValue) * finalHeadingLeft;
                    lateralPosRight = lastLateralPosRight + (-localLatSize / 2 * contourValue) * finalHeadingRight;
                    lastLateralPosLeft = lateralPosLeft;
                    lastLateralPosRight = lateralPosRight;

                    //update positions
                    verts[rightVertIndex] = offset + segmentCenter + lateralPosRight;
                    normals[rightVertIndex] = Quaternion.AngleAxis(90, medialHeading) * finalHeadingRight;
                    uvs[rightVertIndex] = Vector3.one;

                    verts[leftVertIndex] = offset + segmentCenter + lateralPosLeft;
                    normals[leftVertIndex] = Quaternion.AngleAxis(90, medialHeading) * finalHeadingLeft;
                    uvs[leftVertIndex] = Vector3.one;

                    //if top left is not out of range calculate indecies
                    if (rightVertIndex < topIndexCutoff && rightVertIndex != invalidLatIndexPos) indexPos = UpdateIndecies(indexPos, rightVertIndex, 2, vertexNumLat, ref indecies);
                    if (leftVertIndex < topIndexCutoff) indexPos = UpdateIndecies(indexPos, leftVertIndex, -2, vertexNumLat, ref indecies); 
                }
            }

            mesh.vertices = verts;
            mesh.normals = normals;
            mesh.uv = uvs;
            mesh.triangles = indecies;

            mesh.RecalculateBounds();
            mesh.Optimize();
        }
    
        // Build triangles out of the verts based on the pattern outlined above
        int UpdateIndecies(int indexPos, int pos, int x, int y, ref int[] indecies)
        {
            indecies[indexPos++] = pos;
            indecies[indexPos++] = pos + x;
            indecies[indexPos++] = pos + x + y;

            indecies[indexPos++] = pos;
            indecies[indexPos++] = pos + x + y;
            indecies[indexPos++] = pos + y;

            return indexPos;
        }
    }
}
