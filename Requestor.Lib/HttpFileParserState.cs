namespace Requestor.Lib;

internal enum HttpFileParserState
{
    NewRequest,
    UrlFound,
    RequestBody
}