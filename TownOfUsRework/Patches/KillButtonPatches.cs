using HarmonyLib;
using UnityEngine;

namespace TownOfUsRework.Patches {
  using Roles;

  [HarmonyPatch(typeof(KillButtonManager))]
  public static class KillButtonPatches {
    [HarmonyPostfix()]
    [HarmonyPatch(nameof(KillButtonManager.SetTarget))]
    public static void SetTarget([HarmonyArgument(0)] PlayerControl target) {
      if (target == null)
        return;
      Role role = RoleManager.GetRole(PlayerControl.LocalPlayer);
      if (role == null)
        return;
      Material material = target.GetComponent<SpriteRenderer>().material;
      material.SetColor("_OutlineColor", role.RoleColor);
    }

    private static void ResetCooldown(KillButtonData data) => data.Timer = data.MaxTimer;

    [HarmonyPrefix()]
    [HarmonyPatch(nameof(KillButtonManager.PerformKill))]
    public static bool PerformKillPatch(KillButtonManager __instance) {
      if (__instance.isCoolingDown)
        return false;
      PlayerControl localPlayer = PlayerControl.LocalPlayer;
      Role role = RoleManager.GetRole(localPlayer);
      if (role == null)
        return localPlayer.Data.IsImpostor;
      PlayerControl target = __instance.CurrentTarget;
      if (target == null)
        return false;
      switch (role.RoleType) {
        case RoleType.Sheriff:
          if (!target.Data.IsImpostor) {
            target = localPlayer;
          } else {
            ResetCooldown(role.AbilityButtons[0]);
          }

          RPCUtil.KillPlayer(target, localPlayer);
          break;
      }
      return localPlayer.Data?.IsImpostor ?? role is Impostor;
    }
  }
}
