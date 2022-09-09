using HelperAPI.Modules.Interfaces;
using HelperAPI.Services.Interfaces;

namespace HelperAPI.Modules.Implements
{
    public class BookModule : IBaseModule
    {
        public void AddModuleRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/book/get", (ICourseService courseService) => courseService.GetCourse("測試BookId"));
        }
    }
}
