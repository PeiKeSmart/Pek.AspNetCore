namespace Pek.Seo;

/// <summary>
/// 自动生成Sitemap
/// </summary>
public class DHSitemap : Attribute
{
    /// <summary>
    /// 是否开启Sitemap生成
    /// </summary>
    public Boolean IsUse { get; set; }

    /// <summary>
    /// 类型，1为首页
    /// </summary>
    public SiteMap SType { get; set; }

    /// <summary>
    /// 路由数据
    /// </summary>
    public IDictionary<String, Object>? Data { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime? UpdateTime { get; set; }

    /// <summary>
    /// 排序
    /// </summary>
    public Int32 DisplyOrder { get; set; }

    /// <summary>
    /// Url
    /// </summary>
    public String? Url { get; set; }

    /// <summary>
    /// 控制器名称
    /// </summary>
    public String? ControllerName { get; set; }

    /// <summary>
    /// 动作名称
    /// </summary>
    public String? ActionName { get; set; }

    /// <summary>
    /// 语言
    /// </summary>
    public String? UniqueSeoCode { get; set; }

    /// <summary>
    /// 优先级
    /// </summary>
    public Double Priority { get; set; }

    /// <summary>
    /// 更新频率
    /// </summary>
    public SiteMapChangeFreq ChangeFreq { get; set; }

    public DHSitemap()
    {
    }

    /// <summary>
    /// 复制构造函数
    /// </summary>
    /// <param name="sitemap">要复制的DHSitemap对象</param>
    public DHSitemap(DHSitemap sitemap)
    {
        IsUse = sitemap.IsUse;
        SType = sitemap.SType;
        Data = sitemap.Data != null ? new Dictionary<String, Object>(sitemap.Data) : null;
        UpdateTime = sitemap.UpdateTime;
        DisplyOrder = sitemap.DisplyOrder;
        Url = sitemap.Url;
        ControllerName = sitemap.ControllerName;
        ActionName = sitemap.ActionName;
        UniqueSeoCode = sitemap.UniqueSeoCode;
        Priority = sitemap.Priority;
        ChangeFreq = sitemap.ChangeFreq;
    }

    public DHSitemap Clone(DHSitemap sitemap) => new(sitemap);
}