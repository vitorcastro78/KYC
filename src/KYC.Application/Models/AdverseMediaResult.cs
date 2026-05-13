namespace KYC.Application.Models;

public record AdverseMediaHit(string Title, string Url, DateTime? PublishedAt, string Sentiment);

public record AdverseMediaResult(IReadOnlyList<AdverseMediaHit> Hits);
