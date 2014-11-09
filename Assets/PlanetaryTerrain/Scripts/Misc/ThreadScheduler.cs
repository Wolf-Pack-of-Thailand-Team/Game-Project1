
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading;

namespace Planetary {

public class ThreadScheduler : MonoBehaviour {
	
	private static bool initialized = false;
	
	public static int totalThreads = 8;
	private static int numThreads = 0;
	
	private List<Action> actions = new List<Action>();
	private List<Action> currentActions = new List<Action>();
	
	#region Instance
	
	private static ThreadScheduler instance;
	public static ThreadScheduler Instance {
		get {
			Initialize();
			return instance;
		}
	}
	
	void OnDisable() {
		if(instance == this) {
			instance = null;
		}
	}
	
	#endregion
	
	#region Initialization
	
	void Awake() {
		instance = this;
		initialized = true;
	}
	
	static void Initialize() {
		if(!initialized) {
			if(!Application.isPlaying)
				return;
			
			initialized = true;
			
			GameObject go = new GameObject("ThreadScheduler");
			instance = go.AddComponent<ThreadScheduler>();
		}
			
	}
	
	#endregion
	
	#region Runtime
	
	public static void RunOnMainThread(Action action) {
		lock(Instance.actions) {
			Instance.actions.Add(action);
		}
	}
	
	public static Thread RunOnThread(Action action) {
		Initialize();
		
		while(numThreads >= totalThreads) {
			Thread.Sleep(1);
		}
		
		Interlocked.Increment(ref numThreads);
		
		ThreadPool.QueueUserWorkItem(RunAction, action);
		
		return null;
	}
	
	private static void RunAction(object action) {
		try {
			((Action)action)();
		}
		catch {}
		finally {
			Interlocked.Decrement(ref numThreads);
		}
			
	}
	
	void Update() {
		lock(actions) {
			currentActions.Clear();
			currentActions.AddRange(actions);
			actions.Clear();
		}
		for(int i = 0; i < currentActions.Count; i++)
			currentActions[i]();
	}
	
	#endregion
}

}