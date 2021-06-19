using System.Text;
using UnityEngine;
using System.Collections.Generic;

namespace TownOfUsRework {
  public static class Extentions {

    public delegate string Mapper<T>(T element);
    public static string Map<T>(this T[] array, Mapper<T> fn, string joiner = "\n") {
      switch (array.Length) {
        case 0:
          return "";
        case 1:
          T item = array[0];
          return item == null ? "null" : item.ToString();
      }
      StringBuilder builder = new StringBuilder();
      builder.Append(array[0]);
      for (int i = 1;i < array.Length;i++) {
        T item = array[i];
        builder.Append($"{joiner}{fn(item)}");
      }
      return builder.ToString();
    }
    
    public static T Random<T>(this List<T> list) {
      return list[Util.RandomInt(0, list.Count - 1)];
    }

    /// <summary>
    /// Returns a shuffled clone of the list
    /// </summary>
    public static List<T> Shuffle<T>(this List<T> list) {
      List<T> newList = new List<T>();
      for (int i = 0;i < list.Count;i++) {
        newList.Add(list[Util.RandomInt(0, list.Count - 1)]);
      }
      return newList;
    }

    /// <summary>
    /// Returns a list of the dictionary items, shuffled
    /// </summary>
    public static List<(TKey, TValue)> Shuffle<TKey, TValue>(this Dictionary<TKey, TValue> dict) {
      List<(TKey, TValue)> list = new List<(TKey, TValue)>();
      foreach ((TKey key, TValue value) in dict) {
        list.Add((key, value));
      }
      return list.Shuffle();
    }

    public static string Text(this Color32 color, string text) => Util.ColorText(color, text);
    public static string Text(this Color color, string text) => Text((Color32) color, text);
  }
}
