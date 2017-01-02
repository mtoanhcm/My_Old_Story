using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.IO;
using UnityEngine.SceneManagement;
using System.Linq;
using System.Text;
using System;
using System.Text.RegularExpressions;

public class GameplayController : MonoBehaviour {

	public static GameplayController instance;

	[SerializeField] private GameObject _cardPosition, _cardBoardPosition, _cardSpawnPosition = null;
	[SerializeField] private TextAsset _dictionarySource;
	[SerializeField] private Button _hintButton, _soundButton;
	[SerializeField] private GameObject _popUpPannel;
	[SerializeField] private Scrollbar _inputViewBar;

	//Use for show input story field
	[SerializeField] GameObject _endstoryPannel;
	[SerializeField] Text _storyText;

	//Use for input story
	[SerializeField] private GameObject _inputArea;
	[SerializeField] private InputField _inputField;
	[SerializeField] Text _descriptText;
	private Transform _cardTransForStoryText;
	private string _inputPannelText;


	private Transform[] _spawnPos, _cardBoardPos, _cardFramePos;
	private List<GameObject> _cardsChooseRandom, _cardsPlayable;
	private bool _isMoveBack;

	//Use for Do Swap function
	private bool _isPlace, _isSwap;
	private Card _myCard, _targetCard;
	private Vector3 _myHolderPos, _myBoardPos, _myFramePos, _targetHolderPos;

	/// <summary>
	/// Make Instance Plattern
	/// </summary>
	void MakeInstance(){
		if (instance == null) {
			instance = this;
		}
	}

	private TextAsset _testText;
	// Use this for initialization
	void Awake () {
		MakeInstance ();
		_cardsChooseRandom = new List<GameObject> ();
		_cardsPlayable = new List<GameObject> ();
		SystemController.instance.MakeListPlayableCards (_cardsChooseRandom);

		//Get all position to array
		_spawnPos = _cardSpawnPosition.transform.GetComponentsInChildren<Transform>();
		_cardBoardPos = _cardBoardPosition.transform.GetComponentsInChildren<Transform> ();//Position at Board
		_cardFramePos = _cardPosition.transform.GetComponentsInChildren<Transform> ();//Position at Frame
	}

	void Start(){
		CreateCardsFromList ();
	}

	void Update(){
		RunMoveBack ();
		SetAllisPlaceTrue ();
		EnableFrameIfFramePosHaveCard ();
	}

	/// <summary>
	/// Create and show gameobject Cards from randomList Card
	/// </summary>
	void CreateCardsFromList(){
		int i=1;
		foreach (GameObject card in _cardsChooseRandom) {
			GameObject _tempCard = Instantiate (card, _spawnPos [i].position, Quaternion.identity) as GameObject;
			_tempCard.GetComponent<Card> ().BoardPos (_cardBoardPos [i]);
			_tempCard.transform.parent = null;
			i++;
		}
	}

	//Click Button Hint
	public void ClickHintBUtton(){
		SystemController.instance.ClickHintButton ();
	}

	/// <summary>
	/// Use in Card script, add card in playable list with each card create
	/// </summary>
	/// <param name="card">Card.</param>
	public void CreateListCardsPlayable(GameObject card){
		_cardsPlayable.Add (card);
	}

	/// <summary>
	/// Enable or Disable box collider at each Frame position.
	/// Bcs OnMouseDown event will detect random if there are 2 or more collider boxes at the same position
	/// </summary>
	/// <param name="_myPlace">Placeholder of Card.</param>
	/// <param name="_isEmpty">If set to true is empty.</param>
	public void SetBoxCardHolder(Vector3 _myPlace, bool _isEmpty){
		for (int i = 1; i < _cardFramePos.Length; i++) {
			if (_cardFramePos [i].position == _myPlace) {
				_cardFramePos [i].gameObject.GetComponent<BoxCollider2D> ().enabled = _isEmpty;
			}
		}
	}

	/// <summary>
	/// Set all _isPlace of cards are true --> Fix case if drag card too fast will make colliders aren't detect
	/// </summary>
	public void SetAllisPlaceTrue(){
		foreach (GameObject _card in _cardsPlayable) {
			var _cardScript = _card.GetComponent<Card> ();
			for(int i =0;i<_cardFramePos.Length;i++){
				if (!_cardScript.GetIsDrag () && !_cardScript.GetIsSwap()) {
					if (_cardScript.GetHolderPos () == _cardFramePos [i].position) {
						_cardScript.SetIsPlace (true);
		
					}
				}
			}
		}
	}

