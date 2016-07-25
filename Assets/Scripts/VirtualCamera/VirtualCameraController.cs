using UnityEngine;
using System.Collections;

public class VirtualCameraController : MonoBehaviour {

	private Gyroscope gyroscope;
	private LocationService gps;
	private Compass compass;

	private LocationInfo location;
	private float compassTrueHeading;
    private Vector3 compassRawVector;

	private Quaternion baseRotation = Quaternion.Euler(new Vector3(90, 180, 0));
    private Quaternion fixRotation = new Quaternion(0,0,1,0);
	private Quaternion gyroAttitude;
	private Vector3 gyroGravity;
    private Quaternion cameraRotation;

	public UnityEngine.UI.Image WebCamImage;
	private WebCamTexture webCamTexture;

    private enum _RotationMethod
    {
        Attitude,
		GravityAndHeading1,
		GravityAndHeading2,
        GravityAndRawVector,
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

	void _stopReadCamera() {
		if (this.webCamTexture != null) {
			if (this.webCamTexture.isPlaying) {
				this.webCamTexture.Stop ();
			}
			this.webCamTexture = null;
		}
		if (this.WebCamImage != null) {
			this.WebCamImage.material.color = Color.black;
		}
	}

	void _startRealCamera() {
		if (this.WebCamImage == null) {
			return;
		}

		this._stopReadCamera ();

		for (int i = 0; i < WebCamTexture.devices.Length; i++) {  
			if (!WebCamTexture.devices [i].isFrontFacing) {  
				this.webCamTexture = new WebCamTexture ();
				this.webCamTexture.deviceName = WebCamTexture.devices [i].name;  
				Debug.Log (" @ VirtualCameraController: found camera: " + this.webCamTexture.deviceName);
				break;  
			}  
		}  
		if (this.webCamTexture == null) {
			return;
		}

		this.WebCamImage.material.color = Color.white;
		this.WebCamImage.material.mainTexture = this.webCamTexture;
		this.webCamTexture.Play ();
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
        GUI.Label(new Rect(0, height * (row++), 500, height), "compassRawVector: " + this.compassRawVector);

        GUI.Label (new Rect (0, height *(row++), 500, height), "gyroAttitude: " + this._quaternionToString(this.gyroAttitude));
		GUI.Label (new Rect (0, height *(row++), 500, height), "gyroGravity: " + (this.gyroGravity.ToString()));

		GUI.Label (new Rect (0, height *(row++), 500, height), "cameraRotation: " + this._quaternionToString(this.cameraRotation));

}

	void _updateVirtualCamera() {
		if (this.gyroscope != null) {
			this.gyroAttitude = this.gyroscope.attitude;
			this.gyroGravity = this.gyroscope.gravity;
		}
		if (this.gps != null) {
			if(this.gps.status == LocationServiceStatus.Running) {
				this.location = this.gps.lastData;
			}
		}
		if (this.compass != null) {
			this.compassTrueHeading = this.compass.trueHeading;
            this.compassRawVector = this.compass.rawVector;
		}

		switch (this.rotationMethod) {
		case _RotationMethod.Attitude:
			{
				this.cameraRotation = this.baseRotation * this.gyroAttitude * this.fixRotation;
				this.transform.localRotation = this.cameraRotation;
				break;
			}
        case _RotationMethod.GravityAndRawVector:
            {
                this.cameraRotation = Quaternion.LookRotation(-this.compassRawVector, -this.gyroGravity);
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
