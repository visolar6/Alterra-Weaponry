namespace VELD.AlterraWeaponry.Utils;

/// <summary>
/// Helper utilities for managing breakable resource (outcrop) drops.
/// </summary>
internal static class OutcropsUtils
{
    /// <summary>
    /// Choose a resource among the outcrop's prefab list.
    /// </summary>
    public static TechType ChooseRandomResourceTechType(this BreakableResource instance)
    {
        TechType result = TechType.None;
        TechType outcropTechType = CraftData.GetTechType(instance.gameObject);

        foreach (BreakableResource.RandomPrefab randPrefab in instance.prefabList)
        {
            EnsureOutcropDrop(outcropTechType, randPrefab.prefabTechType, randPrefab.chance);
        }

        if (BreakableResourcePatcher.CustomDrops.TryGetValue(outcropTechType, out var dropsList))
        {
            foreach (OutcropDropData dropData in dropsList)
            {
                Main.logger.LogDebug($"Checking {dropData.TechType}. Drop data: {dropData}");
                if (Player.main != null && Player.main.gameObject.GetComponent<PlayerEntropy>() != null)
                {
                    if (Player.main.gameObject.GetComponent<PlayerEntropy>().CheckChance(dropData.TechType, dropData.chance))
                    {
                        result = dropData.TechType;
                        Main.logger.LogDebug($"Chose {dropData.TechType}. Drop data: {dropData}");
                        break;
                    }
                }
            }
        }
        return result;
    }

    /// <summary>
    /// Converts a BreakableResource.RandomPrefab to an OutcropDropData.
    /// </summary>
    public static OutcropDropData ToOutcropDropData(this BreakableResource.RandomPrefab randomPrefab)
    {
        return new OutcropDropData()
        {
            TechType = randomPrefab.prefabTechType,
            chance = randomPrefab.chance,
        };
    }

    /// <summary>
    /// Removes the vanilla drops of this breakable resource.
    /// </summary>
    public static bool RemoveVanillaDrops(this BreakableResource instance)
    {
        TechType outcropTechType = CraftData.GetTechType(instance.gameObject);
        if (instance.prefabList.Count > 0)
        {
            if (BreakableResourcePatcher.CustomDrops.ContainsKey(outcropTechType))
            {
                BreakableResourcePatcher.CustomDrops[outcropTechType].RemoveAll(
                    (ocdd) => instance.prefabList.Find((randPrefab) => randPrefab.prefabTechType == ocdd.TechType) != null
                );
            }

            instance.prefabList.Clear();
            return true;
        }
        return false;
    }

    /// <summary>
    /// Ensures that the TechType will spawn when outcropTechType is broken.
    /// Recommended value for chance: less than 0.5f
    /// </summary>
    /// <param name="outcropTechType">TechType of the outcrop.</param>
    /// <param name="resourceTechType">TechType of the resource to spawn when an outcrop is broken.</param>
    /// <param name="chance">Spawn chance (between 0 and 1). Recommended value: less than 0.5f</param>
    /// <returns>An instance of the created OutcropDropData.</returns>
    public static OutcropDropData EnsureOutcropDrop(TechType outcropTechType, TechType resourceTechType, float chance = 0.25f)
    {
        if (BreakableResourcePatcher.CustomDrops.ContainsKey(outcropTechType))
        {
            var outcropDropsDatas = BreakableResourcePatcher.CustomDrops[outcropTechType];
            if (outcropDropsDatas.Find((ocdd) => ocdd.TechType == resourceTechType) == null)
            {
                OutcropDropData data = new() { TechType = resourceTechType, chance = chance };
                BreakableResourcePatcher.CustomDrops[outcropTechType].Add(data);
                return data;
            }
            else
            {
                var existingOutcrop = outcropDropsDatas.Find((odd) => odd.TechType == resourceTechType);
                existingOutcrop.chance = chance;
                return existingOutcrop;
            }
        }
        else
        {
            OutcropDropData data = new() { TechType = resourceTechType, chance = chance };
            BreakableResourcePatcher.CustomDrops.Add(new(outcropTechType, new() { data }));
            return data;
        }
    }

    /// <summary>
    /// Ensures that the TechType will spawn when outcropTechType is broken (with OutcropDropData).
    /// </summary>
    public static OutcropDropData EnsureOutcropDrop(TechType outcropTechType, OutcropDropData dropData)
    {
        return EnsureOutcropDrop(outcropTechType, dropData.TechType, dropData.chance);
    }

