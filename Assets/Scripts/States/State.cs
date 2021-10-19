using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// Deps
[RequireComponent(typeof(SharedState))]

public abstract class State : MonoBehaviour
{
  //=== Params
  [Header("State management")]
  [Tooltip("Whether this should be the starting state")]
  [SerializeField] bool startingState;

  [Tooltip("The state key used by this state")]
  public string stateKey;

  //=== State

  // If it's the currently active state
  protected bool isCurrentState { get; private set; }

  // If it was the active state last frame
  protected bool wasCurrentState { get; private set; }

  //=== Events
  public UnityEvent OnStateEnabled;
  public UnityEvent OnStateDisabled;

  //=== Refs
  private SharedState _sharedState;

  private void Awake()
  {
    // Events
    if (OnStateEnabled == null) OnStateEnabled = new UnityEvent();
    if (OnStateDisabled == null) OnStateDisabled = new UnityEvent();

    // Get refs
    _sharedState = GetComponent<SharedState>();

    OnAwake();
  }

  private void Start()
  {
    // Initialize if starting state
    if (startingState) Enable();

    OnStart();
  }

  private void Update()
  {
    // Keep state awareness updated
    wasCurrentState = isCurrentState;
    isCurrentState = _sharedState.GetState(GetStateKeyName()) == this.GetType().Name;

    // Check if state was disabled this frame
    if (!isCurrentState && wasCurrentState)
    {
      OnStateDisable();
      OnStateDisabled.Invoke();
    }

    OnUpdate();
  }

  // Overridable methods

  protected virtual void OnStart() { }
  protected virtual void OnAwake() { }
  protected virtual void OnUpdate() { }
  protected virtual void OnStateEnable() { }
  protected virtual void OnStateDisable() { }

  protected abstract string GetDefaultStateKeyName();

  //=== Interface

  // Sets this as the current state and starts emitting events
  public void Enable()
  {
    // Ignore redundant calls
    if (isCurrentState) return;
    
    // Set state
    _sharedState.SetState(GetStateKeyName(), this.GetType().Name);
    isCurrentState = true;

    OnStateEnable();
    OnStateEnabled.Invoke();
  }

  // Get key name
  public string GetStateKeyName()
  {
    return String.IsNullOrEmpty(stateKey) ? GetDefaultStateKeyName() : stateKey;
  }

}
