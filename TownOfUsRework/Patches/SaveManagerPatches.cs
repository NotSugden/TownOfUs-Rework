using HarmonyLib;
using InnerNet;

namespace TownOfUsRework.Patches {
  [HarmonyPatch(typeof(SaveManager))]
  class SaveManagerPatches {
    [HarmonyPostfix()]
    [HarmonyPatch(nameof(SaveManager.ChatModeType), MethodType.Getter)]
    public static void ChatModeTypePatch(out QuickChatModes __result) => __result = QuickChatModes.FreeChatOrQuickChat;

    [HarmonyPostfix()]
    [HarmonyPatch(nameof(SaveManager.BirthDateYear), MethodType.Getter)]
    public static void BirthDateYearPatch(out int __result) => __result = 2000;
  }
}
