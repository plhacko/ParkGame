using Unity.VisualScripting;

namespace Mapbox.Examples
{
	using Mapbox.Unity.Map;
	using Mapbox.Unity.Utilities;
	using Mapbox.Utils;
	using UnityEngine;
	using UnityEngine.EventSystems;
	using System;
    using UnityEngine.UI;


    public class QuadTreeCameraMovement : MonoBehaviour
	{
		[SerializeField]
		[Range(1, 20)]
		public float _panSpeed = 1.0f;

		[SerializeField]
		float _zoomSpeed = 0.25f;

		[SerializeField]
		public Camera _referenceCamera;

		[SerializeField]
		AbstractMap _mapManager;

		[SerializeField]
		bool _useDegreeMethod;

		private Vector3 _origin;
		private Vector3 _mousePosition;
		private Vector3 _mousePositionPrevious;
		private bool _shouldDrag;
		private bool _isInitialized = false;
		private Plane _groundPlane = new Plane(Vector3.up, 0);
		private bool _dragStartedOnUI = false;

		// Select Region
		private bool _selectingRegion = false;
		public LineRenderer lineRenderer;
		private Vector3 _firstCornerPosition;
		private Vector3 _secondCornerPosition;

		private Vector3 _initialPosition;

		private Vector3 _lastPosition;

		void Awake()
		{
			if (null == _referenceCamera)
			{
				_referenceCamera = GetComponent<Camera>();
				if (null == _referenceCamera) { Debug.LogErrorFormat("{0}: reference camera not set", this.GetType().Name); }
			}
			_mapManager.OnInitialized += () =>
			{
				_isInitialized = true;
			};
		}

		public void Update()
		{
			if (Input.GetMouseButtonDown(0) && EventSystem.current.IsPointerOverGameObject())
			{
				_dragStartedOnUI = true;
			}

			if (Input.GetMouseButtonUp(0))
			{
				_dragStartedOnUI = false;
			}
		}


		private void LateUpdate()
		{
			if (!_isInitialized) { return; }


			if (_selectingRegion && !_dragStartedOnUI)
			{
				HandleSelectRegion();
			}

			if (!_dragStartedOnUI && !_selectingRegion)
			{
				if (Input.touchSupported && Input.touchCount > 0)
				{
					HandleTouch();
				}
				else
				{
					HandleMouseAndKeyBoard();
				}
			}
		}

        private void HandleSelectRegion()
        {
			if (Input.touchPressureSupported && Input.touchCount > 0)
			{
				HandleTouchSelectRegion();
			}
			else
			{
				HandleMouseSelectRegion();
			}
        }

        private void HandleMouseSelectRegion()
        {
			Vector3 mousePosition = Input.mousePosition;
			mousePosition.z = Camera.main.transform.localPosition.y;

			if (Input.GetMouseButtonDown(0))
			{
				lineRenderer.positionCount = 4;
				_firstCornerPosition = Camera.main.ScreenToWorldPoint(mousePosition);
				lineRenderer.SetPosition(0, new Vector3(_firstCornerPosition.x, 0.1f, _firstCornerPosition.z));
				lineRenderer.SetPosition(1, new Vector3(_firstCornerPosition.x, 0.1f, _firstCornerPosition.z));
				lineRenderer.SetPosition(2, new Vector3(_firstCornerPosition.x, 0.1f, _firstCornerPosition.z));
				lineRenderer.SetPosition(3, new Vector3(_firstCornerPosition.x, 0.1f, _firstCornerPosition.z));
			}

			if (Input.GetMouseButton(0))
			{
				_secondCornerPosition = Camera.main.ScreenToWorldPoint(mousePosition);
				lineRenderer.SetPosition(0, new Vector3(_firstCornerPosition.x, 0.1f, _firstCornerPosition.z));
				lineRenderer.SetPosition(1, new Vector3(_secondCornerPosition.x, 0.1f, _firstCornerPosition.z));
				lineRenderer.SetPosition(2, new Vector3(_secondCornerPosition.x, 0.1f, _secondCornerPosition.z));
				lineRenderer.SetPosition(3, new Vector3(_firstCornerPosition.x, 0.1f, _secondCornerPosition.z));
			}
        }

