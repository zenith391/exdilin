using System.Collections.Generic;
using UnityEngine;

namespace Blocks;

public abstract class BlockAbstractBow : Block
{
	private class BlockBowArrow
	{
		private enum ArrowState
		{
			NONE,
			LOADING,
			DRAWING,
			FLYING,
			EMBEDDED,
			STOPPED
		}

		private GameObject _go;

		private Rigidbody _rb;

		private Collider _collider;

		private Quaternion _arrowRotation = Quaternion.Euler(-180f, -180f, 90f);

		private RaycastHit _hit;

		private BlockAbstractBow _sourceBow;

		private Block _hitBlock;

		private float _lifetimeInFlight = 20f;

		private float _lifetimeAfterHit = 5f;

		private ArrowState _state;

		public BlockBowArrow(BlockAbstractBow source)
		{
			_state = ArrowState.LOADING;
			_sourceBow = source;
			_go = Object.Instantiate(Resources.Load(_sourceBow.GetAmmoPrefabName())) as GameObject;
			_collider = _go.GetComponent<Collider>();
			_go.transform.position = _sourceBow.goT.position + _sourceBow.goT.rotation * _sourceBow.GetAmmoPositionOffset();
			_go.transform.rotation = _sourceBow.goT.rotation * _arrowRotation;
			_go.transform.parent = _sourceBow.goT.parent;
			MeshFilter[] componentsInChildren = _go.GetComponentsInChildren<MeshFilter>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				GameObject gameObject = componentsInChildren[i].gameObject;
				int ammoSubmeshColorMapping = _sourceBow.GetAmmoSubmeshColorMapping(i);
				if (ammoSubmeshColorMapping >= 0)
				{
					Materials.SetMaterial(gameObject, componentsInChildren[i].mesh, gameObject.name, _sourceBow.GetPaint(ammoSubmeshColorMapping), _sourceBow.GetTexture(ammoSubmeshColorMapping), Vector3.one, _sourceBow.GetScale(), string.Empty);
				}
			}
		}

		private void AlignToDirection(Vector3 direction)
		{
			_go.transform.up = -direction;
		}

		public void Destroy()
		{
			Object.Destroy(_go);
			_go = null;
			_rb = null;
			_collider = null;
			_sourceBow = null;
			_hitBlock = null;
		}

		public bool IsReadyForCleanup()
		{
			return _state == ArrowState.NONE;
		}

		public bool IsReadyToShoot()
		{
			if (_state != ArrowState.LOADING)
			{
				return _state == ArrowState.DRAWING;
			}
			return true;
		}

		public void Shoot(Vector3 direction, float force)
		{
			_rb = _go.AddComponent<Rigidbody>();
			_rb.mass = 0.25f;
			_rb.useGravity = _sourceBow.ShotsObeyGravity();
			_rb.AddForce(force * direction, ForceMode.VelocityChange);
			_go.transform.parent = null;
			_state = ArrowState.FLYING;
		}

