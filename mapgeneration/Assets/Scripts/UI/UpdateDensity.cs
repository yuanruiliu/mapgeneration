using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UpdateDensity : MonoBehaviour {
	private CreateSettings uiController = null;

	public void UpdatePercentageDensity () {
		GameObject uiControllerObj = GameObject.FindWithTag ("GameController");
		GameObject densitySlider = GameObject.FindWithTag ("DensitySlider");
		int sliderValue = 0;
		
		if (densitySlider != null) {
			sliderValue = (int) densitySlider.GetComponent <Slider>().value;
		} else {
			Debug.Log("densitySlider not found!");
			return;
		}
		
		if (uiControllerObj != null) {
			uiController = uiControllerObj.GetComponent <CreateSettings>();
			
			if (uiController == null) {
				Debug.Log ("Unable to find 'GameController' script");
				return;
			} else {
				uiController.UpdateDensity (sliderValue);
			}	
		} else {
			Debug.Log("uiControllerObj not found!");
		}
	}
}
