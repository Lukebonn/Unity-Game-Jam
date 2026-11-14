using System.Collections.Generic;
using System.Text;
using UnityEngine;

// Puts SeaGamez's enemy textures on when an enemy spawns: every part whose name matches a material
// in Resources/EnemyMaterials gets that material. Names are compared without spaces, dashes or
// capitals, so the part "Fly-EasyBody" finds the material "Fly-Easy-Body". Parts with no match
// keep the material they already have. Doing it here (not in the prefabs) keeps the enemy prefabs
// and models untouched for whoever is working on them.
public static class EnemySkins
{
    private static Dictionary<string, Material> byName;

    public static void Apply(GameObject enemy)
    {
        if (byName == null)
        {
            byName = new Dictionary<string, Material>();
            foreach (Material material in Resources.LoadAll<Material>("EnemyMaterials"))
                byName[Key(material.name)] = material;
        }
        if (byName.Count == 0) return;

        foreach (Renderer part in enemy.GetComponentsInChildren<Renderer>(true))
        {
            if (byName.TryGetValue(Key(part.name), out Material material))
                part.sharedMaterial = material;
        }
    }

    // "Fly-Easy-Body" and "Fly-EasyBody" both become "flyeasybody"
    public static string Key(string name)
    {
        var key = new StringBuilder(name.Length);
        foreach (char c in name)
            if (char.IsLetterOrDigit(c)) key.Append(char.ToLowerInvariant(c));
        return key.ToString();
    }
}
