using System;

namespace OvoUi.Lang;

[AttributeUsage(AttributeTargets.Class)]
public sealed class LangManagerAttribute(string resourcePath) : Attribute
{
    public string ResourcePath { get; } = resourcePath;

    public bool AutoDiscover { get; set; } = true;

    public string DefaultCulture { get; set; } = "";
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class LangResourceAttribute(string culture, string resourcePath) : Attribute
{
    public string Culture { get; } = culture;

    public string ResourcePath { get; } = resourcePath;
}