using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Magpie.Mapping;
using Magpie.Model;
using Magpie.Repository;
using Magpie.Document.Writer;

namespace Magpie.API.Controllers
{
    public class WorkingSetsController : ApiController
    {
        IRepository<WorkingSet> workingSetRepository;
        IRepository<UserTaskFilter> userTaskFilterRepository;
        PolicyGenerationPropertiesRepository policyGenerationPropertiesRepository;
        DocumentRepository documentRepository;
        TenantRepository tenantRepository;

        private readonly string awsAccessKey = "AKIAJWCLLJ2DMOGDGTWQ";
        private readonly string awsSecretKey = "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";
        private readonly string awsBucketName = "magpie-documents";
        private readonly string awsBucketOffset = "UPLOADS";   // https://s3-us-west-2.amazonaws.com/magpie-documents/UPLOADS/";

        public WorkingSetsController()
        {
            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["MagpieClient"].ConnectionString;
            workingSetRepository = new WorkingSetRepository(connectionString);
            userTaskFilterRepository = new UserTaskFilterRepository(connectionString);
            policyGenerationPropertiesRepository = new PolicyGenerationPropertiesRepository(connectionString);
            documentRepository = new DocumentRepository(connectionString);
            tenantRepository = new TenantRepository(connectionString);
        }

        public WorkingSetsController(IRepository<WorkingSet> WorkingSetRepository, IRepository<UserTaskFilter> UserTaskFilterRepository)
        {
            #region Preconditions

            if (this.workingSetRepository == null)
                throw new ArgumentNullException();

            #endregion

            this.workingSetRepository = WorkingSetRepository;
            this.userTaskFilterRepository = UserTaskFilterRepository;
        }

        [Authorize]
        [Route("api/WorkingSets")]
        public IHttpActionResult Get()
        {
            #region Preconditions

            if (workingSetRepository == null)
                throw new InvalidOperationException();

            #endregion

            try
            {
                var workingSets = workingSetRepository.GetItems();

                var dtoWorkingSets = workingSets.Select(utf => WorkingSetMapper.TranslateModelWorkingSetToDTOWorkingSet(utf));

                return Ok(dtoWorkingSets);
            }
            catch (Exception ex)
            {
                return InternalServerError();
            }
        }

        [Authorize]
        [Route("api/WorkingSets/{id}")]
        public IHttpActionResult Get(int id)
        {
            #region Preconditions

            if (workingSetRepository == null)
                throw new InvalidOperationException();

            if (id <= 0)
                throw new ArgumentOutOfRangeException();

            #endregion

            try
            {
                var workingSet = workingSetRepository.GetItem(id);

                var dtoTaskWorkingSet = WorkingSetMapper.TranslateModelWorkingSetToDTOWorkingSet(workingSet);

                return Ok(dtoTaskWorkingSet);
            }
            catch (Exception ex)
            {
                string res = ex.ToString();
                return InternalServerError();
            }
        }


        [Authorize]
        [Route("api/CreatePoliciesAndProcedures/{id}")]
        public IHttpActionResult Get(int id, string name = null)
        {
            try
            {
                #region Preconditions

                if (id <= 0)
                    throw new ArgumentOutOfRangeException();

                #endregion

                var workingSet = workingSetRepository.GetItem(id);

                var awsConfigInfo = new DocumentGenerator.AwsConfigInfo(awsAccessKey, awsSecretKey, awsBucketName);

                var tenant = tenantRepository.GetTenant();

                var documentRepositoryInfo = new DocumentRepositoryInfo();
                documentRepositoryInfo.FilePath = $"{awsBucketOffset}/{tenant.Id}/{workingSet.Name.Replace(" ", "")}";
                documentRepositoryInfo.WorkingSetId = workingSet.Id;

                documentRepositoryInfo.PolicyGenerationPropertiesEntries = policyGenerationPropertiesRepository.GetPolicyGenerationProperties(tenant.Id).PolicyGenerationPropertiesEntries;

                ConfigurationController configurationController = new ConfigurationController();
                var configResult = configurationController.Get();
                var configInfo = configResult as System.Web.Http.Results.OkNegotiatedContentResult<DTO.Configuration>;

                var sourceDirectory = configInfo.Content.ConfigurationEntries["ProfileAndProceduresSourceDir"];

                AddToRepository(sourceDirectory, documentRepositoryInfo, awsConfigInfo);
                
                return Ok();
            }
            catch (Exception ex)
            {
                string res = ex.ToString();
                return InternalServerError();
            }

        }
        private void AddToRepository(string sourceDocumentDirectory, DocumentRepositoryInfo documentRepositoryInfo, DocumentGenerator.AwsConfigInfo awsConfigInfo)
        {
            var generateFromTemplate = new DocumentGenerator.GenerateFromTemplate();

            var documentRepositoryInfoList = generateFromTemplate.GenerateDocument(sourceDocumentDirectory, documentRepositoryInfo, awsConfigInfo);

            foreach (var item in documentRepositoryInfoList)
            {
                if (string.IsNullOrWhiteSpace(item.SaveError))
                {
                    var workingSetDocument = new WorkingSetDocument();
                    workingSetDocument.DocumentTitle = item.DocumentTitle;
                    workingSetDocument.DocumentUri = $"{documentRepositoryInfo.FilePath}/{item.DocumentTitle}";
                    workingSetDocument.WorkingSetId = documentRepositoryInfo.WorkingSetId;

                    documentRepository.Add(workingSetDocument);
                }
            }
        }
    }
}

