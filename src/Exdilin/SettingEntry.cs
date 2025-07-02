namespace Exdilin;

public class SettingEntry
{
	public Mod mod;

	public string value;

	public string defaultValue = string.Empty;

	public string id = string.Empty;

	public string label = string.Empty;

	public string _old;

	public string _valedit;

	public SettingEntry(string id, string label, string defaultValue)
	{
		this.id = id;
		this.label = label;
		this.defaultValue = defaultValue;
		value = defaultValue;
	}
}
