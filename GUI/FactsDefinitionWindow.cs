using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.IO;


public struct FactDefinition{

	[XmlAttribute("name")]
	public string name;
	[XmlAttribute("factType")]
	public string factType;
	[XmlAttribute("argsType")]
	public string argsType;

	[XmlAttribute("expression")]
	public string expression;



	public FactDefinition(string n){
		
		name = n;
		
		factType = "Property";
		
		argsType = "Position";
		
		expression = "";
		
	}

	public FactDefinition(string n, string f){
		
		name = n;
		
		factType = f;
		
		argsType = "Position";
		
		expression = "";
		
	}

	public FactDefinition(string n, string f, string a){
		
		name = n;
		
		factType = f;
		
		argsType = a;
		
		expression = "";
		
	}


	public FactDefinition(string n, string f, string a, string e){
		
		name = n;
		
		factType = f;
		
		argsType = a;
		
		expression = e;
		
	}
}

[XmlRoot("FactsDefinitionsContainer")]
public class FactDefinitionsContainer{




	[XmlArray("Definitions")]
	[XmlArrayItem("FactDefinition")]
	public List<FactDefinition> list = new List<FactDefinition> ();

	public FactDefinitionsContainer() {


	
	}

	public void Add(FactDefinition definition){

		list.Add (definition);
	}

	public void Remove(FactDefinition definition){

		list.Remove (definition);
	}

	public void Save(string path)
	{

		Directory.CreateDirectory(Path.GetDirectoryName(path));

		var serializer = new XmlSerializer(typeof(FactDefinitionsContainer));

		try {
			using(var stream = new FileStream(path, FileMode.Create))
			{
				serializer.Serialize(stream, this);
			}
		}

		catch (FileNotFoundException e){

			Debug.Log (e.Message);
		}
	}


	public bool Contains(FactDefinition definition){

		return list.Contains (definition);
	}


	public static FactDefinitionsContainer Load(string path)
	{

		Directory.CreateDirectory(Path.GetDirectoryName(path));

		var serializer = new XmlSerializer(typeof(FactDefinitionsContainer));

		try {
			using(var stream = new FileStream(path, FileMode.Open))
			{
				return serializer.Deserialize(stream) as FactDefinitionsContainer;
			}
		}

		catch (FileNotFoundException e){

			Debug.Log (e.Message);

			return new FactDefinitionsContainer();
		}
	}
}

[XmlRoot("ArgumentTypes")]
public class ArgumentTypes{

	[XmlArray("types")]
	[XmlArrayItem("string")]
	public List<string> list = new List<string> ();

	public ArgumentTypes() {

	}
	
	public void Add(string arg){
		
		list.Add (arg);
	}
	
	public void Remove(string arg){
		
		list.Remove (arg);
	}
	
	public void Save(string path)
	{

		Directory.CreateDirectory(Path.GetDirectoryName(path));

		var serializer = new XmlSerializer(typeof(ArgumentTypes));
		
		try {
			using(var stream = new FileStream(path, FileMode.Create))
			{
				serializer.Serialize(stream, this);
			}
		}
		
		catch (FileNotFoundException e){
			
			Debug.Log (e.Message);
		}
	}

	public bool Contains(string arg){
		
		return list.Contains (arg);
	}
	
	
	public static ArgumentTypes Load(string path)
	{


		Directory.CreateDirectory(Path.GetDirectoryName(path));

		var serializer = new XmlSerializer(typeof(ArgumentTypes));
		
		try {
			using(var stream = new FileStream(path, FileMode.Open))
			{
				return serializer.Deserialize(stream) as ArgumentTypes;
			}
		}
		
		catch (FileNotFoundException e){
			
			Debug.Log (e.Message);
			
			return new ArgumentTypes();
		}
	}

}




public class FactsDefinitionWindow : EditorWindow
{

	static FactDefinitionsContainer definitions = new FactDefinitionsContainer();

	//string personalityPaths = "Assets/" + Application.loadedLevelName + "/personalities.xml";
	public static string definitionsPath = "Assets/" + Application.loadedLevelName + "/definitions.xml";
	public static string argumentsPath = "Assets/" + Application.loadedLevelName + "/argumentTypes.xml";

