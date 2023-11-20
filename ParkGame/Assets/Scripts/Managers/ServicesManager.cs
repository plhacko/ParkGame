using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Net.Security;
using System.Resources;
using System.Threading.Tasks;
using Firebase;
using Firebase.Auth;
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
    All = UnityServices | FirebaseAuth | UnityAuth
}

public class ServicesManager : MonoBehaviour
{
    public static ServicesManager Instance;

    private ServiceType state = ServiceType.None;
    public ServiceType State => state;

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
        state = ServiceType.None;

        await InitializeUnityServices();     

        if (IsSignedToFirebase())
            state |= ServiceType.FirebaseAuth;
        if (AreInitializedUnityServices())
            state |= ServiceType.UnityServices;
        if (IsSignedToUnityAuth())
            state |= ServiceType.UnityAuth;
    }

    private async Task InitializeUnityServices()
    {
        await UnityServices.InitializeAsync();
        state |= ServiceType.UnityServices;
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

        state |= ServiceType.FirebaseAuth;
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

        state |= ServiceType.FirebaseAuth;
        return FirebaseAuthServiceError.None;
    }

    public bool IsSignedToFirebase()
    {
        return FirebaseAuth.DefaultInstance.CurrentUser != null;
    }

    public async Task SignUpAndLogInToUnityAuth()
    {
        if (state < ServiceType.UnityServices)
            throw new System.Exception("Unity Services are not initialized");

#if UNITY_EDITOR
        if (ParrelSync.ClonesManager.IsClone())
        {
            string customArgument = ParrelSync.ClonesManager.GetArgument();
            AuthenticationService.Instance.SwitchProfile($"Clone_{customArgument}_Profile");
        }
#endif
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        state |= ServiceType.UnityAuth;
    }

    public bool IsSignedToUnityAuth()
    {
        return AuthenticationService.Instance.IsSignedIn;
    }

    public void SignOutFromFirebase()
    {
        state = ServiceType.All ^ ServiceType.FirebaseAuth;
        FirebaseAuth.DefaultInstance.SignOut();
    }

    public void SignOutFromUnityAuth()
    {
        state = ServiceType.All ^ ServiceType.UnityAuth;
        AuthenticationService.Instance.SignOut();
    }

    public void SignOutFromAll()
    {
        SignOutFromFirebase();
        SignOutFromUnityAuth();
    }

}
