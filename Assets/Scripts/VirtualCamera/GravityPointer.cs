using UnityEngine;
using System.Collections;

public class GravityPointer : MonoBehaviour {

	public GameObject DataSource;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		if (DataSource == null) {
			return;
		}

		VirtualCameraController vcc = DataSource.GetComponent<VirtualCameraController> ();
		this.transform.localPosition = vcc.Gravity.normalized * 3;
	}
}
