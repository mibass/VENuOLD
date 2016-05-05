//trackClick.cs
//based on trackClick.js written by: Thomas Wester
//Handles track selection, displaying tooltip.

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ToolTip;

 
public class trackClick : MonoBehaviour {
 
	public GameObject tooltipObject;

	void Deselect() {
		//Not sure how to hold a reference to all the tracks, so using FindGameObjectsWithTag
		GameObject[] objects = GameObject.FindGameObjectsWithTag("trackSelected");
		int objectCount = objects.Count();

		//Unhighlight any selected tracks, and set their layer to "track"
		foreach (GameObject obj in objects) {
			LineRenderer ln = obj.GetComponent<LineRenderer>();
			ln.SetColors(Color.cyan, Color.cyan);
			obj.tag = "track";
			//tooltipObject.transform.SetParent(tooltipParent.transform);
			tooltipObject.SetActive(false);
		}
	}

	void OnMouseOver() { 
		//Check for left-click or single touch, make sure there's no UI element in front
		if (Input.GetMouseButtonDown(0) && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) {
			if (gameObject.transform.parent.tag != "trackSelected") {
				//Deselect any other active tracks and display the tooltip
				Deselect();
				tooltipObject.SetActive(true);

				//Highlight the track
				LineRenderer ln = gameObject.transform.parent.GetComponent<LineRenderer>();        
				ln.SetColors(Color.red, Color.yellow);

				gameObject.transform.parent.tag = "trackSelected";  
				tooltip.values v = getInfo();

				tooltipObject.SendMessage("DispText", v);
			}
			else {
				Deselect();
			}
		}
	}

	tooltip.values getInfo() {
		//Get the values to display on the tooltip
		int nHits = gameObject.transform.parent.childCount;
		Vector3 origin = gameObject.transform.parent.GetChild(0).position;
		float length = 0f;

		GameObject seg = gameObject.transform.parent.GetChild(0).gameObject;
		BoxCollider bc = seg.GetComponent<BoxCollider>();

		Vector3 unitRotation = Vector3.Normalize(seg.transform.rotation.eulerAngles);

		//The segment object's origin is at the midpoint of the endpoints, so subtract half a length
		origin -= seg.transform.forward *  bc.size.z / 2f;

		//Sum up the lengths of each segment object's box colliders to get the total length.
		for (int i = 0; i < nHits; i++) {
			seg = gameObject.transform.parent.GetChild(i).gameObject;
			bc = seg.GetComponent<BoxCollider>();
			length += bc.size.z;
		}

		//Capitalize the first letter of the track name, and add a space between the number. E.g. "track0" -> "Track0"
		string name = gameObject.transform.parent.name;
		name = name.Substring(0, 1).ToUpper() + name.Substring(1, name.Length - 1);

		tooltip.values v = new tooltip.values(name, 0f, 0f, Mathf.Round(length * 100) / 100, 0f, 0f, 0f, origin.ToString(), nHits + 1);

		return v;
	}

	void Update() {

	}

	//Avoid using GameObject.Find by adding a reference. Also allows tooltipObject to be enabled/disabled.
	void  SetTooltipRef(GameObject tt){
		tooltipObject = tt;
	}

	void Start() { 

	}




}
