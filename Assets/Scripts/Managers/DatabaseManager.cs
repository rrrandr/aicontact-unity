using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

public class DatabaseManager : MonoBehaviour
{
    private string backendURL = "https://snapcamera-be.invo.zone/api/user/";

    public UserData currentUser;

    public static DatabaseManager _instance;
    public static DatabaseManager Instance
    {
        get { return _instance; }
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
    }

    public void SignInUser(LoginData loginDate, Action<UnityWebRequest, LoginData> onCompleted)
    {
        StartCoroutine(LoginCouroutine(loginDate, onCompleted));
    }

    private IEnumerator LoginCouroutine(LoginData loginDate, Action<UnityWebRequest, LoginData> onCompleted)
    {
        using (UnityWebRequest request = new UnityWebRequest(backendURL + "login", "POST"))
        {
            string jsonBody = JsonConvert.SerializeObject(loginDate);
            byte[] rawBody = new System.Text.UTF8Encoding().GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(rawBody);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("API Log: User logged in Successfully!");
                Debug.Log("API Log: " + request.downloadHandler.text);
                onCompleted?.Invoke(request, loginDate);
            }
            else
            {
                Debug.Log("API Log: User login Failed!");
                Debug.Log("API Log: " + request.downloadHandler.text);
                onCompleted?.Invoke(request, loginDate);
            }
        };
    }

    public void SignUpUser(UserData userData, Action<UnityWebRequest> onCompleted)
    {
        StartCoroutine(RegisterUserCouroutine(userData, onCompleted));
    }

    private IEnumerator RegisterUserCouroutine(UserData user, Action<UnityWebRequest> onCompleted)
    {
        using (UnityWebRequest request = new UnityWebRequest(backendURL + "register", "POST"))
        {
            string jsonBody = JsonConvert.SerializeObject(user);
            byte[] rawBody = new System.Text.UTF8Encoding().GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(rawBody);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("API Log: User registered Successfully!");
                Debug.Log("API Log: " + request.downloadHandler.text);
                currentUser = JsonConvert.DeserializeObject<Response>(request.downloadHandler.text).user;
                onCompleted?.Invoke(request);
            }
            else
            {
                Debug.Log("API Log: User registration Failed!");
                Debug.Log("API Log: " + request.downloadHandler.text);
                onCompleted?.Invoke(request);
            }
        };
    }

    public void GetUser(string email, Action<UnityWebRequest, UserData> onCompleted)
    {
        StartCoroutine(GetUserCouroutine(email, onCompleted));
    }

    public IEnumerator GetUserCouroutine(string email, Action<UnityWebRequest, UserData> onCompleted)
    {
        using (UnityWebRequest request = new UnityWebRequest(backendURL + email, "GET"))
        {
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            yield return request.SendWebRequest();

            Response response = new();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("API Log: User data fetched Successfully!");
                Debug.Log("API Log:  " + request.downloadHandler.text);
                response = JsonConvert.DeserializeObject<Response>(request.downloadHandler.text);
                currentUser = response.user;
                onCompleted?.Invoke(request, response.user);
            }
            else
            {
                Debug.Log("API Log: " + request.downloadHandler.text);
                onCompleted?.Invoke(request, response.user);
            }
        };
    }

    public void UpdateUser(UserData userData, Action<UnityWebRequest, UserData> onCompleted)
    {
        StartCoroutine(UpdateUserCouroutine(userData, onCompleted));
    }

    public IEnumerator UpdateUserCouroutine(UserData userData, Action<UnityWebRequest, UserData> onCompleted)
    {
        using (UnityWebRequest request = new UnityWebRequest(backendURL + "update", "PATCH"))
        {
            userData.email = currentUser.email;
            string jsonBody = JsonConvert.SerializeObject(userData);
            byte[] rawBody = new System.Text.UTF8Encoding().GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(rawBody);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("API Log: User data updated Successfully!");
                Debug.Log("API Log:  " + request.downloadHandler.text);
                currentUser = JsonConvert.DeserializeObject<Response>(request.downloadHandler.text).user;
                onCompleted?.Invoke(request, userData);
            }
            else
            {
                Debug.Log("API Log: " + request.downloadHandler.text);
                onCompleted?.Invoke(request, userData);
            }
        };
    }

}

[Serializable]
public class UserData
{
    public string email;
    public string password;
    public string subscription_date;
    public string terms_accepted;
}

public struct LoginData
{
    public string email;
    public string password;
}

public struct Response
{
    public string code;
    public string status;
    public string message;
    public UserData user;
}
