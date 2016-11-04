using UnityEngine;
using System;
using System.Reflection;
using System.Linq;
using System.Collections;
using System.Collections.Generic;





//CLASS FOR MATCHING
class RuleMatcher{

	//Match the rule condition against state, and return bindings
	public static bool MatchCondition(List<Atom> ruleConditions, State state, LogicalOperator op){


		if (op == LogicalOperator.AND) {
			foreach (Atom condition in ruleConditions)
				if (!condition.isVerified (state))
					return false;
		}

		else{
			foreach (Atom condition in ruleConditions)
				if (condition.isVerified(state))
					return true;
		}

		return true;
	}



}

//CLASS FOR PLANNING
public class Planner 
{



	//Instantiate action from action schema
	static PrimitiveTask InstantiateAction(PrimitiveTask schema, State state, HTNAgent agent){
		

			//If conditions found matches, instantiate action from schema, with a variable binding
			if (RuleMatcher.MatchCondition (schema.preconditions, state,schema.logicalOperator)){

					PrimitiveTask groundAction = new PrimitiveTask (schema.name,schema.parent);

					if (schema.actionType == ActionTypes.MOVEMENT) {
			
						if (schema.groundData.animationState != "")
							groundAction = new Move (agent, schema.name, schema.groundData.radius, schema.groundData.speed, schema.groundData.animationState);
						else 
							groundAction = new Move (agent, schema.name, schema.groundData.radius, schema.groundData.speed);
						
					} else if (schema.actionType == ActionTypes.ANIMATION) {
					
						groundAction = new Animate (agent, schema.name, schema.groundData.animationState);
					
					} else if (schema.actionType == ActionTypes.CUSTOM) {
					
						Type customType = Type.GetType (schema.groundData.name);
						ConstructorInfo customConstructor = customType.GetConstructor (new Type[2]{typeof(HTNAgent),typeof(string)});
						groundAction = (PrimitiveTask)customConstructor.Invoke (new object[]{agent,schema.name});

					}
					
					groundAction.parent = schema.parent;

					groundAction.actionType = schema.actionType;

					groundAction.cost = schema.cost;

					groundAction.logicalOperator = schema.logicalOperator;

					groundAction.arguments = schema.arguments;

					groundAction.preconditions = schema.preconditions;

					groundAction.cuncurrentTasks = schema.cuncurrentTasks;

					groundAction.effects = schema.effects;



					return groundAction;
	

				}

		return null;
	}



	//SHOP

	static List<PrimitiveTask> SHOP(List<Task> tasks, List<PrimitiveTask> plan, string personality, State state, HTNAgent agent){

		Task task = tasks [0];

		if (task.GetType () == typeof(PrimitiveTask)) {

			PrimitiveTask pt = (PrimitiveTask)task;

			PrimitiveTask groundAction = InstantiateAction (pt, state, agent);

			if (groundAction != null) {

				plan.Add (groundAction);

				tasks.Remove (pt);

				if (tasks.Count > 0)
					return SHOP (tasks, plan, personality, pt.apply (state), agent);
			} else
				return null;
		} else if (task.GetType () == typeof(CompoundTask)){

			CompoundTask ct = (CompoundTask)task;


			List<Method> orderdedMethods = new List<Method>();

			switch (agent.methodsOrdering) {

				case MethodsOrdering.NONE:
					orderdedMethods = ct.methods;
					break;
				case MethodsOrdering.PREF:
					orderdedMethods = ct.methods.OrderBy (m => m.preference != personality).ToList ();
					break;

				case MethodsOrdering.COSTANDPREF:
					orderdedMethods = ct.methods.OrderBy (m => m.leastCost ()).ThenBy (m => m.preference != personality).ToList ();
					break;
								
			}


			int countFail = 0;

			foreach(Method m in orderdedMethods){

				if (RuleMatcher.MatchCondition(m.preconditions,state,m.logicalOperator)){

					List<Task> tempTasks = new List<Task>(tasks);

					tempTasks.Remove(task);

					int j=0;

					foreach (Task subtask in m.subtasks){
						tempTasks.Insert(0 + j ,subtask);
						j++;
					}
					List<PrimitiveTask> tempPlan = SHOP(tempTasks,plan,personality,state,agent);


					if (tempPlan == null){
						countFail++;
					}

					else {

						plan.Concat(tempPlan);
						break;
					}

				}

				else 
					countFail ++;
			}

			if (countFail == orderdedMethods.Count)
				return null;
		}

		return plan;
	}

