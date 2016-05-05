using UnityEngine;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine.UI;

public class eventScroller : MonoBehaviour {

	public GameObject[] events;
	public GameObject currentEvent;
	int eventNumber = 1;

	// Use this for initialization
	void Start() {
//		#if UNITY_EDITOR

//		#else
		// Loads prefab gameobjects located in Resources
		// folder in your project's Assets folder.
		events = Resources.LoadAll("", typeof(GameObject))
			.Cast<GameObject>()
			.ToArray();

		for (int i = 0; i < events.Length; i++) {
			events[i].name = i.ToString ();
		}
		foreach(GameObject ev in events) {
			Debug.Log("found event " + ev.name);
		}
		Debug.Log("loaded event is " + events[eventNumber].name);
		currentEvent = Instantiate(events[eventNumber]) as GameObject;
		currentEvent.SetActive (true);
//		#endif
	}
	
	public bool clicked() {
            return Input.anyKeyDown && !Input.GetKey(KeyCode.F);
	}

	// Update is called once per frame
	void Update () {
		if (clicked ()) {
//			#if UNITY_EDITOR
//			LoadNextJSONEvent();
//			#else
			currentEvent.SetActive (false);
			if (eventNumber < events.Length - 1) {
				eventNumber++;
			} else {
				eventNumber = 1;
			}
			Debug.Log("loaded event is " + events[eventNumber].name);
			currentEvent = Instantiate(events[eventNumber]) as GameObject;
			currentEvent.SetActive (true);
//			#endif
		}
	}
		

	public void LoadNextJSONEvent(){
		string currentEvent = PlayerPrefs.GetString("File To Load");
		DirectoryInfo dir = new DirectoryInfo(Application.streamingAssetsPath);
		FileInfo[] filesInfo = dir.GetFiles("*.json");
		int currentIndex = -1;

		for(int i = 0; i < filesInfo.Length; i++)
			if(filesInfo[i].Name == currentEvent)
				currentIndex = i;

		//if(currentIndex == filesInfo.Length - 1){
		//	//no more files!
		//	Debug.Log("No more files!");
		//}
		if(currentIndex == -1){
			//don't know where we are. did File To Load not get set?
			Debug.Log("file not found!");

		}
		else{
			PlayerPrefs.SetString("File To Load", filesInfo[(currentIndex + 1) % filesInfo.Length].Name);
			Debug.Log("loading file " + PlayerPrefs.GetString("File To Load"));
			Application.LoadLevel(Application.loadedLevel);
			//the file is loaded elsewhere. All that script needs is the name of the new file.
			}
	}


}
