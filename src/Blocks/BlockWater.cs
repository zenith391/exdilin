using System.Collections.Generic;
using UnityEngine;

namespace Blocks;

public class BlockWater : BlockAbstractWater
{
	private static Dictionary<string, int> f__switch_map4;

	private Dictionary<int, BlockWaterInfo> blockInfos;

	private List<BlockWaterInfo> blockInfoList;

	private int counter;

	private float mass;

	protected static BlockWater mainWater = null;

	public static HashSet<Block> blocksWithinWater;

	private static float maxWaterLevel = -1000000f;

	private float origPosY;

	private bool isInfiniteOcean = true;

	private Bounds waterBounds;

	public bool isReflective = true;

	private GameObject reflectiveWater;

	private float fogMultiplier = 1f;

	private Color fogColorOverride = Color.white;

	private static Color32 bubbleColor = Color.white;

	private bool doSnap;

	public float snapper = 80f;

	public BlockWater(List<List<Tile>> tiles)
		: base(tiles)
	{
		Transform transform = goT.Find("Reflective Water");
		if (transform != null)
		{
			reflectiveWater = transform.gameObject;
		}
	}

	public new static void Register()
	{
	}

	public static bool BlockWithinWater(Block block, bool checkDensity = false)
	{
		int instanceId = block.GetInstanceId();
		if (blocksWithinWater != null && blocksWithinWater.Contains(block))
		{
			if (checkDensity && mainWater.waterDensity == 0f)
			{
				return !(block is BlockAnimatedCharacter);
			}
			return true;
		}
		for (int i = 0; i < BlockAbstractWater.waterCubes.Count; i++)
		{
			BlockWaterCube blockWaterCube = BlockAbstractWater.waterCubes[i];
			if (blockWaterCube.blocksWithinWater != null && blockWaterCube.blocksWithinWater.Contains(instanceId))
			{
				return true;
			}
		}
		return false;
	}

	public static bool BlockWithinTaggedWater(Block block, string tag)
	{
		int instanceId = block.GetInstanceId();
		for (int i = 0; i < BlockAbstractWater.waterCubes.Count; i++)
		{
			BlockWaterCube blockWaterCube = BlockAbstractWater.waterCubes[i];
			if (blockWaterCube.blocksWithinTaggedWater.TryGetValue(tag, out var value) && value.Contains(instanceId))
			{
				return true;
			}
		}
		return false;
	}

	public override void OnCreate()
	{
		TextureTo(GetTexture(), GetTextureNormal(), permanent: true);
		UpdateSolidness();
		if (go.GetComponent<Collider>() != null)
		{
			go.GetComponent<Collider>().isTrigger = false;
		}
		UpdateRenderState();
	}

	public override void OnReconstructed()
	{
		base.OnReconstructed();
		UpdateSolidness();
		TextureTo(Scarcity.GetNormalizedTexture(GetTexture()), Vector3.up, permanent: true, 0, force: true);
	}

	private void UpdateSolidness()
	{
		string texture = GetTexture();
		isSolid = IsSolidTexture(texture);
		isReflective = IsReflectiveTexture(texture);
		if (go.GetComponent<Collider>() != null)
		{
			go.GetComponent<Collider>().isTrigger = !isSolid;
		}
		int layer = ((!isSolid) ? 13 : 4);
		go.layer = layer;
		if (goT.parent != null)
		{
			goT.parent.gameObject.layer = layer;
		}
		if (go.GetComponent<Collider>() is BoxCollider)
		{
			BoxCollider boxCollider = (BoxCollider)go.GetComponent<Collider>();
			Vector3 scale = GetScale();
			boxCollider.size = new Vector3(scale.x * 10f, scale.y, scale.z * 10f);
		}
		hasSlowWaves = texture == "Texture Water Stream" || Blocksworld.renderingWater;
	}

	private void UpdateRenderState()
	{
		if (go.activeSelf && reflectiveWater != null)
		{
			go.GetComponent<MeshRenderer>().enabled = !Blocksworld.renderingWater;
			reflectiveWater.SetActive(Blocksworld.renderingWater);
		}
	}

