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

	private Transform target;
	private Transform[] spawnedObstacles;

	#endregion Variables

	// Start is called before the first frame update
	void Start() {
		SpawnTarget();

		spawnedObstacles = new Transform[Random.Range(0, 10)];
		SpawnObstacles();
	}

	// Update is called once per frame
	void Update() {

	}

	/// <summary>
	/// Spawns target object and places it somewhere within a given bounds
	/// </summary>


	private void SpawnTarget() {
		Collider[] collidedObjects;

		target = Instantiate(targetObj, transform);
		do {
			target.localPosition = new Vector3(Random.Range(-11f, 11f), 1.5f, Random.Range(-11f, 11f));
			collidedObjects = Physics.OverlapBox(target.position, target.localScale * 1.25f, Quaternion.identity, objectsToAvoid);

		} while (collidedObjects.Length > 0);

		target.GetComponent<Target>().spawner = this;
	}

	private void SpawnObstacles() {
		Collider[] collidedObjects;
		int spawnAttempts;

		for (int i = 0; i < spawnedObstacles.Length; i++) {
			spawnedObstacles[i] = Instantiate(obstacle, transform);

			spawnAttempts = 0;
			do {
				spawnedObstacles[i].localScale = new Vector3(Random.Range(1f, 5f), 3f, Random.Range(1f, 5f));
				spawnedObstacles[i].localPosition = new Vector3(Random.Range(-10f, 10f), 1.5f, Random.Range(-10f, 10f));

				//collidedObjects = Physics.OverlapBox(spawnedObstacles[i].position, spawnedObstacles[i].localScale, Quaternion.identity, objectsToAvoid);
				//collidedObjects = Physics.OverlapSphere(spawnedObstacles[i].position, 2f, objectsToAvoid);
				//spawnAttempts++;

			} while (/*collidedObjects.Length > 0 && spawnAttempts < 10*/Physics.CheckBox(spawnedObstacles[i].position, spawnedObstacles[i].localScale / 2, Quaternion.identity, objectsToAvoid));
			//Debug.Log(Physics.CheckBox(spawnedObstacles[i].position, spawnedObstacles[i].localScale / 2, Quaternion.identity, objectsToAvoid).ToString());
		}
	}

	public void ResetSpawn() {
		for (int i = 0; i < spawnedObstacles.Length; i++) {
			if (spawnedObstacles[i] != null) {
				Destroy(spawnedObstacles[i].gameObject);
				spawnedObstacles[i] = null;
			}
		}

		if (target != null) {
			Destroy(target.gameObject);
			target = null;
		}

		SpawnTarget();

		spawnedObstacles = new Transform[Random.Range(5, 10)];
		SpawnObstacles();
	}

	private void OnDrawGizmos() {
		if (Application.isPlaying) {
			Gizmos.color = Color.red;

			foreach (Transform obst in spawnedObstacles) {
				if (obst != null) {
					Gizmos.DrawWireCube(obst.position, obst.localScale * 1.25f);
				}
			}
		}
	}
}
