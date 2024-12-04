using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityMainThreadDispatcher;

public enum HttpStateEnum : int { Free = 0, UpdateState = 2, GetState = 4, GetChat = 8, SendMessage = 16 };

public enum HttpMethod : int { GET, PATCH, POST }

public class HttpRequests : MonoBehaviour
{
    #region Variables
    [SerializeField] private int StopedRequests;
    [SerializeField] private Request currentRequest;
    [SerializeField] private Queue<Request> Requests;
    Action CancelationCallback;
    #endregion

    #region Props
    [SerializeField] private HttpStateEnum state => currentRequest == null ? HttpStateEnum.Free : currentRequest.State; // 1 for sending and 0 for free
    DewaniaHostConstants Constants => NetworkInstance.Instance.Constants;
    #endregion

    #region Unity Fns
    private void Start()
    {
        Requests = new Queue<Request>();
    }

    private void Update()
    {
        if (state == HttpStateEnum.Free)
        {
            if (Requests.TryDequeue(out Request request))
            {
                currentRequest = request;
                StartCoroutine(SendRequestIEnu(currentRequest.Url, currentRequest.Data, currentRequest.Method,
            currentRequest.IncludeBearer, currentRequest.ExtraHeaders,
            currentRequest.OnSuccess, currentRequest.OnFail));
            }
        }
    }
    #endregion

    #region Request
    public void SendRequset(string url, string data, HttpStateEnum state, HttpMethod method, bool includeBearer, Dictionary<string, string> extraHeaders,
                            Action<object> onSuccess, Action<Error> onFail)
    {
        Requests.Enqueue(new Request(url, data, state, method, includeBearer, extraHeaders, onSuccess, onFail));
    }

    public void ResendCurrentRequest()
    {
        if (currentRequest == null)
        {
            Debugging.Warn("current request is empty");
            return;
        }

        StartCoroutine(SendRequestIEnu(currentRequest.Url, currentRequest.Data, currentRequest.Method,
            currentRequest.IncludeBearer, currentRequest.ExtraHeaders,
            currentRequest.OnSuccess, currentRequest.OnFail));
    }

