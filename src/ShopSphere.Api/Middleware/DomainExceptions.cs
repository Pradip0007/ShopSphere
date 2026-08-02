namespace ShopSphere.Api.Middleware;

public abstract class DomainException(string message) : Exception(message);

public sealed class ConflictException(string message) : DomainException(message);
public sealed class BusinessRuleException(string message) : DomainException(message);