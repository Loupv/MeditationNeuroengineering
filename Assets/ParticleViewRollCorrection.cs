using UnityEngine;
using System.Collections;

[RequireComponent(typeof(ParticleSystem))]
public class ParticleViewRollCorrection : MonoBehaviour
{
	private float lastRotation = 0f;

	private ParticleSystem.Particle[] particles = new ParticleSystem.Particle[1000];

	void LateUpdate()
	{
		if (!Camera.main)
			return;

		float currentRotation = Camera.main.transform.rotation.eulerAngles.z;
		if (lastRotation == currentRotation)
			return;

		float rotationDifference = currentRotation - lastRotation;

		int num = GetComponent<ParticleSystem>().GetParticles(particles);
		for (int i = 0; i < num; i++)
			particles[i].rotation += rotationDifference;

		GetComponent<ParticleSystem>().SetParticles(particles, num);
		//particleSystem.startRotation += rotationDifference * Mathf.Deg2Rad;

		lastRotation = currentRotation;
	}
}