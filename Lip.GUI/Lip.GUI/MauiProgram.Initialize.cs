using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Lip.GUI;

partial class MauiProgram
{
    private static class DefaultSettings
    {
        public const string ConfigFileName = "config.json";

        public const string PluginsDir = "plugins";

        public const string DataDirectory = ".lipui";

        public const string ConfigsDirectory = DataDirectory + "/configs";

        public const string IconsDirectory = DataDirectory + "/icons";
    }


    static MauiProgram() => Initialize();

    internal static Config Config
    {
        get => field ?? throw new NullReferenceException();
        private set
        {
            if (field is not null)
                field.PropertyChanged -= ConfigPropertyChanged;

            field = value;
            field.PropertyChanged += ConfigPropertyChanged;
        }
    }

    private static void ConfigPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    => ConfigChanged();

    public static string WorkingDirectory { get; private set; }

    public static string ProgramDirectory { get; private set; }

    public static string IconsDirectory { get; private set; }


    private static bool s_configChanged = false;
    private static uint s_configEditCount = 0;
    private static readonly Lock s_lock = new();
    private static bool s_isSaving = false;
    private static bool s_isSaveRequesting = false;

    [MemberNotNull(nameof(Config), nameof(WorkingDirectory), nameof(ProgramDirectory), nameof(IconsDirectory))]
    private static void Initialize()
    {
        InitializeWorkingDir();
        InitializeConfig();
    }

    [MemberNotNull(nameof(WorkingDirectory), nameof(ProgramDirectory), nameof(IconsDirectory))]
    private static void InitializeWorkingDir()
    {
        string currentDir = FileSystem.Current.AppDataDirectory;

        ProgramDirectory = currentDir;

        string path = Path.Combine(currentDir, DefaultSettings.DataDirectory);
        if (Directory.Exists(path) is false)
        {
            Directory.CreateDirectory(path);
        }
        WorkingDirectory = path;

        path = Path.Combine(currentDir, DefaultSettings.IconsDirectory);
        if (Directory.Exists(path) is false)
        {
            Directory.CreateDirectory(path);
        }
        IconsDirectory = path;
    }

    [MemberNotNull(nameof(Config))]
    private static void InitializeConfig()
    {
        string path = Path.Combine(WorkingDirectory, DefaultSettings.ConfigFileName);
        if (File.Exists(path))
        {
            string str = File.ReadAllText(path);
            Config = JsonSerializer.Deserialize<Config>(str) ?? throw new NullReferenceException();
        }
        else
        {
            Config = new Config() { Servers = [] };
        }
    }

    private static void ConfigChanged()
    {
        //s_configChanged = true;
        //s_configEditCount++;

        //if (s_configEditCount >= 0xf)
        Task.Run(SaveConfig);
    }

    internal static void SaveConfig()
    {
        if (s_isSaving)
        {
            s_isSaveRequesting = true;
            return;
        }

        lock (s_lock)
        {
            s_isSaving = true;
            if (s_configChanged)
            {
                string path = Path.Combine(WorkingDirectory, DefaultSettings.ConfigFileName);
                if (File.Exists(path)) File.Delete(path);

                using FileStream file = File.Create(path);
                using var writer = new StreamWriter(file);

                writer.Write(Config.Serialize());

                s_configChanged = false;
                s_configEditCount = 0;
            }

            if (s_isSaveRequesting)
            {
                s_isSaveRequesting = false;
                SaveConfig();
            }

            s_isSaving = false;
        }
    }
}
