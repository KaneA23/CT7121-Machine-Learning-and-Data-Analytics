using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// Controls objects that are to be spawned.
/// </summary>
public class Spawner : MonoBehaviour {

	#region Variables

	[Header("Spawn Prefabs")]
	[SerializeField] private Transform targetObj;
	[SerializeField] private Transform obstacle;

	[Space(5)]
	[SerializeField] private LayerMask objectsToAvoid;

	[HideInInspector] public Transform target;

	[SerializeField] private Vector3 startPos;

	#endregion Variables

	// Start is called before the first frame update
	private void Start() {
		target = Instantiate(targetObj, transform);
		target.GetComponent<Target>().spawner = this;
		target.localPosition = startPos;
	}

	/// <summary>
	/// Spawns target object and places it somewhere within a given bounds
	/// </summary>
	private void SpawnTarget() {
		Vector3 spawnPoint;
		Collider[] collisions;
		do {
			float x = Random.Range(-7.5f, 7.5f);
			float z = Random.Range(-7.5f, 7.5f);

			float tempX = x + transform.position.x;
			float tempZ = z + transform.position.z;
			spawnPoint = new Vector3(tempX, 1.5f, tempZ);

			collisions = Physics.OverlapSphere(spawnPoint, 1.5f, objectsToAvoid.value);
		} while (collisions.Length > 0);

		target.position = spawnPoint;
	}

	private void SpawnTargetOnOtherSide() {
		Vector3 tempPos = target.transform.localPosition;
		tempPos.x *= -1;
		tempPos.z *= -1;

		target.transform.localPosition = tempPos;
	}

	/// <summary>
	/// Spawns in different sized obstacles and places them randomly around training areas
	/// </summary>
	private void SpawnObstacles() {
		// 0-5 obstacles can spawn at a time
		int count = Random.Range(0, 5);
		Transform spawnedObj;

		Vector3 spawnPoint;
		Collider[] collisions;
		for (int i = 0; i < count; i++) {
			spawnedObj = Instantiate(obstacle, transform);
			spawnedObj.localScale = new Vector3(Random.Range(1f, 5f), 3f, Random.Range(1f, 5f));

			// Finds a place to spawn the object
			do {
				float x = Random.Range(-10f, 10f);
				float z = Random.Range(-10f, 10f);

				float tempX = x + transform.position.x;
				float tempZ = z + transform.position.z;
				spawnPoint = new Vector3(tempX, 1.5f, tempZ);

				collisions = Physics.OverlapSphere(spawnPoint, 5f, objectsToAvoid.value);
			} while (collisions.Length > 0);

			spawnPoint = new Vector3(spawnPoint.x - transform.position.x, 1.5f, spawnPoint.z - transform.position.z);
			spawnedObj.localPosition = spawnPoint;
		}
	}

	/// <summary>
	/// Removes any obstacles that exist and begins placing new obstacles and targets
	/// </summary>
	public void ResetSpawn() {
		int children = transform.childCount;
		//for (int i = 0; i < children; i++) {
		//	if (transform.GetChild(i).CompareTag("Obstacles")) {
		//		Destroy(transform.GetChild(i).gameObject);
		//	}
		//}

		//SpawnObstacles();

		SpawnTarget();
		//SpawnTargetOnOtherSide();
	}

	//private void OnDrawGizmos()
	//{
	//    if (Application.isPlaying)
	//    {
	//        Gizmos.color = Color.blue;
	//        Gizmos.DrawWireSphere(target.position, 1.5f);
	//    }
	//}
}
