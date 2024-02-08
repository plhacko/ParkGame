using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Net.Security;
using System.Resources;
using System.Threading.Tasks;
using Firebase;
using Firebase.Auth;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay.Models;
using UnityEngine;

[Flags]
public enum ServiceType
{
    None = 0,
    UnityServices = 1,
    UnityAuth = 2,
    FirebaseAuth = 4,
    GPS = 8,
    All = UnityServices | FirebaseAuth | UnityAuth | GPS
}

public class ServicesManager : MonoBehaviour
{
    public static ServicesManager Instance;

    public ServiceType State = ServiceType.None;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);
            Initialize();
        }
        else
        {
            if (Instance != this)
            {
                Destroy(gameObject);
            }
        }
    }

    private async void Initialize()
    {
        State = ServiceType.None;    

        await InitializeUnityServices();     

        if (IsSignedToFirebase())
            State |= ServiceType.FirebaseAuth;
        if (AreInitializedUnityServices())
            State |= ServiceType.UnityServices;
        if (IsSignedToUnityAuth())
            State |= ServiceType.UnityAuth;
        if (IsLocationServiceInitialized())
            State |= ServiceType.GPS;
    }

    private async Task InitializeUnityServices()
    {
        if (AreInitializedUnityServices())
            return;

        Debug.Log("Initializing Unity Services");
        await UnityServices.InitializeAsync();
        State |= ServiceType.UnityServices;
    }

    public bool AreInitializedUnityServices()
    {
        return UnityServices.State == ServicesInitializationState.Initialized;
    }

    public async Task<FirebaseAuthServiceError> LogInToFirebase(string email, string password)
    {
        // Login user
        AuthResult result = null;
        var auth = FirebaseAuth.DefaultInstance;
        await auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWith(
            task => 
            {
                if (task.IsCanceled)
                {
                    Debug.LogError("SignInWithEmailAndPasswordAsync was canceled.");
                    return;
                }

                if (task.IsFaulted)
                {
                    AggregateException ex = task.Exception;
                    
                    if (ex != null) {
                        foreach (Exception e in ex.InnerExceptions) {
                            if (e is FirebaseException fbEx)
                            {
                                Debug.LogError("Encountered a FirebaseException:" + fbEx.Message);
                            }
                        }
                    }

                    Debug.LogError("SignInWithEmailAndPasswordAsync encountered an error: " + task.Exception);
                    return;
                }

                result = task.Result;
                Debug.LogFormat("User signed in successfully: {0} ({1})", result.User.DisplayName, result.User.UserId);
            }
        );

        if (result == null)
        {
            Debug.LogError("Login failed");
            return FirebaseAuthServiceError.FailedToLoginUser;
        }

        State |= ServiceType.FirebaseAuth;
        return FirebaseAuthServiceError.None;
    }

    public enum FirebaseAuthServiceError
    {
        None = 0,
        FailedToCreateUser = 1,
        FailedToLoginUser = 2

    }

    public async Task<FirebaseAuthServiceError> SignUpAndLoginToFirebase(string email, string password, string name)
    {
        // Register user 
        AuthResult result = null;
        var auth = FirebaseAuth.DefaultInstance;
        await auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWith(
            task => 
            {
                if (task.IsCanceled) {
                    Debug.LogError("CreateUserWithEmailAndPasswordAsync was canceled.");
                    return;
                }
                if (task.IsFaulted) {
                    Debug.LogError("CreateUserWithEmailAndPasswordAsync encountered an error: " + task.Exception);
                    return;
                }
                
                result = task.Result;
                Debug.LogFormat("Firebase user created successfully: {0} ({1})", result.User.DisplayName, result.User.UserId);
            }
        );

        if (result == null)
        {
            Debug.LogError("Failed to create user");
            return FirebaseAuthServiceError.FailedToCreateUser;
        }

        var userProfile = new UserProfile();
        userProfile.DisplayName = name;
        await result.User.UpdateUserProfileAsync(userProfile);

        // Login user
        AuthResult loginResult = null;
        await auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWith(
            task => 
            {
                if (task.IsCanceled) {
                    Debug.LogError("SignInWithEmailAndPasswordAsync was canceled.");
                    return;
                }
                if (task.IsFaulted) {
                    Debug.LogError("SignInWithEmailAndPasswordAsync encountered an error: " + task.Exception);
                    return;
                }

                loginResult = task.Result;
                Debug.LogFormat("User signed in successfully: {0} ({1})", result.User.DisplayName, result.User.UserId);
            }
        );

        if (loginResult == null)
        {
            Debug.LogError("Failed to login user");
            return FirebaseAuthServiceError.FailedToLoginUser;
        }

        State |= ServiceType.FirebaseAuth;
        return FirebaseAuthServiceError.None;
    }

    public bool IsSignedToFirebase()
    {
        return FirebaseAuth.DefaultInstance.CurrentUser != null;
    }

    public async Task SignUpAndLogInToUnityAuth()
    {
        if (State < ServiceType.UnityServices)
            throw new System.Exception("Unity Services are not initialized");

#if UNITY_EDITOR
        if (ParrelSync.ClonesManager.IsClone())
        {
            string customArgument = ParrelSync.ClonesManager.GetArgument();
            AuthenticationService.Instance.SwitchProfile($"Clone_{customArgument}_Profile");
        }
#endif
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        State |= ServiceType.UnityAuth;
    }

    public bool IsSignedToUnityAuth()
    {
        return AuthenticationService.Instance.IsSignedIn;
    }

    public void SignOutFromFirebase()
    {
        State = ServiceType.All ^ ServiceType.FirebaseAuth;
        FirebaseAuth.DefaultInstance.SignOut();
    }

    public void SignOutFromUnityAuth()
    {
        State = ServiceType.All ^ ServiceType.UnityAuth;
        AuthenticationService.Instance.SignOut();
    }

    public void SignOutFromAll()
    {
        SignOutFromFirebase();
        SignOutFromUnityAuth();
    }

    public bool IsLocationServiceInitialized()
    {
        return UnityEngine.Input.location.status == LocationServiceStatus.Running;
    }

    private bool GPSDontAskAgain = false;

    public bool GPSPermissionGranted()
    {
#if UNITY_EDITOR
        // No permission handling needed in Editor
        return true; 
#elif UNITY_ANDROID
        var fineLocation = UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.FineLocation);
        var locationEnabled = UnityEngine.Input.location.isEnabledByUser;

        return fineLocation && locationEnabled;
