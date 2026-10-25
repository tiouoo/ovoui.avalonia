using System;

namespace OvoUi.Lang;

public sealed class LangKey
{
    public string Key { get; }

    public ILangManager Manager { get; }

    public LangKey(string key, ILangManager manager)
    {
        Key = key ?? throw new ArgumentNullException(nameof(key));
        Manager = manager ?? throw new ArgumentNullException(nameof(manager));
    }

    public static implicit operator string?(LangKey? langKey) => langKey?.Key;

    public override string ToString() => Key;
}