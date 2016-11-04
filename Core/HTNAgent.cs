using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq.Expressions;
using GameDevWare.Dynamic.Expressions.CSharp;



public enum AgentState{IDLE, PLANNING, EXECUTING};  
public enum MethodsOrdering{NONE, PREF, COSTANDPREF};

public class AgentStateMachine{
	
	
	public AgentState current;
	HTNAgent agent;


	public AgentStateMachine(AgentState state, HTNAgent a){
		
		current = state; 
		agent = a;


	}
	
	
	public void Update() {
		
		if (current == AgentState.IDLE) {



			//Transizione a PLANNING: se l'agente ha un task obiettivo non completato, o non esiste una condizione obiettivo, ma il manager delle azioni è comunque in stato STOP

			if (agent.goal.methods.Count > 0 && !agent.goal.completed && (Time.frameCount % 60 == 0 || Time.frameCount == 1) )
				current = AgentState.PLANNING;

			//Transizione a EXECUTING: se il manager delle azioni è in stato EXECUTE 
			else if (agent.actionManager.state == ManagerState.EXECUTE)
				current = AgentState.EXECUTING;
			
			else if (agent.actionManager.state == ManagerState.PAUSE){

				PrimitiveTask currentTask = (PrimitiveTask)agent.actionManager.currentAction;

				foreach (CuncurrentTask cuncurrent in currentTask.cuncurrentTasks)
					if (!currentTask.CheckCuncurrency (cuncurrent,agent))
						return;

				agent.actionManager.Resume ();
			}


		} else if (current == AgentState.PLANNING) {

			/*In this state, agent plans to goal
			 Transition to EXECUTING : if agent has a plan towards goal (not empty actions list)
			 Transizion to IDLE: agent has no plan towards goal (empty actions list)*/


			//Generate or fix plan
			List<Task> tasks = new List<Task>();

			//If manager has stopped with INVALID state, fix plan and get a new plan
			if (agent.actionManager.state == ManagerState.INVALID){

				//Climb the hierarchy and get the first valid node
				CompoundTask validNode = Planner.GetValidNode((PrimitiveTask)agent.actionManager.currentAction, agent.state);

				tasks.Add(validNode);

				
				//Generate first segment of fixed plan, planning from the node found

				List<PrimitiveTask> firstSegment = Planner.GeneratePlan(tasks, agent);

				if (firstSegment.Count > 0){
					//Get second segment from the current plan, deleting tasks who have node found as father or forefather
					List<PrimitiveTask> secondSegment = new List<PrimitiveTask>();

					foreach (PrimitiveTask task in agent.plan)
						if (!task.IsMyForeFather(validNode) && !task.completed)
							secondSegment.Add(task);

					//Fix the plan concatenating first segment with second segment
					agent.plan = firstSegment.Concat(secondSegment).ToList<PrimitiveTask>();
				}

				//No plan..
				else{
					agent.plan = new List<PrimitiveTask>();

					//If agent is a member of a group, notify it to the leader, who will stop execution of global plan
					agent.communicationSystem.sendMessage(new Message(agent.leader,MessageContent.ORDER_FAILURE));
				}

			}

			//Otherwise generate plan 
			else {
				tasks.Add (agent.goal);
				
				agent.plan = Planner.GeneratePlan(tasks, agent);


			}


			if (agent.plan!=null) { 


				foreach (IGroundAction action in agent.plan)
					agent.actionManager.ScheduleAction (action);
				
				agent.actionManager.NextAction ();

				current = AgentState.EXECUTING;

				agent.actionManager.state = ManagerState.EXECUTE;

			} else {

				agent.actionManager.state = ManagerState.STOP;

				current = AgentState.IDLE;
			}

		
		} else if (current == AgentState.EXECUTING) {

			//Dentro lo stato: esecuzione azione corrente del piano
			//Transizione a IDLE : se il piano è completato con successo, oppure il manager delle azioni è andato in pausa
			//Transizione a PLANNING : se il piano non è valido (precondizioni azione non valide)

			agent.actionManager.RunCurrentAction();

			if (agent.actionManager.state == ManagerState.EMPTY){ 

				current = AgentState.IDLE;

				agent.actionManager.state = ManagerState.STOP;

				agent.goal.completed = true;
			}
			else if (agent.actionManager.state == ManagerState.INVALID) current = AgentState.PLANNING;

			else if (agent.actionManager.state == ManagerState.PAUSE) current = AgentState.IDLE; 

		}
	

	}
	
}




