using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class DecrementObjectives : MonoBehaviour {
	private CreateSettings uiController;
	private const int OBJECTIVE_NEUTRAL = 0;
	private const int OBJECTIVE_RED = 1;
	private const int OBJECTIVE_BLUE = 2;
	
	public void OnClick(int type){
		GameObject uiControllerObj = GameObject.FindWithTag ("GameController");
		
		if (uiControllerObj != null) {
			uiController = uiControllerObj.GetComponent <CreateSettings>();
			
			if (uiController == null) {
				Debug.Log ("Unable to find 'GameController' script");
				return;
			} else {
				switch(type){
					case OBJECTIVE_NEUTRAL:
						uiController.DecreaseNumObjs (OBJECTIVE_NEUTRAL);
						break;
					case OBJECTIVE_RED:
						uiController.DecreaseNumObjs (OBJECTIVE_RED);
						break;
					case OBJECTIVE_BLUE:
						uiController.DecreaseNumObjs (OBJECTIVE_BLUE);
						break;
					default:
						Debug.Log("Invalid type received in decrement function.");
						break;
				}
			}
		} else {
			Debug.Log("No uiControllerObj found!");
			return;
		}
	}
}
