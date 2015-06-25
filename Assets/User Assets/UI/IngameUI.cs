using UnityEngine;
using System.Collections;
using UnityStandardAssets.CrossPlatformInput;

public class IngameUI : MonoBehaviour {

	private bool isInMenu;

	// Use this for initialization
	void Start () {
		isInMenu = false;
		LockCursor();
	}
	
	// Update is called once per frame
	void Update () {
		if (!isInMenu) {
			isInMenu = CrossPlatformInputManager.GetButtonDown ("Open Menu");
			LockCursor();
		} else {
			isInMenu = !CrossPlatformInputManager.GetButtonDown ("Fire1");
			UnlockCursor();
		}
	}

	void LockCursor () {
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
	}

	void UnlockCursor () {
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
	}
}