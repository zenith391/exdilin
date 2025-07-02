using System.Collections.Generic;
using Blocks;
using UnityEngine;

public class ConnectednessGraph
{
	public const int TypeGlue = 1;

	public const int TypeJoint = 2;

	public const int TypeBase = 4;

	private static HashSet<Block> visited = new HashSet<Block>();

	public static void Update(Block block)
	{
		visited.Clear();
		ComputeConnected(block);
	}

	public static void Update(List<Block> blocks)
	{
		visited.Clear();
		foreach (Block block in blocks)
		{
			ComputeConnected(block);
			visited.Add(block);
		}
	}

	public static void Update(ITBox obj)
	{
		if (obj is Block)
		{
			Update((Block)obj);
		}
		else
		{
			Update(((Bunch)obj).blocks);
		}
	}

	private static void PrintConnections(List<Block> blocks)
	{
		foreach (Block block in blocks)
		{
			string text = block?.ToString() + " ==> ";
			for (int i = 0; i < block.connections.Count; i++)
			{
				if (i > 0)
				{
					text += ", ";
				}
				string text2 = text;
				text = text2 + block.connectionTypes[i] + ":" + block.connections[i];
			}
			BWLog.Info(text);
		}
	}

	public static List<List<Block>> FindChunkSubgraphs()
	{
		visited.Clear();
		List<List<Block>> list = new List<List<Block>>();
		foreach (Block item in BWSceneManager.AllBlocks())
		{
			if (!visited.Contains(item))
			{
				List<Block> list2 = new List<Block>();
				ConnectedComponent(item, 1, list2, clearVisited: false);
				list.Add(list2);
			}
		}
		return list;
	}

	public static bool BlocksChunkIsConnectedBySomeJoint(Block block)
	{
		List<Block> list = ConnectedComponent(block, 3);
		foreach (Block item in list)
		{
			foreach (int connectionType in item.connectionTypes)
			{
				if (Mathf.Abs(connectionType) == 2)
				{
					return true;
				}
			}
		}
		return false;
	}

	public static List<Block> ConnectedComponent(Block block, int connectionType, List<Block> result = null, bool clearVisited = true)
	{
		if (result == null)
		{
			result = new List<Block>();
		}
		if (clearVisited)
		{
			visited.Clear();
		}
		ConnectedComponentRecursive(block, connectionType, result);
		return result;
	}

	private static void ConnectedComponentRecursive(Block block, int connectionType, List<Block> result)
	{
		if (visited.Contains(block))
		{
			return;
		}
		visited.Add(block);
		result.Add(block);
		for (int i = 0; i < block.connections.Count; i++)
		{
			if ((Mathf.Abs(block.connectionTypes[i]) & connectionType) != 0)
			{
				ConnectedComponentRecursive(block.connections[i], connectionType, result);
			}
		}
	}

	public static void Remove(Block b1)
	{
		foreach (Block connection in b1.connections)
		{
			int num = connection.connections.IndexOf(b1);
			if (num != -1)
			{
				connection.connections.RemoveAt(num);
				connection.connectionTypes.RemoveAt(num);
				connection.ConnectionsChanged();
			}
		}
	}

	public static void RemoveSafe(Block b1)
	{
		foreach (Block connection in b1.connections)
		{
			int num = connection.connections.IndexOf(b1);
			if (num != -1)
			{
				if (num < connection.connections.Count)
				{
					connection.connections.RemoveAt(num);
				}
				if (num < connection.connectionTypes.Count)
				{
					connection.connectionTypes.RemoveAt(num);
				}
				connection.ConnectionsChanged();
			}
		}
		b1.connections = new List<Block>();
		b1.connectionTypes = new List<int>();
	}

	private static void RemoveAllConnectionsExcept(Block b1, Block blockToKeep)
	{
		for (int i = 0; i < b1.connections.Count; i++)
		{
			if (visited.Contains(b1.connections[i]))
			{
				continue;
			}
			Block block = b1.connections[i];
			if (block != blockToKeep)
			{
				int num = block.connections.IndexOf(b1);
				if (num != -1)
				{
					block.connections.RemoveAt(num);
					block.connectionTypes.RemoveAt(num);
					block.ConnectionsChanged();
				}
			}
		}
		List<int> list = new List<int>();
		for (int j = 0; j < b1.connections.Count; j++)
		{
			Block block2 = b1.connections[j];
			if (block2 != blockToKeep && !visited.Contains(b1.connections[j]))
			{
				list.Add(j);
			}
		}
		list.Reverse();
		foreach (int item in list)
		{
			b1.connections.RemoveAt(item);
			b1.connectionTypes.RemoveAt(item);
		}
		b1.ConnectionsChanged();
	}

	private static int GetIndexOfConnectedAnimatedBlockster(Block b1)
	{
		for (int i = 0; i < b1.connections.Count; i++)
		{
			if (b1.connections[i] is BlockAnimatedCharacter)
			{
				return i;
			}
		}
		return -1;
	}

