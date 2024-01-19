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
            Destroy(gameObject);
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

    public void RequestGPSPermission()
    {
#if UNITY_EDITOR
        // No permission handling needed in Editor
#elif UNITY_ANDROID
        if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.FineLocation)) {
            UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.FineLocation);
        }

        // First, check if user has location service enabled
        if (!UnityEngine.Input.location.isEnabledByUser) {
            // TODO Failure
            Debug.Log("Android and Location not enabled");
        }

#elif UNITY_IOS
        //TODO
#else
        // No permission handling needed in Editor
#endif
        State |= ServiceType.GPS;
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
        {
            Debug.Log("OnApplicationFocus");
            if (!GPSPermissionGranted())
            {
                RequestGPSPermission();
            }
        }
    }
}
