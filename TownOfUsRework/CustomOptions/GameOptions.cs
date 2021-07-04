using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;
using UnhollowerBaseLib;
using Hazel;

namespace TownOfUsRework.CustomOptions {
  using Roles;

  public class GameOption {
    public object CurrentValue;
    public string Name;
    public OptionType Type;
    public OptionBehaviour Option;
    #region Number Option
    public float MaxValue = -1f;
    public float MinValue = 0f;
    public float Increment = 1f;
    #endregion
  }

  [HarmonyPatch(typeof(GameOptionsMenu))]
  public class GameOptions {
    private static GameOption HeaderOption(string name) => new GameOption() {
      Name = name,
      Type = OptionType.Header
    };
    private static GameOption RolePercentage(RoleType roleType) => new GameOption() {
      CurrentValue = 0f,
      MaxValue = 100f,
      Name = RoleManager.GetRoleColor(roleType).Text(RoleManager.GetRoleName(roleType)),
      Type = OptionType.Number,
      Increment = 5f,
    };

    public static Dictionary<CustomOptionKey, GameOption> AllOptions = new Dictionary<CustomOptionKey, GameOption>() {
      { CustomOptionKey.RolePercentageHeader, HeaderOption("Role Chances") },
      { CustomOptionKey.Sheriff, RolePercentage(RoleType.Sheriff) }
    };

    public static void SyncSetting(CustomOptionKey key, object value) {
      GameOption option = AllOptions[key];
      option.CurrentValue = value;
    }

    public static object ReadOption(MessageReader reader, CustomOptionKey key) {
      switch (key) {
        case CustomOptionKey.Sheriff:
          return reader.ReadSingle();
      }
      // Should never return unless incompatible versions are used
      return null;
    }

    public static int GetOptionInt(CustomOptionKey key) {
      return AllOptions[key].Option.GetInt();
    }
    public static float GetOptionFloat(CustomOptionKey key) {
      return AllOptions[key].Option.GetFloat();
    }
    public static bool GetOptionBool(CustomOptionKey key) {
      return AllOptions[key].Option.GetBool();
    }

    [HarmonyPostfix()]
    [HarmonyPatch(nameof(GameOptionsMenu.Start))]
    public static void CreateOptions(GameOptionsMenu __instance) {
      ToggleOption togglePrefab = Object.FindObjectOfType<ToggleOption>();
      NumberOption numberPrefab = Object.FindObjectOfType<NumberOption>();
      StringOption stringPrefab = Object.FindObjectOfType<StringOption>();

      Vector3 pos = __instance.Children[__instance.Children.Length - 1].transform.localPosition;

      int i = 0;
      List<OptionBehaviour> newOptions = new List<OptionBehaviour>();
      foreach (OptionBehaviour opt in __instance.Children) {
        newOptions.Add(opt);
      }
      foreach ((CustomOptionKey key, GameOption option) in AllOptions) {
        OptionBehaviour gameOption = null;

        switch (option.Type) {
          case OptionType.Header:
            ToggleOption headerOption = Object.Instantiate(togglePrefab, togglePrefab.transform.parent);
            gameOption = headerOption;
            gameOption.transform.GetChild(1).gameObject.SetActive(false);
            gameOption.transform.GetChild(2).gameObject.SetActive(false);
            headerOption.TitleText.text = option.Name;
            break;
          case OptionType.Toggle:
            ToggleOption toggleOption = Object.Instantiate(togglePrefab, togglePrefab.transform.parent);
            gameOption = toggleOption;
            toggleOption.CheckMark.enabled = toggleOption.oldValue = (bool) option.CurrentValue;
            toggleOption.TitleText.text = option.Name;
            break;
          case OptionType.Number:
            NumberOption numberOption = Object.Instantiate(numberPrefab, togglePrefab.transform.parent);
            gameOption = numberOption;
            numberOption.Value = (float) option.CurrentValue;
            numberOption.TitleText.text = option.Name;
            numberOption.ValidRange = new FloatRange(option.MinValue, option.MaxValue);
            numberOption.Increment = option.Increment;
            break;
          case OptionType.String:
            StringOption stringOption = Object.Instantiate(stringPrefab, togglePrefab.transform.parent);
            gameOption = stringOption;
            stringOption.Value = 0;
            stringOption.TitleText.text = option.Name;
            break;
        }
        gameOption.OnValueChanged = (System.Action<OptionBehaviour>) delegate (OptionBehaviour opt) {
          object thisIsStupid = null;
          if (opt is StringOption stringOption) {
            stringOption.ValueText.text = ((string[]) option.CurrentValue)[stringOption.Value];
            thisIsStupid = stringOption.Value;
          } else if (opt is NumberOption numberOption) {
            thisIsStupid = numberOption.Value;
          } else if (opt is ToggleOption toggleOption) {
            thisIsStupid = toggleOption.CheckMark.enabled;
          }
          // `thisIsStupid` will always have a value

          option.CurrentValue = thisIsStupid;

          RPCUtil.SyncSetting(key, option);
        };

        if (gameOption is StringOption) {
          ((StringOption) gameOption).Value = 0;
        }

        gameOption.name = gameOption.gameObject.name = option.Name;

        float offset = 0.5f * (i++ + 1);

        gameOption.transform.localPosition = new Vector3(
          pos.x,
          pos.y - offset,
          pos.z
        );

        option.Option = gameOption;

        newOptions.Add(gameOption);
      }

      __instance.Children = new Il2CppReferenceArray<OptionBehaviour>(newOptions.ToArray());
    }
  }
}
