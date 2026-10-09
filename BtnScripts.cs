using UnityEngine;
using UnityEngine.UI;

public class CodexButton : MonoBehaviour
{
    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        SoundManager.Instance?.PlaySFX(SFXType.OpenBook);
    }
}