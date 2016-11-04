using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;


public class Animate: PrimitiveTask, IGroundAction
{
	GameObject agentObj;
	Animator anim;
	string animationState;

	public Animate (HTNAgent a, string n, string ast){
		
		name = n;
		animationState = ast;
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
					
					anim.CrossFade ("Idle",0.0f);
					
					return;
				}

			//Add custom code
			try {
				anim = agentObj.GetComponent<Animator>();

				if (anim!=null)
					anim.CrossFade("Idle",0.0f);
				
			}
			
			catch (MissingComponentException e){
				Debug.Log(e.Message);
			}


		}
		
	}
	
	
	//Runs at every frame during action's execution
	public void Update(ActionManager manager, State state){

		//Check cuncurrency
		foreach (CuncurrentTask cuncurrent in cuncurrentTasks)
			if (!CheckCuncurrency (cuncurrent,agentObj.GetComponent<HTNAgent>())){
				
				manager.Suspend ();
				
				anim.CrossFade ("Idle",0.0f);
				
				return;
			}
		
		//Monitor preconditions validity in current state: if not valid, notify to manager
		if (preconditions.Count > 0 && !RuleMatcher.MatchCondition (preconditions, state, logicalOperator))

			manager.NotValid ();
		
	

		else {

			//Add custom code

			if (anim!=null && anim.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
				anim.CrossFade(animationState,0.0f);


			if (anim!=null && !anim.GetCurrentAnimatorStateInfo(0).IsName(animationState))
				OnComplete(manager, state);
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

