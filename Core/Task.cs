using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;

[Serializable]
public enum ActionTypes {MOVEMENT,ANIMATION, CUSTOM};

//Interface for ground action
public interface IGroundAction {

	//Start
	void Start(ActionManager manager, State state);

	//Running action's code at every frame
	void Update(ActionManager manager, State state);

	//When action is completed
	void OnComplete (ActionManager manager, State state);

}

//Structure for ground action data
[Serializable]
public struct GroundData{


	//MOVEMENT DATA
	//arrival circle radius
	public float radius;
	
	//movement speed
	public float speed;
	

	//ANIMATION DATA
	public string animationState;


	//CUSTOM DATA
	
	//class name
	public string name;



}



public enum ManagerState {STOP, EMPTY, INVALID, PAUSE, EXECUTE}

//Action manager for ground actions scheduling and execution
public class ActionManager{

	//actions queue
	public Queue<IGroundAction> actions;
	//action currently running
	public IGroundAction currentAction;

	public ManagerState state;

	HTNAgent agent;


	public ActionManager(HTNAgent a){

		actions = new Queue<IGroundAction> ();
		state = ManagerState.STOP;

		agent = a;

	}

	public void ScheduleAction(IGroundAction action){

		actions.Enqueue (action);
	}

	//Next action in queue
	public void NextAction(){

		//If there are still actions or agent hasn't reached the goal, get and start next action. Otherwise change state to COMPLETED plan

		if (actions.Count > 0) {
			
			IGroundAction action = actions.Dequeue ();

			currentAction = action;

			//Start action

			action.Start (this, agent.state);



		} else if (actions.Count == 0 || RuleMatcher.MatchCondition (agent.goalCondition, agent.state,agent.logicalOperator)) {
			state = ManagerState.EMPTY;
			currentAction = null;
		}
	}

	//Notification of not valid preconditions : change state to INVALID plan
	public void NotValid(){
		
		state = ManagerState.INVALID;

		actions.Clear ();

	}

	//Suspend current action execution
	public void Suspend(){
		
		state = ManagerState.PAUSE;
		
	}

	//Resume execution
	public void Resume(){
		
		state = ManagerState.EXECUTE;
		
	}

	//Execute current action
	public void RunCurrentAction(){
		

		//update action
		if (state == ManagerState.EXECUTE)
			currentAction.Update (this, agent.state);

	}




}

[Serializable]
//Structure for define cuncurrent task
public class CuncurrentTask {


	//If the cuncurrent task must or must not being executed by another agent
	public bool sign;

	//Cuncurrent task
	public string task;

	[SerializeField]
	//Variable referring to an agent performing the task
	public string agentVarName;


	public CuncurrentTask(string t, bool s, string v){

		sign = s;
		task = t;
		agentVarName = v;

	}

}






[Serializable]
//[XmlRoot("Task")]
//Generic task
public abstract class Task {


	//[XmlAttribute("name")]
	public string name;
	public bool completed;

	[NonSerialized]
	public CompoundTask parent;






	//Check if node in hierarchy is forefather of this task
	public bool IsMyForeFather(CompoundTask node){

		CompoundTask currentParent = new CompoundTask();

		Task currentTask = this;

		while (currentTask.parent!=null) {

			currentParent = currentTask.parent;

			if (currentParent.Equals(node))
				return true;

			currentTask = currentParent;
		}

		return false;

	}
	
}




public enum LogicalOperator{AND, OR};


[Serializable]
//Primitive task : action schema for ground actions
public class PrimitiveTask : Task {

	public List<Term> arguments;

	//Preconditions and effects of task execution
	public List<Atom> preconditions;
	public List<Atom> effects;

	//Cuncurrent tasks
	public List<CuncurrentTask> cuncurrentTasks;

	//Data for ground actions (to be used only by ground actions)
	public GroundData groundData;

	//task cost
	public float cost;

	//repeat task while conditions are satisfied
	public bool loop;

	//logical operator in preconditions
	public LogicalOperator logicalOperator;

	//ground action type
	public ActionTypes actionType;


	public PrimitiveTask(){
		
		
		name = "New Method"; 
		
		arguments = new List<Term>();
		arguments.Capacity = 3;
		
		
		preconditions = new List<Atom> ();
		effects = new List<Atom> ();
		cuncurrentTasks = new List<CuncurrentTask> ();

		cost = 1;
		
		loop = false;

		logicalOperator = LogicalOperator.AND;
		
		actionType = ActionTypes.MOVEMENT;
		
		groundData = new GroundData ();
		groundData.animationState = "";
		groundData.name = "";
		
		parent = null;
		
		
	}

	public PrimitiveTask(CompoundTask p){


		name = "New Method"; 
		
		arguments = new List<Term>();
		arguments.Capacity = 3;

		
		preconditions = new List<Atom> ();
		effects = new List<Atom> ();
		cuncurrentTasks = new List<CuncurrentTask> ();

		cost = 1;

		loop = false;

		logicalOperator = LogicalOperator.AND;

		actionType = ActionTypes.MOVEMENT;

		groundData = new GroundData ();
		groundData.animationState = "";
		groundData.name = "";

		parent = p;


	}

	public PrimitiveTask(string n, CompoundTask p) {

		name = n; 

		arguments = new List<Term>();
		arguments.Capacity = 3;


		preconditions = new List<Atom>();
		effects = new List<Atom>();
		cuncurrentTasks = new List<CuncurrentTask> ();


		cost = 1;

		loop = false;

		logicalOperator = LogicalOperator.AND;

		actionType = ActionTypes.MOVEMENT;

		groundData = new GroundData ();
		groundData.animationState = "";
		groundData.name = "";

		parent = p;
		
	
	}


