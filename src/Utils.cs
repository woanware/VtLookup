using CsvHelper;
using VirusTotalNet.Results;

namespace VtLookup
{
    internal class Utils
    {
        public static void WriteFileReport(CsvWriter csvWriter, FileReport fileReport)
        {
            if (fileReport.ResponseCode == VirusTotalNet.ResponseCodes.FileReportResponseCode.Present)
            {
                csvWriter.WriteField(fileReport.MD5);
                csvWriter.WriteField(fileReport.SHA256);
                csvWriter.WriteField(fileReport.Permalink);
                csvWriter.WriteField(fileReport.Positives);
                csvWriter.WriteField(fileReport.Total);
                csvWriter.WriteField(fileReport.ScanDate);

                List<string> results = new List<string>();
                foreach (var se in fileReport.Scans)
                {
                    if (se.Value.Detected == true)
                    {
                        results.Add($"{se.Key}: {se.Value.Result}");
                    }
                }
                results.Sort();
                csvWriter.WriteField(string.Join(",", results));
            }
            else
            {
                csvWriter.WriteField(fileReport.MD5);
                csvWriter.WriteField(fileReport.SHA256);
                csvWriter.WriteField(fileReport.Permalink);
                csvWriter.WriteField("-");
                csvWriter.WriteField("-");
                csvWriter.WriteField("-");
                csvWriter.WriteField("-");
                csvWriter.WriteField("-");
            }
            csvWriter.NextRecord();
        }
    }
}
