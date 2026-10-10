using Avalonia.Controls;

namespace OvoUi.Demo.Pages;

public partial class AutoCompleteBoxPage : UserControl
{
    public AutoCompleteBoxPage()
    {
        InitializeComponent();
        DataContext = new AutoCompleteBoxDemoViewModel();
    }
}

public class AutoCompleteBoxDemoViewModel
{
    public IReadOnlyList<string> Cities { get; } =
    [
        "BeiJing", "ShangHai", "TianJin", "ChongQing", "ShiJiaZhuang", "TangShan", "QinHuangDao", "HanDan", "BaoDing",
        "ZhangJiaKou",
        "TaiYuan", "DaTong", "ChangZhi", "HuHeHaoTe", "BaoTou", "ErDuoSi", "ShenYang", "DaLian", "AnShan", "JinZhou",
        "ChangChun", "JiLin", "HarBin", "QiQiHaEr", "NanJing", "WuXi", "XuZhou", "ChangZhou", "SuZhou", "NanTong",
        "HangZhou", "NingBo", "WenZhou", "JiaXing", "ShaoXing", "JinHua", "HeFei", "WuHu", "BengBu", "FuZhou",
        "XiaMen", "QuanZhou", "ZhangZhou", "NanChang", "JiuJiang", "GanZhou", "JiNan", "QingDao", "YanTai", "WeiFang",
        "ZhengZhou", "LuoYang", "KaiFeng", "NanYang", "WuHan", "YiChang", "XiangYang", "ChangSha", "ZhuZhou", "YueYang",
        "GuangZhou", "ShenZhen", "ZhuHai", "FoShan", "DongGuan", "ZhongShan", "NanNing", "GuiLin", "HaiKou", "SanYa",
        "ChengDu", "MianYang", "LeShan", "GuiYang", "ZunYi", "KunMing", "DaLi", "LaSa", "XiAn", "BaoJi",
        "LanZhou", "TianShui", "XiNing", "YinChuan", "WuLuMuQi", "KaShi", "HongKong", "Macao", "TaiBei", "GaoXiong"
    ];

    public string SourceCode { get; } = """
                                        public partial class AutoCompleteBoxPage : UserControl
                                        {
                                            public AutoCompleteBoxPage()
                                            {
                                                InitializeComponent();
                                                DataContext = new AutoCompleteBoxDemoViewModel();
                                            }
                                        }

                                        public class AutoCompleteBoxDemoViewModel
                                        {
                                            public IReadOnlyList<string> Cities { get; } =
                                            [
                                                "BeiJing", "ShangHai", "TianJin", "ChongQing", "ShiJiaZhuang",
                                                "ShenYang", "DaLian", "ChangChun", "HarBin", "NanJing",
                                                "SuZhou", "HangZhou", "NingBo", "WenZhou", "HeFei", "FuZhou",
                                                "XiaMen", "NanChang", "JiNan", "QingDao", "ZhengZhou", "LuoYang",
                                                "WuHan", "ChangSha", "GuangZhou", "ShenZhen", "ZhuHai", "FoShan",
                                                "NanNing", "GuiLin", "HaiKou", "SanYa", "ChengDu", "GuiYang",
                                                "KunMing", "LaSa", "XiAn", "LanZhou", "XiNing", "YinChuan",
                                                "WuLuMuQi", "HongKong", "Macao", "TaiBei", "GaoXiong"
                                            ];
                                        }
                                        """;
}