using System;
using UnityEngine;

// Token: 0x020001B3 RID: 435
public class LerpVector3
{
	// Token: 0x060017AF RID: 6063 RVA: 0x000A6CB6 File Offset: 0x000A50B6
	public LerpVector3(float x, float y, float z, float alpha)
	{
		this._head = new Vector3(x, y, z);
		this._tail = new Vector3(x, y, z);
		this._alpha = Mathf.Clamp(alpha, 0f, 1f);
	}

	// Token: 0x060017B0 RID: 6064 RVA: 0x000A6CF1 File Offset: 0x000A50F1
	public LerpVector3(LerpVector3 source)
	{
		this._head = source._head;
		this._tail = source._tail;
		this._alpha = source._alpha;
	}

	// Token: 0x1700006F RID: 111
	// (get) Token: 0x060017B1 RID: 6065 RVA: 0x000A6D1D File Offset: 0x000A511D
	public Vector3 tail
	{
		get
		{
			return this._tail;
		}
	}

	// Token: 0x17000070 RID: 112
	// (get) Token: 0x060017B2 RID: 6066 RVA: 0x000A6D25 File Offset: 0x000A5125
	// (set) Token: 0x060017B3 RID: 6067 RVA: 0x000A6D2D File Offset: 0x000A512D
	public float alpha
	{
		get
		{
			return this._alpha;
		}
		set
		{
			this._alpha = Mathf.Clamp(value, 0f, 1f);
		}
	}

	// Token: 0x060017B4 RID: 6068 RVA: 0x000A6D45 File Offset: 0x000A5145
	public void Copy(LerpVector3 source)
	{
		this._head = source._head;
		this._tail = source._tail;
		this._alpha = source._alpha;
	}

	// Token: 0x060017B5 RID: 6069 RVA: 0x000A6D6B File Offset: 0x000A516B
	public void OffsetHead(Vector3 offset)
	{
		this._head += offset;
	}

	// Token: 0x060017B6 RID: 6070 RVA: 0x000A6D7F File Offset: 0x000A517F
	public void Update()
	{
		this._tail = Vector3.Lerp(this._tail, this._head, this._alpha);
	}

	// Token: 0x060017B7 RID: 6071 RVA: 0x000A6D9E File Offset: 0x000A519E
	public void Set(float x, float y, float z)
	{
		this._head.Set(x, y, z);
		this._tail.Set(x, y, z);
	}

	// Token: 0x060017B8 RID: 6072 RVA: 0x000A6DBC File Offset: 0x000A51BC
	public void Set(float x, float y, float z, float alpha)
	{
		this._head.Set(x, y, z);
		this._tail.Set(x, y, z);
		this._alpha = Mathf.Clamp(alpha, 0f, 1f);
	}

	// Token: 0x060017B9 RID: 6073 RVA: 0x000A6DF1 File Offset: 0x000A51F1
	public void Set(Vector3 position)
	{
		this._head = position;
		this._tail = position;
	}

	// Token: 0x060017BA RID: 6074 RVA: 0x000A6E01 File Offset: 0x000A5201
	public void Set(Vector3 position, float alpha)
	{
		this._head = position;
		this._tail = position;
		this._alpha = alpha;
	}

	// Token: 0x060017BB RID: 6075 RVA: 0x000A6E18 File Offset: 0x000A5218
	public Vector3 DebugGetHead()
	{
		return this._head;
	}

	// Token: 0x060017BC RID: 6076 RVA: 0x000A6E20 File Offset: 0x000A5220
	public Vector3 DebugGetTail()
	{
		return this._tail;
	}

	// Token: 0x0400129C RID: 4764
	private Vector3 _head;

	// Token: 0x0400129D RID: 4765
	private Vector3 _tail;

	// Token: 0x0400129E RID: 4766
	private float _alpha;
}
