using UnityEngine;

namespace TW.Common
{
	public class SingletonBehaviour<T> : MonoBehaviour where T : MonoBehaviour
	{
		protected static T _instance;

		public static T Instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = FindFirstObjectByType<T>();
					if (_instance == null)
					{
						var go = new GameObject(typeof(T).Name);
						_instance = go.AddComponent<T>();
					}
				}
				return _instance;
			}
		}

		public static bool Instantiated()
		{
			return _instance != null;
		}

		protected virtual void Awake()
		{
			if (_instance == null)
			{
				_instance = this as T;
			}
			else
			{
				if (this != _instance)
				{
					Destroy(gameObject);
				}
			}
		}
	}
}

