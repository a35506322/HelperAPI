using HelperAPI.Modules.Interfaces;
using HelperAPI.Services.Implements;
using HelperAPI.Services.Interfaces;

namespace HelperAPI.Modules.Implements
{
    public class CourseModule : IBaseModule
    {
        public void AddModuleRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/course/get", (ICourseService courseService) => courseService.GetCourse("測試CourseId"));
        }
    }
}
