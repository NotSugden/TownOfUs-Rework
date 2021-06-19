using BepInEx;
using BepInEx.IL2CPP;
using HarmonyLib;
using Reactor;

namespace TownOfUsRework {
  [BepInPlugin(Id, "TownOfUsRework", Version)]
  [BepInDependency(ReactorPlugin.Id)]
  public class TOURework : BasePlugin {
    public const string Id = "cf.sugden.townofusrework";
    public const string Version = "1.0.0-dev";

    public static void LogMessage(params string[] messages) {
      foreach (string message in messages)
        PluginSingleton<TOURework>.Instance.Log.LogMessage(message);
    }

    public Harmony Harmony = new Harmony(Id);

    public override void Load() {
      this.Harmony.PatchAll();
    }
  }
}
