using Lip.CLI;
using Spectre.Console;
using Spectre.Console.Cli;

var commandApp = new CommandApp();

commandApp.SetDefaultCommand<CommandRoot>();

commandApp.Configure(config =>
{
    config.SetApplicationName("lip");

    config.PropagateExceptions();

    config.AddBranch<CacheSettings>("cache", config =>
    {
        config.SetDescription("Inspect and manage lip's cache.");

        config.AddCommand<CacheAddCommand>("add");
        config.AddCommand<CacheCleanCommand>("clean");
        config.AddCommand<CacheListCommand>("list");
    });
    config.AddBranch<ConfigSettings>("config", config =>
    {
        config.SetDescription("Manage the lip configuration files.");

        config.AddCommand<ConfigDeleteCommand>("delete");
        config.AddCommand<ConfigGetCommand>("get");
        config.AddCommand<ConfigListCommand>("list");
        config.AddCommand<ConfigSetCommand>("set");
    });
    config.AddCommand<InitCommand>("init");
    config.AddCommand<InstallCommand>("install");
    config.AddCommand<ListCommand>("list");
    config.AddCommand<MigrateCommand>("migrate");
    config.AddCommand<PackCommand>("pack");
    config.AddCommand<PruneCommand>("prune");
    config.AddCommand<RunCommand>("run");
    config.AddCommand<UninstallCommand>("uninstall");
    config.AddCommand<UpdateCommand>("update");
    config.AddCommand<ViewCommand>("view");
});

try
{
    var result = await commandApp.RunAsync(args);

    // To make sure that the progress bar is displayed correctly.
    await Task.Delay(10);

    return result;
}
catch (Exception ex)
{
    AnsiConsole.WriteException(ex);
    return 1;
}