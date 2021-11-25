using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks
{
	// Token: 0x0200008F RID: 143
	public class BlockFlightYoke : BlockAbstractAntiGravity
	{
		// Token: 0x06000BC7 RID: 3015 RVA: 0x00054870 File Offset: 0x00052C70
		public BlockFlightYoke(List<List<Tile>> tiles) : base(tiles)
		{
			this.informAboutVaryingGravity = false;
			this.meshesToDrive.Add(this.goT.Find("FlightYokeTriggers"));
			this.meshesToDrive.Add(this.goT.Find("FlightYokeHandle"));
			this.meshesToDrive.Add(this.goT.Find("FlightYokeConnectors"));
		}

		// Token: 0x06000BC8 RID: 3016 RVA: 0x000548E8 File Offset: 0x00052CE8
		public new static void Register()
		{
			PredicateRegistry.Add<BlockFlightYoke>("FlightYoke.IncreaseModelGravityInfluence", null, (Block b) => new PredicateActionDelegate(((BlockAbstractAntiGravity)b).IncreaseModelGravityInfluence), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockFlightYoke>("FlightYoke.IncreaseChunkGravityInfluence", null, (Block b) => new PredicateActionDelegate(((BlockAbstractAntiGravity)b).IncreaseChunkGravityInfluence), new Type[]
			{
				typeof(float)
			}, null, null);
			BlockFlightYoke.predicateAlign = PredicateRegistry.Add<BlockFlightYoke>("FlightYoke.AlignInGravityFieldChunk", null, (Block b) => new PredicateActionDelegate(((BlockAbstractAntiGravity)b).AlignInGravityFieldChunk), new Type[]
			{
				typeof(float)
			}, null, null);
			BlockFlightYoke.predicateAlignTerrain = PredicateRegistry.Add<BlockFlightYoke>("FlightYoke.AlignTerrainChunk", null, (Block b) => new PredicateActionDelegate(((BlockAbstractAntiGravity)b).AlignTerrainChunk), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockFlightYoke>("FlightYoke.PositionInGravityFieldChunk", null, (Block b) => new PredicateActionDelegate(((BlockAbstractAntiGravity)b).PositionInGravityFieldYChunk), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockFlightYoke>("FlightYoke.PositionInGravityFieldXChunk", null, (Block b) => new PredicateActionDelegate(((BlockAbstractAntiGravity)b).PositionInGravityFieldXChunk), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockFlightYoke>("FlightYoke.PositionInGravityFieldZChunk", null, (Block b) => new PredicateActionDelegate(((BlockAbstractAntiGravity)b).PositionInGravityFieldZChunk), new Type[]
			{
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockFlightYoke>("FlightYoke.TurnTowardsTagChunk", null, (Block b) => new PredicateActionDelegate(((BlockAbstractAntiGravity)b).TurnTowardsTagChunk), new Type[]
			{
				typeof(string),
				typeof(float)
			}, null, null);
			BlockFlightYoke.predicateIncreaseLocalTorque = PredicateRegistry.Add<BlockFlightYoke>("FlightYoke.IncreaseLocalTorqueChunk", null, (Block b) => new PredicateActionDelegate(((BlockAbstractAntiGravity)b).IncreaseLocalTorqueChunk), new Type[]
			{
				typeof(Vector3),
				typeof(float)
			}, null, null);
			BlockFlightYoke.predicateIncreaseLocalVel = PredicateRegistry.Add<BlockFlightYoke>("FlightYoke.IncreaseLocalVelocityChunk", null, (Block b) => new PredicateActionDelegate(((BlockAbstractAntiGravity)b).IncreaseLocalVelocityChunk), new Type[]
			{
				typeof(Vector3),
				typeof(float)
			}, null, null);
			BlockFlightYoke.predicateAlignAlongDPad = PredicateRegistry.Add<BlockFlightYoke>("FlightYoke.AlignAlongDPadChunk", null, (Block b) => new PredicateActionDelegate(((BlockAbstractAntiGravity)b).AlignAlongDPadChunk), new Type[]
			{
				typeof(string),
				typeof(float)
			}, null, null);
			BlockFlightYoke.predicateBankTurn = PredicateRegistry.Add<BlockFlightYoke>("FlightYoke.BankTurnChunk", null, (Block b) => new PredicateActionDelegate(((BlockAbstractAntiGravity)b).BankTurnChunk), new Type[]
			{
				typeof(string),
				typeof(float)
			}, null, null);
			BlockFlightYoke.predicateFlightSim = PredicateRegistry.Add<BlockFlightYoke>("FlightYoke.FlightSimChunk", null, (Block b) => new PredicateActionDelegate(((BlockAbstractAntiGravity)b).FlightSimChunk), new Type[]
			{
				typeof(string),
				typeof(float)
			}, null, null);
			BlockFlightYoke.predicateTiltFlightSim = PredicateRegistry.Add<BlockFlightYoke>("FlightYoke.TiltFlightSimChunk", null, (Block b) => new PredicateActionDelegate(((BlockAbstractAntiGravity)b).TiltFlightSimChunk), new Type[]
			{
				typeof(string),
				typeof(float)
			}, null, null);
			BlockFlightYoke.predicateHover = PredicateRegistry.Add<BlockFlightYoke>("FlightYoke.HoverInGravityFieldChunk", null, (Block b) => new PredicateActionDelegate(((BlockAbstractAntiGravity)b).HoverInGravityFieldChunk), new Type[]
			{
				typeof(float),
				typeof(float)
			}, new string[]
			{
				"Force",
				"Relative height"
			}, null);
			List<List<Tile>> value = new List<List<Tile>>
			{
				new List<Tile>
				{
					Block.ThenTile(),
					BlockFlightYoke.AlignAlongDPadTile(),
					BlockFlightYoke.HoverTile(),
					BlockFlightYoke.AlignTile()
				},
				new List<Tile>
				{
					Block.ThenTile(),
					BlockFlightYoke.ThrustTile()
				},
				Block.EmptyTileRow()
			};
			Block.defaultExtraTiles["Flight Yoke"] = value;
		}

		// Token: 0x06000BC9 RID: 3017 RVA: 0x00054DB7 File Offset: 0x000531B7
		public static Tile AlignAlongDPadTile()
		{
			return new Tile(BlockFlightYoke.predicateAlignAlongDPad, new object[]
			{
				"L",
				5f
			});
		}

		// Token: 0x06000BCA RID: 3018 RVA: 0x00054DDE File Offset: 0x000531DE
		public static Tile BankTurnTile()
		{
			return new Tile(BlockFlightYoke.predicateBankTurn, new object[]
			{
				"L",
				5f
			});
		}

		// Token: 0x06000BCB RID: 3019 RVA: 0x00054E05 File Offset: 0x00053205
		public static Tile HoverTile()
		{
			return new Tile(BlockFlightYoke.predicateHover, new object[]
			{
				1f,
				5f
			});
		}

		// Token: 0x06000BCC RID: 3020 RVA: 0x00054E31 File Offset: 0x00053231
		public static Tile AlignTile()
		{
			return new Tile(BlockFlightYoke.predicateAlign, new object[]
			{
				1f
			});
		}

		// Token: 0x06000BCD RID: 3021 RVA: 0x00054E50 File Offset: 0x00053250
		public static Tile ThrustTile()
		{
			return new Tile(BlockFlightYoke.predicateIncreaseLocalVel, new object[]
			{
				Vector3.back,
				5f
			});
		}

		// Token: 0x06000BCE RID: 3022 RVA: 0x00054E7C File Offset: 0x0005327C
		public override void Play2()
		{
			base.Play2();
			base.UpdateConnectedCache();
			BlockAccelerations.AddModel(Block.connectedCache[this]);
		}

		// Token: 0x06000BCF RID: 3023 RVA: 0x00054E9C File Offset: 0x0005329C
		public override void Stop(bool resetBlock = true)
		{
			base.Stop(resetBlock);
			this.tiltAngle = 0f;
			this.turnAngle = 0f;
			for (int i = 0; i < this.meshesToDrive.Count; i++)
			{
				this.meshesToDrive[i].localRotation = Quaternion.identity;
			}
		}

		// Token: 0x06000BD0 RID: 3024 RVA: 0x00054EF8 File Offset: 0x000532F8
		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (Blocksworld.CurrentState == State.Play)
			{
				this.tiltAngle = Mathf.Lerp(this.currentDpad.y * 20f, this.tiltAngle, 0.9f);
				this.turnAngle = Mathf.Lerp(this.currentDpad.x * 65f, this.turnAngle, 0.9f);
			}
		}

		// Token: 0x06000BD1 RID: 3025 RVA: 0x00054F64 File Offset: 0x00053364
		public override void Update()
		{
			base.Update();
			if (Blocksworld.CurrentState == State.Play)
			{
				Quaternion localRotation = Quaternion.AngleAxis(this.turnAngle, Vector3.forward) * Quaternion.AngleAxis(this.tiltAngle, Vector3.left);
				for (int i = 0; i < this.meshesToDrive.Count; i++)
				{
					this.meshesToDrive[i].localRotation = localRotation;
				}
			}
		}

		// Token: 0x0400095A RID: 2394
		public static Predicate predicateAlign;

		// Token: 0x0400095B RID: 2395
		public static Predicate predicateAlignTerrain;

		// Token: 0x0400095C RID: 2396
		public static Predicate predicateIncreaseLocalTorque;

		// Token: 0x0400095D RID: 2397
		public static Predicate predicateIncreaseLocalVel;

		// Token: 0x0400095E RID: 2398
		public static Predicate predicateAlignAlongDPad;

		// Token: 0x0400095F RID: 2399
		public static Predicate predicateBankTurn;

		// Token: 0x04000960 RID: 2400
		public static Predicate predicateFlightSim;

		// Token: 0x04000961 RID: 2401
		public static Predicate predicateTiltFlightSim;

		// Token: 0x04000962 RID: 2402
		public static Predicate predicateHover;

		// Token: 0x04000963 RID: 2403
		private List<Transform> meshesToDrive = new List<Transform>();

		// Token: 0x04000964 RID: 2404
		private float tiltAngle;

		// Token: 0x04000965 RID: 2405
		private float turnAngle;
	}
}
