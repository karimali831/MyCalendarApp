using System;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Appology.Helpers;
using Appology.MiCalendar.Helpers;
using Appology.MiCalendar.Service;
using Appology.Model;
using Appology.Service;
using Appology.Write.Service;

namespace Appology.Controllers.Api
{
    [RoutePrefix("api/admin")]
    [CamelCaseControllerConfig]
    public class UserController : ApiController
    {
        private readonly IUserService userService;
        private readonly ITypeService typeService;
        private readonly IDocumentService documentService;
        private readonly IEventService eventService;
        private readonly IFeatureRoleService featureRoleService;
        private readonly ICacheService cache;
        private readonly string rootUrl = ConfigurationManager.AppSettings["RootUrl"];

        public UserController(
            IUserService userService, 
            ITypeService typeService, 
            IEventService eventService,
            IDocumentService documentService,
            IFeatureRoleService featureRoleService,
            ICacheService cache)
        {
            this.userService = userService ?? throw new ArgumentNullException(nameof(userService));
            this.typeService = typeService ?? throw new ArgumentNullException(nameof(typeService));
            this.eventService = eventService ?? throw new ArgumentNullException(nameof(eventService));
            this.documentService = documentService ?? throw new ArgumentNullException(nameof(documentService));
            this.featureRoleService = featureRoleService ?? throw new ArgumentNullException(nameof(featureRoleService));
            this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        private async Task<User> GetUser()
        {
            bool isLocal = this.rootUrl == "http://localhost:53822";
            return await userService.GetUser(isLocal ? "karimali831@googlemail.com" : null);
        }

        [Route("user")]
        [HttpGet]
        public async Task<HttpResponseMessage> LoadUser()
        {
            var user = await GetUser();
            return Request.CreateResponse(HttpStatusCode.OK, user);
        }

        [Route("users")]
        [HttpGet]
        public async Task<HttpResponseMessage> GetUsers()
        {
            var users = await userService.GetAllAsync();

            //limit=10&offset=0&order=%7B"order":"desc","orderBy":"createdAt"%7D

            return Request.CreateResponse(HttpStatusCode.OK, new
            {
                users = users.Select(x => new
                {
                    id = x.UserID,
                    firstName = x.Name.Contains(' ') ? x.Name.Split(' ')[0] : x.Name,
                    lastName = x.Name.Contains(' ') ? x.Name.Split(' ')[1] : "",
                    displayName = x.Name,
                    userName = (string)null,
                    email = x.Email,
                    password = "",
                    avatarUrl = CalendarUtils.AvatarSrc(x.UserID, x.Avatar, x.Name, absoluteUrl: true),
                    globalRole = ""
                }),
                count = users.Count()
            });
        }
    }
}
