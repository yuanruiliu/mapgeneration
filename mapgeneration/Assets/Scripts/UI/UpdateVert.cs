using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UpdateVert : MonoBehaviour {
	private CreateSettings uiController = null;
	
	public void UpdateVertDimensions () {
		GameObject uiControllerObj = GameObject.FindWithTag ("GameController");
		GameObject vertSlider = GameObject.FindWithTag ("VertSlider");
		int sliderValue = 0;
		
		if (vertSlider != null) {
			sliderValue = (int) vertSlider.GetComponent <Slider>().value;
		} else {
			Debug.Log("vertSlider not found!");
			return;
		}
		
		if (uiControllerObj != null) {
			uiController = uiControllerObj.GetComponent <CreateSettings>();
			
			if (uiController == null) {
				Debug.Log ("Unable to find 'GameController' script");
				return;
			} else {
				uiController.UpdateVert (sliderValue);
			}
		}
	}
}
