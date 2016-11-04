/*
	Copyright (c) 2016 Denis Zykov, GameDevWare.com

	https://www.assetstore.unity3d.com/#!/content/56706
	
	This source code is distributed via Unity Asset Store, 
	to use it in your project you should accept Terms of Service and EULA 
	https://unity3d.com/ru/legal/as_terms
*/

using System;

namespace GameDevWare.Dynamic.Expressions
{
	[AttributeUsage(AttributeTargets.Class)]
	public sealed class ExpressionKnownTypeAttribute : Attribute
	{
		public Type Type { get; private set; }

		public ExpressionKnownTypeAttribute(Type type)
		{
			if(type == null) throw new ArgumentNullException("type");

			this.Type = type;
		}
	}
}
