using System;
using System.Diagnostics;
using System.IO;
using NuGet.Common;

namespace Gogux.VSCoverage.Tool
{
    public class CoverageReportGenerator
    {
        private readonly string _sourceReportPath;
        private readonly string _outputPath;

        public CoverageReportGenerator(string sourceReportPath, string outputPath)
        {
            _sourceReportPath = SanitizeSourceReportPath(sourceReportPath);
            _outputPath = SanitizeOutputPath(outputPath);
        }

        public void Generate()
        {
            var codeCoveragePath = Path.Combine(NuGetEnvironment.GetFolderPath(NuGetFolderPath.NuGetHome),
                @"packages\microsoft.codecoverage\16.3.0-preview-20190808-03\build\netstandard1.0\CodeCoverage\CodeCoverage.exe");

            var reportGeneratorPath = Path.Combine(NuGetEnvironment.GetFolderPath(NuGetFolderPath.NuGetHome),
                @"packages\reportgenerator\4.2.16\tools\netcoreapp2.1\ReportGenerator.dll");

            var completeXmlReportFileName = Path.Combine(_outputPath, Path.Combine(_outputPath, $"{Path.GetFileNameWithoutExtension(_sourceReportPath)}.xml"));
            var commandText = $"/C {codeCoveragePath} analyze /output:\"{completeXmlReportFileName}\" \"{_sourceReportPath}\"";
            
            var cmdStartInfo = new ProcessStartInfo();
            cmdStartInfo.FileName = "cmd.exe";
            cmdStartInfo.Arguments = commandText;

            var cmd = new Process { StartInfo = cmdStartInfo };
            cmd.Start();
            cmd.WaitForExit();

            commandText = $"/C dotnet {reportGeneratorPath} \"-reports:{completeXmlReportFileName}\" \"-targetdir:{_outputPath}\" \"-reporttypes:HTML;HTMLSummary\"";
            
            cmdStartInfo = new ProcessStartInfo();
            cmdStartInfo.FileName = "cmd.exe";
            cmdStartInfo.Arguments = commandText;
            
            cmd = new Process { StartInfo = cmdStartInfo };
            cmd.Start();
            cmd.WaitForExit();
        }

        private static string SanitizeSourceReportPath(string sourceReportPath)
        {
            var sanitizedValue = sourceReportPath.Trim();
            
            if (!File.Exists(sanitizedValue)) throw new ArgumentException($"File {sanitizedValue} not found");

            return sanitizedValue;
        }

        private static string SanitizeOutputPath(string outputPath)
        {
            if (string.IsNullOrWhiteSpace(outputPath)) return @"%HOMEDRIVE%%HOMEPATH%\coveragereport";
            
            var sanitizedValue = outputPath.Trim();
            
            if (!Directory.Exists(outputPath)) throw new ArgumentException($"Directory {outputPath} not found");

            return sanitizedValue;
        }
    }
}