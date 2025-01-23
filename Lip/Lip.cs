using Lip.Context;

namespace Lip;

/// <summary>
/// The main class of the Lip library.
/// </summary>
/// <param name="runtimeConfig">The runtime configuration.</param>
/// <param name="context">The context.</param>
public partial class Lip(RuntimeConfig runtimeConfig, IContext context)
{
    private readonly IContext _context = context;
    private readonly PathManager _pathManager = new(context.FileSystem, runtimeConfig.Cache);
    private readonly RuntimeConfig _runtimeConfig = runtimeConfig;
}
