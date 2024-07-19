using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class HPBar : MonoBehaviour
{
    public UnityEvent onHPDepleted;

    [Range(0, 100)]
    public int maxHP = 100;
    private int currentHP;

    public enum OutputType
    {
        None,
        StandardText,
        TMPro,
        HorizontalSlider,
        Dial
    };

    [Tooltip("Select the output type")]
    public OutputType outputType;
    public Text standardText;
    public TextMeshProUGUI textMeshProText;
    public Slider standardSlider;
    public Image dialSlider;

    private void Awake()
    {
        currentHP = maxHP;
        if (!standardText && GetComponent<Text>())
        {
            standardText = GetComponent<Text>();
        }
        if (!textMeshProText && GetComponent<TextMeshProUGUI>())
        {
            textMeshProText = GetComponent<TextMeshProUGUI>();
        }
        if (!standardSlider && GetComponent<Slider>())
        {
            standardSlider = GetComponent<Slider>();
        }
        if (!dialSlider && GetComponent<Image>())
        {
            dialSlider = GetComponent<Image>();
        }
        if (standardSlider)
        {
            standardSlider.maxValue = maxHP;
            standardSlider.value = currentHP;
        }
        if (dialSlider)
        {
            dialSlider.fillAmount = 1f;
        }
        UpdateHPDisplay();
    }

    public void IncreaseHP(int amount)
    {
        currentHP += amount;
        if (currentHP > maxHP)
        {
            currentHP = maxHP;
        }
        UpdateHPDisplay();
    }

    public void DecreaseHP(int amount)
    {
        currentHP -= amount;
        if (currentHP <= 0)
        {
            currentHP = 0;
            onHPDepleted.Invoke();
        }
        UpdateHPDisplay();
    }

    private void UpdateHPDisplay()
    {
        if (standardSlider)
        {
            standardSlider.value = currentHP;
        }
        if (dialSlider)
        {
            dialSlider.fillAmount = (float)currentHP / maxHP;
        }
        if (standardText)
        {
            standardText.text = currentHP.ToString();
        }
        if (textMeshProText)
        {
            textMeshProText.text = currentHP.ToString();
        }
    }
}