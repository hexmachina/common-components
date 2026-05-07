using UnityEngine;
using UnityEngine.Events;

namespace TW.Common
{
	public class AwakeEvent : MonoBehaviour
	{
		public UnityEvent onAwake;
		// Start is called before the first frame update
		void Awake()
		{
			onAwake.Invoke();
		}

	}
}
