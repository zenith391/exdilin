using System.Collections.Generic;
using UnityEngine;

namespace Blocks;

public class BlockAbstractLaser : Block
{
	private LaserBeam _beam;

	private List<AbstractProjectile> projectiles = new List<AbstractProjectile>();

	private bool _beaming;

	public float beamStrength = 1f;

	private bool _pulsing;

	private bool _firingProjectile;

	private ParticleSystem smokeParticleSystem;

	private GameObject smokeParticleSystemGo;

	private AudioSource ourSource;

	public static ParticleSystem projectileHitParticleSystem;

	private ParticleSystem muzzleFlashParticleSystem;

	private GameObject muzzleFlashParticleSystemGo;

	private Color32 gray = new Color32(127, 127, 127, byte.MaxValue);

	public float segmentTime = 100f;

	private float smokeRndOffset = 2f;

	public float pulseSpeedMultiplier = 4f;

	public float pulseFrequencyFraction = 1f;

	public float projectileSpeedMultiplier = 4f;

	public float projectileFrequencyFraction = 1f;

	public int pulsesPerCycle = 2;

	private float pulseLengthFraction = 0.4f;

	private float pulseCycleTime = 0.25f;

	public int projectilesPerCycle = 2;

	private float projectileLengthFraction = 0.4f;

	private float projectileCycleTime = 0.25f;

	public LaserMetaData laserMeta;

	public float beamSizeMultiplier = 1f;

	public float pulseSizeMultiplier = 1f;

	public float projectileSizeMultiplier = 1f;

	public Vector3 laserExitOffset = Vector3.zero;

	public Vector3 projectileExitOffset = Vector3.zero;

	public float pulseLengthMultiplier = 1f;

	public float pulseFrequencyMultiplier = 1f;

	public float projectileLengthMultiplier = 1f;

	public float projectileFrequencyMultiplier = 1f;

	public bool fixLaserColor;

	public Color laserColor = Color.white;

	public BlockAnimatedCharacter animatedCharacter;

	private LaserPulse _currentPulse;

	public static HashSet<Block> Hits = new HashSet<Block>();

	public static HashSet<Block> ModelHits = new HashSet<Block>();

	private Projectile _currentProjectile;

	public static HashSet<Block> ProjectileHits = new HashSet<Block>();

	public static HashSet<Block> ModelProjectileHits = new HashSet<Block>();

	public static Dictionary<string, HashSet<Block>> TagHits = new Dictionary<string, HashSet<Block>>();

	public static Dictionary<string, HashSet<Block>> ModelTagHits = new Dictionary<string, HashSet<Block>>();

	public static Dictionary<string, HashSet<Block>> TagProjectileHits = new Dictionary<string, HashSet<Block>>();

	public static Dictionary<string, HashSet<Block>> ModelTagProjectileHits = new Dictionary<string, HashSet<Block>>();

	private bool _paused;

	private static Dictionary<string, Color> laserColors = null;

	public static bool anyLaserBeamOrPulseAvailable = false;

	public static bool anyProjectileAvailable = false;

	private static HashSet<string> transparentTexturesForLaser = new HashSet<string> { "Glass", "Transparent", "Texture Soccer Net", "Water", "Ice Material", "Ice Material_Terrain", "Invisible" };

	private static Vector3[] upArr = new Vector3[2]
	{
		Vector3.up,
		Vector3.right
	};

	private static int[] triangleOffsets = new int[12]
	{
		0, 1, 2, 2, 1, 3, 2, 1, 0, 3,
		1, 2
	};

