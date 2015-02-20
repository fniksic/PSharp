﻿//-----------------------------------------------------------------------
// <copyright file="PSharpClassifier.cs">
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
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Tagging;

using Microsoft.PSharp.Parsing;

namespace Microsoft.PSharp.VisualStudio
{
    /// <summary>
    /// The P# classifier.
    /// </summary>
    internal sealed class PSharpClassifier : ITagger<ClassificationTag>
    {
        private IClassificationTypeRegistryService TypeRegistry;
        private ITextBuffer Buffer;
        private ITagAggregator<PSharpTokenTag> Aggregator;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="buffer">ITextBuffer</param>
        /// <param name="tagAggregator">ITagAggregator</param>
        /// <param name="typeRegistry">IClassificationTypeRegistryService</param>
        internal PSharpClassifier(ITextBuffer buffer, ITagAggregator<PSharpTokenTag> tagAggregator,
            IClassificationTypeRegistryService typeRegistry)
        {
            this.TypeRegistry = typeRegistry;
            this.Buffer = buffer;
            this.Aggregator = tagAggregator;
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged
        {
            add { }
            remove { }
        }

        public IEnumerable<ITagSpan<ClassificationTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            foreach (var tagSpan in this.Aggregator.GetTags(spans))
            {
                var tagSpans = tagSpan.Span.GetSpans(spans[0].Snapshot);
                yield return
                    new TagSpan<ClassificationTag>(tagSpans[0],
                    new ClassificationTag(this.GetClassificationType(tagSpan.Tag.Type)));
            }
        }

        /// <summary>
        /// Returns the classification type from the given token.
        /// </summary>
        /// <param name="tokenType">TokenType</param>
        /// <returns>IClassificationType</returns>
        private IClassificationType GetClassificationType(TokenType tokenType)
        {
            string classification;
            switch (tokenType)
            {
                case TokenType.None:
                    classification = "PSharp";
                    break;

                case TokenType.NewLine:
                    classification = "PSharp.NewLine";
                    break;

                case TokenType.WhiteSpace:
                    classification = "PSharp.WhiteSpace";
                    break;

                case TokenType.Comment:
                case TokenType.CommentStart:
                case TokenType.CommentEnd:
                    classification = "PSharp.Comment";
                    break;

                case TokenType.Region:
                    classification = "PSharp.Region";
                    break;

                case TokenType.EventIdentifier:
                case TokenType.MachineIdentifier:
                case TokenType.StateIdentifier:
                case TokenType.ActionIdentifier:
                case TokenType.TypeIdentifier:
                case TokenType.Identifier:
                    classification = "PSharp.Identifier";
                    break;

                case TokenType.LeftCurlyBracket:
                    classification = "PSharp.LeftCurlyBracket";
                    break;

                case TokenType.RightCurlyBracket:
                    classification = "PSharp.RightCurlyBracket";
                    break;

                case TokenType.LeftParenthesis:
                    classification = "PSharp.LeftParenthesis";
                    break;

                case TokenType.RightParenthesis:
                    classification = "PSharp.RightParenthesis";
                    break;

                case TokenType.LeftSquareBracket:
                    classification = "PSharp.LeftSquareBracket";
                    break;

                case TokenType.RightSquareBracket:
                    classification = "PSharp.RightSquareBracket";
                    break;

                case TokenType.MachineLeftCurlyBracket:
                    classification = "PSharp.LeftCurlyBracket";
                    break;

                case TokenType.MachineRightCurlyBracket:
                    classification = "PSharp.RightCurlyBracket";
                    break;

                case TokenType.StateLeftCurlyBracket:
                    classification = "PSharp.LeftCurlyBracket";
                    break;

                case TokenType.StateRightCurlyBracket:
                    classification = "PSharp.RightCurlyBracket";
                    break;

                case TokenType.Semicolon:
                    classification = "PSharp.Semicolon";
                    break;

                case TokenType.Doublecolon:
                    classification = "PSharp.Doublecolon";
                    break;

                case TokenType.Comma:
                    classification = "PSharp.Comma";
                    break;

                case TokenType.Dot:
                    classification = "PSharp.Dot";
                    break;

                case TokenType.AndOperator:
                case TokenType.OrOperator:
                case TokenType.NotOperator:
                case TokenType.EqualOperator:
                case TokenType.LessThanOperator:
                case TokenType.GreaterThanOperator:
                case TokenType.PlusOperator:
                case TokenType.MinusOperator:
                case TokenType.MultiplyOperator:
                case TokenType.DivideOperator:
                case TokenType.ModOperator:
                    classification = "PSharp.Operator";
                    break;

                case TokenType.Private:
                case TokenType.Protected:
                case TokenType.Internal:
                case TokenType.Public:
                case TokenType.Abstract:
                case TokenType.Virtual:
                case TokenType.Override:

                case TokenType.NamespaceDecl:
                case TokenType.ClassDecl:
                case TokenType.StructDecl:
                case TokenType.Using:

                case TokenType.MachineDecl:
                case TokenType.StateDecl:
                case TokenType.EventDecl:
                case TokenType.ActionDecl:

                case TokenType.OnAction:
                case TokenType.DoAction:
                case TokenType.GotoState:
                case TokenType.DeferEvent:
                case TokenType.IgnoreEvent:
                case TokenType.ToMachine:
                case TokenType.Entry:
                case TokenType.Exit:

                case TokenType.This:
                case TokenType.Base:

                case TokenType.New:
                case TokenType.As:
                case TokenType.ForLoop:
                case TokenType.WhileLoop:
                case TokenType.DoLoop:
                case TokenType.IfCondition:
                case TokenType.ElseCondition:
                case TokenType.Break:
                case TokenType.Continue:
                case TokenType.Return:

                case TokenType.CreateMachine:
                case TokenType.SendEvent:
                case TokenType.RaiseEvent:
                case TokenType.DeleteMachine:
                case TokenType.Assert:
                case TokenType.Payload:
                    classification = "PSharp.Keyword";
                    break;

                default:
                    throw new ArgumentException("Unable to find classification type for " +
                        tokenType.ToString() + " tokenType");
            }

            return this.TypeRegistry.GetClassificationType(classification);
        }
    }
}
