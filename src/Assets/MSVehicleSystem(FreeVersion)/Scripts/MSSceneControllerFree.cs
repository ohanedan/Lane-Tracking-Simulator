﻿using System.Collections;
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

	[Space(10)][Tooltip("If this variable is true, the control for this variable will be activated.")]
	public bool enable_switchingCameras_Input = true;
	[Tooltip("The key that must be pressed to toggle between the cameras of the vehicle.")]
	public KeyCode switchingCameras = KeyCode.C;
}

public class MSSceneControllerFree : MonoBehaviour {
	[Space(10)][Tooltip("Here you can configure the vehicle controls, choose the desired inputs and also, deactivate the unwanted ones.")]
	public ControlsFree controls;
	[Tooltip("All vehicles in the scene containing the 'MS Vehicle Controller' component must be associated with this list.")]
	public GameObject[] vehicles;
	[Space(10)][Tooltip("This variable is responsible for defining the vehicle in which the player will start. It represents an index of the 'vehicles' list, where the number placed here represents the index of the list. The selected index will be the starting vehicle.")]
	public int startingVehicle = 0;
	
	[Space(10)][Tooltip("If this variable is true, useful data will appear on the screen, such as the car's current gear, speed, brakes, among other things.")]
	public bool UIVisualizer = true;
	AutonomousScript autonomousObject;
	//
	Text gearText;
	Text kmhText;
	Text handBrakeText;
	Text pauseText;
	Image backGround;

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
	int clampGear;
	bool pause = false;
	bool error;
	bool enterAndExitBool;
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
			//UI transform.find
			gearText = transform.Find ("Canvas/Strings/gearText").GetComponent<Text> ();
			kmhText = transform.Find ("Canvas/Strings/kmhText").GetComponent<Text> ();
			handBrakeText = transform.Find ("Canvas/Strings/handBrakeText").GetComponent<Text> ();
			pauseText = transform.Find ("Canvas/Strings/pauseText").GetComponent<Text> ();
			backGround = transform.Find ("Canvas/Strings").GetComponent<Image> ();
			//end transform.find

			vehicleCode = vehicles [currentVehicle].GetComponent<MSVehicleControllerFree> ();
			
			Time.timeScale = 1;
			enterAndExitBool = false;
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
			//
			EnableUI (UIVisualizer);
			//
			if (vehicles.Length > 0 && currentVehicle < vehicles.Length && UIVisualizer && vehicleCode) {
				if (vehicleCode.isInsideTheCar) {
					clampGear = Mathf.Clamp (vehicleCode.currentGear, -1, 1);
					if (clampGear == 0) {
						clampGear = 1;
					}

					gearText.text = "Gear: " + vehicleCode.currentGear;
					kmhText.text = "Velocity(km/h): " + (int)(vehicleCode.KMh * clampGear);
					handBrakeText.text = "HandBreak: " + vehicleCode.handBrakeTrue;
					pauseText.text = "Pause: " + pause;
				}
			}
		}
	}

	void EnableUI(bool enable){
		if (gearText.gameObject.activeSelf != enable) {
			gearText.gameObject.SetActive (enable);
			kmhText.gameObject.SetActive (enable);
			handBrakeText.gameObject.SetActive (enable);
			pauseText.gameObject.SetActive (enable);
			backGround.gameObject.SetActive (enable);
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

	void Mobile_CameraInput(){
		if (!error) {
			if (vehicleCode.isInsideTheCar) {
				vehicleCode.InputsCamerasMobile ();
			}
		}
	}

	void Mobile_EnterAndExitVehicle(){
		if (!error && !enterAndExitBool) {
			enterAndExitBool = true;
		}
	}
}
