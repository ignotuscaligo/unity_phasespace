using System;
using System.Timers;
using UnityEngine;
using Phasespace;

public delegate void OwlUpdated();

public class OwlInterface : MonoBehaviour {
	public string serverAddress = "127.0.0.1";
	public bool serverRunning = false; // editor-visible indication of server being connected
	public bool getMarkers;
	public bool getRigids;
	public bool getCameras;
	public float spaceScalar = 1f;
	public Phasespace.NativeMarker[] markers;
	public Phasespace.NativeRigid[] rigids;
	public Phasespace.NativeCamera[] cameras;
	public OwlUpdated onOwlUpdate;
	public ConfigManager configuration;

	private int serverFrequency = 120; // hz
	private OwlState state = OwlState.Disconnected;

	// left over from old async method of communicating with Owl server
	private Timer updateTimer = null;
	private System.Object asyncLock = new System.Object();
	private static float updateFrequency = 480f; // hz
	private static int maxMarkers = 32;
	private static int maxRigids = 32;
	private static int maxCameras = 32;

	private enum OwlState {
		Disconnected = 0,
		Connected = 1,
		Error = 2
	};

	public void Awake() {
		if (configuration) {
			// tell config to hold place for 'owl_address', use serverAddress if not set in config
			configuration.RegisterProperty("owl_address", defaultValue: serverAddress);
		}
	}

	public void Start() {
		if (configuration) {
			// get value from file if exists, else use above value
			serverAddress = configuration.GetProperty("owl_address");
		}
		state = OwlState.Disconnected;
		UpdateOwl(); // start
	}

	public void Update() {
		UpdateOwl(); // continue
	}

	private void Stop(OwlState exitState = OwlState.Disconnected) {
		if (state == OwlState.Connected) {
			Debug.Log("Stop owl");
			Owl.Done();
			if (exitState == OwlState.Error) {
				Debug.LogError("Owl was stopped due to an error!");
			}
			state = exitState;
		}
	}

	private void UpdateOwl() {
		if (state == OwlState.Disconnected) {
			Debug.Log("Start owl");

			try {
				// clear any current connection
				// also check if dll is present
				Owl.Done();
			} catch (DllNotFoundException e) {
				Debug.LogError("Owl DLL not found!");
				Debug.LogError(e);
				state = OwlState.Error;
				return;
			}

			uint flags = (uint)Phasespace.InitFlags.Slave;

			Debug.Log("Initializing server");
			if (Owl.Init(serverAddress, flags) < 0) {
				Debug.LogError("Owl init fail!");
				state = OwlState.Error;
				return;
			}

			Owl.SetFloat((int)Phasespace.Set.Frequency, serverFrequency);

			if (ErrorOccurred()) {
				Stop(exitState: OwlState.Error);
				return;
			}

			Owl.SetInteger((int)Phasespace.Set.Streaming, (int)Phasespace.CommonFlags.Enable);

			if (ErrorOccurred()) {
				Stop(exitState: OwlState.Error);
				return;
			}

			Debug.Log("Running server!");

			state = OwlState.Connected;
		} else if (state == OwlState.Connected) {
			bool gotData = false;
			if (getMarkers) {
				// get markers
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
						Stop(exitState: OwlState.Error);
						return;
					}
				} while (n > 0);
			}

			if (getRigids) {
				// get rigid bodies
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
						Stop(exitState: OwlState.Error);
						return;
					}
				} while (n > 0);
			}

			if (getCameras) {
				// get cameras
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
					Stop(exitState: OwlState.Error);
					return;
				}
			}

			if (gotData) {
				if (onOwlUpdate != null) {
					onOwlUpdate();
				}
			}
		}
		serverRunning = (state == OwlState.Connected);
	}

	public void OnApplicationQuit() {
		Stop();
	}

	private bool ErrorOccurred() {
		int err = Owl.GetError();
		if (err != (int)Phasespace.Error.NoError) {
			Debug.LogError("Owl error: "+(Phasespace.Error)err);
			return true;
		}
		return false;
	}
}