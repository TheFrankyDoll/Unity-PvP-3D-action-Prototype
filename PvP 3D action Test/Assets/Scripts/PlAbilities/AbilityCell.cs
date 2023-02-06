using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AbilityCell : MonoBehaviour
{
    public PlayerAbility RelatedAbility;
    [Space(5)]
    public Image AbilityImage;
    public Image Cooldown;

    public void Redraw()
    {
        AbilityImage.sprite = RelatedAbility.UIAbilitySprite;
    }

    void Update()
    {
        //Если RelatedAbility не существует, то что то УЖЕ пошло не так, так что проверка не нужна.
        Cooldown.fillAmount = RelatedAbility.CooldownTimer / RelatedAbility.Cooldown;
    }
}
