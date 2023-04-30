using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour {
	[SerializeField] private Texture2D image;

	[Header("Prefabs")]
	[SerializeField] private GameObject wallPrefab;
	[SerializeField] private GameObject floorPrefab;

	// Start is called before the first frame update
	void Start() {

	}

	// Update is called once per frame
	void Update() {
		GenerateLevel();
	}

	private void GenerateLevel() {
		Color[] pixelsInImage = image.GetPixels();

		int roomWidth = image.width;
		int roomHeight = image.height;

		Vector3[] spawnPosisions = new Vector3[pixelsInImage.Length];
		Vector3 startSpawnPos = new Vector3(-Mathf.Round(roomWidth / 2), 0, -roomHeight / 2);
		Vector3 currentSpawnPos = startSpawnPos;

		int index = 0;
		for (int z = 0; z < roomHeight; z++) {
			for (int x = 0; x < roomHeight; x++) {
				spawnPosisions[index] = currentSpawnPos;
				index++;
				currentSpawnPos.x++;
			}

			currentSpawnPos.x = startSpawnPos.x;
			currentSpawnPos.z++;
		}

		Transform roomPart = null;
		index = 0;
		foreach (Vector3 pos in spawnPosisions) {
			Color colour = pixelsInImage[index];
			if (colour.Equals(Color.white)) {
				//roomPart = Instantiate(floorPrefab, pos, Quaternion.identity).transform;
				index++;
				continue;
			} else if (colour.Equals(Color.black)) {
				roomPart = Instantiate(wallPrefab, pos, Quaternion.identity).transform;
			}

			index++;
		}
	}
}
