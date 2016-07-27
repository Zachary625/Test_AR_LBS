using UnityEngine;
using System.Collections;

public class VirtualCameraController : MonoBehaviour {

	private Gyroscope gyroscope;
	private LocationService gps;
	private Compass compass;

	private LocationInfo location;
	private float compassTrueHeading;
    private Vector3 compassRawVector;

	private Vector3 acceleration;
	private Vector3 velocity;
	private Vector3 displacement;

	private Quaternion baseRotation = Quaternion.Euler(new Vector3(90, 180, 0));
    private Quaternion fixRotation = new Quaternion(0,0,1,0);
	private Quaternion gyroAttitude;
	private Vector3 gyroGravity;
    private Quaternion cameraRotation;

	public UnityEngine.UI.Image WebCamImage;
	private WebCamTexture webCamTexture;

	public float AccelerationThreshold = 0.01f;
	public float VelocityThreshold = 0.001f;

    private enum _RotationMethod
    {
        Attitude,
		AttitudeAndTrueHeading,
		Length,
    }

	private enum _DisplacementMethod
	{
		Basic,
		AccEvents,
		Length,
	}

    private _RotationMethod rotationMethod;
	private _DisplacementMethod displacementMethod;

	private bool _useDisplacement = true;
	private bool _useWebCam = true;

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

