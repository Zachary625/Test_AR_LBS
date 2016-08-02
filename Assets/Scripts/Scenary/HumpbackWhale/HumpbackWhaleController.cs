using UnityEngine;
using System.Collections;

public class HumpbackWhaleController : MonoBehaviour {

    public float MinimumRadius = 5;
    public float MaximumRadius = 10;

    public float MinimumScale = 0.5f;
    public float MaximumScale = 2;

    public float MinimumAngularSpeed = 2;
    public float MaximumAngularSpeed = 5;

    private float radius;
    private float scale;
    private float angularSpeed;

    private float angle;

	// Use this for initialization
	void Start () {
        this.radius = Random.Range(this.MinimumRadius, this.MaximumRadius);
        this.scale = Random.Range(this.MinimumScale, this.MaximumScale);
        this.angularSpeed = Random.Range(this.MinimumAngularSpeed, this.MaximumAngularSpeed);

        this.angle = Random.Range(0, 360);

        this.transform.localScale = new Vector3(this.scale, this.scale, this.scale);

    }

    // Update is called once per frame
    void Update () {
        this.angle += this.angularSpeed * Time.deltaTime;

        float angleRadians = Mathf.Deg2Rad * this.angle;
        this.transform.localPosition = new Vector3(Mathf.Cos(angleRadians), 0 ,- Mathf.Sin(angleRadians)) * this.radius;
        this.transform.localRotation = Quaternion.AngleAxis(this.angle, Vector3.up);
    }
}