	public override void Play()
	{
		base.Play();
		if (mainWater != null)
		{
			BWLog.Warning("Possible error - multiple main water objects - this can cause problems with the blocks within list");
		}
		mainWater = this;
		if (blocksWithinWater == null)
		{
			blocksWithinWater = new HashSet<Block>();
		}
		buoyancyMultiplier = 1f;
		splashInfos.Clear();
		playingSplashInfos.Clear();
		blocksWithinWater.Clear();
		blockInfos = null;
		blockInfoList = null;
		waterBounds = GetWaterBounds();
		IgnoreRaycasts(value: false);
		UpdateSolidness();
		FindSplashAudioSources();
		maxWaterLevel = Mathf.Max(-10000000f, waterBounds.max.y);
		origPosY = goT.position.y;
		CollisionManager.AddIgnoreTriggerGO(go);
		UpdateRenderState();
	}

	public override void Play2()
	{
	}

	public override void Stop(bool resetBlock = true)
	{
		maxWaterLevel = -1000000f;
		mainWater = null;
		blocksWithinWater = null;
		IgnoreRaycasts(value: false);
		go.GetComponent<Collider>().enabled = true;
		UpdateSolidness();
		blockInfos = null;
		blockInfoList = null;
		fogColorOverride = Color.white;
		fogMultiplier = 1f;
		base.Stop(resetBlock);
		UpdateRenderState();
	}

	public override void ResetFrame()
	{
		streamVelocity.Set(0f, 0f, 0f);
	}

	public override Bounds GetWaterBounds()
	{
		if (go == null || go.GetComponent<Collider>() == null)
		{
			BWLog.Warning("GetWaterBounds called on block with no collider");
			return default(Bounds);
		}
		Bounds bounds = go.GetComponent<Collider>().bounds;
		if (isInfiniteOcean)
		{
			float num = 9999999f;
			bounds.Expand(num);
			bounds.center -= new Vector3(0f, num * 0.5f, 0f);
		}
		return bounds;
	}

	public override void Update()
	{
		base.Update();
		if (Blocksworld.CurrentState != State.Play)
		{
			waterBounds = GetWaterBounds();
		}
		bool flag = waterBounds.Contains(Blocksworld.cameraPosition);
		if (flag != cameraWasWithinWater)
		{
			UpdateUnderwaterLightColors(flag);
			if (isLava)
			{
				Blocksworld.worldSky.go.GetComponent<Renderer>().enabled = !flag && !Blocksworld.renderingSkybox;
				Blocksworld.mainCamera.backgroundColor = new Color(1f, 0.5f, 0f);
				Debug.Log("Water set camera background color to " + Blocksworld.mainCamera.backgroundColor.ToString());
			}
		}
		bool flag2 = flag && isLava;
		fogMultiplier = ((!flag2) ? 1f : 0.15f);
		fogColorOverride = ((!flag2) ? Color.white : new Color(1f, 0.5f, 0f));
		UpdateSounds(bubbleSoundWithin && flag);
		cameraWasWithinWater = flag;
		BlockAbstractWater.cameraWithinOcean = flag;
	}

	public bool IsSolidTexture(string texture)
	{
		texture = Scarcity.GetNormalizedTexture(texture);
		switch (texture)
		{
		case "Water":
		case "Texture Water Stream":
		case "Texture Lava":
			return false;
		default:
			return true;
		}
	}

	public bool IsReflectiveTexture(string texture)
	{
		texture = Scarcity.GetNormalizedTexture(texture);
		switch (texture)
		{
		case "Water":
		case "Texture Water Stream":
		case "Ice Material":
			return true;
		default:
			return false;
		}
	}

