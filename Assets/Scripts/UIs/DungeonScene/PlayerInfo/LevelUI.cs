using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class LevelUI : UI_Base
{
    public int level;
    public TextMeshProUGUI levelText;

    private void Awake()
    {
        levelText = GetComponentInChildren<TextMeshProUGUI>();
    }

    public void Set_LevelText(int level)
    {
        this.level = level;
        UpdateLevelText();
    }

    public void UpdateLevelText()
    {
        levelText.text = $"Lv : {level.ToString()}";
    }
}
