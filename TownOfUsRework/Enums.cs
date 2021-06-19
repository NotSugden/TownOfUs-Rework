
namespace TownOfUsRework {
  public enum RPCMethod {
    Begin = 100,
    SetRole, // roleId, playerId

    SyncSetting, // settingId, newValue
    KillPlayer // targetId, killerId
  }
  public enum RoleTeam {
    Crew,
    Impostors,
    Neutral
  }
  public enum RoleType {
    None,
    Crewmate,
    Impostor,
    Sheriff
  }
  public enum OptionType {
    Header,
    Toggle,
    Number,
    String
  }
  public enum CustomOptionKey {
    RolePercentageHeader,
    Sheriff
  }
}
