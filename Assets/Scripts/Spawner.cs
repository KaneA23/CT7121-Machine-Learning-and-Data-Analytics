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

	// Start is called before the first frame update
	void Start() {
		SpawnObject();
	}

	// Update is called once per frame
	void Update() {
		if (Input.GetKeyDown(KeyCode.H)) {
			Destroy(spawnedObj.gameObject);
			SpawnObject();
		}
	}

	/// <summary>
	/// Creates gameobjec and places it somewhere within a given bounds
	/// </summary>
	public void SpawnObject() {
		spawnedObj = Instantiate(spawnObj, transform.parent);
		spawnedObj.localPosition = new Vector3(Random.Range(-spawningBounds.x, spawningBounds.x), 1.5f, Random.Range(-spawningBounds.y, spawningBounds.y));
	}
}
