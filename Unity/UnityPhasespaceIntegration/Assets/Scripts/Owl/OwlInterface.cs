using System.Timers;
using UnityEngine;
using Phasespace;

public delegate void OwlUpdated();

public class OwlInterface : MonoBehaviour {
	public string serverAddress = "127.0.0.1";
	public bool serverRunning = false;
	public bool getMarkers;
	public bool getRigids;
	public bool getCameras;
	public float spaceScalar = 1f;
	public Phasespace.NativeMarker[] markers;
	public Phasespace.NativeRigid[] rigids;
	public Phasespace.NativeCamera[] cameras;
	public OwlUpdated onOwlUpdate;
	public ConfigManager configuration;

	private int serverFrequency = 120;
	private bool connected = false;
	private Timer updateTimer = null;
	private Object asyncLock = new Object();

	private static float updateFrequency = 480f; // hz
	private static int maxMarkers = 32;
	private static int maxRigids = 32;
	private static int maxCameras = 32;

	public OwlInterface() {
		//markers = new Phasespace.NativeMarker[maxMarkers];
		//rigids = new Phasespace.NativeRigid[maxRigids];
		//cameras = new Phasespace.NativeCamera[maxCameras];
	}

	public void Awake() {
		Debug.Log("Owl Awake()");
		if (configuration) {
			// tell config to hold place for 'owl_address', use serverAddress if not set in config
			configuration.RegisterProperty("owl_address", defaultValue: serverAddress);
		}
	}

	public void Start() {
		Debug.Log("Owl Start()");
		if (configuration) {
			// get value from file if exists, else use above value
			serverAddress = configuration.GetProperty("owl_address");
		}
		UpdateOwl(); // start
	}

	public void Update() {
		UpdateOwl(); // continue
		serverRunning = connected;
	}

	private void Stop() {
		if (connected) {
			Debug.Log("Stop owl");
			Owl.Done();
			connected = false;
			serverRunning = connected;
		}
	}

	private void UpdateOwl() {
		if (!connected) {
			Debug.Log("Start owl");
			Owl.Done(); // clear just in case
			uint flags = (uint)Phasespace.InitFlags.Slave;

			Debug.Log("Initializing server");
			if (Owl.Init(serverAddress, flags) < 0) {
				Debug.LogError("Owl init fail!");
				return;
			}

			if (ErrorOccurred()) {
				Stop();
				return;
			}

			Owl.SetFloat((int)Phasespace.Set.Frequency, serverFrequency);

			if (ErrorOccurred()) {
				Stop();
				return;
			}

			Owl.SetInteger((int)Phasespace.Set.Streaming, (int)Phasespace.CommonFlags.Enable);

			if (ErrorOccurred()) {
				Stop();
				return;
			}

			Debug.Log("Running server!");

			connected = true;
			serverRunning = connected;
		} else {
			// get some markers
			bool gotData = false;
			if (getMarkers) {
				int n = 0;
				do {
					n = Owl.GetMarkers(ref markers);
					if (n > 0) {
						gotData = true;
						for (int i = 0; i < n; i++) {
							markers[i].x *= spaceScalar;
							markers[i].y *= spaceScalar;
							markers[i].z *= spaceScalar;
						}
					}
					if (ErrorOccurred()) {
						Stop();
						return;
					}
				} while (n > 0);
			}

			if (getRigids) {
				int n = 0;
				do {
					n = Owl.GetRigids(ref rigids);
					if (n > 0) {
						gotData = true;
						for (int i = 0; i < n; i++) {
							rigids[i].pose.px *= spaceScalar;
							rigids[i].pose.py *= spaceScalar;
							rigids[i].pose.pz *= spaceScalar;
						}
					}
					if (ErrorOccurred()) {
						Stop();
						return;
					}
				} while (n > 0);
			}

			if (getCameras) {
				int n = Owl.GetCameras(ref cameras);
				if (n > 0) {
					gotData = true;
					for (int i = 0; i < n; i++) {
						cameras[i].pose.px *= spaceScalar;
						cameras[i].pose.py *= spaceScalar;
						cameras[i].pose.pz *= spaceScalar;
					}
				}
				if (ErrorOccurred()) {
					Stop();
					return;
				}
			}

			if (gotData) {
				if (onOwlUpdate != null) {
					onOwlUpdate();
				}
			}
		}
	}

	public void OnApplicationQuit() {
		Stop();
	}

	private bool ErrorOccurred() {
		int err = Owl.GetError();
		if (err != (int)Phasespace.Error.NoError) {
			Debug.Log("Error: "+(Phasespace.Error)err);
			return true;
		}
		return false;
	}
}