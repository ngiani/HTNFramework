﻿/*
	Copyright (c) 2016 Denis Zykov, GameDevWare.com

	https://www.assetstore.unity3d.com/#!/content/56706
	
	This source code is distributed via Unity Asset Store, 
	to use it in your project you should accept Terms of Service and EULA 
	https://unity3d.com/ru/legal/as_terms
*/

using System;

namespace GameDevWare.Dynamic.Expressions.CSharp
{
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
	public class TokenAttribute : Attribute
	{
		public string Value { get; set; }

		public TokenAttribute(string value)
		{
			this.Value = value;
		}
	}
}
