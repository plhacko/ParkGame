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

    private void Awake() {
        mapSelectToggle.onValueChanged.AddListener(OnToggleValueChanged);
        mapCreatorButton.onClick.AddListener(OnMapCreatorButtonClicked);
        mainMenuButton.onClick.AddListener(OnMainMenuButtonClicked);
        currentLocationButton.onClick.AddListener(OnCurrentLocationButtonClicked);
        map = quadTreeCameraMovement.GetComponent<AbstractMap>();
        // TODO set location of map to the current location
    }

    private void OnCurrentLocationButtonClicked()
    {
        // TODO set location of map to the current location
        map._options.locationOptions.latitudeLongitude = "50.0840313988596, 14.423742114802605";
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
}
