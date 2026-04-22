using UnityEngine;
using UnityEngine.UI;
public class EntityHPBar : MonoBehaviour
{
    [SerializeField]Image _healthFillImage;

    public void UpdateHPBar(float currentHP,float maxHP)
    {
        Debug.Log($"Current Hp {currentHP} , MaxHp{maxHP}");
        float fillAmount = currentHP / maxHP;
        _healthFillImage.fillAmount = fillAmount;
    }
}
