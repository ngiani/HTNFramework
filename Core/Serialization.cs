using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;



//Workaround to solve class serialization problem
[Serializable]
public class SerializedTask{

	public string type;
	public SerializedTask parent;
	
	//Compound data
	public string name;
	public List<SerializedMethod> methods;
	
	//Primitive data
	public float cost;
	public List<Atom> preconditions;
	public List<Term> arguments;
	public List<CuncurrentTask> cuncurrentTasks;
	public List<Atom> effects;
	public ActionTypes actionType;
	public GroundData groundData;



	public bool loop;
	public LogicalOperator logicalOperator;


	public SerializedTask(CompoundTask t, SerializedTask p){

		type = "Compound";
		parent = p;

		name = t.name;

		methods = new List<SerializedMethod> ();

		foreach (Method m in t.methods)
			methods.Add (new SerializedMethod (m,this));
		
		cost = 0;
		effects = null;
		preconditions = null;
		cuncurrentTasks = null;
	}
	
	public SerializedTask(PrimitiveTask t, SerializedTask p){

		
		type = "Primitive";
		parent = p;

		name = t.name;
		
		cost = t.cost;
		
		preconditions = t.preconditions;
		
		cuncurrentTasks = t.cuncurrentTasks;

		effects = t.effects;


		arguments = t.arguments;
		arguments.Capacity = 3;


		actionType = t.actionType;

		groundData.name = t.groundData.name;
		groundData.radius = t.groundData.radius;
		groundData.speed = t.groundData.speed;
		groundData.animationState = t.groundData.animationState;

		loop = t.loop;
		logicalOperator = t.logicalOperator;
	}

	public Task DeSerialize(CompoundTask p){


		if (type == "Compound") {

			CompoundTask cp = new CompoundTask(name, p);

			foreach (SerializedMethod sm in methods) {

				Method m = sm.DeSerialize (cp);

				cp.addMethod (m);

			}

			return cp;

		} else {

			PrimitiveTask tp = new PrimitiveTask(name, p);


			tp.cost = cost;


			tp.effects = effects;
		
			tp.cuncurrentTasks = cuncurrentTasks;

			tp.preconditions = preconditions;

			arguments.Capacity = 3;
			tp.arguments = arguments;

			tp.actionType = actionType;

			tp.groundData = groundData;

			tp.loop = loop;

			tp.logicalOperator = logicalOperator;

			return tp;
		}
	}
	

	
}

[Serializable]
public class SerializedMethod {

	public string name;

	public SerializedTask parent;

	public string preference;

	public List<Atom> preconditions;

	public List<SerializedTask> subtasks;

	LogicalOperator logicalOperator;

	public SerializedMethod(Method m, SerializedTask p){

		
		name = m.name;

		parent = p;

		preference = m.preference;

		preconditions = m.preconditions;

		logicalOperator = m.logicalOperator;

		subtasks = new List<SerializedTask> ();


		foreach (Task t in m.subtasks) {
			if (t.GetType() == typeof(CompoundTask))
				subtasks.Add (new SerializedTask ((CompoundTask)t,parent));
			else if (t.GetType() == typeof(PrimitiveTask))
				subtasks.Add(new SerializedTask((PrimitiveTask)t, parent));
		}
	}


	public Method DeSerialize(CompoundTask p){

		Method m = new Method (name, null);
		
		m = new Method (name, p);

		m.preference = preference;

		m.logicalOperator = logicalOperator;

		m.preconditions = preconditions;


		foreach (SerializedTask st in subtasks)
			m.subtasks.Add (st.DeSerialize (p));


		return m;
	}

}

[Serializable]
public class SerializedFact{

	public string name;
	public bool sign;
	public List<string> values;

	public SerializedFact(string n){

		name = n;
		sign = true;
		values = new List<string> ();
	}

	public SerializedFact(string n, bool s){

		name = n;
		sign = s;
		values = new List<string> ();
	}
}

[Serializable]
public class SerializedVariable {
	
	public string key;
	public string value;
	
	public SerializedVariable(string n){
		
		key = n;
		value = "";
	}
	
	public SerializedVariable(string n, string v){
		
		key = n;
		value = v;
	}

}


