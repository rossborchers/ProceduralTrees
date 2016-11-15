using UnityEngine;
using System.Collections;

namespace LSystem
{
    public class Branch : Module 
    {
        [SerializeField]
        protected float lengthCutoff = 0.001f;

        [SerializeField]
        protected float radiusCutoff = 0.001f;

        [SerializeField]
        protected float startLength = 1f;
        [SerializeField]
        protected float lengthChangeCoefficient = 1f;

        [SerializeField]
        protected float startRadius = 1f;
        [SerializeField]
        protected float radiusChangeCoefficient = 1f;

        [SerializeField]
        protected float topRadiusMultiplier = 1f;

        [SerializeField]
        protected float bottomRadiusMultiplier = 1f;

        [SerializeField]
        protected float startGrowSpeed = 4f;
        [SerializeField]
        protected float growSpeedChangeCoefficient = 1f;

        [SerializeField]
        protected float startFaceNum = 6f;
        [SerializeField]
        protected float faceNumChangeCoefficient = 0.8f;

        [SerializeField]
        protected GameObject endObject = null;

        [SerializeField]
        protected Material branchMaterial;

        [SerializeField]
        protected bool setStaticOnComplete = true;

        protected delegate void GrowLoopCallback(float radius);
        protected MeshFilter filter;
        protected MeshRenderer branchRenderer;

        protected float bottomRadius;
        protected float topRadius = 0;
        protected float faces;

        public override void Execute(ParameterBundle bundle)
        {
            StartCoroutine(Grow(bundle));
        }

        IEnumerator Grow(ParameterBundle bundle)
        {
            //Get parameters
            Vector3 heading;
            Sentence sentence;
            CharGameObjectDict implementations;
            int generation;
            RuleSet rules;

            // Check valid state for growing
            if (!GetCoreParameters(bundle, out sentence, out implementations, out rules)
             || !GetPositionParameters(bundle, out generation, out heading)) yield break;

            // Setup Renderer
            if ((filter = gameObject.GetComponent<MeshFilter>()) == null)
            {
                filter = gameObject.AddComponent<MeshFilter>();
            }
            if ((branchRenderer = gameObject.GetComponent<MeshRenderer>()) == null)
            {
                branchRenderer = gameObject.AddComponent<MeshRenderer>();
            }
            branchRenderer.material = branchMaterial;

            //Match start position to previous position. As growth progresses position will be offset
            //While heading stays the same
            transform.position = previous.transform.position;
            transform.up = heading;

            // Try and pick up length and radius where last branch left off.
            float length, radius, growSpeed;
            if (bundle.Get("BranchLength", out length)) startLength = length;
            if (bundle.Get("BranchRadius", out radius)) startRadius = radius;
            if (bundle.Get("BranchGrowSpeed", out growSpeed)) startGrowSpeed = growSpeed;
            if (bundle.Get("BranchFaceNum", out faces)) startFaceNum = faces;

            GrowLoopCallback growLoopCallback;
            bundle.Get("GrowLoopCallback", out growLoopCallback);

            radius = startRadius;
            length = startLength;
            growSpeed = startGrowSpeed;
            faces = startFaceNum;

            // Since we don't want to continue drawing where it doesn't matter!
            // Note that future emergence is killed here.
            if (length < lengthCutoff || radius < radiusCutoff) yield break;

            if (endObject != null)
            { 
                  endObject = (GameObject)Instantiate(endObject, transform);
                  Module mod = endObject.GetComponent<Module>();
                  if (mod != null)
                  {
                    AssignPrevious(mod, this);
                    if (mod.GetType() != typeof(Seed))
                    {
                        Kill(mod);
                        mod.Execute(bundle);
                    }
                  }
                  endObject.transform.up = heading;
            }

            // Update mesh and extend transform towards final position
            float distance = Vector3.Distance(transform.position, previous.transform.position);
            while(distance < length)
            {
                float completionRatio = distance / length;
                transform.position += heading * Mathf.Min(heading.magnitude * Time.deltaTime * Mathf.Lerp(startGrowSpeed, growSpeed * growSpeedChangeCoefficient, completionRatio), length);
                distance = Vector3.Distance(transform.position, previous.transform.position);

                bottomRadius = Mathf.Lerp(0, startRadius, completionRatio);
                filter = UpdateBranch(Vector3.up * -distance, distance, bottomRadius * bottomRadiusMultiplier, topRadius * topRadiusMultiplier, Mathf.Max(2,(int)(faces)), filter);
                if(growLoopCallback != null) growLoopCallback(bottomRadius);

                if(endObject != null)
                {
                      endObject.transform.position = transform.position;
                      endObject.transform.up = heading;
                }
               
                yield return null;
            }
            bottomRadius = startRadius;

            // Update parameters for next branch
            bundle.SetOrPut("BranchLength", length * lengthChangeCoefficient);
            bundle.SetOrPut("BranchRadius", radius * radiusChangeCoefficient);
            bundle.SetOrPut("BranchGrowSpeed", growSpeed * growSpeedChangeCoefficient);
            bundle.SetOrPut("BranchFaceNum", faces * faceNumChangeCoefficient);

            // For coordination between branches
            growLoopCallback = NextBranchGrowLoopCallback;
            bundle.SetOrPut("GrowLoopCallback", growLoopCallback);

            if (setStaticOnComplete) gameObject.isStatic = true;

            EnqueueProcessNextModule(transform, sentence, implementations, rules, bundle);
        }

