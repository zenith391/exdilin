using System;
using System.Collections.Generic;
using Blocks;
using UnityEngine;

// Token: 0x0200014C RID: 332
public class ConnectednessGraph
{
	// Token: 0x06001488 RID: 5256 RVA: 0x00090574 File Offset: 0x0008E974
	public static void Update(Block block)
	{
		ConnectednessGraph.visited.Clear();
		ConnectednessGraph.ComputeConnected(block);
	}

	// Token: 0x06001489 RID: 5257 RVA: 0x00090588 File Offset: 0x0008E988
	public static void Update(List<Block> blocks)
	{
		ConnectednessGraph.visited.Clear();
		foreach (Block block in blocks)
		{
			ConnectednessGraph.ComputeConnected(block);
			ConnectednessGraph.visited.Add(block);
		}
	}

	// Token: 0x0600148A RID: 5258 RVA: 0x000905F4 File Offset: 0x0008E9F4
	public static void Update(ITBox obj)
	{
		if (obj is Block)
		{
			ConnectednessGraph.Update((Block)obj);
		}
		else
		{
			ConnectednessGraph.Update(((Bunch)obj).blocks);
		}
	}

	// Token: 0x0600148B RID: 5259 RVA: 0x00090624 File Offset: 0x0008EA24
	private static void PrintConnections(List<Block> blocks)
	{
		foreach (Block block in blocks)
		{
			string text = block + " ==> ";
			for (int i = 0; i < block.connections.Count; i++)
			{
				if (i > 0)
				{
					text += ", ";
				}
				string text2 = text;
				text = string.Concat(new object[]
				{
					text2,
					block.connectionTypes[i],
					":",
					block.connections[i]
				});
			}
			BWLog.Info(text);
		}
	}

	// Token: 0x0600148C RID: 5260 RVA: 0x000906F4 File Offset: 0x0008EAF4
	public static List<List<Block>> FindChunkSubgraphs()
	{
		ConnectednessGraph.visited.Clear();
		List<List<Block>> list = new List<List<Block>>();
		foreach (Block block in BWSceneManager.AllBlocks())
		{
			if (!ConnectednessGraph.visited.Contains(block))
			{
				List<Block> list2 = new List<Block>();
				ConnectednessGraph.ConnectedComponent(block, 1, list2, false);
				list.Add(list2);
			}
		}
		return list;
	}

	// Token: 0x0600148D RID: 5261 RVA: 0x00090780 File Offset: 0x0008EB80
	public static bool BlocksChunkIsConnectedBySomeJoint(Block block)
	{
		List<Block> list = ConnectednessGraph.ConnectedComponent(block, 3, null, true);
		foreach (Block block2 in list)
		{
			foreach (int value in block2.connectionTypes)
			{
				if (Mathf.Abs(value) == 2)
				{
					return true;
				}
			}
		}
		return false;
	}

	// Token: 0x0600148E RID: 5262 RVA: 0x00090838 File Offset: 0x0008EC38
	public static List<Block> ConnectedComponent(Block block, int connectionType, List<Block> result = null, bool clearVisited = true)
	{
		if (result == null)
		{
			result = new List<Block>();
		}
		if (clearVisited)
		{
			ConnectednessGraph.visited.Clear();
		}
		ConnectednessGraph.ConnectedComponentRecursive(block, connectionType, result);
		return result;
	}

	// Token: 0x0600148F RID: 5263 RVA: 0x00090860 File Offset: 0x0008EC60
	private static void ConnectedComponentRecursive(Block block, int connectionType, List<Block> result)
	{
		if (ConnectednessGraph.visited.Contains(block))
		{
			return;
		}
		ConnectednessGraph.visited.Add(block);
		result.Add(block);
		for (int i = 0; i < block.connections.Count; i++)
		{
			if ((Mathf.Abs(block.connectionTypes[i]) & connectionType) != 0)
			{
				ConnectednessGraph.ConnectedComponentRecursive(block.connections[i], connectionType, result);
			}
		}
	}

