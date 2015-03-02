using UnityEngine;
using Phasespace;

public class CameraTracker : MonoBehaviour {
	public OwlInterface owl;
	public int index;
	public Phasespace.Camera cam;

	private bool bound = false;

	public void Start() {
		owl.onOwlUpdate += OwlUpdated;
		cam = new Phasespace.Camera();
	}

	public void Update() {
		transform.localPosition = cam.position;
		transform.localRotation = cam.orientation;
	}

	public void OwlUpdated() {
		if (cam != null) {
			if (index >= 0 && index < owl.cameras.Length) {
				cam.SetFromNative(owl.cameras[index]);
			}
		}
	}
}