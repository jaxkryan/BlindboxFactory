using System;
using System.Collections.Generic;
using System.Linq;
using ZLinq;
using Script.Gacha.Machine;
using Script.Machine;
using Unity.VisualScripting;

public static class ListExtension {
    public static void AddAfter<T>(this List<T> list, T item, T after) {
        if (!list.Contains(item)) throw new System.Exception($"Item {after.GetType()} is not in the list");
        List<T> temp = list;
        list.Clear();
        foreach (var i in temp) {
            list.Add(i);
            if (i.Equals(after)) list.Add(item);
        }
    }

    public static void AddAfter<T, TAfter>(this List<T> list, T item, bool forAllCopy = false) where TAfter : T {
        if (!list.Contains(item)) throw new System.Exception($"Item {typeof(TAfter)} is not in the list");

        bool shouldAdd = true;

        List<T> temp = list;
        list.Clear();
        foreach (var i in temp) {
            list.Add(i);
            if (i is TAfter && shouldAdd) {
                list.Add(item);
                if (!forAllCopy) shouldAdd = false;
            }
        }
    }

    public static void AddBefore<T>(this List<T> list, T item, T before) {
        if (!list.Contains(item)) throw new System.Exception($"Item {before.GetType()} is not in the list");
        List<T> temp = list;
        list.Clear();
        foreach (var i in temp) {
            if (i.Equals(before)) list.Add(item);
            list.Add(i);
        }
    }

    public static void AddBefore<T, TBefore>(this List<T> list, T item, bool forAllCopy = false) where TBefore : T {
        if (!list.Contains(item)) throw new System.Exception($"Item {typeof(TBefore)} is not in the list");

        bool shouldAdd = true;

        List<T> temp = list;
        list.Clear();
        foreach (var i in temp) {
            if (i is TBefore && shouldAdd) {
                list.Add(item);
                if (!forAllCopy) shouldAdd = false;
            }

            list.Add(i);
        }
    }
    
    public static bool IsNullOrEmpty<T>(this IList<T> list) {
        return list == null || !(list.Count > 0);
    }

    public static void AddIfNew<T>(this List<T> list, T item) {
        if (!list.Contains(item)) list.Add(item);
    }
    
    public static List<T> Clone<T>(this IList<T> list) {
        List<T> newList = new List<T>();
        foreach (T item in list) {
            newList.Add(item);
        }

        return newList;
    }

    public static IItemRequirement<TItem> Compose<TItem>(this List<IItemRequirement<TItem>> list) where TItem : class  {
        if (list == null || list[0] == null) return ItemRequirementBase<TItem>.None;
        var root = list[0];
        int i = 1;
        while (i < list.Count) {
            var e = list[i++];
            if (e == null) continue;
            root.SetNext(e);
        }
        return root;
    }
    

    public static IEnumerable<MachineBase> FindMachinesOfType(this IEnumerable<MachineBase> list, Type type) {
        return !type.IsSubclassOf(typeof(MachineBase)) ? Enumerable.Empty<MachineBase>() : list.Where(m => m.GetType() == type);
    }
}