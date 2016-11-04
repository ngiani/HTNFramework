using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;



enum itemType {COMPOUND, PRIMITIVE, METHOD};

struct HTNEditorItem{

	public Rect area;
	public itemType type;
	public object item;

	public HTNEditorItem(Rect a, object i, itemType t){

		area = a;
		item = i;
		type = t;
	}
}



public class HTNEditorWindow : EditorWindow
{

	public static CompoundTask goal;
	public static HTNAgent agent;
	static List<HTNEditorItem> items;
	static HTNEditorItem selected;
	static int persIndex;
	static int varIndex;
	static int actionIndex;

	static Vector2 scrollPositionMethoPre = new Vector2(0,0);
	static Vector2 scrollPositionPre = new Vector2(0,0);
	static Vector2 scrollPositionArg = new Vector2(0,0);
	static Vector2 scrollPositionEff = new Vector2(0,0);
	static Vector2 scrollPositionCunc = new Vector2(0,0);
	static Vector2 scrollPositionHTN = new Vector2 (0, 0);


	Vector2 editorPos = new Vector2(350,25);
	Vector2 itemSize = new Vector2(75, 25);

	public static void  ShowWindow (HTNAgent a, CompoundTask g) {
		goal = g;
		agent = a;

		persIndex = 0;
		varIndex = 0;
		actionIndex = 0;




		if (agent.serializedGoal.name.Length > 0)
			goal = (CompoundTask)agent.serializedGoal.DeSerialize (null);

		EditorWindow window = EditorWindow.GetWindow(typeof(HTNEditorWindow));

		window.maxSize = new Vector2 (700, 700);
		window.minSize = new Vector2 (700, 700);
		
		window.position = new Rect (200, 200,700, 700);


	}


	void removeTask(object data){
		
		ArrayList list = (ArrayList)data;
		
		Method method = (Method)list [0];
		
		Task task = (Task)list [1];
		
		method.deleteTask(task);
	}

	Vector2 DrawCompoundTask(CompoundTask task, Method parent, Vector2 position){


		Vector2 dimension = new Vector2 (300, 75);
		Rect area = new Rect (position, dimension);

		GUIStyle style = new GUIStyle ();
		
		style.fontSize = 17;
		style.fontStyle = FontStyle.Bold;
		
		if (selected.item != null) {
			if (selected.item.Equals(task))
				style.normal.textColor = Color.blue;
			else 
				style.normal.textColor = Color.black;
		}
		
		Texture methodTexture = Resources.Load("Textures/compound") as Texture;

		GUI.Label(new Rect(position,new Vector2(100,50)), methodTexture );
		GUI.Label (new Rect(new Vector2(area.x + 40,area.y),dimension), task.name,style);
	

		//MODEL/CONTROLLER : Add method to this task 
		if (Event.current.type == EventType.ContextClick
		    &&
		    area.Contains(Event.current.mousePosition)
		    ) {
		
			
				GenericMenu menu = new GenericMenu();
				menu.AddItem(new GUIContent("Add Method"), false, task.addMethod);

				if (parent!=null){
					ArrayList removeData = new ArrayList(2);
					
					removeData.Add(parent);
					removeData.Add(task);
					menu.AddItem(new GUIContent("Delete"), false, removeTask,removeData);
				}


				menu.ShowAsContext ();
			}

		int index = 0;


		float lastHeight = position.y + 20;

		if (task.methods.Count > 0) {

    		
			//VIEW: Draw methods from this task
			foreach (Method method in task.methods) {

			   if (method != null){

						items.Add(new HTNEditorItem(
							new Rect(new Vector2 (position.x + 15, lastHeight + 20), new Vector2(100,20)),
							method,itemType.METHOD));


						//Lines are drawn for showing hierarchy relationship
						Drawing.DrawLine (new Vector2(position.x, position.y + 10) , new Vector2 (position.x, lastHeight + 30), Color.black, 1.0f,false);
						Drawing.DrawLine (new Vector2 (position.x, lastHeight + 30), new Vector2 (position.x + 10 , lastHeight + 30), Color.black, 1.0f,false);


						lastHeight = DrawMethod (method, task, new Vector2 (position.x + 15, lastHeight + 20)).y;


						index ++;
			    }
			}
		
		}



		return new Vector2 (position.x + 15, lastHeight);
	}
	

	void removeMethod(object data){

		ArrayList list = (ArrayList)data;

		CompoundTask comp = (CompoundTask)list [0];

		Method method = (Method)list [1];

		comp.removeMethod (method);
	}

