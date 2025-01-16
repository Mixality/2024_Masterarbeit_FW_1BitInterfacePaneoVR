using UnityEngine;
using System;

public class Hours_Mechanic : MonoBehaviour {

    private const float
        hoursrot = 360f / 12f,
        minutesrot = 360f / 60f,
        secondsrot = 360f / 60f;

    public Transform hours, minutes, seconds;

    void Update () {
        DateTime time = DateTime.Now;
        hours.localRotation = Quaternion.Euler(-90.0f, 0f, time.Hour * +hoursrot + 10f);
        minutes.localRotation = Quaternion.Euler(-90.0f, 0f, time.Minute * +minutesrot);
        seconds.localRotation = Quaternion.Euler(-90.0f, 0f, time.Second * +secondsrot);
    }
}