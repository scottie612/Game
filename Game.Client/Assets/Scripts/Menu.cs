using UnityEngine;

public abstract class Menu : MonoBehaviour
{
    public virtual void ToggleVisibility()
    {
        gameObject.SetActive(!gameObject.activeSelf);
    }
}