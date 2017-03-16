/* License: free to use
 * JoystickController.cs
 * A virtual joystick for Xbox-One controller 
 * Works with left analogue joystick of Xbox-One and Xbox-One TriggerButtons.
 * Also user can select ArrowKeys (Left, Right) for testing without controller.
 * Select the states from Inspector (Script settings).
 * Angle mapping on UI element is in Z Axis.
 * 
 * Developer: Asema Hassan
 * (In case it was helpful please like the post on my blog)
 * http://asemahassan.blogspot.de
 * Dated: 01-03-17
 */

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class JoystickController : MonoBehaviour
{
	public JSControllerState _controllerState = JSControllerState.None;
	/// <summary>
	/// The user response active can be toggled from any script
	/// To control the input from user
	/// </summary>
	public static bool _userResponseActive = false;
	/// <summary>
	/// The user response angle stores the final angle 
	/// that user has pointed at using joystick
	/// </summary>
	public static float _userResponseAngle = 0f;

	private static RectTransform knobTransform = null;
	[SerializeField]
	private GameObject pivotHandle = null;
	[SerializeField]
	private Text angleValueText = null;
	[SerializeField]
	private float stepAmount = 1.0f;
	private static Vector3 rotationEuler;

	//activity messages
	[SerializeField]
	private Text activityMessage = null;

	// Use this for initialization
	void Start ()
	{
		if (pivotHandle != null) {
			knobTransform = pivotHandle.GetComponent<RectTransform> ();
		}
		//For testing as its only one class you can set it here to true
		_userResponseActive = true;
			
	}

	// Update is called once per frame
	void Update ()
	{
		#region IN_ORDER_TO_TEST_THIS_SCENE_ONLY
		if (Input.GetKeyUp (KeyCode.R)) {
			if (activityMessage != null) {
				activityMessage.text = "R key pressed to reset angle";
			}
			ResetJoyStickAngle ();
		}

		if (Input.GetKeyUp (KeyCode.X)) {
			if (activityMessage != null) {
				activityMessage.text = "X key pressed to start over again";
			}
			if (!_userResponseActive) {
				_userResponseActive = true;
			}
		}
		#endregion

		if (!_userResponseActive)
			return;

		switch (_controllerState) {
		case JSControllerState.None:
			{
				Debug.LogError ("Please select a state from JSControllerState");
				#if UNITY_EDITOR
				UnityEditor.EditorApplication.isPlaying = false;
				#endif
				break;
			}
		case JSControllerState.Arrow_Keys:
			{
				UseArrowKeys ();
				LockPlayerResponse ();
				break;
			}
		case JSControllerState.XboxOne_AnalogThumbstick:
			{
				UseAnalogStick ();
				LockPlayerResponse ();
				break;
			}

		case JSControllerState.XboxOne_TriggerButtons:
			{
				UseTriggerButtons ();
				LockPlayerResponse ();
				break;
			}
		default:
			break;
		}
	}

	/// <summary>
	/// Uses the arrow keys.
	/// When testing in Editor without XboxOne controller can use this method.
	/// </summary>
	private void UseArrowKeys ()
	{
		if (Input.GetKey (KeyCode.LeftArrow)) {
			rotationEuler += Vector3.forward * stepAmount;
		}

		if (Input.GetKey (KeyCode.RightArrow)) {
			rotationEuler -= Vector3.forward * stepAmount;
		}

		knobTransform.transform.rotation = Quaternion.Euler (rotationEuler);
		float angle = 360 - knobTransform.rotation.eulerAngles.z;
		if (angleValueText != null) {
			angleValueText.text = angle.ToString ();
		}
		_userResponseAngle = angle;

	}

	/// <summary>
	/// Uses the trigger buttons to set angle in Z.
	/// Virtual joystick will be updated accordingly
	/// </summary>
	private void UseTriggerButtons ()
	{
		if (Input.GetAxis ("Oculus_GearVR_LIndexTrigger") > 0) {
			rotationEuler += Vector3.forward * stepAmount;
		}
			
		if (Input.GetAxis ("Oculus_GearVR_RIndexTrigger") > 0) {
			rotationEuler -= Vector3.forward * stepAmount;
		}

		knobTransform.transform.rotation = Quaternion.Euler (rotationEuler);
		float angle = 360 - knobTransform.rotation.eulerAngles.z;

		if (angleValueText != null) {
			angleValueText.text = angle.ToString ();
		}
		_userResponseAngle = angle;
			
	}

	/// <summary>
	/// Uses the left analoque stick to get the user response angle 
	/// Mostly effective when asking user for directional task
	/// </summary>
	private void UseAnalogStick ()
	{
		/*Input Settings:
         * Oculus_GearVR_LThumbstickX (X axis )&& Oculus_GearVR_LThumbstickY (Y axis)
         *  Gravity 0, Dead 0.5, Sensitivity 1
         * Snap/Invert all should be false
         */

		float x = Input.GetAxis ("Oculus_GearVR_LThumbstickX");
		float y = Input.GetAxis ("Oculus_GearVR_LThumbstickY");

		if (x != 0.0f || y != 0.0f) {
			float angle = Mathf.Atan2 (x, y) * Mathf.Rad2Deg;
			angle = (180.0f - angle);
			if (angleValueText != null) {
				angleValueText.text = angle.ToString ();
			}

			_userResponseAngle = angle;
			// multiplying  angle with -1 to show the correct orientation of knob on virtual joystick
			knobTransform.transform.rotation = Quaternion.Euler (new Vector3 (0, 0, -1 * angle));
		}
	}

	/// <summary>
	/// Locks the player response.
	/// If you want to get user response again, set _userResponseActive=true
	/// Like shown in Update testing region.
	/// </summary>
	private void LockPlayerResponse ()
	{
		#if UNITY_EDITOR
		if (Input.GetKeyUp (KeyCode.L)) {
			Debug.Log ("User response angle locked: " + _userResponseAngle);
			if (activityMessage != null) {
				activityMessage.text = "L key pressed to lock angle \n" +
				"Press X key to start over again";
			}
			_userResponseActive = false;
			ResetJoyStickAngle ();
		}
		#else   
		if(Input.GetButton("Oculus_GearVR_AButton"))
		{
		Debug.Log ("User response angle locked: " + _userResponseAngle);
		if (activityMessage != null) {
		activityMessage.text = "L key pressed to lock angle \n" +
		"Press X key to start over again";
		}
		_userResponseActive = false;
		ResetJoyStickAngle();
		}    
		#endif
	}

	/// <summary>
	/// Resets the joy stick angle
	/// Resets the _knobTransform eulerAngles back to zero
	/// </summary>
	public static void ResetJoyStickAngle ()
	{
		rotationEuler = Vector3.zero;
		Quaternion tempQuat = knobTransform.transform.rotation;
		tempQuat.eulerAngles = Vector3.zero;
		knobTransform.transform.rotation = tempQuat;
	}
}
