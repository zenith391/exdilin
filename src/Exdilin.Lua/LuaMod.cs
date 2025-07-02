using System;
using System.IO;
using MoonSharp.Interpreter;

namespace Exdilin.Lua;

public class LuaMod : Mod
{
	private string _Name;

	private Version _Version;

	private bool _ChangesGameplay;

	private string _Id;

	private Script script;

	public override string Name => _Name;

	public override Version Version => _Version;

	public override string Id => _Id;

	public bool ChangesGameplay => _ChangesGameplay;

	public LuaMod(Script script, string path)
	{
		this.script = script;
		script.Globals["exdilin"] = UserData.Create(new ExdilinAPI(this));
		_Name = (string)script.Globals["name"];
		_Id = Path.GetFileName(path);
		_Version = new Version(1, 0, 0);
		if (exists(script.Globals, "changesGameplay"))
		{
			_ChangesGameplay = (bool)script.Globals["changesGameplay"];
		}
		else
		{
			_ChangesGameplay = true;
		}
	}

	private bool exists(Table table, string key)
	{
		return table.Get(key).IsNotNil();
	}

	public DynValue SafeCall(object fn, params object[] args)
	{
		try
		{
			return script.Call(fn, args);
		}
		catch (ScriptRuntimeException ex)
		{
			string text = "Runtime error in Lua mod " + Id + ": " + ex.DecoratedMessage;
			if (ModLoader.IsOverlayLoaded)
			{
				BWStandalone.Overlays.ShowMessage(text);
			}
			else
			{
				ModLoader.pendingErrorMsg.Add(text);
			}
			BWLog.Error(ex.Message + "\n" + ex.StackTrace);
			return DynValue.Nil;
		}
		catch (Exception ex2)
		{
			string text2 = "Unhandled runtime error in Lua mod " + Id + ": " + ex2.Message;
			if (ModLoader.IsOverlayLoaded)
			{
				BWStandalone.Overlays.ShowMessage(text2);
			}
			else
			{
				ModLoader.pendingErrorMsg.Add(text2);
			}
			BWLog.Error(ex2.Message + "\n" + ex2.StackTrace);
			return DynValue.Nil;
		}
	}

	public override void PreInit()
	{
		if (exists(script.Globals, "pre_init"))
		{
			SafeCall(script.Globals["pre_init"]);
		}
	}

	public override void Init()
	{
		if (exists(script.Globals, "init"))
		{
			SafeCall(script.Globals["init"]);
		}
	}

	public override void Register(RegisterType registerType)
	{
		if (exists(script.Globals, "register"))
		{
			SafeCall(script.Globals["register"], registerType.ToString().ToLower());
		}
	}
}
