using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Phasespace {
	public class Owl {
		[DllImport("libowlsock", CharSet = CharSet.Ansi)]
		private static extern int owlInit([MarshalAs(UnmanagedType.LPStr)] string server, uint flags);
		[DllImport("libowlsock")]
		private static extern void owlDone();
		[DllImport("libowlsock")]
		private static extern int owlGetError();
		[DllImport("libowlsock")]
		private static extern void owlSetFloat(uint pname, float param);
		[DllImport("libowlsock")]
		private static extern void owlSetInteger(uint pname, int param);
		[DllImport("libowlsock")]
		private static extern int owlGetMarkers(NativeMarker[] markers, uint count);
		[DllImport("libowlsock")]
		private static extern int owlGetRigids(NativeRigid[] rigids, uint count);
		[DllImport("libowlsock")]
		private static extern int owlGetCameras(NativeCamera[] cameras, uint count);
		
		public static float MaxFrequency = 480.0f;

		public static int Init(string server, uint flags) {
			Done();
			return owlInit(server, flags);
		}

		public static void Done() {
			owlDone();
		}

		public static int GetError() {
			return owlGetError();
		}

		public static void SetFloat(uint parameter, float value) {
			owlSetFloat(parameter, value);
		}

		public static void SetInteger(uint parameter, int value) {
			owlSetInteger(parameter, value);
		}

		public static int GetMarkers(ref NativeMarker[] markers) {
			return owlGetMarkers(markers, (uint)markers.Length);
		}

		public static int GetRigids(ref NativeRigid[] rigids) {
			return owlGetRigids(rigids, (uint)rigids.Length);
		}

		public static int GetCameras(ref NativeCamera[] cameras) {
			return owlGetCameras(cameras, (uint)cameras.Length);
		}

		public static Quaternion ConvertQuaternion(Quaternion input) {
			return new Quaternion(input.x, input.y, -input.z, -input.w);
		}
	}

	[StructLayout(LayoutKind.Sequential), Serializable]
	public struct NativeMarker {
		public int id;
		public int frame;
		public float x;
		public float y;
		public float z;
		public float cond;
		public uint flag;
	};

	[Serializable]
	public class Marker {
		public int id;
		public int frame;
		public Vector3 position;
		public float condition;
		public uint flag;

		public void SetFromNative(NativeMarker native, bool ignoreLowCond=false) {
			id = native.id;
			frame = native.frame;
			if (!ignoreLowCond || (ignoreLowCond && native.cond > 0)) {
				position = new Vector3(native.x, native.y, -native.z);
			}
			condition = native.cond;
			flag = native.flag;
		}

		public static explicit operator Marker(NativeMarker native) {
			Marker ret = new Marker();
			ret.SetFromNative(native);
			return ret;
		}
	}

	[StructLayout(LayoutKind.Sequential), Serializable]
	public struct NativeRigid {
		public int id;
		public int frame;
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 7)]
		public Pose pose;
		public float cond;
		public uint flag;
	};

	[Serializable]
	public class Rigid {
		public int id;
		public int frame;
		public Vector3 position;
		public Quaternion orientation;
		public float condition;
		public uint flag;

		public void SetFromNative(NativeRigid native) {
			id = native.id;
			position = new Vector3(native.pose.px, native.pose.py, -native.pose.pz);
			// comes in as w, x, y, z
			// unity is -x, -z, -y, w
			orientation = Owl.ConvertQuaternion(new Quaternion(native.pose.rx, native.pose.ry, native.pose.rz, native.pose.rw));
			condition = native.cond;
			flag = native.flag;
		}

		public static explicit operator Phasespace.Rigid(NativeRigid native) {
			Phasespace.Rigid ret = new Phasespace.Rigid();
			ret.SetFromNative(native);
			return ret;
		}
	}

	[StructLayout(LayoutKind.Sequential), Serializable]
	public struct NativeCamera {
		public int id;
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 7)]
		public Pose pose;
		public float cond;
		public uint flag;
	};

	[Serializable]
	public class Camera {
		public int id;
		public Vector3 position;
		public Quaternion orientation;
		public float condition;
		public uint flag;

		public void SetFromNative(NativeCamera native) {
			id = native.id;
			position = new Vector3(native.pose.px, native.pose.py, -native.pose.pz);
			// comes in as w, x, y, z
			// unity is -x, -z, -y, w
			orientation = Owl.ConvertQuaternion(new Quaternion(native.pose.rx, native.pose.ry, native.pose.rz, native.pose.rw));
			condition = native.cond;
			flag = native.flag;
		}

		public static explicit operator Phasespace.Camera(NativeCamera native) {
			Phasespace.Camera ret = new Phasespace.Camera();
			ret.SetFromNative(native);
			return ret;
		}
	}

	[StructLayout(LayoutKind.Sequential), Serializable]
	public struct Pose {
		public float px;
		public float py;
		public float pz;
		public float rw;
		public float rx;
		public float ry;
		public float rz;
	}

	public enum Error {
		NoError = 0x0000,
		InvalidValue = 0x0020,
		InvalidEnum = 0x0021,
		InvalidOperation = 0x0022
	};

	public enum CommonFlags {
		Create = 0x0100,
		Destroy = 0x0101,
		Enable = 0x0102,
		Disable = 0x0103
	};

	public enum InitFlags {
		Slave = 0x0001,
		File = 0x0002,
		PostProcess = 0x0010,
		Mode1 = 0x0100,
		Mode2 = 0x0200,
		Mode3 = 0x0300,
		Mode4 = 0x0400,
		Laser = 0x0A00,
		Calib = 0x0C00,
		CalibPlanar = 0x0F00
	};

	/* Gets */
	public enum Get {
		Version = 0x0500,
		FrameNumber = 0x0510
	};

	/* Sets */
	public enum Set {
		Frequency = 0x0200,
		Streaming = 0x0201,
		Interpolation = 0x0202,
		Buttons = 0x0210,
		Markers = 0x0211,
		Rigids = 0x0212,
		Commdata = 0x0220,
		TimeStamp = 0x0221,
		Planes = 0x02A0,
		Detectors = 0x02A1,
		Images = 0x02A2,
		Transform = 0xC200,
		FrameBufferSize = 0x02B0
	};

	/* Trackers */
	public enum TrackerType {
		Point = 0x0300,
		Rigid = 0x0301
	};

	/* Markers */
	public enum MarkerCommand {
		SetLed = 0x0400,
		SetPosition = 0x0401,
		ClearMarker = 0x0402
	};
}


