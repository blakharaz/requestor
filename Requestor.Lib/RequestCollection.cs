namespace Requestor.Lib;

public class RequestCollection
{
    public IDictionary<string, string> Variables { get; } = new Dictionary<string, string>();

    public IList<RequestSettings> Requests { get; } = new List<RequestSettings>();
}

