using UnityEngine;
using UnityEngine.Events;

namespace TW.Common
{
	public class TriggerListener : MonoBehaviour
	{
		[SerializeField, TagSelector] private string triggerTag;
		[SerializeField] private LayerMask layerMask;

		[SerializeField] private UnityEvent<Collider> onTriggerEnter = new();
		[SerializeField] private UnityEvent<Collider> onTriggerExit = new();
		private void OnTriggerEnter(Collider other)
		{
			if ((triggerTag == string.Empty || other.CompareTag(triggerTag)) && ColliderUtil.IsLayerInMask(other.gameObject.layer, layerMask))
			{
				onTriggerEnter.Invoke(other);
			}
		}

		private void OnTriggerExit(Collider other)
		{
			if ((triggerTag == string.Empty || other.CompareTag(triggerTag)) && ColliderUtil.IsLayerInMask(other.gameObject.layer, layerMask))
			{
				onTriggerExit.Invoke(other);
			}
		}


	}
}

