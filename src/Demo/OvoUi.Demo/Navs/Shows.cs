using System.Collections.Generic;
using OvoUi.Demo.Models;
using OvoUi.Demo.Pages;

namespace OvoUi.Demo.Navs;

public static class Shows
{
    public static readonly List<Page> ShowsList =
    [
        Page.View<FlyoutPage>("Flyout"),
        Page.View<ContentExpanderPage>("ContentExpander"),
        Page.View<GlassCardPage>("GlassCard"),
        Page.View<MarkdownLinePage>("MarkdownLine"),
        Page.View<ShimmerPage>("ShimmerText"),
        Page.View<AvatarPage>("Avatar"),
        Page.View<QrCodePage>("QRCode"),
        Page.View<ImageViewerPage>("ImageViewer"),
        Page.View<ZoomViewPage>("ZoomView"),
        Page.View<DiffViewPage>("DiffView"),
        Page.View<TimelinePage>("Timeline"),
        Page.View<TwoTonePathIconPage>("TwoTonePathIcon"),
        Page.View<NumberDisplayerPage>("NumberDisplayer"),
        Page.View<MarqueePage>("Marquee"),
        Page.View<BannerPage>("Banner"),
        Page.View<BadgePage>("Badge"),
        Page.View<DescriptionsPage>("Descriptions"),
        Page.View<ToolTipPage>("ToolTip"),
        Page.View<DisableContainerPage>("DisableContainer"),
        Page.View<DualBadgePage>("DualBadge"),
        Page.View<ProgressBarPage>("ProgressBar"),
        Page.View<ExpanderPage>("Expander"),
        Page.View<ListBoxPage>("ListBox"),
        Page.View<TreeViewPage>("TreeView"),
        Page.View<CarouselPage>("Carousel"),
    ];
}
