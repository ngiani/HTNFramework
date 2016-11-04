using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using GameDevWare.Dynamic.Expressions.CSharp;



//Camera sensor
public abstract class Sensor {
	
	
	protected HTNAgent agent;

	
	static Type GetType( string TypeName )
	{
		
		// Try Type.GetType() first. This will work with types defined
		// by the Mono runtime, etc.
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
	
	
	
	//Sense fact from object's tag. In this case is a property(one argument: the object itself)
	public void SenseFactFromTag(GameObject obj){
		
		
		if (obj.tag != "Untagged") {
			string factName = obj.tag;
			
			
			Atom existingFact = agent.state.facts.Find((Atom fact) => fact.name == factName && fact.terms.Find((Term Term) => Term.value.Equals(obj))!=null);
			
			if (existingFact==null){
				
				Atom newFact = new Atom (factName);
				
				newFact.addTerm(new Term(obj));
				
				agent.state.addFact(newFact);
			}
		}
	}
	
	private static object GetField(object inObj, string fieldName)
	{
		object ret = null;
		FieldInfo info = inObj.GetType().GetField(fieldName);
		if (info != null)
			ret = info.GetValue(inObj);
		return ret;
	}
	
	
	//Sense and check facts about object from user definitions
	public void SenseFactFromDefinition(GameObject obj, List<GameObject> objs, FactDefinition definition){
		
		
		string factName = definition.name;
		
		bool pairSense = false; //whether the object should be compared with other object to sense the fact 
		
		switch (definition.factType){
			
			case "Property" : pairSense = false; break;
			case "Relation" : pairSense = true; break;
		}
		
		//If it is a relation, fact must be evaluated along with every other object in the scene
		if (pairSense) {

			
			foreach (GameObject obj2 in objs) {

			  if (!obj2.Equals(obj)){
					
				bool evaluationResult = false;
				
				
					Atom existingFact = agent.state.facts.Find((Atom fact) => fact.name == factName && fact.terms[0].value.Equals(obj) && fact.terms[1].value.Equals(obj2));  
					
					
					switch (definition.argsType) {
						
					case "Position":
						try {

							evaluationResult = CSharpExpression.Evaluate<Vector3,Vector3,bool> (definition.expression, obj.transform.position, obj2.transform.position, "arg1", "arg2", null);

						} catch (Exception e) {
							UnityEngine.Debug.Log (e.Message);
						}
						break;
						
					case "Rotation":
						try {
							evaluationResult = CSharpExpression.Evaluate<Vector3,Vector3,bool> (definition.expression, obj.transform.rotation.eulerAngles, obj2.transform.rotation.eulerAngles, "arg1", "arg2", null);
						} catch (Exception e) {
							UnityEngine.Debug.Log (e.Message);
						}
						break;
						
					case "Scale": 
						try {
							evaluationResult = CSharpExpression.Evaluate<Vector3,Vector3,bool> (definition.expression, obj.transform.localScale, obj2.transform.localScale, "arg1", "arg2", null);
						} catch (Exception e) {
							UnityEngine.Debug.Log (e.Message);
						}
						
						break;
						
					case "Mass": 
						try {
							
							Rigidbody rigidBody = obj.GetComponent<Rigidbody> ();
							Rigidbody rigidBody2 = obj2.GetComponent<Rigidbody> ();
							
							if (rigidBody != null && rigidBody2 != null)
								evaluationResult = CSharpExpression.Evaluate<float,float,bool> (definition.expression, rigidBody.mass, rigidBody2.mass, "arg1", "arg2", null);
						} catch (Exception e) {
							UnityEngine.Debug.Log (e.Message);
						}
						
						break;
						
					case "Self":
						
						try {
							
							evaluationResult = CSharpExpression.Evaluate<GameObject,GameObject,bool> (definition.expression, obj, obj2, "arg1", "arg2", null);
						} catch (Exception e) {
							
							UnityEngine.Debug.Log (e.Message);
						}
						
						break;
						
					default :
						
						
						try {
							
							//Get custom argument info
							string[] customArgument = definition.argsType.Split('/');
							
							string componentName = customArgument[0];
							
							string fieldName = customArgument[1];
							
							//Get arguments from component
							Component component = obj.GetComponent(GetType(componentName));
							Component component2 = obj2.GetComponent(GetType(componentName));
							
							if (component!=null){
								
								Type t;
								object arg1;
								object arg2;
								
								FieldInfo field = component.GetType().GetField(fieldName);
								
								if (field == null){
									
									PropertyInfo property = component.GetType().GetProperty(fieldName);
									
									arg1 = property.GetValue(component,null);
									
									t = property.PropertyType;
								}
								
								else {
									
									
									arg1 = field.GetValue(component);
									
									t = field.FieldType;
									
								}
								
								
								FieldInfo field2 = component2.GetType().GetField(fieldName);
								
								if (field2 == null){
									
									PropertyInfo property = component2.GetType().GetProperty(fieldName);
									
									arg2 = property.GetValue(component2,null);
									
								}
								
								else {
									
									
									arg2 = field.GetValue(component2);
									
								}
								
								
								
								if( t == typeof(string))
									evaluationResult = CSharpExpression.Evaluate<string,string,bool>(definition.expression, (string)arg1,(string)arg2 , "arg", null);
								
								else if (t == typeof(int))
									evaluationResult = CSharpExpression.Evaluate<int,int,bool>(definition.expression, (int)arg1,(int)arg2 , "arg", null);
								
								else if (t == typeof(float))
									evaluationResult = CSharpExpression.Evaluate<float,float,bool>(definition.expression, (float)arg1, (float)arg2 , "arg", null);
								
								else if (t == typeof(double))
									evaluationResult = CSharpExpression.Evaluate<double,double,bool>(definition.expression, (double)arg1, (double)arg2 , "arg", null);
								
								else if (t == typeof(bool))
									evaluationResult = CSharpExpression.Evaluate<bool,bool,bool>(definition.expression, (bool)arg1, (bool) arg2 , "arg", null);
								
								else if (t == typeof(Vector2))
									evaluationResult = CSharpExpression.Evaluate<Vector2,Vector2,bool>(definition.expression, (Vector2)arg1, (Vector2)arg2 , "arg", null);
								
								else if (t == typeof(Vector3))
									evaluationResult = CSharpExpression.Evaluate<Vector3,Vector3,bool>(definition.expression, (Vector3)arg1, (Vector3) arg2 , "arg", null);
								
								else if (t == typeof(Color))
									evaluationResult = CSharpExpression.Evaluate<Color,Color,bool>(definition.expression, (Color)arg1, (Color)arg2 , "arg", null);
							}
						} catch (Exception e) {
							UnityEngine.Debug.Log (e.Message);
						}

						break;
						
					}
					
					//If result of expression evaluation is positive, add a positive fact about the object(s) or change sign if it exists yet
					if (evaluationResult == true) {
						
						if (existingFact == null) {

							Atom newFact = new Atom (factName);
						
							newFact.addTerm (new Term(obj));
						
							newFact.addTerm (new Term(obj2));
						
							agent.state.addFact (newFact);	
						} else
							existingFact.sign = true;
					}
					
					//Otherwise, add a negative fact, or change sign if fact exists yet
					else {

						if (existingFact == null) {
						
							Atom newFact = new Atom (factName, false);
						
							newFact.addTerm (new Term(obj));
							
							newFact.addTerm (new Term(obj2));

						
							agent.state.addFact (newFact);
						} else
							existingFact.sign = false;
						
					}
					
					
				}
			  }
			
		} 
		
		
		//If it is a property, just check object data 
		else {
			
			
			bool evaluationResult = false;
			
			
			Atom existingFact = agent.state.facts.Find((Atom fact) => fact.name == factName && fact.terms[0].value.Equals(obj));
			
				
				switch (definition.argsType) {
					
				case "Position":
					try {
						evaluationResult = CSharpExpression.Evaluate<Vector3,bool> (definition.expression, obj.transform.position, "arg", null);
					} catch (Exception e) {
					UnityEngine.Debug.Log (e.Message);
					}
					break;
					
				case "Rotation":
					try {
						evaluationResult = CSharpExpression.Evaluate<Vector3,bool> (definition.expression, obj.transform.rotation.eulerAngles, "arg", null);
					} catch (Exception e) {
						UnityEngine.Debug.Log (e.Message);
					}
					break;
					
				case "Scale": 
					try {
						
						evaluationResult = CSharpExpression.Evaluate<Vector3,bool> (definition.expression, obj.transform.localScale, "arg", null);
					} catch (Exception e) {
						UnityEngine.Debug.Log (e.Message);
					}
					
					break;
					
				case "Mass": 
					try {
						
						
						Rigidbody rigidBody = obj.GetComponent<Rigidbody> ();
						
						if (rigidBody != null)
							evaluationResult = CSharpExpression.Evaluate<float,bool> (definition.expression, rigidBody.mass, "arg", null);
					} catch (Exception e) {
						UnityEngine.Debug.Log (e.Message);
					}
					
					break;
					
					
				default : 
					
					try {
						
						//Get custom argument info
						string[] customArgument = definition.argsType.Split('/');
						
						string componentName = customArgument[0];
						
						string fieldName = customArgument[1];
						
						//Get argument from component
						Component component = obj.GetComponent(GetType(componentName));
						
						if (component!=null){
							
							Type t;
							object arg;
							
							FieldInfo field = component.GetType().GetField(fieldName);
							
							if (field == null){
								
								PropertyInfo property = component.GetType().GetProperty(fieldName);
								
								arg = property.GetValue(component,null);
								
								t = property.PropertyType;
							}
							
							else {
								
								
								arg = field.GetValue(component);
								
								t = field.FieldType;
								
							}
							
							
							
							if( t == typeof(string))
								evaluationResult = CSharpExpression.Evaluate<string,bool>(definition.expression, (string)arg , "arg", null);
							
							else if (t == typeof(int))
								evaluationResult = CSharpExpression.Evaluate<int,bool>(definition.expression, (int)arg , "arg", null);
							
							else if (t == typeof(float))
								evaluationResult = CSharpExpression.Evaluate<float,bool>(definition.expression, (float)arg , "arg", null);
							
							else if (t == typeof(double))
								evaluationResult = CSharpExpression.Evaluate<double,bool>(definition.expression, (double)arg , "arg", null);
							
							else if (t == typeof(bool))
								evaluationResult = CSharpExpression.Evaluate<bool,bool>(definition.expression, (bool)arg , "arg", null);
							
							else if (t == typeof(Vector2))
								evaluationResult = CSharpExpression.Evaluate<Vector2,bool>(definition.expression, (Vector2)arg , "arg", null);
							
							else if (t == typeof(Vector3))
								evaluationResult = CSharpExpression.Evaluate<Vector3,bool>(definition.expression, (Vector3)arg , "arg", null);
							
							else if (t == typeof(Color))
								evaluationResult = CSharpExpression.Evaluate<Color,bool>(definition.expression, (Color)arg , "arg", null);
						}
					} catch (Exception e) {
						UnityEngine.Debug.Log (e.Message);
					}
					
					break;
				}
				
				//If result of expression evaluation is positive, add a positive fact about the object(s), or change sign to existing
				if (evaluationResult == true) {
					
					if (existingFact == null) { 
						Atom newFact = new Atom (factName);
							
						newFact.addTerm (new Term(obj));
							
					
						agent.state.addFact (newFact);
					} else
						existingFact.sign = true;
				}
				
				//Otherwise, add a negative fact, or change sign to existing
				else {
					
					if (existingFact == null) { 
						Atom newFact = new Atom (factName,false);

						newFact.addTerm (new Term(obj));


						agent.state.addFact (newFact);
					} else
						existingFact.sign = false;
					
					
				}
				
				
				
			}

		
	}

	
	//If the object is perceptible by the agent
	public abstract bool isPerceptible (GameObject obj);
	
	
	//Objects perceptibles by the agent
	public List<GameObject> getPerceptibles(){

		List<GameObject> perceptibles = new List<GameObject>();

		foreach (GameObject obj in GameObject.FindObjectsOfType(typeof(GameObject))) {
			
			if (obj.layer != LayerMask.NameToLayer("HTNObjects"))
				continue;

			if (obj.Equals(agent.gameObject) || isPerceptible(obj))
				perceptibles.Add(obj);
		}

		return perceptibles;

	}
	

}




public class CameraSensor : Sensor {

