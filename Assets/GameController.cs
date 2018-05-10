using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour {

	public enum CullingType {
		None,
		Custom
	};

	public CullingType cullingType;

	VoxelWorld world;

	[SerializeField]
	GameObject playerPrefab;

	Transform playerTransform;

	bool loading = true;

	public Vector2Int startingSize;

	// [SerializeField]
	// Transform playerTransform;

	public bool getVisible(Transform playerTransform, Chunk chunk) {
		Vector3 chunkLocation = new Vector3(chunk.position.x * Chunk.size.x - Chunk.size.x/2f, 0f, chunk.position.y * Chunk.size.z - Chunk.size.z/2f);

        // Get the distance between the object and the camera
        Vector3 distance = playerTransform.position-chunk.transform.position;

        if (distance.magnitude <= Chunk.size.magnitude * 1.4) {
            return true;
        }

        float d = Vector3.Dot(distance, playerTransform.forward);

        // Calculate the cutoff distance
        float cutoff = Mathf.Tan(90 * Mathf.PI / 180) * d;

        // Calculate the vertical and horizontal distances between the object and the camera
        float xActual = Vector3.Dot(distance, playerTransform.right);
        float yActual = Vector3.Dot(distance, playerTransform.up);
        if (xActual >= cutoff || xActual <= -cutoff || yActual >= cutoff || yActual <= -cutoff) {
            return false;
        }
        return true;
	}

	void Awake() {
		Chunk.prefab = loadResource<Chunk>("chunkPrefab");
		VoxelWorld.prefab = loadResource<VoxelWorld>("voxelWorldPrefab");
	}

	void Start () {
		world = GameObject.Instantiate(VoxelWorld.prefab);
		world.gameController = this;
		for (int x = 0; x <= startingSize.x; x++) {
			for (int z = 0; z <= startingSize.y; z++) {
				// Subtract half of starting size to make the world centered over origin
				world.CreateChunk(new Vector2Int(x - startingSize.x / 2, z - startingSize.y / 2));
			}
		}
		StartCoroutine(GenerateIteratively(world));
	}

	void Update() {
		if (loading) {
			return;
		}

		if (Input.GetKeyDown(KeyCode.A)) {
			Debug.Log("Culling Now");
			cullingType = CullingType.Custom;
		}
		if (Input.GetKeyDown(KeyCode.B)) {
			Debug.Log("No Longer Culling");
			EnableAllChunks(world);
			cullingType = CullingType.None;
		}

		// Cull the scene
		if (cullingType == CullingType.Custom) {
			Cull(playerTransform, world);
		}
	}

	void Cull(Transform playerTransform, VoxelWorld voxelWorld) {
		for (int x = 0; x <= startingSize.x; x++) {
			for (int z = 0; z <= startingSize.y; z++) {
				// Subtract half of starting size to make the world centered over origin
				Chunk chunk = world.GetChunk(new Vector2Int(x - startingSize.x / 2, z - startingSize.y / 2));
				if (getVisible(playerTransform, chunk)) {
					chunk.gameObject.SetActive(true);
				} else {
					chunk.gameObject.SetActive(false);
				}
			}
		}
	}

	void EnableAllChunks(VoxelWorld voxelWorld) {
		for (int x = 0; x <= startingSize.x; x++) {
			for (int z = 0; z <= startingSize.y; z++) {
				world.GetChunk(new Vector2Int(x - startingSize.x / 2, z - startingSize.y / 2)).gameObject.SetActive(true);
			}
		}
	}

	IEnumerator GenerateIteratively(VoxelWorld world) {
		for (int x = 0; x <= startingSize.x; x++) {
			for (int z = 0; z <= startingSize.y; z++) {
				// Subtract half of starting size to make the world centered over origin
				world.GetChunk(new Vector2Int(x - startingSize.x / 2, z - startingSize.y / 2)).Draw();
				yield return null;
			}
		}
		playerTransform = Instantiate(playerPrefab, Vector3.up*100f, Quaternion.identity).transform;
		loading = false;
	}

	/// Loads a resource from the resources folder
	T loadResource<T>(string location) where T : MonoBehaviour {
		return Resources.Load(location, typeof(T)) as T;
	}
}