	public static bool IsWaterTexture(string texture)
	{
		if (texture != null)
		{
			if (f__switch_map4 == null)
			{
				f__switch_map4 = new Dictionary<string, int>(19)
				{
					{ "Plain", 0 },
					{ "Water", 0 },
					{ "Ice Material", 0 },
					{ "Ice Material_Terrain", 0 },
					{ "Grass", 0 },
					{ "Rubber Material", 0 },
					{ "Rubber Material_Terrain", 0 },
					{ "Rock", 0 },
					{ "Sand", 0 },
					{ "Texture Crater", 0 },
					{ "Texture Crater_Terrain", 0 },
					{ "Yellow Brick Road", 0 },
					{ "Grass 2", 0 },
					{ "Terrain Grid", 0 },
					{ "Road Blank", 0 },
					{ "Texture Water Stream", 0 },
					{ "Texture Lava", 0 },
					{ "Dust", 0 },
					{ "Terrain Planet", 0 }
				};
			}
			if (f__switch_map4.TryGetValue(texture, out var value) && value == 0)
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsBubblingWithin(string texture)
	{
		switch (texture)
		{
		case "Plain":
		case "Water":
		case "Texture Water Stream":
		case "Texture Lava":
		case "Ice Material":
		case "Ice Material_Terrain":
			return true;
		default:
			return false;
		}
	}

	public void SnapPosition()
	{
		if (doSnap)
		{
			Vector3 position = goT.position;
			Vector3 position2 = new Vector3(Mathf.Round(position.x / snapper) * snapper, position.y, Mathf.Round(position.z / snapper) * snapper);
			goT.position = position2;
		}
	}

	private void UpdateReflectiveWaterProperties()
	{
		if (reflectiveWater != null)
		{
			Material material = reflectiveWater.GetComponent<Renderer>().material;
			if (Blocksworld.colorDefinitions.TryGetValue(base.currentPaint, out var value))
			{
				material.SetColor("_RefrColor", value[0]);
			}
			Vector4 vector = Vector4.zero;
			if (!isSolid)
			{
				vector = new Vector4(9f, 4.5f, -8f, -3.5f);
			}
			material.SetVector("WaveSpeed", vector);
		}
	}

	public override TileResultCode PaintTo(string paint, bool permanent, int meshIndex = 0)
	{
		if (meshIndex != 0)
		{
			return TileResultCode.True;
		}
		TileResultCode result = base.PaintTo(paint, permanent, meshIndex);
		if (isSolid)
		{
			Material sharedMaterial = go.GetComponent<Renderer>().sharedMaterial;
			Color terrainColor = BlockTerrain.GetTerrainColor(paint);
			if (terrainColor != Color.white)
			{
				sharedMaterial.SetColor("_Color", terrainColor);
			}
		}
		UpdateReflectiveWaterProperties();
		return result;
	}

	public override TileResultCode TextureTo(string texture, Vector3 normal, bool permanent, int meshIndex = 0, bool force = false)
	{
		if (meshIndex != 0)
		{
			return TileResultCode.True;
		}
		texture = Scarcity.GetNormalizedTexture(texture);
		bubbleSoundWithin = IsBubblingWithin(texture);
		bool flag = IsWaterTexture(texture);
		if (texture == "Plain")
		{
			texture = "Water";
		}
		else if (flag && texture != "Water")
		{
			switch (Scarcity.GetNormalizedTexture(texture))
			{
			case "Rubber Material":
			case "Texture Crater":
				texture += "_Terrain";
				break;
			case "Ice Material":
				texture = Scarcity.GetNormalizedTexture(texture) + "_Water";
				break;
			}
		}
		TileResultCode result = base.TextureTo(texture, normal, permanent, meshIndex, flag || force);
		bool flag2 = hasSlowWaves;
		UpdateSolidness();
		isLava = texture == "Texture Lava";
		doSnap = isSolid || isLava;
		if (reflectiveWater != null)
		{
			if (flag2 != hasSlowWaves)
			{
				if (hasSlowWaves)
				{
					reflectiveWater.GetComponent<Renderer>().material = (Material)Resources.Load("Materials/BWWaterPro");
				}
				else
				{
					reflectiveWater.GetComponent<Renderer>().material = (Material)Resources.Load("Materials/BWWaterProStagnant");
				}
			}
			UpdateReflectiveWaterProperties();
		}
		if (Blocksworld.CurrentState != State.Play)
		{
			go.GetComponent<Collider>().isTrigger = false;
		}
		if (bubbleSoundWithin)
		{
			GetBubbleClip();
		}
		return result;
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (!isSolid)
		{
			UpdateWaterForces();
		}
	}

	protected override void SetWaterLevelOffset(float newOffset)
	{
		base.SetWaterLevelOffset(newOffset);
		Vector3 position = goT.position;
		goT.position = new Vector3(position.x, origPosY + waterLevelOffset, position.z);
		waterBounds = GetWaterBounds();
		maxWaterLevel = waterBounds.max.y;
	}

	private void CreateBlockLists()
	{
		if (blockInfos != null)
		{
			return;
		}
		BlockAbstractWater.GatherAllBlocksWithWaterSensors();
		blockInfos = new Dictionary<int, BlockWaterInfo>();
		blockInfoList = new List<BlockWaterInfo>();
		foreach (Block item in BWSceneManager.AllBlocks())
		{
			if (!(item is BlockAbstractWater) && !(item is BlockVolume) && !(item is BlockPosition) && !(item is BlockTerrain) && !(item is BlockSky) && !(item is BlockBillboard))
			{
				BlockWaterInfo blockWaterInfo = new BlockWaterInfo(item);
				blockInfos[item.GetInstanceId()] = blockWaterInfo;
				blockWaterInfo.interval = 1;
				blockWaterInfo.isSimulating = true;
				blockInfoList.Add(blockWaterInfo);
			}
		}
		counter = 0;
	}

	private void UpdateWaterForces()
	{
		CreateBlockLists();
		float magnitude = Physics.gravity.magnitude;
		bool sfxEnabled = Sound.sfxEnabled;
		float num = Blocksworld.fixedDeltaTime / 0.02f;
		counter++;
		Vector3 up = goT.up;
		Bounds bounds = default(Bounds);
		for (int num2 = blockInfoList.Count - 1; num2 >= 0; num2--)
		{
			BlockWaterInfo blockWaterInfo = blockInfoList[num2];
			int interval = blockWaterInfo.interval;
			if ((counter + blockWaterInfo.counterOffset) % interval == 0)
			{
				Block block = blockWaterInfo.block;
				GameObject gameObject = block.go;
				int instanceID = gameObject.GetInstanceID();
				Transform transform = block.goT;
				Vector3 position = transform.position;
				bool flag = block is BlockAnimatedCharacter;
				if (flag)
				{
					position += Vector3.up;
				}
				float maxExtent = blockWaterInfo.maxExtent;
				float num3 = position.y - maxExtent - maxWaterLevel;
				if (num3 > 0f)
				{
					blocksWithinWater.Remove(block);
					if (splashInfos.TryGetValue(instanceID, out var value))
					{
						value.LeaveWater();
						splashInfos.Remove(instanceID);
					}
					int interval2 = Mathf.Min(Mathf.RoundToInt(5f + num3 * 0.2f), 25);
					blockWaterInfo.interval = interval2;
				}
				else
				{
					bool flag2 = position.y + maxExtent < maxWaterLevel;
					if (flag2)
					{
						bounds.center = position;
						bounds.size = blockWaterInfo.scale;
					}
					else
					{
						Collider component = gameObject.GetComponent<Collider>();
						bounds = ((!(component != null)) ? block.GetBounds() : component.bounds);
					}
					if (flag)
					{
						bounds.center += Vector3.up;
					}
					bounds.center -= Vector3.up * WaterLevelOffset(position);
					blockWaterInfo.checkCount++;
					if (flag2 || waterBounds.Intersects(bounds))
					{
						Rigidbody rb = block.chunk.rb;
						if (rb == null)
						{
							if (!blockWaterInfo.hasWaterSensor)
							{
								blockInfoList.RemoveAt(num2);
								blockWaterInfo.isSimulating = false;
							}
							else
							{
								blockWaterInfo.interval = 5;
							}
							blocksWithinWater.Add(block);
						}
						else if (rb.isKinematic)
						{
							if (!blockWaterInfo.hasWaterSensor)
							{
								blockInfoList.RemoveAt(num2);
								blockWaterInfo.isSimulating = false;
							}
							else
							{
								blockWaterInfo.interval = 5;
							}
							blocksWithinWater.Add(block);
						}
						else
						{
							blockWaterInfo.interval = 1;
							Vector3 velocity = rb.velocity;
							Vector3 worldCenterOfMass = rb.worldCenterOfMass;
							float num4 = blockWaterInfo.mass;
							Vector3 scale = blockWaterInfo.scale;
							float num5 = scale.x * scale.y * scale.z;
							float num6 = num4 / num5;
							if ((double)num6 < 0.2501)
							{
								num5 *= 0.5f;
							}
							WaterSplashInfo value2;
							bool flag3 = splashInfos.TryGetValue(instanceID, out value2);
							if (!blocksWithinWater.Contains(block) || !flag3)
							{
								value2 = new WaterSplashInfo(block);
								splashInfos[instanceID] = value2;
								value2.EnterWater();
							}
							else
							{
								value2 = splashInfos[instanceID];
							}
							value2.Update();
							blocksWithinWater.Add(block);
							Bounds bounds2 = bounds;
							float num7;
							if (flag2)
							{
								num7 = num5;
							}
							else
							{
								Vector3 vector = bounds.size;
								float num8 = Mathf.Max(0.5f, vector.x * vector.y * vector.z);
								float num9 = num5 / num8;
								bounds2.SetMinMax(Vector3.Max(bounds.min, waterBounds.min), Vector3.Min(bounds.max, waterBounds.max));
								Vector3 vector2 = bounds2.size;
								float num10 = vector2.x * vector2.y * vector2.z;
								num7 = num10 * num9;
							}
							Vector3 vector3 = up * (num7 * magnitude * waterDensity);
							Vector3 center = bounds2.center;
							Vector3 rhs = center - worldCenterOfMass;
							Vector3 vector4 = velocity + Vector3.Cross(rb.angularVelocity, rhs);
							Vector3 vector5 = vector4 - streamVelocity;
							Vector3 vector6 = -vector5 * (num7 * waterFriction);
							float a = ((!flag) ? 450f : 0f);
							vector6 = vector6.normalized * Mathf.Min(a, vector6.magnitude);
							Vector3 vector7 = vector3 + vector6 + block.GetWaterForce(num7 / num5, vector5, this);
							if (value2.counter < 10 && sfxEnabled)
							{
								value2.forceSum += vector7.magnitude;
								if (value2.forceSum > 40f)
								{
									value2.counter = 1000;
									foreach (GameObject splashAudioSource in BlockAbstractWater.splashAudioSources)
									{
										AudioSource component2 = splashAudioSource.GetComponent<AudioSource>();
										if (!component2.isPlaying)
										{
											splashAudioSource.transform.position = bounds2.max + Vector3.up;
											float volume = Mathf.Clamp((value2.forceSum - 30f) * 0.01f, 0f, 1f);
											component2.volume = volume;
											component2.Play();
											break;
										}
									}
								}
							}
							rb.AddForceAtPosition(vector7 * (block.buoyancyMultiplier * buoyancyMultiplier), center, ForceMode.Force);
							float num11 = num7 * -0.6f;
							Vector3 vector8 = num11 * waterFriction * rb.angularVelocity;
							float a2 = ((!flag) ? 100f : 0f);
							vector8 = vector8.normalized * Mathf.Min(a2, vector8.magnitude);
							rb.AddTorque(vector8 * block.buoyancyMultiplier, ForceMode.Force);
							if (num * vector6.magnitude * 0.01f > Random.value)
							{
								Vector3 vector9 = bounds2.size;
								Vector3 position2 = center - vector9 * 0.5f + new Vector3(Random.value * vector9.x, Random.value * vector9.y, Random.value * vector9.y);
								float lifetime = 0.5f + Random.value * 0.5f;
								bubblePs.Emit(position2, vector4 * 0.1f, 0.1f + Random.value * 0.15f, lifetime, bubbleColor);
							}
						}
					}
					else
					{
						blocksWithinWater.Remove(block);
						if (splashInfos.TryGetValue(instanceID, out var value3))
						{
							value3.LeaveWater();
							splashInfos.Remove(instanceID);
						}
						blockWaterInfo.interval = Mathf.RoundToInt(Mathf.Max(5f + num3 * 0.2f, 2f));
						if (!blockWaterInfo.hasWaterSensor && blockWaterInfo.checkCount == 1)
						{
							Transform parent = transform.parent;
							if (parent != null)
							{
								Rigidbody component3 = parent.GetComponent<Rigidbody>();
								if (component3 == null)
								{
									blockInfoList.RemoveAt(num2);
									blockWaterInfo.isSimulating = false;
								}
							}
						}
					}
				}
			}
		}
	}

	public override void RemovedPlayBlock(Block b)
	{
		CreateBlockLists();
		if (blockInfos.TryGetValue(b.GetInstanceId(), out var value))
		{
			blockInfoList.Remove(value);
			value.isSimulating = false;
		}
	}

	public void AddBlockToSimulation(Block b)
	{
		CreateBlockLists();
		int instanceId = b.GetInstanceId();
		if (blockInfos.TryGetValue(instanceId, out var value))
		{
			if (!value.isSimulating)
			{
				blockInfoList.Add(value);
			}
			blocksWithinWater.Remove(b);
			splashInfos.Remove(instanceId);
			value.isSimulating = true;
			value.interval = 1;
		}
	}

	public bool SimulatesBlock(Block b)
	{
		if (blockInfos != null && blockInfos.TryGetValue(b.GetInstanceId(), out var value))
		{
			return value.isSimulating;
		}
		return true;
	}

	public override bool HasDynamicalLight()
	{
		return true;
	}

	public override Color GetFogColorOverride()
	{
		return fogColorOverride;
	}

	public override float GetFogMultiplier()
	{
		return fogMultiplier;
	}
}
