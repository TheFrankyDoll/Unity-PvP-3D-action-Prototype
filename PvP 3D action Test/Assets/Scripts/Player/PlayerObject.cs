using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;

/// <summary>
/// Основной компонент игрока. Сам по себе не выполняет задачи, но является переходником данных между игроком и сервером.
/// </summary>
public class PlayerObject : NetworkBehaviour
{
    public NetworkPlayer Data { get => GameLogic.main.GetDataOf(gameObject); }

    public AbilitiesList Abilities;

    public override void OnStartLocalPlayer()
    {
        // Если этот объект существует, значит игра началась => включаем камеру и пр.
        CameraController.main.PreConnection = false;
        PreConnectionUI.main.SetActive(false);
        EscToExitUI.main.SetActive(true);
        AbilitiesUI.main.SetActive(true);
        AbilitiesUI.main.RedrawAbbilities();
        Cursor.visible = false;
        CameraController.main.FollowBy = transform;
        CameraController.main.transform.SetPositionAndRotation(transform.position, transform.rotation);

        //Отправляем данные об имени игрока.
        CmdSendPlayerName(PreConnectionUI.main.InputPlayerName.text);
    }

    [Command]
    public void CmdSendPlayerName(string playerName)
    {
        Data.SetPlayerName_Server(playerName);
        GameLogic.main.RpcUpdateScoreboards();
    }





    /// <summary>
    /// Список всех возможных способностей, которые может использовать игрок. 
    /// <para>(Данная архитектура расчитана на то, что у игрока не будут дублироваться способности)</para>
    /// </summary>
    [System.Serializable]
    public struct AbilitiesList
    {
        // Здесь будет перечисленна каждая способность игрока:
        public DashAbility Dash;


        // ...

        /// <summary> Через рефлекции позволяет получить каждую способность в этом списке в виде массива. 
        /// <para>Так как при каждом вызове создаётся новый массив не рекомедуется использовать слишком часто.</para></summary>
        /// <param name="clearNull">Нужно ли вычистить из массива те способности, которых у игрока нет?</param>
        /// <returns> Новый массив со способностями. </returns>
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

        /// <summary> Применить массив данных к этой структуре. 
        /// <para>ВНИМАНИЕ: Длинна массива ДОЛЖНА соответсвовать кол-ву способностей! Получить такой массив можно из GetAbillitiesAsArray(false).</para></summary>
        /// <param name="array">Данные, которые будут применены.</param>
        public void SetAbillitiesFromArray(PlayerAbility[] array)
        {
            System.Reflection.FieldInfo[] fields = typeof(AbilitiesList).GetFields();

            if (array.Length != fields.Length) { Debug.LogError($"Длина поступившего массива ({array.Length}) не совпадает с кол-вом полей в AbilitiesList ({fields.Length})."); return; }

            for (int i = 0; i < fields.Length; i++)
            {
                fields[i].SetValue(this, array[i]);
            }
        }
    }
}
