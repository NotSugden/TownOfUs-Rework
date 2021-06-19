using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace TownOfUsRework.Roles {
  using CustomOptions;
  [HarmonyPatch(typeof(PlayerControl))]
  class RoleManager {
    public static RoleManager Instance = null;

    private readonly Dictionary<byte, Role> RoleDictionary = new Dictionary<byte, Role>();

    public static Dictionary<RoleType, int> RoleChances => new Dictionary<RoleType, int>() {
      { RoleType.Sheriff, GameOptions.GetOptionInt(CustomOptionKey.Sheriff) }
    };

    public static List<RoleType> EnabledRoles {
      get {
        List<RoleType> enabled = new List<RoleType>();
        foreach ((RoleType roleType, int probability) in RoleChances) {
          if (probability > 0)
            enabled.Add(roleType);
        }
        return enabled;
      }
    }

    public static Role GetRole(PlayerControl player) {
      return GetRole<Role>(player.PlayerId);
    }
    public static T GetRole<T>(PlayerControl player) where T : Role {
      return GetRole<T>(player.PlayerId);
    }
    public static Role GetRole(byte playerId) {
      return GetRole<Role>(playerId);
    }
    public static T GetRole<T>(byte playerId) where T : Role {
      return Instance.RoleDictionary.ContainsKey(playerId)
        ? (T) Instance.RoleDictionary[playerId]
        : null;
    }

    public static bool CheckProbability(int probability) {
      if (probability == 0)
        return false;
      else if (probability == 100)
        return true;
      int randomInt = Util.RandomInt(1, 100);
      return randomInt <= probability;
    }

    public static Type GetRoleClass(RoleType type) => type switch {
      RoleType.Crewmate => typeof(Crewmate),
      RoleType.Impostor => typeof(Impostor),
      RoleType.Sheriff => typeof(Sheriff),
      _ => typeof(Crewmate)
    };

    public static RoleTeam GetRoleTeam(RoleType type) => type switch {
      RoleType.Impostor => RoleTeam.Impostors,
      _ => RoleTeam.Crew
    };

    public static Color GetTeamColor(RoleTeam team) => team switch {
      RoleTeam.Crew => Color.white,
      RoleTeam.Impostors => Palette.ImpostorRed,
      RoleTeam.Neutral => Color.grey,
      _ => Color.white
    };

    public static Color GetRoleColor(RoleType type) => type switch {
      RoleType.Impostor => Palette.ImpostorRed,
      RoleType.Sheriff => Color.yellow,
      _ => Color.white
    };

    public static string GetRoleName(RoleType type) => Enum.GetName(typeof(RoleType), type);

    public (PlayerControl, Role) InstantiateRole(RoleType roleType, List<PlayerControl> players) {
      return InstantiateRole(GetRoleClass(roleType), players);
    }
    public (PlayerControl, Role) InstantiateRole(Type roleClass, List<PlayerControl> players) {
      if (players.Count == 0)
        return (null, null);
      PlayerControl player = players[Util.RandomInt(0, players.Count - 1)];
      return InstantiateRole(roleClass, player);
    }
    public (PlayerControl, Role) InstantiateRole(RoleType roleType, PlayerControl player) {
      return InstantiateRole(GetRoleClass(roleType), player);
    }
    public (PlayerControl, Role) InstantiateRole(Type roleClass, PlayerControl player) {
      if (RoleDictionary.ContainsKey(player.PlayerId))
        RoleDictionary.Remove(player.PlayerId);
      Role role = (Role) Activator.CreateInstance(roleClass, new object[] { player });
      RoleDictionary.Add(player.PlayerId, role);
      return (player, role);
    }

    private void RPCSetRole(byte playerId, RoleType roleType) {
      TOURework.LogMessage($"Sending RPC.SetRole {playerId} {roleType}");
      RPCUtil.Send(RPCMethod.SetRole, new object[] { (byte) roleType, playerId });
    }

    private void GiveRoles(List<RoleType> roleTypes, List<PlayerControl> players, RoleType defaultType) {
      int i = 0;

      while (i < players.Count) {
        if (roleTypes.Count == 0)
          break;
        RoleType roleType = roleTypes.Random();
        roleTypes.Remove(roleType);
        PlayerControl player = players[i];
        InstantiateRole(roleType, player);
        RPCSetRole(player.PlayerId, roleType);
        i++;
      }

      while (i < players.Count) {
        PlayerControl player = players[i];
        InstantiateRole(defaultType, player);
        RPCSetRole(player.PlayerId, defaultType);
        i++;
      }
    }

    public void GiveRoles() {
      RoleDictionary.Clear();

      List<RoleType>
        crewAndNeutralRoles = new List<RoleType>(),
        impostorRoles = new List<RoleType>();
      
      foreach ((RoleType type, int probability) in RoleChances.Shuffle()) {
        if (!CheckProbability(probability))
          return;
        RoleTeam team = GetRoleTeam(type);
        switch (team) {
          case RoleTeam.Crew:
          case RoleTeam.Neutral:
            crewAndNeutralRoles.Add(type);
            break;
          case RoleTeam.Impostors:
            impostorRoles.Add(type);
            break;
        }
      }

      crewAndNeutralRoles = crewAndNeutralRoles.Shuffle();
      impostorRoles = impostorRoles.Shuffle();

      foreach (PlayerControl player in PlayerControl.AllPlayerControls) {
        TOURework.LogMessage($"Giving a Role to {player.name}");
        RoleType defaultRole;
        List<RoleType> roles;
        if (player.Data.IsImpostor) {
          defaultRole = RoleType.Impostor;
          roles = impostorRoles;
        } else {
          defaultRole = RoleType.Crewmate;
          roles = crewAndNeutralRoles;
        }

        if (roles.Count == 0) {
          InstantiateRole(defaultRole, player);
          RPCSetRole(player.PlayerId, defaultRole);
          TOURework.LogMessage($"Given Crewmate to {player.name}");
        } else {
          RoleType roleType = roles.Random();
          roles.Remove(roleType);
          (_, Role role) = InstantiateRole(roleType, player);
          RPCSetRole(player.PlayerId, roleType);
          TOURework.LogMessage($"Given {role.RoleName} to {player.name}");
        }
      }
    }

    [HarmonyPostfix()]
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSetInfected))]
    public static void RpcSetInfected() {
      RoleManager manager = Instance = new RoleManager();
      RPCUtil.Send(RPCMethod.Begin);

      manager.GiveRoles();
    }
  }
}
