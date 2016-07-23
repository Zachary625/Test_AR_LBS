using UnityEngine;
using System.Collections;

public class VirtualCameraController : MonoBehaviour {

	private Gyroscope gyroscope;
	private LocationService gps;
	private Compass compass;

	private LocationInfo location;
	private float compassTrueHeading;

	private Quaternion baseRotation = Quaternion.Euler(new Vector3(90, 180, 0));
    private Quaternion fixRotation = new Quaternion(0,0,1,0);
	private Quaternion gyroAttitude;
    private Quaternion cameraRotation;

    private enum _RotationMethod
    {
        BaseGyro,
        BaseCompassGyro,
        CompassBaseGyro,
        Length,
    }

    private _RotationMethod rotationMethod;

	// Use this for initialization
	void Start () {
		Debug.Log (" @ VirtualCameraController.Start(): SystemInfo.supportsGyroscope: " + SystemInfo.supportsGyroscope);
		Debug.Log (" @ VirtualCameraController.Start(): SystemInfo.supportsLocationService: " + SystemInfo.supportsLocationService);

        Debug.Log(" @ VirtualCameraController.Start(): (0,0,1,0): " + new Quaternion(0,0,1,0).eulerAngles.ToString());

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

		if(GUI.Button(new Rect(0, height *(row++),200, height), this.rotationMethod.ToString())) {
            this.rotationMethod = (_RotationMethod)(((int)(this.rotationMethod + 1)) % ((int)(_RotationMethod.Length)));
		}

		GUI.Label (new Rect (0, height *(row++), 500, height), "screen orientation: " + this._getScreenOrientationString());
		GUI.Label (new Rect (0, height *(row++), 500, height), "compassTrueHeading: " + this.compassTrueHeading);

		GUI.Label (new Rect (0, height *(row++), 500, height), "gyroAttitude: " + this._quaternionToString(this.gyroAttitude));

		GUI.Label (new Rect (0, height *(row++), 500, height), "cameraRotation: " + this._quaternionToString(this.cameraRotation));
        /*
		GUI.Label (new Rect (0, height *(row++), 500, height), "latitude: " + this.location.latitude);
		GUI.Label (new Rect (0, height *(row++), 500, height), "longitude: " + this.location.longitude);
		GUI.Label (new Rect (0, height *(row++), 500, height), "altitude: " + this.location.altitude);
        */
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
			

        switch (this.rotationMethod) {
            case _RotationMethod.BaseGyro:
                {
                    this.cameraRotation = this.baseRotation * this.gyroAttitude * this.fixRotation;
                    this.transform.localRotation = this.cameraRotation;
                    break;
                }
            case _RotationMethod.CompassBaseGyro:
                {
                    Quaternion compassRotation = Quaternion.AngleAxis(-this.compassTrueHeading, Vector3.up);
                    this.cameraRotation = compassRotation * this.baseRotation * this.gyroAttitude * this.fixRotation;
                    this.transform.localRotation = this.cameraRotation;
                    break;
                }
            case _RotationMethod.BaseCompassGyro: {
                    Quaternion compassRotation = Quaternion.AngleAxis(-this.compassTrueHeading, Vector3.up);
                    this.cameraRotation = this.baseRotation * compassRotation * this.gyroAttitude * this.fixRotation;
                    this.transform.localRotation = this.cameraRotation;
                    break;
                }
        }
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
