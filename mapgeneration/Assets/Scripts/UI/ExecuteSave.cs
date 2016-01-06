using UnityEngine;
using System.Collections;

public class ExecuteSave : MonoBehaviour {
	public void Save(){
		GameObject mapManager = GameObject.FindWithTag ("MapManager");
		MapManager mapManagerScript = mapManager.GetComponent<MapManager> ();
		mapManagerScript.Save ();
	}
}
