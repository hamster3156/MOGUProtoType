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
        // OpenAI��API�L�[
        private readonly string apiKey;

        // ��b������ێ����郊�X�g
        private readonly List<ChatGPTMessageModel> _messageList;

        // API��URL
        private readonly string apiUrl = "https://api.openai.com/v1/chat/completions";

        public List<string> MessageList = new List<string>();

        // �J�����[�����i�[����t�B�[���h
        public int Calorie { get; private set; }

        // �h�{�f�����i�[����t�B�[���h
        public string Nutrients { get; private set; }

        public string ReplyMessage { get; private set; }

        // �R���X�g���N�^
        public ChatGPTConnection(string apiKey)
        {
            this.apiKey = apiKey;

            // ���X�g�����������ď��񃁃b�Z�[�W�𑗐M���Ă���
            _messageList = new List<ChatGPTMessageModel>
            {
                new ChatGPTMessageModel { role = "system", content = "���̃��b�Z�[�W���͂��Ă���ꍇ�A�ڑ����܂����ƕԓ�����" }
            };
        }

        /// <summary>
        /// ChatGPT�Ƀ��b�Z�[�W�̃��N�G�X�g�𑗐M����
        /// </summary>
        public async UniTask<ChatGPTResponseModel> RequestAsync(string userMessage, int maxTokens)
        {
            // ���[�U�[���`���b�g�𑗂������e�����X�g�ɒǉ����� 
            _messageList.Add(new ChatGPTMessageModel { role = "user", content = userMessage });

            // OpenAI��API���N�G�X�g�ɕK�v�ȃw�b�_�[����ݒ�
            var headers = new Dictionary<string, string>
            {
                { "Authorization", "Bearer " + apiKey },
                { "Content-type", "application/json" },
                { "X-Slack-No-Retry", "1" }
            };

            // ���͐����ŗ��p���郂�f��(ChatGPT�̃o�[�W����)��g�[�N������A�v�����v�g���I�v�V�����ɐݒ�
            var options = new ChatGPTCompletionRequestModel()
            {
                // ChatGPT�̃o�[�W�����������Ŏw�肷��
                model = "gpt-3.5-turbo",
                messages = _messageList,
                max_tokens = maxTokens,
            };
            var jsonOptions = JsonUtility.ToJson(options);

            // �f�o�b�N�Ŏ��������������b�Z�[�W��\������
            Debug.Log("[����]�F" + userMessage);

            // OpenAI�̕��͐���(Completion)��API�̃��N�G�X�g�𑗂�A���ʂ�ϐ��Ɋi�[����
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

                // ChatGPT�̕ԓ����f�o�b�N�ɕ\�����āA���X�g�ɓ��e��ǉ�����
                Debug.Log("[ChatGPT]�F" + responseObject.choices[0].message.content);
                _messageList.Add(responseObject.choices[0].message);

                // �J�����[���Ɖh�{�f���𒊏o���Ċi�[����
                ExtractCalorieAndNutrientsFromResponse(responseObject.choices[0].message.content);

                return responseObject;
            }
        }

        // ���X�|���X����J�����[���Ɖh�{�f���𒊏o���Ċi�[���郁�\�b�h
        private void ExtractCalorieAndNutrientsFromResponse(string responseContent)
        {
            // �J�����[���𒊏o���鐳�K�\��
            Regex calorieRegex = new Regex(@"�J�����[\s*[:�F]\s*��?(\d+)\s*kcal", RegexOptions.IgnoreCase);
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

            // �h�{�f���𒊏o���鐳�K�\��
            Regex nutrientsRegex = new Regex(@"�h�{�f\s*[:�F]\s*(.*)", RegexOptions.IgnoreCase);
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
            // �e�N�X�`�����o�C�g�z��ɕϊ����āABase64�G���R�[�h����
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

                    // �J�����[���Ɖh�{�f���𒊏o���Ċi�[����
                    ExtractCalorieAndNutrientsFromResponse(description);
                }
            }
        }
    }

    // ChatGPT API����Response���󂯎�邽�߂̃N���X

    [Serializable]
    public class ChatGPTResponseModel
    {
        // ��������ID�H
        public string id;

        // �I�u�W�F�N�g�̃^�C�v�H
        public string @object;

        // �����̍쐬����
        public int created;

        // �����̑I����
        public Choice[] choices;

        // �g�p���ꂽ�g�[�N����
        public Usage usage;

        [Serializable]
        public class Choice
        {
            // �I�����̃C���f�b�N�X
            public int index;

            // �I�����ꂽ���b�Z�[�W
            public ChatGPTMessageModel message;

            // �������I�������Ǝ��̗��R
            public string finish_reason;
        }

        // ChatGPT�̃g�[�N���Ɋւ�����e�H
        // �g�[�N���̗��p�󋵂����邩��
        [Serializable]
        public class Usage
        {
            // �v�����v�g�̃g�[�N�����H
            public int prompt_tokens;

            // �����̃g�[�N����
            public int completion_tokens;

            // ���v�̃g�[�N����
            public int total_tokens;
        }

        [Serializable]
        public class Message
        {
            public string content;
        }
    }

    // ���b�Z�[�W�̖����Ɠ��e���`����N���X

    [Serializable]
    public class ChatGPTMessageModel
    {
        // ���b�Z�[�W�̖���
        public string role;

        // ���b�Z�[�W�̓��e
        public string content;
    }

    // ChatGPT API��Request�𑗂邽�߂�JSON�p�N���X
    [Serializable]
    public class ChatGPTCompletionRequestModel
    {
        // �g�p���郂�f���̃o�[�W����
        public string model;

        // ��b��ۑ����郊�X�g
        public List<ChatGPTMessageModel> messages;

        // �ő�g�[�N����
        public int max_tokens;
    }
}
