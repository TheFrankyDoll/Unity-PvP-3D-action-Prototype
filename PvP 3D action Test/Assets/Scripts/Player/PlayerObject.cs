using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;

/// <summary>
/// �������� ��������� ������. ��� �� ���� �� ��������� ������, �� �������� ������������ ������ ����� ������� � ��������.
/// </summary>
public class PlayerObject : NetworkBehaviour
{
    public NetworkPlayer Data { get => GameLogic.main.GetDataOf(gameObject); }

    public AbilitiesList Abilities;

    public override void OnStartLocalPlayer()
    {
        // ���� ���� ������ ����������, ������ ���� �������� => �������� ������ � ��.
        CameraController.main.PreConnection = false;
        PreConnectionUI.main.SetActive(false);
        EscToExitUI.main.SetActive(true);
        AbilitiesUI.main.SetActive(true);
        AbilitiesUI.main.RedrawAbbilities();
        Cursor.visible = false;
        CameraController.main.FollowBy = transform;
        CameraController.main.transform.SetPositionAndRotation(transform.position, transform.rotation);

        //���������� ������ �� ����� ������.
        CmdSendPlayerName(PreConnectionUI.main.InputPlayerName.text);
    }

    [Command]
    public void CmdSendPlayerName(string playerName)
    {
        Data.SetPlayerName_Server(playerName);
        GameLogic.main.RpcUpdateScoreboards();
    }





    /// <summary>
    /// ������ ���� ��������� ������������, ������� ����� ������������ �����. 
    /// <para>(������ ����������� ��������� �� ��, ��� � ������ �� ����� ������������� �����������)</para>
    /// </summary>
    [System.Serializable]
    public struct AbilitiesList
    {
        // ����� ����� ������������ ������ ����������� ������:
        public DashAbility Dash;


        // ...

        /// <summary> ����� ��������� ��������� �������� ������ ����������� � ���� ������ � ���� �������. 
        /// <para>��� ��� ��� ������ ������ �������� ����� ������ �� ������������ ������������ ������� �����.</para></summary>
        /// <param name="clearNull">����� �� ��������� �� ������� �� �����������, ������� � ������ ���?</param>
        /// <returns> ����� ������ �� �������������. </returns>
        public PlayerAbility[] GetAbillitiesAsArray(bool clearNull = true)
        {
            System.Reflection.FieldInfo[] fields = typeof(AbilitiesList).GetFields();

            PlayerAbility[] array = new PlayerAbility[fields.Length];

            for (int i = 0; i < fields.Length; i++)
            {
                array[i] = fields[i].GetValue(this) as PlayerAbility;
            }

            if (clearNull) array = array.Where(x => x != null).ToArray();
            return array;
        }

        /// <summary> ��������� ������ ������ � ���� ���������. 
        /// <para>��������: ������ ������� ������ �������������� ���-�� ������������! �������� ����� ������ ����� �� GetAbillitiesAsArray(false).</para></summary>
        /// <param name="array">������, ������� ����� ���������.</param>
        public void SetAbillitiesFromArray(PlayerAbility[] array)
        {
            System.Reflection.FieldInfo[] fields = typeof(AbilitiesList).GetFields();

            if (array.Length != fields.Length) { Debug.LogError($"����� ������������ ������� ({array.Length}) �� ��������� � ���-��� ����� � AbilitiesList ({fields.Length})."); return; }

            for (int i = 0; i < fields.Length; i++)
            {
                fields[i].SetValue(this, array[i]);
            }
        }
    }
}
