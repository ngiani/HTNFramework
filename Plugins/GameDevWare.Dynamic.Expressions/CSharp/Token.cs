/*
	Copyright (c) 2016 Denis Zykov, GameDevWare.com

	https://www.assetstore.unity3d.com/#!/content/56706
	
	This source code is distributed via Unity Asset Store, 
	to use it in your project you should accept Terms of Service and EULA 
	https://unity3d.com/ru/legal/as_terms
*/

namespace GameDevWare.Dynamic.Expressions.CSharp
{
	public struct Token : ILineInfo
	{
		public readonly TokenType Type;
		public readonly string Value;
		public readonly int LineNumber;
		public readonly int ColumnNumber;
		public readonly int TokenLength;

		public bool IsValid { get { return this.Type != TokenType.None; } }
		public string Position { get { return string.Format("[{0}:{1}+{2}]", this.LineNumber, this.ColumnNumber, this.TokenLength); } }

		int ILineInfo.LineNumber { get { return this.LineNumber; } }
		int ILineInfo.ColumnNumber { get { return this.ColumnNumber; } }
		int ILineInfo.TokenLength { get { return this.TokenLength; } }

		public Token(TokenType type, string value, int line, int col, int len)
		{
			this.Type = type;
			this.Value = value;
			this.LineNumber = line;
			this.ColumnNumber = col;
			this.TokenLength = len;
		}

		public override string ToString()
		{
			return this.Type + (this.Type == TokenType.Number || this.Type == TokenType.Identifier || this.Type == TokenType.Literal ? "(" + this.Value + ")" : "");
		}
	}
}