	public void addPrecondition(Atom pre){
		
			preconditions.Add (pre);
	
	}


	public void deletePrecondition(Atom pre){
		
		preconditions.Remove(pre);
	}
	
	public void addEffect(Atom eff){

		
		effects.Add (eff);
	}

	public void deleteEffect(Atom eff){
		
		effects.Remove(eff);
	}

	public void addArgument(Term arg){
		
		if (arguments.Count < 3) {
			
			arguments.Add(arg);
			
			
		} else 
			EditorUtility.DisplayDialog ("Arguments size limit", "Can't add more arguments !","OK");
		
	}
	
	public void deleteArgument(Term argument){
		
		arguments.Remove (argument);
	}

	public State apply(State state){
	

		foreach (Atom effect in effects){
			Atom fact = state.facts.Find((Atom a)=> a.name == effect.name && a.terms.SequenceEqual<Term>(effect.terms));

			if (fact!=null)
				fact.sign = effect.sign;

			else{

				Atom newFact = new Atom(effect.name,effect.sign);

				newFact.terms = new List<Term>(effect.terms);

				state.addFact(newFact);
			}
		}

		return state;
	}

	//Check cuncurrency constraints on task
	public bool CheckCuncurrency(CuncurrentTask cuncurrent, HTNAgent agent){

		//Get other agent from cuncurrent task informations


		GameObject otherObject = null;

		foreach (Term term in agent.domainVariables)
			if (term.key == cuncurrent.agentVarName)
				otherObject = (GameObject)term.value;

		//If character is perceptible...
		if (otherObject!=null && agent.sensorySystem.isPerceptible(otherObject)) {

			HTNAgent other = otherObject.GetComponent<HTNAgent> ();


			if (other.actionManager.currentAction != null && other.FSM.current == AgentState.EXECUTING) {

				//Get other agent's current task from other agent's current action
				PrimitiveTask otherTask = (PrimitiveTask)other.actionManager.currentAction;

				//Check if arguments of this task are equal to arguments of other agent's task
				//bool equalArguments = Enumerable.SequenceEqual (otherTask.arguments.OrderBy (a => a), this.arguments.OrderBy (a => a));
		 


				//If the sign of cuncurrent task is true, but other agent is executing another task than the one specified in cuncurrent task, return false
				if (cuncurrent.sign == true && cuncurrent.task != otherTask.name)
					return false;

				//If the sign of cuncurrent task is false, but other agent is executing the same task as the one specified, return false
				else if (cuncurrent.sign == false && cuncurrent.task == otherTask.name)
					return false;
			}

		}

		//Otherwise return true
		return true;


	}



}




[Serializable]

//Compund task
public class CompoundTask : Task {


	/*[XmlArray("methods")]
	[XmlArrayItem("Method")]*/
	public List<Method> methods;

	public CompoundTask() {
		
		name = "New Compound";
		methods = new List<Method> ();
		parent = null;
		
	}

	public CompoundTask(CompoundTask p) {
		
		name = "New Compound";
		methods = new List<Method> ();
		parent = p;
		
	}


	public CompoundTask(string n, CompoundTask p) {

		name = n;
		methods = new List<Method> ();
		parent = p;

	}
	

	public void addMethod(Method m){

		methods.Add (m);

	}

	public void addMethod(){
		
		methods.Add (new Method ("New Method", this));
		
	}

	public void removeMethod(Method m){

		methods.Remove (m);
	}


	

}

[Serializable]
//Method of compound task decomposition
public class Method {


	public string name;

	private float cost;

	public CompoundTask parent;

	/*[XmlArray("preconditions")]
	[XmlArrayItem("Fact")]*/
	public List<Atom> preconditions;

	/*[XmlArray("subtasks")]
	[XmlArrayItem("Task")]*/
	public List<Task> subtasks;

	//[XmlAttribute("preference")]
	public string preference;

	public LogicalOperator logicalOperator;


	public Method(CompoundTask p) {

        name = "New Method";
		preconditions = new List<Atom> ();
		subtasks = new List<Task> ();
		preference = "none";
		logicalOperator = LogicalOperator.AND;
		parent = p;
		cost = -1.0f;
		
	}

	public Method(string n, CompoundTask p) {

		name = n;
		preconditions = new List<Atom> ();
		subtasks = new List<Task> ();
		preference = "none";
		logicalOperator = LogicalOperator.AND;
		parent = p;
		cost = -1.0f;
	}


	public float Cost{


		get{
			if (cost < 0)
				cost = leastCost();
			return cost;
		}

	}

	public void addTask(Task task) {

		subtasks.Add (task);
	}

	public void addCompoundTask(){

		subtasks.Add(new CompoundTask("New Compound", parent));
	}

	public void addPrimitiveTask(){
		
		subtasks.Add(new PrimitiveTask("New Primitive", parent));
	}

	public void deleteTask(Task task){
		
		subtasks.Remove (task);
	}

	public void addPrecondition(Atom pre){
		
			preconditions.Add (pre);

	}

	public void deletePrecondition(Atom pre){
		
		preconditions.Remove(pre);
	}

	//Least cost plan found choosing this method
	public float leastCost(){

		float c = 0.0f;

		foreach (Task subtask in subtasks) {

			if (subtask.GetType() == typeof(CompoundTask)){

				CompoundTask ct = (CompoundTask)subtask;


				float leastCost = Mathf.Infinity;

				foreach (Method m in ct.methods){

					float currentCost = m.Cost;

					if (currentCost < Mathf.Infinity)
						leastCost = currentCost;

				}

				return leastCost;
			}

			else if (subtask.GetType() == typeof(PrimitiveTask)){

				PrimitiveTask pt = (PrimitiveTask)subtask;

				c+=pt.cost;
				
			}
		}

		return c;

	}

}




