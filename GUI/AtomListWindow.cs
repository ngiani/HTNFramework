using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.IO;



[XmlRoot("AtomsContainer")]
public class AtomsContainer{

	[XmlArray("Atoms")]
	[XmlArrayItem("Atom")]
	public List<string> list;

	public AtomsContainer() {

		list = new List<string> ();
	}

	public void Add(string name){

		list.Add (name);
	}

	public void Remove(string name){

		list.Remove (name);
	}

	public void Save(string path)
	{

		Directory.CreateDirectory(Path.GetDirectoryName(path));

		var serializer = new XmlSerializer(typeof(AtomsContainer));

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


	public static AtomsContainer Load(string path)
	{

		Directory.CreateDirectory(Path.GetDirectoryName(path));

		var serializer = new XmlSerializer(typeof(AtomsContainer));

		try {
			using(var stream = new FileStream(path, FileMode.Open))
			{
				return serializer.Deserialize(stream) as AtomsContainer;
			}
		}

		catch (FileNotFoundException e){

			Debug.Log (e.Message);

			return new AtomsContainer();
		}
	}
}

public class AtomListWindow : EditorWindow
{

	public static string path = "Assets/" + Application.loadedLevelName + "/atomNames.xml";
	public static AtomsContainer atomNames = AtomsContainer.Load (path);

	public static void  ShowWindow () {


		EditorWindow window = EditorWindow.GetWindow(typeof(AtomListWindow));

		window.position = new Rect (200, 200,200, 250);



	}


	void OnGUI(){

		GUILayout.BeginArea(new Rect(0,0,200,250));

		if (atomNames.list.Count > 0)
			for(int i=0; i < atomNames.list.Count; i++) {

				string atomName = atomNames.list[i];

				if (atomName!="none"){
					GUILayout.BeginHorizontal(GUILayout.Width(100));

					GUILayout.Label ("Name");


					atomName = GUILayout.TextField(atomName,GUILayout.Width(75));

					atomNames.list[i] = atomName;


					if (GUILayout.Button("Delete",GUILayout.Width(75)))
						atomNames.Remove(atomNames.list[i]);

					GUILayout.EndHorizontal();

					GUILayout.Space(10.0f);

				}



			}

		if (GUILayout.Button("Add Atom",GUILayout.Width(200)))
			atomNames.Add("New Atom");

		GUILayout.EndArea();


	}

	void OnDestroy(){

		atomNames.Save (path);

	}

}