	#region swap position code

	/// <summary>
	/// Dos the swap position.
	/// Use in Card.cs, Active when player release mouse button
	/// </summary>
	/// <param name="_myTrans">My Drag Card.</param>
	/// <param name="_targetTrans">Other target is collider by card drag.</param>
	public void DoSwapPos(Transform _myTrans, Transform _targetTrans){
		_myCard = _myTrans.GetComponent<Card> ();
		_isPlace = _myCard.GetIsPlace ();
		_isSwap = _myCard.GetIsSwap ();
		Vector3 _myHolderPos = _myCard.GetHolderPos ();

		//If drag out of all position needed, card return board
		if (!_isPlace && !_isSwap) {
			Vector3 _boardPos = _myCard.GetBoardPos ();
			_myCard.SetHolderPos (_boardPos);

		}

		//if drag in frame position, card place in frame
		if (_isPlace) {
			Vector3 _framePos = _targetTrans.position;
			_myCard.SetHolderPos (_framePos);
		}

		//if card drag on another card, and swap it
		if (_isSwap) {
			_targetCard = _targetTrans.gameObject.GetComponent<Card> ();
			_targetHolderPos = _targetCard.GetHolderPos ();

			//if my card drag from board
			if (_myHolderPos == _myCard.GetBoardPos ()) {
				_myHolderPos = _targetCard.GetHolderPos ();//Set card pos to target card holder pos
				_myCard.SetHolderPos (_myHolderPos);//re-set card holder pos
				_targetCard.SetHolderPos (_targetCard.GetBoardPos ());//re-set target holder pos to target board pos

				_myCard.SetIsSwap (false);
			}

			//If my card drag from another frame
			else {
				Vector3 _temPos = _myHolderPos;
				_myCard.SetHolderPos (_targetHolderPos);
				_targetCard.SetHolderPos (_temPos);


				_myCard.SetIsSwap (false);
				_myCard.SetIsPlace (true);
				_targetCard.SetIsPlace (true);
				SetBoxCardHolder (_targetCard.GetHolderPos (), false);
			}
		}
	}

	#endregion

	#region Move Back all Cards

	/// <summary>
	/// Use for cards auto run back to their holder positions
	/// </summary>
	void RunMoveBack(){
		if (_isMoveBack) {
			foreach (GameObject _card in _cardsPlayable) {
				var _scriptCard = _card.GetComponent<Card> ();
				if (_scriptCard != null) {
					_card.transform.position = Vector3.MoveTowards (_card.transform.position, _scriptCard.GetHolderPos (), 40 * Time.deltaTime);
				}
			}
		}

		if (!CheckCompleteMoveBackPosOfAllCards ()) {
			_isMoveBack = false;
			SetStoryTextToDescriptText ();
		}
	}

	/// <summary>
	/// Use in Card.cs.
	/// enable move back if card position not in its holder position
	/// </summary>
	/// <param name="value">If set to <c>true</c> value.</param>
	public void SetIsMoveBack(bool value){
		_isMoveBack = value;
	}

	/// <summary>
	/// Use to check if all cards are on holder position or not.
	/// </summary>
	/// <returns><c>true</c>, if complete move back position of all cards was checked, <c>false</c> otherwise.</returns>
	bool CheckCompleteMoveBackPosOfAllCards(){
		bool _isAllCardsAtWrongPos = false;
		foreach (GameObject _card in _cardsPlayable) {
			var _scriptCard = _card.GetComponent<Card> ();
			if (_card.transform.position != _scriptCard.GetHolderPos ()) {
				_isAllCardsAtWrongPos = true;
			}
		}

		return _isAllCardsAtWrongPos;
	}

	#endregion

	#region story input and show

	/// <summary>
	/// Use at Card.cs
	/// Get current card Transform, and add card story text to input field
	/// </summary>
	/// <param name="_myTrans">My trans.</param>
	public void GetCardTransformForSaveStoryText(Transform _myTrans){
		var _scriptCard = _myTrans.GetComponent<Card> ();
		_cardTransForStoryText = _myTrans;//Cache card transform to use later

		string _tempCardStory = _scriptCard.GetMyStoryText ();

		//Remove color mark of text
		if (_tempCardStory != null) {
			_tempCardStory = _tempCardStory.Replace("<color=#ff0000ff>","").Replace("</color>","");
		}

		_inputField.text = _tempCardStory;//Add card story text to input field when start input
	}

