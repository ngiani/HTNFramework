using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;


public class AddArgumentWindow : EditorWindow
{

	static int selectedComponent;
	static int selectedField;
	static FactDefinition definition;

	public static void  ShowWindow(){
		
		EditorWindow window = EditorWindow.GetWindow(typeof(AddArgumentWindow));
		
		window.position = new Rect (200, 200,550, 125);

		
	}

	public static Type GetType( string TypeName )
	{

		var type = Type.GetType( TypeName );
		
		// If it worked, then we're done here
		if( type != null )
			return type;
		
		// Get the name of the assembly (Assumption is that we are using
		// fully-qualified type names)
		var assemblyName = TypeName.Substring( 0, TypeName.IndexOf( '.' ) );
		
		// Attempt to load the indicated Assembly
		var assembly = Assembly.Load( assemblyName );
		if( assembly == null )
			return null;
		
		// Ask that assembly to return the proper Type
		return assembly.GetType( TypeName );
		
	}

	// Use this for initialization
	void OnGUI()
	{

		GUIStyle style = new GUIStyle ();

		style.fontSize = 12;
		style.padding = new RectOffset(175,0,0,0);

		EditorGUILayout.LabelField("Create new argument", style,GUILayout.Width (200));

		GUILayout.Space (40);
		


		GameObject[] objects = GameObject.FindObjectsOfType<GameObject>();

		//Get existing component in the scene
		List<string> components = new List<string> ();

		for (int i=0; i < objects.Length; i++){

			Component[] objectComponents = objects[i].GetComponents(typeof(Component));


			for (int j = 0; j < objectComponents.Length; j++)
				if (!components.Contains(objectComponents[j].GetType().ToString()))
					components.Add(objectComponents[j].GetType().ToString());

		}

		//Display components in pop-up menu
		
		GUILayout.BeginHorizontal (GUILayout.Width(300),GUILayout.Height(100));
			GUILayout.Label ("Component", GUILayout.Width (75));


			selectedComponent = EditorGUILayout.Popup (selectedComponent, components.ToArray (),GUILayout.Width(100));
		

			GUILayout.Space (20);



			//Get existing fields and properties in the selected component
			List<string> fields = new List<string> ();

		
			Type selectedType = GetType(components [selectedComponent]);


			System.Reflection.FieldInfo[] fieldInfo = selectedType.GetFields();

			for (int z = 0; z < fieldInfo.Length; z ++)
				if (fieldInfo[z].FieldType == typeof(string) || fieldInfo[z].FieldType == typeof(int) || fieldInfo[z].FieldType == typeof(bool) ||
				    fieldInfo[z].FieldType== typeof(float) || fieldInfo[z].FieldType == typeof(double) || 
			    	fieldInfo[z].FieldType == typeof(Vector2) || fieldInfo[z].FieldType == typeof(Vector3) || fieldInfo[z].FieldType == typeof(Color))
				fields.Add (fieldInfo [z].Name);

			System.Reflection.PropertyInfo[] propertyInfo = selectedType.GetProperties ();

			for (int z = 0; z < propertyInfo.Length; z ++)
				if ( propertyInfo[z].PropertyType == typeof(string) || propertyInfo[z].PropertyType == typeof(int) || propertyInfo[z].PropertyType == typeof(bool) ||
				    propertyInfo[z].PropertyType== typeof(float) || propertyInfo[z].PropertyType == typeof(double) || 
				    propertyInfo[z].PropertyType == typeof(Vector2) || propertyInfo[z].PropertyType == typeof(Vector3) || propertyInfo[z].PropertyType == typeof(Color))
				fields.Add (propertyInfo [z].Name);
		

			//Display fields in pop-up menu
			
			GUILayout.Label ("Fields", GUILayout.Width (75));

			selectedField = EditorGUILayout.Popup (selectedField, fields.ToArray (),GUILayout.Width(150));



		//GUILayout.BeginVertical (GUILayout.Width (200));

			//Add new argument on button click
			if (GUILayout.Button ("Add argument")) {
				FactsDefinitionWindow.argsTypes.Add (components [selectedComponent] + "/" + fields [selectedField]);
				EditorUtility.DisplayDialog("","New argument addedd","OK");
			}

		//GUILayout.EndVertical ();

		GUILayout.EndHorizontal ();
	}



}

