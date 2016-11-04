using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

//public delegate void ExecutionRequest(ActionManager manager);

public class Move : PrimitiveTask, IGroundAction
{
	float arrivalRay;
	float speed;
	bool moving;
	HTNAgent agent;
	GameObject agentObj;
	GameObject target;
	string animationState;
	Animator anim;

	public Move(HTNAgent a, string n, float r, float s, string ast){
		
		name = n;
		arrivalRay = r;
		speed = s;
		moving = false;
		animationState = ast;
		agent = a;
		agentObj = a.gameObject;



	}


	public Move(HTNAgent a, string n, float r, float s){

		name = n;
		arrivalRay = r;
		speed = s;
		moving = false;
		animationState = "";
		agentObj = a.gameObject;



	}


	public Move(HTNAgent a, string n){
		
		name = n;
		arrivalRay = 0.01f;
		speed = 1.5f;
		moving = false;
		animationState = "";
		agentObj = a.gameObject;



	}
		



	//This starts agent's action execution
	public void Start(ActionManager manager, State state){



		//Monitor precondition validity in current state: if not valid, notify to manager
		if (preconditions.Count > 0 && !RuleMatcher.MatchCondition (preconditions, state,logicalOperator))
			manager.NotValid ();

		else {


			//Check cuncurrency
			foreach (CuncurrentTask cuncurrent in cuncurrentTasks)
				if (!CheckCuncurrency (cuncurrent,agentObj.GetComponent<HTNAgent>())){
					
					manager.Suspend ();
					
					return;
				}

			if (agentObj.GetComponent <CharacterController> () == null)
				agentObj.AddComponent<CharacterController> ();

			target = (GameObject)arguments [0].value;

			moving = true;


			try {
				anim = agentObj.GetComponent<Animator>();

				if (anim!=null)
					anim.CrossFade(animationState,0.0f);
				
			}
			
			catch (MissingComponentException e){
				Debug.Log(e.Message);
			}


		}
	
	}

	void MoveTowardsTarget(CharacterController controller, Vector3 target){
		
		var offset = controller.transform.position - new Vector3(target.x,agentObj.transform.position.y,target.z);
		
		if (offset.magnitude > arrivalRay) {
			
			
			offset = offset.normalized * speed;
			
			controller.transform.LookAt(new Vector3(target.x,agentObj.transform.position.y,target.z));
			
			controller.Move (-offset * Time.deltaTime);
		} else {
			moving = false;
		}
	}

	//Runs at every frame during action's execution
	public void Update(ActionManager manager, State state){

		//Check cuncurrency
		foreach (CuncurrentTask cuncurrent in cuncurrentTasks)
			if (!CheckCuncurrency (cuncurrent,agentObj.GetComponent<HTNAgent>())){
				
				manager.Suspend ();

				
				return;
			}
	
		//Monitor preconditions validity in current state: if not valid, notify to manager
		if (preconditions.Count > 0 && !RuleMatcher.MatchCondition (preconditions, state, logicalOperator)) {
				manager.NotValid ();
		

		}
			

		else {


			if (anim!=null && anim.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
				anim.CrossFade(animationState,0.0f);

			CharacterController controller = agentObj.GetComponent <CharacterController> ();

			MoveTowardsTarget (controller, target.transform.position);

			//Monitor action completeness
			if (!moving)
				OnComplete (manager, state);
		}




	}

	public void OnComplete(ActionManager manager, State state){

		if (anim!=null)
			anim.CrossFade ("Idle",0.0f);

		foreach (Atom atom in effects){
			
			Atom fact = new Atom(atom.name,atom.sign);
			
			foreach (Term var in atom.terms)
				fact.addTerm(new Term(var.value));
			

			Atom existingFact = state.Contains(fact);

			if (state.Contains(fact) == null)
				state.addFact(fact);
			else 
				existingFact.sign = fact.sign;
		}

		manager.NextAction ();

		completed = true;
	}



}



