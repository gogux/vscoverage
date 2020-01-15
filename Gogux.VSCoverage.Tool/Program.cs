using System;
using System.Collections.Generic;
using System.Linq;
using CommandLine;

namespace Gogux.VSCoverage.Tool
{
    public class Options
    {
        [Option('r', "coverageReportPath", Required = true, HelpText = "Code coverage (.coverage) file path")]
        public string CoverageReportPaths { get; set; }
        
        [Option('o', "outputPath", Required = false, HelpText = "Output path")]
        public string OutputPath { get; set; }
    }

    public static class Program
    {
        public static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(options => new CoverageReportGenerator(
                        options.CoverageReportPaths.Split(';', StringSplitOptions.RemoveEmptyEntries),
                        options.OutputPath)
                    .Generate())
                .WithNotParsed(DisplayErrors);
        }

        private static void DisplayErrors(IEnumerable<Error> errors)
        {
            var enumerableErrors = errors as Error[] ?? errors.ToArray();
            
            if (enumerableErrors.Any(error => error.Tag == ErrorType.HelpRequestedError))
                Console.WriteLine("Usage: gvsc -r[--coverageReportPath] <.cover report path> -o[--outputPath] <output path>");
        }
    }
}