using UnityEngine;
using System.Collections;

public class CheckLoadValidity : MonoBehaviour {
	private bool showButton;
	private GameObject mapManager;
	private MapManager mapManagerScript;
	private bool loadFileIsEmpty;

	public void OnClick(){
		mapManager = GameObject.FindWithTag ("MapManager");
		mapManagerScript = mapManager.GetComponent<MapManager> ();
		mapManagerScript.Load ();
		loadFileIsEmpty = mapManagerScript.LoadFileIsEmpty ();
		showButton = true;
	}

	void OnGUI(){
		if (loadFileIsEmpty) {
			if (showButton) {
				if (GUI.Button (new Rect (450, 450, 150, 50), "")) {
					showButton = false;
				} else {
					GUI.Label (new Rect (460, 465, 150, 20), "Load file is empty!");
				}
			}
		}
	}
}
