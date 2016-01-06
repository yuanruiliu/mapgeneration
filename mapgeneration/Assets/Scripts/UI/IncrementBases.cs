using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class IncrementBases : MonoBehaviour {
	private CreateSettings uiController;

	public void OnClick(){
		GameObject uiControllerObj = GameObject.FindWithTag ("GameController");

		if (uiControllerObj != null) {
			uiController = uiControllerObj.GetComponent <CreateSettings>();
			
			if (uiController == null) {
				Debug.Log ("Unable to find 'GameController' script");
				return;
			} else {
				uiController.IncreaseNumBases ();
			}
		} else {
			Debug.Log("uiControllerObj not found!");
			return;
		}
	}
}