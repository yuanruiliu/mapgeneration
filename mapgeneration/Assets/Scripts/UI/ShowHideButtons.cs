using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ShowHideButtons : MonoBehaviour {
	private CreateSettings uiController;
	private int[] data;
	private Button incrementNeutralButton;
	private Button incrementBlueButton;
	private Button incrementRedButton;
	private Button decrementRedButton;
	private Button decrementBlueButton;

	private GameObject incrementRedButtonObject;
	private GameObject decrementRedButtonObject;
	private GameObject incrementBlueButtonObject;
	private GameObject decrementBlueButtonObject;
	
	private const int OBJECTIVES_NEUTRAL = 0;
	private const int OBJECTIVES_RED = 1;
	private const int OBJECTIVES_BLUE = 2;
	private const int BASES = 3;
	private const int MAX_NUM_BASES = 2;
	private const int HAS_ONE_BASE = 1;
	private const int INITIAL_NUM_RED_OBJ = 0;
	private const int INITIAL_NUM_BLUE_OBJ = 0;

	private int maxAllowedObjs;

	void Start(){
		incrementNeutralButton = GameObject.FindWithTag ("NeutralButtonUp").GetComponent<Button>();

		incrementBlueButtonObject = GameObject.FindWithTag ("BlueButtonUp");
		incrementBlueButton = incrementBlueButtonObject.GetComponent<Button>();
		decrementBlueButtonObject = GameObject.FindWithTag ("BlueButtonDown");
		decrementBlueButton = decrementBlueButtonObject.GetComponent<Button>();

		incrementRedButtonObject = GameObject.FindWithTag ("RedButtonUp");
		incrementRedButton = incrementRedButtonObject.GetComponent<Button> ();
		decrementRedButtonObject = GameObject.FindWithTag ("RedButtonDown");
		decrementRedButton = decrementRedButtonObject.GetComponent<Button> ();
	}

	public void ShowAndHideButtons(){
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

		if (data [BASES] == MAX_NUM_BASES) {
			incrementRedButtonObject.SetActive (true);
			decrementRedButtonObject.SetActive (true);
			incrementBlueButtonObject.SetActive (true);
			decrementBlueButtonObject.SetActive (true);

			if (data [OBJECTIVES_NEUTRAL] + data [OBJECTIVES_RED] + data [OBJECTIVES_BLUE] < maxAllowedObjs) {
				incrementNeutralButton.interactable = true;
				incrementBlueButton.interactable = true;
				incrementRedButton.interactable = true;
				decrementRedButton.interactable = false;
			} else if (data [OBJECTIVES_NEUTRAL] + data [OBJECTIVES_RED] + data [OBJECTIVES_BLUE] == maxAllowedObjs) {
				incrementRedButton.interactable = false;

				if(data[OBJECTIVES_RED] == INITIAL_NUM_RED_OBJ){
					decrementRedButton.interactable = false;
				}
			} else {
				Debug.Log ("Sum of total objectives (red + blue + neutral) exceeds allowed max sum.");
				return;
			}
		} else if (data [BASES] == HAS_ONE_BASE) {
			incrementBlueButtonObject.SetActive (true);
			decrementBlueButtonObject.SetActive (true);
			
			if(incrementRedButtonObject.activeInHierarchy){
				incrementRedButtonObject.SetActive (false);
				decrementRedButtonObject.SetActive (false);
				
				uiController.ResetRedObjs();
			}
			
			if(data[OBJECTIVES_NEUTRAL] + data[OBJECTIVES_BLUE] < maxAllowedObjs){
				incrementNeutralButton.interactable = true;
				incrementBlueButton.interactable = true;

				if(data[OBJECTIVES_BLUE] == INITIAL_NUM_BLUE_OBJ){
					decrementBlueButton.interactable = false;
				}
			} else if (data [OBJECTIVES_NEUTRAL] + data [OBJECTIVES_BLUE] == maxAllowedObjs){
				incrementBlueButton.interactable = false;
			} else {
				Debug.Log ("Sum of total objectives (blue + neutral) exceeds allowed max sum.");
				return;
			}
		} else {
			if(incrementBlueButtonObject.activeInHierarchy){
				incrementBlueButtonObject.SetActive (false);
				decrementBlueButtonObject.SetActive (false);

				uiController.ResetBlueObjs();
			}

			if(data[OBJECTIVES_NEUTRAL] < maxAllowedObjs){
				incrementNeutralButton.interactable = true;
			}
		}
	}
}
