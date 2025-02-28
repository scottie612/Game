using UnityEngine;

public class UIManager : Singleton<UIManager>
{
    
    [SerializeField] private Hotbar Hotbar;
    [SerializeField] private GameMenu GameMenu;

    public void ChangeSelectedHotbarIndex(int index)
    {
        Hotbar.SetSelectedIndex(index);
    }

    public void ToggleGameMenuVisibility()
    {
        GameMenu.ToggleVisibility();
    }
}
