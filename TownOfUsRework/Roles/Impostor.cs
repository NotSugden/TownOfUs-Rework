using UnityEngine;

namespace TownOfUsRework.Roles {
  public class Impostor : Role {
    public override RoleType RoleType => RoleType.Impostor;
    public override string ImpostorText => null;

    public Impostor(PlayerControl player) : base(player) { }
  }
}