	public BlockAbstractLaser(List<List<Tile>> tiles)
		: base(tiles)
	{
		smokeParticleSystemGo = Object.Instantiate(Resources.Load("Blocks/BlockLaser Smoke Particle System")) as GameObject;
		smokeParticleSystem = smokeParticleSystemGo.GetComponent<ParticleSystem>();
		muzzleFlashParticleSystemGo = Object.Instantiate(Resources.Load("Particles/Muzzle Flash")) as GameObject;
		muzzleFlashParticleSystem = muzzleFlashParticleSystemGo.GetComponent<ParticleSystem>();
		muzzleFlashParticleSystem.enableEmission = false;
		if (projectileHitParticleSystem == null)
		{
			GameObject gameObject = Object.Instantiate(Resources.Load("Particles/Projectile Hit Particle System")) as GameObject;
			projectileHitParticleSystem = gameObject.GetComponent<ParticleSystem>();
			projectileHitParticleSystem.enableEmission = false;
		}
		muzzleFlashParticleSystemGo.transform.parent = goT;
		muzzleFlashParticleSystemGo.transform.localPosition = Vector3.zero;
		muzzleFlashParticleSystemGo.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
		pulseCycleTime = 0.25f / (float)pulsesPerCycle;
		projectileCycleTime = 0.25f / (float)projectilesPerCycle;
		if (laserColors == null)
		{
			laserColors = new Dictionary<string, Color>();
		}
		loopName = "Laser Beam Loop";
	}

	public virtual bool CanFireProjectiles()
	{
		return false;
	}

	public virtual bool CanFireLaser()
	{
		return true;
	}

	public static bool IsHitByPulseOrBeam(Block block)
	{
		if (anyLaserBeamOrPulseAvailable)
		{
			return Hits.Contains(block);
		}
		return false;
	}

	public static bool IsHitByTaggedPulseOrBeam(Block block, string tagName)
	{
		if (TagHits.TryGetValue(tagName, out var value))
		{
			return value.Contains(block);
		}
		return false;
	}

	public static bool IsHitByProjectile(Block block)
	{
		if (anyProjectileAvailable)
		{
			return ProjectileHits.Contains(block);
		}
		return false;
	}

	public static bool IsHitByProjectileModel(Block modelBlock)
	{
		if (anyProjectileAvailable)
		{
			return ModelProjectileHits.Contains(modelBlock);
		}
		return false;
	}

