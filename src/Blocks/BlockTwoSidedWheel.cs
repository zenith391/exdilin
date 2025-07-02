using System.Collections.Generic;
using UnityEngine;

namespace Blocks;

public class BlockTwoSidedWheel : BlockAbstractWheel
{
	private List<GameObject> pSideGOs;

	private List<GameObject> nSideGOs;

	private int nAxleIndex = -1;

	private int pAxleIndex = -1;

	private bool recomputeVisibility;

	public BlockTwoSidedWheel(List<List<Tile>> tiles)
		: base(tiles, string.Empty, string.Empty)
	{
		recomputeVisibility = true;
		nSideGOs = new List<GameObject>();
		pSideGOs = new List<GameObject>();
		for (int i = 0; i < goT.childCount; i++)
		{
			Transform child = goT.GetChild(i);
			if (child.name.EndsWith(" N"))
			{
				nSideGOs.Add(child.gameObject);
				if (nAxleIndex < 0 && (child.name.Contains("Axle") || child.name.EndsWith(" X N")))
				{
					nAxleIndex = nSideGOs.Count - 1;
				}
			}
			if (child.name.EndsWith(" P"))
			{
				pSideGOs.Add(child.gameObject);
				if (pAxleIndex < 0 && (child.name.Contains("Axle") || child.name.EndsWith(" X P")))
				{
					pAxleIndex = pSideGOs.Count - 1;
				}
			}
		}
	}

	public override void Update()
	{
		base.Update();
		if (!recomputeVisibility)
		{
			return;
		}
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		Vector3 position = goT.position;
		foreach (Block connection in connections)
		{
			Vector3 position2 = connection.goT.position;
			Vector3 lhs = position2 - position;
			float num = Vector3.Dot(lhs, goT.right);
			flag2 = flag2 || num < 0f;
			flag3 = flag3 || num > 0f;
			flag = flag || (!connection.isRuntimeInvisible && !connection.isTransparent);
		}
		if (flag)
		{
			for (int i = 0; i < nSideGOs.Count; i++)
			{
				nSideGOs[i].SetActive(flag2);
			}
			for (int j = 0; j < pSideGOs.Count; j++)
			{
				pSideGOs[j].SetActive(flag3);
			}
		}
		else
		{
			for (int k = 0; k < nSideGOs.Count; k++)
			{
				nSideGOs[k].SetActive(k != nAxleIndex);
			}
			for (int l = 0; l < pSideGOs.Count; l++)
			{
				pSideGOs[l].SetActive(l != pAxleIndex);
			}
		}
		recomputeVisibility = false;
	}

	public override void ConnectionsChanged()
	{
		recomputeVisibility = true;
		base.ConnectionsChanged();
	}
}
