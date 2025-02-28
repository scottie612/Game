using UnityEngine;
using UnityEngine.UI;

public class HotbarItem : MonoBehaviour
{
    public Image IsActiveBorder;

    public void SetActive(bool active)
    {
        IsActiveBorder.enabled = active;
    }
    
}
