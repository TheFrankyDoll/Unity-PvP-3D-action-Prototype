using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;

public class ScoreBoardUI : MonoBehaviour
{
    /// <summary> [��������] ������ �� ����������� ��������� �������. </summary>
    public static ScoreBoardUI main;

    public GameObject ScorePrefab;

    public List<ScoreboardCell> Cells = new List<ScoreboardCell>();

    public void Awake()
    {
        main = this;
    }

    /// <summary>
    /// �������� ��� ������.
    /// </summary>
    public void WipeScoreboard()
    {
        foreach (var old in Cells) Destroy(old.gameObject);
        Cells.Clear();
    }

    /// <summary>
    /// �������� ��� ������ � ������ ����� ��� ������� ������ � ����.
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
    /// ��������� ������ � ����� ������ �� ������ ���������� �� NetworkIdentity.
    /// </summary>
    public void UpdateScoreboardData(NetworkPlayer player)
    {
        if (!Cells.Exists(x => x.PlayerIdentity == player.PlayerIdentity)) return; //����� ����� ��������� ���� ����� ������ ������� ���� (�� �������� �����).

        var targetCell = Cells.First(x => x.PlayerIdentity == player.PlayerIdentity);

        bool sort = targetCell.Score != player.Score; //��������� ������ ���� ���� ����������.
        targetCell.PlayerName = player.PlayerName;
        targetCell.Score = player.Score;
        targetCell.Redraw();

        if(sort) SortScoreboard();
    }

    /// <summary>
    /// ��������� ������� ����� � ������ � transform.child, ������ �� ����� �� ���.
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
