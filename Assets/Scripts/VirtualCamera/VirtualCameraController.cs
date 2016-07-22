using UnityEngine;
using System.Collections;

public class VirtualCameraController : MonoBehaviour {

	private Gyroscope gyroscope;
	private LocationService gps;
	private Compass compass;

	// Use this for initialization
	void Start () {
		Debug.Log (" @ VirtualCameraController.Start(): SystemInfo.supportsGyroscope: " + SystemInfo.supportsGyroscope);
		Debug.Log (" @ VirtualCameraController.Start(): SystemInfo.supportsLocationService: " + SystemInfo.supportsLocationService);
		if (SystemInfo.supportsGyroscope) {
			this.gyroscope = Input.gyro;
			this.gyroscope.enabled = true;
		}
		if (SystemInfo.supportsLocationService && Input.location.isEnabledByUser) {
			this.gps = Input.location;
			this.gps.Start ();
		}

		this.compass = Input.compass;
		this.compass.enabled = true;
	}

	void Stop() {
		if (this.gps != null) {
			this.gps.Stop ();
		}
	}
	
	// Update is called once per frame
	void Update () {
		this._updateVirtualCamera ();
	}

	void OnGUI() {
		this._debugGUI ();
	}

	void _debugGUI() {
		GUI.Label (new Rect (0, 0, 300, 100), this._getScreenOrientationString ());
		if (this.gps != null) {
			GUI.Label (new Rect (0, 100, 300, 100), "latitude: " + this.gps.lastData.latitude);
			GUI.Label (new Rect (0, 130, 300, 100), "longitude: " +  this.gps.lastData.longitude);
			GUI.Label (new Rect (0, 160, 300, 100), "altitude: " +  this.gps.lastData.altitude);
		}
	}

	void _updateVirtualCamera() {
		if (this.gyroscope == null) {
			return;
		}
		if (this.gps == null) {
			return;
		}
		if (this.compass == null) {
			return;
		}

		Quaternion attitude = this.gyroscope.attitude;
		LocationInfo locationInfo = this.gps.lastData;
		Quaternion rotation = attitude;
		Quaternion rotationFix;


		GameObject cameraSet = this.transform.parent.gameObject;
		Vector3 cameraSetEulerAngles;

		cameraSetEulerAngles = new Vector3 (90, 180, 0);
		rotationFix = new Quaternion (0, 0, 1, 0);  

		cameraSet.transform.eulerAngles = cameraSetEulerAngles;  
		this.transform.localRotation = rotation * rotationFix;
	}

	string _getScreenOrientationString() {
		string result = "";
		switch (Screen.orientation) {
			case ScreenOrientation.Portrait: {
				result = "↑";
				break;
			}
			case ScreenOrientation.PortraitUpsideDown: {
				result = "↓";
				break;
			}
			case ScreenOrientation.LandscapeLeft: {
				result = "←";
				break;
			}
			case ScreenOrientation.LandscapeRight: {
				result = "→";
				break;
			}
			case ScreenOrientation.AutoRotation:
			{
				result = "〇";
				break;
			}
			case ScreenOrientation.Unknown:
			{
				result = "？";
				break;
			}
		}
		return result;
	}
}
