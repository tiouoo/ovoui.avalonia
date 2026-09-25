namespace OvoUi.Common.Converter;

public interface IMarkupExtension<out TReturn>
{
    public TReturn ProvideValue(IServiceProvider serviceProvider);
}

public interface IMarkupExtension : IMarkupExtension<object>;