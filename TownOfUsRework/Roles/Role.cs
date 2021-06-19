using UnityEngine;
using HarmonyLib;
using Reactor.Extensions;
using System.Collections.Generic;

namespace TownOfUsRework.Roles {
  [HarmonyPatch(typeof(IntroCutscene))]
  public abstract class Role {
    public abstract RoleType RoleType { get; }
    public abstract string ImpostorText { get; }

    public string RoleName => RoleManager.GetRoleName(RoleType);
    public Color RoleColor => RoleManager.GetRoleColor(RoleType);
    public RoleTeam RoleTeam => RoleManager.GetRoleTeam(RoleType);
    public Color TeamColor => RoleManager.GetTeamColor(RoleTeam);

    public List<KillButtonData> AbilityButtons = new List<KillButtonData>();

    public PlayerControl player;
    public Role(PlayerControl player) {
      this.player = player;
    }

    [HarmonyPostfix()]
    [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.BeginCrewmate))]
    private static void IntroCrewmate(IntroCutscene __instance) => PatchIntroCutscene(__instance);
    [HarmonyPostfix()]
    [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.BeginImpostor))]
    private static void IntroImpostor(IntroCutscene __instance) => PatchIntroCutscene(__instance);

    public static void PatchIntroCutscene(IntroCutscene cutscene) {
      TOURework.LogMessage("Intro Cutscene Patch");
      PlayerControl localPlayer = PlayerControl.LocalPlayer;
      Role role = RoleManager.GetRole(localPlayer);

      if (role == null || role.RoleType == RoleType.Crewmate)
        return;

      string roleName = cutscene.Title.text = role.RoleName;
      string impostorText = role.ImpostorText;
      Color roleColor = cutscene.Title.color = cutscene.BackgroundBar.material.color = role.RoleColor;
      if (impostorText != null)
        cutscene.ImpostorText.text = impostorText;
      localPlayer.nameText.text += $"\n{roleName}";
      localPlayer.nameText.color = roleColor;
    }
  }
}
