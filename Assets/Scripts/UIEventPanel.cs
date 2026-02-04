using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIEventPanel : MonoBehaviour
{
    public GameObject root;
    public TMP_Text dialogueText;
    public Image timerFill;

    [Header("Hint Item Icons (optional)")]
    public GameObject itemIconsRoot;
    public Image[] itemIcons;  
    public Image[] itemIconBackgrounds; 

    [Header("Hint Icon Background Sprites")]
    public Sprite bgHoldSprite;  
    public Sprite bgSubmitSprite;

    public void Show(string text)
    {
        if (root != null) root.SetActive(true);
        SetText(text);
    }

    public void SetText(string text)
    {
        if (dialogueText != null) dialogueText.text = text;
    }

    public void SetTimer01(float t01)
    {
        if (timerFill != null) timerFill.fillAmount = Mathf.Clamp01(t01);
    }

    public void Hide()
    {
        if (root != null) root.SetActive(false);
    }

    public void SetItemIcons(Sprite[] sprites)
    {
        SetItemIcons(sprites, null);
    }

    public void SetItemIcons(Sprite[] sprites, bool[] submitFlags)
    {
        if (itemIconsRoot == null || itemIcons == null || itemIcons.Length == 0) return;

        bool hasAny = (sprites != null && sprites.Length > 0);
        itemIconsRoot.SetActive(hasAny);

        for (int i = 0; i < itemIcons.Length; i++)
        {
            if (itemIcons[i] != null)
            {
                itemIcons[i].gameObject.SetActive(false);
                itemIcons[i].sprite = null;
            }

            if (itemIconBackgrounds != null && i < itemIconBackgrounds.Length && itemIconBackgrounds[i] != null)
            {
                itemIconBackgrounds[i].gameObject.SetActive(false);
                itemIconBackgrounds[i].sprite = null;
            }
        }

        if (!hasAny) return;

        int count = Mathf.Min(itemIcons.Length, sprites.Length);

        for (int i = 0; i < count; i++)
        {
            if (itemIcons[i] != null)
            {
                itemIcons[i].sprite = sprites[i];
                itemIcons[i].gameObject.SetActive(true);
            }

            bool isSubmit = (submitFlags != null && i < submitFlags.Length && submitFlags[i]);
            Sprite bg = isSubmit ? bgSubmitSprite : bgHoldSprite;

            if (itemIconBackgrounds != null && i < itemIconBackgrounds.Length && itemIconBackgrounds[i] != null)
            {
                itemIconBackgrounds[i].sprite = bg;
                itemIconBackgrounds[i].gameObject.SetActive(true);
            }
        }
    }
}
