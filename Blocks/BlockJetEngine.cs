using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks
{
	// Token: 0x02000098 RID: 152
	public class BlockJetEngine : BlockAbstractRocket
	{
		// Token: 0x06000C3A RID: 3130 RVA: 0x00056D68 File Offset: 0x00055168
		public BlockJetEngine(List<List<Tile>> tiles) : base(tiles, "Blocks/Jet Exhaust", "Blocks/Rocket Flame")
		{
			this.setSmokeColor = true;
			this.smokeColorMeshIndex = 0;
			this.blades = this.GetSubMeshGameObject(1);
			this.initBladesAngle = this.blades.transform.localRotation.eulerAngles.y;
			this.bladesAngle = this.initBladesAngle;
			this.bladesAngleVelocity = 0f;
		}

		// Token: 0x06000C3B RID: 3131 RVA: 0x00056DE0 File Offset: 0x000551E0
		public new static void Register()
		{
			PredicateRegistry.Add<BlockJetEngine>("JetEngine.Fire", (Block b) => new PredicateSensorDelegate(((BlockAbstractRocket)b).IsFiring), (Block b) => new PredicateActionDelegate(((BlockAbstractRocket)b).FireRocket), new Type[]
			{
				typeof(float)
			}, new string[]
			{
				"Force"
			}, null);
			PredicateRegistry.Add<BlockJetEngine>("JetEngine.Smoke", (Block b) => new PredicateSensorDelegate(((BlockAbstractRocket)b).IsFlaming), (Block b) => new PredicateActionDelegate(((BlockAbstractRocket)b).Flame), null, null, null);
			PredicateRegistry.Add<BlockJetEngine>("JetEngine.Flame", (Block b) => new PredicateSensorDelegate(((BlockAbstractRocket)b).IsSmoking), (Block b) => new PredicateActionDelegate(((BlockAbstractRocket)b).Smoke), null, null, null);
			Block.AddSimpleDefaultTiles(new GAF("JetEngine.Fire", new object[]
			{
				2f
			}), new string[]
			{
				"Jet Engine"
			});
		}

		// Token: 0x06000C3C RID: 3132 RVA: 0x00056F14 File Offset: 0x00055314
		private void ResetBladesAngle()
		{
			this.SetBladesAngle(this.initBladesAngle);
			this.bladesAngleVelocity = 0f;
		}

		// Token: 0x06000C3D RID: 3133 RVA: 0x00056F30 File Offset: 0x00055330
		public override void Play()
		{
			base.Play();
			Vector3 vector = base.Scale();
			this.ResetBladesAngle();
			this.canRotate = (Mathf.Abs(vector.x - vector.z) < 0.01f);
		}

		// Token: 0x06000C3E RID: 3134 RVA: 0x00056F71 File Offset: 0x00055371
		public override void Stop(bool resetBlock = true)
		{
			base.Stop(resetBlock);
			this.ResetBladesAngle();
		}

		// Token: 0x06000C3F RID: 3135 RVA: 0x00056F80 File Offset: 0x00055380
		private void SetBladesAngle(float a)
		{
			this.blades.transform.localRotation = Quaternion.Euler(0f, a, 0f);
		}

		// Token: 0x06000C40 RID: 3136 RVA: 0x00056FA4 File Offset: 0x000553A4
		public override void FixedUpdate()
		{
			if (this.canRotate && !this.broken)
			{
				float num = 0f;
				Rigidbody rb = this.chunk.rb;
				if (rb != null && !rb.isKinematic)
				{
					Vector3 up = this.goT.up;
					num = Mathf.Max(0f, Vector3.Dot(rb.velocity, up));
				}
				this.bladesAngleVelocity = 0.8f * this.bladesAngleVelocity + 0.2f * Mathf.Max(200f, 200f + this.smokeForce * 1000f + num * 20f) * Blocksworld.fixedDeltaTime;
				this.bladesAngle += this.bladesAngleVelocity;
				if (this.bladesAngle > 360f)
				{
					this.bladesAngle -= 360f;
				}
				this.SetBladesAngle(this.bladesAngle);
			}
			base.FixedUpdate();
		}

		// Token: 0x06000C41 RID: 3137 RVA: 0x0005709D File Offset: 0x0005549D
		public override bool HasPreferredLookTowardAngleLocalVector()
		{
			return true;
		}

		// Token: 0x06000C42 RID: 3138 RVA: 0x000570A0 File Offset: 0x000554A0
		public override Vector3 GetPreferredLookTowardAngleLocalVector()
		{
			return Vector3.down;
		}

		// Token: 0x040009A8 RID: 2472
		private GameObject blades;

		// Token: 0x040009A9 RID: 2473
		private float bladesAngleVelocity;

		// Token: 0x040009AA RID: 2474
		private float bladesAngle;

		// Token: 0x040009AB RID: 2475
		private float initBladesAngle;

		// Token: 0x040009AC RID: 2476
		private bool canRotate;
	}
}