	/// <summary>
	/// Adds the text from input to card.
	/// Use when complete typing story text of card in input field
	/// </summary>
	public void AddTextFromInputToCard(){
		if (_myCard != null) {
			var _scriptCard = _cardTransForStoryText.GetComponent<Card> ();

			_inputPannelText = _inputField.text;
			//Auto add "." at the end
			string _textEnd = "";

			if (_inputPannelText != "") {
				string _tempEndOfText = _inputPannelText [_inputPannelText.Length - 1].ToString ();
				if (_tempEndOfText != "." && _tempEndOfText != "?" && _tempEndOfText != "!") {
					_textEnd = ".";
				}

				_inputPannelText += _textEnd;
			}

			CheckSpell ();
			_scriptCard.SetMyStoryText (_inputPannelText);
		}
	}

	/// <summary>
	/// Sets the box collider for all cards.
	/// Bcs OnMouseDown still detect collider box when input pannel show, so we have to disable it
	/// </summary>
	/// <param name="_value">If set to <c>true</c> value.</param>
	void SetIsBoxColliderOfAllCards(bool _value){
		foreach (GameObject _card in _cardsPlayable) {
			_card.GetComponent<BoxCollider2D> ().enabled = _value;
		}
	}

	/// <summary>
	/// Show review all story in review area when close input field
	/// </summary>
	/// <param name="_myCard">My card.</param>
	public void SetStoryTextToDescriptText(){
		_descriptText.text = ShowReviewContent ();
	}

	/// <summary>
	/// Use when double click on card
	/// </summary>
	public void ShowInputFields(){
		_inputArea.SetActive (true);
		SetIsBoxColliderOfAllCards (false);
		_inputViewBar.value = 1f;
	}

	/// <summary>
	/// Use when click button OK --> finish input story text
	/// </summary>
	public void CloseInputFiedls(){
		AddTextFromInputToCard ();
		_inputArea.SetActive (false);
		SetIsBoxColliderOfAllCards (true);


		SetStoryTextToDescriptText ();
		UpperCaseFirstLetter (_descriptText);


		_inputField.text = "";

		//EnableFrameIfFramePosHaveCard (_cardTransForStoryText);
	}

	void UpperCaseFirstLetter(Text _text){
		_text.text.Trim ();
		if (!string.IsNullOrEmpty (_text.text)) {
			char[] _tempAllChar = _text.text.ToCharArray ();
			if (char.IsLower (_tempAllChar [0]) && _tempAllChar.Length > 0) {
				_tempAllChar [0] = char.ToUpper (_tempAllChar [0]);
				_text.text = new string (_tempAllChar);
			}
		}
	}

	/// <summary>
	/// Checks the spell correct of text.
	/// </summary>
	void CheckSpell(){
		string _reviewText = _inputPannelText;
		if (_reviewText != "") {
			string[] _words = _reviewText.Split (' ');

			for (int i = 0; i < _words.Length; i++) {
				string _tempWords = _words [i];
				_tempWords = _tempWords.ToLower ();
				//_tempWords = _tempWords.Replace (".", "").Replace (",", "").Replace ("?", "").Replace ("!", "");
				_tempWords = Regex.Replace (_tempWords,"[^0-9A-Za-z]+","");
				if (!SystemController.instance.myDictionary.CheckText (_tempWords)) {
					_words [i] = "<color=#ff0000ff>" + _words [i] + "</color>";
				}
			}

			_reviewText = string.Join (" ", _words);

			_inputPannelText = _reviewText;
		}
	}

	string ShowReviewContent(){

		string _tempCardStory = "";
		string _tempCardStoryChoosen = "";
		StringBuilder _tempContent = new StringBuilder (GetCompleteTextWithOrder ());

		if (_cardTransForStoryText !=null) {
			var _scriptCard = _cardTransForStoryText.GetComponent<Card> ();
			_tempCardStory = _scriptCard.GetMyStoryText ();
			if (_tempCardStory != null) {
				if (_tempCardStory.Length > 0) {
					_tempCardStoryChoosen = "<i><b>" + _tempCardStory + "</b></i>";
		
					_tempContent.Replace (_tempCardStory, _tempCardStoryChoosen);
				}
			}
		}
		return _tempContent.ToString();
	}

