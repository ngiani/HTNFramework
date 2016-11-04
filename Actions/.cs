using UnityEngine;
								using System;
								using System.Collections;


								public class : PrimitiveTask, IGroundAction
								{

									public (string n){

										_name = n;
									}


									public string Name{

										get{return _name;}
										set {_name = value;}
									}

									//This starts agent's action execution
									public void Start(ActionManager manager, State state){
										
										//this is manager's current executing action
										manager.currentAction = this;

										//Monitor precondition validity in current state: if not valid, notify to manager
										if (RuleMatcher.MatchCondition (preconditions, state).Count == 0)
											manager.NotValid ();

										// Get agent
										GameObject agentObj = (GameObject)arguments [0].value;

										if (agentObj.GetComponent <CharacterController>() == null)
											agentObj.AddComponent<CharacterController>();

										//Add custom code
									
									}


									//Runs at every frame during action's execution
									public void Update(ActionManager manager, State state){
										
										//Get agent
										GameObject agentObj = (GameObject)arguments [0].value;

										//Add custom code

										//Monitor preconditions validity in current state: if not valid, notify to manager
										if (RuleMatcher.MatchCondition (preconditions, state).Count == 0 && Time.frameCount % 60 == 0)
											manager.NotValid ();

									}

									public void OnComplete(ActionManager manager){

										manager.NextAction ();
									}

								}