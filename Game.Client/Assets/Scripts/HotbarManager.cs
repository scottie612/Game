using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HotbarManager : Singleton<HotbarManager>
{
    // Start is called before the first frame update

    private List<GameObject> _hotbarSlots = new List<GameObject>();
    void Start()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            GameObject Go = transform.GetChild(i).gameObject;
            var borderGo = Go.transform.Find("Border");
            borderGo.GetComponent<Image>().enabled = false;
            _hotbarSlots.Add(Go);

        }
        _hotbarSlots[0].transform.Find("Border").GetComponent<Image>().enabled = true;
    }

    public void ChangeSelectedHotbarIndex(int index)
    {
        for (int i = 0; i < _hotbarSlots.Count; i++)
        {
            _hotbarSlots[i].transform.Find("Border").GetComponent<Image>().enabled = false;
        }
        _hotbarSlots[index].transform.Find("Border").GetComponent<Image>().enabled = true;
    }
}
