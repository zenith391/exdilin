using System;
using System.Collections.Generic;

[Serializable]
public abstract class EntityTags
{
	public EntityTagsNameSelectMode selectMode;

	public string name;

	public string nameRegexp;

	public string tagsString;

	public string[] tags;

	[NonSerialized]
	public List<string> regexMatches;

	[NonSerialized]
	public string regexError;

	[NonSerialized]
	public List<string> messages = new List<string>();

	public void RemoveTag(string tag)
	{
		if (tags != null)
		{
			List<string> list = new List<string>(tags);
			list.Remove(tag);
			tags = list.ToArray();
		}
	}

	public void RenameTag(string oldName, string newName)
	{
		if (tags != null)
		{
			List<string> list = new List<string>(tags);
			int num = list.IndexOf(oldName);
			if (num >= 0)
			{
				list[num] = newName;
			}
			tags = list.ToArray();
		}
	}
}
