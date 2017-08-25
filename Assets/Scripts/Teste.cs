using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Teste : MonoBehaviour
{
	public Text acionar;

	private bool acionarLiberado = true;

	private float accelerometerUpdateInterval = 1.0f / 60.0f;

	private float lowPassKernelWidthInSeconds = 1.0f;

	public float shakeDetectionThreshold;

	private float lowPassFilterFactor;
	private Vector3 lowPassValue;

	private void Start()
	{
		lowPassFilterFactor = accelerometerUpdateInterval / lowPassKernelWidthInSeconds;
		lowPassValue = Input.acceleration;

		InvokeRepeating("TocarVideo", 1f, 100f);
	}

	private void TocarVideo()
	{
		Handheld.PlayFullScreenMovie(
			"Teste.mp4",
			Color.black,
			FullScreenMovieControlMode.Hidden,
			FullScreenMovieScalingMode.AspectFill
		);
	}

	private void Update()
	{
		Vector3 acceleration = Input.acceleration;
		lowPassValue = Vector3.Lerp(lowPassValue, acceleration, lowPassFilterFactor);
		Vector3 deltaAcceleration = acceleration - lowPassValue;

		if (acionarLiberado && deltaAcceleration.sqrMagnitude >= shakeDetectionThreshold)
		{
			Acionar();
		}
	}

	private void Acionar()
	{
		acionarLiberado = false;

		acionar.text = "Acionar: Sim";

		Handheld.Vibrate();

		Invoke("LiberarAcionar", 1f);
	}

	private void LiberarAcionar()
	{
		acionarLiberado = true;

		acionar.text = "Acionar: Não";
	}
}
