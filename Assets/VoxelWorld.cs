using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelWorld : MonoBehaviour {

	public static VoxelWorld prefab;

	Dictionary<Vector2Int, Chunk> chunks = new Dictionary<Vector2Int, Chunk>();

	public GameController gameController;

	public Vector3 size;

	public Chunk GetChunk(Vector2Int location) {
		if (!chunks.ContainsKey(location)) {
			return null;
		}
		return chunks[location];
	}

	public void CreateChunk(Vector2Int location) {
		if (chunks.ContainsKey(location)) {
			chunks[location].Draw();
			return;
		}
		chunks.Add(location, Chunk.CreateChunk(location, this));
	}

	void drawChunk(Vector2Int location) {
		if (!chunks.ContainsKey(location)) {
			return;
		}
		chunks[location].Draw();
	}
}
