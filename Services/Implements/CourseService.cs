using HelperAPI.Services.Interfaces;

namespace HelperAPI.Services.Implements
{
    public class CourseService : ICourseService
    {
        public string GetCourse(string courseId)
        {
            return courseId;
        }
    }
}
