using System;
using System.Collections.Generic;
using Blocks;

// Token: 0x02000331 RID: 817
public class BlockNameComparer<T> : IComparer<T> where T : Block
{
	// Token: 0x06002522 RID: 9506 RVA: 0x0010F117 File Offset: 0x0010D517
	public int Compare(T a, T b)
	{
		return a.go.name.CompareTo(b.go.name);
	}
}
