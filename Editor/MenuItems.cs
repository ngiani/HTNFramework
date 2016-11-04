using UnityEngine;
using UnityEditor;
using System.Collections;

public class MenuItems
{
	

	[MenuItem("HTN Framework/Define Facts")]
	private static void NewMenuOption()
	{
		FactsDefinitionWindow.ShowWindow ();
	}

	[MenuItem("HTN Framework/NPC Personalities")]
	private static void NewMenuOption2()
	{
		PersonalityEditorWindow.ShowWindow ();
	}

	[MenuItem("HTN Framework/Knowledge update")]
	private static void NewMenuOption3()
	{
		KnowledgeUpdateWindow.ShowWindow ();
	}

	[MenuItem("HTN Framework/Atom names")]
	private static void NewMenuOption4()
	{
		AtomListWindow.ShowWindow ();
	}

}

