using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using OpenAI;
using System.Collections.Generic;

public class CalendarDate : MonoBehaviour
{
    public Action<int, int, int> OpenEatDateAction;
    public Action CloseEatDateAction;

    [SerializeField]
    private ChatGptController chatGptController;

    [SerializeField]
    private Calendar calendar;

    [SerializeField]
    private HolidayController holidayController;

    [SerializeField]
    private RectTransform dayPanelRectTransform;

    [SerializeField]
    private RectTransform todayMealPanelRectTransform;

    [SerializeField]
    private Button returnButton;

    [SerializeField]
    private TMP_Text monthAndDayText;

    [SerializeField]
    private TMP_Text holidayText;

    [SerializeField]
    private TMP_Text todayMonthAndDayText;

    [SerializeField]
    private TMP_Text TodayHolidayText;

    [SerializeField]
    private TMP_Text totalCalorieText;

    [SerializeField]
    private float movePos;

    [SerializeField]
    private float movePosY;

    private float initPosX;
    private float initPosY;

    [SerializeField]
    private Button todayMealButton;

    private bool isTodayMealPanelOpen = false;

    [SerializeField]
    private RectTransform cameraRect;

    private float initCameraPosY;

    [SerializeField]
    private float moveCameraPosY;

    [SerializeField]
    private RawImage picturePrefab;

    [SerializeField]
    private RectTransform pictureParentPos;

    [SerializeField]
    private List<RawImage> pictureList = new List<RawImage>();

    private List<RawImage> subPictures = new List<RawImage>();

    public int toatlCalorie;

    [SerializeField]
    private RectTransform subPictureParentPos;

    [SerializeField]
    private TMP_Text kcalText;

    [SerializeField]
    private TMP_Text eiyou;

    [SerializeField]
    private TMP_Text subEiyou;

    void Start()
    {
        initPosX = dayPanelRectTransform.anchoredPosition.x;
        initPosY = todayMealPanelRectTransform.anchoredPosition.y;

        initCameraPosY = cameraRect.anchoredPosition.y;

        OpenEatDateAction += OpenEatDate;
        CloseEatDateAction += CloseToDayMealPanel;
        returnButton.onClick.AddListener(ReturnEatDate);
        todayMealButton.onClick.AddListener(OnClickToDayMealButton);
    }

    private void OnDestroy()
    {
        OpenEatDateAction -= OpenEatDate;
        CloseEatDateAction -= CloseToDayMealPanel;
        returnButton.onClick.RemoveAllListeners();
        todayMealButton.onClick.RemoveAllListeners();
    }

    private void OpenEatDate(int year, int month, int day)
    {
        DateTime d = new DateTime(DateTime.Now.Year, month, day);
        DateTime today = DateTime.Now.Date; // 今日の日付（時間部分を除く）

        //Debug.Log($"押された日付は{year}年{month}月{day}日");
        monthAndDayText.text = $"{month}月{day}日";

        var holidayName = holidayController.GetHolidayName(new DateTime(year, month, day));
        holidayText.text = holidayName;
        dayPanelRectTransform.DOAnchorPosX(movePos, 0.2f);

        if (d == today)
        {
            foreach (var picture in pictureList)
            {
                var subPictureInstance = Instantiate(picture);
                subPictures.Add(subPictureInstance);
                subPictureInstance.transform.SetParent(subPictureParentPos, false);
                subPictureInstance.gameObject.SetActive(true); // Show the sub picture
                kcalText.text = $"総カロリー　： {toatlCalorie}kcal";
                subEiyou.text = eiyou.text;
            }
        }
        else
        {
            foreach (var subPicture in subPictures)
            {
                subPicture.gameObject.SetActive(false); // Hide the sub picture
                kcalText.text = $"総カロリー　：";
                subEiyou.text = "";
            }
        }
    }

    private void ReturnEatDate()
    {
        dayPanelRectTransform.DOAnchorPosX(initPosX, 0.2f);
    }

    private void OnClickToDayMealButton()
    {
        isTodayMealPanelOpen = !isTodayMealPanelOpen;

        if (isTodayMealPanelOpen)
        {
            calendar.CloseCalendarPanelAction?.Invoke();
            ReturnEatDate();
            CameraButtonChanged(false);

            var currentDate = DateTime.Now;
            todayMonthAndDayText.text = $"{currentDate.Year}年{currentDate.Month}月{currentDate.Day}日";
            TodayHolidayText.text = holidayController.GetHolidayName(currentDate);
            todayMealPanelRectTransform.DOAnchorPosY(movePosY, 0.2f);
        }
        else
        {
            todayMealPanelRectTransform.DOAnchorPosY(initPosY, 0.2f);
            CameraButtonChanged(true);
        }
    }

    public void CalorieUpdate()
    {
        chatGptController.CalorieUp();
        toatlCalorie += chatGptController.Calorie;
        totalCalorieText.text = $"総カロリー　： {toatlCalorie}kcal";
        eiyou.text += chatGptController.Nutrients;
    }

    private void CloseToDayMealPanel()
    {
        isTodayMealPanelOpen = true;
        OnClickToDayMealButton();
        ReturnEatDate();
    }

    public void InstancePicture(Texture2D texture2D)
    {
        var pictureInstance = Instantiate(picturePrefab);
        pictureInstance.texture = texture2D;
        pictureInstance.SetNativeSize();
        pictureInstance.rectTransform.localScale = new Vector3(0.6f, 0.6f, 1);
        
        pictureList.Add(pictureInstance);

        pictureInstance.transform.SetParent(pictureParentPos, false);
    }

    public void CameraButtonChanged(bool isActive)
    {
        if(!isActive)
        {
            cameraRect.DOAnchorPosY(moveCameraPosY, 0.2f);
        }
        else
        {
            cameraRect.DOAnchorPosY(initCameraPosY, 0.2f);
        }
    }
}
