using UnityEngine;
using UnhollowerBaseLib;
using Reactor.Extensions;

namespace TownOfUsRework {
  using Roles;
  public static class Util {
    public static string ColorText(Color32 color, string text) => ColorText(
        string.Format("{0:X2}{1:X2}{2:X2}{3:X2}", new object[] { color.r, color.g, color.b, color.a }),
        text
      );
    public static string ColorText(string hex, string text) => $"<color=#{hex}>{text}</color>";

    public static int RandomInt(int min, int max) {
      return Random.RandomRangeInt(min, max + 1);
    }

    public static PlayerControl GetPlayer(byte playerId) {
      foreach (PlayerControl player in PlayerControl.AllPlayerControls) {
        if (player.PlayerId == playerId)
          return player;
      }
      return null;
    }

    public static void ChangeKillButtonState(PlayerControl player, bool enabled) => ChangeKillButtonState(
      RoleManager.GetRole(player),
      enabled
    );
    public static void ChangeKillButtonState(Role role, bool enabled) {
      if (role == null || role.AbilityButtons.Count == 0)
        return;
      foreach (KillButtonData data in role.AbilityButtons) {
        data.Button.gameObject.SetActive(enabled);
        data.Button.isActive = enabled;
        if (!enabled && data.Button.CurrentTarget != null) {
          data.Button.SetTarget(null);
        }
      }
    }

    public static KillButtonManager CreateAbilityButton(Role role, float maxCooldown = -1f) {
      KillButtonManager button = Object.Instantiate(HudManager.Instance.KillButton, HudManager.Instance.transform);
      KillButtonData data = new KillButtonData() {
        Button = button,
        MaxTimer = maxCooldown
      };
      role.AbilityButtons.Add(data);
      ChangeKillButtonState(role, true);
      return button;
    }

    public static PlayerControl GetClosestTarget() {
      PlayerControl
        localPlayer = PlayerControl.LocalPlayer,
        closestPlayer = null;

      float closestDistance = GameOptionsData.KillDistances[PlayerControl.GameOptions.KillDistance];
      Vector2 currentPosition = localPlayer.GetTruePosition();

      foreach (PlayerControl player in PlayerControl.AllPlayerControls) {
        GameData.PlayerInfo data = player.Data;
        if (
          data.Disconnected ||
          data.IsDead ||
          data.PlayerId == localPlayer.PlayerId
        ) continue;

        Vector2 playerPosition = player.GetTruePosition() - currentPosition;
        float distance = playerPosition.magnitude;
        if (distance > closestDistance || PhysicsHelpers.AnyNonTriggersBetween(
          currentPosition,
          playerPosition.normalized,
          distance,
          Constants.ShipAndObjectsMask
        )) continue;

        closestPlayer = player;
        closestDistance = distance;
      }

      return closestPlayer;
    }

    public static void KillPlayer(byte targetId, byte killerId) => KillPlayer(GetPlayer(targetId), GetPlayer(killerId));
    public static void KillPlayer(PlayerControl target, PlayerControl killer) {
      GameData.PlayerInfo targetData = target.Data;
      GameData.PlayerInfo killerData = killer.Data;

      if (
        targetData.IsDead || targetData.Disconnected ||
        killerData.IsDead || targetData.Disconnected
      ) return;

      target.gameObject.layer = LayerMask.NameToLayer("Ghost");

      if (killer.AmOwner && Constants.ShouldPlaySfx()) {
        SoundManager.Instance.PlaySound(killer.KillSfx, false, .8f);
      } else if (target.AmOwner) {
        HudManager hudManager = HudManager.Instance;
        hudManager.KillOverlay.ShowOne(killerData, targetData);
        hudManager.ShadowQuad.gameObject.SetActive(false);
        target.nameText.GetComponent<MeshRenderer>().material.SetInt("_Mask", 0);
        target.RpcSetScanner(false);
        ImportantTextTask importantTextTask = new GameObject("_Player").AddComponent<ImportantTextTask>();
        importantTextTask.transform.SetParent(target.transform, false);
        if (!PlayerControl.GameOptions.GhostsDoTasks) {
          target.ClearTasks();
          importantTextTask.Text = DestroyableSingleton<TranslationController>.Instance.GetString(
            StringNames.GhostIgnoreTasks,
            new Il2CppReferenceArray<Il2CppSystem.Object>(0)
          );
        } else {
          importantTextTask.Text = DestroyableSingleton<TranslationController>.Instance.GetString(
            StringNames.GhostDoTasks,
            new Il2CppReferenceArray<Il2CppSystem.Object>(0)
          );
        }
        target.myTasks.Insert(0, importantTextTask);
      }

      killer.MyPhysics.StartCoroutine(
        killer.KillAnimations.Random().CoPerformKill(killer, target)
      );
    }
  }
}
