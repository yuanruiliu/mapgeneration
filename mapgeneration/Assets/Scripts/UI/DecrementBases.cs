using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class DecrementBases : MonoBehaviour {
	private CreateSettings uiController;
	
	public void OnClick(){
		GameObject uiControllerObj = GameObject.FindWithTag ("GameController");
		
		if (uiControllerObj != null) {
			uiController = uiControllerObj.GetComponent <CreateSettings>();
			
			if (uiController == null) {
				Debug.Log ("Unable to find 'GameController' script");
				return;
			} else {
				uiController.DecreaseNumBases ();
			}
		} else {
			Debug.Log("No uiControllerObj found!");
			return;
		}
		
	}
}
