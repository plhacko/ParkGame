using System;
using System.Collections;
using UnityEngine;

public class GPSLocator : MonoBehaviour
{
    public static GPSLocator instance = null;

    [SerializeField]
    private int initializationTimeout = 15;

    [SerializeField]
    private float invokeDelay = 0.5f;

    [SerializeField]
    private float updateRate = 1.0f;

    public double Longitude { get { return lon; } }
    public double Lattitude { get { return lat; } }
    public double HorizontalAccuracy { get { return horAcc; } }

    private double lon;
    private double lat;
    private double horAcc;

    public Action OnLocationInitialized;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            if (instance != this)
            {
                Destroy(gameObject);
            }
        }
    }

    public void Initialize()
    {
        if (Input.location.status != LocationServiceStatus.Running)
        {
            StartCoroutine(InitializeLocator());
        }
    }

    IEnumerator InitializeLocator()
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
            yield break;
        }

#elif UNITY_IOS
        //TODO : Check if fine location permission is enabled

        if (!UnityEngine.Input.location.isEnabledByUser) {
            // TODO Failure
            Debug.Log("IOS and Location not enabled");
            yield break;
        }
#endif
        // Start service before querying location
        // with the best precision 
        Input.location.Start(1f, 1f);

        // Wait until service initializes
        int maxWait = initializationTimeout;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            Debug.Log("Waiting");
            yield return new WaitForSecondsRealtime(1);
            maxWait--;
        }

        // Service didn't initialize in 15 seconds
        if (maxWait < 1)
        {
            // TODO Failure
            Debug.LogFormat("Timed out");
            yield break;
        }

        // Connection has failed
        if (Input.location.status != LocationServiceStatus.Running)
        {
            // TODO Failure
            Debug.LogFormat("Unable to determine device location. Failed with status {0}", UnityEngine.Input.location.status);
            yield break;
        }
        else
        {
            OnLocationInitialized?.Invoke();
            Debug.LogFormat("Location service live. status {0}", UnityEngine.Input.location.status);
            // Access granted and location value could be retrieved
            InvokeRepeating("UpdateLocation", invokeDelay, updateRate);
        }
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
            Input.location.Stop();
        }
    }


    // TODO Check if GPS is enabled
    private void UpdateLocation()
    {
        lat = Input.location.lastData.latitude;
        lon = Input.location.lastData.longitude;
        horAcc = Input.location.lastData.horizontalAccuracy;
    }

    public bool IsLocationServiceEnabled()
    {
        return Input.location.isEnabledByUser;
    }

    public bool IsLocationServiceRunning()
    {
        return Input.location.status == LocationServiceStatus.Running;
    }

    public bool IsGPSUsable()
    {
        return IsLocationServiceEnabled() && IsLocationServiceRunning();
    }
}