using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ToggleSymmetry : MonoBehaviour {
	private CreateSettings uiController = null;
	private bool isSymmetric;

	public void OnClick () {
		GameObject uiControllerObj = GameObject.FindWithTag ("GameController");
		GameObject symmetryCheckbox = GameObject.FindWithTag ("SymmetryToggle");
		if (symmetryCheckbox == null) {
			Debug.Log("symmetryCheckbox not found!");
		} else {
			isSymmetric = symmetryCheckbox.GetComponent<Toggle>().isOn;
		}
		
		if (uiControllerObj != null) {
			uiController = uiControllerObj.GetComponent <CreateSettings>();
			
			if (uiController == null) {
				Debug.Log ("Unable to find 'GameController' script");
				return;
			} else {
				uiController.UpdateSymmetricMap(isSymmetric);
			}
		} else {
			Debug.Log("uiControllerObj not found!");
			return;
		}
	}
}
