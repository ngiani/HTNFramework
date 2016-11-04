using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

public class KnowledgeUpdateWindow : EditorWindow
{


	public static float updateRate;
	static string path = "Assets/" + Application.loadedLevelName + "/updateRate.xml";


	public static void ShowWindow(){
		

		EditorWindow window = EditorWindow.GetWindow(typeof(KnowledgeUpdateWindow));
		
		window.position = new Rect (200, 200,260, 50);


	}

	void OnGUI() {

		GUI.Label (new Rect (10, 25, 70, 25),"Update rate: ");
		updateRate = GUI.HorizontalSlider(new Rect(90, 25, 100, 30), updateRate, 60.0F, 120.0F);
		GUI.Label (new Rect (200, 25, 70, 25),((int)updateRate).ToString() + " frames");
	}

	void OnDestroy(){


		var serializer = new XmlSerializer(typeof(float));



		try {

			Directory.CreateDirectory(Path.GetDirectoryName(path));

			using(var stream = new FileStream(path, FileMode.Create))
			{
				serializer.Serialize(stream, updateRate);
			}
		}
		
		catch (FileNotFoundException e){
			
			Debug.Log (e.Message);
		}

	}


	public static void LoadFromFile(){

		Directory.CreateDirectory(Path.GetDirectoryName(path));


		var serializer = new XmlSerializer(typeof(float));
		
		
		try {
			using(var stream = new FileStream(path, FileMode.Open))
			{
				updateRate = (float)serializer.Deserialize(stream);
			}
		}
		
		catch (FileNotFoundException e){
			
			Debug.Log (e.Message);
			
			updateRate = 60.0f;
		}
		
	}
}

