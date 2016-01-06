using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class IncrementObjectives : MonoBehaviour {
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
						uiController.IncreaseNumObjs (OBJECTIVE_NEUTRAL);
						break;
					case OBJECTIVE_RED:
						uiController.IncreaseNumObjs (OBJECTIVE_RED);
						break;
					case OBJECTIVE_BLUE:
						uiController.IncreaseNumObjs (OBJECTIVE_BLUE);
						break;
					default:
						Debug.Log("Invalid type received in increment function.");
						break;
				}
			}
		} else {
			Debug.Log ("uiControllerObj not found!");
			return;
		}
	}
}
