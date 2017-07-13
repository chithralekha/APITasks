namespace Magpie.Mapping
{
    public static class WorkingSetTemplateMapper
    {
        public static Model.WorkingSetTemplate TranslateDTOWorkingTemplateSetToModelWorkingSetTemplate(DTO.WorkingSetTemplate wst)
        {
            if (wst == null)
                return null;

            return new Model.WorkingSetTemplate
            {
                ControlSets = ControlSetMapper.TranslateDTOControlSetListToModelControlSetList(wst.ControlSets),
                Created = wst.Created,
                CreatedByUser = UserMapper.TranslateDTOUserToModelUser(wst.CreatedByUser),
                Name = wst.Name,
                WorkingSetTemplateGuid = wst.WorkingSetTemplateGuid,
                WorkingSetTemplateId = wst.WorkingSetTemplateId
            };
        }

        public static DTO.WorkingSetTemplate TranslateModelWorkingSetTemplateToDTOWorkingSetTemplate(Model.WorkingSetTemplate wst)
        {
            if (wst == null)
                return null;

            return new DTO.WorkingSetTemplate
            {
                ControlSets = ControlSetMapper.TranslateModelControlSetListToDTOControlSetList(wst.ControlSets),
                Created = wst.Created,
                CreatedByUser = UserMapper.TranslateModelUserToDTOUser(wst.CreatedByUser),
                Name = wst.Name,
                WorkingSetTemplateGuid = wst.WorkingSetTemplateGuid,
                WorkingSetTemplateId = wst.WorkingSetTemplateId
            };
        }
    }
}