		public Vector4 GetSelectedRegionBoundingBox()
		{
			var initialLatLong = _mapManager.WorldToGeoPosition(_firstCornerPosition);
			var currentLatLong = _mapManager.WorldToGeoPosition(_secondCornerPosition);

			var minLat = Math.Min(initialLatLong.x, currentLatLong.x);
			var minLong = Math.Min(initialLatLong.y, currentLatLong.y);
			var maxLat = Math.Max(initialLatLong.x, currentLatLong.x);
			var maxLong = Math.Max(initialLatLong.y, currentLatLong.y);

			return new Vector4((float)minLong, (float)minLat, (float)maxLong, (float)maxLat);
		}

		public Vector2 GetSelectedRegionNormalizedSideLengths()
		{
			var initialXY = _firstCornerPosition;
			var currentXY = _secondCornerPosition;

			var xLength = Mathf.Abs(initialXY.x - currentXY.x);
			var yLength = Mathf.Abs(initialXY.z - currentXY.z);

			return xLength > yLength ? new Vector2(1, yLength / xLength) : new Vector2(xLength / yLength, 1);
		}

        private void HandleTouchSelectRegion()
        {
			if (Input.touchCount > 1)
			{
				return;
			}

			Touch touch = Input.GetTouch(0);
			
			Vector3 touchPosition = touch.position;
			touchPosition.z = Camera.main.transform.localPosition.y;

			if (touch.phase == TouchPhase.Began)
            {
                // Save the initial touch position
                _initialPosition = Camera.main.ScreenToWorldPoint(touchPosition);
				lineRenderer.positionCount = 4;
				lineRenderer.SetPosition(0, new Vector3(_initialPosition.x, 0.1f, _initialPosition.z));
				lineRenderer.SetPosition(1, new Vector3(_initialPosition.x, 0.1f, _initialPosition.z));
				lineRenderer.SetPosition(2, new Vector3(_initialPosition.x, 0.1f, _initialPosition.z));
				lineRenderer.SetPosition(3, new Vector3(_initialPosition.x, 0.1f, _initialPosition.z));
            }
            else if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
			{            
                // Save the last touch position
                _lastPosition = Camera.main.ScreenToWorldPoint(touchPosition);
				lineRenderer.SetPosition(0, new Vector3(_initialPosition.x, 0.1f, _initialPosition.z));
				lineRenderer.SetPosition(1, new Vector3(_lastPosition.x, 0.1f, _initialPosition.z));
				lineRenderer.SetPosition(2, new Vector3(_lastPosition.x, 0.1f, _lastPosition.z));
				lineRenderer.SetPosition(3, new Vector3(_initialPosition.x, 0.1f, _lastPosition.z));
			}
			if (touch.phase == TouchPhase.Ended)
			{
				_firstCornerPosition = _initialPosition;
				_secondCornerPosition = _lastPosition;
			}
        }

        void HandleMouseAndKeyBoard()
		{
			// zoom
			float scrollDelta = 0.0f;
			scrollDelta = Input.GetAxis("Mouse ScrollWheel");
			ZoomMapUsingTouchOrMouse(scrollDelta);


			//pan keyboard
			float xMove = Input.GetAxis("Horizontal");
			float zMove = Input.GetAxis("Vertical");

			PanMapUsingKeyBoard(xMove, zMove);


			//pan mouse
			PanMapUsingTouchOrMouse();
		}

		void HandleTouch()
		{
			float zoomFactor = 0.0f;
			//pinch to zoom.
			switch (Input.touchCount)
			{
				case 1:
					{
						PanMapUsingTouchOrMouse();
					}
					break;
				case 2:
					{
						// Store both touches.
						Touch touchZero = Input.GetTouch(0);
						Touch touchOne = Input.GetTouch(1);

						// Find the position in the previous frame of each touch.
						Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
						Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

						// Find the magnitude of the vector (the distance) between the touches in each frame.
						float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
						float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

						// Find the difference in the distances between each frame.
						zoomFactor = 0.01f * (touchDeltaMag - prevTouchDeltaMag);
					}
					ZoomMapUsingTouchOrMouse(zoomFactor);
					break;
				default:
					break;
			}
		}

