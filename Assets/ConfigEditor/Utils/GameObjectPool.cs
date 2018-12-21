using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameObjectPool : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> prefabs;
    [SerializeField]
    private Transform hideParent;
    private Dictionary<string, GameObject> prefabDic;
    private Dictionary<string, Stack<GameObject>> catched;
    public static GameObjectPool instence { get; private set; }
    private void Awake()
    {
        instence = this;
        prefabDic = new Dictionary<string, GameObject>();
        catched = new Dictionary<string, Stack<GameObject>>();
        hideParent.gameObject.SetActive(false);
        for (int i = 0; i < prefabs.Count; i++)
        {
            var prefab = prefabs[i];
            prefabDic[prefab.name] = prefab;
        }
    }

    public T GetInstenceComponent<T>(string name, Transform parent, bool stayWorldPos = false) where T : Component
    {
        var instence = GetInstence(name, parent, stayWorldPos);
        if (instence != null)
        {
            return instence.GetComponent<T>();
        }
        else
        {
            return null;
        }
    }

    public GameObject GetInstence(string name, Transform parent, bool worldPositionStays = false)
    {
        GameObject prefab = null;
        GameObject instence = null;
        if (prefabDic.TryGetValue(name, out prefab))
        {
            if (catched.ContainsKey(name))
            {
                var stack = catched[name];
                if (stack.Count > 0)
                {
                    instence = stack.Pop();
                    instence.transform.SetParent(parent, worldPositionStays);
                }
                else
                {
                    instence = Instantiate(prefab, parent, worldPositionStays);
                }
            }
            else
            {
                instence = Instantiate(prefab, parent, worldPositionStays);
            }
        }

        if (instence != null)
        {
            instence.name = name;
        }
        return instence;
    }
    public void Release(GameObject go, bool worldPositionStays = false)
    {
        if (prefabDic.ContainsKey(go.name))
        {
            if (!catched.ContainsKey(go.name))
            {
                catched[go.name] = new Stack<GameObject>();
            }
            catched[go.name].Push(go);
            go.transform.SetParent(hideParent, worldPositionStays);
        }
        else
        {
            Destroy(go);
        }
    }
}
