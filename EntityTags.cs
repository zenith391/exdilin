using System;
using System.Collections.Generic;

// Token: 0x02000203 RID: 515
[Serializable]
public abstract class EntityTags
{
	// Token: 0x06001A28 RID: 6696 RVA: 0x000C15FC File Offset: 0x000BF9FC
	public void RemoveTag(string tag)
	{
		if (this.tags != null)
		{
			List<string> list = new List<string>(this.tags);
			list.Remove(tag);
			this.tags = list.ToArray();
		}
	}

	// Token: 0x06001A29 RID: 6697 RVA: 0x000C1634 File Offset: 0x000BFA34
	public void RenameTag(string oldName, string newName)
	{
		if (this.tags != null)
		{
			List<string> list = new List<string>(this.tags);
			int num = list.IndexOf(oldName);
			if (num >= 0)
			{
				list[num] = newName;
			}
			this.tags = list.ToArray();
		}
	}

	// Token: 0x040015BF RID: 5567
	public EntityTagsNameSelectMode selectMode;

	// Token: 0x040015C0 RID: 5568
	public string name;

	// Token: 0x040015C1 RID: 5569
	public string nameRegexp;

	// Token: 0x040015C2 RID: 5570
	public string tagsString;

	// Token: 0x040015C3 RID: 5571
	public string[] tags;

	// Token: 0x040015C4 RID: 5572
	[NonSerialized]
	public List<string> regexMatches;

	// Token: 0x040015C5 RID: 5573
	[NonSerialized]
	public string regexError;

	// Token: 0x040015C6 RID: 5574
	[NonSerialized]
	public List<string> messages = new List<string>();
}