		this._enableWebCam ();
	}

	void _showWebCam() {
		if (this.WebCamImage == null) {
			return;
		}

		this.WebCamImage.enabled = true;
	}

	void _hideWebCam() {
		if (this.WebCamImage == null) {
			return;
		}

		this.WebCamImage.enabled = false;
	}

	void _disableWebCam() {
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

	void _enableWebCam() {
		this._disableWebCam();

		if (this.WebCamImage == null) {
			return;
		}

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

		this._disableWebCam();
	}
	
	// Update is called once per frame
	void Update () {
		this._updateVirtualCamera ();

		this._updateRealCamera();
	}

	void OnGUI() {
		this._debugGUI ();
	}

	string _quaternionToString(Quaternion quaternion) {
		return "(" + quaternion.x + ", " + quaternion.y + ", " + quaternion.z + ", " + quaternion.w + ")";
	}

	string _vector3ToString(Vector3 vector3) {
		return "(" + vector3.x + ", " + vector3.y + ", " + vector3.z + ")";
	}

	void _debugGUI() {
		int height = 30;
		int row = 0;

//		if(GUI.Button(new Rect(0, height *(row++),200, height), this.rotationMethod.ToString())) {
//            this.rotationMethod = (_RotationMethod)(((int)(this.rotationMethod + 1)) % ((int)(_RotationMethod.Length)));
//		}

		if(GUI.Button(new Rect(0, height *(row++),200, height), "AR: " + this._useWebCam)) {
			this._useWebCam = !this._useWebCam;
			if (this._useWebCam) {
				this._showWebCam ();
			} else {
				this._hideWebCam ();
			}
		}
		if(GUI.Button(new Rect(0, height *(row++),200, height), "D: " + this._useDisplacement)) {
			this._useDisplacement = !this._useDisplacement;
		}

		if (this._useDisplacement) {
			if(GUI.Button(new Rect(0, height *(row++),200, height), this.displacementMethod.ToString())) {
				this.acceleration = Vector3.zero;
				this.velocity = Vector3.zero;
				this.displacement = Vector3.zero;
				this.displacementMethod = (_DisplacementMethod)(((int)(this.displacementMethod + 1)) % ((int)(_DisplacementMethod.Length)));
			}
		}

//		GUI.Label (new Rect (0, height *(row++), 500, height), "screen orientation: " + this._getScreenOrientationString());
//
//
//        GUI.Label (new Rect (0, height *(row++), 500, height), "gyroAttitude: " + this._quaternionToString(this.gyroAttitude));
//		GUI.Label (new Rect (0, height *(row++), 500, height), "gyroGravity: " + this._vector3ToString(this.gyroGravity));

//		GUI.Label (new Rect (0, height *(row++), 500, height), "compassTrueHeading: " + this.compassTrueHeading);
//		GUI.Label (new Rect(0, height * (row++), 500, height), "compassRawVector: " + this.compassRawVector);

		GUI.Label (new Rect(0, height * (row++), 500, height), "acceleration: " + this._vector3ToString(this.acceleration));
		GUI.Label (new Rect(0, height * (row++), 500, height), "velocity: " + this._vector3ToString(this.velocity));
		GUI.Label (new Rect(0, height * (row++), 500, height), "displacement: " + this._vector3ToString(this.displacement));


//		GUI.Label (new Rect (0, height *(row++), 500, height), "cameraRotation: " + this._quaternionToString(this.cameraRotation));
//
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
		if (SystemInfo.supportsAccelerometer && this._useDisplacement) {
			switch (this.displacementMethod) {
			case _DisplacementMethod.Basic:
				{
					this.acceleration = Input.acceleration;
					this.acceleration -= this.gyroGravity.normalized;
					if (this.acceleration.magnitude > this.AccelerationThreshold) {
						this.velocity += this.acceleration * Time.deltaTime;
					} else {
						this.acceleration = Vector3.zero;
					}

					if (this.velocity.magnitude > this.VelocityThreshold) {
						this.displacement += this.velocity * Time.deltaTime;
					} else {
						this.velocity = Vector3.zero;
					}
					break;
				}
			case _DisplacementMethod.AccEvents:
				{
					this.acceleration = Vector3.zero;
					AccelerationEvent[] accelerationEvents = Input.accelerationEvents;
					float deltaTime = 0;
					foreach (AccelerationEvent accelerationEvent in accelerationEvents) {
						this.acceleration = accelerationEvent.acceleration;
						this.acceleration -= this.gyroGravity.normalized;
						if (this.acceleration.magnitude > this.AccelerationThreshold) {
							this.velocity += this.acceleration * accelerationEvent.deltaTime;
						} else {
							this.acceleration = Vector3.zero;
						}
						if (this.velocity.magnitude > this.VelocityThreshold) {
							this.displacement += this.velocity * accelerationEvent.deltaTime;
						} else {
							this.velocity = Vector3.zero;
						}

					}
					break;
				}
			}
		}

		switch (this.rotationMethod) {
		case _RotationMethod.Attitude:
			{
				this.cameraRotation = this.baseRotation * this.gyroAttitude * this.fixRotation;
				this.transform.localRotation = this.cameraRotation;
				break;
			}
		case _RotationMethod.AttitudeAndTrueHeading:
			{
				Quaternion gyroRotation = this.baseRotation * this.gyroAttitude * this.fixRotation;
				Vector3 cameraEuler = this.cameraRotation.eulerAngles;
				cameraEuler.y = this.compassTrueHeading;
				this.cameraRotation = Quaternion.Euler (cameraEuler);
				this.transform.localRotation = this.cameraRotation;
				break;
			}
		}

		this.transform.localPosition = this.displacement;
	}

    void _updateRealCamera() {
        if (this.webCamTexture == null) {
            return;
        }
        if(this.WebCamImage == null)
        {
            return;
        }

        int videoRotationAngle = this.webCamTexture.videoRotationAngle;
		bool videoVerticallyMirrored = this.webCamTexture.videoVerticallyMirrored;
		Debug.Log(" @ VirtualCameraController._updateRealCamera(): videoRotationAngle: " + videoRotationAngle);
		Debug.Log(" @ VirtualCameraController._updateRealCamera(): videoVerticallyMirrored: " + videoVerticallyMirrored);

		int xScaleFactor = videoVerticallyMirrored ? -1 : 1;

		var webCamAspect = ((float)this.webCamTexture.width) / ((float)this.webCamTexture.height);
		if (videoRotationAngle % 180 == 90)
        {
            Camera camera = this.GetComponent<Camera>();
			Debug.Log (" @ VirtualCameraController._updateRealCamera(): camera.aspect: " + camera.aspect);
			this.WebCamImage.rectTransform.localScale = new Vector3(xScaleFactor / camera.aspect / webCamAspect, camera.aspect * webCamAspect , 1);
			Debug.Log (" @ VirtualCameraController._updateRealCamera(): localScale: " + this.WebCamImage.rectTransform.localScale.ToString());
        }
        else {
			this.WebCamImage.rectTransform.localScale = new Vector3(xScaleFactor / webCamAspect, 1 * webCamAspect, 1);
        }

		this.WebCamImage.rectTransform.localRotation = Quaternion.AngleAxis(videoRotationAngle, Vector3.forward);

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
