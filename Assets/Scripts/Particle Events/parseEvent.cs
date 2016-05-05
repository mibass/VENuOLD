//  ----------------
//  - parseEvent.cs -
//  ----------------
//  Parses the JSON files and then calls the drawTracks and drawSpacepoint scripts.

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using SimpleJSON;
using System.IO;
using System.Linq;
using System.Collections.Generic;

public class parseEvent : MonoBehaviour {

	public string fileName;
	public GameObject loadingText;
	public GameObject nextEventButton;
	public GameObject prevEventButton;
	string jsonString = "";

	IEnumerator<WWW> FinishDownload(string url) {

		WWW fileURL = new WWW (url); 
		//// Wait for the download to complete
		loadingText.SetActive (true);
		yield return fileURL;
		jsonString = fileURL.text;
		loadingText.SetActive (false);
	}

	#if UNITY_EDITOR  // PrefabUtility is only defined in the Unity Editor
	static void CreateNew(GameObject obj, string localPath) {
		Object prefab = PrefabUtility.CreateEmptyPrefab(localPath);
		PrefabUtility.ReplacePrefab(obj, prefab, ReplacePrefabOptions.ConnectToPrefab);
	}
	#endif

	void Start () {
		#if UNITY_EDITOR // If we are in the Unity Editor, parse the JSON files and make prefabs of events
		Debug.Log ("Inside parseEvent -> Start");
		if (PlayerPrefs.HasKey ("File To Load") && PlayerPrefs.GetString ("File To Load") != "") {
			fileName = PlayerPrefs.GetString ("File To Load");
			Debug.Log ("Loading file: " + fileName);
		} else {
			Debug.Log ("<color=purple>PlayerPrefs not Initialized. Using default event.</color>");
			fileName = "prod_eminus_0.1-2.0GeV_isotropic.json";
		}

		if (PlayerPrefs.HasKey ("EventSource") && PlayerPrefs.GetString ("EventSource") == "local") {
			//Enable next/previous event buttons
			nextEventButton.SetActive (true);
			prevEventButton.SetActive (true);
		} else {
			//Disable next/previous event buttons
			nextEventButton.SetActive (false);
			prevEventButton.SetActive (false);
		}


		//Check if the fileName is a url or a path
		if (fileName.Contains ("http")) {
			StartCoroutine(FinishDownload (fileName));
		} else {
			if (Application.platform == RuntimePlatform.Android) {
				string url = "jar:file://" + Application.dataPath + "!/assets/" + fileName;
				StartCoroutine(FinishDownload (url));
			} else {
				StreamReader sr = new StreamReader (Application.streamingAssetsPath + "/" + fileName);
				jsonString = sr.ReadToEnd ();
				sr.Close ();
			}
		}

		JSONNode node = JSONNode.Parse (jsonString);
		Debug.Log ("Parsing jsonString: " + jsonString);
		GameObject Event = new GameObject ();
		GameObject eventTracks = new GameObject ();
		GameObject eventPoints = new GameObject ();
		Event.name = "Event_" + fileName;
		Event.tag = "event";
		eventTracks.name = "Tracks_" + fileName;
		eventPoints.name = "Points_" + fileName;
		eventTracks.transform.parent = Event.transform;
		eventPoints.transform.parent = Event.transform;
		//Call the draw functions in the other scripts
		GetComponent<drawTracks>().drawEventTracks(node, eventTracks);
		GetComponent<drawSpacePoints>().drawPoints(node, eventPoints);
		string localPath = "Assets/Resources/" + fileName + ".prefab";
		CreateNew(Event, localPath);
		#else // If not in the Unity Editor, do something:
		#endif
	}
}