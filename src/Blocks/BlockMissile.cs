using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks;

public class BlockMissile : Block, IMissileLauncher
{
	private enum MissileState
	{
		LOADED,
		LAUNCHED,
		EXPLODED,
		EXPIRED
	}

	public static Predicate predicateMissileLabel;

	public static Predicate predicateMissileType;

	public static Predicate predicateMissileLaunch;

	private int sfxLoopUpdateCounter;

	private const int SFX_LOOP_UPDATE_INTERVAL = 5;

	private float playBurstLevel;

	public static int nextMissileLabel;

	public const int MISSILE_LABEL_COUNT = 6;

	private MissileState missileState;

	private int endWaitCounter;

	private static List<BlockMissileControl> globalMissileControllers;

	public HashSet<int> labels = new HashSet<int>();

	public int[] labelSetCounters = new int[7];

	public string localTargetTag;

	public string controllerTargetTag;

	public float localLockDelay = 0.5f;

	public float controllerLockDelay = 0.5f;

	private IMissile launchedMissile;

	internal Chunk playChunk;

	public float gravityFraction = 1f;

	private float initLifetime = 10f;

	public float burstTime = 10f;

	public bool burstTimeSet;

	public float globalBurstTime = 10f;

	public bool globalBurstTimeSet;

	public float innateExplodeRadius = 3f;

	public bool innateExplodeRadiusSet;

	public int missileType = -1;

	public float smokeOffset = 1f;

	public float smokeSize = 1f;

	private static GameObject smokeGo;

	private static ParticleSystem particles;

	private Color smokeColor = Color.white;

	private bool setSmokeColor = true;

	protected int smokeColorMeshIndex;

	private bool emitSmoke = true;

	internal List<Block> reloadConnections = new List<Block>();

	public Vector3 smokeExitOffset = Vector3.zero;

	public BlockMissile(List<List<Tile>> tiles)
		: base(tiles)
	{
		if (smokeGo == null)
		{
			smokeGo = UnityEngine.Object.Instantiate(Resources.Load("Blocks/Missile Exhaust")) as GameObject;
			particles = smokeGo.GetComponent<ParticleSystem>();
			particles.enableEmission = false;
		}
		sfxLoopUpdateCounter = UnityEngine.Random.Range(0, 5);
		loopName = "Rocket Burst 3 Loop";
		MissileMetaData component = go.GetComponent<MissileMetaData>();
		if (component != null)
		{
			smokeExitOffset = component.exitOffset;
			loopName = component.loopSfx;
		}
		else
		{
			BWLog.Info("Could not find missile meta data component on " + BlockType());
		}
	}

	public override void Pause()
	{
		particles.Pause();
		PlayLoopSound(play: false, GetLoopClip());
	}

	public override void Resume()
	{
		if (particles.isPaused)
		{
			particles.Play();
		}
	}

	public static void WorldLoaded()
	{
		nextMissileLabel = 0;
	}

