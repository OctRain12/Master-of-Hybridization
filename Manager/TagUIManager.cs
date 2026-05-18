using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TagUIManager : MonoBehaviour
{
    public static TagUIManager Instance;
    public GameObject tagWindow;
    public TMP_InputField inputField;
    private SeedEntry currentTargetEntry;   // 当前打标签对象

    void Awake()
    {
        Instance = this;
    }

    //打开标签输入窗口
    public void ShowWindow(SeedEntry entry)
    {
        currentTargetEntry = entry;

        // 获取旧标签并填入输入框
        InventoryManager.Instance.seedTags.TryGetValue(entry, out string oldTag);
        inputField.text = oldTag;
        tagWindow.SetActive(true);
        inputField.ActivateInputField(); // 自动聚焦
        //Debug.Log("打开标签输入窗口");
    }

    //绑定在确定按钮的 OnClick 事件上
    public void ConfirmTag()
    {
        string newTag = inputField.text;
        // 调用仓库的保存方法保存修改后的标签并且触发UI刷新
        InventoryManager.Instance.SetSeedTag(currentTargetEntry, newTag);

        tagWindow.SetActive(false);

    }

    //取消修改
    public void CancelTag()
    {
        tagWindow.SetActive(false);
    }
}