public enum AgentTypes {LEADER, MEMBER, SOLO};

//[ExecuteInEditMode]
public class HTNAgent : MonoBehaviour{


	[HideInInspector]
	public State state = new State();

	[HideInInspector]
	public List<SerializedFact> serializedKnowledge = new List<SerializedFact> ();

	[HideInInspector]
	public List<PrimitiveTask> plan;
	
	[HideInInspector]
	public CompoundTask goal = new CompoundTask ("Goal Task",null);

	[HideInInspector]
	public List<Atom> goalCondition = new List<Atom>();

	//Type of agent
	public AgentTypes agentType;


	[HideInInspector]
	public List<HTNAgent> groupMembers;

	[HideInInspector]
	public HTNAgent leader;

	[HideInInspector]
	public CommunicationSystem communicationSystem;


	[HideInInspector]
	public LogicalOperator logicalOperator = LogicalOperator.AND;

	[HideInInspector]
	public SerializedTask serializedGoal = new SerializedTask(new CompoundTask(null),null);

	[HideInInspector]
	public List<Term> domainVariables;

	[HideInInspector]
	public List<SerializedVariable> serializedDomainVariables = new List<SerializedVariable> ();

	[HideInInspector]
	public List<FactDefinition> definitions;




    [HideInInspector]
    public List<string> knownPersonalities;


	List<GameObject> perceptibles = new List<GameObject> ();

	//Algorithm
	[HideInInspector]
	public bool backTracking;

	[HideInInspector]
	public MethodsOrdering methodsOrdering;

	[HideInInspector]
	public string personality = "none";

	[HideInInspector]
	public int sensor;
	[HideInInspector]
	public float radiusLength;
	public Sensor sensorySystem;

	public AgentStateMachine FSM;




	public ActionManager actionManager;

	//Compile signatures to speed-up parser
	void compileExpressionParser(){

		CSharpExpression.Parse<Vector3,bool> ("arg1.x == 0").CompileAot<Vector3,bool> ();
		CSharpExpression.Parse<float,bool> ("arg1 == 0.0f").CompileAot<float,bool> ();

		CSharpExpression.Parse<Vector3,Vector3,bool> ("arg1.x == 0 && arg2.x == 0").CompileAot<Vector3,Vector3,bool> ();
		CSharpExpression.Parse<float,float,bool> ("arg1 == 0.0f && arg2 == 0.0f").CompileAot<float,float,bool> ();
	}

	// Use this for initialization
	void Start () {


		compileExpressionParser ();

		
		//Knowledge update rate
		KnowledgeUpdateWindow.LoadFromFile ();

		//Get goal
		goal = (CompoundTask)serializedGoal.DeSerialize (null);
		
	

       
		
		//Initialize knowledge

		foreach (SerializedFact fact in serializedKnowledge){
			Atom newFact = new Atom (fact.name,fact.sign);

			foreach (string objName in fact.values)
				newFact.addTerm(new Term(GameObject.Find (objName)));

			state.addFact (newFact);
		}
		

		//Initialize variables


			foreach (SerializedVariable var in serializedDomainVariables) {
				if (var.key!="" && var.value!="")
					domainVariables [serializedDomainVariables.IndexOf (var)].value = GameObject.Find (var.value);

			}
		//Instantiate variables
		InstantiateVariables (goal, domainVariables);

		//Add sensor
		if (sensor == 0)
			sensorySystem = new CameraSensor (this);
		else if (sensor == 1)
			sensorySystem = new RadiusSensor (this, radiusLength);


		try {
			definitions = FactDefinitionsContainer.Load (FactsDefinitionWindow.definitionsPath).list;
		}

		catch (Exception e){
			Debug.Log (e.Message);
		}


		
		//Agent FSM
		FSM = new AgentStateMachine (AgentState.IDLE, this);
		
		//Action manager
		actionManager = new ActionManager(this);





		//Coordination 
		communicationSystem = new CommunicationSystem (this);

		groupMembers = new List<HTNAgent> ();

        //Personalities
		knownPersonalities = PersonalitiesContainer.Load(PersonalityEditorWindow.path).list;



    }

