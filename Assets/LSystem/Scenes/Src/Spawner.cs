using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Spawns LSystems for demonstration and performance testing/
/// </summary>
public class Spawner : MonoBehaviour
{

    [SerializeField]
    [Tooltip("Objects to spawn in the area specified by side spawn size.")]
    protected List<GameObject> sideObjects;

    [SerializeField]
    [Tooltip("Objects to spawn in wideSpawnSize")]
    protected List<GameObject> wideObjects;

    [SerializeField]
    [Tooltip("Time between Instantiating ground dressing objects.")]
    protected float timePerGroundDressingSpawn;

    [SerializeField]
    [Tooltip("Time between Instantiating side objects.")]
    protected float timePerSideSpawn;

    [SerializeField]
    [Tooltip("Size of the side spawn area. Note its per side.")]
    float sideSpawnSize = 4;

    [SerializeField]
    [Tooltip("Size of the center area. side spawn is not instantiated here.")]
    float centerSpawnSize = 2;

    [SerializeField]
    [Tooltip("Size of the wide area.")]
    float wideSpawnSize = 10;

    [SerializeField]
    [Tooltip("Distance from spawn along the forward axis.")]
    float spawnDistance = 5f;

    protected float currentSideTime = 0f;
    protected float currentWideTime = 0f;
    protected bool side;

    protected void Update ()
    {
        // Try create side objects
        currentSideTime += Time.deltaTime;
        if (currentSideTime > timePerSideSpawn)
        {
            float latPos;
            if (side) latPos = Random.Range(-sideSpawnSize - centerSpawnSize, -centerSpawnSize);
            else latPos = Random.Range(centerSpawnSize, centerSpawnSize + sideSpawnSize);
        
            Vector3 positon = transform.position + (Vector3.forward * spawnDistance) + latPos * transform.right;
            GameObject g = (GameObject)Instantiate(sideObjects[Random.Range(0, sideObjects.Count)],  positon, Quaternion.identity);

            // Trigger is used to destroy the mesh when it goes off screen.
            if (g.GetComponent<Collider>() == null)
            {
                Collider coll = g.AddComponent<BoxCollider>();
                coll.isTrigger = true;
            }
            currentSideTime = 0f;
            side = !side; 
        }

        // Try create wide objects
        currentWideTime += Time.deltaTime;
        if (currentWideTime > timePerGroundDressingSpawn)
        {
            float latPos;
            latPos = Random.Range(-wideSpawnSize, wideSpawnSize);

            Vector3 positon = transform.position + (Vector3.forward * spawnDistance) + latPos * transform.right;

            GameObject g = (GameObject)Instantiate(wideObjects[Random.Range(0, wideObjects.Count)], positon, Quaternion.identity);
            if (g.GetComponent<Collider>() == null)
            {
                Collider coll = g.AddComponent<BoxCollider>();
                coll.isTrigger = true;
            }
            currentWideTime = 0f;
        }
    }

    // Destroy Module when it hits this GameObjects collider.
    protected void OnTriggerEnter(Collider coll)
    {
        if(coll.GetComponent<LSystem.Module>() != null)
        {
            Destroy(coll.gameObject);
        }
    }
}