		public void FixedUpdate()
		{
			switch (_state)
			{
			case ArrowState.FLYING:
			{
				_lifetimeInFlight -= Time.fixedDeltaTime;
				if (_lifetimeInFlight <= 0f)
				{
					_state = ArrowState.NONE;
					break;
				}
				Vector3 normalized = _rb.velocity.normalized;
				float magnitude = _rb.velocity.magnitude;
				float num = magnitude * Time.fixedDeltaTime;
				float maxDistance = num * 1.75f;
				Vector3 origin = _go.transform.position - normalized * num * 0.5f;
				AlignToDirection(normalized);
				if (!Physics.Raycast(origin, normalized, out _hit, maxDistance))
				{
					break;
				}
				_hitBlock = BWSceneManager.FindBlock(_hit.collider.gameObject);
				bool flag = _hitBlock is BlockAnimatedCharacter || _hitBlock is BlockCharacter;
				if (_sourceBow.ArrowHit(_hitBlock, _hit.point, normalized) && !flag)
				{
					_state = ArrowState.EMBEDDED;
					_go.transform.parent = _hit.collider.transform;
					Object.DestroyImmediate(_rb);
					_rb = null;
					_go.transform.position = _hit.point - normalized * Random.value * 0.1f;
					if (_hitBlock is BlockAbstractWater)
					{
						_lifetimeAfterHit = 0f;
					}
				}
				else if (_hitBlock != null && _hitBlock.BlockType().EndsWith("Character Skeleton"))
				{
					_rb.velocity = magnitude * 0.2f * Random.onUnitSphere;
				}
				else
				{
					_state = ArrowState.STOPPED;
					_go.transform.position = _hit.point;
					_rb.velocity = Vector3.zero;
					_collider.enabled = true;
				}
				break;
			}
			case ArrowState.EMBEDDED:
			case ArrowState.STOPPED:
				_lifetimeAfterHit -= Time.fixedDeltaTime;
				if (_lifetimeAfterHit <= 0f)
				{
					_state = ArrowState.NONE;
				}
				break;
			}
		}
	}

	public static HashSet<Block> Hits = new HashSet<Block>();

	public static HashSet<Block> ModelHits = new HashSet<Block>();

	public static Dictionary<string, HashSet<Block>> TagHits = new Dictionary<string, HashSet<Block>>();

	public static Dictionary<string, HashSet<Block>> ModelTagHits = new Dictionary<string, HashSet<Block>>();

	private List<BlockBowArrow> _arrows = new List<BlockBowArrow>();

	private const float DEFAULT_SHOT_FORCE = 50f;

	private float _timeBetweenShots;

	private bool _shotSignaled;

	private float _shotForce = 50f;

	public BlockAbstractBow(List<List<Tile>> tiles)
		: base(tiles)
	{
	}

	protected abstract Vector3 GetAmmoPositionOffset();

	protected abstract string GetAmmoPrefabName();

	protected virtual int GetAmmoSubmeshColorMapping(int i)
	{
		return -1;
	}

	protected virtual Vector3 GetFiringDirection()
	{
		return -goT.right;
	}

	protected abstract float GetMinTimeBetweenShots();

	protected virtual bool ShotsObeyGravity()
	{
		return true;
	}

	protected abstract string GetSFXForBlocked();

	protected abstract string GetSFXForHit();

	protected virtual string GetSFXForHitWater()
	{
		return "Water Splash Medium " + Random.Range(1, 4);
	}

	protected abstract string GetSFXForLoad();

	protected abstract string GetSFXForShoot();

	protected virtual bool HasSFXForLoad()
	{
		return true;
	}

	protected virtual void BowLoaded()
	{
	}

	protected virtual void BowShot()
	{
	}

	private void DestroyArrows()
	{
		for (int i = 0; i < _arrows.Count; i++)
		{
			_arrows[i].Destroy();
		}
		_arrows.Clear();
	}

	public override void Destroy()
	{
		DestroyArrows();
		base.Destroy();
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		for (int i = 0; i < _arrows.Count; i++)
		{
			_arrows[i].FixedUpdate();
		}
		if (_timeBetweenShots > 0f)
		{
			_timeBetweenShots -= Time.fixedDeltaTime;
		}
		if (_shotSignaled && _timeBetweenShots <= 0f)
		{
			Vector3 direction = GetFiringDirection();
			BlockAnimatedCharacter blockAnimatedCharacter = BlockAnimatedCharacter.FindBlockOwner(this);
			if (blockAnimatedCharacter != null && Vector3.Dot(blockAnimatedCharacter.goT.up, Vector3.up) > 0.95f)
			{
				direction.y = 0f;
				Vector3 vector = ((!ShotsObeyGravity()) ? Vector3.zero : (-0.005f * Physics.gravity));
				direction = (direction.normalized + vector).normalized;
			}
			_arrows[0].Shoot(direction, _shotForce);
			_shotSignaled = false;
			_timeBetweenShots = GetMinTimeBetweenShots();
			PlayPositionedSound(GetSFXForShoot(), 0.4f);
			BowShot();
		}
		for (int num = _arrows.Count - 1; num >= 0; num--)
		{
			if (_arrows[num].IsReadyForCleanup())
			{
				_arrows[num].Destroy();
				_arrows.RemoveAt(num);
				break;
			}
		}
	}

	public override void Stop(bool resetBlock = true)
	{
		base.Stop(resetBlock);
		DestroyArrows();
	}

	public TileResultCode IsLoaded(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (_arrows.Count > 0 && _arrows[0].IsReadyToShoot())
		{
			return TileResultCode.True;
		}
		return TileResultCode.False;
	}

	public TileResultCode Load(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (_timeBetweenShots > 0f)
		{
			return TileResultCode.Delayed;
		}
		if (_arrows.Count == 0 || !_arrows[0].IsReadyToShoot())
		{
			_arrows.Insert(0, new BlockBowArrow(this));
			if (HasSFXForLoad())
			{
				PlayPositionedSound(GetSFXForLoad(), 0.4f);
			}
			BowLoaded();
		}
		return TileResultCode.True;
	}

	public TileResultCode Shoot(ScriptRowExecutionInfo eInfo, object[] args)
	{
		if (_timeBetweenShots > 0f)
		{
			return TileResultCode.True;
		}
		if (broken || vanished || _arrows.Count == 0 || !_arrows[0].IsReadyToShoot())
		{
			return TileResultCode.False;
		}
		_shotForce = Util.GetFloatArg(args, 0, 50f);
		_shotSignaled = true;
		return TileResultCode.Delayed;
	}

	internal Vector3 LoadingArrowPosition()
	{
		return goT.position;
	}

	internal Quaternion LoadingArrowRotation()
	{
		return goT.rotation;
	}

	internal static void ClearHits()
	{
		Hits.Clear();
		ModelHits.Clear();
		ClearSets(TagHits);
		ClearSets(ModelTagHits);
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

	internal bool ArrowHit(Block block, Vector3 position, Vector3 direction)
	{
		if (block != null)
		{
			Hits.Add(block);
			bool flag = block.CanTriggerBlockListSensor();
			if (flag)
			{
				ModelHits.Add(block.modelBlock);
			}
			List<string> blockTags = TagManager.GetBlockTags(this);
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
			if (block.chunk.rb != null)
			{
				block.chunk.rb.AddForceAtPosition(0.1f * _shotForce * direction, position, ForceMode.Impulse);
			}
			bool flag2 = Invincibility.IsInvincible(block);
			bool flag3 = block is BlockAbstractWater;
			if (flag2)
			{
				block.PlayPositionedSound(GetSFXForBlocked(), 0.2f);
			}
			else if (flag3)
			{
				block.PlayPositionedSound(GetSFXForHitWater(), 0.2f);
			}
			else
			{
				block.PlayPositionedSound(GetSFXForHit(), 0.2f);
			}
			return !flag2;
		}
		return false;
	}

	internal static bool IsHitByArrow(Block block)
	{
		return Hits.Contains(block);
	}

	internal static bool IsHitByArrow(List<Block> blocks)
	{
		for (int i = 0; i < blocks.Count; i++)
		{
			if (IsHitByArrow(blocks[i]))
			{
				return true;
			}
		}
		return false;
	}

	internal static bool IsHitByArrowModel(Block modelBlock)
	{
		return ModelHits.Contains(modelBlock);
	}

	internal static bool IsHitByTaggedArrow(Block block, string tag)
	{
		if (TagHits.TryGetValue(tag, out var value))
		{
			return value.Contains(block);
		}
		return false;
	}

	internal static bool IsHitByTaggedArrow(List<Block> blocks, string tag)
	{
		if (TagHits.TryGetValue(tag, out var value))
		{
			for (int i = 0; i < blocks.Count; i++)
			{
				if (value.Contains(blocks[i]))
				{
					return true;
				}
			}
		}
		return false;
	}

	internal static bool IsHitByTaggedArrowModel(Block modelBlock, string tag)
	{
		if (ModelTagHits.TryGetValue(tag, out var value))
		{
			return value.Contains(modelBlock);
		}
		return false;
	}
}
