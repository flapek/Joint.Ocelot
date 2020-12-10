namespace Joint.Ocelot.Validators
{
    internal interface IAnonymousRouteValidator
    {
        bool HasAccess(string path);
    }
}
