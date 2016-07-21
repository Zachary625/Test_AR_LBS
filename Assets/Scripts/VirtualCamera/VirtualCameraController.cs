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
		this._readGyroscope ();
	}

	void _readGyroscope() {
		if (this.gyroscope == null) {
			return;
		}
		if (this.gps == null) {
			return;
		}
		if (this.compass == null) {
			return;
		}

		GameObject cameraSet = this.transform.parent.gameObject;
		Vector3 cameraSetEulerAngles;
		if (Screen.orientation == ScreenOrientation.LandscapeLeft) {  
			cameraSetEulerAngles = new Vector3 (90, 90, 0);
		} else if (Screen.orientation == ScreenOrientation.Portrait) {  
			cameraSetEulerAngles = new Vector3 (90, 180, 0);
		} else if (Screen.orientation == ScreenOrientation.PortraitUpsideDown) {  
			cameraSetEulerAngles = new Vector3 (90, 180, 0);
		} else if (Screen.orientation == ScreenOrientation.LandscapeRight) {  
			cameraSetEulerAngles = new Vector3 (90, 180, 0);
		} else {  
			cameraSetEulerAngles = new Vector3 (90, 180, 0);
		}  

		if (cameraSetEulerAngles != null) {
			cameraSet.transform.eulerAngles = cameraSetEulerAngles;  
		}

		Quaternion attitude = this.gyroscope.attitude;
		Quaternion rotation = attitude;
		Quaternion rotationFix;
		if (Screen.orientation == ScreenOrientation.LandscapeLeft) {  
			rotationFix = new Quaternion (0, 0,0.7071f,0.7071f);  
		} else if (Screen.orientation == ScreenOrientation.Portrait) {  
			rotationFix = new Quaternion (0, 0, 1, 0);  
		} else if (Screen.orientation == ScreenOrientation.PortraitUpsideDown) {  
			rotationFix = new Quaternion (0, 0, 1, 0);  
		} else if (Screen.orientation == ScreenOrientation.LandscapeRight) {  
			rotationFix = new Quaternion (0, 0, 1, 0);  
		} else {  
			rotationFix = new Quaternion (0, 0, 1, 0);  
		}  

		LocationInfo locationInfo = this.gps.lastData;

		this.transform.localRotation = rotation * rotationFix;
	}
}
