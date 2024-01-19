using System;
using System.Collections;
using System.Collections.Generic;
using Mapbox.Examples;
using Mapbox.Unity.Map;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIMapSelectPageController : UIPageController
{
    [SerializeField] private QuadTreeCameraMovement quadTreeCameraMovement;
    [SerializeField] private Button currentLocationButton;
    [SerializeField] private Toggle mapSelectToggle;
    [SerializeField] private Button mapCreatorButton;
    [SerializeField] private MapSpriteBuilder mapSpriteBuilder;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private string mainMenuSceneName = "Menu";
    [SerializeField] private string mapCreatorSceneName = "MapDrawingScene";
    private AbstractMap map;

    public override void OnEnter() {}

    public override void OnExit() {}

    private void Awake()
    {
        mapSelectToggle.onValueChanged.AddListener(OnToggleValueChanged);
        mapCreatorButton.onClick.AddListener(OnMapCreatorButtonClicked);
        mainMenuButton.onClick.AddListener(OnMainMenuButtonClicked);
        currentLocationButton.onClick.AddListener(OnCurrentLocationButtonClicked);
        map = quadTreeCameraMovement.GetComponent<AbstractMap>();
    }

    private void Start() 
    {
        if (GPSLocator.instance != null && GPSLocator.instance.IsGPSUsable())
        {
            OnCurrentLocationButtonClicked();
        }
    }

    private string GetCurrentLocation()
    {
        if (GPSLocator.instance != null && GPSLocator.instance.IsGPSUsable())
        {
            var lat = GPSLocator.instance.Lattitude;
            var lon = GPSLocator.instance.Longitude;
            return $"{lat}, {lon}";
        }
        else
        {
            // TODO remove this default location
            return "50.0840313988596, 14.423742114802605";
        }
    }

    private void OnCurrentLocationButtonClicked()
    {
        map._options.locationOptions.latitudeLongitude = GetCurrentLocation();
        map._options.locationOptions.zoom = 15f;
        map.UpdateMap();
    }

    private void OnMainMenuButtonClicked()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }

    private void OnMapCreatorButtonClicked()
    {
        mapSpriteBuilder.InstantiateMapSprite();
    }

    private void OnToggleValueChanged(bool arg0)
    {
        mapCreatorButton.interactable = arg0;
        quadTreeCameraMovement.SelectRegionOnValueChanged(mapSelectToggle);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OnMainMenuButtonClicked();
        }
    }
}
