using UnityEngine;

public class KeyboardOrbit : MonoBehaviour {

	public float speed = 10.0f;
	private bool showCursor = true;

	void Start()
	{
		Cursor.visible = true;
	}

	public void Update()
	{
		float speedX = speed * Input.GetAxisRaw("Horizontal");
		float speedZ = speed * Input.GetAxisRaw("Vertical");

		//move camera
		gameObject.transform.Translate(new Vector3(speedX, 0.0f, speedZ));

		//0 = left click, 1 = right click, 2 = middle click
		//in this case, we want to toggle cursor with left click
		/*if (Input.GetMouseButtonDown(0))
		{
			ToggleShowCursor();
		}*/
	}
	/*
	void ToggleShowCursor()
	{
		showCursor = !showCursor;
		Cursor.visible = showCursor;
	}*/
}