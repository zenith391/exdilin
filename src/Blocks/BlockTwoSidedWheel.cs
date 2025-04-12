using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks
{
	// Token: 0x020000E7 RID: 231
	public class BlockTwoSidedWheel : BlockAbstractWheel
	{
		// Token: 0x06001127 RID: 4391 RVA: 0x00076338 File Offset: 0x00074738
		public BlockTwoSidedWheel(List<List<Tile>> tiles) : base(tiles, string.Empty, string.Empty)
		{
			this.recomputeVisibility = true;
			this.nSideGOs = new List<GameObject>();
			this.pSideGOs = new List<GameObject>();
			for (int i = 0; i < this.goT.childCount; i++)
			{
				Transform child = this.goT.GetChild(i);
				if (child.name.EndsWith(" N"))
				{
					this.nSideGOs.Add(child.gameObject);
					if (this.nAxleIndex < 0 && (child.name.Contains("Axle") || child.name.EndsWith(" X N")))
					{
						this.nAxleIndex = this.nSideGOs.Count - 1;
					}
				}
				if (child.name.EndsWith(" P"))
				{
					this.pSideGOs.Add(child.gameObject);
					if (this.pAxleIndex < 0 && (child.name.Contains("Axle") || child.name.EndsWith(" X P")))
					{
						this.pAxleIndex = this.pSideGOs.Count - 1;
					}
				}
			}
		}

		// Token: 0x06001128 RID: 4392 RVA: 0x00076488 File Offset: 0x00074888
		public override void Update()
		{
			base.Update();
			if (this.recomputeVisibility)
			{
				bool flag = false;
				bool flag2 = false;
				bool flag3 = false;
				Vector3 position = this.goT.position;
				foreach (Block block in this.connections)
				{
					Vector3 position2 = block.goT.position;
					Vector3 lhs = position2 - position;
					float num = Vector3.Dot(lhs, this.goT.right);
					flag2 = (flag2 || num < 0f);
					flag3 = (flag3 || num > 0f);
					flag = (flag || (!block.isRuntimeInvisible && !block.isTransparent));
				}
				if (flag)
				{
					for (int i = 0; i < this.nSideGOs.Count; i++)
					{
						this.nSideGOs[i].SetActive(flag2);
					}
					for (int j = 0; j < this.pSideGOs.Count; j++)
					{
						this.pSideGOs[j].SetActive(flag3);
					}
				}
				else
				{
					for (int k = 0; k < this.nSideGOs.Count; k++)
					{
						this.nSideGOs[k].SetActive(k != this.nAxleIndex);
					}
					for (int l = 0; l < this.pSideGOs.Count; l++)
					{
						this.pSideGOs[l].SetActive(l != this.pAxleIndex);
					}
				}
				this.recomputeVisibility = false;
			}
		}

		// Token: 0x06001129 RID: 4393 RVA: 0x00076670 File Offset: 0x00074A70
		public override void ConnectionsChanged()
		{
			this.recomputeVisibility = true;
			base.ConnectionsChanged();
		}

		// Token: 0x04000D72 RID: 3442
		private List<GameObject> pSideGOs;

		// Token: 0x04000D73 RID: 3443
		private List<GameObject> nSideGOs;

		// Token: 0x04000D74 RID: 3444
		private int nAxleIndex = -1;

		// Token: 0x04000D75 RID: 3445
		private int pAxleIndex = -1;

		// Token: 0x04000D76 RID: 3446
		private bool recomputeVisibility;
	}
}
