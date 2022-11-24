using Murmillo.Core;

namespace Murmillo;

internal sealed class MurmilloApp : MurmilloCore
{
    public MurmilloApp() : base(typeof(MurmilloApp))
    {
    }

    public override string AppName => nameof(MurmilloApp);
    public override Version AppVersion => new(0, 1);
    public override string Description => "Default app template";


    private static void Main(string[] args)
    {
        InitializeApp<MurmilloApp>(args).Run();
    }
}