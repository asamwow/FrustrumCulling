using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour {

	VoxelWorld world;

	[SerializeField]
	Vector2Int startingSize;

	// [SerializeField]
	// Transform playerTransform;

	// public bool getVisible(Transform playerTransform, Chunk chunk) {
	// 	/// Chunk location at chunk.position * Chunk.size
	// }

	void Awake() {
		Chunk.prefab = loadResource<Chunk>("chunkPrefab");
		VoxelWorld.prefab = loadResource<VoxelWorld>("voxelWorldPrefab");
	}

	void Start () {
		world = GameObject.Instantiate(VoxelWorld.prefab);
		StartCoroutine(GenerateIteratively(world));
	}

	IEnumerator GenerateIteratively(VoxelWorld world) {
		for (int x = 0; x <= startingSize.x; x++) {
			for (int z = 0; z <= startingSize.y; z++) {
				// Subtract half of starting size to make the world centered over origin
				world.CreateChunk(new Vector3Int(x - startingSize.x / 2, 0, z - startingSize.y / 2));
				yield return new WaitForEndOfFrame();
			}
		}
	}

	/// Loads a resource from the resources folder
	T loadResource<T>(string location) where T : MonoBehaviour {
		return Resources.Load(location, typeof(T)) as T;
	}
}