	// Token: 0x06001490 RID: 5264 RVA: 0x000908D8 File Offset: 0x0008ECD8
	public static void Remove(Block b1)
	{
		foreach (Block block in b1.connections)
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

	// Token: 0x06001491 RID: 5265 RVA: 0x00090960 File Offset: 0x0008ED60
	public static void RemoveSafe(Block b1)
	{
		foreach (Block block in b1.connections)
		{
			int num = block.connections.IndexOf(b1);
			if (num != -1)
			{
				if (num < block.connections.Count)
				{
					block.connections.RemoveAt(num);
				}
				if (num < block.connectionTypes.Count)
				{
					block.connectionTypes.RemoveAt(num);
				}
				block.ConnectionsChanged();
			}
		}
		b1.connections = new List<Block>();
		b1.connectionTypes = new List<int>();
	}

	// Token: 0x06001492 RID: 5266 RVA: 0x00090A20 File Offset: 0x0008EE20
	private static void RemoveAllConnectionsExcept(Block b1, Block blockToKeep)
	{
		for (int i = 0; i < b1.connections.Count; i++)
		{
			if (!ConnectednessGraph.visited.Contains(b1.connections[i]))
			{
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
		}
		List<int> list = new List<int>();
		for (int j = 0; j < b1.connections.Count; j++)
		{
			Block block2 = b1.connections[j];
			if (block2 != blockToKeep && !ConnectednessGraph.visited.Contains(b1.connections[j]))
			{
				list.Add(j);
			}
		}
		list.Reverse();
		foreach (int index in list)
		{
			b1.connections.RemoveAt(index);
			b1.connectionTypes.RemoveAt(index);
		}
		b1.ConnectionsChanged();
	}

	// Token: 0x06001493 RID: 5267 RVA: 0x00090B78 File Offset: 0x0008EF78
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

	// Token: 0x06001494 RID: 5268 RVA: 0x00090BBC File Offset: 0x0008EFBC
	private static void ComputeConnected(Block b1)
	{
		ConnectednessGraph.RemoveAllConnectionsExcept(b1, null);
		Bounds shapeCollisionBounds = b1.GetShapeCollisionBounds();
		Collider[] array = Physics.OverlapSphere(b1.go.GetComponent<Collider>().bounds.center, shapeCollisionBounds.size.magnitude);
		bool flag = b1 is BlockAnimatedCharacter;
		bool flag2 = b1 is BlockMissile;
		foreach (Collider collider in array)
		{
			if (!(collider.gameObject == b1.go))
			{
				Block block = BWSceneManager.FindBlock(collider.gameObject, true);
				if (block != null && !ConnectednessGraph.visited.Contains(block))
				{
					bool flag3 = block is BlockAnimatedCharacter;
					bool flag4 = block is BlockMissile;
					if (!flag || !flag3)
					{
						if (!flag2 || !flag4)
						{
							if (CollisionTest.MultiMeshMeshTest(b1.glueMeshes, block.glueMeshes, false))
							{
								ConnectednessGraph.Connect(b1, block, ((!b1.isTerrain || block.isTerrain) && (b1.isTerrain || !block.isTerrain)) ? 1 : 4, false);
							}
							if (!block.isTerrain && CollisionTest.MultiMeshMeshTest(b1.jointMeshes, block.glueMeshes, false))
							{
								ConnectednessGraph.Connect(b1, block, 2, true);
							}
							if (!b1.isTerrain && CollisionTest.MultiMeshMeshTest(b1.glueMeshes, block.jointMeshes, false))
							{
								ConnectednessGraph.Connect(block, b1, 2, true);
							}
							if (CollisionTest.MultiMeshMeshTest(b1.jointMeshes, block.jointMeshes, false))
							{
								ConnectednessGraph.Connect(b1, block, 2, false);
							}
						}
					}
				}
			}
		}
	}

	// Token: 0x06001495 RID: 5269 RVA: 0x00090D98 File Offset: 0x0008F198
	public static void Connect(Block b1, Block b2, int connectionType, bool directed = false)
	{
		if (b1 == b2)
		{
			return;
		}
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

	// Token: 0x06001496 RID: 5270 RVA: 0x00090E04 File Offset: 0x0008F204
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

	// Token: 0x06001497 RID: 5271 RVA: 0x00090E78 File Offset: 0x0008F278
	public static int FindConnection(Block b1, Block b2)
	{
		int num = b1.connections.IndexOf(b2);
		if (num != -1)
		{
			return b1.connectionTypes[num];
		}
		return 0;
	}

	// Token: 0x06001498 RID: 5272 RVA: 0x00090EA8 File Offset: 0x0008F2A8
	public static void GlueBonds(Block b1, Block ignore, List<Vector3> resultPos1, List<Vector3> resultPos2, List<Block> resultBlock2)
	{
		bool activeSelf = b1.go.activeSelf;
		b1.go.SetActive(true);
		Collider[] array = Physics.OverlapSphere(b1.goT.position, b1.go.GetComponent<Collider>().bounds.extents.magnitude);
		foreach (Collider collider in array)
		{
			if (collider.gameObject != b1.go)
			{
				Block block = BWSceneManager.FindBlock(collider.gameObject, false);
				if (block != null && block != ignore && !block.isTerrain && block.BlockType() != "Position")
				{
					foreach (CollisionMesh collisionMesh in b1.jointMeshes)
					{
						foreach (CollisionMesh collisionMesh2 in block.glueMeshes)
						{
							if (CollisionTest.MeshMeshTest(collisionMesh, collisionMesh2, false))
							{
								Vector3 item = ConnectednessGraph.ComputeCenter(collisionMesh);
								Vector3 item2 = ConnectednessGraph.ComputeCenter(collisionMesh2);
								resultPos1.Add(item);
								resultPos2.Add(item2);
								resultBlock2.Add(block);
								break;
							}
						}
					}
					foreach (CollisionMesh collisionMesh3 in b1.glueMeshes)
					{
						foreach (CollisionMesh collisionMesh4 in block.glueMeshes)
						{
							if (CollisionTest.MeshMeshTest(collisionMesh3, collisionMesh4, false))
							{
								Vector3 item3 = ConnectednessGraph.ComputeCenter(collisionMesh3);
								Vector3 item4 = ConnectednessGraph.ComputeCenter(collisionMesh4);
								resultPos1.Add(item3);
								resultPos2.Add(item4);
								resultBlock2.Add(block);
								break;
							}
						}
					}
				}
			}
		}
		b1.go.SetActive(activeSelf);
	}

	// Token: 0x06001499 RID: 5273 RVA: 0x000910A4 File Offset: 0x0008F4A4
	private static Vector3 ComputeCenter(CollisionMesh cm)
	{
		Vector3 a = Vector3.zero;
		foreach (Triangle triangle in cm.Triangles)
		{
			a += triangle.V1;
			a += triangle.V2;
			a += triangle.V3;
		}
		return a / (float)(cm.Triangles.Length * 3);
	}

	// Token: 0x0400103F RID: 4159
	public const int TypeGlue = 1;

	// Token: 0x04001040 RID: 4160
	public const int TypeJoint = 2;

	// Token: 0x04001041 RID: 4161
	public const int TypeBase = 4;

	// Token: 0x04001042 RID: 4162
	private static HashSet<Block> visited = new HashSet<Block>();
}
