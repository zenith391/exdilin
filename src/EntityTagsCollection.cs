using System;
using UnityEngine;

[Serializable]
public class EntityTagsCollection : MonoBehaviour
{
	public string[] allBlockTags;

	public string[] allPredicateTags;

	public string[] allTextureTags;

	public BlockTags[] blockTags;

	public PredicateTags[] predicateTags;

	public TextureTags[] textureTags;

	public GeneralModelCategoryScores[] categoryScores;
}
