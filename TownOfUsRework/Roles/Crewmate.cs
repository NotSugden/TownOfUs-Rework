using UnityEngine;

namespace TownOfUsRework.Roles {
  public class Crewmate : Role {
    public override RoleType RoleType => RoleType.Crewmate;
    public override string ImpostorText => null;

    public Crewmate(PlayerControl player) : base(player) { }
  }
}
