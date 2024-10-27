using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PageSwitcher : MonoBehaviour
{
    public void SwitchToPage(int index)
    {
        for(int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(i == index);
        }
    }

}
