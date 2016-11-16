using UnityEngine;
using System.Collections.Generic;

public class Spawner : MonoBehaviour
{

    [SerializeField]
    protected List<GameObject> treeObjects;

    [SerializeField]
    protected List<GameObject> groundDressing;

    [SerializeField]
    protected List<GameObject> centerObjects;

    [SerializeField]
    protected float timePerSideSpawn;

    [SerializeField]
    protected float timeGroundDressingSpawn;

    [SerializeField]
    protected float timePerCenterSpawn;

    [SerializeField]
    float sideSpawnSize = 4;

    [SerializeField]
    float centerSpawnSize = 2;

    [SerializeField]
    float dressingSpawnSize = 10;

    [SerializeField]
    float spawnDistance = 5f;

    protected float currentCenterTime = 0f;
    protected float currentGroundDressingTime = 0f;

    protected bool side;
    protected void Update ()
    {
       
        currentCenterTime += Time.deltaTime;
        if (currentCenterTime > timePerCenterSpawn)
        {
            float latPos;
            if (side) latPos = Random.Range(-sideSpawnSize - centerSpawnSize, -centerSpawnSize);
            else latPos = Random.Range(centerSpawnSize, centerSpawnSize + sideSpawnSize);
        
            Vector3 positon = transform.position + (Vector3.forward * spawnDistance) + latPos * transform.right;
        
            GameObject g = (GameObject)Instantiate(treeObjects[Random.Range(0, treeObjects.Count)],  positon, Quaternion.identity);
            if (g.GetComponent<Collider>() == null)
            {
                Collider coll = g.AddComponent<BoxCollider>();
                coll.isTrigger = true;
            }
            currentCenterTime = 0f;
            side = !side; 
        }

        currentGroundDressingTime += Time.deltaTime;
        if (currentGroundDressingTime > timeGroundDressingSpawn)
        {
            float latPos;
            latPos = Random.Range(-dressingSpawnSize, dressingSpawnSize);

            Vector3 positon = transform.position + (Vector3.forward * spawnDistance) + latPos * transform.right;

            GameObject g = (GameObject)Instantiate(groundDressing[Random.Range(0, groundDressing.Count)], positon, Quaternion.identity);
            if (g.GetComponent<Collider>() == null)
            {
                Collider coll = g.AddComponent<BoxCollider>();
                coll.isTrigger = true;
            }
            currentGroundDressingTime = 0f;
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
