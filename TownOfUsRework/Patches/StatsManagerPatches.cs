using HarmonyLib;

namespace TownOfUsRework.Patches {
  [HarmonyPatch(typeof(StatsManager))]
  class StatsManagerPatches {
    [HarmonyPostfix()]
    [HarmonyPatch(nameof(StatsManager.AmBanned), MethodType.Getter)]
    public static void AmBanned(out bool __result) => __result = false;
  }
}