	static List<PrimitiveTask> SHOPNoBack(List<Task> tasks, List<PrimitiveTask> plan, string personality, State state, HTNAgent agent){



		Task task = tasks [0];

		if (task.GetType () == typeof(PrimitiveTask)) {


			PrimitiveTask pt = (PrimitiveTask)task;


			PrimitiveTask groundAction = InstantiateAction (pt, state, agent);

			if (groundAction != null) {

				plan.Add (groundAction);

				tasks.Remove (pt);

				if (tasks.Count > 0)
					return SHOPNoBack (tasks, plan, personality, pt.apply (state), agent);
			} else
				return null;
		} else if (task.GetType () == typeof(CompoundTask)){

			CompoundTask ct = (CompoundTask)task;


			List<Method> orderdedMethods = new List<Method>();

			switch (agent.methodsOrdering) {

			case MethodsOrdering.NONE:
				orderdedMethods = ct.methods;
				break;
			case MethodsOrdering.PREF:
				orderdedMethods = ct.methods.OrderBy (m => m.preference != personality).ToList ();
				break;

			case MethodsOrdering.COSTANDPREF:
				orderdedMethods = ct.methods.OrderBy (m => m.leastCost ()).ThenBy (m => m.preference != personality).ToList ();
				break;

			}
				

			foreach(Method m in orderdedMethods){

				if (RuleMatcher.MatchCondition(m.preconditions,state,m.logicalOperator)){

					List<Task> tempTasks = new List<Task>(tasks);

					tempTasks.Remove(task);

					int j=0;

					foreach (Task subtask in m.subtasks){
						tempTasks.Insert(0 + j ,subtask);
						j++;
					}
					List<PrimitiveTask> tempPlan = SHOPNoBack(tempTasks,plan,personality,state,agent);


					if (tempPlan != null) {

						plan.Concat (tempPlan);
						break;
					} else
						return null;

				}
			}
				
		}

		return plan;
	}

	//Generate plan
	public static List<PrimitiveTask> GeneratePlan(List<Task> tasks, HTNAgent agent){
		

		//Make a copy of the state to be modified by the planner, so that the execution starts from the initial state
		
		State stateCopy = new State();
		
		foreach (Atom fact in agent.state.facts){
			
			Atom factCopy = new Atom (fact.name,fact.sign);
			
			foreach (Term term in fact.terms)
				factCopy.addTerm(term);
			
			stateCopy.addFact (factCopy);
			
		}

		List<PrimitiveTask> plan = new List<PrimitiveTask> ();

		if (agent.backTracking)
			plan = SHOP (tasks, new List<PrimitiveTask> (), agent.personality, stateCopy, agent);
		else
			plan = SHOPNoBack (tasks, new List<PrimitiveTask> (), agent.personality, stateCopy, agent);

		return plan;
	}

	//From the failed task, get a node in hierarchy with a valid method in current state
	public static CompoundTask GetValidNode(PrimitiveTask failedTask, State state){

		CompoundTask parent = new CompoundTask();
		Task current = failedTask;
		

		//Get parent of task, until a parent with a valid method is found, or the root of hierarchy is reached

		while (current.parent!=null) {


			parent = current.parent;


			foreach (Method method in parent.methods)
				if (RuleMatcher.MatchCondition(method.preconditions, state, method.logicalOperator))
					return parent;


			current = parent;

		}

		return parent;

	}



}





