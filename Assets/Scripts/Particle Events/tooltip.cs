//tootip.cs
//based on tooltip.js

using UnityEngine;

namespace ToolTip {
	
public class tooltip : MonoBehaviour {
 
	public class values {
		public string name;
		public float phi;
		public float theta;
		public float length;
		public float range;
		public float pida;
		public float idtruth;
		public string origin;
		public int nhits;

		public values(string name, float phi, float theta, float length, 
			float range, float pida, float idtruth, string origin, int nhits){
			this.name = name;
			this.phi = phi;
			this.theta = theta;
			this.length = length;
			this.range = range;
			this.pida = pida;
			this.idtruth = idtruth;
			this.origin = origin;
			this.nhits = nhits;
		}
	}

	public UnityEngine.UI.Text title;
	public UnityEngine.UI.Text c1;
	public UnityEngine.UI.Text c2;
	public UnityEngine.UI.Text c3;

	void Start () {
		//gameObject.SetActive(false);
		//var testvals = new values("testing", 1, 2, 3, 4, 5, 6, 7, 8);
		//DispText(testvals);
	}

	void DispText(values v){
		title.text = v.name;
		c1.text = "Hits: " + v.nhits + "\n" + "Origin: " + v.origin.ToString() + "\n" + "Length: " + v.length * 0.1 + "[m]"; //AMCLEAN added v.length * 0.1 because we're in decimeters
		c2.text = "";
		c3.text = "";
		//c1.text = "Phi: " + v.phi + "\n" + "Theta: " + v.theta + "\n" + "Length: " + v.length;
		//c2.text = "Range: " + v.range + "\n" + "PIDA: " + v.pida + "\n" + "IDTruth: " + v.idtruth;
		//c3.text = "Origin: " + v.origin + "\n" + "NHits: " + v.nhits;
		gameObject.SetActive(true);
	}

	void Hide(){
		gameObject.SetActive(false);
	}



}
}