using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Magpie.Mapping;
using Magpie.Model;
using Magpie.Repository;
using Magpie.Logging;
using Newtonsoft.Json;

namespace Magpie.API.Controllers
{
    public class TasksController : ApiController
    {
        IFilterableRepository<UserTask, UserTaskFilter> userTaskRepository;
        protected ILog logger = new Logging.Logger.Log(typeof(TasksController));
        private IEnumerable<Model.User> users;

        public TasksController()
        {
            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["MagpieClient"].ConnectionString;
            userTaskRepository = new UserTaskRepository(connectionString);

            var userRepository = new UserRepository(connectionString);
            users = ((UserRepository)userRepository).GetItems();
        }

        public TasksController(IFilterableRepository<UserTask, UserTaskFilter> Repository)
        {
            #region Preconditions

            if (userTaskRepository == null)
                throw new ArgumentNullException();

            #endregion

            userTaskRepository = Repository;
        }

        [Authorize]
        [Route("api/Tasks")]
        public IHttpActionResult Get(int? filterId = null, string sortBy = null, string sortOrder = null)
        {
            #region Preconditions

            if (userTaskRepository == null)
                throw new InvalidOperationException();

            #endregion

            try
            {              
                logger.Info($"TasksController Get: {filterId}, sortBy: {sortBy}, sortOrder: {sortOrder}", users.First(u => u.UserName == User.Identity.Name).Id);

                IEnumerable<UserTask> userTasks = null;

                if (filterId != null)
                {
                    // This cast is appropriate, we need the specific UserTaskRepository behaviour here.
                    userTasks = ((UserTaskRepository)userTaskRepository).GetItems(filterId.Value);
                }
                else
                {
                    userTasks = userTaskRepository.GetItems();
                }

                var dtoTasks = userTasks.Select(ut => UserTaskMapper.TranslateModelUserTaskToDTOTask(ut));

                var taskInfoList = new DTO.TaskList
                {
                    Metadata = new DTO.TaskListMetadata
                    {
                        Count = dtoTasks.Count(),
                        FilterId = filterId,
                        SortBy = sortBy,
                        SortOrder = sortOrder
                    },
                    Tasks = dtoTasks
                };

                logger.Info($"TasksController Get taskInfoList Count: {taskInfoList.Tasks.Count()}", users.First(u => u.UserName == User.Identity.Name).Id);

                return Ok(taskInfoList);
            }
            catch (Exception ex)
            {
                logger.Error($"TasksController Get Error: {ex.Message}", users.First(u => u.UserName == User.Identity.Name).Id);
                return InternalServerError();
            }
        }

        [Authorize]
        [Route("api/Tasks/{id}")]
        public IHttpActionResult Get(int id)
        {
            #region Preconditions

            if (userTaskRepository == null)
                throw new InvalidOperationException();

            if (id <= 0)
                throw new ArgumentOutOfRangeException();

            #endregion

            try
            {
                logger.Info($"TasksController Get id: {id} ", users.First(u => u.UserName == User.Identity.Name).Id);

                var userTask = userTaskRepository.GetItem(id);

                var dtoTask = UserTaskMapper.TranslateModelUserTaskToDTOTask(userTask);

                return Ok(dtoTask);
            }
            catch (Exception ex)
            {
                logger.Error($"TasksController Get id: {id}, Error:  {ex.Message}", users.First(u => u.UserName == User.Identity.Name).Id);
                return InternalServerError();
            }
        }

        [Authorize]
        [Route("api/WorkingSets/{workingSetId}/Tasks")]
        public IHttpActionResult GetWorkingSetTasks(int workingSetId, int? filterId = null, string sortBy = null, string sortOrder = null)
        {
            try
            { 
                logger.Info($"TasksController GetWorkingSetTasks workingSetId: {workingSetId}, filterId: {filterId}", users.First(u => u.UserName == User.Identity.Name).Id);

                IEnumerable<UserTask> userTasks = null;

                if (filterId != null)
                {
                    // This cast is appropriate, we need the specific UserTaskRepository behaviour here.
                    userTasks = ((UserTaskRepository)userTaskRepository).GetItems(filterId.Value, workingSetId);
                }
                else
                {
                    userTasks = userTaskRepository.GetItems(new Model.UserTaskFilter { WorkingSetId = workingSetId });
                }

                var dtoTasks = userTasks.Select(ut => UserTaskMapper.TranslateModelUserTaskToDTOTask(ut));

                var taskInfoList = new DTO.TaskList
                {
                    Metadata = new DTO.TaskListMetadata
                    {
                        Count = dtoTasks.Count(),
                        FilterId = filterId,
                        SortBy = sortBy,
                        SortOrder = sortOrder
                    },
                    Tasks = dtoTasks
                };

                return Ok(taskInfoList);
            }
            catch (Exception ex)
            {
                logger.Error($"TasksController GetWorkingSetTasks workingSetId: {workingSetId}, Error:  {ex.Message}", users.First(u => u.UserName == User.Identity.Name).Id);
                return InternalServerError();
            }
        }

