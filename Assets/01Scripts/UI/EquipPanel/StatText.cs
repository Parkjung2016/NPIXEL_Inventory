using System.Text;
using TMPro;
using UnityEngine;

public class StatText : MonoBehaviour
{
    private TextMeshProUGUI _statText;

    private void Awake()
    {
        _statText = GetComponent<TextMeshProUGUI>();
    }

    public void BindStat(StatData statData)
    {
        UpdateStatText(statData);
        statData.OnValueChanged += HandleStatValueChanged;
    }

    private void HandleStatValueChanged(StatData stat, float current, float prev)
    {
        UpdateStatText(stat);
    }

    private void UpdateStatText(StatData statData)
    {
        StringBuilder sb = new StringBuilder(statData.statName);
        sb.Append(": ");
        sb.Append(statData.Value);
        if (statData.HasModifier())
        {
            sb.AppendLine();
            float modifier = statData.GetTotalModifyValue();
            float modifierPercent = statData.GetTotalModifyValuePercent();
            float value = modifier * (1 + modifierPercent * .01f);
            bool isSum = value > 0;

            sb.Append("(");
            sb.Append("Base ");
            sb.Append(statData.BaseValue);
            if (modifier != 0)
            {
                sb.Append(" / ");
                if (isSum)
                {
                    sb.Append("<color=#FFFF00>");
                    sb.Append("+");
                }
                else
                {
                    sb.Append("<color=#FF0000>");
                }

                sb.Append(modifier);
                sb.Append("</color>");
            }

            if (modifierPercent != 0)
            {
                sb.Append(" / ");

                isSum = modifierPercent > 0;
                if (isSum)
                {
                    sb.Append("<color=#FFFF00>");
                    sb.Append("+");
                }
                else
                {
                    sb.Append("<color=#FF0000>");
                }

                sb.Append(modifierPercent);
                sb.Append("%</color>");
            }

            sb.Append(")");
        }

        _statText.SetText(sb);
    }
}