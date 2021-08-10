using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldTime : MonoBehaviour
{
	[Range(0, 5)]
	public float minToSecond = 1f;

	[SerializeField]
	public int currentMin;
	public int currentTenMin = 00;
	public int currentHour = 12;

	public int minInDay = 1440;

	public int currentTotalWorldMinutes;

	public int dayCount = 1;
	private float timeSinceLastMin;

	private void MinutePassed()
	{
		++currentMin;
		++currentTotalWorldMinutes;
		if (currentMin >= minInDay)
		{
			++dayCount;
			currentMin = 0;
		}

		int min = currentMin % 60;
		int hrs = currentMin / 60;

		if (currentMin % 10 == 0)
		{
			currentTenMin += 10;
			if (currentMin % 60 == 0) currentTenMin = 0;
		};

		if (currentMin % 60 == 0)
		{
			currentHour += 1;
			if (currentMin % 720 == 0) currentHour = 12;
			if (currentHour > 12) currentHour = 1;
		}
	}

	void Update()
	{
		float t = Time.deltaTime;
		if (timeSinceLastMin >= minToSecond)
		{
			timeSinceLastMin = 0f;
			MinutePassed();
		}
		else timeSinceLastMin += t;
	}
}
