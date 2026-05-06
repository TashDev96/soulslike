using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Soulslike.Application
{
    public class BackendClient
    {
        public string BaseUrl { get; set; } = "http://localhost:5189";
        public string Token { get; set; }

        public bool IsLoggedIn => !string.IsNullOrEmpty(Token);

        public IEnumerator Login(string deviceId, Action<string> onSuccess, Action<string> onError)
        {
            var json = $"{{\"deviceId\": \"{deviceId}\"}}";
            var request = new UnityWebRequest($"{BaseUrl}/auth/login", "POST");
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                onError?.Invoke(request.error + ": " + request.downloadHandler.text);
            }
            else
            {
                var response = JsonUtility.FromJson<LoginResponse>(request.downloadHandler.text);
                Token = response.token;
                onSuccess?.Invoke(Token);
            }
        }

        public IEnumerator GetSave(Action<string> onSuccess, Action<string> onError)
        {
            var request = UnityWebRequest.Get($"{BaseUrl}/save");
            request.SetRequestHeader("Authorization", $"Bearer {Token}");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                onError?.Invoke(request.error + ": " + request.downloadHandler.text);
            }
            else
            {
                onSuccess?.Invoke(request.downloadHandler.text);
            }
        }

        public IEnumerator UpdateSave(string jsonSaveData, Action onSuccess, Action<string> onError)
        {
            var request = new UnityWebRequest($"{BaseUrl}/save", "POST");
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonSaveData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", $"Bearer {Token}");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                onError?.Invoke(request.error + ": " + request.downloadHandler.text);
            }
            else
            {
                onSuccess?.Invoke();
            }
        }

        public IEnumerator SaveWriting(WritingData data, Action onSuccess, Action<string> onError)
        {
            var json = JsonUtility.ToJson(data);
            var request = new UnityWebRequest($"{BaseUrl}/writings", "POST");
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", $"Bearer {Token}");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                onError?.Invoke(request.error + ": " + request.downloadHandler.text);
            }
            else
            {
                onSuccess?.Invoke();
            }
        }

        public IEnumerator GetWritings(string locationId, Action<WritingData[]> onSuccess, Action<string> onError)
        {
            var request = UnityWebRequest.Get($"{BaseUrl}/writings/{locationId}");
            request.SetRequestHeader("Authorization", $"Bearer {Token}");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                onError?.Invoke(request.error + ": " + request.downloadHandler.text);
            }
            else
            {
                // JsonUtility cannot parse top-level arrays directly
                string json = "{\"items\":" + request.downloadHandler.text + "}";
                var wrapper = JsonUtility.FromJson<WritingsWrapper>(json);
                onSuccess?.Invoke(wrapper.items);
            }
        }

        [Serializable]
        private class LoginResponse
        {
            public string token;
        }

        [Serializable]
        private class WritingsWrapper
        {
            public WritingData[] items;
        }
    }

    [Serializable]
    public class WritingData
    {
        public string locationId;
        public float[] position;
        public float[] rotation;
        public int[] wordIndexes;
    }
}
