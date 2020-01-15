using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using NuGet.Common;

namespace Gogux.VSCoverage.Tool
{
    public class CoverageReportGenerator
    {
        private readonly List<string> _sourceReportPaths;
        private readonly string _outputPath;

        public CoverageReportGenerator(IEnumerable<string> sourceReportPaths, string outputPath)
        {
            _sourceReportPaths = sourceReportPaths.Select(SanitizeSourceReportPath).ToList();
            _outputPath = SanitizeOutputPath(outputPath);
        }

        public void Generate()
        {
            var codeCoveragePath = Path.Combine(NuGetEnvironment.GetFolderPath(NuGetFolderPath.NuGetHome),
                @"packages\microsoft.codecoverage\16.3.0-preview-20190808-03\build\netstandard1.0\CodeCoverage\CodeCoverage.exe");

            var reportGeneratorPath = Path.Combine(NuGetEnvironment.GetFolderPath(NuGetFolderPath.NuGetHome),
                @"packages\reportgenerator\4.2.16\tools\netcoreapp2.1\ReportGenerator.dll");

            var commandTexts = new List<string>();
            var xmlReports = new List<string>();
            
            foreach (var reportPath in _sourceReportPaths)
            {
                var completeXmlReportFileName = Path.Combine(_outputPath, Path.Combine(_outputPath, $"{Path.GetFileNameWithoutExtension(reportPath)}.xml"));
                commandTexts.Add($"/C {codeCoveragePath} analyze /output:\"{completeXmlReportFileName}\" \"{reportPath}\"");
                xmlReports.Add(completeXmlReportFileName);
            }

            ProcessStartInfo cmdStartInfo = null;

            foreach (var commandText in commandTexts)
            {
                cmdStartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = commandText
                };
                
                var convertCoverageReportToXml = new Process { StartInfo = cmdStartInfo };
                convertCoverageReportToXml.Start();
                convertCoverageReportToXml.WaitForExit();
            }
            
            cmdStartInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/C dotnet {reportGeneratorPath} \"-reports:{string.Join(';', xmlReports)}\" \"-targetdir:{_outputPath}\" \"-reporttypes:HTML;HTMLSummary\""
            };

            var publishMergedReports = new Process { StartInfo = cmdStartInfo };
            publishMergedReports.Start();
            publishMergedReports.WaitForExit();
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
            
            if (!Directory.Exists(sanitizedValue)) throw new ArgumentException($"Directory {sanitizedValue} not found");

            return sanitizedValue;
        }
    }
}