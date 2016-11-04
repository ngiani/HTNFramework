using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

public class ConditionEditorWindow : EditorWindow
{

	static Atom atom;
	static HTNAgent agent;
	static int selectedSign = 0;
	static int selectedName = 0;
	static int varIndex = 0;

	public static void  ShowWindow (HTNAgent a, Atom at) {

		atom = at;
		agent = a;

		EditorWindow window = EditorWindow.GetWindow(typeof(ConditionEditorWindow));
		
		window.position = new Rect (200, 200,220, 250);

	
	}

	void OnGUI(){

		//Name

		GUILayout.Label ("Name", GUILayout.Width(100));

		List<string> names = new List<string> ();

		try {
			names = AtomsContainer.Load (AtomListWindow.path).list;
		}

		catch (Exception e){
			Debug.Log (e.Message);
		}



		if (names.Count > 0) {

			selectedName = names.IndexOf (atom.name);

			selectedName = EditorGUILayout.Popup (selectedName, names.ToArray (), GUILayout.Width (100));

			if (selectedName == -1)
				selectedName = 0;

			atom.name = names [selectedName];
		}

		else
			EditorUtility.DisplayDialog ("", "No names in atom names list. Please, fill the list before adding a condition","OK");	


		//Sign
		switch (atom.sign) {
			
		case true:
			selectedSign = 0;
			break;
		case false:
			selectedSign = 1;
			break;

		}

		GUILayout.Space (20.0f);


		GUILayout.Label ("Sign", GUILayout.Width(100));

		string[] options2 = {"true", "false"};

		selectedSign = EditorGUILayout.Popup(selectedSign,options2,GUILayout.Width(100));

		if (selectedSign == 0)
			atom.sign = true;
		else if (selectedSign == 1)
			atom.sign = false;



		GUILayout.Space (20.0f);
		
		
		GUILayout.Label ("Variables", GUILayout.Width(100));



		if (atom.terms.Count > 0) {



			for (int i=0; i<atom.terms.Count; i++) {

				Term variable = (Term)atom.terms[i];


				GUILayout.BeginHorizontal (GUILayout.Width (200));
				GUILayout.Label ("Name");
					


				List<string> domVarNamesList = new List<string> ();




				foreach (Term v in agent.domainVariables)
					domVarNamesList.Add (v.key);


				foreach (Term v in agent.domainVariables){



					if (v.key == variable.key) {
						varIndex = agent.domainVariables.IndexOf (v);
						break;
					}

				}
				

				string[] DomvarNames = domVarNamesList.ToArray ();
					

				varIndex = EditorGUILayout.Popup (varIndex, DomvarNames);


				string newVar =  agent.domainVariables [varIndex].key;




				GUILayout.Space (10.0f);

				if ( agent.domainVariables [varIndex].key != variable.key) {
					atom.removeTerm(variable);

					atom.addTerm(new Term(newVar));

				}

				if (GUILayout.Button ("Delete", GUILayout.Width (100))) {
					atom.removeTerm(variable);

				}

				GUILayout.EndHorizontal ();
			}

		}

		if (GUILayout.Button ("Add Variable", GUILayout.Width (200))) {


			if (agent.domainVariables.Count > 0) {
				for (int i = 0; i < agent.domainVariables.Count; i++)
					if (!atom.terms.Contains (agent.domainVariables [i])) {
						

						atom.addTerm (new Term (agent.domainVariables [i].key, null));


						break;
					} 
					
					
			} else
				EditorUtility.DisplayDialog ("", "No variables in planning variables list", "OK");
		
		
		}
	}

}

