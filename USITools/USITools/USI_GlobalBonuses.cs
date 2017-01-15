using System;
using System.Collections.Generic;
using UnityEngine;

public class USI_GlobalBonuses : MonoBehaviour
{
    // Static singleton instance
    private static USI_GlobalBonuses instance;
    private List<PlanetaryHabBonus> _habBonuses; 

    // Static singleton property
    public static USI_GlobalBonuses Instance
    {
        get { return instance ?? (instance = new GameObject("USI_GlobalBonuses").AddComponent<USI_GlobalBonuses>()); }
    }

    public List<PlanetaryHabBonus> HabBonusList
    {
        get { return _habBonuses ?? (_habBonuses = new List<PlanetaryHabBonus>()); }
    }

    public void SaveHabBonus(int bodyId, double bonus)
    {
        var count = HabBonusList.Count;
        PlanetaryHabBonus hab = null;
        for (int i = 0; i < count; ++i)
        {
            var thisHab = HabBonusList[i];
            if (thisHab.BodyId == bodyId)
            {
                hab = thisHab;
                break;
            }
        }

        if (hab == null)
        {
            HabBonusList.Add(new PlanetaryHabBonus { BodyId = bodyId, HabBonus = bonus });
        }
        else
        {
            hab.HabBonus = bonus;
        }
    }

    public double GetHabBonus(int bodyId)
    {
        var count = HabBonusList.Count;
        for (int i = 0; i < count; ++i)
        {
            var thisHab = HabBonusList[i];
            if (thisHab.BodyId == bodyId)
            {
                return thisHab.HabBonus;
            }
        }
        return 0d;
    }
}

public class PlanetaryHabBonus
{
    public int BodyId { get; set; }
    public double HabBonus { get; set; }
}