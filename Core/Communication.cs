using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public enum MessageContent {PLAN_ORDER, ORDER_FAILURE};

public struct Message
{

	public HTNAgent receiver;

	public MessageContent content;
	public List<object> data;

	public Message(HTNAgent r, MessageContent c){
		
		receiver = r;
		content = c;
		data = new List<object>();
	}

	public Message(HTNAgent r, MessageContent c, List<object> d){

		receiver = r;
		content = c;
		data = d;
	}
}


public class CommunicationSystem {

	public Queue<Message> messages;
	public HTNAgent agent;

	public CommunicationSystem(HTNAgent a){

		agent = a;
		messages = new Queue<Message> ();

	}

	public void sendMessage(Message message){

		message.receiver.communicationSystem.addMessage (message);
		
	}

	public void readMessage(Message message){

		/*if (message.content == MessageContent.PLAN_ORDER) {

			CompoundTask goal = (CompoundTask)message.data[0];

			agent.goal = goal;

			agent.FSM.current = AgentState.PLANNING;
			agent.actionManager.NotValid();
		}*/

		if (message.content == MessageContent.ORDER_FAILURE) {
			
			agent.FSM.current = AgentState.IDLE;

			foreach (HTNAgent member in agent.groupMembers)
				member.FSM.current = AgentState.IDLE;
		}



	}

	public void readMessages(){

		if (messages.Count > 0)
			readMessage (messages.Dequeue ());
	}

	public void addMessage(Message message){


		messages.Enqueue (message);
	}

}