#elif UNITY_IOS
        //TODO
        return false;
#else 
        return true;
#endif
    }


#if UNITY_ANDROID
    void GPSPermissionGrantedCallback(string permissionName)
    {
        Debug.Log("GPS Permission Granted");
        GPSLocator.instance.Initialize();
        GPSDontAskAgain = false;
    }

    void GPSPermissionDeniedCallback(string permissionName)
    {
        Debug.Log("GPS Permission Denied");
        if (UIController.Singleton.TopStackPageName != "GPSPermission")
        {
            UIController.Singleton.ShowPopUp(
                "GPS Permission Denied",
                "You need to grant GPS permission to use this application",
                "Go to settings", () => OpenAndroidApplicationSettings(),
                "GPSPermission"
            );
        }
    }

    void GPSPermissionDeniedAndDontAskAgainCallback(string permissionName)
    {
        Debug.Log("GPS Permission Denied and Dont Ask Again");
        GPSDontAskAgain = true;
        if (UIController.Singleton.TopStackPageName != "GPSPermission")
        {
            UIController.Singleton.ShowPopUp(
                "GPS Permission Denied",
                "You need to grant GPS permission to use this application",
                "Go to settings", () => OpenAndroidApplicationSettings(),
                "GPSPermission"
            );
        }
    }
#endif

    public void RequestGPSPermission()
    {
#if UNITY_EDITOR
        // No permission handling needed in Editor
#elif UNITY_ANDROID
        var callbacks = new UnityEngine.Android.PermissionCallbacks();
        callbacks.PermissionGranted += GPSPermissionGrantedCallback;
        callbacks.PermissionDenied += GPSPermissionDeniedCallback;
        callbacks.PermissionDeniedAndDontAskAgain += GPSPermissionDeniedAndDontAskAgainCallback;

        UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.FineLocation, callbacks);
#elif UNITY_IOS
        //TODO
#else
#endif
        State |= ServiceType.GPS;
    }


#if UNITY_ANDROID
    void OpenAndroidApplicationSettings()
    {
        try
        {
            using (var unityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (AndroidJavaObject currentActivityObject = unityClass.GetStatic<AndroidJavaObject>("currentActivity"))
            {
                string packageName = currentActivityObject.Call<string>("getPackageName");
        
                using (var uriClass = new AndroidJavaClass("android.net.Uri"))
                using (AndroidJavaObject uriObject = uriClass.CallStatic<AndroidJavaObject>("fromParts", "package", packageName, null))
                using (var intentObject = new AndroidJavaObject("android.content.Intent", "android.settings.APPLICATION_DETAILS_SETTINGS", uriObject))
                {
                    intentObject.Call<AndroidJavaObject>("addCategory", "android.intent.category.DEFAULT");
                    intentObject.Call<AndroidJavaObject>("setFlags", 0x10000000);
                    currentActivityObject.Call("startActivity", intentObject);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }
#endif

    void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
        {
            Debug.Log("OnApplicationFocus");
//             if (GPSPermissionGranted())
//             {
//                 GPSDontAskAgain = false;
//             }
            
//             if (GPSDontAskAgain)
//             {
// #if UNITY_ANDROID
//                 if (UIController.Singleton.TopStackPageName != "GPSPermission")
//                 {
//                     UIController.Singleton.ShowPopUp(
//                         "GPS Permission Denied",
//                         "You need to grant GPS permission to use this application",
//                         "Go to settings", () => OpenAndroidApplicationSettings(),
//                         "GPSPermission"
//                     );
//                 }
// #endif
//             }
            if (!GPSPermissionGranted())
            {
                RequestGPSPermission();
            }

        }
    }
}
