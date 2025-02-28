using UnityEngine;

public class Hotbar : MonoBehaviour
{
    public HotbarItem[] HotbarItems;

    private void Start()
    {
        SetSelectedIndex(0);
    }

    public void SetSelectedIndex(int index)
    {
        for (int i = 0; i < HotbarItems.Length; i++)
        {
            HotbarItems[i].SetActive(false);
        }
        HotbarItems[index].SetActive(true);
    }
}