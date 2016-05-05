//ARDrawParticleTracks.cs
//based on ARDrawParticleTracks.js
//Script for drawing tracks
//    Call filterJSON to create a point array from a parsed JSON string,
//    this calls drawTracksFromArray() for each track filetered.
//Somewhat experimental. We wanted to draw tracks in the AR environment but it did not work as well as we hoped.

using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using SimpleJSON;

 
public class ARDrawParticleTracks : MonoBehaviour {
 
	GameObject dot;
	string fileName;
	GameObject loadingText;
	GameObject parnt;
	private string m_InGameLog = "";
	private Vector2 m_Position = Vector2.zero;
	public Slider slidr;
	public float scalingFactor; //= 0.4f * slidr.value;
	string jsonString = "";

	IEnumerator<WWW> FinishDownload(string url) {

		WWW fileURL = new WWW (url); 
		//// Wait for the download to complete
		loadingText.SetActive (true);
		yield return fileURL;
		jsonString = fileURL.text;
		loadingText.SetActive (false);
	}

	void P(string aText) {
		m_InGameLog += aText + "\n";
	}


	void PlacePoint(Vector3 pt1) {
		GameObject clone;
		clone = Instantiate(dot, transform.position, transform.rotation) as GameObject;
		clone.transform.position = transform.position + pt1;
		clone.transform.localScale = new Vector3(0.005f,0.005f,0.005f);
	}

	//Returns the magnitude of "uncollinearity" (0 is perfectly collinear)
	float Collinear(Vector3 pt1, Vector3 pt2, Vector3 pt3) {
		//Determine if the points are collinear using Magnitude(AB x AC) = 0 => Collinear
		Vector3 side1 = pt2 - pt1; //AB
		Vector3 side2 = pt3 - pt1; //AC

		Vector3 crossprod = Vector3.Cross(side1, side2); // AB x AC

		return Vector3.Distance(crossprod,Vector3.zero);  // Magnitude(AB x AC)         
	}

	void filterJSON(JSONNode N, double threshold, string trackAlgoName) {
		//Stores the final number of points in the array
		int drawnPoints = 0;
		int totalTracks = N["record"]["tracks"][trackAlgoName].Count;

		//Loop over tracks: Decide which points to draw, then draw points and connection lines.
		for (int trackIndex = 0; trackIndex < totalTracks; trackIndex++) {   
			//Stores the points to be drawn
			List<Vector3> spacePointsArray = new List<Vector3>();

			/*
        for(var key : int = 0; key < N["record"]["spacepoints"]["recob::SpacePoints_cluster3d__RecoStage1"].Count; key++){
	        var clone : GameObject;
	 	    clone = Instantiate(dot , transform.position, transform.rotation);
    	    clone.transform.position = transform.position + Vector3(0.1*N["record"]["spacepoints"]["recob::SpacePoints_cluster3d__RecoStage1"][key]["xyz"][0].AsFloat,
    	                                                            0.1*N["record"]["spacepoints"]["recob::SpacePoints_cluster3d__RecoStage1"][key]["xyz"][1].AsFloat,
    	                                                           -0.1*N["record"]["spacepoints"]["recob::SpacePoints_cluster3d__RecoStage1"][key]["xyz"][2].AsFloat);
    	    clone.transform.localScale = Vector3(0.05,0.05,0.05);  
        }
*/

			//Loop over points in the track, define the first two points outside the loop as initial conditions
			int totalPoints = N["record"]["tracks"][trackAlgoName][trackIndex]["points"].Count;

			Vector3 pt1 = new Vector3(
				0.1f*scalingFactor*N["record"]["tracks"][trackAlgoName][trackIndex]["points"][0][0].AsFloat,
				0.1f*scalingFactor*N["record"]["tracks"][trackAlgoName][trackIndex]["points"][0][1].AsFloat,
				-0.1f*scalingFactor*N["record"]["tracks"][trackAlgoName][trackIndex]["points"][0][2].AsFloat);
			Vector3 pt2 = new Vector3(
				0.1f*scalingFactor*N["record"]["tracks"][trackAlgoName][trackIndex]["points"][1][0].AsFloat,
				0.1f*scalingFactor*N["record"]["tracks"][trackAlgoName][trackIndex]["points"][1][1].AsFloat,
				-0.1f*scalingFactor*N["record"]["tracks"][trackAlgoName][trackIndex]["points"][1][2].AsFloat);


			//Always include the first point
			spacePointsArray.Add(pt1); 

			//Loop over the remaining points in the track
			for (int spacePointIndex = 2; spacePointIndex < totalPoints; spacePointIndex++) {
				Vector3 vec = new Vector3(
					0.1f*scalingFactor*N["record"]["tracks"][trackAlgoName][trackIndex]["points"][spacePointIndex][0].AsFloat,
					0.1f*scalingFactor*N["record"]["tracks"][trackAlgoName][trackIndex]["points"][spacePointIndex][1].AsFloat,
					-0.1f*scalingFactor*N["record"]["tracks"][trackAlgoName][trackIndex]["points"][spacePointIndex][2].AsFloat);

				//If the next point is collinear, move the current endpoint to the new point without drawing anything
				//Also always include the final point.
				if (Collinear(pt1, pt2, vec) <= threshold && spacePointIndex != totalPoints - 1) {
					pt2 = vec;
				}
				else {
					if (spacePointIndex + 1 < totalPoints) {
						spacePointsArray.Add(pt2);
					}
					pt1 = pt2;
					pt2 = vec;
				}
			}
			//Always push the last point.
			spacePointsArray.Add(pt2);

			drawTracksFromArray(trackIndex, spacePointsArray);
			drawnPoints += spacePointsArray.Count();
		}
		//P("Drawn Points: " + drawnPoints);
	}

