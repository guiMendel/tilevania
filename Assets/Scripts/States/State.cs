using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Deps
[RequireComponent(typeof(SharedState))]

public abstract class State : MonoBehaviour
{
  // Params
  [Header("State management")]
  [Tooltip("Whether this should be the starting state")]
  [SerializeField] bool startingState;
  [Tooltip("The state key used by this state")]
  public string stateKey;

  // State

  // If it's the currently active state
  protected bool isCurrentState { get; private set; }

  // Refs
  private SharedState _sharedState;

  private void Awake()
  {
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
    isCurrentState = _sharedState.GetState(GetStateKeyName()) == this.GetType().Name;

    OnUpdate();
  }

  // Get key name
  private string GetStateKeyName()
  {
    return String.IsNullOrEmpty(stateKey) ? GetDefaultStateKeyName() : stateKey;
  }

  // Overridable methods

  protected virtual void OnStart() { }
  protected virtual void OnAwake() { }
  protected virtual void OnUpdate() { }
  protected virtual void OnStateEnable() { }

  protected abstract string GetDefaultStateKeyName();

  // Interface

  // Sets this as the current state and starts emitting events
  public void Enable()
  {
    // Set state
    _sharedState.SetState(GetStateKeyName(), this.GetType().Name);
    isCurrentState = true;

    OnStateEnable();
  }
}
