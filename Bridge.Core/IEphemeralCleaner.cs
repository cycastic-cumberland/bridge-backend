namespace Bridge.Core;

public interface IEphemeralCleaner
{
    Type EphemeralType { get; }
    
    Task CleanUpAsync(CancellationToken cancellationToken);
}

public interface IEphemeralCleaner<T> : IEphemeralCleaner
{
    Type IEphemeralCleaner.EphemeralType => typeof(T);
}