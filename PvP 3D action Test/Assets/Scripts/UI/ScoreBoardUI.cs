using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;

public class ScoreBoardUI : MonoBehaviour
{
    /// <summary> [—инглтон] —сылка на единсвенный экземпл€р скрипта. </summary>
    public static ScoreBoardUI main;

    public GameObject ScorePrefab;

    public List<ScoreboardCell> Cells = new List<ScoreboardCell>();

    public void Awake()
    {
        main = this;
    }

    /// <summary>
    /// ќтчищает все €чейки.
    /// </summary>
    public void WipeScoreboard()
    {
        foreach (var old in Cells) Destroy(old.gameObject);
        Cells.Clear();
    }

    /// <summary>
    /// ќтчищает все €чейки и создаЄт новые дл€ каждого игрока с нул€.
    /// </summary>
    public void RedrawScoreboard()
    {
        WipeScoreboard();

        foreach (var data in GameLogic.main.Players)
        {
            ScoreboardCell newCell = Instantiate(ScorePrefab,transform).GetComponent<ScoreboardCell>();

            newCell.PlayerIdentity = data.PlayerIdentity;
            newCell.PlayerName = data.PlayerName;
            newCell.Score = data.Score;

            Cells.Add(newCell);
            newCell.Redraw();
        }

        SortScoreboard();
    }

    /// <summary>
    /// ќбнавл€ет данные в одной €чейке на основе информации из NetworkIdentity.
    /// </summary>
    public void UpdateScoreboardData(NetworkPlayer player)
    {
        if (!Cells.Exists(x => x.PlayerIdentity == player.PlayerIdentity)) return; //“акое может произойти если метод вызван слишком рано (до создани€ счЄта).

        var targetCell = Cells.First(x => x.PlayerIdentity == player.PlayerIdentity);

        bool sort = targetCell.Score != player.Score; //—ортируем только если счЄт отличаетс€.
        targetCell.PlayerName = player.PlayerName;
        targetCell.Score = player.Score;
        targetCell.Redraw();

        if(sort) SortScoreboard();
    }

    /// <summary>
    /// —ортирует пор€док €чеек в списке и transform.child, исход€ из счЄта на них.
    /// </summary>
    public void SortScoreboard()
    {
        List<ScoreboardCell> cellsOrdered = Cells.OrderByDescending(x => x.Score).ToList();
        for (int i = 0; i < cellsOrdered.Count; i++)
        {
            cellsOrdered[i].transform.SetSiblingIndex(i);
        }
    }

    
}
