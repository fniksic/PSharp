﻿// ------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------------------------------------------

using System.Reflection;
using System.IO;
using System;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.PSharp.IO;
using Microsoft.PSharp.LanguageServices.Compilation;
using Microsoft.PSharp.LanguageServices.Parsing;
using Microsoft.PSharp.LanguageServices;

namespace Microsoft.PSharp
{
    /// <summary>
    /// The P# syntax rewriter.
    /// </summary>
    public class SyntaxRewriterProcess
    {
        static void Main(string[] args)
        {
            var usage = "Usage: PSharpSyntaxRewriterProcess.exe /csVersion:major.minor file1.psharp output1.cs file2.psharp output2.cs ...";

            // Number of args must be odd
            if (args.Length % 2 != 1)
            {
                Output.WriteLine(usage);
                return;
            }

            // Parse the first argument: csVersion (we also allow "?", "h", "help")
            var csVersion = new Version(0, 0);
            if (args[0].StartsWith('/') || args[0].StartsWith('-'))
            {
                var parts = args[0].Substring(1).Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                switch (parts[0].ToLower())
                {
                    case "?":
                    case "h":
                    case "help":
                        Output.WriteLine(usage);
                        return;
                    case "csversion":
                        bool parseVersion(string value, out Version version)
                        {
                            if (int.TryParse(value, out int intVer))
                            {
                                version = new Version(intVer, 0);
                                return true;
                            }
                            return Version.TryParse(value, out version);
                        }
                        if (parts.Length != 2 || !parseVersion(parts[1], out csVersion))
                        {
                            Output.WriteLine("Error: option csVersion requires a version major[.minor] value");
                            return;
                        }
                        break;
                    default:
                        Output.WriteLine($"Error: unknown option {parts[0]}");
                        return;
                }
            }
            else
            {
                Output.WriteLine(usage);
                return;
            }

            // Parse the rest of the arguments: pairs of inputfile and outputfile
            int count = 1;
            while (count < args.Length)
            {
                string inputFileName = args[count];
                count++;
                string outputFileName = args[count];
                count++;

                // Get input file as string
                var input = string.Empty;
                try
                {
                    input = File.ReadAllText(inputFileName);
                }
                catch (IOException e)
                {
                    Output.WriteLine("Error: {0}", e.Message);
                    return;
                }

                // Translate and write to output file
                string errors = string.Empty;
                var output = Translate(input, out errors, csVersion);
                if (output == null)
                {
                    // replace Program.psharp with the actual file name
                    errors = errors.Replace("Program.psharp", Path.GetFileName(inputFileName));

                    // print a compiler error with log
                    File.WriteAllText(outputFileName,
                        string.Format("#error Psharp Compiler Error {0} /* {0} {1} {0} */ ", "\n", errors));
                }
                else
                {
                    // Tagging the generated .cs files with the "<auto-generated>" tag so as to avoid StyleCop build errors.
                    output = "//  <auto-generated />\n" + output;
                    File.WriteAllText(outputFileName, output);
                }
            }
        }

        /// <summary>
        /// Translates the specified text from P# to C#.
        /// </summary>
        /// <param name="text">Text</param>
        /// <returns>Text</returns>
        public static string Translate(string text, out string errors, Version csVersion)
        {
            var configuration = Configuration.Create();
            configuration.Verbose = 2;
            configuration.RewriteCSharpVersion = csVersion;
            errors = null;

            var context = CompilationContext.Create(configuration).LoadSolution(text);

            try
            {
                ParsingEngine.Create(context).Run();
                RewritingEngine.Create(context).Run();

                var syntaxTree = context.GetProjects()[0].PSharpPrograms[0].GetSyntaxTree();

                return syntaxTree.ToString();
            }
            catch (ParsingException ex)
            {
                errors = ex.Message;
                return null;
            }
            catch (RewritingException ex)
            {
                errors = ex.Message;
                return null;
            }
        }
    }

    public class RewriterToolTask : ToolTask
    {
        public ITaskItem[] InputFiles { get; set; }

        [Output]
        public ITaskItem[] OutputFiles { get; set; }

        public string CSharpVersion
        {
            get { return this.csVersion.ToString(); }
            set
            {
                // Version.Parse errors if there is no ".minor" part. Allow exceptions to propagate.
                this.csVersion = string.IsNullOrEmpty(value)
                    ? new Version()
                    : int.TryParse(value, out int intVer) ? new Version(intVer, 0) : new Version(value);
            }
        }
        private Version csVersion = new Version();

        protected override string ToolName => "dotnet";

        protected override string GenerateFullPathToTool()
        {
            // The entry assembly should be MSBuild.dll, and the dotnet executable
            // should be two levels above it
            var entryAssemblyDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            var dotnetPath = Path.Combine(new string[] { entryAssemblyDirectory,
                "..", "..", "dotnet" });
            var dotnetFullPath = Path.GetFullPath(dotnetPath);
            Log.LogMessage("EntryAssemblyDirectory: {0}\nDotnetPath: {1}\nDotnetFullPath: {2}",
                entryAssemblyDirectory, dotnetPath, dotnetFullPath);
            return dotnetFullPath;
        }

        protected override string GenerateCommandLineCommands()
        {
            CommandLineBuilder builder = new CommandLineBuilder();

            builder.AppendFileNameIfNotNull(this.GetType().Assembly.Location);
            builder.AppendSwitch(string.Format("/csVersion:{0}", csVersion));
            for (int i = 0; i < InputFiles.Length; i++)
            {
                builder.AppendFileNameIfNotNull(InputFiles[i]);
                builder.AppendFileNameIfNotNull(OutputFiles[i]);
            }

            var commandLine = builder.ToString();
            Log.LogMessage("Generated command line: {0}", commandLine);
            return commandLine;
        }
    }
}
