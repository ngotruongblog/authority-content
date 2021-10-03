using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Contents;

namespace OrchardCore.AuthorityContent
{
    public static class AuthorityContents
    {
        public static async Task FilterResult(
            IAuthorizationService _authorizationService,
            IContentDefinitionManager _contentDefinitionManager,
            IContentManager _contentManager,
            ExecutionResult result, HttpContext context)
        {
            var contentTypeDefinitions = _contentDefinitionManager.ListTypeDefinitions()
                .Where(ctd => ctd.GetSettings<ContentTypeSettings>().Creatable);
            if (result.Data != null)
            {
                var data = (Dictionary<string, object>)result.Data;
                var chas = contentTypeDefinitions.Select(x => x.Name.ToUpper()).ToList();
                var cons = data.Keys.Select(x => x.ToUpper()).ToList();
                var kq = cons.Except(chas);
                if (!kq.Any())
                {
                    foreach (var item in data)
                    {
                        var query = contentTypeDefinitions.Where(x => x.Name.ToUpper() == item.Key.ToUpper());
                        var flag = await _authorizationService.AuthorizeContentTypeDefinitionsAsync(context.User,
                            CommonPermissions.PublishContent, query, _contentManager);
                        if (flag == false)
                        {
                            data.Remove(item.Key);
                        }
                    }
                }
            }
        }
    }
}
