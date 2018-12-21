using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FlashMask : MonoBehaviour
{
    [SerializeField]
    private float minAlpha;
    [SerializeField]
    private float maxAlpha;
    [SerializeField]
    private float duration;
    [SerializeField]
    private Image m_mask;

    private float timer;
    private bool f_b_State;
    private Color color;
    private Transform startParent;

    private void Awake()
    {
        color = m_mask.color;
        startParent = transform.parent;
        if (duration <= 0) enabled = false;
    }
    private void Update()
    {
        UpdateTimer();

        if (duration > 0)
        {
            color.a = Mathf.Lerp(minAlpha, maxAlpha, timer / duration);
            m_mask.color = color;
        }
        else
        {
            enabled = false;
        }

    }

    public void Hide()
    {
        transform.SetParent(startParent);
        gameObject.SetActive(false);
    }

    public void Show(RectTransform target)
    {
        gameObject.SetActive(true);
        transform.SetParent(target);
        transform.localScale = Vector3.one;
        transform.localRotation = Quaternion.Euler(Vector3.zero);
        transform.localPosition = Vector3.zero;

        var rectTransform = transform as RectTransform;
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

#if UNITY_EDITOR
        UnityEditor.EditorGUIUtility.PingObject(target);
#endif
    }

    private void UpdateTimer()
    {
        if (f_b_State)
        {
            timer += Time.deltaTime;
            if (timer > duration)
            {
                f_b_State = false;
            }
        }
        else
        {
            timer -= Time.deltaTime;
            if (timer < 0)
            {
                f_b_State = true;
            }
        }
    }
}