	#endregion

	/*public void EnableFrameIfFramePosHaveCard(Transform _card){
		Card _cardScript = _card.GetComponent<Card> ();
		string _tempCardStoryText = _cardScript.GetMyStoryText ();
		for (int i = 1; i < _cardFramePos.Length; i++) {
			if (_cardFramePos [i].position == _cardScript.GetHolderPos()) {
				if (!string.IsNullOrEmpty (_tempCardStoryText)) {
					_cardFramePos [i].GetChild (0).gameObject.SetActive (true);
				} else {
					_cardFramePos [i].GetChild (0).gameObject.SetActive (false);
				}
			}
		}

		//DisableFrameIfFramePosEmpty ();
	}*/

	/// <summary>
	/// Enables the frame if frame position have card and card have its story
	/// </summary>
	void EnableFrameIfFramePosHaveCard(){
		for (int i = 1; i < _cardFramePos.Length; i++) {
			var _cardFramePosBox = _cardFramePos[i].GetComponent<BoxCollider2D> ();
			if (!_cardFramePosBox.enabled) {
				foreach (GameObject _card in _cardsPlayable) {
					var _scriptCard = _card.GetComponent<Card> ();
					if (_scriptCard.GetHolderPos () == _cardFramePos [i].position) {
						string _tempStoryCard = _scriptCard.GetMyStoryText ();

						if (!string.IsNullOrEmpty (_tempStoryCard)) {
							_cardFramePos [i].GetChild (0).gameObject.SetActive (true);
						} else {
							_cardFramePos [i].GetChild (0).gameObject.SetActive (false);
						}
					}
				}
			} else {
				_cardFramePos [i].GetChild (0).gameObject.SetActive (false);
			}
		}
	}

	#region show story field

	/// <summary>
	/// Shows the end story pannel.
	/// Add the begin text of story
	/// Get all story texts of cards and arrange them by order
	/// </summary>
	public void ShowEndStoryPannel(){
		if (CheckWinCondition (9)) {
			_endstoryPannel.SetActive (true);
			SetIsBoxColliderOfAllCards (false);

			_storyText.text = GetCompleteTextWithOrder ();
		} else {
			_popUpPannel.SetActive (true);
			StartCoroutine (HidePopUpPannel ());
		}
	}

	/// <summary>
	/// Gets the complete text from all cards by order.
	/// </summary>
	string GetCompleteTextWithOrder(){
		string _tempStoryText = "";

		for (int i = 0; i < _cardFramePos.Length; i++) {
			foreach (GameObject _card in _cardsPlayable) {
				if (_card.transform.position == _cardFramePos [i].position) {
					var _scriptCard = _card.GetComponent<Card> ();


					_tempStoryText += _scriptCard.GetMyStoryText () + " ";
				}
			}
		}

		return _tempStoryText;
	}

	/// <summary>
	/// Hides the pop up pannel.
	/// Popup pannel show when WinGameCondition still false and player try press EndStory button
	/// Hide popup pannel after 2s show
	/// </summary>
	/// <returns>The pop up pannel.</returns>
	IEnumerator HidePopUpPannel(){
		yield return new WaitForSeconds (2f);
		_popUpPannel.SetActive (false);
	}

	public void ResetGameButton(){
		SceneManager.LoadScene (1);
	}

	public void ClickButtonHome(){
		SceneManager.LoadScene (0);
	}

	public void ClickButtonResume(){
		_endstoryPannel.SetActive (false);
		SetIsBoxColliderOfAllCards (true);
	}

	#endregion

	#region check win game

	/// <summary>
	/// Win game condition
	/// Win when all cards in frame positions and have their stories.
	/// </summary> 
	/// <returns><c>true</c>, win game <c>false</c> otherwise.</returns>
	private bool CheckWinCondition(int _windNumber){
		int _win = 0;
		for (int i = 1; i < _cardFramePos.Length; i++) {
			foreach (GameObject _card in _cardsPlayable) {
				var _cardScript = _card.GetComponent<Card> ();
				string _tempText = _cardScript.GetMyStoryText ();
				if (!string.IsNullOrEmpty(_tempText) && _cardScript.GetHolderPos() == _cardFramePos[i].position) {
					_win++;
				}
			}
		}

		if (_win == _windNumber) {
			return true;
		} else {
			return false;
		}

	}

	#endregion
}
