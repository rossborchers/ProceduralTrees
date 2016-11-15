using UnityEngine;
using System.Collections.Generic;

public class Spawner : MonoBehaviour
{

    [SerializeField]
    protected List<GameObject> sideObjects;

    [SerializeField]
    protected List<GameObject> centerObjects;

    [SerializeField]
    protected float timePerSideSpawn;

    [SerializeField]
    protected float timePerCenterSpawn;

    [SerializeField]
    protected int maxItems;

    [SerializeField]
    float sideSpawnSize = 4;

    [SerializeField]
    float centerSpawnSize = 2;

    [SerializeField]
    float spawnDistance = 5f;

    protected float currentTime = 0f;

    protected bool side;
    protected void Update ()
    {
       
        currentTime += Time.deltaTime;
        if (currentTime > timePerCenterSpawn)
        {
            float latPos;
            if (side) latPos = Random.Range(-sideSpawnSize - centerSpawnSize, -centerSpawnSize);
            else latPos = Random.Range(centerSpawnSize, centerSpawnSize + sideSpawnSize);
        
            Vector3 positon = transform.position + (Vector3.forward * spawnDistance) + latPos * transform.right;
        
            GameObject g = (GameObject)Instantiate(sideObjects[Random.Range(0, sideObjects.Count)],  positon, Quaternion.identity);
            if (g.GetComponent<Collider>() == null)
            {
                Collider coll = g.AddComponent<BoxCollider>();
                coll.isTrigger = true;
            }
            currentTime = 0f;
            side = !side; 
        }
	}

    protected void OnTriggerEnter(Collider coll)
    {
        if(coll.GetComponent<LSystem.Module>() != null)
        {
            Destroy(coll.gameObject);
        }
    }
}
