using System;
namespace Requestor.Lib;

public class RequestSettings
{
	public RequestVerb Verb { get; set; } = RequestVerb.Get;
	public string Url { get; set; } = "";
	public string? HttpVersion { get; set; }
	public IDictionary<string, string> Headers { get; } = new Dictionary<string, string>();
	public string RequestBody { get; set; } = "";
	public string RequestBodyContentType { get; set; } = "";
	public string? RequestBodyInputFile { get; set; }
	public bool UseVariablesInRequestBody { get; set; } = false;
	public string? RequestBodyInputFileEncoding { get; set; }
    public string? Comment { get; set; }

    public IList<string> Variables { get; } = new List<string>();
}