        [Authorize(Roles = "Staff,Security Officer")]
        [Route("api/Tasks", Name = "AddUserTask")]
        public HttpResponseMessage Post([FromBody]DTO.Task task)
        {
            #region Preconditions

            if (userTaskRepository == null)
                throw new InvalidOperationException();

            if (task == null)
                throw new ArgumentNullException();

            #endregion

            try
            {

                var userTask = UserTaskMapper.TranslateDTOTaskToModelUserTask(task);

                int? newId = userTaskRepository.Add(userTask);

                if (newId == null)
                {
                    string taskDataError = JsonConvert.SerializeObject(task);
                    logger.Error($"TasksController Post userTaskRepository Add Task Failed Task Data: {taskDataError}", users.First(u => u.UserName == User.Identity.Name).Id);
                    throw new HttpResponseException(HttpStatusCode.InternalServerError);
                }

                task.Id = newId.Value;

                var response = Request.CreateResponse<DTO.Task>(HttpStatusCode.Created, task);

                string uri = Url.Link("AddUserTask", new { id = task.Id });
                response.Headers.Location = new Uri(uri);

                string taskData = JsonConvert.SerializeObject(task);
                logger.Info($"Task Create id: {task.Id}, taskData: {taskData}", users.First(u => u.UserName == User.Identity.Name).Id);

                return response;
            }

            catch (Exception ex)
            {
                string taskData = JsonConvert.SerializeObject(task);
                logger.Error($"api/Tasks Post Error: {ex.Message}, taskData: {taskData}", users.First(u => u.UserName == User.Identity.Name).Id);
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }

        [Authorize(Roles = "Staff,Security Officer")]
        [Route("api/Tasks/{id}", Name = "UpdateUserTask")]
        public HttpResponseMessage Put(int id, DTO.Task task)
        {
            #region Preconditions

            if (userTaskRepository == null)
                throw new InvalidOperationException();

            if (id <= 0)
                throw new ArgumentOutOfRangeException();

            if (task == null)
                throw new ArgumentNullException();

            #endregion

            try
            {

                UserTask userTask = null;

                task.Id = id;
                userTask = UserTaskMapper.TranslateDTOTaskToModelUserTask(task);

                if (!userTaskRepository.Update(userTask))
                {
                    string taskDataError = JsonConvert.SerializeObject(task);
                    logger.Error($"TasksController Put userTaskRepository Failed id: {id}, Task Data: {taskDataError}", users.First(u => u.UserName == User.Identity.Name).Id);
                    throw new HttpResponseException(HttpStatusCode.NotFound);
                }

                try
                {
                    new AppServices.NotificationsService().NotifyUsers(userTask);
                }
                catch (Exception ex)
                {
                    string taskDataError = JsonConvert.SerializeObject(task);
                    logger.Error($"TasksController Put userTaskRepository NotifyUsers Failed id: {id}, Message: {ex.Message}, taskData: {taskDataError}", users.First(u => u.UserName == User.Identity.Name).Id);
                }

                var response = Request.CreateResponse<DTO.Task>(HttpStatusCode.OK, task);

                string uri = Url.Link("UpdateUserTask", new { id = task.Id });
                response.Headers.Location = new Uri(uri);

                string taskData = JsonConvert.SerializeObject(task);
                logger.Info($"Task Update id: {task.Id}, taskData: {taskData}", users.First(u => u.UserName == User.Identity.Name).Id);

                return response;
            }

            catch (Exception ex)
            {
                string taskDataError = JsonConvert.SerializeObject(task);
                logger.Error($"api/Tasks/id Put Id: {id}, Error: {ex.Message}, taskData: {taskDataError}", users.First(u => u.UserName == User.Identity.Name).Id);
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }

        [Authorize(Roles = "Staff,Security Officer")]
        [Route("api/Tasks/{id}")]
        public void Delete(int id)
        {
            #region Preconditions

            if (userTaskRepository == null)
                throw new InvalidOperationException();

            if (id <= 0)
                throw new ArgumentOutOfRangeException();

            #endregion

            try
            {
                var userTask = userTaskRepository.GetItem(id);

                if (userTask == null)
                {
                    logger.Error($"pi/Tasks/id Delete userTaskRepository Failed id: {id} ", users.First(u => u.UserName == User.Identity.Name).Id);
                    throw new HttpResponseException(HttpStatusCode.NotFound);
                }

                userTaskRepository.Remove(id);

                logger.Info($"Task Delete id: {id} Deleted by: { User.Identity.Name} ", users.First(u => u.UserName == User.Identity.Name).Id);
            }


            catch (Exception ex)
            {
                logger.Error($"api/Tasks/id Delete Id: {id}, Error: {ex.Message} ", users.First(u => u.UserName == User.Identity.Name).Id);
            }
        }
    }
}
