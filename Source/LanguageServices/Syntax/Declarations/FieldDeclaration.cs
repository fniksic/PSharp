﻿//-----------------------------------------------------------------------
// <copyright file="FieldDeclaration.cs">
//      Copyright (c) Microsoft Corporation. All rights reserved.
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

using Microsoft.PSharp.IO;
using Microsoft.PSharp.LanguageServices.Parsing;

namespace Microsoft.PSharp.LanguageServices.Syntax
{
    /// <summary>
    /// Field declaration syntax node.
    /// </summary>
    internal class FieldDeclaration : PSharpSyntaxNode
    {
        #region fields

        /// <summary>
        /// The machine parent node.
        /// </summary>
        protected MachineDeclaration Machine;

        /// <summary>
        /// The access modifier.
        /// </summary>
        internal readonly AccessModifier AccessModifier;

        /// <summary>
        /// The type identifier.
        /// </summary>
        internal Token TypeIdentifier;

        /// <summary>
        /// The identifier token.
        /// </summary>
        internal Token Identifier;

        /// <summary>
        /// The semicolon token.
        /// </summary>
        internal Token SemicolonToken;

        #endregion

        #region internal API

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="program">Program</param>
        /// <param name="machineNode">MachineDeclarationNode</param>
        /// <param name="modSet">Modifier set</param>
        internal FieldDeclaration(IPSharpProgram program, MachineDeclaration machineNode, ModifierSet modSet)
            : base(program)
        {
            this.Machine = machineNode;
            this.AccessModifier = modSet.AccessModifier;
        }

        /// <summary>
        /// Rewrites the syntax node declaration to the intermediate C#
        /// representation.
        /// </summary>
        internal override void Rewrite(int indentLevel)
        {
            string text = "";

            try
            {
                text = GetIndent(indentLevel) + this.GetRewrittenFieldDeclaration();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception was thrown during rewriting:");
                Debug.WriteLine(ex.Message);
                Debug.WriteLine(ex.StackTrace);
                Error.ReportAndExit("Failed to rewrite field '{0}' of machine '{1}'.",
                    this.Identifier.TextUnit.Text, this.Machine.Identifier.TextUnit.Text);
            }

            base.TextUnit = this.TypeIdentifier.TextUnit.WithText(text);
        }

        #endregion

        #region private methods

        /// <summary>
        /// Returns the rewritten field declaration.
        /// </summary>
        /// <returns>Text</returns>
        private string GetRewrittenFieldDeclaration()
        {
            string text = "";

            if (this.AccessModifier == AccessModifier.Protected)
            {
                text += "protected ";
            }
            else if (this.AccessModifier == AccessModifier.Private)
            {
                text += "private ";
            }

            text += this.TypeIdentifier.TextUnit.Text;
            text += " ";

            text += this.Identifier.TextUnit.Text;

            text += this.SemicolonToken.TextUnit.Text + "\n";

            return text;
        }

        #endregion
    }
}
