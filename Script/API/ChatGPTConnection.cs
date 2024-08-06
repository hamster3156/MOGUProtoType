using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using System.Text.RegularExpressions;

namespace OpenAI
{
    public class ChatGPTConnection
    {
        // OpenAIのAPIキー
        private readonly string apiKey;

        // 会話履歴を保持するリスト
        private readonly List<ChatGPTMessageModel> _messageList;

        // APIのURL
        private readonly string apiUrl = "https://api.openai.com/v1/chat/completions";

        public List<string> MessageList = new List<string>();

        // カロリー情報を格納するフィールド
        public int Calorie { get; private set; }

        // 栄養素情報を格納するフィールド
        public string Nutrients { get; private set; }

        public string ReplyMessage { get; private set; }

        // コンストラクタ
        public ChatGPTConnection(string apiKey)
        {
            this.apiKey = apiKey;

            // リストを初期化して初回メッセージを送信している
            _messageList = new List<ChatGPTMessageModel>
            {
                new ChatGPTMessageModel { role = "system", content = "このメッセージが届いている場合、接続しましたと返答して" }
            };
        }

        /// <summary>
        /// ChatGPTにメッセージのリクエストを送信する
        /// </summary>
        public async UniTask<ChatGPTResponseModel> RequestAsync(string userMessage, int maxTokens)
        {
            // ユーザーがチャットを送った内容をリストに追加する 
            _messageList.Add(new ChatGPTMessageModel { role = "user", content = userMessage });

            // OpenAIのAPIリクエストに必要なヘッダー情報を設定
            var headers = new Dictionary<string, string>
            {
                { "Authorization", "Bearer " + apiKey },
                { "Content-type", "application/json" },
                { "X-Slack-No-Retry", "1" }
            };

            // 文章生成で利用するモデル(ChatGPTのバージョン)やトークン上限、プロンプトをオプションに設定
            var options = new ChatGPTCompletionRequestModel()
            {
                // ChatGPTのバージョンをここで指定する
                model = "gpt-3.5-turbo",
                messages = _messageList,
                max_tokens = maxTokens,
            };
            var jsonOptions = JsonUtility.ToJson(options);

            // デバックで自分が送ったメッセージを表示する
            Debug.Log("[自分]：" + userMessage);

            // OpenAIの文章生成(Completion)にAPIのリクエストを送り、結果を変数に格納する
            using var request = new UnityWebRequest(apiUrl, "POST")
            {
                uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonOptions)),
                downloadHandler = new DownloadHandlerBuffer()
            };

            foreach (var header in headers)
            {
                request.SetRequestHeader(header.Key, header.Value);
            }

            await request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError ||
               request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError(request.error);
                throw new Exception();
            }
            else
            {
                var responseString = request.downloadHandler.text;
                var responseObject = JsonUtility.FromJson<ChatGPTResponseModel>(responseString);

                // ChatGPTの返答をデバックに表示して、リストに内容を追加する
                Debug.Log("[ChatGPT]：" + responseObject.choices[0].message.content);
                _messageList.Add(responseObject.choices[0].message);

                // カロリー情報と栄養素情報を抽出して格納する
                ExtractCalorieAndNutrientsFromResponse(responseObject.choices[0].message.content);

                return responseObject;
            }
        }

        // レスポンスからカロリー情報と栄養素情報を抽出して格納するメソッド
        private void ExtractCalorieAndNutrientsFromResponse(string responseContent)
        {
            // カロリー情報を抽出する正規表現
            Regex calorieRegex = new Regex(@"カロリー\s*[:：]\s*約?(\d+)\s*kcal", RegexOptions.IgnoreCase);
            var calorieMatch = calorieRegex.Match(responseContent);
            if (calorieMatch.Success)
            {
                Calorie = int.Parse(calorieMatch.Groups[1].Value);
                Debug.Log("Extracted Calorie: " + Calorie);
            }
            else
            {
                Debug.LogWarning("Calorie information not found in the response.");
            }

            // 栄養素情報を抽出する正規表現
            Regex nutrientsRegex = new Regex(@"栄養素\s*[:：]\s*(.*)", RegexOptions.IgnoreCase);
            var nutrientsMatch = nutrientsRegex.Match(responseContent);
            if (nutrientsMatch.Success)
            {
                Nutrients = nutrientsMatch.Groups[1].Value.Trim();
                //Debug.Log(s"Extracted Nutrients: " + Nutrients);
            }
            else
            {
                //Debug.LogWarning("Nutrients information not found in the response.");
            }
        }

        public async UniTask GetImageDescriptionAsync(Texture2D snapTexture, string userPrompt, string reply, int maxTokens)
        {
            // テクスチャをバイト配列に変換して、Base64エンコードする
            byte[] bytes = snapTexture.EncodeToPNG();
            string base64Image = Convert.ToBase64String(bytes);

            var options = new
            {
                model = "gpt-4o",

                messages = new[]
                {
                    new
                    {
                        role = "user",
                        content = new List<object>
                        {
                            new { type = "text", text = userPrompt },
                            new { type = "image_url", image_url = new { url = $"data:image/jpeg;base64,{base64Image}" } }
                        }
                    }
                },
                max_tokens = maxTokens,
            };

            string jsonOptions = JsonConvert.SerializeObject(options);

            using (UnityWebRequest www = UnityWebRequest.PostWwwForm(apiUrl, " "))
            {
                www.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonOptions));
                www.uploadHandler.contentType = "application/json";
                www.SetRequestHeader("Authorization", $"Bearer {apiKey}");
                www.SetRequestHeader("Content-Type", "application/json");

                await www.SendWebRequest().ToUniTask();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"Error: {www.error}\nResponse: {www.downloadHandler.text}");
                }
                else
                {
                    ChatGPTResponseModel response = JsonConvert.DeserializeObject<ChatGPTResponseModel>(www.downloadHandler.text);

                    string description = response.choices[0].message.content;
                    ReplyMessage = description;
                    MessageList.Add(description);

                    // カロリー情報と栄養素情報を抽出して格納する
                    ExtractCalorieAndNutrientsFromResponse(description);
                }
            }
        }
    }

    // ChatGPT APIからResponseを受け取るためのクラス

    [Serializable]
    public class ChatGPTResponseModel
    {
        // 応答するID？
        public string id;

        // オブジェクトのタイプ？
        public string @object;

        // 応答の作成時間
        public int created;

        // 応答の選択肢
        public Choice[] choices;

        // 使用されたトークン数
        public Usage usage;

        [Serializable]
        public class Choice
        {
            // 選択肢のインデックス
            public int index;

            // 選択されたメッセージ
            public ChatGPTMessageModel message;

            // 応答が終了したと時の理由
            public string finish_reason;
        }

        // ChatGPTのトークンに関する内容？
        // トークンの利用状況が入るかも
        [Serializable]
        public class Usage
        {
            // プロンプトのトークン数？
            public int prompt_tokens;

            // 応答のトークン数
            public int completion_tokens;

            // 合計のトークン数
            public int total_tokens;
        }

        [Serializable]
        public class Message
        {
            public string content;
        }
    }

    // メッセージの役割と内容を定義するクラス

    [Serializable]
    public class ChatGPTMessageModel
    {
        // メッセージの役割
        public string role;

        // メッセージの内容
        public string content;
    }

    // ChatGPT APIにRequestを送るためのJSON用クラス
    [Serializable]
    public class ChatGPTCompletionRequestModel
    {
        // 使用するモデルのバージョン
        public string model;

        // 会話を保存するリスト
        public List<ChatGPTMessageModel> messages;

        // 最大トークン数
        public int max_tokens;
    }
}
