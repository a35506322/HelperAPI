using FluentValidation;
using FluentValidation.Results;
using HelperAPI.Helper;
using HelperAPI.Models;
using HelperAPI.Modules.Interfaces;
using Org.BouncyCastle.Asn1.Ocsp;

namespace HelperAPI.Modules.Implements
{
    public class SignModule : IBaseModule
    {
        public void AddModuleRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/sign_in", async (IValidator <SignInRequest> validator,SignInRequest signInRequest, JWTHelper jWTHelper) =>
            {
                // 驗證Request
                ValidationResult validationResult = await validator.ValidateAsync(signInRequest);
                if (!validationResult.IsValid)
                {
                    return Results.ValidationProblem(validationResult.ToDictionary());
                }

                // 驗證帳號
                if (!ValidateUser(signInRequest))
                {
                    return Results.BadRequest("帳密錯誤");
                }

                // 產生鑰匙給他
                string jwtToken = jWTHelper.GenerateToken(signInRequest.Account);
                return Results.Ok(jwtToken);

            }).AllowAnonymous();
        }

        private bool ValidateUser(SignInRequest signInRequest)
        {
            string validateAccont = "1001234";
            string validatePassword = "=-09poiu";

            if (signInRequest.Account != validateAccont || signInRequest.Password != validatePassword)
            { 
                return false;
            }
            return true;
        }

       public record SignInRequest (string Account,string Password);
    }
}
