using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilitiesUI : MonoBehaviour
{
    /// <summary> [Синглтон] Ссылка на единсвенный экземпляр скрипта. </summary>
    public static AbilitiesUI main;
    public GameObject CellPrafab;

    // Никакого ограничения на число способностей нет, но конкретно ЭТОТ интерфейс поддерживает только 4.
    public Transform[] CellPositions = new Transform[4]; 
    public AbilityCell[] Cells = new AbilityCell[4];

    void Awake()
    {
        main = this;
        SetActive(false);
    }

    /// <summary> Включает/Выключает этот UI. </summary>
    public void SetActive(bool value)
    {
        gameObject.SetActive(value);
    }

    public void RedrawAbbilities()
    {
        foreach(var oldCell in Cells) { if (oldCell) Destroy(oldCell.gameObject);  }

        var abilities = Mirror.NetworkClient.localPlayer.GetComponent<PlayerObject>().Abilities.GetAbillitiesAsArray(true);

        for(byte i = 0; i < abilities.Length && i < 4; i++)
        {
            AbilityCell newCell = Instantiate(CellPrafab, CellPositions[i]).GetComponent<AbilityCell>();

            newCell.RelatedAbility = abilities[i];
            newCell.Redraw();

            Cells[i] = newCell;
        }
    }
}
