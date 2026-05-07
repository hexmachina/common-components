using System.Collections.Generic;
using UnityEngine;

namespace TW.Common
{
	public class RenderLayerModifier : MonoBehaviour
	{
		[SerializeField] private RenderingLayerMask _renderingLayerMask;
		[SerializeField] private List<Renderer> _renderers;

		public void ApplyRenderLayer(bool value)
		{
			foreach (var renderer in _renderers)
			{
				if (value)
				{
					renderer.renderingLayerMask |= _renderingLayerMask.value;
				}
				else
				{
					renderer.renderingLayerMask &= ~_renderingLayerMask.value;
				}
			}
		}
	}

}
