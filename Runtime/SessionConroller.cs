using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TW.Common
{

	[CreateAssetMenu(menuName = "Data/Settings/Session Control")]
	public class SessionControl : ScriptableObject
	{
		public event Action<bool> onPaused;

		[NonSerialized] private int _showCursorCount = 0;
		[NonSerialized] private int _pauseCount = 0;

		public void SetPaused(bool value)
		{

			if (value)
			{
				_pauseCount++;
			}
			else
			{
				_pauseCount--;
			}
			if (_pauseCount < 0)
			{
				_pauseCount = 0;
			}

			Time.timeScale = _pauseCount > 0 ? 0 : 1;

			onPaused?.Invoke(_pauseCount > 0);
		}

		public void Unpause()
		{
			_pauseCount = 0;
			Time.timeScale = 1;
			onPaused?.Invoke(_pauseCount > 0);
		}


		public void ReloadActiveScene()
		{
			SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);
		}

		public void LoadScene(int buildIndex)
		{
			SceneManager.LoadSceneAsync(buildIndex);
		}

		public void QuitGame()
		{
#if UNITY_EDITOR
			UnityEditor.EditorApplication.ExitPlaymode();
#else
		Application.Quit();
#endif
		}

		public void LockCursor()
		{
			FocusOnGame();
			Cursor.lockState = CursorLockMode.Locked;
		}

		public void ConfineCursor()
		{
			FocusOnGame();
			Cursor.lockState = CursorLockMode.Confined;
		}

		public void FreeCursor()
		{
			Cursor.lockState = CursorLockMode.None;
		}

		public void ToggleCursor()
		{
			Cursor.visible = !Cursor.visible;
			if (Cursor.visible)
			{
				Cursor.lockState = CursorLockMode.None;
			}
			else
			{
				LockCursor();
			}
		}

		public void SetCursorVisibility(bool value)
		{
			if (value)
			{
				_showCursorCount++;
			}
			else
			{
				_showCursorCount--;

			}
			if (_showCursorCount < 0)
			{
				_showCursorCount = 0;
			}
			Cursor.visible = _showCursorCount > 0;
		}



		private void FocusOnGame()
		{
#if UNITY_EDITOR
			if (UnityEditor.EditorApplication.isPlaying)
			{
				UnityEditor.EditorApplication.ExecuteMenuItem("Window/General/Game");
			}
#endif
		}

		public bool AudioPause
		{
			get => AudioListener.pause;
			set => AudioListener.pause = value;
		}
	}
}

