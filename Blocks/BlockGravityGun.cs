using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Blocks
{
	// Token: 0x02000091 RID: 145
	public class BlockGravityGun : Block
	{
		// Token: 0x06000BF4 RID: 3060 RVA: 0x000557A0 File Offset: 0x00053BA0
		public BlockGravityGun(List<List<Tile>> tiles) : base(tiles)
		{
			this.psObjectAttract = (UnityEngine.Object.Instantiate(Resources.Load("Blocks/BlockGravityGun Attract Particle System")) as GameObject);
			this.psObjectRepel = (UnityEngine.Object.Instantiate(Resources.Load("Blocks/BlockGravityGun Repel Particle System")) as GameObject);
			this.particlesAttract = this.psObjectAttract.GetComponent<ParticleSystem>();
			this.particlesRepel = this.psObjectRepel.GetComponent<ParticleSystem>();
			this.ResetParticles(true);
		}

		// Token: 0x06000BF5 RID: 3061 RVA: 0x00055832 File Offset: 0x00053C32
		private void ResetParticles(bool clear = true)
		{
			this.particlesAttract.enableEmission = false;
			this.particlesRepel.enableEmission = false;
			if (clear)
			{
				this.particlesAttract.Clear();
				this.particlesRepel.Clear();
			}
		}

		// Token: 0x06000BF6 RID: 3062 RVA: 0x00055868 File Offset: 0x00053C68
		public static PredicateSensorDelegate IsHitByGravityGun(Block block)
		{
			return (ScriptRowExecutionInfo eInfo, object[] args) => (!BlockGravityGun.blocksHit.Contains(block.go)) ? TileResultCode.False : TileResultCode.True;
		}

		// Token: 0x06000BF7 RID: 3063 RVA: 0x0005588E File Offset: 0x00053C8E
		public static void ClearHits()
		{
			BlockGravityGun.blocksHit.Clear();
		}

		// Token: 0x06000BF8 RID: 3064 RVA: 0x0005589C File Offset: 0x00053C9C
		public new static void Register()
		{
			PredicateRegistry.Add<BlockGravityGun>("GravityGun.Repel", null, (Block b) => new PredicateActionDelegate(((BlockGravityGun)b).Repel), null, null, null);
			PredicateRegistry.Add<BlockGravityGun>("GravityGun.Attract", null, (Block b) => new PredicateActionDelegate(((BlockGravityGun)b).Attract), null, null, null);
			string name = "GravityGun.HitBy";
			if (BlockGravityGun.f__mg_cache0 == null)
			{
				BlockGravityGun.f__mg_cache0 = new PredicateSensorConstructorDelegate(BlockGravityGun.IsHitByGravityGun);
			}
			PredicateRegistry.Add<Block>(name, BlockGravityGun.f__mg_cache0, null, null, null, null);
		}

		// Token: 0x06000BF9 RID: 3065 RVA: 0x00055930 File Offset: 0x00053D30
		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if ((double)Mathf.Abs(this.currentAttraction) > 0.01)
			{
				ParticleSystem particleSystem = this.particlesRepel;
				GameObject gameObject = this.psObjectRepel;
				if (this.currentAttraction > 0f)
				{
					particleSystem = this.particlesAttract;
					gameObject = this.psObjectAttract;
				}
				particleSystem.enableEmission = true;
				gameObject.transform.position = this.go.transform.position;
				gameObject.transform.rotation = this.go.transform.rotation;
				gameObject.transform.Rotate(-90f, 0f, 0f);
				Vector3 position = this.go.transform.position;
				float radius = this.influenceRadius;
				Collider[] array = Physics.OverlapSphere(position, radius);
				Vector3 up = this.go.transform.up;
				Vector3 position2 = this.go.transform.position;
				float d = 5f;
				Vector3 b = position2 - up * d;
				Rigidbody component = this.go.transform.parent.GetComponent<Rigidbody>();
				Vector3 a = default(Vector3);
				List<Rigidbody> list = new List<Rigidbody>();
				foreach (Collider collider in array)
				{
					Transform transform = collider.gameObject.transform;
					Transform parent = transform.parent;
					if (!(parent == null))
					{
						Rigidbody component2 = parent.GetComponent<Rigidbody>();
						if (!(component == component2))
						{
							if (component2 != null)
							{
								if (!list.Contains(component2))
								{
									list.Add(component2);
								}
								Bounds bounds = collider.bounds;
								Vector3 center = bounds.center;
								Vector3 a2 = center - position2;
								Vector3 to = center - b;
								float num = Vector3.Angle(up, to);
								float magnitude = a2.magnitude;
								if (num < this.influenceAngle)
								{
									BlockGravityGun.blocksHit.Add(collider.gameObject);
									Vector3 vector = a2 * -1f;
									vector.Normalize();
									Vector3 size = bounds.size;
									float num2 = size.x * size.y * size.z;
									float num3 = num2 * 20f * this.currentAttraction;
									num3 *= (this.influenceAngle - num) / this.influenceAngle;
									num3 *= (this.influenceRadius - magnitude) / this.influenceRadius;
									vector *= num3;
									component2.AddForceAtPosition(vector, center);
									a += vector;
								}
							}
						}
					}
				}
				foreach (Rigidbody rigidbody in list)
				{
					float d2 = 1f;
					rigidbody.AddForce(d2 * -Physics.gravity);
				}
				Rigidbody component3 = this.go.transform.parent.GetComponent<Rigidbody>();
				if (component3 != null)
				{
					component3.AddForceAtPosition(a * -this.selfForceMultiplier, position2);
				}
			}
			else
			{
				this.ResetParticles(false);
			}
			this.currentAttraction = 0f;
		}

		// Token: 0x06000BFA RID: 3066 RVA: 0x00055C98 File Offset: 0x00054098
		public override void Play()
		{
			base.Play();
			this.ResetParticles(true);
		}

		// Token: 0x06000BFB RID: 3067 RVA: 0x00055CA7 File Offset: 0x000540A7
		public override void Stop(bool resetBlock = true)
		{
			base.Stop(resetBlock);
			this.ResetParticles(true);
		}

		// Token: 0x06000BFC RID: 3068 RVA: 0x00055CB7 File Offset: 0x000540B7
		public TileResultCode Repel(ScriptRowExecutionInfo eInfo, object[] args)
		{
			this.currentAttraction -= 1f;
			return TileResultCode.True;
		}

		// Token: 0x06000BFD RID: 3069 RVA: 0x00055CCC File Offset: 0x000540CC
		public TileResultCode Attract(ScriptRowExecutionInfo eInfo, object[] args)
		{
			this.currentAttraction += 1f;
			return TileResultCode.True;
		}

		// Token: 0x04000987 RID: 2439
		public static List<GameObject> blocksHit = new List<GameObject>();

		// Token: 0x04000988 RID: 2440
		private float currentAttraction;

		// Token: 0x04000989 RID: 2441
		private float influenceAngle = 20f;

		// Token: 0x0400098A RID: 2442
		private float influenceRadius = 15f;

		// Token: 0x0400098B RID: 2443
		private float selfForceMultiplier = 0.25f;

		// Token: 0x0400098C RID: 2444
		private GameObject psObjectAttract;

		// Token: 0x0400098D RID: 2445
		private GameObject psObjectRepel;

		// Token: 0x0400098E RID: 2446
		private ParticleSystem particlesAttract;

		// Token: 0x0400098F RID: 2447
		private ParticleSystem particlesRepel;

		// Token: 0x04000990 RID: 2448
		[CompilerGenerated]
		private static PredicateSensorConstructorDelegate f__mg_cache0;
	}
}