		void ZoomMapUsingTouchOrMouse(float zoomFactor)
		{
			var zoom = Mathf.Max(0.0f, Mathf.Min(_mapManager.Zoom + zoomFactor * _zoomSpeed, 21.0f));
			if (Math.Abs(zoom - _mapManager.Zoom) > 0.0f)
			{
				_mapManager.UpdateMap(_mapManager.CenterLatitudeLongitude, zoom);
			}
		}

		void PanMapUsingKeyBoard(float xMove, float zMove)
		{
			if (Math.Abs(xMove) > 0.0f || Math.Abs(zMove) > 0.0f)
			{
				// Get the number of degrees in a tile at the current zoom level.
				// Divide it by the tile width in pixels ( 256 in our case)
				// to get degrees represented by each pixel.
				// Keyboard offset is in pixels, therefore multiply the factor with the offset to move the center.
				float factor = _panSpeed * (Conversions.GetTileScaleInDegrees((float)_mapManager.CenterLatitudeLongitude.x, _mapManager.AbsoluteZoom));

				var latitudeLongitude = new Vector2d(_mapManager.CenterLatitudeLongitude.x + zMove * factor * 2.0f, _mapManager.CenterLatitudeLongitude.y + xMove * factor * 4.0f);

				_mapManager.UpdateMap(latitudeLongitude, _mapManager.Zoom);
			}
		}

		void PanMapUsingTouchOrMouse()
		{
			if (_useDegreeMethod)
			{
				UseDegreeConversion();
			}
			else
			{
				UseMeterConversion();
			}
		}

		void UseMeterConversion()
		{
			if (Input.GetMouseButtonUp(1))
			{
				var mousePosScreen = Input.mousePosition;
				//assign distance of camera to ground plane to z, otherwise ScreenToWorldPoint() will always return the position of the camera
				//http://answers.unity3d.com/answers/599100/view.html
				mousePosScreen.z = _referenceCamera.transform.localPosition.y;
				var pos = _referenceCamera.ScreenToWorldPoint(mousePosScreen);

				var latlongDelta = _mapManager.WorldToGeoPosition(pos);
				Debug.Log("Latitude: " + latlongDelta.x + " Longitude: " + latlongDelta.y);
			}

			if (Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject())
			{
				var mousePosScreen = Input.mousePosition;
				//assign distance of camera to ground plane to z, otherwise ScreenToWorldPoint() will always return the position of the camera
				//http://answers.unity3d.com/answers/599100/view.html
				mousePosScreen.z = _referenceCamera.transform.localPosition.y;
				_mousePosition = _referenceCamera.ScreenToWorldPoint(mousePosScreen);

				if (_shouldDrag == false)
				{
					_shouldDrag = true;
					_origin = _referenceCamera.ScreenToWorldPoint(mousePosScreen);
				}
			}
			else
			{
				_shouldDrag = false;
			}

			if (_shouldDrag == true)
			{
				var changeFromPreviousPosition = _mousePositionPrevious - _mousePosition;
				if (Mathf.Abs(changeFromPreviousPosition.x) > 0.0f || Mathf.Abs(changeFromPreviousPosition.y) > 0.0f)
				{
					_mousePositionPrevious = _mousePosition;
					var offset = _origin - _mousePosition;

					if (Mathf.Abs(offset.x) > 0.0f || Mathf.Abs(offset.z) > 0.0f)
					{
						if (null != _mapManager)
						{
							float factor = _panSpeed * Conversions.GetTileScaleInMeters((float)0, _mapManager.AbsoluteZoom) / _mapManager.UnityTileSize;
							var latlongDelta = Conversions.MetersToLatLon(new Vector2d(offset.x * factor, offset.z * factor));
							var newLatLong = _mapManager.CenterLatitudeLongitude + latlongDelta;

							_mapManager.UpdateMap(newLatLong, _mapManager.Zoom);
						}
					}
					_origin = _mousePosition;
				}
				else
				{
					if (EventSystem.current.IsPointerOverGameObject())
					{
						return;
					}
					_mousePositionPrevious = _mousePosition;
					_origin = _mousePosition;
				}
			}
		}