	Camera camera;
	
	public CameraSensor(HTNAgent a) {
		
		agent = a;

		
		//Search for camera
		camera = agent.gameObject.GetComponentInChildren<Camera> ();



		if (camera == null) {

			UnityEngine.Debug.Log ("No camera was found in agent ");

		}



	}


	public override bool isPerceptible(GameObject obj){
		
		//Renderer renderer = new Renderer();
		Collider collider = new Collider();
		
		
		/*renderer = obj.GetComponent<Renderer>();
		
		
		if (renderer == null) {
			
			Debug.Log ("No renderer was found in object " + obj.name + "...default one was added");
			
			obj.AddComponent<MeshRenderer>();

			renderer = obj.GetComponentInChildren<MeshRenderer>();
			renderer.enabled = true;
			
		}*/

		collider = obj.GetComponent<Collider>();
	
	
		if (collider == null) {
			
			UnityEngine.Debug.Log ("No collider was found in object " + obj.name + "...default one was added");
			
			obj.AddComponent<BoxCollider>();
			
			obj.GetComponent<BoxCollider>().isTrigger = true;

			collider = obj.GetComponent<BoxCollider>();
			
			
		}


		if (GeometryUtility.TestPlanesAABB(GeometryUtility.CalculateFrustumPlanes(camera), collider.bounds)) 
			return true;

		
		
		return false;
	}

}

public class RadiusSensor : Sensor {

	float radius;
	
	public RadiusSensor(HTNAgent a, float r) {
		
		agent = a;

		radius = r;
		
	}
	
	
	public override bool isPerceptible(GameObject obj){

		bool result = false;

		if (obj != null) {
			float distance = Vector3.Distance (agent.gameObject.transform.position, obj.transform.position);
			result = distance <= radius;
		}
	

		return result;
	}
	
}


