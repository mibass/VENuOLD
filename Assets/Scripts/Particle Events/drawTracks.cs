//Script for drawing tracks
//    Call filterJSON to create a point array from a parsed JSON string,
//    this calls drawTracksFromArray() for each track filetered.
//Adapted from javascript written by Thomas Wester

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using SimpleJSON;
using System.IO;
using System.Linq;
using System.Collections.Generic;

public class drawTracks : MonoBehaviour {

	public GameObject dot;
	private string m_InGameLog = "";
	private Vector2 m_Position = Vector2.zero;
	public string trackAlgoName;
    public string trackAlgoName1;
    public string trackAlgoName2;
    public string trackAlgoName3;
 //   public string [] trackAlgoNames = new string[] {"recob::Tracks_trackkalmanhit__McRecobStage1", "recob::Tracks_pandoraNuKHit__McRecoStage2", "recob::Tracks_pandoraCosmicKHit__McRecoStage2" };
	public GameObject tooltip;

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
	public static float Collinear(Vector3 pt1, Vector3 pt2, Vector3 pt3) {
		//Determine if the points are collinear using Magnitude(AB x AC) = 0 => Collinear
		Vector3 side1 = pt2 - pt1; //AB
		Vector3 side2 = pt3 - pt1; //AC

		Vector3 crossprod = Vector3.Cross(side1, side2); // AB x AC

		return Vector3.Distance(crossprod,Vector3.zero);  // Magnitude(AB x AC)         
	}

	void filterJSON(JSONNode N, double threshold, string trackAlgoName, GameObject eventTracks) {
		//Stores the final number of points in the array
		int drawnPoints = 0;
		int totalTracks = N["record"]["tracks"][trackAlgoName].Count;

		//Loop over tracks: Decide which points to draw, then draw points and connection lines.
		for (int trackIndex = 0; trackIndex < totalTracks; trackIndex++) {   
			//Stores the endpoints of each track segment to be drawn
			List<Vector3> spacePointsArray = new List<Vector3>();

			//Loop over points in the track, define the first two points outside the loop as initial conditions
			int totalPoints = N["record"]["tracks"][trackAlgoName][trackIndex]["points"].Count;

			Vector3 pt1 = new Vector3(
				0.1f*N["record"]["tracks"][trackAlgoName][trackIndex]["points"][0][0].AsFloat,
				0.1f*N["record"]["tracks"][trackAlgoName][trackIndex]["points"][0][1].AsFloat,
				-0.1f*N["record"]["tracks"][trackAlgoName][trackIndex]["points"][0][2].AsFloat);
			Vector3 pt2 = new Vector3(
				0.1f*N["record"]["tracks"][trackAlgoName][trackIndex]["points"][1][0].AsFloat,
				0.1f*N["record"]["tracks"][trackAlgoName][trackIndex]["points"][1][1].AsFloat,
				-0.1f*N["record"]["tracks"][trackAlgoName][trackIndex]["points"][1][2].AsFloat);

			//Always include the first point
			spacePointsArray.Add(pt1); 

			//Loop over the remaining points in the track
			//Default value is -1f, so the collinearity will never be checked (i.e., all points are drawn for each track)
			for (int spacePointIndex = 2; spacePointIndex < totalPoints; spacePointIndex++) {
				Vector3 vec = new Vector3(
					0.1f*N["record"]["tracks"][trackAlgoName][trackIndex]["points"][spacePointIndex][0].AsFloat,
					0.1f*N["record"]["tracks"][trackAlgoName][trackIndex]["points"][spacePointIndex][1].AsFloat,
					-0.1f*N["record"]["tracks"][trackAlgoName][trackIndex]["points"][spacePointIndex][2].AsFloat);

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

			//Always push the last point. There might be an off-by-one error here because I think it's possible for
			//this last point to get added twice because of the above loop. Not a huge problem though.
			spacePointsArray.Add(pt2);
			drawTracksFromArray(eventTracks, trackIndex, spacePointsArray);
			drawnPoints += spacePointsArray.Count();
		}
		//P("Drawn Points: " + drawnPoints); //For debugging. Shows the number of endpoints.
	}

	void drawTracksFromArray(GameObject eventTracks, int index, List<Vector3> arr) {
		//Create a gameobject to hold box collider children and a line renderer
		GameObject trackObject = new GameObject();
		trackObject.transform.position = Vector3.zero; 
		trackObject.transform.rotation = Quaternion.identity;
		trackObject.name = "track" + index;
		trackObject.tag = "track";

		//Add the linerenderer to the object
		LineRenderer lr = trackObject.AddComponent<LineRenderer> ();
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
			segmentObject.SendMessage("SetTooltipRef", tooltip);
			//Wasn't able to get this working. Meant to scale collider boxes with camera distance.
			//segmentObject.AddComponent(ScaleColliderRelativeToCamera); 
			segmentObject.name = "segment" + i;
			segmentObject.transform.parent = trackObject.transform;
			segmentObject.transform.position = (transform.position + pt1 + transform.position + pt2) / 2.0f;

			float boxColliderOffset = 0.4f; //height and width of box collider
			BoxCollider bc = segmentObject.AddComponent<BoxCollider>();
			bc.transform.LookAt(transform.position + pt2);
			bc.center = Vector3.zero;
			Vector3 sv = new Vector3();
			sv.z = Vector3.Distance(pt1, pt2); //z is forward vector
			sv.x = boxColliderOffset;
			sv.y = boxColliderOffset;
			bc.size = sv;
		}
		trackObject.transform.parent = eventTracks.transform;
	}

	void Awake() {
		trackAlgoName = PlayerPrefs.GetString("trackAlgorithm");
		//-------------------------------------------------------
		//--- Loading/parsing is now handled by parseEvent.js ---
		//-------------------------------------------------------
	}

	void Start () {
	
	}

	public void drawEventTracks(JSONNode node, GameObject eventTracks){
		//Changing the second argument to a positive value forces tracks to only draw points deemed "uncollinear"
		//This was used when we tried to reduce the number of points drawn in tracks, but it really pales in 
		//in comparison to the number of spacepoints drawn, so we are just drawing all of the points in each track.
        //foreach (string trackAlgoName in trackAlgoNames) {
            filterJSON(node, -1f, trackAlgoName, eventTracks);
            filterJSON(node, -1f, trackAlgoName1, eventTracks);
            filterJSON(node, -1f, trackAlgoName2, eventTracks);
            filterJSON(node, -1f, trackAlgoName3, eventTracks);
        //}
    }

	void OnGUI() {
		m_Position = GUILayout.BeginScrollView(m_Position);
		GUILayout.Label(m_InGameLog);
		GUILayout.EndScrollView();
	}

}