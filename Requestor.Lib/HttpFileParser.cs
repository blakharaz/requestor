namespace Requestor.Lib;

public class HttpFileParser
{
    private HttpFileParserState State { get; set; } = HttpFileParserState.NewRequest;

    public Task<RequestCollection> ParseAsync(string filename)
    {
        using (var stream = File.Open(filename, FileMode.Open))
        {
            return ParseAsync(stream);
        }
    }

    public async Task<RequestCollection> ParseAsync(Stream input)
    {
        var result = new RequestCollection();
        RequestSettings? currentRequest = null;

        using (StreamReader reader = new StreamReader(input))
        {
            while (!reader.EndOfStream)
            {
                string? line = await reader.ReadLineAsync();

                if (line is null)
                {
                    break;
                }

                line = line.TrimEnd();

                if (line.StartsWith("#"))
                {
                    continue;
                }

                if (State == HttpFileParserState.NewRequest)
                {
                    if (line == "")
                    {
                        continue;
                    }

                    if (line.StartsWith('@'))
                    {
                        // TODO: 
                        continue;
                    }

                    string[] firstLineParts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    RequestVerb verb;
                    string url;

                    if (firstLineParts.Length == 2)
                    {
                        verb = ParseVerb(firstLineParts[0]) ?? throw new InvalidDataException($"{firstLineParts[0]} is no valid verb");
                        url = firstLineParts[1];
                    }
                    else if (firstLineParts.Length == 1)
                    {
                        verb = RequestVerb.Get;
                        url = firstLineParts[0];
                    }
                    else
                    {
                        throw new InvalidDataException($"{line} is no valid request start line");
                    }

                    currentRequest = new RequestSettings { Verb = verb, Url = url };
                    result.Requests.Add(currentRequest);

                    State = HttpFileParserState.UrlFound;
                }
                else if (State == HttpFileParserState.UrlFound)
                {
                    if (currentRequest is null)
                    {
                        throw new InvalidDataException("invalid state");
                    }

                    if (line == "")
                    {
                        State = HttpFileParserState.RequestBody;
                        continue;
                    }

                    if (line.StartsWith('?') || line.StartsWith('&'))
                    {
                        currentRequest.Url += line;
                    }
                    else if (line.Contains(':'))
                    {
                        string[] headerParts = line.Split(':', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                        if (headerParts.Length != 2)
                        {
                            throw new InvalidDataException($"{line} is no valid header definition");
                        }

                        currentRequest.Headers[headerParts[0]] = headerParts[1];
                    }
                }
                else if (State == HttpFileParserState.RequestBody)
                {
                    if (currentRequest is null)
                    {
                        throw new InvalidDataException("invalid state");
                    }

                    if (line == "")
                    {
                        State = HttpFileParserState.NewRequest;
                        currentRequest = null;
                        continue;
                    }

                    currentRequest.RequestBody = currentRequest.RequestBody.Length == 0 ? line : currentRequest.RequestBody + '\n' + line;
                }
            }
        }

        return result;
    }

    private RequestVerb? ParseVerb(string verb)
    {
        switch (verb.ToUpperInvariant())
        {
            case "DELETE": return RequestVerb.Delete;
            case "GET": return RequestVerb.Get;
            case "HEAD": return RequestVerb.Head;
            case "POST": return RequestVerb.Post;
            case "PUT": return RequestVerb.Put;
        }

        return null;
    }
}