        /// <summary>
        /// Called on the next branch grow loop to let this branch match its bottom radius
        /// </summary>
        /// <param name="radius"></param>
        protected void NextBranchGrowLoopCallback(float radius)
        {
            topRadius = radius;
            float distance = Vector3.Distance(transform.position, previous.transform.position);
            filter = UpdateBranch(Vector3.up * -distance, distance, bottomRadius * bottomRadiusMultiplier, topRadius * topRadiusMultiplier, Mathf.Max(2, (int)(faces)), filter);
        }

        // http://wiki.unity3d.com/index.php/ProceduralPrimitives
        protected MeshFilter UpdateBranch(Vector3 offset, float height, float bottomRadius, float topRadius, int sides, MeshFilter filter)
        {
            Mesh mesh = filter.mesh;
            mesh.Clear();

            int nbVerticesCap = sides + 1;

            #region Vertices

            // bottom + top + sides
            Vector3[] vertices = new Vector3[nbVerticesCap + nbVerticesCap + sides * 2 + 2];
            int vert = 0;
            float _2pi = Mathf.PI * 2f;

            // Bottom cap
            vertices[vert++] = offset + new Vector3(0f, 0f, 0f);
            while (vert <= sides)
            {
                float rad = (float)vert / sides * _2pi;
                vertices[vert] = offset + new Vector3(Mathf.Cos(rad) * bottomRadius, 0f, Mathf.Sin(rad) * bottomRadius);
                vert++;
            }

            // Top cap
            vertices[vert++] = offset + new Vector3(0f, height, 0f);
            while (vert <= sides * 2 + 1)
            {
                float rad = (float)(vert - sides - 1) / sides * _2pi;
                vertices[vert] = offset + new Vector3(Mathf.Cos(rad) * topRadius, height, Mathf.Sin(rad) * topRadius);
                vert++;
            }

            // Sides
            int v = 0;
            while (vert <= vertices.Length - 4)
            {
                float rad = (float)v / sides * _2pi;
                vertices[vert] = offset + new Vector3(Mathf.Cos(rad) * topRadius, height, Mathf.Sin(rad) * topRadius);
                vertices[vert + 1] = offset + new Vector3(Mathf.Cos(rad) * bottomRadius, 0, Mathf.Sin(rad) * bottomRadius);
                vert += 2;
                v++;
            }
            vertices[vert] = vertices[sides * 2 + 2];
            vertices[vert + 1] = vertices[sides * 2 + 3];

            #endregion

            #region normals

            // bottom + top + sides
            Vector3[] normals = new Vector3[vertices.Length];
            vert = 0;

            // Bottom cap
            while (vert <= sides)
            {
                normals[vert++] = Vector3.down;
            }

            // Top cap
            while (vert <= sides * 2 + 1)
            {
                normals[vert++] = Vector3.up;
            }

            // Sides
            v = 0;
            while (vert <= vertices.Length - 4)
            {
                float rad = (float)v / sides * _2pi;
                float cos = Mathf.Cos(rad);
                float sin = Mathf.Sin(rad);

                normals[vert] = new Vector3(cos, 0f, sin);
                normals[vert + 1] = normals[vert];

                vert += 2;
                v++;
            }
            normals[vert] = normals[sides * 2 + 2];
            normals[vert + 1] = normals[sides * 2 + 3];
            #endregion

            #region UVs
            Vector2[] uvs = new Vector2[vertices.Length];

            // Bottom cap
            int u = 0;
            uvs[u++] = new Vector2(0.5f, 0.5f);
            while (u <= sides)
            {
                float rad = (float)u / sides * _2pi;
                uvs[u] = new Vector2(Mathf.Cos(rad) * .5f + .5f, Mathf.Sin(rad) * .5f + .5f);
                u++;
            }

            // Top cap
            uvs[u++] = new Vector2(0.5f, 0.5f);
            while (u <= sides * 2 + 1)
            {
                float rad = (float)u / sides * _2pi;
                uvs[u] = new Vector2(Mathf.Cos(rad) * .5f + .5f, Mathf.Sin(rad) * .5f + .5f);
                u++;
            }

            // Sides
            int u_sides = 0;
            while (u <= uvs.Length - 4)
            {
                float t = (float)u_sides / sides;
                uvs[u] = new Vector3(t, 1f);
                uvs[u + 1] = new Vector3(t, 0f);
                u += 2;
                u_sides++;
            }
            uvs[u] = new Vector2(1f, 1f);
            uvs[u + 1] = new Vector2(1f, 0f);
            #endregion

            #region Triangles
            int nbTriangles = sides + sides + sides * 2;
            int[] triangles = new int[nbTriangles * 3 + 3];

            // Bottom cap
            int tri = 0;
            int i = 0;
            while (tri < sides - 1)
            {
                triangles[i] = 0;
                triangles[i + 1] = tri + 1;
                triangles[i + 2] = tri + 2;
                tri++;
                i += 3;
            }
            triangles[i] = 0;
            triangles[i + 1] = tri + 1;
            triangles[i + 2] = 1;
            tri++;
            i += 3;

            // Top cap
            //tri++;
            while (tri < sides * 2)
            {
                triangles[i] = tri + 2;
                triangles[i + 1] = tri + 1;
                triangles[i + 2] = nbVerticesCap;
                tri++;
                i += 3;
            }

            triangles[i] = nbVerticesCap + 1;
            triangles[i + 1] = tri + 1;
            triangles[i + 2] = nbVerticesCap;
            tri++;
            i += 3;
            tri++;

            // Sides
            while (tri <= nbTriangles)
            {
                triangles[i] = tri + 2;
                triangles[i + 1] = tri + 1;
                triangles[i + 2] = tri + 0;
                tri++;
                i += 3;

                triangles[i] = tri + 1;
                triangles[i + 1] = tri + 2;
                triangles[i + 2] = tri + 0;
                tri++;
                i += 3;
            }
            #endregion

            mesh.vertices = vertices;
            mesh.normals = normals;
            mesh.uv = uvs;
            mesh.triangles = triangles;

            mesh.RecalculateBounds();
            mesh.Optimize();

            return filter;
        }

    }

}
