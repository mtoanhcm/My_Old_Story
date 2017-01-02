using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ScrollBarMove : MonoBehaviour {

	[SerializeField] private ScrollRect scrollrect;
	[SerializeField] private InputField story;
	private int num = 56;
	
	// Update is called once per frame
	void Update () {
		if (story.text.Length > num) {
			float a = 1 - (float)num / (float)story.characterLimit;
			scrollrect.verticalNormalizedPosition = a;
			num = story.text.Length + 5;
		}
	}
}
