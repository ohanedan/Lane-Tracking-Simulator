using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Linq;
using System;

[Serializable]
public class ControlsFree {
	[HideInInspector]
	public bool handBrakeAutonomous = false;
	public KeyCode switchingCameras = KeyCode.C;
}

public class MSSceneControllerFree : MonoBehaviour {
	public ControlsFree controls;
	public GameObject[] vehicles;
	public int startingVehicle = 0;

	AutonomousScript autonomousObject;

	#region customizeInputs
	[HideInInspector]
	public float verticalInput = 0;
	[HideInInspector]
	public float horizontalInput = 0;
	[HideInInspector]
	public float mouseXInput = 0;
	[HideInInspector]
	public float mouseYInput = 0;
	[HideInInspector]
	public float mouseScrollWheelInput = 0;
	#endregion

	int currentVehicle = 0;
	bool pause = false;
	bool error;
	string sceneName;

	MSVehicleControllerFree vehicleCode;
	void Awake () {
		error = false;
		CheckEqualKeyCodes ();
		autonomousObject = GameObject.Find("/Autonomous").GetComponent<AutonomousScript>();
		MSSceneControllerFree[] sceneControllers = FindObjectsOfType(typeof(MSSceneControllerFree)) as MSSceneControllerFree[];
		if (sceneControllers.Length > 1) {
			Debug.LogError ("Only one controller is allowed per scene, otherwise the controllers would conflict with each other.");
			error = true;
			for (int x = 0; x < sceneControllers.Length; x++) {
				sceneControllers [x].gameObject.SetActive (false);
			}
		}
		if (startingVehicle >= vehicles.Length) {
			error = true;
			Debug.LogError ("Vehicle selected to start does not exist in the 'vehicles' list");
		}
		if (vehicles.Length == 0) {
			error = true;
			Debug.LogError ("There is no vehicle in the scene or no vehicle has been associated with the controller.");
		}
		for (int x = 0; x < vehicles.Length; x++) {
			if (vehicles [x]) {
				if (!vehicles [x].GetComponent<MSVehicleControllerFree> ()) {
					error = true;
					Debug.LogError ("The vehicle associated with the index " + x + " does not have the 'MSVehicleController' component. So it will be disabled.");
				}
			}else{
				error = true;
				Debug.LogError ("No vehicle was associated with the index " + x + " of the vehicle list.");
			}
		}
		if (error) {
			for (int x = 0; x < vehicles.Length; x++) {
				if (vehicles [x]) {
					MSVehicleControllerFree component = vehicles [x].GetComponent<MSVehicleControllerFree> ();
					if (component) {
						component.disableVehicle = true;
					}
					vehicles [x].SetActive (false);
				}
			}
			return;
		}
		else {
			vehicleCode = vehicles [currentVehicle].GetComponent<MSVehicleControllerFree> ();
			
			Time.timeScale = 1;
			sceneName = SceneManager.GetActiveScene ().name;
			currentVehicle = startingVehicle;
			//
			for (int x = 0; x < vehicles.Length; x++) {
				if (vehicles [x]) {
					vehicles [x].GetComponent<MSVehicleControllerFree> ().isInsideTheCar = false;
				}
			}
			if (vehicles.Length > startingVehicle && vehicles [currentVehicle]) {
				vehicles [startingVehicle].GetComponent<MSVehicleControllerFree> ().isInsideTheCar = true;
			}
		}
	}

	void CheckEqualKeyCodes(){
		var type = typeof(ControlsFree);
		var fields = type.GetFields();
		var values = (from field in fields
			where field.FieldType == typeof(KeyCode)
			select (KeyCode)field.GetValue(controls)).ToArray();

		foreach (var value in values) {
			if (Array.FindAll (values, (a) => {
				return a == value;
			}).Length > 1) {
				Debug.LogError ("There are similar commands in the 'controls' list. Use different keys for each command.");
				error = true;
			}
		}
	}

	void Update () {
		if (!error) {
			#region customizeInputsValues
			verticalInput = autonomousObject.verticalVal/255.0f;
			horizontalInput = autonomousObject.horizontalVal/127.0f;
			#endregion

			vehicleCode = vehicles [currentVehicle].GetComponent<MSVehicleControllerFree> ();

			if (autonomousObject.reset) {
				SceneManager.LoadScene (sceneName);
				autonomousObject.reset = false;
			}
			controls.handBrakeAutonomous = autonomousObject.handbrake;
			pause = autonomousObject.pause;
			
			if (pause) {
				Time.timeScale = Mathf.Lerp (Time.timeScale, 0.0f, Time.fixedDeltaTime * 5.0f);
			} else {
				Time.timeScale = Mathf.Lerp (Time.timeScale, 1.0f, Time.fixedDeltaTime * 5.0f);
			}
		}
	}

	void EnableVehicle(int index){
		currentVehicle = Mathf.Clamp (currentVehicle, 0, vehicles.Length-1);
		if (index != currentVehicle) {
			if (vehicles [currentVehicle]) {
				//change vehicle
				for (int x = 0; x < vehicles.Length; x++) {
					vehicles [x].GetComponent<MSVehicleControllerFree> ().ExitTheVehicle ();
				}
				vehicles [currentVehicle].GetComponent<MSVehicleControllerFree> ().EnterInVehicle ();
				vehicleCode = vehicles [currentVehicle].GetComponent<MSVehicleControllerFree> ();
			}
		}
	}
}
