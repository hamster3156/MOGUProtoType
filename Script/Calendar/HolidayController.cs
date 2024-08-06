using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;

public class HolidayController : MonoBehaviour
{
    public string csvFileName = "syukujitsu.csv"; // CSVファイル名
    private List<string> holidays; // 祝日リストをクラスレベルで保持

    void Start()
    {
        int year = DateTime.Now.Year; // 現在の年をデフォルト値として使用
        string filePath = Path.Combine(Application.streamingAssetsPath, csvFileName);
        holidays = LoadHolidays(filePath, year); // 祝日リストをロード
    }

    // 年を引数として受け取るようにメソッドを修正
    List<string> LoadHolidays(string filePath, int year)
    {
        List<string> holidayList = new List<string>();

        try
        {
            using (StreamReader sr = new StreamReader(filePath))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    string[] values = line.Split(',');
                    if (values.Length > 1)
                    {
                        string dateStr = values[0].Trim();
                        string holidayName = values[1].Trim();

                        if (DateTime.TryParse(dateStr, out DateTime date))
                        {
                            if (date.Year == year) // 引数で指定された年と一致するか確認
                            {
                                holidayList.Add(dateStr + " - " + holidayName);
                            }
                        }
                    }
                }
            }
        }
        catch (IOException e)
        {
            Debug.LogError("Failed to read file: " + e.Message);
        }

        return holidayList;
    }

    // 日付を引数として受け取り、その日が祝日であれば祝日名を返すメソッド
    public string GetHolidayName(DateTime date)
    {
        foreach (string holiday in holidays)
        {
            string[] parts = holiday.Split('-');
            if (parts.Length > 1)
            {
                string dateStr = parts[0].Trim();
                string holidayName = parts[1].Trim();

                if (DateTime.TryParse(dateStr, out DateTime holidayDate))
                {
                    if (holidayDate == date)
                    {
                        // 休日と返す場合は「振替休日」に変更
                        if(holidayName == "休日")
                        {
                            return "振替休日";
                        }

                        return holidayName; // 指定された日付が祝日であれば祝日名を返す
                    }
                }
            }
        }

        return ""; // 祝日ではない場合は空文字列を返す
    }
}
