namespace Polchan.Application.Files;

public record FileUpload(Stream Stream, string FileName, string ContentType);
