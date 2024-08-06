using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;

public class HolidayController : MonoBehaviour
{
    public string csvFileName = "syukujitsu.csv"; // CSV�t�@�C����
    private List<string> holidays; // �j�����X�g���N���X���x���ŕێ�

    void Start()
    {
        int year = DateTime.Now.Year; // ���݂̔N���f�t�H���g�l�Ƃ��Ďg�p
        string filePath = Path.Combine(Application.streamingAssetsPath, csvFileName);
        holidays = LoadHolidays(filePath, year); // �j�����X�g�����[�h
    }

    // �N�������Ƃ��Ď󂯎��悤�Ƀ��\�b�h���C��
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
                            if (date.Year == year) // �����Ŏw�肳�ꂽ�N�ƈ�v���邩�m�F
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

    // ���t�������Ƃ��Ď󂯎��A���̓����j���ł���Ώj������Ԃ����\�b�h
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
                        // �x���ƕԂ��ꍇ�́u�U�֋x���v�ɕύX
                        if(holidayName == "�x��")
                        {
                            return "�U�֋x��";
                        }

                        return holidayName; // �w�肳�ꂽ���t���j���ł���Ώj������Ԃ�
                    }
                }
            }
        }

        return ""; // �j���ł͂Ȃ��ꍇ�͋󕶎����Ԃ�
    }
}
