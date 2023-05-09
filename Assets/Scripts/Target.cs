using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour {
	[HideInInspector] public Spawner spawner;

	// Start is called before the first frame update
	void Start() {

	}

	// Update is called once per frame
	void Update() {

	}

	private void OnCollisionEnter(Collision collision) {
		//Debug.Log(collision.gameObject.tag + collision.gameObject.name);
		if (collision.gameObject.name == "Agent") {
			//Destroy(gameObject);
			spawner.ResetSpawn();
		}

		if (collision.gameObject.CompareTag("Obstacles") || collision.gameObject.CompareTag("Wall")) {
			transform.localPosition = new Vector3(Random.Range(-11f, 11f), 1.5f, Random.Range(-11f, 11f));
		}
	}

	private void OnCollisionStay(Collision collision) {
		//Debug.Log(collision.gameObject.tag + collision.gameObject.name);
		if (collision.gameObject.CompareTag("Obstacles") || collision.gameObject.CompareTag("Wall")) {
			transform.localPosition = new Vector3(Random.Range(-11f, 11f), 1.5f, Random.Range(-11f, 11f));
		}
	}

	//private void OnDestroy() {
	//	spawner.ResetSpawn();
	//}
}