	void drawTracksFromArray(int index, List<Vector3> arr) {
		//Create a gameobject to hold box collider children and a line renderer
		GameObject trackObject = new GameObject();
		trackObject.transform.position = Vector3.zero; 
		trackObject.transform.rotation = Quaternion.identity;
		trackObject.name = "track" + index;
		trackObject.tag = "track";

		//Add the linerenderer to the object
		LineRenderer lr = trackObject.AddComponent<LineRenderer>();
		lr.useWorldSpace = true; //Don't set 0,0 to the parent GameObject's position
		lr.material = new Material(Shader.Find("Mobile/Particles/Additive"));
		lr.SetWidth(0.05f, 0.05f);
		lr.SetColors(Color.cyan, Color.cyan); 
		lr.gameObject.layer = 11;
		Vector3 pt0 = arr[0];

		lr.SetVertexCount(arr.Count());
		lr.SetPosition(0, transform.position + pt0);
		PlacePoint(pt0);
		for (int i = 1; i < arr.Count(); i++) {
			Vector3 pt1 = arr[i - 1];
			Vector3 pt2 = arr[i];
			lr.SetPosition(i,  transform.position + pt2);
			PlacePoint(pt2);

			//Make a game object for each segment to store on-click behavior and a box collider
			//Put this child object at the midpoint between the current two points
			GameObject segmentObject = new GameObject();
			segmentObject.layer = 11;
			segmentObject.AddComponent<trackClick>();
			//        segmentObject.AddComponent(ScaleColliderRelativeToCamera); 
			segmentObject.name = "segment" + i;
			segmentObject.transform.parent = trackObject.transform;
			segmentObject.transform.position = (transform.position + pt1 + transform.position + pt2) / 2.0f;

			BoxCollider bc;
			//        bc.isTrigger = true;  
			float boxColliderOffset = 0.4f; //height and width of box collider
			bc = segmentObject.AddComponent<BoxCollider>();
			bc.transform.LookAt(transform.position + pt2);
			bc.center = Vector3.zero;
			Vector3 sv = new Vector3();
			sv.z = Vector3.Distance(pt1, pt2); //z is forward vector
			sv.x = boxColliderOffset;
			sv.y = boxColliderOffset;
			bc.size = sv;
		}
	}

	void Awake() {
		if(PlayerPrefs.HasKey("File To Load") && PlayerPrefs.GetString("File To Load") != "") {
			fileName = PlayerPrefs.GetString("File To Load");
			Debug.Log ("Loading file: " + fileName);
		}
		else {
			Debug.Log("<color=purple>PlayerPrefs not Initialized. Using default event.</color>");
			fileName = "prod_eminus_0.1-2.0GeV_isotropic.json";
		}
	}

	void Start() {
		//Read in from a file (different paths for different platforms)
		scalingFactor = 0.4f * slidr.value;

		//Check if the fileName is a url or a path
		if (fileName.Contains("http")) {
			StartCoroutine(FinishDownload (fileName));
		}
		else{
			if (Application.platform == RuntimePlatform.Android) {
				string url="jar:file://" + Application.dataPath + "!/assets/" + fileName;
				StartCoroutine(FinishDownload (url));
			}
			else {
				StreamReader sr = new StreamReader(Application.streamingAssetsPath  + "/" + fileName);
				jsonString = sr.ReadToEnd();
				sr.Close();
			}
		}

		//Filter and draw the tracks from the JSON file.
		//Parameter 2 is the filter threshold, and parameter 3 is the algorithm name found in the JSON file.
		filterJSON(JSONNode.Parse(jsonString), -1, "recob::Tracks_cctrack__RecoStage1");
	}

	void OnGUI() {
		m_Position = GUILayout.BeginScrollView(m_Position);
		GUILayout.Label(m_InGameLog);
		GUILayout.EndScrollView();
	}



}
