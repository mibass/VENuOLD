//__________________________________________________________________________________________________
//drawSpacePoints.cs
//
//Adapted from DrawSpacePoints.js
//__________________________________________________________________________________________________

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using SimpleJSON;
using System.IO;
using System.Linq;
using System.Collections.Generic;

public class drawSpacePoints : MonoBehaviour {

	public GameObject dot;
	public float maxPoints = 100;
	private string m_InGameLog = "";
	private Vector2 m_Position = Vector2.zero;
	public string spacePointAlgoName;
    public string spacePointAlgoName1;
    public string spacePointAlgoName2;
    public string spacePointAlgoName3;
    public string fileName;

	void Awake()
	{
//		maxPoints = PlayerPrefs.GetInt("maxSpacePoints");
		spacePointAlgoName = PlayerPrefs.GetString("pointAlgorithm");

		if(PlayerPrefs.HasKey("File To Load"))
		{
			fileName = PlayerPrefs.GetString("File To Load");
		}
		else
		{
			fileName = "prodgenie_bnb_nu_cosmic.json";
			Debug.Log("<color=red>No Event Loaded</color>");
		}
	}

	void P(string aText)
	{
		m_InGameLog += aText + "\n";
	}

	void DrawSpacePoints(JSONNode N, GameObject eventP, string spacePointAlgoName)
	{
		Debug.Log ("maxPoints = " + maxPoints);	
		List<GameObject> spacePointsArray = new List<GameObject>();
		int totalPts = N["record"]["spacepoints"][spacePointAlgoName].Count;

		//Calculate the proper iterator so that certain points can be skipped (for performance reasons)
		float iter = totalPts / maxPoints;

		//If the event has fewer points than the maximum number allowed, draw all of them.
		if (iter < 1.0f) {
			iter = 1.0f;
		}

		for(float key = 0.0f; key < totalPts; key += iter){
			GameObject clone;

			//Round the loop variable here (to get an index) to minimize rounding error in the loop.
			int roundKey = (int) Mathf.Round(key);

			clone = Instantiate(dot, transform.position, transform.rotation) as GameObject;
			clone.transform.position = transform.position + new Vector3(
				0.1f*N["record"]["spacepoints"][spacePointAlgoName][roundKey]["xyz"][0].AsFloat,
				0.1f*N["record"]["spacepoints"][spacePointAlgoName][roundKey]["xyz"][1].AsFloat,
				-0.1f*N["record"]["spacepoints"][spacePointAlgoName][roundKey]["xyz"][2].AsFloat);
			clone.transform.localScale = new Vector3(0.005f,0.005f,0.005f);
			clone.gameObject.layer = 10;
			clone.transform.parent = eventP.transform;
			clone.name = "point" + roundKey;
			clone.tag = "point";
			spacePointsArray.Add(clone);	
		}
	}

	void Start() 
	{

	}

	public void drawPoints(JSONNode node, GameObject eventPoints)
	{
		if(maxPoints != 0){
			DrawSpacePoints(node, eventPoints, spacePointAlgoName);
            DrawSpacePoints(node, eventPoints, spacePointAlgoName1);
            DrawSpacePoints(node, eventPoints, spacePointAlgoName2);
            DrawSpacePoints(node, eventPoints, spacePointAlgoName3);
        }
	}

	void  OnGUI()
	{
		m_Position = GUILayout.BeginScrollView(m_Position);
		GUILayout.Label(m_InGameLog);
		GUILayout.EndScrollView();
	}


	void Update () 
	{

	}


}