    /// <summary>
    /// Ensure that resources TechTypes can spawn for outcrops TechTypes (multiple values).
    /// </summary>
    /// <param name="values">Tuples of (outcropTechType, resourceTechType, chance)</param>
    /// <returns>Array of created OutcropDropData instances.</returns>
    public static OutcropDropData[] EnsureOutcropDrop(params (TechType, TechType, float)[] values)
    {
        var array = new OutcropDropData[values.Length];
        for (int i = 0; i < values.Length; i++)
        {
            var v = values[i];
            array[i] = EnsureOutcropDrop(v.Item1, v.Item2, v.Item3);
        }
        return array;
    }

    /// <summary>
    /// Ensure that resources DropData can spawn for outcrops TechTypes (multiple values).
    /// </summary>
    /// <param name="values">Tuples of (outcropTechType, OutcropDropData)</param>
    /// <returns>Array of created OutcropDropData instances.</returns>
    public static OutcropDropData[] EnsureOutcropDrop(params (TechType, OutcropDropData)[] values)
    {
        var array = new OutcropDropData[values.Length];
        for (int i = 0; i < values.Length; i++)
        {
            var v = values[i];
            array[i] = EnsureOutcropDrop(v.Item1, v.Item2);
        }
        return array;
    }

    /// <summary>
    /// Sets an outcrop drop data for a specific outcrop tech type if it does not already exist.
    /// </summary>
    public static bool SetOutcropDrop(TechType outcropTechType, TechType resourceTechType, out OutcropDropData dropData, float chance = 0.25f)
    {
        if (BreakableResourcePatcher.CustomDrops.ContainsKey(outcropTechType))
        {
            if (BreakableResourcePatcher.CustomDrops[outcropTechType].Exists((ocdd) => ocdd.TechType == resourceTechType))
            {
                dropData = BreakableResourcePatcher.CustomDrops[outcropTechType].Find((ocdd) => ocdd.TechType == resourceTechType);
                return false;  // Did not add a new OutcropDropData
            }

            dropData = new() { TechType = resourceTechType, chance = chance };
            BreakableResourcePatcher.CustomDrops[outcropTechType].Add(dropData);
            return true;
        }
        dropData = new() { TechType = resourceTechType, chance = chance };
        BreakableResourcePatcher.CustomDrops.Add(new(outcropTechType, new() { dropData }));
        return true;
    }

    /// <summary>
    /// Sets an outcrop drop data for a specific outcrop tech type if it does not already exist.
    /// </summary>
    public static bool SetOutcropDrop(TechType outcropTechType, TechType resourceTechType, float chance = 0.25f)
    {
        return SetOutcropDrop(outcropTechType, resourceTechType, out _, chance);
    }

    /// <summary>
    /// Sets multiple outcrop drop data for multiple outcrops tech types (multiple values).
    /// </summary>
    /// <param name="values">Tuples of (outcropTechType, resourceTechType, chance)</param>
    /// <returns>Array of bools indicating if each was successfully created.</returns>
    public static bool[] SetOutcropDrop(params (TechType, TechType, float)[] values)
    {
        var array = new bool[values.Length];
        for (int i = 0; i < values.Length; i++)
        {
            var v = values[i];
            array[i] = SetOutcropDrop(v.Item1, v.Item2, v.Item3);
        }
        return array;
    }

    /// <summary>
    /// Sets multiple outcrop drop data for multiple outcrops tech types with output array.
    /// </summary>
    public static bool[] SetOutcropDrop(out OutcropDropData[] dropDatas, params (TechType, TechType, float)[] values)
    {
        var array = new bool[values.Length];
        dropDatas = new OutcropDropData[values.Length];
        for (int i = 0; i < values.Length; i++)
        {
            var v = values[i];
            array[i] = SetOutcropDrop(v.Item1, v.Item2, out dropDatas[i], v.Item3);
        }
        return array;
    }

    /// <summary>
    /// Sets an outcrop drop data (with OutcropDropData) if it does not already exist.
    /// </summary>
    public static bool SetOutcropDrop(TechType outcropTechType, OutcropDropData dropData, out OutcropDropData outputDropData)
    {
        return SetOutcropDrop(outcropTechType, dropData.TechType, out outputDropData, dropData.chance);
    }

    /// <summary>
    /// Sets an outcrop drop data (with OutcropDropData) if it does not already exist.
    /// </summary>
    public static bool SetOutcropDrop(TechType outcropTechType, OutcropDropData dropData)
    {
        return SetOutcropDrop(outcropTechType, dropData.TechType, dropData.chance);
    }
}
