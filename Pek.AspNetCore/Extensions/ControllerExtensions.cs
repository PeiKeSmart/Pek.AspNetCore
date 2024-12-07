using Microsoft.AspNetCore.Mvc;

using Pek.Mime;
using Pek.ResumeFileResult;

namespace Pek;

/// <summary>
/// Controller扩展方法
/// </summary>
public static class ControllerExtensions
{
    private static readonly IMimeMapper _mimeMapper = new MimeMapper();
    /// <summary>
    /// 可断点续传和多线程下载的FileResult
    /// </summary>
    /// <param name="controller"></param>
    /// <param name="fileContents">文件二进制流</param>
    /// <param name="contentType">Content-Type</param>
    /// <param name="fileDownloadName">下载的文件名</param>
    /// <returns></returns>
    public static ResumeFileContentResult ResumeFile(this ControllerBase controller, Byte[] fileContents, String contentType, String fileDownloadName) => ResumeFile(controller, fileContents, contentType, fileDownloadName, null);

    /// <summary>
    /// 可断点续传和多线程下载的FileResult
    /// </summary>
    /// <param name="controller"></param>
    /// <param name="fileContents">文件二进制流</param>
    /// <param name="fileDownloadName">下载的文件名</param>
    /// <returns></returns>
    public static ResumeFileContentResult ResumeFile(this ControllerBase controller, Byte[] fileContents, String fileDownloadName) => ResumeFile(controller, fileContents, _mimeMapper.GetMimeFromPath(fileDownloadName), fileDownloadName, null);

    /// <summary>
    /// 可断点续传和多线程下载的FileResult
    /// </summary>
    /// <param name="controller"></param>
    /// <param name="fileContents">文件二进制流</param>
    /// <param name="contentType">Content-Type</param>
    /// <param name="fileDownloadName">下载的文件名</param>
    /// <param name="etag">ETag</param>
    /// <returns></returns>
    public static ResumeFileContentResult ResumeFile(this ControllerBase controller, Byte[] fileContents, String? contentType, String fileDownloadName, String? etag)
    {
        return new ResumeFileContentResult(fileContents, contentType, etag)
        {
            FileDownloadName = fileDownloadName
        };
    }

    /// <summary>
    /// 可断点续传和多线程下载的FileResult
    /// </summary>
    /// <param name="controller"></param>
    /// <param name="fileStream">文件二进制流</param>
    /// <param name="contentType">Content-Type</param>
    /// <param name="fileDownloadName">下载的文件名</param>
    /// <returns></returns>
    public static ResumeFileStreamResult ResumeFile(this ControllerBase controller, FileStream fileStream, String contentType, String fileDownloadName) => ResumeFile(controller, fileStream, contentType, fileDownloadName, null);

    /// <summary>
    /// 可断点续传和多线程下载的FileResult
    /// </summary>
    /// <param name="controller"></param>
    /// <param name="fileStream">文件二进制流</param>
    /// <param name="fileDownloadName">下载的文件名</param>
    /// <returns></returns>
    public static ResumeFileStreamResult ResumeFile(this ControllerBase controller, FileStream fileStream, String fileDownloadName) => ResumeFile(controller, fileStream, _mimeMapper.GetMimeFromPath(fileDownloadName), fileDownloadName, null);

    /// <summary>
    /// 可断点续传和多线程下载的FileResult
    /// </summary>
    /// <param name="controller"></param>
    /// <param name="fileStream">文件二进制流</param>
    /// <param name="contentType">Content-Type</param>
    /// <param name="fileDownloadName">下载的文件名</param>
    /// <param name="etag">ETag</param>
    /// <returns></returns>
    public static ResumeFileStreamResult ResumeFile(this ControllerBase controller, FileStream fileStream, String? contentType, String fileDownloadName, String? etag)
    {
        return new ResumeFileStreamResult(fileStream, contentType, etag)
        {
            FileDownloadName = fileDownloadName
        };
    }

    /// <summary>
    /// 可断点续传和多线程下载的FileResult
    /// </summary>
    /// <param name="controller"></param>
    /// <param name="virtualPath">服务端本地文件的虚拟路径</param>
    /// <param name="contentType">Content-Type</param>
    /// <param name="fileDownloadName">下载的文件名</param>
    /// <returns></returns>
    public static ResumeVirtualFileResult ResumeFile(this ControllerBase controller, String virtualPath, String contentType, String fileDownloadName) => ResumeFile(controller, virtualPath, contentType, fileDownloadName, null);

    /// <summary>
    /// 可断点续传和多线程下载的FileResult
    /// </summary>
    /// <param name="controller"></param>
    /// <param name="virtualPath">服务端本地文件的虚拟路径</param>
    /// <param name="fileDownloadName">下载的文件名</param>
    /// <returns></returns>
    public static ResumeVirtualFileResult ResumeFile(this ControllerBase controller, String virtualPath, String fileDownloadName) => ResumeFile(controller, virtualPath, _mimeMapper.GetMimeFromPath(virtualPath), fileDownloadName, null);

    /// <summary>
    /// 可断点续传和多线程下载的FileResult
    /// </summary>
    /// <param name="controller"></param>
    /// <param name="virtualPath">服务端本地文件的虚拟路径</param>
    /// <param name="contentType">Content-Type</param>
    /// <param name="fileDownloadName">下载的文件名</param>
    /// <param name="etag">ETag</param>
    /// <returns></returns>
    public static ResumeVirtualFileResult ResumeFile(this ControllerBase controller, String virtualPath, String? contentType, String fileDownloadName, String? etag)
    {
        return new ResumeVirtualFileResult(virtualPath, contentType, etag)
        {
            FileDownloadName = fileDownloadName
        };
    }

    /// <summary>
    /// 可断点续传和多线程下载的FileResult
    /// </summary>
    /// <param name="controller"></param>
    /// <param name="physicalPath">服务端本地文件的物理路径</param>
    /// <param name="contentType">Content-Type</param>
    /// <param name="fileDownloadName">下载的文件名</param>
    /// <returns></returns>
    public static ResumePhysicalFileResult ResumePhysicalFile(this ControllerBase controller, String physicalPath, String contentType, String fileDownloadName) => ResumePhysicalFile(controller, physicalPath, contentType, fileDownloadName, etag: null);

    /// <summary>
    /// 可断点续传和多线程下载的FileResult
    /// </summary>
    /// <param name="controller"></param>
    /// <param name="physicalPath">服务端本地文件的物理路径</param>
    /// <param name="fileDownloadName">下载的文件名</param>
    /// <returns></returns>
    public static ResumePhysicalFileResult ResumePhysicalFile(this ControllerBase controller, String physicalPath, String fileDownloadName) => ResumePhysicalFile(controller, physicalPath, _mimeMapper.GetMimeFromPath(physicalPath), fileDownloadName, etag: null);

    /// <summary>
    /// 可断点续传和多线程下载的FileResult
    /// </summary>
    /// <param name="controller"></param>
    /// <param name="physicalPath">服务端本地文件的物理路径</param>
    /// <param name="contentType">Content-Type</param>
    /// <param name="fileDownloadName">下载的文件名</param>
    /// <param name="etag">ETag</param>
    /// <returns></returns>
    public static ResumePhysicalFileResult ResumePhysicalFile(this ControllerBase controller, String physicalPath, String? contentType, String fileDownloadName, String? etag)
    {
        return new ResumePhysicalFileResult(physicalPath, contentType, etag)
        {
            FileDownloadName = fileDownloadName
        };
    }
}