	Vector2 DrawMethod (Method method, CompoundTask parent, Vector2 position){
		
		Vector2 dimension = new Vector2 (300, 75);
		Rect area = new Rect (position, dimension);

		GUIStyle style = new GUIStyle ();

		style.fontSize = 15;
		style.fontStyle = FontStyle.Bold;

		
		if (selected.item != null) {
			if (selected.item.Equals(method))
				style.normal.textColor = Color.blue;
			else 
				style.normal.textColor = Color.black;
		}
			

		Texture methodTexture = Resources.Load("Textures/method") as Texture;

		GUI.Label(new Rect(position,new Vector2(100,50)), methodTexture );
		GUI.Label (new Rect(new Vector2(area.x + 40,area.y),dimension), method.name,style);


		Event currentEvent = Event.current;
		
		
		if (currentEvent.type == EventType.ContextClick
		    &&
		    area.Contains(currentEvent.mousePosition)
		    ) {


			GenericMenu menu = new GenericMenu();
			menu.AddItem(new GUIContent("Add Compound"), false, method.addCompoundTask);
			menu.AddItem(new GUIContent("Add Primitive"), false, method.addPrimitiveTask);

			ArrayList removeData = new ArrayList(2);

			removeData.Add(parent);
			removeData.Add(method);

			menu.AddItem(new GUIContent("Delete"), false, removeMethod,(object)removeData );

			menu.ShowAsContext ();
		}

		int index = 0;

		float lastHeight = position.y +20;

		if (method.subtasks!=null && method.subtasks.Count > 0) {

			foreach (Task task in method.subtasks) {

				//Lines are drawn for showing hierarchy relationship
				Drawing.DrawLine (new Vector2(position.x, position.y + 20) , new Vector2 (position.x, lastHeight + 40), Color.black, 1.0f,false);
				Drawing.DrawLine (new Vector2 (position.x, lastHeight + 40), new Vector2 (position.x + 10 , lastHeight + 40), Color.black, 1.0f,false);


				if (task.GetType() == typeof(PrimitiveTask)){


					items.Add(new HTNEditorItem(
						new Rect(new Vector2 (position.x + 15, lastHeight + 30), new Vector2(100,20)),
						task, itemType.PRIMITIVE));

					lastHeight = DrawPrimitiveTask ((PrimitiveTask)task, method, new Vector2(position.x + 15, lastHeight + 30 )).y;


				}

				else if (task.GetType() == typeof(CompoundTask)){


					items.Add(new HTNEditorItem(
						new Rect(new Vector2 (position.x + 15, lastHeight + 30), new Vector2(100,20)),
						task,itemType.COMPOUND));

					lastHeight = DrawCompoundTask ((CompoundTask)task, method, new Vector2(position.x + 15, lastHeight + 30)).y;

				}

				index ++ ;
			}

		}

		return new Vector2 (position.x + 15, lastHeight);
	}



	Vector2 DrawPrimitiveTask(PrimitiveTask task, Method parent, Vector2 position){

		Vector2 dimension = new Vector2 (300, 75);
		Rect area = new Rect (position, dimension);


		GUIStyle style = new GUIStyle ();

		style.fontSize = 15;

		if (selected.item != null) {
			if (selected.item.Equals(task))
				style.normal.textColor = Color.blue;
			else 
				style.normal.textColor = Color.black;
		}

		if (Event.current.type == EventType.ContextClick
		    &&
		    area.Contains(Event.current.mousePosition)
		    ) {


			GenericMenu menu = new GenericMenu();

			ArrayList removeData = new ArrayList(2);
			
			removeData.Add(parent);
			removeData.Add(task);

			menu.AddItem(new GUIContent("Delete"), false, removeTask,removeData);
			
			menu.ShowAsContext ();
		}
		Texture primitiveTexture = Resources.Load("Textures/primitive") as Texture;

		GUI.Label(new Rect(position,new Vector2(100,50)), primitiveTexture );
		GUI.Label (new Rect(new Vector2(area.x + 40,area.y),dimension), task.name,style);


		/*string taskName = string.Concat (task.Name, "(");

		foreach (Variable argument in task.arguments) {


		}*/

		return position;

	}

