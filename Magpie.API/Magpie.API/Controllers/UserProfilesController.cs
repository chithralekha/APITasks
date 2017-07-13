using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Magpie.Mapping;
using Magpie.Model;
using Magpie.Repository;

namespace Magpie.API.Controllers
{
    public class UserProfilesController : ApiController
    {
        IRepository<User> userRepository;

        public UserProfilesController()
        {
            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["MagpieClient"].ConnectionString;
            userRepository = new UserRepository(connectionString);
        }

        public UserProfilesController(IRepository<User> Repository)
        {
            #region Preconditions

            if (userRepository == null)
                throw new ArgumentNullException();

            #endregion

            userRepository = Repository;
        }

        [Authorize]
        [Route("api/UserProfiles")]
        public IHttpActionResult Get()
        {
            #region Preconditions

            if (userRepository == null)
                throw new InvalidOperationException();

            #endregion

            try
            {
                var users = ((UserRepository)userRepository).GetUserProfiles();

                var dtoUsers = users.Select(cs => UserMapper.TranslateModelUserToDTOUser(cs));

                return Ok(dtoUsers);
            }
            catch (Exception)
            {
                return InternalServerError();
            }
        }

        [Authorize]
        [Route("api/UserProfiles/{id}")]
        public IHttpActionResult Get(string id)
        {
            #region Preconditions

            if (userRepository == null)
                throw new InvalidOperationException();

            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentOutOfRangeException();

            #endregion

            try
            {
                var user = ((UserRepository)userRepository).GetUserProfile(id);

                var dtoUser = UserMapper.TranslateModelUserToDTOUser(user);

                return Ok(dtoUser);
            }
            catch (Exception)
            {
                return InternalServerError();
            }
        }

        [Authorize]
        [Route("api/UserProfiles", Name = "AddUserProfile")]
        public HttpResponseMessage Post([FromBody]DTO.User dtoUser)
        {
            #region Preconditions

            if (userRepository == null)
                throw new InvalidOperationException();

            if (dtoUser == null)
                throw new ArgumentNullException();

            if (!string.IsNullOrWhiteSpace(dtoUser.Id))
                throw new ArgumentOutOfRangeException();

            #endregion

            dtoUser.Id = null;

            var user = UserMapper.TranslateDTOUserToModelUser(dtoUser);

            string newId = ((UserRepository)userRepository).AddUserProfile(user);

            if (string.IsNullOrWhiteSpace(newId))
                throw new HttpResponseException(HttpStatusCode.InternalServerError);

            dtoUser.Id = newId;

            var response = Request.CreateResponse<DTO.User>(HttpStatusCode.Created, dtoUser);

            string uri = Url.Link("AddUserProfile", new { id = dtoUser.Id });
            response.Headers.Location = new Uri(uri);
            return response;
        }

        [Authorize]
        [Route("api/UserProfiles/{id}")]
        public void Put(string id, [FromBody]DTO.User dtoUser)
        {
            #region Preconditions

            if (userRepository == null)
                throw new InvalidOperationException();

            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentOutOfRangeException();

            if (dtoUser == null)
                throw new ArgumentNullException();

            #endregion

            dtoUser.Id = id;

            var user = UserMapper.TranslateDTOUserToModelUser(dtoUser);

            if (!((UserRepository)userRepository).UpdateUserProfile(user))
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
        }

        [Authorize]
        [Route("api/Roles")]
        public IHttpActionResult GetRoles()
        {
            #region Preconditions

            if (userRepository == null)
                throw new InvalidOperationException();

            #endregion

            try
            {
                var roles = ((UserRepository)userRepository).GetRoles(false);

                var dtoRoles = UserMapper.TranslateModelRoleListToDTORoleList(roles);

                return Ok(dtoRoles);
            }
            catch (Exception)
            {
                return InternalServerError();
            }
        }

        [Authorize]
        [Route("api/PhoneCarriers")]
        public IHttpActionResult GetPhoneCarriers()
        {
            #region Preconditions

            if (userRepository == null)
                throw new InvalidOperationException();

            #endregion

            try
            {
                var phoneCarriers = ((UserRepository)userRepository).GetPhoneCarriers();

                var dtoPhoneCarriers = UserMapper.TranslateModelPhoneCarrierListToDTOPhoneCarrierList(phoneCarriers);

                return Ok(dtoPhoneCarriers);
            }
            catch (Exception)
            {
                return InternalServerError();
            }
        }
    }
}
