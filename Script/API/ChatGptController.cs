using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace OpenAI
{
    public class ChatGptController : MonoBehaviour
    {
        [SerializeField] private MealStatus mealStatus;
        [SerializeField] private CalendarDate calendarDate;

        [Header("OpenAIのAPIキー")]
        [SerializeField] private string apiKey;

        [Header("生成する文章の最大トークン数")]
        [SerializeField] private int maxTokens = 150;

        [Header("ChatGPTの返答を表示するテキスト")]
        [SerializeField] private Text chatGptResponseText;

        private ChatGPTConnection chatGPTConnection;

        [SerializeField, TextArea(10, 20)]
        private string eatPrompt;

        [SerializeField] private Texture2D snapTexture;

        private string replyMessage;

        public int Calorie { get; private set; }

        public string Nutrients { get; private set; }

        [SerializeField]
        private TMP_Text testText;

        void Start()
        {
            chatGPTConnection = new ChatGPTConnection(apiKey);
        }

        public async void ReqestImage(Texture2D texture2D)
        {
            await chatGPTConnection.GetImageDescriptionAsync(texture2D, eatPrompt,　null, 150);
            replyMessage = chatGPTConnection.MessageList[chatGPTConnection.MessageList.Count - 1];

            string savePath = Application.persistentDataPath + "/eatrecord.txt";
            File.WriteAllText(savePath, replyMessage);
            
            calendarDate.CalorieUpdate();
        }

        public void CalorieUp()
        {
            Calorie = chatGPTConnection.Calorie;
            Nutrients = chatGPTConnection.Nutrients;
            testText.text = chatGPTConnection.ReplyMessage;
        }

        public void LoadReplyMessage()
        {
            if(!File.Exists(Application.persistentDataPath + "/eatrecord.txt"))
            {
                Debug.LogWarning("ファイルが存在しません");
                return;
            }

            string savePath = Application.persistentDataPath + "/eatrecord.txt";
            replyMessage = File.ReadAllText(savePath);
            Debug.Log(replyMessage);
        }
    }
}