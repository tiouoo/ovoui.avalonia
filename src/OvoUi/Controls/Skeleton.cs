using Avalonia;
using Avalonia.Controls;

namespace OvoUi.Controls
{
    public class Skeleton : ContentControl
    {
        public static readonly StyledProperty<bool> IsActiveProperty =
            AvaloniaProperty.Register<Skeleton, bool>(nameof(IsActive));

        public static readonly StyledProperty<bool> IsLoadingProperty =
            AvaloniaProperty.Register<Skeleton, bool>(nameof(IsLoading));

        public bool IsActive
        {
            get { return GetValue(IsActiveProperty); }
            set { SetValue(IsActiveProperty, value); }
        }

        public bool IsLoading
        {
            get => GetValue(IsLoadingProperty);
            set => SetValue(IsLoadingProperty, value);
        }
    }
}