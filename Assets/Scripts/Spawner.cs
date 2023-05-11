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

    [HideInInspector] public Transform target;
    [SerializeField] private Transform[] spawnedObstacles;

    #endregion Variables

    // Start is called before the first frame update
    void Start()
    {
        //spawnedObstacles = new Transform[Random.Range(0, 10)];
        //StartCoroutine(nameof(SpawnObstacles));//SpawnObstacles();

        ///target = Instantiate(targetObj, transform);
        ///target.localPosition = new Vector3(0f, 1.0f, 2f);
        ///target.GetComponent<Target>().spawner = this;
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
            collidedObjects = Physics.OverlapBox(target.localPosition, target.localScale * 1.25f, Quaternion.identity, objectsToAvoid);

        } while (collidedObjects.Length > 0);

        //target.GetComponent<Target>().spawner = this;
    }

    private void SpawnObstacles()
    {
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

    public void InitialSpawn()
    {

    }

    public void ResetSpawn()
    {
        if (target == null)
        {
            target = Instantiate(targetObj, transform);
            target.localPosition = new Vector3(0f, 1.0f, 2f);
            target.GetComponent<Target>().spawner = this;
        }
        else
        {
            int children = transform.childCount;
            for (int i = 0; i < children; i++)
            {
                if (transform.GetChild(i).CompareTag("Obstacles"))
                {
                    Destroy(transform.GetChild(i).gameObject);
                }
            }

            SpawnObstacles();
            SpawnTarget();
        }
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
