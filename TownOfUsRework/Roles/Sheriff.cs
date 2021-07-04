using UnityEngine;

namespace TownOfUsRework.Roles {
  using Patches;
  class Sheriff : Role {
    public override RoleType RoleType => RoleType.Sheriff;
    public override string ImpostorText => "Kill the Impostors";

    public KillButtonManager KillButton;

    public Sheriff(PlayerControl player) : base(player) {
      if (!player.AmOwner)
        return;
      KillButton = Util.CreateAbilityButton(this);
    }
  }
}
