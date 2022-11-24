namespace Murmillo.Core;

public abstract class MurmilloCore
{
    protected MurmilloCore(Type appClassType)
    {
        if (!appClassType.IsClass || appClassType.BaseType != typeof(MurmilloCore))
            throw new InvalidOperationException(
                $"Slave class type must be a class deriving from '{nameof(MurmilloCore)}'");
        AppClassType = appClassType;
    }

    public abstract string AppName { get; }
    public abstract Version AppVersion { get; }
    public abstract string Description { get; }
    public virtual string? Url => null;
    public virtual DefaultFeatures Features => DefaultFeatures.All;
    public Guid Id { get; init; } = Guid.NewGuid();
    public Type AppClassType { get; }
    private WebApplication? AppContextHandle { get; set; }

    private WebApplication App => AppContextHandle ??
                                  throw new InvalidOperationException(
                                      $"App '{AppName} not mounted. Mount using '{nameof(Mount)}'");

    public virtual void Mount(params string[] args)
    {
        Console.WriteLine($"Mounting app '{AppName}' {Id} v.{AppVersion}...");
        var builder = WebApplication.CreateBuilder(args);
        OnBuild(builder);
        AppContextHandle = builder.Build();
        OnMounted(AppContextHandle!);
    }

    public virtual void Install()
    {
        OnInstall(App);
    }

    public virtual void Run(string? url = null)
    {
        Console.WriteLine($"Running Murmillo app '{AppName}' in url '{url ?? "default"}'");
        var app = App;
        OnRun(app);
        app.Run(url ?? Url);
        OnQuit();
    }

    protected virtual void OnBuild(WebApplicationBuilder builder)
    {
        Console.WriteLine("Building app context...");

        void EnableFeature(DefaultFeatures flags, FeatureEnabledCallback<WebApplicationBuilder> function)
        {
            if ((Features & flags) != 0) function(builder);
        }

        EnableFeature(DefaultFeatures.Controllers, builder => builder.Services.AddControllers());
        EnableFeature(DefaultFeatures.Swagger, builder =>
        {
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
        });
        EnableFeature(DefaultFeatures.Logging, builder =>
        {
            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();
        });
    }

    protected virtual void OnMounted(WebApplication webApp)
    {
        Console.WriteLine("Mounting app context...");

        void EnableFeature(DefaultFeatures flags, FeatureEnabledCallback<WebApplication> function)
        {
            if ((Features & flags) != 0) function(webApp);
        }

        EnableFeature(DefaultFeatures.Controllers, app =>
        {
            if (!app.Environment.IsDevelopment()) return;
            app.UseSwagger();
            app.UseSwaggerUI();
        });

        EnableFeature(DefaultFeatures.Https, app => app.UseHttpsRedirection());
        EnableFeature(DefaultFeatures.Authorization, app => app.UseAuthorization());
        EnableFeature(DefaultFeatures.Controllers, app => app.MapControllers());
    }

    protected virtual void OnInstall(WebApplication app)
    {
    }

    protected virtual void OnRun(WebApplication app)
    {
    }

    protected virtual void OnQuit()
    {
    }

    public override string ToString()
    {
        return AppName;
    }

    public override int GetHashCode()
    {
        return (AppName.GetHashCode() * 397) ^ (Description.GetHashCode() * 397);
    }

    public override bool Equals(object? obj)
    {
        if (obj == null) return false;
        if (obj.GetType() != GetType()) return false;
        return GetHashCode() == obj.GetType().GetHashCode();
    }

    public static T CreateApp<T>() where T : MurmilloCore, new()
    {
        Console.WriteLine($"Allocating Murmillo app typeof '{typeof(T).Name}'");
        return new T();
    }

    public static T InitializeApp<T>(params string[] args) where T : MurmilloCore, new()
    {
        var app = CreateApp<T>();
        app.Mount(args);
        app.Install();
        return app;
    }

    private delegate void FeatureEnabledCallback<in T>(T arg) where T : class;
}