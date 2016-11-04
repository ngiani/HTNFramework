/*
	Copyright (c) 2016 Denis Zykov, GameDevWare.com

	https://www.assetstore.unity3d.com/#!/content/56706
	
	This source code is distributed via Unity Asset Store, 
	to use it in your project you should accept Terms of Service and EULA 
	https://unity3d.com/ru/legal/as_terms
*/

using System;
using System.Linq.Expressions;

namespace GameDevWare.Dynamic.Expressions.CSharp
{
	public static class CSharpExpression
	{
		public const string ARG1_DEFAULT_NAME = "arg1";
		public const string ARG2_DEFAULT_NAME = "arg2";
		public const string ARG3_DEFAULT_NAME = "arg3";
		public const string ARG4_DEFAULT_NAME = "arg4";

		public static ResultT Evaluate<ResultT>(string expression, Type[] knownTypes = null)
		{
			var func = Parse<ResultT>(expression, knownTypes).CompileAot();
			var result = func.Invoke();
			return result;
		}
		public static ResultT Evaluate<Arg1T, ResultT>(string expression, Arg1T arg1, string arg1Name = ARG1_DEFAULT_NAME, Type[] knownTypes = null)
		{
			var func = Parse<Arg1T, ResultT>(expression, arg1Name, knownTypes).CompileAot();
			var result = func.Invoke(arg1);
			return result;
		}
		public static ResultT Evaluate<Arg1T, Arg2T, ResultT>(string expression, Arg1T arg1, Arg2T arg2, string arg1Name = ARG1_DEFAULT_NAME, string arg2Name = ARG2_DEFAULT_NAME, Type[] knownTypes = null)
		{
			var func = Parse<Arg1T, Arg2T, ResultT>(expression, arg1Name, arg2Name, knownTypes).CompileAot();
			var result = func.Invoke(arg1, arg2);
			return result;
		}
		public static ResultT Evaluate<Arg1T, Arg2T, Arg3T, ResultT>(string expression, Arg1T arg1, Arg2T arg2, Arg3T arg3, string arg1Name = ARG1_DEFAULT_NAME, string arg2Name = ARG2_DEFAULT_NAME, string arg3Name = ARG3_DEFAULT_NAME, Type[] knownTypes = null)
		{
			var func = Parse<Arg1T, Arg2T, Arg3T, ResultT>(expression, arg1Name, arg2Name, arg3Name, knownTypes).CompileAot();
			var result = func.Invoke(arg1, arg2, arg3);
			return result;
		}
		public static ResultT Evaluate<Arg1T, Arg2T, Arg3T, Arg4T, ResultT>(string expression, Arg1T arg1, Arg2T arg2, Arg3T arg3, Arg4T arg4, string arg1Name = ARG1_DEFAULT_NAME, string arg2Name = ARG2_DEFAULT_NAME, string arg3Name = ARG3_DEFAULT_NAME, string arg4Name = ARG4_DEFAULT_NAME, Type[] knownTypes = null)
		{
			var func = Parse<Arg1T, Arg2T, Arg3T, Arg4T, ResultT>(expression, arg1Name, arg2Name, arg3Name, arg4Name, knownTypes).CompileAot();
			var result = func.Invoke(arg1, arg2, arg3, arg4);
			return result;
		}

