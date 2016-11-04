using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;





[Serializable]
//State as collection of facts
public class State {

	public List<Atom> facts;

	public State(){

		facts = new List<Atom> ();
	}


	public void addFact(Atom f){

		facts.Add (f);
	}

	public void removeFact(Atom f) {

		facts.Remove (f);
	}

	public Atom Contains(Atom newFact){

		foreach(Atom fact in facts)
			if (fact.name == newFact.name && fact.sign == newFact.sign){

				int i=0;

				foreach(Term term in fact.terms){
			

					if (!newFact.terms[i].value.Equals(term.value)) 
						break;
					i++;
				}

				if (i==fact.terms.Count)
					return fact;

		}

		return null;
	}
}


[Serializable]
//Atom with variables
public class Atom {


	public string name;

	public bool sign;

	[SerializeField]
	public List<Term> terms;

	public Atom(){

		name = "New Atom";
		sign = true;
		terms = new List<Term> ();

	}

	public Atom(string n){

		name = n;
		sign = true;
		terms = new List<Term> ();
	}

	public Atom(string n, bool s){
		name = n;
		sign = s;
		terms = new List<Term> ();

	}

	public void addTerm( Term term){

		terms.Add (term);
	}

	public void removeTerm( Term term){

		terms.Remove (term);
	}

	public bool isVerified(State state){
		

		foreach (Atom fact in state.facts)
			if (fact.name == name && fact.sign == sign) {
				
				int i = 0;

				foreach (Term term in fact.terms){
					if (!term.value.Equals(terms[i].value))
						break;
					i++;
				}

				if (i == fact.terms.Count)
					return true;
			}


		return false;
	}


}



[Serializable]
public class Term{

	public string key;
	public GameObject value;
	
	public Term(string n){
		
		key = n;
		
		value = null;
		
	}

	public Term(GameObject v){
		
		key = "";
		
		value = v;
	}
	
	public Term(string n, GameObject v){
		
		key = n;
		
		value = v;
	}


	
	
	
}