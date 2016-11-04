using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.IO;



[XmlRoot("PersonalitiesContainer")]
public class PersonalitiesContainer{
	
	[XmlArray("Personalities")]
	[XmlArrayItem("Personality")]
	public List<string> list;
	
	public PersonalitiesContainer() {
		
		list = new List<string> ();
	}
	
	public void Add(string personality){
		
		list.Add (personality);
	}
	
	public void Remove(string personality){
		
		list.Remove (personality);
	}
	
	public void Save(string path)
	{

		Directory.CreateDirectory(Path.GetDirectoryName(path));

		var serializer = new XmlSerializer(typeof(PersonalitiesContainer));
		
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
	
	
	public static PersonalitiesContainer Load(string path)
	{

		Directory.CreateDirectory(Path.GetDirectoryName(path));

		var serializer = new XmlSerializer(typeof(PersonalitiesContainer));
		
		try {
			using(var stream = new FileStream(path, FileMode.Open))
			{
				return serializer.Deserialize(stream) as PersonalitiesContainer;
			}
		}
		
		catch (FileNotFoundException e){
			
			Debug.Log (e.Message);
			
			return new PersonalitiesContainer();
		}
	}
}

public class PersonalityEditorWindow : EditorWindow
{
	
	public static string path = "Assets/" + Application.loadedLevelName + "/personalities.xml";
	public static PersonalitiesContainer personalities = PersonalitiesContainer.Load (path);

	public static void  ShowWindow () {
		

		EditorWindow window = EditorWindow.GetWindow(typeof(PersonalityEditorWindow));
		
		window.position = new Rect (200, 200,200, 250);


		
	}


	void OnGUI(){

		GUILayout.BeginArea(new Rect(0,0,200,250));

		if (personalities.list.Count > 0)
			for(int i=0; i < personalities.list.Count; i++) {

				string personality = personalities.list[i];
					
					if (personality!="none"){
						GUILayout.BeginHorizontal(GUILayout.Width(100));

							GUILayout.Label ("Name");


							personality = GUILayout.TextField(personality,GUILayout.Width(75));
							personalities.list[i] = personality;

					
							if (GUILayout.Button("Delete",GUILayout.Width(75)))
								personalities.Remove(personalities.list[i]);

						GUILayout.EndHorizontal();

						GUILayout.Space(10.0f);

					}



				}

		if (GUILayout.Button("Add Personality",GUILayout.Width(200)))
			personalities.Add("New Personality");

		GUILayout.EndArea();
	

	}

	void OnDestroy(){
		
		personalities.Save (path);
		
	}

}


