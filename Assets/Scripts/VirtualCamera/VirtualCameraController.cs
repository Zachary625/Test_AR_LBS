using UnityEngine;
using System.Collections;

public class VirtualCameraController : MonoBehaviour {

	private Gyroscope gyroscope;
	private LocationService gps;
	private Compass compass;

	private LocationInfo location;
	private float compassTrueHeading;

	private Quaternion baseRotation = Quaternion.Euler(new Vector3(90, 180, 0));
	private Quaternion gyroAttitude;
	private Quaternion gyroRotation;
	private Quaternion cameraRotation;

	private bool useCompass = false;

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

//		GameObject cameraSet = this.transform.parent.gameObject;
//		Vector3 cameraSetEulerAngles = new Vector3 (90, 180, 0);
//		cameraSet.transform.eulerAngles = cameraSetEulerAngles;  

		this._startRealCamera ();

	}

	void _startRealCamera() {
		WebCamTexture webcamTexture = new WebCamTexture ();  

		for (int i = 0; i < WebCamTexture.devices.Length; i++) {  
			if (!WebCamTexture.devices [i].isFrontFacing) {  
				webcamTexture.deviceName = WebCamTexture.devices [i].name;  
				break;  
			}  
		}  
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

	string _quaternionToString(Quaternion quaternion) {
		return "(" + quaternion.x + ", " + quaternion.y + ", " + quaternion.z + ", " + quaternion.w + ")";
	}

	void _debugGUI() {
		int height = 30;
		int row = 0;
		if(GUI.Button(new Rect(0, height *(row++),100, height), this.useCompass? "north":"front")) {
			this.useCompass = !this.useCompass;		
		}
		GUI.Label (new Rect (0, height *(row++), 500, height), "screen orientation: " + this._getScreenOrientationString());
		GUI.Label (new Rect (0, height *(row++), 500, height), "heading: " + this.compass.trueHeading);

		GUI.Label (new Rect (0, height *(row++), 500, height), "gyroAttitude: " + this._quaternionToString(this.gyroAttitude));

		GUI.Label (new Rect (0, height *(row++), 500, height), "gyroRotation: " + this._quaternionToString(this.gyroRotation));

		GUI.Label (new Rect (0, height *(row++), 500, height), "cameraRotation: " + this._quaternionToString(this.gyroRotation));

		GUI.Label (new Rect (0, height *(row++), 500, height), "latitude: " + this.location.latitude);
		GUI.Label (new Rect (0, height *(row++), 500, height), "longitude: " + this.location.longitude);
		GUI.Label (new Rect (0, height *(row++), 500, height), "altitude: " + this.location.altitude);
	}

	void _updateVirtualCamera() {
		if (this.gyroscope != null) {
			this.gyroAttitude = this.gyroscope.attitude;
		}
		if (this.gps != null) {
			if(this.gps.status == LocationServiceStatus.Running) {
				this.location = this.gps.lastData;
			}
		}
		if (this.compass != null) {
			this.compassTrueHeading = this.compass.trueHeading;
		}
			
		this.gyroRotation = this.baseRotation * this.gyroAttitude * new Quaternion (0, 0, 1, 0);
		this.transform.localRotation = this.gyroRotation;
		if (this.useCompass) {
			this.transform.RotateAround (new Vector3(0,0,0), new Vector3(0,1,0), this.transform.eulerAngles.y - this.compassTrueHeading);
		}

		this.cameraRotation = this.transform.rotation;
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
