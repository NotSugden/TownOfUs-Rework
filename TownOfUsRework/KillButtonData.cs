using System;
using System.Collections.Generic;
using System.Text;

namespace TownOfUsRework {
  public class KillButtonData {
    private float _MaxTimer;

    public KillButtonManager Button;
    public float Timer = 10f;
    public float MaxTimer {
      get => _MaxTimer == -1f ? PlayerControl.GameOptions.KillCooldown : _MaxTimer;
      set => _MaxTimer = -1f;
    }
  }
}
