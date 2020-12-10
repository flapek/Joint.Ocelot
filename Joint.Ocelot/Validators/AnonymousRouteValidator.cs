using Joint.Ocelot.Options;
using System.Collections.Generic;
using System.Linq;

namespace Joint.Ocelot.Validators
{
    public sealed class AnonymousRouteValidator : IAnonymousRouteValidator
    {
        private readonly HashSet<string> _routes;

        public AnonymousRouteValidator(AnonymousRoutesOptions options) 
            => _routes = new HashSet<string>(options.Routes ?? Enumerable.Empty<string>());

        public bool HasAccess(string path) => _routes.Contains(path);
    }
}
