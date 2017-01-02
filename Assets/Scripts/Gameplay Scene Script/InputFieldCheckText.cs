using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class InputFieldCheckText : MonoBehaviour {
	private InputField _myInputField;

	void Awake(){
		_myInputField = GetComponent<InputField> ();
	}

	void Start(){
		_myInputField.onValueChanged.AddListener (val => CheckAll ());
	}

	void CheckAll(){
		DisableEnterButton ();
		UpperCaseFirstLetter ();
	}

	void DisableEnterButton(){
		if (Input.GetKey (KeyCode.Return)) {
			_myInputField.text = _myInputField.text.Replace ("\n", "");
		}
	}

	void UpperCaseFirstLetter(){
		_myInputField.text.Trim ();
		if (!string.IsNullOrEmpty (_myInputField.text)) {
			char[] _tempAllChar = _myInputField.text.ToCharArray ();
			if (char.IsLower (_tempAllChar [0]) && _tempAllChar.Length > 0) {
				_tempAllChar [0] = char.ToUpper (_tempAllChar [0]);
				_myInputField.text = new string (_tempAllChar);
			}
		}
	}
}
