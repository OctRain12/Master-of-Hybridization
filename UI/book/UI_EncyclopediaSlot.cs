using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class UI_EncyclopediaSlot : MonoBehaviour, IPointerClickHandler
{
    [Header("视觉组件")]
    public Image plantIcon;
    public GameObject perfectGlowEffect; // aabbcc 金色发光边框
    public TextMeshProUGUI speciesName; 

    private SpeciesData currentSpecies;
    private UI_EncyclopediaWindow parentWindow;

    public void Init(SpeciesData species, UI_EncyclopediaWindow window)
    {
        currentSpecies = species;
        parentWindow = window;
        
        bool isDiscovered = EncyclopediaManager.Instance.IsDiscovered(species);
        bool isPerfected = EncyclopediaManager.Instance.IsPerfected(species);

        plantIcon.sprite = species.matureSprite; // 使用果实或植株图标
        speciesName.text = species.speciesName;

        if (isDiscovered)
        {
            // 已点亮：原色显示
            plantIcon.color = Color.white;
            if (perfectGlowEffect != null) perfectGlowEffect.SetActive(isPerfected);
        }
        else
        {
            // 未点亮：保留 Alpha 轮廓的纯黑剪影，隐藏特效
            plantIcon.color = new Color(0f, 0f, 0f, 0.9f);
            speciesName.text = "???";
            if (perfectGlowEffect != null) perfectGlowEffect.SetActive(false);
        }
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (parentWindow != null && currentSpecies != null)
        {
            parentWindow.ShowDetail(currentSpecies);
        }
    }

}
