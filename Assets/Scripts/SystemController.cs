using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Assets.Helpers;

public class SystemController : MonoBehaviour {

	public static SystemController instance;
	public DictionaryHelper myDictionary;

	private List<GameObject> _cardList;
	private int _cardsChooseNum;

	[SerializeField] private AudioSource _audioSource;
	[SerializeField] private GameObject _hintPannel;

	// Use this for initialization
	void Awake() {
		MakeSingleton ();
		_cardsChooseNum = 9;
		myDictionary = new DictionaryHelper ("EnDic");
	}

	void Start(){
		GetListCard ();
		_audioSource.Play ();
	}

	void Update(){
		CheckSoundButtonState ();
		CheckHintButton ();
	}
	
	void MakeSingleton(){
		if (instance != null) {
			Destroy (gameObject);
		} else if (instance == null) {
			instance = this;
			DontDestroyOnLoad (instance);
		}
	}

	public void ToggleMusic(Button _myButton){
		_audioSource.mute = !_audioSource.mute;
		var _buttonColor = _myButton.GetComponent<Image> ();


		if (_audioSource.mute) {
			_buttonColor.color = Color.red;
		} else {
			_buttonColor.color = Color.white;
		}
	}

	void GetListCard(){
		var _tempList = Resources.LoadAll ("Cards");
		_cardList = new List<GameObject>();
		foreach (var card in _tempList) {
			_cardList.Add (card as GameObject);
		}
	}

	public void MakeListPlayableCards(List<GameObject> cardsChoose){
		while (cardsChoose.Count < _cardsChooseNum) {
			GameObject randomGO = _cardList [Random.Range (0, _cardList.Count)];
			if (!cardsChoose.Contains (randomGO)) {
				cardsChoose.Add (randomGO);
			}
		}
	}

	public void ClickHintButton(){
		_hintPannel.SetActive (true);
	}

	public bool GetAudioSrouceState(){
		return _audioSource.mute;
	}

	/// <summary>
	/// Checks the state of the sound button.
	/// Control sound button when change screen
	/// </summary>
	void CheckSoundButtonState(){
		GameObject _myButtonGO = GameObject.FindGameObjectWithTag ("SoundButton") as GameObject;
		Button _myButton = _myButtonGO.GetComponent<Button> ();

		_myButton.onClick.RemoveAllListeners ();
		_myButton.onClick.AddListener (() => ToggleMusic (_myButton));

		if (_audioSource.mute) {
			var _color = _myButtonGO.GetComponent<Image> ();
			_color.color = Color.red;
		}
	}

	void CheckHintButton(){
		GameObject _myButtonGO = GameObject.FindGameObjectWithTag ("HintButton") as GameObject;
		Button _myButton = _myButtonGO.GetComponent<Button> ();

		_myButton.onClick.RemoveAllListeners ();
		_myButton.onClick.AddListener (() => ClickHintButton ());
	}
}
