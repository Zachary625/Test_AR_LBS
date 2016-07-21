using UnityEngine;
using System.Collections;

public class VirtualCameraController : MonoBehaviour {

	private Gyroscope gyroscope;

	// Use this for initialization
	void Start () {
		Debug.Log (" @ VirtualCameraController.Start(): SystemInfo.supportsGyroscope: " + SystemInfo.supportsGyroscope);
		if (SystemInfo.supportsGyroscope) {
			this.gyroscope = Input.gyro;
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

		Debug.Log (" @ VirtualCameraController._readGyroscope(): " + this.gyroscope.attitude.ToString());
		this.gameObject.transform.rotation = this.gyroscope.attitude;
	}
}
