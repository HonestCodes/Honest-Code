using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using GSwap.Services;
using GSwap.Models.Responses;
using GSwap.Services.Security;
using System.Security.Principal;
using GSwap.Models;
using System.Web;
using GSwap.Models.Domain.Files;

namespace GSwap.Web.Controllers.Api.Common.Files
{
    [RoutePrefix("api/files")]
    public class FilesAPIController : ApiController
    {
        private IFileService _fileService;
        private IUserAuthData _currentUser;
        public IPrincipal _principal = null;


        public FilesAPIController(IFileService fileService, IPrincipal user) {
            _principal = user;
            _fileService = fileService;
            _currentUser = _principal.Identity.GetCurrentUser();
        }
        
        [Route("cooks/photos"), HttpPost(), Authorize(Roles = "User")]
        public HttpResponseMessage UploadFiles()
        {
            HttpFileCollection hfc = HttpContext.Current.Request.Files; 


            HttpStatusCode code = HttpStatusCode.OK;

            ItemsResponse<string> response = new ItemsResponse<string>();

            response.Items = _fileService.UploadFiles(hfc);
          
            // make association here

           
                if (response.Items == null)
                {
                    code = HttpStatusCode.BadRequest;
                    response.IsSuccessful = false;
                }

                return Request.CreateResponse(code, response);
            
        }

        [Route("cooks/account/photo"), HttpPost(), Authorize(Roles = "Chef")]
        public HttpResponseMessage CookProfilePhoto()
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }

            HttpFileCollection hfc = HttpContext.Current.Request.Files;

            HttpPostedFile file = hfc[0];

            ItemResponse<int> response = new ItemResponse<int>();

            int newId = _fileService.CookProfilePhoto(file, _currentUser.Id);
            response.Item = newId;

            return Request.CreateResponse(HttpStatusCode.OK, response);


        }


        [Route("cooks/account/photo/{id:int}"), HttpGet()]
        public HttpResponseMessage CookFileInfo(int id)
        {
            HttpStatusCode code = HttpStatusCode.OK;

            ItemResponse<File> response = new ItemResponse<File>();

            response.Item = _fileService.CookFileInfo(id);
            if (response.Item == null)
            {
                code = HttpStatusCode.NotFound;
                response.IsSuccessful = false;
            }

            return Request.CreateResponse(code, response);
        }


        [Route("meal/{mealId:int}"), HttpPost(), Authorize(Roles = "Chef")]
        public HttpResponseMessage AddMealPhotos(int mealId)
        {
            HttpFileCollection hfc = HttpContext.Current.Request.Files;


            HttpStatusCode code = HttpStatusCode.OK;

            ItemsResponse<int> response = new ItemsResponse<int>();

            response.Items = _fileService.UploadMealPhotos(hfc, mealId, _currentUser.Id);

            if (response.Items == null)
            {
                code = HttpStatusCode.BadRequest;
                response.IsSuccessful = false;
            }

            return Request.CreateResponse(code, response);
        }


        [Route("meal/{mealId:int}"), HttpGet, Authorize(Roles = "Chef")]
        public HttpResponseMessage GetMealPhotos(int mealId)
        {
            
            HttpStatusCode code = HttpStatusCode.OK;

            ItemsResponse<File> response = new ItemsResponse<File>();

            response.Items = _fileService.GetAllPhotosById(mealId, _currentUser.Id);

            if (response.Items == null)
            {
                code = HttpStatusCode.BadRequest;
                response.IsSuccessful = false;
            }

            return Request.CreateResponse(code, response);
        }

        [Route("uploaded"), HttpGet()]
        public HttpResponseMessage GetFilesForLoggedInUser()
        {
            HttpStatusCode code = HttpStatusCode.OK;

            ItemsResponse<File> response = new ItemsResponse<File>();

            response.Items = _fileService.GetFilesForLoggedInUser(_currentUser.Id);
            if (response.Items == null)
            {
                code = HttpStatusCode.NotFound;
                response.IsSuccessful = false;
            }

            return Request.CreateResponse(code, response);
        }
    }
}
