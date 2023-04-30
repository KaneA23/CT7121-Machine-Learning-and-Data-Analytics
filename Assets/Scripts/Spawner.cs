using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// Controls objects that are to be spawned.
/// </summary>
public class Spawner : MonoBehaviour {
	[SerializeField] private Transform spawnObj;
	[SerializeField] private Vector2 spawningBounds;

	private Transform spawnedObj;

	[SerializeField] private LayerMask objectsToAvoid;

	// Start is called before the first frame update
	void Start() {
		SpawnObject();
	}

	// Update is called once per frame
	void Update() {
		if (Input.GetKeyDown(KeyCode.H)) {
			Destroy(spawnedObj.gameObject);
			if (spawnedObj.GetComponent<Target>() == null) {
				SpawnObject();
			}
		}
	}

	/// <summary>
	/// Spawns target object and places it somewhere within a given bounds
	/// </summary>
	public void SpawnObject() {
		Collider[] obstaclesHit;

		spawnedObj = Instantiate(spawnObj, transform.parent);
		do {
			spawnedObj.localPosition = new Vector3(Random.Range(-spawningBounds.x, spawningBounds.x), 1.5f, Random.Range(-spawningBounds.y, spawningBounds.y));

			obstaclesHit = Physics.OverlapBox(spawnedObj.localPosition, spawnedObj.localScale, Quaternion.identity, objectsToAvoid);

		} while (obstaclesHit.Length > 0);

		Target target = spawnedObj.GetComponent<Target>();
		if (target != null) {
			spawnedObj.GetComponent<Target>().spawner = this;
		}
	}

	//private void OnDrawGizmos() {
	//	Gizmos.color = Color.cyan;
	//	Gizmos.DrawCube(spawnedObj.position, spawnedObj.localScale);
	//}
}
