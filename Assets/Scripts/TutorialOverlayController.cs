using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TutorialOverlayController : MonoBehaviour
{
    [Header("UI")]
    public GameObject root; 
    public Image pageImage;
    public Sprite[] pages;
    public KeyCode nextKey = KeyCode.Space;

    [Header("When finished")]
    public UnityEvent onTutorialFinished; 
    public bool startGameTimer = true; 

    private int index = 0;
    private bool finished = false;

    private void Awake()
    {
        if (root == null) root = gameObject;

        GameGate.SetGameplayEnabled(false);
        Time.timeScale = 0f;

        ShowPage(0);
    }

    private void Update()
    {
        if (finished) return;

        if (Input.GetKeyDown(nextKey))
        {
            index++;

            if (pages != null && index < pages.Length)
            {
                ShowPage(index);
            }
            else
            {
                FinishTutorial();
            }
        }
    }

    private void ShowPage(int i)
    {
        if (pageImage != null && pages != null && pages.Length > 0)
        {
            i = Mathf.Clamp(i, 0, pages.Length - 1);
            pageImage.sprite = pages[i];
        }
    }

    private void FinishTutorial()
    {
        finished = true;

        if (root != null) root.SetActive(false);

        Time.timeScale = 1f;
        GameGate.SetGameplayEnabled(true);

        if (GameTimer.I != null)
            GameTimer.I.StartTimer(resetFirst: true);

        onTutorialFinished?.Invoke();
    }

}