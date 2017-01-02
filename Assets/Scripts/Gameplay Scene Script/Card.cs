using UnityEngine;
using System.Collections;

public class Card : MonoBehaviour {

	private Transform _boardPos, _targetCardTrans;
	private GameObject _otherCard;
	private SpriteRenderer _myRender;
	private float _speed;
	private Vector3 _holderPos;

	[SerializeField] private AudioClip _cardAtPositionClip;
	[SerializeField] private GameObject _effectParticle;
	private AudioSource _audioSource;
	private bool _isPlayClockSound;

	//Use for mouse drag and drop
	private Vector3 _mousePos;
	private Vector3 _clickPos;
	private Vector3 _offSet;

	[SerializeField] private bool _isDrag;//Only check Collider if _isDrag true
	[SerializeField] private bool _isPlace;//Check if frame empty
	[SerializeField] private bool _isSwap;//Check if can switch card in frame

	//for double click
	private float _delayClick;
	private int _clickCount;

	private string _myStoryText;

	void Awake () {
		_myRender = GetComponent<SpriteRenderer> ();
		_audioSource = GetComponent<AudioSource> ();
		_speed = Random.Range (20, 23);
	}

	void Start () {
		GameplayController.instance.CreateListCardsPlayable (gameObject);
		_holderPos = _boardPos.position;
		StartCoroutine (Move ());
	}

	void Update(){
		PlayClockSound ();
	}

	/// <summary>
	/// Play sound when card have put in holder position
	/// Awalays check in Update function
	/// </summary>
	void PlayClockSound(){
		if (transform.position == _holderPos) {
			if (_isPlayClockSound) {
				Instantiate (_effectParticle, transform.position, Quaternion.identity);
				_audioSource.PlayOneShot (_cardAtPositionClip);
				_isPlayClockSound = false;
			}
		} else {
			_isPlayClockSound = true;
		}
	}

	/// <summary>
	/// Move card at start game
	/// Self-Destroy this action when card stay at board position
	/// </summary>
	IEnumerator Move(){
		Vector3 _tempPos = transform.position;
		Vector3 _tempTargetPos = _boardPos.position;

		if (_tempPos != _tempTargetPos) {
			transform.position = Vector3.MoveTowards (_tempPos, _tempTargetPos, _speed * Time.deltaTime);
		} else {
			yield break;
		}

		yield return new WaitForSeconds (0.01f);
		StartCoroutine (Move ());
	}

	#region drag and drop

	void OnMouseDown(){
		_clickPos = Camera.main.ScreenToWorldPoint (Input.mousePosition);
		_offSet = transform.position - _clickPos;
		_myRender.sortingOrder = 4;
		_isDrag = true;


		GameplayController.instance.SetBoxCardHolder (_holderPos, true);
		GameplayController.instance.GetCardTransformForSaveStoryText (transform);

		//Check double click if card is in frame
		if (_isPlace) {
			_clickCount++;
			if (Time.time < _delayClick + 0.2f) {
				if (_clickCount >= 2) {
					GameplayController.instance.ShowInputFields ();
					_clickCount = 0;
				}
			} else {
				_delayClick = Time.time;
			}
		}
			
	}

	void OnMouseDrag(){
		_mousePos = Camera.main.ScreenToWorldPoint (Input.mousePosition);
		transform.position = new Vector3 (_mousePos.x, _mousePos.y, 0) + _offSet;
	}

	void OnMouseUp(){
		_isDrag = false;
		_myRender.sortingOrder = 1;

		if (_holderPos == Vector3.zero) {
			_holderPos = _boardPos.position;
		}

		GameplayController.instance.DoSwapPos (transform, _targetCardTrans);
		GameplayController.instance.SetIsMoveBack (true);
		GameplayController.instance.SetBoxCardHolder (_holderPos, false);
	}

	#endregion

	#region Collider

	void OnTriggerStay2D(Collider2D target){
		if (_isDrag) {
			if (target.tag == "HolderPos") {
				if (_isSwap) {
					_isPlace = false;
				} else {
					_isPlace = true;
					_targetCardTrans = target.transform;
				}


			}

			if (target.tag == "Card") {
				var _targetCard = target.GetComponent<Card> ();
				_targetCardTrans = target.transform;
				if (_targetCard.GetIsPlace()) {
					_targetCard.SetIsPlace (false);
					_isSwap = true;
				}
			}

		}
	}

	void OnTriggerExit2D(Collider2D target){
		if (_isDrag) {
			if (target.tag == "HolderPos") {
				_isPlace = false;
			}

			if (target.tag == "Card") {
				var _targetCard = target.GetComponent<Card> ();
				if (_isSwap) {
					_targetCard.SetIsPlace (true);
				}
				_isSwap = false;
			}


		}
	}

	#endregion


	#region getter setter
	public void BoardPos(Transform target){
		this._boardPos = target;
	}

	public Vector3 GetBoardPos(){
		return this._boardPos.position;
	}

	public Vector3 GetHolderPos(){
		return this._holderPos;
	}

	public void SetHolderPos(Vector3 _myHolderPos){
		_holderPos = _myHolderPos;
	}

	public void SetIsSwap(bool value){
		this._isSwap = value;
	}

	public bool GetIsSwap(){
		return this._isSwap;
	}

	public void SetIsPlace(bool value){
		this._isPlace = value;
	}

	public bool GetIsPlace(){
		return this._isPlace;
	}

	public bool GetIsDrag(){
		return this._isDrag;
	}

	public void SetMyStoryText(string _value){
		this._myStoryText = _value;
	}

	public string GetMyStoryText(){
		return this._myStoryText;
	}
	#endregion
}
