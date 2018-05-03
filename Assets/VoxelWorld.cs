using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelWorld : MonoBehaviour {

	public static VoxelWorld prefab;

	Dictionary<Vector3Int, Chunk> chunks = new Dictionary<Vector3Int, Chunk>();

	public Chunk GetChunk(Vector3Int location) {
		if (!chunks.ContainsKey(location)) {
			return null;
		}
		return chunks[location];
	}

	public void CreateChunk(Vector3Int location) {
		if (chunks.ContainsKey(location)) {
			chunks[location].Draw();
			return;
		}
		chunks.Add(location, Chunk.CreateChunk(location, this));
	}

	void drawChunk(Vector3Int location) {
		if (!chunks.ContainsKey(location)) {
			return;
		}
		chunks[location].Draw();
	}
}
