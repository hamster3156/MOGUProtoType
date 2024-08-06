using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class DateTimeManager : MonoBehaviour
{
    private bool showColon = true;

    [SerializeField]
    private TMP_Text dateText;

    [SerializeField]
    private TMP_Text monthAndDayWeakText;

    private DateTime currentDateTime;

    private float seconds;

    [SerializeField]
    private float closeTime = 0.5f;

    void Update()
    {
        currentDateTime = DateTime.Now;

        if (showColon)
        {
            dateText.text = currentDateTime.ToString("HH:mm");
        }
        else
        {
            dateText.text = currentDateTime.ToString("HH mm"); // Colonをスペースに変更
        }

        monthAndDayWeakText.text = currentDateTime.ToString("M月d日 (ddd)");
     

        seconds += Time.deltaTime;
        if (seconds > 0.5f)
        {
            showColon = !showColon;
            seconds = 0;
        }
    }
}
