using System;
using UnityEngine;

[Serializable]
public class TextureMetaData
{
	public string name;

	public Vector3 preferredSize;

	public bool fourSidesIgnoreRightLeft = true;

	public bool twoSidesMirror = true;

	public float mipMapBias;

	public TextureApplicationChangeRule[] applicationRules;
}
