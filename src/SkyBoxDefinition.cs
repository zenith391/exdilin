using System;
using UnityEngine;

[Serializable]
public class SkyBoxDefinition
{
	public string name;

	public string materialResourcePath;

	public string iOSMaterialResourcePath;

	public Vector3 lightRotation;

	public string fogColor = "White";

	public float fogDensity = 50f;

	public float fogStart = 200f;

	public string platformSpecificMaterialResourcePath => materialResourcePath;
}
