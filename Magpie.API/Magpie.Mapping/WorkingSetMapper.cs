namespace Magpie.Mapping
{
    public static class WorkingSetMapper
    {
        public static Model.WorkingSet TranslateDTOWorkingSetToModelWorkingSet(DTO.WorkingSet ws)
        {
            if (ws == null)
                return null;

            return new Model.WorkingSet
            {
                Deployed = ws.Deployed,
                DeployedByUser = UserMapper.TranslateDTOUserToModelUser(ws.DeployedByUser),
                Description = ws.Description,
                Name = ws.Name,
                WorkingSetGuid = ws.WorkingSetGuid,
                Id = ws.Id,
                WorkingSetTemplate = WorkingSetTemplateMapper.TranslateDTOWorkingTemplateSetToModelWorkingSetTemplate(ws.WorkingSetTemplate),
                WorkingSetCompliance = ws.WorkingSetCompliance,
                WorkingSetDataPoint = WorkingSetHistoryMapper.TranslateDTOWorkingSetHistoryToModelWorkingSet(ws.WorkingSetDataPoint),
                Users = UserMapper.TranslateDTOUserListToModelUserList(ws.Users)
            };
        }

        public static DTO.WorkingSet TranslateModelWorkingSetToDTOWorkingSet(Model.WorkingSet ws)
        {
            if (ws == null)
                return null;

            return new DTO.WorkingSet
            {
                Deployed = ws.Deployed,
                DeployedByUser = UserMapper.TranslateModelUserToDTOUser(ws.DeployedByUser),
                Description = ws.Description,
                Name = ws.Name,
                WorkingSetGuid = ws.WorkingSetGuid,
                Id = ws.Id,
                WorkingSetTemplate = WorkingSetTemplateMapper.TranslateModelWorkingSetTemplateToDTOWorkingSetTemplate(ws.WorkingSetTemplate),
                WorkingSetCompliance = ws.WorkingSetCompliance,
                WorkingSetDataPoint = WorkingSetHistoryMapper.TranslateModelWorkingSetHistoryToDTOWorkingSet(ws.WorkingSetDataPoint),
                Users = UserMapper.TranslateModelUserListToDTOUserList(ws.Users)
            };
        }
    }
}