	public new static void Register()
	{
		predicateMissileLaunch = PredicateRegistry.Add<BlockMissile>("Missile.Launch", (Block b) => ((BlockMissile)b).MissileIsLaunched, (Block b) => ((BlockMissile)b).Launch, new Type[1] { typeof(float) });
		predicateMissileType = PredicateRegistry.Add<BlockMissile>("Missile.SetMissileType", null, (Block b) => ((BlockMissile)b).SetMissileType, new Type[2]
		{
			typeof(int),
			typeof(float)
		}, new string[2] { "Type", "Yield" });
		PredicateRegistry.Add<BlockMissile>("Missile.TargetTag", null, (Block b) => ((BlockMissile)b).TargetTag, new Type[2]
		{
			typeof(string),
			typeof(float)
		}, new string[2] { "Tag name", "Lock delay" });
		predicateMissileLabel = PredicateRegistry.Add<BlockMissile>("Missile.Label", null, (Block b) => ((BlockMissile)b).Label, new Type[1] { typeof(int) });
		PredicateRegistry.Add<BlockMissile>("Missile.Ballistic", null, (Block b) => ((BlockMissile)b).Ballistic, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockMissile>("Missile.InitLifetime", (Block b) => ((BlockMissile)b).MissileIsExpired, (Block b) => ((BlockMissile)b).InitLifetime, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockMissile>("Missile.InFlightTimeGreaterThan", (Block b) => ((BlockMissile)b).InFlightTimeGreaterThan, null, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockMissile>("Missile.InFlightTimeLessThan", (Block b) => ((BlockMissile)b).InFlightTimeLessThan, null, new Type[1] { typeof(float) });
		Predicate predicate = PredicateRegistry.Add<BlockMissile>("Missile.SetBurstTime", null, (Block b) => ((BlockMissile)b).SetBurstTime, new Type[1] { typeof(float) });
		predicate.updatesIconOnArgumentChange = true;
		PredicateRegistry.Add<BlockMissile>("Missile.Reload", (Block b) => ((BlockMissile)b).MissileIsLoaded, (Block b) => ((BlockMissile)b).Reload);
		Dictionary<string, int> dictionary = new Dictionary<string, int>
		{
			{ "Missile A", 0 },
			{ "Missile B", 0 },
			{ "Missile C", 0 }
		};
		foreach (KeyValuePair<string, int> item in dictionary)
		{
			List<List<Tile>> value = new List<List<Tile>>
			{
				new List<Tile>
				{
					Block.ThenTile(),
					MissileTypeTile(item.Value),
					MissileLabelTile(1)
				},
				new List<Tile>
				{
					Block.ButtonTile("R"),
					Block.ThenTile(),
					MissileLaunchTile()
				},
				Block.EmptyTileRow()
			};
			Block.defaultExtraTiles[item.Key] = value;
			Block.defaultExtraTilesProcessors[item.Key] = delegate(List<List<Tile>> tiles)
			{
				foreach (List<Tile> tile in tiles)
				{
					foreach (Tile item2 in tile)
					{
						GAF gaf = item2.gaf;
						if (gaf.Predicate == predicateMissileLabel)
						{
							gaf.Args[0] = nextMissileLabel + 1;
							nextMissileLabel = (nextMissileLabel + 1) % 6;
						}
					}
				}
			};
		}
	}

	public static Tile MissileTypeTile(int type)
	{
		return new Tile(predicateMissileType, type, 3f);
	}

	public static Tile MissileLabelTile(int label)
	{
		return new Tile(predicateMissileLabel, label);
	}

	public static Tile MissileLaunchTile()
	{
		return new Tile(predicateMissileLaunch, 1f);
	}

	public override void Play()
	{
		base.Play();
		playChunk = null;
		if (particles.isPaused)
		{
			particles.Play();
		}
		if (globalMissileControllers == null)
		{
			globalMissileControllers = new List<BlockMissileControl>();
			foreach (Block item2 in BWSceneManager.AllBlocks())
			{
				if (item2 is BlockMissileControl item)
				{
					globalMissileControllers.Add(item);
				}
			}
		}
		foreach (BlockMissileControl globalMissileController in globalMissileControllers)
		{
			globalMissileController.RegisterMissileLauncher(this);
		}
		UpdateConnectedCache();
		List<Block> list = Block.connectedCache[this];
		foreach (Block item3 in list)
		{
			if (item3 is BlockModelMissileControl blockModelMissileControl)
			{
				blockModelMissileControl.RegisterMissileLauncher(this);
			}
		}
		endWaitCounter = 0;
		initLifetime = 10f;
		burstTime = 10f;
		burstTimeSet = false;
		globalBurstTime = 10f;
		globalBurstTimeSet = false;
		gravityFraction = 1f;
		Vector3 vector = Scale();
		smokeOffset = vector.y * 0.5f;
		smokeSize = Mathf.Min(vector.x, vector.z);
		missileState = MissileState.LOADED;
		ResetFrame();
		labels.Clear();
		labelSetCounters = new int[7];
		if (setSmokeColor)
		{
			UpdateSmokeColorPaint(GetPaint(smokeColorMeshIndex), smokeColorMeshIndex);
			UpdateSmokeColorTexture(GetTexture(smokeColorMeshIndex), smokeColorMeshIndex);
		}
		reloadConnections = new List<Block>();
		for (int i = 0; i < connections.Count; i++)
		{
			Block block = connections[i];
			if (!(block is BlockMissile))
			{
				reloadConnections.Add(block);
			}
		}
	}

	public override bool CanTriggerBlockListSensor()
	{
		return missileState == MissileState.LOADED;
	}

	public override void Stop(bool resetBlock = true)
	{
		globalMissileControllers = null;
		DestroyLaunchedMissile();
		Util.UnparentTransformSafely(goT);
		go.SetActive(value: true);
		if (goShadow != null)
		{
			goShadow.SetActive(value: true);
		}
		missileState = MissileState.LOADED;
		ResetFrame();
		labelSetCounters = new int[7];
		labels.Clear();
		reloadConnections.Clear();
		particles.Clear();
		base.Stop(resetBlock);
	}

	public override void Destroy()
	{
		DestroyLaunchedMissile();
		base.Destroy();
	}

	public override TileResultCode ExplodeLocal(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (broken)
		{
			return TileResultCode.True;
		}
		if (missileState == MissileState.LAUNCHED && launchedMissile != null)
		{
			float floatArg = Util.GetFloatArg(args, 0, 3f);
			ExplodeMissile(floatArg);
		}
		else if (missileState == MissileState.LOADED)
		{
			base.ExplodeLocal(eInfo, args);
		}
		return TileResultCode.True;
	}

	private void DestroyLaunchedMissile()
	{
		if (launchedMissile != null)
		{
			launchedMissile.Destroy();
			launchedMissile = null;
		}
	}

	private void DeactivateLaunchedMissile()
	{
		if (launchedMissile != null)
		{
			launchedMissile.Deactivate();
		}
	}

	public override bool BreakByDetachExplosion()
	{
		return missileState == MissileState.LOADED;
	}

	private bool StepEndWaitCounter()
	{
		if (endWaitCounter > 40)
		{
			DestroyLaunchedMissile();
			endWaitCounter = 0;
			return true;
		}
		if (endWaitCounter == 0)
		{
			DeactivateLaunchedMissile();
		}
		endWaitCounter++;
		return false;
	}

	public override TileResultCode Explode(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (missileState == MissileState.LAUNCHED && launchedMissile != null)
		{
			launchedMissile.Break();
			return TileResultCode.True;
		}
		return base.Explode(eInfo, args);
	}

	public void RemoveMissile()
	{
		if (launchedMissile != null)
		{
			launchedMissile.SetExpired();
		}
		else
		{
			BWSceneManager.RemovePlayBlock(this);
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (launchedMissile != null)
		{
			launchedMissile.FixedUpdate();
			if (missileType != -1 && missileState == MissileState.LAUNCHED && !launchedMissile.IsBroken() && CollisionManager.IsImpactingBlock(this))
			{
				float explodeRadius = ((!innateExplodeRadiusSet) ? size.magnitude : innateExplodeRadius);
				ExplodeMissile(explodeRadius);
			}
			if (launchedMissile.HasExploded())
			{
				StepEndWaitCounter();
				missileState = MissileState.EXPLODED;
			}
			else if (launchedMissile.HasExpired())
			{
				StepEndWaitCounter();
				missileState = MissileState.EXPIRED;
			}
		}
		if (Sound.sfxEnabled && !vanished && go.activeInHierarchy)
		{
			float num = 1f;
			float f = Mathf.Min(size.x, size.z);
			float num2 = Mathf.Sqrt(f);
			bool flag = missileState == MissileState.LAUNCHED && launchedMissile.IsBursting();
			float num3 = ((!flag) ? (-0.04f) : 0.04f);
			playBurstLevel = Mathf.Clamp(playBurstLevel + num3, 0f, Mathf.Clamp(0.5f * num, 0.1f, 1f));
			float value = (0.98f + Mathf.Min(0.1f, 0.02f * num)) / (0.5f + 0.5f * num2);
			value = Mathf.Clamp(value, 0.25f, 1.25f);
			if (sfxLoopUpdateCounter % 5 == 0)
			{
				PlayLoopSound(flag || playBurstLevel > 0.01f, GetLoopClip(), playBurstLevel, null, value);
				UpdateWithinWaterLPFilter();
			}
			sfxLoopUpdateCounter++;
		}
		else
		{
			PlayLoopSound(play: false, GetLoopClip());
		}
	}

	public bool MissileGone()
	{
		if (missileState != MissileState.EXPIRED)
		{
			return missileState == MissileState.EXPLODED;
		}
		return true;
	}

	public bool IsLoaded()
	{
		return missileState == MissileState.LOADED;
	}

	public bool CanLaunch()
	{
		if (missileState == MissileState.LOADED)
		{
			return !isTreasure;
		}
		return false;
	}

	private void UpdateSmokeColorTexture(string texture, int meshIndex)
	{
		if (setSmokeColor && meshIndex == smokeColorMeshIndex && subMeshGameObjects.Count > smokeColorMeshIndex)
		{
			emitSmoke = texture != "Glass";
		}
	}

	private void UpdateSmokeColorPaint(string paint, int meshIndex)
	{
		if (setSmokeColor && meshIndex == smokeColorMeshIndex && subMeshGameObjects.Count > smokeColorMeshIndex)
		{
			Renderer renderer = ((meshIndex != 0) ? subMeshGameObjects[meshIndex - 1].GetComponent<Renderer>() : go.GetComponent<Renderer>());
			smokeColor = BlockAbstractRocket.GetSmokeColor(paint, renderer);
		}
	}

	private void SetInnateExplodeRadius(float r)
	{
		innateExplodeRadius = r;
		innateExplodeRadiusSet = true;
	}

	public override void Exploded()
	{
		base.Exploded();
		if (missileState == MissileState.LAUNCHED || missileState == MissileState.LOADED)
		{
			float explodeRadius = ((!innateExplodeRadiusSet) ? size.magnitude : innateExplodeRadius);
			ExplodeMissile(explodeRadius, force: true);
		}
	}

	public override bool TreatAsVehicleLikeBlock()
	{
		return false;
	}

	public void LaunchMissile(float burstMultiplier)
	{
		Chunk chunk = base.chunk;
		PlayPositionedSound("missile_launcher_fire_single");
		Quaternion oldLocalRotation = goT.rotation;
		Vector3 oldLocalPosition = goT.position;
		if (reloadConnections.Count > 0)
		{
			oldLocalRotation = Quaternion.Inverse(reloadConnections[0].goT.rotation) * goT.rotation;
			oldLocalPosition = reloadConnections[0].goT.InverseTransformPoint(goT.position);
		}
		chunk.RemoveBlock(this);
		Blocksworld.blocksworldCamera.ChunkDirty(chunk);
		playChunk = chunk;
		Vector3 velocity = Vector3.zero;
		Vector3 angularVelocity = Vector3.zero;
		bool oldRbWasKinematic = false;
		Rigidbody rb = chunk.rb;
		if (rb != null)
		{
			if (!rb.isKinematic)
			{
				angularVelocity = rb.angularVelocity;
				Vector3 lhs = goT.position - rb.worldCenterOfMass;
				velocity = rb.velocity + Vector3.Cross(lhs, rb.angularVelocity);
			}
			oldRbWasKinematic = rb.isKinematic;
			if (chunk.blocks.Count == 0)
			{
				rb.isKinematic = ConnectionsOfType(2).Count == 0;
			}
		}
		Chunk chunk2 = new Chunk(new List<Block> { this }, forceRigidbody: true);
		Blocksworld.chunks.Add(chunk2);
		Rigidbody rb2 = chunk2.rb;
		rb2.velocity = velocity;
		rb2.angularVelocity = angularVelocity;
		Blocksworld.blocksworldCamera.SetSingleton(this, s: true);
		MissileBody missileBody = new MissileBody
		{
			block = this,
			chunk = chunk2,
			oldLocalRotation = oldLocalRotation,
			oldLocalPosition = oldLocalPosition,
			oldRbWasKinematic = oldRbWasKinematic,
			lifetime = initLifetime,
			burstMultiplier = burstMultiplier
		};
		launchedMissile = missileBody;
		missileState = MissileState.LAUNCHED;
	}

	public bool CanReload()
	{
		bool flag = launchedMissile == null && (missileState == MissileState.EXPLODED || missileState == MissileState.EXPIRED);
		if (flag)
		{
			foreach (Block reloadConnection in reloadConnections)
			{
				if (reloadConnection.broken)
				{
					flag = false;
					break;
				}
			}
		}
		return flag;
	}

	public void Reload()
	{
		bool active = true;
		go.SetActive(active);
		if (goShadow != null)
		{
			goShadow.SetActive(active);
		}
		didFix = false;
		missileState = MissileState.LOADED;
		Appeared();
	}

	public bool HasLabel(int label)
	{
		return labels.Contains(label);
	}

	public IMissile GetLaunchedMissile()
	{
		return launchedMissile;
	}

	public void AddControllerTargetTag(string t, float lockDelay)
	{
		controllerTargetTag = t;
		controllerLockDelay = lockDelay;
	}

	public void SetGlobalBurstTime(float bt)
	{
		globalBurstTimeSet = true;
		globalBurstTime = bt;
	}

	public override void ResetFrame()
	{
		localLockDelay = 0.5f;
		controllerLockDelay = 0.5f;
		localTargetTag = null;
		controllerTargetTag = null;
		globalBurstTimeSet = false;
		innateExplodeRadiusSet = false;
		burstTimeSet = false;
		missileType = -1;
		for (int i = 0; i < labelSetCounters.Length; i++)
		{
			int num = labelSetCounters[i];
			num--;
			if (num == 0)
			{
				labels.Remove(i);
			}
			else
			{
				labelSetCounters[i] = num;
			}
		}
	}

	public void AddLocalExplosion(Vector3 pos, Vector3 vel, float radius)
	{
		RadialExplosionCommand c = new RadialExplosionCommand(5f, pos, vel, radius * 0.5f, radius, radius * 2f, new HashSet<Block> { this }, string.Empty);
		Blocksworld.AddFixedUpdateCommand(c);
		Sound.PlayPositionedOneShot("missile_detonation", pos, 5f, Mathf.Max(120f, radius * 30f));
	}

	public void EmitSmoke(Vector3 pos, Vector3 velocity)
	{
		if (emitSmoke)
		{
			Transform transform = goT;
			Vector3 vector = transform.TransformDirection(smokeExitOffset);
			ParticleSystem.Particle particle = new ParticleSystem.Particle
			{
				position = pos + vector,
				velocity = velocity,
				size = smokeSize,
				remainingLifetime = 1f,
				rotation = UnityEngine.Random.Range(-180, 180),
				startLifetime = 1f,
				color = smokeColor,
				randomSeed = (uint)UnityEngine.Random.Range(17, 9999999)
			};
			particles.Emit(particle);
		}
	}

	private void ExplodeMissile(float explodeRadius, bool force = false)
	{
		if (launchedMissile != null && launchedMissile.CanExplode())
		{
			launchedMissile.Explode(explodeRadius);
			missileState = MissileState.EXPLODED;
		}
		if (missileState == MissileState.LOADED && force)
		{
			go.SetActive(value: false);
			if (goShadow != null)
			{
				goShadow.SetActive(value: false);
			}
			Vector3 vel = Vector3.zero;
			Rigidbody rb = chunk.rb;
			if (rb != null && !rb.isKinematic)
			{
				vel = rb.velocity;
			}
			AddLocalExplosion(goT.position, vel, explodeRadius);
			missileState = MissileState.EXPLODED;
		}
	}

	public TileResultCode MissileIsExploded(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (missileState == MissileState.EXPLODED)
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public TileResultCode Reload(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (CanReload())
		{
			Reload();
		}
		return TileResultCode.True;
	}

	public TileResultCode InFlightTimeGreaterThan(ScriptRowExecutionInfo eInfo, object[] args)
	{
		float floatArg = Util.GetFloatArg(args, 0, 1f);
		if (launchedMissile != null && missileState == MissileState.LAUNCHED && !(launchedMissile.GetInFlightTime() <= floatArg))
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public TileResultCode InFlightTimeLessThan(ScriptRowExecutionInfo eInfo, object[] args)
	{
		float floatArg = Util.GetFloatArg(args, 0, 1f);
		if (launchedMissile != null && missileState == MissileState.LAUNCHED && !(launchedMissile.GetInFlightTime() >= floatArg))
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public TileResultCode InitLifetime(ScriptRowExecutionInfo eInfo, object[] args)
	{
		initLifetime = Util.GetFloatArg(args, 0, 10f);
		if (launchedMissile != null && missileState == MissileState.LAUNCHED)
		{
			launchedMissile.SetLifetime(initLifetime);
		}
		return TileResultCode.True;
	}

	public TileResultCode SetBurstTime(ScriptRowExecutionInfo eInfo, object[] args)
	{
		burstTimeSet = true;
		burstTime = Util.GetFloatArg(args, 0, 10f);
		return TileResultCode.True;
	}

	public TileResultCode Ballistic(ScriptRowExecutionInfo eInfo, object[] args)
	{
		gravityFraction = Util.GetFloatArg(args, 0, 1f);
		return TileResultCode.True;
	}

	public TileResultCode Launch(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (CanLaunch())
		{
			float floatArg = Util.GetFloatArg(args, 0, 1f);
			LaunchMissile(floatArg);
		}
		return TileResultCode.True;
	}

	public TileResultCode MissileIsLaunched(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (missileState == MissileState.LAUNCHED)
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public TileResultCode MissileIsExpired(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (missileState == MissileState.EXPIRED)
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public TileResultCode MissileIsExploaded(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (missileState == MissileState.EXPLODED)
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public TileResultCode MissileIsLoaded(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (missileState == MissileState.LOADED)
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public TileResultCode SetMissileType(ScriptRowExecutionInfo eInfo, object[] args)
	{
		missileType = Util.GetIntArg(args, 0, -1);
		innateExplodeRadius = Util.GetFloatArg(args, 1, 3f);
		innateExplodeRadiusSet = true;
		switch (missileType)
		{
		case 2:
			burstTimeSet = true;
			burstTime = 0f;
			gravityFraction = 1f;
			break;
		case 1:
			burstTimeSet = true;
			burstTime = 1f;
			gravityFraction = 1f;
			break;
		case 0:
			gravityFraction = 0f;
			break;
		}
		return TileResultCode.True;
	}

	public static string GetMissileTypeIconName(int type)
	{
		return type switch
		{
			0 => "Missile Type", 
			1 => "Ballistic Type", 
			2 => "Bomb Type", 
			_ => "Unknown Type", 
		};
	}

	public TileResultCode TargetTag(ScriptRowExecutionInfo eInfo, object[] args)
	{
		localTargetTag = Util.GetStringArg(args, 0, string.Empty);
		localLockDelay = Util.GetFloatArg(args, 1, 0.5f);
		return TileResultCode.True;
	}

	public TileResultCode Label(ScriptRowExecutionInfo eInfo, object[] args)
	{
		int intArg = Util.GetIntArg(args, 0, 0);
		labels.Add(intArg);
		labelSetCounters[intArg] = 2;
		return TileResultCode.True;
	}

	public override TileResultCode TextureTo(string texture, Vector3 normal, bool permanent, int meshIndex = 0, bool force = false)
	{
		TileResultCode result = base.TextureTo(texture, normal, permanent, meshIndex, force);
		UpdateSmokeColorTexture(texture, meshIndex);
		return result;
	}

	public override TileResultCode PaintTo(string paint, bool permanent, int meshIndex = 0)
	{
		TileResultCode result = base.PaintTo(paint, permanent, meshIndex);
		UpdateSmokeColorPaint(paint, meshIndex);
		return result;
	}

	protected override bool IgnoreChangesToDefaultForPredicate(Predicate predicate)
	{
		return predicate == predicateMissileLabel;
	}
}
