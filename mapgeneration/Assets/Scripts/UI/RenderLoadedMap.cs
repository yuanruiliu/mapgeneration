using UnityEngine;
using System.Collections;

public class RenderLoadedMap : MonoBehaviour {
	private GameObject mapManager;
	private MapManager mapManagerScript;
	private bool isRenderingLoadFile;

	public void RenderMap(){
		mapManager = GameObject.FindWithTag ("MapManager");
		mapManagerScript = mapManager.GetComponent<MapManager> ();
		isRenderingLoadFile = mapManagerScript.RenderingLoadFile ();

		if (isRenderingLoadFile) {
			mapManagerScript.onRenderButtonClick();
		} else {
			Debug.Log ("Rendering not possible!");
		}
	}
}
