using UnityEngine;
using System.Collections;

public class VirtualCameraController : MonoBehaviour {

	private Gyroscope gyroscope;
	private LocationService gps;

	// Use this for initialization
	void Start () {
		Debug.Log (" @ VirtualCameraController.Start(): SystemInfo.supportsGyroscope: " + SystemInfo.supportsGyroscope);
		Debug.Log (" @ VirtualCameraController.Start(): SystemInfo.supportsLocationService: " + SystemInfo.supportsLocationService);
		if (SystemInfo.supportsGyroscope) {
			this.gyroscope = Input.gyro;
		}
		if (SystemInfo.supportsLocationService) {
			this.gps = Input.location;
			this.gps.Start ();
		}
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

		this.gameObject.transform.rotation = this.gyroscope.attitude;
	}
}
