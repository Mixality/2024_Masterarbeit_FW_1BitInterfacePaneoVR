using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WatchTime : MonoBehaviour {
	// Use this for initialization
	void Start () {
		InvokeRepeating("LaunchTime", 0f, 1.0f);
	}

	
	void LaunchTime () {
		System.DateTime theTime = System.DateTime.Now;
		GetComponent<TMPro.TextMeshPro> ().text = (theTime.ToString("HH:mm"));
	}
}
