﻿using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Org.BouncyCastle.Utilities.Collections;

namespace Raven.Server.Documents.Indexes.Static.Roslyn.Rewriters
{
    public class MethodDynamicParametersRewriter : CSharpSyntaxRewriter
    {
        public static MethodDynamicParametersRewriter Instance = new MethodDynamicParametersRewriter();

        private MethodDynamicParametersRewriter()
        {
        }

        private const string dynamicStr = "dynamic";
        public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            var originalParameters = node.ParameterList.Parameters;
            var sb = new StringBuilder();
            sb.Append('(');
            var first = true;
            foreach (var param in originalParameters)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    sb.Append(", ");
                }
                if (param.Type.ToString() == dynamicStr)
                {
                    sb.Append(param);
                    continue;
                }
                sb.Append($"{dynamicStr} d_{param.Identifier.WithLeadingTrivia()}");
            }
            sb.Append(')');
            var modifiedParameterList = node.WithParameterList(SyntaxFactory.ParseParameterList(sb.ToString()));
            var statements = new List<StatementSyntax>();
            foreach (var param in originalParameters)
            {
                if (param.Type.ToString() == dynamicStr)
                    continue;
                statements.Add(SyntaxFactory.ParseStatement($"{param.Type} {param.Identifier.WithLeadingTrivia()} = ({param.Type})d_{param.Identifier.WithLeadingTrivia()};"));
            }
            if (statements.Count == 0)
                return modifiedParameterList;
            return modifiedParameterList.WithBody(modifiedParameterList.Body.WithStatements(modifiedParameterList.Body.Statements.InsertRange(0, statements))); //TODO: deal with indentation
        }
    }
}