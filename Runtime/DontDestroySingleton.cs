namespace TW.Common
{
	public class DontDestroySingleton : SingletonBehaviour<DontDestroySingleton>
	{
		protected override void Awake()
		{
			base.Awake();
			DontDestroyOnLoad(_instance);
		}
	}

}