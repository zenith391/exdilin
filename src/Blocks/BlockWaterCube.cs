using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks;

public class BlockWaterCube : BlockAbstractWater
{
	public HashSet<int> blocksWithinWater = new HashSet<int>();

	public Dictionary<string, HashSet<int>> blocksWithinTaggedWater = new Dictionary<string, HashSet<int>>();

	protected Mesh waterMesh;

	protected List<int> waterSurfaceMeshVertices = new List<int>();

	protected List<Vector3> origWaterSurfaceMeshVertices = new List<Vector3>();

	protected Vector3 origScale = Vector3.one;

	protected Vector3 origPos = Vector3.zero;

	protected float origWaterLevel = 1f;

	protected const int MAX_SPLASH_SOUNDS = 5;

	protected Vector3 colliderSize = Vector3.one;

	protected Vector3 colliderLocalOffset = Vector3.zero;

	private static bool wakeUpWaterSensorBlocks;

	private Bounds waterBounds;

	private const float OFF_THRESHOLD = 0.05f;

	private List<GameObject> audioSourceObjects = new List<GameObject>();

	public BlockWaterCube(List<List<Tile>> tiles)
		: base(tiles)
	{
	}

	public new static void Register()
	{
		PredicateRegistry.Add<BlockWaterCube>("WaterBlock.IncreaseWaterLevel", null, (Block b) => ((BlockWaterCube)b).IncreaseWaterLevelOffset, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockWaterCube>("WaterBlock.StepIncreaseWaterLevel", null, (Block b) => ((BlockWaterCube)b).StepIncreaseWaterLevel, new Type[2]
		{
			typeof(float),
			typeof(float)
		}, new string[2] { "Units", "Duration" });
		PredicateRegistry.Add<BlockWaterCube>("WaterBlock.SetLiquidProperties", null, (Block b) => ((BlockAbstractWater)b).SetLiquidProperties, new Type[1] { typeof(string) });
		PredicateRegistry.Add<BlockWaterCube>("WaterBlock.SetMaxPositiveWaterLevelOffset", (Block b) => ((BlockAbstractWater)b).AtMaxPositiveWaterLevelOffset, (Block b) => ((BlockWaterCube)b).SetMaxPositiveWaterLevelOffset, new Type[1] { typeof(float) });
		PredicateRegistry.Add<BlockWaterCube>("WaterBlock.SetMaxNegativeWaterLevelOffset", (Block b) => ((BlockAbstractWater)b).AtMaxNegativeWaterLevelOffset, (Block b) => ((BlockWaterCube)b).SetMaxNegativeWaterLevelOffset, new Type[1] { typeof(float) });
		List<List<Tile>> list = new List<List<Tile>>();
		list.Add(new List<Tile>
		{
			Block.ThenTile(),
			new Tile(new GAF("WaterBlock.SetLiquidProperties", "Water"))
		});
		list.Add(Block.EmptyTileRow());
		List<List<Tile>> value = list;
		Block.defaultExtraTiles["Water Cube"] = value;
	}

	public override void Play()
	{
		base.Play();
		go.layer = 13;
		go.GetComponent<Collider>().isTrigger = true;
		blocksWithinWater.Clear();
		FindWaterSurfaceMeshVertices();
		origScale = Scale();
		origPos = GetPosition();
		origWaterLevel = Mathf.Abs(goT.TransformDirection(origScale).y);
		colliderSize = size;
		colliderLocalOffset = Vector3.zero;
	}

	public override TileResultCode TextureTo(string texture, Vector3 normal, bool permanent, int meshIndex = 0, bool force = false)
	{
		if (texture != "Water_Block")
		{
			return TileResultCode.False;
		}
		return base.TextureTo(texture, normal, permanent, meshIndex, force);
	}

	public TileResultCode IncreaseWaterLevelOffset(ScriptRowExecutionInfo eInfo, object[] args)
	{
		float incPerSecond = eInfo.floatArg * Util.GetFloatArg(args, 0, 0f);
		IncreaseWaterLevelOffset(incPerSecond);
		return TileResultCode.True;
	}

	public TileResultCode StepIncreaseWaterLevel(ScriptRowExecutionInfo eInfo, object[] args)
	{
		float num = eInfo.floatArg * Util.GetFloatArg(args, 1, 1f);
		if (num < 0.01f)
		{
			num = 1f;
		}
		float incPerSecond = eInfo.floatArg * Util.GetFloatArg(args, 0, 0f) / num;
		IncreaseWaterLevelOffset(incPerSecond);
		if (!(eInfo.timer < num))
		{
			return TileResultCode.True;
		}
		return TileResultCode.Delayed;
	}

	protected void IncreaseWaterLevelOffset(float incPerSecond)
	{
		float num = origWaterLevel - 0.044999998f;
		float num2 = Mathf.Max(waterLevelOffset + incPerSecond * Blocksworld.fixedDeltaTime, 0f - num);
		wakeUpWaterSensorBlocks = true;
		SetWaterLevelOffset(num2);
	}

	protected override void SetWaterLevelOffset(float amount)
	{
		float num = origWaterLevel + waterLevelOffset;
		base.SetWaterLevelOffset(amount);
		float num2 = origWaterLevel + waterLevelOffset;
		if (num2 < 0.05f && num > 0.05f)
		{
			go.GetComponent<Renderer>().enabled = false;
		}
		else if (num < 0.05f && num2 > 0.05f)
		{
			go.GetComponent<Renderer>().enabled = true;
		}
		if (!(waterMesh == null))
		{
			Vector3[] vertices = waterMesh.vertices;
			Vector3 vector = goT.InverseTransformDirection(Vector3.up) * waterLevelOffset;
			for (int i = 0; i < waterSurfaceMeshVertices.Count; i++)
			{
				int num3 = waterSurfaceMeshVertices[i];
				Vector3 vector2 = origWaterSurfaceMeshVertices[i];
				vector2 += vector;
				vertices[num3] = vector2;
			}
			waterMesh.vertices = vertices;
			BoxCollider component = go.GetComponent<BoxCollider>();
			component.size = origScale + Util.Abs(vector) * Mathf.Sign(amount);
			component.center = vector * 0.5f;
			colliderLocalOffset = Vector3.up * waterLevelOffset * 0.5f;
			colliderSize = component.size;
		}
	}

	public override Vector3 GetEffectLocalOffset()
	{
		return colliderLocalOffset;
	}

	public override Vector3 GetEffectSize()
	{
		return colliderSize;
	}

	private void FindWaterSurfaceMeshVertices()
	{
		origWaterSurfaceMeshVertices.Clear();
		waterSurfaceMeshVertices.Clear();
		MeshFilter component = go.GetComponent<MeshFilter>();
		waterMesh = component.sharedMesh;
		Transform transform = goT;
		Vector3 position = transform.position;
		Vector3[] vertices = waterMesh.vertices;
		for (int i = 0; i < vertices.Length; i++)
		{
			Vector3 vector = vertices[i];
			Vector3 vector2 = transform.TransformPoint(vector);
			if ((vector2 - position).y > 0f)
			{
				waterSurfaceMeshVertices.Add(i);
				origWaterSurfaceMeshVertices.Add(vector);
			}
		}
	}

	public override void Stop(bool resetBlock = true)
	{
		SetWaterLevelOffset(0f);
		base.Stop(resetBlock);
		go.layer = 4;
		go.GetComponent<Collider>().isTrigger = false;
		blocksWithinWater.Clear();
	}

	public override void Update()
	{
		base.Update();
		Vector3 cameraPosition = Blocksworld.cameraPosition;
		bool flag = go.GetComponent<Collider>().bounds.Contains(cameraPosition);
		UpdateSounds(flag);
		if (flag != cameraWasWithinWater)
		{
			UpdateUnderwaterLightColors(flag);
		}
		cameraWasWithinWater = flag;
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		HashSet<int> hashSet = new HashSet<int>(blocksWithinWater);
		blocksWithinWater.Clear();
		blocksWithinTaggedWater.Clear();
		waterBounds = go.GetComponent<Collider>().bounds;
		if (CollisionManager.triggering.TryGetValue(colliderName, out var value))
		{
			float magnitude = Physics.gravity.magnitude;
			float num = Blocksworld.fixedDeltaTime / 0.02f;
			foreach (GameObject item in value)
			{
				if (item == null)
				{
					continue;
				}
				Transform transform = item.transform;
				Rigidbody component = transform.GetComponent<Rigidbody>();
				if (component == null)
				{
					continue;
				}
				bool isKinematic = component.isKinematic;
				foreach (object item2 in transform)
				{
					Transform transform2 = (Transform)item2;
					GameObject gameObject = transform2.gameObject;
					Block block = BWSceneManager.FindBlock(gameObject);
					if (block == null)
					{
						BWLog.Info("Could not find block " + gameObject.name);
						continue;
					}
					Bounds bounds = gameObject.GetComponent<Collider>().bounds;
					bounds.extents = ((!(bounds.extents == Vector3.zero)) ? bounds.extents : Vector3.one);
					Vector3 position = transform2.position;
					if (block is BlockAnimatedCharacter)
					{
						bounds.center += Vector3.up;
					}
					if (!bounds.Intersects(waterBounds))
					{
						continue;
					}
					Vector3 velocity = component.velocity;
					Vector3 worldCenterOfMass = component.worldCenterOfMass;
					float mass = block.GetMass();
					Vector3 vector = block.Scale();
					float num2 = vector.x * vector.y * vector.z;
					float num3 = mass / num2;
					if ((double)num3 < 0.2501)
					{
						num2 *= 0.5f;
					}
					Bounds bounds2 = bounds;
					Vector3 vector2 = bounds.size;
					float num4 = Mathf.Max(0.5f, vector2.x * vector2.y * vector2.z);
					float num5 = num2 / num4;
					bounds2.SetMinMax(Vector3.Max(bounds.min, waterBounds.min), Vector3.Min(bounds.max, waterBounds.max));
					Vector3 vector3 = bounds2.size;
					float num6 = vector3.x * vector3.y * vector3.z;
					float num7 = num6 * num5;
					Vector3 vector4 = Vector3.up * num7 * magnitude * waterDensity;
					Vector3 rhs = bounds2.center - worldCenterOfMass;
					Vector3 vector5 = velocity + Vector3.Cross(component.angularVelocity, rhs);
					Vector3 vector6 = vector5 - streamVelocity;
					Vector3 vector7 = -vector6 * num7 * waterFriction;
					Vector3 vector8 = vector4 + vector7 + block.GetWaterForce(num7 / num2, vector6, this);
					int instanceId = block.GetInstanceId();
					WaterSplashInfo waterSplashInfo;
					if (!splashInfos.ContainsKey(instanceId))
					{
						waterSplashInfo = new WaterSplashInfo(block);
						splashInfos.Add(instanceId, waterSplashInfo);
						waterSplashInfo.EnterWater();
					}
					else
					{
						waterSplashInfo = splashInfos[instanceId];
					}
					waterSplashInfo.Update();
					waterSplashInfo.forceSum += vector8.magnitude;
					blocksWithinWater.Add(instanceId);
					if (waterSplashInfo.counter < 10 && waterSplashInfo.forceSum > 40f)
					{
						waterSplashInfo.counter = 1000;
						if (Sound.sfxEnabled)
						{
							PlaySplashSound(bounds2.center);
						}
					}
					hashSet.Remove(instanceId);
					if (!isKinematic)
					{
						Vector3 force = vector8 * block.buoyancyMultiplier * buoyancyMultiplier;
						if (block is BlockAnimatedCharacter)
						{
							force /= 4f;
						}
						component.AddForceAtPosition(force, bounds2.center, ForceMode.Force);
						float num8 = num7 * -0.6f;
						Vector3 vector9 = num8 * component.angularVelocity * waterFriction;
						component.AddTorque(vector9 * block.buoyancyMultiplier, ForceMode.Force);
						if (num * vector7.magnitude * 0.01f > UnityEngine.Random.value)
						{
							Vector3 vector10 = bounds2.size;
							Vector3 position2 = bounds2.center - vector10 * 0.5f + new Vector3(UnityEngine.Random.value * vector10.x, UnityEngine.Random.value * vector10.y, UnityEngine.Random.value * vector10.y);
							float lifetime = 0.5f + UnityEngine.Random.value * 0.5f;
							bubblePs.Emit(position2, vector5 * 0.1f, 0.1f + UnityEngine.Random.value * 0.15f, lifetime, Color.white);
						}
					}
				}
			}
		}
		if (blocksWithinWater.Count > 0)
		{
			List<string> blockTags = TagManager.GetBlockTags(this);
			if (blockTags.Count > 0)
			{
				for (int i = 0; i < blockTags.Count; i++)
				{
					string key = blockTags[i];
					blocksWithinTaggedWater[key] = blocksWithinWater;
				}
			}
		}
		foreach (int item3 in hashSet)
		{
			splashInfos.Remove(item3);
		}
		if (!wakeUpWaterSensorBlocks)
		{
			return;
		}
		HashSet<Chunk> hashSet2 = new HashSet<Chunk>();
		BlockAbstractWater.GatherAllBlocksWithWaterSensors();
		foreach (Block blocksWithWaterSensor in BlockAbstractWater.blocksWithWaterSensors)
		{
			Chunk chunk = blocksWithWaterSensor.chunk;
			if (hashSet2.Contains(chunk))
			{
				continue;
			}
			GameObject gameObject2 = chunk.go;
			Rigidbody component2 = gameObject2.GetComponent<Rigidbody>();
			if (component2 != null)
			{
				component2.WakeUp();
				if (component2.IsSleeping() && component2.isKinematic)
				{
					Transform transform3 = gameObject2.transform;
					Vector3 position3 = transform3.position;
					transform3.position = position3;
				}
			}
			hashSet2.Add(chunk);
		}
		wakeUpWaterSensorBlocks = false;
	}

	private void PlaySplashSound(Vector3 pos)
	{
		GameObject gameObject = null;
		AudioSource audioSource = null;
		if (audioSourceObjects.Count < 5)
		{
			gameObject = new GameObject(go.name + " Splash " + (audioSourceObjects.Count + 1));
			audioSource = gameObject.AddComponent<AudioSource>();
			Sound.SetWorldAudioSourceParams(audioSource);
			audioSourceObjects.Add(gameObject);
		}
		if (gameObject == null)
		{
			for (int i = 0; i < audioSourceObjects.Count; i++)
			{
				GameObject gameObject2 = audioSourceObjects[i];
				AudioSource component = gameObject2.GetComponent<AudioSource>();
				if (!component.isPlaying)
				{
					audioSource = component;
					gameObject = gameObject2;
					break;
				}
			}
		}
		if (gameObject != null)
		{
			string name = "Water Splash Medium " + UnityEngine.Random.Range(1, 4);
			gameObject.transform.position = pos;
			Sound.PlaySound(name, audioSource);
		}
	}

	public override void Destroy()
	{
		base.Destroy();
		for (int i = 0; i < audioSourceObjects.Count; i++)
		{
			UnityEngine.Object.Destroy(audioSourceObjects[i]);
		}
		audioSourceObjects.Clear();
	}

	public override TileResultCode PaintToAction(ScriptRowExecutionInfo eInfo, object[] args)
	{
		bool flag = false;
		int intArg = Util.GetIntArg(args, 1, 0);
		if (intArg == 0)
		{
			string stringArg = Util.GetStringArg(args, 0, string.Empty);
			string paint = GetPaint(intArg);
			if (paint != stringArg)
			{
				flag = true;
			}
		}
		TileResultCode result = base.PaintToAction(eInfo, args);
		if (flag)
		{
			UpdateUnderwaterLightColors(cameraWasWithinWater);
		}
		return result;
	}

	public override bool ColliderIsTriggerInPlayMode()
	{
		return true;
	}
}
