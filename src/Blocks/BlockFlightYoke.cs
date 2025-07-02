using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks;

public class BlockFlightYoke : BlockAbstractAntiGravity
{
	public static Predicate predicateAlign;

	public static Predicate predicateAlignTerrain;

	public static Predicate predicateIncreaseLocalTorque;

	public static Predicate predicateIncreaseLocalVel;

	public static Predicate predicateAlignAlongDPad;

	public static Predicate predicateBankTurn;

	public static Predicate predicateFlightSim;

	public static Predicate predicateTiltFlightSim;

	public static Predicate predicateHover;

	private List<Transform> meshesToDrive = new List<Transform>();

	private float tiltAngle;

	private float turnAngle;

	public BlockFlightYoke(List<List<Tile>> tiles)
		: base(tiles)
	{
		informAboutVaryingGravity = false;
		meshesToDrive.Add(goT.Find("FlightYokeTriggers"));
		meshesToDrive.Add(goT.Find("FlightYokeHandle"));
		meshesToDrive.Add(goT.Find("FlightYokeConnectors"));
	}

	public new static void Register()
	{
		PredicateRegistry.Add<BlockFlightYoke>("FlightYoke.IncreaseModelGravityInfluence", null, (Block b) => ((BlockAbstractAntiGravity)b).IncreaseModelGravityInfluence, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockFlightYoke>("FlightYoke.IncreaseChunkGravityInfluence", null, (Block b) => ((BlockAbstractAntiGravity)b).IncreaseChunkGravityInfluence, new Type[1] { typeof(float) });
		predicateAlign = PredicateRegistry.Add<BlockFlightYoke>("FlightYoke.AlignInGravityFieldChunk", null, (Block b) => ((BlockAbstractAntiGravity)b).AlignInGravityFieldChunk, new Type[1] { typeof(float) });
		predicateAlignTerrain = PredicateRegistry.Add<BlockFlightYoke>("FlightYoke.AlignTerrainChunk", null, (Block b) => ((BlockAbstractAntiGravity)b).AlignTerrainChunk, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockFlightYoke>("FlightYoke.PositionInGravityFieldChunk", null, (Block b) => ((BlockAbstractAntiGravity)b).PositionInGravityFieldYChunk, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockFlightYoke>("FlightYoke.PositionInGravityFieldXChunk", null, (Block b) => ((BlockAbstractAntiGravity)b).PositionInGravityFieldXChunk, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockFlightYoke>("FlightYoke.PositionInGravityFieldZChunk", null, (Block b) => ((BlockAbstractAntiGravity)b).PositionInGravityFieldZChunk, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockFlightYoke>("FlightYoke.TurnTowardsTagChunk", null, (Block b) => ((BlockAbstractAntiGravity)b).TurnTowardsTagChunk, new Type[2]
		{
			typeof(string),
			typeof(float)
		});
		predicateIncreaseLocalTorque = PredicateRegistry.Add<BlockFlightYoke>("FlightYoke.IncreaseLocalTorqueChunk", null, (Block b) => ((BlockAbstractAntiGravity)b).IncreaseLocalTorqueChunk, new Type[2]
		{
			typeof(Vector3),
			typeof(float)
		});
		predicateIncreaseLocalVel = PredicateRegistry.Add<BlockFlightYoke>("FlightYoke.IncreaseLocalVelocityChunk", null, (Block b) => ((BlockAbstractAntiGravity)b).IncreaseLocalVelocityChunk, new Type[2]
		{
			typeof(Vector3),
			typeof(float)
		});
		predicateAlignAlongDPad = PredicateRegistry.Add<BlockFlightYoke>("FlightYoke.AlignAlongDPadChunk", null, (Block b) => ((BlockAbstractAntiGravity)b).AlignAlongDPadChunk, new Type[2]
		{
			typeof(string),
			typeof(float)
		});
		predicateBankTurn = PredicateRegistry.Add<BlockFlightYoke>("FlightYoke.BankTurnChunk", null, (Block b) => ((BlockAbstractAntiGravity)b).BankTurnChunk, new Type[2]
		{
			typeof(string),
			typeof(float)
		});
		predicateFlightSim = PredicateRegistry.Add<BlockFlightYoke>("FlightYoke.FlightSimChunk", null, (Block b) => ((BlockAbstractAntiGravity)b).FlightSimChunk, new Type[2]
		{
			typeof(string),
			typeof(float)
		});
		predicateTiltFlightSim = PredicateRegistry.Add<BlockFlightYoke>("FlightYoke.TiltFlightSimChunk", null, (Block b) => ((BlockAbstractAntiGravity)b).TiltFlightSimChunk, new Type[2]
		{
			typeof(string),
			typeof(float)
		});
		predicateHover = PredicateRegistry.Add<BlockFlightYoke>("FlightYoke.HoverInGravityFieldChunk", null, (Block b) => ((BlockAbstractAntiGravity)b).HoverInGravityFieldChunk, new Type[2]
		{
			typeof(float),
			typeof(float)
		}, new string[2] { "Force", "Relative height" });
		List<List<Tile>> value = new List<List<Tile>>
		{
			new List<Tile>
			{
				Block.ThenTile(),
				AlignAlongDPadTile(),
				HoverTile(),
				AlignTile()
			},
			new List<Tile>
			{
				Block.ThenTile(),
				ThrustTile()
			},
			Block.EmptyTileRow()
		};
		Block.defaultExtraTiles["Flight Yoke"] = value;
	}

	public static Tile AlignAlongDPadTile()
	{
		return new Tile(predicateAlignAlongDPad, "L", 5f);
	}

	public static Tile BankTurnTile()
	{
		return new Tile(predicateBankTurn, "L", 5f);
	}

	public static Tile HoverTile()
	{
		return new Tile(predicateHover, 1f, 5f);
	}

	public static Tile AlignTile()
	{
		return new Tile(predicateAlign, 1f);
	}

	public static Tile ThrustTile()
	{
		return new Tile(predicateIncreaseLocalVel, Vector3.back, 5f);
	}

	public override void Play2()
	{
		base.Play2();
		UpdateConnectedCache();
		BlockAccelerations.AddModel(Block.connectedCache[this]);
	}

	public override void Stop(bool resetBlock = true)
	{
		base.Stop(resetBlock);
		tiltAngle = 0f;
		turnAngle = 0f;
		for (int i = 0; i < meshesToDrive.Count; i++)
		{
			meshesToDrive[i].localRotation = Quaternion.identity;
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (Blocksworld.CurrentState == State.Play)
		{
			tiltAngle = Mathf.Lerp(currentDpad.y * 20f, tiltAngle, 0.9f);
			turnAngle = Mathf.Lerp(currentDpad.x * 65f, turnAngle, 0.9f);
		}
	}

	public override void Update()
	{
		base.Update();
		if (Blocksworld.CurrentState == State.Play)
		{
			Quaternion localRotation = Quaternion.AngleAxis(turnAngle, Vector3.forward) * Quaternion.AngleAxis(tiltAngle, Vector3.left);
			for (int i = 0; i < meshesToDrive.Count; i++)
			{
				meshesToDrive[i].localRotation = localRotation;
			}
		}
	}
}
