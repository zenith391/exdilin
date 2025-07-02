using System;

public class RarityLevel
{
	private RarityLevelEnum _enumValue;

	private string _stringValue;

	public RarityLevelEnum EnumValue => _enumValue;

	public string StringValue => _stringValue;

	public bool rare => _enumValue != RarityLevelEnum.common;

	public bool hasRarityBorder
	{
		get
		{
			if (_enumValue != RarityLevelEnum.common)
			{
				return _enumValue != RarityLevelEnum.uncommon;
			}
			return false;
		}
	}

	public RarityLevel(RarityLevelEnum enumValue)
	{
		_enumValue = enumValue;
		_stringValue = _enumValue.ToString().Replace('_', '-').ToLowerInvariant();
	}

	public RarityLevel(string rarityString)
	{
		_stringValue = rarityString.ToLowerInvariant();
		try
		{
			_enumValue = (RarityLevelEnum)Enum.Parse(typeof(RarityLevelEnum), _stringValue.Replace('-', '_'), ignoreCase: true);
		}
		catch
		{
			_enumValue = RarityLevelEnum.common;
		}
	}
}
