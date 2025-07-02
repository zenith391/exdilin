using System;
using UnityEngine;

[Serializable]
public class BlockMeshMetaData
{
	public string defaultPaint;

	public string defaultTexture;

	public bool canBeTextured;

	public bool canBeMaterialTextured = true;

	public Vector3 defaultTextureNormal;

	public TextureSideRule[] textureSideRules;
}
