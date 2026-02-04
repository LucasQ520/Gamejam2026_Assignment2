using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIController : MonoBehaviour
{
    [Header("Oxygen Bar")]
    public Image oxygenFillImage; 

    [Header("Suspicion (show left -> right)")]
    public Image[] suspicionSlots; 

    [Header("Mask UI")]
    public Image maskIconImage;     // 面具UI Image
    public Sprite breathMaskIcon;   // 呼吸面罩UI图
    public Sprite humanMaskIcon;    // 人类面具UI图

    [Header("GameOver")]
    public GameObject gameOverRoot;
    public TMP_Text gameOverText;

    private void Start()
    {
        if (gameOverRoot != null) gameOverRoot.SetActive(false);

        // 怀疑全部隐藏
        if (suspicionSlots != null)
        {
            for (int i = 0; i < suspicionSlots.Length; i++)
            {
                if (suspicionSlots[i] != null) suspicionSlots[i].gameObject.SetActive(false);
            }
        }
    }

    // 氧气条
    public void SetOxygen(float current, float max)
    {
        if (oxygenFillImage == null) return;
        float t = (max <= 0f) ? 0f : Mathf.Clamp01(current / max);
        oxygenFillImage.fillAmount = t;
    }

    // 怀疑method
    public void SetSuspicion(int current, int max)
    {
        if (suspicionSlots == null) return;

        int showCount = Mathf.Clamp(current, 0, suspicionSlots.Length);
        for (int i = 0; i < suspicionSlots.Length; i++)
        {
            if (suspicionSlots[i] == null) continue;
            suspicionSlots[i].gameObject.SetActive(i < showCount);
        }
    }

    // 根据当前面具换 icon sprite
    public void SetMask(MaskController.MaskType mask)
    {
        if (maskIconImage != null)
        {
            if (mask == MaskController.MaskType.BreathMask)
            {
                if (breathMaskIcon != null) maskIconImage.sprite = breathMaskIcon;
            }
            else
            {
                if (humanMaskIcon != null) maskIconImage.sprite = humanMaskIcon;
            }
        }

       
    }

    public void ShowGameOver(string reason)
    {
        if (gameOverRoot != null) gameOverRoot.SetActive(true);
        if (gameOverText != null) gameOverText.text = "GAME OVER\n" + reason + "\nPress R to Restart";
    }
}
