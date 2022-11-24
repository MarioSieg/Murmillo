namespace Murmillo.Core;

[Flags]
public enum DefaultFeatures : uint
{
    None = 0,
    Controllers = 1 << 0,
    Swagger = 1 << 1,
    Authorization = 1 << 2,
    Https = 1 << 3,
    Logging = 1 << 4,
    All = Controllers | Swagger | Authorization | Https | Logging
}