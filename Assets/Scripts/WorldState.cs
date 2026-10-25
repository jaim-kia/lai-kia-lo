using System.Collections.Generic;
using UnityEngine;

public class WorldState : MonoBehaviour
{
    public static WorldState Instance;

    private readonly HashSet<string> flags = new HashSet<string>();

    private void Awake()
    {
        Instance = this;
    }

    public bool Has(string key) => flags.Contains(key);

    public void Set(string key) => flags.Add(key);

    public void Unset(string key) => flags.Remove(key);

    // hooks for a future save system
    public string[] Export()
    {
        var arr = new string[flags.Count];
        flags.CopyTo(arr);
        return arr;
    }

    public void Import(IEnumerable<string> saved)
    {
        flags.Clear();
        foreach (var key in saved) flags.Add(key);
    }

    public void ResetAll() => flags.Clear(); // new game
}