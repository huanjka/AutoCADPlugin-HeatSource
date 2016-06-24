using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HeatSource.Utils;

namespace HeatSource.View
{
    class Constants
    {
        public const string CATEGORY_DATABASE = "数据库";
        public const string CATEGORY_FORMULA = "公式计算";
        public const string CATEGORY_INPUT = "用户输入";

        public const string CATEGORY_DETAIL = "详细";

        public const string CATEGORY_UI = "UI设置";

        //public static List<string> DeviceFee = new List<string>() { "锅炉造价", "换热器造价", "水泵造价", "管线造价", "附属", "设备总费用" };
        //public static List<string> HeatFee = new List<string>() { "供热区域", "住宅类型", "面积", "采暖热指标", "全年耗热量", "燃气总费用" };
        //public static List<string> WaterFee = new List<string>() { "水泵运行能耗", "电费单价", "耗电总费用" };

        public static List<string> BuildingType = new List<string>() { "住宅", "学校,办公", "医院,幼托", "旅店", "商店", "食堂,餐厅", "影剧院,展览馆", "大礼堂,体育馆" };
        public static List<string> HeatStyle = new List<string>() { "地板供暖", "空调供暖", "散热器采暖", "其他" };
        public static List<string> SaveStyle = new List<string>() { "采取节能措施", "未采取节能措施"};
        public static List<string> LocationType = new List<string>() { "严寒", "寒冷", "夏热冬冷"};
        public static List<string> Province = new List<string>() { "北京市", "天津市", "上海市", "重庆市", "河北省", 
        "山西省", "陕西省","山东省","河南省","辽宁省","吉林省","黑龙江省","江苏省", "浙江省","安徽省","江西省","福建省","湖北省","湖南省","四川省", 
        "贵州省", "云南省","广东省","海南省","甘肃省", "青海省","台湾省","内蒙古自治区", "新疆维吾尔自治区","西藏自治区","广西壮族自治区","宁夏回族自治区","香港特别行政区","澳门特别行政区",};
        public static List<List<string>> City = new List<List<string>>()
        {
            { new List<string> {"北京市"}},
            { new List<string> {"天津市"}},
            { new List<string> {"上海市"}},
            { new List<string> {"重庆市"}},
            { new List<string> {"石家庄市","唐山市","保定市","邢台市","张家口市","承德市"}},
            { new List<string> {"太原市","大同市","运城市","晋城市","阳泉市"}},
            { new List<string> {"西安市","咸阳市","宝鸡市","汉中市","安康市","榆林市","延安市"}},
            { new List<string> {"济南市","德州市"}},
            { new List<string> {"郑州市","安阳市","洛阳市"}},
            { new List<string> {"大连市","沈阳市","丹东市","鞍山市","抚顺市","阜新市","本溪市"}},
            { new List<string> {"长春市","通化市","四平市"}},
            { new List<string> {"齐齐哈尔市","哈尔滨市","大庆市","佳木斯市","牡丹江市","伊春市"}},
            { new List<string> {"徐州市","盐城市","南京市","南通市"}},
            { new List<string> {"宁波市","杭州市"}},
            { new List<string> {"合肥市","安庆市","蚌埠市"}},
            { new List<string> {"南昌市","赣州市","九江市"}},
            { new List<string> {"武汉市","宜昌市","黄石市"}},
            { new List<string> {"长沙市","株洲市","永州市","岳阳市"}},
            { new List<string> {"成都市","绵阳市","南充市","宜宾市"}},
            { new List<string> {"贵阳市","遵义市"}},
            { new List<string> {"韶关市"}},
            { new List<string> {"兰州市","天水市","酒泉市","平凉市"}},
            { new List<string> {"赤峰市","包头市","通辽市","呼和浩特市","鄂尔多斯市"}},
            { new List<string> {"乌鲁木齐市","哈密地区","克拉玛依市","吐鲁番地区"}},
            { new List<string> {"拉萨市","日喀则地区","那曲地区"}},
            { new List<string> {"桂林市"}},
            { new List<string> {"银川市"}}
        };

        // 地区冷热类型划分
        // 严寒地区
        public static List<string> SeverColdAreas = new List<string>() { "博克图", "伊春市", "呼玛", "海拉尔", "满洲里", "阿尔山市", "玛多", "海伦", "齐齐哈尔市", "富锦", "哈尔滨市", "牡丹江市", "大庆市", "安达", "佳木斯市", "二连浩特市", "多伦", "大柴旦", "阿勒泰地区", "那曲地区", "长春市", "四平市", "通化市", "延吉市", "通辽市", "抚顺市", "阜新市", "沈阳市", "本溪市", "鞍山市", "呼和浩特市", "包头市", "鄂尔多斯市", "赤峰市", "额济纳旗", "大同市", "乌鲁木齐市", "克拉玛依市", "酒泉市", "西宁", "日喀则地区", "甘孜", "康定市" };

        // 寒冷地区
        public static List<string> ColdAreas = new List<string>() { "丹东市", "大连市", "张家口市", "承德市", "唐山市", "靑岛市", "洛阳市", "太原市", "阳泉市", "晋城市", "天水市", "榆林市", "延安市", "宝鸡市", "银川市", "平凉市", "兰州市", "喀什市", "伊宁市", "阿坝市", "拉萨市", "林芝市", "北京市", "天津市", "石家庄市", "保定市", "邢台市", "济南市", "德州市", "兖州市", "郑州市", "安阳市", "徐州市", "运城市", "西安市", "咸阳市", "吐鲁番地区", "库尔勒市", "哈密地区" };

        // 夏热冬冷地区
        public static List<string> SummerHotWinterColdAreas = new List<string>() { "南京市", "蚌埠市", "盐城市", "南通市", "合肥市", "安庆市", "九江市", "武汉市", "黄石市", "岳阳市", "汉中市", "安康市", "上海市", "杭州市", "宁波市", "宜昌市", "长沙市", "南昌市", "株洲市", "永州市", "赣州市", "韶关市", "桂林市", "重庆市", "达县市", "万州市", "涪陵市", "南充市", "宜宾市", "成都市", "贵阳市", "遵义市", "凯里市", "绵阳市" };

        // 采暖热指标，直接按地区冷热类型分类
        // 严寒地区
        // [8,2] = [8种建筑类型， 是否采取节能措施(采取节能措施， 未采取节能措施)]
        public static int[,] SeverColdAreasHeatingIndex = new int [8,2]{ { 50,64}, { 70,80}, { 70,80}, { 60,70}, { 70, 80}, { 130, 140}, { 105, 115}, { 150, 165} };

        // 寒冷地区
        public static int[,] ColdAreasHeatingIndex = new int[8, 2] { { 45,60}, {60,70 }, { 60,70} , { 55,65}, { 60,70}, { 115,130}, { 95,105}, { 125,140} };

        // 夏热冬冷地区
        public static int[,] SummerHotWinterColdAreasHeatingIndex = new int[8, 2] { {40,58 }, { 45,60}, { 55,65}, { 50,60}, { 55,65}, { 100,115}, { 80,95}, { 100, 115} };

        //add pantianxiang
        public static void InitilizeConstant()
        {
            //从excel加载城市列表
            City = ExcelUtil.citylist();

            //从excel加载城市的热指标系数
            SeverColdAreas = ExcelUtil.getSeverColdAreas();
            ColdAreas = ExcelUtil.getColdAreas();
            SummerHotWinterColdAreas = ExcelUtil.getSummerHotWinterColdAreas();
        }
    }
}
