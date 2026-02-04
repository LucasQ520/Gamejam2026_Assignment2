using UnityEngine;

public class OxygenSystem : MonoBehaviour
{
    [Header("Oxygen")]
    public float maxOxygen = 100f;
    public float oxygen;

    [Header("Rates")]
    public float drainPerSecond = 8f;   //每秒掉氧
    public float regenPerSecond = 12f;  //每秒回氧

    private MaskController mask;

    private void Start()
    {
        mask = GetComponent<MaskController>();
        oxygen = maxOxygen;

        var ui = FindFirstObjectByType<UIController>();
        if (ui != null) ui.SetOxygen(oxygen, maxOxygen);
    }

    private void Update()
    {
        if (GameManager.I != null && GameManager.I.IsGameOver) return;
        if (mask == null) return;

        if (mask.IsWearingBreathMask())
        {
            //回氧
            oxygen += regenPerSecond * Time.deltaTime;
        }
        else
        {
            //掉氧
            oxygen -= drainPerSecond * Time.deltaTime;
        }

        oxygen = Mathf.Clamp(oxygen, 0f, maxOxygen);

        var ui = FindFirstObjectByType<UIController>();
        if (ui != null) ui.SetOxygen(oxygen, maxOxygen);

        if (oxygen <= 0f)
        {
            GameManager.I.GameOver("Oxygen = 0");
        }
    }
    
    public void ApplyOxygenPercentDelta(float delta01)
    {
        
        oxygen += maxOxygen * delta01;
        oxygen = Mathf.Clamp(oxygen, 0f, maxOxygen);

        //刷新UI
        var ui = FindFirstObjectByType<UIController>();
        if (ui != null) ui.SetOxygen(oxygen, maxOxygen);

        //死亡判定
        if (oxygen <= 0f)
        {
            GameManager.I?.GameOver("Oxygen = 0");
        }
    }

}