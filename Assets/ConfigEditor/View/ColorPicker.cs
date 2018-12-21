using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorPicker : MonoBehaviour
{
    public static ColorPicker instence { get; private set; }
    [SerializeField]
    private CarbonColorChooser chooser;
    [SerializeField]
    private Button m_close;
    [SerializeField]
    private GameObject m_body;
    System.Action<Color> onSelect { get; set; }
    void Awake()
    {
        instence = this;
        m_close.onClick.AddListener(Close);
    }

    private void Close()
    {
        m_body.SetActive(false);
        if(onSelect!= null)
        {
            onSelect.Invoke(chooser.color);
        }
    }

    public void OpenSelectColor(Color color,System.Action<Color> onSelect)
    {
        m_body.SetActive(true);
        this.onSelect = onSelect;
        this.chooser.SetColor(color);
        SetPosition(Input.mousePosition);
    }

    private void SetPosition(Vector3 mousePosition)
    {
        chooser.transform.position = mousePosition;
    }
}
