﻿//-----------------------------------------------------------------------
// <copyright file="EventDeclarationVisitor.cs">
//      Copyright (c) 2015 Pantazis Deligiannis (p.deligiannis@imperial.ac.uk)
// 
//      THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
//      EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
//      MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
//      IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
//      CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
//      TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
//      SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;

using Microsoft.PSharp.Parsing.Syntax;

namespace Microsoft.PSharp.Parsing
{
    /// <summary>
    /// The P# event declaration parsing visitor.
    /// </summary>
    public sealed class EventDeclarationVisitor : BaseParseVisitor
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="tokenStream">TokenStream</param>
        public EventDeclarationVisitor(TokenStream tokenStream)
            : base(tokenStream)
        {

        }

        /// <summary>
        /// Visits the syntax node.
        /// </summary>
        /// <param name="program">Program</param>
        /// <param name="parentNode">Node</param>
        /// <param name="modifier">Modifier</param>
        public void Visit(IPSharpProgram program, NamespaceDeclarationNode parentNode, Token modifier)
        {
            var node = new EventDeclarationNode();
            node.Modifier = modifier;
            node.EventKeyword = base.TokenStream.Peek();

            base.TokenStream.Index++;
            base.TokenStream.SkipWhiteSpaceAndCommentTokens();

            if (base.TokenStream.Done ||
                (base.TokenStream.Peek().Type != TokenType.Identifier &&
                base.TokenStream.Peek().Type != TokenType.HaltEvent &&
                base.TokenStream.Peek().Type != TokenType.DefaultEvent))
            {
                throw new ParsingException("Expected event identifier.",
                    new List<TokenType>
                {
                    TokenType.Identifier,
                    TokenType.HaltEvent,
                    TokenType.DefaultEvent
                });
            }

            if (base.TokenStream.Peek().Type == TokenType.Identifier)
            {
                base.TokenStream.Swap(new Token(base.TokenStream.Peek().TextUnit,
                    TokenType.EventIdentifier));
            }

            node.Identifier = base.TokenStream.Peek();

            base.TokenStream.Index++;
            base.TokenStream.SkipWhiteSpaceAndCommentTokens();

            if (base.TokenStream.Done ||
                (base.TokenStream.Peek().Type != TokenType.Assert &&
                base.TokenStream.Peek().Type != TokenType.Assume &&
                base.TokenStream.Peek().Type != TokenType.Colon &&
                base.TokenStream.Peek().Type != TokenType.Semicolon))
            {
                throw new ParsingException("Expected \":\" or \";\".",
                    new List<TokenType>
                {
                    TokenType.Assert,
                    TokenType.Assume,
                    TokenType.Colon,
                    TokenType.Semicolon
                });
            }

            if (base.TokenStream.Peek().Type == TokenType.Assert ||
                base.TokenStream.Peek().Type == TokenType.Assume)
            {
                node.AssertAssumeKeyword = base.TokenStream.Peek();

                base.TokenStream.Index++;
                base.TokenStream.SkipWhiteSpaceAndCommentTokens();

                if (base.TokenStream.Done ||
                base.TokenStream.Peek().Type != TokenType.Identifier)
                {
                    throw new ParsingException("Expected identifier.",
                        new List<TokenType>
                    {
                        TokenType.Identifier
                    });
                }

                node.AssertIdentifier = base.TokenStream.Peek();

                base.TokenStream.Index++;
                base.TokenStream.SkipWhiteSpaceAndCommentTokens();

                if (base.TokenStream.Done ||
                    (base.TokenStream.Peek().Type != TokenType.Colon &&
                    base.TokenStream.Peek().Type != TokenType.Semicolon))
                {
                    throw new ParsingException("Expected \":\" or \";\".",
                        new List<TokenType>
                    {
                        TokenType.Colon,
                        TokenType.Semicolon
                    });
                }
            }

            if (base.TokenStream.Peek().Type == TokenType.Colon)
            {
                node.ColonToken = base.TokenStream.Peek();

                base.TokenStream.Index++;
                base.TokenStream.SkipWhiteSpaceAndCommentTokens();

                var typeNode = new PTypeNode();
                new TypeIdentifierVisitor(base.TokenStream).Visit(typeNode);
                node.PayloadType = typeNode;
            }

            if (base.TokenStream.Done ||
                base.TokenStream.Peek().Type != TokenType.Semicolon)
            {
                throw new ParsingException("Expected \";\".",
                    new List<TokenType>
                {
                    TokenType.Semicolon
                });
            }

            node.SemicolonToken = base.TokenStream.Peek();

            if (base.TokenStream.IsPSharp)
            {
                parentNode.EventDeclarations.Add(node);
            }
            else
            {
                (program as PProgram).EventDeclarations.Add(node);
            }
        }
    }
}