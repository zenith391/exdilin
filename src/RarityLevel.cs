using System;

// Token: 0x02000273 RID: 627
public class RarityLevel
{
	// Token: 0x06001D30 RID: 7472 RVA: 0x000CE19C File Offset: 0x000CC59C
	public RarityLevel(RarityLevelEnum enumValue)
	{
		this._enumValue = enumValue;
		this._stringValue = this._enumValue.ToString().Replace('_', '-').ToLowerInvariant();
	}

	// Token: 0x06001D31 RID: 7473 RVA: 0x000CE1D0 File Offset: 0x000CC5D0
	public RarityLevel(string rarityString)
	{
		this._stringValue = rarityString.ToLowerInvariant();
		try
		{
			this._enumValue = (RarityLevelEnum)Enum.Parse(typeof(RarityLevelEnum), this._stringValue.Replace('-', '_'), true);
		}
		catch
		{
			this._enumValue = RarityLevelEnum.common;
		}
	}

	// Token: 0x17000135 RID: 309
	// (get) Token: 0x06001D32 RID: 7474 RVA: 0x000CE23C File Offset: 0x000CC63C
	public RarityLevelEnum EnumValue
	{
		get
		{
			return this._enumValue;
		}
	}

	// Token: 0x17000136 RID: 310
	// (get) Token: 0x06001D33 RID: 7475 RVA: 0x000CE244 File Offset: 0x000CC644
	public string StringValue
	{
		get
		{
			return this._stringValue;
		}
	}

	// Token: 0x17000137 RID: 311
	// (get) Token: 0x06001D34 RID: 7476 RVA: 0x000CE24C File Offset: 0x000CC64C
	public bool rare
	{
		get
		{
			return this._enumValue != RarityLevelEnum.common;
		}
	}

	// Token: 0x17000138 RID: 312
	// (get) Token: 0x06001D35 RID: 7477 RVA: 0x000CE25A File Offset: 0x000CC65A
	public bool hasRarityBorder
	{
		get
		{
			return this._enumValue != RarityLevelEnum.common && this._enumValue != RarityLevelEnum.uncommon;
		}
	}

	// Token: 0x040017D7 RID: 6103
	private RarityLevelEnum _enumValue;

	// Token: 0x040017D8 RID: 6104
	private string _stringValue;
}
