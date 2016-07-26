using UnityEngine;
using System.Collections;

public class VirtualCompassController : MonoBehaviour {

	private Compass compass;
	private LineRenderer lineRenderer;

	// Use this for initialization
	void Start () {
		this.compass = Input.compass;
		this.compass.enabled = true;

		this.lineRenderer = this.GetComponent<LineRenderer> ();
		this.lineRenderer.useWorldSpace = false;
		this.lineRenderer.SetVertexCount (2);
		this.lineRenderer.SetColors (Color.yellow, Color.yellow);
		this.lineRenderer.SetWidth (0.01f, 0.01f);
	}
	
	// Update is called once per frame
	void Update () {
		this.lineRenderer.SetPosition (0, Vector3.zero);
		this.lineRenderer.SetPosition (1, this.compass.rawVector.normalized * 3);
	}
}
