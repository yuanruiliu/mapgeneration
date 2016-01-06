using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CheckButtonClickability : MonoBehaviour{
	private CreateSettings uiController;
	private int[] data;
	private Button buttonToCheck;
	
	private const int OBJECTIVES_NEUTRAL = 0;
	private const int OBJECTIVES_RED = 1;
	private const int OBJECTIVES_BLUE = 2;
	private const int BASES = 3;
	
	private const int MIN_NUM_OBJECTIVES = 0;
	private const int MAX_NUM_OBJECTIVES = 5;
	private const int MIN_NUM_BASES = 0;
	private const int MAX_NUM_BASES = 2;

	private string[] buttonDownTags = {"NeutralButtonDown", "RedButtonDown", "BlueButtonDown"};
	private string[] buttonUpTags = {"NeutralButtonUp", "RedButtonUp", "BlueButtonUp"};
	private int maxAllowedObjs;

	public void UpdateButtonColor(bool is_base_change){
		GameObject uiControllerObj = GameObject.FindWithTag ("GameController");
		
		if (uiControllerObj != null) {
			uiController = uiControllerObj.GetComponent <CreateSettings>();
			maxAllowedObjs = uiController.GetAllowedMaxObjectives ();
		}
		if (uiController == null) {
			Debug.Log ("Unable to find 'GameController' script");
			return;
		}
		data = uiController.GetInputValues ();

		if (is_base_change) {
			Button basesButtonUp = GameObject.FindWithTag ("BasesButtonUp").GetComponent<Button>();
			Button basesButtonDown = GameObject.FindWithTag ("BasesButtonDown").GetComponent<Button>();

			if(data[BASES] == MIN_NUM_BASES){
				basesButtonDown.interactable = false;
				basesButtonUp.interactable = true;
			} else if (data[BASES] == MAX_NUM_BASES){
				basesButtonDown.interactable = true;
				basesButtonUp.interactable = false;
			} else {
				basesButtonDown.interactable = true;
				basesButtonUp.interactable = true;
			}

		}

		for (int i = 0; i < buttonDownTags.Length; i++) {
			GameObject taggedObject = GameObject.FindWithTag (buttonDownTags[i]);

			if(taggedObject != null){
				buttonToCheck = taggedObject.GetComponent<Button>();

				if(data[i] == MIN_NUM_OBJECTIVES){
					buttonToCheck.interactable = false;
				} else {
					buttonToCheck.interactable = true;
				}
			}
		}

		for(int i = 0; i < buttonUpTags.Length; i++){
			GameObject taggedObject = GameObject.FindWithTag (buttonUpTags[i]);

			if(taggedObject != null){
				buttonToCheck = taggedObject.GetComponent<Button>();

				if (data [OBJECTIVES_NEUTRAL] + data [OBJECTIVES_RED] + data [OBJECTIVES_BLUE] < maxAllowedObjs) {
					buttonToCheck.interactable = true;
				} else {
					buttonToCheck.interactable = false;
				}
			}
		}
	}
}