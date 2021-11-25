using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks
{
	// Token: 0x02000095 RID: 149
	public class BlockHover : Block
	{
		// Token: 0x06000C27 RID: 3111 RVA: 0x0005678F File Offset: 0x00054B8F
		public BlockHover(List<List<Tile>> tiles) : base(tiles)
		{
		}

		// Token: 0x06000C28 RID: 3112 RVA: 0x000567A4 File Offset: 0x00054BA4
		public new static void Register()
		{
			PredicateRegistry.Add<BlockHover>("Hover.IncreaseHeight", null, (Block b) => new PredicateActionDelegate(((BlockHover)b).IncreaseHeight), null, null, null);
			PredicateRegistry.Add<BlockHover>("Hover.DecreaseHeight", null, (Block b) => new PredicateActionDelegate(((BlockHover)b).DecreaseHeight), null, null, null);
		}

		// Token: 0x06000C29 RID: 3113 RVA: 0x0005680C File Offset: 0x00054C0C
		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if ((double)Math.Abs(this.hoverHeight) > 0.01)
			{
				RaycastHit[] array = Physics.RaycastAll(this.goT.position, Vector3.down, this.hoverHeight + 5f);
				Array.Sort<RaycastHit>(array, new RaycastDistanceComparer(this.goT.position));
				Vector3 vector = default(Vector3);
				bool flag = false;
				foreach (RaycastHit raycastHit in array)
				{
					Block block = BWSceneManager.FindBlock(raycastHit.collider.gameObject, false);
					if (block != null)
					{
						Rigidbody component = block.goT.parent.GetComponent<Rigidbody>();
						if (component != null)
						{
							if (!this.allRigidbodies.Contains(component))
							{
								flag = true;
							}
						}
						else
						{
							flag = true;
						}
					}
					if (flag)
					{
						vector = raycastHit.point;
						break;
					}
				}
				if (flag)
				{
					float magnitude = (vector - this.goT.position).magnitude;
					Debug.DrawLine(this.goT.position, vector);
					float num = magnitude - this.hoverHeight;
					float num2 = num - this.prevError;
					this.prevError = num;
					float num3 = 0.5f;
					float num4 = 8f;
					float num5 = num3 * num + num4 * num2;
					num5 = -1f + Mathf.Clamp(num5, -1f, 1f);
					foreach (Rigidbody rigidbody in this.allRigidbodies)
					{
						rigidbody.AddForce(Physics.gravity * rigidbody.mass * num5);
					}
				}
			}
			this.hoverHeight = this.defaultHoverHeight;
		}

		// Token: 0x06000C2A RID: 3114 RVA: 0x00056A0C File Offset: 0x00054E0C
		public override void Play()
		{
			base.Play();
			this.UpdateRigidBodyList();
		}

		// Token: 0x06000C2B RID: 3115 RVA: 0x00056A1C File Offset: 0x00054E1C
		private void UpdateRigidBodyList()
		{
			List<Block> list = ConnectednessGraph.ConnectedComponent(this, 3, null, true);
			this.allRigidbodies = new List<Rigidbody>();
			foreach (Block block in list)
			{
				Rigidbody component = block.goT.parent.GetComponent<Rigidbody>();
				if (component != null && !this.allRigidbodies.Contains(component))
				{
					this.allRigidbodies.Add(component);
				}
			}
		}

		// Token: 0x06000C2C RID: 3116 RVA: 0x00056ABC File Offset: 0x00054EBC
		public TileResultCode IncreaseHeight(ScriptRowExecutionInfo eInfo, object[] args)
		{
			this.hoverHeight += 1f;
			return TileResultCode.True;
		}

		// Token: 0x06000C2D RID: 3117 RVA: 0x00056AD1 File Offset: 0x00054ED1
		public TileResultCode DecreaseHeight(ScriptRowExecutionInfo eInfo, object[] args)
		{
			this.hoverHeight -= 1f;
			return TileResultCode.True;
		}

		// Token: 0x0400099D RID: 2461
		private float hoverHeight;

		// Token: 0x0400099E RID: 2462
		private float defaultHoverHeight = 5f;

		// Token: 0x0400099F RID: 2463
		private float prevError;

		// Token: 0x040009A0 RID: 2464
		private List<Rigidbody> allRigidbodies;
	}
}
