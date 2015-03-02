using UnityEngine;
using Phasespace;

public class MarkerTracker : MonoBehaviour {
	public OwlInterface owl;
	public int index;
	public Marker marker;

	private bool bound = false;

	public void Start() {
		owl.onOwlUpdate += OwlUpdated;
		marker = new Phasespace.Marker();
	}

	public void Update() {
		transform.localPosition = marker.position;
	}

	public void OwlUpdated() {
		if (marker != null) {
			if (index >= 0 && index < owl.markers.Length) {
				marker.SetFromNative(owl.markers[index], ignoreLowCond: true);
			}
		}
	}

	public bool IsActive() {
		if (marker != null) {
			return (marker.condition > 0);
		}
		return false;
	}
}