using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Calendar : MonoBehaviour
{
    public Action CloseCalendarPanelAction;

    [Header("月を表示するテキスト")]
    [SerializeField] private TMP_Text monthAndYearsText;

    [SerializeField] private GameObject[] dayPanels; // 事前に用意した49個のパネル

    private DateTime currentDate = DateTime.Now;

    private int daysInMonth;
    private int startDayOfWeek;

    [SerializeField] private Slider slider;

    private int currentYear = DateTime.Now.Year;

    [SerializeField] private GameObject buttonPrefab;
    [SerializeField] private Transform createPos;

    [Header("ボタンを生成するかどうか")]
    [SerializeField] private bool isCreateButton;

    private TMP_Text[] dayTexts; // パネルのテキストコンポーネントを格納する配列
    private Image[] panelImages; // パネルのイメージコンポーネントを格納する配列
    private Button[] buttons; // パネルのボタンコンポーネントを格納する配列

    [SerializeField]
    private HolidayController holidayController;

    [SerializeField]
    private CalendarDate calendarDate;

    [SerializeField]
    private Button calendarButton;

    [SerializeField]
    private RectTransform calendarPanelRecTransform;

    [SerializeField]
    private float movePos;

    private float initPos;

    private int currentMonth;

    private bool isCalendarOpen = false;

    private bool isInitWeek = false;

    void Start()
    {
        initPos = calendarPanelRecTransform.anchoredPosition.y;
        calendarButton.onClick.AddListener(CalendarButtonClicked);

        currentMonth = currentDate.Month;

        Initialize();
        InitializeMonthAndYear(monthAndYearsText, true);
        CreateButtonPrefab(buttonPrefab, createPos, isCreateButton);
        slider.value = currentDate.Month;
        slider.onValueChanged.AddListener((value) => ChangeValueOnMonthAndYear());
        SetCalendar(currentDate.Month);

        CloseCalendarPanelAction += CloseCalendarPanel;
    }

    private void OnDestroy()
    {
        slider.onValueChanged.RemoveAllListeners();
        calendarButton.onClick.RemoveAllListeners();

        for (int i = 0; i < dayPanels.Length; i++)
        {
            if (i > 6)
            {
                buttons[i].onClick.RemoveAllListeners();
            }
        }

        CloseCalendarPanelAction -= CloseCalendarPanel;
    }

    private void Initialize()
    {
        dayTexts = new TMP_Text[dayPanels.Length];
        panelImages = new Image[dayPanels.Length];
        buttons = new Button[dayPanels.Length];

        for (int i = 0; i < dayPanels.Length; i++)
        {
            dayTexts[i] = dayPanels[i].GetComponentInChildren<TMP_Text>();
            panelImages[i] = dayPanels[i].GetComponent<Image>();
            buttons[i] = dayPanels[i].GetComponent<Button>();

            if (i > 6)
            {
                int index = i;
                buttons[i].onClick.RemoveAllListeners();  // 既存のリスナーを削除
                buttons[i].onClick.AddListener(() => ButtonClicked(dayTexts[index]));
            }
        }
    }

    void SetCalendar(int currentMonth)
    {
        daysInMonth = DateTime.DaysInMonth(currentDate.Year, currentMonth);
        startDayOfWeek = (int)new DateTime(currentDate.Year, currentMonth, 1).DayOfWeek;

        if (!isInitWeek)
        {
            string[] weekdays = { "日", "月", "火", "水", "木", "金", "土" };

            for (int i = 0; i < weekdays.Length; i++)
            {
                if (i < dayTexts.Length)
                {
                    dayTexts[i].text = weekdays[i];
                    dayTexts[i].color = (i == 0) ? Color.red : (i == 6) ? Color.blue : Color.black;
                }
            }

            isInitWeek = true;
        }

        for (int i = 7; i < dayPanels.Length; i++)
        {
            dayTexts[i].text = "";
            panelImages[i].color = Color.white;
            buttons[i].enabled = false;

            if (i >= startDayOfWeek + 7 && i < startDayOfWeek + 7 + daysInMonth)
            {
                int day = i - startDayOfWeek - 7 + 1;
                dayTexts[i].text = day.ToString();
                dayTexts[i].color = Color.black;

                if (DateTime.Now.Year == currentYear && currentMonth == DateTime.Now.Month && day == DateTime.Now.Day)
                {
                    panelImages[i].color = Color.yellow;
                }

                // 祝日の色設定
                DateTime dayDate = new DateTime(currentDate.Year, currentMonth, day);
                string holidayName = holidayController.GetHolidayName(dayDate);
                if (!string.IsNullOrEmpty(holidayName))
                {
                    dayTexts[i].color = Color.red;
                }
                else
                {
                    // 日曜日を赤色、土曜日を青色に設定
                    if ((i - 7) % 7 == 0)
                    {
                        dayTexts[i].color = Color.red;
                    }
                    else if ((i - 7) % 7 == 6)
                    {
                        dayTexts[i].color = Color.blue;
                    }
                }

                buttons[i].enabled = true;
            }
        }
    }



    private void InitializeMonthAndYear(TMP_Text monthAndYearsText,
        bool isCurrentMonthAndYear = true, int changeYear = 0, int changeMonth = 0)
    {
        if (isCurrentMonthAndYear)
        {
            currentMonth = currentDate.Month;
            monthAndYearsText.text = $"{currentDate.Year}年{currentDate.Month}月";
            return;
        }
        else
        {
            currentMonth = changeMonth;
            monthAndYearsText.text = $"{changeYear}年{changeMonth}月";
        }
    }

    private void ChangeValueOnMonthAndYear()
    {
        var changeValue = (int)slider.value;
        SetCalendar(changeValue);
        InitializeMonthAndYear(monthAndYearsText, false, currentDate.Year, changeValue);
    }

    /// <summary>
    /// カレンダーの曜日と日付のパネルを生成する
    /// </summary>
    private void CreateButtonPrefab(GameObject buttonPrefab, Transform createPos, bool isCreateButton)
    {
        if (!isCreateButton)
        {
            return;
        }

        for (int i = 0; i < 49; i++)
        {
            GameObject button = Instantiate(buttonPrefab, createPos);
            InitializeButton(i, button);
        }
    }

    /// <summary>
    /// 生成したボタンの表示名やコンポーネントを初期化す
    /// </summary>
    private void InitializeButton(int buttonNumber, GameObject button)
    {
        var textTypeName = "(TMP)";

        // 曜日オブジェクトの名称を変更する
        switch (buttonNumber)
        {
            case 0:
                button.name = $"SundayButton {textTypeName}";
                break;

            case 1:
                button.name = $"MondayButton {textTypeName}";
                break;

            case 2:
                button.name = $"TuesdayButton {textTypeName}";
                break;

            case 3:
                button.name = $"WednesdayButton {textTypeName}";
                break;

            case 4:
                button.name = $"ThursdayButton {textTypeName}";
                break;

            case 5:
                button.name = $"FridayButton {textTypeName}";
                break;
            case 6:
                button.name = $"SaturdayButton {textTypeName}";
                break;
        }

        // 曜日オブジェクトから、UIのButtonを削除して処理を終了する
        if (buttonNumber < 7)
        {
            Destroy(buttons[buttonNumber]);
            buttons[buttonNumber] = null;
            return;
        }

        // 日付オブジェクトの名称を変更する
        button.name = $"DayButton_{buttonNumber} {textTypeName}";
    }

    private void ButtonClicked(TMP_Text dayText)
    {
        var day = int.Parse(dayText.text);
        calendarDate.OpenEatDateAction?.Invoke(currentDate.Year, currentMonth, day);
    }

    private void CalendarButtonClicked()
    {
        isCalendarOpen = !isCalendarOpen;

        if (isCalendarOpen)
        {
            calendarDate.CloseEatDateAction?.Invoke();
            calendarDate.CameraButtonChanged(false);
            calendarPanelRecTransform.DOAnchorPosY(movePos, 0.2f);
        }
        else
        {
            calendarPanelRecTransform.DOAnchorPosY(initPos, 0.2f);
            calendarDate.CameraButtonChanged(true);
            InitSliderValue();
        }
    }

    private void CloseCalendarPanel()
    {
        isCalendarOpen = true;
        CalendarButtonClicked();
        InitSliderValue();
    }

    private void InitSliderValue()
    {
        slider.value = currentDate.Month;
        SetCalendar(currentDate.Month);
        InitializeMonthAndYear(monthAndYearsText, false, currentDate.Year, currentDate.Month);
    }
}
