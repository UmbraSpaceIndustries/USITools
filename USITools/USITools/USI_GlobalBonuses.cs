using System;
using System.Collections.Generic;
using System.Linq;
using KolonyTools;
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
        var hab = HabBonusList.FirstOrDefault(h => h.BodyId == bodyId);
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
        var hab = HabBonusList.FirstOrDefault(h => h.BodyId == bodyId);
        if (hab == null)
        {
            return 0d;
        }
        else
        {
            return hab.HabBonus;
        }
    }
}

public class PlanetaryHabBonus
{
    public int BodyId { get; set; }
    public double HabBonus { get; set; }
}