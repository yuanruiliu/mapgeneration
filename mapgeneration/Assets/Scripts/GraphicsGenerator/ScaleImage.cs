using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ScaleImage : MonoBehaviour, IScrollHandler {

	float scaleFactor = 0.2f;
	float maxScaleValue = 2.0f;
	float minScaleValue = 1.0f;

	public void ScaleUp () {
		Vector3 currentScale = this.transform.localScale;
		if (currentScale.x + scaleFactor > maxScaleValue) {
			this.transform.localScale = new Vector3(maxScaleValue, maxScaleValue, maxScaleValue);
			return;
		}
		this.transform.localScale += new Vector3(scaleFactor, scaleFactor, 0);
	}
	public void ScaleDown() {
		Vector3 currentScale = this.transform.localScale;
		if ((currentScale.x - scaleFactor) < minScaleValue) {
			this.transform.localScale = new Vector3(minScaleValue, minScaleValue, minScaleValue);
			return;
		}
		this.transform.localScale -= new Vector3(scaleFactor, scaleFactor, 0);
	}

	public void OnScroll(PointerEventData data)
	{
		PointerEventData ped = (PointerEventData)data;
		//scroll up on scroll wheel => delta.y = 1, scroll down on scroll wheel => delta.y = -1
		if (ped.scrollDelta.y > 0) {
			ScaleUp ();
		} else {
			ScaleDown ();
		}
		//Debug.Log(ped.scrollDelta.y);
	}

	public void SetMinScrollValue(float value)
	{
		minScaleValue = value;
	}
}
