using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour {

	VoxelWorld world;

	[SerializeField]
	GameObject playerPrefab;

	Transform playerTransform;

	bool loading = true;

	public Vector2Int startingSize;

	// [SerializeField]
	// Transform playerTransform;

	public bool getVisible(Transform playerTransform, Chunk chunk) {
		Vector3 chunkLocation = new Vector3(chunk.position.x * Chunk.size.x - Chunk.size.x/2f, chunk.position.y * Chunk.size.y - Chunk.size.y/2f, chunk.position.z * Chunk.size.z - Chunk.size.z/2f);
		Debug.Log(Vector3.Dot(playerTransform.forward, chunkLocation - playerTransform.position));

        // Get the distance between the object and the camera
        Vector3 distance = chunk.transform.position - playerTransform.position;

        if (distance.magnitude <= Chunk.size.magnitude * 1.4)
        {
            return true;
        }

        float d = Vector3.Dot(distance, playerTransform.forward);



        // Calculate the cutoff distance
        float cutoff = Mathf.Tan(Camera.main.fieldOfView * Mathf.PI / 180) * d;

        // Calculate the vertical and horizontal distances between the object and the camera
        float xActual = Vector3.Dot(distance, playerTransform.right);
        float yActual = Vector3.Dot(distance, playerTransform.up);


        if (
            xActual >= cutoff ||
            xActual <= -cutoff ||
            yActual >= cutoff ||
            yActual <= -cutoff
            )
        {
            Debug.Log("FALSE");
            return false;
        }

        Debug.Log("True");



        return true;
	}

	void Awake() {
		Chunk.prefab = loadResource<Chunk>("chunkPrefab");
		VoxelWorld.prefab = loadResource<VoxelWorld>("voxelWorldPrefab");
		// VoxelWorld.size = new Vector3(startingSize.x, Chunk.size.z, startingSize.y);
	}

	void Start () {
		world = GameObject.Instantiate(VoxelWorld.prefab);
		world.gameController = this;
		for (int x = 0; x <= startingSize.x; x++) {
			for (int z = 0; z <= startingSize.y; z++) {
				// Subtract half of starting size to make the world centered over origin
				world.CreateChunk(new Vector3Int(x - startingSize.x / 2, 0, z - startingSize.y / 2));
			}
		}
		StartCoroutine(GenerateIteratively(world));
	}

	void Update() {
		if (!loading) {
            // Cull the scene
            Cull(playerTransform, world);
        }



	}

	void Cull(Transform playerTransform, VoxelWorld voxelWorld) {
		for (int x = 0; x <= startingSize.x; x++) {
			for (int z = 0; z <= startingSize.y; z++) {
				// Subtract half of starting size to make the world centered over origin
				Chunk chunk = world.GetChunk(new Vector3Int(x - startingSize.x / 2, 0, z - startingSize.y / 2));
				if (getVisible(playerTransform, chunk)) {
					chunk.gameObject.SetActive(true);
				} else {
					chunk.gameObject.SetActive(false);
				}
			}
		}
	}

	IEnumerator GenerateIteratively(VoxelWorld world) {
		for (int x = 0; x <= startingSize.x; x++) {
			for (int z = 0; z <= startingSize.y; z++) {
				// Subtract half of starting size to make the world centered over origin
				world.GetChunk(new Vector3Int(x - startingSize.x / 2, 0, z - startingSize.y / 2)).Draw();
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
