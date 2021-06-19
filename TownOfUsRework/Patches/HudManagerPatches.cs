using HarmonyLib;

namespace TownOfUsRework.Patches {
  [HarmonyPatch(typeof(HudManager))]
  class HudManagerPatches {
    [HarmonyPostfix()]
    [HarmonyPatch(nameof(HudManager.SetHudActive))]
    public static void SetHudActivePatch([HarmonyArgument(0)] bool isActive) {
      PlayerControl localPlayer = PlayerControl.LocalPlayer;
      if (localPlayer.Data.IsDead)
        return;
      Util.ChangeKillButtonState(localPlayer, isActive);
    }
  }
}
