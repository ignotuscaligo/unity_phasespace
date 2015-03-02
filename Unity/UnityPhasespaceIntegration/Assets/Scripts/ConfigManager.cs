using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;

public class ConfigManager : MonoBehaviour {
	public string configFile = "traces.conf";

	private Dictionary<string, string> properties;

	public ConfigManager() : base() {
		properties = new Dictionary<string, string>();
	}

	public void Awake() {
		Debug.Log("Loading configuration");
		string fullPath = Application.dataPath+"/"+configFile;
		if (File.Exists(fullPath)) {
			readPropertiesFromFile(fullPath);
		}
		Debug.Log("Writing file with registered properties");
		writePropertiesToFile(fullPath);
		Debug.Log("Configuration finished");
	}

	private void readPropertiesFromFile(string path) {
		Debug.Log(String.Format("Reading properties from file: '{0}'", path));
		string[] lines = File.ReadAllLines(path);
		foreach (string line in lines) {
			Debug.Log(String.Format("Parsing string: '{0}'", line));
			string[] blocks = line.Split(':');
			if (blocks.Length == 2) {
				Debug.Log(String.Format("Adding property '{0}', value '{1}'", blocks[0], blocks[1]));
				SetProperty(blocks[0], blocks[1]);
			} else {
				Debug.Log("String invalid, skipping");
			}
		}
	}

	private void writePropertiesToFile(string path) {
		Debug.Log(String.Format("Writing properties to file: '{0}'", path));
		List<string> fileData = new List<string>();
		foreach (string key in properties.Keys) {
			Debug.Log(String.Format("Adding property to write: '{0}', '{1}'", key, properties[key]));
			fileData.Add(String.Format("{0}:{1}\n", key, properties[key]));
		}
		File.WriteAllLines(path, fileData.ToArray());
		Debug.Log("Write complete");
	}

	public void RegisterProperty(string key, string defaultValue = "") {
		if (!properties.ContainsKey(key)) {
			Debug.Log(String.Format("Registering property: '{0}'", key));
			properties.Add(key, defaultValue);
		}
	}

	public string GetProperty(string key, string defaultValue = "") {
		Debug.Log(String.Format("Getting property: '{0}'", key));
		RegisterProperty(key, defaultValue: defaultValue);
		return properties[key];
	}

	public void SetProperty(string key, string value) {
		Debug.Log(String.Format("Setting property: '{0}'", key));
		RegisterProperty(key, defaultValue: value);
		properties[key] = value;
	}
}

public class VisibleKeyValuePair {
	public string key;
	public string value;
}