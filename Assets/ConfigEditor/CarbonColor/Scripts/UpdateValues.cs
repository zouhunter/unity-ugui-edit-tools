using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpdateValues : MonoBehaviour
{

    [SerializeField]
    private string redName = "R: ";
    [SerializeField]
    private string greenName = "G: ";
    [SerializeField]
    private string blueName = "B: ";
    [SerializeField]
    private string alphaName = "A: ";

    [SerializeField]
    private Text redText;
    [SerializeField]
    private Slider redSlider;

    [SerializeField]
    private Text greenText;
    [SerializeField]
    private Slider greenSlider;

    [SerializeField]
    private Text blueText;
    [SerializeField]
    private Slider blueSlider;

    [SerializeField]
    private Text alphaText;
    [SerializeField]
    private Slider alphaSlider;

    [SerializeField]
    private InputField hexField;

    [SerializeField]
    private Image previewImage;

    public Action<float, Color> onColorChanged { get; set; }
    private void Awake()
    {
        redSlider.onValueChanged.AddListener(UpdateRedSlider);
        greenSlider.onValueChanged.AddListener(UpdateGreenSlider);
        blueSlider.onValueChanged.AddListener(UpdateBlueSlider);
        alphaSlider.onValueChanged.AddListener(UpdateAlphaSlider);
        hexField.onValueChanged.AddListener(UpdateInputField);
    }
    public Color color
    {
        get
        {
            return previewImage.color;
        }
    }
    public string hexColor
    {
        get
        {
            return hexField.text;
        }
    }
    public void SetValues(Color color)
    {
        redText.text = redName + (int)(color.r * 255);
        redSlider.value = (int)(color.r * 255);

        greenText.text = greenName + (int)(color.g * 255);
        greenSlider.value = (int)(color.g * 255);

        blueText.text = blueName + (int)(color.b * 255);
        blueSlider.value = (int)(color.b * 255);

        color.a = (alphaSlider.value / 255);

        previewImage.color = color;

        hexField.text = ColorUtility.ToHtmlStringRGBA(color);
    }
    private void UpdateRedSlider(float value)
    {
        redText.text = redName + (int)(value);
        UpdateColorFromValues();
    }
    private void UpdateGreenSlider(float value)
    {
        greenText.text = greenName + (int)(value);
        UpdateColorFromValues();
    }
    private void UpdateBlueSlider(float value)
    {
        blueText.text = blueName + (int)(value);
        UpdateColorFromValues();
    }
    private void UpdateAlphaSlider(float value)
    {
        alphaText.text = alphaName + (int)(value);
        UpdateColorFromValues();
    }
    private void UpdateInputField(string text)
    {
        Color color;
        if (ColorUtility.TryParseHtmlString("#" + text, out color))
        {

            redText.text = redName + (int)(color.r * 255);
            redSlider.value = (int)(color.r * 255);

            greenText.text = greenName + (int)(color.g * 255);
            greenSlider.value = (int)(color.g * 255);

            blueText.text = blueName + (int)(color.b * 255);
            blueSlider.value = (int)(color.b * 255);

            alphaText.text = alphaName + (int)(color.a * 255);
            alphaSlider.value = (int)(color.a * 255);

            UpdateColorFromValues();
        }

    }
    private void UpdateColorFromValues()
    {
        Color newColor = new Color(redSlider.value / 255, greenSlider.value / 255, blueSlider.value / 255, alphaSlider.value / 255);

        hexField.text = ColorUtility.ToHtmlStringRGBA(newColor);

        float h = 0, s = 0, v = 0;

        Color.RGBToHSV(newColor, out h, out s, out v);

        if (onColorChanged != null)
        {
            onColorChanged.Invoke(((360 * h) - 180), newColor);
        }

        previewImage.color = newColor;
    }
}