		void UseDegreeConversion()
		{
			if (Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject())
			{
				var mousePosScreen = Input.mousePosition;
				//assign distance of camera to ground plane to z, otherwise ScreenToWorldPoint() will always return the position of the camera
				//http://answers.unity3d.com/answers/599100/view.html
				mousePosScreen.z = _referenceCamera.transform.localPosition.y;
				_mousePosition = _referenceCamera.ScreenToWorldPoint(mousePosScreen);

				if (_shouldDrag == false)
				{
					_shouldDrag = true;
					_origin = _referenceCamera.ScreenToWorldPoint(mousePosScreen);
				}
			}
			else
			{
				_shouldDrag = false;
			}

			if (_shouldDrag == true)
			{
				var changeFromPreviousPosition = _mousePositionPrevious - _mousePosition;
				if (Mathf.Abs(changeFromPreviousPosition.x) > 0.0f || Mathf.Abs(changeFromPreviousPosition.y) > 0.0f)
				{
					_mousePositionPrevious = _mousePosition;
					var offset = _origin - _mousePosition;

					if (Mathf.Abs(offset.x) > 0.0f || Mathf.Abs(offset.z) > 0.0f)
					{
						if (null != _mapManager)
						{
							// Get the number of degrees in a tile at the current zoom level.
							// Divide it by the tile width in pixels ( 256 in our case)
							// to get degrees represented by each pixel.
							// Mouse offset is in pixels, therefore multiply the factor with the offset to move the center.
							float factor = _panSpeed * Conversions.GetTileScaleInDegrees((float)_mapManager.CenterLatitudeLongitude.x, _mapManager.AbsoluteZoom) / _mapManager.UnityTileSize;

							var latitudeLongitude = new Vector2d(_mapManager.CenterLatitudeLongitude.x + offset.z * factor, _mapManager.CenterLatitudeLongitude.y + offset.x * factor);
							_mapManager.UpdateMap(latitudeLongitude, _mapManager.Zoom);
						}
					}
					_origin = _mousePosition;
				}
				else
				{
					if (EventSystem.current.IsPointerOverGameObject())
					{
						return;
					}
					_mousePositionPrevious = _mousePosition;
					_origin = _mousePosition;
				}
			}
		}

		private Vector3 getGroundPlaneHitPoint(Ray ray)
		{
			float distance;
			if (!_groundPlane.Raycast(ray, out distance)) { return Vector3.zero; }
			return ray.GetPoint(distance);
		}

		public void SelectRegionOnValueChanged(Toggle toggle)
		{
			_selectingRegion = toggle.isOn;
		    lineRenderer.gameObject.SetActive(toggle.isOn);
			if (toggle.isOn)
			{
				Vector2 screenSpaceInitial;
				Vector2 screenSpaceFinal;

				Debug.Log(Screen.width + " " + Screen.height);

				var portrait = Screen.width < Screen.height;
				if (portrait)
				{
					screenSpaceInitial = new Vector2(0.25f * Screen.width, 0.5f * Screen.height - 0.25f * Screen.width);
					screenSpaceFinal = new Vector2(0.75f * Screen.width, 0.5f * Screen.height + 0.25f * Screen.width);
				}
				else
				{
					screenSpaceInitial = new Vector2(0.5f * Screen.width - 0.25f * Screen.height, 0.25f * Screen.height);
					screenSpaceFinal = new Vector2(0.5f * Screen.width + 0.25f * Screen.height, 0.75f * Screen.height);
				}

				Debug.Log(screenSpaceInitial + " " + screenSpaceFinal);

				// Transform into world space
				_initialPosition = _referenceCamera.ScreenToWorldPoint(new Vector3(screenSpaceInitial.x, screenSpaceInitial.y, _referenceCamera.transform.localPosition.y));
				_lastPosition = _referenceCamera.ScreenToWorldPoint(new Vector3(screenSpaceFinal.x, screenSpaceFinal.y, _referenceCamera.transform.localPosition.y));

				lineRenderer.positionCount = 4;	
				lineRenderer.SetPosition(0, new Vector3(_initialPosition.x, 0.1f, _initialPosition.z));
				lineRenderer.SetPosition(1, new Vector3(_lastPosition.x, 0.1f, _initialPosition.z));
				lineRenderer.SetPosition(2, new Vector3(_lastPosition.x, 0.1f, _lastPosition.z));
				lineRenderer.SetPosition(3, new Vector3(_initialPosition.x, 0.1f, _lastPosition.z));

			}
		}
	}
}