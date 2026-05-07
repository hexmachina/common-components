using UnityEngine;
using UnityEngine.Events;

namespace TW.Common
{
	public class EnableEvent : MonoBehaviour
	{
		public UnityEvent onEnabled = new();

		public UnityEvent onDisabled = new();

		public UnityEvent<bool> onEnableChanged = new();
		public UnityEvent<bool> onEnableChangedInverted = new();

		private void OnEnable()
		{
			onEnabled.Invoke();
			onEnableChanged.Invoke(true);
			onEnableChangedInverted.Invoke(false);
		}

		private void OnDisable()
		{
			onDisabled.Invoke();
			onEnableChanged.Invoke(false);
			onEnableChangedInverted.Invoke(true);
		}

		public void ToggleEnabled()
		{
			enabled = !enabled;
		}
	}
}

