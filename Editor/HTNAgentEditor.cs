using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


[CustomEditor(typeof(HTNAgent))]
public class HTNAgentEditor : Editor
{
	
	public static HTNAgent agent;
	public static List<SerializedFact> internalKnowledge;
	int persIndex = 0;
	bool foldout;
	bool foldout2;
	bool foldout3;
	bool foldout4;
	string[] sensors = {"camera","radius"};
	int sensIndex = 0;
	string[] factTypes = {"Property","Relation"};
	string[] factSigns = { "false", "true"};
	int selectedFactType = 0;
	int selectedName = 0;
	int backTracking = 0;
	int chosenOrdering = 0;

	void OnEnable(){

		agent = (HTNAgent)target;
		
	}

	public override void OnInspectorGUI()
	{



		DrawDefaultInspector();


		//Sensory
		GUILayout.BeginHorizontal (GUILayout.Width (300));
			GUILayout.Label ("Sensor",GUILayout.Width(110));
			
			if (agent.sensor == 0)
				sensIndex = 0;
		else if (agent.sensor == 1)
				sensIndex = 1;
			
			sensIndex = EditorGUILayout.Popup (sensIndex, sensors, GUILayout.Width (100));
			
			if (sensIndex == 1){
				GUILayout.Label("Length");
				agent.radiusLength = EditorGUILayout.FloatField(agent.radiusLength,GUILayout.Width(25));
			}
		
		GUILayout.EndHorizontal ();


		//Goal
		if(GUILayout.Button("Edit Goal"))
		{
			HTNEditorWindow.ShowWindow(agent,agent.goal);
		}


		agent.sensor = sensIndex;


		//Personality 

		if (agent.knownPersonalities == null)
			agent.knownPersonalities = PersonalitiesContainer.Load ("Assets/personalities.xml").list;

		if (PersonalityEditorWindow.personalities.list.Count > 0) {
				
			foreach (string personality in PersonalityEditorWindow.personalities.list){

				if (!agent.knownPersonalities.Contains(personality))
					agent.knownPersonalities.Add(personality);
			}


				GUILayout.BeginHorizontal (GUILayout.Width (200));
				
					GUILayout.Label ("Personality");


					persIndex = PersonalityEditorWindow.personalities.list.Concat(new List<string>(new string[]{"none"})).ToList().IndexOf(agent.personality);

				    
					string[] personalities = PersonalityEditorWindow.personalities.list.Concat (new List<string> (new string[]{ "none" })).ToList ().ToArray();

					GUILayout.Space(40.0f);
					
					persIndex = EditorGUILayout.Popup (persIndex, personalities, GUILayout.Width (100));

					if (persIndex == -1)
						persIndex = 0;
					agent.personality = personalities [persIndex];
				
				GUILayout.EndHorizontal ();


				
		}


		//Algorithm

		GUILayout.BeginHorizontal (GUILayout.Width (200));
			GUILayout.Label("Backtracking");

			string[] options = { "Yes", "No" };

			backTracking = EditorGUILayout.Popup(backTracking,options,GUILayout.Width(100));

			if (backTracking == 0)
				agent.backTracking = true;
			else if (backTracking == 1)
				agent.backTracking = false;
		GUILayout.EndHorizontal ();

		GUILayout.BeginHorizontal (GUILayout.Width (200));
			GUILayout.Label("Methods ordering");

			GUILayout.Space(10.0f);
			string[] options2 = { "None", "Preferences", "Preferences and costs" };

			chosenOrdering = EditorGUILayout.Popup(chosenOrdering,options2,GUILayout.Width(100));

			switch (chosenOrdering) {
				case 0:
					agent.methodsOrdering = MethodsOrdering.NONE;
					break;
				case 1:
					agent.methodsOrdering = MethodsOrdering.PREF;
					break;
				case 2: 
					agent.methodsOrdering = MethodsOrdering.COSTANDPREF;
					break;
				default:
					break;
			}
		GUILayout.EndHorizontal ();


		//Domain variables
		foldout = EditorGUILayout.Foldout (foldout, "Internal knowledge");

		if (foldout) {

			//MODEL

			GUIStyle style = new GUIStyle();

			style.alignment = (TextAnchor)TextAlignment.Left;


			EditorGUILayout.BeginHorizontal(style,GUILayout.Width(400));

			EditorGUILayout.LabelField("Size", GUILayout.Width(130));

			int oldSize = agent.serializedKnowledge.Count;

			string size = EditorGUILayout.TextField(oldSize.ToString(),GUILayout.Width(300));


			int newSize = 0;

			System.Int32.TryParse(size, out newSize); 

			if (newSize > oldSize)
				for (int i = 0; i < newSize - oldSize; i++) {

					SerializedFact fact = new SerializedFact ("New Fact");
					fact.values.Add (GameObject.FindObjectOfType<GameObject>().name);

					agent.serializedKnowledge.Add(fact);
				}

			else if (newSize < oldSize)
				for (int i = oldSize - 1; i > (oldSize - (oldSize - newSize) -1); i--)
					agent.serializedKnowledge.Remove(agent.serializedKnowledge[i]);

			EditorGUILayout.EndHorizontal();

			if (agent.serializedKnowledge.Count > 0)
				for(int i=0; i<newSize; i++){

					SerializedFact fact = agent.serializedKnowledge[i];

					EditorGUILayout.BeginHorizontal(style,GUILayout.Width(400));

						//Fact name

						EditorGUILayout.LabelField("Name", GUILayout.Width(60));

						List<string> names = new List<string> ();

						try {
							names = AtomsContainer.Load (AtomListWindow.path).list;
						}

						catch (Exception e){
							Debug.Log (e.Message);
						}


						if (names.Count > 0) {

							selectedName = names.IndexOf (fact.name);

							selectedName = EditorGUILayout.Popup (selectedName, names.ToArray (), GUILayout.Width (50));


							if (selectedName == -1)
								selectedName = 0;

							fact.name = names [selectedName];
						}

						else
							EditorUtility.DisplayDialog ("", "No names in atom names list. Please, fill the list before adding a definition","OK");	

						//Fact sign
						fact.sign = System.Convert.ToBoolean(EditorGUILayout.Popup(System.Convert.ToInt32(fact.sign),factSigns,GUILayout.Width(60)));

						//Fact type

						if (fact.values.Count == 1)
							selectedFactType = 0;

						else if (fact.values.Count == 2){
							selectedFactType = 1;
						}

						selectedFactType = EditorGUILayout.Popup(selectedFactType,factTypes,GUILayout.Width(40));

						if (selectedFactType == 1 && fact.values.Count == 1)
							fact.values.Add(GameObject.FindObjectOfType<GameObject>().name);

						if (selectedFactType == 0 && fact.values.Count == 2)
							fact.values.Remove(fact.values [1]);

						//Fact arguments
						
						GameObject found = GameObject.Find (fact.values [0]);

						fact.values[0] = ((GameObject)EditorGUILayout.ObjectField (found, typeof(GameObject), true, GUILayout.Width (80))).name;

						if (fact.values.Count == 2) {

							found = GameObject.Find (fact.values [1]);

							fact.values [1] = ((GameObject)EditorGUILayout.ObjectField (found, typeof(GameObject), true, GUILayout.Width (80))).name;
							
						}

					EditorGUILayout.EndHorizontal();

					agent.serializedKnowledge[i] = fact;

				}

		}

		//Joint plan 
		if (agent.agentType == AgentTypes.LEADER) {

			foldout3 = EditorGUILayout.Foldout (foldout3, "Joint plan");

			if (foldout3){
			GUIStyle style = new GUIStyle();
			
			style.alignment = (TextAnchor)TextAlignment.Left;
			
			
			EditorGUILayout.BeginHorizontal(style,GUILayout.Width(400));
			
			EditorGUILayout.LabelField("Size", GUILayout.Width(130));
			
			int oldSize = agent.groupMembers.Count;
			
			string size = EditorGUILayout.TextField(oldSize.ToString(),GUILayout.Width(300));
			
			
			int newSize = 0;
			
			System.Int32.TryParse(size, out newSize); 
			
			
			if (newSize > oldSize)
				
				for (int i=0; i < newSize - oldSize; i++){


					HTNAgent[] agents = GameObject.FindObjectsOfType<HTNAgent>();

					foreach (HTNAgent a in agents)
						if (a.agentType == AgentTypes.MEMBER)
							agent.groupMembers.Add(a);
					
				}
			else if (newSize < oldSize)
				for (int i = oldSize - 1; i > (oldSize - (oldSize - newSize) -1); i--){
					agent.groupMembers.Remove(agent.groupMembers[i]);
				}
			
			EditorGUILayout.EndHorizontal();
			
			
			//VIEW 
			
			
			if (agent.groupMembers.Count > 0)
				for(int i=0; i<newSize; i++){
					
					
					HTNAgent member = agent.groupMembers[i];
					
					
					EditorGUILayout.BeginHorizontal(style,GUILayout.Width(400));
					
					//Agent
					EditorGUILayout.LabelField("Agent", GUILayout.Width(130));

					
					member = (HTNAgent)EditorGUILayout.ObjectField(member, typeof(HTNAgent) ,GUILayout.Width(100));
					
					
					//Goal
					if(GUILayout.Button("Edit Goal"))
					{
						HTNEditorWindow.ShowWindow(member,member.goal);
					}

					member.domainVariables = agent.domainVariables;
					member.serializedDomainVariables = agent.serializedDomainVariables;
					
					
					EditorGUILayout.EndHorizontal();

					if (member.agentType == AgentTypes.MEMBER)
						agent.groupMembers[i] = member;
					else 
						EditorUtility.DisplayDialog("INVALID ACTION","Can only add MEMBER type agents","OK");
					
				}

			}

		}



		//Domain variables
		foldout2 = EditorGUILayout.Foldout (foldout2, "Planning variables");
		
		if (foldout2) {
			
			//MODEL
			
			
			GUIStyle style = new GUIStyle();
			
			style.alignment = (TextAnchor)TextAlignment.Left;
			
			
			EditorGUILayout.BeginHorizontal(style,GUILayout.Width(400));
			
				EditorGUILayout.LabelField("Size", GUILayout.Width(130));
				
				int oldSize = agent.domainVariables.Count;
				
				string size = EditorGUILayout.TextField(oldSize.ToString(),GUILayout.Width(300));
				
				
				int newSize = 0;
				
				System.Int32.TryParse(size, out newSize); 
				
				
				if (newSize > oldSize)
					
					for (int i=0; i < newSize - oldSize; i++){
						
						agent.domainVariables.Add(new Term(""));
						agent.serializedDomainVariables.Add(new SerializedVariable(""));
				
					}
				else if (newSize < oldSize)
					for (int i = oldSize - 1; i > (oldSize - (oldSize - newSize) -1); i--){
						agent.domainVariables.Remove(agent.domainVariables[i]);
						agent.serializedDomainVariables.Remove(agent.serializedDomainVariables[i]);
					}
			
			EditorGUILayout.EndHorizontal();
			
			
			//VIEW 
			
			
			if (agent.domainVariables.Count > 0)
				for(int i=0; i<newSize; i++){
					
						
					SerializedVariable variable = agent.serializedDomainVariables[i];
						
						
					EditorGUILayout.BeginHorizontal(style,GUILayout.Width(400));
					
						//Name
						EditorGUILayout.LabelField("Name", GUILayout.Width(130));
						
						variable.key = EditorGUILayout.TextField(variable.key ,GUILayout.Width(100));
						
						
						//Value
						GameObject found = GameObject.Find (variable.value);
						
							
						GameObject obj = ((GameObject)EditorGUILayout.ObjectField (found, typeof(GameObject), true, GUILayout.Width (200)));

						if (obj == null)
							variable.value = "";
						else 
							variable.value = obj.name;

					EditorGUILayout.EndHorizontal();
					
					agent.serializedDomainVariables[i] = variable;
					agent.domainVariables[i].key = variable.key;

				}
			
			
			
		}


	}
}

