using Hazel;

namespace TownOfUsRework {
  using CustomOptions;
  class RPCUtil {
    public static void Send(
      RPCMethod method, object[] parameters = null
    ) {
      TOURework.LogMessage($"Sending RPC: {method} - Params {(parameters == null ? "None" : parameters.Map(x => x.ToString(), ", "))}");
      MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(
        PlayerControl.LocalPlayer.NetId,
        (byte) method,
        SendOption.Reliable,
        -1
      );

      if (parameters != null) {
        foreach (object parameter in parameters) {
          if (parameter is byte @byte)
            writer.Write(@byte);
          else if (parameter is bool @bool)
            writer.Write(@bool);
          else if (parameter is int @int)
            writer.Write(@int);
          else if (parameter is float @float)
            writer.Write(@float);
        }
      }

      AmongUsClient.Instance.FinishRpcImmediately(writer);
    }

    public static void KillPlayer(PlayerControl target, PlayerControl killer) {
      Util.KillPlayer(target, killer);
      Send(RPCMethod.KillPlayer, new object[] { target.PlayerId, killer.PlayerId });
    }

    public static void SyncSetting(CustomOptionKey key, GameOption option) {
      if (option.CurrentValue != null) Send(RPCMethod.SyncSetting, new object[] { key, option.CurrentValue });
    }
  }
}
