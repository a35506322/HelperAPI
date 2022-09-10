using FluentValidation;
using FluentValidation.Results;
using HelperAPI.Models;
using HelperAPI.Modules.Interfaces;
using HelperAPI.Services.Implements;
using HelperAPI.Services.Interfaces;
using HelperAPI.Validation;
using Org.BouncyCastle.Ocsp;
using System;
using static NPOI.HSSF.Util.HSSFColor;

namespace HelperAPI.Modules.Implements
{
    public class CourseModule : IBaseModule
    {
        public void AddModuleRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/course/get", (ICourseService courseService) => courseService.GetCourse("測試CourseId"));
            app.MapPost("/course/post",  async (IValidator <CourseRequest> validator ,CourseRequest request) => {
                /*
                 FluentValidaion 官方目前使用minmalAPI沒有自動驗證
                 按照官方說法就是寫這樣
                 */
                ValidationResult validationResult = await validator.ValidateAsync(request);
                if (!validationResult.IsValid)
                {
                    return Results.ValidationProblem(validationResult.ToDictionary());
                }
                return Results.Ok("Model is valid for update!");
            });
        }
    }
}
