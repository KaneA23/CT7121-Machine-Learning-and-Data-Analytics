using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : MonoBehaviour {
	// Start is called before the first frame update
	void Start() {
		transform.localScale = new Vector3(Random.Range(1f, 5f), 3, Random.Range(1f, 5f));
	}

	// Update is called once per frame
	void Update() {

	}
}
