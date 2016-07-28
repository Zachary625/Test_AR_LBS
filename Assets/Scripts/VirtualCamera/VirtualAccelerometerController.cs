using UnityEngine;
using System.Collections;

public class VirtualAccelerometerController : MonoBehaviour {

	private LineRenderer lineRenderer;

	public GameObject DataSource;

	// Use this for initialization
	void Start () {
		this.lineRenderer = this.GetComponent<LineRenderer> ();
		this.lineRenderer.useWorldSpace = false;
		this.lineRenderer.SetVertexCount (2);
		this.lineRenderer.SetWidth (0.01f, 0.01f);
	}
	
	// Update is called once per frame
	void Update () {
		if (DataSource == null) {
			return;
		}
		VirtualCameraController vcc = DataSource.GetComponent<VirtualCameraController> ();
		this.lineRenderer.SetPosition (0, Vector3.zero);
		this.lineRenderer.SetPosition (1, vcc.Gravity.normalized * 3);
	}
}
