using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GenerateBlueprint : MonoBehaviour {
	private CreateSettings uiController;
	private bool showButton;
	private int[] data; 

	public void OnClick(){
		GameObject uiControllerObj = GameObject.FindWithTag ("GameController");
		
		if (uiControllerObj != null) {
			uiController = uiControllerObj.GetComponent <CreateSettings>();
			
			if (uiController == null) {
				Debug.Log ("Unable to find 'GameController' script");
				return;
			} else {
				data = uiController.GetInputValues ();
				showButton = true;

				DrawBlueprint();

				ActivateButton activateButtonScript = GetComponent<ActivateButton> ();
				activateButtonScript.enabled = true;
			}
		} else {
			Debug.Log("No uiControllerObj found!");
			return;
		}
		
	}

	void OnGUI(){
		string isSymmetric;

		if(data.Length == 8){
			if (data [7] == 1) {
				isSymmetric = "true";
			} else {
				isSymmetric = "false";
			}

			if (showButton) {
				if (GUI.Button (new Rect (10, 10, 250, 170), "")) {
					showButton = false;
				} else {
					GUI.Label (new Rect (20, 15, 230, 20), "Neutral Objectives: " + data[0].ToString ());
					GUI.Label (new Rect (20, 35, 230, 20), "Red Objectives: " + data[1].ToString ());
					GUI.Label (new Rect (20, 55, 230, 20), "Blue Objectives: " + data[2].ToString ());
					GUI.Label (new Rect (20, 75, 230, 20), "Number of Bases: " + data[3].ToString ());
					GUI.Label (new Rect (20, 95, 230, 20), "Horizontal Dimensions: " + data[4].ToString ());
					GUI.Label (new Rect (20, 115, 230, 20), "Vertical Dimensions: " + data[5].ToString ());
					GUI.Label (new Rect (20, 135, 230, 20), "Cover Density: " + data[6].ToString ());
					GUI.Label (new Rect (20, 155, 230, 20), "Symmetric Map?: " + isSymmetric);
				}
			}
		} else {
			Debug.Log ("Buffer overflow.");
			return;
		}
	}

	void DrawBlueprint() {
		GameObject mapManagerGO = GameObject.FindWithTag("MapManager");
		MapManager mapManagerScript = mapManagerGO.GetComponent<MapManager>();
		mapManagerScript.StartMapGenerationProcess();
	}
}






