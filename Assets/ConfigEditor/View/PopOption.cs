using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopOption : MonoBehaviour {
    [SerializeField]
    private string popItemKey;
    [SerializeField]
    private Button m_cansaleContent;
    [SerializeField]
    private Transform parent;

    private List<Button> created = new List<Button>();
    public static PopOption Instence { get; private set; }
    //private string[] options;
    private void Awake()
    {
        m_cansaleContent.onClick.AddListener(Clear);
        Instence = this;
        Clear();
    }

    public void ShowPop(string[] options,System.Action<int> onSelect)
    {
        if (options == null || options.Length == 0 || onSelect == null)
        {
            return;
        }
        else
        {
            m_cansaleContent.gameObject.SetActive(true);

            parent.transform.position = Input.mousePosition;

            for (int i = 0; i < options.Length; i++)
            {
                var btn = GameObjectPool.instence.GetInstenceComponent<Button>(popItemKey, parent);
                var index = i;
                btn.GetComponentInChildren<Text>().text = options[index];
                btn.onClick.AddListener(() => {
                    if (options != null && options.Length > 0){
                        onSelect.Invoke(index);
                    }
                    Clear();
                });
                created.Add(btn);
            }
        }
    }
    public void ShowPop(string[] options,bool[] states, System.Action<int> onSelect)
    {
        if (options == null || options.Length == 0 || onSelect == null || states==null || states.Length < options.Length)
        {
            return;
        }
        else
        {
            m_cansaleContent.gameObject.SetActive(true);

            parent.transform.position = Input.mousePosition;

            for (int i = 0; i < options.Length; i++)
            {
                var btn = GameObjectPool.instence.GetInstenceComponent<Button>(popItemKey, parent);
                var index = i;
                btn.GetComponentInChildren<Text>().text = options[index];
                btn.gameObject.SetActive(states[i]);
                btn.onClick.AddListener(() => {
                    if (options != null && options.Length > 0)
                    {
                        onSelect.Invoke(index);
                    }
                    Clear();
                });
                created.Add(btn);
            }
        }
    }

    public void Clear()
    {
        for (int i = 0; i < created.Count; i++)
        {
            var btn = created[i];
            btn.onClick.RemoveAllListeners();
            GameObjectPool.instence.Release(btn.gameObject, false);
        }

        created.Clear();

        m_cansaleContent.gameObject.SetActive(false);
    }
}
