using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UpdateHorz : MonoBehaviour {
	private const int MAX_ALLOWED_DIMENSION_SIZE = 50;
	private CreateSettings uiController = null;

	public void UpdateHorzDimensions () {
		GameObject uiControllerObj = GameObject.FindWithTag ("GameController");
		GameObject horzSlider = GameObject.FindWithTag ("HorzSlider");
		int sliderValue = 0;

		if (horzSlider != null) {
			sliderValue = (int) horzSlider.GetComponent <Slider>().value;
		} else {
			Debug.Log("horzSlider not found!");
			return;
		}
		
		if (uiControllerObj != null) {
			uiController = uiControllerObj.GetComponent <CreateSettings>();
			
			if (uiController == null) {
				Debug.Log ("Unable to find 'GameController' script");
				return;
			} else {
				uiController.UpdateHorz (sliderValue);
			}
		}
	}
}
