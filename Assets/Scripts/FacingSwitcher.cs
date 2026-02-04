using UnityEngine;

public class FacingSwitcher : MonoBehaviour
{
    public GameObject front;
    public GameObject back;

    private void Start()
    {
        SetFacingFront(true);
    }

    public void SetFacingFront(bool isFront)
    {
        if (front != null) front.SetActive(isFront);
        if (back != null)  back.SetActive(!isFront);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            bool frontActive = front != null && front.activeSelf;
            SetFacingFront(!frontActive);
        }
    }
}