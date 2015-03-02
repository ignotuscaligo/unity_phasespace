using UnityEngine;
using Phasespace;

public class RigidTracker : MonoBehaviour {
	public OwlInterface owl;
	public int index;
	public Phasespace.Rigid rigid;

	private bool bound = false;

	public void Start() {
		owl.onOwlUpdate += OwlUpdated;
		rigid = new Phasespace.Rigid();
	}

	public void Update() {
		transform.localPosition = rigid.position;
		transform.localRotation = rigid.orientation;
	}

	public void OwlUpdated() {
		if (rigid != null) {
			if (index >= 0 && index < owl.rigids.Length) {
				rigid.SetFromNative(owl.rigids[index]);
			}
		}
	}
}