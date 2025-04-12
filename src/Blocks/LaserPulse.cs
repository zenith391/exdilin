using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks
{
	// Token: 0x0200005B RID: 91
	public class LaserPulse : AbstractProjectile
	{
		// Token: 0x0600074D RID: 1869 RVA: 0x00031FA8 File Offset: 0x000303A8
		public LaserPulse(BlockAbstractLaser sender) : base(sender)
		{
			Vector3 fireDirectionUp = sender.GetFireDirectionUp();
			Vector3 fireDirectionForward = sender.GetFireDirectionForward();
			this._segments.Add(new AbstractProjectile.Segment(sender, base.GetSenderPosition() + this.extraSpeed * Blocksworld.fixedDeltaTime, fireDirectionForward, fireDirectionUp, this.kSpeed, this.extraSpeed, this.CreateSegmentGo()));
			this._segments[0].ReceivingEnergy = true;
			this.segmentTime = sender.segmentTime;
		}

		// Token: 0x0600074E RID: 1870 RVA: 0x0003204C File Offset: 0x0003044C
		public override void Destroy()
		{
			foreach (AbstractProjectile.Segment segment in this._segments)
			{
				segment.Destroy();
			}
		}

		// Token: 0x0600074F RID: 1871 RVA: 0x000320A8 File Offset: 0x000304A8
		protected override GameObject GetSegmentPrefab()
		{
			if (LaserPulse.segmentPrefab == null)
			{
				LaserPulse.segmentPrefab = new GameObject("Pulse");
				Mesh mesh = new Mesh();
				mesh.Clear();
				List<Vector3> list = new List<Vector3>();
				List<Vector2> list2 = new List<Vector2>();
				List<int> list3 = new List<int>();
				float sizeMult = 1f;
				BlockAbstractLaser.DrawLaserLine(-Vector3.forward * 0.5f, Vector3.forward * 0.5f, Vector3.up, list, list2, list3, sizeMult);
				mesh.vertices = list.ToArray();
				mesh.uv = list2.ToArray();
				mesh.triangles = list3.ToArray();
				mesh.name = "Pulse Mesh";
				MeshFilter meshFilter = LaserPulse.segmentPrefab.AddComponent<MeshFilter>();
				meshFilter.mesh = mesh;
				MeshRenderer meshRenderer = LaserPulse.segmentPrefab.AddComponent<MeshRenderer>();
				meshRenderer.material = (Material)Resources.Load("Materials/Laser2");
				LaserPulse.segmentPrefab.SetActive(false);
			}
			return LaserPulse.segmentPrefab;
		}

		// Token: 0x06000750 RID: 1872 RVA: 0x000321A0 File Offset: 0x000305A0
		protected override GameObject CreateSegmentGo()
		{
			GameObject original = this.GetSegmentPrefab();
			GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(original);
			gameObject.SetActive(true);
			Color laserColor = this._sender.GetLaserColor();
			MeshRenderer component = gameObject.GetComponent<MeshRenderer>();
			component.material.SetColor("_Color", laserColor);
			return gameObject;
		}

		// Token: 0x06000751 RID: 1873 RVA: 0x000321E7 File Offset: 0x000305E7
		public override bool ShouldBeDestroyed()
		{
			return this._segments.Count == 0;
		}

		// Token: 0x06000752 RID: 1874 RVA: 0x000321F8 File Offset: 0x000305F8
		public override void StopReceivingEnergy()
		{
			for (int i = 0; i < this._segments.Count; i++)
			{
				AbstractProjectile.Segment segment = this._segments[i];
				segment.ReceivingEnergy = false;
				segment.StartFraction = 0f;
			}
		}

		// Token: 0x06000753 RID: 1875 RVA: 0x00032240 File Offset: 0x00030640
		protected override void StepSegments()
		{
			float num = 0f;
			this.toRemove.Clear();
			for (int i = 0; i < this._segments.Count; i++)
			{
				AbstractProjectile.Segment segment = this._segments[i];
				segment.TravelledTime += Blocksworld.fixedDeltaTime;
				float num2 = Vector3.Dot(segment.ExtraVelocity, segment.Direction);
				if (this.gravityInfluence > 0f)
				{
					segment.ExtraVelocity += Physics.gravity * this.gravityInfluence * Blocksworld.fixedDeltaTime;
				}
				float num3 = segment.HeadSpeed + num2;
				float num4 = segment.TailSpeed + num2;
				segment.TravelledDist += num3 * Blocksworld.fixedDeltaTime;
				if (segment.ReceivingEnergy)
				{
					segment.Length += num3 * Blocksworld.fixedDeltaTime;
					segment.StartFraction = 1f - this.EnergyFraction;
				}
				segment.Length += num;
				float length = segment.Length;
				segment.Length += (num3 - num4) * Blocksworld.fixedDeltaTime;
				num = length - segment.Length;
				if (segment.Reflected && segment.ReceivingEnergy && num3 == 0f)
				{
					segment.Length = length;
				}
				segment.Origin += Util.ProjectOntoPlane(segment.ExtraVelocity, segment.Direction) * Blocksworld.fixedDeltaTime;
				Vector3 origin = segment.Origin + segment.Direction * (segment.TravelledDist - segment.Length);
				float num5 = this.segmentTime / this.kSpeed;
				RaycastHit raycastHit;
				if ((double)segment.Length < 0.0001 || segment.TravelledTime > num5)
				{
					this.toRemove.Add(segment);
				}
				else if (Physics.Raycast(origin, segment.Direction, out raycastHit, segment.Length))
				{
					Block block = BWSceneManager.FindBlock(raycastHit.collider.gameObject, false);
					if (block != null && (block != this._sender || segment.IsReflection))
					{
						this.AddHit(block, segment.Direction, raycastHit.point, raycastHit.normal);
					}
				}
			}
			for (int j = 0; j < this.toRemove.Count; j++)
			{
				AbstractProjectile.Segment segment2 = this.toRemove[j];
				segment2.Destroy();
				this._segments.Remove(segment2);
			}
			if (this._segments.Count == 0)
			{
				return;
			}
			AbstractProjectile.Segment segment3 = this._segments[this._segments.Count - 1];
			Vector3 a = segment3.Origin + segment3.Direction * segment3.TravelledDist;
			Vector3 vector = a - segment3.Direction * segment3.Length;
			int num6 = 0;
			RaycastHit hit;
			while (Physics.Raycast(vector, segment3.Direction, out hit, segment3.Length))
			{
				if (num6++ > 10)
				{
					Debug.LogWarning("Too many raycasts for pulsed laser projectile! Exiting loop");
					break;
				}
				Block block2 = BWSceneManager.FindBlock(hit.collider.gameObject, false);
				if (block2 == null || block2 is BlockSky)
				{
					break;
				}
				if (!this.travelThroughTransparent || !BlockAbstractLaser.IsTransparent(block2.GetTexture(0)))
				{
					segment3.HeadSpeed = 0f;
					segment3.ExtraVelocity = Vector3.zero;
					segment3.Reflected = false;
					if (this.canReflect && BlockAbstractLaser.IsReflective(block2.GetPaint(0)))
					{
						Vector3 point = hit.point;
						Vector3 vector2 = Vector3.Reflect(segment3.Direction, hit.normal);
						Vector3 up = Vector3.Reflect(segment3.Up, hit.normal);
						segment3.TravelledDist = (segment3.Origin - point).magnitude;
						segment3.Reflected = true;
						Vector3 extraSpeed = base.GetExtraSpeed(block2, vector2, point);
						AbstractProjectile.Segment segment4 = new AbstractProjectile.Segment(null, point + extraSpeed * Blocksworld.fixedDeltaTime, vector2, up, this.kSpeed, extraSpeed, this.CreateSegmentGo());
						segment4.IsReflection = true;
						segment4.TravelledTime = segment3.TravelledTime;
						this._segments.Add(segment4);
					}
					else
					{
						this.UpdateHitEffect(hit, segment3);
					}
					break;
				}
				float num7 = (hit.point - vector).magnitude;
				if (num7 < 0.0001f)
				{
					num7 = 0.0001f;
					vector = a - segment3.Direction * segment3.Length;
				}
				else
				{
					vector = hit.point;
				}
				segment3.Length -= num7;
				if (segment3.Length <= 0.0001f)
				{
					break;
				}
			}
		}

		// Token: 0x06000754 RID: 1876 RVA: 0x00032760 File Offset: 0x00030B60
		public override void Update()
		{
			float projectileSizeMultiplier = this._sender.projectileSizeMultiplier;
			for (int i = 0; i < this._segments.Count; i++)
			{
				AbstractProjectile.Segment segment = this._segments[i];
				Vector3 direction = segment.Direction;
				Vector3 vector = segment.Origin + direction * (segment.TravelledDist - segment.Length);
				Vector3 vector2 = vector + direction * segment.Length;
				float magnitude = (vector2 - vector).magnitude;
				if (magnitude >= 0.001f)
				{
					Transform goT = segment.goT;
					goT.localScale = new Vector3(projectileSizeMultiplier, projectileSizeMultiplier, magnitude);
					goT.position = 0.5f * (vector + vector2);
				}
			}
		}

		// Token: 0x04000586 RID: 1414
		protected List<AbstractProjectile.Segment> _segments = new List<AbstractProjectile.Segment>();

		// Token: 0x04000587 RID: 1415
		private float segmentTime = 100f;

		// Token: 0x04000588 RID: 1416
		private static GameObject segmentPrefab;

		// Token: 0x04000589 RID: 1417
		private List<AbstractProjectile.Segment> toRemove = new List<AbstractProjectile.Segment>();
	}
}
