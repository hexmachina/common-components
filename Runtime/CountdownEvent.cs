using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace TW.Common
{
	public class CountdownEvent : MonoBehaviour
	{
		public UnityEvent onCountdownCompleted = new();

		private Coroutine _coroutine;

		private float _currentTime;

		public void StartCountdown(float duration)
		{
			_coroutine ??= StartCoroutine(CountingDown(duration));
		}

		private IEnumerator CountingDown(float duration)
		{
			_currentTime = duration;
			while (_currentTime > 0f)
			{
				_currentTime -= Time.deltaTime;
				yield return null;
			}
			onCountdownCompleted.Invoke();
			_coroutine = null;
		}
	}
}

