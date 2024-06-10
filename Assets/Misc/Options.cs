using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Options : MonoBehaviour
{
    public void setQualityVeryLow()
    {
        QualitySettings.SetQualityLevel(0, false);
    }

    public void setQualityLow()
    {
        QualitySettings.SetQualityLevel(1, false);
    }

    public void setQualityMedium()
    {
        QualitySettings.SetQualityLevel(2, false);
    }

    public void setQualityHigh()
    {
        QualitySettings.SetQualityLevel(3, false);
    }

    public void setQualityVeryHigh()
    {
        QualitySettings.SetQualityLevel(4, false);
    }

    public void setQualityUltra()
    {
        QualitySettings.SetQualityLevel(5, false);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
