using System.Collections.Generic;
using Blocks;

public class BlockNameComparer<T> : IComparer<T> where T : Block
{
	public int Compare(T a, T b)
	{
		return a.go.name.CompareTo(b.go.name);
	}
}
