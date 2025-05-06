// This file is part of OssObjectClientProxy, you can customize it here


using Volo.Abp.Content;
// ReSharper disable once CheckNamespace
using System.Threading.Tasks;

namespace LINGYUN.Abp.OssManagement;

public partial class OssObjectClientProxy
{
    public Task<string> GenerateUrlAsync(GetOssObjectInput input)
    {
        // TODO: Implement URL generation logic
        throw new System.NotImplementedException();
    }

    public Task<IRemoteStreamContent> DownloadAsync(string urlKey)
    {
        // TODO: Implement download logic
        throw new System.NotImplementedException();
    }
}
