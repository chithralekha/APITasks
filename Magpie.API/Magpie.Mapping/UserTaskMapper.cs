namespace Magpie.Mapping
{
    public static class UserTaskMapper
    {
        public static Model.UserTask TranslateDTOTaskToModelUserTask(DTO.Task t)
        {

            if (t == null)
                return null;

            return new Model.UserTask
            {
                Code = t.Code,
                Comments = Mapper.TranslateDTOCommentListToModelCommentList(t.Comments),
                Completed = t.Completed,
                Control = ControlMapper.TranslateDTOControlToModelControl(t.Control),
                ControlId = t.ControlId,
                ControlCode = t.ControlCode,
                ControlTitle = t.ControlTitle,
                Created = t.Created,
                CreatedByUserId = t.CreatedByUserId,
                Description = t.Description,
                Due = t.Due,
                DueStatus = Mapper.TranslateDTODueStatusToModelDueStatus(t.DueStatus),
                Events = Mapper.TranslateDTOEventListToModelEventList(t.Events),
                Id = t.Id,
                Link = t.Link,
                RaciTeam = Mapper.TranslateDTORaciTeamToModelRaciTeam(t.RaciTeam),
                TaskDefinitionId = t.TaskDefinitionId,
                TaskState = Mapper.TranslateDTOTaskStateToModelTaskState(t.TaskState),
                Title = t.Title,
                WorkingSet = WorkingSetMapper.TranslateDTOWorkingSetToModelWorkingSet(t.WorkingSet),
                WorkingSetId = t.WorkingSetId
            };
        }
        public static DTO.Task TranslateModelUserTaskToDTOTask(Model.UserTask ut)
        {
            if (ut == null)
                return null;

            return new DTO.Task
            {
                Code = ut.Code,
                Comments = Mapper.TranslateModelCommentListToDTOCommentList(ut.Comments),
                Completed = ut.Completed,
                Control = ControlMapper.TranslateModelControlToDTOControl(ut.Control),
                ControlId = ut.ControlId,
                ControlCode = ut.ControlCode,
                ControlTitle = ut.ControlTitle,
                Created = ut.Created,
                CreatedByUserId = ut.CreatedByUserId,
                Description = ut.Description,
                Due = ut.Due,
                DueStatus = Mapper.TranslateModelDueStatusToDTODueStatus(ut.DueStatus),
                Events = Mapper.TranslateModelEventListToDTOEventList(ut.Events),
                Id = ut.Id,
                Link = ut.Link,
                RaciTeam = Mapper.TranslateModelRaciTeamToDTORaciTeam(ut.RaciTeam),
                TaskDefinitionId = ut.TaskDefinitionId,
                TaskState = Mapper.TranslateModelTaskStateToDTOTaskState(ut.TaskState),
                Title = ut.Title,
                WorkingSet = WorkingSetMapper.TranslateModelWorkingSetToDTOWorkingSet(ut.WorkingSet),
                WorkingSetId = ut.WorkingSetId

            };
        }
    }
}
