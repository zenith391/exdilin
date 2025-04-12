using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks
{
	// Token: 0x020000E9 RID: 233
	public class BlockVolume : Block
	{
		// Token: 0x0600113C RID: 4412 RVA: 0x000770E4 File Offset: 0x000754E4
		public BlockVolume(List<List<Tile>> tiles) : base(tiles)
		{
			this.go.GetComponent<Renderer>().enabled = Options.BlockVolumeDisplay;
			if (this.goShadow != null)
			{
				this.goShadow.GetComponent<Renderer>().enabled = false;
			}
		}

		// Token: 0x0600113D RID: 4413 RVA: 0x00077124 File Offset: 0x00075524
		public new static void Register()
		{
		}

		// Token: 0x0600113E RID: 4414 RVA: 0x00077128 File Offset: 0x00075528
		public override void Play()
		{
			base.Play();
			if (this.goT.parent.GetComponent<Rigidbody>())
			{
				this.goT.parent.GetComponent<Rigidbody>().isKinematic = true;
			}
			this.go.GetComponent<Renderer>().enabled = false;
			this.go.GetComponent<Collider>().isTrigger = true;
			this.IgnoreRaycasts(true);
		}

		// Token: 0x0600113F RID: 4415 RVA: 0x00077194 File Offset: 0x00075594
		public override void Stop(bool resetBlock = true)
		{
			base.Stop(resetBlock);
			this.go.GetComponent<Renderer>().enabled = Options.BlockVolumeDisplay;
			this.go.GetComponent<Collider>().isTrigger = false;
			if (this.goShadow != null)
			{
				this.goShadow.GetComponent<Renderer>().enabled = false;
			}
			this.IgnoreRaycasts(false);
		}

		// Token: 0x06001140 RID: 4416 RVA: 0x000771F7 File Offset: 0x000755F7
		public override bool IsColliding(float terrainOffset = 0f, HashSet<Block> exclude = null)
		{
			return false;
		}
	}
}
