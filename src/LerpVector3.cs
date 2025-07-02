using UnityEngine;

public class LerpVector3
{
	private Vector3 _head;

	private Vector3 _tail;

	private float _alpha;

	public Vector3 tail => _tail;

	public float alpha
	{
		get
		{
			return _alpha;
		}
		set
		{
			_alpha = Mathf.Clamp(value, 0f, 1f);
		}
	}

	public LerpVector3(float x, float y, float z, float alpha)
	{
		_head = new Vector3(x, y, z);
		_tail = new Vector3(x, y, z);
		_alpha = Mathf.Clamp(alpha, 0f, 1f);
	}

	public LerpVector3(LerpVector3 source)
	{
		_head = source._head;
		_tail = source._tail;
		_alpha = source._alpha;
	}

	public void Copy(LerpVector3 source)
	{
		_head = source._head;
		_tail = source._tail;
		_alpha = source._alpha;
	}

	public void OffsetHead(Vector3 offset)
	{
		_head += offset;
	}

	public void Update()
	{
		_tail = Vector3.Lerp(_tail, _head, _alpha);
	}

	public void Set(float x, float y, float z)
	{
		_head.Set(x, y, z);
		_tail.Set(x, y, z);
	}

	public void Set(float x, float y, float z, float alpha)
	{
		_head.Set(x, y, z);
		_tail.Set(x, y, z);
		_alpha = Mathf.Clamp(alpha, 0f, 1f);
	}

	public void Set(Vector3 position)
	{
		_head = position;
		_tail = position;
	}

	public void Set(Vector3 position, float alpha)
	{
		_head = position;
		_tail = position;
		_alpha = alpha;
	}

	public Vector3 DebugGetHead()
	{
		return _head;
	}

	public Vector3 DebugGetTail()
	{
		return _tail;
	}
}
