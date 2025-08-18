using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Voxel_Engine
{
    public class UnityMainThreadDispatcher : MonoBehaviour {

	    private static UnityMainThreadDispatcher _instance = null;
		private static readonly Queue<Action> ExecutionQueue = new Queue<Action>();

		private const int MaxProcessMilliseconds = 1;

		private Coroutine _processCoroutine;

		public void Update() {
			lock(ExecutionQueue)
			{
				if (_processCoroutine == null && ExecutionQueue.Count > 0)
					_processCoroutine = StartCoroutine(ProcessQueueCoroutine());
			}
		}

		private IEnumerator ProcessQueueCoroutine()
		{
			while (ExecutionQueue.Count > 0)
			{
				var startTime = Time.realtimeSinceStartup;
				while (ExecutionQueue.Count > 0 && (Time.realtimeSinceStartup - startTime) * 1000f < MaxProcessMilliseconds)
				{
					var func = ExecutionQueue.Dequeue();
					//TODO: This is sometimes null. I don't know why but nothing seems to be missing ;(
					func?.Invoke();
				}
				yield return null;
			}
			_processCoroutine = null;
		}

		/// <summary>
		/// Locks the queue and adds the IEnumerator to the queue
		/// </summary>
		/// <param name="action">IEnumerator function that will be executed from the main thread.</param>
		public void Enqueue(IEnumerator action) {
			lock (ExecutionQueue) {
				ExecutionQueue.Enqueue (() => {
					StartCoroutine (action);
				});
			}
		}

		/// <summary>
		/// Locks the queue and adds the Action to the queue
		/// </summary>
		/// <param name="action">function that will be executed from the main thread.</param>
		public void Enqueue(Action action)
		{
			Enqueue(ActionWrapper(action));
		}

		/// <summary>
		/// Locks the queue and adds the Action to the queue, returning a Task which is completed when the action completes
		/// </summary>
		/// <param name="action">function that will be executed from the main thread.</param>
		/// <returns>A Task that can be awaited until the action completes</returns>
		public Task EnqueueAsync(Action action)
		{
			var tcs = new TaskCompletionSource<bool>();

			void WrappedAction() {
				try 
				{
					action();
					tcs.TrySetResult(true);
				} catch (Exception ex) 
				{
					tcs.TrySetException(ex);
				}
			}

			Enqueue(ActionWrapper(WrappedAction));
			return tcs.Task;
		}
		
		public Task<T> EnqueueAsync<T>(Func<T> func)
		{
			var tcs = new TaskCompletionSource<T>();

			void WrappedAction()
			{
				try
				{
					T result = func();
					tcs.TrySetResult(result);
				}
				catch (Exception ex)
				{
					tcs.TrySetException(ex);
				}
			}

			Enqueue(ActionWrapper(WrappedAction));
			return tcs.Task;
		}
		
		IEnumerator ActionWrapper(Action a)
		{
			a();
			yield return null;
		}
		
		public static bool Exists() {
			return _instance != null;
		}

		public static UnityMainThreadDispatcher Instance() {
			if (!Exists ()) {
				throw new Exception ("UnityMainThreadDispatcher could not find the UnityMainThreadDispatcher object. Please ensure you have added the MainThreadExecutor Prefab to your scene.");
			}
			return _instance;
		}


		void Awake() {
			if (_instance == null) {
				_instance = this;
				DontDestroyOnLoad(this.gameObject);
			}
		}

		void OnDestroy() {
				_instance = null;
		}
	}
}