	public static bool IsHitByProjectile(List<Block> list)
	{
		for (int i = 0; i < list.Count; i++)
		{
			Block item = list[i];
			if (ProjectileHits.Contains(item))
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsHitByTaggedProjectile(Block block, string tagName)
	{
		if (TagProjectileHits.TryGetValue(tagName, out var value))
		{
			return value.Contains(block);
		}
		return false;
	}

	public static bool IsHitByLaser(Block b)
	{
		return Hits.Contains(b);
	}

	public static bool IsHitByLaserModel(Block modelBlock)
	{
		return ModelHits.Contains(modelBlock);
	}

	public static bool IsHitByLaser(List<Block> list)
	{
		for (int i = 0; i < list.Count; i++)
		{
			Block item = list[i];
			if (Hits.Contains(item))
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsHitByTaggedLaser(List<Block> list, string tagName)
	{
		for (int i = 0; i < list.Count; i++)
		{
			Block item = list[i];
			if (TagHits.ContainsKey(tagName) && TagHits[tagName].Contains(item))
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsHitByTaggedLaserModel(Block modelBlock, string tagName)
	{
		if (ModelTagHits.ContainsKey(tagName))
		{
			return ModelTagHits[tagName].Contains(modelBlock);
		}
		return false;
	}

	public static bool IsHitByTaggedProjectile(List<Block> list, string tagName)
	{
		for (int i = 0; i < list.Count; i++)
		{
			Block item = list[i];
			if (TagProjectileHits.ContainsKey(tagName) && TagProjectileHits[tagName].Contains(item))
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsHitByTaggedProjectileModel(Block modelBlock, string tagName)
	{
		if (ModelTagProjectileHits.ContainsKey(tagName))
		{
			return ModelTagProjectileHits[tagName].Contains(modelBlock);
		}
		return false;
	}

	public override void Pause()
	{
		smokeParticleSystem.Pause();
		_paused = true;
	}

	public override void Resume()
	{
		smokeParticleSystem.Play();
		_paused = false;
	}

	public override void Play()
	{
		base.Play();
		laserMeta = go.GetComponent<LaserMetaData>();
		if (laserMeta != null)
		{
			loopName = laserMeta.loopSfx;
			float num = 0.85f + 0.15f * Mathf.Min(size.x, size.z);
			pulseSizeMultiplier = num * laserMeta.pulseSizeMultiplier;
			beamSizeMultiplier = num * laserMeta.beamSizeMultiplier;
			projectileSizeMultiplier = num * laserMeta.projectileSizeMultiplier;
			laserExitOffset = laserMeta.exitOffset;
			laserExitOffset.Scale(size);
			projectileExitOffset = laserMeta.projectileExitOffset;
			projectileExitOffset.Scale(size);
			pulseLengthMultiplier = laserMeta.pulseLengthMultiplier;
			pulseFrequencyMultiplier = laserMeta.pulseFrequencyMultiplier;
			fixLaserColor = laserMeta.fixLaserColor;
			laserColor = laserMeta.laserColor;
		}
		else
		{
			BWLog.Info("Could not find laser meta data component");
		}
		muzzleFlashParticleSystemGo.transform.localPosition = laserMeta.exitOffset + Vector3.up * 0.8f;
	}

	public static void ClearHits()
	{
		anyLaserBeamOrPulseAvailable = false;
		anyProjectileAvailable = false;
		Hits.Clear();
		ModelHits.Clear();
		ClearSets(TagHits);
		ClearSets(ModelTagHits);
		ProjectileHits.Clear();
		ModelProjectileHits.Clear();
		ClearSets(TagProjectileHits);
		ClearSets(ModelTagProjectileHits);
	}

	private static void ClearSets(Dictionary<string, HashSet<Block>> dict)
	{
		if (dict.Count <= 0)
		{
			return;
		}
		bool flag = true;
		foreach (KeyValuePair<string, HashSet<Block>> item in dict)
		{
			HashSet<Block> value = item.Value;
			if (value.Count > 0)
			{
				flag = false;
			}
			item.Value.Clear();
		}
		if (flag)
		{
			dict.Clear();
		}
	}

	public override void PlaceInCharacterHand(BlockAnimatedCharacter character)
	{
		animatedCharacter = character;
	}

	public override void RemoveFromCharacterHand()
	{
		animatedCharacter = null;
	}

	public TileResultCode IsBeaming(ScriptRowExecutionInfo eInfo, object[] args)
	{
		eInfo.floatArg = Mathf.Min(eInfo.floatArg, beamStrength);
		if (_beaming)
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public TileResultCode IsPulsing(ScriptRowExecutionInfo eInfo, object[] args)
	{
		eInfo.floatArg = Mathf.Min(eInfo.floatArg, pulseFrequencyFraction);
		if (_pulsing)
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public TileResultCode IsFiringProjectile(ScriptRowExecutionInfo eInfo, object[] args)
	{
		eInfo.floatArg = Mathf.Min(eInfo.floatArg, projectileFrequencyFraction);
		if (_firingProjectile)
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public static bool IsReflective(string paint)
	{
		return paint == "White";
	}

	public static bool IsTransparent(string texture)
	{
		return transparentTexturesForLaser.Contains(texture);
	}

	public override void Destroy()
	{
		Object.Destroy(smokeParticleSystemGo);
		Object.Destroy(muzzleFlashParticleSystemGo);
		ClearBeams();
		base.Destroy();
	}

	public override void Exploded()
	{
		base.Exploded();
		ClearBeams();
	}

	public override TileResultCode ExplodeLocal(ScriptRowExecutionInfo eInfo, object[] args)
	{
		ClearBeams();
		return base.ExplodeLocal(eInfo, args);
	}

	private void ClearBeams()
	{
		foreach (AbstractProjectile projectile in projectiles)
		{
			LaserPulse laserPulse = (LaserPulse)projectile;
			laserPulse.Destroy();
		}
		projectiles.Clear();
		if (_beam != null)
		{
			_beam.Destroy();
			_beam = null;
		}
	}

	public override void ResetFrame()
	{
		base.ResetFrame();
		if (!_beaming && _beam != null)
		{
			_beam.Destroy();
			_beam = null;
		}
		_beaming = false;
		_pulsing = false;
		_firingProjectile = false;
	}

	public override void Update()
	{
		base.Update();
		if (_beam != null)
		{
			_beam.Update(_paused);
		}
		AbstractProjectile abstractProjectile = null;
		for (int i = 0; i < projectiles.Count; i++)
		{
			AbstractProjectile abstractProjectile2 = projectiles[i];
			abstractProjectile2.Update();
			if (abstractProjectile2.ShouldBeDestroyed())
			{
				abstractProjectile = abstractProjectile2;
			}
		}
		if (abstractProjectile != null)
		{
			if (_currentPulse == abstractProjectile)
			{
				_currentPulse = null;
			}
			projectiles.Remove(abstractProjectile);
			abstractProjectile.Destroy();
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (_beam != null)
		{
			_beam.FixedUpdate(fromUpdate: false, _paused);
		}
		anyLaserBeamOrPulseAvailable = anyLaserBeamOrPulseAvailable || _beam != null || projectiles.Count > 0;
		anyProjectileAvailable = projectiles.Count > 0;
		for (int i = 0; i < projectiles.Count; i++)
		{
			AbstractProjectile abstractProjectile = projectiles[i];
			abstractProjectile.FixedUpdate();
		}
		if (Sound.sfxEnabled && !vanished && (_beaming || _pulsing))
		{
			if (ourSource == null)
			{
				ourSource = go.GetComponent<AudioSource>();
			}
			if (!(ourSource != null) || !ourSource.isPlaying)
			{
				PlayLoopSound(_beaming, GetLoopClip(), 0.1f + 0.4f * beamStrength, null, 0.8f + 0.2f * beamStrength);
				UpdateWithinWaterLPFilter();
			}
		}
		else
		{
			PlayLoopSound(play: false, GetLoopClip());
		}
	}

	public override void Stop(bool resetBlock = true)
	{
		base.Stop(resetBlock);
		if (_beam != null)
		{
			_beam.Destroy();
			_beam = null;
		}
		foreach (AbstractProjectile projectile in projectiles)
		{
			projectile.Destroy();
		}
		projectiles.Clear();
		smokeParticleSystem.Clear();
		smokeParticleSystem.Play();
		PlayLoopSound(play: false, GetLoopClip());
		_paused = false;
	}

	public Color GetLaserColor()
	{
		if (fixLaserColor)
		{
			return laserColor;
		}
		string paint = GetPaint();
		if (laserColors.ContainsKey(paint))
		{
			return laserColors[paint];
		}
		return go.GetComponent<Renderer>().sharedMaterial.GetColor("_Color");
	}

	public Vector3 GetFireDirectionUp()
	{
		if (animatedCharacter == null)
		{
			return goT.forward;
		}
		return animatedCharacter.goT.up;
	}

	public Vector3 GetFireDirectionForward()
	{
		if (animatedCharacter == null)
		{
			return goT.up;
		}
		return animatedCharacter.goT.forward;
	}

	private void UpdatePulseVariables(float power)
	{
		pulseSpeedMultiplier = power;
		float num = 50f;
		float t = power / num;
		pulseLengthFraction = Mathf.Lerp(0.43f, 0.1f, t);
		segmentTime = Mathf.Lerp(100f, 400f, t);
	}

	public TileResultCode Pulse(ScriptRowExecutionInfo eInfo, object[] args)
	{
		float num = ((args.Length == 0) ? 4f : ((float)args[0]));
		if (pulseSpeedMultiplier != num)
		{
			UpdatePulseVariables(num);
		}
		if (broken || vanished)
		{
			if (_currentPulse != null)
			{
				_currentPulse.StopReceivingEnergy();
			}
			return TileResultCode.True;
		}
		pulseFrequencyFraction = eInfo.floatArg;
		if (_currentPulse == null)
		{
			_currentPulse = new LaserPulse(this);
			projectiles.Add(_currentPulse);
			if (eInfo.timer == 0f)
			{
				string sfxName = ((!(laserMeta == null)) ? laserMeta.pulseSfx : "Laser Pulse");
				PlayPositionedSound(sfxName, 0.5f);
			}
		}
		_pulsing = true;
		float num2 = 6f - 5f * eInfo.floatArg;
		if (eInfo.timer >= pulseCycleTime * num2 / pulseFrequencyMultiplier)
		{
			if (_currentPulse != null)
			{
				_currentPulse.StopReceivingEnergy();
			}
			_currentPulse = null;
			return TileResultCode.True;
		}
		if (eInfo.timer >= pulseCycleTime * pulseLengthFraction * pulseLengthMultiplier / pulseFrequencyMultiplier)
		{
			_currentPulse.StopReceivingEnergy();
			return TileResultCode.Delayed;
		}
		_currentPulse.EnergyFraction = 1f;
		return TileResultCode.Delayed;
	}

	public TileResultCode FireProjectile(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (broken || vanished)
		{
			if (_currentProjectile != null)
			{
				_currentProjectile.StopReceivingEnergy();
			}
			return TileResultCode.True;
		}
		projectileFrequencyFraction = eInfo.floatArg;
		if (_currentProjectile == null)
		{
			_currentProjectile = new Projectile(this);
			projectiles.Add(_currentProjectile);
			if (eInfo.timer == 0f)
			{
				string sfxName = ((!(laserMeta == null)) ? laserMeta.projectileSfx : "Fire");
				PlayPositionedSound(sfxName, 0.5f);
				Rigidbody rb = chunk.rb;
				if (rb != null && !rb.isKinematic)
				{
					float num = laserMeta.projectileRecoilForce;
					float mass = rb.mass;
					float num2 = num / mass;
					if (num2 > laserMeta.projectileMaxRecoilPerMass)
					{
						num *= laserMeta.projectileMaxRecoilPerMass / num2;
					}
					Vector3 force = -goT.up * num;
					rb.AddForce(force, ForceMode.Impulse);
				}
				muzzleFlashParticleSystemGo.transform.localPosition = laserMeta.exitOffset + Vector3.up * 0.5f;
				muzzleFlashParticleSystemGo.transform.localRotation = Quaternion.Euler(-90f, 0f, Random.value * 360f);
				muzzleFlashParticleSystem.Emit(1);
			}
		}
		_firingProjectile = true;
		float num3 = 6f - 5f * eInfo.floatArg;
		if (eInfo.timer >= projectileCycleTime * num3 / projectileFrequencyMultiplier)
		{
			if (_currentProjectile != null)
			{
				_currentProjectile.StopReceivingEnergy();
			}
			_currentProjectile = null;
			return TileResultCode.True;
		}
		if (eInfo.timer >= projectileCycleTime * projectileLengthFraction * projectileLengthMultiplier / projectileFrequencyMultiplier)
		{
			_currentProjectile.StopReceivingEnergy();
			return TileResultCode.Delayed;
		}
		_currentProjectile.EnergyFraction = eInfo.timer / (projectileCycleTime * projectileLengthFraction * projectileLengthMultiplier / projectileFrequencyMultiplier);
		return TileResultCode.Delayed;
	}

	public TileResultCode Beam(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (broken || vanished)
		{
			return TileResultCode.True;
		}
		if (_beam == null)
		{
			_beam = new LaserBeam(this);
		}
		_beaming = true;
		beamStrength = eInfo.floatArg;
		return TileResultCode.True;
	}

	public void UpdateLaserHitParticles(Vector3 pos, Vector3 normal, Vector3 inDir, bool emitSparks = true, bool emitSmoke = true)
	{
		if (!_paused && Random.value < 0.5f)
		{
			Vector3 normalized = Vector3.Cross(inDir, normal).normalized;
			Vector3 vector = Vector3.Cross(normalized, inDir);
			float num = 0.02f + Random.value * 0.08f;
			float num2 = 2f + Random.value * 8f;
			float num3 = (0f - smokeRndOffset) * 0.5f;
			float num4 = smokeRndOffset;
			Vector3 vector2 = -inDir + normalized * (num3 + Random.value * num4) + vector * (num3 + Random.value * num4);
			vector2.Normalize();
			if (emitSmoke && Random.value < 0.5f)
			{
				num2 = 0.1f + Random.value * 0.4f;
				num = 1f + Random.value * 0.5f;
				ParticleSystem.Particle particle = new ParticleSystem.Particle
				{
					position = pos,
					velocity = vector2 * num2,
					size = 0.2f + Random.value * 0.3f,
					remainingLifetime = num,
					startLifetime = num,
					color = gray
				};
				smokeParticleSystem.Emit(particle);
			}
		}
	}

	public static void DrawLaserLine(Vector3 from, Vector3 to, Vector3 up, List<Vector3> vertices, List<Vector2> uvs, List<int> triangles, float sizeMult = 1f)
	{
		Vector3 normalized = (to - from).normalized;
		Vector3 normalized2 = Vector3.Cross(normalized, up).normalized;
		float num = 0.25f * sizeMult;
		upArr[0] = up;
		upArr[1] = normalized2;
		for (int i = 0; i < upArr.Length; i++)
		{
			Vector3 vector = upArr[i];
			int count = vertices.Count;
			vertices.Add(from - vector * num);
			uvs.Add(new Vector2(0f, 0f));
			vertices.Add(from + vector * num);
			uvs.Add(new Vector2(0f, 1f));
			vertices.Add(to - vector * num);
			uvs.Add(new Vector2(1f, 0f));
			vertices.Add(to + vector * num);
			uvs.Add(new Vector2(1f, 1f));
			for (int j = 0; j < triangleOffsets.Length; j++)
			{
				triangles.Add(count + triangleOffsets[j]);
			}
		}
	}

	public static void DrawProjectileLine(Vector3 from, Vector3 to, Vector3 up, List<Vector3> vertices, List<Vector2> uvs, List<int> triangles, float sizeMult = 1f, float startFraction = 0f)
	{
		float num = 0.5f * sizeMult;
		float num2 = 0.5f * num;
		Transform cameraTransform = Blocksworld.cameraTransform;
		Vector3 position = cameraTransform.position;
		Vector3 rhs = from - position;
		Vector3 lhs = to - from;
		Vector3 normalized = Vector3.Cross(lhs, rhs).normalized;
		int count = vertices.Count;
		vertices.Add(from - normalized * num2);
		uvs.Add(new Vector2(startFraction, 0f));
		vertices.Add(from + normalized * num2);
		uvs.Add(new Vector2(startFraction, 1f));
		vertices.Add(to - normalized * num2);
		uvs.Add(new Vector2(1f, 0f));
		vertices.Add(to + normalized * num2);
		uvs.Add(new Vector2(1f, 1f));
		for (int i = 0; i < triangleOffsets.Length; i++)
		{
			triangles.Add(count + triangleOffsets[i]);
		}
	}

	public static void AddHit(BlockAbstractLaser lb, Block block)
	{
		Hits.Add(block);
		bool flag = block.CanTriggerBlockListSensor();
		if (flag)
		{
			ModelHits.Add(block.modelBlock);
		}
		List<string> blockTags = TagManager.GetBlockTags(lb);
		for (int i = 0; i < blockTags.Count; i++)
		{
			string key = blockTags[i];
			HashSet<Block> hashSet;
			HashSet<Block> hashSet2;
			if (TagHits.ContainsKey(key))
			{
				hashSet = TagHits[key];
				hashSet2 = ModelTagHits[key];
			}
			else
			{
				hashSet = new HashSet<Block>();
				hashSet2 = new HashSet<Block>();
				TagHits.Add(key, hashSet);
				ModelTagHits.Add(key, hashSet2);
			}
			hashSet.Add(block);
			if (flag)
			{
				hashSet2.Add(block.modelBlock);
			}
		}
	}

	public static void AddProjectileHit(BlockAbstractLaser lb, Block block)
	{
		ProjectileHits.Add(block);
		bool flag = block.CanTriggerBlockListSensor();
		if (flag)
		{
			ModelProjectileHits.Add(block.modelBlock);
		}
		List<string> blockTags = TagManager.GetBlockTags(lb);
		for (int i = 0; i < blockTags.Count; i++)
		{
			string key = blockTags[i];
			HashSet<Block> hashSet;
			HashSet<Block> hashSet2;
			if (TagProjectileHits.ContainsKey(key))
			{
				hashSet = TagProjectileHits[key];
				hashSet2 = ModelTagProjectileHits[key];
			}
			else
			{
				hashSet = new HashSet<Block>();
				hashSet2 = new HashSet<Block>();
				TagProjectileHits.Add(key, hashSet);
				ModelTagProjectileHits.Add(key, hashSet2);
			}
			hashSet.Add(block);
			if (flag)
			{
				hashSet2.Add(block.modelBlock);
			}
		}
	}
}
