using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// Controls objects that are to be spawned.
/// </summary>
public class Spawner : MonoBehaviour
{

    #region Variables

    [Header("Spawn Prefabs")]
    [SerializeField] private Transform targetObj;
    [SerializeField] private Transform obstacle;

    [Space(5)]
    [SerializeField] private LayerMask objectsToAvoid;

    private Transform target;
    [SerializeField] private Transform[] spawnedObstacles;

    #endregion Variables

    // Start is called before the first frame update
    void Start()
    {
        //spawnedObstacles = new Transform[Random.Range(0, 10)];
        //StartCoroutine(nameof(SpawnObstacles));//SpawnObstacles();

        target = Instantiate(targetObj, transform);
        target.GetComponent<Target>().spawner = this;
        //SpawnTarget();

    }

    // Update is called once per frame
    void Update()
    {

    }

    /// <summary>
    /// Spawns target object and places it somewhere within a given bounds
    /// </summary>


    private void SpawnTarget()
    {
        Collider[] collidedObjects;

        //target = Instantiate(targetObj, transform);
        do
        {
            target.localPosition = new Vector3(Random.Range(-11f, 11f), 1.5f, Random.Range(-11f, 11f));
            collidedObjects = Physics.OverlapBox(target.position, target.localScale * 1.25f, Quaternion.identity, objectsToAvoid);

        } while (collidedObjects.Length > 0);

        //target.GetComponent<Target>().spawner = this;
    }

    private void SpawnObstacles()
    {
        //Collider[] collidedObjects;
        //int spawnAttempts;
        //
        //for (int i = 0; i < spawnedObstacles.Length; i++)
        //{
        //    spawnedObstacles[i] = Instantiate(obstacle, transform);
        //
        //    spawnAttempts = 0;
        //    do
        //    {
        //        spawnedObstacles[i].localScale = new Vector3(Random.Range(1f, 5f), 3f, Random.Range(1f, 5f));
        //        spawnedObstacles[i].localPosition = new Vector3(Random.Range(-10f, 10f), 1.5f, Random.Range(-10f, 10f));
        //
        //        //collidedObjects = Physics.OverlapBox(spawnedObstacles[i].position, spawnedObstacles[i].localScale, Quaternion.identity, objectsToAvoid);
        //        //collidedObjects = Physics.OverlapSphere(spawnedObstacles[i].position, 2f, objectsToAvoid);
        //        //spawnAttempts++;
        //        yield return null;
        //
        //
        //    } while (/*collidedObjects.Length > 0 && spawnAttempts < 10*/Physics.CheckSphere(spawnedObstacles[i].position, 5f, objectsToAvoid.value));
        //}

        int count = Random.Range(0, 5);
        Transform spawnedObj;

        Vector3 spawnPoint;
        Collider[] collisions;
        for (int i = 0; i < count; i++)
        {
            do
            {
                float x = Random.Range(-10f, 10f);
                float z = Random.Range(-10f, 10f);

                float tempX = x + transform.position.x;
                float tempZ = z + transform.position.z;
                spawnPoint = new Vector3(tempX, 1.5f, tempZ);

                collisions = Physics.OverlapSphere(spawnPoint, 5f, objectsToAvoid.value);
            } while (collisions.Length > 0);

            spawnPoint = new Vector3(spawnPoint.x - transform.position.x, 1.5f, spawnPoint.z - transform.position.z);

            spawnedObj = Instantiate(obstacle, transform);
            spawnedObj.localPosition = spawnPoint;
            spawnedObj.localScale = new Vector3(Random.Range(1f, 5f), 3f, Random.Range(1f, 5f));
        }
    }

    public void ResetSpawn()
    {
        //List<GameObject> obstaclesToRemove = new List<GameObject>();
        //obstaclesToRemove.Add(transform.GetComponentInChildren<Transform>().gameObject);

        //foreach (GameObject child in transform)
        //{
        //    Debug.Log(child.name);
        //    if (child.CompareTag("Obstacles"))
        //    {
        //        Destroy(child);
        //    }
        //}

        int children = transform.childCount;
        for (int i = 0; i < children; i++)
        {
            // Debug.Log(transform.GetChild(i).name);
            if (transform.GetChild(i).CompareTag("Obstacles"))
            {
                Destroy(transform.GetChild(i).gameObject);
            }
        }
        //SpawnTarget();

        //spawnedObstacles = new Transform[Random.Range(2, 5)];
        SpawnObstacles();

        SpawnTarget();
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            Gizmos.color = Color.red;

            foreach (Transform obst in spawnedObstacles)
            {
                if (obst != null)
                {
                    //Gizmos.DrawWireCube(obst.position, obst.localScale * 1.25f);
                    Gizmos.DrawWireSphere(obst.position, 3f);
                }
            }
        }
    }
}