	int selectedFactType = 0;
	int selectedArgType = 0;
	int selectedName = 0;
	string[] factTypes = {"Property","Relation"};
	[XmlArray("ArgumentTypes")]
	[XmlArrayItem("string")]
	public static ArgumentTypes argsTypes = new ArgumentTypes();

	public static void  ShowWindow(){
	

		EditorWindow window = EditorWindow.GetWindow(typeof(FactsDefinitionWindow));
		
		window.position = new Rect (200, 200,1000, 700);


		definitions = FactDefinitionsContainer.Load (definitionsPath);

		/*List<FactDefinition> defaultDefinitions = new List<FactDefinition> (new FactDefinition[] {
							new FactDefinition ("Equal", "Relation", "Self", "arg1.Equals(arg2)"),
							new FactDefinition ("NotEqual", "Relation", "Self", "!arg1.Equals(arg2)")});
		
		
		foreach (FactDefinition fd in defaultDefinitions)
			if (!definitions.Contains (fd))
				definitions.Add (fd);*/




		argsTypes = ArgumentTypes.Load (argumentsPath);

		List<string> defaultArgsTypes = new List<string> (new string[] {
			"Self",
			"Position",
			"Rotation",
			"Scale",
			"Mass"
		});


		foreach (string argType in defaultArgsTypes)
			if (!argsTypes.Contains (argType))
				argsTypes.Add (argType);

	}
	

	void OnGUI (){





		//VIEW 
			for(int i=0; i<definitions.list.Count; i++){

				FactDefinition fd = definitions.list[i];

				GUILayout.BeginHorizontal (GUILayout.Width(200));

					//Name

					GUILayout.Label ("Name", GUILayout.Width(40));

					List<string> names = new List<string> ();

					try {
						names = AtomsContainer.Load (AtomListWindow.path).list;
					}

					catch (Exception e){
						Debug.Log (e.Message);
					}




					selectedName = names.IndexOf (fd.name);

					selectedName = EditorGUILayout.Popup(selectedName,names.ToArray(),GUILayout.Width(50));

					if (selectedName == -1)
						selectedName = 0;
			
					fd.name = names [selectedName];


					
					//Type
					GUILayout.Label("Type");

					if (fd.factType == "Property")
						selectedFactType = 0;

					else if (fd.factType == "Relation")
						selectedFactType = 1;

					selectedFactType = EditorGUILayout.Popup(selectedFactType,factTypes,GUILayout.Width(100));
					fd.factType = factTypes[selectedFactType];

					GUILayout.Space(20);


					//Arguments of expression
					GUILayout.Label("Arguments");
					
					int j;
					
					for (j=0; j< argsTypes.list.Count; j++)
						if (fd.argsType == argsTypes.list[j]){
							selectedArgType = j;
							break;
						}
			
			

					selectedArgType = EditorGUILayout.Popup(selectedArgType,argsTypes.list.ToArray(),GUILayout.Width(100));
					fd.argsType = argsTypes.list[selectedArgType];

					//MODEL/CONTROLLER
					if (GUILayout.Button ("Edit",GUILayout.Width(50))){
						AddArgumentWindow.ShowWindow();
					}

						
					GUILayout.Space(20);


					//Expression
					GUILayout.Label("Expression");
					fd.expression= EditorGUILayout.TextField(fd.expression,GUILayout.Width(300));
	

					definitions.list[i] = fd;

					
					
					GUILayout.Space(20);
			
					if (GUILayout.Button("Delete",GUILayout.Width(100)))
						definitions.Remove(definitions.list[i]);

				GUILayout.EndHorizontal ();
			}

		
			//MODEL/CONTROLLER
			if (GUILayout.Button ("Add Definition",GUILayout.Width(200))){

				if (AtomListWindow.atomNames.list.Count > 0)
					definitions.Add (new FactDefinition ("New Fact"));
				else
					EditorUtility.DisplayDialog ("", "No names in atom names list. Please, fill the list before adding a definition","OK");	
			}

			
		



	}

	void OnDestroy(){

		definitions.Save(definitionsPath);
		argsTypes.Save (argumentsPath);


	}
}