	Vector2 calcLastHeight(Task task, Vector2 pos){

		if (task.GetType () == typeof(CompoundTask)) {

			Vector2 lastPos = pos;

			CompoundTask cp = (CompoundTask) task;

			foreach (Method m in cp.methods)
				foreach (Task t in m.subtasks)
					lastPos = calcLastHeight(t, new Vector2(lastPos.x + 30, lastPos.y + 60));


			return lastPos;
		}

		return pos;
	}
	void OnGUI () {


		//A line separates HTN Editor from task editor
		Drawing.DrawLine (new Vector2 (302, 0), new Vector2 (302, 700), Color.grey, 2.0f,false);
		


					/*HTN EDITOR*/
			items = new List<HTNEditorItem>();

			items.Add(new HTNEditorItem(
				new Rect(new Vector2 (0, 0), new Vector2(100,30)),
				goal,itemType.COMPOUND));

			
			DrawCompoundTask(goal, null, new Vector2(10,0));

			//GUI.EndScrollView ();
			Event currentEvent = Event.current;

			foreach (HTNEditorItem htnItem in items)
				if (currentEvent.type == EventType.MouseDown
					&& currentEvent.button == 0
					&& htnItem.area.Contains (currentEvent.mousePosition)
					    ) {
		
					selected = htnItem;
					
				}

			
				
					/*TASK EDITOR*/

			if (selected.item != null) {
				
					GUIStyle labelStyle = new GUIStyle ();

					labelStyle.fontSize = 12;
					labelStyle.normal.textColor = Color.black;
					labelStyle.fontStyle = FontStyle.Bold;


						
				if (selected.type == itemType.COMPOUND){

					int i = 0;


					CompoundTask selectedCT = (CompoundTask) selected.item;

					

						GUI.Label(new Rect(editorPos,itemSize),"Name",labelStyle);

						selectedCT.name = GUI.TextField(new Rect(new Vector2(editorPos.x + 50, editorPos.y),
				                                         			 new Vector2(75,20)),
														selectedCT.name);

						if (selectedCT.Equals(goal)){
							GUI.Label(new Rect(new Vector2(editorPos.x, editorPos.y + 100),itemSize),"Goal condition",labelStyle);
						    		

							GUI.Label(new Rect(new Vector2(editorPos.x, editorPos.y + 125),new Vector2(120,20)),"Logical operator");
							
							string[] operatorOptions = new string[]{ "AND","OR"};

							int selectedOperator = Array.IndexOf<string> (operatorOptions, agent.logicalOperator.ToString ());

							selectedOperator = EditorGUI.Popup (new Rect (new Vector2 (editorPos.x + 100, editorPos.y + 125), new Vector2 (50, 20)), 
								selectedOperator, operatorOptions);
							if (selectedOperator == 0)
								agent.logicalOperator = LogicalOperator.AND;
							else if (selectedOperator == 1)
								agent.logicalOperator = LogicalOperator.OR;
							


							if (agent.goalCondition.Count> 0)
								
								
									
								for(i = 0 ; i < agent.goalCondition.Count; i++){
									
									Atom cond = agent.goalCondition[i];
									
									
									string condLabelName = string.Concat(cond.name, "(");
												
									foreach (Term var in cond.terms) {
													
										condLabelName = string.Concat (condLabelName, var.key);
													
										if (cond.terms.IndexOf (var) < cond.terms.Count - 1)
												condLabelName = string.Concat (condLabelName, ",");
									}
									
									condLabelName = string.Concat(condLabelName, ")");
									
									
									GUI.Label(new Rect(
											new Vector2(editorPos.x, editorPos.y + 150 + i*25),new Vector2(200,50)),condLabelName);
									
									
									
									if (GUI.Button(new Rect(new Vector2(editorPos.x + 110, editorPos.y + 150 + i*25),
						                        	new Vector2(50,15)),"Edit"))
										ConditionEditorWindow.ShowWindow(agent,cond);
									
									if (GUI.Button(new Rect(new Vector2(editorPos.x + 175, editorPos.y + 150 + i*25),
						                        	new Vector2(50,15)),"Delete")){
											agent.goalCondition.Remove(cond);
											ConditionEditorWindow.GetWindow<ConditionEditorWindow>().Close();
										}
									
								}
							
							
							
							if (GUI.Button (new Rect(new Vector2(editorPos.x, editorPos.y + 150 + i*25),
							                         new Vector2(75,20)),
							                "Add ")){
								
								Atom cond = new Atom("New Condition");
								
								agent.goalCondition.Add(cond);
								
								ConditionEditorWindow.ShowWindow(agent,cond);
								
							}
							//agent.goalCondition = 
						}
	

			}



			else if (selected.type == itemType.PRIMITIVE){
				
				
				PrimitiveTask selectedPT = (PrimitiveTask) selected.item;

			
					
					
				GUI.Label(new Rect(editorPos,itemSize),"Name", labelStyle);
				
				selectedPT.name = GUI.TextField(new Rect(new Vector2(editorPos.x + 50,editorPos.y), new Vector2(75,20)),
				                                selectedPT.name);




				GUI.Label(new Rect(new Vector2(editorPos.x, editorPos.y + 50),itemSize),"Cost", labelStyle);
						
				selectedPT.cost = EditorGUI.FloatField(new Rect(new Vector2(editorPos.x +50, editorPos.y + 50),
				                                                     new Vector2(75,20)),
				                                            selectedPT.cost);



				GUI.Label(new Rect( new Vector2(editorPos.x, editorPos.y + 90), itemSize), "Arguments",labelStyle);

                int i = 0;


				scrollPositionArg = GUI.BeginScrollView(new Rect(new Vector2(editorPos.x, editorPos.y + 120),new Vector2(300,50)),
				                                        scrollPositionArg,new Rect(new Vector2(editorPos.x, editorPos.y + 120),
				                           new Vector2(275,50 + selectedPT.arguments.Count*10 )),false,false);

				
					if (selectedPT.arguments.Count > 0)
	                    for (i = 0; i < selectedPT.arguments.Count; i++)
	                    {



	                        //GUILayout.BeginHorizontal(GUILayout.Width(200));
	                        GUI.Label(new Rect(
	                            new Vector2(editorPos.x, editorPos.y + 120 + i * 25), itemSize), "Name");

							
						
							List<string> domVarNamesList = new List<string> ();
							
							
							foreach (Term v in agent.domainVariables)
								domVarNamesList.Add (v.key);

							varIndex = domVarNamesList.IndexOf(	selectedPT.arguments[i].key);

							if (varIndex == -1)
								varIndex = 0;

							varIndex = EditorGUI.Popup(new Rect(new Vector2(editorPos.x + 105, editorPos.y + 120 + i * 25),
						                                    new Vector2(50, 20)), varIndex, domVarNamesList.ToArray());
							

							selectedPT.arguments[i] = agent.domainVariables[varIndex];


	                        if (GUI.Button(new Rect(new Vector2(editorPos.x + 160, editorPos.y + 120 + i * 25),
					                        	new Vector2(50,15)), "Delete"))
	                            selectedPT.deleteArgument(selectedPT.arguments[i]);

	                    }

				GUI.EndScrollView();



				Vector2 AddButtonPos = new Vector2(0,0);

				if (selectedPT.arguments.Count == 0)

					AddButtonPos = new Vector2(editorPos.x, editorPos.y + 120);

				else if (selectedPT.arguments.Count > 0)

					AddButtonPos = new Vector2(editorPos.x, editorPos.y + 170);


				if (GUI.Button(new Rect(AddButtonPos,new Vector2(75, 20)), "Add")){
                    
					foreach (Term argument in agent.domainVariables)
						if (!selectedPT.arguments.Contains(argument)){
							selectedPT.addArgument(argument);
							break;
						}

				}






				i = 0;

				GUI.Label(new Rect(new Vector2(editorPos.x, editorPos.y + 210 + i*25),new Vector2(120,20)),"Preconditions",labelStyle);
					

				GUI.Label(new Rect(new Vector2(editorPos.x, editorPos.y + 240),new Vector2(120,20)),"Logical operator");


				string[] operatorOptions = new string[]{ "AND","OR"};

				int selectedOperator = Array.IndexOf<string> (operatorOptions, selectedPT.logicalOperator.ToString ());

				selectedOperator = EditorGUI.Popup (new Rect (new Vector2 (editorPos.x + 100, editorPos.y + 240), new Vector2 (50, 20)), 
					selectedOperator, operatorOptions);
				if (selectedOperator == 0)
					selectedPT.logicalOperator = LogicalOperator.AND;
				else if (selectedOperator == 1)
					selectedPT.logicalOperator = LogicalOperator.OR;



				

				scrollPositionPre = GUI.BeginScrollView(new Rect(new Vector2(editorPos.x, editorPos.y + 265),new Vector2(300,50)),
				                                        scrollPositionPre,new Rect(new Vector2(editorPos.x, editorPos.y + 265),
				                           								  new Vector2(275,50 + selectedPT.preconditions.Count*10 )),false,false);

					if (selectedPT.preconditions.Count> 0)

						

						for(i = 0 ; i < selectedPT.preconditions.Count; i++){
								
								Atom pre = selectedPT.preconditions[i];


								string preLabelName = "";
								
								if (pre.sign == false)
									preLabelName = string.Concat("NOT(", pre.name, "(");
								else if (pre.sign == true)
									preLabelName = string.Concat(pre.name, "(");
								
								foreach (Term var in pre.terms) {
									
									preLabelName = string.Concat (preLabelName, var.key);
									
									if (pre.terms.IndexOf (var) < pre.terms.Count - 1)
										preLabelName = string.Concat (preLabelName, ",");
								}
								
								if (pre.sign == true)
									preLabelName = string.Concat(preLabelName, ")");
								else if (pre.sign == false)
									preLabelName = string.Concat(preLabelName, "))");
									
												
								GUI.Label(new Rect(
									new Vector2(editorPos.x, editorPos.y + 265 + i*25),new Vector2(200,50)),preLabelName);


												
								if (GUI.Button(new Rect(new Vector2(editorPos.x + 110, editorPos.y + 265 + i*25),
					                        	new Vector2(50,15)),"Edit"))
									ConditionEditorWindow.ShowWindow(agent,pre);

								if (GUI.Button(new Rect(new Vector2(editorPos.x + 175, editorPos.y + 265 + i*25),
					                        new Vector2(50,15)),"Delete")){
									selectedPT.deletePrecondition(pre);

									ConditionEditorWindow.GetWindow<ConditionEditorWindow>().Close();
								}
				
					}


				GUI.EndScrollView();


				if (selectedPT.preconditions.Count == 0)
					
					AddButtonPos = new Vector2(editorPos.x, editorPos.y + 275);
				
				else if (selectedPT.preconditions.Count > 0)
					
					AddButtonPos = new Vector2(editorPos.x, editorPos.y + 315);


				if (GUI.Button (new Rect(AddButtonPos,
				                         new Vector2(75,20)),
				                "Add ")){
					
					Atom pre = new Atom("New Condition");
					
					selectedPT.addPrecondition(pre);
					
					ConditionEditorWindow.ShowWindow(agent,pre);
					
				}




				i = 0;

				GUI.Label(new Rect(new Vector2(editorPos.x, editorPos.y + 350 + i*25),new Vector2(120,20)),"Concurrent tasks",labelStyle);


				scrollPositionCunc = GUI.BeginScrollView(new Rect(new Vector2(editorPos.x, editorPos.y + 375),new Vector2(350,75)),
				                                         scrollPositionCunc,new Rect(new Vector2(editorPos.x, editorPos.y + 375),
				                           new Vector2(325,75 + selectedPT.cuncurrentTasks.Count*10 )),false,false);
				
					for (i = 0; i < selectedPT.cuncurrentTasks.Count; i++){

						CuncurrentTask cuncurrent = selectedPT.cuncurrentTasks[i];


						string[] options = new string[]{"True","False"};

						int selectedIndex = 0;

						if (cuncurrent.sign == true)
							selectedIndex = 0;

						else if (cuncurrent.sign == false)
							selectedIndex = 1;

						selectedIndex = EditorGUI.Popup(new Rect(new Vector2(editorPos.x, editorPos.y + 375 + i*25),new Vector2(75,20)),selectedIndex,options);


						if (selectedIndex == 0)
							cuncurrent.sign = true;
						else if (selectedIndex == 1)
							cuncurrent.sign = false;

						cuncurrent.task = GUI.TextField(new Rect(new Vector2(editorPos.x + 100, editorPos.y + 375 + i*25),new Vector2(75,20)),cuncurrent.task);

						selectedPT.cuncurrentTasks[i] = cuncurrent;


						int selectedVar = agent.domainVariables.IndexOf (agent.domainVariables.Find((Term t) => t.key == cuncurrent.agentVarName));

						if (selectedVar == -1)
							selectedVar = 0;

						List<string> variablesNames = new List<string> ();

						foreach (Term var in agent.domainVariables)
							variablesNames.Add (var.key);


						selectedVar = EditorGUI.Popup (new Rect (new Vector2 (editorPos.x + 200, editorPos.y + 375 + i * 25), new Vector2 (50, 20)), 
							selectedVar, variablesNames.ToArray ());

						cuncurrent.agentVarName = agent.domainVariables [selectedVar].key;
							

						if (GUI.Button(new Rect(new Vector2(editorPos.x + 275, editorPos.y + 375 + i*25),new Vector2(45,15)),"Delete")){
								selectedPT.cuncurrentTasks.Remove(selectedPT.cuncurrentTasks[i]);
								ConditionEditorWindow.GetWindow<ConditionEditorWindow>().Close();
							}
				}

				GUI.EndScrollView();

				
				if (selectedPT.cuncurrentTasks.Count == 0)
					
					AddButtonPos = new Vector2(editorPos.x, editorPos.y + 375);
				
				else if (selectedPT.cuncurrentTasks.Count > 0)
					
					AddButtonPos = new Vector2(editorPos.x, editorPos.y + 425);

				if (GUI.Button(new Rect(AddButtonPos,new Vector2(75,20)),"Add"))
					selectedPT.cuncurrentTasks.Add(new CuncurrentTask(" ",false, agent.domainVariables[0].key));







				i = 0;
				

				GUI.Label(new Rect(new Vector2(editorPos.x, editorPos.y + 475 + i*25),new Vector2(120,20)),"Effects",labelStyle);


				scrollPositionEff = GUI.BeginScrollView(new Rect(new Vector2(editorPos.x, editorPos.y + 500),new Vector2(300,50)),
				                                        scrollPositionEff,new Rect(new Vector2(editorPos.x, editorPos.y + 475),
				                           new Vector2(275,50 + selectedPT.effects.Count*10 )),false,false);

					
						if (selectedPT.effects.Count> 0)
							

						for(i= 0; i < selectedPT.effects.Count; i++){
							
							Atom eff = selectedPT.effects[i];
							

								
							string effLabelName = "";

							if (eff.sign == false)
								effLabelName = string.Concat("NOT(", eff.name, "(");

							else if (eff.sign == true)
								effLabelName = string.Concat(eff.name, "(");

							foreach (Term var in eff.terms) {

								effLabelName = string.Concat (effLabelName, var.key);

								if (eff.terms.IndexOf (var) < eff.terms.Count - 1)
										effLabelName = string.Concat (effLabelName, ",");
							}
							
							
							if (eff.sign == true)
								effLabelName = string.Concat(effLabelName, ")");
							else if (eff.sign == false)
								effLabelName = string.Concat(effLabelName, "))");


							GUI.Label(new Rect(new Vector2(editorPos.x, editorPos.y + 475 +  i*25 ),new Vector2(200,50)),effLabelName);
							
							if (GUI.Button(new Rect(new Vector2(editorPos.x + 110, editorPos.y + 475 + i*25 ),
							                        new Vector2(45,15)),"Edit"))
									ConditionEditorWindow.ShowWindow(agent,eff);
							
							if (GUI.Button(new Rect(new Vector2(editorPos.x + 175, editorPos.y + 475 + i*25 ),
					                        	new Vector2(45,15)),"Delete")){
									selectedPT.deleteEffect(eff);
									ConditionEditorWindow.GetWindow<ConditionEditorWindow>().Close();
								}
					
						
					}
				
				GUI.EndScrollView();


				if (selectedPT.effects.Count == 0)
					
					AddButtonPos = new Vector2(editorPos.x, editorPos.y + 500);
				
				else if (selectedPT.effects.Count > 0)
					
					AddButtonPos = new Vector2(editorPos.x, editorPos.y + 550);
				
				if (GUI.Button (new Rect(AddButtonPos,
				                         new Vector2(75,20)),
				                "Add")){
					
					Atom eff = new Atom("New Effect");
					
					selectedPT.addEffect(eff);
					
					ConditionEditorWindow.ShowWindow(agent,eff);
					
				}





				GUI.Label(new Rect(new Vector2(editorPos.x, editorPos.y + 600),new Vector2(120,20)),"Ground Action",labelStyle);


				string[] actionsOptions = {"Move","Animate", "Custom"}; 

				if (selectedPT.actionType == ActionTypes.MOVEMENT)
					actionIndex = 0;
				else if (selectedPT.actionType == ActionTypes.ANIMATION)
					actionIndex = 1;
				else if (selectedPT.actionType == ActionTypes.CUSTOM)
					actionIndex = 2;


				//if (GUI.Button (new Rect(new Vector2(editorPos.x, editorPos.y + 500), new Vector2(200,20)),"Edit Action"));
				actionIndex = EditorGUI.Popup(new Rect(new Vector2(editorPos.x, editorPos.y + 625), new Vector2(100,20)),actionIndex,actionsOptions);

				/*if (selectedPT.groundData == null)
					selectedPT.groundData = new GroundData();*/

				if (actionIndex == 0) {
					selectedPT.actionType = ActionTypes.MOVEMENT;

					GUI.Label (new Rect (new Vector2 (editorPos.x + 110 , editorPos.y + 625), new Vector2 (100, 20)), "Speed");


					selectedPT.groundData.speed = EditorGUI.FloatField(new Rect (new Vector2 (editorPos.x + 160, editorPos.y + 625), new Vector2 (50, 20)), selectedPT.groundData.speed);


					GUI.Label (new Rect (new Vector2 (editorPos.x + 220 , editorPos.y + 625), new Vector2 (100, 20)), "Radius");


					selectedPT.groundData.radius = EditorGUI.FloatField(new Rect (new Vector2 (editorPos.x + 270, editorPos.y + 625), new Vector2 (50, 20)), selectedPT.groundData.radius);


					if (agent.gameObject.GetComponent<Animator>()!=null){
						GUI.Label (new Rect (new Vector2 (editorPos.x + 110 , editorPos.y + 650), new Vector2 (100, 20)), "Animation ");
						
						
						selectedPT.groundData.animationState = GUI.TextField(new Rect (new Vector2 (editorPos.x + 180, editorPos.y + 650), new Vector2 (50, 20)), selectedPT.groundData.animationState);
					}

				}

				else if (actionIndex == 1){
					selectedPT.actionType = ActionTypes.ANIMATION;

					if (agent.gameObject.GetComponent<Animator>()!=null){
						GUI.Label (new Rect (new Vector2 (editorPos.x + 110 , editorPos.y + 625), new Vector2 (100, 20)), "State");

						selectedPT.groundData.animationState = GUI.TextField(new Rect(new Vector2(editorPos.x + 150, editorPos.y + 625), new Vector2(75,20)),
						                                                     selectedPT.groundData.animationState);
					}
				}

				else if (actionIndex == 2){
					selectedPT.actionType = ActionTypes.CUSTOM;


					selectedPT.groundData.name = GUI.TextField(new Rect(new Vector2(editorPos.x + 110, editorPos.y + 625), new Vector2(75,20)),
					                                         selectedPT.groundData.name.ToString());
					
					if (GUI.Button(new Rect(new Vector2(editorPos.x + 200, editorPos.y + 625),
					                        new Vector2(100,20)),"Create/Open")){


						if (!File.Exists ("Assets/Actions/" + (string)selectedPT.groundData.name + ".cs")) {
							using (var stream = new StreamWriter ("Assets/Actions/" + (string)selectedPT.groundData.name + ".cs")) {

								stream.Write (
									@"using UnityEngine;
using System;
using System.Collections;


public class " + (string)selectedPT.groundData.name + @": PrimitiveTask, IGroundAction
{
		GameObject agentObj;

		public " + (string)selectedPT.groundData.name + @"(HTNAgent a, string n){

		name = n;
		agentObj = a.gameObject;	

	}


	public string Name{

		get{return name;}
		set {name = value;}
	}

	//This starts agent's action execution
	public void Start(ActionManager manager, State state){
		

		
		//Monitor precondition validity in current state: if not valid, notify to manager
		if (preconditions.Count > 0 && !RuleMatcher.MatchCondition (preconditions, state,logicalOperator))
			manager.NotValid ();
		
		

		else {
			
			foreach (CuncurrentTask cuncurrent in cuncurrentTasks)
				if (!CheckCuncurrency (cuncurrent,agentObj.GetComponent<HTNAgent>())){

						manager.Suspend ();

						return;
					}

		
			//Add custom code
		}
		
	}


	//Runs at every frame during action's execution
	public void Update(ActionManager manager, State state){
		

		
		//Monitor preconditions validity in current state: if not valid, notify to manager
		if (preconditions.Count > 0 && !RuleMatcher.MatchCondition (preconditions, state,logicalOperator))
			manager.NotValid ();

		
		else {

			foreach (CuncurrentTask cuncurrent in cuncurrentTasks)
				if (!CheckCuncurrency (cuncurrent,agentObj.GetComponent<HTNAgent>())){

						manager.Suspend ();

						return;
					}

			//Add custom code

		}
		
	}

	public void OnComplete(ActionManager manager, State state){

		//Add custom code


		//Flag as completed
		completed = true;

		//Apply effects
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


		//Call next action
		manager.NextAction ();
	}

}");

								stream.Close ();
							}

						}

						UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal("Assets/Actions/" + (string)selectedPT.groundData.name +".cs", 1);

					}


				}



			}


		else if (selected.type == itemType.METHOD){

				Method selectedM = (Method) selected.item;

				
				GUI.Label(new Rect(editorPos,itemSize),"Name",labelStyle);
				
				selectedM.name = GUI.TextField(new Rect(new Vector2(editorPos.x + 50,editorPos.y), new Vector2(75,20)),
				                               selectedM.name);


				if (AtomListWindow.atomNames.list.Count > 0){
					GUI.Label(new Rect(new Vector2(editorPos.x,editorPos.y + 75),itemSize),"Preference",labelStyle);

					string[] options = new string[PersonalityEditorWindow.personalities.list.Count +  2];

					options [0] = "none";

					PersonalityEditorWindow.personalities.list.CopyTo(options,1);



					 
					persIndex =  new List<string>(options).IndexOf(selectedM.preference);



					persIndex = EditorGUI.Popup(new Rect(new Vector2(editorPos.x + 100, editorPos.y + 75),itemSize),persIndex,options);

					selectedM.preference = options[persIndex];
				}




					GUI.Label(new Rect(new Vector2(editorPos.x, editorPos.y + 125),new Vector2(120,20)),"Preconditions",labelStyle);




					GUI.Label(new Rect(new Vector2(editorPos.x, editorPos.y + 150),new Vector2(120,20)),"Logical operator");


					string[] operatorOptions = new string[]{ "AND","OR"};

					int selectedOperator = Array.IndexOf<string> (operatorOptions, selectedM.logicalOperator.ToString ());

					selectedOperator = EditorGUI.Popup (new Rect (new Vector2 (editorPos.x + 100, editorPos.y + 150), new Vector2 (50, 20)), 
												selectedOperator, operatorOptions);
					if (selectedOperator == 0)
						selectedM.logicalOperator = LogicalOperator.AND;
					else if (selectedOperator == 1)
						selectedM.logicalOperator = LogicalOperator.OR;

					

					scrollPositionMethoPre = GUI.BeginScrollView(new Rect(new Vector2(editorPos.x, editorPos.y + 175),new Vector2(300,100)),
					                                             scrollPositionMethoPre,new Rect(new Vector2(editorPos.x, editorPos.y + 175),
				                                												 new Vector2(275,100 + selectedM.preconditions.Count*25 )),false,false);


						int i = 0;
						
						if (selectedM.preconditions.Count> 0)
							
							for(i= 0; i < selectedM.preconditions.Count; i++){
								Atom pre = selectedM.preconditions[i];

								string preLabelName = "";

								if (pre.sign == false)
									preLabelName = string.Concat("NOT(", pre.name, "(");
								else if (pre.sign == true)
									preLabelName = string.Concat(pre.name, "(");
								
								foreach (Term var in pre.terms) {

									preLabelName = string.Concat (preLabelName, var.key);

									if (pre.terms.IndexOf (var) < pre.terms.Count - 1)
										preLabelName = string.Concat (preLabelName, ",");
								}
							
								if (pre.sign == true)
									preLabelName = string.Concat(preLabelName, ")");
								else if (pre.sign == false)
									preLabelName = string.Concat(preLabelName, "))");


								GUI.Label(new Rect(
										new Vector2(editorPos.x, editorPos.y + 200 + i*25),new Vector2(200,50)),preLabelName);


								
								if (GUI.Button(new Rect(new Vector2(editorPos.x + 110, editorPos.y + 200 + i*25),
					                        			new Vector2(45,15)),"Edit"))
									ConditionEditorWindow.ShowWindow(agent,pre);
								
								if (GUI.Button(new Rect(new Vector2(editorPos.x + 175, editorPos.y + 200 + i*25),
					                        			new Vector2(45,15)),"Delete")){
									selectedM.deletePrecondition(pre);

									ConditionEditorWindow.GetWindow<ConditionEditorWindow>().Close();
							}
				
						}
						
					GUI.EndScrollView();

					
					Vector2 AddButtonPos = new Vector2(0,0);

					if (selectedM.preconditions.Count == 0)
						
						AddButtonPos = new Vector2(editorPos.x, editorPos.y + 175);
					
					else if (selectedM.preconditions.Count > 0)
						
						AddButtonPos = new Vector2(editorPos.x, editorPos.y + 275);


					if (GUI.Button (new Rect(AddButtonPos,
					                         new Vector2(75,20)),
					                "Add ")){
						
						Atom pre = new Atom("New Condition");
						
						selectedM.addPrecondition(pre);
						
						ConditionEditorWindow.ShowWindow(agent,pre);
						
					}

		}

		
	}

	
	
  }


	void OnDestroy(){

		agent.serializedGoal = new SerializedTask (goal,null);

	}


	
}