	private static void ComputeConnected(Block b1)
	{
		RemoveAllConnectionsExcept(b1, null);
		Bounds shapeCollisionBounds = b1.GetShapeCollisionBounds();
		Collider[] array = Physics.OverlapSphere(b1.go.GetComponent<Collider>().bounds.center, shapeCollisionBounds.size.magnitude);
		bool flag = b1 is BlockAnimatedCharacter;
		bool flag2 = b1 is BlockMissile;
		Collider[] array2 = array;
		foreach (Collider collider in array2)
		{
			if (collider.gameObject == b1.go)
			{
				continue;
			}
			Block block = BWSceneManager.FindBlock(collider.gameObject, checkChildGos: true);
			if (block == null || visited.Contains(block))
			{
				continue;
			}
			bool flag3 = block is BlockAnimatedCharacter;
			bool flag4 = block is BlockMissile;
			if ((!flag || !flag3) && (!flag2 || !flag4))
			{
				if (CollisionTest.MultiMeshMeshTest(b1.glueMeshes, block.glueMeshes))
				{
					Connect(b1, block, ((!b1.isTerrain || block.isTerrain) && (b1.isTerrain || !block.isTerrain)) ? 1 : 4);
				}
				if (!block.isTerrain && CollisionTest.MultiMeshMeshTest(b1.jointMeshes, block.glueMeshes))
				{
					Connect(b1, block, 2, directed: true);
				}
				if (!b1.isTerrain && CollisionTest.MultiMeshMeshTest(b1.glueMeshes, block.jointMeshes))
				{
					Connect(block, b1, 2, directed: true);
				}
				if (CollisionTest.MultiMeshMeshTest(b1.jointMeshes, block.jointMeshes))
				{
					Connect(b1, block, 2);
				}
			}
		}
	}

	public static void Connect(Block b1, Block b2, int connectionType, bool directed = false)
	{
		if (b1 != b2)
		{
			b1.connections.Add(b2);
			b1.connectionTypes.Add(connectionType);
			b2.connections.Add(b1);
			if (directed)
			{
				b2.connectionTypes.Add(-connectionType);
			}
			else
			{
				b2.connectionTypes.Add(connectionType);
			}
			b1.ConnectionsChanged();
			b2.ConnectionsChanged();
		}
	}

	public static void Disconnect(Block b1, Block b2)
	{
		int num = b1.connections.IndexOf(b2);
		if (num != -1)
		{
			b1.connections.RemoveAt(num);
			b1.connectionTypes.RemoveAt(num);
			b1.ConnectionsChanged();
		}
		int num2 = b2.connections.IndexOf(b1);
		if (num2 != -1)
		{
			b2.connections.RemoveAt(num2);
			b2.connectionTypes.RemoveAt(num2);
			b2.ConnectionsChanged();
		}
	}

	public static int FindConnection(Block b1, Block b2)
	{
		int num = b1.connections.IndexOf(b2);
		if (num != -1)
		{
			return b1.connectionTypes[num];
		}
		return 0;
	}

	public static void GlueBonds(Block b1, Block ignore, List<Vector3> resultPos1, List<Vector3> resultPos2, List<Block> resultBlock2)
	{
		bool activeSelf = b1.go.activeSelf;
		b1.go.SetActive(value: true);
		Collider[] array = Physics.OverlapSphere(b1.goT.position, b1.go.GetComponent<Collider>().bounds.extents.magnitude);
		Collider[] array2 = array;
		foreach (Collider collider in array2)
		{
			if (!(collider.gameObject != b1.go))
			{
				continue;
			}
			Block block = BWSceneManager.FindBlock(collider.gameObject);
			if (block == null || block == ignore || block.isTerrain || !(block.BlockType() != "Position"))
			{
				continue;
			}
			CollisionMesh[] jointMeshes = b1.jointMeshes;
			foreach (CollisionMesh collisionMesh in jointMeshes)
			{
				CollisionMesh[] glueMeshes = block.glueMeshes;
				foreach (CollisionMesh collisionMesh2 in glueMeshes)
				{
					if (CollisionTest.MeshMeshTest(collisionMesh, collisionMesh2))
					{
						Vector3 item = ComputeCenter(collisionMesh);
						Vector3 item2 = ComputeCenter(collisionMesh2);
						resultPos1.Add(item);
						resultPos2.Add(item2);
						resultBlock2.Add(block);
						break;
					}
				}
			}
			CollisionMesh[] glueMeshes2 = b1.glueMeshes;
			foreach (CollisionMesh collisionMesh3 in glueMeshes2)
			{
				CollisionMesh[] glueMeshes3 = block.glueMeshes;
				foreach (CollisionMesh collisionMesh4 in glueMeshes3)
				{
					if (CollisionTest.MeshMeshTest(collisionMesh3, collisionMesh4))
					{
						Vector3 item3 = ComputeCenter(collisionMesh3);
						Vector3 item4 = ComputeCenter(collisionMesh4);
						resultPos1.Add(item3);
						resultPos2.Add(item4);
						resultBlock2.Add(block);
						break;
					}
				}
			}
		}
		b1.go.SetActive(activeSelf);
	}

	private static Vector3 ComputeCenter(CollisionMesh cm)
	{
		Vector3 zero = Vector3.zero;
		Triangle[] triangles = cm.Triangles;
		for (int i = 0; i < triangles.Length; i++)
		{
			Triangle triangle = triangles[i];
			zero += triangle.V1;
			zero += triangle.V2;
			zero += triangle.V3;
		}
		return zero / (cm.Triangles.Length * 3);
	}
}