	public void InstantiateVariables(Task task, List<Term> variables){

		if (task.GetType () == typeof(PrimitiveTask)) {

			PrimitiveTask pt = (PrimitiveTask)task;

			foreach (Term variable in variables) {
				int index = pt.arguments.FindIndex ((Term arg) => arg.key == variable.key);

				if (index >= 0)
					pt.arguments[index] = variable;

				foreach (Atom pre in pt.preconditions){

					index = pre.terms.FindIndex((Term arg) => arg.key == variable.key);

					if (index >= 0)
						pre.terms[index] = variable;

				}


				foreach (Atom eff in pt.effects){
					
					index = eff.terms.FindIndex((Term arg) => arg.key == variable.key);
					
					if (index >= 0)
						eff.terms[index] = variable;
					
				}
			
			}

		} else if (task.GetType () == typeof(CompoundTask)) {

			CompoundTask ct = (CompoundTask)task;

			foreach (Method m in ct.methods){

				foreach (Atom pre in m.preconditions){

					foreach (Term variable in variables){
						int index = pre.terms.FindIndex((Term arg) => arg.key == variable.key);
						
						if (index >= 0)
							pre.terms[index] = variable;

					}
					
				}


				foreach(Task subTask in m.subtasks)
					InstantiateVariables(subTask, variables);


			}
		}

	}


	
	// Update is called once per frame
	void Update () {



		if (Time.frameCount % (int)KnowledgeUpdateWindow.updateRate == 0 || Time.frameCount == 1 ) {

			perceptibles = sensorySystem.getPerceptibles ();
			SenseFacts (definitions, perceptibles);
			//SenseAgents (perceptibles);


		
		}


		FSM.Update ();
		communicationSystem.readMessages ();

	}



	void OnGUI(){
		

		GUIStyle style = new GUIStyle ();

		style.normal.textColor = Color.black;

		float startX = 0;

		int i = 0;

		GUI.Label (new Rect (startX, i * 25, 500, 50), "CURRENT KNOWLEDGE:", style);

		i = 2;

		foreach (Atom fact in state.facts) {

				if (fact.sign == true){
					

					if (fact.terms.Count == 1)
						GUI.Label(new Rect(startX,i*25,500,50),fact.name + "(" + fact.terms[0].value.ToString().Replace("(UnityEngine.GameObject)","") + ")", style);

					if (fact.terms.Count == 2)
					GUI.Label(new Rect(startX,i*25,500,50),fact.name + "(" + fact.terms[0].value.ToString().Replace("(UnityEngine.GameObject)","") + "," + 
					          fact.terms[1].value.ToString().Replace("(UnityEngine.GameObject)","") + ")", style);

					i++;
			}

		}
			
		i = 0;

		GUI.Label (new Rect (startX + 3 * Screen.width / 4, i * 25, 500, 50), "CURRENT PLAN: ", style);

		i = 2;


		if (plan!=null && plan.Count > 0)
			foreach (PrimitiveTask task in plan) {



				PrimitiveTask currentTask = (PrimitiveTask)actionManager.currentAction;

				if (actionManager.currentAction != null && currentTask.name == task.name)
					style.normal.textColor = new Color (0.0F, 0.5F, 0.0F, 1.0F);
				else 
					style.normal.textColor = Color.black;

				GUI.Label (new Rect (startX + 3 * Screen.width / 4, i * 25, 250, 50), task.name, style);

				i++;
			}
	}

	//Sense facts about every perceptible object from all definitions
	public void SenseFacts(List<FactDefinition> definitions, List<GameObject> perceptibles){


		
		foreach (GameObject obj in perceptibles) {


			//objects to be compared with any perceptible obj to sense binary facts
			GameObject[] objs = new GameObject[perceptibles.Count];
			
			perceptibles.CopyTo (objs);
			
			List<GameObject> objsList = objs.ToList ();

			objsList.Remove(obj);

			sensorySystem.SenseFactFromTag (obj);
			
			foreach (FactDefinition definition in definitions)
				sensorySystem.SenseFactFromDefinition (obj, objsList, definition);



		}
		
	}
	




}







