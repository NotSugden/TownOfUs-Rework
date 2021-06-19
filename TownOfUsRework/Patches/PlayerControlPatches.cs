using HarmonyLib;
using UnityEngine;
using Hazel;

namespace TownOfUsRework.Patches {
  using Roles;
  using CustomOptions;

  [HarmonyPatch(typeof(PlayerControl))]
  class PlayerControlPatches {
    [HarmonyPostfix()]
    [HarmonyPatch(nameof(PlayerControl.FixedUpdate))]
    public static void FixedUpdatePatch(PlayerControl __instance) {
      if (ShipStatus.Instance == null || !__instance.AmOwner) return;
      GameData.PlayerInfo data = __instance.Data;
      if (data == null || data.IsImpostor || data.IsDead)
        return;
      Role role = RoleManager.GetRole(__instance);
      if (role == null)
        return;
      foreach (KillButtonData buttonData in role.AbilityButtons) {
        KillButtonManager button = buttonData.Button;
        button.SetTarget(Util.GetClosestTarget());
        if (!button.isCoolingDown)
          continue;
        float newCooldown = buttonData.Timer - Time.fixedDeltaTime;
        buttonData.Timer = Mathf.Clamp(newCooldown, 0f, buttonData.MaxTimer);
        button.SetCoolDown(buttonData.Timer, buttonData.MaxTimer);
      }
    }

    [HarmonyPostfix()]
    [HarmonyPatch(nameof(PlayerControl.Die))]
    public static void DiePatch(PlayerControl __instance) {
      if (!__instance.AmOwner)
        return;
      Util.ChangeKillButtonState(__instance, false);
    }

    [HarmonyPostfix()]
    [HarmonyPatch(nameof(PlayerControl.HandleRpc))]
    public static void HandleCustomRpc([HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader) {
      if (callId < 100)
        return;
      TOURework.LogMessage($"Recieved RPC {callId}");
      switch ((RPCMethod) callId) {
        case RPCMethod.Begin:
          RoleManager.Instance = new RoleManager();
          break;
        case RPCMethod.SetRole:
          RoleManager.Instance.InstantiateRole(
            (RoleType) reader.ReadByte(),
            Util.GetPlayer(reader.ReadByte())
          );
          break;
        case RPCMethod.SyncSetting:
          CustomOptionKey key = (CustomOptionKey) reader.ReadByte();
          GameOptions.SyncSetting(
            key,
            GameOptions.ReadOption(reader, key)
          );
          break;
        case RPCMethod.KillPlayer:
          Util.KillPlayer(
            reader.ReadByte(),
            reader.ReadByte()
          );
          break;
      }
    }

    [HarmonyPostfix()]
    [HarmonyPatch(nameof(PlayerControl.RpcSyncSettings))]
    public static void SyncSettings() {
      if (!AmongUsClient.Instance.AmHost)
        return;
      foreach ((CustomOptionKey key, GameOption option) in GameOptions.AllOptions) {
        RPCUtil.SyncSetting(key, option);
      }
    }
  }
}
