namespace FEwS.Forums.Storage;

internal interface IGuidFactory
{
    Guid Create();
}

internal class GuidFactory : IGuidFactory
{
    public Guid Create()
    {
        return Guid.NewGuid();
    }
}