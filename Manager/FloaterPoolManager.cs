using System.Collections.Generic;
using UnityEngine;

public class FloaterPoolManager : MonoBehaviour
{
    public static FloaterPoolManager Instance { get; private set; }
    [SerializeField] private UI_HarvestFloater floaterPrefab;
    [SerializeField] private Transform poolContainer; // 挂载此脚本的节点

    private Queue<UI_HarvestFloater> pool = new Queue<UI_HarvestFloater>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    /// <summary>
    /// 在屏幕上生成一个收获飘字（图标 + 数量），并自动回收
    /// </summary>
    /// <param name="icon"></param>
    /// <param name="amount"></param>
    /// <param name="screenPosition"></param>
        public void Spawn(Sprite icon, int amount, Vector2 screenPosition)
        {
            UI_HarvestFloater floater;
            if (pool.Count > 0)
            {
                floater = pool.Dequeue();
            }
            else
            {
                floater = Instantiate(floaterPrefab, poolContainer);
            }

            // 强行将层级提到最后，确保渲染在最上层
            floater.transform.SetAsLastSibling(); 
            floater.Trigger(icon, amount, screenPosition);
        }
        public void ReturnToPool(UI_HarvestFloater floater)
        {
            pool.Enqueue(floater);
        }
}