		public static Expression<Func<ResultT>> Parse<ResultT>(string expression, Type[] knownTypes = null)
		{
			if (expression == null) throw new ArgumentNullException("expression");

			var tokens = Tokenizer.Tokenize(expression);
			var parseTree = Parser.Parse(tokens);
			var expressionTree = parseTree.ToExpressionTree();
			var expressionBuilder = new ExpressionBuilder(new ParameterExpression[0], resultType: typeof(ResultT), knownTypes: knownTypes);
			var body = expressionBuilder.Build(expressionTree);
			return Expression.Lambda<Func<ResultT>>(body, expressionBuilder.Parameters);
		}
		public static Expression<Func<Arg1T, ResultT>> Parse<Arg1T, ResultT>(string expression, string arg1Name = ARG1_DEFAULT_NAME, Type[] knownTypes = null)
		{
			if (expression == null) throw new ArgumentNullException("expression");

			var tokens = Tokenizer.Tokenize(expression);
			var parseTree = Parser.Parse(tokens);
			var expressionTree = parseTree.ToExpressionTree();
			var expressionBuilder = new ExpressionBuilder(new[]
			{
				Expression.Parameter(typeof(Arg1T), arg1Name ?? ARG1_DEFAULT_NAME)
			}, resultType: typeof(ResultT), knownTypes: knownTypes);
			var body = expressionBuilder.Build(expressionTree);
			return Expression.Lambda<Func<Arg1T, ResultT>>(body, expressionBuilder.Parameters);
		}
		public static Expression<Func<Arg1T, Arg2T, ResultT>> Parse<Arg1T, Arg2T, ResultT>(string expression, string arg1Name = ARG1_DEFAULT_NAME, string arg2Name = ARG2_DEFAULT_NAME, Type[] knownTypes = null)
		{
			if (expression == null) throw new ArgumentNullException("expression");

			var tokens = Tokenizer.Tokenize(expression);
			var parseTree = Parser.Parse(tokens);
			var expressionTree = parseTree.ToExpressionTree();
			var expressionBuilder = new ExpressionBuilder(new[]
			{
				Expression.Parameter(typeof(Arg1T), arg1Name ?? ARG1_DEFAULT_NAME),
				Expression.Parameter(typeof(Arg2T), arg2Name ?? ARG2_DEFAULT_NAME),
			}, resultType: typeof(ResultT), knownTypes: knownTypes);
			var body = expressionBuilder.Build(expressionTree);
			return Expression.Lambda<Func<Arg1T, Arg2T, ResultT>>(body, expressionBuilder.Parameters);
		}
		public static Expression<Func<Arg1T, Arg2T, Arg3T, ResultT>> Parse<Arg1T, Arg2T, Arg3T, ResultT>(string expression, string arg1Name = ARG1_DEFAULT_NAME, string arg2Name = ARG2_DEFAULT_NAME, string arg3Name = ARG3_DEFAULT_NAME, Type[] knownTypes = null)
		{
			if (expression == null) throw new ArgumentNullException("expression");

			var tokens = Tokenizer.Tokenize(expression);
			var parseTree = Parser.Parse(tokens);
			var expressionTree = parseTree.ToExpressionTree();
			var expressionBuilder = new ExpressionBuilder(new ParameterExpression[]
			{
				Expression.Parameter(typeof(Arg1T), arg1Name ?? ARG1_DEFAULT_NAME),
				Expression.Parameter(typeof(Arg2T), arg2Name ?? ARG2_DEFAULT_NAME),
				Expression.Parameter(typeof(Arg3T), arg3Name ?? ARG3_DEFAULT_NAME),
			}, resultType: typeof(ResultT), knownTypes: knownTypes);
			var body = expressionBuilder.Build(expressionTree);
			return Expression.Lambda<Func<Arg1T, Arg2T, Arg3T, ResultT>>(body, expressionBuilder.Parameters);
		}
		public static Expression<Func<Arg1T, Arg2T, Arg3T, Arg4T, ResultT>> Parse<Arg1T, Arg2T, Arg3T, Arg4T, ResultT>(string expression, string arg1Name = ARG1_DEFAULT_NAME, string arg2Name = ARG2_DEFAULT_NAME, string arg3Name = ARG3_DEFAULT_NAME, string arg4Name = ARG4_DEFAULT_NAME, Type[] knownTypes = null)
		{
			if (expression == null) throw new ArgumentNullException("expression");

			var tokens = Tokenizer.Tokenize(expression);
			var parseTree = Parser.Parse(tokens);
			var expressionTree = parseTree.ToExpressionTree();
			var expressionBuilder = new ExpressionBuilder(new ParameterExpression[]
			{
				Expression.Parameter(typeof(Arg1T), arg1Name ?? ARG1_DEFAULT_NAME),
				Expression.Parameter(typeof(Arg2T), arg2Name ?? ARG2_DEFAULT_NAME),
				Expression.Parameter(typeof(Arg3T), arg3Name ?? ARG3_DEFAULT_NAME),
				Expression.Parameter(typeof(Arg4T), arg3Name ?? ARG4_DEFAULT_NAME),
			}, resultType: typeof(ResultT), knownTypes: knownTypes);
			var body = expressionBuilder.Build(expressionTree);
			return Expression.Lambda<Func<Arg1T, Arg2T, Arg3T, Arg4T, ResultT>>(body, expressionBuilder.Parameters);
		}
	}
}