/*
// planar tracker (may be temporary)
OWL_PLANAR_TRACKER      0x030A

OWL_SET_FILTER          0x0310

// undocumented freatures
// use at your own risk
OWL_FEATURE0            0x03F0 // optical
OWL_FEATURE1            0x03F1 // offsets
OWL_FEATURE2            0x03F2 // projection
OWL_FEATURE3            0x03F3 // predicted
OWL_FEATURE4            0x03F4 // valid min
OWL_FEATURE5            0x03F5 // query min
OWL_FEATURE6            0x03F6 // storedepth
OWL_FEATURE7            0x03F7 // 
OWL_FEATURE8            0x03F8 // rejection
OWL_FEATURE9            0x03F9 // filtering
OWL_FEATURE10           0x03FA // window size
OWL_FEATURE11           0x03FB // LS cutoff
OWL_FEATURE12           0x03FC // off-fill
OWL_FEATURE_LAST        0x03FD // last feature

// calibration only
OWL_CALIB_TRACKER       0x0C01
OWL_CALIB_RESET         0x0C10
OWL_CALIB_LOAD          0x0C11
OWL_CALIB_SAVE          0x0C12
OWL_CALIBRATE           0x0C13
OWL_CAPTURE_RESET       0x0C20
OWL_CAPTURE_START       0x0C21
OWL_CAPTURE_STOP        0x0C22
OWL_CALIB_ACTIVE        0x0C30

// planar calib tracker (may be temporary)
OWL_CALIBPL_TRACKER     0x0CA1
*/


/*
// calibration only
OWL_CALIB_STATUS        0x0C51
OWL_CALIB_ERROR         0x0C52
*/