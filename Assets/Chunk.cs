using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour {

	public static Vector3Int size = new Vector3Int(32, 64, 32);
	public static Chunk prefab;

	MeshRenderer renderer;
	MeshFilter filter;
	MeshCollider collider;

	/// Position of the chunk in chunk coordinates
	public Vector2Int position {get; private set;}

	/// World this chunk is in
	VoxelWorld world;

	/// The blocks this chunk is composed of
	Block[,,] blocks;

	public static Chunk CreateChunk(Vector2Int position, VoxelWorld world) {
		Chunk newChunk = GameObject.Instantiate(prefab);
		newChunk.position = position;
		newChunk.world = world;
		Vector3 location = new Vector3(position.x, 0f, position.y);
		location.x *= size.x * 2f;
		location.y *= size.y * 2f;
		location.z *= size.z * 2f;
		newChunk.transform.position = location;
		newChunk.transform.parent = world.transform;
		return newChunk;
	}

	void Awake() {
		renderer = GetComponent<MeshRenderer>();
		filter = GetComponent<MeshFilter>();
		collider = GetComponent<MeshCollider>();
		blocks = new Block[size.x, size.y, size.z];
	}

	void Start() {
		for (int x = 0; x < size.x; x++) {
			for (int z = 0; z < size.z; z++) {
				int xWorld = Mathf.FloorToInt(x + position.x * size.x);
				int zWorld = Mathf.FloorToInt(z + position.y * size.z);
				int height = GenerateHeight(0.3f, xWorld, zWorld);
				for (int y = 0; y < height; y++) {
					blocks[x, y, z] = Block.Create(Block.Type.Dirt);
				}
			}
		}
	}

	int GenerateHeight(float frequency, int xWorld, int zWorld) {
		return Mathf.RoundToInt(Mathf.PerlinNoise(xWorld*0.05145f*frequency, zWorld*0.05145f*frequency)*size.y);
	}

	public void Draw() {
		Mesh mesh = new Mesh();
		int quadCount = 0;
		Chunk[] neighboringChunks = new Chunk[4];
		for (int i = 0; i < 4; i++) {
			neighboringChunks[i] = world.GetChunk(indexToDirection2D(i) + position);
		}

		for (int x = 0; x < size.x; x++) {
			for (int y = 0; y < size.y; y++) {
				for (int z = 0; z < size.z; z++) {
					if (blocks[x, y, z] == null) {
						continue;
					}
					// Create quad where empty space is found
					for (int i = 0; i < 6; i++) {
						bool outOfBounds;
						if (getBlock(indexToDirection(i) + new Vector3Int(x, y, z), neighboringChunks, out outOfBounds) == null) {
							if (!outOfBounds) {
								quadCount++;
							}
						}
					}
				}
			}
		}

		Vector3[] vertices = new Vector3[quadCount * 4];
		Vector3[] normals = new Vector3[quadCount * 4];
		int[] triangles = new int[quadCount * 6];

		int quadIndex = 0;

		for (int x = 0; x < size.x; x++) {
			for (int y = 0; y < size.y; y++) {
				for (int z = 0; z < size.z; z++) {
					if (blocks[x, y, z] == null) {
						continue;
					}
					// Create quad where empty space is found
					Vector3 blockCenter = new Vector3(x, y, z) * 2f;
					for (int i = 0; i < 6; i++) {
						bool outOfBounds;
						if (getBlock(indexToDirection(i) + new Vector3Int(x, y, z), neighboringChunks, out outOfBounds) != null) {
							continue;
						}
						if (outOfBounds) {
							continue;
						}
						Vector3Int direction = indexToDirection(i);
						Vector3Int perpDir0, perpDir1;
						bool reverse = false;
						switch(i) {
							case 0:
								perpDir0 = Vector3Int.right;
								perpDir1 = new Vector3Int(0, 0, 1);
								break;
							case 1:
								perpDir0 = Vector3Int.right;
								perpDir1 = new Vector3Int(0, 0, 1);
								reverse = true;
								break;
							case 2:
								perpDir0 = Vector3Int.up;
								perpDir1 = new Vector3Int(0, 0, 1);
								reverse = true;
								break;
							case 3:
								perpDir0 = Vector3Int.up;
								perpDir1 = new Vector3Int(0, 0, 1);
								break;
							case 4:
								perpDir1 = Vector3Int.right;
								perpDir0 = Vector3Int.up;
								break;
							default:
								perpDir1 = Vector3Int.right;
								perpDir0 = Vector3Int.up;
								reverse = true;
								break;
						}
						int corner0Index = quadIndex * 4 + 0;
						int corner1Index = quadIndex * 4 + 1;
						int corner2Index = quadIndex * 4 + 2;
						int corner3Index = quadIndex * 4 + 3;

						vertices[corner0Index] = blockCenter + direction + perpDir0 + perpDir1;
						vertices[corner1Index] = blockCenter + direction - perpDir0 + perpDir1;
						vertices[corner2Index] = blockCenter + direction - perpDir0 - perpDir1;
						vertices[corner3Index] = blockCenter + direction + perpDir0 - perpDir1;

						normals[corner0Index] = direction;
						normals[corner1Index] = direction;
						normals[corner2Index] = direction;
						normals[corner3Index] = direction;

						if (reverse) {
							triangles[quadIndex * 6 + 0] = corner0Index;
							triangles[quadIndex * 6 + 1] = corner2Index;
							triangles[quadIndex * 6 + 2] = corner1Index;
							triangles[quadIndex * 6 + 3] = corner0Index;
							triangles[quadIndex * 6 + 4] = corner3Index;
							triangles[quadIndex * 6 + 5] = corner2Index;
						} else {
							triangles[quadIndex * 6 + 0] = corner0Index;
							triangles[quadIndex * 6 + 1] = corner1Index;
							triangles[quadIndex * 6 + 2] = corner2Index;
							triangles[quadIndex * 6 + 3] = corner0Index;
							triangles[quadIndex * 6 + 4] = corner2Index;
							triangles[quadIndex * 6 + 5] = corner3Index;
						}
						quadIndex++;
					}
				}
			}
		}
		mesh.vertices = vertices;
		mesh.triangles = triangles;
		mesh.normals = normals;
		filter.mesh = mesh;
		collider.sharedMesh = mesh;
	}

	/// Gets a block from this chunk
	public Block GetBlock(Vector3Int position) {
		if (position.x < 0 || position.x >= size.x || position.y < 0 || position.y >= size.y || position.z < 0 || position.z >= size.z) {
			return null;
		}
		return blocks[position.x, position.y, position.z];
	}

	/// Converts at 1D index to a 3D direction
	Vector3Int indexToDirection(int index) {
		switch(index) {
			case 0:
				return Vector3Int.down;
			case 1:
				return Vector3Int.up;
			case 2:
				return Vector3Int.left;
			case 3:
				return Vector3Int.right;
			case 4:
				return new Vector3Int(0, 0, -1);
			default:
				return new Vector3Int(0, 0, 1);
		}
	}

	/// Converts at 1D index to a 3D direction
	Vector2Int indexToDirection2D(int index) {
		switch(index) {
			case 0:
				return new Vector2Int(0, -1);
			case 1:
				return new Vector2Int(0, 1);
			case 2:
				return new Vector2Int(-1, 0);
			default:
				return new Vector2Int(1, 0);
		}
	}

	/// Gets block that could potentially be in the neihbor
	Block getBlock(Vector3Int position, Chunk[] neighboringChunks, out bool outOfBounds) {
		if (position.y < 0 || position.y >= size.y) {
			outOfBounds = true;
			return null;
		}

		int outCount = 0;
		if (position.x < 0 || position.x >= size.x) {
			outCount++;
		}
		if (position.z < 0 || position.z >= size.z) {
			outCount++;
		}

		if (outCount > 1) {
			Debug.LogError("Getting block that is not in chunk nor in neighbor.");
			outOfBounds = true;
			return null;
		}

		if (outCount == 0) {
			outOfBounds = false;
			return blocks[position.x, position.y, position.z];
		}

		if (position.x < 0) {
			if (neighboringChunks[2] == null) {
				outOfBounds = true;
				return null;
			}
			outOfBounds = false;
			return neighboringChunks[2].GetBlock(new Vector3Int(size.x + position.x, position.y, position.z));
		}
		if (position.x >= size.x) {
			if (neighboringChunks[3] == null) {
				outOfBounds = true;
				return null;
			}
			outOfBounds = false;
			return neighboringChunks[3].GetBlock(new Vector3Int(position.x - size.x, position.y, position.z));
		}
		if (position.z < 0) {
			if (neighboringChunks[0] == null) {
				outOfBounds = true;
				return null;
			}
			outOfBounds = false;
			return neighboringChunks[0].GetBlock(new Vector3Int(position.x, position.y, size.z + position.z));
		}
		// if (position.z >= size.z) {
			if (neighboringChunks[1] == null) {
				outOfBounds = true;
				return null;
			}
			outOfBounds = false;
			return neighboringChunks[1].GetBlock(new Vector3Int(position.x, position.y, position.z - size.z));
		// }
		// outOfBounds = true;
		// return null;
	}
}
