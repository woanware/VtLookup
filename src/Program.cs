using CsvHelper;
using Spectre.Console;
using System.CommandLine;
using System.Globalization;
using VirusTotalNet;
using VtLookup;

/// <summary>
/// 
/// </summary>
var inputFile = new Option<FileInfo>(new[] { "-i", "--inputfile" },
                                        description: "File path for file containing lookup values e.g. sha256")
{
    IsRequired = true
};

/// <summary>
/// 
/// </summary>
var outputDir = new Option<string>(new[] { "-o", "--outputdir" },
                                   description: "Output directory")
{
    IsRequired = false
};

RootCommand rootCommand = new RootCommand("Performs VirusTotal lookups")
{
    inputFile,
    outputDir,
};

var initCommand = new Command("init", "Initialises API config");
rootCommand.Add(initCommand);

rootCommand.SetHandler((inputFile, outputDir) =>
{
    if (inputFile.Exists == false)
    {
        Console.WriteLine($"Input file does not exist");
        return;
    }

    Config config = new Config();
    string ret = config.Load();
    if (string.IsNullOrEmpty(ret) == false)
    {
        Console.WriteLine(ret);
        return;
    }
    
    if (string.IsNullOrEmpty(config.ApiKey))
    {
        Console.WriteLine("API key not set. Use 'init' command");
        return;
    }

    string outputPath = string.Empty;
    if (string.IsNullOrEmpty(outputDir) == false)
    {
        outputPath = $"{outputDir}\\VtLookup-{DateTime.UtcNow.ToString("yyyy-MM-dd_HH-mm-ss", CultureInfo.InvariantCulture)}.csv";
    }
    else
    {
        outputPath = $"VtLookup-{DateTime.UtcNow.ToString("yyyy-MM-dd_HH-mm-ss", CultureInfo.InvariantCulture)}.csv";
    }

    VirusTotal virusTotal = new VirusTotal(config.ApiKey);
    virusTotal.UseTLS = true;

    virusTotal.FileReportBatchSizeLimit = config.BatchSize;

    var values = File.ReadAllLines(inputFile.FullName);

    using (FileStream fsOutput = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.Read))
    using (StreamWriter swOutput = new StreamWriter(fsOutput))
    using (CsvWriter csvWriter = new CsvWriter(swOutput, CultureInfo.InvariantCulture))
    {
        csvWriter.WriteField("MD5");
        csvWriter.WriteField("SHA256");
        csvWriter.WriteField("Permalink");
        csvWriter.WriteField("Positives");
        csvWriter.WriteField("Total");
        csvWriter.WriteField("ScanDate");
        csvWriter.WriteField("Scans");
        csvWriter.NextRecord();

        AnsiConsole.Progress()
            .AutoRefresh(true)
            .Columns(new ProgressColumn[]
            {
                new ProgressBarColumn(),        
                new PercentageColumn(),         
                new RemainingTimeColumn(),      
                new SpinnerColumn(Spinner.Known.Dots2),           
            })
            .Start(ctx =>
            {
                var task = ctx.AddTask("[blue]Performing lookups[/]");

                task.MaxValue = values.LongLength;

                while (!ctx.IsFinished)
                {
                    List<string> batch = new List<string>();
                    foreach (string val in values)
                    {
                        if (batch.Count == config.BatchSize)
                        {
                            var fileReports = virusTotal.GetFileReportsAsync(batch).Result;
                            foreach (var fileReport in fileReports)
                            {
                                Utils.WriteFileReport(csvWriter, fileReport);
                            }

                            batch = new List<string>();
                            task.Increment(config.BatchSize);
                        }

                        batch.Add(val);
                    }

                    if (batch.Any())
                    {
                        var fileReports = virusTotal.GetFileReportsAsync(batch).Result;
                        foreach (var fileReport in fileReports)
                        {
                            Utils.WriteFileReport(csvWriter, fileReport);
                        }

                        task.Increment(batch.Count);
                    }
                }
            });
    }

}, inputFile, outputDir);

initCommand.SetHandler((delayOptionValue) =>
{
    Console.WriteLine($"Enter your API key: ");
    string apiKey = Console.ReadLine();
    if (string.IsNullOrEmpty(apiKey.Trim()))
    {
        Console.WriteLine($"No API key entered");
        return;
    }

    Config config = new Config
    {
        ApiKey = apiKey
    };

    string ret = config.Save();
    if (string.IsNullOrEmpty(ret) == false)
    {
        Console.WriteLine(ret);
        return;
    }

});

await rootCommand.InvokeAsync(args);