    private IEnumerator SendRequestIEnu(string url, string data, HttpMethod method, bool includeBearer,
        Dictionary<string, string> extraHeaders, Action<object> onSuccess, Action<Error> onFail)
    {
        Debugging.Print($"start sending request {url} : {data} : {method}");

        byte[] body = data != null ? Encoding.UTF8.GetBytes(data) : null;

        using (UnityWebRequest webRequest = new UnityWebRequest(url, method.ToString(), new DownloadHandlerBuffer(),
            new UploadHandlerRaw(body)))
        {
            webRequest.timeout = Constants.HttpTimeout;

            AddMainHeaders(webRequest);

            if (includeBearer)
                webRequest.SetRequestHeader("Authorization", "Bearer " + DewaniaSession.AccessToken);
            if (extraHeaders != null)
                foreach (KeyValuePair<string, string> keyValuePair in extraHeaders)
                    webRequest.SetRequestHeader(keyValuePair.Key, keyValuePair.Value);

            webRequest.SendWebRequest();

            bool redirectLimit = false;
            bool Continue = true;
            yield return new WaitUntil(() =>
            {
                if (currentRequest == null)
                    currentRequest = new Request(webRequest.url, data, state, GetMethod(webRequest.method), includeBearer, extraHeaders, onSuccess, onFail);

                Continue = (StopedRequests & (int)currentRequest.State) == 0;
                if (!Continue)
                    return true;

                if (webRequest.redirectLimit > Constants.HttpRedirectLimit)
                {
                    redirectLimit = true;
                    return true;
                }

                return webRequest.isDone;
            });

            FinishCurrentRequest(Continue);
            if (!Continue)
            {
                StopedRequests -= ((int)currentRequest.State);
                yield break;
            }

            if (redirectLimit)
            {
                Debugging.Print($"request limit out {webRequest.redirectLimit}");
                onFail?.Invoke(new Error(ErrorType.RequestRedirectLimitOut));
                yield break;
            }

            Debugging.Print("sent http request = ", webRequest);
            Debugging.Print("sent http request = ", webRequest.result, webRequest.downloadHandler.text);

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                currentRequest = null;
                onSuccess?.Invoke(webRequest.downloadHandler.text);
            }
            else if (webRequest.downloadHandler != null && !string.IsNullOrEmpty(webRequest.downloadHandler.text))
            {
                OnHostResponseWithError(webRequest.downloadHandler.text, onFail);
            }
            else
            {
                onFail?.Invoke(new Error(ErrorType.NetwrokConnection));
            }
        }
    }

    private HttpMethod GetMethod(string method)
    {
        switch (method)
        {
            case "Get":
                return HttpMethod.GET;
            case "POST":
                return HttpMethod.POST;
            default:
                return HttpMethod.PATCH;
        }
    }

    private static void OnHostResponseWithError(object data, Action<Error> onFail)
    {
        Dispatcher.Enqueue(() =>
        {
            JObject obj = JObject.Parse(data.ToString());
            if (obj.TryGetValue("errorMessage", out JToken token))
            {
                if (ErrorMessageEqualTo(token, "invalid update sequence"))
                {
                    DewaniaHostController.GetGameState(DewaniaHostController.OnGettingGameStateSuccess);
                }
                else if (ErrorMessageEqualTo(token, "no game found"))
                {
                    onFail?.Invoke(new Error(ErrorType.GameEnded));
                }
                else if (ErrorMessageEqualTo(token, "jwt malformed"))
                {
                    onFail?.Invoke(new Error(ErrorType.AccessTokenMissing));
                }
                else if (ErrorMessageEqualTo(token, "jwt expired"))
                {
                    onFail?.Invoke(new Error(ErrorType.AccessTokenExpired));
                }
                else if (ErrorMessageEqualTo(token, "invalid token"))
                {
                    onFail?.Invoke(new Error(ErrorType.AccessTokenInvalid));
                }
                else
                {
                    onFail?.Invoke(new Error(ErrorType.Unknonw));
                }
            }
        });
    }

    private static bool ErrorMessageEqualTo(JToken token, string message)
    {
        try
        {
            if ((string)token == message)
                return true;
        }
        catch (Exception e)
        {
            Debugging.Error("while parssing error message " + e.Message);
        }

        return false;
    }

    private void FinishCurrentRequest(bool Continue)
    {
        if (Continue)
        {
            CancelationCallback?.Invoke();
            CancelationCallback = null;
            StopedRequests = 0;
        }
    }

    private void AddMainHeaders(UnityWebRequest webRequest)
    {
        for (int i = 0, count = Constants.MainHttpHeaders_Keys.Count; i < count; i++)
        {
            webRequest.SetRequestHeader(Constants.MainHttpHeaders_Keys[i], Constants.MainHttpHeaders_Values[i]);
        }
    }
    #endregion

    #region Fns
    public void CancelGetRequest(Action OnCancel)
    {
        if (state == HttpStateEnum.Free)
        {
            OnCancel?.Invoke();
            return;
        }

        Requests = new Queue<Request>(Requests.Where(o => o.State != HttpStateEnum.GetState));

        if (state == HttpStateEnum.GetState)
        {
            CancelationCallback += OnCancel;
            StopedRequests = ((int)HttpStateEnum.GetState);
        }
        else
        {
            OnCancel?.Invoke();
        }
    }

    public void CancelUpdateRequest(Action OnCancel)
    {
        if (state == HttpStateEnum.Free)
        {
            OnCancel?.Invoke();
            return;
        }

        Requests = new Queue<Request>(Requests.Where(o => o.State != HttpStateEnum.UpdateState));

        if (state == HttpStateEnum.UpdateState)
        {
            CancelationCallback += OnCancel;
            StopedRequests = ((int)HttpStateEnum.UpdateState);
        }
        else
        {
            OnCancel?.Invoke();
        }
    }

    public void CancelStateRequests(Action OnCancel)
    {
        if (state == HttpStateEnum.Free)
        {
            OnCancel?.Invoke();
            return;
        }

        Requests = new Queue<Request>(Requests.Where(o => o.State != HttpStateEnum.GetState && o.State != HttpStateEnum.UpdateState));

        if (state == HttpStateEnum.GetState || state == HttpStateEnum.UpdateState)
        {
            CancelationCallback += OnCancel;
            StopedRequests = ((int)HttpStateEnum.GetState) & ((int)HttpStateEnum.UpdateState);
        }
        else
        {
            OnCancel?.Invoke();
        }
    }

    public void CancelGetChatRequests(Action OnCancel)
    {
        if (state == HttpStateEnum.Free)
        {
            OnCancel?.Invoke();
            return;
        }

        Requests = new Queue<Request>(Requests.Where(o => o.State != HttpStateEnum.GetChat));

        if (state == HttpStateEnum.GetChat)
        {
            CancelationCallback += OnCancel;
            StopedRequests = ((int)HttpStateEnum.GetChat);
        }
        else
        {
            OnCancel?.Invoke();
        }
    }

    public void CancelSendMessageRequests(Action OnCancel)
    {
        if (state == HttpStateEnum.Free)
        {
            OnCancel?.Invoke();
            return;
        }

        Requests = new Queue<Request>(Requests.Where(o => o.State != HttpStateEnum.SendMessage));

        if (state == HttpStateEnum.SendMessage)
        {
            CancelationCallback += OnCancel;
            StopedRequests = ((int)HttpStateEnum.SendMessage);
        }
        else
        {
            OnCancel?.Invoke();
        }
    }

    public void CancelAnyReqeust(Action OnCancel)
    {
        if (state == HttpStateEnum.Free)
        {
            OnCancel?.Invoke();
            return;
        }

        Requests.Clear();
        CancelationCallback += OnCancel;
        StopedRequests = int.MaxValue;
    }
    #endregion
}

[Serializable]
public class Request
{
    public Request(string url, string data, HttpStateEnum state, HttpMethod method, bool includeBearer,
        Dictionary<string, string> extraHeaders,
        Action<object> onSuccess, Action<Error> onFail)
    {
        this.url = url;
        this.data = data;
        this.method = method;
        this.includeBearer = includeBearer;
        this.extraH = extraHeaders;
        this.onSuccess = onSuccess;
        this.onFail = onFail;
    }

    [SerializeField] private string url;

    public string Url
    {
        get { return url; }
    }

    [SerializeField] private string data;

    public string Data
    {
        get { return data; }
    }

    [SerializeField] private HttpStateEnum state;

    public HttpStateEnum State
    {
        get { return state; }
    }

    [SerializeField] private HttpMethod method;

    public HttpMethod Method
    {
        get { return method; }
    }

    [SerializeField] private bool includeBearer;

    public bool IncludeBearer
    {
        get { return includeBearer; }
    }

    private Dictionary<string, string> extraH;

    public Dictionary<string, string> ExtraHeaders
    {
        get { return extraH; }
    }

    private Action<object> onSuccess;

    public Action<object> OnSuccess
    {
        get { return onSuccess; }
    }

    private Action<Error> onFail;

    public Action<Error> OnFail
    {
        get { return onFail; }
    }

    public override string ToString()
    {
        return JsonConvert.SerializeObject(this);
    }

    internal void SendRequest()
    {
        throw new NotImplementedException();
    }
}